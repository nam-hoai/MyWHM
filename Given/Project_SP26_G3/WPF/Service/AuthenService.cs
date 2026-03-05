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
        private readonly MyContext _context;
        public AuthenService()
        {
            _context = new();
        }
        public Person Login(string username, string password)
        {
            var user = _context.Persons
                .FirstOrDefault(p => p.PersonName == username);

            if (user == null)
                return null;

            if (user.Password != password)
                return null;

            return user;
        }

        public Person GetUser(string username)
        {
            return _context.Persons.FirstOrDefault(p => p.PersonName == username);
        }
    }
}
