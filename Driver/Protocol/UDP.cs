using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace EthernetGlobalData.Protocol
{
    public class UDP
    {
        protected UdpClient Client;
        protected IPEndPoint localEP;
        public static bool messageReceived = false;

        public UDP(string ipAddress, int port)
        {
            localEP = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            Client = new UdpClient(localEP);
        }

        public void Send(byte[] data)
        {
            try
            {
                Client.Send(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data: {ex.Message}");                
            }
        }

        public byte[] Receive()
        {
            try
            {
                byte[] receiveBytes = this.Client.Receive(ref this.localEP);

                return receiveBytes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data: {ex.Message}");

                return null;
            }
        }

        public void Close()
        { 
            this.Client.Close();
        }
    }
}
