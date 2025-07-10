using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Moq.Protected;

namespace Enterprise.Customs.Business.Testing
{
	class BaseHouseBillTestCase : TestCaseWithFactory
	{
		public void TestTypeOfChildBills()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			AssertType<ChildBillCollection<Bill, BaseJobDeclaration>>(declaration.Bills.AddNew().ChildBills);
		}

		public void TestGlobalManifestRequirements()
		{
			var mockDec = Factory.NewMoq<BaseJobDeclaration>();
			mockDec.Protected().Setup<bool>("SupportTransferFromCustomsToManifestEvent").Returns(true);
			var dec = mockDec.Object;
			var log = dec.Logs.AddNew(Events.TransferFromManifestToCustoms, "MK322423");
			dec.JE_MasterBill = "MB1";
			dec.JE_HouseBill = "HB1";
			var masterBill = dec.PrimaryMasterBill;
			AssertEquals("MB1", masterBill.CU_BillNum);
			AssertEquals(true, masterBill.CU_BillNumInfo.ReadOnly);
			AssertEquals(BillTypeList.Codes.MasterBill, masterBill.CU_BillType);
			AssertEquals(true, masterBill.CU_BillTypeInfo.ReadOnly);
			ICanDelete masterBillCanDelete = masterBill;
			AssertEquals(false, masterBillCanDelete.CanDelete);
			AssertEquals(Bill.ReasonForCannotDeleteWhenIntegratedWithManifest, masterBillCanDelete.ReasonForNotAbleToDelete);
			var houseBill = dec.PrimaryHouseBill;
			AssertEquals("HB1", houseBill.CU_BillNum);
			AssertEquals(true, houseBill.CU_BillNumInfo.ReadOnly);
			AssertEquals(BillTypeList.Codes.HouseBill, houseBill.CU_BillType);
			AssertEquals(true, houseBill.CU_BillTypeInfo.ReadOnly);
			var houseBillCanDelete = houseBill;
			AssertEquals(false, houseBillCanDelete.CanDelete);
			AssertEquals(Bill.ReasonForCannotDeleteWhenIntegratedWithManifest, houseBillCanDelete.ReasonForNotAbleToDelete);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = ZString.Empty;
			}
			AssertEquals(false, masterBill.CU_BillNumInfo.ReadOnly);
			AssertEquals(true, masterBillCanDelete.CanDelete);
			AssertEquals(Bill.ReasonForCannotDeleteWhenSynchronised, masterBillCanDelete.ReasonForNotAbleToDelete);
			AssertEquals(false, houseBill.CU_BillNumInfo.ReadOnly);
			AssertEquals(true, houseBillCanDelete.CanDelete);
			AssertEquals(Bill.ReasonForCannotDeleteWhenSynchronised, houseBillCanDelete.ReasonForNotAbleToDelete);
		}

		public void TestIsLowestBill()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			Bill houseBill = masterBill.ChildBills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			Bill subHouseBill = houseBill.ChildBills.AddNew();
			subHouseBill.CU_BillType = BillTypeList.Codes.SubHouseBill;
			AssertEquals("masterBill is not the lowest", false, masterBill.IsLowestBill);
			AssertEquals("houseBill is not the lowest", false, houseBill.IsLowestBill);
			AssertEquals("subHouseBil is the lowest", true, subHouseBill.IsLowestBill);

			houseBill.ChildBills.Remove(subHouseBill);
			AssertEquals("masterBill is not the lowest", false, masterBill.IsLowestBill);
			AssertEquals("houseBill is the lowest", true, houseBill.IsLowestBill);
			AssertEquals("subHouseBil is the lowest", true, subHouseBill.IsLowestBill);

			masterBill.ChildBills.Remove(houseBill);
			AssertEquals("masterBill is the lowest", true, masterBill.IsLowestBill);
			AssertEquals("houseBill is the lowest", true, houseBill.IsLowestBill);
			AssertEquals("subHouseBil is the lowest", true, subHouseBill.IsLowestBill);
		}

		public void TestBillType()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("IsMasterBill", true, bill.IsMasterBill);
			AssertEquals("IsHouseBill", false, bill.IsHouseBill);
			AssertEquals("IsSubHouseBill", false, bill.IsSubHouseBill);

			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals("IsMasterBill", false, bill.IsMasterBill);
			AssertEquals("IsHouseBill", true, bill.IsHouseBill);
			AssertEquals("IsSubHouseBill", false, bill.IsSubHouseBill);

			bill.CU_BillType = BillTypeList.Codes.SubHouseBill;
			AssertEquals("IsMasterBill", false, bill.IsMasterBill);
			AssertEquals("IsHouseBill", false, bill.IsHouseBill);
			AssertEquals("IsSubHouseBill", true, bill.IsSubHouseBill);
		}

		public void TestSettingCU_CU_ParentBillManagesCU_GUIPresentationRecord()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("GUIPresentationRecord", true, masterBill1.CU_GUIPresentationRecord);

			Bill masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("GUIPresentationRecord", false, masterBill2.CU_GUIPresentationRecord);

			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals("GUIPresentationRecord", false, houseBill2.CU_GUIPresentationRecord);

			houseBill2.CU_CU_ParentBill = masterBill1.PK;
			AssertEquals("linked to the primary parent:GUIPresentationRecord", true, houseBill2.CU_GUIPresentationRecord);

			houseBill2.CU_CU_ParentBill = masterBill2.PK;
			AssertEquals("GUIPresentationRecord", false, houseBill2.CU_GUIPresentationRecord);

			Bill houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill1.CU_CU_ParentBill = masterBill1.PK;
			AssertEquals("GUIPresentationRecord", true, houseBill1.CU_GUIPresentationRecord);

			Bill houseBill3 = declaration.Bills.AddNew();
			houseBill3.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill3.CU_CU_ParentBill = masterBill1.PK;
			AssertEquals("GUIPresentationRecord as there is a GUI presentation record for the parent", false, houseBill3.CU_GUIPresentationRecord);
		}

		public void TestCU_ParentBillUniqueCodeReadOnly()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("IsMasterBill", true, bill.IsMasterBill);
			AssertEquals("CU_ParentBillUniqueCode readonly for master bill", true, bill.CU_ParentBillUniqueCodeInfo.ReadOnly);

			bill.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals("IsMasterBill", false, bill.IsMasterBill);
			AssertEquals("CU_ParentBillUniqueCode readonly for house bill", false, bill.CU_ParentBillUniqueCodeInfo.ReadOnly);

			bill.CU_BillType = BillTypeList.Codes.SubHouseBill;
			AssertEquals("IsMasterBill", false, bill.IsMasterBill);
			AssertEquals("CU_ParentBillUniqueCode readonly for house bill", false, bill.CU_ParentBillUniqueCodeInfo.ReadOnly);
		}

		public void TestICanDelete()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill houseBill = declaration.Bills.AddNew();
			AssertEquals(true, ((ICanDelete)houseBill).CanDelete);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			AssertEquals(false, ((ICanDelete)houseBill).CanDelete);
			AssertEquals(Bill.ReasonForCannotDeleteWhenSynchronised, ((ICanDelete)houseBill).ReasonForNotAbleToDelete);

			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals(true, ((ICanDelete)houseBill).CanDelete);
		}

		public void TestWillBeDeletedDuringSave()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.Bills.AddNew();
			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_GUIPresentationRecord = false;
			Assert(houseBill.WillBeDeletedDuringSave);
			houseBill.CU_HouseBill = "123";
			Assert(!houseBill.WillBeDeletedDuringSave);
			houseBill.CU_HouseBill = "";
			houseBill.CU_IssueDate = ZDateTime.Now;
			Assert(!houseBill.WillBeDeletedDuringSave);
			houseBill.CU_IssueDate = ZDateTime.Empty;
			houseBill.PackingGroups.AddNew();
			Assert(!houseBill.WillBeDeletedDuringSave);
			houseBill.PackingGroups.RemoveAndDeleteAll();
			Assert(houseBill.WillBeDeletedDuringSave);
		}

		public void TestIsSavedByFactory()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			var bill = declaration.Bills.AddNew();

			CombineAssertions(() =>
			{
				Assert("can be saved properly", bill.IsSavedByFactory);
				declaration.MakeNonPersistent();
				Assert("should stop being saved if declaration not persistent", !bill.IsSavedByFactory);
				bill.Delete();
				Assert("can be saved properly for delete", bill.IsSavedByFactory);
			});
		}
	}
}
