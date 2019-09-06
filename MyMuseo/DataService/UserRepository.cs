using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;
using System.Threading.Tasks;


namespace MyMuseo.DataService
{
    public class UserRepository
    {
        private readonly IDbConnection connection;

        public UserRepository()
        {
            connection = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
        }

    }

    public interface IUserRepository : Microsoft.AspNet.Identity.IUserStore<User>, Microsoft.AspNet.Identity.IUserLoginStore<User>, Microsoft.AspNet.Identity.IUserPasswordStore<User>, Microsoft.AspNet.Identity.IUserSecurityStampStore<User>
    {
    }
}
