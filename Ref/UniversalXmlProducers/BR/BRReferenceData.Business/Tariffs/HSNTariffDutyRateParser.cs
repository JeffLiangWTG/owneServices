using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class HSNTariffDutyRateParser : BaseParser
	{
		public HSNTariffDutyRateParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream inputFileStream, string fileType, string outputFileName, DateTime publicationTime)
		{
			IEnumerable<RefCusTariff> refRateList = null;
			XmlWriterConfiguration writerConfiguration = null;

			switch (fileType)
			{
				case Constants.HSNTariffDutyRateFileType.XLS_TEC:
					refRateList = GetDataFromXls(inputFileStream);
					writerConfiguration = Helper.GetRefCusRateWriterConfiguration(new DateTime(2022, 04, 01), new DateTime(2022, 12, 31, 23, 59, 00), Constants.Rates.Types.Duty, Constants.Rates.Codes.Duty, Constants.PreferenceCodes.NORMAL);
					break;
				case Constants.HSNTariffDutyRateFileType.XLS_RATES:
					refRateList = GetDataFromXlsRates(inputFileStream);
					writerConfiguration = Helper.GetRefCusRateWriterConfiguration(new DateTime(2023, 04, 27));
					break;
			}

			if (writerConfiguration != null)
			{
				Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writerConfiguration, refRateList);
			}
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
				if (Regex.IsMatch(ncm, RatesUtils.NcmPattern))
				{
					var tec = Regex.Match(xls.GetCellValue(rowId, 3).ToString(), @"\d+").Value;
					var cusRate = RatesUtils.FeedRefCusTariffRate(tec, Constants.Rates.Codes.Duty, Constants.PreferenceCodes.NORMAL);
					var rates = new RefCusRate[] { };
					if (cusRate != null)
					{
						rates = new RefCusRate[] { cusRate };
					}
					result.Add(new RefCusTariff { ZZ1_TariffCode = ncm.Replace(".", ""), RefCusRates = rates });
				}
			}

			return result;
		}

		public static IEnumerable<RefCusTariff> GetDataFromXlsRates(Stream stream)
		{
			Contract.Assume(stream != null);

			var xls = new XlsFile(stream, true);
			Contract.Assume(xls != null);

			xls.ActiveSheet = 1;
			var rowCount = xls.GetRowCount(xls.ActiveSheet);

			var result = new HashSet<RefCusTariff>();
			for (var rowId = 2; rowId <= rowCount; rowId++)
			{
				var ncm = xls.GetXlsValueAsString(rowId, "A").Replace(".", "");
				if (ncm.Length == 8)
				{
					string[] rateCodes = new string[] { Constants.Rates.Codes.COFINS, Constants.Rates.Codes.PIS, Constants.Rates.Codes.Duty, Constants.Rates.Codes.IPI };
					var rates = new List<RefCusRate>();
					foreach (var rateCode in rateCodes)
					{
						var dataIndex = GetColumnsIndexForRate(rateCode);
						var startDate = xls.GetXlsValueAsDateTime(rowId, dataIndex.Item2);
						var endDate = xls.GetXlsValueAsDateTime(rowId, dataIndex.Item3);

						if (startDate.HasValue)
						{
							var rateFormula = Regex.Replace(xls.GetXlsValueAsString(rowId, dataIndex.Item1), "[^0-9.]", "");
							if (rateCode == Constants.Rates.Codes.IPI)
							{
								var ipiHasRate = xls.GetXlsValueAsString(rowId, "O");
								rateFormula = rateFormula == "0" && ipiHasRate == "S" ? "" : rateFormula;
							}
							var cusRate = RatesUtils.FeedRefCusTariffRate(rateFormula, rateCode, rateCode == Constants.Rates.Codes.Duty ? Constants.PreferenceCodes.NORMAL : null, startDate, endDate);
							if (cusRate != null)
							{
								rates.Add(cusRate);
							}
						}
						else
						{
							ParserErrorCollector.Instance.AppendLine($"Tariff: {ncm} Rate Code: {rateCode}, has no details");
						}
					}
					if (rates.Any())
					{
						result.Add(new RefCusTariff { ZZ1_TariffCode = ncm, RefCusRates = rates.ToArray() });
					}
				}
			}
			return result;
		}

		static (string, string, string) GetColumnsIndexForRate(string duty)
		{
			switch (duty)
			{
				case Constants.Rates.Codes.COFINS:
					return ("N", "K", "L");
				case Constants.Rates.Codes.PIS:
					return ("M", "I", "J");
				case Constants.Rates.Codes.Duty:
					return ("C", "D", "E");
				case Constants.Rates.Codes.IPI:
					return ("H", "G", "F");
				default:
					return (string.Empty, string.Empty, string.Empty);
			}
		}
	}
}
