using System.Collections.Specialized;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public class SendsMessagesToCustomsReturningResultsAsProperties : ISendsMessagesToCustoms
	{
		public SendsMessagesToCustomsReturningResultsAsProperties(bool continueIfAtAllPossible) : this(continueIfAtAllPossible, YesNoCancel.Cancel)
		{
		}
		public SendsMessagesToCustomsReturningResultsAsProperties(YesNoCancel yesNoCancelResponse) : this(false, yesNoCancelResponse)
		{
		}
		public SendsMessagesToCustomsReturningResultsAsProperties(bool continueIfAtAllPossible, YesNoCancel yesNoCancelResponse)
		{
			fMergeResult = new StringBuilder();
			this.continueIfAtAllPossible = continueIfAtAllPossible;
			this.yesNoCancelResponse = yesNoCancelResponse;
		}

		#region Properties Exposed that contain Results of Merge.

		public ZString MergeResult
		{
			get { return fMergeResult.ToString().Trim(); }
		}
		readonly StringBuilder fMergeResult;

		#endregion

		protected void AddLineToResult(ZString lineToAdd)
		{
			if (!lineToAdd.IsEmpty)
			{
				fMergeResult.Append(lineToAdd + "\r\n");
			}
		}

		readonly bool continueIfAtAllPossible;
		readonly YesNoCancel yesNoCancelResponse;

		#region ISendsMessagesToCustoms Members

		public void NotifyUserOfASuccessfulSend(string text)
		{
			AddLineToResult(Res.GetString("12854b6e-5bb1-4c71-84af-05cddd51dbca", "Entry Merged."));
		}

		public bool ContinueWithAction(string message, string caption)
		{
			AddLineToResult(Res.GetString("91b097f2-83f6-4e2e-beb6-87432d9ba523", "Caution: {0}", message));
			return continueIfAtAllPossible;
		}

		public bool AskUserToContinueWithAction(string message, string caption, BusinessObject topLevelBusinessObject)
		{
			return ContinueWithAction(message, caption);
		}

		public bool YesNoQuery(string message, string caption, MessageStyle messageStyle = MessageStyle.Question)
		{
			return continueIfAtAllPossible;
		}

		public YesNoCancel YesNoCancelQuery(string message, string caption, MessageStyle messageStyle = MessageStyle.Question)
		{
			return yesNoCancelResponse;
		}

		bool ISendsMessagesToCustoms.ShowUserConfirmation(string message, string caption, string confirmationPrompt, string confirmationString)
		{
			return continueIfAtAllPossible;
		}

		public void NotifyUserOfAnInvalidOperation(string text)
		{
			AddLineToResult(Res.GetString("bbdd3cc2-a6b9-401b-8b9f-a4af7ca3e358", "Invalid Operation: {0}", text));
		}

		public void WarnUserAboutSomething(string message, string caption)
		{
			AddLineToResult(Res.GetString("14681cc9-0db1-4c4f-afed-3fc350858f39", "Warning: {0}", message));
		}

		public bool ContinueWithSend(StringCollection warnings)
		{
			AddLineToResult(Res.GetString("2e1bfc53-e67c-402e-85bc-5a98f1aa41ba", "The following Message Errors were given when merging this entry:"));
			foreach (string warning in warnings)
			{
				AddLineToResult("\t" + warning);
			}
			return continueIfAtAllPossible;
		}

		public void MessageSendErrorAlert(StringCollection errors)
		{
			AddLineToResult(Res.GetString("88b41ff2-c316-408f-b60b-df7e9c7240c0", "MERGE FAILED: Critical errors were found when attempting to merge this entry:"));
			foreach (string error in errors)
			{
				AddLineToResult("\t" + error);
			}
		}

		public SingleMessageManager[] WhichMessagesShouldWeSend(SingleMessageManager[] allManagers)
		{
			return System.Array.Empty<SingleMessageManager>();
		}

		public SingleMessageManager[] WhichMessagesShouldWeWithdraw(SingleMessageManager[] allManagers)
		{
			return System.Array.Empty<SingleMessageManager>();
		}

		public SingleMessageManager[] WhichMessagesShouldWeReset(SingleMessageManager[] allManagers)
		{
			return System.Array.Empty<SingleMessageManager>();
		}

		public DeferredAmendmentSavingOptions ShowBackdoorForSavingOnAmendmentFormGetConfirmationFromUsers()
		{
			return new DeferredAmendmentSavingOptions();
		}

		public ContinueWithSave GetAmendmentWithdrawalReason(AmendmentWithdrawalReason reason)
		{
			return ContinueWithSave.No;
		}
		#endregion
	}
}
