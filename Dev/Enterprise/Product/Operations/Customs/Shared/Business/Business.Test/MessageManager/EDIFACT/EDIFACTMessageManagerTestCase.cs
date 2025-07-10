using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;

namespace Enterprise.Customs.Business.MessageManagers.Testing
{
	[TestsSubclassesOf(typeof(EDIFACTMessageManager))]
	public abstract class EDIFACTMessageManagerTestCase : TestCaseWithFactory
	{
		public virtual void TestShouldSendMessagesInTestMode()
		{
			SetTestMode(true);
			Assert(messageManager.ShouldSendMessagesInTestModeForTesting);
			SetTestMode(false);
			Assert(!messageManager.ShouldSendMessagesInTestModeForTesting);
		}

		public virtual void TestIsWaitingForResponse()
		{
			dataWrapper.MessageStatus = MessageStatusList.Codes.AwaitingOriginal;
			Assert("IsWaitingForResponse", messageManager.IsWaitingForResponse);
			dataWrapper.MessageStatus = MessageStatusList.Codes.ErrorOriginal;
			Assert("Not IsWaitingForResponse", !messageManager.IsWaitingForResponse);
			dataWrapper.MessageStatus = ZString.Empty;
			Assert("Not IsWaitingForResponse", !messageManager.IsWaitingForResponse);
		}

		public virtual void TestCanSendWithdrawal()
		{
			dataWrapper.JobStatus = EntryStatusList.Codes.Error;
			Assert("CanSendWithdrawal", messageManager.CanSendWithdrawal);
			dataWrapper.JobStatus = EntryStatusList.Codes.Clear;
			Assert("CanSendWithdrawal", messageManager.CanSendWithdrawal);
			dataWrapper.JobStatus = EntryStatusList.Codes.Cancelled;
			Assert("Can Not SendWithdrawal", !messageManager.CanSendWithdrawal);
			dataWrapper.JobStatus = ZString.Empty;
			Assert("Can Not SendWithdrawal", !messageManager.CanSendWithdrawal);
		}

		public virtual void TestCanSendOriginal()
		{
			Assert("CanSendOriginal", messageManager.CanSendOriginal);
		}

		public virtual void TestBusinessObject()
		{
			AssertEquals("BusinessObject", dataWrapper.TopLevelBusinessObject, messageManager.BusinessObject);
		}

		public abstract void TestCanSendThisMessage();
		public abstract void TestGetMessageBuilder();
		public abstract void TestMessageFriendlyName();
		public abstract void TestPopulateMessages();

		public abstract void SetTestMode(bool testMode);

		protected abstract IEDIFACTMessageAttachee GetDataWrapper();
		protected abstract EDIFACTMessageManager GetMessageManager();

		protected override void SetUp()
		{
			base.SetUp();
			dataWrapper = GetDataWrapper();
			messageManager = GetMessageManager();
		}

		protected EDIFACTMessageManager messageManager;
		protected IEDIFACTMessageAttachee dataWrapper;
		protected TestUserNotification notification = new TestUserNotification();
	}
}
