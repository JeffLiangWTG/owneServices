using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(Bill))]
	class HouseBillTest : Customs.Business.Testing.BaseHouseBillTest<Bill, JobDeclaration>
	{
		public void TestNewBillCreatesPackGroupToContainer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cont1 = declaration.CusContainers.AddNew();
			cont1.CO_ContainerNumber = "CN001";
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			bill1.CU_HouseBill = "HB1";

			var packGrp1 = bill1.PackingGroups[0];
			AssertEquals(cont1.PK, packGrp1.CR_CO_Container);
			AssertEquals("CN001", packGrp1.Packages[0].CW_ContainerNoOrEquipmentNo);
		}

		public void TestWI00242609_NotCreateUnnecessaryPackingGroup()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = "ECI";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_MasterBill = "MB1";
			AssertEquals(1, declaration.Bills.Count);
			AssertEquals(1, declaration.PackingGroups.Count);
			declaration.JE_HouseBill = "HB1";
			AssertEquals(2, declaration.Bills.Count);
			AssertEquals(1, declaration.PackingGroups.Count);
			AssertEquals(1, declaration.PackingGroups[0].Packages.Count);

			var cont1 = declaration.CusContainers.AddNew();
			cont1.CO_ContainerNumber = "CN001";
			Factory.Save();

			// Master is ignored
			var mBill = declaration.Bills.Cast<Bill>().First(b => b.CU_BillNum == "MB1");
			AssertEquals(0, mBill.PackingGroups.Count);
			AssertEquals(0, mBill.LoosePackageCount); // called from validation. Should not alter count.
			AssertEquals(0, mBill.PackingGroups.Count);

			// added container to House and created package
			var hBill = declaration.Bills.Cast<Bill>().First(b => b.CU_BillNum == "HB1");
			AssertEquals(1, hBill.PackingGroups.Count);
			var hBillPackGrp = hBill.PackingGroups[0];
			AssertEquals(cont1.PK, hBillPackGrp.CR_CO_Container);
			AssertEquals(1, hBillPackGrp.Packages.Count);
			AssertEquals(0, hBill.LoosePackageCount); // called from validation. Should not alter count.
			AssertEquals(1, hBill.PackingGroups.Count);

			// add additional bill
			var hBill2 = declaration.Bills.AddNew();
			hBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			hBill2.CU_HouseBill = "HB2";

			// Master unaffected
			AssertEquals(0, mBill.PackingGroups.Count);

			// House unaffected
			AssertEquals(1, hBill.PackingGroups.Count);
			AssertEquals(cont1.PK, hBillPackGrp.CR_CO_Container);
			AssertEquals(hBill.PK, hBillPackGrp.CR_CU_HouseBill);

			// House2 linked to container
			AssertEquals(1, hBill2.PackingGroups.Count);
			var h2PackGrp = hBill2.PackingGroups[0];
			AssertEquals(cont1.PK, h2PackGrp.CR_CO_Container);
			AssertEquals(hBill2.PK, h2PackGrp.CR_CU_HouseBill);

			// Delete Second House Bill.
			hBill2.Delete();
			Factory.Save();

			// Master and House are unaffected
			AssertEquals(0, mBill.PackingGroups.Count);
			AssertEquals(1, hBill.PackingGroups.Count);
			AssertEquals(hBillPackGrp, hBill.PackingGroups[0]);
			AssertEquals(cont1.PK, hBillPackGrp.CR_CO_Container);
			AssertEquals(1, hBillPackGrp.Packages.Count);
			AssertEquals(1, declaration.PackingGroups.Count);
		}

		public void TestBillNumToUpperForMessagingPurpose()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "a";
			AssertEquals("A", bill.CU_BillNum);
		}

		public void TestSetParentBillForHouseBills()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MasterBill = "MB1";
			declaration.JE_HouseBill = "HB1";
			AssertNotNull("PrimaryMasterBill", declaration.PrimaryMasterBill);
			AssertNotNull("PrimaryHouseBill", declaration.PrimaryHouseBill);

			Bill bill3 = declaration.Bills.AddNew();
			AssertEquals("PreCondition:BillType is set", BillTypeList.Codes.HouseBill, bill3.CU_BillType);
			AssertEquals("For house bill, the parent should point to PrimaryMasterBill", declaration.PrimaryMasterBill, bill3.ParentBill);
		}

		public void TestCodePropertyAndDescriptionProperty()
		{
			Bill masterBill = Declaration.Bills.AddNew();
			masterBill.CU_BillNum = "2";

			Bill bill1 = Declaration.Bills.AddNew();
			bill1.CU_HouseBill = "1";
			bill1.CU_MasterBill = "2";

			Bill bill2 = Declaration.Bills.AddNew();
			bill2.CU_HouseBill = "2";

			AssertEquals("HB:2 (MB:2)", CodePropertyAttribute.CodeFromBusinessObject(bill2));
			AssertEquals("HB:1 (MB:2)", CodePropertyAttribute.CodeFromBusinessObject(bill1));

			AssertEquals("HB:2 (MB:2)", DescriptionPropertyAttribute.DescriptionFromBusinessObject(bill2));
			AssertEquals("HB:1 (MB:2)", DescriptionPropertyAttribute.DescriptionFromBusinessObject(bill1));
		}

		public void TestLoosePackageCount()
		{
			Declaration.JE_HouseBill = "KIRSTENROCKS";
			Bill houseBill1 = Declaration.Bills[0];
			Package loosePackage1 = Declaration.Packages.AddNew();
			loosePackage1.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			loosePackage1.CW_PackQty = 1;
			loosePackage1.CW_PackType = "PK";
			AssertEquals("houseBill.LoosePackageCount", 1, houseBill1.LoosePackageCount);
			Package loosePackage2 = Declaration.Packages.AddNew();
			loosePackage2.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			loosePackage2.CW_PackQty = 4;
			loosePackage2.CW_PackType = "PK";
			AssertEquals("houseBill.LoosePackageCount", 5, houseBill1.LoosePackageCount);

			CusContainer container = Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000027";
			Package containerisedPackage = Declaration.Packages.AddNew();
			containerisedPackage.CW_HouseBill = houseBill1.CU_BillUniqueCode;
			containerisedPackage.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;
			containerisedPackage.CW_PackQty = 7;
			containerisedPackage.CW_PackType = "PK";
			AssertEquals("houseBill.LoosePackageCount", 5, houseBill1.LoosePackageCount);

			Bill houseBill2 = Declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = "SODOESDONG";
			Package loosePackageOnOtherHouseBill = Declaration.Packages.AddNew();
			loosePackageOnOtherHouseBill.CW_HouseBill = houseBill2.CU_BillUniqueCode;
			loosePackageOnOtherHouseBill.CW_PackQty = 55;
			loosePackageOnOtherHouseBill.CW_PackType = "PK";
			AssertEquals("houseBill.LoosePackageCount", 5, houseBill1.LoosePackageCount);
		}

		public void TestECI_ApportionedValues()
		{
			DecCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			DecCreator.SetupTestForSea();
			DecCreator.SetupTestForExportToAU();
			DecCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			DecCreator.SetupTestContainer("OOCL0000006", ContainerModeList.Codes.FCL, ContainerSizeList.Codes.ContainerIc20Ft, 11m, 10, "PK");
			Declaration.JE_TotalWeight = 21m;
			Declaration.JE_ECI_InvoiceAmount = 70.00m;
			Declaration.JE_HouseBill = "SCOTTYNEEDSSOMELOVIN";
			Package package = Declaration.Packages.AddNew();
			package.CW_HouseBill = Customs.Business.Bill.BillAndParentCombination.BillAndParentBill(Declaration.JE_HouseBill, BillTypeList.Codes.HouseBill, Declaration.JE_MasterBill, BillTypeList.Codes.MasterBill);
			package.CW_PackQty = 2;
			package.CW_PackType = "PK";

			Bill houseBill = Declaration.Bills[0];
			AssertEquals("houseBill.ECI_ApportionedLoosePackageWeight", 10m, houseBill.ECI_ApportionedLoosePackageWeight);
			AssertEquals("houseBill.ECI_ApportionedLoosePackageValue", 33.33m, houseBill.ECI_ApportionedLoosePackageValue);
		}

		public void TestValidationIsRightType()
		{
			AssertEquals("Bill.Validation.GetType()", typeof(BillValidation), Bill.Validation.GetType());
		}

		public override void TestLinkMasterBillToExistingHouseBills()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_HouseBill = "HB1";
			var houseBill1 = declaration.PrimaryHouseBill;
			houseBill1.CU_NoOfPacks = 10;
			AssertEquals("No Parent Bill", ZGuid.Empty, houseBill1.CU_CU_ParentBill);

			var houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_NoOfPacks = 10;
			AssertEquals("No Parent Bill", ZGuid.Empty, houseBill2.CU_CU_ParentBill);

			declaration.JE_MasterBill = "MasterBill1";
			var primaryMasterBill = declaration.PrimaryMasterBill;
			AssertNotNull(primaryMasterBill);
			AssertEquals("Existing House Bill 1 should have Parent Master Bill 1", primaryMasterBill.PK, houseBill1.CU_CU_ParentBill);
			AssertEquals("Existing House Bill 2 should have Parent Master Bill 1", primaryMasterBill.PK, houseBill2.CU_CU_ParentBill);
		}

		#region TestTypedPackingGroupCollection
		public void TestTypedPackingGroupCollection()
		{
			AssertEquals("Bill.PackingGroups.GetType()", typeof(PackingGroupCollection), Bill.PackingGroups.GetType());
		}
		#endregion

		public void TestDeleteBillIfNumberIsEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_HouseBill = "HB1234";
			AssertEquals(1, declaration.Bills.Count);

			declaration.JE_HouseBill = ZString.Empty;
			AssertEquals(0, declaration.Bills.Count);
		}

		public override void TestWillBeDeletedDuringSave()
		{
			Assert("If house bill is blank, HB record should not be created or should be deleted.", condition: true);
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return Bill;
		}

		#region Bill
		protected new Bill Bill
		{
			get { return (Bill)base.Bill; }
		}

		protected override Customs.Business.Bill GetNewHouseBill()
		{
			return Declaration.Bills.AddNew();
		}
		#endregion

		#region Declaration
		protected override Customs.Business.BaseJobDeclaration GetJobDeclaration()
		{
			Customs.Business.BaseJobDeclaration result = JobDeclaration.New(Factory);
			result.DisableDefaultPackingInformation = true;
			return result;
		}

		protected new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}
		#endregion

		#region DecCreator
		TestECIWriteOffCreator DecCreator
		{
			get
			{
				if (fDecCreator == null)
				{
					fDecCreator = new TestECIWriteOffCreator(Declaration);
				}
				return fDecCreator;
			}
		}
		TestECIWriteOffCreator fDecCreator;
		#endregion
		#endregion
	}
}
