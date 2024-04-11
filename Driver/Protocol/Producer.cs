using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EthernetGlobalData.Interfaces;
using Microsoft.EntityFrameworkCore;
using EthernetGlobalData.Data;
using EthernetGlobalData.Services;
using Humanizer;

namespace EthernetGlobalData.Protocol
{
    public class Producer : IProtocol
    {
        private List<Task> tasks = new List<Task>();
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ProtocolContext _context;

        public Producer(ProtocolContext context)
        {
            _context = context;
        }

        public void Start(IList<EthernetGlobalData.Models.Node> nodes)
        {
            foreach (Models.Node node in nodes)
            {
                if (node.CommunicationType != "Producer")
                    continue;                                                
                
                Protocol.Header header = new Protocol.Header
                {
                    ID = node.Channel.IP,
                    MajorSignature = node.MajorSignature,
                    MinorSignature = node.MinorSignature,
                    ExchangeID = node.Exchange,
                    Points = node.Points,
                };

                UDP transportLayer = new UDP(node.Channel.IP, node.Channel.Port);

                Protocol protocol = new Protocol(transportLayer, header);

                tasks.Add(Communicate(protocol));
            }
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException)
            {
            }

            tasks.Clear();
        }

        public async Task Communicate(Protocol protocol)
        {            
            protocol.Write();

            protocol.UpdateMessageNumber();   

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
