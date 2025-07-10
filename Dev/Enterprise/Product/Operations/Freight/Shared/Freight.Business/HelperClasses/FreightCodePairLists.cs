using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public static class FreightCodePairLists
	{
		public static CodeDescriptionPairList LinkableTransportModeList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(Constants.TransportModes.Air, Res.GetString("Freight|TransportModeList|Air", "Air Freight"));
			result.AddPair(Constants.TransportModes.Sea, Res.GetString("Freight|TransportModeList|Sea", "Sea Freight"));
			result.AddPair(Constants.TransportModes.Road, Res.GetString("Freight|TransportModeList|Road", "Road Freight"));
			result.AddPair(Constants.TransportModes.Rail, Res.GetString("Freight|TransportModeList|Rail", "Rail Freight"));
			return result;
		}

		public static CodeDescriptionPairList RoutingTransportModeList()
		{
			CodeDescriptionPairList result = LinkableTransportModeList();
			result.AddPair(Constants.TransportModes.Storage, Res.GetString("Freight|TransportModeList|Storage", "Storage"));
			result.AddPair(Constants.TransportModes.InlandWaterwayTransport, Res.GetString("Freight|TransportModeList|InlandWaterwayTransport2", "Inland Waterways"));
			return result;
		}

		public static CodeDescriptionPairList AdditionalTransportModeList(this Transport transport)
		{
			var result = new CodeDescriptionPairList();
			if (transport.IsAdditionalTransportModeApplicable)
			{
				if (transport.JW_TransportMode == Constants.TransportModes.InlandWaterwayTransport)
				{
					result.AddPair(Constants.TransportModes.Rail, Res.GetString("Freight|AdditionalTransportMode|RailFreight", "Rail Freight"));
				}

				result.AddPair(Constants.TransportModes.Road, Res.GetString("Freight|AdditionalTransportMode|RoadFreight", "Road Freight"));
			}

			return result;
		}

		public static CodeDescriptionPairList AdditionalTransportModeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.TransportModes.Rail, Res.GetString("Freight|AdditionalTransportMode|RailFreight", "Rail Freight"));
			result.AddPair(Constants.TransportModes.Road, Res.GetString("Freight|AdditionalTransportMode|RoadFreight", "Road Freight"));
			return result;
		}

		public static CodeDescriptionPairList RoutingTransportTypeList(ZString transportMode)
		{
			var result = new CodeDescriptionPairList();

			switch (transportMode)
			{
				case Constants.TransportModes.Air:
					result.AddPair(Constants.TransportPlanningType.Flight1, Res.GetString("Freight|TransportTypeList|Flight1", "Flight 1"));
					result.AddPair(Constants.TransportPlanningType.Flight2, Res.GetString("Freight|TransportTypeList|Flight2", "Flight 2"));
					result.AddPair(Constants.TransportPlanningType.Flight3, Res.GetString("Freight|TransportTypeList|Flight3", "Flight 3"));
					result.AddPair(Constants.TransportPlanningType.Other, Res.GetString("Freight|TransportTypeList|Other", "Other"));
					break;

				case Constants.TransportModes.Sea:
				case Constants.TransportModes.Road:
				case Constants.TransportModes.Rail:
					result.AddPair(Constants.TransportPlanningType.MainVessel, Res.GetString("Freight|TransportTypeList|MainVessel", "Main Vessel"));
					result.AddPair(Constants.TransportPlanningType.PreCarriage, Res.GetString("Freight|TransportTypeList|PreCarriage", "Pre-Carriage Vessel"));
					result.AddPair(Constants.TransportPlanningType.OnForwarding, Res.GetString("Freight|TransportTypeList|OnForwarding", "On-forwarding Vessel"));
					result.AddPair(Constants.TransportPlanningType.Other, Res.GetString("Freight|TransportTypeList|Other", "Other"));
					break;

				case Constants.TransportModes.InlandWaterwayTransport:
					result.AddPair(Constants.TransportPlanningType.PreCarriage, Res.GetString("Freight|TransportTypeList|PreCarriage", "Pre-Carriage Vessel"));
					result.AddPair(Constants.TransportPlanningType.OnForwarding, Res.GetString("Freight|TransportTypeList|OnForwarding", "On-forwarding Vessel"));
					break;

				case "":
					result.AddPair(Constants.TransportPlanningType.Flight1, Res.GetString("Freight|TransportTypeList|Flight1", "Flight 1"));
					result.AddPair(Constants.TransportPlanningType.Flight2, Res.GetString("Freight|TransportTypeList|Flight2", "Flight 2"));
					result.AddPair(Constants.TransportPlanningType.Flight3, Res.GetString("Freight|TransportTypeList|Flight3", "Flight 3"));
					result.AddPair(Constants.TransportPlanningType.MainVessel, Res.GetString("Freight|TransportTypeList|MainVessel", "Main Vessel"));
					result.AddPair(Constants.TransportPlanningType.PreCarriage, Res.GetString("Freight|TransportTypeList|PreCarriage", "Pre-Carriage Vessel"));
					result.AddPair(Constants.TransportPlanningType.OnForwarding, Res.GetString("Freight|TransportTypeList|OnForwarding", "On-forwarding Vessel"));
					result.AddPair(Constants.TransportPlanningType.Other, Res.GetString("Freight|TransportTypeList|Other", "Other"));
					break;

				default:
					break;
			}

			return result;
		}

		public static CodeDescriptionPairList AgentTypeList(bool isAir, BusinessObjectFactory factory = null)
		{
			var baseList = new CodeDescriptionPairList(OLookUpEditType.AgentType);
			if (isAir)
			{
				baseList.AddPair(Constants.AgentType.AWBCoload, Constants.AgentTypeDescriptions.AWBCoload);
				baseList.AddPair(Constants.AgentType.AWBMaster, Constants.AgentTypeDescriptions.AWBMaster);

				if (factory != null)
				{
					return factory.GetCachedValue("FreightCodePairLists.JK_AgentType_List_AIR_With_SuperManifest", () => baseList);
				}

				return baseList;
			}

			if (factory != null)
			{
				return factory.GetCachedCodeDescriptionPairList(OLookUpEditType.AgentType);
			}

			return baseList;
		}

		public static CodeDescriptionPairList ConsolModeList(ZString agentType, ZString transportMode)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			switch (transportMode)
			{
				case Constants.TransportModes.Air:
					if (agentType != Constants.AgentType.Other)
					{
						result.AddPair(Constants.ContainerModes.Loose, Res.GetString("Freight|PackingModeList|Loose", "Loose"));
						result.AddPair(Constants.ContainerModes.ULD, Res.GetString("Freight|PackingModeList|ULD", "Unit Load Device"));
						result.AddPair(Constants.ContainerModes.BuyersConsol, Res.GetString("Freight|PackingModeList|BuyersConsol", "Buyer's Consolidation"));
						result.AddPair(Constants.ContainerModes.ShippersConsol, Res.GetString("Freight|PackingModeList|ShippersConsol", "Shipper's Consolidation"));
					}
					result.AddPair(Constants.ContainerModes.Other, Res.GetString("Freight|PackingModeList|Other", "Other"));
					break;

				case Constants.TransportModes.Sea:
					if (agentType != Constants.AgentType.Other)
					{
						if (agentType != Constants.AgentType.Direct)
						{
							result.AddPair(Constants.ContainerModes.Groupage, Res.GetString("Freight|PackingModeList|Groupage", "Groupage / Freight All Kinds"));
						}

						result.AddPair(Constants.ContainerModes.FCL, Res.GetString("Freight|PackingModeList|FCL", "Full Container Load"));
						result.AddPair(Constants.ContainerModes.LCL, Res.GetString("Freight|PackingModeList|LCL", "Less Container Load"));
						result.AddPair(Constants.ContainerModes.Bulk, Res.GetString("Freight|PackingModeList|Bulk", "Bulk"));
						result.AddPair(Constants.ContainerModes.Liquid, Res.GetString("Freight|PackingModeList|Liquid", "Liquid"));
						result.AddPair(Constants.ContainerModes.BreakBulk, Res.GetString("Freight|PackingModeList|BreakBulk", "Break Bulk"));
						result.AddPair(Constants.ContainerModes.BuyersConsol, Res.GetString("Freight|PackingModeList|BuyersConsol", "Buyer's Consolidation"));
						result.AddPair(Constants.ContainerModes.ShippersConsol, Res.GetString("Freight|PackingModeList|ShippersConsol", "Shipper's Consolidation"));
						result.AddPair(Constants.ContainerModes.RollOnRollOff, Res.GetString("Freight|PackingModeList|RollOnRollOff", "Roll On/Roll Off"));
					}
					result.AddPair(Constants.ContainerModes.Other, Res.GetString("Freight|PackingModeList|Other", "Other"));
					break;

				case Constants.TransportModes.Road:
					if (agentType != Constants.AgentType.Other)
					{
						result.AddPair(Constants.ContainerModes.FCL, Res.GetString("Freight|PackingModeList|FCL", "Full Container Load"));
						result.AddPair(Constants.ContainerModes.FTL, Res.GetString("Freight|PackingModeList|FTL", "Full Truck Load"));
						result.AddPair(Constants.ContainerModes.LCL, Res.GetString("Freight|PackingModeList|LCL", "Less Container Load"));
						result.AddPair(Constants.ContainerModes.LTL, Res.GetString("Freight|PackingModeList|LTL", "Less Truck Load"));
						result.AddPair(Constants.ContainerModes.BuyersConsol, Res.GetString("Freight|PackingModeList|BuyersConsol", "Buyer's Consolidation"));
						result.AddPair(Constants.ContainerModes.ShippersConsol, Res.GetString("Freight|PackingModeList|ShippersConsol", "Shipper's Consolidation"));
						result.AddPair(Constants.ContainerModes.Groupage, Res.GetString("Freight|PackingModeList|Groupage", "Groupage / Freight All Kinds"));
					}
					result.AddPair(Constants.ContainerModes.Other, Res.GetString("Freight|PackingModeList|Other", "Other"));
					break;

				case Constants.TransportModes.Rail:
					if (agentType != Constants.AgentType.Other)
					{
						result.AddPair(Constants.ContainerModes.FCL, Res.GetString("Freight|PackingModeList|FCL", "Full Container Load"));
						result.AddPair(Constants.ContainerModes.LCL, Res.GetString("Freight|PackingModeList|LCL", "Less Container Load"));
						result.AddPair(Constants.ContainerModes.Bulk, Res.GetString("Freight|PackingModeList|Bulk", "Bulk"));
						result.AddPair(Constants.ContainerModes.Liquid, Res.GetString("Freight|PackingModeList|Liquid", "Liquid"));
						result.AddPair(Constants.ContainerModes.BreakBulk, Res.GetString("Freight|PackingModeList|BreakBulk", "Break Bulk"));
						result.AddPair(Constants.ContainerModes.BuyersConsol, Res.GetString("Freight|PackingModeList|BuyersConsol", "Buyer's Consolidation"));
						result.AddPair(Constants.ContainerModes.ShippersConsol, Res.GetString("Freight|PackingModeList|ShippersConsol", "Shipper's Consolidation"));
						result.AddPair(Constants.ContainerModes.Groupage, Res.GetString("Freight|PackingModeList|Groupage", "Groupage / Freight All Kinds"));
						result.AddPair(Constants.ContainerModes.RollOnRollOff, Res.GetString("Freight|PackingModeList|RollOnRollOff", "Roll On/Roll Off"));
					}
					result.AddPair(Constants.ContainerModes.Other, Res.GetString("Freight|PackingModeList|Other", "Other"));
					break;

				case Constants.TransportModes.Courier:
					if (agentType != Constants.AgentType.Other)
					{
						result.AddPair(Constants.ContainerModes.OnBoardCourier, Res.GetString("Freight|PackingModeList|OnBoardCourier", "On Board Courier"));
						result.AddPair(Constants.ContainerModes.Unaccompanied, Res.GetString("Freight|PackingModeList|Unaccompanied", "Unaccompanied"));
					}
					result.AddPair(Constants.ContainerModes.Other, Res.GetString("Freight|PackingModeList|Other", "Other"));
					break;

				case Constants.TransportModes.Other:
					result.AddPair(Constants.ContainerModes.Other, Res.GetString("Freight|PackingModeList|Other", "Other"));
					break;

				case "":
					result.AddPair(Constants.ContainerModes.LCL, Res.GetString("Freight|PackingModeList|LCL", "Less Container Load"));
					result.AddPair(Constants.ContainerModes.FCL, Res.GetString("Freight|PackingModeList|FCL", "Full Container Load"));
					result.AddPair(Constants.ContainerModes.Groupage, Res.GetString("Freight|PackingModeList|Groupage", "Groupage / Freight All Kinds"));
					result.AddPair(Constants.ContainerModes.BuyersConsol, Res.GetString("Freight|PackingModeList|BuyersConsol", "Buyer's Consolidation"));
					result.AddPair(Constants.ContainerModes.ShippersConsol, Res.GetString("Freight|PackingModeList|ShippersConsol", "Shipper's Consolidation"));
					result.AddPair(Constants.ContainerModes.Loose, Res.GetString("Freight|PackingModeList|Loose", "Loose"));
					result.AddPair(Constants.ContainerModes.ULD, Res.GetString("Freight|PackingModeList|ULD", "Unit Load Device"));
					result.AddPair(Constants.ContainerModes.BreakBulk, Res.GetString("Freight|PackingModeList|BreakBulk", "Break Bulk"));
					result.AddPair(Constants.ContainerModes.Bulk, Res.GetString("Freight|PackingModeList|Bulk", "Bulk"));
					result.AddPair(Constants.ContainerModes.Liquid, Res.GetString("Freight|PackingModeList|Liquid", "Liquid"));
					result.AddPair(Constants.ContainerModes.RollOnRollOff, Res.GetString("Freight|PackingModeList|RollOnRollOff", "Roll On/Roll Off"));
					result.AddPair(Constants.ContainerModes.LTL, Res.GetString("Freight|PackingModeList|LTL", "Less Truck Load"));
					result.AddPair(Constants.ContainerModes.FTL, Res.GetString("Freight|PackingModeList|FTL", "Full Truck Load"));
					result.AddPair(Constants.ContainerModes.Other, Res.GetString("Freight|PackingModeList|Other", "Other"));
					break;

				default:
					break;
			}

			return result;
		}

		public static CodeDescriptionPairList JS_TransportModeList()
		{
			return new CodeDescriptionPairList(OLookUpEditType.TransportType);
		}

		public static CodeDescriptionPairList JS_ShipmentTypeList()
		{
			return new CodeDescriptionPairList(OLookUpEditType.ShipmentType);
		}

		public static CodeDescriptionPairList JS_PackingModeList(string transportMode)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			switch (transportMode)
			{
				case Constants.TransportModes.Air:
					result.AddPair(Constants.ContainerModes.Loose, Res.GetString("Freight|PackingModeList|Loose", "Loose"));
					result.AddPair(Constants.ContainerModes.ULD, Res.GetString("Freight|PackingModeList|ULD", "Unit Load Device"));
					result.AddPair(Constants.ContainerModes.AgentConsol, Res.GetString("Freight|PackingModeList|AgentConsol", "Agent Consolidation"));
					result.AddPair(Constants.ContainerModes.BuyersConsol, Res.GetString("Freight|PackingModeList|BuyersConsol", "Buyer's Consolidation"));
					result.AddPair(Constants.ContainerModes.ShippersConsol, Res.GetString("Freight|PackingModeList|ShippersConsol", "Shipper's Consolidation"));
					break;

				case Constants.TransportModes.Sea:
					result.AddPair(Constants.ContainerModes.FCL, Res.GetString("Freight|PackingModeList|FCL", "Full Container Load"));
					result.AddPair(Constants.ContainerModes.LCL, Res.GetString("Freight|PackingModeList|LCL", "Less Container Load"));
					result.AddPair(Constants.ContainerModes.Bulk, Res.GetString("Freight|PackingModeList|Bulk", "Bulk"));
					result.AddPair(Constants.ContainerModes.Liquid, Res.GetString("Freight|PackingModeList|Liquid", "Liquid"));
					result.AddPair(Constants.ContainerModes.BreakBulk, Res.GetString("Freight|PackingModeList|BreakBulk", "Break Bulk"));
					result.AddPair(Constants.ContainerModes.BuyersConsol, Res.GetString("Freight|PackingModeList|BuyersConsol", "Buyer's Consolidation"));
					result.AddPair(Constants.ContainerModes.ShippersConsol, Res.GetString("Freight|PackingModeList|ShippersConsol", "Shipper's Consolidation"));
					result.AddPair(Constants.ContainerModes.RollOnRollOff, Res.GetString("Freight|PackingModeList|RollOnRollOff", "Roll On/Roll Off"));
					break;

				case Constants.TransportModes.SeaAir:
					result.AddPair(Constants.ContainerModes.LCL, Res.GetString("Freight|PackingModeList|LCL", "Less Container Load"));
					result.AddPair(Constants.ContainerModes.Loose, Res.GetString("Freight|PackingModeList|Loose", "Loose"));
					result.AddPair(Constants.ContainerModes.ULD, Res.GetString("Freight|PackingModeList|ULD", "Unit Load Device"));
					break;

				case Constants.TransportModes.AirSea:
					result.AddPair(Constants.ContainerModes.Loose, Res.GetString("Freight|PackingModeList|Loose", "Loose"));
					result.AddPair(Constants.ContainerModes.ULD, Res.GetString("Freight|PackingModeList|ULD", "Unit Load Device"));
					result.AddPair(Constants.ContainerModes.LCL, Res.GetString("Freight|PackingModeList|LCL", "Less Container Load"));
					break;

				case Constants.TransportModes.Road:
					result.AddPair(Constants.ContainerModes.FCL, Res.GetString("Freight|PackingModeList|FCL", "Full Container Load"));
					result.AddPair(Constants.ContainerModes.FTL, Res.GetString("Freight|PackingModeList|FTL", "Full Truck Load"));
					result.AddPair(Constants.ContainerModes.LCL, Res.GetString("Freight|PackingModeList|LCL", "Less Container Load"));
					result.AddPair(Constants.ContainerModes.LTL, Res.GetString("Freight|PackingModeList|LTL", "Less Truck Load"));
					result.AddPair(Constants.ContainerModes.BuyersConsol, Res.GetString("Freight|PackingModeList|BuyersConsol", "Buyer's Consolidation"));
					result.AddPair(Constants.ContainerModes.ShippersConsol, Res.GetString("Freight|PackingModeList|ShippersConsol", "Shipper's Consolidation"));
					break;

				case Constants.TransportModes.Rail:
					result.AddPair(Constants.ContainerModes.FCL, Res.GetString("Freight|PackingModeList|FCL", "Full Container Load"));
					result.AddPair(Constants.ContainerModes.LCL, Res.GetString("Freight|PackingModeList|LCL", "Less Container Load"));
					result.AddPair(Constants.ContainerModes.Bulk, Res.GetString("Freight|PackingModeList|Bulk", "Bulk"));
					result.AddPair(Constants.ContainerModes.Liquid, Res.GetString("Freight|PackingModeList|Liquid", "Liquid"));
					result.AddPair(Constants.ContainerModes.BreakBulk, Res.GetString("Freight|PackingModeList|BreakBulk", "Break Bulk"));
					result.AddPair(Constants.ContainerModes.BuyersConsol, Res.GetString("Freight|PackingModeList|BuyersConsol", "Buyer's Consolidation"));
					result.AddPair(Constants.ContainerModes.ShippersConsol, Res.GetString("Freight|PackingModeList|ShippersConsol", "Shipper's Consolidation"));
					break;

				case Constants.TransportModes.Courier:
					result.AddPair(Constants.ContainerModes.OnBoardCourier, Res.GetString("Freight|PackingModeList|OnBoardCourier", "On Board Courier"));
					result.AddPair(Constants.ContainerModes.Unaccompanied, Res.GetString("Freight|PackingModeList|Unaccompanied", "Unaccompanied"));
					break;

				case "":
					result.AddPair(Constants.ContainerModes.LCL, Res.GetString("Freight|PackingModeList|LCL", "Less Container Load"));
					result.AddPair(Constants.ContainerModes.FCL, Res.GetString("Freight|PackingModeList|FCL", "Full Container Load"));
					result.AddPair(Constants.ContainerModes.BuyersConsol, Res.GetString("Freight|PackingModeList|BuyersConsol", "Buyer's Consolidation"));
					result.AddPair(Constants.ContainerModes.ShippersConsol, Res.GetString("Freight|PackingModeList|ShippersConsol", "Shipper's Consolidation"));
					result.AddPair(Constants.ContainerModes.Loose, Res.GetString("Freight|PackingModeList|Loose", "Loose"));
					result.AddPair(Constants.ContainerModes.ULD, Res.GetString("Freight|PackingModeList|ULD", "Unit Load Device"));
					result.AddPair(Constants.ContainerModes.BreakBulk, Res.GetString("Freight|PackingModeList|BreakBulk", "Break Bulk"));
					result.AddPair(Constants.ContainerModes.Bulk, Res.GetString("Freight|PackingModeList|Bulk", "Bulk"));
					result.AddPair(Constants.ContainerModes.Liquid, Res.GetString("Freight|PackingModeList|Liquid", "Liquid"));
					result.AddPair(Constants.ContainerModes.RollOnRollOff, Res.GetString("Freight|PackingModeList|RollOnRollOff", "Roll On/Roll Off"));
					result.AddPair(Constants.ContainerModes.AgentConsol, Res.GetString("Freight|PackingModeList|AgentConsol", "Agent Consolidation"));
					result.AddPair(Constants.ContainerModes.LTL, Res.GetString("Freight|PackingModeList|LTL", "Less Truck Load"));
					result.AddPair(Constants.ContainerModes.FTL, Res.GetString("Freight|PackingModeList|FTL", "Full Truck Load"));
					result.AddPair(Constants.ContainerModes.OnBoardCourier, Res.GetString("Freight|PackingModeList|OnBoardCourier", "On Board Courier"));
					result.AddPair(Constants.ContainerModes.Unaccompanied, Res.GetString("Freight|PackingModeList|Unaccompanied", "Unaccompanied"));
					break;

				default:
					break;
			}

			return result;
		}

		public static CodeDescriptionPairList CartageJobFWDBookingActionList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.PreBookingAdvice, FreightConstants.LocalCartageBookingStatus.Description.PreBookingAdvice);
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.FirmBookingRequest, FreightConstants.LocalCartageBookingStatus.Description.FirmBookingRequest);
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.BookingModificationRequest, FreightConstants.LocalCartageBookingStatus.Description.BookingModificationRequest);
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.BookingCancellationRequest, FreightConstants.LocalCartageBookingStatus.Description.BookingCancellationRequest);
			return result;
		}

		public static CodeDescriptionPairList CartageJobBookingActionList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.BookingAccepted, FreightConstants.LocalCartageBookingStatus.Description.BookingAccepted);
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.BookingRejected, FreightConstants.LocalCartageBookingStatus.Description.BookingRejected);
			return result;
		}

		public static CodeDescriptionPairList CartageJobBookingStatusList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.WorkCommenced, FreightConstants.LocalCartageBookingStatus.Description.WorkCommenced);
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.GoodsReceivedOnBoard, FreightConstants.LocalCartageBookingStatus.Description.GoodsReceivedOnBoard);
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.GoodsUnloadedDelivered, FreightConstants.LocalCartageBookingStatus.Description.GoodsUnloadedDelivered);
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.WorkCompleted, FreightConstants.LocalCartageBookingStatus.Description.WorkCompleted);
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.DemurrageEventAtWharf, FreightConstants.LocalCartageBookingStatus.Description.DemurrageEventAtWharf);
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.DemurrageEventAtCFSDepot, FreightConstants.LocalCartageBookingStatus.Description.DemurrageEventAtCFSDepot);
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.DemurrageEventAtPickupPoint, FreightConstants.LocalCartageBookingStatus.Description.DemurrageEventAtPickupPoint);
			result.AddPair(FreightConstants.LocalCartageBookingStatus.Codes.SlotDateBooked, FreightConstants.LocalCartageBookingStatus.Description.SlotDateBooked);
			return result;
		}

		public static CodeDescriptionPairList CartageJobBookingLookupList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddRange(CartageJobFWDBookingActionList());
			result.AddRange(CartageJobBookingActionList());
			result.AddRange(CartageJobBookingStatusList());
			return result;
		}

		public static CodeDescriptionPairList GatewayForwarderHandlingTypeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(AgentStatusList.Codes.GatewayAgent, AgentStatusList.Descriptions.GatewayAgent);
			result.AddPair(AgentStatusList.Codes.GatewayAgentWithTariff, AgentStatusList.Descriptions.GatewayAgentWithTariff);

			return result;
		}

		public static CodeDescriptionPairList CarrierBookingStatusList()
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList();
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.NotSent, FreightConstants.CarrierBookingStatus.Description.NotSent);

			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Sent, FreightConstants.CarrierBookingStatus.Description.BookingRequest.Sent);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Acknowledged, FreightConstants.CarrierBookingStatus.Description.BookingRequest.Acknowledged);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.RejectedByInterchange, FreightConstants.CarrierBookingStatus.Description.BookingRequest.RejectedByInterchange);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Confirmed, FreightConstants.CarrierBookingStatus.Description.BookingRequest.Confirmed);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.Rejected, FreightConstants.CarrierBookingStatus.Description.BookingRequest.Rejected);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalSent, FreightConstants.CarrierBookingStatus.Description.BookingRequest.WithdrawalSent);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalRejected, FreightConstants.CarrierBookingStatus.Description.BookingRequest.WithdrawalRejected);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.WithdrawalAccepted, FreightConstants.CarrierBookingStatus.Description.BookingRequest.WithdrawalAccepted);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.BookingRequest.PendingProcessing, FreightConstants.CarrierBookingStatus.Description.BookingRequest.PendingProcessing);

			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Sent, FreightConstants.CarrierBookingStatus.Description.ShippingInstruction.Sent);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Acknowledged, FreightConstants.CarrierBookingStatus.Description.ShippingInstruction.Acknowledged);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.RejectedByInterchange, FreightConstants.CarrierBookingStatus.Description.ShippingInstruction.RejectedByInterchange);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Confirmed, FreightConstants.CarrierBookingStatus.Description.ShippingInstruction.Confirmed);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.Rejected, FreightConstants.CarrierBookingStatus.Description.ShippingInstruction.Rejected);
			result.AddPair(FreightConstants.CarrierBookingStatus.Codes.ShippingInstruction.PendingProcessing, FreightConstants.CarrierBookingStatus.Description.ShippingInstruction.PendingProcessing);

			return result;
		}

		public static CodeDescriptionPairList BillOfLadingBillTypeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.BillOfLadingBillType.Codes.Straight, Constants.BillOfLadingBillType.Descriptions.Straight);
			result.AddPair(Constants.BillOfLadingBillType.Codes.ToOrder, Constants.BillOfLadingBillType.Descriptions.ToOrder);
			result.AddPair(Constants.BillOfLadingBillType.Codes.BlankEndorse, Constants.BillOfLadingBillType.Descriptions.BlankEndorse);

			return result;
		}

		public static CodeDescriptionPairList BillOfLadingBillTermsList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.BillOfLadingBillTerms.Codes.Transferable, Constants.BillOfLadingBillTerms.Descriptions.Transferable);
			result.AddPair(Constants.BillOfLadingBillTerms.Codes.NonTransferable, Constants.BillOfLadingBillTerms.Descriptions.NonTransferable);

			return result;
		}

		public static CodeDescriptionPairList BillOfLadingBillStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillReceived, FreightConstants.BillOfLadingBillStatus.Descriptions.OriginalBillReceived);
			result.AddPair(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred, FreightConstants.BillOfLadingBillStatus.Descriptions.OriginalBillTransferred);
			result.AddPair(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress, FreightConstants.BillOfLadingBillStatus.Descriptions.OriginalBillAmendmentInProgress);
			result.AddPair(FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper, FreightConstants.BillOfLadingBillStatus.Descriptions.SwitchedToPaper);
			result.AddPair(FreightConstants.BillOfLadingBillStatus.Codes.Surrendered, FreightConstants.BillOfLadingBillStatus.Descriptions.Surrendered);

			return result;
		}

		public static CodeDescriptionPairList HouseBillOfLadingBillStatusList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(FreightConstants.BillOfLadingBillStatus.Codes.SentForPublication, FreightConstants.BillOfLadingBillStatus.Descriptions.SentForPublication);
			result.AddPair(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillPublished, FreightConstants.BillOfLadingBillStatus.Descriptions.OriginalBillPublished);
			result.AddPair(FreightConstants.BillOfLadingBillStatus.Codes.PublishingRejected, FreightConstants.BillOfLadingBillStatus.Descriptions.PublishingRejected);
			result.AddPair(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillAmendmentInProgress, FreightConstants.BillOfLadingBillStatus.Descriptions.OriginalBillAmendmentInProgress);
			result.AddPair(FreightConstants.BillOfLadingBillStatus.Codes.OriginalBillTransferred, FreightConstants.BillOfLadingBillStatus.Descriptions.OriginalBillTransferred);
			result.AddPair(FreightConstants.BillOfLadingBillStatus.Codes.SwitchedToPaper, FreightConstants.BillOfLadingBillStatus.Descriptions.SwitchedToPaper);
			result.AddPair(FreightConstants.BillOfLadingBillStatus.Codes.Surrendered, FreightConstants.BillOfLadingBillStatus.Descriptions.Surrendered);

			return result;
		}

		public static CodeDescriptionPairList PackageGroupingList()
		{
			var result = new CodeDescriptionPairList();

			result.AddPair(Constants.PackageGrouping.Codes.DoNotGroup, Constants.PackageGrouping.Description.DoNotGroup);
			result.AddPair(Constants.PackageGrouping.Codes.GroupByShipment, Constants.PackageGrouping.Description.GroupByShipment);
			result.AddPair(Constants.PackageGrouping.Codes.GroupByPackLine, Constants.PackageGrouping.Description.GroupByPackLine);

			return result;
		}

		public static CodeDescriptionPairList PackLineHouseBillPaymentTypeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(FreightConstants.PackLineHouseBillPaymentType.Codes.PaymentInCash, FreightConstants.PackLineHouseBillPaymentType.Descriptions.PaymentInCash);
			result.AddPair(FreightConstants.PackLineHouseBillPaymentType.Codes.PaymentByCreditCard, FreightConstants.PackLineHouseBillPaymentType.Descriptions.PaymentByCreditCard);
			result.AddPair(FreightConstants.PackLineHouseBillPaymentType.Codes.PaymentByCheque, FreightConstants.PackLineHouseBillPaymentType.Descriptions.PaymentByCheque);
			result.AddPair(FreightConstants.PackLineHouseBillPaymentType.Codes.Other, FreightConstants.PackLineHouseBillPaymentType.Descriptions.Other);
			result.AddPair(FreightConstants.PackLineHouseBillPaymentType.Codes.ElectronicFundsTransfer, FreightConstants.PackLineHouseBillPaymentType.Descriptions.ElectronicFundsTransfer);
			result.AddPair(FreightConstants.PackLineHouseBillPaymentType.Codes.AccountHolderWithCarrier, FreightConstants.PackLineHouseBillPaymentType.Descriptions.AccountHolderWithCarrier);
			result.AddPair(FreightConstants.PackLineHouseBillPaymentType.Codes.NotPrePaid, FreightConstants.PackLineHouseBillPaymentType.Descriptions.NotPrePaid);
			return result;
		}
	}
}
