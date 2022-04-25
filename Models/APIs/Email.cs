using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models.APIs
{
    public class Email
    {
        static string email_api_root = "email/";

        public static async Task<string> email_api_template(Dictionary<string, string> request_body, string api_name)
        {
            string url = email_api_root + api_name;
            string response = await ApiTemplate.post(request_body, url);
            return response;
        }
        public static async Task<string> send_email_confirmation_api(string user_email, string user_full_name, string confirmation_link)
        {
            var request_body = new Dictionary<string, string>
              {
                  { "email", user_email },
                  { "full_name", user_full_name },
                  { "confirmation_link", confirmation_link }
              };

            var response = await email_api_template(request_body, "send_confirmation_email");

            return response;
        }

        public static async Task<string> approve_staff_registration_api(string user_email, string user_full_name, string confirmation_link)
        { 
            var request_body = new Dictionary<string, string>
              {
                  { "user_email", user_email },
                  { "user_full_name", user_full_name },
                  { "confirmation_link", confirmation_link }
              };

            var response = await email_api_template(request_body, "approve_staff_registration");

            return response;
        }

        public static async Task<string> notify_registration_success(string user_email, string user_full_name)
        {
            var request_body = new Dictionary<string, string>
              {
                  { "email", user_email },
                  { "full_name", user_full_name },
                  { "login_url", "https://localhost:44380/Identity/Account/Login?ReturnUrl=%2F"},
              };

            var response = await email_api_template(request_body, "notify_registration_success");

            return response;
        }

        public static async Task<string> send_password_reset_link_api(string user_email, string user_full_name, string reset_link)
        {
            var request_body = new Dictionary<string, string>
              {
                  { "email", user_email },
                  { "full_name", user_full_name },
                  { "reset_link", reset_link }
              };

            var response = await email_api_template(request_body, "send_password_reset_link");

            return response;
        }

        public static async Task<string> register_and_send_password_reset_link_api(string user_email, string user_full_name, string reset_link, string default_password)
        {
            var request_body = new Dictionary<string, string>
              {
                  { "email", user_email },
                  { "full_name", user_full_name },
                  { "reset_link", reset_link },
                  { "password", default_password }
              };

            var response = await email_api_template(request_body, "register_success_reset_password");

            return response;
        }

    }

}
