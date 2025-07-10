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
	public class LEBITTariffParser : BaseParser
	{
		public LEBITTariffParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream inputFileStream, string outputFileName, DateTime publicationTime)
		{
			var refTariffList = GetDataFromXls(inputFileStream);
			var writerConfiguration = Helper.GetChildTariffConfiguration(Constants.TariffTypes.Codes.LEBIT, Constants.TradeGroupCodes.LEBIT, Constants.DataGroupingCodes.Mercosul, true);
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
				if (Regex.IsMatch(ncm, Pattern))
				{
					ncm = ncm.Replace(".", "");
					var exNumber = xls.GetCellValue(rowId, 2).ToString();
					var description = xls.GetCellValue(rowId, 3).ToString();
					var percent = decimal.Parse(xls.GetCellValue(rowId, 4).ToString(), CultureInfo.CurrentCulture);
					var startDate = xls.GetCellValue(rowId, 5).ToString();
					var inclusionAct = xls.GetCellValue(rowId, 6).ToString();
					if (result.Where(x => x.ZZ1_TariffCode.Contains(ncm)).Any())
					{
						result.RemoveWhere(x => x.ZZ1_TariffCode.Contains(ncm));
					}
					result.Add(FeedRefCusTariffRate(ncm, exNumber, description, percent, startDate, inclusionAct));
				}
			}

			return result;
		}

		static RefCusTariff FeedRefCusTariffRate(string ncm, string exNumber, string description, decimal percent, string startData, string inclusionAct)
		{
			var refCusTariffRelationship = new RefCusTariffRelationship[] { new RefCusTariffRelationship { ZZH_TariffCode = ncm } };

			var splitInclusionAct = inclusionAct.Split(' ');
			var refCusTariffAttribute = new List<RefCusTariffAttribute>()
			{
				new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActType,
					ZZ3_Value = splitInclusionAct[0].ToUpper(CultureInfo.CurrentCulture)
				},
				new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActIssuingBody,
					ZZ3_Value = splitInclusionAct[1].ToUpper(CultureInfo.CurrentCulture)
				},
				new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActNumber,
					ZZ3_Value = splitInclusionAct[splitInclusionAct.Length - 1]
				},
			};

			if (Regex.IsMatch(exNumber, @"^\d{3}"))
			{
				refCusTariffAttribute.Add(new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActNumberEx,
					ZZ3_Value = exNumber
				});
			}
			else
			{
				exNumber = DefaultExNumber;
			}

			var formula = percent > 0m ? $"VFD * {percent / 100:0.00#}" : "0";
			var refCusRate = new RefCusRate[]
			{
				new RefCusRate
				{
					ZZ2_RateFormula = formula,
					ZZ2_RateFormulaDerivedFrom = percent.ToString("0.##",CultureInfo.CurrentCulture),
					ZZ2_ZY1_NKRateCode = string.Equals(exNumber, DefaultExNumber, StringComparison.Ordinal) ? Constants.Rates.Codes.IPI : Constants.Rates.Codes.Duty,
					ZZ2_ZZS_NKPreference = "1",
					RefCusApplicabilities = new []
					{
						new RefCusApplicability()
					}
				}
			};

			var date = DateTime.ParseExact(startData, "dd/MM/yyyy", null);
			return new RefCusTariff { ZZ1_TariffCode = $"{ncm}_{exNumber}", ZZ1_Description = description, ZZ1_StartDate = date, RefCusRates = refCusRate, RefCusTariffRelationships = refCusTariffRelationship, RefCusTariffAttributes = refCusTariffAttribute.ToArray() };
		}

		const string Pattern = @"^(\d{4}\.\d{2}\.\d{2})$";
		const string DefaultExNumber = "000";
	}
}
