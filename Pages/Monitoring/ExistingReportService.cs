using Microsoft.EntityFrameworkCore;
using перенос_бд_на_Web.Models;

namespace перенос_бд_на_Web.Pages.Monitoring
{
    public class ExistingReportService
    {
        private readonly ApplicationContext _context;

        public ExistingReportService(ApplicationContext context)
        {
            _context = context;
        }
        public async Task<List<ReportModel>> GetAllReportsAsync()
        {
            return await _context.report
                .Select(r => new ReportModel
                {
                    Id = r.id_report,
                    ReportDate = r.report_date.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToListAsync();
        }
        public async Task<Report> LoadReportAsync(Guid reportId)
        {
            var report = await _context.report.FindAsync(reportId);
            if (report == null)
            {
                throw new KeyNotFoundException("Report not found.");
            }

            return report;
        }
    }
}
