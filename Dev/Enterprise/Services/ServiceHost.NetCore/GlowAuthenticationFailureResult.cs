using System.Net;
using System.Threading.Tasks;
using CargoWise.Definitions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Enterprise.Services.ServiceHost.NetCore
{
	public class GlowAuthenticationFailureResult : IActionResult
	{
		public GlowAuthenticationFailureResult(HttpRequest request, string cw1AuthResult = null, string glowAuthenticationResult = null)
		{
			Request = request;
			this.cw1AuthResult = cw1AuthResult;
			this.glowAuthenticationResult = glowAuthenticationResult;
		}

		public HttpRequest Request { get; private set; }

		const string GlowAuthenticationResultHeaderName = "Glow-Authentication-Result";
		readonly string glowAuthenticationResult;
		readonly string cw1AuthResult;

		public Task ExecuteResultAsync(ActionContext context)
		{
			var response = context.HttpContext.Response;
			response.StatusCode = (int)HttpStatusCode.Unauthorized;

			if (glowAuthenticationResult != null)
			{
				response.Headers.Append(GlowAuthenticationResultHeaderName, glowAuthenticationResult);
			}

			if (cw1AuthResult != null)
			{
				response.Headers.Append(ApiProxyConstants.Cw1AuthenticationResultHeaderName, cw1AuthResult);
			}

			return Task.CompletedTask;
		}
	}
}
