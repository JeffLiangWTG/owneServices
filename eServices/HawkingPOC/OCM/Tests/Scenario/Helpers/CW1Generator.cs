using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using OcmPoc.Tests.Scenario.Configuration;

namespace OcmPoc.Tests.Scenario.Helpers
{
	class CW1Generator : IGenerator
	{
		readonly string recipient;
		readonly int quantity;
		readonly HttpClient http;

		XDocument messageContent;

		public CW1Generator(string recipient, int quantity)
		{
			this.recipient = recipient;
			this.quantity = quantity;

			http = new HttpClient();
		}

		public async Task GenerateMessages()
		{
			for (int i = 0; i < quantity; i++)
			{
				await SendMessage();
			}
		}

		private async Task SendMessage()
		{
			using (var stream = new MemoryStream())
			using (var content = new StreamContent(stream))
			{
				content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
				messageContent.Save(stream);
				stream.Seek(0, SeekOrigin.Begin);

				var response = await http.PostAsync(TestConfig.CW1.MessageFlowsUri, content);
				if (response.IsSuccessStatusCode) { return; }

				throw new Exception($"{response.RequestMessage.RequestUri}: {response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
			}
		}

		public IGenerator PrepareMessage(string line, int lineCount)
		{
			messageContent =
				new XDocument(
					new XElement("CW1Message",
						new XElement("To", recipient),
						new XElement("TrackingId", Guid.NewGuid()),
						new XStreamingElement("Content",
							Enumerable.Range(1, lineCount).Select(_ => new XElement("string", line)))));

			return this;
		}
	}
}
