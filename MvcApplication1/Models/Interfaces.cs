using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcApplication1.Models
{
    public interface IUserRepository
    {
        User GetUserByName(string name);
        void Save(User user);
    }

    public interface ISmsSender
    {
         
    }
}
