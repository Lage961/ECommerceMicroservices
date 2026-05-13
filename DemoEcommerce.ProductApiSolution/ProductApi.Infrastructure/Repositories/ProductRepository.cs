using eCommerce.SharedLibrary.Logs;
using eCommerce.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using ProductApi.Application.Interfaces;
using ProductApi.Domain.Entities;
using ProductApi.Infrastructure.Data;
using System.Linq.Expressions;

namespace ProductApi.Infrastructure.Repositories
{
    internal class ProductRepository(ProductDbContext context) : IProduct
    {
        public async Task<Response> CreateAsync(Product entity)
        {
            try
            {
                var getProduct = await GetByAsync(p => p.Name == entity.Name);

                if(getProduct is not null && !string.IsNullOrEmpty(getProduct.Name)) 
                {
                    return new Response(false, "Product with the same name already exists");
                }

                var currentEntity = context.Products.Add(entity).Entity;
                await context.SaveChangesAsync();

                if (currentEntity is not null && currentEntity.Id > 0)
                {
                    return new Response(true, "Product added successfully");
                }
                else
                {
                    return new Response(false, "Failed to add product");
                }
            }
            catch (Exception ex) 
            {
                // Log the exception using my logging mechanism
                LogException.LogExceptions(ex);

                // Display a user-friendly message or return an appropriate response
                return new Response(false, "Error ocurred adding new product");
            }
        }

        public async Task<Response> DeleteAsync(Product entity)
        {
            try
            {
                var product = await FindByIdAsync(entity.Id);

                if(product is null) 
                {
                    return new Response(false, "Product not found");
                }

                context.Products.Remove(entity);
                await context.SaveChangesAsync();

                return new Response(true, $"{entity.Name} was removed successfully"); ;
            }
            catch (Exception ex) 
            {
                // Log the exception using my logging mechanism
                LogException.LogExceptions(ex);

                // Display a user-friendly message or return an appropriate response
                return new Response(false, "Error ocurred removing product");
            }
        }

        public async Task<Product> FindByIdAsync(int id)
        {
            try
            {
                var product = await context.Products.FindAsync(id);
                if(product is not null)
                {
                    return product;
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception ex)
            {
                // Log the exception using my logging mechanism
                LogException.LogExceptions(ex);

                // Display a user-friendly message or return an appropriate response
                throw new Exception("Error ocurred retrieving product"); 
            }
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            try
            {
                var products = await context.Products
                    .AsNoTracking()
                    .ToListAsync();

                return products;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);

                throw new InvalidOperationException("Error ocurred retrieving products");
            }
        }

        public async Task<Product> GetByAsync(Expression<Func<Product, bool>> predicate)
        {
            try
            {
                var product = await context.Products
                    .Where(predicate)
                    .FirstOrDefaultAsync();

                if (product is not null)
                {
                    return product;
                }

                return null!;
            }
            catch (Exception ex)
            {
                LogException.LogExceptions(ex);

                throw new Exception("Error ocurred retrieving product");
            }

        }

        public async Task<Response> UpdateAsync(Product entity)
        {
            try
            {
                var product = await FindByIdAsync(entity.Id);

                if(product is null)
                {
                    return new Response(false, "Product not found");
                }

                context.Entry(product).State = EntityState.Detached;
                context.Products.Update(entity);
                await context.SaveChangesAsync();

                return new Response(true, $"{entity.Name} is Updated successifully");

            }
            catch (Exception ex) 
            {
                LogException.LogExceptions(ex);

                return new Response(false, "Error ocurred Updating product");
            }
        }
    }
}
