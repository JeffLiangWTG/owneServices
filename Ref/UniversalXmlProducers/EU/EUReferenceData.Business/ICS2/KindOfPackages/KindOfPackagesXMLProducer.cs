using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.KindOfPackages.Business
{
	public sealed class KindOfPackagesXMLProducer : CommonXMLProducer
	{
		public KindOfPackagesXMLProducer()
			: base("EUICS2_KindOfPackages.xml", "EU ICS2 - Kind Of Packages"
				  , GetRefCusCodeListConfigurationForKindOfPackages())
		{
		}

		protected override CommonDataParser DataParser
		{
			get
			{
				return new KindOfPackagesDataParser();
			}
		}

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForKindOfPackages()
		{
			var cusCodeListConfig = XmlWriterHelper.CusCodeListConfig();
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "PKG");

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
