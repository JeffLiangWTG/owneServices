using System;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NODocSADHLineTax))]
sealed class NODocSADHLineTaxTest : DocBaseWrapperTest
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => NODocSADHLineTax.New(null, Factory));
	}

	protected override DocBaseWrapper GetNewDocumentWrapper() => NODocSADHLineTax.New(Fee, Factory);

	public void TestWrapper()
	{
		var wrapper = NODocSADHLineTax.New(Fee, Factory);
		CombineAssertions(() =>
		{
			AssertEquals("Type", "TL", wrapper.Type);
			AssertEquals("BaseValue", "20,00", wrapper.BaseValue);
			AssertEquals("Rate in %", "0,05%", wrapper.Rate);
			AssertEquals("ChargeAmount for %", 10, wrapper.ChargeAmount);

			fee.CF_MethodOfPayment = "K";
			AssertEquals("Rate in Kg", "0,05K", wrapper.Rate);
			AssertEquals("ChargeAmount for Kg", 10, wrapper.ChargeAmount);
		});
	}

	public void TestWrapperTotals()
	{
		var wrapper = new NODocSADHLineTax(Fee, 20, Factory);
		CombineAssertions(() =>
		{
			AssertEquals("Type", "TL", wrapper.Type);
			AssertEquals("ChargeAmount", 20, wrapper.ChargeAmountTotal);
		});
	}

	public void TestIDutyCategory() => CombineAssertions(() =>
	{
		var wrapper = NODocSADHLineTax.New(Fee, Factory);
		Fee.CF_ChargeType = "TL1";
		var dutyCategoryWrapper = (IDutyCategory)wrapper;
		AssertEquals("TL1 - IsCustomsDuty", expected: true, dutyCategoryWrapper.IsCustomsDuty);
		AssertEquals("TL1 - IsAgriculturalDuty", expected: false, dutyCategoryWrapper.IsAgriculturalDuty);
		AssertEquals("TL1 - IsExciseDuty", expected: false, dutyCategoryWrapper.IsExciseDuty);
		AssertEquals("TL1 - IsVAT", expected: false, dutyCategoryWrapper.IsVAT);

		Fee.CF_ChargeType = "RT100";
		AssertEquals("RT100 - IsCustomsDuty", expected: false, dutyCategoryWrapper.IsCustomsDuty);
		AssertEquals("RT100 - IsAgriculturalDuty", expected: true, dutyCategoryWrapper.IsAgriculturalDuty);
		AssertEquals("RT100 - IsExciseDuty", expected: false, dutyCategoryWrapper.IsExciseDuty);
		AssertEquals("RT100 - IsVAT", expected: false, dutyCategoryWrapper.IsVAT);
	});

	CusEntryLineFee Fee
	{
		get
		{
			if (fee == null)
			{
				fee = Factory.New<CusEntryLineFee>();
				fee.CF_ChargeAmount = 10.00m;
				fee.CF_ChargeType = NOCustomDutyCodeList.Codes.TL1;
				fee.CF_BaseValue = 20.00m;
				fee.CF_Rate = 0.05m;
				fee.CF_MethodOfPayment = "%";
			}
			return fee;
		}
	}
	CusEntryLineFee fee;
}
