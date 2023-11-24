using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using MyApp.Models.ViewModel;
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
    }
}
