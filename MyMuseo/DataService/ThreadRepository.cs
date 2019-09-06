using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;

namespace MyMuseo.DataService
{
	public class ThreadRepository
	{
		private readonly IDbConnection _db;

		public ThreadRepository()
		{
			_db = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
		}

		public List<Thread> GetAllThreads()
		{
			return this._db.Query<Thread>("SELECT * FROM [Threads] ORDER BY ThreadID DESC").ToList();
		}

        public List<Post> GetThreadPosts(int threadId)
        {
            return _db.Query<Post>("SELECT * FROM [Posts] WHERE ThreadId = @ThreadId", new { ThreadId = threadId }).ToList();
        }

        public List<Reply> GetPostReplies(int postId)
        {
            return _db.Query<Reply>("SELECT * FROM [Replies] WHERE PostId = @PostId", new { PostId = postId }).ToList();
        }

        public bool InsertThread(Thread ThreadModel)
		{
            int rowsAffected = this._db.Execute(@"INSERT Threads (
                                [ThreadTopic],
                                [ThreadText],
                                [ThreadTypeId],
                                [PostByCollectorId],
                                [ThreadImage],
                                [ThreadStartDate],
                                [ThreadEndDate],
                                [CreatedDate] ) 
                        values (
                                @ThreadTopic, 
                                @ThreadText, 
                                @ThreadTypeId, 
                                @PostByCollectorId, 
                                @ThreadImage, 
                                @ThreadStartDate,
                                @ThreadEndDate,  
                                @CreatedDate )",
				        ThreadModel);

			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

        public Thread GetThreadById(int threadId)
        {
            return _db.Query<Thread>("SELECT * FROM [Threads] WHERE ThreadId = @ThreadId", new { ThreadId = threadId }).Single();
        }

        public bool UpdateThread(Thread model)
        {
            int rowsAffected = this._db.Execute(
                        @"UPDATE [Threads] SET 
                            [ThreadTopic] = @ThreadTopic
                        WHERE ThreadId = @ThreadId" , model);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool DeleteThread(int ThreadId)
		{
			int rowsAffected = this._db.Execute(@"DELETE FROM [Threads] WHERE ThreadId = @ThreadId",
				new { ThreadId = ThreadId });

			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

        public bool InsertPost(Post PostModel)
        {
            int rowsAffected = this._db.Execute(@"INSERT Posts (
                                [PostTopic],
                                [PostText],
                                [ThreadId],
                                [PostByCollectorId],
                                [CreatedDate] ) 
                        values (
                                @PostTopic, 
                                @PostText, 
                                @ThreadId, 
                                @PostByCollectorId, 
                                @CreatedDate )",
                                PostModel);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool DeletePost(int PostId)
        {
            int rowsAffected = this._db.Execute(@"DELETE FROM [Posts] WHERE PostId = @PostId",
                new { PostId = PostId });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public Post GetPostById(int postId)
        {
            return _db.Query<Post>("SELECT * FROM [Posts] WHERE PostId = @PostId", new { PostId = postId }).Single();
        }

        public bool InsertReply(Reply ReplyModel)
        {
            int rowsAffected = this._db.Execute(@"INSERT Replies (
                                [ReplyTopic],
                                [ReplyText],
                                [PostId],
                                [ReplyByCollectorId],
                                [CreatedDate] ) 
                        values (
                                @ReplyTopic, 
                                @ReplyText, 
                                @PostId, 
                                @ReplyByCollectorId, 
                                @CreatedDate )",
                                ReplyModel);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool DeleteReply(int ReplyId)
        {
            int rowsAffected = this._db.Execute(@"DELETE FROM [Replies] WHERE ReplyId = @ReplyId",
                new { ReplyId = ReplyId });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public Reply GetReplyById(int replyId)
        {
            return _db.Query<Reply>("SELECT * FROM [Replies] WHERE ReplyId = @ReplyId", new { ReplyId = replyId }).Single();
        }

        public bool InsertThreadLike(ThreadLike model)
        {
            int rowsAffected = this._db.Execute(@"INSERT ThreadLikes (
                                [ThreadId],
                                [LikeByCollectorId],
                                [CreatedDate] ) 
                        values (
                                @ThreadId, 
                                @LikeByCollectorId, 
                                @CreatedDate )",
                                model);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public int GetThreadLikesCount(int threadId)
        {
            return this._db.ExecuteScalar<int>("SELECT COUNT(*) FROM ThreadLikes WHERE ThreadId=" + threadId);
        }

        public bool DeleteThreadLike(int id)
        {
            int rowsAffected = this._db.Execute(@"DELETE FROM [ThreadLikes] WHERE ReplyId = @ThreadLikeId",
                new { ThreadLikeId = id });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool InsertThreadResponse(ThreadResponse model)
        {
            int rowsAffected = this._db.Execute(@"INSERT ThreadResponses (
                                [ThreadResponseTypeId],
                                [ThreadId],
                                [ResponseByCollectorId],
                                [CreatedDate] ) 
                        values (
                                @ThreadResponseTypeId, 
                                @ThreadId, 
                                @ResponseByCollectorId, 
                                @CreatedDate )",
                                model);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public bool DeleteThreadResponse(int id)
        {
            int rowsAffected = this._db.Execute(@"DELETE FROM [ThreadResponses] WHERE ThreadResponseId = @ThreadResponseId",
                new { ThreadResponseId = id });

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }

        public int GetThreadInterestedCount(int threadId)
        {
            return this._db.ExecuteScalar<int>("SELECT COUNT(*) FROM ThreadResponses WHERE ThreadResponseTypeId = 1 AND ThreadId=" + threadId);
        }

        public int GetThreadGoingCount(int threadId)
        {
            return this._db.ExecuteScalar<int>("SELECT COUNT(*) FROM ThreadResponses WHERE ThreadResponseTypeId = 2 AND ThreadId=" + threadId);
        }

        public List<ThreadLike> GetThreadLikesForACollector(int collectorId)
        {
            return this._db.Query<ThreadLike>(@"SELECT * FROM [ThreadLikes] L
                                            JOIN Threads T
                                            ON T.ThreadId = L.ThreadId
                                            WHERE T.PostByCollectorId = @CollectorId 
                                            ORDER BY L.CreatedDate DESC", new {CollectorId = collectorId }).ToList();
        }

        public List<Post> GetThreadPostsForACollector(int collectorId)
        {
            return this._db.Query<Post>(@"SELECT P.PostId, P.PostTopic, P.PostText, P.ThreadId, P.PostByCollectorId FROM [Posts] P
                                            JOIN Threads T
                                            ON T.ThreadId = P.ThreadId
                                            WHERE T.PostByCollectorId = @CollectorId AND P.PostByCollectorId != @CollectorId
                                            ORDER BY P.CreatedDate DESC", new { CollectorId = collectorId }).ToList();
        }


    }
}