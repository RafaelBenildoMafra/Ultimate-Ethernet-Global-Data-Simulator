using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace EthernetGlobalData.Models
{
    [Index(nameof(ChannelName), IsUnique = true)]
    public class Channel
    {
        [Key]
        public int ChannelID { get; set; }

        [Required]
        public string? ChannelName { get; set; }
        public string? IP { get; set; }
        public int Port { get; set; }
    }
}
