using System;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public interface IFolderLocationProvider
	{
		string GetPdfDownloadFolder(DateTime? tariffDate);
		string GetJsonCreationFolder(DateTime? tariffDate);
		string GetOutputTariffXmlFilePath(string chapter, DateTime date);
	}
}
