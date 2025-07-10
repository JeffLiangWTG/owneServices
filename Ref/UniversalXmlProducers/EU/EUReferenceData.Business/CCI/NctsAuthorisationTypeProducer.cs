using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Business
{
	public class AuthorisationTypeProducer : CommonXMLProducer
	{
		public AuthorisationTypeProducer() :
			base(Constants.AuthorisationType.OutputFileName,
				Constants.AuthorisationType.OutputFileDataSource,
				GetRefCusCodeListConfigurationForAuthorisationType())
		{
		}

		protected override CommonDataParser DataParser => new AuthorisationTypeDataParser();

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForAuthorisationType()
		{
			var cusCodeListConfig = XmlWriterHelper.CusCodeListConfig(defaultStartDateWithMinimumDate: true);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, isKeyColumn: true, "AUTH");
			cusCodeListConfig.IncludeColumn(x => x.RefCusCodeListLanguages);

			var cusCodeListLanguageConfig = new EntityTypeConfiguration<RefCusCodeListLanguage>(setDataAttribute: true);
			cusCodeListLanguageConfig.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, isKeyColumn: true);
			cusCodeListLanguageConfig.IncludeColumn(x => x.ZXA_Description);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListLanguageConfig);
			return writerConfiguration;
		}
	}
}
