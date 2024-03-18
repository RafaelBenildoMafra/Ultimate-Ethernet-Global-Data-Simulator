using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;

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

        //private string GetLocalIpAddress()
        //{
        //    string hostName = Dns.GetHostName(); // Retrive the Name of HOST

        //    Console.WriteLine(hostName);

        //    // Get the IP
        //    string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();

        //    return myIP;
        //}
    }
}
