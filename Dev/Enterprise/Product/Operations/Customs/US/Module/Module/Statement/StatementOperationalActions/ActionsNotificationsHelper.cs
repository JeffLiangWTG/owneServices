using CargoWise.Types;
using Enterprise.Customs.US.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Module
{
	public class ActionsNotificationsHelper
	{
		public ActionsNotificationsHelper(LogControllerLink jobLink)
		{
			JobNumberLink = jobLink;
		}

		public LogControllerLink JobNumberLink { get; }

		public bool CanSendMessage { get; private set; } = true;

		public ZString NotificationsAsString { get; private set; }

		public void UpdateCanSendMessage(bool sendMessage, ZString reason)
		{
			CanSendMessage &= sendMessage;
			NotificationsAsString += reason;
		}

		public void CheckIfValidToSendAuthorizationOrPayment(CusStatementHeader statementHeader)
		{
			var errorMessageText = statementHeader.GetCheckIfValidToSendAuthorizationOrPaymentMessage();
			if (errorMessageText != null && !errorMessageText.Item1.IsEmpty)
			{
				UpdateCanSendMessage(false, errorMessageText.Item1);
			}
		}
	}
}
