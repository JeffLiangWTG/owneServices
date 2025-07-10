using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgRateCommodityDefaultingRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void Test_TransportMode()
		{
			var rateCommodityDefaultingRule = Factory.New<OrgRateCommodityDefaultingRule>();

			rateCommodityDefaultingRule.ORC_TransportMode = "";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_TransportModeInfo);
			rateCommodityDefaultingRule.ORC_TransportMode = "XXX";
			AssertHasError(rateCommodityDefaultingRule.ORC_TransportModeInfo, "Enter a valid Transport Mode.");
			rateCommodityDefaultingRule.ORC_TransportMode = Constants.TransportModes.Sea;
			AssertNoErrors(rateCommodityDefaultingRule.ORC_TransportModeInfo);
		}

		public void Test_ContainerMode()
		{
			var rateCommodityDefaultingRule = Factory.New<OrgRateCommodityDefaultingRule>();

			rateCommodityDefaultingRule.ORC_ContainerMode = "";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_ContainerModeInfo);
			rateCommodityDefaultingRule.ORC_ContainerMode = "XXX";
			AssertHasError(rateCommodityDefaultingRule.ORC_ContainerModeInfo, "Enter a valid Container Mode.");
			rateCommodityDefaultingRule.ORC_TransportMode = Constants.TransportModes.Sea;
			rateCommodityDefaultingRule.ORC_ContainerMode = Constants.ContainerModes.FCL;
			AssertNoErrors(rateCommodityDefaultingRule.ORC_ContainerModeInfo);
		}

		public void Test_CommodityCode()
		{
			var rateCommodityDefaultingRule = Factory.New<OrgRateCommodityDefaultingRule>();

			rateCommodityDefaultingRule.ORC_RH_NKCommodityCode = "";
			AssertHasError(rateCommodityDefaultingRule.ORC_RH_NKCommodityCodeInfo, "Please enter a Commodity.");
			rateCommodityDefaultingRule.ORC_RH_NKCommodityCode = "XXX";
			AssertHasError(rateCommodityDefaultingRule.ORC_RH_NKCommodityCodeInfo, "Enter a valid Commodity.");
			rateCommodityDefaultingRule.ORC_RH_NKCommodityCode = "ALUM";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_RH_NKCommodityCodeInfo);
		}

		public void Test_Direction()
		{
			var rateCommodityDefaultingRule = Factory.New<OrgRateCommodityDefaultingRule>();

			rateCommodityDefaultingRule.ORC_Direction = "";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_DirectionInfo);
			rateCommodityDefaultingRule.ORC_Direction = "XXX";
			AssertHasError(rateCommodityDefaultingRule.ORC_DirectionInfo, "Enter a valid Direction.");
			rateCommodityDefaultingRule.ORC_Direction = "IMP";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_DirectionInfo);
		}

		public void Test_Origin()
		{
			var rateCommodityDefaultingRule = Factory.New<OrgRateCommodityDefaultingRule>();

			rateCommodityDefaultingRule.ORC_Origin = "";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_OriginInfo);
			rateCommodityDefaultingRule.ORC_Origin = "XXX";
			AssertHasError(rateCommodityDefaultingRule.ORC_OriginInfo, "Enter a valid Origin.");
			rateCommodityDefaultingRule.ORC_Origin = "AUSYD";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_OriginInfo);
			rateCommodityDefaultingRule.ORC_Origin = "AU";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_OriginInfo);
		}

		public void Test_Destination()
		{
			var rateCommodityDefaultingRule = Factory.New<OrgRateCommodityDefaultingRule>();

			rateCommodityDefaultingRule.ORC_Destination = "";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_DestinationInfo);
			rateCommodityDefaultingRule.ORC_Destination = "XXX";
			AssertHasError(rateCommodityDefaultingRule.ORC_DestinationInfo, "Enter a valid Destination.");
			rateCommodityDefaultingRule.ORC_Destination = "AUSYD";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_DestinationInfo);
			rateCommodityDefaultingRule.ORC_Destination = "AU";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_DestinationInfo);
		}

		public void Test_ServiceLevel()
		{
			var rateCommodityDefaultingRule = Factory.New<OrgRateCommodityDefaultingRule>();

			rateCommodityDefaultingRule.ORC_RS_NKServiceLevel = "";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_RS_NKServiceLevelInfo);
			rateCommodityDefaultingRule.ORC_RS_NKServiceLevel = "XXX";
			AssertHasError(rateCommodityDefaultingRule.ORC_RS_NKServiceLevelInfo, "Enter a valid Service Level.");
			rateCommodityDefaultingRule.ORC_RS_NKServiceLevel = "DIR";
			AssertNoErrors(rateCommodityDefaultingRule.ORC_RS_NKServiceLevelInfo);
		}
	}
}
