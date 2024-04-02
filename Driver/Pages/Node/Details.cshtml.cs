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
    public class DetailsModel : PageModel
    {
        private readonly ProtocolContext _context;

        public DetailsModel(ProtocolContext context)
        {
            _context = context;
        }

        public EthernetGlobalData.Models.Node Node { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var node = await _context.Node
                .Include(p=> p.Channel)
                .FirstOrDefaultAsync(m => m.NodeID == id);

            if (node == null)
            {
                return NotFound();
            }
            else
            {
                Node = node;
            }
            return Page();
        }
    }
}
