using System;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace перенос_бд_на_Web.Services
{
    public interface ISliceService
    {
        Task<List<string>> GetFilePathsInRangeAsync(DateTime startDateTime, DateTime endDateTime, string experimentalKit);
    }
}
