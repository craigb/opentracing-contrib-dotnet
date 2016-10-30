using System.Collections.Generic;
using System.Linq;
using Constants;
using Microsoft.AspNetCore.Mvc;

namespace Samples.CustomersApi.Controllers
{
    [Route("customers")]
    public class CustomersController : Controller
    {
        private static readonly List<Customer> _customers = new List<Customer>
        {
            new Customer(1, "Marcel Belding"),
            new Customer(2, "Phyllis Schriver"),
            new Customer(3, "Estefana Balderrama"),
            new Customer(4, "Kenyetta Lone"),
            new Customer(5, "Vernita Fernald"),
            new Customer(6, "Tessie Storrs")
        };

        [HttpGet]
        public IActionResult Index()
        {
            return Json(_customers);
        }

        [HttpGet("{id:int}")]
        public IActionResult Index(int id)
        {
            var customer = _customers.FirstOrDefault(x => x.CustomerId == id);

            if (customer == null)
                return NotFound();

            return Json(customer);
        }
    }
}