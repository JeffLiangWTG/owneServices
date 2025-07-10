using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class CSVProcessorItem
	{
		public string Code { get; set; }

		public string Description { get; set; }

		public string StartDate { get; set; }

		public string EndDate { get; set; }
	}

	public sealed class CSVProcessorItemMap : ClassMap<CSVProcessorItem>
	{
		public CSVProcessorItemMap()
		{
			Map(m => m.Code).Name("ZZD_Code");
			Map(m => m.Description).Name("ZZD_Description");
			Map(m => m.StartDate).Name("ZZD_StartDate");
			Map(m => m.EndDate).Name("ZZD_EndDate");
		}
	}
}
