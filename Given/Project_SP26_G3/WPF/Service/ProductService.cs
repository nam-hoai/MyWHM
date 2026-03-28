using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WPF.Models;

namespace WPF.Service
{
    public class ProductService : IProductService
    {
        public ProductService(){}
        public List<Person> GetAllUser()
        {
            using var _context = new MyContext();
            return _context.Persons.AsNoTracking().Where(p => p.RoleId == 2).ToList();
        }
        public List<Warehouse> GetAllLocation()
        {
            using var _context = new MyContext();
            return _context.Warehouses.AsNoTracking().ToList();
        }
        public List<Product> GetAllProduct()
        {
            using var _context = new MyContext();
            return _context.Products.AsNoTracking()
                .Include(p => p.Cat)
                .Include(p=> p.SenderNavigation)
                .Include(p=> p.LocationNavigation)
                .ToList();
        }
        public List<Category> GetAllCategory()
        {
            using var _context = new MyContext();
            return _context.Categories.AsNoTracking().ToList();
        }
        private Product? FindProduct(int id)
        {
            using var _context = new MyContext();
            return _context.Products.AsNoTracking().FirstOrDefault(p => p.ProductId == id);
        }
        public Product GetProduct(int id)
        {
            return FindProduct(id)
            ?? throw new Exception($"No found product with ID = {id}");
        }
        public void Add(Product product)
        {
            using var _context = new MyContext();
            //check duplication
            if (_context.Products.Any(p => p.ProductCode.Equals(product.ProductCode)))
            {
                throw new Exception($"{product.ProductName} is duplicated 'cause {product.ProductCode} is used, try with another");
            }
            if (_context.Products.Any(p => p.ProductId == product.ProductId))
            {
                Product p = _context.Products.First(p => p.ProductId == product.ProductId);
                throw new Exception($"Product ID: {product.ProductId} is used for {(p.ProductName??"N/A")}, try with another");
            }
            var warehouse = _context.Warehouses.FirstOrDefault(x => x.WarehouseName.Equals(product.Location));
            if (warehouse == null)
            {
                MessageBox.Show($"Location {product.Location} is not exist.");
                return;
            }
            if (warehouse.Status == false)
            {
                MessageBox.Show($"Location {product.Location} is disable, try with another");
                return;
            }
            _context.Products.Add(product);
            _context.SaveChanges();
        }
        public void Update(Product product)
        {
            using var _context = new MyContext();
            try
            {
                //Find Product
                var existing = _context.Products.FirstOrDefault(x => x.ProductId == product.ProductId);
                if (existing == null)
                {
                    throw new Exception("No found product!");
                }
                //check warehouse status
                var warehouse = _context.Warehouses.FirstOrDefault(x => x.WarehouseName.Equals(product.Location));
                if (warehouse == null)
                {
                    MessageBox.Show($"Location {product.Location} is not exist.");
                    return;
                }
                if (warehouse.Status == false)
                {
                    MessageBox.Show($"Location {product.Location} is disable, try with another");
                    return;
                }
                //check duplication
                bool isDuplicate = _context.Products
                .Any(x => x.Sender!=existing.Sender 
                       && x.ProductId!=existing.ProductId
                       && x.ProductCode==existing.ProductCode);

                if (isDuplicate)
                    throw new Exception("This product already exists");
                
                
                // Update Product
                existing.ProductName = product.ProductName;
                existing.Quantity = product.Quantity;
                existing.Location = product.Location;
                existing.Sender = product.Sender;
                existing.DateIn = product.DateIn;
                existing.DateOut = product.DateOut;
                existing.CatId = product.CatId;
                existing.Status = product.Status;

                _context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void Delete(Product product)
        {
            using var _context = new MyContext();
            var existing = _context.Products.FirstOrDefault(x => x.ProductId == product.ProductId);

            if (existing != null)
            {
                _context.Products.Remove(existing);
                _context.SaveChanges();
            }
        }
        public List<Product> Search(string key, DateTime? dateIn, DateTime? dateOut)
        {
            using var _context = new MyContext();
            var query = _context.Products.AsNoTracking()
                .Include(p => p.Cat)
                .Include(p => p.SenderNavigation)
                .Include(p => p.LocationNavigation).AsQueryable();

            if (!string.IsNullOrWhiteSpace(key))
            {
                //search by single word
                var keywords = key.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in keywords)
                {
                    query = query.Where(p =>
                        (p.ProductName??"").Contains(word) ||
                        p.ProductCode.Contains(word) ||
                        (p.Sender??"").Contains(word) ||
                        (p.Location??"").Contains(word) ||
                        (p.Cat.CatName??"").Contains(word) ||
                        p.Status.ToString().Equals(word));
                }
            }
            //search by date
            var fromDate = dateIn?.Date;
            var toDate = dateOut?.Date.AddDays(1).AddTicks(-1);

            if (fromDate.HasValue)
            {
                query = query.Where(p => p.DateIn >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(p => p.DateIn <= toDate.Value);
            }

            return query.ToList();
        }
    }
}
