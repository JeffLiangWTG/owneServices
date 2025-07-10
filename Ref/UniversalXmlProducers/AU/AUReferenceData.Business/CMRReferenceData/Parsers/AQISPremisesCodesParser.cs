using System;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public class AQISPremisesCodesParser : BaseCMRReferenceDataParser
	{
		protected override string FileNamePrefix => ApplicationConfig.AQISPremisesCodesFilePrefix;

		protected override string OutputXMLName => "RefCusCodeList_AU_AQISPremisesCodes.xml";

		protected override string DataSource => "AU AQIS Premises Codes";

		protected static string CodeType { get => CMRConstants.CodeTypes.CMRAP; }

		const bool IsAutoExpiryOn = false;

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate)
		{
			var defaultStartDate = CMRXMLWriterConfigurationBuilder.GetDefaultStartDate(IsAutoExpiryOn, publishedDate);
			return CMRXMLWriterConfigurationBuilder.BuildRefCusCodeListExpirableConfiguration(CodeType, defaultStartDate, CMRConstants.CodeListAttributeNames.AQISPremisesPortCode);
		}

		protected static PropertyMapping<RefCusCodeList>[] CodeListMappings => new PropertyMapping<RefCusCodeList>[]
		{
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Code, 1, 10),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Description, 18, 240),
		};

		protected static PropertyMapping<RefCusCodeListAttribute>[] CodeListAttributeMappings => new PropertyMapping<RefCusCodeListAttribute>[]
		{
			 new PropertyMapping<RefCusCodeListAttribute>(entity => entity.ZZE_Value, 12, 5),
		};

		LineToEntityConverter<RefCusCodeList> CodeListConverter => codeListConverter ??= new LineToEntityConverter<RefCusCodeList>(CodeListMappings, 1);
		LineToEntityConverter<RefCusCodeList> codeListConverter;

		LineToEntityConverter<RefCusCodeListAttribute> AttributeConverter => attributeConverter ??= new LineToEntityConverter<RefCusCodeListAttribute>(CodeListAttributeMappings, 1);
		LineToEntityConverter<RefCusCodeListAttribute> attributeConverter;

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			foreach (var line in content.NonEmptyLines())
			{
				var code = ProcessLine(line);
				if (code != null)
				{
					xmlWriter.PopulateData(code);
				}
			}
		}

		protected RefCusCodeList ProcessLine(string line)
		{
			RefCusCodeList code = null;

			if (!string.IsNullOrWhiteSpace(line))
			{
				code = CodeListConverter.Convert(line);
				var attribute = AttributeConverter.Convert(line);
				if (code.ZZD_Code != null && code.ZZD_Description != null && attribute.ZZE_Value != null)
				{
					code.RefCusCodeListAttributes = new[] { attribute };
				}
				else
				{
					Console.Error.WriteLine(InsufficientInfoErrorMessage, line);
					code = null;
				}
			}

			return code;
		}
	}
}
