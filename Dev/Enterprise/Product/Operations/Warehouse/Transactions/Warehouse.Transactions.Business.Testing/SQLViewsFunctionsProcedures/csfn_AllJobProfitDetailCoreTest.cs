using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class csfn_AllJobProfitDetailCoreTest : WhsTestCaseWithFactory
	{
		#region TestCsfn_AllJobProfitDetailCore_AdHocServiceJob

		[TestDate(2011, 02, 19)]
		public void TestCsfn_AllJobProfitDetailCore_AdHocServiceJob()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient("Client");
			var chargeCode = Helper.CreateChargeCode("WAH", "Ad Hoc Service Jobs Charge Code",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adHocJob = Helper.CreateWhsAdHocServiceJob(whs, client, ZDateTime.Today, "ADH", true);
			var adHocJobHeader = (Job)adHocJob.JobHeader;
			Factory.Save();

			Helper.CreateJobCharge(adHocJobHeader, chargeCode, 10m, adHocJob.WSJ_OH_Client);
			Factory.Save();

			var result = LoadFunction("");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function without job type supplied.", 1,
				result.Count);
			AssertLine(result[0], adHocJobHeader);
			result = LoadFunction("W");
			AssertEquals(
				"Ad Hoc Service Jobs should be returned by Job Profit Summary Function with warehouse job type specified.",
				1, result.Count);
			AssertLine(result[0], adHocJobHeader);
		}

		#endregion

		#region TestCsfn_AllJobProfitDetailCore_ExcludesNonAdHocWorkItems

		[TestDate(2011, 02, 19)]
		public void TestCsfn_AllJobProfitDetailCore_ExcludesNonAdHocWorkItems()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient("Client");
			var chargeCode = Helper.CreateChargeCode("WAH", "Ad Hoc Service Jobs Charge Code",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adHocJob = Helper.CreateWhsAdHocServiceJob(whs, client, ZDateTime.Today, "ADH", true);
			var adHocJobHeader = (Job)adHocJob.JobHeader;

			var workItem = Factory.NewWithValidTestData<WorkItem>();
			workItem.WKI_WorkItemType = "UDF";
			var jobHeader = new JobHeader.Loader(workItem).TryLoadOrCreate();
			jobHeader.JH_OA_LocalChargesAddr = client.MainAddress.PK;
			Factory.Save();

			Helper.CreateJobCharge(adHocJobHeader, chargeCode, 10m, adHocJob.WSJ_OH_Client);
			Helper.CreateJobCharge((Job)jobHeader, chargeCode, 10m);
			Factory.Save();

			var result = LoadFunction("");
			AssertEquals("Both Work Items should be returned by Job Profit Summary Function without job type supplied.",
				2, result.Count);
			AssertCollectionContains(
				"Should find ad hoc service job when running Job Profit Summary Function without job type supplied.",
				adHocJobHeader.PK, result.Select(w => w["JH_PK"]));
			AssertCollectionContains(
				"Should find other WorkItem  when running Job Profit Summary Function without job type supplied.",
				jobHeader.PK, result.Select(w => w["JH_PK"]));
			result = LoadFunction("W");
			AssertEquals(
				"Only Ad Hoc Service Jobs should be returned by Job Profit Summary Function with Warheouse job type selected.",
				1, result.Count);
			AssertCollectionContains(
				"Should find ad hoc service job  when running Job Profit Summary Function with Warehouse job type supplied.",
				adHocJobHeader.PK, result.Select(w => w["JH_PK"]));
			AssertCollectionNotContains(
				"Should not find other WorkItem  when running Job Profit Summary Function with Warehouse job type supplied.",
				jobHeader.PK, result.Select(w => w["JH_PK"]));
		}

		#endregion

		#region TestCsfn_AllJobProfitDetailCore_VASOrder

		[TestDate(2011, 02, 19)]
		public void TestCsfn_AllJobProfitDetailCore_VASOrder()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("Product", client);
			var vasOrder = CreateFinalisedVASOrder(whs, client, product);
			var chargeCode = Helper.CreateChargeCode("WAH", "VASOrder Service", ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");

			Helper.CreateRatingJob(vasOrder, WhsVASOrderSchema.Constants.Prefix);
			var jobHeader = (Job)vasOrder.Job;
			Factory.Save();
			Helper.CreateJobCharge(jobHeader, chargeCode, 10m, client.PK);
			Factory.Save();

			var result = LoadFunction("");
			AssertEquals("VASOrder service should be returned by Job Profit Summary Function without job type supplied.", 1, result.Count);
			AssertLine(result[0], jobHeader);
			result = LoadFunction("W");
			AssertEquals("VASOrder service should be returned by Job Profit Summary Function with warehouse job type specified.", 1, result.Count);
			AssertLine(result[0], jobHeader);
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

		#region implementation

		#region AssertLine

		void AssertLine(DynamicBusinessObject result, Job expectedJobHeader)
		{
			AssertEquals("JH_PK", expectedJobHeader.PK, result["JH_PK"]);
			AssertEquals("JH_OA_AgentCollectAddr", expectedJobHeader.JH_OA_AgentCollectAddr,
				result["JH_OA_AgentCollectAddr"]);
			AssertEquals("JH_OA_LocalChargesAddr", expectedJobHeader.JH_OA_LocalChargesAddr,
				result["JH_OA_LocalChargesAddr"]);
			AssertEquals("JH_ParentID", expectedJobHeader.JH_ParentID, result["JH_ParentID"]);
			AssertEquals("JH_JobNum", expectedJobHeader.JH_JobNum, result["JH_JobNum"]);
			AssertEquals("JH_JobLocalReference", expectedJobHeader.JH_JobLocalReference,
				result["JH_JobLocalReference"]);
			AssertEquals("JH_Status", expectedJobHeader.JH_Status, result["JH_Status"]);
			AssertEquals("JH_SystemCreateTimeUtc", expectedJobHeader.JH_SystemCreateTimeUtc.Date,
				((ZDateTime)result["JH_SystemCreateTimeUtc"]).Date);
			AssertEquals("JH_JobOpened", expectedJobHeader.JH_A_JOP.Date, ((ZDateTime)result["JH_A_JOP"]).Date);
			AssertEquals("JH_JobClosed", expectedJobHeader.JH_A_JCL.Date, ((ZDateTime)result["JH_A_JCL"]).Date);
			AssertEquals("JH_GB", expectedJobHeader.JH_GB, result["JH_GB"]);
			AssertEquals("JH_GE", expectedJobHeader.JH_GE, result["JH_GE"]);
			AssertEquals("JH_GS_NKRepOps", expectedJobHeader.JH_GS_NKRepOps, result["JH_GS_NKRepOps"]);
			AssertEquals("JH_GS_NKRepSales", expectedJobHeader.JH_GS_NKRepSales, result["JH_GS_NKRepSales"]);
			AssertEquals("JH_ProfitLossReasonCode", expectedJobHeader.JH_ProfitLossReasonCode,
				result["JH_ProfitLossReasonCode"]);
			AssertNotEquals("AL_JH", ZGuid.Empty, result["AL_JH"]);
			AssertNotEquals("AL_PK", ZGuid.Empty, result["AL_PK"]);
			AssertNotEquals("AL_AC", ZGuid.Empty, result["AL_AC"]);
			AssertNotEquals("AL_GE", ZGuid.Empty, result["AL_GE"]);
			AssertNotEquals("AL_GB", ZGuid.Empty, result["AL_GB"]);
			AssertEquals("AL_AH", ZGuid.Empty, result["AL_AH"]);
			AssertEquals("AL_OH", ZGuid.Empty, result["AL_OH"]);
			AssertNotEquals("AL_PostDate", ZDateTime.Empty, result["AL_PostDate"]);
			AssertEquals("Al_ReverseDate", ZDateTime.Empty, result["Al_ReverseDate"]);
			AssertNotEquals("AL_LineType", ZString.Empty, result["AL_LineType"]);
			AssertNotEquals("AL_Desc", ZString.Empty, result["AL_Desc"]);
			AssertNotEquals("AL_RevRecognitionType", ZString.Empty, result["AL_RevRecognitionType"]);
			AssertNotEquals("AL_LineAmount", ZDecimal.Zero, result["AL_LineAmount"]);
			AssertNotEquals("AL_RevenueRecognitionDate", ZDateTime.Empty, result["AL_RevenueRecognitionDate"]);
			AssertEquals("AL_LinesExistForCriteria", "Y", result["AL_LinesExistForCriteria"]);
		}

		#endregion

		#region LoadFunction

		DynamicBusinessObjectCollection LoadFunction(ZString type)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			string sql = @"SELECT *
FROM
csfn_AllJobProfitDetailCore(
@client,
'1900-01-01 00:00:00',
'2011-02-20 23:59:29',
@type,
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
null,
'',
null,
'',
''
)";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@client", GlbCompany.CurrentCompany.PK, GlbCompanySchema.PK);
			sqlParams.Add("@type", type, DummyBizoSchema.Z0_Code);

			result.Load(sql, sqlParams);
			return result;
		}

		#endregion

		#endregion
	}
}
