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
using Microsoft.Extensions.Configuration;
using System.IO;
using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.S3.Model;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Hosting;

namespace DDAC_Assignment.Controllers
{
    public class AdvertisementsController : Controller
    {
        private readonly DDAC_AssignmentNewsDatabase _context;
        const string bucketname = "ddacimagebucket";
        Uri addressForRefresh = new Uri("https://40jdw173md.execute-api.us-east-1.amazonaws.com/refreshNewsTemplateAPI");
        Uri addressForReturn = new Uri("https://pauyre9e93.execute-api.us-east-1.amazonaws.com/getNewsTemplate");
        HttpClient clientRefresh;
        HttpClient clientReturn;
        List<NewsTemplate> advertisementTemplate;
        private IWebHostEnvironment _hostEnvironemnt;

        public AdvertisementsController(DDAC_AssignmentNewsDatabase context, IWebHostEnvironment environment)
        {
            clientRefresh = new HttpClient();
            clientReturn = new HttpClient();
            clientRefresh.BaseAddress = addressForRefresh;
            clientReturn.BaseAddress = addressForReturn;
            GetApiModel();
            _context = context;
            _hostEnvironemnt = environment;
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

        // GET: Advertisements
        public async Task<IActionResult> Index(string searchString, string publishedDate)
        {
            var advertisement = from m in _context.Advertisement
                                select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                advertisement = advertisement.Where(s => s.Description.Contains(searchString) || s.Advertiser.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(publishedDate))
            {
                if (publishedDate == "future")
                {
                    advertisement = advertisement.Where(s =>s.PublishedDate >= DateTime.Now);
                }
                else if (publishedDate == "pastSevenDays")
                {
                    advertisement = advertisement.Where(s => s.PublishedDate >= DateTime.Now.AddDays(-7) && s.PublishedDate <= DateTime.Now);
                }
                else if (publishedDate == "pastThirtyDays")
                {
                    advertisement = advertisement.Where(s => s.PublishedDate >= DateTime.Now.AddMonths(-1) && s.PublishedDate <= DateTime.Now);
                }
                else if(publishedDate == "pastThreeMonths")
                {
                    advertisement = advertisement.Where(s => s.PublishedDate >= DateTime.Now.AddMonths(-3) && s.PublishedDate <= DateTime.Now);
                }
                else if(publishedDate == "pastOneYear")
                {
                    advertisement = advertisement.Where(s => s.PublishedDate >= DateTime.Now.AddYears(-1) && s.PublishedDate <= DateTime.Now);
                }
            }

            await GetImageFromS3();
            return View(await advertisement.ToListAsync());
        }

        public async Task GetImageFromS3()
        {
            List<string> accesskeylist = getAWSCredential();
            var result = new List<S3Object>();
            List<string> presignedURLS = new List<string>();

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
                        Prefix = "advertisementImages"
                    };
                    ListObjectsResponse response = await s3Client.ListObjectsAsync(request).ConfigureAwait(false);
                    result.AddRange(response.S3Objects);
                    token = response.NextMarker;
                }
                while (token != null);

                //create each presign URL to the objects
                foreach (var image in result)
                {
                    //create presigned URL for temp access from public
                    GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                    {
                        BucketName = bucketname,
                        Key = image.Key,
                        Expires = DateTime.Now.AddMinutes(1)
                    };

                    //get the generated URL path
                    presignedURLS.Add(s3Client.GetPreSignedURL(request) + "\n" + image.Key);
                }
                ViewBag.URLs = presignedURLS;
            }
            catch (Exception ex)
            {

            }
        }

        // GET: Advertisements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisement
                .FirstOrDefaultAsync(m => m.ID == id);
            if (advertisement == null)
            {
                return NotFound();
            }


            if (advertisement.Position == "Header")
            {
                ViewBag.Position = "Header";
            }
            else
            {
                ViewBag.Position = "Footer";
            }

            ViewBag.data = advertisementTemplate.Find(item => item.ParentCategory == "Local");
            ViewBag.category = advertisementTemplate.Find(item => item.ParentCategory == "Local").ParentCategory;

            for (int i = 0; i< advertisementTemplate.Count; i++)
            {
                if (advertisement.Category.Equals(advertisementTemplate[i].ParentCategory))
                {
                    ViewBag.data = advertisementTemplate[i];
                    ViewBag.category = advertisementTemplate[i].ParentCategory;
                }
            }

            //get image from S3
            await ViewImageFromS3(advertisement);

            return View(advertisement);
        }

        public async Task GetApiModel(){
            GetApiModel getApiModel = new GetApiModel();
            //HttpResponseMessage responseRefresh = clientRefresh.GetAsync(clientRefresh.BaseAddress).Result;
            HttpResponseMessage responseReturn = clientReturn.GetAsync(clientReturn.BaseAddress).Result;

            if (responseReturn.IsSuccessStatusCode)
            {
                string data = responseReturn.Content.ReadAsStringAsync().Result;
                getApiModel = JsonConvert.DeserializeObject<GetApiModel>(data);

                if (getApiModel != null)
                {
                    /*if ("001" == getApiModel.body[0].MessageID)
                    {
                        headerTemplate = getApiModel.body[0];
                        footerTemplate = getApiModel.body[1];
                    }
                    else
                    {
                        headerTemplate = getApiModel.body[1];
                        footerTemplate = getApiModel.body[2];
                    }*/
                    advertisementTemplate = getApiModel.body;
                }
            }
        }

        // GET: Advertisements/Create
        public async Task<IActionResult> Create(string msg = "")
        {
            //generate the listing for the drop down box
            IQueryable<Category> querydropdownlist = from m in _context.Category
                                                     orderby m.CategoryName
                                                     select m;
            List<Category> list = new List<Category>(await querydropdownlist.Distinct().ToListAsync());
            ViewBag.Category = list;
            ViewBag.msg = msg;
            return View();
        }

        // POST: Advertisements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Description,Advertiser,Position,Category,PublishedDate,Visibility,Duration,ImagePath")] Advertisement advertisement, IFormFile image)
        {
            if (ModelState.IsValid)
            {

                if (image != null)
                {
                    advertisement.ImagePath = image.FileName;
                    await uploadImageToS3Async(image);   
                }
                else
                {
                    string message = "You must select an image!!!";
                    return RedirectToAction("Create", "Advertisements", new { msg = message });
                }
                _context.Add(advertisement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(advertisement);
        }

        //upload image to S3
        public async Task uploadImageToS3Async(IFormFile image)
        {
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
                            BucketName = bucketname + "/advertisementImages",
                            CannedACL = S3CannedACL.PublicRead
                        };

                        var transferringtoS3 = new TransferUtility(S3client);
                        await transferringtoS3.UploadAsync(uploadRequest);
                    }
                }
            }
        }

        // GET: Advertisements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisement.FindAsync(id);
            if (advertisement == null)
            {
                return NotFound();
            }
            //generate the listing for the drop down box
            IQueryable<Category> querydropdownlist = from m in _context.Category
                                                     orderby m.CategoryName
                                                     select m;
            List<Category> list = new List<Category>(await querydropdownlist.Distinct().ToListAsync());
            ViewBag.Category = list;

            return View(advertisement);
        }

        // POST: Advertisements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Description,Advertiser,Position,Category,PublishedDate,Visibility,Duration,ImagePath")] Advertisement advertisement, IFormFile image)
        {
            if (id != advertisement.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (image != null)
                    {
                        await uploadImageToS3Async(image);


                        await DeleteImage(advertisement.ImagePath);
                        advertisement.ImagePath = image.FileName;
                    }
                    _context.Update(advertisement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdvertisementExists(advertisement.ID))
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
            return View(advertisement);
        }

        public async Task DeleteImage(string ImagePath)
        {
            //var originNews = await _context.News.FindAsync(id);
            string message = "";
            List<string> credentialInfo = getAWSCredential();
            var S3client = new AmazonS3Client(credentialInfo[0], credentialInfo[1], credentialInfo[2], Amazon.RegionEndpoint.USEast1);

            //start deleting the related items
            try
            {
                /*if (string.IsNullOrEmpty(ImagePath))
                {
                    return BadRequest("The " + ImagePath + " parameter is empty!");
                }*/
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketname + "/advertisementImages",
                    Key = ImagePath
                };
                await S3client.DeleteObjectAsync(deleteObjectRequest);
                message = ImagePath + " is deleted from the S3 bucket! Please check the S3 bucket whether is it deleted!";
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
            }
        }

        // GET: Advertisements/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var advertisement = await _context.Advertisement
                .FirstOrDefaultAsync(m => m.ID == id);
            if (advertisement == null)
            {
                return NotFound();
            }
            //get image from S3
            await ViewImageFromS3(advertisement);

            return View(advertisement);
        }

        // POST: Advertisements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var advertisement = await _context.Advertisement.FindAsync(id);

            await DeleteImage(advertisement.ImagePath);

            _context.Advertisement.Remove(advertisement);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AdvertisementExists(int id)
        {
            return _context.Advertisement.Any(e => e.ID == id);
        }

        public async Task ViewImageFromS3(Advertisement advertisement)
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
                        Prefix = "advertisementImages"
                    };
                    ListObjectsResponse response = await s3Client.ListObjectsAsync(request).ConfigureAwait(false);
                    result.AddRange(response.S3Objects);
                    token = response.NextMarker;
                }
                while (token != null);

                //create each presign URL to the objects
                var path = "advertisementImages/" + advertisement.ImagePath;
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
    }
}
