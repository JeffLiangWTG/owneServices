using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(WarehouseOperatorTransactionsFilterStripBusinessObject))]
	class WarehouseOperatorTransactionsFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new WarehouseOperatorTransactionsFilterStripBusinessObject();

		public void TestProductCodeFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.ProductCode];
			filter.IsActive = true;
			filter.Property = "ProductCode1";
			Assert(transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction2.MatchesFilter(filterObj.Filter));
			filter.Property = "XXX";
			Assert(!transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction2.MatchesFilter(filterObj.Filter));
		}

		public void TestStatusFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.Status];
			filter.IsActive = true;
			filter.Property = "QUE";
			Assert(transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction2.MatchesFilter(filterObj.Filter));
			Assert(!transaction3.MatchesFilter(filterObj.Filter));
			filter.Property = "VAL";
			Assert(!transaction1.MatchesFilter(filterObj.Filter));
			Assert(transaction2.MatchesFilter(filterObj.Filter));
			Assert(!transaction3.MatchesFilter(filterObj.Filter));
			filter.Property = "CLS";
			Assert(!transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction2.MatchesFilter(filterObj.Filter));
			Assert(transaction3.MatchesFilter(filterObj.Filter));
		}

		public void TestTransactionTypeFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.TransactionType];
			filter.IsActive = true;
			filter.Property = "ORD";
			Assert(transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction2.MatchesFilter(filterObj.Filter));
			filter.Property = "REC";
			Assert(!transaction1.MatchesFilter(filterObj.Filter));
			Assert(transaction2.MatchesFilter(filterObj.Filter));
		}

		public void TestOwnerReferenceFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.OwnerReference];
			filter.IsActive = true;
			filter.Property = "OwnerReference1";
			Assert(transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction2.MatchesFilter(filterObj.Filter));
			filter.Property = "XXX";
			Assert(!transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction2.MatchesFilter(filterObj.Filter));
		}

		public void TestExportTypeFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.ExportType];
			filter.IsActive = true;
			filter.Property = "EXP";
			Assert(transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction3.MatchesFilter(filterObj.Filter));
			filter.Property = "BLN";
			Assert(!transaction1.MatchesFilter(filterObj.Filter));
			Assert(transaction3.MatchesFilter(filterObj.Filter));
		}

		public void TestIsCustomsControlledFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.IsCustomsControlled];
			filter.IsActive = true;
			filter.Property0 = false;
			Assert(transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction2.MatchesFilter(filterObj.Filter));
			filter.Property0 = true;
			Assert(!transaction1.MatchesFilter(filterObj.Filter));
			Assert(transaction2.MatchesFilter(filterObj.Filter));
		}

		public void TestTransactionDateFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleDateFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.TransactionDate];
			filter.IsActive = true;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = ZDate.BrettsBirthday;
			filter.Property2 = ZDate.BrettsBirthday;
			Assert(transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction2.MatchesFilter(filterObj.Filter));
		}

		public void TestBatchNumberFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.BatchNumber];
			filter.IsActive = true;
			filter.Property = "Batch1";
			Assert(transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction2.MatchesFilter(filterObj.Filter));
		}

		public void TestLineReferenceFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.LineReference];
			filter.IsActive = true;
			filter.Property = "LineReference1";
			Assert(transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction2.MatchesFilter(filterObj.Filter));
			filter.Property = "XXX";
			Assert(!transaction1.MatchesFilter(filterObj.Filter));
			Assert(!transaction2.MatchesFilter(filterObj.Filter));
		}

		public void TestCountryOfOriginFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleNkFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.CountryOfOrigin];
			filter.IsActive = true;

			filter.Property = "GB";
			CombineAssertions("Filter is GB", () =>
			{
				Assert("Transaction1", !transaction1.MatchesFilter(filterObj.Filter));
				Assert("Transaction2", transaction2.MatchesFilter(filterObj.Filter));
			});

			filter.Property = "AT";
			CombineAssertions("Filter is AT", () =>
			{
				Assert("Transaction1", !transaction1.MatchesFilter(filterObj.Filter));
				Assert("Transaction2", !transaction2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestCustomsEntryNumberFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.CustomsEntryNumber];
			filter.IsActive = true;

			filter.Property = "123";
			CombineAssertions("Filter is 123", () =>
			{
				Assert("Transaction1", transaction1.MatchesFilter(filterObj.Filter));
				Assert("Transaction2", !transaction2.MatchesFilter(filterObj.Filter));
			});

			filter.Property = "321";
			CombineAssertions("Filter is 321", () =>
			{
				Assert("Transaction1", !transaction1.MatchesFilter(filterObj.Filter));
				Assert("Transaction1", !transaction2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestIsSystemCreatedFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleFlagsFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.IsSystemCreated];
			transaction1.Batch.WOB_IsSystemCreated = true;
			Factory.Save();
			filter.IsActive = true;
			filter.Property0 = false;
			CombineAssertions("Filter is false", () =>
			{
				Assert("Transaction1", !transaction1.MatchesFilter(filterObj.Filter));
				Assert("Transaction2", transaction2.MatchesFilter(filterObj.Filter));
			});

			filter.Property0 = true;
			CombineAssertions("Filter is true", () =>
			{
				Assert("Transaction1", transaction1.MatchesFilter(filterObj.Filter));
				Assert("Transaction2", !transaction2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestWarehouseFilter()
		{
			var helper = new ZAWhsDataTestHelper(Factory);
			var warehouse1 = helper.GetNewWhsWarehouse(testAddress1.PK, isVirtualWarehouse: true, "WZ1");
			var warehouse2 = helper.GetNewWhsWarehouse(testAddress2.PK, isVirtualWarehouse: true, "WZ2");
			var warehouse3 = helper.GetNewWhsWarehouse(testAddress3.PK, isVirtualWarehouse: true, "WZ3");
			Factory.Save();

			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.Warehouse];

			CombineAssertions("WarehouseFilter's settings", () =>
			{
				AssertEquals(FilterVisibility.AlwaysVisible, filter.Visibility);
				AssertEquals(expected: false, filter.SupportsFiltersMatchComparisonOperator);
			});

			filter.IsActive = true;

			filter.Property = warehouse1.PK;

			var a = filterObj.Filter;

			CombineAssertions("Filter is warehouse1.PK", () =>
			{
				AssertEquals("Transaction1", expected: true, transaction1.MatchesFilter(filterObj.Filter));
				AssertEquals("Transaction2", expected: false, transaction2.MatchesFilter(filterObj.Filter));
				AssertEquals("Transaction3", expected: true, transaction3.MatchesFilter(filterObj.Filter));
				AssertEquals("Transaction4", expected: false, transaction4.MatchesFilter(filterObj.Filter));
			});

			filter.Property = warehouse2.PK;

			CombineAssertions("Filter is warehouse2.PK", () =>
			{
				AssertEquals("Transaction1", expected: false, transaction1.MatchesFilter(filterObj.Filter));
				AssertEquals("Transaction2", expected: false, transaction2.MatchesFilter(filterObj.Filter));
				AssertEquals("Transaction3", expected: false, transaction3.MatchesFilter(filterObj.Filter));
				AssertEquals("Transaction4", expected: true, transaction4.MatchesFilter(filterObj.Filter));
			});

			filter.Property = warehouse3.PK;

			CombineAssertions("Filter is warehouse3.PK", () =>
			{
				AssertEquals("Transaction1", expected: false, transaction1.MatchesFilter(filterObj.Filter));
				AssertEquals("Transaction2", expected: true, transaction2.MatchesFilter(filterObj.Filter));
				AssertEquals("Transaction3", expected: false, transaction3.MatchesFilter(filterObj.Filter));
				AssertEquals("Transaction4", expected: false, transaction4.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestProductOwnerFilter()
		{
			var filterObj = new WarehouseOperatorTransactionsFilterStripBusinessObject();
			var filter = (ModuleGuidFilter)filterObj[WarehouseOperatorTransactionsFilterStripBusinessObject.FilterConstants.ProductOwner];

			CombineAssertions("ProductOwnerFilter's settings", () =>
			{
				AssertEquals(FilterVisibility.AlwaysVisible, filter.Visibility);
				AssertEquals(expected: false, filter.SupportsFiltersMatchComparisonOperator);
			});

			filter.IsActive = true;

			filter.Property = testOrg1.PK;

			CombineAssertions("Filter is testOrg1.PK", () =>
			{
				Assert("Transaction1", transaction1.MatchesFilter(filterObj.Filter));
				Assert("Transaction2", transaction2.MatchesFilter(filterObj.Filter));
				Assert("Transaction3", transaction3.MatchesFilter(filterObj.Filter));
				Assert("Transaction4", !transaction4.MatchesFilter(filterObj.Filter));
			});

			filter.Property = testOrg2.PK;

			CombineAssertions("Filter is testOrg2.PK", () =>
			{
				Assert("Transaction1", !transaction1.MatchesFilter(filterObj.Filter));
				Assert("Transaction2", !transaction2.MatchesFilter(filterObj.Filter));
				Assert("Transaction3", !transaction3.MatchesFilter(filterObj.Filter));
				Assert("Transaction4", transaction4.MatchesFilter(filterObj.Filter));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.OH_Code = "NRW";
			testAddress1 = testOrg1.Addresses.AddNew();
			testAddress1.OA_Address1 = "ADD1";
			testAddress1.OA_Code = "PC1";
			testAddress3 = testOrg1.Addresses.AddNew();
			testAddress3.OA_Address1 = "ADD3";
			testAddress3.OA_Code = "PC3";

			testOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg2.OH_Code = "NRL";
			testAddress2 = testOrg2.Addresses.AddNew();
			testAddress2.OA_Address1 = "ADD2";
			testAddress2.OA_Code = "PC2";
			Factory.Save();

			var batch1 = Factory.New<CusWHSOperatorTransactionBatch>();
			batch1.WOB_Batch = "Batch1";
			batch1.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch1.WOB_OA_Warehouse = testAddress1.PK;
			batch1.WOB_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			batch1.WOB_SystemCreateUser = "XXX";
			batch1.WOB_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			batch1.WOB_SystemLastEditUser = "XXX";

			var batch2 = Factory.New<CusWHSOperatorTransactionBatch>();
			batch2.WOB_Batch = "Batch2";
			batch2.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch2.WOB_OA_Warehouse = testAddress3.PK;
			batch2.WOB_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			batch2.WOB_SystemCreateUser = "XXX";
			batch2.WOB_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			batch2.WOB_SystemLastEditUser = "XXX";

			var batch3 = Factory.New<CusWHSOperatorTransactionBatch>();
			batch3.WOB_Batch = "Batch3";
			batch3.WOB_GC_Company = GlbCompany.CurrentCompany.PK;
			batch3.WOB_OA_Warehouse = testAddress2.PK;
			batch3.WOB_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			batch3.WOB_SystemCreateUser = "XXX";
			batch3.WOB_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			batch3.WOB_SystemLastEditUser = "XXX";

			var orgSupplierPart1 = Factory.NewWithValidTestData<Customs.Business.OrgSupplierPart>();
			orgSupplierPart1.OP_PartNum = "ProductCode1";
			Factory.Save();

			var orgSupplierPart2 = Factory.NewWithValidTestData<Customs.Business.OrgSupplierPart>();
			orgSupplierPart2.OP_PartNum = "ProductCode2";
			Factory.Save();

			transaction1 = Factory.New<CusWHSOperatorTransaction>();
			transaction1.WOT_TransactionType = "ORD";
			transaction1.WOT_WOB_CusWHSTransactionBatch = batch1.PK;
			transaction1.WOT_Status = "QUE";
			transaction1.WOT_ExportType = "EXP";
			transaction1.WOT_OwnerReference = "OwnerReference1";
			transaction1.WOT_OH_ProductOwner = testOrg1.PK;
			transaction1.WOT_Quantity = 1;
			transaction1.WOT_IsCustomsControlled = false;
			transaction1.WOT_TotalValue = 2;
			transaction1.WOT_RX_NKCurrency = "GBP";
			transaction1.WOT_RN_NKOrigin = "";
			transaction1.WOT_BatchLineNo = 1;
			transaction1.WOT_TransactionDate = ZDate.BrettsBirthday;
			transaction1.WOT_LineReference = "LineReference1";
			transaction1.WOT_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			transaction1.WOT_SystemCreateUser = "XXX";
			transaction1.WOT_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			transaction1.WOT_SystemLastEditUser = "XXX";
			transaction1.WOT_CustomsEntryNumber = "123";
			transaction1.WOT_OP_Product = orgSupplierPart1.PK;

			transaction2 = Factory.New<CusWHSOperatorTransaction>();
			transaction2.WOT_TransactionType = "REC";
			transaction2.WOT_WOB_CusWHSTransactionBatch = batch2.PK;
			transaction2.WOT_Status = "VAL";
			transaction2.WOT_ExportType = "";
			transaction2.WOT_OwnerReference = "OwnerReference2";
			transaction2.WOT_OH_ProductOwner = testOrg1.PK;
			transaction2.WOT_Quantity = 1;
			transaction2.WOT_IsCustomsControlled = true;
			transaction2.WOT_TotalValue = 2;
			transaction2.WOT_RX_NKCurrency = "GBP";
			transaction2.WOT_RN_NKOrigin = "GB";
			transaction2.WOT_BatchLineNo = 1;
			transaction2.WOT_TransactionDate = ZDate.BrettsBirthday.AddDays(-1);
			transaction2.WOT_LineReference = "LineReference2";
			transaction2.WOT_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			transaction2.WOT_SystemCreateUser = "XXX";
			transaction2.WOT_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			transaction2.WOT_SystemLastEditUser = "XXX";
			transaction2.WOT_CustomsEntryNumber = "456";
			transaction2.WOT_OP_Product = orgSupplierPart2.PK;

			transaction3 = Factory.New<CusWHSOperatorTransaction>();
			transaction3.WOT_TransactionType = "ORD";
			transaction3.WOT_WOB_CusWHSTransactionBatch = batch1.PK;
			transaction3.WOT_Status = "CLS";
			transaction3.WOT_ExportType = "BLN";
			transaction3.WOT_OwnerReference = "OwnerReference2";
			transaction3.WOT_OH_ProductOwner = testOrg1.PK;
			transaction3.WOT_Quantity = 1;
			transaction3.WOT_IsCustomsControlled = false;
			transaction3.WOT_TotalValue = 2;
			transaction3.WOT_RX_NKCurrency = "GBP";
			transaction3.WOT_RN_NKOrigin = "";
			transaction3.WOT_BatchLineNo = 2;
			transaction3.WOT_TransactionDate = ZDate.BrettsBirthday;
			transaction3.WOT_LineReference = "LineReference3";
			transaction3.WOT_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			transaction3.WOT_SystemCreateUser = "XXX";
			transaction3.WOT_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			transaction3.WOT_SystemLastEditUser = "XXX";

			transaction4 = Factory.New<CusWHSOperatorTransaction>();
			transaction4.WOT_TransactionType = "ORD";
			transaction4.WOT_WOB_CusWHSTransactionBatch = batch3.PK;
			transaction4.WOT_Status = "CLS";
			transaction4.WOT_ExportType = "BLN";
			transaction4.WOT_OwnerReference = "OwnerReference4";
			transaction4.WOT_OH_ProductOwner = testOrg2.PK;
			transaction4.WOT_Quantity = 1;
			transaction4.WOT_IsCustomsControlled = false;
			transaction4.WOT_TotalValue = 2;
			transaction4.WOT_RX_NKCurrency = "GBP";
			transaction4.WOT_RN_NKOrigin = "";
			transaction4.WOT_BatchLineNo = 2;
			transaction4.WOT_TransactionDate = ZDate.BrettsBirthday;
			transaction4.WOT_LineReference = "LineReference4";
			transaction4.WOT_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
			transaction4.WOT_SystemCreateUser = "XXX";
			transaction4.WOT_SystemLastEditTimeUtc = ZDateTime.BrettsBirthday;
			transaction4.WOT_SystemLastEditUser = "XXX";

			Factory.Save();
		}

		CusWHSOperatorTransaction transaction1;
		CusWHSOperatorTransaction transaction2;
		CusWHSOperatorTransaction transaction3;
		CusWHSOperatorTransaction transaction4;
		OrgHeader testOrg1;
		OrgHeader testOrg2;
		OrgAddress testAddress1;
		OrgAddress testAddress2;
		OrgAddress testAddress3;
	}
}
