using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgParkContainerTypeCollection))]
	sealed class OrgParkContainerTypeCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgParkContainerTypeCollection>
	{
		public void TestContainsContainerClass()
		{
			OrgCarrierAppointedAgentPorts carrierConfig = Factory.New<OrgCarrierAppointedAgentPorts>();
			OrgParkContainerTypeCollection types = new OrgParkContainerTypeCollection(carrierConfig);

			OrgParkContainerType type1 = types.AddNew();
			type1.PT_ContainerStorageClass = "ZUB";

			OrgParkContainerType type2 = types.AddNew();
			type2.PT_ContainerStorageClass = "RAK";

			AssertEquals(true, types.ContainsContainerClass("ZUB"));
			AssertEquals(false, types.ContainsContainerClass("CLN"));
			AssertEquals(true, types.ContainsContainerClass("ZUB"));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrgParkContainerType>();
		}

		protected override OrgParkContainerTypeCollection GetCollectionToTest()
		{
			OrgCarrierAppointedAgentPorts carrierConfig = Factory.New<OrgCarrierAppointedAgentPorts>();
			return new OrgParkContainerTypeCollection(carrierConfig);
		}
	}
}
