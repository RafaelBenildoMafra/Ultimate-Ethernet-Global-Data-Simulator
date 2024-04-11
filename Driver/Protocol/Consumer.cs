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
            protocol.Read();

            if (protocol.Payload == null)
            {   
                protocol.TransportLayer.Close();

                cancellationTokenSource.Cancel();

                return;
            }

            TreatPayload(protocol);
        }

        public async void TreatPayload(Protocol protocol)
        {
            foreach (Models.Point point in protocol.MessageHeader.Points)
            {
                switch(point.DataType)
                {
                    case Models.DataType.Boolean:

                        string[] address = point.Address.Split(".");

                        ushort bytePos = Convert.ToUInt16(address[0]);
                        ushort bitPos = Convert.ToUInt16(address[1]);

                        BitArray bitArray = new BitArray(protocol.Payload[bytePos]);

                        point.Value = bitArray[bitPos] == true ? 1 : 0;

                        _context.Attach(point).State = EntityState.Modified;

                        try
                        {
                            await _context.SaveChangesAsync();
                        }
                        catch (DbUpdateConcurrencyException)
                        {
                            if (!PointExists(point.PointID))
                            {
                                break;
                            }
                            else
                            {
                                throw;
                            }
                        }

                        break;
                }
            }
        }

        private bool PointExists(int id)
        {
            return _context.Point.Any(e => e.PointID == id);
        }
    }
}
