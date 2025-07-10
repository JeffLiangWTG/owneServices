using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class PisCofinsReductionTariffDownloader
	{
		public static byte[] Download(HttpClient client)
		{
			using (var loginResponse = CaptchaSolvingUtils.Instance.GetLoginResponse(client))
			{
				var BaseCalculationReductionPisCofins = client.GetAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_TABELAS_ADUANEIRAS_DOWNLOAD_BASE_CALCULATION_REDUCTION_PIS_COFINS").Value)?.Result;
				Contract.Assume(BaseCalculationReductionPisCofins.IsSuccessStatusCode);

				var viewId = CaptchaSolvingUtils.GetViewId(BaseCalculationReductionPisCofins.Content.ReadAsStringAsync()?.Result);

				DoPrePostDownload(client, viewId);

				DoPrePostDownloadPart2(client, viewId);

				var downloadResponse = DoPostDownloadXML(client, viewId);

				var bXml = downloadResponse.Content.ReadAsByteArrayAsync()?.Result;

				Contract.Assume(bXml != null);

				return bXml;
			}
		}

		static HttpResponseMessage DoPrePostDownload(HttpClient client, string viewId)
		{
			var prePost = new Dictionary<string, string>();
			prePost.Add("AJAXREQUEST", "_viewRoot");
			prePost.Add("j_id111:fundamentoLegalReducaoPisCofinsMB_tipoConsulta", "0");
			prePost.Add("j_id111:valorConsulta_codigo", "");
			prePost.Add("j_id111:panelDownloadArquivoOpenedState", "");
			prePost.Add("j_id111", "j_id111");
			prePost.Add("autoScroll", "");
			prePost.Add("javax.faces.ViewState", viewId);
			prePost.Add("j_id111:j_id176", "j_id111:j_id176");
			using (var prePostBody = new FormUrlEncodedContent(prePost))
			{
				var prePostResponse = client.PostAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_TABELAS_ADUANEIRAS_DOWNLOAD_BASE_CALCULATION_REDUCTION_PIS_COFINS").Value, prePostBody)?.Result;

				Contract.Assume(prePostResponse != null);
				Contract.Assume(prePostResponse.IsSuccessStatusCode);

				return prePostResponse;
			}
		}

		static HttpResponseMessage DoPrePostDownloadPart2(HttpClient client, string viewId)
		{
			var prePost2 = new Dictionary<string, string>();
			prePost2.Add("AJAXREQUEST", "_viewRoot");
			prePost2.Add("subviewAjaxCarregando:formAjaxCarregando", "subviewAjaxCarregando:formAjaxCarregando");
			prePost2.Add("uniqueToken", "");
			prePost2.Add("subviewAjaxCarregando:formAjaxCarregando:mpStatusOpenedState", "");
			prePost2.Add("javax.faces.ViewState", viewId);
			prePost2.Add("subviewAjaxCarregando:formAjaxCarregando:j_id6", "subviewAjaxCarregando:formAjaxCarregando:j_id6");
			prePost2.Add("ajaxSingle", "subviewAjaxCarregando:formAjaxCarregando:mpStatus");
			using (var prePostBody2 = new FormUrlEncodedContent(prePost2))
			{
				var prePostResponse2 = client.PostAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_TABELAS_ADUANEIRAS_DOWNLOAD_BASE_CALCULATION_REDUCTION_PIS_COFINS").Value, prePostBody2)?.Result;

				Contract.Assume(prePostResponse2 != null);
				Contract.Assume(prePostResponse2.IsSuccessStatusCode);

				return prePostResponse2;
			}
		}

		static HttpResponseMessage DoPostDownloadXML(HttpClient client, string viewId)
		{
			var p2 = new Dictionary<string, string>();
			p2.Add("j_id111:fundamentoLegalReducaoPisCofinsMB_tipoConsulta", "0");
			p2.Add("j_id111:valorConsulta_codigo", "");
			p2.Add("j_id111:panelDownloadArquivoOpenedState", "");
			p2.Add("j_id111", "j_id111");
			p2.Add("autoScroll", "");
			p2.Add("javax.faces.ViewState", viewId);
			p2.Add("j_id111:j_id187", "j_id111:j_id187");
			using (var postBody2 = new FormUrlEncodedContent(p2))
			{
				var downloadResponse = client.PostAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_TABELAS_ADUANEIRAS_DOWNLOAD_BASE_CALCULATION_REDUCTION_PIS_COFINS").Value, postBody2).Result;

				Contract.Assume(downloadResponse != null);
				Contract.Assume(downloadResponse.IsSuccessStatusCode);

				return downloadResponse;
			}
		}
	}
}
