using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using System;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class CustomsOfficeDepartmentsHelper
	{
		const string CodeType = "DEPTC";
		const string DataGrouping = "JP";

		static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);
		static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

		public static XmlWriterConfiguration GetRefCusCodeListConfiguration()
		{
			var cusCodeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, CodeType);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Description, false);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, MinimumDateTime);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, MaximumDateTime);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, DataGrouping);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListConfig);

			return writerConfiguration;
		}
	}
}
