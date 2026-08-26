namespace EndustriB2C.Entities;

public class UserAddress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = "Adres";
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }

    public User User { get; set; } = null!;
}
