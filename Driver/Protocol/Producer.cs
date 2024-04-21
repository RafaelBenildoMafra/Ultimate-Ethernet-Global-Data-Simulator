using EthernetGlobalData.Data;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using static EthernetGlobalData.Protocol.Protocol;

namespace EthernetGlobalData.Protocol
{
    public class Producer
    {
        private List<Task> tasks = new List<Task>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ApplicationDbContext _context;

        public struct MessageHeader
        {
            public string ID { get; set; }
            public int ExchangeID { get; set; }
            public int MajorSignature { get; set; }
            public int MinorSignature { get; set; }
            public int MessageNumber { get; set; }
            public ICollection<Models.Point> Points { get; set; }
        }

        [BindProperty]
        public Models.Point Point { get; set; } = default!;
        public MessageHeader Header { get; set; }
        public MessageData Message { get; set; }

        public static List<Producer> Producers = new List<Producer>();

        public Producer(MessageHeader messageHeader, MessageData messageData)
        {
            Header = messageHeader;
            Message = messageData;
            Producers.Add(this);
        }

        public static MessageStatus Start(UDP transportLayer, CancellationTokenSource token, IServiceScopeFactory scope)
        {
            try
            {
                byte[] recievedBytes = transportLayer.Receive();

                foreach (Producer producer in Producers)
                {
                    Task task = Task.Run(async () =>
                    {
                        using (var service = scope.CreateScope())
                        {
                            ApplicationDbContext context = service.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                            producer.Message.Status = producer.Write();

                            if (producer.Message.Status == MessageStatus.ErrorReading)
                            {
                                transportLayer.Close();

                                token.Cancel();

                                return;
                            }                            
                        }
                    }, token.Token);
                }
                return MessageStatus.Sent;
            }
            catch
            {
                return MessageStatus.ErrorStoring;
            }

            return MessageStatus.Stored;

        }

        public MessageStatus Write()
        {
            try
            {
                byte[] headerBytes = new byte[HeaderSize];

                BitConverter.GetBytes(0XD).CopyTo(headerBytes, 0); //PDU type
                BitConverter.GetBytes(0X1).CopyTo(headerBytes, 1); //PDU version number
                BitConverter.GetBytes(Header.MessageNumber).CopyTo(headerBytes, 2); //Request ID

                // Producer ID
                string[] octets = Header.ID.Split('.');
                BitConverter.GetBytes(Convert.ToInt16(octets[0])).CopyTo(headerBytes, 4);
                BitConverter.GetBytes(Convert.ToInt16(octets[1])).CopyTo(headerBytes, 5);
                BitConverter.GetBytes(Convert.ToInt16(octets[2])).CopyTo(headerBytes, 6);
                BitConverter.GetBytes(Convert.ToInt16(octets[3])).CopyTo(headerBytes, 7);

                // Exchange ID
                BitConverter.GetBytes(Header.ExchangeID).CopyTo(headerBytes, 8);

                // Timestamp
                byte[] byteTime = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                byteTime.CopyTo(headerBytes, 12);

                // Status
                BitConverter.GetBytes(0x00).CopyTo(headerBytes, 20);

                // Signature
                BitConverter.GetBytes(Header.MinorSignature).CopyTo(headerBytes, 24);
                BitConverter.GetBytes(Header.MajorSignature).CopyTo(headerBytes, 25);

                // Reserved
                BitConverter.GetBytes(0x00000000).CopyTo(headerBytes, 28);

                Message.Payload.InsertRange(0, headerBytes);

                MessageStatus result = TreatMessage(Message);

                TransportLayer.Send(Message.Payload.ToArray());

                UpdateMessageNumber();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                return MessageStatus.ErrorSending;
            }
        }

        public MessageStatus TreatMessage(MessageData message)
        {
            try
            {
                foreach (Models.Point point in Header.Points)
                {
                    if (point.DataType != Models.DataType.Boolean)
                    {
                        switch (point.DataType)
                        {
                            case Models.DataType.Word:
                                short wordValue = Convert.ToInt16(point.Value);
                                byte[] wordBytes = BitConverter.GetBytes(wordValue);
                                message.Payload.InsertRange(HeaderSize + Convert.ToInt16(point.Address), wordBytes);
                                break;
                            case Models.DataType.Real:
                                float realValue = Convert.ToSingle(point.Value);
                                byte[] realBytes = BitConverter.GetBytes(realValue);
                                message.Payload.InsertRange(HeaderSize + Convert.ToInt16(point.Address), realBytes);
                                break;
                            case Models.DataType.Long:
                                long longValue = Convert.ToInt64(point.Value);
                                byte[] longBytes = BitConverter.GetBytes(longValue);
                                message.Payload.InsertRange(HeaderSize + Convert.ToInt16(point.Address), longBytes);
                                break;
                        }
                    }
                    else
                    {
                        string[] address;

                        address = point.Address.Split(".");

                        byte access = Message.Payload[HeaderSize + Convert.ToInt32(address[0])];

                        BitArray bitArray = new BitArray(new byte[] { access });

                        bitArray.Set(Convert.ToInt32(address[1]), point.Value == 1 ? true : false);

                        // Convert the modified BitArray back to a byte
                        byte[] byteArray = new byte[(bitArray.Length + 7) / 8];
                        bitArray.CopyTo(byteArray, 0);
                        access = byteArray[0];

                        Message.Payload[HeaderSize + Convert.ToInt32(address[0])] = access;
                    }                    
                }
                return MessageStatus.Stored;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);

                return MessageStatus.ErrorStoring;
            }
        }

        public void UpdateMessageNumber()
        {

        }
    }
}
