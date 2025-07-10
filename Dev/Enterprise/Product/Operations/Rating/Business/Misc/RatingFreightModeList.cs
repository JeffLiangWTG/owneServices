using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	public static class RatingFreightModeLists
	{
		public static CodeDescriptionPairList RateModeList()
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			result.AddPair(RateMode.LSE, ResString.GetMultilingualString("FreightModeList|LSE|v2", "Air Freight (LSE)"));
			result.AddPair(RateMode.ULD, ResString.GetMultilingualString("FreightModeList|ULD", "Air Freight (ULD)"));
			result.AddPair(RateMode.SEA, ResString.GetMultilingualString("FreightModeList|SEA", "Sea Freight (LCL and FCL)"));
			result.AddPair(RateMode.LCL, ResString.GetMultilingualString("FreightModeList|LCL", "Sea Freight (LCL)"));
			result.AddPair(RateMode.FCL, ResString.GetMultilingualString("FreightModeList|FCL", "Sea Freight (FCL)"));
			result.AddPair(RateMode.ROA, ResString.GetMultilingualString("FreightModeList|ROA", "Road Freight (LCL/LTL, FCL and FTL)"));
			result.AddPair(RateMode.LRO, ResString.GetMultilingualString("FreightModeList|LRO", "Road Freight (LCL/LTL)"));
			result.AddPair(RateMode.FRO, ResString.GetMultilingualString("FreightModeList|FRO", "Road Freight (FCL)"));
			result.AddPair(RateMode.FTL, ResString.GetMultilingualString("FreightModeList|FTL|v2", "Road Freight (FTL)"));
			result.AddPair(RateMode.COU, ResString.GetMultilingualString("FreightModeList|COU", "Courier"));
			result.AddPair(RateMode.RAI, ResString.GetMultilingualString("FreightModeList|RAI", "Rail Freight (LCL, FCL and FWL)"));
			result.AddPair(RateMode.LRA, ResString.GetMultilingualString("FreightModeList|LRA", "Rail Freight (LCL)"));
			result.AddPair(RateMode.FRA, ResString.GetMultilingualString("FreightModeList|FRA", "Rail Freight (FCL)"));
			result.AddPair(RateMode.FWL, ResString.GetMultilingualString("FreightModeList|FWL|v2", "Rail Freight (FWL)"));
			result.AddPair(RateMode.BCN, ResString.GetMultilingualString("FreightModeList|BCN", "Buyers Consol (BCN)"));
			result.AddPair(RateMode.SCN, ResString.GetMultilingualString("FreightModeList|SCN", "Shippers Consol (SCN)"));
			result.AddPair(RateMode.BLK, ResString.GetMultilingualString("FreightModeList|BLK", "Bulk (BLK)"));
			result.AddPair(RateMode.BBK, ResString.GetMultilingualString("FreightModeList|BBK", "Break Bulk (BBK)"));
			result.AddPair(ContainerModes.Liquid, ResString.GetMultilingualString("FreightModeList|LQD", "Liquid (LQD)"));
			result.AddPair(RateMode.ROR, ResString.GetMultilingualString("FreightModeList|ROR", "Roll On Roll Off (ROR)"));
			result.AddPair(RateMode.OBC, ResString.GetMultilingualString("FreightModeList|OBC", "On Board Courier (OBC)"));
			result.AddPair(RateMode.UNA, ResString.GetMultilingualString("FreightModeList|UNA", "Unaccompanied (UNA)"));
			return result;
		}

		public static CodeDescriptionPairList TransportModeList()
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			result.AddPair(TransportModes.Air, ResString.GetMultilingualString("FreightTransportModeList|Air", "Air Freight"));
			result.AddPair(TransportModes.Sea, ResString.GetMultilingualString("FreightTransportModeList|Sea", "Sea Freight"));
			result.AddPair(TransportModes.Road, ResString.GetMultilingualString("FreightTransportModeList|Road", "Road Freight"));
			result.AddPair(TransportModes.Rail, ResString.GetMultilingualString("FreightTransportModeList|Rail", "Rail Freight"));
			result.AddPair(TransportModes.Courier, ResString.GetMultilingualString("FreightTransportModeList|Courier", "Courier"));
			result.AddPair(TransportModes.AirSea, ResString.GetMultilingualString("FreightTransportModeList|AirSea", "Air Sea"));
			result.AddPair(TransportModes.SeaAir, ResString.GetMultilingualString("FreightTransportModeList|SeaAir", "Sea Air"));

			return result;
		}

		public static CodeDescriptionPairList ContainerModeList(string transportMode)
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			switch (transportMode)
			{
				case TransportModes.Air:
					result.AddPair(ContainerModes.Loose, Res.GetString("FreightContainerModeList|Loose", "Loose"));
					result.AddPair(ContainerModes.ULD, Res.GetString("FreightContainerModeList|ULD", "Unit Load Device"));
					result.AddPair(ContainerModes.BuyersConsol, Res.GetString("FreightContainerModeList|BuyersConsol", "Buyers Consol"));
					result.AddPair(ContainerModes.ShippersConsol, Res.GetString("FreightContainerModeList|ShippersConsol", "Shippers Consol"));
					break;

				case TransportModes.Sea:
					result.AddPair(RateMode.SEA, ResString.GetMultilingualString("FreightModeList|SEA", "Sea Freight (LCL and FCL)"));
					result.AddPair(ContainerModes.FCL, Res.GetString("FreightContainerModeList|FCL", "Full Container Load"));
					result.AddPair(ContainerModes.LCL, Res.GetString("FreightContainerModeList|LCL", "Less Container Load"));
					result.AddPair(ContainerModes.Bulk, Res.GetString("FreightContainerModeList|BLK", "Bulk"));
					result.AddPair(ContainerModes.BreakBulk, Res.GetString("FreightContainerModeList|BBK", "Break Bulk"));
					result.AddPair(ContainerModes.Liquid, Res.GetString("FreightContainerModeList|Liquid", "Liquid"));
					result.AddPair(ContainerModes.RollOnRollOff, Res.GetString("FreightContainerModeList|RollOnRollOff", "Roll On Roll Off"));
					result.AddPair(ContainerModes.BuyersConsol, Res.GetString("FreightContainerModeList|BuyersConsol", "Buyers Consol"));
					result.AddPair(ContainerModes.ShippersConsol, Res.GetString("FreightContainerModeList|ShippersConsol", "Shippers Consol"));
					break;

				case TransportModes.Road:
					result.AddPair(RateMode.ROA, ResString.GetMultilingualString("FreightModeList|ROA", "Road Freight (LCL/LTL, FCL and FTL)"));
					result.AddPair(RateMode.LRO, ResString.GetMultilingualString("FreightModeList|LRO", "Road Freight (LCL/LTL)"));
					result.AddPair(ContainerModes.FCL, Res.GetString("FreightContainerModeList|FCL", "Full Container Load"));
					result.AddPair(ContainerModes.LCL, Res.GetString("FreightContainerModeList|LCL", "Less Container Load"));
					result.AddPair(ContainerModes.FTL, Res.GetString("FreightContainerModeList|FTL", "Full Truck Load"));
					result.AddPair(ContainerModes.LTL, Res.GetString("FreightContainerModeList|LTL", "Less Truck Load"));
					result.AddPair(ContainerModes.BuyersConsol, Res.GetString("FreightContainerModeList|BuyersConsol", "Buyers Consol"));
					result.AddPair(ContainerModes.ShippersConsol, Res.GetString("FreightContainerModeList|ShippersConsol", "Shippers Consol"));
					break;

				case TransportModes.Rail:
					result.AddPair(RateMode.RAI, ResString.GetMultilingualString("FreightModeList|RAI", "Rail Freight (LCL, FCL and FWL)"));
					result.AddPair(RateMode.FWL, ResString.GetMultilingualString("FreightModeList|FWL|v2", "Rail Freight (FWL)"));
					result.AddPair(ContainerModes.FCL, Res.GetString("FreightContainerModeList|FCL", "Full Container Load"));
					result.AddPair(ContainerModes.LCL, Res.GetString("FreightContainerModeList|LCL", "Less Container Load"));
					result.AddPair(ContainerModes.Bulk, Res.GetString("FreightContainerModeList|BLK", "Bulk"));
					result.AddPair(ContainerModes.Liquid, Res.GetString("FreightContainerModeList|Liquid", "Liquid"));
					result.AddPair(ContainerModes.BreakBulk, Res.GetString("FreightContainerModeList|BBK", "Break Bulk"));
					result.AddPair(ContainerModes.BuyersConsol, Res.GetString("FreightContainerModeList|BuyersConsol", "Buyers Consol"));
					result.AddPair(ContainerModes.ShippersConsol, Res.GetString("FreightContainerModeList|ShippersConsol", "Shippers Consol"));
					break;

				case TransportModes.Courier:
					result.AddPair(RateMode.COU, ResString.GetMultilingualString("FreightContainerModeList|Courier", "Courier"));
					result.AddPair(ContainerModes.OnBoardCourier, Res.GetString("FreightContainerModeList|OnBoardCourier", "On Board Courier"));
					result.AddPair(ContainerModes.Unaccompanied, Res.GetString("FreightContainerModeList|Unaccompanied", "Unaccompanied"));
					break;

				case TransportModes.AirSea:
					result.AddPair(ContainerModes.Loose, Res.GetString("FreightContainerModeList|Loose", "Loose"));
					result.AddPair(ContainerModes.ULD, Res.GetString("FreightContainerModeList|ULD", "Unit Load Device"));
					result.AddPair(ContainerModes.LCL, Res.GetString("FreightContainerModeList|LCL", "Less Container Load"));
					break;

				case TransportModes.SeaAir:
					result.AddPair(ContainerModes.Loose, Res.GetString("FreightContainerModeList|Loose", "Loose"));
					result.AddPair(ContainerModes.ULD, Res.GetString("FreightContainerModeList|ULD", "Unit Load Device"));
					result.AddPair(ContainerModes.LCL, Res.GetString("FreightContainerModeList|LCL", "Less Container Load"));
					break;

				case "":
					result.AddPair(ContainerModes.Loose, Res.GetString("FreightContainerModeList|Loose", "Loose"));
					result.AddPair(ContainerModes.ULD, Res.GetString("FreightContainerModeList|ULD", "Unit Load Device"));
					result.AddPair(RateMode.SEA, ResString.GetMultilingualString("FreightModeList|SEA", "Sea Freight (LCL and FCL)"));
					result.AddPair(ContainerModes.FCL, Res.GetString("FreightContainerModeList|FCL", "Full Container Load"));
					result.AddPair(ContainerModes.LCL, Res.GetString("FreightContainerModeList|LCL", "Less Container Load"));
					result.AddPair(RateMode.ROA, ResString.GetMultilingualString("FreightModeList|ROA", "Road Freight (LCL/LTL, FCL and FTL)"));
					result.AddPair(RateMode.LRO, ResString.GetMultilingualString("FreightModeList|LRO", "Road Freight (LCL/LTL)"));
					result.AddPair(ContainerModes.FTL, Res.GetString("FreightContainerModeList|FTL", "Full Truck Load"));
					result.AddPair(ContainerModes.LTL, Res.GetString("FreightContainerModeList|LTL", "Less Truck Load"));
					result.AddPair(RateMode.RAI, ResString.GetMultilingualString("FreightModeList|RAI", "Rail Freight (LCL, FCL and FWL)"));
					result.AddPair(RateMode.FWL, ResString.GetMultilingualString("FreightModeList|FWL|v2", "Rail Freight (FWL)"));
					result.AddPair(RateMode.COU, ResString.GetMultilingualString("FreightContainerModeList|Courier", "Courier"));
					result.AddPair(ContainerModes.OnBoardCourier, Res.GetString("FreightContainerModeList|OnBoardCourier", "On Board Courier"));
					result.AddPair(ContainerModes.Unaccompanied, Res.GetString("FreightContainerModeList|Unaccompanied", "Unaccompanied"));
					result.AddPair(ContainerModes.BuyersConsol, Res.GetString("FreightContainerModeList|BuyersConsol", "Buyers Consol"));
					result.AddPair(ContainerModes.ShippersConsol, Res.GetString("FreightContainerModeList|ShippersConsol", "Shippers Consol"));
					result.AddPair(ContainerModes.Bulk, Res.GetString("FreightContainerModeList|BLK", "Bulk"));
					result.AddPair(ContainerModes.BreakBulk, Res.GetString("FreightContainerModeList|BBK", "Break Bulk"));
					result.AddPair(ContainerModes.Liquid, Res.GetString("FreightContainerModeList|Liquid", "Liquid"));
					result.AddPair(ContainerModes.RollOnRollOff, Res.GetString("FreightContainerModeList|RollOnRollOff", "Roll On Roll Off"));
					break;

				default:
					break;
			}

			return result;
		}
		public static CodeDescriptionPairList OrgRoleList()
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			result.AddPair(OrgRoles.LocalClient, ResString.GetMultilingualString("FreightOrgRolesList|LocalClient", "Local Client"));
			result.AddPair(OrgRoles.OverseasAgent, ResString.GetMultilingualString("FreightOrgRolesList|OverseasAgent", "Overseas Agent"));

			return result;
		}
	}
}
