using Microsoft.AspNetCore.Mvc;
using MyApp.Models;
using MyApp.DataAccessLayer;
using MyApp.DataAccessLayer.Infrastructure.Repository;
using MyApp.DataAccessLayer.Infrastructure.IRepository;
using MyApp.Models.ViewModel;

namespace MyWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private IUnitOfWork _unitofwork;


        public CategoryController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }

        public IActionResult Index()
        {
            CategoryVM categoryvm = new CategoryVM();
            categoryvm.categories = _unitofwork.Category.GetAll();
            return View(categoryvm);
        }

        //[HttpGet]
        //public IActionResult Create()
        //{
        //    return View();
        //}
        //[HttpPost]
        //public IActionResult Create(Category category)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitofwork.Category.Add(category);
        //        _unitofwork.save();
        //        TempData["success"] = "Category Created Successfully";
        //        return RedirectToAction("Index");
        //    }
        //    return View(category);
        //}

        [HttpGet]
        public IActionResult CreateUpdate(int? id)
        {
            CategoryVM vm = new CategoryVM();
            if (id == 0 || id == null)
            {
                return View(vm);
            }
            else
            {
                vm.category = _unitofwork.Category.GetT(x => x.Id == id);
                if (vm.category == null)
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
        public IActionResult CreateUpdate(CategoryVM vm)
        {
            if (ModelState.IsValid)
            {
                if(vm.category.Id == 0)
                {
                    _unitofwork.Category.Add(vm.category);
                    TempData["success"] = "Category Created Successfully";
                }
                else
                {
                    _unitofwork.Category.Update(vm.category);
                    TempData["success"] = "Category Updated Successfully";
                }
                _unitofwork.save();
                return RedirectToAction("index");
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == 0 || id == null)
            {
                return NotFound();
            }
            else
            {
                var category = _unitofwork.Category.GetT(x => x.Id == id);
                if (category == null)
                {
                    return NotFound();
                }
                else
                {
                    return View(category);
                }
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteData(int? id)
        {
            var category = _unitofwork.Category.GetT(x => x.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            else
            {
                _unitofwork.Category.Delete(category);
                _unitofwork.save();
                TempData["success"] = "Category Deleted Successfully";
                return RedirectToAction("Index");
            }

        }
    }
}
