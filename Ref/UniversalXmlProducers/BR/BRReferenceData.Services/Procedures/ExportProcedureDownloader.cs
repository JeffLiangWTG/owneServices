using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class ExportProcedureDownloader
	{
		public byte[] Download(HttpClient client)
		{
			using (var loginResponse = CaptchaSolvingUtils.Instance.GetLoginResponse(client))
			{
				var cpcPageResponse = client.GetAsyncEx(URL_ENQUADRAMENTO)?.Result;
				Contract.Assume(cpcPageResponse.IsSuccessStatusCode, nameof(cpcPageResponse.IsSuccessStatusCode));

				var viewId = CaptchaSolvingUtils.GetViewId(cpcPageResponse.Content.ReadAsStringAsync()?.Result);

				DoPrePostDownload(client, viewId);

				DoPrePostDownloadPart2(client, viewId);

				var downloadResponse = DoPostDownloadCSV(client, viewId);

				var bCpc = downloadResponse.Content.ReadAsByteArrayAsync()?.Result;

				Contract.Assume(bCpc != null);

				return bCpc;
			}
		}

		HttpResponseMessage DoPrePostDownload(HttpClient client, string viewId)
		{
			var prePost = new Dictionary<string, string>();
			prePost.Add("AJAXREQUEST", "_viewRoot");
			prePost.Add("j_id111:enquadramentoOperacaoMB_tipoConsulta", "0");
			prePost.Add("j_id111:valorConsulta_codigo", "");
			prePost.Add("j_id111:panelDownloadArquivoOpenedState", "");
			prePost.Add("j_id111", "j_id111");
			prePost.Add("autoScroll", "");
			prePost.Add("javax.faces.ViewState", viewId);
			prePost.Add("j_id111:j_id176", "j_id111:j_id176");
			using (var prePostBody = new FormUrlEncodedContent(prePost))
			{
				var prePostResponse = client.PostAsyncEx(URL_ENQUADRAMENTO, prePostBody)?.Result;

				Contract.Assume(prePostResponse != null);
				Contract.Assume(prePostResponse.IsSuccessStatusCode);

				return prePostResponse;
			}
		}

		HttpResponseMessage DoPrePostDownloadPart2(HttpClient client, string viewId)
		{
			var prePost2 = new Dictionary<string, string>();
			prePost2.Add("AJAXREQUEST", "_viewRoot");
			prePost2.Add("subviewAjaxCarregando:formAjaxCarregando", "subviewAjaxCarregando:formAjaxCarregando");
			prePost2.Add("uniqueToken", "");
			prePost2.Add("subviewAjaxCarregando:formAjaxCarregando:mpStatusOpenedState", "subviewAjaxCarregando:formAjaxCarregando:mpStatusOpenedState");
			prePost2.Add("javax.faces.ViewState", viewId);
			prePost2.Add("subviewAjaxCarregando:formAjaxCarregando:" + viewId, "subviewAjaxCarregando:formAjaxCarregando:" + viewId);
			prePost2.Add("ajaxSingle", "subviewAjaxCarregando:formAjaxCarregando:mpStatus");
			using (var prePostBody2 = new FormUrlEncodedContent(prePost2))
			{
				var prePostResponse2 = client.PostAsyncEx(URL_ENQUADRAMENTO, prePostBody2)?.Result;

				Contract.Assume(prePostResponse2 != null);
				Contract.Assume(prePostResponse2.IsSuccessStatusCode);

				return prePostResponse2;
			}
		}

		HttpResponseMessage DoPostDownloadCSV(HttpClient client, string viewId)
		{
			var p2 = new Dictionary<string, string>();
			p2.Add("j_id111:enquadramentoOperacaoMB_tipoConsulta", "0");
			p2.Add("j_id111:valorConsulta_codigo", "");
			p2.Add("j_id111:panelDownloadArquivoOpenedState", "");
			p2.Add("j_id111", "j_id111");
			p2.Add("autoScroll", "");
			p2.Add("javax.faces.ViewState", viewId);
			p2.Add("j_id111:j_id181", "j_id111:j_id181");
			using (var postBody2 = new FormUrlEncodedContent(p2))
			{
				var downloadResponse = client.PostAsyncEx(URL_ENQUADRAMENTO, postBody2).Result;

				Contract.Assume(downloadResponse != null);
				Contract.Assume(downloadResponse.IsSuccessStatusCode);

				return downloadResponse;
			}
		}

		readonly string URL_ENQUADRAMENTO = ConfigurationProvider.Configuration.GetSection("URL_TABELAS_ADUANEIRAS_DOWNLOAD_ENQUADRAMENTO").Value;
	}
}
