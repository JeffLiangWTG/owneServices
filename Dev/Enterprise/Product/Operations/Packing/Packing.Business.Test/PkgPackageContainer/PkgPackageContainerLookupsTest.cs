namespace Enterprise.Packing.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Core;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Business;
	using Enterprise.ZArchitecture.Core;

	public class PkgPackageContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestContainerModes

		public void TestContainerModes()
		{
			var container = Factory.New<PkgPackageContainer>();
			AssertContainsExactElementsInAnyOrder(Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.ContainerMode), container.Lookups.ContainerModes);
		}

		#endregion

		#region TestContainerStatuses

		public void TestContainerStatuses()
		{
			var container = Factory.New<PkgPackageContainer>();
			AssertEquals(FreightDataRegistry.Instance.ContainerStatusList.Value, container.Lookups.ContainerStatuses);
		}

		#endregion

		#region TestContainerQualities

		public void TestContainerQualities()
		{
			var container = Factory.New<PkgPackageContainer>();
			AssertEquals(FreightDataRegistry.Instance.ContainerQualityList.Value, container.Lookups.ContainerQualities);
		}

		#endregion

		#region TestAirVentFlowRateUnits

		public void TestAirVentFlowRateUnits()
		{
			var container = Factory.New<PkgPackageContainer>();

			AssertContainsExactElementsInAnyOrder(
				new CodeDescriptionPair[]
				{
					new CodeDescriptionPair("2L", "Cubic feet per minute"),
					new CodeDescriptionPair("MQH", "Cubic meters per hour"),
					new CodeDescriptionPair("P1", "Percent"),
				},
				container.Lookups.AirVentFlowRateUnits);
		}

		#endregion

		#region TestSealParty_List

		public void TestSealParty_List()
		{
			var container = Factory.New<PkgPackageContainer>();

			AssertContainsExactElementsInAnyOrder(
				new CodeDescriptionPair[]
				{
					new CodeDescriptionPair(Constants.ContainerSealParties.Codes.CarrierShippingLine, Constants.ContainerSealParties.Descriptions.CarrierShippingLine),
					new CodeDescriptionPair(Constants.ContainerSealParties.Codes.ConsignorShipper, Constants.ContainerSealParties.Descriptions.ConsignorShipper),
					new CodeDescriptionPair(Constants.ContainerSealParties.Codes.Customs, Constants.ContainerSealParties.Descriptions.Customs),
					new CodeDescriptionPair(Constants.ContainerSealParties.Codes.Quarantine, Constants.ContainerSealParties.Descriptions.Quarantine),
					new CodeDescriptionPair(Constants.ContainerSealParties.Codes.Terminal, Constants.ContainerSealParties.Descriptions.Terminal),
				},
				container.Lookups.SealParty_List
				);
		}

		#endregion
	}
}

