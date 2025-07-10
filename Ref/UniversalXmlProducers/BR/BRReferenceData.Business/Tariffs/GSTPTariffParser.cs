using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class GSTPTariffParser : BaseParser
	{
		public GSTPTariffParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream inputFileStreamSGPC, Stream inputFileStreamNCM, Stream inputFileStreamTEC, string outputFileName, DateTime publicationTime)
		{
			var refRateList = GetDataFromXlsx(inputFileStreamSGPC, inputFileStreamNCM, inputFileStreamTEC);
			var writerConfiguration = Helper.GetChildTariffConfiguration(Constants.TariffTypes.Codes.GSTP, Constants.TradeGroupCodes.GSTP, Constants.DataGroupingCodes.Mercosul);
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writerConfiguration, refRateList);
		}

		public static IEnumerable<RefCusTariff> GetDataFromXlsx(Stream streamSGPC, Stream streamNCM, Stream streamTEC)
		{
			Contract.Assume(streamSGPC != null && streamNCM != null && streamTEC != null);

			var xlsSGPC = new XlsFile(streamSGPC, true);
			var xlsNCM = new XlsFile(streamNCM, true);
			var xlsTEC = new XlsFile(streamTEC, true);

			Contract.Assume(xlsSGPC != null);

			xlsSGPC.ActiveSheet = 1;
			var cellStartAddress = xlsSGPC.Find("NCM SH2017", xlsSGPC.GetAutoFilterRange(), new FlexCel.Core.TCellAddress("A1"), true, true, false, true);
			var rowCount = xlsSGPC.GetRowCount(xlsSGPC.ActiveSheet);
			var legalAct = xlsSGPC.GetCellValue(4, 2)?.ToString() ?? string.Empty;

			var result = new HashSet<RefCusTariff>();
			for (var rowId = cellStartAddress.Row + 1; rowId <= rowCount; rowId++)
			{
				var rawNcm = xlsSGPC.GetCellValue(rowId, 1)?.ToString() ?? string.Empty;
				var ncm = rawNcm.Length != 8 ? "0" + rawNcm.Substring(0, 3) + "." + rawNcm.Substring(3, 2) + "." + rawNcm.Substring(5, 2) : rawNcm.Substring(0, 4) + "." + rawNcm.Substring(4, 2) + "." + rawNcm.Substring(6, 2);
				var exCode = xlsSGPC.GetCellValue(rowId, 3)?.ToString() ?? string.Empty;
				var exDescription = xlsSGPC.GetCellValue(rowId, 4)?.ToString() ?? string.Empty;
				var percent = xlsSGPC.GetCellValue(rowId, 5)?.ToString() ?? string.Empty;

				if (string.IsNullOrEmpty(exDescription))
				{
					exDescription = xlsSGPC.GetCellValue(rowId, 2)?.ToString() ?? string.Empty;
				}

				if (Regex.IsMatch(ncm, RatesUtils.NcmPattern))
				{
					var startDate = GetStartDateFromXls(xlsNCM, ncm);
					var tec = Regex.Match(GetTecFromXls(xlsTEC, ncm), @"\d+").Value;
					result.Add(FeedRefCusTariff(ncm, tec, exCode, exDescription, startDate, percent, legalAct));
				}
			}

			return result;
		}

		static string GetStartDateFromXls(XlsFile xls, string ncm)
		{
			ncm = ncm.Replace(".", string.Empty);

			Contract.Assume(xls != null);

			var rowCount = xls.GetRowCount(xls.ActiveSheet);

			for (var rowId = 2; rowId <= rowCount; rowId++)
			{
				var ncmXls = xls.GetCellValue(rowId, 1)?.ToString() ?? string.Empty;

				if (ncmXls == ncm)
				{
					return DateTime.FromOADate(double.Parse(xls.GetCellValue(rowId, 2).ToString(), CultureInfo.CurrentCulture)).ToString("dd-MM-yyyy", CultureInfo.CurrentCulture) ?? string.Empty;
				}
			}

			return string.Empty;
		}

		static string GetTecFromXls(XlsFile xls, string ncm)
		{
			Contract.Assume(xls != null);

			var cellStartAddress = xls.Find("NCM", xls.GetAutoFilterRange(), new FlexCel.Core.TCellAddress("A1"), true, true, false, true);
			var rowCount = xls.GetRowCount(xls.ActiveSheet);

			for (var rowId = cellStartAddress.Row + 1; rowId <= rowCount; rowId++)
			{
				var ncmXls = xls.GetCellValue(rowId, 1)?.ToString() ?? string.Empty;

				if (ncmXls == ncm)
				{
					string tec = xls.GetCellValue(rowId, 3)?.ToString() ?? string.Empty;
					return tec;
				}
			}

			return string.Empty;
		}

		static RefCusTariff FeedRefCusTariff(string ncm, string tec, string exCode, string exDescription, string startDate, string percent, string legalAct)
		{
			string legalActType = legalAct.Split('/')[0];
			string legalActIssuingBody = legalAct.Split('/')[1].Split(' ')[0];
			string legalActNumber = legalAct.Split('/')[1].Split(' ')[1];
			string legalActYear = legalAct.Split('/')[2];

			var refCusTarrifRelationship = new RefCusTariffRelationship[]
			{
				new RefCusTariffRelationship
				{
					ZZH_TariffCode = ncm.Replace(".", "")
				}
			};
			var refCusRate = new RefCusRate[]
			{
				new RefCusRate
				{
					ZZ2_ZY1_NKRateCode = Constants.Rates.Codes.IPI,
					ZZ2_ZZS_NKPreference = Constants.PreferenceCodes.FullCollection,
					ZZ2_RateFormula = percent, // TODO: Implement the rate formula here in a future WI
					ZZ2_RateFormulaDerivedFrom = percent,
					RefCusApplicabilities = new []
					{
						new RefCusApplicability()
					}
				}
			};

			var tariffCode = ncm.Replace(".", "") + (string.IsNullOrEmpty(exCode) ? "_000" : "_" + exCode);

			return new RefCusTariff
			{
				ZZ1_TariffCode = tariffCode,
				ZZ1_Description = string.IsNullOrEmpty(exDescription) ? tariffCode : exDescription,
				ZZ1_StartDate = !string.IsNullOrEmpty(startDate) ? Convert.ToDateTime(startDate, CultureInfo.CurrentCulture) : Convert.ToDateTime((DateTime?)null, CultureInfo.CurrentCulture),
				RefCusRates = refCusRate,
				RefCusTariffRelationships = refCusTarrifRelationship,
				RefCusTariffAttributes = GetRefCusTariffAttributes(legalActType, legalActIssuingBody, legalActNumber, legalActYear, exCode).ToArray(),
			};
		}

		static IEnumerable<RefCusTariffAttribute> GetRefCusTariffAttributes(string legalActType, string legalActIssuingBody, string legalActNumber, string legalActYear, string exCode)
		{
			if (!string.IsNullOrEmpty(legalActType))
			{
				yield return new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActType,
					ZZ3_Value = legalActType
				};
			}

			if (!string.IsNullOrEmpty(legalActIssuingBody))
			{
				yield return new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActIssuingBody,
					ZZ3_Value = legalActIssuingBody
				};
			}

			if (!string.IsNullOrEmpty(legalActNumber))
			{
				yield return new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActNumber,
					ZZ3_Value = legalActNumber
				};
			}

			if (!string.IsNullOrEmpty(legalActYear))
			{
				yield return new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActYear,
					ZZ3_Value = legalActYear
				};
			}

			if (!string.IsNullOrEmpty(exCode))
			{
				yield return new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActNumberEx,
					ZZ3_Value = exCode
				};
			}
		}
	}
}
