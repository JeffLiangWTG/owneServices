using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public sealed class AQISProcessingTypeParser : BaseCMRReferenceDataParser
	{
		protected override string FileNamePrefix => ApplicationConfig.AQISProcessingTypeFilePrefix;

		protected override string OutputXMLName => "RefCusCodeList_AU_AQISProcessingType.xml";

		protected override string DataSource => "AU AQIS Processing Type";

		const bool IsAutoExpiryOn = true;

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate)
		{
			var defaultStartDate = CMRXMLWriterConfigurationBuilder.GetDefaultStartDate(IsAutoExpiryOn, publishedDate);
			return CMRXMLWriterConfigurationBuilder.BuildRefCusCodeListConfiguration(CMRConstants.CodeTypes.CMRPT, defaultStartDate, CMRConstants.CodeListAttributeNames.AQISProcessingCargoType);
		}

		static PropertyMapping<RefCusCodeList>[] CodeListMappings => new PropertyMapping<RefCusCodeList>[]
		{
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Code, 1, 10),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Description, 16, 240)
		};

		static PropertyMapping<RefCusCodeListAttribute>[] CodeListAttributeMappings => new PropertyMapping<RefCusCodeListAttribute>[]
		{
			new PropertyMapping<RefCusCodeListAttribute>(entity => entity.ZZE_Value, 12, 3),
		};

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			var codeListConverter = new LineToEntityConverter<RefCusCodeList>(CodeListMappings, 1);
			var attributeConverter = new LineToEntityConverter<RefCusCodeListAttribute>(CodeListAttributeMappings, 1);

			using (var reader = new StringReader(content))
			{
				var line = string.Empty;
				var cusCodeDic = new Dictionary<string, RefCusCodeList>();
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						var code = codeListConverter.Convert(line);
						var attribute = attributeConverter.Convert(line);
						if (code.ZZD_Code != null && attribute.ZZE_Value != null)
						{
							if (cusCodeDic.TryGetValue(code.ZZD_Code, out var refCusCodeList))
							{
								refCusCodeList.RefCusCodeListAttributes = refCusCodeList.RefCusCodeListAttributes.Concat(new RefCusCodeListAttribute[] { attribute }).ToArray();
							}
							else
							{
								code.RefCusCodeListAttributes = new RefCusCodeListAttribute[] { attribute };
								cusCodeDic.Add(code.ZZD_Code, code);
							}
						}
						else
						{
							Console.Error.WriteLine(InsufficientInfoErrorMessage, line);
						}
					}
				}

				foreach (var cusCode in cusCodeDic.Values)
				{
					xmlWriter.PopulateData(cusCode);
				}
			}
		}
	}
}
