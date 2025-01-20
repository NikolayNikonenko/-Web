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
        public async Task<List<ReportWithIndicatorsViewModel>> GetAllReportsAsync()
        {
            return await _context.report
                .Select(r => new ReportWithIndicatorsViewModel
                {
                    Id = r.id_report,
                    ReportDate = r.report_date
                })
                .ToListAsync();
        }
        public async Task<ReportWithIndicatorsViewModel> LoadReportAsync(Guid reportId)
        {
            var report = await _context.report.FindAsync(reportId);
            if (report == null) throw new KeyNotFoundException("Report not found.");

            var indicators = await GetIndicatorsByReportIdAsync(reportId);

            return new ReportWithIndicatorsViewModel
            {
                Id = report.id_report,
                ReportDate = report.report_date,
                Indicators = indicators
            };
        }

        public async Task<List<ReliabilityAndValidityIndicators>> GetIndicatorsByReportIdAsync(Guid reportId)
        {
            return await _context.reliability_and_validity_indicators
                .Where(r => r.id_report == reportId)
                .ToListAsync();
            
        }
    }
}
