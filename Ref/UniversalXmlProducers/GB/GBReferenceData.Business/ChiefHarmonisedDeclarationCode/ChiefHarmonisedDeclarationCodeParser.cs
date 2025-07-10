using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SharedReferenceData.Business;

namespace CargoWise.RefDbRepo.GBReferenceData.Business.ChiefHarmonisedDeclarationCode
{
	public class ChiefHarmonisedDeclarationCodeParser
	{
		public void DownloadAndConvertToRefCusCodeListXML(string outputPath, string temporaryDownloadFile, DateTime publicationDate)
		{
			var list = ChiefHarmonisedDeclarationCodeExtractor.Extract(temporaryDownloadFile);
			var refCusCodeList = GenerateRefCusCodeList(list);
			Helper.ExportToXMLFile("GB Harmonised Declaration Code", Path.Combine(outputPath, "RefCusCodeListZZ_GB_DC44.xml"), Helper.GetRefCusCodeListWriterConfigurationWithAttributes("GB"), publicationDate, UpdateType.Full, refCusCodeList);
		}

		IEnumerable<RefCusCodeList> GenerateRefCusCodeList(IEnumerable<ChiefHarmonisedData> harmonisedDataList)
		{
			var result = new List<RefCusCodeList>();
			foreach (var harmonisedData in harmonisedDataList)
			{
				var refCusCode = new RefCusCodeList
				{
					ZZD_Code = harmonisedData.Code,
					ZZD_Description = harmonisedData.Description
				};

				var refCusCodeListAttributes = GetAttributes(harmonisedData.Level);
				GenerateDetailCodes(harmonisedData.Details, refCusCodeListAttributes);
				AddRefCusCodesToList(harmonisedData, refCusCode, refCusCodeListAttributes.ToArray(), result);
			}
			return result;
		}

		static List<RefCusCodeListAttribute> GetAttributes(string level)
		{
			var levels = level == "BOTH" ? new[] { "HEADER", "ITEM" } : new[] { level };
			var refCusCodeListAttributes = new List<RefCusCodeListAttribute>();
			foreach (var value in levels)
			{
				refCusCodeListAttributes.Add(new RefCusCodeListAttribute()
				{
					ZZE_ZXE_NKName = "Level",
					ZZE_Value = value
				});
			}
			return refCusCodeListAttributes;
		}

		void GenerateDetailCodes(string details, List<RefCusCodeListAttribute> list)
		{
			var statusCodes = new List<string>();
			if (HasStatusCodesInDetails(details))
			{
				var matches = StatusCodes.Matches(details);
				statusCodes = GetValidStatusCodes(matches);
				statusCodes.AddRange(GetSeriesCodesFromDescription(details));
				if (HasExceptionStatusCodesInDetails(details))
				{
					var validStatusCodes = ValidStatusCodes.ToList();
					validStatusCodes.RemoveAll(c => statusCodes.Contains(c));
					statusCodes = validStatusCodes;
				}
			}
			foreach (var code in statusCodes.Distinct().OrderBy(x => x))
			{
				list.Add(new RefCusCodeListAttribute()
				{
					ZZE_ZXE_NKName = "ACTAV",
					ZZE_Value = code
				});
			}
		}

		static void AddRefCusCodesToList(ChiefHarmonisedData harmonisedData, RefCusCodeList refCusCode, RefCusCodeListAttribute[] refCusCodeListAttributes, List<RefCusCodeList> refCusCodeList)
		{
			if (harmonisedData.ImportExport == "BOTH")
			{
				refCusCode.ZZD_ZZK_NKCodeType = "DC44I";
				refCusCode.RefCusCodeListAttributes = refCusCodeListAttributes;
				refCusCodeList.Add(refCusCode);

				var refCusCodeListExport = new RefCusCodeList
				{
					ZZD_Code = harmonisedData.Code,
					ZZD_Description = harmonisedData.Description,
					ZZD_ZZK_NKCodeType = "DC44E"
				};

				refCusCodeListExport.RefCusCodeListAttributes = refCusCodeListAttributes;
				refCusCodeList.Add(refCusCodeListExport);
			}
			else
			{
				refCusCode.ZZD_ZZK_NKCodeType = "DC44" + harmonisedData.ImportExport.Substring(0, 1);
				refCusCode.RefCusCodeListAttributes = refCusCodeListAttributes;
				refCusCodeList.Add(refCusCode);
			}
		}

		bool HasStatusCodesInDetails(string details) => !NoStatusCode.IsMatch(details);

		List<string> GetValidStatusCodes(MatchCollection matches) => matches.Cast<Match>().Where(m => ValidStatusCodes.Contains(m.Value)).Select(m => m.Value).ToList();

		List<string> GetSeriesCodesFromDescription(string details)
		{
			var codes = new List<string>();
			var seriesMatches = SeriesStatusCode.Matches(details);
			foreach (var match in seriesMatches)
			{
				codes.AddRange(ValidStatusCodes.Where(c => c.StartsWith(match.ToString(), StringComparison.Ordinal)));
			}
			return codes;
		}

		bool HasExceptionStatusCodesInDetails(string details) => ExceptionStatusCodes.IsMatch(details);

		ImmutableHashSet<string> ValidStatusCodes
		{
			/*
			 * Codes defined at https://www.gov.uk/government/publications/uk-trade-tariff-document-status-codes-for-harmonised-declarations/uk-trade-tariff-document-status-codes-for-harmonised-declarations
			 */

			get
			{
				return validStatusCodes ?? (validStatusCodes = new HashSet<string>() {
						"AC", "AE", "AF", "AG", "AP", "AS", "AT", "CP",
						"EA", "EE", "EL", "EP", "ES", "FP", "GE", "GP",
						"HP", "IA", "IE", "IP", "IS", "JA", "JE", "JP", "JS",
						"LE", "LP", "RE", "SP", "TP", "UA", "UE", "UP", "US",
						"XA", "XB", "XF", "XO", "XU", "XW", "XX"
					}.ToImmutableHashSet());
			}
		}
		ImmutableHashSet<string> validStatusCodes;

		Regex NoStatusCode => noStatusCode ?? (noStatusCode = new Regex(@"(no status code\b).*(\brequired)", RegexOptions.IgnoreCase));
		Regex noStatusCode;

		Regex SeriesStatusCode => seriesStatusCode ?? (seriesStatusCode = new Regex(@"((?<=[ ])[A-Z](?=[, ]))(?=.*series)"));
		Regex seriesStatusCode;

		Regex StatusCodes => statusCodes ?? (statusCodes = new Regex("(?<![A-Z])[A-Z]{2}(?![A-Z])"));
		Regex statusCodes;

		Regex ExceptionStatusCodes => exceptionStatusCodes ?? (exceptionStatusCodes = new Regex(@".*(\bexcept)", RegexOptions.IgnoreCase));
		Regex exceptionStatusCodes;
	}
}
