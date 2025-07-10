using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgRateCommodityDefaultingRuleLookups : AutoOrgRateCommodityDefaultingRuleLookups
	{
		public OrgRateCommodityDefaultingRuleLookups() : this(null) { }
		public OrgRateCommodityDefaultingRuleLookups(AutoOrgRateCommodityDefaultingRule parent) : base(parent)
		{
		}

		public CodeDescriptionPairList ContainerModeList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				var parent = (OrgRateCommodityDefaultingRule)Parent;

				switch (parent.ORC_TransportMode)
				{
					case Constants.TransportModes.Air:
						result.AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
						result.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
						result.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
						result.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
						result.AddPair(Constants.ContainerModes.AgentConsol, Constants.ContainerModeDescriptions.AgentConsol);
						break;

					case Constants.TransportModes.Sea:
						result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
						result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
						result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
						result.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
						result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
						result.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
						result.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
						result.AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
						break;

					case Constants.TransportModes.SeaAir:
						result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
						result.AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
						result.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
						break;

					case Constants.TransportModes.AirSea:
						result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
						result.AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
						result.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
						break;

					case Constants.TransportModes.Road:
						result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
						result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
						result.AddPair(Constants.ContainerModes.FTL, Constants.ContainerModeDescriptions.FTL);
						result.AddPair(Constants.ContainerModes.LTL, Constants.ContainerModeDescriptions.LTL);
						result.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
						result.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
						break;

					case Constants.TransportModes.Rail:
						result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
						result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
						result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
						result.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
						result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
						result.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
						result.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
						break;

					case Constants.TransportModes.Courier:
						result.AddPair(Constants.ContainerModes.OnBoardCourier, Constants.ContainerModeDescriptions.OnBoardCourier);
						result.AddPair(Constants.ContainerModes.Unaccompanied, Constants.ContainerModeDescriptions.Unaccompanied);
						break;

					default:
						break;
				}

				return result;
			}
		}

		public CodeDescriptionPairList DirectionList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Constants.FreightShipmentDirection.Code.Export, Constants.FreightShipmentDirection.Description.Export);
				result.AddPair(Constants.FreightShipmentDirection.Code.Import, Constants.FreightShipmentDirection.Description.Import);
				result.AddPair(Constants.FreightShipmentDirection.Code.Domestic, Constants.FreightShipmentDirection.Description.Domestic);
				result.AddPair(Constants.FreightShipmentDirection.Code.Other, Constants.FreightShipmentDirection.Description.Other);
				return result;
			}
		}

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (transportModeList == null)
				{
					transportModeList = new CodeDescriptionPairList();
					transportModeList.AddPair("SEA", ResString.GetMultilingualString("28306e35-7bf7-4ffb-9191-1e40227dd34b", "SEA"));
					transportModeList.AddPair("AIR", ResString.GetMultilingualString("74c3d541-f087-47aa-9447-555e69c6579f", "AIR"));
					transportModeList.AddPair("FSA", ResString.GetMultilingualString("5a39c0ee-23e1-41a2-b123-02c5d284d112", "FSA"));
					transportModeList.AddPair("FAS", ResString.GetMultilingualString("7ecb9375-6041-4791-9074-7fab23f54dd2", "FAS"));
					transportModeList.AddPair("ROA", ResString.GetMultilingualString("586df6b5-c209-4300-bde6-a0e432e3b1e0", "ROA"));
					transportModeList.AddPair("RAI", ResString.GetMultilingualString("7388ab2c-fbf0-47df-a931-b4215dc0f707", "RAI"));
					transportModeList.AddPair("COU", ResString.GetMultilingualString("345d832c-f30c-4cfa-86b6-1d891464e57d", "COU"));
				}
				return transportModeList;
			}
		}

		CodeDescriptionPairList transportModeList;

		public LocationCollection Locations
		{
			get
			{
				return Factory.GetCachedValue("LocationCollection", delegate
				{ return new LocationCollection(Factory); });
			}
		}
	}
}
