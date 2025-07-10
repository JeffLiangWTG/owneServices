using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	#region WhsErrorTypesTest class
	public class WhsErrorTypesTest : TestCase
	{
		public void TestMessages()
		{
			AssertEquals("ZErrorMessageBox", WhsErrorTypes.ZErrorMessageBox.Message);
			AssertEquals("This Job is Finalised.", WhsErrorTypes.JobIsFinalised.Message);
			AssertEquals("This line is Finalized.", WhsErrorTypes.LineIsFinalised.Message);
			AssertEquals("Finalise", WhsErrorTypes.FinaliseZErrorMessageBox.Message);
			AssertEquals("No lines have been entered. A Docket must have lines before it can be finalized.", WhsErrorTypes.NoLinesEntered.Message);
			AssertEquals("You do not have the Security Rights to perform this operation.", WhsErrorTypes.NoSecurityRights.Message);
			AssertEquals("Cannot perform this operation because the job is finalized or canceled.", WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled.Message);
			AssertEquals("Cannot perform this operation because the selected lines are finalized.", WhsErrorTypes.CannotPerformThisOperationBecauseAllSelectedLinesAreFinalised.Message);
			AssertEquals("Cannot perform this operation because there is no Warehouse or Client entered.", WhsErrorTypes.CannotPerformThisOperationBecauseNoWarehouseOrClient.Message);
			AssertEquals("Cannot perform this operation because the lines cannot be added.", WhsErrorTypes.CannotPerformThisOperationBecauseCollectionDoesNotAllowNew.Message);
			AssertEquals("Error has occurred while Adjusting in for the new client.", WhsErrorTypes.CannotFinaliseChildAdjustment.Message);
			AssertEquals("The docket has been updated by another job. Reload this docket before it can be finalized.", WhsErrorTypes.CannotFinaliseWithoutReload.Message);
		}
	}

	#endregion

	#region NotificationBufferWithDefaultResponseTestCase

	public class NotificationBufferWithDefaultResponseTestCase : TestCase
	{
		public void TestConstructor()
		{
			DefaultNotification = new NotificationBufferWithDefaultResponse();
			Assert("Default response shoult init true", DefaultNotification.DefaultResponse);
			DefaultNotification = new NotificationBufferWithDefaultResponse(true);
			Assert("Set default response thru constructor", DefaultNotification.DefaultResponse);
			DefaultNotification = new NotificationBufferWithDefaultResponse(false);
			Assert("Set default response false thru construcor", !DefaultNotification.DefaultResponse);
		}

		public void TestPropertyDefaultResponse()
		{
			DefaultNotification = new NotificationBufferWithDefaultResponse();
			DefaultNotification.DefaultResponse = true;
			Assert("DefaultResponse state persists", DefaultNotification.DefaultResponse);

			DefaultNotification.DefaultResponse = false;
			Assert("DefaultResponse state persists", !DefaultNotification.DefaultResponse);
		}

		public void TestQueryUserBehavaiour()
		{
			DefaultNotification = new NotificationBufferWithDefaultResponse(true);
			Assert("Precondition - Default should initialise true", DefaultNotification.DefaultResponse);

			NotificationManager notificationManager = new NotificationManager();
			NotificationBuffer normalNortification = new NotificationBuffer();

			notificationManager.Push(normalNortification);
			QueryUserMsgBoxEventArgs msgBoxEventArgs = new QueryUserMsgBoxEventArgs("Hallo", false);
			notificationManager.Peek.QueryUser(msgBoxEventArgs);
			Assert("Response unaltered with NotificationManager ", !msgBoxEventArgs.Response);

			notificationManager.Push(DefaultNotification);
			notificationManager.Peek.QueryUser(msgBoxEventArgs);
			Assert("Response altered to defualt NotificationManager ", msgBoxEventArgs.Response);
		}

		#region Implementation

		NotificationBufferWithDefaultResponse DefaultNotification;

		#endregion
	}

	#endregion
}
