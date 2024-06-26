﻿using EthernetGlobalData.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EthernetGlobalData.Pages.Point
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public EthernetGlobalData.Models.Point Point { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var point = await _context.Point.FirstOrDefaultAsync(m => m.PointID == id);

            if (point == null)
            {
                return NotFound();
            }

            Point = point;
            ViewData["NodeID"] = new SelectList(_context.Node, "NodeID", "NodeName");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Point).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PointExists(Point.PointID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool PointExists(int id)
        {
            return _context.Point.Any(e => e.PointID == id);
        }
    }
}
