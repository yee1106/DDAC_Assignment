using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models.APIs
{
    public class Email
    {
        static string email_api_root = "email/";

        public static async Task<string> send_email_confirmation_api(string user_email, string user_full_name, string confirmation_link)
        {
            var request_body = new Dictionary<string, string>
              {
                  { "email", user_email },
                  { "full_name", user_full_name },
                  { "confirmation_link", confirmation_link }
              };

            string url = email_api_root + "send_confirmation_email";
            string response = await ApiTemplate.post(request_body, url);

            return response;
        }
    }

}
