using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models
{
    public class GetApiModel
    {
        public string statusCode { get; set; }
        public List<NewsTemplate> body { get; set; }
    }
}
