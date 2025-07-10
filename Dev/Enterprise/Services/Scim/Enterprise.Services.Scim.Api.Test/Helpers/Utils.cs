using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Enterprise.Services.Scim.Tests.Helpers
{
	public static class Utils
	{
		public static string ReturnResponseFromResponseSteam(Stream stream)
		{
			string result = Encoding.UTF8.GetString(((MemoryStream)stream).ToArray());
			return result;
		}

		public static JToken ExtractJsonFromBody(string json)
		{
			return JsonConvert.DeserializeObject<JToken>(json);
		}
	}
}
