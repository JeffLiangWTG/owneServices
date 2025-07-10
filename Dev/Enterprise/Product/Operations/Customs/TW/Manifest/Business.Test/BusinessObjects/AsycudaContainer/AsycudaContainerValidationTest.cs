using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class AsycudaContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestParentType()
		{
			var cont = Factory.New<AsycudaContainer>();
			var validation = new AsycudaContainerValidation(cont);
			AssertType<AsycudaContainer>(validation.Parent);
		}

		public void TestCheckACN_ContainerNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var containers = header.Containers;
			var container1 = containers.AddNew();
			var container2 = containers.AddNew();

			var mesasge = "This container does not appear on any bills.";
			container1.Validation.ValidateACN_ContainerNumber();
			container2.Validation.ValidateACN_ContainerNumber();
			AssertHasWarning(container1.ACN_ContainerNumberInfo, mesasge);
			AssertHasWarning(container2.ACN_ContainerNumberInfo, mesasge);

			bill.LinkContainer(container1.PK);
			container1.Validation.ValidateACN_ContainerNumber();
			container2.Validation.ValidateACN_ContainerNumber();
			AssertNoWarning(container1.ACN_ContainerNumberInfo, mesasge);
			AssertHasWarning(container2.ACN_ContainerNumberInfo, mesasge);

			bill.LinkContainer(container2.PK);
			container1.Validation.ValidateACN_ContainerNumber();
			container2.Validation.ValidateACN_ContainerNumber();
			AssertNoWarning(container1.ACN_ContainerNumberInfo, mesasge);
			AssertNoWarning(container2.ACN_ContainerNumberInfo, mesasge);
		}
	}
}
