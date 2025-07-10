using System.Net.Http;

namespace Enterprise.Services.Scim.Api.Helpers
{
	public class UriProvider : IUriProvider
	{
		public string GetAbsoluteUriWithVirtualPath(HttpRequestMessage requestMessage)
		{
			return requestMessage.RequestUri.AbsoluteUri;
		}
	}
}
