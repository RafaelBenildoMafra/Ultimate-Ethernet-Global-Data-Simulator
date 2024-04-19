namespace EthernetGlobalData.Protocol
{
    public interface IProtocol
    {
        Task Start(IList<EthernetGlobalData.Models.Node> nodes, IList<Models.Channel> channels);

        void Stop();

        Task Communicate(Protocol protocol);
    }
}
