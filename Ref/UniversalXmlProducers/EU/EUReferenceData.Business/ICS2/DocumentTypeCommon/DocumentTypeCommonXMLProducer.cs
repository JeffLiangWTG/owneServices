using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.DocumentTypeCommon.Business
{
	public class DocumentTypeCommonXMLProducer : CommonXMLProducer
	{
		public DocumentTypeCommonXMLProducer()
			: base("EUICS2_DocumentTypeCommon.xml",
				"EU ICS2 – Document Type Common (CL013)",
				GetRefCusCodeListConfigurationForDocumentTypeCommon(),
				UpdateType.Full)
		{
		}

		protected override CommonDataParser DataParser
		{
			get
			{
				return new DocumentTypeCommonDataParser();
			}
		}

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForDocumentTypeCommon()
		{
			var cusCodeListConfig = new EntityTypeConfiguration<RefCusCodeList>(true);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "IC2DT");
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Code, true);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_Description, false);
			cusCodeListConfig.IncludeColumn(x => x.ZZD_StartDate, false);
			cusCodeListConfig.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "EUN");
			cusCodeListConfig.IncludeColumn(x => x.RefCusCodeListLanguages, false);
			cusCodeListConfig.IncludeColumn(x => x.RefCusCodeListAttributes, false);

			var cusCodeListLanguageConfig = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
			cusCodeListLanguageConfig.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
			cusCodeListLanguageConfig.IncludeColumn(x => x.ZXA_Description, false);

			var cusCodeListAttributeConfig = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
			cusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			cusCodeListAttributeConfig.IncludeColumn(x => x.ZZE_Value, false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListLanguageConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListAttributeConfig);

			return writerConfiguration;
		}
	}
}
