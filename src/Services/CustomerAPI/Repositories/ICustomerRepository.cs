using CustomerAPI.DTOs;
using CustomerAPI.Models;
namespace CustomerAPI.Repositories
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Customer>> GetCustomers();
        Task<Customer> GetCustomer(string id);
        Task CreateCustomer(CustomerCreateDto dto,Customer customer);
        Task UpdateCustomer(Customer customer);
        Task DeleteCustomer(string id);
    }
}