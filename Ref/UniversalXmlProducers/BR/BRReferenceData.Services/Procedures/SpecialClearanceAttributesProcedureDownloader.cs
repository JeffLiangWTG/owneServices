using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class SpecialClearanceAttributesProcedureDownloader : BaseDownloaderWithHrefSearch
	{
		protected override byte[] DownloadFile(string href, HttpClient client)
		{
			SetupDefaultHeaderRequest(client);
			return base.DownloadFile(href, client);
		}
		protected override string Url => ConfigurationProvider.Configuration.GetSection("URL_SPECIAL_CLEARANCE_ATTRIBUTES").Value;

		protected override string InnerHtml => "Códigos de enquadramento da operação na exportação";

		static void SetupDefaultHeaderRequest(HttpClient client)
		{
			client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
			client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
			client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
			client.DefaultRequestHeaders.Add("Cache-Control", "no-cache");
			client.DefaultRequestHeaders.Add("Connection", "keep-alive");
			client.DefaultRequestHeaders.Add("Dnt", "1");
			client.DefaultRequestHeaders.Add("Host", "www.gov.br");
			client.DefaultRequestHeaders.Add("Pragma", "no-cache");
			client.DefaultRequestHeaders.Add("Referer", "https://www.gov.br/siscomex/pt-br/informacoes/tratamento-administrativos/tratamento-administrativo-de-exportacao-1");
			client.DefaultRequestHeaders.Add("Sec-Ch-Ua", "Microsoft Edge\";v=\"117\", \"Not;A=Brand\";v=\"8\", \"Chromium\";v=\"117");
			client.DefaultRequestHeaders.Add("Sec-Ch-Ua-Mobile", "?0");
			client.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", "Windows");
			client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
			client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
			client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
			client.DefaultRequestHeaders.Add("Sec-Fetch-User", "?1");
			client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
			client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36 Edg/117.0.2045.36");
		}
	}
}
