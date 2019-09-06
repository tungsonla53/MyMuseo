using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;


namespace MyMuseo.DataService
{
    public class BannersRepository
    {
        private readonly IDbConnection _db;

        public BannersRepository()
        {
            _db = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
        }

        public List<Banner> GetAllBanners()
        {
            return this._db.Query<Banner>("SELECT * FROM [Banners] ORDER BY DisplayOrder").ToList();
        }

        public Banner GetBanner(int bannerId)
        {
            return this._db.Query<Banner>("SELECT * FROM [Banners] WHERE BannerId =" + bannerId).SingleOrDefault();
        }

        public bool UpdateBannerOrder(int bannerId, int displayOrder)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Banners] SET 
                            [DisplayOrder] = @DisplayOrder
                        WHERE BannerId = '" + bannerId + "'"
                        , new { DisplayOrder = displayOrder });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public int InsertBanner(Banner bannerModel)
        {
            int id = this._db.Query<int>(@"INSERT Banners (
                                [Title],
                                [LinkTo],
                                [Image],
                                [ImageMobile],
                                [DisplayOrder],
                                [CreatedOnUtc],
                                [UpdatedOnUtc] ) 
                        values (
                                @Title, 
                                @LinkTo, 
                                @Image, 
                                @ImageMobile, 
                                @DisplayOrder,
                                @CreatedOnUtc, 
                                @UpdatedOnUtc ); SELECT CAST(SCOPE_IDENTITY() as int)",
                        bannerModel).Single();
            return id;
        }

        public bool DeleteBanner(int bannerId)
        {
            int rowsAffected = this._db.Execute(@"DELETE FROM [Banners] WHERE BannerId = @BannerId",
                new { BannerId = bannerId });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool UpdateBanner(Banner bannerModel)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Banners] SET 
                            [Title] = @Title,
                            [LinkTo] = @LinkTo,
                            [Image] = @Image,
                            [ImageMobile] = @ImageMobile,
                            [UpdatedOnUtc] = @UpdatedOnUtc
                        WHERE BannerId = " +
                        bannerModel.BannerId, bannerModel);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }
    }
}
