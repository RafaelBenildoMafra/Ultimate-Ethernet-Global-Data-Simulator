using EthernetGlobalData.Data;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using static EthernetGlobalData.Protocol.Protocol;

namespace EthernetGlobalData.Protocol
{
    public class Producer
    {
        private static CancellationTokenSource source = new CancellationTokenSource();

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
        public Message Message { get; set; }

        public static List<Producer> Producers = new List<Producer>();

        public Producer(MessageHeader messageHeader, Message messageData)
        {
            Header = messageHeader;
            Message = messageData;
            Producers.Add(this);
        }

        public static MessageStatus Start(UDP transportLayer, IServiceScopeFactory scope)
        {
            try
            {
                CancellationToken token = source.Token;

                transportLayer.Connect();
                
                foreach (Producer producer in Producers)
                {
                    Task task = Task.Run(async () =>
                    {
                        while (!token.IsCancellationRequested)
                        {
                            using (var service = scope.CreateScope())
                            {
                                ApplicationDbContext context = service.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                                producer.Message = producer.PrepareMessage();

                                transportLayer.Send(producer.Message.Data.ToArray());

                                if (producer.Message.Status == MessageStatus.ErrorSending)
                                {
                                    Stop();
                                    return;
                                }
                            }
                        }
                    }, token);
                }
                return MessageStatus.Sent;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return MessageStatus.ErrorStoring;
            }
        }

        public static void Stop()
        {
            source.Cancel();
        }

        public Message PrepareMessage()
        {
            try
            {   
                this.Message.Data.Clear();

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

                // Payload
                Message.Data.InsertRange(0, headerBytes);

                lock (Message)
                {
                    Message = TreatMessage(Message);
                }
                
                if(Message.Status == MessageStatus.ErrorStoring)
                {
                    Console.WriteLine(Message.Status.ToString());
                }

                //Update Request ID
                MessageHeader messageHeader = this.Header;
                messageHeader.MessageNumber++;
                this.Header = messageHeader;

                return this.Message;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return this.Message;
            }
        }

        public Message TreatMessage(Message message)
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
                                int wordAddress = Convert.ToInt16(point.Address);

                                UpdatePayload(wordBytes, wordAddress);

                                break;

                            case Models.DataType.Real:
                                float realValue = Convert.ToSingle(point.Value);
                                byte[] realBytes = BitConverter.GetBytes(realValue);
                                int realAddress = Convert.ToInt16(point.Address);

                                UpdatePayload(realBytes, realAddress);

                                break;

                            case Models.DataType.Long:
                                long longValue = Convert.ToInt64(point.Value);
                                byte[] longBytes = BitConverter.GetBytes(longValue);
                                int longAddress = Convert.ToInt16(point.Address);

                                UpdatePayload(longBytes, longAddress);

                                break;

                            default:
                                throw new InvalidOperationException("Unsupported data type.");
                        }
                    }
                    else
                    {
                        TreatBoolean(point);
                    }                    
                }
                message.Status = MessageStatus.Stored;
                return message;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                message.Status = MessageStatus.ErrorStoring;
                return message;
            }
        }

        private void TreatBoolean(Models.Point point)
        {   

            string[] address;

            address = point.Address.Split(".");

            int bytePos = Convert.ToInt32(address[0]);
            int bitPos = Convert.ToInt32(address[1]);

            if (this.Message.Data.Count <= Protocol.HeaderSize + bytePos)
            {   
                for (int i = 0; i < Convert.ToInt32(address[0]); i++)
                {
                    this.Message.Data.Add(0x00);
                }
            }

            byte byteValue = this.Message.Data[HeaderSize + bytePos];

            BitArray bitArray = new BitArray(new byte[] { byteValue });

            BitArray reversedBitArray = new BitArray(bitArray.Count);

            for (int i = 0; i < bitArray.Count; i++)
            {
                reversedBitArray[i] = bitArray[bitArray.Count - 1 - i];
            }

            reversedBitArray.Set(bitPos, point.Value == 1 ? true : false);

            // Convert the modified BitArray back to a byte
            byte[] byteArray = new byte[1];
            reversedBitArray.CopyTo(byteArray, 0);
            byteValue = byteArray[0];

            this.Message.Data[HeaderSize + bytePos] = byteValue;
        }

        private void UpdatePayload(byte[] valueBytes, int address)
        {   

            if(this.Message.Data.Count <= Protocol.HeaderSize + address + valueBytes.Length)
            {   
                foreach(byte value in valueBytes)
                {
                    this.Message.Data.Add(0x00);
                }                
            }

            for(int i = 0; i < valueBytes.Length; i++)
            {
                this.Message.Data[Protocol.HeaderSize + address + i] = valueBytes[i];
            }
        }

        public void UpdateMessageNumber()
        {

        }
    }
}
