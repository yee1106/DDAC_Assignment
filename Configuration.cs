using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment
{
    public class Configuration
    {
        //Api Url
        public static string base_url = "https://xc50vkv0wa.execute-api.us-east-1.amazonaws.com/";
        public static string writeNewsDataApi_url = "https://40jdw173md.execute-api.us-east-1.amazonaws.com/refreshNewsTemplateAPI";
        public static string getNewsDataApi_url = "https://pauyre9e93.execute-api.us-east-1.amazonaws.com/getNewsTemplate";

        //bucket
        public static string bucketName = "ddacimagebucket";

        public class default_admin
        {
             public static string UserName = "ddac.enews@gmail.com";
             public static string Email = "ddac.enews@gmail.com";
            public static string password = "DDACadmin123@";
        }

    }
}
