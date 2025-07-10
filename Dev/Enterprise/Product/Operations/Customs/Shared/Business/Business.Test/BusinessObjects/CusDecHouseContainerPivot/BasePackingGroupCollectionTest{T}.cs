using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class BasePackingGroupCollectionTest<T> : BusinessObjectCollectionViewTestCase<T> where T : BasePackingGroupCollection
	{
		public void TestContainerNumbers()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;

			Bill bill = declaration.Bills.AddNew();

			BaseCusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX1234562";

			BaseCusContainer container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CRUX3456232";

			bill.PackingGroups.AddNew().CR_CO_Container = container1.PK;
			bill.PackingGroups.AddNew().CR_CO_Container = container2.PK;

			AssertEquals("CRUX1234562,CRUX3456232", bill.PackingGroups.ContainerNumbersLinked);
		}

		public void TestGetElementWithNoContainerExcept()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;

			Bill bill = declaration.Bills.AddNew();
			BasePackingGroup packGroup = bill.PackingGroups.AddNew();
			AssertNull(bill.PackingGroups.GetElementWithNoContainerExcept(packGroup));

			BaseCusContainer container = declaration.CusContainers.AddNew();
			packGroup.CR_CO_Container = container.PK;
			AssertNull(bill.PackingGroups.GetElementWithNoContainerExcept(Factory.New<BasePackingGroup>()));

			packGroup.CR_CO_Container = ZGuid.Empty;
			AssertEquals(packGroup, bill.PackingGroups.GetElementWithNoContainerExcept(Factory.New<BasePackingGroup>()));
		}

		public void TestSetDefaultValues()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			BasePackingGroup packGroup = bill.PackingGroups.AddNew();
			AssertEquals(bill.PK, packGroup.CR_CU_HouseBill);
		}

		public void TestDeclarationAndForeignKeySet()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill houseBill = declaration.Bills.AddNew();
			BasePackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			AssertEquals(houseBill.PK, packingGroup.CR_CU_HouseBill);
			AssertEquals(declaration, packingGroup.Declaration);
		}

		public void TestBasicCollectionFunctionality()
		{
			BasePackingGroup packingGroup = HouseBill.PackingGroups.AddNew();
			AssertEquals("Indexer should return PackingGroup Just Added", packingGroup, HouseBill.PackingGroups[0]);
		}

		public void TestGetElementWithContainer()
		{
			BaseCusContainer container1 = HouseBill.Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "ASDF1234560";
			BaseCusContainer container2 = HouseBill.Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "FDSA1234560";

			BasePackingGroup packingGroup = HouseBill.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container1.PK;

			AssertEquals("PackGroup is linked to container1", packingGroup, HouseBill.PackingGroups.GetElementWithContainer(container1));
			AssertEquals("Nothing is linked to container2", null, HouseBill.PackingGroups.GetElementWithContainer(container2));
		}

		public void TestGetElementWithContainerAddedViaAddNew()
		{
			BaseCusContainer container = HouseBill.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000006";
			BasePackingGroup packingGroup = HouseBill.PackingGroups.AddNew();
			packingGroup = HouseBill.PackingGroups.AddNew(container);
			BasePackingGroup result = HouseBill.PackingGroups.GetElementWithContainer(container);
			AssertEquals("Should have found this Packing group", packingGroup, result);
		}

		public void TestGetElementWithNoContainer()
		{
			BaseCusContainer container = HouseBill.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000006";
			BasePackingGroup packingGroup = HouseBill.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;
			packingGroup = HouseBill.PackingGroups.AddNew();
			BasePackingGroup result = HouseBill.PackingGroups.GetElementWithNoContainer();
			AssertEquals("Should have found this Packing group", packingGroup, result);
			packingGroup.Delete();
			BasePackingGroup packingGroup2 = HouseBill.PackingGroups.GetElementWithNoContainer();
			AssertNull(packingGroup2);
		}

		public void TestCountOfAllPackages()
		{
			BaseCusContainer container = HouseBill.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000006";
			BasePackingGroup packingGroup = HouseBill.PackingGroups.AddNew();
			packingGroup.CR_CO_Container = container.PK;
			packingGroup.Packages.AddNew();
			packingGroup.Packages.AddNew();
			packingGroup = HouseBill.PackingGroups.AddNew();
			packingGroup.Packages.AddNew();
			int result = HouseBill.PackingGroups.CountOfAllPackages();
			AssertEquals("Should have 3 Packages all up", 3, result);
		}

		public void TestHasPackageRecord()
		{
			BasePackingGroupCollection packingGroups = GetCollectionToTest();
			AssertEquals("PackingGroups.HasPackageRecord", false, packingGroups.HasPackageRecord);
			BasePackingGroup packingGroup = packingGroups.AddNew();
			AssertEquals("PackingGroups.HasPackageRecord", false, packingGroups.HasPackageRecord);
			BasePackage package = packingGroup.Packages.AddNew();
			AssertEquals("PackingGroups.HasPackageRecord", true, packingGroups.HasPackageRecord);
		}

		protected Bill HouseBill
		{
			get
			{
				if (fHouseBill == null)
				{
					BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
					declaration.DisableDefaultPackingInformation = true;
					fHouseBill = declaration.Bills.AddNew();
				}
				return fHouseBill;
			}
		}
		Bill fHouseBill;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BasePackingGroup result = Factory.New<BasePackingGroup>();
			result.CR_CU_HouseBill = HouseBill.PK;
			return result;
		}
	}
}
