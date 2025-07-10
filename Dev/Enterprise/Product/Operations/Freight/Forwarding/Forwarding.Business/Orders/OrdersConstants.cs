using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	/// <summary>
	/// Constants for the order management system.
	/// </summary>
	public static class OrdersConstants
	{
		public static CodeDescriptionPairList GetContainerModeList(ZString transportMode)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			switch (transportMode)
			{
				case Constants.TransportModes.Air:
					result.AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
					result.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					result.AddPair(Constants.ContainerModes.AgentConsol, Constants.ContainerModeDescriptions.AgentConsol);
					result.AddPair(Constants.ContainerModes.Other, Constants.ContainerModeDescriptions.Other);
					break;

				case Constants.TransportModes.Sea:
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
					result.AddPair(Constants.ContainerModes.Other, Constants.ContainerModeDescriptions.Other);
					break;

				case Constants.TransportModes.SeaAir:
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
					result.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					result.AddPair(Constants.ContainerModes.Other, Constants.ContainerModeDescriptions.Other);
					break;

				case Constants.TransportModes.AirSea:
					result.AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
					result.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.Other, Constants.ContainerModeDescriptions.Other);
					break;

				case Constants.TransportModes.Road:
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.FTL, Constants.ContainerModeDescriptions.FTL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.LTL, Constants.ContainerModeDescriptions.LTL);
					result.AddPair(Constants.ContainerModes.Other, Constants.ContainerModeDescriptions.Other);
					break;

				case Constants.TransportModes.Rail:
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Constants.ContainerModes.Other, Constants.ContainerModes.Other);
					break;

				case Constants.TransportModes.Courier:
					result.AddPair(Constants.ContainerModes.OnBoardCourier, Constants.ContainerModeDescriptions.OnBoardCourier);
					result.AddPair(Constants.ContainerModes.Unaccompanied, Constants.ContainerModeDescriptions.Unaccompanied);
					break;

				case Constants.TransportModes.Mail:
					result.AddPair(Constants.ContainerModes.Mail, Constants.ContainerModeDescriptions.Mail);
					break;

				case Constants.TransportModes.Unknown:
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Constants.ContainerModes.Other, Constants.ContainerModeDescriptions.Other);
					break;

				default:
					result.AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
					result.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					result.AddPair(Constants.ContainerModes.AgentConsol, Constants.ContainerModeDescriptions.AgentConsol);
					result.AddPair(Constants.ContainerModes.OnBoardCourier, Constants.ContainerModeDescriptions.OnBoardCourier);
					result.AddPair(Constants.ContainerModes.Unaccompanied, Constants.ContainerModeDescriptions.Unaccompanied);
					result.AddPair(Constants.ContainerModes.Mail, Constants.ContainerModeDescriptions.Mail);
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.FTL, Constants.ContainerModeDescriptions.FTL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.LTL, Constants.ContainerModeDescriptions.LTL);
					result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Constants.ContainerModes.Other, Constants.ContainerModeDescriptions.Other);
					break;
			}

			return result;
		}

		public static CodeDescriptionPairList GetTransportModeList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.TransportType);
			result.AddPair(Constants.TransportModes.Mail, Res.GetString("129cbb9d-b178-4091-9b92-06dd9188403a", "Post"));
			result.AddPair(Constants.TransportModes.Unknown, Constants.TransportModeDescriptions.Unknown);
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public static class OrdersAttachedState
		{
			public const string Both = "All";
			public const string AttachedOnly = "Attached";
			public const string UnattachedOnly = "Unattached";
		}

		#region Query Decider Filter List Codes

		public class DateFilterTypes
		{
			public DateFilterTypes(ICustomLabelsConfigOrgProvider orgConfigProvider)
			{
				fCustomLabelsProvider = new Order.CustomLabelsProvider(orgConfigProvider, false);
				fOrgConfigProvider = orgConfigProvider;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
			public const string None = "None";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string All = "All";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string MostCommon = "Common";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string OrderDate = "Order Date";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string ConfirmedDate = "Confirmed Date";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string FollowUpDate = "Follow Up Date";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string ReqInStore = "Required In Store";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string ReqExWorks = "Required ExWorks";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string ShipmentWindowStart = "Shipment Window Start";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string ShipmentWindowEnd = "Shipment Window End";
			public MultilingualString E_CustomDate1 { get { return GetCaptionByPropertyName(Order.Schema.JD_EstimateUserDate1); } }
			public MultilingualString A_CustomDate1 { get { return GetCaptionByPropertyName(Order.Schema.JD_ActualUserDate1); } }
			public MultilingualString E_CustomDate2 { get { return GetCaptionByPropertyName(Order.Schema.JD_EstimateUserDate2); } }
			public MultilingualString A_CustomDate2 { get { return GetCaptionByPropertyName(Order.Schema.JD_ActualUserDate2); } }
			public MultilingualString E_CustomDate3 { get { return GetCaptionByPropertyName(Order.Schema.JD_EstimateUserDate3); } }
			public MultilingualString A_CustomDate3 { get { return GetCaptionByPropertyName(Order.Schema.JD_ActualUserDate3); } }
			public MultilingualString E_CustomDate4 { get { return GetCaptionByPropertyName(Order.Schema.JD_EstimateUserDate4); } }
			public MultilingualString A_CustomDate4 { get { return GetCaptionByPropertyName(Order.Schema.JD_ActualUserDate4); } }

			#region Implementation

			protected ICustomLabelsProvider fCustomLabelsProvider;
			protected ICustomLabelsConfigOrgProvider fOrgConfigProvider;
			protected CustomLabelInfoList fCustomLabelsInfoList;

			protected MultilingualString GetCaptionByPropertyName(string propertyName)
			{
				if (fCustomLabelsInfoList == null)
				{
					fCustomLabelsInfoList = fCustomLabelsProvider.GetCustomFields(fOrgConfigProvider.ConfigOrg, fOrgConfigProvider.Factory);
				}
				return fCustomLabelsInfoList.GetFieldByPropertyName(propertyName).Caption;
			}

			#endregion
		}

		public static class NumberFilterTypes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string None = "None";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string All = "All";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string MostCommon = "Common";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string OrderNumber = "Order #";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string BookingConfRef = "Confirm #";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string InvoiceNumber = "Invoice #";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string MasterBill = "Master Bill";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string HouseBill = "House Bill";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string ProductNo = "Product #";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string ShipmentNo = "Shipment #";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string ContainerNo = "Container #";
		}

		public static class OrgFilterTypes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string None = "None";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string All = "All";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string BuyerSupplier = "Buyer / Supplier";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string SendingRecvAgent = "Send. / Recv. Agent";
		}

		public static class PortFilterTypes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string None = "None";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string All = "All";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string LoadDischargeCode = "Load / Discharge";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string AvailableAtDeliveredToCode = "Origin / Dest.";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string LoadDischargeDesc = "Consol Load / Discharge";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string AvailableAtDeliveredToDesc = "Shipment Origin / Dest.";
		}

		public static class StaffFilterTypes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter related")]
			public const string StaffFilterUserRegistered = "Registered";
		}

		#endregion

		public static class ConvertToShipmentPackLinesUserAction
		{
			public const string Existing = "EXISTING";
			public const string New = "NEW";
		}
	}
}
