using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using SimpleIdServer.Scim.DTOs;
using SimpleIdServer.Scim.Serialization;

namespace Enterprise.Services.Scim.Api.ScimHttpResults
{
	public class ScimErrorResult : IHttpActionResult
	{
		readonly string detail;
		readonly string scimType;
		readonly HttpStatusCode status;

		public ScimErrorResult(string detail, HttpStatusCode status, string scimType)
		{
			this.detail = detail;
			this.status = status;
			this.scimType = scimType;
		}

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			var serializer = new SCIMSerializer();
			var result = serializer.Serialize(new SCIMErrorRepresentation(((int)status).ToString(), detail, scimType));

			var response = new HttpResponseMessage()
			{
				StatusCode = status,
				Content = new StringContent(result.ToString())
			};

			response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
			return Task.FromResult(response);
		}
	}
}
