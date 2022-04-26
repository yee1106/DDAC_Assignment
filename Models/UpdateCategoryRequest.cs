using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models
{
    public class UpdateCategoryRequest
    {
        public int CategoryID { get; set; }

        public string CategoryName { get; set; }

        public string ParentCategoryName { get; set; }

        public string Description { get; set; }

        public string RequestType { get; set; }

        public string StaffName { get; set; }

        public DateTime RequestTime { get; set; }
    }
}
