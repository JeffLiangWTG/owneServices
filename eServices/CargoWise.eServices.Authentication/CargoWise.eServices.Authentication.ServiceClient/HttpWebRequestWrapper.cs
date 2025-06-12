using System.Net;

namespace CargoWise.eServices.Authentication.ServiceClient
{
	internal class HttpWebRequestWrapper : IRequest
	{
		private readonly HttpWebRequest _httpWebRequest;

		internal HttpWebRequestWrapper(HttpWebRequest request) 
		{
			_httpWebRequest = request;
		}

		public WebResponse GetResponse()
		{
			return _httpWebRequest.GetResponse();
		}
	}
}
