using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class TestJobOrderContainerValidation : BusinessObjectValidationTestCase
	{
		public void TestValidateJ1_ContainerNumber()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			Order order = factory.New<Order>();
			OrderContainer container = order.PlannedContainers.AddNew();

			container.J1_ContainerNumber = "";
			AssertEquals(false, container.J1_ContainerNumberInfo.HasWarnings());

			container.J1_ContainerNumber = "xxx";
			AssertEquals(true, container.J1_ContainerNumberInfo.HasWarnings());

			container.J1_ContainerNumber = "ABCD1234560";
			AssertEquals(false, container.J1_ContainerNumberInfo.HasWarnings());
		}

		public void TestValidateJ1_RC()
		{
			RefContainer container = Factory.LoadTop1<RefContainer>(new ZQuery());

			OrderContainer cont = Factory.New<OrderContainer>();
			cont.J1_RC = container.PK;
			AssertNoErrors(cont.J1_RCInfo);

			cont.J1_RC = ZGuid.Empty;
			AssertHasErrors(cont.J1_RCInfo);
		}

		public void TestValidateJ1_ContainerCount()
		{
			var container = Factory.New<OrderContainer>();
			container.J1_ContainerCount = 1;
			AssertNoErrors(container.J1_ContainerCountInfo);

			container.J1_ContainerCount = -1;
			AssertHasErrors("Should have an error when J1_ContainerCount is negative", container.J1_ContainerCountInfo);
		}
	}
}
