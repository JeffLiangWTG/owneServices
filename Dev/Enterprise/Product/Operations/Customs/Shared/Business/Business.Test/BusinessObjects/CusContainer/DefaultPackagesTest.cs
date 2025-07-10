using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.Business.Testing
{
	public abstract class DefaultPackagesTest : TestCaseWithFactory
	{
		public virtual void TestEndToEndTestForDetachedContainerRow()
		{
			BaseJobDeclaration declaration = GetDeclarationPackageRelevant();

			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "1";
			AssertEquals("PreCondition:One packing group is defaulted", 1, bill.PackingGroups.Count);

			BasePackage package = bill.PackingGroups[0].Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_PackType = "KG";

			BasePackage package2 = bill.PackingGroups[0].Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = "TT";

			AssertEquals("No reference to container yet", ZGuid.Empty, bill.PackingGroups[0].CR_CO_Container);

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OCLU1111110";
			AssertEquals("PreCondition:One packing group is defaulted", 1, container.PackingGroups.Count);
			AssertEquals("The existing packing group is linked to the new container", container, bill.PackingGroups[0].Container);
			AssertEquals("There is only one packing group", 1, declaration.PackingGroups.Count);

			container.Delete();//simulating detached row

			AssertEquals("reference to container is removed", ZGuid.Empty, declaration.PackingGroups[0].CR_CO_Container);
			AssertNoExceptionThrown(delegate
			{ Factory.Save(); });
			AssertEquals(bill.PackingGroups[0].PK, package.CW_CR_HouseContainer);
			AssertEquals(bill.PackingGroups[0].PK, package2.CW_CR_HouseContainer);
		}

		public virtual void TestLinkToExistingPackingGroupWithoutContainer()
		{
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "1";
			AssertEquals(true, bill.HasChanges);
			AssertEquals("one packing group should have been defaulted", 1, bill.PackingGroups.Count);

			BasePackingGroup packGroupForBill = bill.PackingGroups[0];
			AssertEquals("Container is not defaulted yet", ZGuid.Empty, packGroupForBill.CR_CO_Container);

			BaseCusContainer container = declaration.CusContainers.AddNew();
			AssertEquals(false, container.HasChanges);
			container.CO_ContainerNumber = "OCLU1111110";
			AssertEquals(true, container.HasChanges);
			AssertEquals("one packing group should have been defaulted", 1, container.PackingGroups.Count);
			AssertEquals("packGroupForBill should have been linked to the container", container.PackingGroups[0], bill.PackingGroups[0]);
		}

		public virtual void TestHasChangesForLinkToPackingGroup()
		{
			Bill housebill = declaration.Bills.AddNew();
			housebill.CU_BillType = BillTypeList.Codes.HouseBill;
			housebill.CU_BillNum = "1";
			BaseCusContainer container = declaration.CusContainers.AddNew();

			AssertEquals("There is a packing group defaulted", 1, declaration.PackingGroups.Count);
			Assert("HasChanges should not be on at this stage", declaration.HasChanges);

			housebill.CU_BillType = BillTypeList.Codes.HouseBill;
			housebill.CU_BillNum = "2";
			container.CO_ContainerNumber = "2";
			Factory.Save();//To clear HasChanges

			BasePackage package = declaration.Packages.AddNew();
			using (package.SuspendSettingHasChanges())
			{
				package.CW_PackQty = 1;//NZ deletes packages if this is empty
			}
			AssertEquals(false, package.HasChanges);

			package.CW_HouseBill = housebill.CU_BillUniqueCode;
			AssertEquals(true, package.HasChanges);

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BasePackage packageLoaded = factory2.Load<BasePackage>(package.PK);
			AssertEquals("Unless HasChanges is on, the change is not persisted to DB", package.CW_HouseBill, packageLoaded.CW_HouseBill);
		}

		public virtual void TestDefaultPackagesToNewPackingGroupIfAllAreLinkedForHouseBill()
		{
			IPackingInformation packingInformation = declaration.PackingInformationCollection.AddNew();
			Bill bill = CreateHouseBillWithoutCausingDefaultPackages();
			packingInformation.HouseBillContainer = new HouseBillContainer(bill, null);

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_BillNum = "2";
			AssertEquals("A new packing information should have been created and linked to the bill2", 2, declaration.PackingInformationCollection.Count);

			IPackingInformation newPackingInfo = declaration.PackingInformationCollection.GetElement(1);
			AssertEquals("new bill2 is linked to a new packingInfo", bill2, newPackingInfo.HouseBillContainer.HouseBill);
		}

		public virtual void TestDefaultPackagesLinkedToHouseBillForContainer()
		{
			Bill bill = CreateHouseBillWithoutCausingDefaultPackages();

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OCLU1111110";
			AssertEquals("container is linked", 1, declaration.PackingInformationCollection.Count);
			IPackingInformation packingInfo = declaration.PackingInformationCollection.GetElement(0);
			AssertEquals("new container has a pivot linked to bill which does not have previous pivot", bill, packingInfo.HouseBillContainer.HouseBill);
		}

		public virtual void TestDefaultPackagesLinkedToContainerForHouseBill()
		{
			Bill bill = CreateHouseBillWithoutCausingDefaultPackages();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "1";
			BaseCusContainer containerWithPack = CreateContainerWithoutCausingDefaultPackages();
			containerWithPack.CO_ContainerNumber = "CRUX1234562";
			BasePackage package = declaration.Packages.AddNew();
			package.CW_HouseBill = bill.CU_BillUniqueCode;
			package.CW_ContainerNoOrEquipmentNo = containerWithPack.CO_ContainerNumber;
			AssertEquals("One pivot is created", 1, declaration.PackingGroups.Count);

			BaseCusContainer containerWithoutPack = CreateContainerWithoutCausingDefaultPackages();
			AssertEquals("PreCondition:Still one pivot", 1, declaration.PackingGroups.Count);

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_BillNum = "2";
			AssertEquals("bill2 is linked", 2, declaration.PackingGroups.Count);
			BasePackingGroup packGroup = declaration.PackingGroups[1];
			AssertEquals("new bill is linked to container which does not have previous pivot", containerWithoutPack, packGroup.Container);
		}

		public virtual void TestDefaultToTheOnlyContainerWhenCreatingHouseBill()
		{
			BaseCusContainer container = CreateContainerWithoutCausingDefaultPackages();
			AssertEquals(1, declaration.CusContainers.Count);

			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "1";
			AssertEquals("bill is linked", 1, declaration.PackingInformationCollection.Count);
			IPackingInformation packingInfo = declaration.PackingInformationCollection.GetElement(0);
			AssertEquals("new container has a pivot linked to bill which does not have previous pivot", container, packingInfo.HouseBillContainer.Container);
		}

		public virtual void TestDontDefaultIfDeclarationIsPluggedIntoShipmentForHouseBill()
		{
			BaseJobDeclaration declaration = GetDeclarationPackageRelevant();

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(true, declaration.IsPluggedIntoShipment);

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "1";
			AssertEquals("No packages should be defaulted as packages should be copied from freight", 0, declaration.PackingInformationCollection.Count);

			declaration.JE_OverrideFreightDefaults = true;
			houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillNum = "2";
			AssertEquals("There should be one package created as freight default is to be overriden", 1, declaration.PackingInformationCollection.Count);
		}

		public virtual void TestDontDefaultIfDeclarationIsPluggedIntoShipmentForContainer()
		{
			BaseJobDeclaration declaration = GetDeclarationPackageRelevant();

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(true, declaration.IsPluggedIntoShipment);

			BaseCusContainer container = declaration.CusContainers.AddNew();
			AssertEquals("No packages should be defaulted as packages should be copied from freight", 0, declaration.PackingInformationCollection.Count);

			declaration.JE_OverrideFreightDefaults = true;
			declaration.JE_MasterBill = "1";
			declaration.JE_HouseBill = "2";

			container.CO_ContainerNumber = "OCLU1111110";
			AssertEquals(1, container.PackingGroups.Count);
			AssertEquals("There should be one package created as freight default is to be overriden", 1, declaration.PackingInformationCollection.Count);
		}

		Bill CreateHouseBillWithoutCausingDefaultPackages()
		{
			Bill bill = Factory.New<Bill>();
			declaration.Bills.Add(bill);
			return bill;
		}

		BaseCusContainer CreateContainerWithoutCausingDefaultPackages()
		{
			BaseCusContainer container = Factory.New<BaseCusContainer>();
			declaration.CusContainers.Add(container);
			return container;
		}

		BaseJobDeclaration declaration;
		protected abstract BaseJobDeclaration GetDeclarationPackageRelevant();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = GetDeclarationPackageRelevant();
			AssertEquals(true, declaration.IsPackingInformationRelevant);
		}
	}
}
