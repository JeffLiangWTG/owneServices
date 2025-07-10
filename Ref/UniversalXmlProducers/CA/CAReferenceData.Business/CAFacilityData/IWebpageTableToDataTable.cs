using System.Data;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CAFacilityData
{
	public interface IWebpageTableToDataTable
	{
		DataTable ExtractDatatableFromGrid(string headerXPath, string dataXPath);
	}
}
