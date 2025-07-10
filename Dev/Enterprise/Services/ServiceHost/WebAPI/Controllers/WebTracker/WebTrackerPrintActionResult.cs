using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Enterprise.Tracking.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	public class WebTrackerPrintActionResult : IHttpActionResult
	{
		public WebTrackerPrintActionResult(IWebTrackerPrintResult webTrackerPrintResult)
		{
			WebTrackerPrintResult = webTrackerPrintResult;
		}

		public IWebTrackerPrintResult WebTrackerPrintResult;

		public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(Execute());
		}

		HttpResponseMessage Execute()
		{
			if (WebTrackerPrintResult == null)
			{
				return new HttpResponseMessage(HttpStatusCode.NotFound);
			}

			if (!string.IsNullOrEmpty(WebTrackerPrintResult.ErrorMessage))
			{
				return new HttpResponseMessage(HttpStatusCode.BadRequest)
				{
					Content = new StringContent(WebTrackerPrintResult.ErrorMessage),
				};
			}

			var result = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StreamContent(WebTrackerPrintResult.FileContents),
			};
			result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue((NoResString)"inline") // Not a code smell.
			{
				FileName = WebTrackerPrintResult.FileName,
			};
			result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

			return result;
		}
	}
}
