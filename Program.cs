using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using перенос_бд_на_Web.Data;
using перенос_бд_на_Web.Models;
using перенос_бд_на_Web.Services;
using перенос_бд_на_Web.Pages.TM;

namespace перенос_бд_на_Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Настройки Kestrel
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = 10485760; // 10 MB
            });


            // Add services to the container
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddScoped<PowerImbalanceService>();
            builder.Services.AddDbContext<ApplicationContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


            builder.Services.AddScoped<SlicesService>();

            builder.Services.AddScoped<SliceService>();

            builder.Services.AddScoped<ISliceService, SliceService>();




            builder.Services.AddScoped<ActionService>();

            builder.Services.AddScoped<CorrData>();

            builder.Services.AddScoped<ReliabilityAnalyzer>();
            builder.Services.AddLogging();
            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapBlazorHub();
            app.MapControllers();
            app.MapFallbackToPage("/_Host");


            app.Run();
        }
    }
}