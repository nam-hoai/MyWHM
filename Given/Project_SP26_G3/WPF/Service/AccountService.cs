using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF.Models;
using WPF.Service;
public class AccountService : IAccountService
{
    private readonly MyContext _context;
    public AccountService()
    {
        _context = new MyContext();
    }
    public void Add(Person person)
    {
        _context.Persons.Add(person);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var p = _context.Persons.Find(id);

        if (p != null)
        {
            _context.Persons.Remove(p);
            _context.SaveChanges();
        }
    }

    public List<Person> GetAll()
    {
        return _context.Persons.ToList();
    }

    public List<Person> Search(string key)
    {
        var query = _context.Persons.AsQueryable();

        if (!string.IsNullOrWhiteSpace(key))
        {
            //search by single word
            var keywords = key.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in keywords)
            {
                query = query.Where(p =>
                    p.PersonName.Contains(word) ||
                    (p.Phone??"").Contains(word) ||
                    (p.Address??"").Contains(word));
            }
        }

        return query.ToList();
    }

    public void Update(Person person)
    {
        var p = _context.Persons.Find(person.PersonId);

        if (p != null)
        {
            p.PersonName = person.PersonName;
            p.Password = person.Password;
            p.Address = person.Address;
            p.Phone = person.Phone;
            p.Birthdate = person.Birthdate;
            p.Status = person.Status;

            _context.SaveChanges();
        }
    }
}
