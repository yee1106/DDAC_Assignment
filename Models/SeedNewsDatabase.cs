using Amazon.S3;
using Amazon.S3.Transfer;
using DDAC_Assignment.Data;
using DDAC_Assignment.Models.APIs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models
{
    public class SeedNewsDatabase
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            //this function: to first load to store the default category data
            using (var context = new DDAC_AssignmentNewsDatabase(
            serviceProvider.GetRequiredService<DbContextOptions<DDAC_AssignmentNewsDatabase>>()))
            {
                if (context.News.Any())
                {
                    return;
                }
                if (!context.News.Any())
                {
                    /*Uri addressForReturn = new Uri("https://pauyre9e93.execute-api.us-east-1.amazonaws.com/getNewsTemplate");
                    HttpClient clientReturn = new HttpClient();
                    GetApiModel getApiModel = new GetApiModel();

                    clientReturn.BaseAddress = addressForReturn;
                    HttpResponseMessage responseReturn = clientReturn.GetAsync(clientReturn.BaseAddress).Result;

                    if (responseReturn.IsSuccessStatusCode)
                    {
                        string data = responseReturn.Content.ReadAsStringAsync().Result;
                        getApiModel = JsonConvert.DeserializeObject<GetApiModel>(data);

                        if (getApiModel != null)
                        {
                            foreach(var newsItem in getApiModel.body)
                            {
                                context.News.Add(
                                   new News
                                   {
                                       Title = newsItem.Title,
                                       Content = newsItem.Content,
                                       Actor = newsItem.Actor,
                                       PublishedDate = DateTime.Parse(newsItem.PublishedDate),
                                       Category = newsItem.ParentCategory,
                                       ParentCategory = "None",
                                       Status = "Pending",
                                       LastUpdated = DateTime.Parse(newsItem.LastUpdatedDate),
                                       Visibility = "Invisible"
                                   }
                               );
                            }
                            context.SaveChanges();
                        }
                    }*/
                    List<NewsTemplate> newsTemplateData = GetNewsDataApi.Get();
                    foreach (var newsItem in newsTemplateData)
                    {
                        context.News.Add(
                           new News
                           {
                               Title = newsItem.Title,
                               Content = newsItem.Content,
                               Actor = newsItem.Actor,
                               PublishedDate = DateTime.Parse(newsItem.PublishedDate),
                               Category = newsItem.ParentCategory,
                               ParentCategory = "None",
                               Status = false,
                               LastUpdated = DateTime.Parse(newsItem.LastUpdatedDate),
                               Visibility = "Invisible"
                           }
                       );
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
