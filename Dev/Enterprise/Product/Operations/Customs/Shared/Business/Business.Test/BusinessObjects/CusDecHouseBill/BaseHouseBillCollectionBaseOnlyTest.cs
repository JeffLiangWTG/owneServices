using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseHouseBillCollectionBaseOnlyTest : TestCaseWithFactory
	{
		public void TestNoAddNewForGlobalManifest()
		{
			var mockDec = Factory.NewMoq<BaseJobDeclaration>();
			mockDec.Protected().Setup<bool>("SupportTransferFromCustomsToManifestEvent").Returns(true);
			var dec = mockDec.Object;
			var log = dec.Logs.AddNew(Events.TransferFromManifestToCustoms, "MK322423");
			var bills = dec.Bills;
			AssertEquals(false, bills.AllowNew);
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Reference = ZString.Empty;
			}
			AssertEquals(true, bills.AllowNew);
		}

		public void TestLoadingDoesNotClearDeclarationData()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MasterBill = "MB1";
			declaration.JE_HouseBill = "HB1";
			AssertEquals(2, declaration.Bills.Count);
			declaration.Bills.Load();
			AssertEquals(2, declaration.Bills.Count);
			AssertEquals("MB1", declaration.JE_MasterBill);
			AssertEquals("HB1", declaration.JE_HouseBill);
		}

		public void TestFindByBillNumberAndType()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;
			bill1.CU_BillNum = "BILLNUM";
			bill1.CU_NoOfPacks = 10m;
			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "BILLNUM";
			bill2.CU_NoOfPacks = 15m;
			var bill3 = bill1.ChildBills.AddNew();
			bill3.CU_BillNum = "BILLNUM";
			bill3.CU_NoOfPacks = 10m;
			var bill4 = bill1.ChildBills.AddNew();
			bill4.CU_BillNum = "BILLNUM";
			bill4.CU_NoOfPacks = 15m;
			var bill5 = bill3.ChildBills.AddNew();
			bill5.CU_BillNum = "BILLNUM";
			bill5.CU_NoOfPacks = 10m;
			var bill6 = bill3.ChildBills.AddNew();
			bill6.CU_BillNum = "BILLNUM";
			bill6.CU_NoOfPacks = 15m;
			AssertEquals(bill1, declaration.Bills.FindByBillNumberAndType("BILLNUM", BillTypeList.Codes.MasterBill));
			AssertEquals(bill3, declaration.Bills.FindByBillNumberAndType("BILLNUM", BillTypeList.Codes.HouseBill));
			AssertEquals(bill5, declaration.Bills.FindByBillNumberAndType("BILLNUM", BillTypeList.Codes.SubHouseBill));
			AssertEquals(bill1, declaration.Bills.FindByBillNumberAndType("BILLNUM", BillTypeList.Codes.MasterBill));
			AssertEquals(bill3, declaration.Bills.FindByBillNumberAndType("BILLNUM", BillTypeList.Codes.HouseBill));
			AssertEquals(bill5, declaration.Bills.FindByBillNumberAndType("BILLNUM", BillTypeList.Codes.SubHouseBill));
			Func<Bill, bool> match = (x) => x.CU_NoOfPacks == 15m;
			AssertEquals(bill2, declaration.Bills.FindByBillNumberAndType("BILLNUM", BillTypeList.Codes.MasterBill, match));
			AssertEquals(bill4, declaration.Bills.FindByBillNumberAndType("BILLNUM", BillTypeList.Codes.HouseBill, match));
			AssertEquals(bill6, declaration.Bills.FindByBillNumberAndType("BILLNUM", BillTypeList.Codes.SubHouseBill, match));
			match = (x) => x.CU_NoOfPacks == 10m;
			AssertEquals(bill1, declaration.Bills.FindByBillNumberAndType("BILLNUM", BillTypeList.Codes.MasterBill, match));
			AssertEquals(bill3, declaration.Bills.FindByBillNumberAndType("BILLNUM", BillTypeList.Codes.HouseBill, match));
			AssertEquals(bill5, declaration.Bills.FindByBillNumberAndType("BILLNUM", BillTypeList.Codes.SubHouseBill, match));
		}

		[ExpectNoExceptions]
		public void TestNoExceptionWhenBillRemovedAfterBillTypeChange()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill1 = declaration.Bills.AddNew();
			bill1.CU_BillNum = "Bill 1";
			bill1.CU_BillType = BillTypeList.Codes.MasterBill;

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_BillNum = "Bill 2";
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_CU_ParentBill = bill1.PK;

			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.Delete();
			AssertEquals(ZGuid.Empty, bill1.CU_CU_ParentBill);
		}
	}
}
