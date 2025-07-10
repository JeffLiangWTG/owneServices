using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DataObjects;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	public class CargoControlAndTransitHouseManifestMessageSender : DocDataObjectMessageSender
	{
		protected override ZString MenuFilterDoesNotMatchMessageError => (NoResString)"The consol is not arriving in Brazil.";  // ErrorMessage

		protected override ZString MenuItemMessageError => (NoResString)"Can not load CCT House Manifest correctly.";  // ErrorMessage

		protected override bool IsValidBeforeSending(BusinessObject bizObj, IStmMenuItem menuItem, INotifications notifications)
		{
			if (!base.IsValidBeforeSending(bizObj, menuItem, notifications))
			{
				return false;
			}

			if (bizObj is ForwardingConsol consol)
			{
				return true;
			}

			notifications.AddMessageError((NoResString)"Can not load Consol correctly."); // ErrorMessage
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

			var forwardingConsol = bizObj as ForwardingConsol;

			var shipmentIDsWithInvalidIssueDate = CargoControlAndTransitHelper.GetShipmentsWithInvalidIssueDate(forwardingConsol);
			if (shipmentIDsWithInvalidIssueDate.Any())
			{
				var message = $@"HAWB has been detected with a 'future' Issue Date for Shipment(s) {string.Join(", ", shipmentIDsWithInvalidIssueDate)}, which will lead to an erroneous association at CCT. As per CCT Requirements, the CCT House Manifest must be sent when each linked HAWB has an 'actual' Issue Date.
If the above Shipment(s) has been declared with a 'future' Issue Date by mistake, please withdraw the previous CCT Shipment Report and resend after correction.";
				notifications.AddMessageError(message); // ErrorMessage
				return false;
			}

			if (forwardingConsol.IsDirect)
			{
				notifications.AddMessageError((NoResString)"Advanced Manifest is not available from Consolidations with type DRT."); // ErrorMessage
				return false;
			}

			return true;
		}
	}
}
