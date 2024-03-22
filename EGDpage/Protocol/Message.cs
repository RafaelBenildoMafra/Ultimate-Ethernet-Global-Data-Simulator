using System;

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
        }

        public Header MessageHeader { get; set; }

        public int MessageNumber { get; set; }

        public Message(UDP transportLayer, Header messageHeader)
        {
            TransportLayer = transportLayer;
            MessageHeader = messageHeader;
        }

        public void WriteMessage()
        {
            byte[] headerBytes = new byte[HeaderSize];

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
            BitConverter.GetBytes(0xFF).CopyTo(headerBytes, 32);

            TransportLayer.Send(headerBytes);
        }

        public void UpdateMessageNumber()
        {
            MessageNumber ++;
        }
    }
}
