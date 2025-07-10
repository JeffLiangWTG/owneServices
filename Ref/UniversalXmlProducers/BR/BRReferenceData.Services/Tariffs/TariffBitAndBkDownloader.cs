using System;
using System.Globalization;
using System.Net.Http;
using System.Text;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class TariffBitAndBkDownloader
	{
		public static byte[] Download(HttpClient client)
		{
			return DownloadJson(client);
		}

		static byte[] DownloadJson(HttpClient client)
		{
			SetupDefaultHeaderRequest(client);
			var today = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture);
			using (var prePostBody2 = new StringContent(
				"{\"version\":\"1.0.0\",\"queries\":[{\"Query\":{\"Commands\":[{\"SemanticQueryDataShapeCommand\":{\"Query\":{\"Version\":2,\"From\":[{\"Name\":\"p\",\"Entity\":\"Planilha1\",\"Type\":0}],\"Select\":[{\"Column\":{\"Expression\":{\"SourceRef\":{\"Source\":\"p\"}},\"Property\":\"NCM\"},\"Name\":\"Planilha1.NCM\"},{\"Column\":{\"Expression\":{\"SourceRef\":{\"Source\":\"p\"}},\"Property\":\"PUBLICADA\"},\"Name\":\"Planilha1.PUBLICADA\"},{\"Column\":{\"Expression\":{\"SourceRef\":{\"Source\":\"p\"}},\"Property\":\"DESCRIÇÃO\"},\"Name\":\"Planilha1.DESCRIÇÃO\"}],\"Where\":[{\"Condition\":{\"And\":{\"Left\":{\"Comparison\":{\"ComparisonKind\":2,\"Left\":{\"Column\":{\"Expression\":{\"SourceRef\":{\"Source\":\"p\"}},\"Property\":\"Início da vigência\"}},\"Right\":{\"Literal\":{\"Value\":\"datetime'" + today + "T00:00:00'\"}}}},\"Right\":{\"Comparison\":{\"ComparisonKind\":3,\"Left\":{\"Column\":{\"Expression\":{\"SourceRef\":{\"Source\":\"p\"}},\"Property\":\"Início da vigência\"}},\"Right\":{\"Literal\":{\"Value\":\"datetime'" + today + "T23:59:00'\"}}}}}}}],\"OrderBy\":[{\"Direction\":2,\"Expression\":{\"Column\":{\"Expression\":{\"SourceRef\":{\"Source\":\"p\"}},\"Property\":\"DESCRIÇÃO\"}}}]},\"Binding\":{\"Primary\":{\"Groupings\":[{\"Projections\":[0,1,2],\"Subtotal\":1}]},\"DataReduction\":{\"DataVolume\":3,\"Primary\":{\"Window\":{\"Count\":500}}},\"Version\":1},\"ExecutionMetricsKind\":1}}]},\"QueryId\":\"\",\"ApplicationContext\":{\"DatasetId\":\"74fb8907-9b39-4267-a434-2e7c1c8b6e9f\",\"Sources\":[{\"ReportId\":\"8a5711e8-759b-424b-82d0-80f5a88e0826\",\"VisualId\":\"3b684114a3a0bed5b12f\"}]}}],\"cancelQueries\":[],\"modelId\":3489892}",
				Encoding.UTF8,
				"application/json"))
			{
				var prePostResponse2 = client.PostAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_BIT_BK").Value, prePostBody2)?.Result;

				return prePostResponse2?.Content?.ReadAsByteArrayAsync()?.Result;
			}
		}

		static void SetupDefaultHeaderRequest(HttpClient client)
		{
			client.DefaultRequestHeaders.Add("Host", "wabi-brazil-south-api.analysis.windows.net");
			client.DefaultRequestHeaders.Add("Connection", "keep-alive");
			client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
			client.DefaultRequestHeaders.Add("sec-ch-ua", " Not A;Brand\";v=\"99\", \"Chromium\"; v=\"96\", \"Google Chrome\"; v=\"96\"");
			client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
			client.DefaultRequestHeaders.Add("RequestId", $"f4761d0d-63a8-0b3c-0d60-4f27d945ee{GetRandow()}");
			client.DefaultRequestHeaders.Add("ActivityId", $"07e3c663-eaac-7f14-be8c-dac859f{GetRandow()}e3d");
			client.DefaultRequestHeaders.Add("Accept", "application/json, text/plain, */*");
			client.DefaultRequestHeaders.Add("X-PowerBI-ResourceKey", "ffb78654-6608-49e8-82a4-df6c777008db");
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Safari/537.36");

			client.DefaultRequestHeaders.Add("sec-ch-ua-platform", "Windows");
			client.DefaultRequestHeaders.Add("Origin", "https://app.powerbi.com");
			client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "cross-site");
			client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
			client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
			client.DefaultRequestHeaders.Add("Referer", "https://app.powerbi.com/");
			client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
		}

		static int GetRandow()
		{
			var rnd = new Random();
#pragma warning disable CA5394 // This code is not used for security
			return rnd.Next(10, 99);
#pragma warning restore CA5394 // This code is not used for security
		}
	}
}
