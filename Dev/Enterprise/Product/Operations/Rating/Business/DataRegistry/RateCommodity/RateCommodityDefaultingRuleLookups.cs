using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class RateCommodityDefaultingRuleLookups : ZLookups
	{
		public RateCommodityDefaultingRuleLookups(RateCommodityDefaultingRule parent) : base(parent)
		{
		}

		#region RateCommodityCode

		public virtual RefCommodityCodeCollection CommodityCodes => new RefCommodityCodeCollection(Factory ?? new BusinessObjectFactory());

		#endregion

		#region Origin/Destination

		public virtual LocationCollection Locations
		{
			get
			{
				return new LocationCollection(Factory ?? new BusinessObjectFactory());
			}
		}

		#endregion

		#region ContainerMode

		public CodeDescriptionPairList ContainerModeList
		{
			get
			{
				var result = new CodeDescriptionPairList();

				var parent = (RateCommodityDefaultingRule)Parent;

				switch (parent.TransportMode)
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

		#endregion

		#region Direction
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

		#endregion

		#region Transport Mode

		public CodeDescriptionPairList TransportModeList
		{
			get
			{
				if (transportModeList == null)
				{
					transportModeList = new CodeDescriptionPairList();
					transportModeList.AddPair("SEA", ResString.GetMultilingualString("28306e35-7bf7-4ffb-9191-1e40227dd34a", "SEA"));
					transportModeList.AddPair("AIR", ResString.GetMultilingualString("74c3d541-f087-47aa-9447-555e69c6579a", "AIR"));
					transportModeList.AddPair("FSA", ResString.GetMultilingualString("5a39c0ee-23e1-41a2-b123-02c5d284d113", "FSA"));
					transportModeList.AddPair("FAS", ResString.GetMultilingualString("7ecb9375-6041-4791-9074-7fab23f54dd3", "FAS"));
					transportModeList.AddPair("ROA", ResString.GetMultilingualString("586df6b5-c209-4300-bde6-a0e432e3b1e1", "ROA"));
					transportModeList.AddPair("RAI", ResString.GetMultilingualString("7388ab2c-fbf0-47df-a931-b4215dc0f708", "RAI"));
					transportModeList.AddPair("COU", ResString.GetMultilingualString("345d832c-f30c-4cfa-86b6-1d891464e57e", "COU"));
				}
				return transportModeList;
			}
		}

		CodeDescriptionPairList transportModeList;

		#endregion

		#region ServiceLevel

		public virtual RefServiceLevelCollection ServiceLevels
		{
			get
			{
				return new RefServiceLevelCollection(Factory ?? new BusinessObjectFactory());
			}
		}

		#endregion
	}
}
