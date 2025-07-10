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
	public class IPIExTariffParser : BaseParser
	{
		public IPIExTariffParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream inputFileStream, string outputFileName, DateTime publicationTime)
		{
			var refTariffList = GetDataFromXls(inputFileStream);
			var writerConfiguration = Helper.GetChildTariffConfiguration(Constants.TariffTypes.Codes.ExTariffIPI, Constants.TradeGroupCodes.ALL, Constants.DataGroupingCodes.Brazil, true, true, null, false, Constants.TariffRateTypes.IPI, constantPreferenceDataGrouping: null);
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writerConfiguration, refTariffList);
		}

		protected static IEnumerable<RefCusTariff> GetDataFromXls(Stream dataSource)
		{
			Contract.Assume(dataSource != null);

			var xls = new XlsFile(dataSource, true);

			Contract.Assume(xls != null);

			var result = new List<RefCusTariff>();

			var rowCount = xls.GetRowCount(xls.ActiveSheet);

			for (int rowId = 9; rowId <= rowCount; rowId++)
			{
				var tariffCode = xls.GetXlsValueAsString(rowId, 2, string.Empty).KeepNumericsOnly();
				var rate = xls.GetXlsValueAsDecimal(rowId, 4);
				var ncm = xls.GetXlsValueAsString(rowId, 1, string.Empty);

				if (!string.IsNullOrEmpty(tariffCode) && rate.HasValue && !string.IsNullOrEmpty(ncm))
				{
					var refCusTariff = new RefCusTariff
					{
						ZZ1_Description = Regex.Replace(xls.GetCellValue(rowId, 3)?.ToString() ?? string.Empty, @"\r\n?|\n", " "),
						ZZ1_TariffCode = ncm.Replace(".", "") + "_" + tariffCode.PadLeft(2, '0'),
						RefCusRates = GetRefCusRates(rate.Value).ToArray(),
						RefCusTariffRelationships = GetRefCusTariffRelationships(ncm).ToArray()
					};

					result.Add(refCusTariff);
				}
			}

			return result;
		}

		protected static IEnumerable<RefCusRate> GetRefCusRates(decimal rate)
		{
			yield return new RefCusRate
			{
				ZZ2_RateFormula = rate > 0 ? "VFD * " + (rate / 100).ToString("0.####", CultureInfo.CurrentCulture) : "0",
				ZZ2_RateFormulaDerivedFrom = rate.ToString("0.##", CultureInfo.CurrentCulture),
				ZZ2_ZY1_NKRateCode = Constants.Rates.Codes.IPI,
				RefCusApplicabilities = RefCusApplicabilities.ToArray()
			};
		}

		protected static IEnumerable<RefCusTariffRelationship> GetRefCusTariffRelationships(string ncm)
		{
			yield return new RefCusTariffRelationship
			{
				ZZH_TariffCode = ncm.KeepNumericsOnly()
			};
		}

		protected static IEnumerable<RefCusApplicability> RefCusApplicabilities
		{
			get
			{
				yield return new RefCusApplicability { };
			}
		}
	}
}
