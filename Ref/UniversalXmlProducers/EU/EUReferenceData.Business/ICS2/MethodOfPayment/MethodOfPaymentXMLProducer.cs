using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.MethodOfPayment.Business
{
	public class MethodOfPaymentXMLProducer : CommonXMLProducer
	{
		public MethodOfPaymentXMLProducer()
			:base(
			"EUICS2_TransportMethodOfPayment.xml",
			"EU ICS2 - Transport Method Of Payment",
			GetRefCusCodeListConfigurationForMethodOfPayment()
			)
		{
		}

		protected override CommonDataParser DataParser
		{
			get
			{
				return new MethodOfPaymentDataParser();
			}
		}

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForMethodOfPayment()
		{
			var cusCodeListConfig =XmlWriterHelper.CusCodeListConfig();
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "MOP");

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
