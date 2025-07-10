using System;
using System.Collections.Specialized;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public class SendsMessagesToCustomsShutterUpperer : ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem
	{
		public SendsMessagesToCustomsShutterUpperer(bool throwExceptionOnInvalidOperation)
		{
			this.ThrowExceptionOnInvalidOperation = throwExceptionOnInvalidOperation;
		}

		public SendsMessagesToCustomsShutterUpperer()
		{
		}

		#region ISendsMessagesToCustoms Members

		public virtual void NotifyUserOfAnInvalidOperation(string text)
		{
			if (ThrowExceptionOnInvalidOperation)
			{
				throw new ApplicationException("Error : " + text + System.Environment.NewLine);
			}
			else
			{
				InvalidOperationText = text;
			}
		}

		public void NotifyUserOfASuccessfulSend(string text)
		{
			SuccessfulSendText = text;
			SuccessfulSendOccured = true;
		}

		public void WarnUserAboutSomething(string message, string caption)
		{
			Warning = message;
			WarningCaption = caption;
		}

		public bool ContinueWithAction(string message, string caption)
		{
			ContinueWithActionMessage = message;
			ContinueWithActionCaption = caption;
			PastCaptions.Add(caption);
			PastMessages.Add(message);
			return AnswerToContinueWithAction;
		}

		public bool AskUserToContinueWithAction(string message, string caption, BusinessObject topLevelBusinessObject)
		{
			return ContinueWithAction(message, caption);
		}

		public bool YesNoQuery(string message, string caption, MessageStyle messageStyle = MessageStyle.Question)
		{
			PastYesNoQuestionsAsked.Add(message);
			return AnswerToContinueWithAction;
		}

		public YesNoCancel YesNoCancelQuery(string message, string caption, MessageStyle messageStyle)
		{
			PastYesNoCancelQuestionsAsked.Add(message);
			return AnswerToContinueWithYNCAction;
		}

		bool ISendsMessagesToCustoms.ShowUserConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
		{
			return AnswerToContinueWithAction;
		}

		public bool ContinueWithSend(StringCollection warnings)
		{
			LastWarnings = warnings;
			return AnswerToContinueWithAction;
		}

		public void MessageSendErrorAlert(StringCollection errors)
		{
			LastErrors = errors;
		}

		public SingleMessageManager[] WhichMessagesShouldWeSend(SingleMessageManager[] allManagers)
		{
			if (ReturnAllForWhichMessagesShouldWeSend)
			{
				return allManagers;
			}
			else
			{
				return Array.Empty<SingleMessageManager>();
			}
		}

		public SingleMessageManager[] WhichMessagesShouldWeWithdraw(SingleMessageManager[] allManagers)
		{
			if (ReturnAllForWhichMessagesShouldWeWithdraw)
			{
				return allManagers;
			}
			else
			{
				return Array.Empty<SingleMessageManager>();
			}
		}

		public SingleMessageManager[] WhichMessagesShouldWeReset(SingleMessageManager[] allManagers)
		{
			if (ReturnAllForWhichMessagesShouldWeReset)
			{
				return allManagers;
			}
			else
			{
				return Array.Empty<SingleMessageManager>();
			}
		}

		#endregion

		public StringCollection PastCaptions = new StringCollection();
		public StringCollection PastMessages = new StringCollection();

		public bool AnswerToContinueWithAction = true;
		public YesNoCancel AnswerToContinueWithYNCAction = YesNoCancel.Yes;
		public string ContinueWithActionCaption;
		public string ContinueWithActionMessage;
		public bool ThrowExceptionOnInvalidOperation = true;
		public string Warning;
		public string WarningCaption;
		public string InvalidOperationText;
		public bool SuccessfulSendOccured;

		public bool ReturnAllForWhichMessagesShouldWeWithdraw;
		public bool ReturnAllForWhichMessagesShouldWeSend;
		public bool ReturnAllForWhichMessagesShouldWeReset;
		public StringCollection LastWarnings;
		public StringCollection LastErrors;
		public StringCollection PastYesNoQuestionsAsked = new StringCollection();
		public StringCollection PastYesNoCancelQuestionsAsked = new StringCollection();
		public string SuccessfulSendText;

		public string LastErrorsAsString
		{
			get
			{
				string result = string.Empty;
				if (LastErrors != null)
				{
					foreach (string s in LastErrors)
					{
						result += s + System.Environment.NewLine;
					}
				}
				return result;
			}
		}

		public void NotifyUserOfASuccessfulOperation(string text, string caption)
		{
			SuccessfulSendText = text;
			SuccessfulSendOccured = true;
		}

		ContinueWithSave ISendMessagesToCustomsExtraMembersAndDetermineRequiredMessagesAndSendThem.DetermineRequiredMessagesAndSendThem(IMessageManager messageManager)
		{
			return ContinueWithSave.Yes; // do nothing
		}
	}

	public interface ISendsMessagesToCustoms
	{
		void NotifyUserOfAnInvalidOperation(string text);
		void NotifyUserOfASuccessfulSend(string text);
		void WarnUserAboutSomething(string message, string caption);
		bool ContinueWithAction(string message, string caption);
		bool YesNoQuery(string message, string caption, MessageStyle messageStyle = MessageStyle.Question);
		YesNoCancel YesNoCancelQuery(string message, string caption, MessageStyle messageStyle = MessageStyle.Question);
		bool ContinueWithSend(StringCollection warnings);
		void MessageSendErrorAlert(StringCollection errors);
		bool ShowUserConfirmation(string message, string caption, string confirmationPrompt, string confirmationString);
		SingleMessageManager[] WhichMessagesShouldWeSend(SingleMessageManager[] allManagers);
		SingleMessageManager[] WhichMessagesShouldWeWithdraw(SingleMessageManager[] allManagers);
		SingleMessageManager[] WhichMessagesShouldWeReset(SingleMessageManager[] allManagers);
		bool AskUserToContinueWithAction(string message, string caption, BusinessObject topLevelBusinessObject);
	}

	public interface ISendsMessagesToCustomsExtraMembers : ISendsMessagesToCustoms
	{
		void NotifyUserOfASuccessfulOperation(string text, string caption);
	}

	public enum YesNoCancel
	{
		Yes,
		No,
		Cancel
	}

	public enum MessageStyle
	{
		Default,
		Information,
		Warning,
		Error,
		Question
	}
}
