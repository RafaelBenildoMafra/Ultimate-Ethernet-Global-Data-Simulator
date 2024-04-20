//using EthernetGlobalData.Data;
//using static EthernetGlobalData.Protocol.Consumer;
//using static EthernetGlobalData.Protocol.Protocol;
//using System.Collections;

//namespace EthernetGlobalData.Protocol
//{
//    public class Producer : IProtocol
//    {
//        private List<Task> tasks = new List<Task>();
//        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
//        private readonly ApplicationDbContext _context;

//        public Producer(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task Communicate(Protocol protocol)
//        {
//            protocol.Write();

//            protocol.UpdateMessageNumber();

//            await Task.Delay(TimeSpan.FromSeconds(1));
//        }

//        public void Write()
//        {
//            byte[] headerBytes = new byte[HeaderSize];

//            BitConverter.GetBytes(0XD).CopyTo(headerBytes, 0); //PDU type
//            BitConverter.GetBytes(0X1).CopyTo(headerBytes, 1); //PDU version number
//            BitConverter.GetBytes(MessageNumber).CopyTo(headerBytes, 2); //Request ID

//            // Producer ID
//            string[] octets = MessageHeader.ID.Split('.');
//            BitConverter.GetBytes(Convert.ToInt16(octets[0])).CopyTo(headerBytes, 4);
//            BitConverter.GetBytes(Convert.ToInt16(octets[1])).CopyTo(headerBytes, 5);
//            BitConverter.GetBytes(Convert.ToInt16(octets[2])).CopyTo(headerBytes, 6);
//            BitConverter.GetBytes(Convert.ToInt16(octets[3])).CopyTo(headerBytes, 7);

//            // Exchange ID
//            BitConverter.GetBytes(MessageHeader.ExchangeID).CopyTo(headerBytes, 8);

//            // Timestamp
//            byte[] byteTime = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
//            byteTime.CopyTo(headerBytes, 12);

//            // Status
//            BitConverter.GetBytes(0x00).CopyTo(headerBytes, 20);

//            // Signature
//            BitConverter.GetBytes(MessageHeader.MinorSignature).CopyTo(headerBytes, 24);
//            BitConverter.GetBytes(MessageHeader.MajorSignature).CopyTo(headerBytes, 25);

//            // Reserved
//            BitConverter.GetBytes(0x00000000).CopyTo(headerBytes, 28);

//            //////////////////// Payload ////////////////////
//            messageData.Bytes.InsertRange(0, headerBytes);

//            foreach (Point point in MessageHeader.Points)
//            {
//                if (point.DataType != DataType.Boolean)
//                {
//                    switch (point.DataType)
//                    {
//                        case DataType.Word:
//                            short wordValue = Convert.ToInt16(point.Value);
//                            byte[] wordBytes = BitConverter.GetBytes(wordValue);
//                            messageData.Bytes.InsertRange(HeaderSize + Convert.ToInt16(point.Address), wordBytes);
//                            break;
//                        case DataType.Real:
//                            float realValue = Convert.ToSingle(point.Value);
//                            byte[] realBytes = BitConverter.GetBytes(realValue);
//                            messageData.Bytes.InsertRange(HeaderSize + Convert.ToInt16(point.Address), realBytes);
//                            break;
//                        case DataType.Long:
//                            long longValue = Convert.ToInt64(point.Value);
//                            byte[] longBytes = BitConverter.GetBytes(longValue);
//                            messageData.Bytes.InsertRange(HeaderSize + Convert.ToInt16(point.Address), longBytes);
//                            break;
//                    }
//                }
//                else
//                {
//                    string[] address;

//                    address = point.Address.Split(".");

//                    byte access = messageData.Bytes[HeaderSize + Convert.ToInt32(address[0])];

//                    BitArray bitArray = new BitArray(new byte[] { access });

//                    bitArray.Set(Convert.ToInt32(address[1]), point.Value == 1 ? true : false);

//                    // Convert the modified BitArray back to a byte
//                    byte[] byteArray = new byte[(bitArray.Length + 7) / 8];
//                    bitArray.CopyTo(byteArray, 0);
//                    access = byteArray[0];

//                    messageData.Bytes[HeaderSize + Convert.ToInt32(address[0])] = access;
//                }
//            }

//            TransportLayer.Send(messageData.Bytes.ToArray());
//        }

//        public void UpdateMessageNumber()
//        {
//            MessageNumber++;
//        }
//    }
//}
