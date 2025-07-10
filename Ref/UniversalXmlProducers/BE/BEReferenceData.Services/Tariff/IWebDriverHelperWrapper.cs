using System.Collections.Generic;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.BEReferenceData.Services
{
	public interface IWebDriverHelperWrapper : IWebDriverHelper
	{
		List<string> DownloadFiles(string remoteUrl, string xpath, bool isMonthly);
	}
}
