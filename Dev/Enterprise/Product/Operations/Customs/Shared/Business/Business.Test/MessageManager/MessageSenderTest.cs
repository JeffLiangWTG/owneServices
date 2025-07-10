using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class MessageSenderTest : TestCaseWithFactory
	{
		#region implementation

		protected class SendsMessagesToCustoms : ISendsMessagesToCustoms
		{
			public bool ShouldContinueWithAction = true;
			public bool ShouldContinueWithSend = true;
			public bool ReturnTrueOnShowUserConfirmation = true;
			public bool ReturnTrueOnYesNoQuery = true;
			public YesNoCancel ReturnYesNoCancel = YesNoCancel.Yes;
			public List<string> LastMessages = new List<string>();

			#region ISendsMessagesToCustoms Members

			public bool ContinueWithAction(string message, string caption)
			{
				LastMessages.Add(message);
				return ShouldContinueWithAction;
			}

			public bool AskUserToContinueWithAction(string message, string caption, BusinessObject topLevelBusinessObject)
			{
				return ContinueWithAction(message, caption);
			}

			public bool ContinueWithSend(System.Collections.Specialized.StringCollection warnings)
			{
				return ShouldContinueWithSend;
			}

			public void MessageSendErrorAlert(System.Collections.Specialized.StringCollection errors)
			{
				return;
			}

			public void NotifyUserOfASuccessfulSend(string text)
			{
				LastMessages.Add(text);
				return;
			}

			public void NotifyUserOfAnInvalidOperation(string text)
			{
				LastMessages.Add(text);
				return;
			}

			public bool ShowUserConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
			{
				LastMessages.Add(message);
				return ReturnTrueOnShowUserConfirmation;
			}

			public void WarnUserAboutSomething(string message, string caption)
			{
				LastMessages.Add(message);
				return;
			}

			public SingleMessageManager[] WhichMessagesShouldWeReset(SingleMessageManager[] allManagers)
			{
				return allManagers;
			}

			public SingleMessageManager[] WhichMessagesShouldWeSend(SingleMessageManager[] allManagers)
			{
				return allManagers;
			}

			public SingleMessageManager[] WhichMessagesShouldWeWithdraw(SingleMessageManager[] allManagers)
			{
				return allManagers;
			}

			public bool YesNoQuery(string message, string caption, MessageStyle messageStyle = MessageStyle.Question)
			{
				LastMessages.Add(message);
				return ReturnTrueOnYesNoQuery;
			}

			public YesNoCancel YesNoCancelQuery(string message, string caption, MessageStyle messageStyle = MessageStyle.Question)
			{
				LastMessages.Add(message);
				return ReturnYesNoCancel;
			}

			#endregion
		}

		#endregion
	}
}
