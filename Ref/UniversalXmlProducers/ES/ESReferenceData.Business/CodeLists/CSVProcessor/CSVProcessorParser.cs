using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CsvHelper;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class CSVProcessorParser<T, TMap> : CommonParser, ICSVProcessorParser
						where T : CSVProcessorItem
						where TMap : CsvHelper.Configuration.ClassMap<T>
	{
		public CSVProcessorParser(IDateTimeProvider dateTimeProvider, string dateFormat, string codeType) : base(dateTimeProvider)
		{
			this.dateFormat = dateFormat;
			this.codeType = codeType;
		}
		readonly string dateFormat;
		readonly string codeType;

		public string ConvertRecordsToXMLFile(string inputCsvFilePath, string outputXmlFilePath, string dataSource)
		{
			ErrorBuilder.Clear();
			var records = GetRecordsFromCsv(inputCsvFilePath);
			if (records.Count > 0)
			{
				var result = PopulateRefCusCodeList(records);

				Helper.ExportToXMLFile(dataSource, outputXmlFilePath, XMLWriterConfiguration, DateTimeProvider.CurrentLocalDate, result);
			}
			else
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Unable to locate any CSVProcessor Item records for CSVProcessor CSV. File may only contain header record. Input file details: {inputCsvFilePath}");
			}
			return ErrorBuilder.ToString();
		}

		List<RefCusCodeList> PopulateRefCusCodeList(List<T> records)
		{
			var result = new List<RefCusCodeList>();

			foreach (var record in records)
			{
				var code = record.Code;
				var description = record.Description;
				var startDate = Helper.GetDateTime(record.StartDate, dateFormat);
				var endDate = Helper.GetDateTime(record.EndDate, dateFormat);
				if (CheckDataIsValid(code, description, startDate.SuccessfullyParsed, record.StartDate))
				{
					DateTime? endDateValue = null;
					if (endDate.SuccessfullyParsed)
					{
						endDateValue = endDate.DateTime;
					}
					var levelAttributes = GetLevelAttributes(record);
					var otherAttributes = GetOtherAttributes(record);
					AddToRefList(result, code, description, startDate.DateTime, endDateValue, levelAttributes, otherAttributes);
				}
			}

			return result;
		}

		static string[] GetLevelAttributes(T record)
		{
			var levelAttributes = new List<string>();

			void AddLevelAttribute(string currentAttribute, string codeInDatabase)
			{
				if (BooleanAttributeValueIsYes(currentAttribute))
				{
					levelAttributes.Add(codeInDatabase);
				}
			}

			if (record is CSVProcessorAttributesItem recordWithAttributes)
			{
				AddLevelAttribute(recordWithAttributes.HeaderLevel, CodeListsConstants.RefCusCodeListAttributeValues.LEVEL_HEADER);
				AddLevelAttribute(recordWithAttributes.HouseLevel, CodeListsConstants.RefCusCodeListAttributeValues.LEVEL_HOUSE);
				AddLevelAttribute(recordWithAttributes.ItemLevel, CodeListsConstants.RefCusCodeListAttributeValues.LEVEL_ITEM);
			}
			return levelAttributes.ToArray();
		}

		static RefCusCodeListAttribute[] GetOtherAttributes(T record)
		{
			var attributes = new List<RefCusCodeListAttribute>();

			if (record is CSVProcessorAttributesItem recordWithAttributes)
			{
				AddAttribute(CodeListsConstants.RefCusCodeListAttributeNames.REFERENCE, recordWithAttributes.Reference);
				AddAttribute(CodeListsConstants.RefCusCodeListAttributeNames.ITEM_NUMBER, recordWithAttributes.ItemNumber);
				AddAttribute(CodeListsConstants.RefCusCodeListAttributeNames.COMPLEMENT, recordWithAttributes.Complement);
			}

			if (record is CSVProcessorNationalAttributesItem recordWithNationalAttributes)
			{
				if (BooleanAttributeValueIsYes(recordWithNationalAttributes.National))
				{
					AddAttribute(CodeListsConstants.RefCusCodeListAttributeNames.IS_NATIONAL, CodeListsConstants.RefCusCodeListAttributeValues.Yes);
				}

				if (BooleanAttributeValueIsYes(recordWithNationalAttributes.UE))
				{
					AddAttribute(CodeListsConstants.RefCusCodeListAttributeNames.IS_NATIONAL, CodeListsConstants.RefCusCodeListAttributeValues.No);
				}
			}

			return attributes.ToArray();

			void AddAttribute(string name, string value)
			{
				attributes.Add(new RefCusCodeListAttribute { ZZE_ZXE_NKName = name, ZZE_Value = value });
			}
		}

		static bool BooleanAttributeValueIsYes(string attributeValue)
		{
			const string Yes = "Y";
			return string.Equals(attributeValue, Yes, StringComparison.Ordinal);
		}

		bool CheckDataIsValid(string code, string description, bool startDateSuccessfullyParsed, string startDateString)
		{
			var result = true;
			if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(description) || !startDateSuccessfullyParsed)
			{
				ErrorBuilder.AppendLine("Unable to import CSVProcessor record due to empty 'code', 'description', 'start date'. Details:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {code}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {description}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Start Date: {startDateString}");
				result = false;
			}
			return result;
		}

		static void AddToRefList(List<RefCusCodeList> result, string code, string description, DateTime startDate, DateTime? endDate, string[] levelAttributes, RefCusCodeListAttribute[] otherAttributes)
		{
			var codeList = new RefCusCodeList { ZZD_Code = code, ZZD_Description = description, ZZD_StartDate = startDate };
			if (endDate != null)
			{
				codeList.ZZD_EndDate = endDate.Value;
			}

			var cusCodeListAttributes = new List<RefCusCodeListAttribute>();
			foreach (var levelAttribute in levelAttributes)
			{
				cusCodeListAttributes.Add(new RefCusCodeListAttribute { ZZE_ZXE_NKName = CodeListsConstants.RefCusCodeListAttributeNames.LEVEL, ZZE_Value = levelAttribute });
			}
			cusCodeListAttributes.AddRange(otherAttributes);
			codeList.RefCusCodeListAttributes = cusCodeListAttributes.ToArray();

			result.Add(codeList);
		}

		protected override XmlWriterConfiguration XMLWriterConfiguration
		{
			get
			{
				var writerConfiguration = new XmlWriterConfiguration();
				var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
				codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
				codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
				codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
				codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
				codeListConfiguration.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
				codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.CountryCode);
				writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

				if (typeof(T) == typeof(CSVProcessorAttributesItem) || typeof(T) == typeof(CSVProcessorNationalAttributesItem))
				{
					codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);
					var codelistAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
					codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
					codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, true);
					writerConfiguration.IncludeEntityTypeConfiguration(codelistAttributeConfiguration);
				}

				return writerConfiguration;
			}
		}

		static List<T> GetRecordsFromCsv(string inputCsvString)
		{
			using (TextReader reader = new StringReader(inputCsvString))
			using (var csvReader = new CsvReader(reader))
			{
				csvReader.Configuration.Delimiter = ";";
				csvReader.Configuration.MissingFieldFound = null;
				csvReader.Configuration.RegisterClassMap<TMap>();
				return new List<T>(csvReader.GetRecords<T>());
			}
		}
	}
}
