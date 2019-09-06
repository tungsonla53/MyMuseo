using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMuseo.Models
{
    public class FileDetails
    {
        public int FileId { get; set; }
        public String FileName { get; set; }
        public byte[] FileContent { get; set; }
        public int CollectorId { get; set; }
        public DateTime UploadedDate { get; set; }
    }
}