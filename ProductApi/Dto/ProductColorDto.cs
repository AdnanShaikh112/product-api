namespace ProductApi.Dto;

public class ProductColorDto
{
    public int ProductId { get; set; }
    public required string ProductName { get; set; }
    public decimal ProductPrice { get; set; }
    public string? Features { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string? Description { get; set; }
    public required string Size { get; set; }
    public IFormFile? Image { get; set; }
    public List<string>? Colors { get; set; }
}
