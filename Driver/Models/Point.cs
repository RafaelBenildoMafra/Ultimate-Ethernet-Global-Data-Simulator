using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EthernetGlobalData.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Point
    {
        [Key]
        public int PointID { get; set; }

        [ForeignKey("Node")]
        public int NodeID { get; set; }

        [Required]
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
