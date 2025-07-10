using CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.PLReferenceData.Services.Taric4;

internal class WebDriverHelperFactory : IWebDriverHelperFactory
{
	public IWebDriverHelper Create(string downloadFolder) =>
		new WebDriverHelper(downloadFolder);
}
