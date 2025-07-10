using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using CargoWise.Definitions;

namespace Enterprise.Services.ServiceHost
{
	public class GlowAuthenticationFailureResult : IHttpActionResult
	{
		public GlowAuthenticationFailureResult(HttpRequestMessage request, string cw1AuthResult = null, string glowAuthenticationResult = null)
		{
			Request = request;
			this.cw1AuthResult = cw1AuthResult;
			this.glowAuthenticationResult = glowAuthenticationResult;
		}

		public HttpRequestMessage Request { get; private set; }

		const string GlowAuthenticationResultHeaderName = "Glow-Authentication-Result";
		readonly string glowAuthenticationResult;
		readonly string cw1AuthResult;

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(Execute());
		}

		HttpResponseMessage Execute()
		{
			var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);

			if (glowAuthenticationResult != null)
			{
				response.Headers.Add(GlowAuthenticationResultHeaderName, glowAuthenticationResult);
			}

			if (cw1AuthResult != null)
			{
				response.Headers.Add(ApiProxyConstants.Cw1AuthenticationResultHeaderName, cw1AuthResult);
			}
			response.RequestMessage = Request;
			return response;
		}
	}
}
