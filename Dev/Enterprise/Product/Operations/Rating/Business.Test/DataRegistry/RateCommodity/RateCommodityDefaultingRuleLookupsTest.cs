using CargoWise.EntityFramework.Testing;

namespace Enterprise.Rating.Business.Testing
{
	public class RateCommodityDefaultingRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestDirectionList()
		{
			var directionList = lookups.DirectionList;

			AssertContainsExactElementsInAnyOrder(new string[] { "EXP", "IMP", "DOM", "OTH" }, directionList.GetAllCodes());
		}

		public void TestTransportModeList()
		{
			var transportModeList = lookups.TransportModeList;

			AssertContainsExactElementsInAnyOrder(new string[] { "SEA", "AIR", "FSA", "FAS", "ROA", "RAI", "COU" }, transportModeList.GetAllCodes());
		}

		public void TestContainerModeList()
		{
			parent.TransportMode = Core.Constants.TransportModes.Air;
			AssertContainsExactElementsInAnyOrder(new string[] { "LSE", "ULD", "BCN", "SCN", "CON" }, lookups.ContainerModeList.GetAllCodes());

			parent.TransportMode = Core.Constants.TransportModes.Sea;
			AssertContainsExactElementsInAnyOrder(new string[] { "FCL", "LCL", "BLK", "LQD", "BBK", "BCN", "SCN", "ROR" }, lookups.ContainerModeList.GetAllCodes());

			parent.TransportMode = Core.Constants.TransportModes.SeaAir;
			AssertContainsExactElementsInAnyOrder(new string[] { "LSE", "LCL", "ULD" }, lookups.ContainerModeList.GetAllCodes());

			parent.TransportMode = Core.Constants.TransportModes.AirSea;
			AssertContainsExactElementsInAnyOrder(new string[] { "LSE", "LCL", "ULD" }, lookups.ContainerModeList.GetAllCodes());

			parent.TransportMode = Core.Constants.TransportModes.Road;
			AssertContainsExactElementsInAnyOrder(new string[] { "FCL", "LCL", "FTL", "LTL", "BCN", "SCN" }, lookups.ContainerModeList.GetAllCodes());

			parent.TransportMode = Core.Constants.TransportModes.Rail;
			AssertContainsExactElementsInAnyOrder(new string[] { "FCL", "LCL", "BLK", "LQD", "BBK", "BCN", "SCN" }, lookups.ContainerModeList.GetAllCodes());

			parent.TransportMode = Core.Constants.TransportModes.Courier;
			AssertContainsExactElementsInAnyOrder(new string[] { "OBC", "UNA" }, lookups.ContainerModeList.GetAllCodes());
		}

		#region Implementation

		public RateCommodityDefaultingRuleLookupsTest()
		{
			parent = new RateCommodityDefaultingRule();
			lookups = new RateCommodityDefaultingRuleLookups(parent);
		}

		readonly RateCommodityDefaultingRuleLookups lookups;
		readonly RateCommodityDefaultingRule parent;

		#endregion
	}
}

