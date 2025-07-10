using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RateCommodityDefaultingRule))]
	public class RateCommodityDefaultingRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestRateCommodityCode()
		{
			var rateCommodityDefaultingRule = new RateCommodityDefaultingRule();

			rateCommodityDefaultingRule.RateCommodityCode = "";
			AssertHasError(rateCommodityDefaultingRule.RateCommodityCodeInfo, "Please enter a Commodity.");
			rateCommodityDefaultingRule.RateCommodityCode = "XXX";
			AssertHasError(rateCommodityDefaultingRule.RateCommodityCodeInfo, "Enter a valid Commodity.");
			rateCommodityDefaultingRule.RateCommodityCode = "GEN";
			AssertNoErrors(rateCommodityDefaultingRule.RateCommodityCodeInfo);
		}

		public void Test_Origin()
		{
			var rateCommodityDefaultingRule = new RateCommodityDefaultingRule();

			rateCommodityDefaultingRule.Origin = "";
			AssertNoErrors(rateCommodityDefaultingRule.OriginInfo);
			rateCommodityDefaultingRule.Origin = "XXX";
			AssertHasError(rateCommodityDefaultingRule.OriginInfo, "Enter a valid Origin.");
			rateCommodityDefaultingRule.Origin = "AUSYD";
			AssertNoErrors(rateCommodityDefaultingRule.OriginInfo);
		}

		public void Test_Destination()
		{
			var rateCommodityDefaultingRule = new RateCommodityDefaultingRule();

			rateCommodityDefaultingRule.Destination = "";
			AssertNoErrors(rateCommodityDefaultingRule.DestinationInfo);
			rateCommodityDefaultingRule.Destination = "XXX";
			AssertHasError(rateCommodityDefaultingRule.DestinationInfo, "Enter a valid Destination.");
			rateCommodityDefaultingRule.Destination = "AUSYD";
			AssertNoErrors(rateCommodityDefaultingRule.DestinationInfo);
		}

		public void TestTransportMode()
		{
			var rateCommodityDefaultingRule = new RateCommodityDefaultingRule();

			rateCommodityDefaultingRule.TransportMode = "";
			AssertNoErrors(rateCommodityDefaultingRule.TransportModeInfo);
			rateCommodityDefaultingRule.TransportMode = "XXX";
			AssertHasError(rateCommodityDefaultingRule.TransportModeInfo, "Enter a valid Transport Mode.");
			rateCommodityDefaultingRule.TransportMode = "SEA";
			AssertNoErrors(rateCommodityDefaultingRule.TransportModeInfo);
		}

		public void TestContainerMode()
		{
			var rateCommodityDefaultingRule = new RateCommodityDefaultingRule();

			rateCommodityDefaultingRule.ContainerMode = "";
			AssertNoErrors(rateCommodityDefaultingRule.ContainerModeInfo);
			rateCommodityDefaultingRule.ContainerMode = "XXX";
			AssertHasError(rateCommodityDefaultingRule.ContainerModeInfo, "Enter a valid Container Mode.");
			rateCommodityDefaultingRule.ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertHasError(rateCommodityDefaultingRule.ContainerModeInfo, "Enter a valid Container Mode.");
			rateCommodityDefaultingRule.TransportMode = "SEA";
			rateCommodityDefaultingRule.ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertNoErrors(rateCommodityDefaultingRule.ContainerModeInfo);
			rateCommodityDefaultingRule.ContainerMode = Core.Constants.ContainerModes.OnBoardCourier;
			AssertHasError(rateCommodityDefaultingRule.ContainerModeInfo, "Enter a valid Container Mode.");
			rateCommodityDefaultingRule.TransportMode = "COU";
			AssertNoErrors(rateCommodityDefaultingRule.ContainerModeInfo);
		}

		public void Test_Direction()
		{
			var rateCommodityDefaultingRule = new RateCommodityDefaultingRule();

			rateCommodityDefaultingRule.Direction = "";
			AssertNoErrors(rateCommodityDefaultingRule.DirectionInfo);
			rateCommodityDefaultingRule.Direction = "XXX";
			AssertHasError(rateCommodityDefaultingRule.DirectionInfo, "Enter a valid Direction.");
			rateCommodityDefaultingRule.Direction = "IMP";
			AssertNoErrors(rateCommodityDefaultingRule.DirectionInfo);
		}

		public void Test_ServiceLevel()
		{
			var rateCommodityDefaultingRule = new RateCommodityDefaultingRule();

			rateCommodityDefaultingRule.ServiceLevel = "";
			AssertNoErrors(rateCommodityDefaultingRule.ServiceLevelInfo);
			rateCommodityDefaultingRule.ServiceLevel = "XXX";
			AssertHasError(rateCommodityDefaultingRule.ServiceLevelInfo, "Enter a valid Service Level.");
			rateCommodityDefaultingRule.ServiceLevel = "DIR";
			AssertNoErrors(rateCommodityDefaultingRule.ServiceLevelInfo);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new RateCommodityDefaultingRule();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
