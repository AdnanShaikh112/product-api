using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Models;

namespace ProductApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(AppDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParams queryParams)
        {
            var query = context.Products.AsQueryable();

            bool isDesc = queryParams.SortOrder?.ToLower() == "desc";

            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                query = query.Where(p =>
                    p.ProductName.Contains(queryParams.Search));
            }

            if (queryParams.FromDate.HasValue)
            {
                query = query.Where(p =>
                    p.PurchaseDate >= queryParams.FromDate.Value);
            }

            if (queryParams.ToDate.HasValue)
            {
                query = query.Where(p =>
                    p.PurchaseDate <= queryParams.ToDate.Value);
            }
            if (!string.IsNullOrEmpty(queryParams.Features))
            {
                var selected = queryParams.Features.Split(',');

                query = query.Where(p => selected.All(f => p.Features!.Contains(f)));
            }
            if (queryParams.MinPrice.HasValue)
            {
                query = query.Where(p => p.ProductPrice >= queryParams.MinPrice.Value);
            }

            if (queryParams.MaxPrice.HasValue)
            {
                query = query.Where(p => p.ProductPrice <= queryParams.MaxPrice.Value);
            }

            query = queryParams.SortBy?.ToLower() switch
            {
                "name" => isDesc
                    ? query.OrderByDescending(p => p.ProductName)
                    : query.OrderBy(p => p.ProductName),

                "price" => isDesc
                    ? query.OrderByDescending(p => p.ProductPrice)
                    : query.OrderBy(p => p.ProductPrice),

                "purchasedate" => isDesc
                    ? query.OrderByDescending(p => p.PurchaseDate)
                    : query.OrderBy(p => p.PurchaseDate),

                _ => query.OrderBy(p => p.ProductId)
            };

            var totalRecords = await query.CountAsync();

            var products = await query.Skip((queryParams.Page - 1) * queryParams.PageSize)
                .Take(queryParams.PageSize).ToListAsync();

            return Ok(new { Data = products, TotalRecords = totalRecords });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id) =>
            Ok(await context.Products.FindAsync(id));

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            context.Products.Add(product);
            await context.SaveChangesAsync();
            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Product product)
        {
            var existing = await context.Products.FindAsync(id);
            if (existing == null) return NotFound();

            existing.ProductName = product.ProductName;
            existing.ProductPrice = product.ProductPrice;
            existing.Features = product.Features;
            existing.PurchaseDate = product.PurchaseDate;

            await context.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await context.Products.FindAsync(id);
            if (product == null) return NotFound();

            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("price-range")]
        public async Task<IActionResult> GetPriceRange()
        {
            var minPrice = await context.Products.MinAsync(p => p.ProductPrice);
            var maxPrice = await context.Products.MaxAsync(p => p.ProductPrice);

            int roundedMin = RoundDown(minPrice);
            int roundedMax = RoundUp(maxPrice);

            return Ok(new
            {
                Min = roundedMin,
                Max = roundedMax
            });
        }
        private int RoundDown(decimal value)
        {
            return ((int)value / 1000) * 1000;
        }
        private int RoundUp(decimal value)
        {
            return (((int)value / 1000) + 1) * 1000;
        }
    }
}
