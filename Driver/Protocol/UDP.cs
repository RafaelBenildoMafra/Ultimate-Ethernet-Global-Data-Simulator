using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EthernetGlobalData.Protocol
{
    public class UDP
    {
        protected Socket socket;
        protected IPEndPoint endPoint;
        public UDP(string ipAddress, int port)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
        }
        public void Send(byte[] data)
        {
            socket.SendTo(data, endPoint);
        }
    }
}
