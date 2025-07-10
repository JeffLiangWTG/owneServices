using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class AutoPartsTariffParser : BaseParser
	{
		public AutoPartsTariffParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream inputFileStream, string outputFileName, DateTime publicationTime)
		{
			var refTariffList = GetDataFromXls(inputFileStream);
			var writerConfiguration = Helper.GetChildTariffConfiguration(Constants.TariffTypes.Codes.AutoPartList, Constants.TradeGroupCodes.ALL, Constants.DataGroupingCodes.Brazil, true, true, RateFormulaDerivedFrom);
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writerConfiguration, refTariffList);
		}

		public static IEnumerable<RefCusTariff> GetDataFromXls(Stream stream)
		{
			Contract.Assume(stream != null);

			var xls = new XlsFile(stream, true);
			Contract.Assume(xls != null);

			xls.ActiveSheet = 1;
			var cellStartAddress = xls.Find("NCM", xls.GetAutoFilterRange(), new FlexCel.Core.TCellAddress("A1"), true, true, false, true);
			var rowCount = xls.GetRowCount(xls.ActiveSheet);

			var result = new HashSet<RefCusTariff>();
			for (var rowId = cellStartAddress.Row; rowId <= rowCount; rowId++)
			{
				var ncm = xls.GetCellValue(rowId, 1)?.ToString() ?? string.Empty;
				var annex = xls.GetCellValue(rowId, 5)?.ToString() ?? string.Empty;
				if (Regex.IsMatch(ncm, Pattern) && string.Equals("I", annex, StringComparison.Ordinal))
				{
					ncm = ncm.Replace(".", "");
					var exNumber = xls.GetCellValue(rowId, 2).ToString();
					var description = xls.GetCellValue(rowId, 3).ToString();
					var legalAct = xls.GetCellValue(rowId, 4).ToString();
					result.Add(FeedRefCusTariffRate(ncm, exNumber, description, legalAct));
				}
			}

			return result;
		}

		static RefCusTariff FeedRefCusTariffRate(string ncm, string exNumber, string description, string legalAct)
		{
			var refCusTariffRelationship = new RefCusTariffRelationship[] { new RefCusTariffRelationship { ZZH_TariffCode = ncm } };
			legalAct = Regex.Replace(legalAct, @"\(([^\)]+)\)", "");
			var splitLegalAct = legalAct.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
			var splitInclusionAct = splitLegalAct[splitLegalAct.Length - 1].Split('/');
			var refCusTariffAttribute = new List<RefCusTariffAttribute>()
			{
				new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActType,
					ZZ3_Value = splitLegalAct[0].ToUpper(CultureInfo.CurrentCulture)
				},
				new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActIssuingBody,
					ZZ3_Value = splitLegalAct[1].ToUpper(CultureInfo.CurrentCulture)
				},
				new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActNumber,
					ZZ3_Value = splitInclusionAct[0]
				},
				new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActYear,
					ZZ3_Value = splitInclusionAct[splitInclusionAct.Length - 1]
				},
				new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActNumberEx,
					ZZ3_Value = exNumber
				},
			};

			var refCusRate = new RefCusRate[]
			{
				new RefCusRate
				{
					ZZ2_ZY1_NKRateCode = Constants.Rates.Codes.Duty,
					ZZ2_ZZS_NKPreference = "95",
					RefCusApplicabilities = new RefCusApplicability[] { new RefCusApplicability() { ZZT_AdditionalCode = string.Empty }}
				}
			};

			return new RefCusTariff { ZZ1_TariffCode = $"{ncm}_{exNumber}", ZZ1_Description = description, RefCusRates = refCusRate, RefCusTariffRelationships = refCusTariffRelationship, RefCusTariffAttributes = refCusTariffAttribute.ToArray() };
		}

		const string Pattern = @"^(\d{4}\.\d{2}\.\d{2})$";
		const string RateFormulaDerivedFrom = "2";
	}
}
