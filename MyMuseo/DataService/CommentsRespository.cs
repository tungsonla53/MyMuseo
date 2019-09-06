using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;

namespace MyMuseo.DataService
{
	public class CommentsRespository
	{
		private readonly IDbConnection _db;

		public CommentsRespository()
		{
			_db = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
		}

		public List<Comment> GetAllComments(int amount, string sort)
		{
			return this._db.Query<Comment>("SELECT * FROM [Comments] ORDER BY CommentID " + sort).ToList();
		}

        public List<Comment> GetCollectionComments(int collectionId)
        {
            return _db.Query<Comment>("SELECT * FROM [Comments] WHERE CollectionId =@CollectionId", new { CollectionId = collectionId }).ToList();
        }


        public List<Comment> GetCollectibleComments(int collectibleId)
        {
            return _db.Query<Comment>("SELECT * FROM [Comments] WHERE CollectibleId =@CollectibleId", new { CollectibleId = collectibleId }).ToList();
        }

        public List<Comment> GetCommentsForCollector(int collectorId)
        {
            return _db.Query<Comment>("SELECT * FROM [Comments] WHERE CollectibleId IN (SELECT CollectibleId FROM [Collectibles] WHERE CollectorId = @CollectorId) AND ParentId = 0 AND Coalesce(FlagAsAbuse, 0) = 0 ", new { CollectorId = collectorId }).ToList();
        }

        public List<Comment> GetCommentsRepliesForCollector(int collectorId)
        {
            return _db.Query<Comment>("SELECT * FROM [Comments] WHERE CollectibleId IN (SELECT CollectibleId FROM [Collectibles] WHERE CollectorId = @CollectorId) AND Coalesce(FlagAsAbuse, 0) = 0  ORDER BY CreatedDate DESC", new { CollectorId = collectorId }).ToList();
        }

        public Comment GetCommentById(int commentId)
        {
            return _db.Query<Comment>("SELECT * FROM [Comments] WHERE CommentId =@CommentId", new { CommentId = commentId }).SingleOrDefault();
        }

        public Comment GetReplies(int parentId)
		{
			return _db.Query<Comment>("SELECT * FROM [Comments] WHERE ParentId =@ParentId", new { ParentId = parentId }).SingleOrDefault();
		}

        public bool InsertComment(Comment CommentModel)
		{
            int rowsAffected = this._db.Execute(@"INSERT Comments (
                                [CommentText],
                                [CollectionId],
                                [CollectibleId],
                                [PostByCollectorId],
                                [ParentId],
                                [IsApproved],
                                [CreatedDate] ) 
                        values (
                                @CommentText, 
                                @CollectionId, 
                                @CollectibleId, 
                                @PostByCollectorId, 
                                @ParentId, 
                                @IsApproved, 
                                @CreatedDate )",
				        CommentModel);

			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

		public bool DeleteComment(int CommentId)
		{
			int rowsAffected = this._db.Execute(@"DELETE FROM [Comments] WHERE CommentId = @CommentId",
				new { CommentId = CommentId });

			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

		public bool UpdateComment(Comment CommentModel)
		{
			int rowsAffected = this._db.Execute(
                        @"UPDATE [Comments] SET 
                            [CommentText] = @CommentText,
                            [CollectionId] = @CollectionId, 
                            [PostByCollectorId] = @PostByCollectorId,
                            [IsApproved] = @IsApproved
                        WHERE CommentId = " +
						CommentModel.CommentId, CommentModel);
           
            
			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

        public bool FlagComment(int commentId)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Comments] SET 
                            [FlagAsAbuse] = @FlagAsAbuse
                        WHERE CommentId = " +
                        commentId, new { FlagAsAbuse = true});

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

    }
}