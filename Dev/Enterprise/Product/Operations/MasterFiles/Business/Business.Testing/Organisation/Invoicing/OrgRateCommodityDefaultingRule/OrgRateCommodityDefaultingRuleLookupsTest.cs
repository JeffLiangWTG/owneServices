using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRateCommodityDefaultingRuleLookupsTest : BusinessObjectLookupsTestCase
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
			parent.ORC_TransportMode = Constants.TransportModes.Air;
			AssertContainsExactElementsInAnyOrder(new string[] { "LSE", "ULD", "BCN", "SCN", "CON" }, lookups.ContainerModeList.GetAllCodes());

			parent.ORC_TransportMode = Constants.TransportModes.Sea;
			AssertContainsExactElementsInAnyOrder(new string[] { "FCL", "LCL", "BLK", "LQD", "BBK", "BCN", "SCN", "ROR" }, lookups.ContainerModeList.GetAllCodes());

			parent.ORC_TransportMode = Constants.TransportModes.SeaAir;
			AssertContainsExactElementsInAnyOrder(new string[] { "LSE", "LCL", "ULD" }, lookups.ContainerModeList.GetAllCodes());

			parent.ORC_TransportMode = Constants.TransportModes.AirSea;
			AssertContainsExactElementsInAnyOrder(new string[] { "LSE", "LCL", "ULD" }, lookups.ContainerModeList.GetAllCodes());

			parent.ORC_TransportMode = Constants.TransportModes.Road;
			AssertContainsExactElementsInAnyOrder(new string[] { "FCL", "LCL", "FTL", "LTL", "BCN", "SCN" }, lookups.ContainerModeList.GetAllCodes());

			parent.ORC_TransportMode = Constants.TransportModes.Rail;
			AssertContainsExactElementsInAnyOrder(new string[] { "FCL", "LCL", "BLK", "LQD", "BBK", "BCN", "SCN" }, lookups.ContainerModeList.GetAllCodes());

			parent.ORC_TransportMode = Constants.TransportModes.Courier;
			AssertContainsExactElementsInAnyOrder(new string[] { "OBC", "UNA" }, lookups.ContainerModeList.GetAllCodes());
		}

		#region Implementation

		public OrgRateCommodityDefaultingRuleLookupsTest()
		{
			parent = Factory.NewWithValidTestData<OrgRateCommodityDefaultingRule>();
			lookups = new OrgRateCommodityDefaultingRuleLookups(parent);
		}

		readonly OrgRateCommodityDefaultingRuleLookups lookups;
		readonly OrgRateCommodityDefaultingRule parent;

		#endregion

	}
}
