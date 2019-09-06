using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;

namespace MyMuseo.DataService
{
	public class DiscussionsRespository
	{
		private readonly IDbConnection _db;

		public DiscussionsRespository()
		{
			_db = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
		}

		public List<Discussion> GetAllDiscussions(int amount, string sort)
		{
			return this._db.Query<Discussion>("SELECT * FROM [Discussions] ORDER BY DiscussionID " + sort).ToList();
		}

        public List<Discussion> GetGroupDiscussions(int groupId)
        {
            return _db.Query<Discussion>("SELECT * FROM [Discussions] WHERE GroupId =@GroupId", new { GroupId = groupId }).ToList();
        }

        public Discussion GetDiscussion(int discussionId)
        {
            return _db.Query<Discussion>("SELECT * FROM [Discussions] WHERE DiscussionId =@DiscussionId", new { DiscussionId = discussionId }).SingleOrDefault();
        }

        public List<Discussion> GetDiscussionReplies(int discussionId)
        {
            return _db.Query<Discussion>("SELECT * FROM [Discussions] WHERE ParentId = @ParentId", new { ParentId = discussionId }).ToList();
        }

        public bool InsertDiscussion(Discussion DiscussionModel)
		{
            int rowsAffected = this._db.Execute(@"INSERT Discussions (
                                [DiscussionTopic],
                                [DiscussionText],
                                [GroupId],
                                [PostByCollectorId],
                                [ParentId],
                                [IsApproved],
                                [CreatedDate] ) 
                        values (
                                @DiscussionTopic, 
                                @DiscussionText, 
                                @GroupId, 
                                @PostByCollectorId, 
                                @ParentId, 
                                @IsApproved, 
                                @CreatedDate )",
				        DiscussionModel);

			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

		public bool DeleteDiscussion(int DiscussionId)
		{
			int rowsAffected = this._db.Execute(@"DELETE FROM [Discussions] WHERE DiscussionId = @DiscussionId",
				new { DiscussionId = DiscussionId });

			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

    }
}