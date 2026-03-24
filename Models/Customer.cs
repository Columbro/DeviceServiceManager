
namespace DeviceServiceManager.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string CustomerNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Nullable because it is optional in the database
        public string? ContactPerson { get; set; }
        public string? Email {  get; set; }
        public string? Phone { get; set; }

        // Foreign key for addresses
        public int BillingAddressId { get; set; }
        public int DeliveryAddressId { get; set; }

        public Address? BillingAddress { get; set; }
        public Address? DeliveryAddress { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
