using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public partial class AsycudaBill
	{
		public void CalculateStampTaxToMasterBill()
		{
			if (this.IsChildMasterBill)
			{
				var stampTax = new RefCusTaxOrFee.Loader(Factory).LoadMostRecentEffectiveTaxOrFeeFromCodeDate(Core.Constants.CountryCodes.Turkey, TaxCodeList.Codes.StampTax, Header.AMA_DateAtCustomsOffice);
				if (stampTax != null)
				{
					var tax = AsycudaTaxes.Cast<AsycudaTax>().FirstOrDefault(x => x.AET_ChargeType == TaxCodeList.Codes.StampTax);
					if (tax == null)
					{
						tax = AsycudaTaxes.AddNew();
						tax.AET_ChargeType = TaxCodeList.Codes.StampTax;
					}
					tax.AET_ChargeAmount = stampTax.ZZF_Value;
				}
			}
		}

		public void CalculateCustomsDuty()
		{
			if (this.AsycudaTaxes.IsExistOverrideTax(TaxCodeList.Codes.CustomsDuty))
			{
				return;
			}

			var customsValueWithTYR = ConvertToLocal(ABL_CustomsValue, ABL_RX_NKCustomsValueCurrency);
			if (customsValueWithTYR == ZDecimal.Zero)
			{
				return;
			}

			(var exemptionDutyAmount1, var exemptionDutyRate1) = CalculateExemptionDuty(customsValueWithTYR, ExemptionCode1);
			(var exemptionDutyAmount2, var exemptionDutyRate2) = CalculateExemptionDuty(customsValueWithTYR, ExemptionCode2);

			var dutyAmountOnPackage = ZDecimal.Zero;
			var dutyRateOnPackage = ZDecimal.Zero;

			foreach (var item in Packs.Cast<ICustomsDutyCalculationData>())
			{
				item.CustomsValue = customsValueWithTYR;
				var helper = new BillOrPackageDutyCalcHelper(Factory, item);

				(var dutyAmount, var dutyRate) = helper.CalculateDuty();
				dutyAmountOnPackage += dutyAmount;
				dutyRateOnPackage += dutyRate;
			}

			var chargeAmount = exemptionDutyAmount1 + exemptionDutyAmount2 + dutyAmountOnPackage;
			var chargeRate = exemptionDutyRate1 + exemptionDutyRate2 + dutyRateOnPackage;

			ManageCharge(TaxCodeList.Codes.CustomsDuty, chargeAmount, chargeRate, customsValueWithTYR, MethodOfPaymentList.Codes.Cash);
		}

		(ZDecimal DutyAmount, ZDecimal DutyRate) CalculateExemptionDuty(ZDecimal customsValueWithTYR, ZString exemptionCode)
		{
			if (!exemptionCode.IsEmpty && this.Lookups.ExemptionCodeList.ContainsCode(exemptionCode))
			{
				var calculationData = new BillCustomsDutyCalculationData(Core.Constants.CountryCodes.Turkey, Header.DateAtCustomsOffice, TaxCodeList.RelatedMiscCodes.ExemptionRateType, TaxCodeList.Codes.CustomsDuty, TaxCodeList.RelatedMiscCodes.ExemptionType, exemptionCode, customsValueWithTYR, ExportCountry, ZString.Empty);
				var helper = new BillOrPackageDutyCalcHelper(Factory, calculationData);
				return helper.CalculateDuty();
			}
			return (0, 0);
		}

		public void CalculateBanderolDuty()
		{
			if (this.AsycudaTaxes.IsExistOverrideTax(TaxCodeList.Codes.TRTBandrol))
			{
				return;
			}

			var totalAmount = ZDecimal.Zero;
			var totalCustomsQty = ZDecimal.Zero;

			var packedItems = Packs.Select(pack => pack.PackedItem).WhereNotNull().Where(item => !item.API_CustomsQty.IsEmpty && !item.API_CustomsUQ.IsEmpty).ToList();
			if (packedItems.Any() && packedItems.Select(item => item.API_CustomsUQ).AllSame())
			{
				foreach (var packedItem in packedItems)
				{
					var customsQty = packedItem.API_CustomsQty;
					var (amount, dutyRate) = CalculateBanderolRate(customsQty, packedItem.API_CustomsUQ, packedItem.BanderolTariff);

					totalAmount += amount;
					totalCustomsQty += customsQty;
				}
			}

			var totalAmountLocalCurrency = ConvertToLocal(totalAmount, ABL_RX_NKCustomsValueCurrency);
			var totalBaseValue = ZDecimal.Zero;
			if (!totalCustomsQty.IsEmpty)
			{
				totalBaseValue = totalAmount / totalCustomsQty;
			}

			var totalBaseValueLocalCurrency = ConvertToLocal(totalBaseValue, ABL_RX_NKCustomsValueCurrency);
			ManageCharge(TaxCodeList.Codes.TRTBandrol, totalAmountLocalCurrency, totalCustomsQty, totalBaseValueLocalCurrency, MethodOfPaymentList.Codes.Cash);
		}

		(ZDecimal amount, ZDecimal dutyRate) CalculateBanderolRate(ZDecimal customsValue, ZString customsUQ, ZString banderolTariff)
		{
			var calculationData = new BillCustomsDutyCalculationData(Core.Constants.CountryCodes.Turkey, ZDateTime.Today, TaxCodeList.RelatedMiscCodes.BanderolRateType, TaxCodeList.Codes.TRTBandrol, TaxCodeList.RelatedMiscCodes.BanderolTariffType, banderolTariff, customsValue, ExportCountry, ZString.Empty);
			var helper = new BillOrPackageDutyCalcHelper(Factory, calculationData, new Dictionary<string, decimal>() { { customsUQ, customsValue } });
			return helper.CalculateDuty();
		}

		void ManageCharge(ZString type, ZDecimal amount, ZDecimal rate, ZDecimal baseValue, ZString methodOfPayment)
		{
			var chargeByType = AsycudaTaxes.FindByType(type);
			if (amount != ZDecimal.Zero)
			{
				var tax = chargeByType ?? AsycudaTaxes.AddNew(type);
				using (tax.SuspendOnCalculation())
				{
					tax.AET_ChargeAmount = amount;
					tax.AET_Rate = rate;
					tax.AET_BaseValue = baseValue;
					tax.AET_MethodOfPayment = methodOfPayment;
				}
			}
			else
			{
				chargeByType?.Delete();
			}
		}

		ZDecimal ConvertToLocal(ZDecimal fromAmount, ZString fromCurrency) => Header.ConvertUsingCustomsRate(
					Header.AMA_DateAtCustomsOffice,
					fromAmount,
					fromCurrency,
					Core.Constants.CurrencyCodes.Turkey,
					Header.Branch.Company
		);
	}

	class BillCustomsDutyCalculationData : ICustomsDutyCalculationData
	{
		public BillCustomsDutyCalculationData(ZString dataGrouping, ZDateTime effectiveDate, ZString rateType, ZString rateCode, ZString tariffType, ZString tariffCode, ZDecimal customsValue, ZString exportCountry, ZString additionalCode)
		{
			DataGrouping = dataGrouping;
			EffectiveDate = effectiveDate;
			RateType = rateType;
			RateCode = rateCode;
			TariffType = tariffType;
			TariffCode = tariffCode;
			CustomsValue = customsValue;
			ExportCountry = exportCountry;
			AdditionalCode = additionalCode;
		}

		public ZString DataGrouping { get; }
		public ZDateTime EffectiveDate { get; }
		public ZString RateType { get; }
		public ZString RateCode { get; }
		public ZString TariffType { get; }
		public ZString TariffCode { get; }
		public ZDecimal CustomsValue { get; set; }
		public ZString ExportCountry { get; }
		public ZString AdditionalCode { get; }

		public ZString GetFixedDutyFormula()
		{
			return ZString.Empty;
		}
	}
}
