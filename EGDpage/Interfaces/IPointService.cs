using EthernetGlobalData.Models;

namespace EthernetGlobalData.Interfaces
{
    public interface IPointService
    {
        IList<Point> GetPoints();

        void AddPoint(Point Point);

        void DeletePoint(int id);
    }
}
