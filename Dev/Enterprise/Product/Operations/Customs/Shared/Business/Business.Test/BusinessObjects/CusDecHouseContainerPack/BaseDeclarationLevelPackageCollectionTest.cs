using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BaseDeclarationLevelPackageCollection<BasePackage>))]
	public class BaseDeclarationLevelPackageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestBaseDeclarationLevelPackageCollectionOnRemove_DeclarationDeleted()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var package = declaration.Packages.AddNew();
			AssertEquals(package, ((IPackingInformationCollection)declaration.Packages).GetElement(0));
			declaration.Delete();
			AssertNoExceptionThrown(() => package.Delete());
		}

		public void TestIPackingInformationCollection()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BasePackage pack1 = declaration.Packages.AddNew();
			AssertEquals(pack1, ((IPackingInformationCollection)declaration.Packages).GetElement(0));

			BasePackage pack2 = (BasePackage)((IPackingInformationCollection)declaration.Packages).AddNew();
			AssertEquals(2, declaration.Packages.Count);

			BaseCusContainer container = declaration.CusContainers.AddNew();
			Bill bill = declaration.Bills.AddNew();

			((IPackingInformation)pack1).HouseBillContainer = new HouseBillContainer(bill, null);
			((IPackingInformation)pack2).HouseBillContainer = new HouseBillContainer(null, container);

			AssertEquals(pack1, ((IPackingInformationCollection)declaration.Packages).GetElementWithNoContainer());
			AssertEquals(pack2, ((IPackingInformationCollection)declaration.Packages).GetElementWithNoHouseBill());
		}

		public void TestGetMatchingElementForShipmentSynch()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "789";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "123";

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX89432";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CRUX89436";

			var packingGroup1 = declaration.PackingGroups.AddNew();
			packingGroup1.CR_CU_HouseBill = bill.PK;
			packingGroup1.CR_CO_Container = container1.PK;
			var package1 = packingGroup1.Packages.AddNew();

			var packingGroup2 = declaration.PackingGroups.AddNew();
			packingGroup2.CR_CU_HouseBill = bill.PK;
			var package2 = packingGroup2.Packages.AddNew();

			var packingGroup3 = declaration.PackingGroups.AddNew();
			packingGroup3.CR_CU_HouseBill = bill.PK;
			packingGroup3.CR_CO_Container = container2.PK;
			var package3 = packingGroup3.Packages.AddNew();

			AssertEquals("3 elements", 3, declaration.Packages.Count);

			declaration.Packages.MarkAsDeleteForShipmentSynch();
			Assert(package1.IsGoingToBeDeletedAfterShipmentSynch);
			Assert(package2.IsGoingToBeDeletedAfterShipmentSynch);
			Assert(package3.IsGoingToBeDeletedAfterShipmentSynch);

			AssertEquals(package2, declaration.Packages.GetMatchingElementForShipmentSynch(bill, null));
			Assert(!package2.IsGoingToBeDeletedAfterShipmentSynch);

			AssertEquals(package3, declaration.Packages.GetMatchingElementForShipmentSynch(bill, container2));
			Assert(!package3.IsGoingToBeDeletedAfterShipmentSynch);

			AssertNull(declaration.Packages.GetMatchingElementForShipmentSynch(bill2, container2));
		}

		public void TestAllowNew()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(true, declaration.Packages.AllowNew);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(false, declaration.Packages.AllowNew);

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals(true, declaration.Packages.AllowNew);
		}

		public void TestGetElementWithNoContainer()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;

			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_HouseBill = "HBL1";
			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CRUX1111";

			BasePackage package = declaration.Packages.AddNew();
			package.CW_HouseBill = bill.CU_HouseBill;

			AssertEquals(package, declaration.Packages.GetElementWithNoContainer());

			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			AssertNull(declaration.Packages.GetElementWithNoContainer());
		}

		public void TestSetCollectionRelationships()
		{
			Bill houseBill = Declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "HOUSEBILL1";
			BasePackingGroup packingGroup = houseBill.PackingGroups.AddNew();
			BasePackage package1 = packingGroup.Packages.AddNew();

			AssertEquals("Declaration.Packages.Count", 1, Declaration.Packages.Count);
			AssertEquals(Declaration.Bills, package1.BillsOnDeclaration_List);
			AssertEquals(Declaration.ContainersAndEquipmentsOnDeclaration_List, package1.ContainersAndEquipmentsOnDeclaration_List);

			BasePackage package2 = Declaration.Packages.AddNew();

			AssertEquals("HouseBillsOnDeclaration_List is Linked Correctly", Declaration.Bills, package2.BillsOnDeclaration_List);
			AssertEquals("ContainersAndEquipmentsOnDeclaration_List is Linked Correctly", Declaration.ContainersAndEquipmentsOnDeclaration_List, package2.ContainersAndEquipmentsOnDeclaration_List);
		}

		public void TestOveriddenLoad()
		{
			Bill houseBill1 = Declaration.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_HouseBill = "HOUSEBILL1";

			Bill houseBill2 = Declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = "HOUSEBILL2";

			BasePackingGroup packingGroup1 = houseBill1.PackingGroups.AddNew();
			BasePackage package1 = packingGroup1.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = "11";

			BasePackingGroup packingGroup2 = houseBill1.PackingGroups.AddNew();
			BasePackage package2 = packingGroup2.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = "22";

			BasePackingGroup packingGroup3 = houseBill1.PackingGroups.AddNew();
			BasePackage package3 = packingGroup3.Packages.AddNew();
			package3.CW_PackQty = 3;
			package3.CW_PackType = "33";

			BasePackage package4 = packingGroup3.Packages.AddNew();
			package4.CW_PackQty = 4;
			package4.CW_PackType = "44";

			var collection = GetCollectionToTest();
			collection.Load();

			AssertEquals("Collection.Count", 4, collection.Count);
			AssertEquals("Has Package1", true, collection.Contains(package1));
			AssertEquals("Has Package2", true, collection.Contains(package2));
			AssertEquals("Has Package3", true, collection.Contains(package3));
			AssertEquals("Has Package4", true, collection.Contains(package4));
		}

		public void TestCreateRelationshipFilterNoResultQuery()
		{
			BaseJobDeclaration dec1 = GetJobDeclaration();
			dec1.DisableDefaultPackingInformation = true;
			BaseJobDeclaration dec2 = GetJobDeclaration();
			dec2.DisableDefaultPackingInformation = true;
			dec2.Bills.AddNew();

			BaseJobDeclaration dec3 = GetJobDeclaration();
			dec3.DisableDefaultPackingInformation = true;
			Bill houseBill3 = dec3.Bills.AddNew();
			BasePackingGroup packingGroup3 = houseBill3.PackingGroups.AddNew();
			BasePackage package = packingGroup3.Packages.AddNew();

			AssertEquals("Packages for Declaration should not load package for the other declaration", 0, dec1.Packages.Count);
			AssertEquals("Packages for Declaration should not load package for the other declaration", 0, dec2.Packages.Count);
			AssertEquals("Packages for Declaration should not load package for the other declaration", 1, dec3.Packages.Count);
			AssertEquals("Packages for Declaration should not load package for the other declaration", true, dec3.Packages.Contains(package));
		}

		public void TestHasDangerousGoods()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.Packages.AddNew();
			BasePackage pack2 = declaration.Packages.AddNew();
			declaration.Packages.AddNew();
			Assert("No dangerous goods", !declaration.Packages.HasDangerousGoods);
			pack2.UNDGs.AddNew().DI_DG = Factory.NewWithValidTestData<MasterFiles.Business.UNDGSubstance>().PK;
			Assert("Has dangerous goods", declaration.Packages.HasDangerousGoods);
		}

		public void TestLowestPackages()
		{
			var package1 = Declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = Core.Constants.PkgUnit.Bag;

			var package2 = Declaration.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = Core.Constants.PkgUnit.Box;

			var package3 = Declaration.Packages.AddNew();
			package1.CW_PackQty = 3;
			package1.CW_PackType = Core.Constants.PkgUnit.Basket;

			AssertEquals("Lowest packages count should be 3", 3, Declaration.Packages.LowestPackages.Count());

			package3.CW_CW_Parent = package2.PK;
			AssertEquals("Lowest packages count should be 2", 2, Declaration.Packages.LowestPackages.Count());

			Declaration.Packages.AddNew();
			package1.CW_PackQty = 4;
			package1.CW_PackType = Core.Constants.PkgUnit.Roll;

			package3.CW_CW_Parent = ZGuid.Empty;
			AssertEquals("Lowest packages count should be 4", 4, Declaration.Packages.LowestPackages.Count());
		}

		public void TestSetDefaultFromPreviousPackage()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var package1 = declaration.Packages.AddNew();
			package1.CW_NetWeightUQ = "LB";
			package1.CW_GrossWeightUQ = "LB";
			package1.CW_VolumeUQ = "L";
			var package2 = declaration.Packages.AddNew();
			AssertEquals("LB", package2.CW_NetWeightUQ);
			AssertEquals("LB", package2.CW_GrossWeightUQ);
			AssertEquals("L", package2.CW_VolumeUQ);
		}

		#region Implementation
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BaseDeclarationLevelPackageCollection<BasePackage>(Declaration);
		}

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			return Factory.New<BaseJobDeclaration>();
		}

		protected BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = GetJobDeclaration();
					fDeclaration.DisableDefaultPackingInformation = true;
				}
				return fDeclaration;
			}
		}
		BaseJobDeclaration fDeclaration;
		#endregion
	}
}
