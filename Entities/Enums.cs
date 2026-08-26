namespace EndustriB2C.Entities;

public enum UserType : byte
{
    Admin = 0,
    User = 1
}

public enum DiscountType : byte
{
    Percent = 0,
    Amount = 1
}

public enum OrderStatus : byte
{
    Pending = 0,
    Confirmed = 1,
    Preparing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5
}
