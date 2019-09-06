using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;


namespace MyMuseo.DataService
{
    public class ShoppingCartRepository
    {
        private readonly IDbConnection _db;

        public ShoppingCartRepository()
        {
            _db = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
        }

        public List<Cart> Carts()
        {
            return this._db.Query<Cart>("SELECT * FROM [Cart]").ToList();
        }

        public bool Add(Cart model)
        {
            int rowsAffected = this._db.Execute(@"INSERT Cart (
                                [CartId],
                                [CollectibleId],
                                [Count],
                                [Title],
                                [Image],
                                [Price],
                                [CreatedDate] ) 
                        values (
                                @CartId, 
                                @CollectibleId, 
                                @Count, 
                                @Title, 
                                @Image,
                                @Price, 
                                @CreatedDate )",
                        model);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool Update(Cart model)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Cart] SET 
                            [CartId] = @CartId,
                            [CollectibleId] = @CollectibleId, 
                            [Count] = @Count,
                            [Title] = @Title,
                            [Image] = @Image,
                            [Price] = @Price
                        WHERE RecordId = " +
                        model.RecordId, model);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool Remove(Cart model)
        {
            int rowsAffected = this._db.Execute(
                        @"DELETE [Cart] 
                        WHERE RecordId = " +
                        model.RecordId, model);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

    }
}
