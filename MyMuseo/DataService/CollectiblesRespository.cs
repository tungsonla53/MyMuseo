using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;

namespace MyMuseo.DataService
{
	public class CollectiblesRespository
	{
		private readonly IDbConnection _db;

		public CollectiblesRespository()
		{
			_db = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
		}

        public List<Collectible> SearchCollectibles(string searchText)
        {
            string[] searchList = searchText.Split(' ');
            string searchTitle = "";
            string searchArtist = "";
            string searchMedium = "";
            string searchConcat = "";
            
            for (int sIndex = 0; sIndex < searchList.Length; sIndex++)
            {
                if (sIndex == 0)
                {
                    searchConcat = "CONCAT([Collectibles].[Title], [Collectors].[FirstName], [Collectors].[LastName],[Collectibles].[ArtistName], [Collectibles].[Medium], [Category].[Name]) LIKE '%" + searchList[sIndex] + "%'";
                }
                else
                {
                    searchConcat += " AND CONCAT([Collectibles].[Title], [Collectors].[FirstName], [Collectors].[LastName],[Collectibles].[ArtistName], [Collectibles].[Medium], [Category].[Name]) LIKE '%" + searchList[sIndex] + "%'";
                }
            }

            for (int sIndex = 0; sIndex < searchList.Length; sIndex++)
            {
                if (sIndex == 0)
                {
                    searchTitle = "(Title LIKE '%" + searchList[sIndex] + "%'";
                }
                else
                {
                    searchTitle += " OR Title LIKE '%" + searchList[sIndex] + "%'";
                }
            }
            searchTitle += ") ";

            for (int sIndex = 0; sIndex < searchList.Length; sIndex++)
            {
                if (sIndex == 0)
                {
                    searchArtist = "OR (ArtistName  LIKE '%" + searchList[sIndex] + "%'";
                }
                else
                {
                    searchArtist += " OR ArtistName LIKE '%" + searchList[sIndex] + "%'";
                }
                
            }
            searchArtist += ") ";
            for (int sIndex = 0; sIndex < searchList.Length; sIndex++)
            {
                if (sIndex == 0)
                {
                    searchArtist = "OR (Medium  LIKE '%" + searchList[sIndex] + "%'";
                }
                else
                {
                    searchMedium += " OR Medium LIKE '%" + searchList[sIndex] + "%'";
                }

            }
            searchMedium += ") ";

            //return this._db.Query<Collectible>("SELECT * FROM [Collectibles] WHERE " + searchTitle + searchArtist + searchMedium).ToList();
            return this._db.Query<Collectible>("SELECT * FROM [Collectibles] JOIN [Collectors] ON Collectors.CollectorId = Collectibles.CollectorId JOIN [Category] ON Collectibles.CategoryId = Category.CategoryId WHERE " + searchConcat).ToList();

        }

        public List<Collectible> GetAllCollectibles(int amount, string sort)
		{
			return this._db.Query<Collectible>("SELECT * FROM [Collectibles] ORDER BY CollectibleID " + sort).ToList();
		}

        public List<Collectible> GetCollectiblesByCategory(int categoryId, string sort)
        {
            return this._db.Query<Collectible>("SELECT * FROM [Collectibles] WHERE CategoryId = @CategoryId ORDER BY " + sort, new { CategoryId = categoryId }).ToList();
        }

        public List<Collectible> GetCollectibles(int collectorId)
        {
            return _db.Query<Collectible>("SELECT * FROM [Collectibles] WHERE CollectorId =@CollectorId ORDER BY CreatedDate DESC", new { CollectorId = collectorId }).ToList();
        }

        public List<Collectible> GetCollectiblesByDisplayOrder(int collectorId)
        {
            return _db.Query<Collectible>("SELECT * FROM [Collectibles] WHERE CollectorId =@CollectorId ORDER BY DisplayOrder", new { CollectorId = collectorId }).ToList();
        }

        public List<Collectible> GetAvailableCollectibles(int collectorId)
        {
            return _db.Query<Collectible>("SELECT * FROM [Collectibles] WHERE (CollectorId =@CollectorId AND CollectionId = 0) ORDER BY CreatedDate DESC", new { CollectorId = collectorId }).ToList();
        }

        public List<Collectible> GetFavorites(int collectorId)
        {
            return _db.Query<Collectible>("SELECT * FROM [Collectibles] WHERE CollectorId =@CollectorId", new { CollectorId = collectorId }).ToList();
        }

        public List<Collectible> GetCollectiblesOfCollection(int collectionId)
        {
            return _db.Query<Collectible>("SELECT * FROM [Collectibles] WHERE CollectionId =@CollectionId", new { CollectionId = collectionId }).ToList();
        }

        public Collectible GetCollectible(int collectibleId)
		{
			return _db.Query<Collectible>("SELECT * FROM [Collectibles] WHERE CollectibleId =@CollectibleId", new { CollectibleId = collectibleId }).SingleOrDefault();
		}

        public int InsertCollectible(Collectible collectibleModel)
		{
            int id = this._db.Query<int>(@"INSERT Collectibles (
                                [CollectorId],
                                [CollectionId],
                                [ArtistId],
                                [CategoryId],
                                [ThumbImage],
                                [NormalImage],
                                [OriginalImage],
                                [Title],
                                [Description],                              
                                [NotForSale],
                                [AllowOffers],
                                [DisplayOrder],
                                [Price],
                                [OfferPrice],
                                [ArtistName],
                                [Width],
                                [Height],
                                [Weight],
                                [Medium],
                                [ItemDate],
                                [CreatedDate],
                                [AudioFile]
                                ) 
                        values (
                                @CollectorId, 
                                @CollectionId, 
                                @ArtistId, 
                                @CategoryId, 
                                @ThumbImage, 
                                @NormalImage, 
                                @OriginalImage, 
                                @Title,
                                @Description,                            
                                @NotForSale, 
                                @AllowOffers,
                                @DisplayOrder, 
                                @Price, 
                                @OfferPrice, 
                                @ArtistName, 
                                @Width,
                                @Height, 
                                @Weight, 
                                @Medium, 
                                @ItemDate, 
                                @CreatedDate, @AudioFile ); SELECT CAST(SCOPE_IDENTITY() as int)",
				        collectibleModel).Single();
			return id;
		}

		public bool DeleteCollectible(int collectibleId)
		{
			int rowsAffected = this._db.Execute(@"DELETE FROM [Collectibles] WHERE CollectibleID = @CollectibleID",
				new { CollectibleID = collectibleId });

			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

		public bool UpdateCollectible(Collectible collectibleModel)
		{
			int rowsAffected = this._db.Execute(
                        @"UPDATE [Collectibles] SET 
                            [CollectionId] = @CollectionId,
                            [CategoryId] = @CategoryId,
                            [ArtistId] = @ArtistId,
                            [ArtistName] = @ArtistName, 
                            [Title] = @Title,
                            [Description] = @Description, 
                            [NotForSale] = @NotForSale,
                            [AllowOffers] = @AllowOffers,
                            [Price] = @Price, 
                            [OfferPrice] = @OfferPrice, 
                            [Width] = @Width,
                            [Height] = @Height, 
                            [Weight] = @Weight, 
                            [Medium] = @Medium, 
                            [ItemDate] = @ItemDate
                          
                        WHERE CollectibleId = " +
						collectibleModel.CollectibleId, collectibleModel);
           
            
			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

        public bool UpdateCollectibleImage(int collectibleId, string thumbImage, string originalImage)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collectibles] SET 
                            [ThumbImage] = @ThumbImage,
                            [NormalImage] = @NormalImage,
                            [OriginalImage] = @OriginalImage
                        WHERE CollectibleId = " +
                        collectibleId, new { ThumbImage = thumbImage, NormalImage = originalImage, OriginalImage = originalImage });


            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool DetachItemsFromCollection (int collectionId)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collectibles] SET 
                            [CollectionId] = 0
                        WHERE CollectionId = " +
                        collectionId, null);
            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool UpdateDisplayOrder(int collectibleId, int displayOrder)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collectibles] SET 
                            [DisplayOrder] = @DisplayOrder
                        WHERE CollectibleId = '" + collectibleId + "'"
                        , new { DisplayOrder = displayOrder });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

    }
}