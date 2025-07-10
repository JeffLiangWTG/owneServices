using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.EntityFrameworkCore;

namespace CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists
{
	public static class CodeListsHelper
	{
		public static XmlWriterConfiguration GetRefCusCodeListWriterConfiguration(string codeType)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			DefaultRefCusCodeListWriterConfiguration(writerConfiguration, new EntityTypeConfiguration<RefCusCodeList>(true), codeType);
			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeListWriterConfigurationWithAttributes(bool isValueKeyColumn, string codeType, string dataGrouping = "", bool enableAttributeDates = false)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			DefaultRefCusCodeListWriterConfiguration(writerConfiguration, codeListConfiguration, codeType, dataGrouping);

			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);
			var codelistAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true, enableExpirable: enableAttributeDates);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
			codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, isValueKeyColumn);
			if (enableAttributeDates)
			{
				codelistAttributeConfiguration.IncludeColumnWithDefaultValue(x => x.ZZE_StartDate, false, Constants.MinimumDateTime);
				codelistAttributeConfiguration.IncludeColumnWithDefaultValue(x => x.ZZE_EndDate, false, Constants.MaximumDateTime);
			}
			writerConfiguration.IncludeEntityTypeConfiguration(codelistAttributeConfiguration);

			return writerConfiguration;
		}

		public static XmlWriterConfiguration GetRefCusCodeListLanguageWriterConfiguration(string codeType, string dataGrouping)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(false);

			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, dataGrouping);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

			codeListConfiguration.IncludeColumn(x => x.RefCusCodeListLanguages, false);
			var codelistWithGermanLanguageConfiguration = new EntityTypeConfiguration<RefCusCodeListLanguage>(true);
			codelistWithGermanLanguageConfiguration.IncludeColumnWithConstantValue(x => x.ZXA_ZX6_NKLanguage, true, CodeListsConstants.Export.CodeListsLanguageConstantValue);
			codelistWithGermanLanguageConfiguration.IncludeColumn(x => x.ZXA_Description, false);
			writerConfiguration.IncludeEntityTypeConfiguration(codelistWithGermanLanguageConfiguration);

			return writerConfiguration;
		}

		public static void AddToAttributes(IList<KeyValueAttribute> result, string key, string value) => AddToAttributes(result, key, value, (x) => true);

		public static void AddToAttributes(IList<KeyValueAttribute> result, string key, string value, DateTime startDate, DateTime endDate) => AddToAttributes(result, key, value, (x) => true, startDate, endDate);

		public static void AddToAttributes(IList<KeyValueAttribute> result, string key, string value, Func<string, bool> extendedValueConditionToAdd)
		{
			AddToAttributes(result, key, value, extendedValueConditionToAdd, Constants.MinimumDateTime, Constants.MinimumDateTime);
		}

		public static void AddToAttributes(IList<KeyValueAttribute> result, string key, string value, Func<string, bool> extendedValueConditionToAdd, DateTime startDate, DateTime endDate)
		{
			if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value) && extendedValueConditionToAdd(value))
			{
				result.Add(new KeyValueAttribute { Key = key, Value = value, StartDate = startDate, EndDate = endDate });
			}
		}

		public static void AddAllowedToAttributesAsBool(IList<KeyValueAttribute> result, string name, string value)
		{
			AddAllowedToAttributesAsBool(result, name, value, Constants.MinimumDateTime, Constants.MaximumDateTime);
		}

		public static void AddAllowedToAttributesAsBool(IList<KeyValueAttribute> result, string name, string value, DateTime startDate, DateTime endDate)
		{
			if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(value) && value != CodeListsConstants.AttributeValues.NotAllowed)
			{
				var attribute = new KeyValueAttribute { Key = name, Value = GetBoolAsStringFromAttributeValue(), StartDate = startDate, EndDate = endDate };
				result.Add(attribute);
			}

			string GetBoolAsStringFromAttributeValue() => value == CodeListsConstants.AttributeValues.Required ? CodeListsConstants.AttributeValues.Yes : CodeListsConstants.AttributeValues.No;
		}

		public static void CreateOrUpdateRefCusCodeListWithLevelAttribute(List<RefCusCodeList> result, IEnumerable<IKeyValuesWithAttributes> keyValues, string levelAttributeValue)
		{
			foreach (var keyValuesRecord in keyValues)
			{
				var levelAttributes = keyValuesRecord.Attributes.Where(a =>
					a.Key == CodeListsConstants.XMLEntryElementNames.LEVEL_ITEM || a.Key == CodeListsConstants.XMLEntryElementNames.LEVEL_HEADER ||
					a.Key == CodeListsConstants.XMLEntryElementNames.LEVEL_HOUSE).ToArray();

				var existingRecord = result.SingleOrDefault(x => x.ZZD_Code == keyValuesRecord.Code);
				if (existingRecord == null)
				{
					// One of these will be added to the dictionary when GetKeyValues() is called, even if it's not needed
					foreach (var levelAttribute in levelAttributes)
					{
						levelAttribute.Key = LevelAttributeKeyConverter.GetKeyFromValue(levelAttributeValue);
						levelAttribute.Value = levelAttributeValue;
					}

					result.Add(CreateRefListWithAttributes(keyValuesRecord));
				}
				else
				{
					var attributes = existingRecord.RefCusCodeListAttributes;
					foreach (var levelAttribute in levelAttributes)
					{
						attributes = attributes.Append(new RefCusCodeListAttribute
						{
							ZZE_ZXE_NKName = CodeListsConstants.XMLEntryElementNames.LEVEL,
							ZZE_Value = levelAttributeValue,
							ZZE_StartDate = levelAttribute.StartDate,
							ZZE_EndDate = levelAttribute.EndDate,
						}).ToArray();
					}

					existingRecord.RefCusCodeListAttributes = attributes;
				}
			}
		}

		public static void UpdateRefCusCodeListWithObligationAttribute(List<RefCusCodeList> result, IEnumerable<IKeyValuesWithAttributes> keyValues)
		{
			foreach (var keyValuesRecord in keyValues)
			{
				var obligationAttribute = keyValuesRecord.Attributes.SingleOrDefault(a => a.Key == CodeListsConstants.XMLEntryElementNames.OBLIGATION);
				if (obligationAttribute?.Value != null)
				{
					var existingRecord = result.SingleOrDefault(x => x.ZZD_Code == keyValuesRecord.Code);
					if (existingRecord != null)
					{
						var attributes = existingRecord.RefCusCodeListAttributes.Append(new RefCusCodeListAttribute { ZZE_ZXE_NKName = CodeListsConstants.XMLEntryElementNames.OBLIGATION, ZZE_Value = obligationAttribute.Value }).ToArray();
						existingRecord.RefCusCodeListAttributes = attributes;
					}
				}
			}
		}

		public static void GetBaseErrorDetailsXML(StringBuilder detailsStringBuilder, IKeyValues keyValues, XElement entry, string codeName)
		{
			detailsStringBuilder.AppendLine("DETAILS:");
			detailsStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{codeName}: {entry.Element(codeName)?.Value}");
			detailsStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{CodeListsConstants.XMLEntryElementNames.DESCRIPTION}: {keyValues.Description}");
			detailsStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{CodeListsConstants.XMLEntryElementNames.START_DATE}: {entry.Element(CodeListsConstants.XMLEntryElementNames.START_DATE)?.Value}");
			detailsStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{CodeListsConstants.XMLEntryElementNames.END_DATE}: {entry.Element(CodeListsConstants.XMLEntryElementNames.END_DATE)?.Value}");
		}

		public static void GetBaseErrorDetailsTSV(StringBuilder detailsStringBuilder, string[] record)
		{
			detailsStringBuilder.AppendLine("DETAILS:");
			detailsStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {record[CodeListsConstants.TSVColumnIndexes.CODE]}");
			detailsStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Qualifikator (Qualifier): {record[CodeListsConstants.TSVColumnIndexes.QUALIFIER]}");
			detailsStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Gültig von (Valid From): {record[CodeListsConstants.TSVColumnIndexes.VALID_FROM]}");
			detailsStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Gültig bis (Valid To): {record[CodeListsConstants.TSVColumnIndexes.VALID_TO]}");
			detailsStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Beschreibung (Description): {record[CodeListsConstants.TSVColumnIndexes.DESCRIPTION]}");
		}

		public static RefCusCodeList CreateRefList(IKeyValues keyValues)
		{
			return new RefCusCodeList
			{
				ZZD_Code = keyValues.Code,
				ZZD_Description = keyValues.Description,
				ZZD_StartDate = keyValues.StartDate,
				ZZD_EndDate = keyValues.EndDate
			};
		}

		public static RefCusCodeList CreateRefListWithDirectionAttributes(IKeyValues keyValues, bool isExport, bool isImport)
		{
			var result = CreateRefList(keyValues);
			result.RefCusCodeListAttributes = CreateDirectionAttributes(isExport, isImport);
			return result;
		}

		public static RefCusCodeListAttribute[] CreateDirectionAttributes(bool isExport, bool isImport)
		{
			var attributes = new List<RefCusCodeListAttribute>();
			if (isExport)
			{
				attributes.Add(new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = CodeListsConstants.XMLEntryElementNames.DIRECTION,
					ZZE_Value = CodeListsConstants.AttributeValues.Export
				});
			}
			if (isImport)
			{
				attributes.Add(new RefCusCodeListAttribute
				{
					ZZE_ZXE_NKName = CodeListsConstants.XMLEntryElementNames.DIRECTION,
					ZZE_Value = CodeListsConstants.AttributeValues.Import
				});
			}
			return attributes.ToArray();
		}

		public static RefCusCodeList CreateRefListWithAttributes(IKeyValuesWithAttributes keyValues)
		{
			var mappedRefCusCodeListElement = CreateRefList(keyValues);
			var attributes = keyValues.Attributes;
			if (attributes.Count > 0)
			{
				var refCusCodeListsAttributes = new List<RefCusCodeListAttribute>();
				foreach (var attribute in attributes)
				{
					string key = LevelAttributeKeyConverter.ConvertToOutputKey(attribute.Key);

					refCusCodeListsAttributes.Add(new RefCusCodeListAttribute { ZZE_ZXE_NKName = key, ZZE_Value = attribute.Value, ZZE_StartDate = attribute.StartDate, ZZE_EndDate = attribute.EndDate});
				}
				mappedRefCusCodeListElement.RefCusCodeListAttributes = refCusCodeListsAttributes.ToArray();
			}

			return mappedRefCusCodeListElement;
		}

		public static RefCusCodeList CreateRefListLanguage(IKeyValues keyValues)
		{
			var mappedRefCusCodeListElement = CreateRefList(keyValues);
			var refCusCodeListsLanguages = new List<RefCusCodeListLanguage>
			{
				new RefCusCodeListLanguage() { ZXA_Description = keyValues.Description }
			};
			mappedRefCusCodeListElement.RefCusCodeListLanguages = refCusCodeListsLanguages.ToArray();

			return mappedRefCusCodeListElement;
		}

		static void DefaultRefCusCodeListWriterConfiguration(XmlWriterConfiguration writerConfiguration, EntityTypeConfiguration<RefCusCodeList> codeListConfiguration, string codeType, string dataGrouping = "")
		{
			codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
			codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
			if (string.IsNullOrWhiteSpace(codeType))
			{
				codeListConfiguration.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
			}
			else
			{
				codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			}
			codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, string.IsNullOrEmpty(dataGrouping) ? "DE" : dataGrouping);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
			codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
			writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);
		}
	}
}
