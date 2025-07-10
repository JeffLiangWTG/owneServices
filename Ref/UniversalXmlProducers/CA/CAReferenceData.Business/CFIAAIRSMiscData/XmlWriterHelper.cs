using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.CAReferenceData.Business.CFIAAIRSMiscData
{
	public static class XmlWriterHelper
	{
		public static XmlWriterConfiguration GetRefCusCodelistConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();

			var codelistConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			codelistConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codelistConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			codelistConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "CFIAM");
			codelistConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "CA");
			codelistConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, new DateTime(1900, 1, 1));
			codelistConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.DefaultValues.MaxDateTime);

			writerConfiguration.IncludeEntityTypeConfiguration(codelistConfiguration);

			return writerConfiguration;
		}
	}
}
