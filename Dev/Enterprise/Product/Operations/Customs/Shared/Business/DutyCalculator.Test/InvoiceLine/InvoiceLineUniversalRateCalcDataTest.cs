using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

[TestedType(typeof(InvoiceLineUniversalRateCalcData))]
[CountrySpecificTest(Enterprise.Core.Constants.CountryCodes.Latvia)]
sealed class InvoiceLineUniversalRateCalcDataTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When InvoiceLine is null", () => new InvoiceLineUniversalRateCalcData(null, Factory.New<RateView>()));
	}

	public void TestDateOfValuation()
	{
		invoice.JZ_ValuationDateOverride = ZDateTime.BrettsBirthday.AddDays(10);
		rateCalcData = new InvoiceLineUniversalRateCalcData(invoiceLine, Factory.New<RateView>());
		AssertEquals(nameof(rateCalcData.DateOfValuation), ZDateTime.BrettsBirthday.AddDays(10), rateCalcData.DateOfValuation);

		var mockInvoiceLine = Factory.NewMoq<BaseJobComInvoiceLine>();
		_ = mockInvoiceLine.Setup(m => m.EffectiveAssessmentDate).Returns(ZDate.BrettsBirthday);
		invoiceLine = mockInvoiceLine.Object;
		rateCalcData = new InvoiceLineUniversalRateCalcData(invoiceLine, Factory.New<RateView>());
		AssertEquals(nameof(rateCalcData.DateOfValuation), ZDate.BrettsBirthday, rateCalcData.DateOfValuation);
	}

	public void TestValueForDuty()
	{
		invoiceLine.JI_CustomsQuantity = 3;
		invoiceLine.JI_CustomsUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		rateCalcData = new InvoiceLineUniversalRateCalcData(invoiceLine, Factory.New<RateView>());
		AssertEquals(nameof(rateCalcData.ValueForDuty), 0m, rateCalcData.ValueForDuty);

		invoiceLine.JI_LinePrice = 2.34m;
		AssertEquals(nameof(rateCalcData.ValueForDuty), 2.34m, rateCalcData.ValueForDuty);

		var dataWithCustomsFormula = rateCalcData as IUniversalRateDataWithCustomsValueFormula;
		AssertNotNull("Data with IUniversalRateDataWithCustomsValueFormula", dataWithCustomsFormula);

		dataWithCustomsFormula.CustomsValueFormula = "1.23456 * [KGM]";
		AssertEquals(nameof(rateCalcData.ValueForDuty), 3.704m, rateCalcData.ValueForDuty);
	}

	public void TestCustomsValue()
	{
		rateCalcData = new InvoiceLineUniversalRateCalcData(invoiceLine, Factory.New<RateView>());
		AssertEquals(nameof(rateCalcData.CustomsValue), 0m, rateCalcData.CustomsValue);

		invoice.JZ_InvoiceAmount = 2.34m;
		invoiceLine.JI_LinePrice = invoice.JZ_InvoiceAmount;
		AssertEquals(nameof(rateCalcData.CustomsValue), 2.34m, rateCalcData.CustomsValue);
	}

	public void TestUnitOfMeasureValueList()
	{
		invoiceLine.JI_CustomsQuantity = 10;
		invoiceLine.JI_CustomsUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		invoiceLine.JI_CustomsSecondQuantity = 2;
		invoiceLine.JI_CustomsSecondUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Hectokilogram;
		invoiceLine.JI_CustomsThirdQuantity = 5;
		invoiceLine.JI_CustomsThirdUnitQty = UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram;
		rateCalcData = new InvoiceLineUniversalRateCalcData(invoiceLine, Factory.New<RateView>());

		CombineAssertions("InvoiceLine UnitOfMeasureValueList", () =>
		{
			AssertEquals("Count", 6, rateCalcData.UnitOfMeasureValueList.Count);

			// Weight - User Entered
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue(UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram, 10m);
			// Weight - Automatically Added By Conversion
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue("KG", 10m);
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue(UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Gram, 10000m);
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue(UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Tonne, 0.01m);

			// Automatically Added By Conversion
			rateCalcData.UnitOfMeasureValueList.AssertDictionaryValue(UniversalReferenceConstants.FlatRate, 1m);
		});
	}

	public void TestCountrySpecificValueList()
	{
		rateCalcData = new InvoiceLineUniversalRateCalcData(invoiceLine, Factory.New<RateView>());
		AssertEquals("CountrySpecificValueList.Count", 1, rateCalcData.CountrySpecificValueList.Count);
		AssertEquals("CountrySpecificValueList[NIHIL]", 0m, rateCalcData.CountrySpecificValueList["NIHIL"]);
	}

	public void TestMeursingExpressionList_WithNoMeursingExpressionsInRateFormula()
	{
		rateCalcData = new InvoiceLineUniversalRateCalcData(invoiceLine, Factory.New<RateView>());

		var meursingExpressionList = rateCalcData.MeursingExpressionList;
		AssertArrayEqualsByElements(nameof(rateCalcData.MeursingExpressionList), new Dictionary<string, string>().ToArray(), meursingExpressionList.ToArray());
		AssertSame("Cached MeursingExpressionList", meursingExpressionList, rateCalcData.MeursingExpressionList);
	}

	public void TestAdditionalInformationList()
	{
		var entryLine = Factory.New<CusEntryLine>();
		rateCalcData = new InvoiceLineUniversalRateCalcData(invoiceLine, Factory.New<RateView>());
		AssertEquals("AdditionalInformationList.Count", 0, rateCalcData.AdditionalInformationList.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<BaseJobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		invoice = declaration.Invoices.AddNew();
		invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
		invoiceLine = invoice.InvoiceLines.AddNew();
	}

	BaseJobDeclaration declaration;
	BaseJobComInvoiceHeader invoice;
	BaseJobComInvoiceLine invoiceLine;
	IUniversalRateCalcData rateCalcData;
}
