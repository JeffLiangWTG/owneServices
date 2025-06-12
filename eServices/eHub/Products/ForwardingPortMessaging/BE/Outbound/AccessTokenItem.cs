using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers
{

    [Serializable]
    public class AccessTokenItem
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType {get; set;}

        [JsonProperty("scope")]
        public string scope { get; set; }

        [JsonProperty("error")]
        public string error { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

        public static AccessTokenItem LoadFromJsonString(string jsonString)
        {
            return JsonConvert.DeserializeObject<AccessTokenItem>(jsonString);         
        }			
    }
}
