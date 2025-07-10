using System;
using System.IO;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public sealed class AQISDocumentTypesParser : BaseCMRReferenceDataParser
	{
		protected override string FileNamePrefix => ApplicationConfig.AQISDocumentTypesFilePrefix;

		protected override string OutputXMLName => "RefCusCodeList_AU_AQISDocumentTypes.xml";

		const bool IsAutoExpiryOn = true;

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate)
		{
			var defaultStartDate = CMRXMLWriterConfigurationBuilder.GetDefaultStartDate(IsAutoExpiryOn, publishedDate);
			return CMRXMLWriterConfigurationBuilder.BuildRefCusCodeListConfiguration(CMRConstants.CodeTypes.CMRDT, defaultStartDate, CMRConstants.CodeListAttributeNames.AQISDocumentTypeCountryCode);
		}

		protected override string DataSource => "AU AQIS Document Types";

		static PropertyMapping<RefCusCodeList>[] CodeListMappings => new PropertyMapping<RefCusCodeList>[]
		{
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Code, 1, 10),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Description, 15, 240)
		};

		static PropertyMapping<RefCusCodeListAttribute>[] CodeListAttributeMappings => new PropertyMapping<RefCusCodeListAttribute>[]
		{
			new PropertyMapping<RefCusCodeListAttribute>(entity => entity.ZZE_Value, 12, 2),
		};

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			var codeListConverter = new LineToEntityConverter<RefCusCodeList>(CodeListMappings, 1);
			var attributeConverter = new LineToEntityConverter<RefCusCodeListAttribute>(CodeListAttributeMappings, 1);

			using (var reader = new StringReader(content))
			{
				var line = string.Empty;
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						var code = codeListConverter.Convert(line);
						var attribute = attributeConverter.Convert(line);
						if (code.ZZD_Code != null && code.ZZD_Description != null && attribute.ZZE_Value != null)
						{
							code.RefCusCodeListAttributes = new RefCusCodeListAttribute[] { attribute };
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
	}
}
