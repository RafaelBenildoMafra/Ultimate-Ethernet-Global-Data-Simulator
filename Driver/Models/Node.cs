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
    }
}
