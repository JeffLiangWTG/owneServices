namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public interface IWebRequestWrapper
	{
		string GetContentFromPost(string url, string postData, bool sanitize = true);

		string GetContent(string url);
	}
}
