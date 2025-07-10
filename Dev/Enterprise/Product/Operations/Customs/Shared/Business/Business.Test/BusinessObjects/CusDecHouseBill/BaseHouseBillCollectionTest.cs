using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BillCollection<Bill, BaseJobDeclaration>))]
	public class BaseHouseBillCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFindAnyBillWithHouseBillMasterBillCombination()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "1";

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_BillNum = "1";
			bill2.CU_CU_ParentBill = bill1.PK;

			Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.HouseBill;
			bill3.CU_BillNum = "";
			bill3.CU_CU_ParentBill = bill1.PK;

			Bill bill4 = declaration.Bills.AddNew();
			bill4.CU_BillType = BillTypeList.Codes.HouseBill;
			bill4.CU_BillNum = "1";
			bill4.CU_CU_ParentBill = ZGuid.Empty;

			AssertEquals(bill1, declaration.Bills.FindAnyBillWithHouseBillMasterBillCombination("", "1"));
			AssertEquals(bill2, declaration.Bills.FindAnyBillWithHouseBillMasterBillCombination("1", "1"));
			AssertEquals(bill4, declaration.Bills.FindAnyBillWithHouseBillMasterBillCombination("1", ""));

			//it is looking for a house bill 
			AssertEquals(bill3, declaration.Bills.FindHouseBillsByHouseBillNumAndParentMasterBill("", "1"));
		}

		public void TestAllowNew()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			AssertEquals(true, declaration.Bills.AllowNew);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(false, declaration.Bills.AllowNew);

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals(true, declaration.Bills.AllowNew);
		}

		public void TestGetElementWithoutPackingGroups()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			Bill houseBill = declaration.Bills.AddNew();
			AssertEquals(houseBill, declaration.Bills.GetElementWithoutPackingGroups());

			houseBill.PackingGroups.AddNew();
			AssertNull(declaration.Bills.GetElementWithoutPackingGroups());
		}

		public void TestFindByUniqueCode()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			Bill bill1 = declaration.Bills.AddNew();
			AssertEquals("PreCondition:BillType", BillTypeList.Codes.MasterBill, bill1.CU_BillType);
			bill1.CU_BillNum = "1";

			Bill bill2 = declaration.Bills.AddNew();
			AssertEquals("PreCondition:BillType", BillTypeList.Codes.HouseBill, bill2.CU_BillType);
			bill2.CU_BillNum = "2";

			AssertEquals(bill1, declaration.Bills.FindByBillUniqueCode(bill1.CU_BillUniqueCode));
			AssertEquals(bill2, declaration.Bills.FindByBillUniqueCode(bill2.CU_BillUniqueCode));
			AssertEquals(null, declaration.Bills.FindByBillUniqueCode(bill2.CU_BillUniqueCode + ":3"));
		}

		public void TestGUIPresentationFlag()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_HouseBill = "111";
			AssertEquals("Bill object is created and GUI Presentation Flag is set", 1, declaration.Bills.Count);
			AssertEquals("Bill object is created and GUI Presentation Flag is set", true, declaration.Bills[0].CU_GUIPresentationRecord);
			AssertEquals("Bill type is house bill", BillTypeList.Codes.HouseBill, declaration.Bills[0].CU_BillType);

			declaration.JE_MasterBill = "08155555555";
			AssertEquals("Bill object is created and GUI Presentation Flag is set", 2, declaration.Bills.Count);
			AssertEquals("Bill object is created and GUI Presentation Flag is set", true, declaration.Bills[1].CU_GUIPresentationRecord);
			AssertEquals("Bill type is house bill", BillTypeList.Codes.MasterBill, declaration.Bills[1].CU_BillType);

			declaration.Bills.RemoveAndDeleteAll();
			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "M1";
			AssertEquals("this is the first master bill and GUI presentation flag is set", true, bill1.CU_GUIPresentationRecord);

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "M2";
			AssertEquals("this is not the first master bill", false, bill2.CU_GUIPresentationRecord);

			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_BillNum = "H2";
			houseBill2.CU_CU_ParentBill = bill2.PK;
			AssertEquals("this is not linked to primary master bill", false, houseBill2.CU_GUIPresentationRecord);

			Bill houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_BillNum = "H1";
			houseBill1.CU_CU_ParentBill = bill1.PK;
			AssertEquals("this is linked to primary master bill", true, houseBill1.CU_GUIPresentationRecord);

			declaration.JE_HouseBill = "H1";
			AssertEquals(bill1, declaration.PrimaryMasterBill);
			AssertEquals("BillNumber updated", "H1", houseBill1.CU_BillNum);

			bill1.ChildBills[0].Delete();
			AssertNull("There is no primary house bill linked to the master bill", declaration.PrimaryHouseBill);

			declaration.JE_MasterBill = "M2";
			AssertEquals("GUI presentation record should change", bill2, declaration.PrimaryMasterBill);
			AssertEquals("GUi presentation record should change", houseBill2, declaration.PrimaryHouseBill);
		}

		public virtual void TestCloneSingleHouseBill()
		{
			Bill houseBill = Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "123Test";
			if (houseBill.PackingGroups.Count == 0)
			{
				houseBill.PackingGroups.AddNew();
			}

			BaseJobDeclaration declarationCloned = Declaration.GetNewRelatedDeclaration(Factory);
			AssertEquals(houseBill.CU_HouseBill, declarationCloned.PrimaryHouseBill.CU_HouseBill);
			AssertEquals("House bill copied", 1, declarationCloned.Bills.Count);
			AssertEquals("Packing Group copied", 1, declarationCloned.Bills[0].PackingGroups.Count);
		}

		public virtual void TestCloneHasChanges()
		{
			Bill houseBill = Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "123Test";
			if (houseBill.PackingGroups.Count == 0)
			{
				houseBill.PackingGroups.AddNew();
			}
			Factory.Save();//to remove changes

			BaseJobDeclaration declarationCloned = Declaration.GetNewRelatedDeclaration(Factory);
			AssertEquals("House bill copied", 1, declarationCloned.Bills.Count);
			AssertEquals("Packing Group copied", 1, declarationCloned.Bills[0].PackingGroups.Count);
			AssertEquals("Has changes on house bill is false", false, declarationCloned.Bills.HasChanges);
			AssertEquals("Has changes on packing false", false, declarationCloned.Bills[0].PackingGroups.HasChanges);
		}

		#region TestDeleteFromCollectionTriggersUpdateOfHouseBillOnDeclaration
		public void TestDeleteFromCollectionTriggersUpdateOfHouseBillOnDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			Bill houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_HouseBill = "HOUSEBILL1";
			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill2.CU_HouseBill = "HOUSEBILL2";
			AssertEquals("Precondition: Declaration.JE_HouseBill", "HOUSEBILL1", declaration.JE_HouseBill);

			declaration.Bills.RemoveAndDelete(houseBill1);
			AssertEquals("Declaration.JE_HouseBill", "HOUSEBILL2", declaration.JE_HouseBill);

			declaration.Bills.RemoveAndDelete(houseBill2);
			AssertEquals("Declaration.JE_HouseBill", "", declaration.JE_HouseBill);
		}

		public void TestDeletingHouseBillDeletesPackages()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobDeclaration declarationLoaded = factory2.Load<BaseJobDeclaration>(declaration.PK);
			Bill billLoaded = declarationLoaded.Bills[0];
			AssertNotNull(declarationLoaded);
			AssertNoExceptionThrown(delegate
			{ declarationLoaded.Bills.RemoveAndDelete(billLoaded); });
			AssertNoExceptionThrown(delegate
			{ factory2.Save(); });

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			declarationLoaded = factory3.Load<BaseJobDeclaration>(declaration.PK);
			AssertEquals(0, declarationLoaded.PackingGroups.Count);
			AssertEquals(0, declarationLoaded.Packages.Count);
		}

		public void TestLowestBillsCountWhenBillRemoved()
		{
			BaseJobDeclaration declaration = GetDeclaration();

			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillNum = "Master Bill";
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillNum = "House Bill 1";
			houseBill.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill.CU_CU_ParentBill = masterBill.PK;

			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillNum = "House Bill 2";
			houseBill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			houseBill2.CU_CU_ParentBill = masterBill.PK;

			AssertEquals(2, declaration.LowestBills.Count);
			AssertEquals("HOUSE BILL 1", declaration.LowestBills[0].CU_BillNum.ToUpper());
			AssertEquals("HOUSE BILL 2", declaration.LowestBills[1].CU_BillNum.ToUpper());
			AssertEquals(3, declaration.Bills.Count);

			houseBill.Delete();
			AssertEquals(2, declaration.Bills.Count);
			AssertEquals("MASTER BILL", declaration.Bills[0].CU_BillNum.ToUpper());
			AssertEquals("HOUSE BILL 2", declaration.Bills[1].CU_BillNum.ToUpper());
			AssertEquals(1, declaration.LowestBills.Count);
			AssertEquals("HOUSE BILL 2", declaration.LowestBills[0].CU_BillNum.ToUpper());
		}

		#endregion

		#region Bills
		IBillCollection<Bill, BaseJobDeclaration> Bills
		{
			get
			{
				if (fBills == null)
				{
					fBills = (IBillCollection<Bill, BaseJobDeclaration>)GetCollectionToTest();
				}
				return fBills;
			}
		}
		IBillCollection<Bill, BaseJobDeclaration> fBills;
		#endregion

		#region Declaration
		BaseJobDeclaration Declaration
		{
			get { return (BaseJobDeclaration)Bills.Master; }
		}
		#endregion

		#region GetCollectionToTest
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			BaseJobDeclaration declaration = GetDeclaration();
			declaration.DisableDefaultPackingInformation = true;
			return (BillCollection<Bill, BaseJobDeclaration>)declaration.Bills;
		}

		protected virtual BaseJobDeclaration GetDeclaration()
		{
			return BaseJobDeclaration.New(Factory);
		}
		#endregion
	}
}
