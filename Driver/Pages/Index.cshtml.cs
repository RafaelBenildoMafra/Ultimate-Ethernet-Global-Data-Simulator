using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EthernetGlobalData.Data;

namespace EthernetGlobalData.Models
{
    public class IndexModel : PageModel
    {
        private readonly EthernetGlobalData.Data.ProtocolContext _context;

        public IndexModel(EthernetGlobalData.Data.ProtocolContext context)
        {
            _context = context;
        }

        public IList<Channel> Channel { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Channel = await _context.Channel.ToListAsync();
        }
    }
}
