using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.IEReferenceData.Business;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.ROSErrors.Business
{
	public class ROSErrorsListProducer
	{
		public string ConvertToXmlFile(IReadOnlyDictionary<string, string> errorCodeList, DateTime publicationDateTime, string outputPath)
		{
			var CodeType = Constants.AESCodeTypes.ROSErrorListType;
			ErrorBuilder.Clear();
			if (!string.IsNullOrEmpty(outputPath))
			{
				var refCusCodeLists = PopulateRefCusCodeList(errorCodeList);
				if (refCusCodeLists.Count > 0)
				{
					Helper.ExportToXmlFile($"{Constants.IECountryCode} {CodeType}", Path.Combine(outputPath, $"RefCusCodeListZZ_{Constants.IECountryCode}_{CodeType}.xml"), GetWriterConfiguration(CodeType), publicationDateTime, refCusCodeLists);
				}
				else
				{
					ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"There were no valid records for {CodeType}, unable to generate XML file.");
				}
			}
			else
			{
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Output file path is empty for Producer of type {CodeType}");
			}
			return ErrorBuilder.ToString();
		}

		List<RefCusCodeList> PopulateRefCusCodeList(IReadOnlyDictionary<string, string> codeDescriptionPairs)
		{
			var result = new List<RefCusCodeList>();
			foreach (var pair in codeDescriptionPairs)
			{
				if (CheckDataIsValid(pair.Key, pair.Value))
				{
					var refCusCodeList = new RefCusCodeList()
					{
						ZZD_Code = pair.Key,
						ZZD_Description = pair.Value
					};

					result.Add(refCusCodeList);
				}
			}
			return result;
		}

		bool CheckDataIsValid(string key, string value)
		{
			var result = !string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value);
			if (!result)
			{
				ErrorBuilder.AppendLine($"Unable to import record due to empty Code or Description. DETAILS:");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Code: {(string.IsNullOrWhiteSpace(key) ? "EMPTY" : key)}");
				ErrorBuilder.AppendLine(CultureInfo.InvariantCulture, $"Description: {(string.IsNullOrWhiteSpace(value) ? "EMPTY" : key)}");
			}
			return result;
		}

		StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		StringBuilder errorBuilder;

		static XmlWriterConfiguration GetWriterConfiguration(string codeType)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var refCusCodeList = new EntityTypeConfiguration<RefCusCodeList>(true);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, true, codeType);
			refCusCodeList.IncludeColumn(x => x.ZZD_Code, true);
			refCusCodeList.IncludeColumn(x => x.ZZD_Description, false);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_StartDate, false, Constants.MinimumDateTime);
			refCusCodeList.IncludeColumnWithDefaultValue(x => x.ZZD_EndDate, false, Constants.MaximumDateTime);
			refCusCodeList.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, true, Constants.IECountryCode);

			writerConfiguration.IncludeEntityTypeConfiguration(refCusCodeList);
			return writerConfiguration;
		}
	}
}
