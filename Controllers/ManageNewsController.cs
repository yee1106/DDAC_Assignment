using System;
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
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Html;
using X.PagedList;

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

        string bucketname = Configuration.bucketName;

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
        public async Task<IActionResult> Index(int? page, string searchString, string Category, string Status, string Visibility, string sortExpression="", string msg=null)
        {
            ViewBag.msg = msg;

            var news = from m in _context.News
                         select m;
            //generate the listing for the drop down box
            IQueryable<string> querydropdownlist = from m in _context.News
                                                   orderby m.Category
                                                   select m.Category;

            IEnumerable<SelectListItem> items = new SelectList(await querydropdownlist.Distinct().ToListAsync());
            ViewBag.Category = items;

            var statusList = new List<SelectListItem>();
            statusList.Add(new SelectListItem() { Text = "Pending", Value = "0" });
            statusList.Add(new SelectListItem() { Text = "Approved", Value = "1" });
            ViewBag.Status = statusList;

            var visibilityList = new List<SelectListItem>();
            visibilityList.Add(new SelectListItem() { Text = "Visible", Value = "Visible" });
            visibilityList.Add(new SelectListItem() { Text = "Invisible", Value = "Invisible" });
            ViewBag.Visibility = visibilityList;


            ViewBag.totalNews = news.Count();
            int totalApproved = 0;
            int totalNotPublish = 0;
            foreach (var item in news)
            {
                if (item.Status)
                {
                    totalApproved++;
                }
                if(item.PublishedDate > DateTime.Now)
                {
                    totalNotPublish++;
                }
            }
            ViewBag.totalNotPublish = totalNotPublish;
            ViewBag.totalApproved = totalApproved;
            ViewBag.today = DateTime.Now;

            //searching
            if (!String.IsNullOrEmpty(searchString))
            {
                news = news.Where(s => s.Title.Contains(searchString) || s.Actor.Contains(searchString) || s.Content.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(Category))
            {
                news = news.Where(s => s.Category.Equals(Category));
            }

            if (!String.IsNullOrEmpty(Status))
            {
                news = news.Where(s => s.Status == (Status == "0" ? false : true));
            }

            if (!String.IsNullOrEmpty(Visibility))
            {
                news = news.Where(s => s.Visibility.Equals(Visibility));
            }

            /*ViewData["sortByTitle"] = String.IsNullOrEmpty(sortByTitle) ? "Title" : "";
            var sortByTitleVar = String.IsNullOrEmpty(sortByTitle) ? (news = news.OrderBy(s => s.Title)) : (news = news.OrderByDescending(s => s.Title));

            ViewData["sortByPublishedDate"] = String.IsNullOrEmpty(sortByPublishedDate) ? "PublishedDate" : "";
            var sortByPublishedDateVar = String.IsNullOrEmpty(sortByPublishedDate) ? (news = news.OrderBy(s => s.PublishedDate)) : (news = news.OrderByDescending(s => s.PublishedDate));*/

            //sorting 
            ViewData["SortParamTitle"] = "title";
            ViewData["SortPublishedDateDesc"] = "publisheddate";
            ViewData["SortCategory"] = "category";
            ViewData["SortActor"] = "actor";

            ViewData["SortIconTitle"] = "";
            ViewData["SortIconPublishedDate"] = "";
            ViewData["SortIconCategory"] = "";
            ViewData["SortIconActor"] = "";

            SortOrder sortOrder;
            string sortproperty;

            switch (sortExpression.ToLower())
            {
                case "title_desc":
                    sortOrder = SortOrder.Descending;
                    sortproperty = "title";
                    ViewData["SortIconTitle"] = "fa fa-arrow-up";
                    ViewData["SortParamTitle"] = "title";
                    break;
                case "publisheddate":
                    sortOrder = SortOrder.Ascending;
                    sortproperty = "publisheddate";
                    ViewData["SortIconPublishedDate"] = "fa fa-arrow-down";
                    ViewData["SortPublishedDateDesc"] = "publisheddate_desc";
                    break;
                case "publisheddate_desc":
                    sortOrder = SortOrder.Descending;
                    ViewData["SortIconPublishedDate"] = "fa fa-arrow-up";
                    sortproperty = "publisheddate";
                    ViewData["SortPublishedDateDesc"] = "publisheddate";
                    break;
                case "category":
                    sortOrder = SortOrder.Ascending;
                    sortproperty = "category";
                    ViewData["SortIconCategory"] = "fa fa-arrow-down";
                    ViewData["SortCategory"] = "category_desc";
                    break;
                case "category_desc":
                    sortOrder = SortOrder.Descending;
                    sortproperty = "category";
                    ViewData["SortIconCategory"] = "fa fa-arrow-up";
                    ViewData["SortCategory"] = "category";
                    break;
                case "actor":
                    sortOrder = SortOrder.Ascending;
                    sortproperty = "actor";
                    ViewData["SortIconActor"] = "fa fa-arrow-down";
                    ViewData["SortActor"] = "actor_desc";
                    break;
                case "actor_desc":
                    sortOrder = SortOrder.Descending;
                    sortproperty = "actor";
                    ViewData["SortIconActor"] = "fa fa-arrow-up";
                    ViewData["SortActor"] = "actor";
                    break;
                default:
                    sortOrder = SortOrder.Ascending;
                    sortproperty = "title";
                    ViewData["SortIconTitle"] = "fa fa-arrow-down";
                    ViewData["SortParamTitle"] = "title_desc";
                    break;
            }

            news = Sorting(news, sortproperty, sortOrder);

            //pagination
            var pageNumber = page ?? 1;
            int pageSize = 10;
            var newsList = news.ToPagedList(pageNumber, pageSize);

            return View(newsList);
            //return View(await news.ToListAsync());

            //return View(await _context.News.ToListAsync());
        }

        //sorting
        public IQueryable<News> Sorting(IQueryable<News> news, string SortProperty, SortOrder sortOrder)
        {
            /*var news = from m in _context.News
                       select m;*/
            if (SortProperty.ToLower() == "title")
            {
                if(sortOrder == SortOrder.Ascending)
                {
                    news = news.OrderBy(s => s.Title);
                }
                else
                    news = news.OrderByDescending(s => s.Title);
            }
            else if(SortProperty.ToLower() == "publisheddate")
            {
                if (sortOrder == SortOrder.Ascending)
                {
                    news = news.OrderBy(s => s.PublishedDate);
                }
                else
                    news = news.OrderByDescending(s => s.PublishedDate);
            }
            else if (SortProperty.ToLower() == "category")
            {
                if (sortOrder == SortOrder.Ascending)
                {
                    news = news.OrderBy(s => s.Category);
                }
                else
                    news = news.OrderByDescending(s => s.Category);
            }
            else if (SortProperty.ToLower() == "actor")
            {
                if (sortOrder == SortOrder.Ascending)
                {
                    news = news.OrderBy(s => s.Actor);
                }
                else
                    news = news.OrderByDescending(s => s.Actor);
            }

            return news;
        }

        // GET: News/Details/5
        public async Task<IActionResult> Details(int? id, int switchPageId = -1)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ID = id;

            if(switchPageId != -1)
            {
                ID = switchPageId;
            }

            var news = await _context.News
                .FirstOrDefaultAsync(m => m.ID == ID);


            if (news == null)
            {
                return NotFound();
            }

            var newsItems = from m in _context.News
                            orderby m.PublishedDate
                            select m;

            var newsList = new List<News>(newsItems);

            for (int i = 0; i<newsList.Count; i++)
            {
                if(newsList[i].ID == ID)
                {
                    if (i == 0)
                    {
                        ViewBag.hasPrevious = false;
                    }
                    else
                    {
                        ViewBag.hasPrevious = true;
                        ViewBag.previousId = newsList[i - 1].ID;
                    }
                    if(i == newsList.Count - 1)
                    {
                        ViewBag.hasNext = false;
                    }
                    else
                    {
                        ViewBag.hasNext = true;
                        ViewBag.nextId = newsList[i + 1].ID;

                    }

                    break;
                }
            }

            if (!string.IsNullOrEmpty(news.ImagePath))
            {
                ViewBag.isImagePathNull = "No";
            }
            else
            {
                ViewBag.isImagePathNull = "Yes";
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
        public async Task<IActionResult> Create([Bind("ID,Title,Content,Actor,Visibility,PublishedDate,Category,ParentCategory,Status,ImagePath")] News news, IFormFile image)
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
                //news.Status = "Pending";
                news.LastUpdated = DateTime.Now;

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
                    news.ImagePath = Guid.NewGuid() + "-" + image.FileName;
                    await uploadImageToS3Async(image, news.ImagePath);
                }

                _context.Add(news);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(news);
        }

        //upload image to S3
        public async Task uploadImageToS3Async(IFormFile image, string imagePath)
        {
            string message = "";
            List<string> accesskeylist = getAWSCredential();
            if (image != null)
            {
                using (var S3client = new AmazonS3Client(accesskeylist[0], accesskeylist[1], accesskeylist[2], Amazon.RegionEndpoint.USEast1))
                {
                    using (var uploadStream = new MemoryStream())
                    {
                        await image.CopyToAsync(uploadStream);
                        var uploadRequest = new TransferUtilityUploadRequest
                        {
                            InputStream = uploadStream,
                            Key = imagePath,
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
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,Content,Actor,PublishedDate,Visibility,Category,ParentCategory,Status,ImagePath")] News news, IFormFile image)
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
                    news.LastUpdated = DateTime.Now;
                    if (image != null)
                    {
                        await DeleteImage(news.ImagePath);
                        news.ImagePath = Guid.NewGuid() + "-" + image.FileName;
                        await uploadImageToS3Async(image, news.ImagePath);
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

        // approve news written
        public async Task<IActionResult> Approve(int? id)
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

            // set status to true to represent approved
            news.Status = true; 

            // update and save news
            _context.Update(news);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "ManageNews", new { msg = "The news has been approved." });
        }
    }
}
