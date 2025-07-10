using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class DeclarationLevelPackingGroupCollectionBaseOnlyTest : BaseDeclarationLevelPackingGroupCollectionTest<BaseJobDeclaration>
	{
	}

	[TestedType(typeof(BaseDeclarationLevelPackingGroupCollection))]
	public abstract class BaseDeclarationLevelPackingGroupCollectionTest<TJobDeclaration> : BusinessObjectCollectionTestCase
		where TJobDeclaration : BaseJobDeclaration
	{
		public void TestGetPackingGroupsWithNoPackages()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(0, declaration.PackingGroups.GetPackingGroupsWithNoPackages().Count);

			BasePackingGroup packGroup1 = declaration.PackingGroups.AddNew();
			AssertEquals(1, declaration.PackingGroups.GetPackingGroupsWithNoPackages().Count);
			AssertEquals(true, declaration.PackingGroups.GetPackingGroupsWithNoPackages().Contains(packGroup1));

			BasePackingGroup packGroup2 = declaration.PackingGroups.AddNew();
			AssertEquals(2, declaration.PackingGroups.GetPackingGroupsWithNoPackages().Count);
			packGroup2.Packages.AddNew();
			AssertEquals(1, declaration.PackingGroups.GetPackingGroupsWithNoPackages().Count);
			AssertEquals(true, declaration.PackingGroups.GetPackingGroupsWithNoPackages().Contains(packGroup1));

			declaration.PackingGroups.IsSynchronising = true;
			AssertEquals("Always empty when syncronising", 0, declaration.PackingGroups.GetPackingGroupsWithNoPackages().Count);
		}

		public void TestLoadingHouseBillsPackingGroupsAndPackagesAreIndepedent()
		{
			var mockDeclaration = Factory.NewMoq<TJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			var declaration = mockDeclaration.Object;
			declaration.DisableDefaultPackingInformation = true;

			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			var packGroup1 = declaration.PackingGroups.AddNew();
			packGroup1.CR_CU_HouseBill = bill1.PK;
			var package1 = declaration.Packages.AddNew();
			package1.CW_CR_HouseContainer = packGroup1.PK;
			package1.CW_PackQty = 1;
			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			var packGroup2 = declaration.PackingGroups.AddNew();
			packGroup2.CR_CU_HouseBill = bill2.PK;
			var package2 = declaration.Packages.AddNew();
			package2.CW_CR_HouseContainer = packGroup2.PK;
			package2.CW_PackQty = 1;
			Factory.Save();

			AssertEquals("package is not deleted", false, package1.IsDeleted);
			AssertEquals("package is not deleted", false, package2.IsDeleted);

			var factory2 = new BusinessObjectFactory();
			var mock = factory2.LoadMoq<TJobDeclaration>(declaration.PK);
			var decLoaded = mock.Object;
			mock.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);
			mock.Protected().Setup<IBillCollection<Bill, BaseJobDeclaration>>("CreateNewBillCollection").Returns(new TestHouseBillCollection(decLoaded));
			mock.Protected().Setup<BaseDeclarationLevelPackingGroupCollection>("CreateNewPackingGroups").Returns(new TestPackGroupCollection(decLoaded));

			AssertEquals("two house bills", 2, decLoaded.Bills.Count);
			AssertEquals("two pack groups. See the comment in CreateRelationshipFilter() in DeclarationLevelPackingGroupCollection", 2, decLoaded.PackingGroups.Count);
			AssertEquals("two packages. See the comment in CreateRelationshipFilter() in DeclarationLevelPackageCollection", 2, decLoaded.Packages.Count);
		}

		class TestHouseBillCollection : BillCollection<Bill, BaseJobDeclaration>
		{
			public TestHouseBillCollection(BaseJobDeclaration declaration)
				: base(declaration, declaration.Factory)
			{
			}

			protected override void SetCollectionRelationships(BusinessObject dependent)
			{
				base.SetCollectionRelationships(dependent);
				int packingGroupsCollectionIsAccessed = Declaration.PackingGroups.Count;
			}
		}

		class TestPackGroupCollection : BaseDeclarationLevelPackingGroupCollection
		{
			public TestPackGroupCollection(BaseJobDeclaration declaration)
				: base(declaration)
			{
			}

			protected override void SetCollectionRelationships(BusinessObject child)
			{
				base.SetCollectionRelationships(child);
				int packagesCollectionIsAccessed = Declaration.Packages.Count;
			}
		}

		public void TestAllowNew()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(true, declaration.PackingGroups.AllowNew);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(false, declaration.PackingGroups.AllowNew);

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals(true, declaration.PackingGroups.AllowNew);
		}

		public void TestDeclarationIsSet()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(declaration, declaration.PackingGroups.AddNew().Declaration);
		}

		public void TestRelationshipFilter()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			Bill bill = declaration.Bills.AddNew();
			BasePackingGroup packGroup = bill.PackingGroups.AddNew();

			BaseJobDeclaration declaration2 = Factory.New<BaseJobDeclaration>();

			BaseDeclarationLevelPackingGroupCollection collection = new BaseDeclarationLevelPackingGroupCollection(declaration);
			collection.Load();
			AssertEquals(1, collection.Count);

			collection = new BaseDeclarationLevelPackingGroupCollection(declaration2);
			collection.Load();
			AssertEquals(0, collection.Count);
		}

		public void TestGetElementWithHouseBillAndContainerOrEquipment()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			var bill = declaration.Bills.AddNew();
			var container = declaration.CusContainers.AddNew();

			var packGroup1 = declaration.PackingGroups.AddNew();
			packGroup1.CR_CO_Container = container.PK;

			var packGroup2 = declaration.PackingGroups.AddNew();
			packGroup2.CR_CU_HouseBill = bill.PK;

			AssertEquals(null, declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(bill, container));
			AssertEquals(packGroup1, declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(null, container));
			AssertEquals(packGroup2, declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(bill, null));
			AssertEquals(null, declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(null, null));

			var packGroup3 = declaration.PackingGroups.AddNew();
			AssertEquals(packGroup3, declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(null, null));

			var packGroup4 = declaration.PackingGroups.AddNew();
			packGroup4.CR_CU_HouseBill = bill.PK;
			packGroup4.CR_CO_Container = container.PK;
			AssertEquals(packGroup4, declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(bill, container));

			var equipment = declaration.Equipments.AddNew();
			AssertEquals(null, declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(bill, equipment));
			AssertEquals(null, declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(null, equipment));

			var packGroup5 = declaration.PackingGroups.AddNew();
			packGroup5.CR_CU_HouseBill = bill.PK;
			packGroup5.CR_CEQ_Equipment = equipment.PK;
			var packGroup6 = declaration.PackingGroups.AddNew();
			packGroup6.CR_CEQ_Equipment = equipment.PK;
			AssertEquals(packGroup5, declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(bill, equipment));
			AssertEquals(packGroup6, declaration.PackingGroups.GetElementWithHouseBillAndContainerOrEquipment(null, equipment));
		}

		public void TestGetElementWithNoHouseBill()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			Bill bill = declaration.Bills.AddNew();

			BasePackingGroup packGroup1 = declaration.PackingGroups.AddNew();
			packGroup1.CR_CU_HouseBill = bill.PK;

			BasePackingGroup packGroup2 = declaration.PackingGroups.AddNew();

			AssertEquals(packGroup2, declaration.PackingGroups.GetElementWithNoHouseBill());
		}

		public void TestGetElementWithNoContainer()
		{
			var collection = new BaseDeclarationLevelPackingGroupCollection(Factory.New<BaseJobDeclaration>());

			var mBill = Factory.New<Bill>();
			mBill.CU_BillType = BillTypeList.Codes.MasterBill;
			var mbGrp = collection.AddNew();
			mbGrp.CR_CU_HouseBill = mBill.PK;

			var hBill = Factory.New<Bill>();
			hBill.CU_BillType = BillTypeList.Codes.HouseBill;
			var hbGrp = collection.AddNew();
			hbGrp.CR_CU_HouseBill = hBill.PK;

			var grp = collection.GetElementWithNoContainer();
			AssertEquals(hBill.PK, grp.CR_CU_HouseBill);

			hbGrp.CR_CO_Container = Factory.New<BaseCusContainer>().PK;
			grp = collection.GetElementWithNoContainer();
			AssertNull(grp);

			collection.RemoveAndDelete(hbGrp);
			grp = collection.GetElementWithNoContainer();
			AssertEquals(mBill.PK, grp.CR_CU_HouseBill);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BaseDeclarationLevelPackingGroupCollection(Declaration);
		}

		protected BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<BaseJobDeclaration>();
				}
				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;
	}
}
