using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace MyWebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        //[HttpGet]
        public IActionResult Index()
        {
            IEnumerable<Product> product = _unitOfWork.Product.GetAll(IncludeProperties:"Category");
            return View(product);
        }
        [HttpGet]
        public IActionResult Details(int? ProductId)
        {
            Cart cart = new Cart()
            {
                Product = _unitOfWork.Product.GetT(x => x.Id == ProductId, IncludeProperties: "Category"),
                Count = 1,
                ProductId = (int)ProductId

            };
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult Details(Cart cart)
        {
            if (ModelState.IsValid)
            {
                var claimsidentity = (ClaimsIdentity)User.Identity;
                var claims = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
                cart.ApplicationUserId = claims.Value;

                var cartitem = _unitOfWork.Cart.GetT(x=> x.ProductId==cart.ProductId && x.ApplicationUserId == claims.Value);

                if (cartitem == null)
                {
                    _unitOfWork.Cart.Add(cart);
                    TempData["success"] = "Item Successfully Added to Cart";
                }
                else
                {
                    _unitOfWork.Cart.IncrementCartItem(cartitem, cart.Count);
                    TempData["success"] = "Item Successfully Added to Cart";
                }
                _unitOfWork.save();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}