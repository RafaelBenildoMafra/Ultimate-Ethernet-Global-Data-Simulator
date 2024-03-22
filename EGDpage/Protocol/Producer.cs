using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using EthernetGlobalData.Interfaces;
using Microsoft.EntityFrameworkCore;
using EthernetGlobalData.Data;
using EthernetGlobalData.Services;
using Humanizer;

namespace EthernetGlobalData.Protocol
{
    public class Producer
    {

        private readonly ProtocolContext _context;

        public Producer(ProtocolContext context)
        {
            _context = context;
        }

        public static void Start(IList<EthernetGlobalData.Models.Node> nodes)
        {
            List<Task> tasks = new List<Task>();

            foreach (Models.Node node in nodes)
            {
                if (node.CommunicationType != "Producer")
                    continue;                                                

                Message.Header header = new Message.Header
                {
                    ProducerID = node.Channel.IP,
                    MajorSignature = node.MajorSignature,
                    MinorSignature = node.MinorSignature,
                    ExchangeID = node.Exchange,
                };

                UDP transportLayer = new UDP(node.Channel.IP, node.Channel.Port);

                Message message = new Message(transportLayer, header);

                tasks.Add(Communicate(message));
            }
        }

        private static async Task Communicate(Message message)
        {            
            message.WriteMessage();

            message.UpdateMessageNumber();            

            Console.WriteLine("EGD Message sent to the broadcast address");

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
