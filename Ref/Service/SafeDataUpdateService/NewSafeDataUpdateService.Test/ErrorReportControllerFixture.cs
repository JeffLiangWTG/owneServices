using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.ErrorReporting;
using Newtonsoft.Json;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	[TestFixture]
	class ErrorReportControllerFixture
	{
		[Test]
		public async Task ReportErrorAsync()
		{
			using (var factory = IntegrationTestHelper.WebAppFactory)
			using (var httpClient = factory.CreateClient())
			{
				var baseUri = httpClient.BaseAddress;
				var postUrl = $"{baseUri}api/ErrorReport/ReportError";
				var error = new WebError { Message = ErrorReportingKnownExceptionsMapping.ErrorReportingTestMessage, StackTrace = "StackTrace" };

				var message = JsonConvert.SerializeObject(error);
				using (var httpContent = new StringContent(message))
				{
					httpContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Json);
					using (var response = await httpClient.PostAsync(new Uri(postUrl), httpContent))
					{
						_ = response.Content.ReadAsStringAsync().Result;
						Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
					}
				}
			}
		}
	}
}
