using System;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public sealed class AQISCommodityCodesParser : BaseCMRReferenceDataParser
	{
		protected override string FileNamePrefix => ApplicationConfig.AQISCommodityCodesFilePrefix;

		protected override string OutputXMLName => "RefCusCodeList_AU_AQISCommodityCodes.xml";

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate) => CMRXMLWriterConfigurationBuilder.BuildRefCusCodeListConfiguration(CMRConstants.CodeTypes.CMRAC, Constants.DataGrouping, CMRConstants.CodeListAttributeNames.AQISImportedFoodsIndicator);

		protected override string DataSource => "AU AQIS Commodity Codes";

		static PropertyMapping<RefCusCodeList>[] CodeListMappings => new PropertyMapping<RefCusCodeList>[]
		{
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Code, 1, 4),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Description, 8, 240)
		};

		static PropertyMapping<RefCusCodeListAttribute>[] CodeListAttributeMappings => new PropertyMapping<RefCusCodeListAttribute>[]
		{
			new PropertyMapping<RefCusCodeListAttribute>(entity => entity.ZZE_Value, 6, 1),
		};

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			var codeListConverter = new LineToEntityConverter<RefCusCodeList>(CodeListMappings, 1);
			var attributeConverter = new LineToEntityConverter<RefCusCodeListAttribute>(CodeListAttributeMappings, 1);

			foreach (var line in content.NonEmptyLines())
			{
				var code = codeListConverter.Convert(line);
				var attribute = attributeConverter.Convert(line);
				if (code.ZZD_Code != null && code.ZZD_Description != null && attribute.ZZE_Value != null)
				{
					code.RefCusCodeListAttributes = new[] { attribute };
					xmlWriter.PopulateData(code);
				}
				else
				{
					Console.Error.WriteLine(InsufficientInfoErrorMessage, line);
				}
			}
		}
	}
}
