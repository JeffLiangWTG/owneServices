using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(Bill))]
	public abstract class BaseHouseBillTest<TBill, TJobDeclaration> : EnterpriseBusinessObjectTestCase
		where TBill : Bill
		where TJobDeclaration : BaseJobDeclaration
	{
		public void TestInvoicesAndDelete()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill houseBill = declaration.Bills.AddNew();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_CU_RelatedHouseBill = houseBill.PK;
			AssertEquals(true, houseBill.Invoices.Contains(invoice));

			houseBill.Delete();
			AssertEquals("reference is removed", ZGuid.Empty, invoice.JZ_CU_RelatedHouseBill);
		}

		public void TestBillAndParentCombination()
		{
			AssertEquals("111", Bill.BillAndParentCombination.BillAndParentBill("111", ""));
			AssertEquals("111 (222)", Bill.BillAndParentCombination.BillAndParentBill("111", "222"));
			AssertEquals("HB:111", Bill.BillAndParentCombination.BillTypeAndNumber("111", BillTypeList.Codes.HouseBill));
			AssertEquals("111", Bill.BillAndParentCombination.BillAndParentBill("111", ""));
			AssertEquals("HB:111 (MB:222)", Bill.BillAndParentCombination.BillAndParentBill("111", BillTypeList.Codes.HouseBill, "222", BillTypeList.Codes.MasterBill));
		}

		public void TestCU_BillUniqueCode()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "111";
			AssertEquals("CU_BillUniqueCode for master bill", "MB:111", masterBill.CU_BillUniqueCode);

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "222";
			houseBill.CU_CU_ParentBill = ZGuid.Empty;
			AssertEquals("CU_BillUniqueCode for house bill", "HB:222", houseBill.CU_BillUniqueCode);

			houseBill.CU_CU_ParentBill = masterBill.PK;
			AssertEquals("CU_BillUniqueCode for house bill", "HB:222 (MB:111)", houseBill.CU_BillUniqueCode);
		}

		public void TestCU_ParentBillUniqueCodeSetsFK()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "111";

			Bill masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "222";

			Bill houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_BillNum = "222";

			houseBill.CU_ParentBillUniqueCode = masterBill.CU_BillUniqueCode;
			AssertEquals("CU_CU_ParentBill should have been set", masterBill.PK, houseBill.CU_CU_ParentBill);
			AssertEquals("ParentBill", masterBill, houseBill.ParentBill);

			houseBill.CU_ParentBillUniqueCode = masterBill2.CU_BillUniqueCode;
			AssertEquals("CU_CU_ParentBill should have been set", masterBill2.PK, houseBill.CU_CU_ParentBill);
			AssertEquals("ParentBill", masterBill2, houseBill.ParentBill);

			houseBill.CU_ParentBillUniqueCode = "CRAP";
			AssertEquals("CU_CU_ParentBill should have been set to ZGuid.Empty", ZGuid.Empty, houseBill.CU_CU_ParentBill);
			AssertNull("ParentBill", houseBill.ParentBill);
		}

		public void TestSettingToMasterBillTypeClearsParentBill()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.HouseBill;
			bill2.CU_CU_ParentBill = bill.PK;
			AssertEquals("CU_CU_ParentBill", bill.PK, bill2.CU_CU_ParentBill);

			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("CU_CU_ParentBill is cleared", ZGuid.Empty, bill2.CU_CU_ParentBill);
			AssertEquals("as it is readonly to users", true, bill2.CU_CU_ParentBillInfo.ReadOnly);
			AssertEquals("as it is readonly to users", true, bill2.CU_ParentBillUniqueCodeInfo.ReadOnly);
		}

		public void TestGUIPresentationRecordUpdatesChildBills()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "1";
			AssertEquals("GUI Presentation record", true, bill.CU_GUIPresentationRecord);

			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = BillTypeList.Codes.MasterBill;
			bill2.CU_BillNum = "2";
			AssertEquals("GUI Presentation record", false, bill2.CU_GUIPresentationRecord);

			Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = BillTypeList.Codes.HouseBill;
			bill3.CU_CU_ParentBill = bill.PK;
			bill3.CU_BillNum = "3";
			AssertEquals("GUI Presentation record", true, bill3.CU_GUIPresentationRecord);

			Bill bill4 = declaration.Bills.AddNew();
			bill4.CU_BillType = BillTypeList.Codes.HouseBill;
			bill4.CU_CU_ParentBill = bill2.PK;
			bill4.CU_BillNum = "4";
			AssertEquals("GUI Presentation record", false, bill4.CU_GUIPresentationRecord);

			declaration.JE_MasterBill = "2";
			AssertEquals("bill2 becomes GUI presentation record", true, bill2.CU_GUIPresentationRecord);
			AssertEquals("bill is not a gui presentation record any more", false, bill.CU_GUIPresentationRecord);
			AssertEquals("GUI Presentation record", true, bill4.CU_GUIPresentationRecord);
			AssertEquals("GUI Presentation record", false, bill3.CU_GUIPresentationRecord);
			AssertEquals("JE_HouseBill should point to bill4 which is linked to master bill 2", "4", declaration.JE_HouseBill);
		}

		public void TestCU_BillUniqueCodeMasterNoHouse()
		{
			Bill.CU_BillType = BillTypeList.Codes.MasterBill;
			Bill.CU_MasterBill = "111";
			AssertEquals("MB:111", Bill.CU_BillUniqueCode);
		}

		public void TestCU_BillUniqueCodeHouseNoMaster()
		{
			Bill.CU_BillType = BillTypeList.Codes.HouseBill;
			Bill.CU_HouseBill = "111";
			AssertEquals("HB:111", Bill.CU_BillUniqueCode);
		}

		public void TestCU_BillUniqueCodeMasterAndHouse()
		{
			Bill masterBill = Bill.Declaration.Bills.AddNew();
			masterBill.CU_BillType = BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "222";
			Bill.CU_BillType = BillTypeList.Codes.HouseBill;
			Bill.CU_HouseBill = "111";
			Bill.CU_MasterBill = "222";
			AssertEquals("PreCondition;CU_CU_ParentBill is set", masterBill.PK, Bill.CU_CU_ParentBill);
			AssertEquals("HB:111 (MB:222)", Bill.CU_BillUniqueCode);
		}

		public void TestCU_BillUniqueCodeWithNoHouseOrMaster()
		{
			Bill.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals("", Bill.CU_BillUniqueCode);
		}

		public void TestCU_HouseBillForEachBillType()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			Bill bill = Declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals(ZString.Empty, bill.CU_HouseBill);
			Bill houseBill = Declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals(ZString.Empty, houseBill.CU_HouseBill);
			houseBill.CU_BillNum = "HBL";
			AssertEquals("HBL", houseBill.CU_HouseBill);

			Bill subHouseBill = Declaration.Bills.AddNew();
			subHouseBill.CU_BillType = BillTypeList.Codes.SubHouseBill;
			AssertEquals(ZString.Empty, subHouseBill.CU_HouseBill);
			subHouseBill.CU_CU_ParentBill = houseBill.PK;
			AssertEquals("HBL", subHouseBill.CU_HouseBill);
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestSettingMasterBillOnSubHouse()
		{
			Bill bill = Declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.SubHouseBill;
			bill.CU_MasterBill = "123";
		}

		public void TestCU_HouseBillProxiesThroughToDeclaration()
		{
			var mockDeclaration = Factory.NewMoq<TJobDeclaration>();
			mockDeclaration.Protected().Setup<bool>("IsPackingInformationRelevantCore").Returns(true);

			BaseJobDeclaration declaration = mockDeclaration.Object;

			Bill houseBill1 = declaration.Bills.AddNew();
			houseBill1.CU_BillType = BillTypeList.Codes.HouseBill;

			Bill houseBill2 = declaration.Bills.AddNew();
			houseBill2.CU_BillType = BillTypeList.Codes.HouseBill;

			AssertEquals("Precondition: Declaration.JE_HouseBill", "", declaration.JE_HouseBill);
			houseBill1.CU_HouseBill = "HOUSEBILL1";
			AssertEquals("Declaration.JE_HouseBill", "HOUSEBILL1", declaration.JE_HouseBill);

			houseBill2.CU_HouseBill = "HOUSEBILL2";
			AssertEquals("Declaration.JE_HouseBill", "HOUSEBILL1", declaration.JE_HouseBill);

			declaration.Bills.RemoveAndDelete(houseBill1);
			AssertEquals("Declaration.JE_HouseBill", "HOUSEBILL2", declaration.JE_HouseBill);

			declaration.Bills.RemoveAndDelete(houseBill2);
			AssertEquals("Declaration.JE_HouseBill", "", declaration.JE_HouseBill);
		}

		public void TestAssignedContainers()
		{
			BaseCusContainer container1 = Declaration.CusContainers.AddNew();
			BaseCusContainer container2 = Declaration.CusContainers.AddNew();

			BasePackingGroup packingGroup1 = Bill.PackingGroups.AddNew();
			packingGroup1.CR_CO_Container = container1.PK;
			AssertEquals("Assigned Container", 1, Bill.Containers.Count);

			BasePackingGroup packingGroup2 = Bill.PackingGroups.AddNew();
			packingGroup2.CR_CO_Container = container2.PK;
			AssertEquals("Assigned Containers", 2, Bill.Containers.Count);

			packingGroup1.Delete();
			AssertEquals("Assigned Container", 1, Bill.Containers.Count);
			packingGroup2.Delete();
			AssertEquals("Assigned Container", 0, Bill.Containers.Count);
		}

		public void TestJobDeclaration()
		{
			Bill.CU_JE = Declaration.PK;
			AssertEquals("Declaration Accessor", Declaration, Bill.Declaration);
			Bill.CU_JE = ZGuid.Empty;
			AssertEquals("Declaration Accessor", Declaration, Bill.Declaration);
		}

		public void TestPackingGroupCollection()
		{
			var billPackingGroup = Declaration.PackingGroups.AddNew();
			billPackingGroup.CR_CU_HouseBill = Bill.PK;
			AssertEquals("billPackingGroup is changed", true, billPackingGroup.HasChanges);
			var packingGroupCollection = Bill.PackingGroups;
			BasePackingGroup packingGroup = Bill.PackingGroups.AddNew();
			AssertNotNull(packingGroup);
			AssertCollectionContains(billPackingGroup, packingGroupCollection);
			AssertEquals("Not reset HasChanges.", true, billPackingGroup.HasChanges);
		}

		public void TestReloadPackingGroup()
		{
			var bill = GetNewHouseBill();
			CombineAssertions(() =>
			{
				typeof(Bill).GetField("fPackingGroups", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(bill, null);
				bill.ReloadPackingGroup(true);
				Assert("reload when fPackingGroups is null", !bill.PackingGroups.HasReloadedFromDb);
				bill.ReloadPackingGroup(true);
				Assert("reload when fPackingGroups is not null", bill.PackingGroups.HasReloadedFromDb);
			});
		}

		public void TestRebuildChildBillsWhenDeleted()
		{
			var declaration = Factory.New<BaseJobDeclaration>();

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;

			var houseBill = declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_CU_ParentBill = bill.PK;

			AssertEquals(1, bill.ChildBills.Count);

			houseBill.Delete();

			AssertEquals(0, bill.ChildBills.Count);
		}

		public virtual void TestLinkMasterBillToExistingHouseBills()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
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

			var masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = BillTypeList.Codes.MasterBill;
			AssertEquals("Existing House Bill 1 should have Parent Master Bill 1", primaryMasterBill.PK, houseBill1.CU_CU_ParentBill);
			AssertEquals("Existing House Bill 2 should have Parent Master Bill 1", primaryMasterBill.PK, houseBill2.CU_CU_ParentBill);

			var houseBill3 = declaration.Bills.AddNew();
			houseBill3.CU_BillType = BillTypeList.Codes.HouseBill;
			AssertEquals("No Parent Bill", ZGuid.Empty, houseBill3.CU_CU_ParentBill);
		}

		public virtual void TestWillBeDeletedDuringSave()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_GUIPresentationRecord = false;
			bill.CU_BillNum = "12345";
			bill.OnSaving();
			Assert(!bill.IsDeleted);

			bill.CU_BillNum = ZString.Empty;
			((ISupportDataImporting)bill).IsImportingData = true;
			bill.OnSaving();
			Assert(!bill.IsDeleted);

			((ISupportDataImporting)bill).IsImportingData = false;
			bill.OnSaving();
			Assert(bill.IsDeleted);
		}

		#region Implementation
		#region Shipment
		ForwardingShipment fShipment;
		protected ForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<ForwardingShipment>();
				}
				return fShipment;
			}
		}
		#endregion

		#region Declaration
		BaseJobDeclaration fDeclaration;
		protected BaseJobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = GetJobDeclaration();
				}
				return fDeclaration;
			}
		}
		#endregion

		#region GetJobDeclaration
		protected virtual BaseJobDeclaration GetJobDeclaration()
		{
			return Factory.New<BaseJobDeclaration>();
		}
		#endregion

		#region HouseBill
		Bill fBill;
		protected Bill Bill
		{
			get
			{
				if (fBill == null)
				{
					fBill = GetNewHouseBill();
				}
				return fBill;
			}
		}
		#endregion

		#region GetNewHouseBill
		protected virtual Bill GetNewHouseBill()
		{
			return Declaration.Bills.AddNew();
		}
		#endregion

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(factory);
			Bill result = testDec.Bills.AddNew();
			result.CU_BillType = BillTypeList.Codes.HouseBill;
			result.CU_HouseBill = "HAWB";
			if (result.PackingGroups.Count == 0)
			{
				result.PackingGroups.AddNew();
			}

			result.ChildBills.AddNew();
			return result;
		}
		#endregion
	}
}
