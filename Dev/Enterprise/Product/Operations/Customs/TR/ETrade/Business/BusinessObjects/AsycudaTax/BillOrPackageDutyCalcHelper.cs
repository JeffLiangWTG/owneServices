using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class BillOrPackageDutyCalcHelper : IUniversalRateCalcData
	{
		public BillOrPackageDutyCalcHelper(BusinessObjectFactory factory, ICustomsDutyCalculationData customsDutyCalculationData)
			: this(factory, customsDutyCalculationData, new Dictionary<string, decimal>())
		{
		}

		public BillOrPackageDutyCalcHelper(BusinessObjectFactory factory, ICustomsDutyCalculationData customsDutyCalculationData, IDictionary<string, decimal> unitOfMeasureValueList)
		{
			this.factory = Argument.NotNull(factory, nameof(factory));
			iDutyData = customsDutyCalculationData;
			UnitOfMeasureValueList = unitOfMeasureValueList;
		}

		readonly BusinessObjectFactory factory;
		readonly ICustomsDutyCalculationData iDutyData;

		#region IUniversalRateCalcData

		public DateTime DateOfValuation => iDutyData.EffectiveDate.ToDateTime();

		public decimal ValueForDuty => iDutyData.CustomsValue;

		public decimal CustomsValue => iDutyData.CustomsValue;

		public IDictionary<string, decimal> UnitOfMeasureValueList { get; }

		public IDictionary<string, decimal> CountrySpecificValueList { get; } = new Dictionary<string, decimal>();

		public IList<Tuple<string, string>> AdditionalInformationList { get; } = new List<Tuple<string, string>>();

		public IDictionary<string, string> MeursingExpressionList { get; } = new Dictionary<string, string>();

		#endregion

		public (ZDecimal DutyAmount, ZDecimal DutyRate) CalculateDuty()
		{
			var rate = GetDutyTaxRate();

			if (rate != null)
			{
				var fixedDutyFormula = iDutyData.GetFixedDutyFormula();
				if (fixedDutyFormula != ZString.Empty)
				{
					return GetCalculateDutyResult(fixedDutyFormula);
				}
				else
				{
					var rateFormula = rate.ZZ2_RateFormula;
					return GetCalculateDutyResult(rateFormula);
				}
			}

			return (0, 0);
		}

		(ZDecimal DutyAmount, ZDecimal DutyRate) GetCalculateDutyResult(ZString rateFormula)
		{
			var calculator = new UniversalRateCalculator(rateFormula, this);
			var dutyAmount = Utilities.Round(calculator.Calculate(), 3);
			var dutyRate = dutyAmount / CustomsValue * 100;
			return (dutyAmount, dutyRate);
		}

		RateView GetDutyTaxRate()
		{
			RateView result = null;

			if (!iDutyData.TariffCode.IsEmpty)
			{
				var currentTariff = new TariffView.Loader(factory).LoadMostRecentCachedTariff(iDutyData.DataGrouping, iDutyData.TariffType, iDutyData.TariffCode, iDutyData.EffectiveDate);
				if (currentTariff != null)
				{
					var tradeGroupCountry = iDutyData?.ExportCountry ?? string.Empty;

					var criteria = new SpecificRateSelectionCriteria(tradeGroupCountry,
						iDutyData.DataGrouping,
						ZString.Empty,
						ZString.Empty,
						iDutyData.AdditionalCode.IsEmpty ? null : new HashSet<ZString>() { iDutyData.AdditionalCode },
						iDutyData.EffectiveDate,
						iDutyData.RateType,
						iDutyData.RateCode);

					var rates = currentTariff.GetApplicableRates(criteria).ToList();
					if (rates.Count == 1 || iDutyData.AdditionalCode.IsEmpty)
					{
						result = rates.FirstOrDefault();
					}
					else
					{
						result = rates.FirstOrDefault(x => x.FilteredRateApplicabilities.Any(y => y.ZZT_AdditionalCode == iDutyData.AdditionalCode));
					}
				}
			}

			return result;
		}
	}
}
