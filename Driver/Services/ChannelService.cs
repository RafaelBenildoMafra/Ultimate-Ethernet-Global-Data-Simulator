using EthernetGlobalData.Data;
using EthernetGlobalData.Interfaces;
using EthernetGlobalData.Models;

namespace EthernetGlobalData.Services
{
    public class ChannelService : IChannelService
    {
        private readonly ProtocolContext _context = default!;

        public ChannelService(ProtocolContext context)
        {
            _context = context;
        }

        public IList<Channel> GetChannels()
        {
            if (_context.Node != null)
            {
                return _context.Channel.ToList();
            }
            return new List<Channel>();
        }

        public void AddChannel(Channel Channel)
        {
            if (_context.Channel != null)
            {
                _context.Channel.Add(Channel);
                _context.SaveChanges();
            }
        }

        public void DeleteChannel(int id)
        {
            if (_context.Channel != null)
            {
                var Channel = _context.Channel.Find(id);
                if (Channel != null)
                {
                    _context.Channel.Remove(Channel);
                    _context.SaveChanges();
                }
            }
        }
    }
}
