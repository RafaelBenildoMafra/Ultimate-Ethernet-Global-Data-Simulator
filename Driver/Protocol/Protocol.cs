using EthernetGlobalData.Models;
using System.Collections;

namespace EthernetGlobalData.Protocol
{
    public class Protocol
    {
        public UDP TransportLayer { get; set; }
        public const int HeaderSize = 32;
        public List<byte> Payload = new List<byte>();

        public struct Header
        {
            public string ID { get; set; }
            public int ExchangeID { get; set; }
            public int MajorSignature { get; set; }
            public int MinorSignature { get; set; }
            public ICollection<Point> Points { get; set; }
        }

        public Header MessageHeader { get; set; }

        public int MessageNumber { get; set; }

        public Protocol(UDP transportLayer, Header messageHeader)
        {
            TransportLayer = transportLayer;
            MessageHeader = messageHeader;
        }

        public void Write()
        {
            byte[] headerBytes = new byte[HeaderSize];

            BitConverter.GetBytes(0XD).CopyTo(headerBytes, 0); //PDU type
            BitConverter.GetBytes(0X1).CopyTo(headerBytes, 1); //PDU version number
            BitConverter.GetBytes(MessageNumber).CopyTo(headerBytes, 2); //Request ID

            // Producer ID
            string[] octets = MessageHeader.ID.Split('.');
            BitConverter.GetBytes(Convert.ToInt16(octets[0])).CopyTo(headerBytes, 4);
            BitConverter.GetBytes(Convert.ToInt16(octets[1])).CopyTo(headerBytes, 5);
            BitConverter.GetBytes(Convert.ToInt16(octets[2])).CopyTo(headerBytes, 6);
            BitConverter.GetBytes(Convert.ToInt16(octets[3])).CopyTo(headerBytes, 7);

            // Exchange ID
            BitConverter.GetBytes(MessageHeader.ExchangeID).CopyTo(headerBytes, 8);

            // Timestamp
            byte[] byteTime = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            byteTime.CopyTo(headerBytes, 12);

            // Status
            BitConverter.GetBytes(0x00).CopyTo(headerBytes, 20);

            // Signature
            BitConverter.GetBytes(MessageHeader.MinorSignature).CopyTo(headerBytes, 24);
            BitConverter.GetBytes(MessageHeader.MajorSignature).CopyTo(headerBytes, 25);

            // Reserved
            BitConverter.GetBytes(0x00000000).CopyTo(headerBytes, 28);

            //////////////////// Payload ////////////////////
            Payload.InsertRange(0, headerBytes);

            foreach (Point point in MessageHeader.Points)
            {
                if (point.DataType != DataType.Boolean)
                {
                    switch (point.DataType)
                    {
                        case DataType.Word:
                            short wordValue = Convert.ToInt16(point.Value);
                            byte[] wordBytes = BitConverter.GetBytes(wordValue);
                            Payload.InsertRange(HeaderSize + Convert.ToInt16(point.Address), wordBytes);
                            break;
                        case DataType.Real:
                            float realValue = Convert.ToSingle(point.Value);
                            byte[] realBytes = BitConverter.GetBytes(realValue);
                            Payload.InsertRange(HeaderSize + Convert.ToInt16(point.Address), realBytes);
                            break;
                        case DataType.Long:
                            long longValue = Convert.ToInt64(point.Value);
                            byte[] longBytes = BitConverter.GetBytes(longValue);
                            Payload.InsertRange(HeaderSize + Convert.ToInt16(point.Address), longBytes);
                            break;
                    }
                }
                else
                {
                    string[] address;

                    address = point.Address.Split(".");

                    byte access = Payload[HeaderSize + Convert.ToInt32(address[0])];

                    BitArray bitArray = new BitArray(new byte[] { access });

                    bitArray.Set(Convert.ToInt32(address[1]), point.Value == 1 ? true : false);

                    // Convert the modified BitArray back to a byte
                    byte[] byteArray = new byte[(bitArray.Length + 7) / 8];
                    bitArray.CopyTo(byteArray, 0);
                    access = byteArray[0];

                    Payload[HeaderSize + Convert.ToInt32(address[0])] = access;
                }
            }

            TransportLayer.Send(Payload.ToArray());
        }

        public List<byte> Read()
        {
            string messageState = "Header";

            byte[] recievedBytes = TransportLayer.Receive();

            if (recievedBytes != null)
            {
                try
                {
                    switch (messageState)
                    {
                        case "Header":

                            if (recievedBytes[0] == 0x0D)
                            {
                                goto case "Producer";
                            }
                            break;

                        case "Producer":

                            string[] parts = this.MessageHeader.ID.Split('.');

                            for (int i = 0; i < 4; i++)
                            {
                                if (recievedBytes[4 + i] != byte.Parse(parts[i]))
                                {
                                    break;
                                }
                                else if (i == 3)
                                {
                                    goto case "Exchange";
                                }
                            }

                            break;

                        case "Exchange": //Exchange Identifier

                            byte[] exchange = new byte[4];

                            for (int i = 0; i < 4; i++)
                            {
                                exchange[i] = recievedBytes[8 + i];
                            }

                            if (BitConverter.ToInt32(exchange, 0) == this.MessageHeader.ExchangeID)
                            {
                                goto case "Signature";
                            }

                            break;

                        case "Signature":

                            if (recievedBytes[24] == this.MessageHeader.MinorSignature && recievedBytes[25]
                                == this.MessageHeader.MajorSignature)
                            {
                                goto case "Payload";
                            }

                            break;

                        case "Payload": //Payload Verification

                            if (recievedBytes.Length > 1432)
                            {
                                break;
                            }
                            else
                            {
                                for (int i = 32; i < recievedBytes.Length; i++)
                                {
                                    Payload.Add(recievedBytes[i]);
                                }

                                return Payload;
                            }
                    }
                    return null;
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }


        public void UpdateMessageNumber()
        {
            MessageNumber++;
        }
    }
}
