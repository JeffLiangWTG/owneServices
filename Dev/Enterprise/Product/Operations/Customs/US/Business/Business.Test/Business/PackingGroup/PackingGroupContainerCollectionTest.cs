using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PackingGroupContainerCollection))]
	sealed class PackingGroupContainerCollectionTest : Customs.Business.Testing.BasePackingGroupContainerCollectionTest<PackingGroupContainerCollection>
	{
		public void TestUniqueBillByMasterBill()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;

			var container = declaration.CusContainers.AddNew();
			var packGroup = declaration.PackingGroups.AddNew();
			packGroup.CR_CO_Container = container.PK;
			packGroup.CR_CU_HouseBill = bill1.PK;

			packGroup = declaration.PackingGroups.AddNew();
			packGroup.CR_CO_Container = container.PK;
			packGroup.CR_CU_HouseBill = bill2.PK;

			AssertEquals(2, container.PackingGroups.Count);
			AssertEquals(bill1, container.PackingGroups.UniqueBillByMasterBill);

			bill2.CU_MasterBill = "M123";
			AssertEquals(null, container.PackingGroups.UniqueBillByMasterBill);

			bill1.CU_MasterBill = "M123";
			AssertEquals(bill1, container.PackingGroups.UniqueBillByMasterBill);
		}

		protected override PackingGroupContainerCollection GetCollectionToTest() => new PackingGroupContainerCollection(Container);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<PackingGroup>();
			result.CR_CO_Container = Container.PK;
			return result;
		}

		CusContainer container;
		new CusContainer Container
		{
			get
			{
				if (container == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.DisableDefaultPackingInformation = true;
					container = declaration.CusContainers.AddNew();
				}
				return container;
			}
		}
	}
}
