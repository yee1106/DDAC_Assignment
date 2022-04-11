﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DDAC_Assignment.Data;
using DDAC_Assignment.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;

namespace DDAC_Assignment.Controllers
{
    public class ManageNewsController : Controller
    {
        private readonly DDAC_AssignmentNewsDatabase _context;
        private IWebHostEnvironment _hostEnvironemnt;

        public ManageNewsController(DDAC_AssignmentNewsDatabase context, IWebHostEnvironment environment)
        {
            _context = context;
            _hostEnvironemnt = environment;
        }

        const string bucketname = "newsimagebucket";

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

        // GET: News
        public async Task<IActionResult> Index(string searchString, string Category)
        {
            var news = from m in _context.News
                         select m;
            //generate the listing for the drop down box
            IQueryable<string> querydropdownlist = from m in _context.News
                                                   orderby m.Category
                                                   select m.Category;

            IEnumerable<SelectListItem> items = new SelectList(await querydropdownlist.Distinct().ToListAsync());
            ViewBag.Category = items;

            if (!String.IsNullOrEmpty(searchString))
            {
                news = news.Where(s => s.Title.Contains(searchString) || s.Actor.Contains(searchString) || s.Content.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(Category))
            {
                news = news.Where(s => s.Category.Equals(Category));
            }
            return View(await news.ToListAsync());

            //return View(await _context.News.ToListAsync());
        }

        // GET: News/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .FirstOrDefaultAsync(m => m.ID == id);
            if (news == null)
            {
                return NotFound();
            }

            //get image from S3
            await ViewImageFromS3(news);
           
            //ViewBag.path = news.ImagePath;

            return View(news);
        }

        public async Task ViewImageFromS3(News news)
        {
            List<string> accesskeylist = getAWSCredential();
            var result = new List<S3Object>();
            List<string> presignedURLS = new List<string>();
            var urlForImage = "";
            try
            {
                AmazonS3Client s3Client = new AmazonS3Client(accesskeylist[0], accesskeylist[1], accesskeylist[2], Amazon.RegionEndpoint.USEast1);

                //grab the objects and its information
                string token = null;
                do
                {
                    ListObjectsRequest request = new ListObjectsRequest()
                    {
                        BucketName = bucketname,
                        Prefix = "newsImages"
                    };
                    ListObjectsResponse response = await s3Client.ListObjectsAsync(request).ConfigureAwait(false);
                    result.AddRange(response.S3Objects);
                    token = response.NextMarker;
                }
                while (token != null);

                //create each presign URL to the objects
                var path = "newsImages/" + news.ImagePath;
                foreach (var image in result)
                {
                    if (image.Key == path)
                    {
                        //create presigned URL for temp access from public
                        GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                        {
                            BucketName = bucketname,
                            Key = image.Key,
                            Expires = DateTime.Now.AddMinutes(1)
                        };

                        //get the generated URL path
                        //presignedURLS.Add(s3Client.GetPreSignedURL(request));
                        urlForImage = s3Client.GetPreSignedURL(request);
                    }
                }
                
            }
            catch (Exception ex)
            {

            }
            ViewBag.path = urlForImage;
        }
        // GET: News/Create
        public async Task<IActionResult> Create()
        {
            //generate the listing for the drop down box
            IQueryable<Category> querydropdownlist = from m in _context.Category
                                                   orderby m.CategoryName
                                                   select m;
            List<Category> list = new List<Category>(await querydropdownlist.Distinct().ToListAsync());
            ViewBag.Category = list;

            return View();
        }

        // POST: News/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,Content,Actor,PublishedDate,Category,ParentCategory,Status,ImagePath")] News news, IFormFile image)
        {
            if (ModelState.IsValid)
            {
                //check the parent category of selected category
                IQueryable<Category> querydropdownlist = from m in _context.Category
                                                         orderby m.CategoryName
                                                         select m;
                List<Category> list = new List<Category>(await querydropdownlist.Distinct().ToListAsync());
                news.ParentCategory = "None";
                foreach (var item in list)
                {
                    if (item.CategoryName == news.Category)
                    {
                        news.ParentCategory = item.ParentCategory;
                    }
                }

                //set the status to pending after create
                news.Status = "Pending";

                /*//file upload
                long size = image.Length;
                var filePath = ""; string fileContents = null;
                if (image.ContentType.ToLower() != "image/png") //not text file..
                {
                    return BadRequest("Unable to create the news because " + image.FileName + " is not  image file!!");
}
                if (image.Length == 0)
                {
                    return BadRequest("The " + image.FileName + "file is empty content!");
                }
                else if (image.Length > 1048576)
                {
                    return BadRequest("The " + image.FileName + "file is exceed 1 MB !");
                }
                else
                {
                    //copy file to the server, not use local file path
                    filePath = Path.Combine(_hostEnvironemnt.WebRootPath, "newsImage", image.FileName); ;
                    using(var uploadStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(uploadStream);
                    }
                    news.ImagePath = image.FileName;
                }*/

                /*string message = "";
                List<string> accesskeylist = getAWSCredential();
                if (image != null)
                {
                    using (var S3client = new AmazonS3Client(accesskeylist[0], accesskeylist[1], accesskeylist[2], Amazon.RegionEndpoint.USEast1))
                    {
                        using (var uploadStream = new MemoryStream())
                        {
                            image.CopyToAsync(uploadStream);
                            var uploadRequest = new TransferUtilityUploadRequest
                            {
                                InputStream = uploadStream,
                                Key = image.FileName,
                                BucketName = bucketname + "/newsImages",
                                CannedACL = S3CannedACL.PublicRead
                            };

                            var transferringtoS3 = new TransferUtility(S3client);
                            await transferringtoS3.UploadAsync(uploadRequest);
                        }
                    }
                    news.ImagePath = image.FileName;
                }*/
                if (image != null)
                {
                    await uploadImageToS3Async(image);
                    news.ImagePath = image.FileName;
                }

                _context.Add(news);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(news);
        }

        //upload image to S3
        public async Task uploadImageToS3Async(IFormFile image)
        {
            string message = "";
            List<string> accesskeylist = getAWSCredential();
            if (image != null)
            {
                using (var S3client = new AmazonS3Client(accesskeylist[0], accesskeylist[1], accesskeylist[2], Amazon.RegionEndpoint.USEast1))
                {
                    using (var uploadStream = new MemoryStream())
                    {
                        image.CopyToAsync(uploadStream);
                        var uploadRequest = new TransferUtilityUploadRequest
                        {
                            InputStream = uploadStream,
                            Key = image.FileName,
                            BucketName = bucketname + "/newsImages",
                            CannedACL = S3CannedACL.PublicRead
                        };

                        var transferringtoS3 = new TransferUtility(S3client);
                        await transferringtoS3.UploadAsync(uploadRequest);
                    }
                }
            }
        }

        // GET: News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }
            //generate the listing for the drop down box
            IQueryable<Category> querydropdownlist = from m in _context.Category
                                                     orderby m.CategoryName
                                                     select m;
            List<Category> list = new List<Category>(await querydropdownlist.Distinct().ToListAsync());
            ViewBag.Category = list;
            return View(news);
        }

        // POST: News/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,Content,Actor,PublishedDate,Category,ParentCategory,Status,ImagePath")] News news, IFormFile image)
        {
            if (id != news.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //generate the listing for the drop down box
                    IQueryable<Category> querydropdownlist = from m in _context.Category
                                                             orderby m.CategoryName
                                                             select m;
                    List<Category> list = new List<Category>(await querydropdownlist.Distinct().ToListAsync());
                    news.ParentCategory = "None";
                    foreach (var item in list)
                    {
                        if (item.CategoryName == news.Category)
                        {
                            news.ParentCategory = item.ParentCategory;
                        }
                    }

                    //file upload
                    /*if (image != null)
                    {
                        long size = image.Length;
                        var filePath = ""; string fileContents = null;
                    
                        if (image.ContentType.ToLower() != "image/png") //not text file..
                        {
                            return BadRequest("Unable to create the news because " + image.FileName + " is not  image file!!");
                        }
                        if (image.Length == 0)
                        {
                            return BadRequest("The " + image.FileName + "file is empty content!");
                        }
                        else if (image.Length > 1048576)
                        {
                            return BadRequest("The " + image.FileName + "file is exceed 1 MB !");
                        }
                        else
                        {
                            //copy file to the server, not use local file path
                            filePath = Path.Combine(_hostEnvironemnt.WebRootPath, "newsImage", image.FileName); ;
                            using (var uploadStream = new FileStream(filePath, FileMode.Create))
                            {
                                await image.CopyToAsync(uploadStream);
                            }
                            news.ImagePath = image.FileName;
                        }

                    }*/

                    if (image != null)
                    {
                        await uploadImageToS3Async(image);
                        

                        await DeleteImage(news.ImagePath);
                        news.ImagePath = image.FileName;
                    }

                    _context.Update(news);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewsExists(news.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(news);
        }

        // GET: News/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .FirstOrDefaultAsync(m => m.ID == id);
            if (news == null)
            {
                return NotFound();
            }
            //get image from S3
            await ViewImageFromS3(news);
            //ViewBag.path = news.ImagePath;

            return View(news);
        }

        public async Task<IActionResult> DeleteImage(string ImagePath)
        {
            //var originNews = await _context.News.FindAsync(id);
            string message = "";
            List<string> credentialInfo = getAWSCredential();
            var S3client = new AmazonS3Client(credentialInfo[0], credentialInfo[1], credentialInfo[2], Amazon.RegionEndpoint.USEast1);

            //start deleting the related items
            try
            {
                if (string.IsNullOrEmpty(ImagePath))
                {
                    return BadRequest("The " + ImagePath + " parameter is empty!");
                }
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketname + "/newsImages",
                    Key = ImagePath
                };
                await S3client.DeleteObjectAsync(deleteObjectRequest);
                message = ImagePath + " is deleted from the S3 bucket! Please check the S3 bucket whether is it deleted!";
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
            }

            return RedirectToAction("ViewImages", "UploadFile", new { msg = message });
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.FindAsync(id);

            await DeleteImage(news.ImagePath);

            _context.News.Remove(news);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.ID == id);
        }
    }
}