using EthernetGlobalDataDriver;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPAddress broadcast = IPAddress.Parse("192.168.1.255");

        string myIP = GetLocalIPAddress();

        string ProducerID = myIP;
        ushort exchangeID = 0;
        ushort majorSignature = 0;
        ushort minorSignature = 0;
        uint messageNumber = 0;


        Node node = new Node(ProducerID, exchangeID, majorSignature, minorSignature, messageNumber);

        Message message = new Message();

        BuildHeader(message, node);

        byte[] sendbuf = Encoding.ASCII.GetBytes(args[0]);
 
        IPEndPoint ep = new IPEndPoint(broadcast, 18246);

        s.SendTo(sendbuf, ep);

        Console.WriteLine("Message sent to the broadcast address");
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    public static void BuildHeader(Message message, Node node)
    {
        //PDU type
        message.Header[0] = 0x0D;

        //PDU version number
        message.Header[1] = 0x01;

        //Request ID
        uint MessageNumber = node.UpdateMessage(node.MessageNumber);

        message.Header[2] = Convert.ToByte((MessageNumber & 0xFF));
        message.Header[3] = Convert.ToByte(((MessageNumber >> 8) & 0xFF));

        //Producer ID
        string[] octets = node.ProducerID.Split('.');
        
        message.Header[4] =  Convert.ToByte((octets[3]));
        message.Header[5] =  Convert.ToByte(Convert.ToInt16(octets[2]));
        message.Header[6] =  Convert.ToByte(Convert.ToInt16(octets[1]));
        message.Header[7] =  Convert.ToByte(Convert.ToInt16(octets[0]));

        //Exchange ID
        message.Header[8] = Convert.ToByte(node.ExchangeID & 0xFF); 
        message.Header[9] = Convert.ToByte((node.ExchangeID >> 8) & 0xFF);
        message.Header[10] =  Convert.ToByte((node.ExchangeID >> 16) & 0xFF); 
        message.Header[11] =  Convert.ToByte((node.ExchangeID >> 24) & 0xFF); 

        //Timestamp
        DateTime timestamp = DateTime.UtcNow;
        byte[] byteTime = BitConverter.GetBytes(DateTime.Now.Ticks);

        message.Header[12] = byteTime[7];
        message.Header[13] = byteTime[6]; 
        message.Header[14] = byteTime[5]; 
        message.Header[15] = byteTime[4]; 
        message.Header[16] = byteTime[3]; 
        message.Header[17] = byteTime[2]; 
        message.Header[18] = byteTime[1]; 
        message.Header[19] = byteTime[0]; 

        //Status
        message.Header[20] = 0x00; 
        message.Header[21] = 0x00; 
        message.Header[22] = 0x00; 
        message.Header[23] = 0x00; 

        //Signature
        message.Header[24] = Convert.ToByte(node.MinorSignature); 
        message.Header[25] = Convert.ToByte(node.MajorSignature); 
        message.Header[26] = 0x00; 
        message.Header[27] = 0x00; 

        //Reserved
        message.Header[28] = 0x00; 
        message.Header[29] = 0x00; 
        message.Header[30] = 0x00; 
        message.Header[31] = 0x00;
    }


}