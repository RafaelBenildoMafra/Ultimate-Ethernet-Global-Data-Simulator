﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using EthernetGlobalData.Data;
using EthernetGlobalData.Models;

namespace EthernetGlobalData.Pages.Point
{
    public class DetailsModel : PageModel
    {
        private readonly ProtocolContext _context;

        public DetailsModel(ProtocolContext context)
        {
            _context = context;
        }

        public EthernetGlobalData.Models.Point Point { get; set; } = default!;
        
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var point = await _context.Point
                .Include(p => p.Node.Channel)
                .FirstOrDefaultAsync(m => m.PointID == id);            

            if (point == null)
            {
                return NotFound();               
            }
            else
            {                   
                Point = point;
            }
            return Page();
        }
    }
}
