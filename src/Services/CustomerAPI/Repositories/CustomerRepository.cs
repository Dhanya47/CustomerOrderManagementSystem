using CustomerAPI.Data;
using CustomerAPI.DTOs;
using CustomerAPI.Models;
using Google.Cloud.Spanner.Data;
namespace CustomerAPI.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly SpannerDbContext _context;
        public CustomerRepository(SpannerDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Customer>> GetCustomers()
        {
            var customers = new List<Customer>();
            using var connection = _context.GetConnection();
            await connection.OpenAsync();
            //var cmd = connection.CreateSelectCommand("SELECT * FROM Customers");
            var cmd = connection.CreateSelectCommand(@"
               SELECT
                   c.CustomerId,
                   c.Name,
                   c.Email,
                   c.Phone,
                   c.AddressId,
                   a.AddressId AS AddrId,
                   a.Street,
                   a.City,
                   a.State,
                   a.ZipCode
               FROM Customers c
               LEFT JOIN Addresses a ON c.AddressId = a.AddressId
            ");
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                customers.Add(new Customer
                {
                    CustomerId = reader.GetFieldValue<string>("CustomerId"),
                    Name = reader.GetFieldValue<string>("Name"),
                    Email = reader.GetFieldValue<string>("Email"),
                    Phone = reader.GetFieldValue<string>("Phone"),
                    AddressId = Convert.ToInt32(reader.GetFieldValue<long>("AddressId")),
                    Address = reader.IsDBNull(reader.GetOrdinal("Street")) ? null : new Address
                    {
                        AddressId = reader.GetFieldValue<int>("AddressId"),
                        Street = reader.GetFieldValue<string>("Street"),
                        City = reader.GetFieldValue<string>("City"),
                        State = reader.GetFieldValue<string>("State"),
                        ZipCode = reader.GetFieldValue<string>("ZipCode")
                    }
                });
            }
            return customers;
        }
        public async Task<Customer?> GetCustomer(string id)
        {
            using var connection = _context.GetConnection();
            await connection.OpenAsync();
            var cmd = connection.CreateSelectCommand(@"
       SELECT
           c.CustomerId,
           c.Name,
           c.Email,
           c.Phone,
           c.AddressId,
           a.AddressId AS AddrId,
           a.Street,
           a.City,
           a.State,
           a.ZipCode
       FROM Customers c
       LEFT JOIN Addresses a ON c.AddressId = a.AddressId
       WHERE c.CustomerId = @id
   ");
            cmd.Parameters.Add("id", SpannerDbType.String, id);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Customer
                {
                    CustomerId = reader.GetFieldValue<string>("CustomerId"),
                    Name = reader.GetFieldValue<string>("Name"),
                    Email = reader.GetFieldValue<string>("Email"),
                    Phone = reader.GetFieldValue<string>("Phone"),
                    AddressId = Convert.ToInt32(reader.GetFieldValue<long>("AddressId")),
                    Address = reader.IsDBNull(reader.GetOrdinal("Street")) ? null : new Address
                    {
                        AddressId = reader.GetFieldValue<int>("AddressId"),
                        Street = reader.GetFieldValue<string>("Street"),
                        City = reader.GetFieldValue<string>("City"),
                        State = reader.GetFieldValue<string>("State"),
                        ZipCode = reader.GetFieldValue<string>("ZipCode")
                    }
                };
            }
            return null;
        }
        public async Task CreateCustomer(CustomerCreateDto dto,Customer customer)
        {
            using var connection = _context.GetConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                int addressId;
                if (dto.AddressId != 0)
                {
                    var checkCmd = connection.CreateSelectCommand(
                        "SELECT AddressId FROM Addresses WHERE AddressId = @id",
                        new SpannerParameterCollection
                        {
                            { "id", SpannerDbType.Int64, dto.AddressId }
                        });
                    checkCmd.Transaction = transaction;
                    using var reader = await checkCmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        addressId = dto.AddressId;
                    }
                    else
                    {
                        addressId = dto.Address.AddressId.Value;
                        await InsertAddress(connection, transaction, dto.Address, addressId);
                    }
                }
                else
                {
                    addressId = dto.Address.AddressId.Value; ;
                    await InsertAddress(connection, transaction, dto.Address, addressId);
                }
                var customerCmd = connection.CreateInsertCommand(
                    "Customers",
                    new SpannerParameterCollection
                    {
                       { "CustomerId", SpannerDbType.String, customer.CustomerId },
                       { "Name", SpannerDbType.String, customer.Name },
                       { "Email", SpannerDbType.String, customer.Email },
                       { "Phone", SpannerDbType.String, customer.Phone },
                       { "AddressId", SpannerDbType.Int64, (long)customer.AddressId }
                    });
                customerCmd.Transaction = transaction;
                await customerCmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                try
                {
                    await transaction.RollbackAsync();
                }
                catch { }
                throw;
            }
        }
       // private long GenerateAddressId()
        //{
        //    return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //}
        private async Task InsertAddress(SpannerConnection connection,SpannerTransaction transaction,AddressDto address,long addressId)
        {
            var cmd = connection.CreateInsertCommand(
                "Addresses",
                new SpannerParameterCollection
                {
           { "AddressId", SpannerDbType.Int64, (long)addressId },
           { "Street", SpannerDbType.String, address.Street },
           { "City", SpannerDbType.String, address.City },
           { "State", SpannerDbType.String, address.State },
           { "ZipCode", SpannerDbType.String, address.ZipCode }
                });
            cmd.Transaction = transaction;
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task UpdateCustomer(Customer customer)
        {
            using var connection = _context.GetConnection();
            await connection.OpenAsync();
            var cmd = connection.CreateDmlCommand(
                "UPDATE Customers SET Name=@Name, Email=@Email, Phone=@Phone, AddressId=@AddressId WHERE CustomerId=@CustomerId");
            cmd.Parameters.Add("Name", SpannerDbType.String, customer.Name);
            cmd.Parameters.Add("Email", SpannerDbType.String, customer.Email);
            cmd.Parameters.Add("Phone", SpannerDbType.String, customer.Phone);
            cmd.Parameters.Add("AddressId", SpannerDbType.Int64, customer.AddressId);
            cmd.Parameters.Add("CustomerId", SpannerDbType.String, customer.CustomerId);
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task DeleteCustomer(string id)
        {
            using var connection = _context.GetConnection();
            await connection.OpenAsync();
            var cmd = connection.CreateDmlCommand(
                $"DELETE FROM Customers WHERE CustomerId='{id}'");
            await cmd.ExecuteNonQueryAsync();
        }
    }
}