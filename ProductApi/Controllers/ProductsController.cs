using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Data;
using ProductApi.Dto;
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

            var products = await query.Include(p => p.ProductColors).ThenInclude(pc => pc.Color)
                .Skip((queryParams.Page - 1) * queryParams.PageSize).Take(queryParams.PageSize)
                .Select(p => new ProductColorDto()
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductPrice = p.ProductPrice,
                    Features = p.Features,
                    PurchaseDate = p.PurchaseDate,
                    Description = p.Description,
                    Size = p.Size,
                    Colors = p.ProductColors.Select(pc => pc.Color.ColorName).ToList()

                }).ToListAsync();

            return Ok(new { Data = products, TotalRecords = totalRecords });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await context.Products.Where(p => p.ProductId == id)
                .Select(p => new
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    ProductPrice = p.ProductPrice,
                    Features = p.Features,
                    PurchaseDate = p.PurchaseDate,
                    Description = p.Description,
                    Size = p.Size,
                    ColorIds = p.ProductColors.Select(pc => pc.Color.ColorId).ToList(),
                    ImagePath = p.ImagePath
                }).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductDto dto)
        {
            string? imagePath = null;

            if (dto.Image != null)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
                var fullPath = Path.Combine(folder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await dto.Image.CopyToAsync(stream);

                imagePath = "/images/" + fileName;
            }

            var product = new Product
            {
                ProductName = dto.ProductName,
                ProductPrice = dto.ProductPrice,
                Features = dto.Features,
                PurchaseDate = dto.PurchaseDate,
                Description = dto.Description,
                Size = dto.Size,
                ImagePath = imagePath
            };

            context.Products.Add(product);
            await context.SaveChangesAsync();

            foreach (var colorId in dto.ColorIds)
            {
                context.ProductColors.Add(new ProductColor
                {
                    ProductId = product.ProductId,
                    ColorId = colorId
                });
            }

            await context.SaveChangesAsync();

            var response = new ProductResponseDto
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductPrice = product.ProductPrice,
                Features = product.Features,
                PurchaseDate = product.PurchaseDate,
                Description = product.Description,
                Size = product.Size,
                ColorIds = dto.ColorIds,
                Image = dto.Image
            };

            return Ok(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, ProductDto product)
        {
            var existing = await context.Products.FindAsync(id);
            if (existing == null) return NotFound();

            existing.ProductName = product.ProductName;
            existing.ProductPrice = product.ProductPrice;
            existing.Features = product.Features;
            existing.PurchaseDate = product.PurchaseDate;
            existing.Description = product.Description;
            existing.Size = product.Size;

            var oldColors = context.ProductColors
            .Where(pc => pc.ProductId == id);

            context.ProductColors.RemoveRange(oldColors);

            foreach (var colorId in product.ColorIds)
            {
                context.ProductColors.Add(new ProductColor
                {
                    ProductId = id,
                    ColorId = colorId
                });
            }

            if (product.Image != null)
            {
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + Path.GetExtension(product.Image.FileName);
                var fullPath = Path.Combine(folder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await product.Image.CopyToAsync(stream);

                existing.ImagePath = "/images/" + fileName;
            }

            await context.SaveChangesAsync();

            var response = new ProductResponseDto
            {
                ProductId = existing.ProductId,
                ProductName = existing.ProductName,
                ProductPrice = existing.ProductPrice,
                Features = existing.Features,
                PurchaseDate = existing.PurchaseDate,
                Description = existing.Description,
                Size = existing.Size,
                ColorIds = product.ColorIds,
                Image = product.Image
            };

            return Ok(response);
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
