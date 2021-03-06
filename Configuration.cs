using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment
{
    public class Configuration
    {
        //Api Url
        public static string base_url = "https://v4nwtjbpm6.execute-api.us-east-1.amazonaws.com/";
        // public static string writeNewsDataApi_url = "https://f4zuma6ylf.execute-api.us-east-1.amazonaws.com/writeNewsApi/";
        // public static string getNewsDataApi_url = "https://i5tru1prm1.execute-api.us-east-1.amazonaws.com/readNewsApi/";
        public static string writeNewsDataApi_url = "https://hgthey6w51.execute-api.us-east-1.amazonaws.com/writeNewsApi";
        public static string getNewsDataApi_url = "https://xj1uzxn50g.execute-api.us-east-1.amazonaws.com/readNewsApi";

        //s3 bucket
        public static string bucketName = "ddacimagebucket1";


        //AWS SQS
        public static string SQSQueue = "DDACUpdateCategoryQueue";

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
