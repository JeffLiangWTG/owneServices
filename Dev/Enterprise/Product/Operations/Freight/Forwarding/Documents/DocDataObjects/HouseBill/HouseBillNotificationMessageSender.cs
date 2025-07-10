using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public class HouseBillNotificationMessageSender : DocDataObjectMessageSender
	{
		protected override ZString MenuFilterDoesNotMatchMessageError => (NoResString)"The shipment does not match Bill Of Lading menu filter.";  // ErrorMessage

		protected override ZString MenuItemMessageError => (NoResString)"Can not load Bill Of Lading correctly.";  // ErrorMessage

		protected override bool AllowSendMessageAmendment => true;

		protected override bool IsValidBeforeSending(BusinessObject bizObj, IStmMenuItem menuItem, INotifications notifications)
		{
			if (!base.IsValidBeforeSending(bizObj, menuItem, notifications))
			{
				return false;
			}

			if (bizObj is ForwardingShipment)
			{
				return true;
			}

			notifications.AddMessageError((NoResString)"Can not load shipment correctly.");  // ErrorMessage

			return false;
		}
	}
}
