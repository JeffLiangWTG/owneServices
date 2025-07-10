using CsvHelper.Configuration;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class CSVProcessorNationalAttributesItem : CSVProcessorItem
	{
		public string National { get; set; }
		public string UE { get; set; }
	}

	public sealed class CSVProcessorNationalAttributesItemMap : ClassMap<CSVProcessorNationalAttributesItem>
	{
		public CSVProcessorNationalAttributesItemMap()
		{
			Map(m => m.Code).Name("ZZD_Code");
			Map(m => m.Description).Name("ZZD_Description");
			Map(m => m.StartDate).Name("ZZD_StartDate");
			Map(m => m.EndDate).Name("ZZD_EndDate");
			Map(m => m.National).Name("ZZD_National_Value");
			Map(m => m.UE).Name("ZZD_UE_Value");
		}
	}
}
