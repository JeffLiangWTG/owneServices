using System.Collections.Generic;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public interface IWebDriverHelperWrapper : IWebDriverHelper
	{
		List<string> DownloadFiles(string remoteUrl);
	}
}
