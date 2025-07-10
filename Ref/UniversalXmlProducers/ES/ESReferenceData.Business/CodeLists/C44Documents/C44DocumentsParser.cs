using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CsvHelper;

namespace CargoWise.RefDbRepo.ESReferenceData.Business
{
	public class C44DocumentsParser : CommonParser
	{
		public C44DocumentsParser(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider) { }

		public string ConvertRecordsToXMLFile(IEnumerable<(string Code, string Description)> documents,
											(string, string, string, string, string) nctsDocuments,
											string outPutFileWithPath)
		{
			ErrorBuilder.Clear();
			processedCodes = new HashSet<string>();
			var result = GetC44DocumentsToExport(documents, nctsDocuments);
			
			if (result.Count > 0)
			{
				Helper.ExportToXMLFile(Constants.DataSources.DC44_Codes, outPutFileWithPath, XMLWriterConfiguration, DateTimeProvider.CurrentLocalDate, result);
			}
			else
			{
				ErrorBuilder.AppendLine("Unable to locate any records for C44 Documents. File may only contain header record");
			}

			return ErrorBuilder.ToString();
		}

		List<RefCusCodeList> GetC44DocumentsToExport(IEnumerable<(string Code, string Description)> documents, (string, string, string, string, string) nctsDocuments)
		{
			var result = new List<RefCusCodeList>();

			foreach (var (code, description) in documents)
			{
				if (CheckDataIsValid(code, description))
				{
					AddToRefList(result, code, description, CodeListsConstants.Import.CodeTypes.IMPORT_C44_DOCUMENTS);
					AddToRefList(result, code, description, CodeListsConstants.Export.CodeTypes.EXPORT_C44_DOCUMENTS);
				}
			}

			result.AddRange(GetC44DocumentsToExportNCTS(nctsDocuments));

			return result;
		}

		List<RefCusCodeList> GetC44DocumentsToExportNCTS((string csvCSRDT213, string csvTRSUPNAC, string csvTRSUPNCA, string csvTRSUPNHO, string csvTRSUPNPA) nctsDocuments)
		{
			processedCodes = new HashSet<string>();
			var result = new List<RefCusCodeList>();

			var headerExclusionSet = GetHashSetForNctsCsv(nctsDocuments.csvTRSUPNCA);
			var houseExclusionSet = GetHashSetForNctsCsv(nctsDocuments.csvTRSUPNHO);
			var itemExclusionSet = GetHashSetForNctsCsv(nctsDocuments.csvTRSUPNPA);

			if (headerExclusionSet.Count == 0
				&& houseExclusionSet.Count == 0
				&& itemExclusionSet.Count == 0)
			{
				ErrorBuilder.AppendLine("Customs has returned empty CSV for all exclusion documents.");
			}

			var recordsForCodeList = new List<C44DocumentItem>();
			recordsForCodeList.AddRange(GetRecordsFromCsv(nctsDocuments.csvCSRDT213));
			recordsForCodeList.AddRange(GetRecordsFromCsv(nctsDocuments.csvTRSUPNAC));

			foreach (var record in recordsForCodeList)
			{
				var code = record.Code;
				var description = record.Description;
				if (CheckDataIsValid(code, description))
				{
					AddToRefList(result, code, description, CodeListsConstants.Ncts.CodeTypes.NCTS_C44_DOCUMENTS, GetRefCusCodeListAttributes(code));
				}
			}
			return result;

			RefCusCodeListAttribute[] GetRefCusCodeListAttributes(string code)
			{
				var cusCodeListAttributes = new List<RefCusCodeListAttribute>();

				void AddAttribute(string name, string attribute)
				{
					cusCodeListAttributes.Add(new RefCusCodeListAttribute { ZZE_ZXE_NKName = name, ZZE_Value = attribute });
				}

				var levelName = CodeListsConstants.RefCusCodeListAttributeNames.LEVEL;

				if (!headerExclusionSet.Contains(code))
					AddAttribute(levelName, CodeListsConstants.RefCusCodeListAttributeValues.LEVEL_HEADER);

				if (!houseExclusionSet.Contains(code))
					AddAttribute(levelName, CodeListsConstants.RefCusCodeListAttributeValues.LEVEL_HOUSE);

				if (!itemExclusionSet.Contains(code))
					AddAttribute(levelName, CodeListsConstants.RefCusCodeListAttributeValues.LEVEL_ITEM);

				AddAttribute(CodeListsConstants.RefCusCodeListAttributeNames.COMPLEMENT, CodeListsConstants.RefCusCodeListAttributeValues.No);
				AddAttribute(CodeListsConstants.RefCusCodeListAttributeNames.ITEM_NUMBER, CodeListsConstants.RefCusCodeListAttributeValues.No);
				AddAttribute(CodeListsConstants.RefCusCodeListAttributeNames.REFERENCE, CodeListsConstants.RefCusCodeListAttributeValues.Yes);

				return cusCodeListAttributes.ToArray();
			}
		}

		static HashSet<string> GetHashSetForNctsCsv(string inputCsvString)
		{
			var result = new HashSet<string>();
			if (!inputCsvString.Contains("<html", StringComparison.InvariantCulture))
			{
				var records = GetRecordsFromCsv(inputCsvString);
				foreach (var record in records)
				{
					result.Add(record.Code);
				}
			}
			return result;
		}

		static List<C44DocumentItem> GetRecordsFromCsv(string inputCsvString)
		{
			var itemList = new List<C44DocumentItem>();
			using (TextReader reader = new StringReader(inputCsvString))
			using (var csvReader = new CsvReader(reader))
			{
				csvReader.Configuration.Delimiter = ";";
				csvReader.Configuration.MissingFieldFound = null;
				csvReader.Configuration.BadDataFound = null;
				csvReader.Configuration.RegisterClassMap<C44DocumentItemMap>();
				while (csvReader.Read())
				{
					var record = csvReader.GetRecord<C44DocumentItem>();
					if (record != null && !(string.IsNullOrEmpty(record.Code) && string.IsNullOrEmpty(record.Description)))
					{
						itemList.Add(record);
					}
				}
			}
			return itemList;
		}

		static void AddToRefList(List<RefCusCodeList> result, string code, string description, string codeType, RefCusCodeListAttribute[] refCusCodeListAttributes = null)
		{
			result.Add(new RefCusCodeList
			{
				ZZD_Code = code,
				ZZD_Description = description,
				ZZD_ZZK_NKCodeType = codeType,
				RefCusCodeListAttributes = refCusCodeListAttributes
			});
		}

		bool CheckDataIsValid(string code, string description)
		{
			var result = true;
			if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(description) || processedCodes.Contains(code))
			{
				ErrorBuilder.AppendLine("Unable to import C44 Document as missing or duplicated attribute 'code' or 'description'. Details:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {code}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {description}");
				result = false;
			}
			else
			{
				processedCodes.Add(code);
			}

			return result;
		}
		HashSet<string> processedCodes;

		protected override XmlWriterConfiguration XMLWriterConfiguration
		{
			get
			{
				var writerConfiguration = new XmlWriterConfiguration();
				var codeListConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
				codeListConfiguration.IncludeColumn(x => x.ZZD_ZZK_NKCodeType, true);
				codeListConfiguration.IncludeColumn(x => x.ZZD_Code, true);
				codeListConfiguration.IncludeColumn(x => x.ZZD_Description, false);
				codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
				codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
				codeListConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.CountryCode);
				writerConfiguration.IncludeEntityTypeConfiguration(codeListConfiguration);

				codeListConfiguration.IncludeColumn(x => x.RefCusCodeListAttributes, false);
				var codelistAttributeConfiguration = new EntityTypeConfiguration<RefCusCodeListAttribute>(true);
				codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_ZXE_NKName, true);
				codelistAttributeConfiguration.IncludeColumn(x => x.ZZE_Value, true);
				writerConfiguration.IncludeEntityTypeConfiguration(codelistAttributeConfiguration);

				return writerConfiguration;
			}
		}
	}
}
