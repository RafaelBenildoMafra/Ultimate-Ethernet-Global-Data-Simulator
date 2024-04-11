using System.Threading;

namespace EthernetGlobalData.Protocol
{
    public interface IProtocol
    {
        void Start(IList<EthernetGlobalData.Models.Node> nodes);

        void Stop();

        Task Communicate(Protocol protocol);
    }
}
