using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using MyMuseo.Models;


namespace MyMuseo.DataService
{
    public class FileDetailsRepository
    {
        private readonly IDbConnection _db;

        public FileDetailsRepository()
        {
            _db = new SqlConnection(ConfigurationManager.ConnectionStrings["MyMuseoDb"].ConnectionString);
        }

        public List<FileDetails> GetFileDetails(int collectorId)
        {
            return _db.Query<FileDetails>("SELECT * FROM [FileDetails] WHERE CollectorId = @CollectorId", new { CollectorId = collectorId }).ToList();
        }

        public FileDetails GetSingleFileDetails(int fileId)
        {
            return _db.Query<FileDetails>("SELECT * FROM [FileDetails] WHERE FileId = @FileId", new { FileId = fileId }).SingleOrDefault();
        }

        public bool InsertFileDetails(FileDetails fileDetailsModel)
        {
            int rowsAffected = this._db.Execute(@"INSERT FileDetails (
                                [FileName],
                                [FileContent],
                                [CollectorId],
                                [UploadedDate] ) 
                        values (
                                @FileName, 
                                @FileContent, 
                                @CollectorId, 
                                @UploadedDate )",
                        fileDetailsModel);

            if (rowsAffected > 0)
            {
                return true;
            }

            return false;
        }


    }
}
