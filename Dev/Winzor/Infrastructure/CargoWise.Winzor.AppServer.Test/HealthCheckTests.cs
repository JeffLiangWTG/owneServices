using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CargoWise.Blazor.AppServer.Test
{
	[TestFixture, BlazorWithPlaywrightPage]
	public class HealthCheckTests
	{
		[Test]
		public async Task HealthCheckReturnsHealthyWithProcessId()
		{
			var blazorProcess = BlazorWithPlaywrightPageAttribute.BlazorProcess;
			Assert.That(blazorProcess.HasExited, Is.False);

			var address = BlazorWithPlaywrightPageAttribute.BlazorAddress;
			using var client = new HttpClient();

			var responseMessage = await client.GetAsync(new Uri($"{address}/health"));
			var responseContent = await responseMessage.Content.ReadAsStringAsync();

			Assert.That(responseMessage.IsSuccessStatusCode, Is.True, message: $"expecting success response from endpoint. response content: {responseContent}");
			Assert.That(responseContent, Is.Not.Empty);

			// include the response body in the test result if it fails to parse (eg, html instead of json)
			using var document = TryParse(responseContent, out var result) ? result : null;
			if (document == null)
			{
				Assert.Fail($"Failed to parse json {address} {nameof(responseContent)}: '{responseContent}'");
			}

			var json = document.RootElement;
			var health = json.GetProperty("status").GetString();
			Assert.That(health, Is.EqualTo("Healthy"));

			var pid = json.GetProperty("data").GetProperty("ProcessId").GetInt32();
			Assert.That(pid, Is.EqualTo(blazorProcess.Id));
		}

		static bool TryParse(string json, out JsonDocument document)
		{
			try
			{
				document = JsonDocument.Parse(json);
				return true;
			}
			catch
			{
				document = null;
				return false;
			}
		}
	}
}
