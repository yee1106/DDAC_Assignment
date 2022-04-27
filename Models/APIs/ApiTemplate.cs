using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static DDAC_Assignment.Configuration;
using System.Text.Json;
using Newtonsoft.Json;
using System;
using System.Net;
using System.IO;
using System.Text;


namespace DDAC_Assignment.Models.APIs
{
    public class ApiTemplate
    {

        private static readonly HttpClient client = new HttpClient();

        private static string base_url = Configuration.base_url;

        public static async Task<string> post(Dictionary<string, string> request_body, string url)
        {
            var api_url = base_url + url;

            var content = JsonConvert.SerializeObject(request_body).ToString();

            var request = new StringContent(content, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(api_url, request);

            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }
        
        

    }
}
