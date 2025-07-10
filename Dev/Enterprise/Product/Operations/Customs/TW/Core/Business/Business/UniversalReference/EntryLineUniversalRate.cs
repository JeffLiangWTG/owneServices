using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.TW.Business
{
	public class EntryLineUniversalRate : Customs.Business.EntryLineUniversalRate
	{
		internal EntryLineUniversalRate(CusEntryLine entryLine)
			: base(entryLine, entryLine.CL_CustomsValueForDutyCalculation, 3)
		{
		}

		internal EntryLineUniversalRate(CusEntryLine entryLine, ZDecimal customsValue)
			: base(entryLine, customsValue, 3)
		{
		}

		protected override IEnumerable<(ZString uq, ZDecimal qty)> GetUnitOfMeasureValues()
		{
			foreach (JobComInvoiceLine invoiceLine in EntryLine.InvoiceLines)
			{
				foreach (var data in GetUnitOfMeasureValueList(invoiceLine))
				{
					if (!data.uq.IsEmpty)
					{
						yield return data;
					}
				}
			}
		}

		public static Dictionary<string, decimal> GetUnitOfMeasureValueListByInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			var unitOfMeasureValueListForInvoiceLine = new Dictionary<string, decimal>();
			if (invoiceLine != null)
			{
				foreach (var data in GetUnitOfMeasureValueListFromInvoiceLine(invoiceLine, new List<ZString>()))
				{
					if (!data.uq.IsEmpty)
					{
						unitOfMeasureValueListForInvoiceLine.AddNewKeyOrAccumulateValue(data.uq, data.qty);
					}
				}
			}
			return unitOfMeasureValueListForInvoiceLine;
		}

		static IEnumerable<(ZString uq, ZDecimal qty)> GetUnitOfMeasureValueList(JobComInvoiceLine invoiceLine)
		{
			var invoiceLineUnitList = new List<ZString>();
			return GetUnitOfMeasureValueListFromInvoiceLine(invoiceLine, invoiceLineUnitList).Union(GetUnitOfMeasureValueListFromTaxes(invoiceLine, invoiceLineUnitList));
		}

		static IEnumerable<(ZString uq, ZDecimal qty)> GetUnitOfMeasureValueListFromInvoiceLine(JobComInvoiceLine invoiceLine, List<ZString> invoiceLineUnitList)
		{
			yield return CreateResultIfNeed(invoiceLineUnitList, invoiceLine.JI_CustomsUnitQty, invoiceLine.JI_CustomsQuantity);
			yield return CreateResultIfNeed(invoiceLineUnitList, invoiceLine.JI_CustomsSecondUnitQty, invoiceLine.JI_CustomsSecondQuantity);

			(var convertedCustomsQuantity, var convertedCustomsUnitQty) = UnitConverter.TryConvertToInterchangableUnit(invoiceLine.JI_CustomsUnitQty, invoiceLine.JI_CustomsQuantity);
			yield return CreateResultIfNeed(invoiceLineUnitList, convertedCustomsUnitQty, convertedCustomsQuantity);

			(var convertedCustomsSecondQuantity, var convertedCustomsSecondUnitQty) = UnitConverter.TryConvertToInterchangableUnit(invoiceLine.JI_CustomsSecondUnitQty, invoiceLine.JI_CustomsSecondQuantity);
			yield return CreateResultIfNeed(invoiceLineUnitList, convertedCustomsSecondUnitQty, convertedCustomsSecondQuantity);
		}

		static IEnumerable<(ZString uq, ZDecimal qty)> GetUnitOfMeasureValueListFromTaxes(JobComInvoiceLine invoiceLine, List<ZString> invoiceLineUnitList)
		{
			foreach (JobComInvoiceLineTax invoiceLineTax in invoiceLine.Taxes)
			{
				var secondaryTariffsUnitQty1 = invoiceLineTax.JLT_BaseQuantityUQ;
				var secondaryTariffsQuantity1 = invoiceLineTax.JLT_BaseQuantity;
				yield return CreateResultIfNeed(invoiceLineUnitList, secondaryTariffsUnitQty1, secondaryTariffsQuantity1);

				(var convertedSecondaryTariffsQuantity1, var convertedSecondaryTariffsUnitQty1) = UnitConverter.TryConvertToInterchangableUnit(secondaryTariffsUnitQty1, secondaryTariffsQuantity1);
				yield return CreateResultIfNeed(invoiceLineUnitList, convertedSecondaryTariffsUnitQty1, convertedSecondaryTariffsQuantity1);
			}
		}

		static (ZString uq, ZDecimal qty) CreateResultIfNeed(List<ZString> invoiceLineUnitList, ZString inputUQ, decimal inputQuantity)
		{
			if (!inputUQ.IsEmpty && !invoiceLineUnitList.Contains(inputUQ))
			{
				invoiceLineUnitList.Add(inputUQ);
				return (inputUQ, inputQuantity);
			}
			return (ZString.Empty, ZDecimal.Zero);
		}

		protected override IEnumerable<(ZString uq, ZDecimal qty)> GetCountrySpecificValues()
		{
			yield return (TWSpecificRateParameterList.Codes.AlcoholPercentage, RandomLine.JI_AlcoholPercentage);
			var entryLineQty = ((CusEntryLine)EntryLine).CL_EntryLineQty;
			yield return (TWSpecificRateParameterList.Codes.UnitCustomsValue, entryLineQty.IsDefault ? ZDecimal.Zero : (ZDecimal)(EntryLine.CL_CustomsValue / entryLineQty));
			var chargeTypeList = ChargeTypeHelper.GetChargeTypes(RandomLine.Factory, DateOfValuation);
			foreach (var chargeType in chargeTypeList)
			{
				yield return (chargeType.RateCode, ZDecimal.Zero);
			}
		}

		protected new JobComInvoiceLine RandomLine => (JobComInvoiceLine)base.RandomLine;
	}
}
