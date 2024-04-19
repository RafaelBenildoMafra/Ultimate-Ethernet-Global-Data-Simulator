using EthernetGlobalData.Models;

namespace EthernetGlobalData.Interfaces
{
    public interface IChannelService
    {
        IList<Channel> GetChannels();

        void AddChannel(Channel Channel);

        void DeleteChannel(int id);
    }
}
