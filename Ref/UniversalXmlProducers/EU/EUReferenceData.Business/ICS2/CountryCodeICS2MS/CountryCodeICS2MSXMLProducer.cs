using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.ICS2CountryCodeICS2MS.Business
{
	public class CountryCodeICS2MSXMLProducer : CommonXMLProducer
	{
		public CountryCodeICS2MSXMLProducer() : base(
			Constants.CountryCodeICS2MS.OutputFileName,
			Constants.CountryCodeICS2MS.OutputFileDataSource,
			GetRefCusCodeListConfigurationForICS2CountryCodeICS2M()
			)
		{
		}

		protected override CommonDataParser DataParser => new CountryCodeICS2MSDataParser();

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForICS2CountryCodeICS2M()
		{
			var cusCodeListConfig = XmlWriterHelper.CusCodeListConfig();
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "IC2MS");
			cusCodeListConfig.IncludeColumn(x => x.ZZD_StartDate, false);
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
