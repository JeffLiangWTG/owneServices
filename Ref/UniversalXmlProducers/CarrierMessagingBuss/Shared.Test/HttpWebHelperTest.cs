using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.CarrierMessagingBuss.Shared.Test
{
	internal class HttpWebHelperTest
	{
		[TestCase("https://cmb.wisegrid.net", "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZCI6IkFCQyJ9.7nteR3I7edYTM36zd8SAPZkC70G3gC2fxlwDP5TXa1M", "{\"code\":\"Vikas\",\"description\":\"Sample Desc\"}")]
		[TestCase("https://cmb-test.wisegrid.net", "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZCI6IlBRUiJ9.GeVg7qIEpWTU38DvX50sRuBArYO98lL69KPwCSaMdXc", "{\"description\":\"data not found\"}")]
		public async Task GetAsync(string uri, string token, string expectedData)
		{
			// Setup
			using var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
			{
				Content = new StringContent(expectedData)
			};
			using var httpClient = MockHelper.GetMockedHttpClient(httpResponseMessage, uri);
			var httpClientFactory = MockHelper.GetMockedHttpClientFactory(httpClient);
			var httpWebHelper = new HttpWebHelper<SampleResponse>(httpClientFactory);

			// Act
			var responseResult = await httpWebHelper.GetAsync(uri, token).ConfigureAwait(false);
			var expectedResponse = JsonSerializer.Deserialize<SampleResponse>(expectedData);
			Assert.That(responseResult.Code, Is.EqualTo(expectedResponse.Code));
			Assert.That(responseResult.Description, Is.EqualTo(expectedResponse.Description));
		}

		[Test]
		public async Task GetAsyncWithSampleData()
		{
			// Setup
			var uri = "https://cmb.wisegrid.net";
			var expectedData = @"{
				""code"": ""ACS"",
				""description"": ""Sample Accessorial""
			}";
			using var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
			{
				Content = new StringContent(expectedData)
			};
			using var httpClient = MockHelper.GetMockedHttpClient(httpResponseMessage, uri);
			var httpClientFactory = MockHelper.GetMockedHttpClientFactory(httpClient);
			var httpWebHelper = new HttpWebHelper<SampleResponse>(httpClientFactory);

			// Act
			var responseResult = await httpWebHelper.GetAsync(uri, "sampletoken").ConfigureAwait(false);
			Assert.That(responseResult.Code, Is.EqualTo("ACS"));
			Assert.That(responseResult.Description, Is.EqualTo("Sample Accessorial"));
		}

		[TestCase(HttpStatusCode.Unauthorized, "")]
		[TestCase(HttpStatusCode.NotFound, "404 - not found")]
		[TestCase(HttpStatusCode.InternalServerError, "exception occurred")]
		public void GetAsyncNonSuccessStatusCode(HttpStatusCode httpStatusCode, string content)
		{
			// Setup
			var uri = "https://cmb.wisegrid.net";
			using var httpResponseMessage = new HttpResponseMessage(httpStatusCode);
			if (!string.IsNullOrEmpty(content))
			{
				httpResponseMessage.Content = new StringContent(content);
			}
			using var httpClient = MockHelper.GetMockedHttpClient(httpResponseMessage, uri);
			var httpClientFactory = MockHelper.GetMockedHttpClientFactory(httpClient);
			var httpWebHelper = new HttpWebHelper<SampleResponse>(httpClientFactory);

			// Act & Assert
			Assert.That(Assert.ThrowsAsync<InvalidOperationException>(async () =>
			{
				await httpWebHelper.GetAsync(uri, "sampleToken").ConfigureAwait(false);
			}).Message, Is.EqualTo($"Api response is invalid. Status Code: {httpStatusCode}, Content: {content}"));
		}

		[TestCase("invalid_json")]
		[TestCase("")]
		public void GetAsyncMalformedJsonThrowsJsonException(string responseJson)
		{
			// Setup
			var uri = "https://cmb.wisegrid.net";
			using var httpResponseMessage = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent(responseJson)
			};
			using var httpClient = MockHelper.GetMockedHttpClient(httpResponseMessage, uri);
			var httpClientFactory = MockHelper.GetMockedHttpClientFactory(httpClient);
			var httpWebHelper = new HttpWebHelper<SampleResponse>(httpClientFactory);

			// Act & Assert
			Assert.ThrowsAsync<JsonException>(async () =>
			{
				await httpWebHelper.GetAsync(uri, "sampleToken").ConfigureAwait(false);
			});
		}

		public class SampleResponse
		{
			[JsonPropertyName("code")]
			public string Code { get; set; }

			[JsonPropertyName("description")]
			public string Description { get; set; }
		}
	}
}
