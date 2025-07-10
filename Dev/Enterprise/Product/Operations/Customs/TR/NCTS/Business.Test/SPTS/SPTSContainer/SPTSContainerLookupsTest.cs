using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.TR.NCTS.Business.Testing
{
	class SPTSContainerLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestContainersList()
		{
			var header = Factory.New<SPTSHeader>();
			var container1 = header.HeaderContainers.AddNew();
			container1.BC_ContainerNum = "XX1111";
			var container2 = header.HeaderContainers.AddNew();
			container2.BC_ContainerNum = "XX2222";
			var container3 = header.HeaderContainers.AddNew();
			container3.BC_ContainerNum = "XX3333";
			var bill = header.Bills.AddNew();
			Factory.Save();

			var billContainer = bill.SPTSBillContainers.AddNew();
			var list = billContainer.Lookups.ContainersList;
			AssertEquals(3, list.Count);
			AssertEquals(list[0].Code, "XX1111");
			AssertEquals(list[1].Code, "XX2222");
			AssertEquals(list[2].Code, "XX3333");
		}
	}
}
