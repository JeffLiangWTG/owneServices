using System;
using System.Data;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.Common
{
	public interface IWebpageTableToDataTable
	{
		DataTable ExtractDatatableFromGrid(string headerXPath, string dataXPath);
		DateTime ExtractPublishDate(string xpathPublishDate);
		DateTime ExtractPublishDate(string xpathPublishDate, DateTime defaultDate);
		Uri ExtractUrlFromAnchor(string anchorText);
		Uri ExtractSingleUrlFromFileExtension(string fileExtension);
	}
}
