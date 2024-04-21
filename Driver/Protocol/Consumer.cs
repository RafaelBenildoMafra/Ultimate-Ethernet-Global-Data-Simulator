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
        public struct MessageHeader
        {
            public string ID { get; set; }
            public int ExchangeID { get; set; }
            public int MajorSignature { get; set; }
            public int MinorSignature { get; set; }
            public ICollection<Models.Point> Points { get; set; }
        }

        public static List<Consumer> Consumers = new List<Consumer>();

        [BindProperty]
        public Models.Point Point { get; set; } = default!;        
        public MessageHeader Header { get; set; }
        public MessageData Message { get; set; }

        public Consumer(MessageHeader messageHeader, MessageData messageData)
        {
            Header = messageHeader;
            Message = messageData;
            Consumers.Add(this);
        }

        public static MessageStatus Start(UDP transportLayer, CancellationTokenSource token, IServiceScopeFactory scope)
        {
            try
            {                
                byte[] recievedBytes = transportLayer.Receive();

                foreach (Consumer consumer in Consumers)
                {
                    Task task = Task.Run(async () =>
                    {
                        using (var service = scope.CreateScope())
                        {
                            ApplicationDbContext context = service.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                            consumer.Message = consumer.Read(recievedBytes);

                            if (consumer.Message.Status == MessageStatus.ErrorReading)
                            {
                                transportLayer.Close();

                                token.Cancel();

                                return;
                            }                            

                            MessageStatus messageStatus = await consumer.TreatMessage(context);
                        }
                    }, token.Token);                                                          
                }                
            }
            catch 
            {
                return MessageStatus.ErrorStoring;
            }

            return MessageStatus.Stored;

        }

        public MessageData Read(byte[] recievedBytes)
        {
            this.Message.Payload = new List<byte>();

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
                                goto case "Payload";
                            }

                            break;

                        case "Payload":

                            if (recievedBytes.Length > 1432)
                            {
                                break;
                            }
                            else
                            {
                                for (int i = 32; i < recievedBytes.Length; i++)
                                {
                                    this.Message.Payload.Add(recievedBytes[i]);
                                }
                                this.Message.Status = MessageStatus.Recieved;

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

        public async Task<MessageStatus> TreatMessage(ApplicationDbContext context)
        {
            foreach (Models.Point point in this.Header.Points)
            {
                string[] address = new string[2];
                ushort bytePos;
                ushort bitPos;

                this.Message.Payload = AjustPayloadSize(this.Message.Payload);

                try
                {
                    switch (point.DataType)
                    {
                        case Models.DataType.Boolean:

                            address = point.Address.Split(".");

                            bytePos = Convert.ToUInt16(address[0]);
                            bitPos = Convert.ToUInt16(address[1]);

                            BitArray bitArray = new BitArray(new byte[] { this.Message.Payload[bytePos] });

                            point.Value = bitArray[bitPos] == true ? 1 : 0;

                            await UpdatePoint(point, context);

                            break;

                        case Models.DataType.Word:

                            bytePos = Convert.ToUInt16(point.Address);

                            byte[] byteWord = { this.Message.Payload[bytePos], this.Message.Payload[bytePos + 1] };

                            point.Value = BitConverter.ToUInt16(byteWord, 0);

                            await UpdatePoint(point, context);

                            break;

                        case Models.DataType.Real:

                            bytePos = Convert.ToUInt16(point.Address);

                            byte[] byteReal = {this.Message.Payload[bytePos], this.Message.Payload[bytePos + 1], this.Message.Payload[bytePos + 2],
                            this.Message.Payload[bytePos + 3]};

                            point.Value = BitConverter.ToUInt32(byteReal, 0);

                            await UpdatePoint(point, context);

                            break;

                        case Models.DataType.Long:

                            bytePos = Convert.ToUInt16(point.Address);

                            byte[] byteLong = {this.Message.Payload[bytePos], this.Message.Payload[bytePos + 1], this.Message.Payload[bytePos + 2],
                            this.Message.Payload[bytePos + 3], this.Message.Payload[bytePos + 4], this.Message.Payload[bytePos + 5],
                            this.Message.Payload[bytePos + 6], this.Message.Payload[bytePos + 7] };

                            point.Value = BitConverter.ToInt64(byteLong, 0);

                            await UpdatePoint(point, context);

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
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {   
                throw;
            }
            return MessageStatus.Stored;
        }

        private static async Task UpdatePoint(Models.Point point, ApplicationDbContext context)
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
