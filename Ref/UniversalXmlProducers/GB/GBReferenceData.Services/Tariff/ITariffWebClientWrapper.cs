namespace CargoWise.RefDbRepo.GBReferenceData.Services.Tariff
{
	public interface ITariffWebClientWrapper
	{
		string GetContentFromPost(string url, System.Collections.Specialized.NameValueCollection requestParams);

		string GetContent(string url);
		void DownloadFile(string url, string localPath);
		void SetAuthorisationHeader(string authHeader);
	}
}
