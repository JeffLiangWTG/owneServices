using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class Report_WarehouseJobProfitChargeTest : WhsTestCaseWithFactory
	{
		#region TestWarehouseJobProfitChargeFunction

		#region TestWarehouseJobProfitChargeFunctionDifferentWarehouses

		public void TestWarehouseJobProfitChargeFunctionDifferentWarehouses()
		{
			var whs1 = Helper.CreateWarehouse("Warehouse1", "A", 4, 4);
			var whs2 = Helper.CreateWarehouse("Warehouse2", "A", 4, 4);
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("Product", client);
			var chargeCode =
				Helper.CreateChargeCode("WIN", "Receive Charge Code", ChargeCodeGroupList.Codes.WHSInwards, "");
			var receive1 = SetupReceiveWithJobCharge(client, whs1, product, chargeCode, "receive1");
			var receive2 = SetupReceiveWithJobCharge(client, whs2, product, chargeCode, "receive2");
			Factory.Save();

			var result = LoadFunction();
			AssertEquals("All warehouse jobs should appear on report.", 2, result.Count);
			AssertLine(result[0], (Job)receive1.JobHeader, chargeCode, client, whs1, true, receive1.WD_FinalisedDate,
				ZDateTime.Empty, ZDateTime.Empty, "WD");
			AssertLine(result[1], (Job)receive2.JobHeader, chargeCode, client, whs2, true, receive2.WD_FinalisedDate,
				ZDateTime.Empty, ZDateTime.Empty, "WD");
		}

		#endregion

		#region TestWarehouseJobProfitChargeFunctionDifferentClients

		public void TestWarehouseJobProfitChargeFunctionDifferentClients()
		{
			var whs = Helper.CreateWarehouse("Warehouse", "A", 4, 4);
			var client1 = Helper.CreateClient("Client1");
			var client2 = Helper.CreateClient("Client2");
			var product = Helper.CreateProduct("Product", client1);
			Helper.CreateProductClientRelationShip(client2, product);
			var chargeCode =
				Helper.CreateChargeCode("WIN", "Receive Charge Code", ChargeCodeGroupList.Codes.WHSInwards, "");
			var receive1 = SetupReceiveWithJobCharge(client1, whs, product, chargeCode, "receive1");
			var receive2 = SetupReceiveWithJobCharge(client2, whs, product, chargeCode, "receive2");
			Factory.Save();

			var result = LoadFunction();
			AssertEquals("All warehouse jobs should appear on report.", 2, result.Count);
			AssertLine(result[0], (Job)receive1.JobHeader, chargeCode, client1, whs, true, receive1.WD_FinalisedDate,
				ZDateTime.Empty, ZDateTime.Empty, "WD");
			AssertLine(result[1], (Job)receive2.JobHeader, chargeCode, client2, whs, true, receive1.WD_FinalisedDate,
				ZDateTime.Empty, ZDateTime.Empty, "WD");
		}

		#endregion

		#region TestWarehouseJobProfitChargeFunctionPeriodicInvoice

		public void TestWarehouseJobProfitChargeFunctionPeriodicInvoice()
		{
			var whs = Helper.CreateWarehouse("Warehouse", "A", 4, 4);
			var client = Helper.CreateClient("Client1");
			var chargeCode = Helper.CreateChargeCode("WIN", "Charge Code", ChargeCodeGroupList.Codes.WHSInwards, "");
			var periodicInvoice =
				(WhsInvoice)Helper.CreateWhsInvoice(whs.PK, client.PK, ZDateTime.Today, ZDateTime.Today.AddDays(1));
			Factory.Save();

			var job1 = Helper.CreateRatingJob(periodicInvoice, "ET");
			job1.JH_GB = GlbBranch.CurrentBranch.PK;
			job1.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job1.LocalChargesPK = client.PK;
			Helper.CreateJobCharge(job1, chargeCode, 100m);
			Factory.Save();

			var result = LoadFunction();
			AssertEquals("Periodic Invoice should appear on report.", 1, result.Count);
			AssertLine(result[0], job1, chargeCode, client, whs, false, ZDateTimeOffset.Empty,
				periodicInvoice.ET_StorageFromDate, periodicInvoice.ET_StorageToDate, "ET");
		}

		#endregion

		#region TestWarehouseJobProfitChargeFunction_AdHocServiceJobs

		public void TestWarehouseJobProfitChargeFunction_AdHocServiceJobs()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient("Client");
			var chargeCode = Helper.CreateChargeCode("WAH", "Ad Hoc Service Jobs Charge Code",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");

			var adHocJob = SetupAdHocJobWithJobCharge(client, whs, chargeCode, "ADH1");

			var result = LoadFunction();
			AssertEquals("Ad Hoc Service Jobs should be returned by Job Profit Summary Function.", 1, result.Count);
			AssertLine(result[0], (Job)adHocJob.JobHeader, chargeCode, client, whs, true, new ZDateTimeOffset(adHocJob.WSJ_BillingDate),
				ZDateTime.Empty, ZDateTime.Empty, "ADH");
		}

		#region TestWarehouseJobProfitChargeFunction_AdHocServiceJobs_Filtering

		#region TestWarehouseJobProfitChargeFunction_AdHocServiceJobs_DoesNotFilterWarehouseType

		public void TestWarehouseJobProfitChargeFunction_AdHocServiceJobs_DoesNotFilterWarehouseType()
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

			var result = LoadFunction();
			AssertEquals("No warehouse types are filtered.", 4, result.Count);

			var result1 = result.Single(d => (ZString)d["WW_WarehouseName"] == "Warehouse1");
			var result2 = result.Single(d => (ZString)d["WW_WarehouseName"] == "Warehouse2");
			var result3 = result.Single(d => (ZString)d["WW_WarehouseName"] == "Warehouse3");
			var result4 = result.Single(d => (ZString)d["WW_WarehouseName"] == "Warehouse4");
			AssertLine(result1, (Job)adHocJob1.JobHeader, chargeCode, client, whs1, true, new ZDateTimeOffset(adHocJob4.WSJ_BillingDate),
				ZDateTime.Empty, ZDateTime.Empty, "ADH");
			AssertLine(result2, (Job)adHocJob2.JobHeader, chargeCode, client, whs2, true, new ZDateTimeOffset(adHocJob4.WSJ_BillingDate),
				ZDateTime.Empty, ZDateTime.Empty, "ADH");
			AssertLine(result3, (Job)adHocJob3.JobHeader, chargeCode, client, whs3, true, new ZDateTimeOffset(adHocJob4.WSJ_BillingDate),
				ZDateTime.Empty, ZDateTime.Empty, "ADH");
			AssertLine(result4, (Job)adHocJob4.JobHeader, chargeCode, client, whs4, true, new ZDateTimeOffset(adHocJob4.WSJ_BillingDate),
				ZDateTime.Empty, ZDateTime.Empty, "ADH");
		}

		#endregion

		#region TestWarehouseJobProfitChargeFunction_AdHocServiceJobs_DoesNotFilterVirtual

		public void TestWarehouseJobProfitChargeFunction_AdHocServiceJobs_DoesNotFilterVirtual()
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

			var result = LoadFunction();
			AssertEquals("Virtual Warehouses are valid for Ad Hoc Service Jobs.", 2, result.Count);

			var result1 = result.Single(d => (ZString)d["WW_WarehouseName"] == "Warehouse1");
			var result2 = result.Single(d => (ZString)d["WW_WarehouseName"] == "Warehouse2");
			AssertLine(result1, (Job)adHocJob1.JobHeader, chargeCode, client, whs1, true, new ZDateTimeOffset(adHocJob2.WSJ_BillingDate),
				ZDateTime.Empty, ZDateTime.Empty, "ADH");
			AssertLine(result2, (Job)adHocJob2.JobHeader, chargeCode, client, whs2, true, new ZDateTimeOffset(adHocJob2.WSJ_BillingDate),
				ZDateTime.Empty, ZDateTime.Empty, "ADH");
		}

		#endregion

		#region TestWarehouseJobProfitChargeFunction_AdHocServiceJobs_DoesNotFilterInactive

		public void TestWarehouseJobProfitChargeFunction_AdHocServiceJobs_DoesNotFilterInactive()
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

			var result = LoadFunction();
			AssertEquals("In-active Warehouses are valid for Ad Hoc Service Jobs.", 2, result.Count);

			var result1 = result.Single(d => (ZString)d["WW_WarehouseName"] == "Warehouse1");
			var result2 = result.Single(d => (ZString)d["WW_WarehouseName"] == "Warehouse2");
			AssertLine(result1, (Job)adHocJob1.JobHeader, chargeCode, client, whs1, true, new ZDateTimeOffset(adHocJob2.WSJ_BillingDate),
				ZDateTime.Empty, ZDateTime.Empty, "ADH");
			AssertLine(result2, (Job)adHocJob2.JobHeader, chargeCode, client, whs2, true, new ZDateTimeOffset(adHocJob2.WSJ_BillingDate),
				ZDateTime.Empty, ZDateTime.Empty, "ADH");
		}

		#endregion

		#endregion

		#endregion

		#region TestWarehouseJobProfitChargeFunction_VASOrder

		public void TestWarehouseJobProfitChargeFunction_VASOrder()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("Product", client);
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

			var chargeCode = Helper.CreateChargeCode("WAH", "Ad Hoc Service Jobs Charge Code",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");

			var job = Helper.CreateRatingJob(vasOrder, WhsVASOrderSchema.Constants.Prefix);
			var adHocJobHeader = (Job)vasOrder.Job;
			adHocJobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			adHocJobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			adHocJobHeader.LocalChargesPK = client.PK;
			Helper.CreateJobCharge(adHocJobHeader, chargeCode, 10m, client.PK);
			Factory.Save();

			var result = LoadFunction();
			AssertEquals("VASOrder job should be returned by Job Profit Summary Function.", 1, result.Count);
			AssertLine(result[0], (Job)vasOrder.Job, chargeCode, client, whs, true, initialTransfer.WD_FinalisedDate,
				ZDateTime.Empty, ZDateTime.Empty, "WVO");
		}

		public void TestWarehouseJobProfitChargeFunction_VASOrder_WarehouseTypes_FreeTradeZone()
		{
			TestWarehouseJobProfitChargeFunction_VASOrder_WarehouseTypesCore(WarehouseTypes.Codes.FreeTradeZone, true);
		}

		public void TestWarehouseJobProfitChargeFunction_VASOrder_WarehouseTypes_Product()
		{
			TestWarehouseJobProfitChargeFunction_VASOrder_WarehouseTypesCore(WarehouseTypes.Codes.Product, true);
		}

		public void TestWarehouseJobProfitChargeFunction_VASOrder_WarehouseTypes_Transit()
		{
			TestWarehouseJobProfitChargeFunction_VASOrder_WarehouseTypesCore(WarehouseTypes.Codes.Transit, true);
		}

		public void TestWarehouseJobProfitChargeFunction_VASOrder_WarehouseTypes_Product_IsVirtualWarehouse()
		{
			TestWarehouseJobProfitChargeFunction_VASOrder_WarehouseTypesCore(WarehouseTypes.Codes.Product, true,
				isVirtualWarehouse: true);
		}

		public void TestWarehouseJobProfitChargeFunction_VASOrder_WarehouseTypes_Product_IsInActiveWarehouse()
		{
			TestWarehouseJobProfitChargeFunction_VASOrder_WarehouseTypesCore(WarehouseTypes.Codes.Product, true,
				isActiveWarehouse: false);
		}

		void TestWarehouseJobProfitChargeFunction_VASOrder_WarehouseTypesCore(string warehouseType,
			bool expectedHaveResult, bool isVirtualWarehouse = false, bool isActiveWarehouse = true)
		{
			var whs = Helper.CreateWarehouse("Whs");
			whs.WW_WarehouseType = warehouseType;
			whs.WW_IsVirtualWarehouse = isVirtualWarehouse;
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("Product", client);
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

			whs.WW_IsActive = isActiveWarehouse;
			var chargeCode = Helper.CreateChargeCode("WAH", "Ad Hoc Service Jobs Charge Code",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");

			var job = Helper.CreateRatingJob(vasOrder, WhsVASOrderSchema.Constants.Prefix);
			var adHocJobHeader = (Job)vasOrder.Job;
			adHocJobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			adHocJobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			adHocJobHeader.LocalChargesPK = client.PK;
			Helper.CreateJobCharge(adHocJobHeader, chargeCode, 10m, client.PK);
			Factory.Save();

			var result = LoadFunction();
			if (expectedHaveResult)
			{
				AssertEquals("VASOrder job should be returned by Job Profit Summary Function.", 1, result.Count);
				AssertLine(result[0], (Job)vasOrder.Job, chargeCode, client, whs, true, initialTransfer.WD_FinalisedDate,
					ZDateTime.Empty, ZDateTime.Empty, "WVO");
			}
			else
			{
				AssertEquals("VASOrder job should not be returned by Job Profit Summary Function.", 0, result.Count);
			}
		}

		#endregion

		#endregion

		#region implementation

		#region AssertLine

		void AssertLine(DynamicBusinessObject result, Job expectedJobHeader, AccChargeCode expectedChargeCode,
			OrgHeader localClient, WhsWarehouse whs, bool isFinalised, ZDateTimeOffset finalisedDate, ZDateTime storageFrom,
			ZDateTime storageTo, ZString type)
		{
			AssertEquals("JH_ParentID", expectedJobHeader.JH_ParentID, result["JH_ParentID"]);
			AssertEquals("JH_JobNum", expectedJobHeader.JH_JobNum, result["JH_JobNum"]);
			AssertEquals("JH_JobLocalReference", expectedJobHeader.JH_JobLocalReference,
				result["JH_JobLocalReference"]);
			AssertEquals("JH_Status", expectedJobHeader.JH_Status, result["JH_Status"]);
			AssertEquals("JH_JobOpened", expectedJobHeader.JH_A_JOP.Date, ((ZDateTime)result["JH_JobOpened"]).Date);
			AssertEquals("JH_JobClosed", expectedJobHeader.JH_A_JCL.Date, ((ZDateTime)result["JH_JobClosed"]).Date);
			AssertEquals("RecognitionDateList", "IMM", result["RecognitionDateList"]);
			AssertEquals("JH_SystemCreateTimeUtc", expectedJobHeader.JH_SystemCreateTimeUtc.Date,
				((ZDateTime)result["JH_SystemCreateTimeUtc"]).Date);
			AssertEquals("JH_GC", expectedJobHeader.JH_GC, result["JH_GC"]);
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
			AssertEquals("AC_Code", expectedChargeCode.AC_Code, result["AC_Code"]);
			AssertEquals("AC_ChargeGroup", expectedChargeCode.AC_ChargeGroup, result["AC_ChargeGroup"]);
			AssertEquals("AC_ChargeOtherGroups", expectedChargeCode.AC_ChargeOtherGroups,
				result["AC_ChargeOtherGroups"]);
			AssertEquals("AL_BranchCode", GlbBranch.CurrentBranch.GB_Code, result["AL_BranchCode"]);
			AssertEquals("AL_DepartmentCode", GlbDepartment.CurrentDepartment.GE_Code, result["AL_DepartmentCode"]);
			AssertEquals("JH_ParentTableCode", expectedJobHeader.JH_ParentTableCode, result["JH_ParentTableCode"]);
			AssertEquals("JH_ProfitLossReasonCode", expectedJobHeader.JH_ProfitLossReasonCode,
				result["JH_ProfitLossReasonCode"]);
			AssertEquals("WW_WarehouseName", whs.WW_WarehouseName, result["WW_WarehouseName"]);
			AssertEquals("WD_FinalisedDate", finalisedDate.Date, ((ZDateTimeOffset)result["WD_FinalisedDate"]).Date);
			AssertEquals("ET_StorageFromDate", storageFrom.Date, ((ZDateTime)result["ET_StorageFromDate"]).Date);
			AssertEquals("ET_StorageToDate", storageTo.Date, ((ZDateTime)result["ET_StorageToDate"]).Date);
			AssertEquals("WD_NotFinalised", isFinalised ? "N" : "Y", result["WD_NotFinalised"]);
			AssertEquals("WD_OH_Client", localClient.PK, result["WD_OH_Client"]);
			AssertEquals("WD_WW_Whs", whs.PK, result["WD_WW_Whs"]);
			AssertEquals("Type", type, result["Type"]);
		}

		#endregion

		#region LoadFunction

		DynamicBusinessObjectCollection LoadFunction()
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			string sql = @"SELECT *
FROM
Report_WarehouseJobProfitCharge(
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

		#region SetupAdHocJobWithJobCharge

		WhsAdHocServiceJob SetupAdHocJobWithJobCharge(OrgHeader client, WhsWarehouse whs, AccChargeCode chargeCode,
			ZString code)
		{
			var adHocJob = Helper.CreateWhsAdHocServiceJob(whs, client, ZDateTime.Today, code, finalised: true);
			var adHocJobHeader = (Job)adHocJob.JobHeader;
			adHocJobHeader.JH_GB = GlbBranch.CurrentBranch.PK;
			adHocJobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Helper.CreateJobCharge(adHocJobHeader, chargeCode, 10m, adHocJob.WSJ_OH_Client);
			Factory.Save();

			return adHocJob;
		}

		#endregion

		#endregion
	}
}
