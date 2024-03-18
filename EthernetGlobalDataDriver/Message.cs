using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EthernetGlobalDataDriver
{
    internal class Message
    {

        // Private fields
        public byte[] Header = new byte[32];
        public byte[] Payload = new byte[1368];

        // Constructor
        //public CommandMessage(List<byte> txBuffer, List<byte> rxBuffer)
        //{
        //    this.txBuffer = txBuffer;
        //    this.rxBuffer = rxBuffer;
        //}

        // Properties
        //public List<byte> TxBuffer { get { return txBuffer; } }
        //public List<byte> RxBuffer { get { return rxBuffer; } }
        
    }
}
