namespace ProductApi.Dto;

public class ProductDto
{
    public string ProductName { get; set; } = null!;
    public decimal ProductPrice { get; set; }
    public string? Features { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? Description { get; set; }
    public required string Size { get; set; }
    public IFormFile? Image { get; set; }
    public List<int> ColorIds { get; set; } = [];
}
