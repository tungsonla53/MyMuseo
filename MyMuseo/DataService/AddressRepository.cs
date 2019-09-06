using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;

namespace MyMuseo.DataService
{
    public class AddressRepository
    {
        private readonly IDbConnection _db;

        public AddressRepository()
        {
            _db = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
        }

        public AddressInfo GetCollectorAddress(int collectorId)
        {
            return _db.Query<AddressInfo>("SELECT * FROM [Address] WHERE CollectorId =@CollectorId", new { CollectorId = collectorId }).SingleOrDefault();
        }

        public bool InsertAddress(AddressInfo objAddress)
        {
            int rowsAffected = this._db.Execute(@"INSERT Address ([Street],[Apt],[CountryId],
                    [City],[Region],[ZipPostalCode],[PhoneNumber],[FaxNumber],[CreatedDate],[UpdatedDate],[CollectorId] ) 
                    values (@Street, @Apt, @CountryId, 
                            @City, @Region, @ZipPostalCode, @PhoneNumber, @FaxNumber, @CreatedDate, @UpdatedDate, @CollectorId )",
                    objAddress
            );

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool UpdateAddress(AddressInfo objAddress)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Address] SET 
                            [Street] = @Street,
                            [Apt] = @Apt, 
                            [CountryId] = @CountryId,
                            [City] = @City,
                            [Region] = @Region, 
                            [PhoneNumber] = @PhoneNumber,
                            [UpdatedDate] = @UpdatedDate
                        WHERE CollectorId = " +
                        objAddress.CollectorId, objAddress);

            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

    }
}