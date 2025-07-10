using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ContractManagement.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingContainerCollectionTest : TestCaseWithFactory
	{
		public void TestFindBoxListProvider()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainerCollection containers = new ForwardingContainerCollection(consol, Factory);
			ForwardingContainer containerAAA = containers.AddNew();
			containerAAA.JC_ContainerNum = "AAA";
			ForwardingContainer containerBBB = containers.AddNew();
			containerBBB.JC_ContainerNum = "BBB";
			ForwardingContainer containerCCC = containers.AddNew();
			containerCCC.JC_ContainerNum = "CCC";

			Factory.Save();

			IFindBoxListProvider provider = containers;

			AssertEquals(containerAAA, provider.GetBusinessObjectFromCode("AAA"));
			AssertEquals(containerBBB, provider.GetBusinessObjectFromCode("BBB"));
			AssertEquals(containerCCC, provider.GetBusinessObjectFromCode("CCC"));
			AssertNull("No container", provider.GetBusinessObjectFromCode("YYY"));
			AssertNull("No container", provider.GetBusinessObjectFromCode("ZZZ"));
		}

		public void TestAllocationRouteDefaultsWhenAdding()
		{
			var consol = Factory.New<ForwardingConsol>();
			var containers = new ForwardingContainerCollection(consol, Factory);

			var contract = Factory.New<RatingContract>();
			var allocationLine = contract.Allocations.AddNew();

			consol.JK_RCA_AllocationLine = allocationLine.PK;

			var container = containers.AddNew();
			AssertEquals("Allocation route is defaulted onto new container", allocationLine.PK, container.JC_RCA_AllocationLine);
		}
	}
}
