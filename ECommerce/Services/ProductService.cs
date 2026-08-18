using ECommerce.DBContext;
using ECommerce.Entities;
using ECommerce.Shared;
using ECommerce.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync();
        Task<List<ECommerce.ViewModel.ProductImages>> GetByIdAsync(int id);
        Task<Product> AddAsync(ProductModel product);
        Task<bool> UpdateAsync(ProductModel product);
        Task<bool> DeleteAsync(int id);
    }
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductService> _logger;
        private readonly IFileService _fileUploadService;

        public ProductService(AppDbContext context, ILogger<ProductService> logger,IFileService fileService)
        {
            _context = context;
            _logger = logger;
            _fileUploadService = fileService;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            return await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<ECommerce.ViewModel.ProductImages>> GetByIdAsync(int id)
        {
            var Images =   await _context.ProductImages
                .AsNoTracking().Where(x=>x.ProductID == id)
                .ToListAsync();
            var result = new List<ECommerce.ViewModel.ProductImages>();
            foreach (var item in Images)
            {
                result.Add(new ECommerce.ViewModel.ProductImages
                {
                    Id = item.Id,
                    ImagePath = item.ImagePath
                });
            }

            return result;
        }

        public async Task<Product> AddAsync(ProductModel product)
        {
           
            product.ImagePath = await this._fileUploadService.UploadFileWithBase64Async(product.ImagePath, product.Extension);
            Product product1 = new Product
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Size = product.Size,
                Color = product.Color,
                Brand = product.Brand,
                StyleId = product.StyleId,
                StockQuantity = product.StockQuantity,
                ImagePath = product.ImagePath,
                CreatedAt = DateTime.UtcNow
            };
            _context.Products.Add(product1);
            await _context.SaveChangesAsync();
            var ProductImages = new List<ECommerce.Entities.ProductImages>();
            foreach (var items in product.ProductImages!)
            {

                items.ImagePath = await this._fileUploadService.UploadFileWithBase64Async(items.ImagePath, items.Extension);
                var ProductItem = new ECommerce.Entities.ProductImages
                {
                    ProductID = product1.Id,
                    ImagePath = items.ImagePath
                };
                ProductImages.Add(ProductItem);
            }
            _context.ProductImages.AddRange(ProductImages);
            await _context.SaveChangesAsync();
            return product1;
        }

        public async Task<bool> UpdateAsync(ProductModel product)
        {
            var existing = await _context.Products.FindAsync(product.Id);
            if (existing is null) return false;

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.Size = product.Size;
            existing.Color = product.Color;
            existing.Brand = product.Brand;
            existing.StyleId = product.StyleId;
            existing.StockQuantity = product.StockQuantity;

            // Only overwrite image if a new one was uploaded
            if (!string.IsNullOrWhiteSpace(product.ImagePath) && product.ImagePath.Contains("data"))
                existing.ImagePath = await this._fileUploadService.UploadFileWithBase64Async(product.ImagePath, product.Extension);

            await _context.SaveChangesAsync();
            var existingImages = await _context.ProductImages.Where(x=>x.ProductID == product.Id).ToListAsync();
            var ProductImages = new List<ECommerce.Entities.ProductImages>();
            foreach (var items in product.ProductImages)
            {
                if(items.ImagePath.Contains("data"))
                 items.ImagePath = await this._fileUploadService.UploadFileWithBase64Async(items.ImagePath, items.Extension);
                var ProductItem = new ECommerce.Entities.ProductImages
                {
                    ProductID = existing.Id,
                    ImagePath = items.ImagePath
                };
                ProductImages.Add(ProductItem);
            }
            if (existingImages!= null)
            _context.ProductImages.RemoveRange(existingImages!);
            _context.ProductImages.AddRange(ProductImages);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return false;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
