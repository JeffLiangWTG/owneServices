using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public class WebDriverHelperWrapper : WebDriverHelper, IWebDriverHelperWrapper
	{
		public WebDriverHelperWrapper(string defaultDownloadDirectory) : base(defaultDownloadDirectory)
		{
			this.defaultDownloadDirectory = defaultDownloadDirectory;
		}

		readonly string defaultDownloadDirectory;

		public List<string> DownloadFiles(string remoteUrl, string xpath, bool isMonthly)
		{
			NavigateWebPageByXpath(xpath, isMonthly ? ApplicationConfig.DownloadIntervalMonthly : ApplicationConfig.DownloadIntervalDaily);

			return Directory.GetFiles(defaultDownloadDirectory, "*").ToList();
		}
	}
}
