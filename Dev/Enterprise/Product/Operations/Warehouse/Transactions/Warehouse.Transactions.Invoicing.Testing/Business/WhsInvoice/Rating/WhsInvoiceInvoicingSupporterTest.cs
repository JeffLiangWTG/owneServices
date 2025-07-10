using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.Invoicing.Testing;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Invoicing
{
	[TestedType(typeof(WhsInvoiceInvoicingSupporter))]
	public class WhsInvoiceInvoicingSupporterTest : JobInvoicingSupporterTest
	{
		#region TestGetReasonNotToAllowAutoRate

		[TestDate(2009, 1, 1)]
		public void TestGetReasonNotToAllowAutoRate()
		{
			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_OH_Client = Org.PK;
			invoice.ET_WW = Helper.CreateWarehouse("AAAA", "A", 2, 2).PK;
			invoice.ET_StorageFromDate = ZDateTime.Empty;
			invoice.ET_StorageToDate = ZDateTime.Empty;
			var invoiceSupporter = new WhsInvoiceInvoicingSupporter(invoice);
			AssertEquals("There are errors that need to be corrected before this Warehouse Periodic Invoice can be Auto Rated.", invoiceSupporter.GetReasonNotToAllowAutoRate());

			invoice.ET_StorageFromDate = ZDateTime.Invalid;
			invoice.ET_StorageToDate = new ZDateTime(2009, 1, 26);
			AssertEquals("There are errors that need to be corrected before this Warehouse Periodic Invoice can be Auto Rated.", invoiceSupporter.GetReasonNotToAllowAutoRate());

			invoice.ET_StorageFromDate = new ZDateTime(2009, 1, 26);
			invoice.ET_StorageToDate = ZDateTime.Invalid;
			AssertEquals("There are errors that need to be corrected before this Warehouse Periodic Invoice can be Auto Rated.", invoiceSupporter.GetReasonNotToAllowAutoRate());

			using (var mutex = new ZGlobalMutex(MutexIDs.WhsInvoiceAutoRate, invoice.PK.ToString()))
			using (invoice.InvoiceBillingCheckLockSuspender())
			{
				mutex.Lock();
				invoice.ET_StorageFromDate = new ZDateTime(2009, 1, 1);
				invoice.ET_StorageToDate = new ZDateTime(2009, 1, 26);
				AssertNull("When validation is suspended, even if it is locked, it shouldn't get an error!", invoiceSupporter.GetReasonNotToAllowAutoRate());
			}

			using (var mutex = new ZGlobalMutex(MutexIDs.WhsInvoiceAutoRate, invoice.PK.ToString()))
			{
				mutex.Lock();
				AssertEquals("Another user or service is Autorating this Periodic Invoice. Please try again later.", invoiceSupporter.GetReasonNotToAllowAutoRate());
			}

			AssertNull("Should validate with no errors.", invoiceSupporter.GetReasonNotToAllowAutoRate());
		}

		[TestDate(2019, 1, 1)]
		public void TestGetReasonNotToAllowAutoRate_AddsFetchHintsIfSuccessful()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2019, 1, 1), data.Part1, 10m);
			Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, new ZDateTime(2019, 1, 1), "", true);
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", new ZDateTimeOffset(2019, 1, 1), data.Part1, 5m);
			Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);

			new Job.Loader(receive).TryCreate();
			new Job.Loader(order).TryCreate();
			Factory.Save();

			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(2019, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(2019, 1, 31);
			new Job.Loader(invoice).TryCreate();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var invoiceSupporter = new WhsInvoiceInvoicingSupporter(otherFactory.Load<WhsInvoice>(invoice.PK));
			AssertNull("Should validate with no errors.", invoiceSupporter.GetReasonNotToAllowAutoRate());

			AssertEquals("Should be 1 Child Job Header fetch hint for each Additional Job.", 3, otherFactory.ActiveFetchHintsForTable(JobHeaderSchema.Constants.TableName));
			AssertEquals("Should be 2 Fetch Hints for Logs for each Job Header, 1 for the Job Header, 1 for the Job Header's Parent.", 8, otherFactory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName));
			AssertEquals("Should be 1 Charge fetch hint for each Additional Job.", 3, otherFactory.ActiveFetchHintsForTable(JobChargeSchema.Constants.TableName));
		}

		[TestDate(2019, 1, 1)]
		public void TestGetReasonNotToAllowAutoRate_DoesNotAddFetchHintsIfNotSuccessful()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;

			using (WarehouseDataRegistry.Instance.AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2019, 1, 1), data.Part1, 10m);
				Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, new ZDateTime(2019, 1, 1), "", true);
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", new ZDateTimeOffset(2019, 1, 1), data.Part1, 5m);
				Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);

				new Job.Loader(receive).TryCreate();
				new Job.Loader(order).TryCreate();
				Factory.Save();
			}

			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(2019, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(2019, 1, 31);
			new Job.Loader(invoice).TryCreate();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var invoiceInOtherFactory = otherFactory.Load<WhsInvoice>(invoice.PK);
			invoiceInOtherFactory.ET_BillingDate = ZDateTime.Empty; // create validation error

			var invoiceSupporter = new WhsInvoiceInvoicingSupporter(invoiceInOtherFactory);
			AssertEquals("There are errors that need to be corrected before this Warehouse Periodic Invoice can be Auto Rated.", invoiceSupporter.GetReasonNotToAllowAutoRate());

			AssertEquals("No fetch hints should be added.", 0, otherFactory.ActiveFetchHintsForTable(JobHeaderSchema.Constants.TableName));
			AssertEquals("No fetch hints should be added.", 0, otherFactory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName));
			AssertEquals("No fetch hints should be added.", 0, otherFactory.ActiveFetchHintsForTable(JobChargeSchema.Constants.TableName));
		}

		[TestDate(2019, 1, 1)]
		public void TestGetReasonNotToAllowAutoRate_SaveRefreshesFetchHints()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;

			using (WarehouseDataRegistry.Instance.AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2019, 1, 1), data.Part1, 10m);
				Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, new ZDateTime(2019, 1, 1), "", true);
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", new ZDateTimeOffset(2019, 1, 1), data.Part1, 5m);
				Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);

				new Job.Loader(receive).TryCreate();
				new Job.Loader(order).TryCreate();
				Factory.Save();
			}

			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(2019, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(2019, 1, 31);
			new Job.Loader(invoice).TryCreate();

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var invoiceSupporter = new WhsInvoiceInvoicingSupporter(otherFactory.Load<WhsInvoice>(invoice.PK));
			AssertNull("Should validate with no errors.", invoiceSupporter.GetReasonNotToAllowAutoRate());

			AssertEquals("Should be 1 Child Job Header fetch hint for each Additional Job.", 3, otherFactory.ActiveFetchHintsForTable(JobHeaderSchema.Constants.TableName));
			AssertEquals("Should be 2 Fetch Hints for Logs for each Job Header, 1 for the Job Header, 1 for the Job Header's Parent.", 8, otherFactory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName));
			AssertEquals("Should be 1 Charge fetch hint for each Additional Job.", 3, otherFactory.ActiveFetchHintsForTable(JobChargeSchema.Constants.TableName));

			otherFactory.ExecuteAllFetchHints();
			otherFactory.Save();
			AssertEquals("All Fetch Hints should be executed.", 0, otherFactory.ActiveFetchHintsForTable(JobHeaderSchema.Constants.TableName));
			AssertEquals("All Fetch Hints should be executed.", 0, otherFactory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName));
			AssertEquals("All Fetch Hints should be executed.", 0, otherFactory.ActiveFetchHintsForTable(JobChargeSchema.Constants.TableName));

			otherFactory.ClearQueryCache(JobHeaderSchema.Constants.TableName);
			otherFactory.ClearQueryCache(StmALogSchema.Constants.TableName);
			otherFactory.ClearQueryCache(JobChargeSchema.Constants.TableName);
			AssertNull("Should validate with no errors.", invoiceSupporter.GetReasonNotToAllowAutoRate());

			AssertEquals("Should be 1 Child Job Header fetch hint for each Additional Job plus Invoice's Child Job Header.", 4, otherFactory.ActiveFetchHintsForTable(JobHeaderSchema.Constants.TableName));
			AssertEquals("Should be 2 Fetch Hints for Logs for each Job Header, 1 for the Job Header, 1 for the Job Header's Parent.", 8, otherFactory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName));
			AssertEquals("Should be 1 Charge fetch hint for each Additional Job.", 3, otherFactory.ActiveFetchHintsForTable(JobChargeSchema.Constants.TableName));
		}

		[TestDate(2019, 1, 1)]
		public void TestGetReasonNotToAllowAutoRate_NoFetchHintsAddedIfJobNotCreated()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			data.Whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			data.Org1.CompanyData.OB_ARWarehouseRatingPeriod = Constants.StorageCalculationPeriods.Monthly;

			using (WarehouseDataRegistry.Instance.AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture.SetTemporaryValue(Guid.Empty, data.Whs1.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty, true))
			{
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", new ZDateTimeOffset(2019, 1, 1), data.Part1, 10m);
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", new ZDateTimeOffset(2019, 1, 1), data.Part1, 5m);
				Helper.CreatePickNew(finaliseOrders: true, finalisePick: true, pickableDockets: order);
				Factory.Save();
			}

			var invoice = Factory.NewWithValidTestData<WhsInvoice>();
			invoice.ET_OH_Client = data.Org1.PK;
			invoice.ET_WW = data.Whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(2019, 1, 1);
			invoice.ET_StorageToDate = new ZDateTime(2019, 1, 31);

			Factory.Save();

			var otherFactory = new BusinessObjectFactory();
			var invoiceSupporter = new WhsInvoiceInvoicingSupporter(otherFactory.Load<WhsInvoice>(invoice.PK));
			AssertNull("Should validate with no errors.", invoiceSupporter.GetReasonNotToAllowAutoRate());

			AssertEquals("No fetch hints should be added.", 0, otherFactory.ActiveFetchHintsForTable(JobHeaderSchema.Constants.TableName));
			AssertEquals("No fetch hints should be added.", 0, otherFactory.ActiveFetchHintsForTable(StmALogSchema.Constants.TableName));
			AssertEquals("No fetch hints should be added.", 0, otherFactory.ActiveFetchHintsForTable(JobChargeSchema.Constants.TableName));
		}

		#endregion

		#region Implementation

		#region Helper

		public WhsTestHelperFunctionsInvoice Helper => helper ?? (helper = new WhsTestHelperFunctionsInvoice(Factory));
		WhsTestHelperFunctionsInvoice helper;

		#endregion

		#region Org

		OrgHeader Org
		{
			get
			{
				if (org == null)
				{
					org = Factory.New<OrgHeader>();
					org.OH_Code = "TESTORG1";
					org.MainAddress.OA_Address1 = "1 HIGH ST";
					org.CompanyData.OB_ARWarehouseRatingPeriod = Core.Constants.StorageCalculationPeriods.BillingPeriod;
					org.CompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoice = true;
				}
				return org;
			}
		}
		OrgHeader org;

		#endregion

		#region GetNewBusinessObject

		protected override IJobInvoicingPlugIn GetNewBusinessObject()
		{
			WhsInvoice whsInvoice = Factory.NewWithValidTestData<WhsInvoice>();
			return whsInvoice;
		}

		#endregion

		#endregion
	}
}
