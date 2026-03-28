using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using WPF.Models;
using WPF.Service;
public class AccountService : IAccountService
{
    public AccountService(){}
    public void Add(Person person)
    {
        using var _context = new MyContext();
        //check duplication
        if (_context.Persons.Any(p => p.PersonName.Equals(person.PersonName)))
            throw new Exception($"{person.PersonName} is duplicated, try with another");

        var existing = _context.Persons.FirstOrDefault(p => p.PersonId == person.PersonId);

        if (existing!=null)
            throw new Exception($"Person ID: {person.PersonId} is used for {existing.PersonName.ToString()}, try with another");
        //Add
        _context.Persons.Add(person);

        _context.SaveChanges();
    }

    public void Delete(Person person)
    {
        using var _context = new MyContext();

        var p = GetPerson(person.PersonId);

        if (p != null)
        {
            _context.Persons.Remove(p);

            _context.SaveChanges();
        }
    }
    public List<Person> GetAll()
    {
        using var _context = new MyContext();

        return _context.Persons.AsNoTracking().Where(p => p.RoleId == 2).ToList();
    }
    public Person GetPerson(int id)
    {
        return FindPerson(id)??throw new Exception($"No found person with ID = {id}");
    }
    private Person? FindPerson(int id)
    {
        using var context = new MyContext();

        return context.Persons.AsNoTracking().FirstOrDefault(p => p.PersonId == id);
    }
    public List<Person> Search(string key)
    {
        using var _context = new MyContext();

        var query = _context.Persons.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(key))
        {
            //search by single word
            var keywords = key.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var word in keywords)
            {
                query = query.Where(p =>
                    p.PersonName.Contains(word) ||
                    (p.Phone ?? "").Contains(word) ||
                    (p.Address ?? "").Contains(word) ||
                    p.Status.ToString().Equals(word));
            }
        }
        return query.ToList();
    }

    public void Update(Person person)
    {
        using var context = new MyContext();
        using var trans = context.Database.BeginTransaction();

        try
        {
            // Kiểm tra trùng lặp tên (trừ chính nó)
            bool isDuplicate = context.Persons.Any(x => x.PersonName == person.PersonName && x.PersonId != person.PersonId);

            if (isDuplicate) throw new Exception("PersonName already exists");

            var existing = context.Persons.FirstOrDefault(x => x.PersonId == person.PersonId);

            if (existing == null) throw new Exception("No found person!");
            // Dùng EF Core để cập nhật thay vì Raw SQL
            context.Entry(existing).CurrentValues.SetValues(person);

            context.SaveChanges();

            trans.Commit();
        }
        catch (Exception)
        {
            trans.Rollback();
            throw;
        }
    }
}
