using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;
using EthernetGlobalData.Interfaces;

namespace EthernetGlobalData.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Point
    {
        [Key]
        public int PointID { get; set; }        

        [ForeignKey("Node")]
        public int NodeID { get; set; }
        public string? Name { get; set; }
        public Node? Node { get; set; }
        public DataType DataType { get; set; }      
        public string? Address { get; set; }
        public long? Value { get; set; }
    }

    public enum DataType
    {
        Boolean,
        Word,
        Real,
        Long
    }
}
