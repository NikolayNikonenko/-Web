using Microsoft.AspNetCore.SignalR;
namespace перенос_бд_на_Web.Models
{
    public class DatabaseNotifier
    {
        private readonly IHubContext<DatabaseChangeHub> _hubContext;

        public DatabaseNotifier(IHubContext<DatabaseChangeHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyDatabaseChange()
        {
            await _hubContext.Clients.All.SendAsync("DatabaseChanged");
        }
    }
}
