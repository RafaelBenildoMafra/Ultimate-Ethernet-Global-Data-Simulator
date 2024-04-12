using EthernetGlobalData.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Threading;

namespace EthernetGlobalData.Protocol
{
    public class Consumer : IProtocol
    {
        private List<Task> tasks = new List<Task>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ProtocolContext _context;
        private List<byte> Payload = new List<byte>();

        [BindProperty]
        public EthernetGlobalData.Models.Point Point { get; set; } = default!;

        public Consumer(ProtocolContext context)
        {
            _context = context;
        }

        public void Start(IList<EthernetGlobalData.Models.Node> nodes)
        {
            foreach (Models.Node node in nodes)
            {
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

                Protocol protocol = new Protocol(new UDP(node.Channel.IP, node.Channel.Port), header);

                tasks.Add(Communicate(protocol));
            }
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException)
            {
            }

            tasks.Clear();
        }

        public async Task Communicate(Protocol protocol)
        {   
            while(true)
            {
                protocol.Read();

                if (protocol.Payload == null)
                {
                    protocol.TransportLayer.Close();

                    cancellationTokenSource.Cancel();

                    return;
                }

                TreatPayload(protocol);

                protocol.Payload.Clear();
            }
        }

        public void TreatPayload(Protocol protocol)
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

                            UpdatePoint(point);

                            break;

                        case Models.DataType.Word:

                            address[0] = point.Address.Split(".")[0];

                            bytePos = Convert.ToUInt16(address[0]);

                            byte[] byteWord = { protocol.Payload[bytePos], protocol.Payload[bytePos + 1] };

                            point.Value = BitConverter.ToUInt16(byteWord, 0);

                            UpdatePoint(point);

                            break;

                        case Models.DataType.Real:

                            address[0] = point.Address.Split(".")[0];

                            bytePos = Convert.ToUInt16(address[0]);

                            byte[] byteReal = {protocol.Payload[bytePos], protocol.Payload[bytePos + 1], protocol.Payload[bytePos + 2],
                            protocol.Payload[bytePos + 3]};

                            point.Value = BitConverter.ToUInt32(byteReal, 0);

                            UpdatePoint(point);

                            break;

                        case Models.DataType.Long:

                            address[0] = point.Address.Split(".")[0];

                            bytePos = Convert.ToUInt16(address[0]);

                            byte[] byteLong = {protocol.Payload[bytePos], protocol.Payload[bytePos + 1], protocol.Payload[bytePos + 2],
                            protocol.Payload[bytePos + 3], protocol.Payload[bytePos + 4], protocol.Payload[bytePos + 5],
                            protocol.Payload[bytePos + 6], protocol.Payload[bytePos + 7] };

                            point.Value = BitConverter.ToInt64(byteLong, 0);

                            UpdatePoint(point);

                            break;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private async void UpdatePoint(Models.Point point)
        {
            _context.Attach(point).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PointExists(point.PointID))
                {
                    throw;
                }
                else
                {
                    throw;
                }
            }
        }

        private bool PointExists(int id)
        {
            return _context.Point.Any(e => e.PointID == id);
        }

        private void AjustPayloadSize(Protocol protocol)
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
