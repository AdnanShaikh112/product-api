namespace ProductApi.Models;

public class ProductQueryParams
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public string? Search { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Features { get; set; }
    public string? SortBy { get; set; }
    public string? SortOrder { get; set; }
}
