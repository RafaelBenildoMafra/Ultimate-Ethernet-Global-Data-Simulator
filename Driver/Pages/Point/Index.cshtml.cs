using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EthernetGlobalData.Data;
using EthernetGlobalData.Models;
using EthernetGlobalData.Protocol;

namespace EthernetGlobalData.Pages.Point
{
    public class IndexModel : PageModel
    {
        private readonly ProtocolContext _context;

        public IndexModel(ProtocolContext context)
        {
            _context = context;
        }

        public IList<EthernetGlobalData.Models.Point> Point { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Point = await _context.Point
                .Include(p => p.Node)
                .ToListAsync();
            
        }
    }
}
