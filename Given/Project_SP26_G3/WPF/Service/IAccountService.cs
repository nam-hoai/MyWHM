using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF.Models;

namespace WPF.Service
{
    public interface IAccountService
    {
        List<Person> GetAll();
        void Add(Person person);
        void Update(Person person);
        void Delete(int id);
        List<Person> Search(string name, string phone, string address);
    }
}
