using ECommerceSystem.Models;
using ECommerceSystem.Repositories;
using ECommerceSystem.Exceptions;
using ECommerceSystem.DTOs;

namespace ECommerceSystem.Services;

/// <summary>
/// Product service with business logic
/// </summary>
public class ProductService
{
    private readonly ProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Product> CreateProductAsync(CreateProductDto dto)
    {
        ValidateProductDto(dto);

        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            SKU = dto.SKU,
            CategoryId = dto.CategoryId,
            Status = ProductStatus.Active
        };

        var result = await _productRepository.AddAsync(product);
        _logger.LogInformation($"Product created: {product.Name} (ID: {product.Id})");
        return result;
    }

    public async Task<Product> UpdateProductAsync(int id, UpdateProductDto dto)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
            throw new EntityNotFoundException(nameof(Product), id);

        product.Name = dto.Name ?? product.Name;
        product.Description = dto.Description ?? product.Description;
        product.Price = dto.Price ?? product.Price;
        product.Status = dto.Status ?? product.Status;
        product.UpdatedAt = DateTime.UtcNow;

        var result = await _productRepository.UpdateAsync(product);
        _logger.LogInformation($"Product updated: {product.Name} (ID: {product.Id})");
        return result;
    }

    public async Task<Product?> GetProductAsync(int id)
    {
        return await _productRepository.GetByIdAsync(id);
    }

    public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId)
    {
        return await _productRepository.GetByCategoryAsync(categoryId);
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<Product>();

        return await _productRepository.SearchProductsAsync(searchTerm);
    }

    public async Task<List<Product>> GetLowStockProductsAsync(int threshold = 10)
    {
        return await _productRepository.GetLowStockProductsAsync(threshold);
    }

    public async Task<List<Product>> GetTopRatedProductsAsync(int count = 10)
    {
        return await _productRepository.GetTopRatedProductsAsync(count);
    }

    public async Task<bool> RestockProductAsync(int productId, int quantity)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new EntityNotFoundException(nameof(Product), productId);

        product.RestockProduct(quantity);
        await _productRepository.UpdateAsync(product);
        _logger.LogInformation($"Product restocked: {product.Name}, Added: {quantity}");
        return true;
    }

    public async Task<decimal> GetInventoryValueAsync()
    {
        return await _productRepository.GetTotalInventoryValueAsync();
    }

    private void ValidateProductDto(CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ValidationException("Product name is required");

        if (dto.Price <= 0)
            throw new ValidationException("Price must be greater than zero");

        if (dto.StockQuantity < 0)
            throw new ValidationException("Stock quantity cannot be negative");

        if (string.IsNullOrWhiteSpace(dto.SKU))
            throw new ValidationException("SKU is required");
    }
}
