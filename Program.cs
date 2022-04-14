using DDAC_Assignment.Data;
using DDAC_Assignment.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            var host = CreateHostBuilder(args).Build();

            //call the initialize function here
            using (var Scope = host.Services.CreateScope())
            {
                var services = Scope.ServiceProvider;

                try
                {
                    var context = services.GetRequiredService<DDAC_AssignmentNewsDatabase>();
                    context.Database.Migrate();
                    CategoryDatabase.Initialize(services); // invoke the category database function before run the program
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>(); // display error msg to the output section
                    logger.LogError(ex, "An error occured seeding the database");
                }
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
