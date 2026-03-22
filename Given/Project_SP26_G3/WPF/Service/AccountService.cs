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
    private readonly MyContext _context;
    public AccountService()
    {
        _context = new MyContext();
    }
    public void Add(Person person)
    {
        //check duplication
        if (_context.Persons.Any(p => p.PersonName.Equals(person.PersonName)))
        {
            throw new Exception($"{person.PersonName} is duplicated, try with another");
        }
        if (_context.Persons.Any(p => p.PersonId == person.PersonId))
        {
            Person p = _context.Persons.First(p => p.PersonId == person.PersonId);
            throw new Exception($"Person ID: {person.PersonId} is used for {p.PersonName.ToString()}, try with another");
        }
        _context.Persons.Add(person);
        _context.SaveChanges();
    }

    public void Delete(Person person)
    {
        var p = GetPerson(person.PersonId);

        if (p != null)
        {
            _context.Persons.Remove(p);
            _context.SaveChanges();
        }
    }
    public List<Person> GetAll()
    {
        return _context.Persons.Where(p => p.RoleId == 2).ToList();
    }
    public Person GetPerson(int id)
    {
        return _context.Persons.FirstOrDefault(p => p.PersonId == id)
            ??throw new Exception($"No found person with ID = {id}");
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
                    (p.Phone ?? "").Contains(word) ||
                    (p.Address ?? "").Contains(word) ||
                    p.Status.ToString().Equals(word));
            }
        }

        return query.ToList();
    }

    public void Update(Person person)
    {
        using var trans = _context.Database.BeginTransaction();
        try
        {
            //Find Person
            var existing = _context.Persons.FirstOrDefault(x => x.PersonId == person.PersonId);
            if (existing == null)
            {
                throw new Exception("No found person!");
            }
            //detach inorder to EF do not track key
            _context.Entry(existing).State = EntityState.Detached;
            //check duplication
            bool isDuplicate = _context.Persons
            .Any(x => x.PersonName == person.PersonName
                   && x.PersonId != person.PersonId);

            if (isDuplicate)
                throw new Exception("PersonName already exists"); 
            // Update Person
            _context.Database.ExecuteSqlInterpolated($@"
            UPDATE Persons
            SET PersonName = {person.PersonName},
                Password = {person.Password},
                Address = {person.Address},
                Phone = {person.Phone},
                Birthdate = {person.Birthdate},
                Status = {person.Status},
                RoleId = {person.RoleId}
            WHERE PersonId = {person.PersonId}
        ");

            trans.Commit();
        }
        catch (Exception)
        {
            trans.Rollback();
            throw;
        }
    }
}
