using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF.Models;

namespace WPF.Service
{
    public class AuthenService : IAuthenService
    {
        public AuthenService()
        {

        }
        public Person Login(string username, string password)
        {
            using var _context = new MyContext();
            var user = _context.Persons.AsNoTracking()
                .FirstOrDefault(p => p.PersonName == username);

            if (user == null)
                return null!;

            if (user.Password != password)
                return null!;

            return user;
        }

        public Person GetUser(string username)
        {
            using var _context = new MyContext();
            return _context.Persons.AsNoTracking()
                .FirstOrDefault(p => p.PersonName == username)!;
        }
    }
}
