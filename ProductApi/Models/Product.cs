namespace ProductApi.Models;

public class Product
{
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public string? Features { get; set; }
    public DateTime? PurchaseDate { get; set; }
}
