using System;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Common
{
	public interface IWebClientWrapper
	{
		string GetContent(string url);

		(DateTime LastModified, string Content) GetDatedContent(string url);

		byte[] GetContentAsByteArray(string url);

		(DateTime LastModified, byte[] Content) GetDatedContentAsByteArray(string url);
	}
}
