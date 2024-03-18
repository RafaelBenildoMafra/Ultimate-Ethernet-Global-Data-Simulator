using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthernetGlobalDataDriver
{
    internal class Node
    {
        private string producerID = "";
        private ushort exchangeID = 0;
        private ushort majorSignature = 0;
        private ushort minorSignature = 0;
        private uint messageNumber = 0;

        public Node(string producerID, ushort exchangeID, ushort majorSignature, ushort minorSignature, uint messageNumber)
        {
            this.producerID = producerID;
            this.exchangeID = exchangeID;
            this.majorSignature = majorSignature;
            this.minorSignature = minorSignature;
            this.messageNumber = messageNumber;
        }
        public string ProducerID { get { return producerID; } }
        public ushort ExchangeID { get { return exchangeID; } }
        public ushort MajorSignature { get { return majorSignature; } }
        public ushort MinorSignature { get { return minorSignature; } }
        public uint MessageNumber { get { return messageNumber; } }

        public uint UpdateMessage(uint CurrentMessage)
        {
            return messageNumber++;
        }
    }
}
