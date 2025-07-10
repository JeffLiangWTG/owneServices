using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class Report_WarehouseJobProfitSummaryTest : WhsTestCaseWithFactory
	{
		#region TestWarehouseJobProfitSummaryFunction

		#region TestWarehouseJobProfitSummaryFunction_Dockets

		#region TestWarehouseJobProfitSummaryFunction_DifferentWarehouses

		public void TestWarehouseJobProfitSummaryFunction_DifferentWarehouses()
		{
			var whs1 = Helper.CreateWarehouse("Warehouse1", "A", 4, 4);
			var whs2 = Helper.CreateWarehouse("Warehouse2", "A", 4, 4);
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("Product", client);
			var chargeCode =
				Helper.CreateChargeCode("WIN", "Receive Charge Code", ChargeCodeGroupList.Codes.WHSInwards, "");
			var receive1 = SetupReceiveWithJobCharge(client, whs1, product, chargeCode, "receive1", 15m);
			var receive2 = SetupReceiveWithJobCharge(client, whs2, product, chargeCode, "receive2", 20m);
			Factory.Save();

			var result = LoadFunction();
			AssertEquals("Dockets from different warehouses should be returned by Job Profit Summary Function.", 2,
				result.Count);
			AssertLine(result[0], (Job)receive1.JobHeader, chargeCode, client, whs1, true, receive1.WD_FinalisedDate,
				ZDateTime.Empty, ZDateTime.Empty, "WD", 15m);
			AssertLine(result[1], (Job)receive2.JobHeader, chargeCode, client, whs2, true, receive1.WD_FinalisedDate,
				ZDateTime.Empty, ZDateTime.Empty, "WD", 20m);
		}

		#endregion

		#region TestWarehouseJobProfitSummaryFunction_DifferentClients

		public void TestWarehouseJobProfitSummaryFunction_DifferentClients()
		{
			var whs = Helper.CreateWarehouse("Warehouse", "A", 4, 4);
			var client1 = Helper.CreateClient("Client1");
			var client2 = Helper.CreateClient("Client2");
			var product = Helper.CreateProduct("Product", client1);
			Helper.CreateProductClientRelationShip(client2, product);
			var chargeCode =
				Helper.CreateChargeCode("WIN", "Receive Charge Code", ChargeCodeGroupList.Codes.WHSInwards, "");
			var receive1 = SetupReceiveWithJobCharge(client1, whs, product, chargeCode, "receive1", 10m);
			var receive2 = SetupReceiveWithJobCharge(client2, whs, product, chargeCode, "receive2", 20m);
			Factory.Save();

			var result = LoadFunction();
			AssertEquals("Dockets from different clients should be returned by Job Profit Summary Function.", 2,
				result.Count);
			AssertLine(result[0], (Job)receive1.JobHeader, chargeCode, client1, whs, true, receive1.WD_FinalisedDate,
				ZDateTime.Empty, ZDateTime.Empty, "WD", 10m);
			AssertLine(result[1], (Job)receive2.JobHeader, chargeCode, client2, whs, true, receive1.WD_FinalisedDate,
				ZDateTime.Empty, ZDateTime.Empty, "WD", 20m);
		}

		#endregion

		#endregion

		#region TestWarehouseJobProfitSummaryFunction_PeriodicInvoice

		public void TestWarehouseJobProfitSummaryFunction_PeriodicInvoice()
		{
			var whs = Helper.CreateWarehouse("Warehouse", "A", 4, 4);
			var client = Helper.CreateClient("Client1");
			var chargeCode = Helper.CreateChargeCode("WIN", "Charge Code", ChargeCodeGroupList.Codes.WHSInwards, "");
			var periodicInvoice =
				(WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Today, ZDateTime.Today.AddDays(1));
			Factory.Save();

			var job = Helper.CreateRatingJob(periodicInvoice);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = client.PK;
			Helper.CreateJobCharge(job, chargeCode, 100m);
			Factory.Save();

			var result = LoadFunction();
			AssertLine(result[0], job, chargeCode, client, whs, false, ZDateTimeOffset.Empty,
				periodicInvoice.ET_StorageFromDate, periodicInvoice.ET_StorageToDate, "ET", 100m);
		}

		#endregion

		#region TestWarehouseJobProfitSummaryFunction_AdHocServiceJobs

		public void TestWarehouseJobProfitSummaryFunction_AdHocServiceJobs()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient("Client");
			var today = ZDateTime.Today;
			var adHocChargeCode = Helper.CreateChargeCode("WAH", "Ad Hoc Service Jobs Charge Code",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");

			var adHocJob = Helper.CreateWhsAdHocServiceJob(whs, client, today, "ADH", true);
			var adHocJobHeader = (Job)adHocJob.JobHeader;
			adHocJobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			adHocJobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Helper.CreateJobCharge(adHocJobHeader, adHocChargeCode, 10m, adHocJob.WSJ_OH_Client);
			Factory.Save();

			var result = LoadFunction();
			AssertEquals("Ad Hoc Service Jobs should be returned by Job Profit Summary Function.", 1, result.Count);
			AssertLine(result[0], adHocJobHeader, adHocChargeCode, client, whs, true, new ZDateTimeOffset(adHocJob.WSJ_BillingDate),
				ZDateTime.Empty, ZDateTime.Empty, "ADH", 10m);
		}

		#endregion

		#region TestWarehouseJobProfitSummaryFunction_VASOrder

		public void TestWarehouseJobProfitSummaryFunction_VASOrder()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("Product", client);
			var vasOrder = CreateFinalisedVASOrder(whs, client, product);
			var chargeCode = Helper.CreateChargeCode("WAH", "VASOrder Service",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");

			Helper.CreateRatingJob(vasOrder, WhsVASOrderSchema.Constants.Prefix);
			var jobHeader = (Job)vasOrder.Job;
			jobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader.LocalChargesPK = client.PK;
			Helper.CreateJobCharge(jobHeader, chargeCode, 10m, client.PK);
			Factory.Save();

			var result = LoadFunction();
			AssertEquals("vas Order should be returned by Job Profit Summary Function.", 1, result.Count);
			AssertLine(result[0], jobHeader, chargeCode, client, whs, true, vasOrder.GetOrCreateInitialTransfer(Notify).WD_FinalisedDate, ZDateTime.Empty,
				ZDateTime.Empty, "WVO", 10m);
		}

		WhsVASOrder CreateFinalisedVASOrder(WhsWarehouse whs, OrgHeader client, OrgSupplierPart product)
		{
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			var vasOrder = Helper.CreateWhsVASOrder(serviceArea, client);
			Helper.CreateWhsVASOrderLine(vasOrder, product, 1m);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 1m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(initialTransfer);

			vasOrder.MarkVASOrderAsCompleted(Notify);
			Factory.Save();
			AssertEquals("Precondition: VAS Order is complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

			vasOrder.WVO_FinalizedTimeUtc = ZDateTime.UtcNow;
			AssertEquals("Precondition: VAS Order is finalised.", true, vasOrder.IsFinalised);
			return vasOrder;
		}

		#endregion

		#endregion

		#region implementation

		#region AssertLine

		void AssertLine(DynamicBusinessObject result, Job expectedJobHeader, AccChargeCode expectedChargeCode,
			OrgHeader localClient, WhsWarehouse whs, bool isFinalised, ZDateTimeOffset finalisedDate, ZDateTime storageFrom,
			ZDateTime storageTo, ZString type, ZDecimal expectedProfit)
		{
			AssertEquals("JH_ParentID", expectedJobHeader.JH_ParentID, result["JH_ParentID"]);
			AssertEquals("JH_JobNum", expectedJobHeader.JH_JobNum, result["JH_JobNum"]);
			AssertEquals("JH_JobLocalReference", expectedJobHeader.JH_JobLocalReference,
				result["JH_JobLocalReference"]);
			AssertEquals("JH_Status", expectedJobHeader.JH_Status, result["JH_Status"]);
			AssertEquals("JH_JobOpened", expectedJobHeader.JH_A_JOP.Date, ((ZDateTime)result["JH_JobOpened"]).Date);
			AssertEquals("JH_JobClosed", expectedJobHeader.JH_A_JCL.Date, ((ZDateTime)result["JH_JobClosed"]).Date);
			AssertEquals("JH_BranchCode", GlbBranch.CurrentBranch.GB_Code, result["JH_BranchCode"]);
			AssertEquals("JH_BranchCodePK", GlbBranch.CurrentBranch.PK, result["JH_BranchCodePK"]);
			AssertEquals("JobBranchManagementCode", GlbBranch.CurrentBranch.GB_AccountingGroupCode,
				result["JobBranchManagementCode"]);
			AssertEquals("JH_DepartmentCode", GlbDepartment.CurrentDepartment.GE_Code, result["JH_DepartmentCode"]);
			AssertEquals("JH_DepartmentCodePK", GlbDepartment.CurrentDepartment.PK, result["JH_DepartmentCodePK"]);
			AssertEquals("JH_OperatorInitials", GlbStaff.CurrentUser.GS_Code, result["JH_OperatorInitials"]);
			AssertEquals("JH_SalesRepInitials", "", result["JH_SalesRepInitials"]);
			AssertEquals("JH_LocalClientCode", localClient.OH_Code, result["JH_LocalClientCode"]);
			AssertEquals("JH_LocalClientName", localClient.OH_FullName, result["JH_LocalClientName"]);
			AssertEquals("JH_OH_LocalCharges", localClient.PK, result["JH_OH_LocalCharges"]);
			AssertEquals("WW_WarehouseName", whs.WW_WarehouseName, result["WW_WarehouseName"]);
			AssertEquals("WD_FinalisedDate", finalisedDate.Date, ((ZDateTimeOffset)result["WD_FinalisedDate"]).Date);
			AssertEquals("ET_StorageFromDate", storageFrom.Date, ((ZDateTime)result["ET_StorageFromDate"]).Date);
			AssertEquals("ET_StorageToDate", storageTo.Date, ((ZDateTime)result["ET_StorageToDate"]).Date);
			AssertEquals("WD_NotFinalised", isFinalised ? "N" : "Y", result["WD_NotFinalised"]);
			AssertEquals("WD_OH_Client", localClient.PK, result["WD_OH_Client"]);
			AssertEquals("WD_WW_Whs", whs.PK, result["WD_WW_Whs"]);
			AssertEquals("Type", type, result["Type"]);
			AssertEquals("RecognitionDateList", "IMM", result["RecognitionDateList"]);
			AssertEquals("JH_Profit", expectedProfit, result["JH_Profit"]);
		}

		#endregion

		#region LoadFunction

		DynamicBusinessObjectCollection LoadFunction()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			string sql = @"SELECT *
FROM
Report_WarehouseJobProfitSummary(
@client,
'1900-01-01 00:00:00',
'2079-06-06 23:59:29',
'',
'',
'',
'',
null,
null,
null,
'',
'',
'',
'',
'1900-01-01 00:00:00',
'2079-06-06 23:59:29'
)
ORDER BY
JH_JobNum
";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@client", GlbCompany.CurrentCompany.PK, GlbCompanySchema.PK);

			result.Load(sql, sqlParams);
			return result;
		}

		#endregion

		#region SetupReceiveWithJobCharge

		WhsReceive SetupReceiveWithJobCharge(OrgHeader client, WhsWarehouse whs, OrgSupplierPart product,
			AccChargeCode chargeCode, ZString code, ZDecimal sellAmount)
		{
			var receive = Helper.CreateWhsReceiveWithInventory(client, whs, code, product, 10m, true, true);
			var receiveJob = Helper.CreateRatingJob(receive);
			receiveJob.JH_GB = GlbBranch.CurrentBranch.PK;
			receiveJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Helper.CreateJobCharge(receiveJob, chargeCode, sellAmount);
			Factory.Save();

			return receive;
		}

		#endregion

		#endregion
	}
}
