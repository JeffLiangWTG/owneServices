using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class DutyLegalBasisCodeListDownloader
	{
		public byte[] Download(HttpClient client)
		{
			using (var loginResponse = CaptchaSolvingUtils.Instance.GetLoginResponse(client))
			{
				var lRTIIPageResponse = client.GetAsyncEx(URL_LEGAL_II)?.Result;
				Contract.Assume(lRTIIPageResponse.IsSuccessStatusCode, nameof(lRTIIPageResponse.IsSuccessStatusCode));

				var viewId = CaptchaSolvingUtils.GetViewId(lRTIIPageResponse.Content.ReadAsStringAsync()?.Result);

				DoPrePostDownload(client, viewId);

				var downloadResponse = DoPostDownloadXML(client, viewId);

				var bXml = downloadResponse.Content.ReadAsByteArrayAsync()?.Result;

				Contract.Assume(bXml != null, nameof(bXml));

				return bXml;
			}
		}

		HttpResponseMessage DoPrePostDownload(HttpClient client, string viewId)
		{
			var prePost = new Dictionary<string, string>();
			prePost.Add("AJAXREQUEST", "_viewRoot");
			prePost.Add("j_id111:fundamentoLegalRegimeTributacaoIIMB_tipoConsulta", "0");
			prePost.Add("j_id111:valorConsulta_codigo", "");
			prePost.Add("j_id111:panelDownloadArquivoOpenedState:", "");
			prePost.Add("j_id111", "j_id111");
			prePost.Add("autoScroll", "");
			prePost.Add("javax.faces.ViewState", viewId);
			prePost.Add("j_id111:j_id176", "j_id111:j_id176");
			using (var prePostBody = new FormUrlEncodedContent(prePost))
			{
				var prePostResponse = client.PostAsyncEx(URL_LEGAL_II, prePostBody)?.Result;

				Contract.Assume(prePostResponse != null, nameof(prePostResponse));
				Contract.Assume(prePostResponse.IsSuccessStatusCode, nameof(prePostResponse.IsSuccessStatusCode));

				return prePostResponse;
			}
		}

		HttpResponseMessage DoPostDownloadXML(HttpClient client, string viewId)
		{
			var p2 = new Dictionary<string, string>();
			p2.Add("AJAXREQUEST", "_viewRoot");
			p2.Add("j_id111:fundamentoLegalRegimeTributacaoIIMB_tipoConsulta", "0");
			p2.Add("j_id111:valorConsulta_codigo", "");
			p2.Add("j_id111:panelDownloadArquivoOpenedState:", "");
			p2.Add("j_id111", "j_id111");
			p2.Add("autoScroll", "");
			p2.Add("javax.faces.ViewState", viewId);
			p2.Add("j_id111:j_id187", "j_id111:j_id187");
			using (var postBody2 = new FormUrlEncodedContent(p2))
			{
				var downloadResponse = client.PostAsyncEx(URL_LEGAL_II, postBody2).Result;

				Contract.Assume(downloadResponse != null, nameof(downloadResponse));
				Contract.Assume(downloadResponse.IsSuccessStatusCode, nameof(downloadResponse.IsSuccessStatusCode));

				return downloadResponse;
			}
		}

		readonly string URL_LEGAL_II = ConfigurationProvider.Configuration.GetSection("URL_TABELAS_ADUANEIRAS_FUNDAMENTO_LEGAL_II").Value;
	}
}
