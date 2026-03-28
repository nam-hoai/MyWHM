using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF.Models;

namespace WPF.Service
{
    public class WarehouseService : IWareHouseService
    {

        public WarehouseService(){}

        public void Add(Warehouse house)
        {
            using var _context = new MyContext();
            //check duplication
            if (_context.Warehouses.Any(p => p.WarehouseName.Equals(house.WarehouseName)))
                throw new Exception($"{house.WarehouseName} is duplicated, try with another");
            
            var existing = _context.Warehouses.First(p => p.WarehouseId == house.WarehouseId);

            if (existing!=null)
                throw new Exception($"Warehouse ID: {house.WarehouseId} is used for {existing.WarehouseName.ToString()}, try with another");
            //Add
            _context.Warehouses.Add(house);

            _context.SaveChanges();
        }

        public void Delete(Warehouse house)
        {
            using var _context = new MyContext();
            var w = GetWareHouse(house.WarehouseId);

            if (w != null)
            {
                _context.Warehouses.Remove(w);
                _context.SaveChanges();
            }
        }

        public List<Warehouse> GetAll()
        {
            using var _context = new MyContext();
            return _context.Warehouses.AsNoTracking().ToList();
        }
        private Warehouse? FindWarehouse(int id)
        {
            using var _context = new MyContext();
            return _context.Warehouses.FirstOrDefault(p => p.WarehouseId == id);
        }
        public Warehouse GetWareHouse(int id)
        {
            return FindWarehouse(id)
            ?? throw new Exception($"No found warehouse with ID = {id}");
        }

        public List<Warehouse> Search(string key)
        {
            using var _context = new MyContext();
            var query = _context.Warehouses.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(key))
            {
                //search by single word
                var keywords = key.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in keywords)
                {
                    query = query.Where(p =>
                        p.WarehouseName.Contains(word) ||
                        (p.Size.ToString()??"").Contains(word) ||
                        p.Status.ToString().Equals(word));
                }
            }

            return query.ToList();
        }

        public void Update(Warehouse house)
        {
            try
            {
                using var _context = new MyContext();
                //Find warehouse
                var existing = _context.Warehouses.FirstOrDefault(x => x.WarehouseId == house.WarehouseId);
                if (existing == null)
                {
                    throw new Exception("No found warehouse!");
                }
                //check duplication
                bool isDuplicate = _context.Warehouses
                .Any(x => x.WarehouseName == house.WarehouseName
                       && x.WarehouseId != house.WarehouseId);

                if (isDuplicate)
                    throw new Exception("Warehouse name already exists");
                // Update warehouse
                existing.WarehouseName = house.WarehouseName;
                existing.Size = house.Size;
                existing.Status = house.Status;

                _context.SaveChanges();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
