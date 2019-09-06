using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;

namespace MyMuseo.DataService
{
	public class CollectionsRespository
	{
		private readonly IDbConnection _db;

		public CollectionsRespository()
		{
			_db = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
		}

        public List<Collection> GetAllCollectionsActive(int amount, string sort)
        {
            return this._db.Query<Collection>("SELECT * FROM [Collections] WHERE CollectorId NOT IN (SELECT CollectorId FROM Collectors WHERE DisplayOrder = -1) ORDER BY CollectionID " + sort).ToList();
        }

        public List<Collection> GetAllCollections(int amount, string sort)
		{
			return this._db.Query<Collection>("SELECT * FROM [Collections] ORDER BY CollectionID " + sort).ToList();
		}

        public List<Collection> GetCollections(int collectorId)
        {
            return _db.Query<Collection>("SELECT * FROM [Collections] WHERE CollectorId =@CollectorId", new { CollectorId = collectorId }).ToList();
        }

        public Collection GetCollection(int collectionId)
		{
			return _db.Query<Collection>("SELECT * FROM [Collections] WHERE CollectionId =@CollectionId", new { CollectionId = collectionId }).SingleOrDefault();
		}

        public int InsertCollection(Collection CollectionModel)
		{
			int id = this._db.Query<int>(@"INSERT Collections (
                                [CollectorId],
                                [ArtistId],
                                [CategoryId],
                                [ThumbImage],
                                [NormalImage],
                                [Name],
                                [Description],
                                [IsForSale],
                                [DisplayOrder],
                                [Price],
                                [CreatedDate],
                                [FeaturedItemId],
                                [IsDraft]) 
                        values (
                                @CollectorId, 
                                @ArtistId, 
                                @CategoryId, 
                                @ThumbImage, 
                                @NormalImage, 
                                @Name, 
                                @Description, 
                                @IsForSale,
                                @DisplayOrder, 
                                @Price, 
                                @CreatedDate, 
                                @FeaturedItemId,
                                @IsDraft); SELECT CAST(SCOPE_IDENTITY() as int)",
				        CollectionModel).Single();
            return id;
		}

		public bool DeleteCollection(int CollectionId)
		{
			int rowsAffected = this._db.Execute(@"DELETE FROM [Collections] WHERE CollectionID = @CollectionID",
				new { CollectionID = CollectionId });

			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

		public bool UpdateCollection(Collection CollectionModel)
		{
			int rowsAffected = this._db.Execute(
                        @"UPDATE [Collections] SET 
                            [CollectorId] = @CollectorId,
                            [ArtistId] = @ArtistId, 
                            [ThumbImage] = @ThumbImage,
                            [NormalImage] = @NormalImage,
                            [Name] = @Name, 
                            [Description] = @Description, 
                            [FeaturedItemId] = @FeaturedItemId,
                            [IsDraft] = @IsDraft,
                            [CreatedDate] = @CreatedDate
                        WHERE CollectionId = " +
						CollectionModel.CollectionId, CollectionModel);
           
            
			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

        public bool UpdateCollectionHeader(Collection CollectionModel)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collections] SET 
                            [Name] = @Name, 
                            [Description] = @Description, 
                            [IsDraft] = @IsDraft
                        WHERE CollectionId = " +
                        CollectionModel.CollectionId, CollectionModel);
            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

        public bool UpdateCollectionImage(int collectionId, string thumbImage, string normalImage)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collections] SET 
                            [NormalImage] = @NormalImage,
                            [ThumbImage] = @ThumbImage
                        WHERE CollectionId = " +
                        collectionId, new { ThumbImage = thumbImage, NormalImage = normalImage });

            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

        public bool UpdateCollectionFeaturedItemId(int collectionId, int featuredItemId)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Collections] SET 
                            [FeaturedItemId] = @FeaturedItemId
                        WHERE CollectionId = " +
                        collectionId, new { FeaturedItemId = featuredItemId });

            if (rowsAffected > 0)
            {
                return true;
            }
            return false;
        }

    }
}