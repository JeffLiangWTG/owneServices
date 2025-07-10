using System.Globalization;
using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.Services
{
	public class LocationOfficeCodesItem
	{
		public string Code { get; set; }
	}

	public sealed class LocationOfficeCodesItemMap : ClassMap<LocationOfficeCodesItem>
	{
		public LocationOfficeCodesItemMap()
		{
			AutoMap();
			Map(m => m.Code).Name("Código");
		}
	}
}
