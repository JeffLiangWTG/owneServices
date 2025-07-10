using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators
{
	internal class GSTCalculator
	{
		public GSTCalculator(CusEntryLine entryLine)
			: this(
				entryLine.VFDWholeNZD,
				entryLine.FreightWholeNZD,
				entryLine.InsuranceWholeNZD,
				entryLine.TotalDutiesAndLevies,
				entryLine.IsZeroRatedGST,
				entryLine.IsDutyOnlyGST,
				entryLine.Header.DateForDutyRate,
				entryLine.Factory)
		{
		}

		public GSTCalculator(JobComInvoiceLine invoiceLine)
			: this(
				invoiceLine.CustomsValueInLocalCurrencyRounded,
				invoiceLine.OverseasFreightInLocalCurrencyRounded,
				invoiceLine.OverseasInsuranceInLocalCurrencyRounded,
				invoiceLine.TotalDutiesAndLevies,
				invoiceLine.EffectiveIsZeroRatedGST,
				invoiceLine.IsDutyOnlyGST,
				invoiceLine.DateForDutyRate,
				invoiceLine.Factory)
		{
		}

		GSTCalculator(ZDecimal valueForDuty, ZDecimal freightValue, ZDecimal insuranceValue, ZDecimal totalDutiesAndLevies, ZBool isZeroRated, ZBool isDutyOnlyGST, ZDateTime dateForGSTPurposes, BusinessObjectFactory factory)
		{
			if (!isZeroRated)
			{
				GSTRate = GetGSTRate(factory, dateForGSTPurposes);

				if (isDutyOnlyGST)
				{
					GSTAmount = Utilities.Round(totalDutiesAndLevies * GSTRate, 2);
				}
				else
				{
					GSTAmount = Utilities.Round((valueForDuty + freightValue + insuranceValue + totalDutiesAndLevies) * GSTRate, 2);
				}
			}
		}

		public readonly ZDecimal GSTAmount;
		public readonly ZDecimal GSTRate;

		internal static decimal GetGST(BusinessObjectFactory factory, ZDecimal amount, ZDateTime dateForGSTPurposes)
		{
			return Utilities.Round(amount * GetGSTRate(factory, dateForGSTPurposes), 2);
		}

		internal static decimal GetGSTRate(BusinessObjectFactory factory, ZDateTime dateForGSTPurposes)
		{
			return UniversalReferenceHelper.GetTaxOrFee(factory, UniversalReferenceConstants.TaxOrFeeCodes.GoodsAndServicesTax, dateForGSTPurposes);
		}
	}
}
