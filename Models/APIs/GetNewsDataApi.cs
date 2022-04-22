using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models.APIs
{
    public class GetNewsDataApi
    {
        public static List<NewsTemplate> Get()
        {
            Uri getNewsDataApi_url = new Uri(Configuration.getNewsDataApi_url);
            HttpClient clientReturn = new HttpClient();
            GetApiModel getApiModel = new GetApiModel();

            clientReturn.BaseAddress = getNewsDataApi_url;
            HttpResponseMessage responseReturn = clientReturn.GetAsync(clientReturn.BaseAddress).Result;

            if (responseReturn.IsSuccessStatusCode)
            {
                string data = responseReturn.Content.ReadAsStringAsync().Result;
                getApiModel = JsonConvert.DeserializeObject<GetApiModel>(data);
                if (getApiModel != null)
                {
                    return getApiModel.body;
                }
            }
            return new List<NewsTemplate>();
        }
    }
}
