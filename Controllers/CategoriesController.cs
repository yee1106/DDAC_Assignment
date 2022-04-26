using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DDAC_Assignment.Data;
using DDAC_Assignment.Models;
using Microsoft.Extensions.Configuration;
using System.IO;
using Amazon.SQS;
using Amazon.SQS.Model;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using DDAC_Assignment.Areas.Identity.Data;

namespace DDAC_Assignment.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DDAC_AssignmentNewsDatabase _context;
        private UserManager<DDAC_AssignmentUser> userManager;

        public CategoriesController(DDAC_AssignmentNewsDatabase context, UserManager<DDAC_AssignmentUser> usrMgr)
        {
            _context = context;
            userManager = usrMgr;
        }

        // GET: Categories
        public async Task<IActionResult> Index(string message="")
        {
            ViewBag.msg = message;
            List<string> credentialInfo = getAWSCredential();
            var sqsClient = new AmazonSQSClient(credentialInfo[0], credentialInfo[1], credentialInfo[2], Amazon.RegionEndpoint.USEast1);
            var queueURL = await sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = Configuration.SQSQueue });

            GetQueueAttributesRequest attReq = new GetQueueAttributesRequest();
            attReq.QueueUrl = queueURL.QueueUrl;
            attReq.AttributeNames.Add("ApproximateNumberOfMessages");
            GetQueueAttributesResponse response1 = await sqsClient.GetQueueAttributesAsync(attReq);

            ViewBag.count = response1.ApproximateNumberOfMessages;
            ViewBag.requestList = await readMessage();

            return View(await _context.Category.ToListAsync());
        }

        public async Task<List<KeyValuePair<UpdateCategoryRequest, string>>> readMessage()
        {

            List<string> credentialInfo = getAWSCredential();
            var sqsClient = new AmazonSQSClient(credentialInfo[0], credentialInfo[1], credentialInfo[2], Amazon.RegionEndpoint.USEast1);

            var queueURL = await sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = Configuration.SQSQueue });
            List<KeyValuePair<UpdateCategoryRequest, string>> updateCategoryRequestList = new List<KeyValuePair<UpdateCategoryRequest, string>>();


            try
            {
                //create received request
                ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest();
                receiveMessageRequest.QueueUrl = queueURL.QueueUrl;
                receiveMessageRequest.MaxNumberOfMessages = 10; // 1 - 10 message for 1 round - for 1 single staff
                receiveMessageRequest.WaitTimeSeconds = 5;  //polling - short polling = 0(min) seconds, long polling = 1 -20 (max) seconds
                receiveMessageRequest.VisibilityTimeout = 1;  //to block other admin to see during the current admin reading
                
                ReceiveMessageResponse receivedContent = await sqsClient.ReceiveMessageAsync(receiveMessageRequest);


                if (receivedContent.Messages.Count > 0)
                {
                    for (int i = 0; i < receivedContent.Messages.Count; i++)
                    {
                        var customer = JsonConvert.DeserializeObject<UpdateCategoryRequest>(receivedContent.Messages[i].Body);
                        var deleteToken = receivedContent.Messages[i].ReceiptHandle;
                        updateCategoryRequestList.Add(new KeyValuePair<UpdateCategoryRequest, string>(customer, deleteToken));
                    }

                }
                else
                {
                    
                }
                updateCategoryRequestList = (List<KeyValuePair<UpdateCategoryRequest, string>>)updateCategoryRequestList.Distinct();
            }
            catch (AmazonSQSException ex)
            {
                 ViewBag.message = "Error: " + ex.Message;
            }
            catch (Exception ex)
            {
                ViewBag.message = "Error: " + ex.Message;
            }
            return updateCategoryRequestList;
        }

        public IActionResult RefreshQueue()
        {
            return RedirectToAction("Index", "Categories", new { message = "" });
        }

        public async Task<IActionResult> deleteMessage(string deleteToken, int CategoryID, string CategoryName, string ParentCategoryName, string Description, string RequestType, string isApproved)
        {
            string msg="";
            if (isApproved == "Yes")
            {
                if (RequestType == "Create"){
                    msg = CategoryName + " category is created!!";
                    Category category = new Category
                    {
                        CategoryName = CategoryName,
                        ParentCategory = ParentCategoryName,
                        Description = Description
                    };
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    await UpdateNavigation();
                }
                if (RequestType == "Delete")
                {
                    msg = CategoryName + " category is deleted!!";
                    var category = await _context.Category.FindAsync(CategoryID);
                    _context.Category.Remove(category);
                    await _context.SaveChangesAsync();
                    await UpdateNavigation();
                }
            }
            else
            {
                msg = "The " + RequestType.ToLower() + " operation for " + CategoryName + " category is rejected!!";
            }

            try
            {
                List<string> credentialInfo = getAWSCredential();
                var sqsClient = new AmazonSQSClient(credentialInfo[0], credentialInfo[1], credentialInfo[2], Amazon.RegionEndpoint.USEast1);
                var response = await sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = Configuration.SQSQueue });

                var delRequest = new DeleteMessageRequest
                {
                    QueueUrl = response.QueueUrl,
                    ReceiptHandle = deleteToken
                };
                var delResponse = await sqsClient.DeleteMessageAsync(delRequest);
                return RedirectToAction("Index", "Categories", new { message = msg });
            }
            catch (AmazonSQSException ex)
            {
                return RedirectToAction("Index", "Categories", new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Categories", new { message = ex.Message });
            }
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

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public async Task<IActionResult> Create()
        {
            /*//generate the listing for the drop down box
            IQueryable<string> querydropdownlist = from m in _context.Category
                                                   orderby m.CategoryName
                                                   select m.CategoryName;
            List<string> list = new List<string>(await querydropdownlist.Distinct().ToListAsync());
            foreach(var item in list) { }*/
            List<string> list = new List<string>();
            var categories = await _context.Category.ToListAsync();
            foreach(var item in categories)
            {
                if(item.ParentCategory == "None")
                {
                    list.Add(item.CategoryName);
                }
            }

            ViewBag.Category = list;
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,CategoryName,ParentCategory,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                //if role is staff then should get approved from admin then just can create category
                if (User.IsInRole("Admin"))
                {
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    await UpdateNavigation();
                }
                else
                {
                    DDAC_AssignmentUser user = await userManager.GetUserAsync(User);
                    UpdateCategoryRequest updateCategoryRequest = new UpdateCategoryRequest
                    {
                        CategoryName = category.CategoryName,
                        ParentCategoryName = category.ParentCategory,
                        Description = category.Description,
                        RequestType = "Create",
                        StaffUserName = user.UserName,
                        RequestTime = DateTime.Now
                    };

                    List<string> credentialInfo = getAWSCredential();
                    var sqsClient = new AmazonSQSClient(credentialInfo[0], credentialInfo[1], credentialInfo[2], Amazon.RegionEndpoint.USEast1);

                    var queueURL = await sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = Configuration.SQSQueue });

                    try //push the message content to queue
                    {
                        //create send message request
                        SendMessageRequest message = new SendMessageRequest();
                        message.MessageBody = JsonConvert.SerializeObject(updateCategoryRequest);
                        message.QueueUrl = queueURL.QueueUrl;


                        //send message now
                        await sqsClient.SendMessageAsync(message);
                        return RedirectToAction("Index", "Categories", new { message = "Your request is send to the admin. Please wait to get approved" });
                    }
                    catch (AmazonSQSException ex)
                    {
                        return RedirectToAction("Index", "Categories", new { message = "Error: " + ex.Message });
                    }
                    catch (Exception ex)
                    {
                        return RedirectToAction("Index", "Categories", new { message = "Error: " + ex.Message });
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            //generate the listing for the drop down box
            IQueryable<string> querydropdownlist = from m in _context.Category
                                                   orderby m.CategoryName
                                                   select m.CategoryName;
            List<string> list = new List<string>(await querydropdownlist.Distinct().ToListAsync());
            list.Remove(category.CategoryName);
            ViewBag.Category = list;
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,CategoryName,ParentCategory,Description")] Category category)
        {
            if (id != category.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                     UpdateItem(id, category);
                    //_context.Update(category);
                    await _context.SaveChangesAsync();
                    await UpdateNavigation();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.ID))
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
            return View(category);
        }

        public async Task UpdateNavigation()
        {
            try
            {
                await _context.SaveChangesAsync();
                var navigationItems = await _context.Category.ToListAsync();
                List<string> navigationButtons = new List<string>();
                foreach (var item in navigationItems)
                {
                    if (item.ParentCategory.Equals("None"))
                    {
                        navigationButtons.Add(item.CategoryName);
                    }
                }
                Program.navigationItem = navigationButtons;
            }catch(Exception ex)
            {
                //await UpdateNavigation();
            }
        }

        public async Task UpdateItem(int id, Category category)
        {
            var categories = _context.Category.ToList();
            var oldCategory = await _context.Category.FindAsync(id);

            if (oldCategory.CategoryName != category.CategoryName)
            {
                foreach (var item in categories)
                {
                    if (item.ParentCategory == oldCategory.CategoryName)
                    {
                        item.ParentCategory = category.CategoryName;
                        _context.Update(item);
                    }
                }

                var news = _context.News.ToList();
                foreach (var newItem in news)
                {
                    if (newItem.ParentCategory == oldCategory.CategoryName)
                    {
                        newItem.ParentCategory = category.CategoryName;
                        _context.Update(newItem);
                    }
                    if (newItem.Category == oldCategory.CategoryName)
                    {
                        newItem.Category = category.CategoryName;
                        _context.Update(newItem);
                    }
                }

                var advertisement = _context.Advertisement.ToList();
                foreach (var advertisementItem in advertisement)
                {
                    if (advertisementItem.Category.Contains(oldCategory.CategoryName))
                    {
                        advertisementItem.Category = advertisementItem.Category.Replace(oldCategory.CategoryName, category.CategoryName);
                    }
                    _context.Update(advertisementItem);
                }
            }
            oldCategory.CategoryName = category.CategoryName;
            //oldCategory.ParentCategory = category.ParentCategory;
            oldCategory.Description = category.Description;
            _context.Update(oldCategory);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            //if role is staff then should get approved from admin then just can create category
            var category = await _context.Category.FindAsync(id);
            if (User.IsInRole("Admin"))
            {
                _context.Category.Remove(category);
                await _context.SaveChangesAsync();
                await UpdateNavigation();
            }
            else
            {
                UpdateCategoryRequest updateCategoryRequest = new UpdateCategoryRequest
                {
                    CategoryID = id,
                    CategoryName = category.CategoryName,
                    ParentCategoryName = category.ParentCategory,
                    Description = category.Description,
                    RequestType = "Delete",
                    StaffUserName = "Chew Chang Wang",
                    RequestTime = DateTime.Now
                };

                List<string> credentialInfo = getAWSCredential();
                var sqsClient = new AmazonSQSClient(credentialInfo[0], credentialInfo[1], credentialInfo[2], Amazon.RegionEndpoint.USEast1);

                var queueURL = await sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = Configuration.SQSQueue });

                try //push the message content to queue
                {
                    //create send message request
                    SendMessageRequest message = new SendMessageRequest();
                    message.MessageBody = JsonConvert.SerializeObject(updateCategoryRequest);
                    message.QueueUrl = queueURL.QueueUrl;


                    //send message now
                    await sqsClient.SendMessageAsync(message);
                    return RedirectToAction("Index", "Categories", new { message = "Your request is send to the admin. Please wait to get approved" });
                }
                catch (AmazonSQSException ex)
                {
                    return RedirectToAction("Index", "Categories", new { message = "Error: " + ex.Message });
                }
                catch (Exception ex)
                {
                    return RedirectToAction("Index", "Categories", new { message = "Error: " + ex.Message });
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.ID == id);
        }
    }
}
