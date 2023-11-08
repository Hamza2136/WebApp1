using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using MyApp.DataAccessLayer;
using MyApp.DataAccessLayer.Infrastructure.Repository;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models.ViewModel;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
namespace MyWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private IUnitOfWork _unitofwork;
        private IWebHostEnvironment _webHostEnvironment;


        public ProductController(IUnitOfWork unitofwork, IWebHostEnvironment webHostEnvironment)
        {
            _unitofwork = unitofwork;
            _webHostEnvironment = webHostEnvironment;
        }

        #region APICALL
        public IActionResult AllProducts()
        {
            var products = _unitofwork.Product.GetAll(IncludeProperties:"Category");
            return Json(new {data =  products});
        }
        #endregion
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddUpdate(int? id)
        {
            ProductVM vm = new ProductVM()
            {
                product = new(),
                categories = _unitofwork.Category.GetAll().Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Id.ToString()
                })
            };
            if (id == 0 || id == null)
            {
                return View(vm);
            }
            else
            {
                vm.product = _unitofwork.Product.GetT(x => x.Id == id);
                if (vm.product == null)
                {
                    return NotFound();
                }
                else
                {
                    return View(vm);
                }
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddUpdate(ProductVM vm, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string filename = String.Empty;
                if(file!= null)
                {
                    string filedir = Path.Combine(_webHostEnvironment.WebRootPath, "ProductImage");
                    filename = Guid.NewGuid().ToString() + "-" + file.FileName;
                    string filepath = Path.Combine(filedir, filename);
                    if (vm.product.ImageUrl != null)
                    {
                        var oldimg = Path.Combine(_webHostEnvironment.WebRootPath, vm.product.ImageUrl.Trim('\\'));
                        if(System.IO.File.Exists(oldimg))
                        {
                            System.IO.File.Delete(oldimg);
                        }
                    }
                    using(var filestream = new FileStream(filepath, FileMode.Create))
                    {
                        file.CopyTo(filestream);
                    }
                    vm.product.ImageUrl = @"\ProductImage\" + filename;
                }
                if(vm.product.Id == 0)
                {
                    _unitofwork.Product.Add(vm.product);
                    TempData["success"] = "Product Added Successfully";
                }
                else
                {
                    _unitofwork.Product.Update(vm.product);
                    TempData["success"] = "Product Updated Successfully";
                }
                _unitofwork.save();
                return RedirectToAction("index");
            }
            return RedirectToAction("Index");
        }

        #region DeleteAPICALL
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var product = _unitofwork.Product.GetT(x => x.Id == id);
            if (product == null)
            {
                return Json(new { success = false, message = "Error in Fetching Data" });
            }
            else
            {
                var oldimg = Path.Combine(_webHostEnvironment.WebRootPath, product.ImageUrl.Trim('\\'));
                if (System.IO.File.Exists(oldimg))
                {
                    System.IO.File.Delete(oldimg);
                }
                _unitofwork.Product.Delete(product);
                _unitofwork.save();
                return Json(new { success = true, message = "Delete Successfull" });
            }
        }
        #endregion
    }
}
