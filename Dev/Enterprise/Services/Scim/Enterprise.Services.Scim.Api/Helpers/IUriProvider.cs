using System.Net.Http;
namespace Enterprise.Services.Scim.Api.Helpers
{
	public interface IUriProvider
	{
		string GetAbsoluteUriWithVirtualPath(HttpRequestMessage request);
	}
}
