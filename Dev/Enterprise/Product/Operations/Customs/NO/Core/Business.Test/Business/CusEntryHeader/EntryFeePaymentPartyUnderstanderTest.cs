using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(EntryFeePaymentPartyUnderstander))]
sealed class EntryFeePaymentPartyUnderstanderTest : TestCaseWithFactory
{
	public void TestShouldBrokerPayThisFee()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var entryLine = entryHeader.AllEntryLines.AddNew();
		var entryLineFee = entryLine.Fees.AddNew();
		var logger = new DetailedLoggerForTest();

		CombineAssertions(() =>
		{
			entryLineFee.CF_IsLandedCostOnly = false;
			entryHeader.CH_PaymentMethod = NOPaymentMethodCodeList.Codes.ForwardersDayCredit;

			var mergedLines = entryHeader.MergedLines.Cast<CusEntryLine>().ToArray();
			var isLandedCostOnly = mergedLines.SelectMany(x => x.Fees).Cast<CusEntryLineFee>().All(x => x.CF_IsLandedCostOnly);
			AssertEquals("For the first test case, islandedCostsOnly should be false", false, isLandedCostOnly);

			AssertShouldBrokerPayThisFeeResponseAndLoggerForPaymentMethod(entryHeader, logger, NOPaymentMethodCodeList.Codes.ForwardersDayCredit, true, "D (with IslandedCostsOnly != 1)");

			entryLineFee.CF_IsLandedCostOnly = true;

			mergedLines = entryHeader.MergedLines.Cast<CusEntryLine>().ToArray();
			isLandedCostOnly = mergedLines.SelectMany(x => x.Fees).Cast<CusEntryLineFee>().All(x => x.CF_IsLandedCostOnly);
			AssertEquals("For the second test case, islandedCostsOnly should be false", true, isLandedCostOnly);

			AssertShouldBrokerPayThisFeeResponseAndLoggerForPaymentMethod(entryHeader, logger, NOPaymentMethodCodeList.Codes.ForwardersDayCredit, false, "D (with IslandedCostsOnly == 1)");
		});
	}

	public void AssertShouldBrokerPayThisFeeResponseAndLoggerForPaymentMethod(
		CusEntryHeader entryHeader,
		DetailedLoggerForTest logger,
		ZString paymentMethod,
		bool expectedIncluded,
		string assertMessage)
	{
		var expectedLogText = "Fee A00 with MoP=Y with deferral payment party " + (paymentMethod.IsEmpty ? (ZString)"empty" : paymentMethod) +
							  (expectedIncluded ? " is always paid by broker - included in rating" : " is never paid by broker - excluded from rating");

		AssertEquals("When CH_PaymentMethod is " + assertMessage + " ShouldBrokerPayThisFee should return",
					 expectedIncluded,
					 entryHeader.ShouldBrokerPayThisFee("A00", "Y", logger));

		AssertEquals("When CH_PaymentMethod is " + assertMessage + " logger should contain a log indicating the fee is " +
					(expectedIncluded ? "" : "not") + " included in rating",
					 true,
					 logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == expectedLogText));

		logger.Logs.Clear();
	}
}
