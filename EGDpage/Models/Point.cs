using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Policy;

namespace EthernetGlobalData.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Point
    {
        [Key]
        public int PointID { get; set; }        

        [ForeignKey("NodeName")]
        public int NodeID { get; set; }

        [Required]
        public string? Name { get; set; }
        public Node? Node { get; set; }

        [RegularExpression(@"^\d+\.\d+$", ErrorMessage = "Address is defined by (byte).(bit) Must be in the format (uint).(uint).")]
        public string? Address { get; set; }
        public long? Value { get; set; }        
    }
}
