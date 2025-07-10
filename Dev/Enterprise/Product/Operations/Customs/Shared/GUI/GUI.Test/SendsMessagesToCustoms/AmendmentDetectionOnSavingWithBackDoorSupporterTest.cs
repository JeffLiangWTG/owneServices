using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class AmendmentDetectionOnSavingWithBackDoorSupporterTest : AmendmentDetectionOnSavingBaseTest
	{
		public virtual void TestContinueWithSaveForSavingOptionsIsCancelled()
		{
			IBackDoorSavingSupportableBizObj topLevelBizObj = GetBizObjWithMessagesToSendButWithCancelledSavingOptions();
			AssertEquals("PreCondition:BackDoor saving should be supported", true, topLevelBizObj.SupportBackDoorForSavingWhenAmendmentDetected);

			IMessageManager manager = topLevelBizObj.GetMessageManagerForAmendmentDetection();
			RequiredMessagesInformation info = manager.GetRequiredMessagesInformation();
			IDeferredAmendmentSavingOptions savingOptions = manager.GetDeferredAmendmentSavingOptions();
			savingOptions.IsCancelled = true;

			AssertEquals("SavingOptions is cancelled", true, savingOptions.IsCancelled);
			AssertEquals("PreCondition:Message to send", true, info.HasMessagesToSend);
			AssertEquals("PreCondition:No Error notification", false, info.AllNotifications.ContainsError());
			AssertEquals("PreCondition:No error from BizObj", false, ((BusinessObject)topLevelBizObj).HasErrors);

			using (IShowPreSaveDialog formOrPlugIn = GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(topLevelBizObj))
			{
				AssertEquals("Should not be able to save as users cancelled", ContinueWithSave.No, formOrPlugIn.ShowPreSaveDialogs());
			}
		}

		public virtual void TestContinueWithSaveForSavingWithoutSending()
		{
			IBackDoorSavingSupportableBizObj topLevelBizObj = GetBizObjWithMessagesToSendButWithSaveWithoutSendingOptions();
			AssertEquals("PreCondition:BackDoor saving should be supported", true, topLevelBizObj.SupportBackDoorForSavingWhenAmendmentDetected);

			IMessageManager manager = topLevelBizObj.GetMessageManagerForAmendmentDetection();
			RequiredMessagesInformation info = manager.GetRequiredMessagesInformation();
			AssertEquals("PreCondition:Message to send", true, info.HasMessagesToSend);
			AssertEquals("PreCondition:No Error notification", false, info.AllNotifications.ContainsError());
			AssertEquals("PreCondition:No error from BizObj", false, ((BusinessObject)topLevelBizObj).HasErrors);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			IDeferredAmendmentSavingOptions savingOptions = manager.GetDeferredAmendmentSavingOptions();
			savingOptions.SetSaveWithEntryChangesValueForTestingTo(true);
			if (savingOptions.ShouldTakeReasonForSavingWithoutSendingSeparately)
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);//Yes to AmendmentWithdrawalReason
			}

			using (IShowPreSaveDialog formOrPlugIn = GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(topLevelBizObj))
			{
				AssertEquals("Should be able to save as users chose to save without sending", ContinueWithSave.Yes, formOrPlugIn.ShowPreSaveDialogs());
			}
		}

		/// <summary>
		/// The implementation for IMessageManageableBizObj.GetDeferredAmendmentSavingOptions() can be mocked
		/// </summary>
		protected abstract IBackDoorSavingSupportableBizObj GetBizObjWithMessagesToSendButWithCancelledSavingOptions();

		/// <summary>
		/// The implementation for IMessageManageableBizObj.GetDeferredAmendmentSavingOptions() can be mocked
		/// </summary>
		protected abstract IBackDoorSavingSupportableBizObj GetBizObjWithMessagesToSendButWithSaveWithoutSendingOptions();
	}
}
