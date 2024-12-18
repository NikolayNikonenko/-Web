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
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace перенос_бд_на_Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.WebHost.UseKestrel();


            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 10485760; // Увеличьте лимит до 10 MB
            });

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 10485760; // Увеличьте лимит до 10 MB
            });

            builder.Services.Configure<KestrelServerOptions>(options =>
            {
                // Установите максимальный размер тела запроса (в байтах)
                options.Limits.MaxRequestBodySize = 10485760; // 10 MB
            });

            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 10485760; // 10 MB
            });


            // Add services to the container
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<WeatherForecastService>();
            builder.Services.AddScoped<PowerImbalanceService>();
            //builder.Services.AddDbContext<ApplicationContext>(options =>
            //options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddDbContextFactory<ApplicationContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

            //builder.Services.AddScoped<SlicesService>();

            builder.Services.AddScoped<SliceService>();

            builder.Services.AddScoped<ISliceService, SliceService>();

            builder.Services.AddScoped<ActionService>();

            builder.Services.AddScoped<DataFilterService>();

            builder.Services.AddScoped<TelemetryMonitoringService>();

            builder.Services.AddScoped<ValidationService>();

            builder.Services.AddScoped<CorrData>();
            builder.Services.AddScoped<ExperimentCorrData>();
            builder.Services.AddScoped<CalculationIntervalServiceForPTI>();

            builder.Services.AddScoped<ReliabilityAnalyzer>();

            builder.Services.AddScoped<ReportService>();


            builder.Services.AddLogging();
            builder.Services.AddControllers();
            builder.Services.AddSignalR();

            var app = builder.Build();

            app.MapHub<DatabaseChangeHub>("/databaseChangeHub");

            // Configure the HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            app.UseDeveloperExceptionPage();
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