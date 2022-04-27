using DDAC_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DDAC_Assignment.Data;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Amazon.S3.Model;
using DDAC_Assignment.Models.customer;
using Microsoft.EntityFrameworkCore;

namespace DDAC_Assignment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DDAC_AssignmentNewsDatabase _context;
        string bucketname = Configuration.bucketName;

        public HomeController(ILogger<HomeController> logger, DDAC_AssignmentNewsDatabase context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<News> news = await _context.News.OrderByDescending(news => news.PublishedDate).Take(10).ToListAsync();
            List<NewsViewModel> newsViewList = new List<NewsViewModel>(news.Count);
            news.ForEach(n => newsViewList.Add(new NewsViewModel {News = n}));
            foreach (var newsView in newsViewList)
            {
                if (!String.IsNullOrEmpty(newsView.News.ImagePath))
                {
                    var newsImage = await ViewImageFromS3(newsView.News, "newsImages");
                    newsView.NewsImageUri = newsImage;
                }
            }
            _logger.LogInformation(newsViewList[0].NewsImageUri);
            return View(newsViewList);
        }

        public async Task<string> ViewImageFromS3(News news, string folderKey)
        {
            List<string> accesskeylist = getAWSCredential();
            var result = new List<S3Object>();
            List<string> presignedURLS = new List<string>();
            var urlForImage = "";
            try
            {
                AmazonS3Client s3Client = new AmazonS3Client(accesskeylist[0], accesskeylist[1], accesskeylist[2],
                    Amazon.RegionEndpoint.USEast1);

                //grab the objects and its information
                string token = null;
                do
                {
                    ListObjectsRequest request = new ListObjectsRequest()
                    {
                        BucketName = bucketname,
                        Prefix = folderKey
                    };
                    ListObjectsResponse response = await s3Client.ListObjectsAsync(request).ConfigureAwait(false);
                    result.AddRange(response.S3Objects);
                    token = response.NextMarker;
                } while (token != null);

                //create each presign URL to the objects
                var formatted = news.ImagePath.Replace(" ", "+");
                var path = $"{folderKey}/" + formatted;
                foreach (var image in result)
                {
                    if (image.Key == path)
                    {
                        /* //create presigned URL for temp access from public
                         GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                         {
                             BucketName = bucketname,
                             Key = image.Key,
                             Expires = DateTime.Now.AddMinutes(1)
                         };

                         //get the generated URL path
                         //presignedURLS.Add(s3Client.GetPreSignedURL(request));
                         urlForImage = s3Client.GetPreSignedURL(request);*/

                        //permanent image access
                        string link = "https://" + image.BucketName + ".s3.amazonaws.com/" + image.Key;
                        urlForImage = link;
                    }
                }

            }
            catch (Exception ex)
            {
            }

            return urlForImage;
        }

        public List<string> getAWSCredential()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build();
            List<string> accesskeylist = new List<string>();
            accesskeylist.Add(configure["AWSCredential:accesskey"]);
            accesskeylist.Add(configure["AWSCredential:secretkey"]);
            accesskeylist.Add(configure["AWSCredential:sectiontoken"]);

            return accesskeylist;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel {RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier});
        }

        public IActionResult Latest()
        {
            return View();
        }
    }
}