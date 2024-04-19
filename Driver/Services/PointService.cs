using EthernetGlobalData.Data;
using EthernetGlobalData.Interfaces;
using EthernetGlobalData.Models;

namespace EthernetGlobalData.Services
{
    public class PointService : IPointService
    {
        private readonly ApplicationDbContext _context = default!;

        public PointService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Point> GetPoints()
        {
            if (_context.Point != null)
            {
                return _context.Point.ToList();
            }
            return new List<Point>();
        }

        public void AddPoint(Point Point)
        {
            if (_context.Point != null)
            {
                _context.Point.Add(Point);
                _context.SaveChanges();
            }
        }

        public void DeletePoint(int id)
        {
            if (_context.Point != null)
            {
                var Point = _context.Point.Find(id);
                if (Point != null)
                {
                    _context.Point.Remove(Point);
                    _context.SaveChanges();
                }
            }
        }
    }
}
