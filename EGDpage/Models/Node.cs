using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using EthernetGlobalData.Interfaces;

namespace EthernetGlobalData.Models
{
    [Index(nameof(NodeName), IsUnique = true)]
    public class Node
    {
        [Key]
        public int NodeID { get; set; }

        [ForeignKey("Channel")]
        public int ChannelID { get; set; }

        [Required]
        public string? NodeName { get; set; }
        public string? CommunicationType { get; set; }
        public Channel? Channel { get; set; }
        public ushort Exchange { get; set; }
        public ushort MajorSignature { get; set; }
        public ushort MinorSignature { get; set; }
        public ICollection<Point>? Points { get; set; }

        //public Node(string producerID, ushort exchangeID, ushort majorSignature, ushort minorSignature, uint messageNumber)
        //{
        //    this.producerID = producerID;
        //    this.exchangeID = exchangeID;
        //    this.majorSignature = majorSignature;
        //    this.minorSignature = minorSignature;
        //    this.
        //
        //
        //    = messageNumber;
        //}
        //public string ProducerID { get { return producerID; } }
        //public ushort ExchangeID { get { return exchangeID; } }
        //public ushort MajorSignature { get { return majorSignature; } }
        //public ushort MinorSignature { get { return minorSignature; } }
        //public uint MessageNumber { get { return messageNumber; } }

        //public uint UpdateMessage(uint CurrentMessage)
        //{
        //    return messageNumber++;
        //}
    }
}
