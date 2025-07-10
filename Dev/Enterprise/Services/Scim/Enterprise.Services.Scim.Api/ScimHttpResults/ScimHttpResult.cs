using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using SimpleIdServer.Scim;

namespace Enterprise.Services.Scim.Api.ScimHttpResults
{
	public class ScimHttpResult : IHttpActionResult
	{
		readonly JObject content;
		readonly HttpStatusCode status;
		readonly string location;
		readonly int version;

		public ScimHttpResult(JObject content, string location, int version, HttpStatusCode status)
		{
			this.content = content;
			this.location = location;
			this.version = version;
			this.status = status;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Http header")]
		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			var response = new HttpResponseMessage()
			{
				StatusCode = status,
				Content = new StringContent(content.ToString())
			};

			response.Content.Headers.TryAddWithoutValidation("Location", location);
			response.Content.Headers.TryAddWithoutValidation("ETag", version.ToString());
			response.Content.Headers.ContentType = new MediaTypeHeaderValue(SCIMConstants.STANDARD_SCIM_CONTENT_TYPE);
			return Task.FromResult(response);
		}
	}
}
