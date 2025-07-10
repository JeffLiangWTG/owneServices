using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(RateCalcUnitOfMeasureAggregator))]
sealed class RateCalcUnitOfMeasureAggregatorTest : TestCaseWithFactory
{
	public void TestGetUomQtyDictionary_Parameter()
	{
		AssertExceptionThrown<ArgumentNullException>(() => RateCalcUnitOfMeasureAggregator.GetUomQtyDictionary(null));
	}

	public void TestGetUomQtyDictionary()
	{
		var invoiceLineOne = CreateNewInvoiceLine(12, "KG", 22, "NMB");
		invoiceLineOne.JI_CustomsThirdQuantity = 100;
		invoiceLineOne.JI_CustomsThirdUnitQty = "PCS";

		var invoiceLineTwo = CreateNewInvoiceLine(15, "KG", 10, "LTR");
		invoiceLineTwo.JI_CustomsThirdQuantity = 5;
		invoiceLineTwo.JI_CustomsThirdUnitQty = "ASV";
		invoiceLineTwo.JI_CustomsFourthQuantity = 52;
		invoiceLineTwo.JI_CustomsFourthUnitQty = "NMB";

		var invoiceLineThree = CreateNewInvoiceLine(24, "PCS", 9.53, "LTR");
		invoiceLineThree.JI_CustomsFifthQuantity = 5;
		invoiceLineThree.JI_CustomsFifthUnitQty = "ASV";
		invoiceLineThree.JI_CustomsFourthQuantity = 15;
		invoiceLineThree.JI_CustomsFourthUnitQty = "NMB";

		var entryLine = Factory.New<CusEntryLine>();
		entryLine.InvoiceLines.Add(invoiceLineOne);
		entryLine.InvoiceLines.Add(invoiceLineTwo);
		entryLine.InvoiceLines.Add(invoiceLineThree);

		var uomQtyDictionary = RateCalcUnitOfMeasureAggregator.GetUomQtyDictionary(entryLine);

		AssertContainsExactElementsInAnyOrder("Qty Values", new KeyValuePair<string, decimal>[] {
			new ("KG", 27),
			new ("KGM", 27),
			//Automatically added by conversion
			new ("GRM", 27000),
			new ("TNE", 0.027m),
			new ("DTN", 0.27m),

			new ("LTR", 19.53m),
			//Automatically added by conversion
			new ("KLT", 0.01953m),
			new ("HLT", 0.1953m),

			new ("NMB", 89),
			new ("PCS", 124),
			new ("ASV", 5),

			//Automatically added by conversion
			new ("FLAT", 1)
		}, uomQtyDictionary);
	}

	public void TestGetUomQtyDictionaryForInvoiceLine() => CombineAssertions(() =>
	{
		var invoiceLine = CreateNewInvoiceLine(12, "KG", 22, "NMB");
		invoiceLine.JI_CustomsThirdQuantity = 100;
		invoiceLine.JI_CustomsThirdUnitQty = "PCS";
		invoiceLine.JI_CustomsFourthQuantity = 52;
		invoiceLine.JI_CustomsFourthUnitQty = "NMB";
		invoiceLine.JI_CustomsFifthQuantity = 5;
		invoiceLine.JI_CustomsFifthUnitQty = "ASV";
		var uomQtyDictionary = RateCalcUnitOfMeasureAggregator.GetUomQtyDictionaryForInvoiceLine(invoiceLine);
		AssertContainsExactElementsInAnyOrder("Qty Values without fourth and fifth quantities", new KeyValuePair<string, decimal>[] {
			new ("KG", 12),
			new ("KGM", 12),
			//Automatically added by conversion
			new ("GRM", 12000),
			new ("TNE", 0.012m),
			new ("DTN", 0.12m),

			new ("ASV", 5),
			new ("NMB", 74),
			new ("PCS", 100),

			//Automatically added by conversion
			new ("FLAT", 1)
		}, uomQtyDictionary);
	});

	JobComInvoiceLine CreateNewInvoiceLine(ZDecimal customsQty, ZString customsUnit, ZDecimal customsSecondQty, ZString customsSecondUnit)
	{
		var invoiceLine = Factory.New<JobComInvoiceLine>();
		invoiceLine.JI_CustomsQuantity = customsQty;
		invoiceLine.JI_CustomsUnitQty = customsUnit;
		invoiceLine.JI_CustomsSecondQuantity = customsSecondQty;
		invoiceLine.JI_CustomsSecondUnitQty = customsSecondUnit;
		return invoiceLine;
	}
}
