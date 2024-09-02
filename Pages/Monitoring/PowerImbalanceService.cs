using Microsoft.EntityFrameworkCore;
using перенос_бд_на_Web.Models;
using System.Threading.Tasks;
using System.Linq;

namespace перенос_бд_на_Web.Services
{
    public class PowerImbalanceService
    {
        private readonly ApplicationContext _context;

        public PowerImbalanceService(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<(ActivePowerImbalance, string)> GetMaxActivePowerImbalanceAsync()
        {
            var maxImbalance = await _context.active_power_imbalance
                .OrderByDescending(api => Math.Abs(api.p_neb_p))
                .FirstOrDefaultAsync();

            if (maxImbalance == null)
            {
                return (null, null);
            }

            var slice = await _context.slices
                .FirstOrDefaultAsync(s => s.SliceID == maxImbalance.SliceID_p);

            return (maxImbalance, slice?.SliceName);
        }

        public async Task<(ReactivePowerImbalance, string)> GetMaxReactivePowerImbalanceAsync()
        {
            var maxImbalance = await _context.reactive_power_imbalance
                .OrderByDescending(rpi => Math.Abs(rpi.q_neb_q))
                .FirstOrDefaultAsync();

            if (maxImbalance == null)
            {
                return (null, null);
            }

            var slice = await _context.slices
                .FirstOrDefaultAsync(s => s.SliceID == maxImbalance.SliceID_q);

            return (maxImbalance, slice?.SliceName);
        }
        public async Task<double> GetAverageTotalActivePowerImbalanceAsync()
        {
            var sliceCount = await _context.active_power_imbalance
                .Select(api => api.SliceID_p)
                .Distinct()
                .CountAsync();

            var totalImbalance = await _context.active_power_imbalance
                .GroupBy(api => api.SliceID_p)
                .Select(g => g.Sum(api => api.p_neb_p))
                .SumAsync();

            return totalImbalance / sliceCount;
        }

        public async Task<double> GetAverageTotalReactivePowerImbalanceAsync()
        {
            var sliceCount = await _context.reactive_power_imbalance
                .Select(rpi => rpi.SliceID_q)
                .Distinct()
                .CountAsync();

            var totalImbalance = await _context.reactive_power_imbalance
                .GroupBy(rpi => rpi.SliceID_q)
                .Select(g => g.Sum(rpi => rpi.q_neb_q))
                .SumAsync();

            return totalImbalance / sliceCount;
        }
    }
}
