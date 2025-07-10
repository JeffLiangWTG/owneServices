using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

static class RateCalcUnitOfMeasureAggregator
{	
	public static IDictionary<string, decimal> GetUomQtyDictionary(Customs.Business.CusEntryLine entryLine)
	{
		_ = Argument.NotNull(entryLine, nameof(entryLine));
		var invoiceLines = entryLine
			  .InvoiceLines
			 .Cast<JobComInvoiceLine>();
		return GetUomQtyDictionaryFromLines(invoiceLines, entryLine.Factory, entryLine.CountryCode);
	}

	public static IDictionary<string, decimal> GetUomQtyDictionaryForInvoiceLine(JobComInvoiceLine invLine)
	{
		_ = Argument.NotNull(invLine, nameof(invLine));
		return GetUomQtyDictionaryFromLines([invLine], invLine.Factory, invLine.CountryCode);
	}

	static IDictionary<string, decimal> GetUomQtyDictionaryFromLines(IEnumerable<JobComInvoiceLine> lines, BusinessObjectFactory factory, ZString countryCode)
	{
		var unitAndQuantityDictionary = lines
			.SelectMany(GetUomAndQtyFromLine)
			.Where(data => HasValueForCustomsUnitAndQuantity(data.Unit, data.Quantity))
			.GroupBy(data => data.Unit)
			.ToDictionary(key => (string)key.Key, values => GetAccumulatedValue(values.Key, values.ToArray()));

		DutyCalculator.RateCalcUnitOfMeasureAggregator.AddConvertibleUnitsToUnitOfMeasureDictionary(unitAndQuantityDictionary, factory, countryCode);
		return unitAndQuantityDictionary;
	}

	static decimal GetAccumulatedValue(ZString unitAsKey, (ZString Unit, ZDecimal Quantity)[] unitsAndQuantities)
	{
		var quantities = unitsAndQuantities.Select(q => q.Quantity).ToArray();

		if (quantities.Length == 0)
		{
			throw new InvalidOperationException("Quantities array must contain at least one value");
		}

		return unitAsKey == UniversalReferenceConstants.RefCusCodeList.CustomsUq.Alcohol.AlcoholStrength
			? quantities[0]
			: quantities.Sum(q => q);
	}

	static IEnumerable<(ZString Unit, ZDecimal Quantity)> GetUomAndQtyFromLine(JobComInvoiceLine invoiceLine)
	{
		yield return (invoiceLine.JI_CustomsUnitQty, invoiceLine.JI_CustomsQuantity);
		yield return (invoiceLine.JI_CustomsSecondUnitQty, invoiceLine.JI_CustomsSecondQuantity);
		yield return (invoiceLine.JI_CustomsThirdUnitQty, invoiceLine.JI_CustomsThirdQuantity);
		yield return (invoiceLine.JI_CustomsFourthUnitQty, invoiceLine.JI_CustomsFourthQuantity);
		yield return (invoiceLine.JI_CustomsFifthUnitQty, invoiceLine.JI_CustomsFifthQuantity);
	}

	static bool HasValueForCustomsUnitAndQuantity(ZString unit, ZDecimal quantity) => !unit.IsEmpty && !quantity.IsEmpty && quantity.IsValid;
}
