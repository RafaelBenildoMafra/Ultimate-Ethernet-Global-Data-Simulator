using EthernetGlobalData.Data;

namespace EthernetGlobalData.Protocol
{
    public class Producer : IProtocol
    {
        private List<Task> tasks = new List<Task>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ApplicationDbContext _context;

        public Producer(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Start(IList<EthernetGlobalData.Models.Node> nodes, IList<Models.Channel> channels)
        {
            foreach (Models.Node node in nodes)
            {
                if (node.CommunicationType != "Producer")
                    continue;

                Protocol.Header header = new Protocol.Header
                {
                    ID = node.Channel.IP,
                    MajorSignature = node.MajorSignature,
                    MinorSignature = node.MinorSignature,
                    ExchangeID = node.Exchange,
                    Points = node.Points,
                };

                UDP transportLayer = new UDP(node.Channel.IP, node.Channel.Port);

                Protocol protocol = new Protocol(transportLayer, header);

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
            protocol.Write();

            protocol.UpdateMessageNumber();

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
