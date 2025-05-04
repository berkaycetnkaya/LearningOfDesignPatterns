using WebApp.Strategy.Models;

namespace WebApp.Strategy.Repositories
{
    public interface IProductRepository
    {
        public Task<Product> GetById(string id);

        public Task<List<Product>> GetAllByUserId(string userId);

        public Task<Product> Save(Product product);

        public Task Update(Product product);

        public Task Delete(Product product);


    }
}
