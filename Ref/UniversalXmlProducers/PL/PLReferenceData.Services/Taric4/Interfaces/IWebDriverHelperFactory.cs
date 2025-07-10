using CargoWise.RefDbRepo.UniversalXMLProducers.Common;

namespace CargoWise.RefDbRepo.PLReferenceData.Services.Taric4.Interfaces;

interface IWebDriverHelperFactory
{
	IWebDriverHelper Create(string downloadFolder);
}
