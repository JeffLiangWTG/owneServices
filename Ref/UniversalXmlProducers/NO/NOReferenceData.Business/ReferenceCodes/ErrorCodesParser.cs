using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.NOReferenceData.Services.ErrorCodes;

namespace CargoWise.RefDbRepo.NOReferenceData.Business.ReferenceCodes
{
	public static class ErrorCodesParser
	{
		public static string ConvertToXmlFile(FeilmeldingListe xmlData, string lastUpdated, string codeType, string outputFileWithPath)
		{
			_ = xmlData ?? throw new ArgumentNullException(nameof(xmlData));
			ErrorBuilder.Clear();

			var result = new List<RefCusCodeList>();

			var errorCodes = xmlData.ErrorMessage.GroupBy(x => (x.MessageNumber, x.MessageText));
			foreach (var singleErrorCode in errorCodes)
			{
				var refCusCode = Convert(singleErrorCode.Key, codeType);
				if (refCusCode != null)
				{
					result.Add(refCusCode);
				}
			}

			if (result.Any())
			{
				var writerConfiguration = GetRefCusCodeListWriterConfiguration(codeType);
				FileHelper.ExportToXMLFile($"NO ReferenceCodes {codeType}", outputFileWithPath, writerConfiguration, DataHelpers.GetModifiedDateTime(lastUpdated, ErrorBuilder), result);
			}
			return ErrorBuilder.ToString();
		}

		static RefCusCodeList Convert((string messageNumber, string messageText) errorCode, string codeType)
		{
			var errNo = errorCode.messageNumber;
			var description = errorCode.messageText;

			if (!string.IsNullOrEmpty(errNo) && !string.IsNullOrEmpty(description))
			{
				return new RefCusCodeList
				{
					ZZD_Code = errNo,
					ZZD_Description = description
				};
			}

			ErrorBuilder.AppendLine("Unable to parse Error Code due to empty code or empty description.");
			ErrorBuilder.AppendLine("DETAILS:");
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Code Type: {0}", codeType).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Error No: {0}", errNo).AppendLine();
			ErrorBuilder.AppendFormat(CultureInfo.InvariantCulture, "Description: {0}", description).AppendLine();
			return null;
		}

		static XmlWriterConfiguration GetRefCusCodeListWriterConfiguration(string codeType)
		{
			var writerConfiguration = new XmlWriterConfiguration();
			var referenceCodeConfiguration = new EntityTypeConfiguration<RefCusCodeList>(true);
			referenceCodeConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZZ_NKDataGrouping, isKeyColumn: true, Constants.CountryCodes.Norway);
			referenceCodeConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_ZZK_NKCodeType, isKeyColumn: true, codeType);
			referenceCodeConfiguration.IncludeColumn(x => x.ZZD_Code, isKeyColumn: true);
			referenceCodeConfiguration.IncludeColumn(x => x.ZZD_Description, isKeyColumn: false);
			referenceCodeConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_StartDate, isKeyColumn: false, Constants.MinimumDateTime);
			referenceCodeConfiguration.IncludeColumnWithConstantValue(x => x.ZZD_EndDate, isKeyColumn: false, Constants.MaximumDateTime);
			writerConfiguration.IncludeEntityTypeConfiguration(referenceCodeConfiguration);
			return writerConfiguration;
		}

		static StringBuilder ErrorBuilder => errorBuilder ?? (errorBuilder = new StringBuilder());
		static StringBuilder errorBuilder;
	}
}
