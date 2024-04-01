using System;
using System.Collections;
using EthernetGlobalData.Models;

namespace EthernetGlobalData.Protocol
{
    public class Message
    {   
        public UDP TransportLayer { get; set; }

        public const int HeaderSize = 32;

        public struct Header
        {
            public string ProducerID { get; set; }
            public int ExchangeID { get; set; }
            public int MajorSignature { get; set; }
            public int MinorSignature { get; set; }
            public ICollection<Point> Points { get; set; }   
        }

        public Header MessageHeader { get; set; }

        public int MessageNumber { get; set; }

        public Message(UDP transportLayer, Header messageHeader)
        {
            TransportLayer = transportLayer;
            MessageHeader = messageHeader;
        }

        public void Write()
        {
            byte[] headerBytes = new byte[HeaderSize];
            List<byte> payload = new List<byte>();

            BitConverter.GetBytes(0XD).CopyTo(headerBytes, 0); //PDU type
            BitConverter.GetBytes(0X1).CopyTo(headerBytes, 1); //PDU version number
            BitConverter.GetBytes(MessageNumber).CopyTo(headerBytes, 2); //Request ID

            // Producer ID
            string[] octets = MessageHeader.ProducerID.Split('.');
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

            // Payload
            payload.InsertRange(0, headerBytes);

            foreach (Point point in MessageHeader.Points)
            {
                if (point.DataType != DataType.Boolean)
                {
                    switch (point.DataType)
                    {
                        case DataType.Word:
                            // If the data type is Word, convert the value to Int16, then to bytes, and insert it into the payload
                            short wordValue = Convert.ToInt16(point.Value);
                            byte[] wordBytes = BitConverter.GetBytes(wordValue);
                            payload.InsertRange(HeaderSize + Convert.ToInt16(point.Address), wordBytes);
                            break;
                        case DataType.Real:
                            // If the data type is Real, convert the value to Single, then to bytes, and insert it into the payload
                            float realValue = Convert.ToSingle(point.Value);
                            byte[] realBytes = BitConverter.GetBytes(realValue);
                            payload.InsertRange(HeaderSize + Convert.ToInt16(point.Address), realBytes);
                            break;
                        case DataType.Long:
                            // If the data type is Long, convert the value to Int64, then to bytes, and insert it into the payload
                            long longValue = Convert.ToInt64(point.Value);
                            byte[] longBytes = BitConverter.GetBytes(longValue);
                            payload.InsertRange(HeaderSize + Convert.ToInt16(point.Address), longBytes);
                            break;
                    }
                }
                else
                {
                    string[] address;

                    address = point.Address.Split(".");

                    byte access = payload[HeaderSize + Convert.ToInt32(address[0])];

                    BitArray bitArray = new BitArray(new byte[] { access });
  
                    bitArray.Set(Convert.ToInt32(address[1]), point.Value == 1 ? true : false);

                    // Convert the modified BitArray back to a byte
                    byte[] byteArray = new byte[(bitArray.Length + 7) / 8];
                    bitArray.CopyTo(byteArray, 0);
                    access = byteArray[0];

                    payload[HeaderSize + Convert.ToInt32(address[0])] = access;
                }
            }

            TransportLayer.Send(payload.ToArray());
        }

        public void UpdateMessageNumber()
        {
            MessageNumber ++;
        }
    }
}
