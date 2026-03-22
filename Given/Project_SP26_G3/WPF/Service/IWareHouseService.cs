using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF.Models;

namespace WPF.Service
{
    public interface IWareHouseService
    {
        List<Warehouse> GetAll();
        Warehouse GetWareHouse(int id);
        void Add(Warehouse house);
        void Update(Warehouse house);
        void Delete(Warehouse house);
        List<Warehouse> Search(string key);
    }
}
