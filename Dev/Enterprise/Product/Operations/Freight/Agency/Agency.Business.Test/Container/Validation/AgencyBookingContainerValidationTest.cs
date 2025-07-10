using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyBookingContainerValidationTest : BusinessObjectValidationTestCase
	{
		#region TestASingleContainerWithoutAContainerNumberIsNotAWarning
		public void TestASingleContainerWithoutAContainerNumberIsNotAWarning()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AgencyBookingContainer container = shipment.BookedContainers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_ContainerNum = "Blah";
			AssertHasWarnings("An Invalid Container Number should still generate a warning.", container.JC_ContainerNumInfo);
			container.JC_ContainerNum = "FAKE4100011";
			AssertNoNotifications("Should not have any Notifications", container.JC_ContainerNumInfo);
			container.JC_ContainerCount = 2;
			container.JC_ContainerNum = "FAKE4100027";
			AssertHasErrors("Entering a Container Count and a container number should still generate a error.", container.JC_ContainerNumInfo);
			container.JC_ContainerCount = 1;
			container.JC_ContainerNum = "";
			AssertNoNotifications("Having an empty container num and a container count of 1 should no longer generate a warning", container.JC_ContainerNumInfo);
		}

		#endregion
		#region TestJC_ContainerCount
		public void TestJC_ContainerCount()
		{
			AgencyBooking shipment = Factory.New<AgencyBooking>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			AgencyBookingContainer container = shipment.BookedContainers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_ContainerCount = 1;
			AssertNoNotifications("Should not have any notifications", container.JC_ContainerCountInfo);
			container.JC_ContainerCount = 2;
			AssertHasErrors("Should still have an error when both the Container number is entered and the count is > 1", container.JC_ContainerCountInfo);
			container.JC_ContainerNum = "";
			container.JC_ContainerCount = 1;
			AssertNoNotifications("Should no longer have any notifications when the container count is 1 and no container num is entered", container.JC_ContainerCountInfo);
		}
		#endregion
	}
}
