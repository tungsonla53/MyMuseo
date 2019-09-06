using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;


namespace MyMuseo.DataService
{
    public class CountryRepository
    {
        private readonly IDbConnection _db;

        public CountryRepository()
        {
            _db = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
        }

        public List<Country> GetAllCountries()
        {
            return this._db.Query<Country>("SELECT * FROM [Country]").ToList();
        }

    }
}
