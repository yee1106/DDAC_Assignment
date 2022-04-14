using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models
{
    public class NewsTemplate
    {
        public string MessageID { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public string Actor { get; set; }

        public string PublishedDate { get; set; }

        public string LastUpdatedDate { get; set; }

        public string ImageDesciption { get; set; }

        public string Category { get; set; }

        public string ParentCategory { get; set; }
    }
}
