using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.RazorPages;
using перенос_бд_на_Web.Models;
using Microsoft.EntityFrameworkCore;

namespace перенос_бд_на_Web.Pages.TM
{

    public class SomeDataTM : PageModel
    {
        private readonly ApplicationContext _telemetry_Context;
        public List<NedostovernayaTM> Tm { get; set; }
        public SomeDataTM(ApplicationContext db)
        {
            _telemetry_Context = db;
        }
        public void OnGet()
        {
            Tm = _telemetry_Context.tm.AsNoTracking().ToList();
        }
    }
}


