using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class WhsJobProfitReportTest : WhsTestCaseWithFactory
	{
		#region TestWhsJobProfitReport

		#region TestWhsJobProfitReport_DateFilter

		#region TestWhsJobProfitReport_StorageDateFilter

		[TestDate(2012, 1, 1)]
		public void TestWhsJobProfitReport_StorageDateFilter()
		{
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("product", client);
			var chargeCode = Helper.CreateChargeCode("WIN", "Receive Jobs Charge Code",
				ChargeCodeGroupList.Codes.WHSInwards, "");
			var periodicInvoice = SetupPeriodicInvoiceWithJobCharge(client, whs, chargeCode);
			Factory.Save();

			var result = ExecuteProcedure(type: "W", storageFromDate: ZDateTime.Today);
			AssertEquals("Storage From Date Filter Set, should return only Periodic Invoice.", 3, result.Count);
			result.ForEach(x => AssertLine(x, periodicInvoice.JobHeader, chargeCode, client));
		}

		#endregion

		#region TestWhsJobProfitReport_JobOpenedDateFilter

		[TestDate(2012, 1, 1)]
		public void TestWhsJobProfitReport_JobOpenedDateFilter()
		{
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("product", client);
			var chargeCode = Helper.CreateChargeCode("WIN", "Receive Jobs Charge Code",
				ChargeCodeGroupList.Codes.WHSInwards, "");
			var receive = SetupReceiveWithJobCharge(client, whs, product, chargeCode, "Receive");
			Factory.Save();

			var result = ExecuteProcedure(type: "W", jobOpenedDate: ZDateTime.Today);
			AssertEquals("Job Opened From Date filter set, should return receive job.", 3, result.Count);
			result.ForEach(x => AssertLine(x, (Job)receive.JobHeader, chargeCode, client));

			result = ExecuteProcedure(type: "W", jobOpenedDate: ZDateTime.Today.AddDays(1));
			AssertEquals("Job Opened From Date filter set, should not return any jobs.", 0, result.Count);
		}

		#endregion

		#region TestWhsJobProfitReport_AdHocFinaliseDate

		[TestDate(2012, 1, 1)]
		public void TestWhsJobProfitReport_AdHocFinaliseDate()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient("Client");
			var today = ZDateTime.Today;
			var chargeCode =
				Helper.CreateChargeCode("WAH", "Charge Code", ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adHocJob = SetupAdHocJobWithJobCharge(client, whs, chargeCode, "ADH1");

			var result = ExecuteProcedure(type: "W", jobFromFinalisedDate: ZDateTime.Today,
				jobToFinalisedDate: ZDateTime.Today.AddDays(1));
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function when inside finalised date range.",
				1, result.Count);
			AssertLine(result[0], (Job)adHocJob.JobHeader, chargeCode, client);
			result = ExecuteProcedure(type: "W", jobFromFinalisedDate: ZDateTime.Today.AddDays(1),
				jobToFinalisedDate: ZDateTime.Today.AddDays(1));
			AssertEquals("Ad Hoc Service Jobs finalised date out of range.", 0, result.Count);
			result = ExecuteProcedure(type: "W", jobFromFinalisedDate: ZDateTime.Today.AddDays(-2),
				jobToFinalisedDate: ZDateTime.Today.AddDays(-1));
			AssertEquals("Ad Hoc Service Jobs finalised date out of range.", 0, result.Count);
		}

		#endregion

		#endregion

		#region TestWhsJobProfitReport_Docket

		public void TestWhsJobProfitReport_Docket()
		{
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("product", client);
			var chargeCode = Helper.CreateChargeCode("WIN", "Charge Code", ChargeCodeGroupList.Codes.WHSInwards, "");
			var receive = SetupReceiveWithJobCharge(client, whs, product, chargeCode, "Receive1");
			Factory.Save();

			var result = ExecuteProcedure();
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function without job type supplied.", 3,
				result.Count);
			result.ForEach(x => AssertLine(x, (Job)receive.JobHeader, chargeCode, client));
			result = ExecuteProcedure(type: "W");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with Warehouse job type specified.",
				3, result.Count);
			result.ForEach(x => AssertLine(x, (Job)receive.JobHeader, chargeCode, client));
			result = ExecuteProcedure(type: "L");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with CFS job type specified.", 0,
				result.Count);
			result = ExecuteProcedure(type: "M");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with MAWB Stock job type specified.",
				0, result.Count);
			result = ExecuteProcedure(type: "C");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with Local Cartage job type specified.",
				0, result.Count);
		}

		#endregion

		#region TestWhsJobProfitReport_PeriodicInvoice

		public void TestWhsJobProfitReport_PeriodicInvoice()
		{
			var whs = Helper.CreateWarehouse("Warehouse", "A", 4, 4);
			var client = Helper.CreateClient("Client1");
			var chargeCode = Helper.CreateChargeCode("WIN", "Charge Code", ChargeCodeGroupList.Codes.WHSInwards, "");
			Factory.Save();
			var periodicInvoice = SetupPeriodicInvoiceWithJobCharge(client, whs, chargeCode);

			var result = ExecuteProcedure();
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function without job type supplied.", 1,
				result.Count);
			result = ExecuteProcedure(type: "W");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with Warehouse job type specified.",
				1, result.Count);
			AssertLine(result[0], periodicInvoice.JobHeader, chargeCode, client);
			result = ExecuteProcedure(type: "L");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with CFS job type specified.", 0,
				result.Count);
			result = ExecuteProcedure(type: "M");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with MAWB Stock job type specified.",
				0, result.Count);
			result = ExecuteProcedure(type: "C");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with Local Cartage job type specified.",
				0, result.Count);
		}

		#endregion

		#region TestWhsJobProfitReport_AdHocServiceJobs

		#region TestWhsJobProfitReport_AdHocServiceJob_ValidWarehouse

		public void TestWhsJobProfitReport_AdHocServiceJob_ValidWarehouse()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient("Client");
			var today = ZDateTime.Today;
			var chargeCode =
				Helper.CreateChargeCode("WAH", "Charge Code", ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adHocJob = SetupAdHocJobWithJobCharge(client, whs, chargeCode, "ADH1");

			var result = ExecuteProcedure();
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function without job type supplied.", 1,
				result.Count);
			result = ExecuteProcedure(type: "W");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with Warehouse job type specified.",
				1, result.Count);
			AssertLine(result[0], (Job)adHocJob.JobHeader, chargeCode, client);
			result = ExecuteProcedure(type: "L");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with CFS job type specified.", 0,
				result.Count);
			result = ExecuteProcedure(type: "M");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with MAWB Stock job type specified.",
				0, result.Count);
			result = ExecuteProcedure(type: "C");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with Local Cartage job type specified.",
				0, result.Count);
		}

		#endregion

		#region TestWhsJobProfitReport_AdHocServiceJob_Filtering

		#region TestView_AdhocServiceJobs_DoesNotFilterWarehouseType

		public void TestView_AdhocServiceJobs_DoesNotFilterWarehouseType()
		{
			var whs1 = Helper.CreateWarehouse("Warehouse1");
			whs1.WW_WarehouseType = WarehouseTypes.Codes.FreeTradeZone;
			var whs2 = Helper.CreateWarehouse("Warehouse2");
			whs2.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			var whs3 = Helper.CreateWarehouse("Warehouse3");
			whs3.WW_WarehouseType = WarehouseTypes.Codes.Transit;
			var whs4 = Helper.CreateWarehouse("Warehouse4");
			whs4.WW_WarehouseType = WarehouseTypes.Codes.Product;
			var client = Helper.CreateClient("Client");

			var chargeCode =
				Helper.CreateChargeCode("WAH", "Charge Code", ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adHocJob1 = SetupAdHocJobWithJobCharge(client, whs1, chargeCode, "ADH1");
			var adHocJob2 = SetupAdHocJobWithJobCharge(client, whs2, chargeCode, "ADH2");
			var adHocJob3 = SetupAdHocJobWithJobCharge(client, whs3, chargeCode, "ADH3");
			var adHocJob4 = SetupAdHocJobWithJobCharge(client, whs4, chargeCode, "ADH4");
			Factory.Save();

			var result = ExecuteProcedure(type: "W");
			AssertEquals("No warehouse types are filtered.", 36, result.Count);

			result.Where(d => (ZString)d["WW_WarehouseName"] == "Warehouse1").ForEach(x => AssertLine(x, (Job)adHocJob1.JobHeader, chargeCode, client));
			result.Where(d => (ZString)d["WW_WarehouseName"] == "Warehouse2").ForEach(x => AssertLine(x, (Job)adHocJob2.JobHeader, chargeCode, client));
			result.Where(d => (ZString)d["WW_WarehouseName"] == "Warehouse3").ForEach(x => AssertLine(x, (Job)adHocJob3.JobHeader, chargeCode, client));
			result.Where(d => (ZString)d["WW_WarehouseName"] == "Warehouse4").ForEach(x => AssertLine(x, (Job)adHocJob4.JobHeader, chargeCode, client));
		}

		#endregion

		#region TestView_AdhocServiceJobs_DoesNotFilterVirtual

		public void TestView_AdhocServiceJobs_DoesNotFilterVirtual()
		{
			var whs1 = Helper.CreateWarehouse("Warehouse1");
			whs1.WW_IsVirtualWarehouse = true;
			var whs2 = Helper.CreateWarehouse("Warehouse2");
			whs2.WW_IsVirtualWarehouse = false;
			var client = Helper.CreateClient("Client");
			var chargeCode =
				Helper.CreateChargeCode("WAH", "Charge Code", ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adHocJob1 = SetupAdHocJobWithJobCharge(client, whs1, chargeCode, "ADH1");
			var adHocJob2 = SetupAdHocJobWithJobCharge(client, whs2, chargeCode, "ADH2");
			Factory.Save();

			var result = ExecuteProcedure(type: "W");
			AssertEquals("All Warehouses are valid for Ad Hoc Service Jobs.", 10, result.Count);

			result.Where(d => (ZString)d["WW_WarehouseName"] == "Warehouse1").ForEach(x => AssertLine(x, (Job)adHocJob1.JobHeader, chargeCode, client));
			result.Where(d => (ZString)d["WW_WarehouseName"] == "Warehouse2").ForEach(x => AssertLine(x, (Job)adHocJob2.JobHeader, chargeCode, client));
		}

		#endregion

		#region TestView_AdhocServiceJobs_DoesNotFilterInactive

		public void TestView_AdhocServiceJobs_DoesNotFilterInactive()
		{
			var whs1 = Helper.CreateWarehouse("Warehouse1");
			whs1.WW_IsActive = true;
			var whs2 = Helper.CreateWarehouse("Warehouse2");
			whs2.WW_IsActive = false;
			var client = Helper.CreateClient("Client");
			var chargeCode =
				Helper.CreateChargeCode("WAH", "Charge Code", ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adHocJob1 = SetupAdHocJobWithJobCharge(client, whs1, chargeCode, "ADH1");
			var adHocJob2 = SetupAdHocJobWithJobCharge(client, whs2, chargeCode, "ADH2");
			Factory.Save();

			var result = ExecuteProcedure(type: "W");
			AssertEquals("All Warehouses are valid for Ad Hoc Service Jobs.", 10, result.Count);

			result.Where(d => (ZString)d["WW_WarehouseName"] == "Warehouse1").ForEach(x => AssertLine(x, (Job)adHocJob1.JobHeader, chargeCode, client));
			result.Where(d => (ZString)d["WW_WarehouseName"] == "Warehouse2").ForEach(x => AssertLine(x, (Job)adHocJob2.JobHeader, chargeCode, client));
		}

		#endregion

		#endregion

		#region TestWhsJobProfitReport_AdHocServiceJobs_ExcludesNonAdHocWorkItems

		public void TestWhsJobProfitReport_AdHocServiceJobs_ExcludesNonAdHocWorkItems()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient("Client");
			var chargeCode = Helper.CreateChargeCode("WAH", "Ad Hoc Service Jobs Charge Code",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adHocJob = Helper.CreateWhsAdHocServiceJob(whs, client, ZDateTime.Today, "ADH", true);
			var adHocJobHeader = (Job)adHocJob.JobHeader;

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_WorkItemType = "UDF";
			var workItemJobHeader = new JobHeader.Loader(workItem).TryLoadOrCreate();
			workItemJobHeader.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			Factory.Save();

			adHocJobHeader.JH_JobLocalReference = "adhocHeader";
			workItemJobHeader.JH_JobLocalReference = "workItemHeader";

			Helper.CreateJobCharge(adHocJobHeader, chargeCode, 10m);
			Helper.CreateJobCharge((Job)workItemJobHeader, chargeCode, 10m);
			Factory.Save();

			var result = ExecuteProcedure();
			AssertEquals("Both Work Items should be returned by Job Profit Summary Function without job type supplied.",
				2, result.Count);
			AssertCollectionContains(
				"Should find ad hoc service job when running Job Profit Summary Function without job type supplied.",
				adHocJobHeader.JH_JobLocalReference, result.Select(w => w["JH_JobLocalReference"]));
			AssertCollectionContains(
				"Should find other WorkItem  when running Job Profit Summary Function without job type supplied.",
				workItemJobHeader.JH_JobLocalReference, result.Select(w => w["JH_JobLocalReference"]));
			result = ExecuteProcedure(type: "W");
			AssertEquals(
				"Only Ad Hoc Service Jobs should be returned by Job Profit Summary Function with Warheouse job type selected.",
				1, result.Count);
			AssertCollectionContains(
				"Should find ad hoc service job  when running Job Profit Summary Function with Warehouse job type supplied.",
				adHocJobHeader.JH_JobLocalReference, result.Select(w => w["JH_JobLocalReference"]));
			AssertCollectionNotContains(
				"Should not find other WorkItem  when running Job Profit Summary Function with Warehouse job type supplied.",
				workItemJobHeader.JH_JobLocalReference, result.Select(w => w["JH_JobLocalReference"]));
		}

		#endregion

		#endregion

		#region TestWhsJobProfitReport_ClientList

		public void TestWhsJobProfitReport_ClientList()
		{
			var whs = Helper.CreateWarehouse("Whs", "A", 4, 4);
			var client1 = Helper.CreateClient("Client1");
			var client2 = Helper.CreateClient("Client2");
			var client3 = Helper.CreateClient("Client3");
			var client4 = Helper.CreateClient("Client4");
			var product = Helper.CreateProduct("product", client1);
			var chargeCode = Helper.CreateChargeCode("WIN", "Charge Code", ChargeCodeGroupList.Codes.WHSInwards, "");

			var receive = SetupReceiveWithJobCharge(client1, whs, product, chargeCode, "Receive1");
			var periodicInvoice = SetupPeriodicInvoiceWithJobCharge(client2, whs, chargeCode);
			var adHocJob = SetupAdHocJobWithJobCharge(client3, whs, chargeCode, "ADH1");
			Factory.Save();

			var result = ExecuteProcedure(type: "W", clientPK: client1.PK);
			AssertEquals("Results should only contain Jobs from Client1.", 11, result.Count);
			result.ForEach(x => AssertLine(x, (Job)receive.JobHeader, chargeCode, client1));
			result = ExecuteProcedure(type: "W", clientPK: client2.PK);
			AssertEquals("Results should only contain Jobs from Client2.", 7, result.Count);
			result.ForEach(x => AssertLine(x, periodicInvoice.JobHeader, chargeCode, client2));
			result = ExecuteProcedure(type: "W", clientPK: client3.PK);
			AssertEquals("Results should only contain Jobs from Client3.", 3, result.Count);
			result.ForEach(x => AssertLine(x, (Job)adHocJob.JobHeader, chargeCode, client3));
			result = ExecuteProcedure(type: "W", clientPK: client4.PK);
			AssertEquals("Results should contain no jobs.", 0, result.Count);
		}

		#endregion

		#region TestWhsJobProfitReport_WarehouseID

		public void TestWhsJobProfitReport_WarehouseID()
		{
			var whs1 = Helper.CreateWarehouse("Whs1", "A", 4, 4);
			var whs2 = Helper.CreateWarehouse("Whs2", "A", 4, 4);
			var whs3 = Helper.CreateWarehouse("Whs3", "A", 4, 4);
			var whs4 = Helper.CreateWarehouse("Whs4", "A", 4, 4);
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("product", client);
			var chargeCode = Helper.CreateChargeCode("WIN", "Receive Jobs Charge Code",
				ChargeCodeGroupList.Codes.WHSInwards, "");

			var receive = SetupReceiveWithJobCharge(client, whs1, product, chargeCode, "Receive1");
			var periodicInvoice = SetupPeriodicInvoiceWithJobCharge(client, whs2, chargeCode);
			var adHocJob = SetupAdHocJobWithJobCharge(client, whs3, chargeCode, "ADH1");
			Factory.Save();

			var result = ExecuteProcedure(type: "W", warehousePK: whs1.PK);
			AssertEquals("Results should only contain Jobs from Warehouse 11.", 11, result.Count);
			result.ForEach(x => AssertLine(x, (Job)receive.JobHeader, chargeCode, client));
			result = ExecuteProcedure(type: "W", warehousePK: whs2.PK);
			AssertEquals("Results should only contain Jobs from Warehouse 7.", 7, result.Count);
			result.ForEach(x => AssertLine(x, periodicInvoice.JobHeader, chargeCode, client));
			result = ExecuteProcedure(type: "W", warehousePK: whs3.PK);
			AssertEquals("Results should only contain Jobs from Warehouse 3.", 3, result.Count);
			result.ForEach(x => AssertLine(x, (Job)adHocJob.JobHeader, chargeCode, client));
			result = ExecuteProcedure(type: "W", warehousePK: whs4.PK);
			AssertEquals("Results should contain no jobs.", 0, result.Count);
		}

		#endregion

		#endregion

		#region implementation

		#region AssertLine

		void AssertLine(DynamicBusinessObject result, Job expectedJobHeader, AccChargeCode expectedCode,
			OrgHeader localClient)
		{
			AssertEquals("JH_GC", expectedJobHeader.JH_GC, result["JH_GC"]);
			AssertEquals("JH_JobNum", expectedJobHeader.JH_JobNum, result["JH_JobNum"]);
			AssertEquals("JH_JobLocalReference", expectedJobHeader.JH_JobLocalReference,
				result["JH_JobLocalReference"]);
			AssertEquals("JH_Status", expectedJobHeader.JH_Status, result["JH_Status"]);
			AssertEquals("JH_BranchCode", GlbBranch.CurrentBranch.GB_Code, result["JH_BranchCode"]);
			AssertEquals("JH_DepartmentCode", GlbDepartment.CurrentDepartment.GE_Code, result["JH_DepartmentCode"]);
			AssertEquals("JH_OperatorInitials", GlbStaff.CurrentUser.GS_Code, result["JH_OperatorInitials"]);
			AssertEquals("JH_SalesRepInitials", ZString.Empty, result["JH_SalesRepInitials"]);
			AssertEquals("JH_LocalClientCode", localClient.OH_Code, result["JH_LocalClientCode"]);
			AssertEquals("JH_LocalClientName", localClient.OH_FullName, result["JH_LocalClientName"]);
			AssertEquals("JH_OverseasAgentCode", "", result["JH_OverseasAgentCode"]);
			AssertEquals("JH_FullName", "", result["JH_FullName"]);
			AssertEquals("AC_Code", expectedCode.AC_Code, result["AC_Code"]);
			AssertEquals("AC_ChargeGroup", expectedCode.AC_ChargeGroup, result["AC_ChargeGroup"]);
		}

		#endregion

		#region ExecuteProcedure

		DynamicBusinessObjectCollection ExecuteProcedure(ZString? type = null,
			ZDateTime? storageFromDate = null, ZDateTime? jobOpenedDate = null, ZDateTime? jobFromFinalisedDate = null,
			ZDateTime? jobToFinalisedDate = null, ZGuid? clientPK = null, ZGuid? warehousePK = null)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			string sql = @"EXEC Report_AllJobProfitDetail
@SelectJobTypeDetails = 'Y',
@JH_GC = @CurrentCompany,
@JH_FromRevenueRecognizedDate = '1900-01-01 00:00:00' ,
@JH_ToRevenueRecognizedDate = '2079-06-06 23:59:29',
@AL_FromDate = '1900-01-01 00:00:00',
@AL_ToDate = '2079-06-06 23:59:29',
@CurrentCountry = 'AU'
";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@CurrentCompany", GlbCompany.CurrentCompany.PK, JobHeaderSchema.JH_GC);
			if (type.HasValue)
			{
				sql += ", @JobType = @type";
				sqlParams.Add("@type", type.Value, WhsDocketSchema.WD_ExternalReference);
			}

			if (storageFromDate.HasValue)
			{
				sql += ", @Whs_FromStorageStart = @StorageFromDate";
				sqlParams.Add("@StorageFromDate", storageFromDate.Value.ToISO8601String(),
					WhsDocketSchema.WD_ExternalReference);
			}

			if (jobOpenedDate.HasValue)
			{
				sql += ", @JH_FromCreatedDate = @jobOpenedDate";
				sqlParams.Add("@jobOpenedDate", jobOpenedDate.Value.ToISO8601String(),
					WhsDocketSchema.WD_ExternalReference);
			}

			if (jobFromFinalisedDate.HasValue)
			{
				sql += ", @Whs_FromFinalised = @jobFromFinalisedDate";
				sqlParams.Add("@jobFromFinalisedDate", jobFromFinalisedDate.Value.ToISO8601String(),
					WhsDocketSchema.WD_ExternalReference);
			}

			if (jobToFinalisedDate.HasValue)
			{
				sql += ", @Whs_ToFinalised = @jobToFinalisedDate";
				sqlParams.Add("@jobToFinalisedDate", jobToFinalisedDate.Value.ToISO8601String(),
					WhsDocketSchema.WD_ExternalReference);
			}

			if (clientPK.HasValue)
			{
				sql += ", @Whs_OH_ClientPKList = @Client";
				sqlParams.Add("@Client", "'" + clientPK.Value.ToString() + "'", WhsDocketSchema.WD_ExternalReference);
			}

			if (warehousePK.HasValue)
			{
				sql += ", @Whs_WarehouseID = @Warehouse";
				sqlParams.Add("@Warehouse", warehousePK.Value, WhsDocketSchema.WD_WW_Whs);
			}

			result.Load(sql, sqlParams);
			return result;
		}

		#endregion

		#region SetupJobs

		#region SetupReceiveWithJobCharge

		WhsReceive SetupReceiveWithJobCharge(OrgHeader client, WhsWarehouse whs, OrgSupplierPart product,
			AccChargeCode chargeCode, ZString code)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, code, product, 10m, true, true);
			var receiveJob = Helper.CreateRatingJob(receive);
			receiveJob.JH_GB = GlbBranch.CurrentBranch.PK;
			receiveJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Helper.CreateJobCharge(receiveJob, chargeCode, 10m);
			Factory.Save();

			return receive;
		}

		#endregion

		#region SetupPeriodicInvoiceWithJobCharge

		WhsInvoice SetupPeriodicInvoiceWithJobCharge(OrgHeader client, WhsWarehouse whs, AccChargeCode chargeCode)
		{
			var periodicInvoice =
				(WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Today, ZDateTime.Today.AddMonths(1));
			Factory.Save();

			var jobHeader = Helper.CreateRatingJob(periodicInvoice, "ET");
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.LocalChargesPK = client.PK;
			Helper.CreateJobCharge(jobHeader, chargeCode, 100m);
			Factory.Save();

			return periodicInvoice;
		}

		#endregion

		#region SetupAdHocJobWithJobCharge

		WhsAdHocServiceJob SetupAdHocJobWithJobCharge(OrgHeader client, WhsWarehouse whs, AccChargeCode chargeCode,
			ZString code)
		{
			var adHocJob = Helper.CreateWhsAdHocServiceJob(whs, client, ZDateTime.Today, code, true);
			var adHocJobHeader = (Job)adHocJob.JobHeader;
			adHocJobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			adHocJobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			Helper.CreateJobCharge(adHocJobHeader, chargeCode, 10m);
			Factory.Save();
			return adHocJob;
		}

		#endregion

		#endregion

		#endregion
	}
}
