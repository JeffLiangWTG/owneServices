using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BillGUIPresentationFlagUpdaterTest : TestCaseWithFactory
	{
		public void TestSynchronisingWhenParentBillChanges()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var masterbill = declaration.Bills.AddNew();
			masterbill.CU_BillType = "MB";
			masterbill.CU_BillNum = "M1";

			var housebill = declaration.Bills.AddNew();
			housebill.CU_BillType = "HB";
			housebill.CU_BillNum = "H1";

			var subhouseBill = (Bill)((IBindingList)declaration.Bills).AddNew();
			subhouseBill.CU_BillType = BillTypeList.Codes.SubHouseBill;
			subhouseBill.CU_CU_ParentBill = housebill.PK;

			AssertEquals("H1", declaration.JE_HouseBill);
		}

		public void TestCorrectPrimaryIsSynch()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.Bills.RemoveAndDeleteAll();

			var masterbill1 = declaration.Bills.AddNew();
			masterbill1.CU_BillType = BillTypeList.Codes.MasterBill;
			masterbill1.CU_BillNum = "M1";
			masterbill1.CU_GUIPresentationRecord = false;

			var housebill1 = declaration.Bills.AddNew();
			housebill1.CU_BillType = BillTypeList.Codes.HouseBill;
			housebill1.CU_BillNum = "H1";
			housebill1.CU_CU_ParentBill = masterbill1.PK;
			housebill1.CU_GUIPresentationRecord = false;

			var masterbill2 = declaration.Bills.AddNew();
			masterbill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterbill2.CU_BillNum = "M2";
			masterbill2.CU_GUIPresentationRecord = false;

			var housebill2 = declaration.Bills.AddNew();
			housebill2.CU_BillType = BillTypeList.Codes.HouseBill;
			housebill2.CU_BillNum = "H1";
			housebill2.CU_CU_ParentBill = masterbill2.PK;
			housebill2.CU_GUIPresentationRecord = false;

			using (declaration.BillGUIPresentationFlagUpdater.SuspendSynch())
			{
				declaration.JE_MasterBill = "M2";
			}
			declaration.JE_HouseBill = "H1";
			var primaryHouseBill = declaration.PrimaryHouseBill;
			AssertSame("Primary House Bill should match master bill details", housebill2, primaryHouseBill);
		}

		public void TestUpdateFromJE_MasterBill()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "1";
			AssertEquals("One bill", 1, declaration.Bills.Count);

			declaration.JE_MasterBill = "";
			AssertEquals("One bill", 1, declaration.Bills.Count);
			AssertEquals("Bill number", "", declaration.Bills[0].CU_BillNum);

			declaration.JE_MasterBill = "1";
			AssertEquals("One bill", 1, declaration.Bills.Count);
			AssertEquals("Bill number", "1", declaration.Bills[0].CU_BillNum);

			Bill bill1 = declaration.Bills[0];

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "2";
			AssertEquals("Not a GUI presentation record", false, bill2.CU_GUIPresentationRecord);

			declaration.JE_MasterBill = "2";
			AssertEquals("There are two bills", 2, declaration.Bills.Count);
			AssertEquals("bill2 is a GUI presentation record", true, bill2.CU_GUIPresentationRecord);
			AssertEquals("bill1 becomes a non-gui", false, bill1.CU_GUIPresentationRecord);
		}

		public void TestSuspendSynchronisation()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			//this is used in BillSynchroniser while bills are synched from shipment. It manages JE_MasterBill/JE_HouseBill and Bills
			using (declaration.BillGUIPresentationFlagUpdater.SuspendSynch())
			{
				declaration.JE_MasterBill = "1";
				AssertEquals("No bill should have been created as it is suspended", 0, declaration.Bills.Count);
			}

			declaration.JE_MasterBill = "2";
			AssertEquals("One bill", 1, declaration.Bills.Count);
		}

		public void TestUpdateJE_HousebillWhenJE_MasterBillChanged()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "1";

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_BillNum = "2";
			bill2.CU_CU_ParentBill = bill1.PK;

			Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.MasterBill;
			bill3.CU_BillNum = "3";

			Bill bill4 = declaration.Bills.AddNew();
			bill4.CU_BillType = BillTypeList.Codes.HouseBill;
			bill4.CU_BillNum = "4";
			bill4.CU_CU_ParentBill = bill3.PK;

			AssertEquals("JE_MasterBill", "1", declaration.JE_MasterBill);
			AssertEquals("JE_HouseBill", "2", declaration.JE_HouseBill);

			declaration.JE_MasterBill = "3";
			AssertEquals("JE_HouseBill", "4", declaration.JE_HouseBill);
			AssertEquals("bill1.CU_GUIPresentationRecord", false, bill1.CU_GUIPresentationRecord);
			AssertEquals("bill2.CU_GUIPresentationRecord", false, bill2.CU_GUIPresentationRecord);
			AssertEquals("bill3.CU_GUIPresentationRecord", true, bill3.CU_GUIPresentationRecord);
			AssertEquals("bill4.CU_GUIPresentationRecord", true, bill4.CU_GUIPresentationRecord);
		}

		public void TestLinkPrimaryHouseBillAndMasterBillFromDeclaration()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "1";
			AssertEquals("One bill", 1, declaration.Bills.Count);

			declaration.JE_HouseBill = "2";
			AssertEquals("Two bills", 2, declaration.Bills.Count);

			AssertEquals("PrimaryMaster bill and primary house bill are linked", declaration.PrimaryMasterBill, declaration.PrimaryHouseBill.ParentBill);

			declaration.Bills.RemoveAndDeleteAll();
			AssertEquals("JE_MasterBill", "", declaration.JE_MasterBill);
			AssertEquals("JE_HouseBill", "", declaration.JE_HouseBill);

			declaration.JE_HouseBill = "2";
			AssertEquals("One bill is created and number is copied", "2", declaration.Bills[0].CU_BillNum);

			declaration.JE_MasterBill = "1";
			AssertEquals("Another bill is created and number is copied", "1", declaration.Bills[1].CU_BillNum);
			AssertEquals("PrimaryMaster bill and primary house bill are linked", declaration.PrimaryMasterBill, declaration.PrimaryHouseBill.ParentBill);
		}

		public void TestSynchroniseFromBillNum()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("JE_MasterBill", "", declaration.JE_MasterBill);

			bill1.CU_BillNum = "1";
			AssertEquals("JE_MasterBill", "1", declaration.JE_MasterBill);

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_CU_ParentBill = bill1.PK;
			AssertEquals("JE_HouseBill", "", declaration.JE_HouseBill);

			bill2.CU_BillNum = "2";
			AssertEquals("JE_HouseBill", "2", declaration.JE_HouseBill);
		}

		public void TestUpdateWhenCU_CU_ParentBillChanged()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "1";
			AssertEquals("GUI Presentation Record", true, bill1.CU_GUIPresentationRecord);
			AssertEquals("JE_MasterBill", "1", declaration.JE_MasterBill);

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "2";
			AssertEquals("GUI Presentation Record", false, bill2.CU_GUIPresentationRecord);
			AssertEquals("JE_MasterBill", "1", declaration.JE_MasterBill);

			Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.HouseBill;
			bill3.CU_BillNum = "3";
			AssertNull("No parent bill yet", bill3.ParentBill);
			AssertEquals("JE_MasterBill", "1", declaration.JE_MasterBill);
			AssertEquals("JE_HouseBill", "", declaration.JE_HouseBill);

			bill3.CU_CU_ParentBill = bill2.PK;
			AssertEquals("still not GUI Presentation Record", false, bill3.CU_GUIPresentationRecord);
			AssertEquals("JE_HouseBill", "", declaration.JE_HouseBill);

			bill3.CU_CU_ParentBill = bill1.PK;
			AssertEquals("GUI Presentation Record", true, bill3.CU_GUIPresentationRecord);
			AssertEquals("JE_MasterBill", "1", declaration.JE_MasterBill);
			AssertEquals("JE_HouseBill", "3", declaration.JE_HouseBill);

			Bill bill4 = declaration.Bills.AddNew();
			bill4.CU_BillType = BillTypeList.Codes.HouseBill;
			bill4.CU_BillNum = "4";
			bill4.CU_CU_ParentBill = bill1.PK;
			AssertEquals("GUI Presentation record", false, bill4.CU_GUIPresentationRecord);
			AssertEquals("JE_MasterBill", "1", declaration.JE_MasterBill);
			AssertEquals("JE_HouseBill", "3", declaration.JE_HouseBill);

			declaration.JE_MasterBill = "2";
			AssertEquals("GUI Presentation Record", false, bill1.CU_GUIPresentationRecord);
			AssertEquals("GUI Presentation Record", true, bill2.CU_GUIPresentationRecord);
			AssertEquals("GUI Presentation Record", false, bill3.CU_GUIPresentationRecord);
			AssertEquals("GUI Presentation record", false, bill4.CU_GUIPresentationRecord);

			declaration.JE_MasterBill = "1";
			AssertEquals("GUI Presentation Record", true, bill1.CU_GUIPresentationRecord);
			AssertEquals("GUI Presentation Record", false, bill2.CU_GUIPresentationRecord);
			AssertNotEquals("one of the two children should be GUI presentation record", bill3.CU_GUIPresentationRecord, bill4.CU_GUIPresentationRecord);
		}

		public void TestUpdateGUIPresentationRecordWhenElementIsRemoved()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill0 = declaration.Bills.AddNew();
			bill0.CU_BillType = BillTypeList.Codes.MasterBill;
			bill0.CU_BillNum = "0";
			AssertEquals("GUI Presentation Record", true, bill0.CU_GUIPresentationRecord);
			AssertEquals("JE_MasterBill", "0", declaration.JE_MasterBill);

			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillNum = "1";
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("GUI Presentation Record", false, bill1.CU_GUIPresentationRecord);
			AssertEquals("JE_MasterBill", "0", declaration.JE_MasterBill);

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_BillNum = "2";
			bill2.CU_CU_ParentBill = bill0.PK;
			AssertEquals("GUI Presentation Record", true, bill2.CU_GUIPresentationRecord);
			AssertEquals("JE_HouseBill", "2", declaration.JE_HouseBill);

			Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.HouseBill;
			bill3.CU_BillNum = "3";
			AssertEquals("GUI Presentation Record", false, bill3.CU_GUIPresentationRecord);
			AssertEquals("JE_HouseBill", "2", declaration.JE_HouseBill);
			bill3.CU_CU_ParentBill = bill1.PK;
			AssertEquals("GUI Presentation Record", false, bill3.CU_GUIPresentationRecord);
			AssertEquals("JE_HouseBill", "2", declaration.JE_HouseBill);

			declaration.Bills.RemoveAndDelete(bill2);
			AssertEquals("is not gui presentation record as it is not linked to the main presentation record", false, bill3.CU_GUIPresentationRecord);
			AssertEquals("JE_HouseBill", "", declaration.JE_HouseBill);

			declaration.Bills.RemoveAndDelete(bill0);
			AssertEquals("bill1 becomes GUI presentation master", true, bill1.CU_GUIPresentationRecord);
			AssertEquals("bill3 becomes GUI presentation house", true, bill3.CU_GUIPresentationRecord);
			AssertEquals("JE_MasterBill", "1", declaration.JE_MasterBill);
			AssertEquals("JE_HouseBill", "3", declaration.JE_HouseBill);
		}

		public void TestUpdateWhenCU_BillTypeChanged()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillNum = "1";
			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals("GUI Presentation record", true, bill1.CU_GUIPresentationRecord);
			AssertEquals("JE_HouseBill", "1", declaration.JE_HouseBill);

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillNum = "2";
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("GUI Presentation record", true, bill2.CU_GUIPresentationRecord);
			AssertEquals("JE_MasterBill", "2", declaration.JE_MasterBill);
			AssertEquals("PrimaryMasterBill", bill2, declaration.PrimaryMasterBill);
			AssertEquals("Primary house bill and primary master bill are linked", bill2, bill1.ParentBill);

			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals("GUI Presentation record", false, bill2.CU_GUIPresentationRecord);
			AssertEquals("JE_MasterBill", "", declaration.JE_MasterBill);

			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("GUI Presentation record", true, bill1.CU_GUIPresentationRecord);
			AssertEquals("JE_MasterBill", "1", declaration.JE_MasterBill);
			AssertEquals("If 1 MB and house bills have not parent (CU_CU_ParentID is empty), house bills will be linked to existing MB",
				"2", declaration.JE_HouseBill);
		}

		public void TestJE_HouseBillUpdatedWhenCU_CU_ParentBillChanged()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_MasterBill = "MB11111";
			declaration.JE_HouseBill = "HB122222";
			AssertEquals(2, declaration.Bills.Count);
			var primaryMaster = declaration.PrimaryMasterBill;
			var primaryHouse = declaration.PrimaryHouseBill;
			AssertEquals("MB11111", primaryMaster.CU_BillNum);
			AssertEquals("HB122222", primaryHouse.CU_BillNum);

			var oldPrimaryHouse = primaryHouse;
			declaration.PrimaryHouseBill.CU_CU_ParentBill = ZGuid.Empty;
			var newHouseBill = declaration.Bills.AddNew();
			newHouseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			newHouseBill.CU_BillNum = "HB9929";
			newHouseBill.CU_CU_ParentBill = declaration.PrimaryMasterBill.PK;
			AssertEquals("HB9929", declaration.JE_HouseBill);
			primaryHouse = declaration.PrimaryHouseBill;
			AssertNotEquals(oldPrimaryHouse.PK, primaryHouse.PK);
			AssertEquals("HB9929", primaryHouse.CU_BillNum);

			newHouseBill.Delete();
			AssertEquals("HB122222", declaration.JE_HouseBill);
			primaryHouse = declaration.PrimaryHouseBill;
			AssertEquals(oldPrimaryHouse.PK, primaryHouse.PK);
		}

		public void TestJE_HouseBillUpdatedWhenNumberWithCU_BillNumberMismatch()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			using (declaration.BillGUIPresentationFlagUpdater.SuspendSynch())
			{
				declaration.JE_MasterBill = "MB11111";
				declaration.JE_HouseBill = "HB122222";
			}
			AssertNull(declaration.PrimaryHouseBill);

			AssertNoExceptionThrown("No exception thrown when invoke setter", () =>
			{
				declaration.JE_HouseBill = "HB122222";
			});

			AssertNotNull(declaration.PrimaryHouseBill);
			AssertEquals("HB122222", declaration.PrimaryHouseBill.CU_BillNum);

			using (declaration.BillGUIPresentationFlagUpdater.SuspendSynch())
			{
				declaration.Bills.RemoveAndDeleteAll();

				var houseBill = declaration.Bills.AddNew();
				houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
				houseBill.CU_BillNum = "HB122222";
				houseBill.CU_GUIPresentationRecord = false;
			}

			AssertNull(declaration.PrimaryHouseBill);
			AssertNoExceptionThrown("No exception thrown when invoke setter", () =>
			{
				declaration.JE_HouseBill = "HB122222";
			});

			AssertNotNull(declaration.PrimaryHouseBill);
			AssertEquals("HB122222", declaration.PrimaryHouseBill.CU_BillNum);

			using (declaration.BillGUIPresentationFlagUpdater.SuspendSynch())
			{
				declaration.Bills.RemoveAndDeleteAll();

				var houseBill1 = declaration.Bills.AddNew();
				houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
				houseBill1.CU_BillNum = "HB122222";
				houseBill1.CU_GUIPresentationRecord = false;

				var houseBill2 = declaration.Bills.AddNew();
				houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
				houseBill2.CU_GUIPresentationRecord = true;
			}
			AssertNotNull(declaration.PrimaryHouseBill);
			AssertEquals(ZString.Empty, declaration.PrimaryHouseBill.CU_BillNum);

			AssertNoExceptionThrown("No exception thrown when invoke setter", () =>
			{
				declaration.JE_HouseBill = "HB122222";
			});

			AssertNotNull(declaration.PrimaryHouseBill);
			AssertEquals("HB122222", declaration.PrimaryHouseBill.CU_BillNum);
		}
	}
}
