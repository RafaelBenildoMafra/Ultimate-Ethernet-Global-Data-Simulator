using System.Threading;

namespace EthernetGlobalData.Protocol
{
    public interface IProtocol
    {
        void Start(IList<EthernetGlobalData.Models.Node> nodes, IList<Models.Channel> channels);

        void Stop();

        Task Communicate(Protocol protocol);
    }
}
