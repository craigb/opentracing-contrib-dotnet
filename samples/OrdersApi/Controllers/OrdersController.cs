using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Constants;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenTracing.Contrib.AspNetCore;

namespace Samples.OrdersApi.Controllers
{
    [Route("orders")]
    public class OrdersController : Controller
    {
        private readonly HttpClient _httpClient;

        public OrdersController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] PlaceOrderCommand cmd)
        {
            await Task.Delay(new Random().Next(1, 80));

            var customer = await GetCustomer(cmd.CustomerId.Value);

            var span = HttpContext.GetCurrentSpan();

            span.Log(new Dictionary<string, object> {
                { "event", "OrderPlaced" },
                { "customer", cmd.CustomerId },
                { "customer_name", customer.Name },
                { "item_number", cmd.ItemNumber },
                { "quantity", cmd.Quantity }
            });

            return Ok();
        }

        private async Task<Customer> GetCustomer(int customerId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(Constants.CustomersUrl + "customers/" + customerId)
            };

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Customer>(body);
        }
    }
}