namespace ProductApi.Models;

public class Product
{
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public string? Features { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? Description { get; set; }
    public required string Size { get; set; }
    public string? ImagePath { get; set; }
    public ICollection<ProductColor> ProductColors { get; set; } = [];
}
