 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.CommonHelper;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using MyApp.Models.ViewModel;
using System.Security.Claims;

namespace MyWebApp.Areas.Admin.Controllers
{
    [Authorize]
    [Area("Admin")]
    public class OrderController : Controller
    {
        private IUnitOfWork _unitofwork;

        public OrderController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        #region APICALL
        public IActionResult AllOrders(string status)
        {
            IEnumerable<OrderHeader> orderheader;
            orderheader = _unitofwork.OrderHeader.GetAll(IncludeProperties: "ApplicationUser");

            if (User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                orderheader = _unitofwork.OrderHeader.GetAll(IncludeProperties: "ApplicationUser");
            }
            else
            {
                var claimsidentity = (ClaimsIdentity)User.Identity;
                var claims = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
                orderheader = _unitofwork.OrderHeader.GetAll(x => x.ApplicationUserId == claims.Value);
            }

            switch (status)
            {
                case "pending":
                    orderheader = orderheader.Where(x => x.PaymentStatus == PaymentStatus.StatusPending);
                    break;
                case "approved":
                    orderheader = orderheader.Where(x => x.PaymentStatus == PaymentStatus.StatusApproved);
                    break;
                default:
                    break;
            }



            return Json(new { data = orderheader });
        }
        #endregion

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult OrderDetails(int id)
        {
            OrderVM vm = new OrderVM()
            {
                OrderHeader = _unitofwork.OrderHeader.GetT(x => x.Id == id, IncludeProperties: "ApplicationUser"),
                OrderDetails = _unitofwork.OrderDetails.GetAll(x => x.Id == id, IncludeProperties: "Product")
            };
            return View(vm);
        }
    }
}
