using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderContainerCollectionTest : TestCaseWithFactory
	{
		public void TestContainerAggregation()
		{
			AssertEquals("3 Containers", 3, CopiedOrder.PlannedContainers.Count);

			foreach (OrderContainer container in CopiedOrder.PlannedContainers)
			{
				if (container.J1_RC == ContainerTypeCommon)
				{
					AssertEquals("Contains Aggregated for same Type", OrderContainer3.J1_ContainerCount + OrderContainer4.J1_ContainerCount, container.J1_ContainerCount);
				}
			}
		}

		public void TestNoContainerNumbers()
		{
			foreach (OrderContainer container in CopiedOrder.PlannedContainers)
			{
				AssertEquals("No Container Numbers Copied", ZString.Empty, container.J1_ContainerNumber);
			}
		}

		#region Implementation

		Order CopiedOrder;
		readonly ZGuid ContainerTypeCommon = ZGuid.NewZGuid();
		OrderContainer OrderContainer3;
		OrderContainer OrderContainer4;

		protected override void SetUp()
		{
			base.SetUp();

			Order orderMain = Factory.New<Order>();

			OrderContainer orderContainer1 = orderMain.PlannedContainers.AddNew();
			OrderContainer orderContainer2 = orderMain.PlannedContainers.AddNew();
			OrderContainer3 = orderMain.PlannedContainers.AddNew();
			OrderContainer4 = orderMain.PlannedContainers.AddNew();

			orderContainer1.J1_ContainerNumber = "123";
			orderContainer1.J1_ContainerCount = 2;
			orderContainer1.J1_RC = ZGuid.NewZGuid();

			orderContainer2.J1_ContainerNumber = "222";
			orderContainer2.J1_ContainerCount = 1;
			orderContainer2.J1_RC = ZGuid.NewZGuid();

			OrderContainer3.J1_ContainerNumber = "";
			OrderContainer3.J1_ContainerCount = 5;
			OrderContainer3.J1_RC = ContainerTypeCommon;

			OrderContainer4.J1_ContainerNumber = "444";
			OrderContainer4.J1_ContainerCount = 2;
			OrderContainer4.J1_RC = ContainerTypeCommon;

			CopiedOrder = (Order)orderMain.TemplateCopy();
		}

		#endregion
	}
}
