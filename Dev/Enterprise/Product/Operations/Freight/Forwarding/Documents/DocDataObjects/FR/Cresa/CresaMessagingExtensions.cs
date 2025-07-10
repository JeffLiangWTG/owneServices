using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	class CresaMessagingExtensions : BaseMessagingExtensions
	{
		public CresaMessagingExtensions(IDocument document, IStmALogParent logParent, IMessageInstructions messageInstructions)
		{
			Argument.NotNull(document, nameof(document));
			var cresa = document?.Data.Value as Cresa;
			Argument.NotNull(cresa, nameof(cresa));
			ecvReference = cresa.ECVReference;

			Argument.NotNull(messageInstructions, nameof(messageInstructions));
			messageContext = new FrenchMessageContext(
				Argument.NotNull(logParent,
				nameof(logParent)),
				messageInstructions.DocumentName,
				ShipmentDocumentDataStoreNames.GoodsReceivedCRESA);

			this.logParent = Argument.NotNull(logParent, nameof(logParent));
		}

		readonly FrenchMessageContext messageContext;
		readonly IStmALogParent logParent;
		readonly ZString ecvReference;

		public override bool? ContinueWithSendingMessage(IUserNotifications notifications)
		{
			if (EventSCMWithMatchingCRFExists(logParent, ecvReference))
			{
				var caption = Res.GetString("001EF918-51B6-490A-842E-FCF170DC52CE", "Warning");
				var notAcceptedMessage = Res.GetString("F6771653-7AC5-4C3A-91FA-C9A48D132703", "The Goods Received (CRESA) message cannot be sent once Clearance Completed (SCM) received from Port Community System.");
				notifications.ShowMessage(notAcceptedMessage, caption);

				return false;
			}

			return null;
		}

		public override bool? ContinueWithSendingMessageAmendment(IUserNotifications notifications) => messageContext.ContinueWithSendingMessageAmendment(notifications);

		public override bool? ContinueWithSendingMessageWithdrawal(IUserNotifications notifications)
		{
			var result = messageContext.ContinueWithSendingMessageWithdrawal(notifications);
			if (result == null && EventSCMWithMatchingCRFExists(logParent, ecvReference))
			{
				var caption = Res.GetString("001EF918-51B6-490A-842E-FCF170DC52CE", "Warning");
				var notAcceptedMessage = Res.GetString("EFC86C5F-1DCF-4D4B-9A51-BC606ABFBFCD", "The Goods Received (CRESA) message cannot be withdrawn/canceled once Clearance Completed (SCM) received from Port Community System");
				notifications.ShowMessage(notAcceptedMessage, caption);

				return false;
			}

			return result;
		}
		public override bool? ContinueWithResetToOriginal(IUserNotifications notifications)
		{
			var result = messageContext.ContinueWithResetToOriginal(notifications);
			if (result == null && EventSCMWithMatchingCRFExists(logParent, ecvReference))
			{
				var caption = Res.GetString("001EF918-51B6-490A-842E-FCF170DC52CE", "Warning");
				var notAcceptedMessage = Res.GetString("BC5B5055-C604-4910-B0AB-274F18AEDE1B", "The Goods Received (CRESA) message cannot be reset to original once Clearance Completed (SCM) received from Port Community System.");
				notifications.ShowMessage(notAcceptedMessage, caption);

				return false;
			}

			return result;
		}

		bool EventSCMWithMatchingCRFExists(IStmALogParent logParent, string ecvReference)
		{
			if (logParent is ForwardingShipment shipment)
			{
				var scmLog = shipment.Logs.GetAllLogs().OfType<StmALog>().Where(log => log.SL_SE_NKEvent == Events.ClearanceCompletedCode).OrderByDescending(log => log.SL_PostedTimeUtc).FirstOrDefault();
				if (ecvReference == scmLog?.Parameters?.GetValueSafe(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.CustomsReferenceNumber))
				{
					return true;
				}
			}

			return false;
		}
	}
}
