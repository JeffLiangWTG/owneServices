using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService.Test
{
	public class CorsIntegrationTest
	{
		[Test]
		public async Task BatchCORS()
		{
			using (var factory = IntegrationTestHelper.WebAppFactory)
			using (var client = factory.CreateClient())
			using (var batchRequest = new HttpRequestMessage(HttpMethod.Post, "http://localhost/odata/$batch"))
			using (var message = new HttpRequestMessage(HttpMethod.Get, "http://localhost/odata/RefAccTaxRateUpdate"))
			{
				var batchContent = new MultipartContent("mixed", "batch_36522ad7-fc75-4b56-8c71-56071383e77b");
				var portalUri = ConfigurationProvider.Configuration["PortalUri"];
				message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
				using (var content = new HttpMessageContent(message))
				{
					content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/http");
					content.Headers.Add("Content-Transfer-Encoding", "binary");
					for (var i = 0; i <= 2; i++)
					{
						batchContent.Add(content);
					}

					batchRequest.Content = batchContent;
					batchRequest.Headers.Add("Origin", portalUri);

					var contentString = await batchContent.ReadAsStringAsync();
					batchRequest.Content = batchContent;
					var response = await client.SendAsync(batchRequest);
					Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
					Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
					Assert.AreEqual(portalUri, response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault());
				}
			}
		}

		[Test]
		public void RefCusTaxOrFeeUpdateCORS()
		{
			var uri = new Uri("http://localhost:37016/odata/RefCusTaxOrFeeUpdate/Default.GetWithOptimizedExpand()?$filter=(ZZF_Code eq 'LFT' eq true or ZZF_Code eq 'LNT' eq true)");
			var portalUri = ConfigurationProvider.Configuration["PortalUri"];
			var result = string.Empty;
			Assert.DoesNotThrowAsync(async () =>
			{
				using (var factory = IntegrationTestHelper.WebAppFactory)
				using (var client = factory.CreateClient())
				{
					client.DefaultRequestHeaders.Add("Origin", portalUri);
					using (var response = await client.GetAsync(uri))
					{
						response.EnsureSuccessStatusCode();
						result = await response.Content.ReadAsStringAsync();

						Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
						Assert.AreEqual(portalUri, response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault());
					}
				}
			});
			Assert.AreEqual(RefCusTaxOrFeeResponse, result);
		}

		readonly string RefCusTaxOrFeeResponse = "{\"@odata.context\":\"http://localhost:37016/odata/$metadata#RefCusTaxOrFeeUpdate\",\"value\":[{\"ZZF_PK\":\"ab07a112-8d73-40fb-8e68-0dce92a4ad84\",\"ZZF_Code\":\"LNT\",\"ZZF_Description\":\"LCT Normal Vehicle Threshold\",\"ZZF_Value\":0,\"ZZF_StartDate\":\"0001-01-01T00:00:00Z\",\"ZZF_EndDate\":\"0001-01-01T00:00:00Z\",\"ZZF_ZZZ_NKDataGrouping\":\"AU\",\"ZZF_Minimum\":0,\"ZZF_Maximum\":0,\"ZZF_Threshold\":0,\"ZZF_ZX0_NKTaxOrFeeType\":\"VAT\"}]}";
	}
}
