using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public class DrawbackSummaryMessageSender : MessageSender
	{
		public DrawbackSummaryMessageSender(JobDeclaration declaration, UpdateActionCode actionCode)
			: base(declaration)
		{
			this.actionCode = actionCode;
		}
		readonly UpdateActionCode actionCode;

		protected override bool Prepare()
		{
			string messageText = "";
			if (MessageManager.CanSendThisMessage(actionCode, out messageText))
			{
				bool okToSend = true;

				if (MessageManager.IsWaitingForResponse)
				{
					okToSend = Job.MessageInitiator.YesNoQuery(AwaitingCustomsResponseWarningMessage, "Warning");
				}

				return okToSend;
			}
			else
			{
				Job.MessageInitiator.NotifyUserOfAnInvalidOperation("System cannot send a Drawback Summary " + actionCode + " message as " + messageText);
				return false;
			}
		}
		const string AwaitingCustomsResponseWarningMessage = "This Drawback is waiting for Customs response.\r\nAre you sure you you want to resend to Customs?";

		protected override bool GenerateMessage()
		{
			MessageManager.PopulateMessage(actionCode);
			return true;
		}

		protected override string SuccessfulSendNotification
		{
			get { return "Drawback Summary " + actionCode + " Message sent"; }
		}

		protected override bool ShouldValidateJob
		{
			get { return true; }
		}
		DrawbackSummaryMessageManager MessageManager
		{
			get
			{
				if (drawbackSummaryMessageManager == null)
				{
					drawbackSummaryMessageManager = new DrawbackSummaryMessageManager(new JobDeclarationDrawbackSupporter(Job, actionCode));
				}
				return drawbackSummaryMessageManager;
			}
		}
		DrawbackSummaryMessageManager drawbackSummaryMessageManager;

		protected override MessageSendingValidation GetMessageSendingValidation(BusinessObject businessObjectForNotifications, IEnumerable<INotification> msgErrors)
		{
			return new DrawbackMessageSendingValidation(businessObjectForNotifications, msgErrors);
		}
	}
}
