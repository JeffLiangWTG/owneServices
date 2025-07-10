using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.ICS2FunctionalErrorCodes.Business
{
	public class ICS2FunctionalErrorCodesXMLProducer : CommonXMLProducer
	{
		public ICS2FunctionalErrorCodesXMLProducer()
			: base(
			"EUICS2_ICS2FunctionalErrorCodes.xml",
			"EU ICS2 - ICS2FunctionalErrorCodes",
			GetRefCusCodeListConfigurationForICS2FunctionalErrorCodes()
			)
		{
		}

		protected override CommonDataParser DataParser
		{
			get
			{
				return new ICS2FunctionalErrorCodesDataParser();
			}
		}

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForICS2FunctionalErrorCodes()
		{
			var cusCodeListConfig = XmlWriterHelper.CusCodeListConfig();
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "CL723");

			var cusCodeListLanguageConfig = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
			cusCodeListLanguageConfig.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
			cusCodeListLanguageConfig.IncludeColumn(x => x.ZXA_Description, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListLanguageConfig);

			return writerConfiguration;
		}
	}
}
