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

namespace EthernetGlobalData.Pages.Connect
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
                .Include(n => n.Channel)
                .Include(n => n.Points)
                .ToListAsync();            
        }

        public IActionResult OnPost()
        {
            _ = OnGetAsync();

            Producer.Start(Node);

            return RedirectToPage("/Connect/Index"); // Redirect to a different page after the method call
        }
    }
}
