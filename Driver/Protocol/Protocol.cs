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
        private static CancellationTokenSource Token = new CancellationTokenSource();
        private static IServiceScopeFactory? _serviceProvider;
        public const int HeaderSize = 32;
        private static List<Thread> threads = new List<Thread>();
        public static UDP TransportLayer { get; set; }

        public class Message
        {
            public List<byte> Data { get; set; }
            public MessageStatus Status { get; set; }

            public Message(List<byte>  payload, MessageStatus status)
            {
                Data = payload;
                Status = status;
            }
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
            ErrorStoring,
        }        
        public int MessageNumber { get; set; }

        public Protocol(IServiceScopeFactory serviceScopeFactory, CancellationTokenSource token)
        {   
            _serviceProvider = serviceScopeFactory;
            Token = token;
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
                Consumer.Consumers.Clear();
                Producer.Producers.Clear();

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

                        new Consumer(header, new Message(new List<byte>(), new MessageStatus()));
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

                        new Producer(header, new Message(new List<byte>(), new MessageStatus()));
                    }
                }
                
                Communicate(transportLayer);
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
            Producer.Stop();
            Consumer.Stop();
            UDP.CloseAllConnections();            
        }

        public static async void Communicate(UDP transportLayer)
        {
            try
            {
                MessageStatus messageStatus = new MessageStatus();

                //while (!Token.IsCancellationRequested)
                //{
                    Thread.Sleep(1000);

                    if (Consumer.Consumers.Any())
                    {
                        messageStatus = Consumer.Start(transportLayer, _serviceProvider);
                    }
                    else if (Producer.Producers.Any())
                    {
                        messageStatus = Producer.Start(transportLayer, _serviceProvider);
                    }
                    else
                    {
                        Token.Cancel();
                        messageStatus = MessageStatus.Discarded;
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(transportLayer.ToString() + messageStatus);
                    Console.ResetColor();
                //}
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ResetColor();
            }
        }
    }
}
