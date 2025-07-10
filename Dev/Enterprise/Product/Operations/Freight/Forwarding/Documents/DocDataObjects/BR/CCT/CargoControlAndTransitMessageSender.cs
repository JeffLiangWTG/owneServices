using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	public class CargoControlAndTransitMessageSender : DocDataObjectMessageSender
	{
		protected override bool IsValidBeforeSending(BusinessObject bizObj, IStmMenuItem menuItem, INotifications notifications)
		{
			if (!base.IsValidBeforeSending(bizObj, menuItem, notifications))
			{
				return false;
			}

			if (bizObj is ForwardingShipment shipment)
			{
				return true;
			}

			notifications.AddMessageError((NoResString)"Can not load Shipment correctly."); // ErrorMessage
			return false;
		}

		protected override bool IsValidForSendingMessage(BusinessObject bizObj, IStmMenuItem menuItem, INotifications notifications)
		{
			if (!base.IsValidForSendingMessage(bizObj, menuItem, notifications))
			{
				return false;
			}

			if (!CargoControlAndTransitHelper.IsStaffCertificateValid())
			{
				notifications.AddMessageError((NoResString)"To send messages to CCT you must have a valid certificate loaded against your staff profile."); // ErrorMessage
				return false;
			}

			var shipment = bizObj as ForwardingShipment;
			if (shipment.JS_ShipmentType == Constants.ShipmentTypes.BlindCoLoadMaster)
			{
				notifications.AddMessageError((NoResString)"Advanced Air Cargo Reporting is only available from Shipments with types STD, BCN, CLD, ASM or HVL."); // ErrorMessage
				return false;
			}

			return true;
		}

		protected override ZString MenuFilterDoesNotMatchMessageError => (NoResString)"The shipment is not arriving in Brazil.";  // ErrorMessage

		protected override ZString MenuItemMessageError => (NoResString)"Can not load CCT Shipment Report correctly.";  // ErrorMessage

		protected override bool AllowSendMessageAmendment => true;
	}
}
