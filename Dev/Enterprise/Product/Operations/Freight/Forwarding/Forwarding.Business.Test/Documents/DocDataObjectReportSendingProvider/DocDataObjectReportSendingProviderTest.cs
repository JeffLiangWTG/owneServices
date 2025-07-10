using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	abstract class DocDataObjectReportSendingProviderTest : TestCaseWithFactory
	{
		public void TestMessageSender()
		{
			var provider = GetSendingProvider();
			AssertEquals(ExpectedMessageSenderFullName, provider.MessageSender.GetType().FullName);
		}

		public void TestSendMessage_MergeFormAndDocumentsMenus()
		{
			AssertSendingMessage();
		}

		protected virtual void AssertSendingMessage()
		{
			using (Factory.AddDisposableService())
			{
				var provider = GetSendingProvider();
				var bizObj = GetValidBusinessObjectForSending();
				var notifications = new NotificationBuffer();

				Assert("Can not process with null business object", !provider.SendMessage(null, notifications));
				AssertEquals("Message can't be sent without providing the object for sending", notifications.AsString.Replace("\r\n", string.Empty));

				notifications = new NotificationBuffer();
				Assert("Send message successfully", provider.SendMessage(bizObj, notifications));
			}
		}

		public void TestModuleIdentifier()
		{
			var provider = GetSendingProvider();
			AssertEquals(ExpectedModuleIdentifier, provider.ModuleIdentifier.Name);
		}

		protected abstract DocDataObjectReportSendingProvider GetSendingProvider();

		protected abstract BusinessObject GetValidBusinessObjectForSending();

		protected abstract ZString ExpectedMessageSenderFullName { get; }

		protected abstract ZString ExpectedModuleIdentifier { get; }
	}
}
