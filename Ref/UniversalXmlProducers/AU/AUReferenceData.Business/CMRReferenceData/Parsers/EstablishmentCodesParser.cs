using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.AUReferenceData.Business
{
	public sealed class EstablishmentCodesParser : BaseCMRReferenceDataParser
	{
		protected override string FileNamePrefix => ApplicationConfig.EstablishmentCodesFilePrefix;

		protected override string OutputXMLName => "RefCusCodeList_AU_EstablishmentCodes.xml";

		protected override string DataSource => "AU Establishment Codes";

		const bool IsAutoExpiryOn = true;

		protected override IXmlWriterConfiguration BuildXMLConfiguration(DateTime publishedDate)
		{
			var defaultStartDate = CMRXMLWriterConfigurationBuilder.GetDefaultStartDate(IsAutoExpiryOn, publishedDate);
			return CMRXMLWriterConfigurationBuilder.BuildRefCusCodeListConfiguration(CMRConstants.CodeTypes.CMREC, defaultStartDate, CMRConstants.CodeListAttributeNames.EstablishmentPremisesIndicator);
		}

		static PropertyMapping<RefCusCodeList>[] CodeListMappings => new PropertyMapping<RefCusCodeList>[]
		{
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Code, 1, 5),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_Description, 55, 80),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_StartDate, 7, 8),
			new PropertyMapping<RefCusCodeList>(entity => entity.ZZD_EndDate, 16, 8, Constants.RefData_Common.MaximumDateTime)
		};

		static PropertyMapping<RefCusCodeListAttribute>[] CodeListAttributeMappings_Type => new PropertyMapping<RefCusCodeListAttribute>[]
		{
			new PropertyMapping<RefCusCodeListAttribute>(entity => entity.ZZE_Value, 25, 10)
		};

		static PropertyMapping<RefCusCodeListAttribute>[] CodeListAttributeMappings_SubType => new PropertyMapping<RefCusCodeListAttribute>[]
		{
			new PropertyMapping<RefCusCodeListAttribute>(entity => entity.ZZE_Value, 36, 10),
		};

		static PropertyMapping<RefCusCodeListAttribute>[] CodeListAttributeMappings_PortCode => new PropertyMapping<RefCusCodeListAttribute>[]
		{
			new PropertyMapping<RefCusCodeListAttribute>(entity => entity.ZZE_Value, 47, 5),
		};

		static PropertyMapping<RefCusCodeListAttribute>[] CodeListAttributeMappings_PremisesIndicator => new PropertyMapping<RefCusCodeListAttribute>[]
		{
			new PropertyMapping<RefCusCodeListAttribute>(entity => entity.ZZE_Value, 53, 1),
		};

		protected override void ParseCore(IXmlWriter xmlWriter, string content)
		{
			var codeListConverter = new LineToEntityConverter<RefCusCodeList>(CodeListMappings, 1);
			var AttributeConverter_Type = new LineToEntityConverter<RefCusCodeListAttribute>(CodeListAttributeMappings_Type, 1);
			var AttributeConverter_SubType = new LineToEntityConverter<RefCusCodeListAttribute>(CodeListAttributeMappings_SubType, 1);
			var AttributeConverter_PortCode = new LineToEntityConverter<RefCusCodeListAttribute>(CodeListAttributeMappings_PortCode, 1);
			var AttributeConverter_PremisesIndicator = new LineToEntityConverter<RefCusCodeListAttribute>(CodeListAttributeMappings_PremisesIndicator, 1);

			using (var reader = new StringReader(content))
			{
				var line = string.Empty;
				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line))
					{
						var code = codeListConverter.Convert(line);

						if (code.ZZD_Code != null && code.ZZD_Description != null)
						{
							if (DateTime.Compare(code.ZZD_StartDate, code.ZZD_EndDate) <= 0)
							{
								var attributesList = new List<RefCusCodeListAttribute>();
								var attribute_Type = AttributeConverter_Type.Convert(line);
								var attribute_SubType = AttributeConverter_SubType.Convert(line);
								var attribute_PortCode = AttributeConverter_PortCode.Convert(line);
								var attribute_PremisesIndicator = AttributeConverter_PremisesIndicator.Convert(line);

								if (!string.IsNullOrEmpty(attribute_Type.ZZE_Value))
								{
									attribute_Type.ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.EstablishmentType;
									attributesList.Add(attribute_Type);
								}

								if (!string.IsNullOrEmpty(attribute_SubType.ZZE_Value))
								{
									attribute_SubType.ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.EstablishmentSubType;
									attributesList.Add(attribute_SubType);
								}

								if (!string.IsNullOrEmpty(attribute_PortCode.ZZE_Value))
								{
									attribute_PortCode.ZZE_ZXE_NKName = CMRConstants.CodeListAttributeNames.EstablishmentPortCode;
									attributesList.Add(attribute_PortCode);
								}

								if (!string.IsNullOrEmpty(attribute_PremisesIndicator.ZZE_Value))
								{
									attributesList.Add(attribute_PremisesIndicator);
								}

								code.RefCusCodeListAttributes = attributesList?.ToArray();

								xmlWriter.PopulateData(code);
							}
							else
							{
								Console.Error.WriteLine(InvalidTimeErrorMessage, line);
							}
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
