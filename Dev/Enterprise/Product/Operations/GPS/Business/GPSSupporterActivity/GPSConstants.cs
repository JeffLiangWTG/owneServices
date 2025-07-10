using Enterprise.ZArchitecture.Core;

namespace Enterprise.GPS.Business
{
	public static class GPSConstants
	{
		// CLN is for Container Leg Notification
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "An explanation that must be at least 15 characters long")]
		public const string IDMarkerBegin = "CLNID=(*";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "An explanation that must be at least 15 characters long")]
		public const string IDMarkerEnd = "*)";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "An explanation that must be at least 15 characters long")]
		public const string ReplyMessageMarkerBegin = "REPLIED:[";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "An explanation that must be at least 15 characters long")]
		public const string ReplyMessageMarkerEnd = "] TO:";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "An explanation that must be at least 15 characters long")]
		public const string FenceSeperator = " - ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "An explanation that must be at least 15 characters long")]
		public const string geofenceEntryMarker = "entered geofence ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "An explanation that must be at least 15 characters long")]
		public const string geofenceExitMarker = "exited geofence ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "An explanation that must be at least 15 characters long")]
		public const string geofenceAtMarker = " at ";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "An explanation that must be at least 15 characters long")]
		public const string InSiteMarker = "In site: ";

		public class GPSInOutActivityType : CodeDescriptionPairList
		{
			public static class Codes
			{
				public const string GIN = "GIN";
				public const string GOT = "GOT";
			}

			public static class Descriptions
			{
				public static MultilingualString GIN
				{
					get { return ResString.GetMultilingualString("26929348-A1C0-4F51-92D6-5F84ECD3A972", "Geofence In"); }
				}

				public static MultilingualString GOT
				{
					get { return ResString.GetMultilingualString("A3AFDAF9-72FA-40D0-8B37-F4BFB57FA388", "Geofence Out"); }
				}
			}

			public GPSInOutActivityType()
			{
				AddPair(Codes.GIN, Descriptions.GIN);
				AddPair(Codes.GOT, Descriptions.GOT);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "An explanation that must be at least 15 characters long")]
		public static class DriverResponses
		{
			public const string reject = "reject";
			public const string futile = "futile";
			public const string deliveredto = "delivered to:";
			public const string pickuptimein = "pickuptimein";
			public const string pickuptimeout = "pickuptimeout";
			public const string deliverytimein = "deliverytimein";
			public const string deliverytimeout = "deliverytimeout";
			public const string waitpointtimein = "waitpointtimein";
			public const string waitpointtimeout = "waitpointtimeout";
		}

		#region GPSEventTypeList

		public class GPSEventTypeList : CodeDescriptionPairList
		{
			public static class Codes
			{
				public const string All = "ALL";
				public const string Custom = "CUS";
			}

			public static class Descriptions
			{
				public static MultilingualString Custom
				{
					get { return ResString.GetMultilingualString("54442654-82dd-49d0-b331-e04fcf39fe75", "Custom Event"); }
				}

				public static MultilingualString All
				{
					get { return ResString.GetMultilingualString("{0082A5AE1CA24453A2B118D843F62CC9}", "All"); }
				}
			}

			public GPSEventTypeList()
			{
				AddPair(Codes.Custom, Descriptions.Custom);
				AddPair(Codes.All, Descriptions.All);
			}
		}

		#endregion

		#region GPSNotificationEventList

		public class GPSNotificationEventList : CodeDescriptionPairList
		{
			public static class Codes
			{
				public const string Rejected = "RJT";
			}

			public static class Descriptions
			{
				public static MultilingualString Delivered
				{
					get { return ResString.GetMultilingualString("9d5bc06a-42c9-4f33-92d2-a5d612678395", "Delivered To Message response"); }
				}
				public static MultilingualString DeliveryTimeIn
				{
					get { return ResString.GetMultilingualString("1c58ae4b-0f8d-4c2b-8d6e-9448c628b9c5", "Delivery Time In Message Response"); }
				}
				public static MultilingualString DeliveryTimeOut
				{
					get { return ResString.GetMultilingualString("9a718cf2-37a4-4e63-a52c-43d0a07b6502", "Delivery Time Out Message Response"); }
				}
				public static MultilingualString Futile
				{
					get { return ResString.GetMultilingualString("2eb90f8b-51d6-4908-9f6f-0abe83f94228", "Futile Response From Vehicle"); }
				}
				public static MultilingualString PickupTimeIn
				{
					get { return ResString.GetMultilingualString("914902a4-efa4-4df9-919b-0a3e1ed5f987", "Pickup Time In Message Response"); }
				}
				public static MultilingualString PickupTimeOut
				{
					get { return ResString.GetMultilingualString("ea48be20-6b86-4c6e-bbd3-e5697f925056", "Pickup Time Out Message Response"); }
				}
				public static MultilingualString Rejected
				{
					get { return ResString.GetMultilingualString("b7a2228f-09fa-469d-aed8-447d3f6d4219", "Reject Response From Vehicle"); }
				}
				public static MultilingualString WaitPointTimeIn
				{
					get { return ResString.GetMultilingualString("d5fad847-f71b-4311-988f-50ff425bc5a6", "Wait Point Time In Message Response"); }
				}
				public static MultilingualString WaitPointTimeOut
				{
					get { return ResString.GetMultilingualString("95874a70-cf56-4550-b3ca-53eac7d6bfa0", "Wait Point Time Out Message Response"); }
				}

				public static MultilingualString InTimeUpdatedByGeofence
				{
					get { return ResString.GetMultilingualString("02680118-9506-4f4c-9f34-83ab4aa0f762", "Time In Updated By Geofence Event"); }
				}
				public static MultilingualString OutTimeUpdatedByGeofence
				{
					get { return ResString.GetMultilingualString("528813ef-d24d-4da9-879a-320abad11c67", "Time Out Updated By Geofence Event"); }
				}
				public static MultilingualString InTimeUpdatedByCustomerSite
				{
					get { return ResString.GetMultilingualString("507b87f9-ac52-4876-8f74-d6ec15a1f89f", "Time In Updated By Customer Site"); }
				}
				public static MultilingualString OutTimeUpdatedByCustomerSite
				{
					get { return ResString.GetMultilingualString("c84bf208-c96f-47fa-b1d0-596a760e45d0", "Time Out Updated By Customer Site"); }
				}

				public static MultilingualString LastMessageCancelled
				{
					get { return ResString.GetMultilingualString("4968fd17-1f54-4f0d-9445-bf391db28d01", "Last Message Canceled"); }
				}
			}

			public GPSNotificationEventList()
			{
				AddPair(GPSInOutActivityType.Codes.GIN, Descriptions.InTimeUpdatedByGeofence);
				AddPair(GPSInOutActivityType.Codes.GOT, Descriptions.OutTimeUpdatedByGeofence);
			}
		}

		#endregion
	}
}
