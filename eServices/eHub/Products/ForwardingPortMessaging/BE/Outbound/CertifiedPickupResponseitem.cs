using System;
using System.Text;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.eHub.Products.ForwardingPortMessaging.BE.Outbound.Helpers
{
	[Serializable]
	public class CertifiedPickupResponseitem
	{
		[JsonProperty("publicReferenceId")]
		public string PublicReferenceId;

		[JsonProperty("externalReferenceId")]
		public string ExternalReferenceId;

		[JsonProperty("statusCode")]
		public string StatusCode;

		[JsonProperty("message")]
		public string Message;

		[JsonProperty("errors")]
		public CertifiedPickupErrorItem Errors;

		public static CertifiedPickupResponseitem LoadFromJsonString(string jsonString)
		{
			try
			{
				return JsonConvert.DeserializeObject<CertifiedPickupResponseitem>(jsonString);
			}
			catch (JsonSerializationException)
			{
				return JsonConvert.DeserializeObject<CertifiedPickupResponseitem>(@"{""statusCode"":500, ""message"": ""Error JsonFormat for response!""}");
			}
		}
	}
}
