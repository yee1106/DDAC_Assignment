using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment
{
    public class Configuration
    {
        //Api Url
        public static string base_url = "https://ybxr7veqs2.execute-api.us-east-1.amazonaws.com/";
        public static string writeNewsDataApi_url = "https://hgthey6w51.execute-api.us-east-1.amazonaws.com/writeNewsApi";
        public static string getNewsDataApi_url = "https://xj1uzxn50g.execute-api.us-east-1.amazonaws.com/readNewsApi";

        //bucket
        public static string bucketName = "ddacimagebucket";

        // AWS SNS Topic
        public static string topicArn = "arn:aws:sns:us-east-1:532715210025:roles_and_permissions_update";

        public class default_admin
        {
             public static string UserName = "ddac.enews@gmail.com";
             public static string Email = "ddac.enews@gmail.com";
             public static string password = "DDACadmin123@";
        }

    }
}
