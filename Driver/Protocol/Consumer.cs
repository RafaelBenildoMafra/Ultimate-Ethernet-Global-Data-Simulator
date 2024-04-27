using Elfie.Serialization;
using EthernetGlobalData.Data;
using EthernetGlobalData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.Collections;
using System.Drawing;
using System.Xml.Linq;
using static EthernetGlobalData.Protocol.Protocol;
using static System.Formats.Asn1.AsnWriter;

namespace EthernetGlobalData.Protocol
{
    public class Consumer
    {
        public static List<Consumer> Consumers = new List<Consumer>();
        private static CancellationTokenSource Source = new CancellationTokenSource();

        [BindProperty]
        public Models.Point Point { get; set; } = default!;        
        public MessageHeader Header { get; set; }
        public Message Message { get; set; }

        public Consumer(MessageHeader messageHeader, Message messageData, CancellationTokenSource source)
        {
            Header = messageHeader;
            Message = messageData;
            Consumers.Add(this);
            Source = source;
        }

        public static async Task<MessageStatus> Start(UDP transportLayer, IServiceScopeFactory scope)
        {
            try
            {
                CancellationToken token = Source.Token;                

                Task task = Task.Run(async () =>
                {
                    while (!token.IsCancellationRequested)
                    {
                        byte[] recievedBytes;

                        lock (transportLayer)
                        {
                            recievedBytes = transportLayer.Receive();
                        }                        

                        foreach (Consumer consumer in Consumers)
                        {
                            if (!consumer.Header.Points.Any())
                            {
                                consumer.Message.Status = MessageStatus.Discarded;
                                Console.WriteLine("No Points Configured for: " + consumer.Header.ID);
                                continue;
                            }

                            using (var service = scope.CreateScope())
                            {
                                ApplicationDbContext context = service.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                                consumer.Message = consumer.TreatHeader(recievedBytes);

                                MessageStatus messageStatus = consumer.TreatPayload(context);
                                                            
                                if (consumer.Message.Status == MessageStatus.ErrorReading)
                                {
                                    Stop();
                                    return;
                                }

                                consumer.Message.Data.Clear();

                                await Task.Delay((int)consumer.Header.Period);
                            }
                        }
                    }
                }, token);                                                                                          
            }
            catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
                return MessageStatus.ErrorReading;
            }

            return MessageStatus.Stored;

        }

        public static void Stop()
        {
            Source.Cancel();
        }

        public Message TreatHeader(byte[] recievedBytes)
        {   
            string messageState = "Header";           

            if (recievedBytes != null)
            {
                try
                {
                    switch (messageState)
                    {
                        case "Header":

                            if (recievedBytes[0] == 0x0D)
                            {
                                goto case "Producer";
                            }
                            break;

                        case "Producer":

                            string[] parts = this.Header.ID.Split('.');

                            for (int i = 0; i < 4; i++)
                            {
                                if (recievedBytes[4 + i] != byte.Parse(parts[i]))
                                {
                                    break;
                                }
                                else if (i == 3)
                                {
                                    goto case "Exchange";
                                }
                            }

                            break;

                        case "Exchange":

                            byte[] exchange = new byte[4];

                            for (int i = 0; i < 4; i++)
                            {
                                exchange[i] = recievedBytes[8 + i];
                            }

                            if (BitConverter.ToInt32(exchange, 0) == this.Header.ExchangeID)
                            {
                                goto case "Signature";
                            }

                            break;

                        case "Signature":

                            if (recievedBytes[24] == this.Header.MinorSignature && recievedBytes[25]
                                == this.Header.MajorSignature)
                            {
                                goto case "Data";
                            }

                            break;

                        case "Data":

                            if (recievedBytes.Length > 1432)
                            {
                                break;
                            }
                            else
                            {
                                for (int i = 32; i < recievedBytes.Length; i++)
                                {
                                    this.Message.Data.Add(recievedBytes[i]);
                                }

                                 return this.Message;
                            }
                    }
                    this.Message.Status = MessageStatus.Discarded;
                    return this.Message;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    this.Message.Status = MessageStatus.ErrorReading;
                    return this.Message;
                }
            }
            else
            {
                this.Message.Status = MessageStatus.ErrorReading;
                return this.Message;
            }
        }

        public MessageStatus TreatPayload(ApplicationDbContext context)
        {
            foreach (Models.Point point in this.Header.Points)
            {
                string[] address = new string[2];
                ushort bytePos;
                ushort bitPos;

                this.Message.Data = AjustPayloadSize(this.Message.Data);

                try
                {
                    switch (point.DataType)
                    {
                        case Models.DataType.Boolean:

                            address = point.Address.Split(".");

                            bytePos = Convert.ToUInt16(address[0]);
                            bitPos = Convert.ToUInt16(address[1]);

                            BitArray bitArray = new BitArray(new byte[] { this.Message.Data[bytePos] });

                            BitArray reversedBitArray = new BitArray(bitArray.Count);

                            for (int i = 0; i < bitArray.Count; i++)
                            {
                                reversedBitArray[i] = bitArray[bitArray.Count - i - 1];
                            }

                            point.Value = reversedBitArray[bitPos] == true ? 1 : 0;

                            UpdatePoint(point, context);

                            break;

                        case Models.DataType.Word:

                            bytePos = Convert.ToUInt16(point.Address);

                            byte[] byteWord = { this.Message.Data[bytePos], this.Message.Data[bytePos + 1] };

                            point.Value = BitConverter.ToUInt16(byteWord, 0);

                            UpdatePoint(point, context);

                            break;

                        case Models.DataType.Real:

                            bytePos = Convert.ToUInt16(point.Address);

                            byte[] byteReal = {this.Message.Data[bytePos], this.Message.Data[bytePos + 1], this.Message.Data[bytePos + 2],
                            this.Message.Data[bytePos + 3]};

                            point.Value = BitConverter.ToUInt32(byteReal, 0);

                            UpdatePoint(point, context);

                            break;

                        case Models.DataType.Long:

                            bytePos = Convert.ToUInt16(point.Address);

                            byte[] byteLong = {this.Message.Data[bytePos], this.Message.Data[bytePos + 1], this.Message.Data[bytePos + 2],
                            this.Message.Data[bytePos + 3], this.Message.Data[bytePos + 4], this.Message.Data[bytePos + 5],
                            this.Message.Data[bytePos + 6], this.Message.Data[bytePos + 7] };

                            point.Value = BitConverter.ToInt64(byteLong, 0);

                            UpdatePoint(point, context);

                            break;

                    }
                }
                catch (Exception e)
                {   
                    Console.WriteLine(e);
                    return MessageStatus.ErrorStoring;
                }
            }
            try
            {
                context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {   
                throw;
            }
            return MessageStatus.Stored;
        }

        private static void UpdatePoint(Models.Point point, ApplicationDbContext context)
        {           
            try
            {
                context.Attach(point).State = EntityState.Modified;

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PointExists(point.PointID, context))
                {
                    throw;
                }
                else
                {
                    throw;
                }
            }
        }

        private static bool PointExists(int id, ApplicationDbContext context)
        {
            return context.Point.Any(e => e.PointID == id);
        }

        private static List<Byte> AjustPayloadSize(List<Byte> bytes)
        {
            if (bytes.Count < 7)
            {
                for (int i = 0; i < 7; i++)
                {
                    bytes.Add(0);
                }
            }

            return bytes;
        }
    }
}
