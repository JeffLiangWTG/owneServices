using System;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class AdditionalCodesXmlProducer : XmlProducer<RefCusCodeList>
	{
		public AdditionalCodesXmlProducer()
		{
			XmlWriter = new XmlWriter(GetWriterConfiguration());
		}

		public override string FilePath => ApplicationConfig.AdditionalCodesUXmlFile;

		public override string DataSource => ApplicationConfig.AdditionalCodesDataSource;

		static IXmlWriterConfiguration GetWriterConfiguration()
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);

			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, "ADDCD");
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, "EUN");
			codeListConfiguration.IncludeColumn(x => x.ZZD_StartDate, false);
			codeListConfiguration.IncludeColumn(x => x.ZZD_EndDate, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListLanguages, false);
			var codelistWithLanguageConfiguration = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
			codelistWithLanguageConfiguration.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, true);
			codelistWithLanguageConfiguration.IncludeColumn(x => x.ZXA_Description, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistWithLanguageConfiguration);

			return writerConfiguration;
		}
	}
}
