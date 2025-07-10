using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	sealed class ConsolAllocationSimulationQuantityProviderTest : TestCaseWithFactory
	{
		public void TestGetTotalCNQuantityForAllocation()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();
			var container4 = consol.Containers.AddNew();

			container1.JC_ContainerCount = 2;
			container2.JC_ContainerCount = 5;
			container3.JC_ContainerCount = 7;
			container4.JC_ContainerCount = 1;

			var quantityProvider = new ConsolAllocationSimulationQuantityProvider(consol);
			var totalQuantity = quantityProvider.GetTotalCNQuantityForAllocation();
			AssertEquals("Total Container count: 2 + 5 + 7 + 1", totalQuantity, new ZDecimal(15));
		}

		public void TestGetTotalTEUQuantityForAllocation()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();
			var container4 = consol.Containers.AddNew();

			var refCont1 = Factory.NewWithValidTestData<RefContainer>();
			var refCont2 = Factory.NewWithValidTestData<RefContainer>();
			var refCont3 = Factory.NewWithValidTestData<RefContainer>();
			var refCont4 = Factory.NewWithValidTestData<RefContainer>();

			refCont1.RC_TEU = 6;
			refCont2.RC_TEU = 1;
			refCont3.RC_TEU = 2;
			refCont4.RC_TEU = 3;

			container1.JC_ContainerCount = 2;
			container2.JC_ContainerCount = 5;
			container3.JC_ContainerCount = 7;
			container4.JC_ContainerCount = 1;

			Factory.Save();

			container1.JC_RC = refCont1.PK;
			container2.JC_RC = refCont2.PK;
			container3.JC_RC = refCont3.PK;
			container4.JC_RC = refCont4.PK;

			var quantityProvider = new ConsolAllocationSimulationQuantityProvider(consol);
			var totalQuantity = quantityProvider.GetTotalTEUQuantityForAllocation();
			AssertEquals("Total TEU Quantity: (6 * 2) + (1 * 5) + (2 * 7) + (3 * 1)", totalQuantity, new ZDecimal(34));
		}

		public void TestGetTotalCNQuantityNotAllocatedToContract()
		{
			var contract = Factory.NewWithValidTestData<RatingContract>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();
			var container4 = consol.Containers.AddNew();

			Factory.Save();

			container1.JC_RCA_AllocationLine = allocationRoute.PK;
			container2.JC_RCA_AllocationLine = allocationRoute.PK;

			container1.JC_ContainerCount = 2;
			container2.JC_ContainerCount = 5;
			container3.JC_ContainerCount = 7;
			container4.JC_ContainerCount = 1;

			var quantityProvider = new ConsolAllocationSimulationQuantityProvider(consol);
			var totalQuantity = quantityProvider.GetTotalCNQuantityNotAllocatedToContract(contract);
			AssertEquals("Total Container count: 7 + 1", totalQuantity, new ZDecimal(8));
		}

		public void TestGetTotalTEUQuantityNotAllocatedToContract()
		{
			var contract = Factory.NewWithValidTestData<RatingContract>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			allocationRoute.RCA_RCT_RatingContract = contract.PK;

			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var container3 = consol.Containers.AddNew();
			var container4 = consol.Containers.AddNew();

			var refCont1 = Factory.NewWithValidTestData<RefContainer>();
			var refCont2 = Factory.NewWithValidTestData<RefContainer>();
			var refCont3 = Factory.NewWithValidTestData<RefContainer>();
			var refCont4 = Factory.NewWithValidTestData<RefContainer>();

			refCont1.RC_TEU = 6;
			refCont2.RC_TEU = 1;
			refCont3.RC_TEU = 2;
			refCont4.RC_TEU = 3;

			Factory.Save();

			container1.JC_RCA_AllocationLine = allocationRoute.PK;
			container2.JC_RCA_AllocationLine = allocationRoute.PK;

			container1.JC_ContainerCount = 2;
			container2.JC_ContainerCount = 5;
			container3.JC_ContainerCount = 7;
			container4.JC_ContainerCount = 1;

			container1.JC_RC = refCont1.PK;
			container2.JC_RC = refCont2.PK;
			container3.JC_RC = refCont3.PK;
			container4.JC_RC = refCont4.PK;

			var quantityProvider = new ConsolAllocationSimulationQuantityProvider(consol);
			var totalQuantity = quantityProvider.GetTotalTEUQuantityNotAllocatedToContract(contract);
			AssertEquals("Total TEU Quantity: (2 * 7) + (3 * 1)", totalQuantity, new ZDecimal(17));
		}
	}
}
