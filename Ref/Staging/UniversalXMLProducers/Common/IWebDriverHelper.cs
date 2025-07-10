using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common
{
	public interface IWebDriverHelper : IDisposable
	{
		string GetWebPage(string url, int loadInterval = 0);
		string GetWebPageByLinkText(string linkText, int loadInterval = 0);
		string GetWebPageByLinkId(string id);
		string GetElementTextByClassName(string className, string elementName);
		string NavigateWebPageByXpath(string xpath, int loadInterval = 0);
		KeyValuePair<string, string> GetDownloadFileInfoByPartialLinkText(string regex, string partialLinkText);
		IEnumerable<KeyValuePair<string, string>> GetDownloadFilesInfoByPartialLinkText(string regex, string partialLinkText);
		void Back(int loadInterval = 0);
		void AcceptCookiesForTaricWebSite(string cookiesButtonText);
	}
}
