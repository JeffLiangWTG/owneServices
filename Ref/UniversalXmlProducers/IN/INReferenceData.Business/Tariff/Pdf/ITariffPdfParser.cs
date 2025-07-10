using System;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public interface ITariffPdfParser
	{
		string ParseFilesAsJson(string pdfRootFolder, DateTime? date);
	}
}