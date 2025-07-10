using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	class AdditionalBillDataProviderTest : DataObjectReaderTest
	{
		public void TestDeclarationPK()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var provider = new AdditionalBillDataProvider<Bill>(declaration);
			AssertEquals("DeclarationPK", declaration.PK, provider.DeclarationPK);
		}

		public void TestClusterKey()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_ClusterKey = 2342;
			var provider = new AdditionalBillDataProvider<Bill>(declaration);
			AssertEquals("ClusterKey", 2342, provider.ClusterKey);
		}

		public void TestGetAndRemoveExistingBillAndGetUnprocessedExistingBills()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "MB1";
			declaration.JE_HouseBill = "HB1";
			var mb1 = declaration.PrimaryMasterBill;
			mb1.CU_Status = "ST1";
			var mb1hb1 = declaration.PrimaryHouseBill;
			var mb2 = declaration.Bills.AddNew();
			mb2.CU_BillType = BillTypeList.Codes.MasterBill;
			mb2.CU_MasterBill = "MB1";
			mb2.CU_Status = "ST2";
			var mb2hb2 = mb2.ChildBills.AddNew();
			mb2hb2.CU_BillNum = "MB1";
			var mb3 = declaration.Bills.AddNew();
			mb3.CU_BillType = BillTypeList.Codes.MasterBill;
			mb3.CU_MasterBill = "MB2";
			var mb3hb3 = mb3.ChildBills.AddNew();
			mb3hb3.CU_BillNum = "HB1";

			var provider = new AdditionalBillDataProvider<Bill>(declaration);
			AssertSame("mb3", mb3, provider.GetAndRemoveExistingBill("Mb2", BillTypeList.Codes.MasterBill, null));
			AssertSame("mb3hb3", mb3hb3, provider.GetAndRemoveExistingBill("hB1", BillTypeList.Codes.HouseBill.ToLower(), (b) => b.CU_MasterBill.EqualsIgnoringCase("MB2")));
			AssertSame("mb1", mb1, provider.GetAndRemoveExistingBill("MB1", BillTypeList.Codes.MasterBill, (b) => b.CU_Status.EqualsIgnoringCase("ST1")));

			var remainingBills = provider.GetUnprocessedExistingBills().ToArray();
			AssertEquals("remainingBills.Length", 3, remainingBills.Length);
			AssertNoExceptionThrown("mb2 should be unprocessed", () => remainingBills.Single(x => x == mb2));
			AssertNoExceptionThrown("mb1hb1 should be unprocessed", () => remainingBills.Single(x => x == mb1hb1));
			AssertNoExceptionThrown("mb2hb2 should be unprocessed", () => remainingBills.Single(x => x == mb2hb2));
		}

		public void TestCreateNewBill()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var provider = new AdditionalBillDataProvider<Bill>(declaration);
			var bill = provider.CreateNewBill();
			AssertEquals("CU_CU_ParentBill", ZGuid.Empty, bill.CU_CU_ParentBill);
			AssertEquals("CU_BillType", ZString.Empty, bill.CU_BillType);
			AssertEquals("CU_BillNum", ZString.Empty, bill.CU_BillNum);
			AssertEquals("CU_GUIPresentationRecord", ZBool.False, bill.CU_GUIPresentationRecord);
			AssertEquals("declaration.Bills.Count", 1, declaration.Bills.Count);
			AssertEquals("provider.GetUnprocessedExistingBills().Count()", 0, provider.GetUnprocessedExistingBills().Count());
		}

		public void TestSyncPrimaryBill()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var mb1 = declaration.Bills.AddNew();
			mb1.CU_MasterBill = "MB1";
			mb1.CU_Status = "ST1";
			var mb1hb1 = mb1.ChildBills.AddNew();
			mb1hb1.CU_BillNum = "HB1";
			var mb2 = declaration.Bills.AddNew();
			mb2.CU_BillType = BillTypeList.Codes.MasterBill;
			mb2.CU_MasterBill = "MB1";
			mb2.CU_Status = "ST2";
			mb2.CU_GUIPresentationRecord = true;
			var mb2hb2 = mb2.ChildBills.AddNew();
			mb2hb2.CU_BillNum = "MB1";
			mb2hb2.CU_GUIPresentationRecord = true;
			var mb3 = declaration.Bills.AddNew();
			mb3.CU_BillType = BillTypeList.Codes.MasterBill;
			mb3.CU_MasterBill = "MB2";
			var mb3hb3 = mb3.ChildBills.AddNew();
			mb3hb3.CU_BillNum = "HB1";
			var row = ((INeedRow)declaration).Row;
			row[BaseJobDeclaration.Schema.JE_MasterBill] = ZString.Empty;
			row[BaseJobDeclaration.Schema.JE_HouseBill] = ZString.Empty;
			declaration.Bills.Cast<INeedRow>().Select(x => x.Row).ForEach(row => row[Bill.Schema.CU_GUIPresentationRecord] = row[Bill.Schema.PK].In(mb1.PK.ToGuid(), mb1hb1.PK.ToGuid()));
			var provider = new AdditionalBillDataProvider<Bill>(declaration);
			AssertEquals("Pre: JE_MasterBill", ZString.Empty, declaration.JE_MasterBill);
			AssertSame("Pre: PrimaryMasterBill", mb1, declaration.PrimaryMasterBill);
			AssertEquals("Pre: JE_HouseBill", ZString.Empty, declaration.JE_HouseBill);
			AssertSame("Pre: PrimaryHouseBill", mb1hb1, declaration.PrimaryHouseBill);

			provider.SyncPrimaryBill(mb1, "MB1", (b) => b.CU_Status.EqualsIgnoringCase("ST1"));
			AssertEquals("JE_MasterBill", "MB1", declaration.JE_MasterBill);

			provider.SyncPrimaryBill(mb1hb1, "HB1", (b) => b.CU_CU_ParentBill == mb1.PK);
			AssertEquals("JE_HouseBill", "HB1", declaration.JE_HouseBill);

			((INeedRow)mb1).Row[Bill.Schema.CU_GUIPresentationRecord] = false;
			((INeedRow)mb3).Row[Bill.Schema.CU_GUIPresentationRecord] = true;
			AssertSame("Pre: PrimaryMasterBill", mb3, declaration.PrimaryMasterBill);
			provider.SyncPrimaryBill(mb3, "MB2", null);
			AssertEquals("JE_MasterBill", "MB2", declaration.JE_MasterBill);
		}
	}
}

