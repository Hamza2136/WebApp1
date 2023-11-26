using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyApp.CommonHelper;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using MyApp.Models.ViewModel;
using Stripe.Checkout;
using System.Security.Claims;

namespace MyWebApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private IUnitOfWork _unitofwork;
        public CartVM vm { get; set; }

        public CartController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public IActionResult Index()
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var claims = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);

            vm = new CartVM()
            {
                ListofCart = _unitofwork.Cart.GetAll(x => x.ApplicationUserId == claims.Value, IncludeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            foreach(var item in vm.ListofCart)
            {
                vm.OrderHeader.OrderTotal += (item.Product.Price * item.Count);
            }
            return View(vm);
        }

        public IActionResult plus(int id)
        {
            var cart = _unitofwork.Cart.GetT(x=> x.Id == id);
            _unitofwork.Cart.IncrementCartItem(cart, 1);
            TempData["success"] = "Item Incremented by 1 in Cart";
            _unitofwork.save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult minus(int id)
        {
            var cart = _unitofwork.Cart.GetT(x=> x.Id == id);
            if (cart.Count <= 1)
            {
                TempData["success"] = "Item Deleted from Cart";
                _unitofwork.Cart.Delete(cart);
            }
            else
            {
                _unitofwork.Cart.DecrementCartItem(cart, 1);
                TempData["success"] = "Item Decremented by 1 in Cart";
            }
            _unitofwork.save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult delete(int id)
        {
            var cart = _unitofwork.Cart.GetT(x=> x.Id == id);
            _unitofwork.Cart.Delete(cart);
            TempData["success"] = "Item Deleted from Cart";
            _unitofwork.save();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult Summary()
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var claims = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
            vm = new CartVM()
            {
                ListofCart = _unitofwork.Cart.GetAll(x => x.ApplicationUserId == claims.Value, IncludeProperties: "Product"),
                OrderHeader = new OrderHeader()
            };
            vm.OrderHeader.ApplicationUser = _unitofwork.ApplicationUser.GetT(x => x.Id == claims.Value);
            vm.OrderHeader.Address = vm.OrderHeader.ApplicationUser.Address;
            vm.OrderHeader.State = vm.OrderHeader.ApplicationUser.State;
            vm.OrderHeader.Name = vm.OrderHeader.ApplicationUser.Name;
            vm.OrderHeader.City = vm.OrderHeader.ApplicationUser.City;
            vm.OrderHeader.PostalCode = vm.OrderHeader.ApplicationUser.PinCode.ToString();
            vm.OrderHeader.Phone = vm.OrderHeader.ApplicationUser.PhoneNumber;

            foreach (var item in vm.ListofCart)
            {
                vm.OrderHeader.OrderTotal += (item.Product.Price * item.Count);
                
            }
            return View(vm);
        }
        [HttpPost]
        public IActionResult Summary(CartVM vm)
        {
            var claimsidentity = (ClaimsIdentity)User.Identity;
            var claims = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
            vm.ListofCart = _unitofwork.Cart.GetAll(x => x.ApplicationUserId == claims.Value, IncludeProperties: "Product");
            vm.OrderHeader.OrderStatus = OrderStatus.StatusPending;
            vm.OrderHeader.PaymentStatus = PaymentStatus.StatusPending;
            vm.OrderHeader.DateOfOrder = DateTime.Now;
            vm.OrderHeader.ApplicationUserId = claims.Value;
            foreach (var item in vm.ListofCart)
            {
                vm.OrderHeader.OrderTotal += (item.Product.Price * item.Count);

            }
            _unitofwork.OrderHeader.Add(vm.OrderHeader);
            _unitofwork.save();
            foreach(var item in vm.ListofCart)
            {
                OrderDetails orderDetails = new OrderDetails()
                {
                    OrderHeaderId = vm.OrderHeader.Id,
                    ProductId = item.ProductId,
                    Count = item.Count,
                    Price = item.Product.Price
                };
                _unitofwork.OrderDetails.Add(orderDetails);
                _unitofwork.save();
            }

            var domain = "https://localhost:44304/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
            
                Mode = "payment",
                SuccessUrl = domain+$"customer/cart/ordersuccess?id={vm.OrderHeader.Id}",
                CancelUrl = domain+$"customer/cart/index",
            };
            foreach (var item in vm.ListofCart)
            {
                var lineitemoptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price*100),
                        Currency = "PKR",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Name,
                        },
                    },
                    Quantity = item.Count,
                };
                options.LineItems.Add(lineitemoptions);
            }

            var service = new SessionService();
            Session session = service.Create(options);
            _unitofwork.OrderHeader.PaymentStatus(vm.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitofwork.save(); 

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

           
            //return RedirectToAction("Index", "Home");
        }

        public IActionResult ordersuccess(int id)
        {
            var orderheader = _unitofwork.OrderHeader.GetT(x=> x.Id == id);
            var service = new SessionService();
            Session session = service.Get(orderheader.SessionId);
            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitofwork.OrderHeader.UpdateStatus(id, OrderStatus.StatusApproved, PaymentStatus.StatusApproved);
            }
            List<Cart> cart = _unitofwork.Cart.GetAll(x => x.ApplicationUserId == orderheader.ApplicationUserId).ToList();
            _unitofwork.Cart.DeleteRange(cart);
            _unitofwork.save();
            return View(id);
        }
    }
}
