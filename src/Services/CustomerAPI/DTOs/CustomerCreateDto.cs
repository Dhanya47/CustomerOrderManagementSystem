namespace CustomerAPI.DTOs
{
    public class CustomerCreateDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int AddressId {  get; set; }
        public AddressDto Address { get; set; }
    }
    public class AddressDto
    {
        public int? AddressId { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
    }
}