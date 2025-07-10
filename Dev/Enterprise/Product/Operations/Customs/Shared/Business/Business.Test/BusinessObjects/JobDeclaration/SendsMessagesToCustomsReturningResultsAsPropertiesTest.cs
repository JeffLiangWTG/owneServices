using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class SendsMessagesToCustomsReturningResultsAsPropertiesTest : TestCase
	{
		public void TestMergeExecuted()
		{
			SendsMessagesToCustomsReturningResultsAsProperties mergeResultGetter = new SendsMessagesToCustomsReturningResultsAsProperties(true);
			AssertEquals("", mergeResultGetter.MergeResult);
			mergeResultGetter.NotifyUserOfASuccessfulSend("Success!");
			AssertEquals("Entry Merged.", mergeResultGetter.MergeResult);
		}

		public void TestWhichMessagesShouldWeSend()
		{
			SendsMessagesToCustomsReturningResultsAsProperties mergeResultGetter = new SendsMessagesToCustomsReturningResultsAsProperties(true);
			AssertEquals("Length", 0, mergeResultGetter.WhichMessagesShouldWeSend(null).Length);
		}

		public void TestWhichMessagesShouldWeWithdraw()
		{
			SendsMessagesToCustomsReturningResultsAsProperties mergeResultGetter = new SendsMessagesToCustomsReturningResultsAsProperties(true);
			AssertEquals("Length", 0, mergeResultGetter.WhichMessagesShouldWeWithdraw(null).Length);
		}

		public void TestGetAmendmentWithdrawalReasonReturnsNo()
		{
			SendsMessagesToCustomsReturningResultsAsProperties mergeResultGetter = new SendsMessagesToCustomsReturningResultsAsProperties(true);
			AssertEquals("GetAmendmentWithdrawalReason", ContinueWithSave.No, mergeResultGetter.GetAmendmentWithdrawalReason(new AmendmentWithdrawalReason()));
		}
	}
}
