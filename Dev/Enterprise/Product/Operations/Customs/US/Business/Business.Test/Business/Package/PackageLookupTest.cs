using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class PackageLookupTest : TestCaseWithFactory
	{
		public void TestBillList()
		{
			var masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "22222";

			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_HouseBill = "1111";
			houseBill1.CU_MasterBill = "22222";

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_HouseBill = "3333";
			houseBill2.CU_MasterBill = "22222";

			var package = declaration.Packages.AddNew();

			AssertEquals(true, package.Lookups.BillList.Contains(houseBill1));
			AssertEquals(true, package.Lookups.BillList.Contains(houseBill2));
		}

		public void TestContainerList()
		{
			declaration.CusContainers.AddNew();
			declaration.CusContainers.AddNew();

			var package = declaration.Packages.AddNew();
			Assert(typeof(ICusContainerCollection<BaseCusContainer>).IsAssignableFrom(package.Lookups.ContainerList.GetType()));
			AssertEquals(2, package.Lookups.ContainerList.Count);
		}

		public void TestPackTypeList()
		{
			var package = declaration.Packages.AddNew();
			AssertEquals(Factory.GetCachedValue<ShippingOrPackingingUnitList>(), package.Lookups.PackTypeList);
		}

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.Bills.AddNew();
		}
	}
}
