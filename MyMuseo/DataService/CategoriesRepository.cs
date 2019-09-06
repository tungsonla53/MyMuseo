using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;

namespace MyMuseo.DataService
{
	public class CategoriesRespository
	{
		private readonly IDbConnection _db;

		public CategoriesRespository()
		{
			_db = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
		}

		public List<Category> GetAllCategories()
		{
			return this._db.Query<Category>("SELECT * FROM [Category] WHERE ParentCategoryId = 0 ORDER BY Name").ToList();
		}

        public List<Category> GetSubCategories(int parentId)
        {
            return this._db.Query<Category>("SELECT * FROM [Category] WHERE ParentCategoryId = @ParentCategoryId", new { ParentCategoryId = parentId }).ToList();
        }

        public Category GetCategory(int categoryId)
		{
			return _db.Query<Category>("SELECT * FROM [Category] WHERE CategoryId = @CategoryId", new { CategoryId = categoryId }).SingleOrDefault();
		}

        public int InsertCategory(Category model)
		{
            int id = this._db.Query < int >(@"INSERT Category (
                                [Name],
                                [Description],
                                [Alias],
                                [ParentCategoryId],
                                [CreatedOnUtc] ) 
                        values (
                                @Name, 
                                @Description, 
                                @Alias, 
                                @ParentCategoryId, 
                                @CreatedOnUtc ); SELECT CAST(SCOPE_IDENTITY() as int)", 
				        model).Single(); 
			return id;
		}

		public bool DeleteCategory(int categoryId)
		{
			int rowsAffected = this._db.Execute(@"DELETE FROM [Category] WHERE CategoryId = @CategoryId",
				new { CategoryId = categoryId });

			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}

		public bool UpdateCategory(Category model)
		{
			int rowsAffected = this._db.Execute(
                        @"UPDATE [Category] SET 
                            [Name] = @Name,
                            [Description] = @Description, 
                            [Alias] = @Alias,
                            [ParentCategoryId] = @ParentCategoryId 
                        WHERE CategoryId = " +
						model.CategoryId, model);

			if (rowsAffected > 0)
			{
				return true;
			}

			return false;
		}
	}
}