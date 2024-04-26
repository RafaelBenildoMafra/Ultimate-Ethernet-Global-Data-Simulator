using System.Net;
using System.Net.Sockets;
using static EthernetGlobalData.Protocol.Protocol;

namespace EthernetGlobalData.Protocol
{
    public class UDP
    {
        protected static List<UdpClient> Clients = new List<UdpClient>();
        protected UdpClient Client;
        protected IPEndPoint localEP;
        public static bool messageReceived = false;

        public UDP(string ipAddress, int port)
        {
            localEP = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            Client = new UdpClient(localEP);
            Clients.Add(Client);
        }

        public void Connect()
        {
            Client.Connect(localEP);
        }

        public  void Send(byte[] data)
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

        public static void CloseAllConnections()
        {
            foreach (UdpClient client in Clients)
            {
                client.Close();
            }
        }

        public void Close()
        {
            this.Client.Close();
        }
    }
}
