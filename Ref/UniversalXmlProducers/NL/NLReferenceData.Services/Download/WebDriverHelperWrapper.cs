using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class WebDriverHelperWrapper : WebDriverHelper, IWebDriverHelperWrapper
	{
		public WebDriverHelperWrapper(string defaultDownloadDirectory) : base(defaultDownloadDirectory)
		{
			this.defaultDownloadDirectory = defaultDownloadDirectory;
		}
		string defaultDownloadDirectory;

		public List<string> DownloadFiles(string remoteUrl)
		{
			var loadInterval = 1;
			var loadIntervalDownload = 10;

			this.GetWebPage(remoteUrl, loadInterval);
			this.NavigateWebPageByXpath("//tr[contains(td[2], 'Q')]/td[4][contains(., 'Detail')]", loadInterval);
			this.NavigateWebPageByXpath("//button[contains(.,'Toggle navigation')]", loadInterval);
			this.NavigateWebPageByXpath("//a[contains(.,'Exporteren')]", loadIntervalDownload);
			this.Back(loadInterval);

			this.NavigateWebPageByXpath("//tr[contains(td[2], 'U')]/td[4][contains(., 'Detail')]", loadInterval);
			this.NavigateWebPageByXpath("//button[contains(.,'Toggle navigation')]", loadInterval);
			this.NavigateWebPageByXpath("//a[contains(.,'Exporteren')]", loadIntervalDownload);
			this.Back(loadInterval);

			this.NavigateWebPageByXpath("//tr[contains(td[2], 'V')]/td[4][contains(., 'Detail')]", loadInterval);
			this.NavigateWebPageByXpath("//button[contains(.,'Toggle navigation')]", loadInterval);
			this.NavigateWebPageByXpath("//a[contains(.,'Exporteren')]", loadIntervalDownload);

			return Directory.GetFiles(defaultDownloadDirectory, "*.xlsx").ToList();
		}
	}
}
