using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BasePackingGroup.Loader))]
	sealed class BasePackingGroupLoaderTest : LoaderTestCase
	{
		public void TestPackages()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;

			var bill = declaration.Bills.AddNew();
			var packGroup = bill.PackingGroups.AddNew();

			var collection = packGroup.Packages;
			Factory.Save();

			collection.AddNew();
			AssertEquals("Precondition", 1, collection.Count);

			var newPackGroup = NewFactory().Load<BasePackingGroup>(packGroup.PK);

			newPackGroup.Delete();
			AssertEquals("Should be empty as the newPackGroup is deleted.", 0, newPackGroup.Packages.Count);

			newPackGroup.Factory.Save();
			Assert("Should be deleted from the data refresh bus.", packGroup.IsDeleted);
			AssertEquals("Should be empty as the packGroup is deleted from the data refresh bus.", 0, collection.Count);
		}

		public void TestGetElementWithHouseBillAndContainer()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			Bill bill = declaration.Bills.AddNew();
			BaseCusContainer container = declaration.CusContainers.AddNew();

			BasePackingGroup packGroup1 = declaration.PackingGroups.AddNew();
			packGroup1.CR_CU_HouseBill = bill.PK;

			var loader = new BasePackingGroup.Loader(Factory);
			AssertEquals(null, loader.GetPackingGroupMatching(bill, container));
			AssertEquals(packGroup1, loader.GetPackingGroupMatching(bill, null));
			AssertEquals(null, loader.GetPackingGroupMatching(null, null));

			BasePackingGroup packGroup3 = declaration.PackingGroups.AddNew();
			AssertEquals(null, loader.GetPackingGroupMatching(null, null));

			BasePackingGroup packGroup4 = declaration.PackingGroups.AddNew();
			packGroup4.CR_CU_HouseBill = bill.PK;
			packGroup4.CR_CO_Container = container.PK;
			AssertEquals(packGroup4, loader.GetPackingGroupMatching(bill, container));
		}

		public void TestGetElementWithHouseBillAndEquipment()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			var bill = declaration.Bills.AddNew();
			var equipment = Factory.New<CusEquipment>();
			var packingGroup1 = declaration.PackingGroups.AddNew();
			packingGroup1.CR_CU_HouseBill = bill.PK;
			packingGroup1.CR_CEQ_Equipment = equipment.PK;
			packingGroup1.CR_SystemCreateTimeUtc = ZDateTime.Now;
			var packingGroup2 = declaration.PackingGroups.AddNew();
			packingGroup2.CR_CU_HouseBill = bill.PK;
			packingGroup2.CR_CEQ_Equipment = equipment.PK;
			packingGroup2.CR_SystemCreateTimeUtc = ZDateTime.Now.AddDays(-1);

			var loader = new BasePackingGroup.Loader(Factory);
			AssertEquals(null, loader.GetPackingGroupMatching(null, null));
			AssertEquals(null, loader.GetPackingGroupMatching(bill, null));
			AssertEquals(packingGroup2, loader.GetPackingGroupMatching(bill, equipment));
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new BasePackingGroup.Loader(Factory);
		}
	}
}
