using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CUSDECItemDetailsFeeWrapper))]
sealed class CUSDECItemDetailsFeeWrapperTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When entryLineFee is null", () => _ = new CUSDECItemDetailsFeeWrapper(null));
		AssertNoExceptionThrown("When happy path", () => _ = new CUSDECItemDetailsFeeWrapper(entryLineFee));
	});

	public void TestFeeAmount() => CombineAssertions(() =>
	{
		entryLineFee.CF_ChargeAmount = 27m;
		var fee1 = GetTestItemDetailsFee();
		AssertEquals("FeeAmount, when no decimals", "27", fee1.FeeAmount);

		entryLineFee.CF_ChargeAmount = 27.42m;
		var fee2 = GetTestItemDetailsFee();
		AssertEquals("FeeAmount, when has decimals", "27,42", fee2.FeeAmount);
	});

	public void TestFeeType()
	{
		entryLineFee.CF_ChargeType = "FA200";
		var fee = GetTestItemDetailsFee();
		AssertEquals("FeeType", "FA", fee.FeeType);
	}

	public void TestFeeTypeSequence()
	{
		entryLineFee.CF_ChargeType = "FA200";
		var fee = GetTestItemDetailsFee();
		AssertEquals("FeeTypeSequence", "200", fee.FeeTypeSequence);
	}

	public void TestFeeBaseValueForCalculation() => CombineAssertions(() =>
	{
		entryLineFee.CF_BaseValue = 10681m;
		var fee1 = GetTestItemDetailsFee();
		AssertEquals("FeeBaseValueForCalculation, when no decimals", "10681", fee1.FeeBaseValueForCalculation);

		entryLineFee.CF_BaseValue = 106.81m;
		var fee2 = GetTestItemDetailsFee();
		AssertEquals("FeeBaseValueForCalculation, when has decimals", "106,81", fee2.FeeBaseValueForCalculation);
	});

	public void TestFeeRate() => CombineAssertions(() =>
	{
		entryLineFee.CF_Rate = 1m;
		var fee1 = GetTestItemDetailsFee();
		AssertEquals("FeeRate, when no decimals", "1", fee1.FeeRate);

		entryLineFee.CF_Rate = 0.25m;
		var fee2 = GetTestItemDetailsFee();
		AssertEquals("FeeRate, when has decimals", "0,25", fee2.FeeRate);
	});

	public void TestFeeRateWhenRateGivenInFractionsOfKroner()
	{
		var testHelper = new RefCusTariffTestHelper(Factory);
		testHelper.SetupGenericTariffData();

		var declaration = Factory.New<JobDeclaration>();
		var fee = GetTestItemDetailsFee();
		declaration.JE_MessageType = "IMP";
		invoiceLine.Declaration.JE_MessageType = "IMP";
		invoiceLine.JI_Tariff = "22222222";
		entryLineFee.CF_Rate = 1.2345m;

		CombineAssertions(() =>
		{
			entryLineFee.CF_ChargeType = "ZX201";
			AssertEquals("ZX201 - Has RateGivenInFractionsOfKroner", expected: "123,45", fee.FeeRate);

			entryLineFee.CF_ChargeType = "ZX200";
			AssertEquals("ZX200 - No RateGivenInFractionsOfKroner", expected: "1,234", fee.FeeRate);
		});
	}

	CUSDECItemDetailsFeeWrapper GetTestItemDetailsFee()
	{
		return new(entryLineFee);
	}

	JobComInvoiceLine AddNewInvoiceLine()
	{
		var newInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		newInvoiceLine.JI_CL = entryLine.PK;
		entryLine.InvoiceLines.Add(newInvoiceLine);
		return newInvoiceLine;
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryLine = entryHeader.MergedLines.AddNew();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = AddNewInvoiceLine();
		entryLineFee = entryLine.Fees.AddNew();
	}

	CusEntryLineFee entryLineFee;
	JobDeclaration declaration;
	CusEntryHeader entryHeader;
	CusEntryLine entryLine;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
}
