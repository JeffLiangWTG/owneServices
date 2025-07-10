using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class CustomsEnclosureCodeListDownloader
	{
		public EnclosureDTO Download(HttpClient client)
		{
			using (var loginResponse = CaptchaSolvingUtils.Instance.GetLoginResponse(client))
			{
				var enclosurePageResponse = client.GetAsyncEx(URL_RECINTO_ALFANDEGADO)?.Result;
				Contract.Assume(enclosurePageResponse.IsSuccessStatusCode);

				var viewId = CaptchaSolvingUtils.GetViewId(enclosurePageResponse.Content.ReadAsStringAsync()?.Result);

				DoPrePostDownload(client, viewId);

				DoPrePostDownloadPart2(client, viewId);

				var downloadResponse = DoPostDownloadXML(client, viewId);

				var bXml = downloadResponse.Content.ReadAsByteArrayAsync()?.Result;

				Contract.Assume(bXml != null);

				EnclosureDTO dto = new EnclosureDTO
				{
					data = bXml,
					downloadedDate = DateTime.Now
				};

				return dto;
			}
		}

		HttpResponseMessage DoPrePostDownload(HttpClient client, string viewId)
		{
			var prePost = new Dictionary<string, string>();
			prePost.Add("AJAXREQUEST", "_viewRoot");
			prePost.Add("j_id113:tipoConsulta", "0");
			prePost.Add("j_id113:valorConsulta_codigo", "");
			prePost.Add("j_id113:downloadRecintoAduaneiroPanelDownloadArquivoOpenedState", "");
			prePost.Add("j_id113", "j_id113");
			prePost.Add("autoScroll", "");
			prePost.Add("javax.faces.ViewState", viewId);
			prePost.Add("j_id113:downloadRecintoAduaneiro", "j_id113:downloadRecintoAduaneiro");
			using (var prePostBody = new FormUrlEncodedContent(prePost))
			{
				var prePostResponse = client.PostAsyncEx(URL_RECINTO_ALFANDEGADO, prePostBody)?.Result;

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
			prePost2.Add("subviewAjaxCarregando:formAjaxCarregando:mpStatusOpenedState", "");
			prePost2.Add("javax.faces.ViewState", viewId);
			prePost2.Add("subviewAjaxCarregando:formAjaxCarregando:j_id8", "subviewAjaxCarregando:formAjaxCarregando:j_id8");
			prePost2.Add("ajaxSingle", "subviewAjaxCarregando:formAjaxCarregando:mpStatus");
			using (var prePostBody2 = new FormUrlEncodedContent(prePost2))
			{
				var prePostResponse2 = client.PostAsyncEx(URL_RECINTO_ALFANDEGADO, prePostBody2)?.Result;

				Contract.Assume(prePostResponse2 != null);
				Contract.Assume(prePostResponse2.IsSuccessStatusCode);

				return prePostResponse2;
			}
		}

		HttpResponseMessage DoPostDownloadXML(HttpClient client, string viewId)
		{
			var p2 = new Dictionary<string, string>();
			p2.Add("j_id113:tipoConsulta", "0");
			p2.Add("j_id113:valorConsulta_codigo", "");
			p2.Add("j_id113:downloadRecintoAduaneiroPanelDownloadArquivoOpenedState", "");
			p2.Add("j_id113", "j_id113");
			p2.Add("autoScroll", "");
			p2.Add("javax.faces.ViewState", viewId);
			p2.Add("j_id113:j_id194", "j_id113:j_id194");
			using (var postBody2 = new FormUrlEncodedContent(p2))
			{
				var downloadResponse = client.PostAsyncEx(URL_RECINTO_ALFANDEGADO, postBody2).Result;

				Contract.Assume(downloadResponse != null);
				Contract.Assume(downloadResponse.IsSuccessStatusCode);

				return downloadResponse;
			}
		}

		readonly string URL_RECINTO_ALFANDEGADO = ConfigurationProvider.Configuration.GetSection("URL_TABELAS_ADUANEIRAS_DOWNLOAD_RECINTO_ALFANDEGADO").Value;
	}
}
