using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.TypeOfMeansOfTransport.Business
{
	public class TypeOfMeansOfTransportXMLProducer : CommonXMLProducer
	{
		public TypeOfMeansOfTransportXMLProducer() : base(
			Constants.TypeOfMeansOfTransport.OutputFileName,
			Constants.TypeOfMeansOfTransport.OutputFileDataSource,
			GetRefCusCodeListConfigurationForTypeOfMeansOfTransport()
			)
		{
		}

		protected override CommonDataParser DataParser => new TypeOfMeansOfTransportDataParser();

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForTypeOfMeansOfTransport()
		{
			var cusCodeListConfig = XmlWriterHelper.CusCodeListConfig();
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "IC2MT");
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
