using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Services.ReferenceCodes;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.ReferenceCodes
{
	public static class ReferenceCodesParser
	{
		public static string ConvertToXmlFile(referanserListe xmlData, string lastUpdated, string codeType, string outputFileWithPath)
		{
			_ = xmlData ?? throw new ArgumentNullException(nameof(xmlData));
			ErrorBuilder.Clear();
			var writerConfiguration = GetRefExchangeRateWriterConfiguration(codeType);
			var result = new List<RefCusCodeList>();

			var groupedRefCodes = xmlData.Reference.GroupBy(x =>
			(
				x.DateStart,
				x.DateEnd,
				x.Code,
				x.Description
			));

			foreach (var refCode in groupedRefCodes)
			{
				var (valid, cusCodeZz) = Convert(refCode.Key, codeType);
				if (valid)
				{
					result.Add(cusCodeZz);
				}
			}

			FileHelper.ExportToXMLFile($"NO ReferenceCodes {codeType}", outputFileWithPath, writerConfiguration, DataHelpers.GetModifiedDateTime(lastUpdated, ErrorBuilder), result);
			return ErrorBuilder.ToString();
		}

		static (bool Valid, RefCusCodeList CusCodeListZZ) Convert((string StartDate, string EndDate, string Code, string Description) referenceCode, string codeType)
		{
			var code = referenceCode.Code;
			var description = referenceCode.Description;
			var (startDateOk, startDate) = referenceCode.StartDate.TryParseDateTime();
			var (endDateOk, endDate) = referenceCode.EndDate.TryParseEndDateTime();

			if (startDateOk && endDateOk && startDate < endDate && !string.IsNullOrEmpty(code) && !string.IsNullOrEmpty(description))
			{
				return (true, new RefCusCodeList
				{
					ZZD_Code = code,
					ZZD_Description = description,
					ZZD_StartDate = startDate,
					ZZD_EndDate = endDate,
				});
			}

			ErrorBuilder.AppendLine("Unable to parse Reference code due to empty code, empty description or invalid Dates.");
			ErrorBuilder.AppendLine("DETAILS:");
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Code Type: {0}", codeType).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Code: {0}", code).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Description: {0}", description).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Start Date: {0}", referenceCode.StartDate).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "End Date: {0}", referenceCode.EndDate).AppendLine();
			return (false, null);
		}

		static XmlWriterConfiguration GetRefExchangeRateWriterConfiguration(string codeType)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var referenceCodeConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			referenceCodeConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCodes.Norway);
			referenceCodeConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, isKeyColumn: true, codeType);
			referenceCodeConfiguration.IncludeColumn(x => x.ZZD_Code, isKeyColumn: true);
			referenceCodeConfiguration.IncludeColumn(x => x.ZZD_Description, isKeyColumn: false);
			referenceCodeConfiguration.IncludeColumn(x => x.ZZD_StartDate, isKeyColumn: false);
			referenceCodeConfiguration.IncludeColumn(x => x.ZZD_EndDate, isKeyColumn: false);
			writerConfiguration.IncludeEntityTypeConfiguration(referenceCodeConfiguration);
			return writerConfiguration;
		}

		static StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		static StringBuilder errorBuilder;
	}
}
