using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF.Models;

namespace WPF.Service
{
    public interface IAuthenService
    {
        Person Login(string username, string password);
        Person GetUser(string username);
    }
}
