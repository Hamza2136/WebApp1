 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyApp.CommonHelper;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models;
using MyApp.Models.ViewModel;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace MyWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
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
                    orderheader = orderheader.Where(x => x.OrderStatus == OrderStatus.StatusApproved);
                    break;
                case "underprocessing":
                    orderheader = orderheader.Where(x => x.OrderStatus == OrderStatus.StatusInProcess);
                    break;
                case "shipped":
                    orderheader = orderheader.Where(x => x.OrderStatus == OrderStatus.StatusShipped);
                    break;
                case "cancelled":
                    orderheader = orderheader.Where(x => x.OrderStatus == OrderStatus.StatusCancelled);
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

        [HttpPost]
        [Authorize (Roles = WebsiteRoles.Role_Admin+","+WebsiteRoles.Role_Employee)]
        public IActionResult OrderDetails(OrderVM vm)
        {
            var orderheader = _unitofwork.OrderHeader.GetT(x => x.Id == vm.OrderHeader.Id);
            orderheader.Name = vm.OrderHeader.Name;
            orderheader.Address = vm.OrderHeader.Address;
            orderheader.Phone = vm.OrderHeader.Phone;
            orderheader.City = vm.OrderHeader.City;
            orderheader.State = vm.OrderHeader.State;
            orderheader.PostalCode = vm.OrderHeader.PostalCode;
            if(vm.OrderHeader.Carrier != null)
            {
                orderheader.Carrier = vm.OrderHeader.Carrier;
            }
            if(vm.OrderHeader.TrackingNumber != null)
            {
                orderheader.TrackingNumber = vm.OrderHeader.TrackingNumber;
            }
            _unitofwork.OrderHeader.Update(orderheader);
            _unitofwork.save();
            TempData["success"] = "Order-Info Updated Successfully";
            return RedirectToAction("OrderDetails", "Order", new { id = vm.OrderHeader.Id });
        }

        [Authorize(Roles = WebsiteRoles.Role_Admin + "," + WebsiteRoles.Role_Employee)]
        public IActionResult InProcess(OrderVM vm)
        {
            _unitofwork.OrderHeader.UpdateStatus(vm.OrderHeader.Id, OrderStatus.StatusInProcess);
            _unitofwork.save();
            TempData["success"] = "Order-Status Updated InProcess";
            return RedirectToAction("OrderDetails", "Order", new {id = vm.OrderHeader.Id });
        }

        [Authorize(Roles = WebsiteRoles.Role_Admin + "," + WebsiteRoles.Role_Employee)]
        public IActionResult Shipped(OrderVM vm)
        {
            var orderheader = _unitofwork.OrderHeader.GetT(x => x.Id == vm.OrderHeader.Id);
            orderheader.OrderStatus = OrderStatus.StatusShipped;
            orderheader.Carrier = vm.OrderHeader.Carrier;
            orderheader.TrackingNumber = vm.OrderHeader.TrackingNumber;
            orderheader.DateOfShipping = DateTime.Now;
            _unitofwork.OrderHeader.Update(orderheader);
            _unitofwork.save();
            TempData["success"] = "Order-Status Updated Shipped";
            return RedirectToAction("OrderDetails", "Order", new { id = vm.OrderHeader.Id });
        }

        [Authorize(Roles = WebsiteRoles.Role_Admin + "," + WebsiteRoles.Role_Employee)]
        public IActionResult Cancelled(OrderVM vm)
        {
            var orderheader = _unitofwork.OrderHeader.GetT(x => x.Id == vm.OrderHeader.Id);
            if (orderheader.PaymentStatus == PaymentStatus.StatusApproved)
            {
                var refund = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = vm.OrderHeader.PaymentIntentId
                };
                var service = new RefundService();
                Refund Refund = service.Create(refund);
                _unitofwork.OrderHeader.UpdateStatus(orderheader.Id, OrderStatus.StatusCancelled, OrderStatus.StatusRefund);
            }
            else
            {
                _unitofwork.OrderHeader.UpdateStatus(orderheader.Id, OrderStatus.StatusCancelled, OrderStatus.StatusCancelled);
            }
            _unitofwork.save();
            TempData["success"] = "Order Cancelled";
            return RedirectToAction("OrderDetails", "Order", new { id = vm.OrderHeader.Id });
        }

        public IActionResult PayNow(OrderVM vm)
        {
            var OrderHeader = _unitofwork.OrderHeader.GetT(x => x.Id == vm.OrderHeader.Id, IncludeProperties: "ApplicationUser");
            var OrderDetails = _unitofwork.OrderDetails.GetAll(x => x.Id == vm.OrderHeader.Id, IncludeProperties: "Product");

            var domain = "http://localhost:34738/";
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),

                Mode = "payment",
                SuccessUrl = domain + $"customer/cart/ordersuccess?id={vm.OrderHeader.Id}",
                CancelUrl = domain + $"customer/cart/index",
            };
            foreach (var item in OrderDetails)
            {
                var lineitemoptions = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Product.Price * 100),
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
        }
    }
}
