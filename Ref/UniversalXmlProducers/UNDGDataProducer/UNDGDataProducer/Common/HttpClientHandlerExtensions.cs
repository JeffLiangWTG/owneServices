using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.UNDGDataProducer.Common
{
	public static class HttpClientHandlerExtensions
	{
		public static HttpClient AssignBrowserHeaders(this HttpClient client)
		{
			RandomizeCookie(client);
			RandomizeUserAgent(client);
			return client;
		}

		static void RandomizeCookie(HttpClient client)
		{
			client.DefaultRequestHeaders.Remove("cookie");
			client.DefaultRequestHeaders.Add("cookie", $"fvv=; NSC_WT-iuuqt_xxx.qpsuofu.dpn_TTM={CommonHelper.RandomCharAndDigit(72)}; JSESSIONID_DGWebPublic={CommonHelper.RandomCharAndDigit(52)}!-{CommonHelper.RandomInteger(100000000, 200000000)}!{CommonHelper.RandomInteger(100000000, 200000000)}; RT=\"z=1&dm=www.portnet.com&si={Guid.NewGuid()}&ss={CommonHelper.RandomCharAndDigit(8)}&sl=0&tt=0\"");
		}

		static void RandomizeUserAgent(HttpClient client)
		{
			var devices = new string[]
			{
				"Chrome/123.0.0.0 Safari/537.36 Edg/123.0.0.0",
				"Chrome/123.0.0.0 Safari/537.36"
			};

			client.DefaultRequestHeaders.Remove("user-agent");
			client.DefaultRequestHeaders.Add("user-agent", $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) {devices[CommonHelper.RandomInteger(0, 1)]}");
		}
	}
}
