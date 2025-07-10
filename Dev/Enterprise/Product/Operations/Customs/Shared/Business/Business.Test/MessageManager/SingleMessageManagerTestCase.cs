using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class SingleMessageManagerTestCase : TestCaseWithFactory
	{
		public virtual void TestGetCommonNotificationsForSending()
		{
			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;

			SingleMessageManager manager = GetNewSingleMessageManager();
			AddError(manager.BusinessObject, "error 1 only");
			AddMessageError(manager.BusinessObject, "message error 1");
			MessageSendingNotificationCollection notifications = manager.GetCommonNotificationsForSending();

			if (manager.IncludeBusinessLayerNotificationsInMessageSendingNotifications)
			{
				AssertContains("error 1 only", notifications.ErrorNotificationsAsString());
				AssertEquals(false, notifications.ErrorNotificationsAsString().Contains(SingleMessageManager.MessageErrorsExistWithNoSecurityRight));
				AssertContains("message error 1", notifications.WarningNotificationsAsString());

				Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = false;
				notifications = manager.GetCommonNotificationsForSending();
				AssertContains("error 1 only", notifications.ErrorNotificationsAsString());
				AssertEquals(true, notifications.ErrorNotificationsAsString().Contains(SingleMessageManager.MessageErrorsExistWithNoSecurityRight));
				AssertEquals("message error 1", false, notifications.WarningNotificationsAsString().Contains("message error 1"));
			}
			else
			{
				Assert(!notifications.ErrorNotificationsAsString().Contains("error 1 only"));
				AssertEquals(false, notifications.ErrorNotificationsAsString().Contains(SingleMessageManager.MessageErrorsExistWithNoSecurityRight));
				Assert(!notifications.WarningNotificationsAsString().Contains("message error 1"));
			}
		}

		protected virtual void AddMessageError(BusinessObject bizObj, string message)
		{
			bizObj.AddRowMessageError(message);
		}

		protected virtual void AddError(BusinessObject bizObj, string error)
		{
			bizObj.AddRowError(error);
		}

		protected abstract SingleMessageManager GetNewSingleMessageManager();
	}
}
