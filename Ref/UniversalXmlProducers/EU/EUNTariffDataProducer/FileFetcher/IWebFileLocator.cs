using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IWebFileLocator
	{
		IEnumerable<IWebFileInfo> GetLocationOfLatestFiles(string basePage);
		IEnumerable<IWebFileInfo> GetLocationOfLatestFiles(string basePage, bool takeLatestOfDuplicatedFiles);
		IEnumerable<IWebFileInfo> GetLocationOfSpecificFiles(string basePage, string[] folderNames, Regex searchPatternRegex);
	}
}
