using Microsoft.AspNetCore.Mvc;

namespace DDAC_Assignment.Controllers
{
    public class CommentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
