using EthernetGlobalData.Data;
using EthernetGlobalData.Models;
using NuGet.Common;
using System.Collections;
using System.Threading;
using static EthernetGlobalData.Protocol.Protocol;

namespace EthernetGlobalData.Protocol
{
    public class Protocol
    {        
        private static CancellationTokenSource token = new CancellationTokenSource();
        private static IServiceScopeFactory? _serviceProvider;
        public const int HeaderSize = 32;
        private static List<Thread> threads = new List<Thread>();
        public static UDP TransportLayer { get; set; }

        public class MessageData
        {
            public List<byte> Payload { get; set; }
            public MessageStatus Status { get; set; }

            public MessageData(){ }
        }
        public enum MessageStatus
        {
            Recieved,
            Sent,
            Readed,
            Stored,
            Discarded,
            ErrorRecieving,
            ErrorReading,
            ErrorSending,
            ErrorStoring
        }        
        public int MessageNumber { get; set; }

        public Protocol(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceProvider = serviceScopeFactory;
        }

        public async Task Start(IList<Models.Node> nodes, IList<Models.Channel> channels)
        {
            try
            {
                foreach (Channel channel in channels)
                {
                    Thread thread = new Thread(() =>
                    {
                        UDP transportLayer = new UDP(channel.IP, channel.Port);

                        StartChannel(nodes, ref transportLayer);
                    });
                    thread.Name = channel.ChannelName;
                    thread.Start();
                    threads.Add(thread);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
            }
        }

        private static void StartChannel(IList<Models.Node> nodes, ref UDP transportLayer)
        {
            try
            {                
                foreach (Node node in nodes)
                {
                    if (node.CommunicationType == "Consumer")
                    {
                        Consumer.MessageHeader header = new Consumer.MessageHeader
                        {
                            ID = node.Channel.IP,
                            MajorSignature = node.MajorSignature,
                            MinorSignature = node.MinorSignature,
                            ExchangeID = node.Exchange,
                            Points = node.Points,
                        };

                        new Consumer(header, new MessageData());
                    }
                    else
                    {
                        Producer.MessageHeader header = new Producer.MessageHeader
                        {
                            MessageNumber = 0,
                            ID = node.Channel.IP,
                            MajorSignature = node.MajorSignature,
                            MinorSignature = node.MinorSignature,
                            ExchangeID = node.Exchange,
                            Points = node.Points,
                        };

                        new Producer(header, new MessageData());
                    }
                }

                Communicate(ref transportLayer);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
            }
        }

        public void Stop()
        {
            UDP.CloseAllConnections();
            token.Cancel();
        }

        public static MessageStatus Communicate(ref UDP transportLayer)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    Consumer.Start(transportLayer, token, _serviceProvider);
                    Producer.Start(transportLayer, token, _serviceProvider);
                }
                return MessageStatus.Recieved;
            }
            catch (Exception ex)
            {   
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ResetColor();
                return MessageStatus.ErrorRecieving;
            }
        }
    }
}
