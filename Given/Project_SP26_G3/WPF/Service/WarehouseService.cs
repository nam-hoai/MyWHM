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
        private readonly MyContext _context;

        public WarehouseService()
        {
            _context = new MyContext();
        }

        public void Add(Warehouse house)
        {
            //check duplication
            if (_context.Warehouses.Any(p => p.WarehouseName.Equals(house.WarehouseName)))
            {
                throw new Exception($"{house.WarehouseName} is duplicated, try with another");
            }
            if (_context.Warehouses.Any(p => p.WarehouseId == house.WarehouseId))
            {
                Warehouse w = _context.Warehouses.First(p => p.WarehouseId == house.WarehouseId);
                throw new Exception($"Warehouse ID: {house.WarehouseId} is used for {w.WarehouseName.ToString()}, try with another");
            }
            _context.Warehouses.Add(house);
            _context.SaveChanges();
        }

        public void Delete(Warehouse house)
        {
            var w = GetWareHouse(house.WarehouseId);

            if (w != null)
            {
                _context.Warehouses.Remove(w);
                _context.SaveChanges();
            }
        }

        public List<Warehouse> GetAll()
        {
            return _context.Warehouses.ToList();
        }

        public Warehouse GetWareHouse(int id)
        {
            return _context.Warehouses.FirstOrDefault(p => p.WarehouseId == id)
            ?? throw new Exception($"No found warehouse with ID = {id}");
        }

        public List<Warehouse> Search(string key)
        {
            var query = _context.Warehouses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(key))
            {
                //search by single word
                var keywords = key.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in keywords)
                {
                    query = query.Where(p =>
                        p.WarehouseName.Contains(word) ||
                        p.Size.ToString().Contains(word) ||
                        p.Status.ToString().Equals(word));
                }
            }

            return query.ToList();
        }

        public void Update(Warehouse house)
        {
            try
            {
                //Find Person
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
                // Update Person
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
