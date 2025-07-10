using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI.Testing
{
	public abstract class AmendmentDetectionOnSavingBaseTest : TestCaseWithFactory
	{
		public void TestContinueWithSaveWhenThereIsNoMessagesToSend()
		{
			var topLevelBizObj = GetBizObjWithoutAnyMessagesToSend();

			var info = topLevelBizObj.GetMessageManagerForAmendmentDetection().GetRequiredMessagesInformation();
			AssertEquals("PreCondition:No Message to send", false, info.HasMessagesToSend);

			using (IShowPreSaveDialog formOrPlugIn = GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(topLevelBizObj))
			{
				AssertEquals("Should be able to save when there is no message to send", ContinueWithSave.Yes, formOrPlugIn.ShowPreSaveDialogs());
			}
		}

		public virtual void TestContinueWithSaveWhenThereAreErrorsNotificationsFromManager()
		{
			var topLevelBizObj = GetBizObjWithMessagesToSendButWithErrorsFromMessageManager();

			var info = topLevelBizObj.GetMessageManagerForAmendmentDetection().GetRequiredMessagesInformation();
			AssertEquals("PreCondition:Message to send", true, info.HasMessagesToSend);
			AssertEquals("But there are error notifications", true, info.AllNotifications.ContainsError());

			using (IShowPreSaveDialog formOrPlugIn = GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(topLevelBizObj))
			{
				AssertEquals("Should not be able to save when there is an error notification", ContinueWithSave.No, formOrPlugIn.ShowPreSaveDialogs());
			}
		}

		public void TestContinueWithSaveWhenUsersCancelOnWhichMessageToSendDialogOrWarningNotification()
		{
			var topLevelBizObj = GetBizObjWithMessagesToSendButWithWarningsFromMessageManager();

			var info = topLevelBizObj.GetMessageManagerForAmendmentDetection().GetRequiredMessagesInformation();
			AssertEquals("PreCondition:Message to send", true, info.HasMessagesToSend);
			AssertEquals("PreCondition:No Error notification", false, info.AllNotifications.ContainsError());
			AssertEquals("PreCondition:Warning notification", true, info.AllNotifications.ContainsWarning() || ((BusinessObject)topLevelBizObj).HasMessageErrors);

			if (!(topLevelBizObj is IBackDoorSavingSupportableBizObj) || !((IBackDoorSavingSupportableBizObj)topLevelBizObj).SupportBackDoorForSavingWhenAmendmentDetected)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);//cancel on which message to send

				using (IShowPreSaveDialog formOrPlugIn = GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(topLevelBizObj))
				{
					AssertEquals("Should not be able to save as users cancelled", ContinueWithSave.No, formOrPlugIn.ShowPreSaveDialogs());

					var message = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals("Last notification should contain FullDescriptionForMessagesToSend", true, message.Contains(info.FullDescriptionsForMessagesToSend));
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			if (!(topLevelBizObj is IBackDoorSavingSupportableBizObj) || !((IBackDoorSavingSupportableBizObj)topLevelBizObj).SupportBackDoorForSavingWhenAmendmentDetected)
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);//Yes to 'Which message to send'
			}

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);//cancel when shown 'Warning notifications'

			using (var formOrPlugIn = GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(topLevelBizObj))
			{
				AssertEquals("Should not be able to save as users cancelled", ContinueWithSave.No, formOrPlugIn.ShowPreSaveDialogs());
			}
		}

		public virtual void TestContinueWithSaveWhenUsersChooseToSendMessages()
		{
			IMessageManageableBizObj topLevelBizObj = GetBizObjWithMessagesToSendButMessageErrors();

			IMessageManager manager = topLevelBizObj.GetMessageManagerForAmendmentDetection();
			RequiredMessagesInformation info = manager.GetRequiredMessagesInformation();
			AssertEquals("PreCondition:Message to send", true, info.HasMessagesToSend);
			AssertEquals("PreCondition:No Error notification", false, info.AllNotifications.ContainsError());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			if (!(topLevelBizObj is IBackDoorSavingSupportableBizObj) || !((IBackDoorSavingSupportableBizObj)topLevelBizObj).SupportBackDoorForSavingWhenAmendmentDetected)
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);//Yes to 'Which message to send'
			}

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);//Yes to warning notifications

			using (var formOrPlugIn = GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(topLevelBizObj))
			{
				AssertEquals("Should be able to save when all is OK", ContinueWithSave.Yes, formOrPlugIn.ShowPreSaveDialogs());
				if (manager.DeferredAmendmentTillAfterSaveSuccessful)
				{
					((BusinessObject)topLevelBizObj).Factory.Save();
				}

				var message = UnitTestUserNotification.Instance.LastMessage.Text;

				AssertEquals("Last notification to users should contain the messages generated ", true, message.Contains("The following messages have been generated"));
			}
		}

		protected abstract IMessageManageableBizObj GetBizObjWithoutAnyMessagesToSend();
		protected abstract IMessageManageableBizObj GetBizObjWithMessagesToSendButWithErrorsFromMessageManager();
		protected abstract IMessageManageableBizObj GetBizObjWithMessagesToSendButMessageErrors();

		protected abstract IMessageManageableBizObj GetBizObjWithMessagesToSendButWithWarningsFromMessageManager();
		protected abstract IShowPreSaveDialog GetFormOrPlugInToWhichAmendmentDetectionIsHookedUp(IMessageManageableBizObj bizObj);

		protected virtual Type GetTypeOfDeclarationToMock()
		{
			return typeof(BaseJobDeclaration);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}
	}
}
