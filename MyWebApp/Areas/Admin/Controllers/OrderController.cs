using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
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
        public IActionResult AllOrders()
        {
            IEnumerable<OrderHeader> orderheader;
            if(User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                orderheader = _unitofwork.OrderHeader.GetAll(IncludeProperties: "ApplicationUser");
            }
            else
            {
                var claimsidentity = (ClaimsIdentity)User.Identity;
                var claims = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
                orderheader = _unitofwork.OrderHeader.GetAll(x=> x.ApplicationUserId == claims.Value);
            }
            return Json(new { data = orderheader });
        }
        #endregion

        public IActionResult Index()
        {
            return View();
        }
    }
}
