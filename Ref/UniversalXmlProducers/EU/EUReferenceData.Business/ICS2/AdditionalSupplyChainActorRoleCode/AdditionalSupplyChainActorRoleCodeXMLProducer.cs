using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.AdditionalInformationCode.Business
{
	public class AdditionalSupplyChainActorRoleCodeXMLProducer : CommonXMLProducer
	{
		public AdditionalSupplyChainActorRoleCodeXMLProducer()
			: base(
			"EUICS2_AdditionalSupplyChainActorRoleCode.xml",
			"EU ICS2 - Additional Supply Chain Actor Role Code (CL704)",
			GetRefCusCodeListConfigurationForAdditionalSupplyChainActorRoleCode()
			)
		{
		}

		protected override CommonDataParser DataParser
		{
			get
			{
				return new AdditionalSupplyChainActorRoleCodeDataParser();
			}
		}

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForAdditionalSupplyChainActorRoleCode()
		{
			var cusCodeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "IC2SA");
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Description, false);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_StartDate, false);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "EUN");
			cusCodeListConfig.IncludeColumn(x => x.RefCusCodeListLanguages, false);

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
