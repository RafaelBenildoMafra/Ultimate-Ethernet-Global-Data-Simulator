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
    public class DeleteModel : PageModel
    {
        private readonly ProtocolContext _context;

        public DeleteModel(ProtocolContext context)
        {
            _context = context;
        }

        [BindProperty]
        public EthernetGlobalData.Models.Node Node { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var node = await _context.Node.FirstOrDefaultAsync(m => m.NodeID == id);

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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var node = await _context.Node.FindAsync(id);
            if (node != null)
            {
                Node = node;
                _context.Node.Remove(Node);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
