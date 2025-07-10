using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(GuaranteedChargesCalculator))]
sealed class GuaranteedChargesCalculatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should throw exception when parent is null", () => new GuaranteedChargesCalculator(null, 0m));

		var entryLineFee = Factory.New<CusEntryLineFee>();
		AssertNoExceptionThrown("No exception expected", () => new GuaranteedChargesCalculator(entryLineFee, 0m));
	}

	public void TestRateCode()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();
		var calculator = new GuaranteedChargesCalculator(entryLineFee, 0m);

		const string expectedCode = TaxTypeList.Codes.GuaranteedCharges;
		AssertEquals("Calculator rate code", expectedCode, calculator.RateCode);
	}

	public void TestCalculateExtraFees()
	{
		var testAmount = 1000m;
		var testRate = 0.2m;
		var testMethodOfPayment = PLMethodOfPaymentList.Codes.L;

		var entryLineFee = Factory.New<CusEntryLineFee>();
		entryLineFee.CF_ChargeAmount = testAmount;
		entryLineFee.CF_MethodOfPayment = testMethodOfPayment;
		var calculator = new GuaranteedChargesCalculator(entryLineFee, testRate);

		var calculationResults = calculator.CalculateExtraFees();

		CombineAssertions(() =>
		{
			AssertEquals("Results count", 1, calculationResults.Count());

			var firstResult = calculationResults.First();

			AssertEquals("Base value", testAmount, firstResult.BaseValue);

			AssertEquals("Rate", testRate, firstResult.Rate);

			var expectedAmount = testAmount * testRate;
			AssertEquals("Amount", expectedAmount, firstResult.Amount);

			AssertEquals("Method of payment", testMethodOfPayment, firstResult.MethodOfPayment);

			var expectedMethodOfCalculation = "%";
			AssertEquals("Method of calculation", expectedMethodOfCalculation, firstResult.MethodOfCalculation);
		});
	}
}
