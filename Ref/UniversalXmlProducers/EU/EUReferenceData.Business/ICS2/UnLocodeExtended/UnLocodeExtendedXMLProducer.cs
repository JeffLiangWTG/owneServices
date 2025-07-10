using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Business.ICS2
{
	public class UnLocodeExtendedXMLProducer : CommonXMLProducer
	{
		public UnLocodeExtendedXMLProducer() :
			base(Constants.UnLocodeExtended.OutputFileName,
				Constants.UnLocodeExtended.OutputFileDataSource,
				GetRefCusCodeListConfigurationForUnLocodeExtended())
		{
		}

		protected override CommonDataParser DataParser => new UnLocodeExtendedDataParser();

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForUnLocodeExtended()
		{
			var cusCodeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Description);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_StartDate);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "EUN");
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType,  true, "IC2UL");

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListConfig);

			return writerConfiguration;
		}
	}
}
