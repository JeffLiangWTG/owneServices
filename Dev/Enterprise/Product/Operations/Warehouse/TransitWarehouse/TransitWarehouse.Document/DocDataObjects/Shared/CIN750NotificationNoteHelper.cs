using CargoWise.Types;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.Warehouse.Transit.Document.DocDataObjects;

namespace Enterprise.Warehouse.Transit.Document
{
	public class CIN750NotificationNoteHelper : TransitLogTableHelper<CIN750Notification, TransitLogColumnIDs.CIN750NotificationColumn>
	{
		protected override ZString GetHeader(TransitLogColumnIDs.CIN750NotificationColumn columnID)
		{
			switch (columnID)
			{
				case TransitLogColumnIDs.CIN750NotificationColumn.MessageType:
					return Padding + Res.GetString("e68750ef-c26c-4ab7-8a36-6488398f6a25", "Message Type");
				case TransitLogColumnIDs.CIN750NotificationColumn.RefType:
					return Res.GetString("d1bcb355-f991-4fb0-a936-6f1158445e3e", "Ref Type");
				case TransitLogColumnIDs.CIN750NotificationColumn.RefCode:
					return Res.GetString("f50f3887-e518-4eb1-8d35-ead183990605", "Ref Code");
				case TransitLogColumnIDs.CIN750NotificationColumn.EnterpriseCode:
					return Res.GetString("a74a750b-869b-4c12-94b0-43845ca9d7cb", "Enterprise Code");
				case TransitLogColumnIDs.CIN750NotificationColumn.CFSWarehouse:
					return Res.GetString("1199ec56-a053-48ec-b943-1a7a01799d5e", "CFS/TWH Warehouse");
				case TransitLogColumnIDs.CIN750NotificationColumn.CFSCINCode:
					return Res.GetString("8e3cea14-2e51-42e5-8939-bac7fff352bf", "CFS/TWH CIN Code");
				case TransitLogColumnIDs.CIN750NotificationColumn.FromCTO:
					return Res.GetString("d563f258-46a5-4ff7-947d-35fd2193edaf", "From CTO");
				case TransitLogColumnIDs.CIN750NotificationColumn.CTOCINCode:
					return Res.GetString("1b6c3c44-e61a-4ec1-818b-abd26baf22d6", "CTO CIN Code");
				case TransitLogColumnIDs.CIN750NotificationColumn.ToCTO:
					return Res.GetString("108479e3-dbad-48f7-9cdf-cc413d9707ec", "To CTO");
				case TransitLogColumnIDs.CIN750NotificationColumn.ToCINCode:
					return Res.GetString("e7b11c38-a601-4a87-8d6e-f77d7409d57d", "To CIN code");
				case TransitLogColumnIDs.CIN750NotificationColumn.Result:
					return Res.GetString("67800013-a317-4e9c-acd7-b70dc6880666", "Result");
				case TransitLogColumnIDs.CIN750NotificationColumn.CustomsStatus:
					return Res.GetString("48b0dc87-23fe-48ba-88ab-77e2910adc50", "Customs Status");
				default:
					return ZString.Empty;
			}
		}

		protected override ZString GetValue(CIN750Notification notification, TransitLogColumnIDs.CIN750NotificationColumn columnID)
		{
			var inNotification = notification as CIN750InNotification;
			var outNotification = notification as CIN750OutNotification;
			switch (columnID)
			{
				case TransitLogColumnIDs.CIN750NotificationColumn.MessageType:
					return notification?.NotificationType is null ? ZString.Empty : Padding + notification.NotificationType;
				case TransitLogColumnIDs.CIN750NotificationColumn.RefType:
					return notification?.RefType?.Code ?? ZString.Empty;
				case TransitLogColumnIDs.CIN750NotificationColumn.RefCode:
					return notification?.RefCode ?? ZString.Empty;
				case TransitLogColumnIDs.CIN750NotificationColumn.EnterpriseCode:
					return notification?.EnterpriseAndServerCode ?? ZString.Empty;
				case TransitLogColumnIDs.CIN750NotificationColumn.Result:
					return notification?.Result ?? ZString.Empty;
				case TransitLogColumnIDs.CIN750NotificationColumn.CFSWarehouse:
					return notification?.DeclaredInWarehouse?.CompanyName ?? ZString.Empty;
				case TransitLogColumnIDs.CIN750NotificationColumn.CFSCINCode:
					return notification?.DeclaredInWarehouseCIN ?? ZString.Empty;
				case TransitLogColumnIDs.CIN750NotificationColumn.CustomsStatus:
					if (notification is CIN750InNotification)
					{
						return inNotification.CustomsStatus?.Code ?? ZString.Empty;
					}
					else if (notification is CIN750OutNotification)
					{
						return outNotification.CustomsStatus?.Code ?? ZString.Empty;
					}
					break;
				case TransitLogColumnIDs.CIN750NotificationColumn.FromCTO:
					if (notification is CIN750InNotification)
					{
						return inNotification.FromCTO?.CompanyName ?? ZString.Empty;
					}
					break;
				case TransitLogColumnIDs.CIN750NotificationColumn.CTOCINCode:
					if (notification is CIN750InNotification)
					{
						return inNotification.FromCTOCIN;
					}
					break;
				case TransitLogColumnIDs.CIN750NotificationColumn.ToCTO:
					if (notification is CIN750OutNotification)
					{
						return outNotification.ToCTO?.CompanyName ?? ZString.Empty;
					}
					break;
				case TransitLogColumnIDs.CIN750NotificationColumn.ToCINCode:
					if (notification is CIN750OutNotification)
					{
						return outNotification.ToCTOCIN;
					}
					break;
			}
			return ZString.Empty;
		}

		const string Padding = "    ";

		public override int MinimumColumnWidth { get; set; } = 6;
	}
}
