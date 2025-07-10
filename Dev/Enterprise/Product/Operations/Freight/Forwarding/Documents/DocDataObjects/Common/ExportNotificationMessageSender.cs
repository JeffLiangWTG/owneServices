using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.NL;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Common
{
	public class ExportNotificationMessageSender : DocDataObjectMessageSender
	{
		protected override bool IsValidBeforeSending(BusinessObject bizObj, IStmMenuItem menuItem, INotifications notifications)
		{
			if (!base.IsValidBeforeSending(bizObj, menuItem, notifications))
			{
				return false;
			}

			if (bizObj is ForwardingShipment shipment)
			{
				if (menuItem.PK.Equals(ShipmentSystemFormMenuItems.CINExportNotificationPK))
				{
					return true;
				}

				notifications.AddMessageError((NoResString)"Can not load Shipment correctly."); // ErrorMessage
				return false;
			}

			if (bizObj is ForwardingConsol consol)
			{
				if (menuItem.PK.Equals(ConsolSystemFormMenuItems.DocumentMenuExportNotificationCargonautNLPK))
				{
					return true;
				}

				notifications.AddMessageError((NoResString)"Can not load Consol correctly."); // ErrorMessage
				return false;
			}

			notifications.AddMessageError((NoResString)"Can not load Business Object correctly."); // ErrorMessage
			return false;
		}

		protected override bool IsValidForSendingMessage(BusinessObject bizObj, IStmMenuItem menuItem, INotifications notifications)
		{
			if (!base.IsValidBeforeSending(bizObj, menuItem, notifications))
			{
				return false;
			}

			var message = string.Empty;
			if (menuItem.PK.Equals(ShipmentSystemFormMenuItems.CINExportNotificationPK))
			{
				var shipment = bizObj as ForwardingShipment;
				message = CINExportNotificationMessageValidator.GetValidationMessage(shipment);
			}
			else if (menuItem.PK.Equals(ConsolSystemFormMenuItems.DocumentMenuExportNotificationCargonautNLPK))
			{
				var consol = bizObj as ForwardingConsol;
				message = CGNExportNotificationMessageValidator.GetValidationMessage(consol);
			}

			if (!string.IsNullOrEmpty(message))
			{
				notifications.AddMessageError(message);
				return false;
			}

			return true;
		}

		protected override ZString MenuFilterDoesNotMatchMessageError => (NoResString)"This is not applicable for Export Notification Message.";  // ErrorMessage

		protected override ZString MenuItemMessageError => (NoResString)"Can not load Export Notification Message correctly."; // ErrorMessage

		protected override bool AllowSendMessageAmendment => true;
	}
}
