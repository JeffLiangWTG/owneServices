using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BasePackage))]
	public class BasePackageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestManualPopulateClusterKey()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.JE_ClusterKey = 2;

			var container = declaration.CusContainers.AddNew();
			container.FillWithValidTestData();

			var bill = declaration.Bills.AddNew();
			bill.FillWithValidTestData();
			bill.CU_ClusterKey = 2;

			var packingGroup = declaration.PackingGroups.AddNew();
			AssertEquals("Should manually populate the value from the declaration.", 2, packingGroup.CR_ClusterKey);

			packingGroup.CR_CO_Container = container.PK;
			packingGroup.CR_CU_HouseBill = bill.PK;

			AssertEquals("Should manual populate the value from the parent bill.", 2, packingGroup.CR_ClusterKey);

			var package = packingGroup.Packages.AddNew();
			package.FillWithValidTestData();
			package.CW_PackQty = 10;

			AssertEquals("Should manual populate the value from the parent packing group.", 2, package.CW_ClusterKey);

			Factory.Save();

			var finallyKey = declaration.JE_ClusterKey;

			AssertEquals("Should populate the value from the parent declaration in OnSaving part.", finallyKey, bill.CU_ClusterKey);
			AssertEquals("Should populate the value from the parent bill in OnSaving part.", finallyKey, packingGroup.CR_ClusterKey);
			AssertEquals("Should populate the value from the parent packing group in OnSaving part.", finallyKey, package.CW_ClusterKey);
		}

		public void TestIsPivotCollectionLoaded()
		{
			var package = Factory.New<BasePackage>();
			Assert(!package.IsPivotCollectionLoaded(PivotLevel.Invoice));
			Assert(!package.IsPivotCollectionLoaded(PivotLevel.InvoiceLine));

			package.InvoiceHeaderPivotCollection.Load();
			Assert(package.IsPivotCollectionLoaded(PivotLevel.Invoice));
			Assert(!package.IsPivotCollectionLoaded(PivotLevel.InvoiceLine));

			package.InvoiceLinePivotCollection.Load();
			Assert(package.IsPivotCollectionLoaded(PivotLevel.Invoice));
			Assert(package.IsPivotCollectionLoaded(PivotLevel.InvoiceLine));
		}

		#region TestIPackLineInfo properties

		public void TestIPackLineInfoProperties()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BasePackage package = declaration.Packages.AddNew();
			package.CW_MarksAndNos = "marks";
			package.CW_PackQty = 10;
			package.CW_PackType = " KG";
			AssertEquals("CW_PackType, trim leading space", "KG", package.CW_PackType);

			AssertEquals("Marks and Numbers", "marks", ((IPackLineInfo)package).MarksAndNumbers);
			AssertEquals("PackType", "KG", ((IPackLineInfo)package).PackType);
			AssertEquals("NumberOfPackages", 10, ((IPackLineInfo)package).NumberOfPackages);
		}

		public void TestContainerNumber()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BasePackage package = declaration.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = "Container1";
			AssertEquals("Container1", ((IPackLineInfo)package).ContainerNumber);
		}

		public void TestDimensions()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BasePackage package = declaration.Packages.AddNew();

			IPackLineInfo packLineInfo = package;
			AssertEquals("Height", ZDecimal.Zero, packLineInfo.Height);
			AssertEquals("Width", ZDecimal.Zero, packLineInfo.Width);
			AssertEquals("Length", ZDecimal.Zero, packLineInfo.Length);
			AssertEquals("Unit of Dimension", ZString.Empty, packLineInfo.UnitOfDimension);

			AssertEquals("Volume", ZDecimal.Zero, packLineInfo.Volume.Amount);
			AssertEquals("Volume UQ", ZString.Empty, packLineInfo.Volume.Unit);
			AssertEquals("Weight", ZDecimal.Zero, packLineInfo.Weight.Amount);
			AssertEquals("Weight UQ", ZString.Empty, packLineInfo.Weight.Unit);
		}

		#endregion

		public virtual void TestIOneToOnePackingInformation()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			Bill bill1 = declaration.Bills.AddNew();
			Bill bill2 = declaration.Bills.AddNew();

			BaseCusContainer container1 = declaration.CusContainers.AddNew();
			BaseCusContainer container2 = declaration.CusContainers.AddNew();

			BasePackage package = declaration.Packages.AddNew();
			AssertNull(((IPackingInformation)package).HouseBillContainer.Container);
			AssertNull(((IPackingInformation)package).HouseBillContainer.HouseBill);

			((IPackingInformation)package).HouseBillContainer = new HouseBillContainer(bill1, null);
			AssertNull(((IPackingInformation)package).HouseBillContainer.Container);
			AssertEquals(bill1, ((IPackingInformation)package).HouseBillContainer.HouseBill);
			AssertEquals(bill1, package.PackingGroup.Bill);

			((IPackingInformation)package).HouseBillContainer = new HouseBillContainer(bill2, container1);
			AssertEquals(container1, ((IPackingInformation)package).HouseBillContainer.Container);
			AssertEquals(bill2, ((IPackingInformation)package).HouseBillContainer.HouseBill);
			AssertEquals(bill2, package.PackingGroup.Bill);
			AssertEquals(container1, package.PackingGroup.Container);

			((IPackingInformation)package).MarksAndNumbers = "MarksAndNumbers";
			AssertEquals("MarksAndNumbers", package.CW_MarksAndNos);

			((IOneToOnePackingInformation)package).PackQty = 13;
			((IOneToOnePackingInformation)package).PackType = "QQ";
			ZGuid dGGuid = ZGuid.NewZGuid();
			AssertEquals(13, package.CW_PackQty);
			AssertEquals("QQ", package.CW_PackType);
		}

		public void TestDuplicatePackingGroup()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var mbill = declaration.Bills.AddNew();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;

			var accessed = declaration.PackingGroups;
			var accessed2 = declaration.Packages;

			var packingGroup = Factory.New<BasePackingGroup>();
			packingGroup.CR_CU_HouseBill = bill.PK;

			bill.CU_BillNum = "ABCD";
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestWhenBillNumberClears()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var mbill = declaration.Bills.AddNew();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			bill.CU_BillNum = "ABCD";
			AssertNoExceptionThrown(() => bill.CU_BillNum = ZString.Empty);
		}

		public void TestICanDelete()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BasePackage package = declaration.Packages.AddNew();
			AssertEquals(true, ((ICanDelete)package).CanDelete);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(false, ((ICanDelete)package).CanDelete);
			AssertEquals(Bill.ReasonForCannotDeleteWhenSynchronised, ((ICanDelete)package).ReasonForNotAbleToDelete);

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals(true, ((ICanDelete)package).CanDelete);
		}

		public void TestDeclaration()
		{
			BasePackage package = declaration.Packages.AddNew();
			AssertEquals("Declaration is set by the collection", declaration, package.Declaration);

			Bill housebill = declaration.Bills.AddNew();
			BasePackingGroup packingGroup = housebill.PackingGroups.AddNew();
			package.Declaration = null;
			package.CW_CR_HouseContainer = packingGroup.PK;
			AssertEquals("Declaration can be referenced from packingGroup", declaration, package.Declaration);
		}

		public void TestCW_HouseBillSettingFullLengthHouseBillAndMasterBill()
		{
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_HouseBill = new ZString().PadRight(declaration.JE_HouseBillInfo.MaxLength, 'A');
			declaration.JE_MasterBill = new ZString().PadRight(declaration.JE_MasterBillInfo.MaxLength, 'B');
			Bill houseBill = declaration.Bills[0];
			AssertEquals("Precondition: HouseBill length on CusDecHouseBill should be at least as long as HouseBill on Declaration", new ZString().PadRight(houseBill.CU_HouseBillInfo.MaxLength, 'A'), houseBill.CU_HouseBill);

			Bill masterBill = declaration.Bills[1];
			AssertEquals("Precondition: MasterBill length on CusDecHouseBill should be at least as long as MasterBill on Declaration", new ZString().PadRight(houseBill.CU_MasterBillInfo.MaxLength, 'B'), masterBill.CU_MasterBill);

			ZString houseAndMasterBill = houseBill.CU_BillUniqueCode;

			BasePackage package = declaration.Packages.AddNew();
			package.CW_HouseBill = houseAndMasterBill;
			AssertEquals("Package should now be linked to the right HouseBill", houseBill.PK, package.Bill.PK);
			AssertEquals("Package.CW_HouseBillInfo.MaxLength", -1, package.CW_HouseBillInfo.MaxLength);
		}

		public void TestSettingEmptyContainerThenRealContainerCanSaveWhenSameValueAlreadyInDB()
		{
			BasePackage package = declaration.Packages.AddNew();
			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			package.CW_PackQty = 1;
			Factory.Save();
			Assert("Factory.Save() should not have failed here", true);

			package.CW_ContainerNoOrEquipmentNo = "";
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			Factory.Save();
			Assert("Factory.Save() should not have failed here", true);
		}

		public void TestPackingGroupsWithPackagesWontGetRecycled()
		{
			BasePackage package = declaration.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			AssertEquals("One pivot created", 1, declaration.PackingGroups.Count);

			BasePackingGroup existingPackingGroup = declaration.PackingGroups[0];

			BasePackage package2 = declaration.Packages.AddNew();
			package2.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			AssertEquals("still attached to the existing pivot", existingPackingGroup, package2.PackingGroup);

			package2.CW_HouseBill = houseBill.CU_BillUniqueCode;
			AssertEquals("Another pivot is created", 2, declaration.PackingGroups.Count);
			AssertNotEquals("existingPackingGroup != newPivot", existingPackingGroup, package2.PackingGroup);
			AssertEquals(1, existingPackingGroup.Packages.Count);
			AssertEquals(true, existingPackingGroup.Packages.Contains(package));
			AssertEquals(1, package2.PackingGroup.Packages.Count);
			AssertEquals(true, package2.PackingGroup.Packages.Contains(package2));

			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
			AssertEquals("Sharing same packing group", package.PackingGroup, package2.PackingGroup);
			AssertEquals("Killed excess packing group", 1, declaration.PackingGroups.Count);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestSettingHouseBillWithoutContainer()
		{
			BasePackage package = declaration.Packages.AddNew();
			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
			AssertEquals("HouseBill.PackingGroups.Count", 1, houseBill.PackingGroups.Count);
			BasePackingGroup newPackingGroup = houseBill.PackingGroups[0];
			AssertEquals("NewPackingGroup.CR_CU_HouseBill", houseBill.PK, newPackingGroup.CR_CU_HouseBill);
			AssertEquals("NewPackingGroup.CR_CO_Container", ZGuid.Empty, newPackingGroup.CR_CO_Container);
			AssertEquals("NewPackingGroup.Packages.Count", 1, newPackingGroup.Packages.Count);
			AssertEquals("NewPackingGroup.Packages[0]", package, newPackingGroup.Packages[0]);
			AssertEquals("Package.CW_CR_HouseContainer", newPackingGroup.PK, package.CW_CR_HouseContainer);

			package.CW_HouseBill = "";
			AssertEquals("HouseBill.PackingGroups.Count", 0, houseBill.PackingGroups.Count);
			AssertEquals("NewPackingGroup.IsDeleted", false, newPackingGroup.IsDeleted);
			AssertEquals("NewPackingGroup.Packages.Count", 1, newPackingGroup.Packages.Count);
			AssertEquals("Package.CW_CR_HouseContainer", newPackingGroup.PK, package.CW_CR_HouseContainer);
		}

		public void TestSettingHouseBillAndContainer()
		{
			BasePackage package = declaration.Packages.AddNew();
			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			AssertEquals("HouseBill.PackingGroups.Count", 1, houseBill.PackingGroups.Count);
			BasePackingGroup newPackingGroup = package.PackingGroup;
			AssertEquals("NewPackingGroup.CR_CU_HouseBill", houseBill.PK, newPackingGroup.CR_CU_HouseBill);
			AssertEquals("NewPackingGroup.CR_CO_Container", container.PK, newPackingGroup.CR_CO_Container);
			AssertEquals("NewPackingGroup.Packages.Count", 1, newPackingGroup.Packages.Count);
			AssertEquals("NewPackingGroup.Packages[0]", package, newPackingGroup.Packages[0]);
			AssertEquals("Package.CW_CR_HouseContainer", newPackingGroup.PK, package.CW_CR_HouseContainer);

			package.CW_ContainerNoOrEquipmentNo = "";
			AssertEquals("HouseBill.PackingGroups.Count", 1, houseBill.PackingGroups.Count);
			newPackingGroup = package.PackingGroup;
			AssertEquals("NewPackingGroup.IsDeleted", false, newPackingGroup.IsDeleted);
			AssertEquals("NewPackingGroup.Packages.Count", 1, newPackingGroup.Packages.Count);
			AssertEquals("Package.CW_CR_HouseContainer", newPackingGroup.PK, package.CW_CR_HouseContainer);
		}

		public void TestSettingContainerThenHouseBill()
		{
			BasePackage package = declaration.Packages.AddNew();
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			AssertEquals("HouseBill.PackingGroups.Count", 0, houseBill.PackingGroups.Count);
			AssertEquals("Package.CW_HouseBill", "", package.CW_HouseBill);
			AssertEquals("Package.CW_ContainerNoOrEquipmentNo", container.CO_ContainerNumber, package.CW_ContainerNoOrEquipmentNo);

			package.CW_HouseBill = houseBill.CU_BillUniqueCode;
			AssertEquals("HouseBill.PackingGroups.Count", 1, houseBill.PackingGroups.Count);
			BasePackingGroup newPackingGroup = houseBill.PackingGroups[0];
			AssertEquals("NewPackingGroup.CR_CU_HouseBill", houseBill.PK, newPackingGroup.CR_CU_HouseBill);
			AssertEquals("NewPackingGroup.CR_CO_Container", container.PK, newPackingGroup.CR_CO_Container);
			AssertEquals("NewPackingGroup.Packages.Count", 1, newPackingGroup.Packages.Count);
			AssertEquals("NewPackingGroup.Packages[0]", package, newPackingGroup.Packages[0]);
			AssertEquals("Package.CW_CR_HouseContainer", newPackingGroup.PK, package.CW_CR_HouseContainer);

			package.CW_HouseBill = "";
			AssertEquals("HouseBill.PackingGroups.Count", 1, declaration.PackingGroups.Count);
			AssertEquals("Package.CW_HouseBill", "", package.CW_HouseBill);
			AssertEquals("Package.CW_ContainerNoOrEquipmentNo", container.CO_ContainerNumber, package.CW_ContainerNoOrEquipmentNo);
		}

		public void TestImportPackageDetailsThroughWizardWhileSuspendingListChangedEvent()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_HouseBill = "H1";

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CRUX2345";

			var container2 = declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CRUX1234";

			declaration.PackingGroups.RemoveAndDeleteAll();

			using (declaration.Packages.SuspendListChanged())//during import wizard
			{
				var package = declaration.Packages.AddNew();
				package.CW_HouseBill = "H1";
				package.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;

				package = declaration.Packages.AddNew();
				package.CW_HouseBill = "H1";
				package.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;

				AssertEquals(1, container1.PackingGroups.Count);
				AssertEquals(1, container2.PackingGroups.Count);
			}
		}

		[ExpectExceptionMessage(typeof(DeveloperNotificationException), "You can only set CW_HouseBill or CW_ContainerNoOrEquipmentNo when accessing a Package via the Declaration.Packages Collection.")]
		public void TestSettingCW_HouseBillGetsExceptionWithNoDeclaration()
		{
			BasePackage package = Factory.New<BasePackage>();
			package.CW_HouseBill = "123";
		}

		[ExpectExceptionMessage(typeof(DeveloperNotificationException), "You can only set CW_HouseBill or CW_ContainerNoOrEquipmentNo when accessing a Package via the Declaration.Packages Collection.")]
		public void TestSettingCW_ContainerNoOrEquipmentNoGetsExceptionWithNoDeclaration()
		{
			BasePackage package = Factory.New<BasePackage>();
			package.CW_ContainerNoOrEquipmentNo = "123";
		}

		public void TestHouseBill()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			Bill houseBill = declaration.Bills.AddNew();
			BasePackingGroup packingGroup = declaration.PackingGroups.AddNew();
			packingGroup.CR_CU_HouseBill = houseBill.PK;
			BasePackage package = declaration.Packages.AddNew();
			package.CW_CR_HouseContainer = packingGroup.PK;
			AssertEquals(houseBill, package.Bill);
		}

		public void TestPackingGroupRelatedBusinessObject()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill houseBill = declaration.Bills.AddNew();
			BasePackingGroup packingGroup = Factory.New<BasePackingGroup>();
			packingGroup.CR_CU_HouseBill = houseBill.PK;
			BasePackage package = Factory.New<BasePackage>();
			package.CW_CR_HouseContainer = packingGroup.PK;
			AssertEquals("PackingGroup should match", packingGroup, package.PackingGroup);
		}

		public void TestIsLowestPackage()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "ABC123";

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = Core.Constants.PkgUnit.Bag;
			AssertEquals(true, package1.IsLowestPackage);

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = Core.Constants.PkgUnit.Box;
			package2.CW_CW_Parent = package1.PK;
			AssertEquals(false, package1.IsLowestPackage);
			AssertEquals(true, package2.IsLowestPackage);
		}

		public void TestCW_CW_Parent()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "ABC123";

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = Core.Constants.PkgUnit.Bag;

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = Core.Constants.PkgUnit.Box;
			package2.CW_CW_Parent = package1.PK;
			AssertEquals(ZGuid.Empty, package1.CW_CW_Parent);
			AssertEquals(package1.PK, package2.CW_CW_Parent);

			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 10;
			package3.CW_PackType = Core.Constants.PkgUnit.Basket;
			package2.CW_CW_Parent = package3.PK;
			AssertEquals(package3.PK, package2.CW_CW_Parent);
		}

		public void TestAncestors()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_MasterBill = "ABC123";

			var package1 = declaration.Packages.AddNew();
			package1.CW_PackQty = 1;
			package1.CW_PackType = Core.Constants.PkgUnit.Bag;

			var package2 = declaration.Packages.AddNew();
			package2.CW_PackQty = 2;
			package2.CW_PackType = Core.Constants.PkgUnit.Box;
			package2.CW_CW_Parent = package1.PK;
			AssertEquals(1, package2.Ancestors.Count());
			AssertEquals(package1.PK, package2.Ancestors.First().PK);

			var package3 = declaration.Packages.AddNew();
			package3.CW_PackQty = 10;
			package3.CW_PackType = Core.Constants.PkgUnit.Basket;
			package3.CW_CW_Parent = package2.PK;
			AssertEquals(2, package3.Ancestors.Count());
			Assert(package3.Ancestors.Any(p => p.PK == package1.PK));
			Assert(package3.Ancestors.Any(p => p.PK == package2.PK));
		}

		public void TestChildrenDetailsDescendFromParent()
		{
			var package1 = SetupParentPackage();
			var package2 = SetupChildPackage(package1.PK);

			AssertEquals("HouseBill is pushed to child", package2.CW_HouseBill, package1.CW_HouseBill);
			Assert("HouseBill is ReadOnly when linked to parent", package2.CW_HouseBill_ReadOnly);
			AssertEquals("Container is pushed to child", package2.CW_ContainerNoOrEquipmentNo, package1.CW_ContainerNoOrEquipmentNo);
			Assert("Container No is ReadOnly when linked to parent", package2.CW_ContainerNoOrEquipmentNo_ReadOnly);
		}

		public void TestChildrenDetailsWritableWhenParentDeleted()
		{
			var package1 = SetupParentPackage();
			var package2 = SetupChildPackage(package1.PK);
			package1.Delete();

			AssertEquals("HouseBill is from deleted Parent", package2.CW_HouseBill, houseBill.CU_BillNum);
			Assert("HouseBill is not ReadOnly as parent deleted", !package2.CW_HouseBill_ReadOnly);
			AssertEquals("Container is from deleted child", package2.CW_ContainerNoOrEquipmentNo, container.CO_ContainerNumber);
			Assert("Container No is not ReadOnly as parent deleted", !package2.CW_ContainerNoOrEquipmentNo_ReadOnly);
		}
		public void TestChildrenDetailsUpdateFromParent()
		{
			var package1 = SetupParentPackage();
			var package2 = SetupChildPackage(package1.PK);
			AssertEquals("HouseBill is pushed to child", package2.CW_HouseBill, package1.CW_HouseBill);
			AssertEquals("Container is pushed to child", package2.CW_ContainerNoOrEquipmentNo, package1.CW_ContainerNoOrEquipmentNo);

			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "OOCL0000007";
			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			AssertEquals("Container is updated on child", package2.CW_ContainerNoOrEquipmentNo, container1.CO_ContainerNumber);

			var houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_HouseBill = "UPDATEDHB";
			package1.CW_HouseBill = houseBill1.CU_HouseBill;
			AssertEquals("HouseBill is pushed to child", package2.CW_HouseBill, houseBill1.CU_HouseBill);
		}

		public void TestCW_Seal_Caption()
		{
			AssertEquals("Seal Number", DataBoundResourceStrings.GetDataForProperty(Factory.New<BasePackage>().CW_SealInfo).Caption);
		}

		BasePackage SetupParentPackage()
		{
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 10;
			package.CW_PackType = Core.Constants.PkgUnit.Bag;
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			package.CW_HouseBill = houseBill.CU_BillNum;
			return package;
		}

		BasePackage SetupChildPackage(ZGuid parentPK)
		{
			var package = declaration.Packages.AddNew();
			package.CW_PackQty = 20;
			package.CW_PackType = Core.Constants.PkgUnit.Box;
			package.CW_CW_Parent = parentPK;
			return package;
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			packingGroup = houseBill.PackingGroups.AddNew();
			BasePackage package = packingGroup.Packages.AddNew();
			package = declaration.Packages[0]; // This will fill in the Declaration object on the Package.
			return package;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var packingGroup = this.packingGroup ?? houseBill.PackingGroups.AddNew();
			BasePackage package = packingGroup.Packages.AddNew();
			package.CW_PackQty = 1;
			package.CW_PackType = "XX";
			return package;
		}

		protected BaseJobDeclaration declaration;
		Bill houseBill;
		BaseCusContainer container;
		BasePackingGroup packingGroup;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = GetJobDeclaration();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.DisableDefaultPackingInformation = true;
			houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "DOESEXIST";
			container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000006";
		}

		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}

		#endregion
	}
}
