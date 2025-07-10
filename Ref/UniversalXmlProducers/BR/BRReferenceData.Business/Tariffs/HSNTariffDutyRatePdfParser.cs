using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class HSNTariffDutyRatePdfParser : BaseParser
	{
		public HSNTariffDutyRatePdfParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(List<Stream> inputFileStream, string outputFileName, DateTime publicationTime)
		{
			var refRateList = GetDataFromXls(inputFileStream);
			var writerConfiguration = Helper.GetRefCusRateWriterConfiguration(new DateTime(2022, 04, 01), new DateTime(2022, 12, 31, 23, 59, 00), Constants.Rates.Types.Duty, Constants.Rates.Codes.Duty, Constants.PreferenceCodes.NORMAL);
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writerConfiguration, refRateList);
		}

		public IEnumerable<RefCusTariff> GetDataFromXls(List<Stream> lstStream)
		{
			Contract.Assume(lstStream != null && lstStream.Count > 0);
			var result = new List<RefCusTariff>();
			foreach (var stream in lstStream)
			{
				var xls = new XlsFile(stream, true);
				Contract.Assume(xls != null);

				xls.ActiveSheet = 1;
				var isAnexoVI = xls.GetCellValue(1, 1)?.ToString()?.Contains("Anexo VI") ?? false;
				var ncmColumn = xls.Find("NCM", xls.GetAutoFilterRange(), new FlexCel.Core.TCellAddress("A1"), true, false, false, false);
				var tecColumn = xls.Find("TEC", new FlexCel.Core.TXlsCellRange(ncmColumn.Row, 1, ncmColumn.Row, xls.GetColCount(xls.ActiveSheet)), new FlexCel.Core.TCellAddress(ncmColumn.Row, 1), false, true, false, false);
				var aliquotColumn = xls.Find("alíquota", new FlexCel.Core.TXlsCellRange(ncmColumn.Row, 1, ncmColumn.Row, xls.GetColCount(xls.ActiveSheet)), new FlexCel.Core.TCellAddress(ncmColumn.Row, 1), false, true, false, false);
				var groundingColumn = xls.Find("fundamentação", new FlexCel.Core.TXlsCellRange(ncmColumn.Row, 1, ncmColumn.Row, xls.GetColCount(xls.ActiveSheet)), new FlexCel.Core.TCellAddress(ncmColumn.Row, 1), false, true, false, false);
				var rowCount = xls.GetRowCount(xls.ActiveSheet);

				for (var rowId = ncmColumn.Row; rowId <= rowCount; rowId++)
				{
					var ncm = xls.GetCellValue(rowId, 1)?.ToString() ?? string.Empty;
					if (Regex.IsMatch(ncm, RatesUtils.NcmPattern) || Regex.IsMatch(ncm, RatesUtils.AlternativeNcmPattern))
					{
						var percentValue = aliquotColumn != null ? xls.GetCellValue(rowId, aliquotColumn.Col).ToString() : "0";
						if (groundingColumn != null && tecColumn != null)
						{
							if (string.IsNullOrWhiteSpace(xls.GetCellValue(rowId, groundingColumn.Col)?.ToString()))
							{
								percentValue = xls.GetCellValue(rowId, tecColumn.Col).ToString();
							}
						}

						var applicabilityStartDate = isAnexoVI ? xls.GetXlsValueAsDateTime(rowId, 5) ?? defaultRefCusApplicabilityDateTime : defaultRefCusApplicabilityDateTime;
						var cusRate = RatesUtils.FeedRefCusTariffRate(percentValue, Constants.Rates.Codes.Duty, Constants.PreferenceCodes.NORMAL, applicabilityStartDate);

						var rates = new RefCusRate[] { };
						if (cusRate != null)
						{
							rates = new RefCusRate[] { cusRate };
						}

						var refCusTariff = new RefCusTariff { ZZ1_TariffCode = ncm.Replace(".", ""), RefCusRates = rates };

						var index = result.FindIndex(x => x.ZZ1_TariffCode == refCusTariff.ZZ1_TariffCode);
						if (index < 0)
						{
							result.Add(refCusTariff);
						}
					}
				}
			}

			return result;
		}

		readonly DateTime defaultRefCusApplicabilityDateTime = new DateTime(2022, 04, 01, 00, 00, 00);
	}
}
