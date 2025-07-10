using Newtonsoft.Json.Linq;

namespace Enterprise.Services.Scim.Tests.Helpers
{
	public static class JTokenExtensions
	{
		public static string GetTokenAsString(this JToken jToken, string token)
		{
			return jToken.SelectToken(token).ToString();
		}
	}
}
