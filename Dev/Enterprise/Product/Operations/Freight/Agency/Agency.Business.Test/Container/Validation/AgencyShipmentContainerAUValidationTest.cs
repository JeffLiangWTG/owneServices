using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class AgencyShipmentContainerAUValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCustomsEntryNumbers()
		{
			AgencyShipment shipment = Factory.NewWithValidTestData<AgencyShipment>();
			AgencyShipmentContainer container = shipment.RealContainers.AddNew();
			AssertNoNotifications("CustomsEntryNumberInfo should has NO Notifications", container.CustomsEntryNumberInfo);
			container.CustomsEntryNumberType = "XXXX";
			AssertHasError("CustomsEntryNumberTypeInfo should has error", container.CustomsEntryNumberTypeInfo, "Enter a valid selection.");
			container.CustomsEntryNumberType = "CAN";
			AssertNoError("CustomsEntryNumberTypeInfo should has NO error", container.CustomsEntryNumberTypeInfo, "Enter a valid selection.");
			AssertHasWarning("CustomsEntryNumberInfo should has warning", container.CustomsEntryNumberInfo, "Entry Number is not specified");
			container.CustomsEntryNumberType = "CCN";
			AssertHasWarning("CustomsEntryNumberInfo should has warning", container.CustomsEntryNumberInfo, "Entry Number is not specified");
			container.CustomsEntryNumber = "111";
			AssertNoNotifications("CustomsEntryNumberInfo should has NO Notifications", container.CustomsEntryNumberInfo);
			container.CustomsEntryNumberType = "CAN";
			AssertHasWarning("CustomsEntryNumberInfo should has warning", container.CustomsEntryNumberInfo, "CAN must be 9 characters.");
			container.CustomsEntryNumber = "AAAAJH4EF";
			AssertNoNotifications("CustomsEntryNumberInfo should has NO Notifications", container.CustomsEntryNumberInfo);
			shipment.CustomsEntryNumberType = "CAN";
			shipment.CustomsEntryNumber = "111";
			container.Validation.ValidateAll();
			AssertHasError("CustomsEntryNumberInfo should has error", container.CustomsEntryNumberInfo, "CAN is available on either Shipment OR Container level");
			container.CustomsEntryNumberType = "";
			AssertNoNotifications("CustomsEntryNumberInfo should has NO Notifications", container.CustomsEntryNumberInfo);
			container.CustomsEntryNumber = "AAAAJH4EF";
			AssertHasError("CustomsEntryNumberInfo should has error", container.CustomsEntryNumberInfo, "CAN is available on either Shipment OR Container level");
			shipment.CustomsEntryNumber = "";
			container.Validation.ValidateAll();
			AssertNoNotifications("CustomsEntryNumberInfo should has NO Notifications", container.CustomsEntryNumberInfo);
		}
	}
}
