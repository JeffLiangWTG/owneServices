using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Res = Enterprise.Freight.Forwarding.Documents.DataObjects.Res;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.BR
{
	class CargoControlAndTransitHelper
	{
		#region Check Shipments Issue Date

		public static List<string> GetShipmentsWithInvalidIssueDate(ForwardingConsol consol)
		{
			var invalidShipments = new List<string>();

			foreach (var shipment in consol.Shipments.OfType<ForwardingShipment>())
			{
				if (CargoControlAndTransitHouseManifestHelper.ShouldExcludeFromShipments(shipment))
				{
					continue;
				}

				var awbHeader = shipment.AWBHeader;
				if (awbHeader != null && IsInvalidShipmentIssueDate(awbHeader.EH_AWBIssueDate, shipment.JS_RL_NKDestination))
				{
					invalidShipments.Add(shipment.JS_UniqueConsignRef);
				}
			}

			return invalidShipments;
		}

		public static bool IsInvalidShipmentIssueDate(ZDateTime awbIssueDate, string shipmentDestination)
		{
			return !awbIssueDate.IsEmpty && awbIssueDate > Env.Time.GetUnlocoTimeFromUtc(shipmentDestination, ZDateTime.UtcNow.ToDateTime());
		}

		public static bool CheckShipmentIssueDateValidAndShowMessage(ForwardingConsol consol, IUserNotifications notifications)
		{
			var shipmentIDsWithInvalidIssueDate = GetShipmentsWithInvalidIssueDate(consol);
			var isValid = !shipmentIDsWithInvalidIssueDate.Any();
			if (!isValid)
			{
				var message = Res.GetString("6E86F796-F2ED-4F01-88A7-304EA6634BB6",
					"HAWB has been detected with a 'future' Issue Date for Shipment(s) {0}, which will lead to an erroneous association at CCT. As per CCT Requirements, the CCT House Manifest must be sent when each linked HAWB has an 'actual' Issue Date.\r\nIf the above Shipment(s) has been declared with a 'future' Issue Date by mistake, please withdraw the previous CCT Shipment Report and resend after correction.",
					string.Join(", ", shipmentIDsWithInvalidIssueDate));
				notifications?.ShowMessage(message, Res.GetString("0A9BF5A5-1EA8-46A1-8A40-CEB4DA384062", "Information"));
			}

			return isValid;
		}

		#endregion

		#region Check Staff Certificate Valid

		public static bool IsStaffCertificateValid()
		{
			var brWrapper = GlbStaff.CurrentUser?.GetBRWrapper();

			return brWrapper?.CCTPassword != null && brWrapper.CCTPassword.GP_PasswordStatus == PasswordStatusList.Codes.Valid;
		}

		public static bool CheckStaffCertificateValidAndShowMessage(IUserNotifications notifications)
		{
			var isValid = IsStaffCertificateValid();
			if (!isValid)
			{
				var message = Res.GetString("1b105cb2-f0b5-4f83-b8db-a77776202ddd", "To send messages to CCT you must have a valid certificate loaded against your staff profile.");
				notifications?.ShowMessage(message, Res.GetString("2d7a614b-d880-4fa4-9933-fa4ecf7a398b", "Information"));
			}

			return isValid;
		}

		#endregion
	}
}
