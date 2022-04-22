using DDAC_Assignment.Models.APIs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models
{
    public class AdvertisementDatabase
    {

        public static List<NewsTemplate> Initialize()
        {
            /*string refreshURL = "https://40jdw173md.execute-api.us-east-1.amazonaws.com/refreshNewsTemplateAPI";
            Uri urlForReturn = new Uri("https://pauyre9e93.execute-api.us-east-1.amazonaws.com/getNewsTemplate");
            HttpClient clientRefresh = new HttpClient();
            HttpClient clientReturn = new HttpClient();
            GetApiModel getApiModel = new GetApiModel();
            //HttpResponseMessage responseRefresh = clientRefresh.GetAsync(clientRefresh.BaseAddress).Result;
            GetApiModel postApiModel = new GetApiModel();

            clientReturn.BaseAddress = urlForReturn;
            HttpResponseMessage responseRefresh1 = clientRefresh.PostAsync(refreshURL, new StringContent(JsonConvert.SerializeObject(postApiModel), Encoding.UTF8, "application/json")).Result;

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
            return new List<NewsTemplate>();*/

            string writeNewsDataApi_url = Configuration.writeNewsDataApi_url;
            HttpClient clientRefresh = new HttpClient();
            GetApiModel postApiModel = new GetApiModel();
            HttpResponseMessage responseRefresh = clientRefresh.PostAsync(writeNewsDataApi_url, new StringContent(JsonConvert.SerializeObject(postApiModel), Encoding.UTF8, "application/json")).Result;
            if (responseRefresh.IsSuccessStatusCode)
            {
                return GetNewsDataApi.Get();
            }
            return new List<NewsTemplate>(); 
        }
    }
}
