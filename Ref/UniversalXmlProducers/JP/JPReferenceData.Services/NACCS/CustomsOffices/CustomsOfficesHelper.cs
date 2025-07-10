using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class CustomsOfficesHelper
	{
		static DateTime MinimumDateTime => new DateTime(1900, 01, 01, 00, 00, 00);
		static DateTime MaximumDateTime => new DateTime(2079, 06, 06, 23, 59, 00);

		public static XmlWriterConfiguration GetRefCusCodeListConfiguration()
		{
			var cusCodeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, Constants.CodeType.CustomsOffices);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Description, false);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, MinimumDateTime);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, MaximumDateTime);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.DataGrouping.JP);
			cusCodeListConfig.IncludeColumn(x => x.RefCusCodeListAttributes, false);

			var cusCodeListAttribute = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			cusCodeListAttribute.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			cusCodeListAttribute.IncludeColumn(x => x.ZZE_Value, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListAttribute);

			return writerConfiguration;
		}
	}
}
