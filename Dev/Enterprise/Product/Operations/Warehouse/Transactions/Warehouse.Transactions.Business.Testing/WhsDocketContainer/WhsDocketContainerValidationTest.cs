namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsDocketContainerValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestCheckWC_ContainerNum

		public void TestCheckWC_ContainerNum()
		{
			Container.WC_ContainerNum = "111";
			AssertNoErrors("Should not have error", Container.WC_ContainerNumInfo);

			Container.WC_ContainerNum = "";
			AssertHasError(Container.WC_ContainerNumInfo, "Please enter a container number");
		}

		#endregion

		#region TestCheckWC_ContainerNum_IsUniqueOnParent

		public void TestCheckWC_ContainerNum_IsUniqueOnParent()
		{
			var whsOrder = Factory.NewWithValidTestData<WhsOrder>();
			whsOrder.Containers.Add(Container);

			var duplicateContainer = whsOrder.Containers.AddNew();
			duplicateContainer.WC_ContainerNum = "111";

			Container.WC_ContainerNum = "";
			AssertHasError("Precondition", Container.WC_ContainerNumInfo, "Please enter a container number");

			Container.WC_ContainerNum = "222";
			AssertNoErrors("Should not have error", Container.WC_ContainerNumInfo);

			Container.WC_ContainerNum = "111";
			AssertHasError(Container.WC_ContainerNumInfo, "Duplicate Container Number: 111");

			Container.WC_ContainerNum = "222";
			AssertNoErrors("Should not have error", Container.WC_ContainerNumInfo);

			duplicateContainer.WC_ContainerNum = "222";
			AssertHasError(Container.WC_ContainerNumInfo, "Duplicate Container Number: 222");
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			Container = Factory.New<WhsDocketContainer>();
		}

		WhsDocketContainer Container;
	}
}
