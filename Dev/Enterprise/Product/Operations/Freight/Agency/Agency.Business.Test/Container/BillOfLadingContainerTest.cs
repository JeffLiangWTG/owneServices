using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class BillOfLadingContainerTest : BaseFreightTest
	{
		public void TestValidation()
		{
			BillOfLadingContainer container = Factory.NewWithValidTestData<BillOfLadingContainer>();
			AssertEquals(typeof(BillOfLadingBookingContainerValidation), container.Validation.GetType());
			container.JC_ContainerMode = Constants.ContainerModes.RollOnRollOff;
			AssertEquals(typeof(AgencyRORContainerValidation), container.Validation.GetType());
			container.JC_ContainerMode = Constants.ContainerModes.BreakBulk;
			AssertEquals(typeof(AgencyTopLevelPackValidation), container.Validation.GetType());
		}

		public void TestCantSetContainerCountToANonPositiveValue()
		{
			BillOfLading shipment = Factory.New<BillOfLading>();
			BillOfLadingContainer container = shipment.RealContainers.AddNew();
			container.JC_ContainerCount = 4;
			AssertEquals("Container.JC_ContainerCount", 4, (int)container.JC_ContainerCount);
			container.JC_ContainerCount = -4;
			AssertEquals("Container.JC_ContainerCount", 1, (int)container.JC_ContainerCount);
			container.JC_ContainerCount = 5;
			AssertEquals("Container.JC_ContainerCount", 5, (int)container.JC_ContainerCount);
			container.JC_ContainerCount = 0;
			AssertEquals("Container.JC_ContainerCount", 1, (int)container.JC_ContainerCount);
		}

		public void TestJC_DepartureDockReceipt_ReadOnly()
		{
			var shipment = Factory.New<BillOfLading>();
			var container = shipment.ShippingContainers.AddNew();
			Env.Security.AgencyBillOfLadingEditDockReceipt.IsAllowed = false;
			AssertEquals("Readonly", true, container.JC_DepartureDockReceiptInfo.ReadOnly);
			Factory.Save();
			AssertEquals("Readonly", true, container.JC_DepartureDockReceiptInfo.ReadOnly);
			Env.Security.AgencyBillOfLadingEditDockReceipt.IsAllowed = true;
			AssertEquals("Writeable if user have rights", false, container.JC_DepartureDockReceiptInfo.ReadOnly);
		}
	}
}
