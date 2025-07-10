using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(RateCalcUnitOfMeasureAggregator))]
sealed class RateCalcUnitOfMeasureAggregatorTest : TestCaseWithFactory
{
	public void TestGetUomQtyDictionary()
	{
		var invLine1 = NewInvoiceLine(90, "KG", 2, "DTN");
		invLine1.JI_CustomsThirdQuantity = 100000;
		invLine1.JI_CustomsThirdUnitQty = "KG";
		invLine1.JI_CustomsFourthQuantity = 100;
		invLine1.JI_CustomsFourthUnitQty = "LTR";
		var invLine2 = NewInvoiceLine(30, "KG", 40, "LTR");
		invLine2.JI_CustomsThirdQuantity = 500000;
		invLine2.JI_CustomsThirdUnitQty = "LTR";
		invLine2.JI_CustomsFourthQuantity = 220;
		invLine2.JI_CustomsFourthUnitQty = "KG";
		var invLine3 = NewInvoiceLine(0.5, "HLT", 120, "LTR");
		invLine3.JI_CustomsThirdQuantity = 3;
		invLine3.JI_CustomsThirdUnitQty = "DTN";
		invLine3.JI_CustomsFourthQuantity = 330;
		invLine3.JI_CustomsFourthUnitQty = "KG";
		var invLine4 = NewInvoiceLine(0, "KG", 0.8, "ASVX");
		invLine4.JI_CustomsFourthQuantity = 10;
		invLine4.JI_CustomsFourthUnitQty = "HG";
		var invLine5 = NewInvoiceLine(0, "KG", 100, "LTR");
		invLine5.JI_CustomsThirdQuantity = 4200m;
		invLine5.JI_CustomsThirdUnitQty = "ASVX";
		invLine5.JI_CustomsFourthQuantity = 0.168m;
		invLine5.JI_CustomsFourthUnitQty = "FC1X";
		invLine5.JI_CustomsFifthQuantity = 25000m;
		invLine5.JI_CustomsFifthUnitQty = "GP1";

		var entryLine = Factory.New<CusEntryLine>();
		entryLine.InvoiceLines.Add(invLine1);
		entryLine.InvoiceLines.Add(invLine2);
		entryLine.InvoiceLines.Add(invLine3);
		entryLine.InvoiceLines.Add(invLine4);
		entryLine.InvoiceLines.Add(invLine5);

		var uomQtyAggregator = new RateCalcUnitOfMeasureAggregator();
		var uomQtyDictionary = uomQtyAggregator.GetUomQtyDictionary(entryLine);

		CombineAssertions("UnitOfMeasureValueList", () =>
		{
			AssertEquals("Count", 14, uomQtyDictionary.Count);

			// Weight - User Entered
			uomQtyDictionary.AssertDictionaryValue("KG", 450m);
			uomQtyDictionary.AssertDictionaryValue("DTN", 5m);
			// Weight - Automatically Added By Conversion
			uomQtyDictionary.AssertDictionaryValue("KGM", 450m);
			uomQtyDictionary.AssertDictionaryValue("GRM", 450000m);
			uomQtyDictionary.AssertDictionaryValue("TNE", 0.45m);

			// Volume - User Entered
			uomQtyDictionary.AssertDictionaryValue("LTR", 360m);
			uomQtyDictionary.AssertDictionaryValue("HLT", 0.5m);
			// Volume - Automatically Added By Conversion
			uomQtyDictionary.AssertDictionaryValue("KLT", 0.36m);

			// Alcohol - User Entered
			uomQtyDictionary.AssertDictionaryValue("ASVX", 4200.8m);
			uomQtyDictionary.AssertDictionaryValue("HG", 10m);
			// Alcohol - Automatically Added By Conversion
			uomQtyDictionary.AssertDictionaryValue("LPA", 4200.8m);

			// Gross Production - User Entered
			uomQtyDictionary.AssertDictionaryValue("GP1", 25000m);
			// Factor Hectolitre - User Entered
			uomQtyDictionary.AssertDictionaryValue("FC1X", 0.168m);

			// Flat - for flat rate
			uomQtyDictionary.AssertDictionaryValue("FLAT", 1m);
		});

		uomQtyDictionary = uomQtyAggregator.GetUomQtyDictionary(invLine1);
		CombineAssertions("InvoiceLine UnitOfMeasureValueList", () =>
		{
			AssertEquals("Count", 9, uomQtyDictionary.Count);

			// Weight - User Entered
			uomQtyDictionary.AssertDictionaryValue("KG", 90m);
			uomQtyDictionary.AssertDictionaryValue("DTN", 2m);
			// Weight - Automatically Added By Conversion
			uomQtyDictionary.AssertDictionaryValue("KGM", 90m);
			uomQtyDictionary.AssertDictionaryValue("GRM", 90000m);
			uomQtyDictionary.AssertDictionaryValue("TNE", 0.09m);

			// Volume - User Entered
			uomQtyDictionary.AssertDictionaryValue("LTR", 100m);
			uomQtyDictionary.AssertDictionaryValue("HLT", 1m);

			// Volume - Automatically Added By Conversion
			uomQtyDictionary.AssertDictionaryValue("KLT", 0.1m);

			// Flat - for flat rate
			uomQtyDictionary.AssertDictionaryValue("FLAT", 1m);
		});
	}

	BaseJobComInvoiceLine NewInvoiceLine(ZDecimal customsQty, ZString customsUnit, ZDecimal customsSecondQty, ZString customsSecondUnit)
	{
		var invLine = Factory.New<BaseJobComInvoiceLine>();
		invLine.JI_CustomsQuantity = customsQty;
		invLine.JI_CustomsUnitQty = customsUnit;
		invLine.JI_CustomsSecondQuantity = customsSecondQty;
		invLine.JI_CustomsSecondUnitQty = customsSecondUnit;
		return invLine;
	}

	public void TestFlatRate()
	{
		var invLine1 = Factory.New<BaseJobComInvoiceLine>();
		invLine1.JI_CustomsQuantity = 10;
		invLine1.JI_CustomsUnitQty = "KG";
		var entryLine = Factory.New<CusEntryLine>();
		entryLine.InvoiceLines.Add(invLine1);

		var uomQtyAggregator = new RateCalcUnitOfMeasureAggregator();
		var uomQtyDictionary = uomQtyAggregator.GetUomQtyDictionary(entryLine);

		uomQtyDictionary.AssertDictionaryValue("KG", 10m);
		uomQtyDictionary.AssertDictionaryValue("FLAT", 1m);

		invLine1.JI_CustomsQuantity = 10000;
		uomQtyDictionary = uomQtyAggregator.GetUomQtyDictionary(entryLine);
		uomQtyDictionary.AssertDictionaryValue("KG", 10000m);
		uomQtyDictionary.AssertDictionaryValue("FLAT", 1m);
	}
}
