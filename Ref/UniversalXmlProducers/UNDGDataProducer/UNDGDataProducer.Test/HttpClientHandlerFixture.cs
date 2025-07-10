using System;
using System.IO;
using NUnit.Framework;
using System.Net.Http;
using CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Common;
using System.Linq;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Test
{
	[TestFixture]
	public class HttpClientHandlerFixture
	{
		TextWriter originErrorOutput;

		[OneTimeSetUp]
		public void SetUp()
		{
			originErrorOutput = Console.Error;
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			if (originErrorOutput != default && originErrorOutput != Console.Error)
			{
				Console.SetError(originErrorOutput);
			}
		}

		/*
		 * Below test cannot run in DAT because it is designed to not allow access to external resources
		 * For developer's convenience we can keep it 
		 *
		static IEnumerable<string> TestUnnos()
		{
			for (var i = 320; i <= 340; i++)
			{
				yield return i.ToString("D4", CultureInfo.InvariantCulture);
			}
		}

		[Test]
		[TestCaseSource(nameof(TestUnnos))]
		public void ShouldNotThrowAndReturnUNDGSearchResult(string unno)
		{
			var url = $"https://www.portnet.com/DGWebPublic/com/pn2/dg/web/newdgchemicalpublic/searchChemicalList.do?%7bactionForm.searchBean.unNoFr%7d={unno}";
			var responseString = string.Empty;

			Assert.DoesNotThrowAsync(async () =>
			{
				using (var httpHandler = new HttpClientHandler(true, null, true))
				{
					var response = await httpHandler.GetAsync(new Uri(url));
					responseString = await response.Content.ReadAsStringAsync();
				}
				Console.WriteLine(responseString);
			});

			Assert.That(responseString.Contains($"<span>{unno}</span>", StringComparison.OrdinalIgnoreCase),
				$"{url}\r\nshould respond with valid search result of {unno}, but actual response is:\r\n{responseString}");
		}
		*/

		[Test]
		public void ShouldThrowAfterRetry()
		{
			var url = $"https://ThisUrlDoesNotExist/{Guid.NewGuid()}";
			var retryIntervals = new TimeSpan[]
			{
				TimeSpan.FromSeconds(0.25),
				TimeSpan.FromSeconds(0.5),
				TimeSpan.FromSeconds(1),
				TimeSpan.FromSeconds(1.5)
			};

			using (var httpHandler = new HttpClientHandler(true, retryIntervals, false))
			using (var sw = new StringWriter())
			{
				Console.SetError(sw);
				Assert.ThrowsAsync<HttpRequestException>(async () => await httpHandler.GetAsync(new Uri(url)));
				Assert.That(sw.ToString().Contains($"Fail to get {url} after retry {retryIntervals.Length} times", StringComparison.OrdinalIgnoreCase));
			}
		}

		[Test]
		public void ShouldRandomizeBrowserHeaders()
		{
			var userAgents = new HashSet<string>();
			var cookies = new HashSet<string>();

			for (var i = 0; i < 10; i++)
			{
				using (var client = new HttpClient())
				{
					client.AssignBrowserHeaders();

					var agent = client.DefaultRequestHeaders.UserAgent.ToString();
					var cookie = client.DefaultRequestHeaders.GetValues("cookie").FirstOrDefault();

					Assert.IsNotNull(agent);
					Assert.IsNotNull(cookie);

					userAgents.Add(agent);
					cookies.Add(cookie);
				}
			}

			Assert.That(userAgents.Count > 1);
			Assert.That(cookies.Count == 10);
		}
	}
}
