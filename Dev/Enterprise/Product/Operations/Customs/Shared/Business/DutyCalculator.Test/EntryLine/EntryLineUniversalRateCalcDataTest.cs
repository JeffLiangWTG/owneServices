using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(EntryLineUniversalRateCalcData))]
[CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Latvia)]
sealed class EntryLineUniversalRateCalcDataTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When Entity is null", () => new EntryLineUniversalRateCalcData(null, Factory.New<RateView>()));
	}

	[TestDate(2020, 1, 31)]
	public void TestDateOfValuation()
	{
		var entryLine = Factory.New<CusEntryLine>();
		var rateCalcData = GetRateCalcDataForTest(entryLine, dummyRateView);
		AssertEquals("DateOfValuation", TestDateAttribute.Date, rateCalcData.DateOfValuation);

		var invHeader = Factory.New<BaseJobComInvoiceHeader>();
		invHeader.JZ_ValuationDateOverride = new ZDateTime(2016, 2, 29);
		entryLine.InvoiceLines.Add(invHeader.JobComInvoiceLines.AddNew());
		rateCalcData = GetRateCalcDataForTest(entryLine, dummyRateView);
		AssertEquals("DateOfValuation", invHeader.JZ_ValuationDateOverride, rateCalcData.DateOfValuation);
	}

	public void TestUnitOfMeasureValueList()
	{
		var invLine1 = Factory.New<BaseJobComInvoiceLine>();
		invLine1.JI_CustomsQuantity = 10;
		invLine1.JI_CustomsUnitQty = "KGM";
		invLine1.JI_CustomsSecondQuantity = 2;
		invLine1.JI_CustomsSecondUnitQty = "DTN";
		invLine1.JI_CustomsThirdQuantity = 5;
		invLine1.JI_CustomsThirdUnitQty = "KGM";
		var invLine2 = Factory.New<BaseJobComInvoiceLine>();
		invLine2.JI_CustomsQuantity = 3;
		invLine2.JI_CustomsUnitQty = "KGM";
		invLine2.JI_CustomsSecondQuantity = 40;
		invLine2.JI_CustomsSecondUnitQty = "LTR";
		var entryLine = Factory.New<CusEntryLine>();
		entryLine.InvoiceLines.Add(invLine1);
		entryLine.InvoiceLines.Add(invLine2);

		var rateCalcData = GetRateCalcDataForTest(entryLine, dummyRateView);

		CombineAssertions("UnitOfMeasureValueList", () =>
		{
			AssertEquals("Count", 9, rateCalcData.UnitOfMeasureValueList.Count);

			// Weight - User Entered
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue("KGM", 13m);
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue("DTN", 2m);
			// Weight - Automatically Added By Conversion
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue("KG", 13m);
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue("GRM", 13000m);
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue("TNE", 0.013m);

			// Volume - User Entered
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue("LTR", 40m);
			// Volume - Automatically Added By Conversion
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue("HLT", 0.4m);
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue("KLT", 0.04m);
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue("FLAT", 1m);
		});
	}

	public void TestCountrySpecificValueList()
	{
		var entryLine = Factory.New<CusEntryLine>();
		var rateCalcData = GetRateCalcDataForTest(entryLine, dummyRateView);
		AssertEquals("CountrySpecificValueList.Count", 1, rateCalcData.CountrySpecificValueList.Count);
		AssertEquals("CountrySpecificValueList[NIHIL]", 0m, rateCalcData.CountrySpecificValueList["NIHIL"]);
	}

	public void TestAdditionalInformationList()
	{
		var entryLine = Factory.New<CusEntryLine>();
		var rateCalcData = GetRateCalcDataForTest(entryLine, dummyRateView);
		AssertEquals("AdditionalInformationList.Count", 0, rateCalcData.AdditionalInformationList.Count);
	}

	public void TestMeursingExpressionList_WithNoMeursingExpressionsInRateFormula()
	{
		var entryLine = Factory.New<BaseJobDeclaration>()
			.CustomsEntryHeaders.AddNew()
			.MergedLines.AddNew();

		var rateCalcData = GetRateCalcDataForTest(entryLine, dummyRateView);

		var meursingExpressionList = rateCalcData.MeursingExpressionList;
		AssertArrayEqualsByElements(nameof(rateCalcData.MeursingExpressionList), new Dictionary<string, string>().ToArray(), meursingExpressionList.ToArray());
		AssertSame("Cached MeursingExpressionList", meursingExpressionList, rateCalcData.MeursingExpressionList);
	}

	protected override void SetUp()
	{
		base.SetUp();
		dummyRateView = Factory.New<RateView>();
	}

	RateView dummyRateView;

	IUniversalRateCalcData GetRateCalcDataForTest(CusEntryLine entryLine, RateView rateView) => new EntryLineUniversalRateCalcData(entryLine, rateView);
}
