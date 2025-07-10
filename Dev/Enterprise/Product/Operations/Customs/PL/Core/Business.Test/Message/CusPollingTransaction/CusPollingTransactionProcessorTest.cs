using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using InterchangeStatus = Enterprise.Messaging.Business.EDIInterchange.Status;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class CusPollingTransactionProcessorTest : TestCaseWithFactory
{
	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestNothingHappensWhenNowAboveNextAttempt()
		=> CusPollingTransactionTestHelper.AssertNothingHappensWhenNowAboveNextAttempt(Factory, RunProcessing);

	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestMessageSentWhenNowEqualToNextAttempt()
		=> CusPollingTransactionTestHelper.AssertMessageSentWhenNowEqualToNextAttempt(Factory, RunProcessing);

	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestMessageSentWhenNowBelowNextAttempt()
		=> CusPollingTransactionTestHelper.AssertMessageSentWhenNowBelowNextAttempt(Factory, RunProcessing);

	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestBacklogsCreatedWhenNowBelowNextAttemptMinusOneHour()
		=> CusPollingTransactionTestHelper.AssertBacklogsCreatedWhenNowBelowNextAttemptMinusOneHour(Factory, RunProcessing);

	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestTransactionInterchangeReceived()
		=> CusPollingTransactionTestHelper.AssertTransactionInterchangeReceived(Factory, RunProcessing);

	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestTransactionInterchangeFailed_CAN()
		=> CusPollingTransactionTestHelper.AssertTransactionInterchangeFailed(Factory, RunProcessing, InterchangeStatus.Cancelled);

	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestTransactionInterchangeFailed_DCD()
		=> CusPollingTransactionTestHelper.AssertTransactionInterchangeFailed(Factory, RunProcessing, InterchangeStatus.Discarded);

	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestTransactionInterchangeFailed_FAL()
		=> CusPollingTransactionTestHelper.AssertTransactionInterchangeFailed(Factory, RunProcessing, InterchangeStatus.Failed);

	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestTransactionInterchangeFailed_ERR()
		=> CusPollingTransactionTestHelper.AssertTransactionInterchangeFailed(Factory, RunProcessing, InterchangeStatus.Error);

	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestTransactionInterchangeFailed_REJ()
		=> CusPollingTransactionTestHelper.AssertTransactionInterchangeFailed(Factory, RunProcessing, InterchangeStatus.SyntaxRejected);

	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestPlcTimeout()
		=> CusPollingTransactionTestHelper.AssertPlcTimeout(Factory, RunProcessing);

	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	public void TestBlgTimeout()
		=> CusPollingTransactionTestHelper.AssertBlgTimeout(Factory, RunProcessing);

	[TestDate(2025, 1, 6, 11, 11, 0)]
	[TestTimeZoneUNLOCO("AUSYD")]
	[ExpectNoExceptions]
	public void TestPlcTimeout_NoConcurrencyError()
	{
		var staff = Factory.New<GlbStaff>();
		staff.GS_Code = "~XT";
		staff.GS_LoginName = "s_xt";
		staff.GS_IsActive = true;
		staff.StaffPlainTextPassword = "P@ssw0rd";
		staff.GS_IsSystemAccount = true;
		Factory.Save();

		CusPollingTransactionTestHelper.AssertPlcTimeout(Factory, branch =>
		{
			var savingEventHandler = new BusinessObjectFactory.SavingEventHandler(_ => CusPollingTransactionTestHelper.EmulateXtProcessing(staff.GS_LoginName));

			try
			{
				Factory.Saving += savingEventHandler;
				RunProcessing(branch);
			}
			finally
			{
				Factory.Saving -= savingEventHandler;
			}
		});
	}

	void RunProcessing(GlbBranch branch)
	{
		var serviceLogger = new LoggingInformation();
		using var testDisposableEnvironment = DisposableEnvironment.ForBranch(branch.PK.ToGuid());
		var testTransactionProcessor = new CusPollingTransactionProcessor(serviceLogger, CancellationToken.None);
		testTransactionProcessor.ProcessForBranch(Factory);
		Factory.Save();
	}
}
