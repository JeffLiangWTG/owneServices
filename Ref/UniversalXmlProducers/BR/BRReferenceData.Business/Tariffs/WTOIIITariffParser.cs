using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.BRReferenceData.Business
{
	public class WTOIIITariffParser : BaseParser
	{
		public WTOIIITariffParser(string dataSource) : base(dataSource)
		{
		}

		public void ExportToXMLFile(Stream inputFileStreamTEC, string outputFileName, DateTime publicationTime)
		{
			var refRateList = GetDataFromXlsx(inputFileStreamTEC);
			var writerConfiguration = Helper.GetChildTariffConfiguration(Constants.TariffTypes.Codes.WTOIII, Constants.TradeGroupCodes.WTO);
			Helper.ExportToXMLFile(outputFileName, dataSource, publicationTime, writerConfiguration, refRateList);
		}

		static IEnumerable<RefCusTariff> GetDataFromXlsx(Stream streamTEC)
		{
			Contract.Assume(streamTEC != null);

			var xlsTEC = new XlsFile(streamTEC, true);

			Contract.Assume(xlsTEC != null);
			var result = new HashSet<RefCusTariff>();

			var tariffAttributeFull = xlsTEC.GetXlsValueAsString(12, 2);
			var legalActType = SplitAndCollect(tariffAttributeFull, '/', 0);
			var legalActFull = SplitAndCollect(tariffAttributeFull, '/', 1);
			var legalActIssuingBody = SplitAndCollect(legalActFull, ' ', 0);
			var legalActNumber = SplitAndCollect(legalActFull, ' ', 1);
			var legalActYear = SplitAndCollect(tariffAttributeFull, '/', 2);
			var tariffStartDate = new DateTime(2016, 01, 01);

			for (var row = 15; row <= xlsTEC.RowCount; row++)
			{
				var tariff = xlsTEC.GetXlsValueAsString(row, 1).KeepNumericsOnly();

				if (string.IsNullOrEmpty(tariff))
				{
					break;
				}

				var exTariff = xlsTEC.GetXlsValueAsString(row, 3).PadLeft(3, '0');
				var exTariffDec = decimal.Parse(exTariff, CultureInfo.CurrentCulture);
				var tariffDescription = xlsTEC.GetXlsValueAsString(row, exTariffDec > 0 ? 4 : 2);
				var tariffRatePercent = xlsTEC.GetXlsValueAsDecimal(row, 5).GetValueOrDefault();
				var tariffRate = (tariffRatePercent * 100).ToString("0", CultureInfo.CurrentCulture);
				var tariffRateFormula = tariffRatePercent > 0 ? "VFD * " + tariffRatePercent.ToString("0.0##", CultureInfo.CurrentCulture) : "0";

				var refCusTariff = new RefCusTariff()
				{
					ZZ1_TariffCode = tariff + "_" + exTariff,
					ZZ1_Description = tariffDescription,
					ZZ1_StartDate = tariffStartDate
				};

				refCusTariff.RefCusRates = new RefCusRate[]
				{
					new RefCusRate()
					{
						ZZ2_RateFormula = tariffRateFormula,
						ZZ2_RateFormulaDerivedFrom = tariffRate,
						ZZ2_ZY1_NKRateCode = Constants.Rates.Codes.Duty,
						ZZ2_ZZS_NKPreference = Constants.PreferenceCodes.FullCollection,
						RefCusApplicabilities = new RefCusApplicability[]
						{
							new RefCusApplicability()
							{
								ZZT_AdditionalCode = string.Empty
							}
						}
					}
				};

				refCusTariff.RefCusTariffRelationships = new RefCusTariffRelationship[]
				{
					new RefCusTariffRelationship() { ZZH_TariffCode = tariff }
				};

				refCusTariff.RefCusTariffAttributes = GetRefCusTariffAttributes(legalActType, legalActIssuingBody, legalActNumber, legalActYear, exTariff).ToArray();

				result.Add(refCusTariff);
			}

			return result;
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

			if (!string.IsNullOrEmpty(exCode) && exCode != "000")
			{
				yield return new RefCusTariffAttribute
				{
					ZZ3_Name = Constants.TariffAttributes.LegalActNumberEx,
					ZZ3_Value = exCode
				};
			}
		}

		public static string SplitAndCollect(string text, char separator, int index)
		{
			var array = text?.Split(separator);
			return array?.Any() ?? false ? array.ElementAtOrDefault(index) : string.Empty;
		}
	}
}
