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
        Person GetPerson(int id);
        void Add(Person person);
        void Update(Person person);
        void Delete(Person person);
        List<Person> Search(string key);
    }
}
