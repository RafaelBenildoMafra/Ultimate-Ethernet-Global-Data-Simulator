using EthernetGlobalData.Data;
using EthernetGlobalData.Models;

namespace EthernetGlobalData.Services
{
    public class NodeService
    {
        private readonly ProtocolContext _context = default!;

        public NodeService(ProtocolContext context)
        {
            _context = context;
        }

        public IList<Node> GetNodes()
        {
            if (_context.Node != null)
            {
                return _context.Node.ToList();
            }
            return new List<Node>();
        }

        public void AddNode(Node Node)
        {
            if (_context.Node != null)
            {
                _context.Node.Add(Node);
                _context.SaveChanges();
            }
        }

        public void DeleteNode(int id)
        {
            if (_context.Node != null)
            {
                var Node = _context.Node.Find(id);
                if (Node != null)
                {
                    _context.Node.Remove(Node);
                    _context.SaveChanges();
                }
            }
        }
    }
}
