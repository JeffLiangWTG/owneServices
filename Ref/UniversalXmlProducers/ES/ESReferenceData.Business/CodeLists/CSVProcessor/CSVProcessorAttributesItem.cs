using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class CSVProcessorAttributesItem : CSVProcessorItem
	{
		public string HeaderLevel { get; set; }

		public string HouseLevel { get; set; }

		public string ItemLevel { get; set; }

		public string Reference { get; set; }

		public string ItemNumber { get; set; }

		public string Complement { get; set; }
	}

	public sealed class CSVProcessorAttributesItemMap : ClassMap<CSVProcessorAttributesItem>
	{
		public CSVProcessorAttributesItemMap()
		{
			Map(m => m.Code).Name("ZZD_Code");
			Map(m => m.Description).Name("ZZD_Description");
			Map(m => m.StartDate).Name("ZZD_StartDate");
			Map(m => m.EndDate).Name("ZZD_EndDate");
			Map(m => m.HeaderLevel).Name("Header Level");
			Map(m => m.HouseLevel).Name("House Level");
			Map(m => m.ItemLevel).Name("Item Level");
			Map(m => m.Reference).Name("Reference");
			Map(m => m.ItemNumber).Name("ItemNumber");
			Map(m => m.Complement).Name("Complement");
		}
	}
}
