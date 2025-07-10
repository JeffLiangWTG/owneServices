using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.EUReferenceData.CommonXmlCoverter.Business;

namespace CargoWise.RefDbRepo.EUReferenceData.Business.ICS2
{
	public class TypeOfGoodsXMLProducer : CommonXMLProducer
	{
		public TypeOfGoodsXMLProducer() :
			base(Constants.TypeOfGoods.OutputFileName,
				Constants.TypeOfGoods.OutputFileDataSource,
				GetRefCusCodeListConfigurationForTypeOfGoods())
		{
		}

		protected override CommonDataParser DataParser => new TypeOfGoodsDataParser();

		static XmlWriterConfiguration GetRefCusCodeListConfigurationForTypeOfGoods()
		{
			var cusCodeListConfig = XmlWriterHelper.CusCodeListConfig();
			cusCodeListConfig.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, isKeyColumn: true, "IC2TG");
			cusCodeListConfig.IncludeColumn(x => x.ZZD_StartDate, isKeyColumn: false);
			cusCodeListConfig.IncludeColumn(x => x.RefCusCodeListLanguages, isKeyColumn: false);

			var cusCodeLanguageListConfig = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
			cusCodeLanguageListConfig.IncludeColumn(x => x.ZXA_ZX6_NKLanguage, isKeyColumn: true);
			cusCodeLanguageListConfig.IncludeColumn(x => x.ZXA_Description, isKeyColumn: false);

			var writerConfiguration = new XmlWriterConfiguration();
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeListConfig);
			writerConfiguration.IncludeEntityTypeConfiguration(cusCodeLanguageListConfig);

			return writerConfiguration;
		}
	}
}
