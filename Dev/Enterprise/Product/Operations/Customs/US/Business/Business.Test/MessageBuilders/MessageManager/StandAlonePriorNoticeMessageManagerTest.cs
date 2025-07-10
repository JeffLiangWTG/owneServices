using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class StandAlonePriorNoticeMessageManagerTest : TestCaseWithFactory
	{
		public void TestBusinessObject()
		{
			var manager = new StandAlonePriorNoticeMessageManager(Action);
			AssertEquals(Declaration, manager.BusinessObject);
		}

		public void TestHasActiveMessages()
		{
			var manager = new StandAlonePriorNoticeMessageManager(Action);
			AssertEquals(false, manager.HasActiveMessages);
		}

		public void TestCanSendOriginal()
		{
			var manager = new StandAlonePriorNoticeMessageManager(Action);
			AssertEquals(true, manager.CanSendOriginal);
		}

		public void TestCanSendWithdrawal()
		{
			var manager = new StandAlonePriorNoticeMessageManager(Action);
			AssertEquals(false, manager.CanSendWithdrawal);
		}

		public void TestIsWaitingForResponse()
		{
			var manager = new StandAlonePriorNoticeMessageManager(Action);
			AssertEquals(false, manager.IsWaitingForResponse);
		}

		public void TestMessageFriendlyName()
		{
			var manager = new StandAlonePriorNoticeMessageManager(Action);
			AssertEquals("Standalone Prior Notice", manager.MessageFriendlyName);
		}

		ImportMessageSendingActionCollection actionCollection;
		ImportMessageSendingActionCollection ActionCollection
			=> actionCollection ?? (actionCollection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.StandAlonePriorNotice));

		StandAlonePriorNoticeMessageSendingAction action;
		StandAlonePriorNoticeMessageSendingAction Action => action ?? (action = new StandAlonePriorNoticeMessageSendingAction(Declaration, ActionCollection));

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				}
				return declaration;
			}
		}
	}
}
