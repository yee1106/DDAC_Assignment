using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DDAC_Assignment.Data;
using DDAC_Assignment.Models;
using DDAC_Assignment.Models.customer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Model.Internal.MarshallTransformations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace DDAC_Assignment.Controllers
{
    public class NewsController : Controller
    {
        private readonly ILogger<NewsController> _logger;
        private readonly DDAC_AssignmentNewsDatabase _context;


        public NewsController(ILogger<NewsController> logger, DDAC_AssignmentNewsDatabase context)
        {
            _logger = logger;
            _context = context;
        }

        string bucketname = Configuration.bucketName;

        public async Task<IActionResult> Index(string id)
        {
            ViewBag.id = id;
            var news = await  _context.News.FirstAsync(n => n.ID.ToString() == id);

            var headerAdvertisement = await  _context.Advertisement.FirstOrDefaultAsync(a =>
                DateTime.Now >= a.PublishedDate && DateTime.Now <= a.PublishedDate.AddDays(a.Duration) &&
                a.Category == news.Category && a.Position == "Header" && a.Visibility == "Visible");
            //var headerAdvertisement = _context.Advertisement.FirstOrDefault();

            var footerAdvertisement = await _context.Advertisement.FirstOrDefaultAsync(a =>
                DateTime.Now >= a.PublishedDate && DateTime.Now <= a.PublishedDate.AddDays(a.Duration) &&
                a.Category == news.Category && a.Position == "Footer" && a.Visibility == "Visible");
            //var footerAdvertisement = _context.Advertisement.FirstOrDefault();


            if (headerAdvertisement != null)
            {

                var headerAd = await ViewImageFromS3Ad(headerAdvertisement, "advertisementImages");
                ViewBag.headerAd = await ViewImageFromS3Ad(headerAdvertisement, "advertisementImages");
                _logger.LogInformation(headerAd);
            }

            if (footerAdvertisement != null)
            {
                var footerAd = await ViewImageFromS3Ad(footerAdvertisement, "advertisementImages");
                ViewBag.footerAd = await ViewImageFromS3Ad(footerAdvertisement, "advertisementImages");
                _logger.LogInformation(footerAd);
            }

            ViewBag.newsImage = await ViewImageFromS3(news, "newsImages");

            


            return View(news);
        }

        public async Task<string> ViewImageFromS3(News news,string folderKey)
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

        public async Task<string> ViewImageFromS3Ad(Advertisement ad, string folderKey)
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
                var formatted = ad.ImagePath.Replace(" ", "+");
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

        public IActionResult Page(string page)
        {
            if (!string.IsNullOrEmpty(page))
            {
                Program.selectedPage = page;
                ViewBag.pageName = page;
            }

            List<News> news = _context.News.Where(n => n.Category.Equals(page)).OrderByDescending(n => n.PublishedDate)
                .Take(10).ToList();
            return View(news);
        }
    }
}