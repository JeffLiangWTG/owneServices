using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Xml.Linq;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TariffsDownloader : BaseAduanaTableDownloader
	{
		public IEnumerable<string> Download(HttpClient client)
		{
			var list = new List<string>();
			using var responseLogin = DoLogin(client);
			if (!responseLogin.IsSuccessStatusCode)
			{
				AddLog("Unable to solve captcha on (Tabelas Aduananeiras)");
				return list;
			}

			using var response = TariffsXmlDownload(client, ViewId(responseLogin));
			if (response.IsSuccessStatusCode)
			{
				list.AddRange(DataParser(response.Content.ReadAsStream()));
			}

			return list;
		}

		static HttpResponseMessage TariffsXmlDownload(HttpClient client, string viewId)
		{
			var prePost = new Dictionary<string, string>
			{
				{ "j_id111:subItemNcmMB_tipoConsulta", "0" },
				{ "j_id111:valorConsulta_codigo", "" },
				{ "j_id111:panelDownloadArquivoOpenedState", "" },
				{ "j_id111", "j_id111" },
				{ "autoScroll", "" },
				{ "javax.faces.ViewState", viewId },
				{ "j_id111:j_id229", "j_id111:j_id229" }
			};
			using (var prePostBody = new FormUrlEncodedContent(prePost))
			{
				var prePostResponse = client.PostAsyncEx(URL_LVL6_SUBITEM, prePostBody)?.Result;
				return prePostResponse;
			}
		}

		static IEnumerable<string> DataParser(Stream sXml)
		{
			var list = new List<string>();

			var xml = XDocument.Load(sXml);

			var elementList = xml.Root.Descendants(TariffsConstants.TagNCM);

			foreach (var element in elementList)
			{
				var tariffCode = element.GetElementValueAsString(TariffsConstants.TagCode, 8);
				if (tariffCode.Length == 8)
				{
					list.Add(tariffCode);
				}
			}

			return list;
		}

		static class TariffsConstants
		{
			public const string TagNCM = "Ncm";
			public const string TagCode = "codigo";
		}
	}
}
