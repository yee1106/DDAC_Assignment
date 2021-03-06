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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DDAC_Assignment.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace DDAC_Assignment
{
    public class Program
    {
        public static List<NewsTemplate> advertisementTemplateList = new List<NewsTemplate>();
        public static List<string> navigationItem = new List<string>();
        public static string selectedPage = "";
        public static async Task Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            var host = CreateHostBuilder(args).Build();
            

            //invoke news template URL and retrive data
            try
            {
                advertisementTemplateList = AdvertisementDatabase.Initialize();
            }
            catch (Exception ex){}

            //call the initialize function here
            using (var Scope = host.Services.CreateScope())
            {
                var services = Scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>(); // display error msg to the output section

                try
                {
                    var context = services.GetRequiredService<DDAC_AssignmentNewsDatabase>();
                    context.Database.Migrate();
                    CategoryDatabase.Initialize(services); // invoke the category database function before run the program
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occured seeding the category database");
                }

                try
                {
                    var context = services.GetRequiredService<DDAC_AssignmentNewsDatabase>();
                    context.Database.Migrate();
                    SeedNewsDatabase.Initialize(services); // invoke the category database function before run the program
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occured seeding the news database");
                }

                try
                {
                    var context = services.GetRequiredService<DDAC_AssignmentNewsDatabase>();
                    context.Database.Migrate();
                    navigationItem = Navigation.getNavigationItem(services); // invoke the category database function before run the program
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occured seeding the news database");
                }

                // TODO create topic for SNS
                //var snsClient = new AmazonSimpleNotificationServiceClient();

                //var topicRequest = new CreateTopicRequest
                //{
                //    Name = "CodingTestResults"
                //};

                //var topicResponse = snsClient.CreateTopic(topicRequest);

                //var topicAttrRequest = new SetTopicAttributesRequest
                //{
                //    TopicArn = topicResponse.TopicArn,
                //    AttributeName = "DisplayName",
                //    AttributeValue = "Coding Test Results"
                //};

                //snsClient.SetTopicAttributes(topicAttrRequest);

                // seed user database
                try
                {
                    var context = services.GetRequiredService<DDAC_AssignmentContext>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var userManager = services.GetRequiredService<UserManager<DDAC_AssignmentUser>>();
                    await SeedUserDatabase.SeedRolesAsync(roleManager);
                    await SeedUserDatabase.SeedAdminAsync(userManager, roleManager);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the user database.");
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
