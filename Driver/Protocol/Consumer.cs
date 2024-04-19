using EthernetGlobalData.Data;
using EthernetGlobalData.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace EthernetGlobalData.Protocol
{
    public class Consumer
    {
        private static List<Task> tasks = new List<Task>();
        private static List<Thread> threads = new List<Thread>();
        private static CancellationTokenSource token = new CancellationTokenSource();
        private static List<byte> Payload = new List<byte>();
        private static IServiceProvider _serviceProvider;

        [BindProperty]
        public EthernetGlobalData.Models.Point Point { get; set; } = default!;

        public Consumer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public static async Task<bool> Start(IList<Models.Node> nodes, IList<Models.Channel> channels)
        {
            foreach (Channel channel in channels)
            {
                Thread thread = new Thread(() =>
                {
                    UDP transportLayer = new UDP(channel.IP, channel.Port);

                    foreach (Node node in nodes)
                    {
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                            if (node.CommunicationType != "Consumer")
                                continue;

                            Protocol.Header header = new Protocol.Header
                            {
                                ID = node.Channel.IP,
                                MajorSignature = node.MajorSignature,
                                MinorSignature = node.MinorSignature,
                                ExchangeID = node.Exchange,
                                Points = node.Points,
                            };

                            Protocol protocol = new Protocol(transportLayer, header);

                            Task task = Task.Run(async () =>
                            {
                                await Communicate(protocol, context);

                            }, token.Token);

                            tasks.Add(task);
                        }
                    }
                });
                thread.Name = channel.ChannelName;
                thread.Start();
                threads.Add(thread);
            }

            return true;
        }

        public void Stop()
        {

            UDP.CloseAllConnections();

            token.Cancel();

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException)
            {
            }

            tasks.Clear();
        }

        public static async Task Communicate(Protocol protocol, ApplicationDbContext context)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    protocol.Read();

                    if (protocol.Payload == null)
                    {
                        protocol.TransportLayer.Close();

                        token.Cancel();

                        return;
                    }

                    await TreatPayload(protocol, context);

                    protocol.Payload.Clear();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static async Task TreatPayload(Protocol protocol, ApplicationDbContext context)
        {
            foreach (Models.Point point in protocol.MessageHeader.Points)
            {
                string[] address = new string[2];
                ushort bytePos;
                ushort bitPos;

                AjustPayloadSize(protocol);

                try
                {
                    switch (point.DataType)
                    {
                        case Models.DataType.Boolean:

                            address = point.Address.Split(".");

                            bytePos = Convert.ToUInt16(address[0]);
                            bitPos = Convert.ToUInt16(address[1]);

                            BitArray bitArray = new BitArray(new byte[] { protocol.Payload[bytePos] });

                            point.Value = bitArray[bitPos] == true ? 1 : 0;

                            await UpdatePoint(point, context);

                            break;

                        case Models.DataType.Word:

                            bytePos = Convert.ToUInt16(point.Address);

                            byte[] byteWord = { protocol.Payload[bytePos], protocol.Payload[bytePos + 1] };

                            point.Value = BitConverter.ToUInt16(byteWord, 0);

                            await UpdatePoint(point, context);

                            break;

                        case Models.DataType.Real:

                            bytePos = Convert.ToUInt16(point.Address);

                            byte[] byteReal = {protocol.Payload[bytePos], protocol.Payload[bytePos + 1], protocol.Payload[bytePos + 2],
                            protocol.Payload[bytePos + 3]};

                            point.Value = BitConverter.ToUInt32(byteReal, 0);

                            await UpdatePoint(point, context);

                            break;

                        case Models.DataType.Long:

                            bytePos = Convert.ToUInt16(point.Address);

                            byte[] byteLong = {protocol.Payload[bytePos], protocol.Payload[bytePos + 1], protocol.Payload[bytePos + 2],
                            protocol.Payload[bytePos + 3], protocol.Payload[bytePos + 4], protocol.Payload[bytePos + 5],
                            protocol.Payload[bytePos + 6], protocol.Payload[bytePos + 7] };

                            point.Value = BitConverter.ToInt64(byteLong, 0);

                            await UpdatePoint(point, context);

                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static async Task UpdatePoint(Models.Point point, ApplicationDbContext context)
        {
            context.Attach(point).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
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

        private static void AjustPayloadSize(Protocol protocol)
        {
            if (protocol.Payload.Count < 7)
            {
                for (int i = 0; i < 7; i++)
                {
                    protocol.Payload.Add(0);
                }
            }
        }
    }
}
