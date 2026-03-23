using CustomerAPI.DTOs;
using CustomerAPI.Models;
using CustomerAPI.PubSub;
using CustomerAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
namespace CustomerAPI.Controllers
{
    [ApiController]
    [Route("api/customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository _repository;
        private readonly PubSubPublisher _publisher;
        public CustomerController(ICustomerRepository repository, PubSubPublisher publisher)
        {
            _repository = repository;
            _publisher = publisher;
        }
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _repository.GetCustomers();
            return Ok(customers);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(string id)
        {
            var customers = await _repository.GetCustomer(id);
            return Ok(customers);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer(CustomerCreateDto dto)
        {
            var customer = new Customer {
                CustomerId = Guid.NewGuid().ToString(),
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                AddressId = dto.AddressId
            };
            await _repository.CreateCustomer(dto,customer);
            await _publisher.PublishMessage("CustomerCreated");
            return Ok(customer);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(string id, Customer customer)
        {
            if (customer == null)
                return BadRequest();
            customer.CustomerId = id;
            await _repository.UpdateCustomer(customer);
            // Publish event
            await _publisher.PublishMessage("CustomerUpdated");
            return Ok(customer);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            await _repository.DeleteCustomer(id);
            return Ok("Customer deleted successfully");
        }
    }
}