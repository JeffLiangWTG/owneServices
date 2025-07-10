using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AdditionalInformationCodeSubset.Business
{
	public class AdditionalInformationCodeSubsetXMLProducer : CommonXMLProducer
	{
		public AdditionalInformationCodeSubsetXMLProducer()
			: base(
			"EUICS2_AdditionalInformationCodeSubset.xml",
			"EU ICS2 – Additional Information Code Subset (CL752)",
			GetRefCusCodeListConfigurationForAdditionalInformationCodeSubset()
			)
		{
		}

		protected override CommonDataParser DataParser
		{
			get
			{
				return new AdditionalInformationCodeSubsetDataParser();
			}
		}

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForAdditionalInformationCodeSubset()
		{
			var cusCodeListConfig = XmlWriterHelper.CusCodeListConfig();
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "IC2AI");

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
