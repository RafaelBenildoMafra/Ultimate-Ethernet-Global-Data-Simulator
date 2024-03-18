using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EthernetGlobalData.Data;
using EthernetGlobalData.Models;

namespace EthernetGlobalData.Pages.Node
{
    public class IndexModel : PageModel
    {
        private readonly ProtocolContext _context;

        public IndexModel(ProtocolContext context)
        {
            _context = context;
        }

        public IList<EthernetGlobalData.Models.Node> Node { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Node = await _context.Node
                .Include(n => n.Channel).ToListAsync();
        }
    }
}
