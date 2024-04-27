using EthernetGlobalData.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Common;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using static EthernetGlobalData.Protocol.Protocol;

namespace EthernetGlobalData.Protocol
{
    public class Producer
    {
        private static CancellationTokenSource Source = new CancellationTokenSource();
        public static List<Producer> Producers = new List<Producer>();

        [BindProperty]
        public Models.Point Point { get; set; } = default!;
        public MessageHeader Header { get; set; }
        public Message Message { get; set; }        

        public Producer(MessageHeader messageHeader, Message messageData, CancellationTokenSource source)
        {
            Header = messageHeader;
            Message = messageData;
            Producers.Add(this);
            Source = source;
        }

        public static async Task<MessageStatus> Start(UDP transportLayer, IServiceScopeFactory scope)
        {
            try
            {
                CancellationToken token = Source.Token;

                transportLayer.Connect();
                
                foreach (Producer producer in Producers)
                {
                    if (!producer.Header.Points.Any())
                    {
                        producer.Message.Status = MessageStatus.Discarded;
                        Console.WriteLine("No Points Configured for: " + producer.Header.ID);
                        continue;
                    }

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

                                await Task.Delay((int)producer.Header.Period);
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
            Source.Cancel();
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
                this.Message.Data.InsertRange(0, headerBytes);

                lock (this.Message)
                {
                    this.Message = TreatMessage(this.Message);
                }
                
                if(this.Message.Status == MessageStatus.ErrorStoring)
                {
                    Console.WriteLine(this.Message.Status.ToString());
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
                                int realValue = Convert.ToInt32(point.Value);
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

            int bytePos = Convert.ToInt16(address[0]);
            int bitPos = Convert.ToInt16(address[1]);

            if (this.Message.Data.Count <= Protocol.HeaderSize + bytePos)
            {   
                for (int i = 0; i < Convert.ToInt16(address[0]); i++)
                {
                    this.Message.Data.Add(0x00);
                }
            }

            byte byteValue = this.Message.Data[Protocol.HeaderSize + bytePos - 1];

            BitArray bitArray = new BitArray(new byte[] { byteValue });

            bitArray.Set(bitPos, point.Value == 1 ? true : false);

            BitArray reversedBitArray = new BitArray(bitArray.Count);

            for (int i = 0; i < bitArray.Count; i++)
            {
                reversedBitArray[i] = bitArray[bitArray.Count - i - 1];
            }            

            // Convert the modified BitArray back to a byte
            byte[] byteArray = new byte[1];
            reversedBitArray.CopyTo(byteArray, 0);
            byteValue = byteArray[0];

            this.Message.Data[HeaderSize + bytePos - 1] = byteValue;
        }

        private void UpdatePayload(byte[] valueBytes, int address)
        {   

            if(this.Message.Data.Count <= Protocol.HeaderSize + address + valueBytes.Length)
            {   
                for(int i = 0; i < valueBytes.Length + address; i++)
                {
                    this.Message.Data.Add(0x00);
                }                
            }

            for(int i = 0; i < valueBytes.Length; i++)
            {
                this.Message.Data[Protocol.HeaderSize + address + i - 1] = valueBytes[i];
            }
        }

        public void UpdateMessageNumber()
        {

        }
    }
}
