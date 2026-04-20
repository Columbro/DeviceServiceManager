
namespace DeviceServiceManager.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; } = string.Empty;
        public string HouseNumber { get; set; } = string.Empty;
        public string ZipCode {  get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int CountryId { get; set; } = 1;
        public DateTime CreatedAt { get; set; }

        public Address Clone()
        {
            return new Address
            {
                Id = this.Id,
                Street = this.Street,
                HouseNumber = this.HouseNumber,
                ZipCode = this.ZipCode,
                City = this.City,
                CountryId = this.CountryId,
                CreatedAt = this.CreatedAt
            };
        }
    }
}
