using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AllPackagesParentBillCollection))]
	sealed class AllPackagesParentBillCollectionTest : BusinessObjectCollectionViewTestCase<AllPackagesParentBillCollection>
	{
		public void TestIsThisPartOfTheCollection()
		{
			ParentBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			ParentBill.CU_BillNum = "1";
			var package1 = ParentBill.PackingGroups.AddNew().Packages.AddNew();

			var houseBill = ParentBill.ChildBills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "2";
			var package2 = houseBill.PackingGroups.AddNew().Packages.AddNew();

			var subhouseBill = houseBill.ChildBills.AddNew();
			subhouseBill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subhouseBill.CU_BillNum = "3";
			var package3 = subhouseBill.PackingGroups.AddNew().Packages.AddNew();

			var masterBill2 = Declaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "4";
			var package4 = masterBill2.PackingGroups.AddNew().Packages.AddNew();

			AssertEquals(3, ParentBill.AllPackages.Count);
			AssertEquals(true, ParentBill.AllPackages.Contains(package1));
			AssertEquals(true, ParentBill.AllPackages.Contains(package2));
			AssertEquals(true, ParentBill.AllPackages.Contains(package3));

			AssertEquals(1, masterBill2.AllPackages.Count);
			AssertEquals(true, masterBill2.AllPackages.Contains(package4));

			houseBill.CU_CU_ParentBill = masterBill2.PK;

			AssertEquals(1, ParentBill.AllPackages.Count);
			AssertEquals(true, ParentBill.AllPackages.Contains(package1));

			AssertEquals(3, masterBill2.AllPackages.Count);
			AssertEquals(true, masterBill2.AllPackages.Contains(package4));
			AssertEquals(true, masterBill2.AllPackages.Contains(package2));
			AssertEquals(true, masterBill2.AllPackages.Contains(package3));

			package4.CW_HouseBill = ParentBill.CU_BillUniqueCode;//should have refreshed binding

			AssertEquals(2, ParentBill.AllPackages.Count);
			AssertEquals(true, ParentBill.AllPackages.Contains(package1));
			AssertEquals(true, ParentBill.AllPackages.Contains(package4));

			AssertEquals(2, masterBill2.AllPackages.Count);
			AssertEquals(true, masterBill2.AllPackages.Contains(package2));
			AssertEquals(true, masterBill2.AllPackages.Contains(package3));
		}

		public void TestIsThisPartOfCollectionForGrandChildren()
		{
			ParentBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			ParentBill.CU_BillNum = "1";
			ParentBill.PackingGroups.AddNew().Packages.AddNew();

			var houseBill = ParentBill.ChildBills.AddNew();
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "2";
			houseBill.PackingGroups.AddNew().Packages.AddNew();

			var subhouseBill = houseBill.ChildBills.AddNew();
			subhouseBill.CU_BillType = Customs.Business.BillTypeList.Codes.SubHouseBill;
			subhouseBill.CU_BillNum = "3";
			subhouseBill.PackingGroups.AddNew().Packages.AddNew();

			var masterBill2 = Declaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "4";
			masterBill2.PackingGroups.AddNew().Packages.AddNew();

			var houseBill2 = masterBill2.ChildBills.AddNew();
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "5";
			houseBill2.PackingGroups.AddNew().Packages.AddNew();

			AssertEquals("AllPackages.Count", 3, ParentBill.AllPackages.Count);
			AssertEquals("AllPackages.Count", 2, houseBill.AllPackages.Count);
			AssertEquals("AllPackages.Count", 2, masterBill2.AllPackages.Count);
			AssertEquals("AllPackages.Count", 1, houseBill2.AllPackages.Count);

			subhouseBill.CU_CU_ParentBill = houseBill2.PK;
			AssertEquals("AllPackages.Count", 2, ParentBill.AllPackages.Count);
			AssertEquals("AllPackages.Count", 1, houseBill.AllPackages.Count);
			AssertEquals("AllPackages.Count", 3, masterBill2.AllPackages.Count);
			AssertEquals("AllPackages.Count", 2, houseBill2.AllPackages.Count);
		}

		protected override AllPackagesParentBillCollection GetCollectionToTest() => ParentBill.AllPackages;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var childBill = ParentBill.ChildBills.AddNew();
			var packGroup = Factory.New<PackingGroup>();
			packGroup.CR_CU_HouseBill = childBill.PK;
			var package = Factory.New<Package>();
			package.CW_CR_HouseContainer = packGroup.PK;
			return package;
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		Bill parentBill;
		Bill ParentBill => parentBill ?? (parentBill = Declaration.Bills.AddNew());
	}
}
