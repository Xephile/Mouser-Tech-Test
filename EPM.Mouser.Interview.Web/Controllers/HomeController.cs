using EPM.Mouser.Interview.Data;
using EPM.Mouser.Interview.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace EPM.Mouser.Interview.Web.Controllers
{
    [Route("")]
    public class HomeController : Controller
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public HomeController(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = new IndexViewModel
            {
                Products = await _warehouseRepository.List()
            };
            return View(model);
        }

        [HttpGet]
        [Route("/{id:long}")]
        public async Task<IActionResult> Product(long id)
        {
            var product = await _warehouseRepository.Get(id);
            if (product is null) return View();

            var model = new ProductViewModel
            {
                Product = product
            };
            return View(model);
        }
    }
}
