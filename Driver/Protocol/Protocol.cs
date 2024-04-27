using EthernetGlobalData.Data;
using EthernetGlobalData.Models;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using NuGet.Common;
using System.Collections;
using System.Threading;
using static EthernetGlobalData.Protocol.Protocol;

namespace EthernetGlobalData.Protocol
{
    public class Protocol
    {        
        private static CancellationTokenSource source = new CancellationTokenSource();
        private static IServiceScopeFactory? _serviceProvider;
        public const int HeaderSize = 32;
        private static List<Thread> threads = new List<Thread>();

        public static UDP TransportLayer { get; set; }

        public struct MessageHeader
        {
            public string ID { get; set; }
            public int ExchangeID { get; set; }
            public int MajorSignature { get; set; }
            public int MinorSignature { get; set; }
            public int MessageNumber { get; set; }
            public ICollection<Models.Point> Points { get; set; }
            public ulong Period { get; set; }
        }

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
            source = token;
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
                    MessageHeader header = new MessageHeader 
                    {
                        ID = node.Channel.IP,
                        MajorSignature = node.MajorSignature,
                        MinorSignature = node.MinorSignature,
                        ExchangeID = node.Exchange,
                        Points = node.Points,
                        Period = node.Period,
                    };

                    if (node.CommunicationType == "Consumer")
                    {
                        new Consumer(header, new Message(new List<byte>(), new MessageStatus()), source);
                    }
                    else
                    {
                        new Producer(header, new Message(new List<byte>(), new MessageStatus()), source);
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

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Communication Stopped by User");
            Console.ResetColor();
        }

        public static MessageStatus Communicate(UDP transportLayer)
        {
            try
            {   
                MessageStatus messageStatus = new MessageStatus();

                if (Consumer.Consumers.Any())
                {
                    Task.Run(async () =>
                    {
                        messageStatus = await Consumer.Start(transportLayer, _serviceProvider);
                    },source.Token);
                }
                if (Producer.Producers.Any())
                {
                    Task.Run(async () =>
                    {
                        messageStatus = await Producer.Start(transportLayer, _serviceProvider);
                    },source.Token);
                }
                
                if(!Consumer.Consumers.Any() && !Producer.Producers.Any())
                {
                    source.Cancel();
                    messageStatus = MessageStatus.Discarded;
                }

                return MessageStatus.Sent;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ResetColor();

                return MessageStatus.ErrorSending;
            }            
        }
    }
}
