using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF.Models;

namespace WPF.Service
{
    public interface IProductService
    {
        List<Product> GetAllProduct();
        List<Category> GetAllCategory();
        List<Person> GetAllUser();
        List<Warehouse> GetAllLocation();
        Product GetProduct(int id);
        void Add(Product product);
        void Update(Product product);
        void Delete(Product product);
        List<Product> Search(string key, DateTime? dateIn, DateTime? dateOut);
    }
}
