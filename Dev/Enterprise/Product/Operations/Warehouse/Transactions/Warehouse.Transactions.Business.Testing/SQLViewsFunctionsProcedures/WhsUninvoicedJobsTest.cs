using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

// Extracted from Warehouse\Transactions\Warehouse.Transactions.Business.Testing\SQLViewsFunctionsProcedures\WhsProductCategoryAndChildCategoriesTest.cs - refer to the original file for checking history
namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	internal class WhsUninvoicedJobsTest : WhsTestCaseWithFactory
	{
		#region TestView

		public void TestView_Dockets()
		{
			var whs1 = Helper.CreateWarehouse("1", "A", 4, 2);
			var whs2 = Helper.CreateWarehouse("2", "A", 2, 2);
			var orders = new List<WhsOrder>();
			var receives = new List<WhsReceive>();
			SetupData(whs1, whs2, ref orders, ref receives);
			var result = LoadView(whs1);

			AssertEquals(7, result.Count);
			AssertReportLine(result[0], receives[0]);
			AssertReportLine(result[1], receives[4]);
			AssertReportLine(result[2], receives[5]);
			AssertReportLine(result[3], orders[0]);
			AssertReportLine(result[4], orders[1]);
			AssertReportLine(result[5], orders[2]);
			AssertReportLine(result[6], orders[3]);

			var result2 = LoadView(whs2);
			AssertEquals(1, result2.Count);
			AssertReportLine(result2[0], receives[6]);
		}

		#endregion

		#region TestView_Loading

		public void TestLoadView_Loading()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			Factory.Save();

			Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var pickLine2 = order1.Lines.Single().PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew("CTN");
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var loadPkgPackagePivot = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot.WLP_GS_NKLoadingUser = "E";

			var package2 = order2.PackageJob.Packages.AddNew("CTN");
			package2.Pack(order2.Lines[0].ReleaseLines[0], 5m);
			Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			Factory.Save();

			var results = LoadView(data.Whs1);
			AssertEquals("Should return 3 records.", 3, results.Count);
			AssertReportLine(results[0], order1);
			AssertReportLine(results[1], order2);
			AssertReportLine(results[2], receive);
		}

		#endregion

		#region TestView_AdhocServiceJobs

		#region TestView_AdhocServiceJobs_NoCharge

		public void TestView_AdhocServiceJobs_NoCharge()
		{
			var whs = Helper.CreateWarehouse("Warehouse");
			var client = Helper.CreateClient("Client");
			var date = ZDateTime.Today;
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(whs, client, date, "ADH1", true);
			Factory.Save();

			var result = LoadView(whs);
			AssertEquals("Ad Hoc Service Jobs with no charges should appear on Uninvoiced Jobs report.", 1,
				result.Count);
			AssertReportLine(result[0], adhocServiceJob);

			adhocServiceJob.JobHeader.MarkAsInactive();
			Factory.Save();

			var jobHeaders = new BusinessObjectFactory()
				.Load<JobHeader>(new ZQuery(JobHeaderSchema.JH_ParentID, adhocServiceJob.PK)).Where(x => x.JH_IsActive);
			AssertEquals("After delete should not have job Header", 0, jobHeaders.Count());
			var result2 = LoadView(whs);
			AssertEquals(
				"When Job Header is missing still Ad Hoc Service Jobs should appear on Uninvoiced Jobs report.", 1,
				result2.Count);
			AssertReportLine(result2[0], adhocServiceJob);
		}

		#endregion

		#region TestView_AdhocServiceJobs_HasPostedCharges

		public void TestView_AdhocServiceJobs_HasPostedCharges()
		{
			var whs = Helper.CreateWarehouse("Warehouse");
			var client = Helper.CreateClient("Client");
			var date = ZDateTime.Today;
			client.OH_IsDebtor = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			Factory.Save();

			var adHocChargeCode = Helper.CreateChargeCode("WAHT", "Ad Hoc Service Job Handling",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adhocServiceJob1 = Helper.CreateWhsAdHocServiceJob(whs, client, date, "ADH1", true);
			var adhocServiceJob2 = Helper.CreateWhsAdHocServiceJob(whs, client, date, "ADH2", true);
			Factory.Save();

			Helper.CreateJobCharge((Job)adhocServiceJob1.JobHeader, adHocChargeCode, 10m,
				adhocServiceJob1.WSJ_OH_Client);
			Helper.CreateJobCharge((Job)adhocServiceJob1.JobHeader, adHocChargeCode, 20m,
				adhocServiceJob1.WSJ_OH_Client);
			Helper.PostInvoice((Job)adhocServiceJob1.JobHeader);

			Helper.CreateJobCharge((Job)adhocServiceJob2.JobHeader, adHocChargeCode, 10m,
				adhocServiceJob2.WSJ_OH_Client);
			Helper.CreateJobCharge((Job)adhocServiceJob2.JobHeader, adHocChargeCode, 20m,
				adhocServiceJob2.WSJ_OH_Client);
			Helper.PostInvoice((Job)adhocServiceJob2.JobHeader);

			Factory.Save();

			var result = LoadView(whs);
			AssertEquals("Ad Hoc Service Jobs with all charges posted should not appear on uninvoiced jobs report.", 0,
				result.Count);
		}

		#endregion

		#region TestView_AdhocServiceJobs_HasUnpostedCharges

		public void TestView_AdhocServiceJobs_HasUnpostedCharges()
		{
			var whs = Helper.CreateWarehouse("Warehouse");
			var client = Helper.CreateClient("Client");
			var date = ZDateTime.Today;
			client.OH_IsDebtor = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var adHocChargeCode = Helper.CreateChargeCode("WAHT", "Ad Hoc Service Job Handling",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adhocServiceJob1 = Helper.CreateWhsAdHocServiceJob(whs, client, date, "ADH1", true);
			var adhocServiceJob2 = Helper.CreateWhsAdHocServiceJob(whs, client, date, "ADH2", true);
			Factory.Save();

			Helper.CreateJobCharge((Job)adhocServiceJob1.JobHeader, adHocChargeCode, 10m,
				adhocServiceJob1.WSJ_OH_Client);
			Helper.CreateJobCharge((Job)adhocServiceJob1.JobHeader, adHocChargeCode, 20m,
				adhocServiceJob1.WSJ_OH_Client);

			Helper.CreateJobCharge((Job)adhocServiceJob2.JobHeader, adHocChargeCode, 10m,
				adhocServiceJob2.WSJ_OH_Client);

			Factory.Save();

			var result = LoadView(whs);
			AssertEquals("Ad Hoc Service Jobs with unposted charges should appear on Uninvoiced Jobs report.", 2,
				result.Count);
			AssertReportLine(result[0], adhocServiceJob1);
			AssertReportLine(result[1], adhocServiceJob2);
		}

		#endregion

		#region TestView_AdhocServiceJobs_HasPostedAndUnpostedCharges

		public void TestView_AdhocServiceJobs_HasPostedAndUnpostedCharges()
		{
			var whs = Helper.CreateWarehouse("Warehouse");
			var client = Helper.CreateClient("Client");
			var date = ZDateTime.Today;
			client.OH_IsDebtor = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var adHocChargeCode = Helper.CreateChargeCode("WAHT", "Ad Hoc Service Job Handling",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adhocServiceJob1 = Helper.CreateWhsAdHocServiceJob(whs, client, date, "ADH1", true);
			var adhocServiceJob2 = Helper.CreateWhsAdHocServiceJob(whs, client, date, "ADH2", true);
			Factory.Save();

			Helper.CreateJobCharge((Job)adhocServiceJob1.JobHeader, adHocChargeCode, 10m,
				adhocServiceJob1.WSJ_OH_Client);
			Helper.PostInvoice((Job)adhocServiceJob1.JobHeader);
			Helper.CreateJobCharge((Job)adhocServiceJob1.JobHeader, adHocChargeCode, 20m,
				adhocServiceJob1.WSJ_OH_Client);

			Helper.CreateJobCharge((Job)adhocServiceJob2.JobHeader, adHocChargeCode, 10m,
				adhocServiceJob2.WSJ_OH_Client);

			Factory.Save();

			var result = LoadView(whs);
			AssertEquals("Ad Hoc Service Jobs with unposted charges should appear on Uninvoiced Jobs report.", 2,
				result.Count);
			AssertReportLine(result[0], adhocServiceJob1);
			AssertReportLine(result[1], adhocServiceJob2);
		}

		#endregion

		#region TestView_AdhocServiceJobs_HasChargeWithNoValue

		public void TestView_AdhocServiceJobs_HasChargesWithNoValue()
		{
			var whs = Helper.CreateWarehouse("Warehouse");
			var client = Helper.CreateClient("Client");
			var date = ZDateTime.Today;
			client.OH_IsDebtor = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var adHocChargeCode = Helper.CreateChargeCode("WAHT", "Ad Hoc Service Job Handling",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(whs, client, date, "ADH1", true);
			Factory.Save();

			Helper.CreateJobCharge((Job)adhocServiceJob.JobHeader, adHocChargeCode, 0m);

			Factory.Save();

			var result = LoadView(whs);
			AssertEquals("Ad Hoc Service Jobs with charges with no value should appear on Uninvoiced Jobs report.", 1,
				result.Count);
			AssertReportLine(result[0], adhocServiceJob);
		}

		#endregion

		#region TestView_AdhocServiceJobs_HasPostedChargesAndChargeWithNoValue

		public void TestView_AdhocServiceJobs_HasPostedChargesAndChargeWithNoValue()
		{
			var whs = Helper.CreateWarehouse("Warehouse");
			var client = Helper.CreateClient("Client");
			var date = ZDateTime.Today;
			client.OH_IsDebtor = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var adHocChargeCode = Helper.CreateChargeCode("WAHT", "Ad Hoc Service Job Handling",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(whs, client, date, "ADH1", true);
			Factory.Save();

			Helper.CreateJobCharge((Job)adhocServiceJob.JobHeader, adHocChargeCode, 10m, adhocServiceJob.WSJ_OH_Client);
			Helper.PostInvoice((Job)adhocServiceJob.JobHeader);
			Helper.CreateJobCharge((Job)adhocServiceJob.JobHeader, adHocChargeCode, 0m, adhocServiceJob.WSJ_OH_Client);

			Factory.Save();

			var result = LoadView(whs);
			AssertEquals("Ad Hoc Service Jobs charges posted and 0 value charges are considered invoiced.", 0,
				result.Count);
		}

		#endregion

		#region TestView_AdhocServiceJobs_UnfinalisedJob

		public void TestView_AdhocServiceJobs_UnfinalisedJob()
		{
			var whs = Helper.CreateWarehouse("Warehouse");
			var client = Helper.CreateClient("Client");
			var date = ZDateTime.Today;
			client.OH_IsDebtor = true;
			whs.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var adHocChargeCode = Helper.CreateChargeCode("WAHT", "Ad Hoc Service Job Handling",
				ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(whs, client, date, "ADH1", false);
			Factory.Save();

			Helper.CreateJobCharge((Job)adhocServiceJob.JobHeader, adHocChargeCode, 10m, adhocServiceJob.WSJ_OH_Client);
			Helper.CreateJobCharge((Job)adhocServiceJob.JobHeader, adHocChargeCode, 20m, adhocServiceJob.WSJ_OH_Client);

			Factory.Save();

			var result = LoadView(whs);
			AssertEquals(
				"Ad Hoc Service Jobs with that are not finalised and have charges should appear on Uninvoiced Jobs report.",
				1, result.Count);
			AssertReportLine(result[0], adhocServiceJob);
		}

		#endregion

		#endregion

		#region TestView_VASOrderJobs

		#region TestView_VASOrderJobs_NoCharge

		public void TestView_VASOrderJobs_NoCharge()
		{
			var whs = Helper.CreateWarehouse("Warehouse");
			var client = Helper.CreateClient("Client");
			client.OH_IsDebtor = true;
			var vasOrder = Helper.CreateWhsVASOrder(whs.Areas[0], client);
			Factory.Save();

			var result = LoadView(whs);
			AssertEquals("VASOrder with no charges should appear on Uninvoiced Jobs report.", 1, result.Count);
			AssertReportLine(result[0], vasOrder);
		}

		#endregion

		#region TestView_VASOrderJobs_PostedCharges

		public void TestView_VASOrderJobs_PostedCharges()
		{
			var whs = Helper.CreateWarehouse("Warehouse");
			var client = Helper.CreateClient("Client");
			client.OH_IsDebtor = true;
			var vasOrder = Helper.CreateWhsVASOrder(whs.Areas[0], client);
			new JobHeader.Loader(vasOrder).TryLoadOrCreate();
			var vasOrderChargeCode = CreateVasOrderChargeCode();
			Factory.Save();

			Helper.CreateJobCharge((Job)vasOrder.Job, vasOrderChargeCode, 10m, client.PK);
			Factory.Save();

			var result1 = LoadView(whs);
			AssertReportLine(result1[0], vasOrder);

			Helper.PostInvoice((Job)vasOrder.Job);
			Factory.Save();

			var result2 = LoadView(whs);
			AssertEquals("VASOrder with charge posted should not appear on uninvoiced jobs report.", 0, result2.Count);
		}

		#endregion

		#region TestView_VASOrderJobs_HasPostedAndUnpostedCharges

		public void TestView_VASOrderJobs_HasPostedAndUnpostedCharges()
		{
			var whs = Helper.CreateWarehouse("Warehouse");
			var client = Helper.CreateClient("Client");
			client.OH_IsDebtor = true;
			var vasOrder = Helper.CreateWhsVASOrder(whs.Areas[0], client);
			new JobHeader.Loader(vasOrder).TryLoadOrCreate();
			var vasOrderChargeCode = CreateVasOrderChargeCode();
			Factory.Save();

			AssertReportLine(LoadView(whs).Single(), vasOrder);

			Helper.CreateJobCharge((Job)vasOrder.Job, vasOrderChargeCode, 10m, client.PK);
			Factory.Save();

			AssertReportLine(LoadView(whs).Single(), vasOrder);

			Helper.PostInvoice((Job)vasOrder.Job);
			Factory.Save();

			AssertEquals("VASOrder has posted without unposted charge therefore should not show in report.", 0,
				LoadView(whs).Count);

			var secondCharge = Helper.CreateJobCharge((Job)vasOrder.Job, vasOrderChargeCode, 10m, client.PK);
			Factory.Save();
			//vas order has posted and unposted charges
			AssertReportLine(LoadView(whs).Single(), vasOrder);

			secondCharge.Delete();
			Factory.Save();
			AssertEquals("VASOrder has posted without unposted charge therefore should not show in report.", 0,
				LoadView(whs).Count);
		}

		#endregion

		#region TestView_VASOrderJobs_HasChargeWithNoValue

		public void TestView_VASOrderJobs_HasChargesWithNoValue()
		{
			var whs = Helper.CreateWarehouse("Warehouse");
			var client = Helper.CreateClient("Client");
			client.OH_IsDebtor = true;
			var vasOrder = Helper.CreateWhsVASOrder(whs.Areas[0], client);
			new JobHeader.Loader(vasOrder).TryLoadOrCreate();
			var vasOrderChargeCode = CreateVasOrderChargeCode();
			Factory.Save();

			Helper.CreateJobCharge((Job)vasOrder.Job, vasOrderChargeCode, 0m, client.PK);
			Helper.PostInvoice((Job)vasOrder.Job);
			Factory.Save();

			var result = LoadView(whs);
			//"VASOrder with charge posted should appear on uninvoiced jobs report."
			AssertReportLine(result.Single(), vasOrder);

			Helper.CreateJobCharge((Job)vasOrder.Job, vasOrderChargeCode, 1m, client.PK);
			Helper.PostInvoice((Job)vasOrder.Job);
			Factory.Save();

			AssertEquals("VASOrder with posted charge should not appear on uninvoiced jobs report.", 0,
				LoadView(whs).Count);
		}

		#endregion

		#region TestView_VASOrderJobs_Status

		public void TestView_VASOrderJobs_Status()
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("Product", client);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 1m);
			var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, client, product, 1m);
			Factory.Save();

			AssertEquals("VASOrder does not have transfer in.", WhsVASOrderStatuses.Codes.Entered,
				LoadView(whs).Single(FilterVASOrders)["status"]);

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			Factory.Save();
			AssertEquals("Precondition", false, initialTransfer.IsFinalised);
			AssertEquals("VASOrder has transfer in and is not Finalised.", WhsVASOrderStatuses.Codes.TransferringIn,
				LoadView(whs).Single(FilterVASOrders)["status"]);

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			AssertIsFinalisedPrecondition(initialTransfer);
			AssertEquals("VASOrder's transfer in is Finalised.", WhsVASOrderStatuses.Codes.Working,
				LoadView(whs).Single(FilterVASOrders)["status"]);

			vasOrder.WVO_WorkCompletedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			AssertEquals("VASOrder work Completed is true.", WhsVASOrderStatuses.Codes.WorkCompleted,
				LoadView(whs).Single(FilterVASOrders)["status"]);

			WhsTransfer transferOut;
			using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
			{
				transferOut = vasOrder.GetOrCreateReturnTransfer(Notify);
			}

			Factory.Save();
			AssertNotNull("Precondition", transferOut);
			AssertEquals("VASOrder has transfer out and is not Finalised.", WhsVASOrderStatuses.Codes.TransferringOut,
				LoadView(whs).Single(FilterVASOrders)["status"]);

			vasOrder.WVO_FinalizedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			AssertEquals("VASOrder is Finalised.", WhsVASOrderStatuses.Codes.Finalized,
				LoadView(whs).Single(FilterVASOrders)["status"]);
		}

		public void TestView_VASOrderJobs_Status_Cancel()
		{
			var whs = Helper.CreateWarehouse("Warehouse");
			var client = Helper.CreateClient("Client");
			client.OH_IsDebtor = true;
			var vasOrder = Helper.CreateWhsVASOrder(whs.Areas[0], client);
			Factory.Save();

			AssertReportLine(LoadView(whs).Single(), vasOrder);

			vasOrder.WVO_CancelledTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			AssertEquals("Cancel VASOrder should not appear on Uninvoiced Jobs report.", 0, LoadView(whs).Count);
		}

		#endregion

		#region TestView_VASOrderJobs_FinaliseDate

		[TestDate(2022, 02, 01)]
		public void TestView_VASOrderJobs_FinaliseDate_TrasferInOnly()
		{
			TestView_VASOrderJobs_FinaliseDate_Core(withTransferOut: false);
		}

		[TestDate(2022, 02, 01)]
		public void TestView_VASOrderJobs_FinaliseDate_TransferOut()
		{
			TestView_VASOrderJobs_FinaliseDate_Core(withTransferOut: true);
		}

		void TestView_VASOrderJobs_FinaliseDate_Core(bool withTransferOut)
		{
			var whs = Helper.CreateWarehouse("Whs");
			var client = Helper.CreateClient("Client");
			var product = Helper.CreateProduct("Product", client);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(whs);
			Helper.CreateWhsReceiveWithInventory(client, whs, "R1", product, 1m);
			var vasOrder = Helper.CreateWhsVASOrderWithLine(serviceArea, client, product, 1m);
			Factory.Save();

			var expectedFinaliseDate = ZDateTimeOffset.Empty;
			AssertReportLine(LoadView(whs).Single(FilterVASOrders), vasOrder, expectedFinaliseDate);

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(Notify);
			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			vasOrder.MarkVASOrderAsCompleted(Notify);
			Factory.Save();

			if (withTransferOut)
			{
				WhsTransfer transferOut;
				using (Helper.GetPutawayEngineManagerForVASTransferLineMock())
				{
					transferOut = vasOrder.GetOrCreateReturnTransfer(Notify);
				}

				AssertNotNull("Precondition", transferOut);
				expectedFinaliseDate = transferOut.WD_FinalisedDate;
			}
			else
			{
				expectedFinaliseDate = initialTransfer.WD_FinalisedDate;
			}

			vasOrder.WVO_FinalizedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			AssertEquals("Precondition: VAS Order is finalised.", true, vasOrder.IsFinalised);

			AssertReportLine(LoadView(whs).Single(FilterVASOrders), vasOrder, expectedFinaliseDate);
		}

		#endregion

		#endregion

		#region TestView_PickByBOM_Receive

		public void TestView_PickByBOM_Receive_Finalised()
		{
			TestView_PickByBOM_Receive_Core(isFinalised: true);
		}

		public void TestView_PickByBOM_Receive_NotFinalised()
		{
			TestView_PickByBOM_Receive_Core(isFinalised: false);
		}

		void TestView_PickByBOM_Receive_Core(bool isFinalised)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var bike = Helper.CreateProduct(data.Org1, "BIKE");
			bike.OP_IsComponentPickedOnSalesOrder = true;
			var wheel = Helper.CreateProduct(data.Org1, "WHEEL");
			Helper.CreateProductBOM(bike, wheel, 2m, "UNT");

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", wheel, 50m);
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = Helper.CreateWhsOrderLine(order, bike, 5m);
			var pick = Helper.CreatePickNew(order);
			var wheelOrderLine = orderLine1.ChildComponentLines.Single();
			var wheelPickLine = wheelOrderLine.PickLines.Single();
			var kitPickLine = orderLine1.PickLines.Single();
			pick.FinaliseAllOrders();
			Factory.Save();

			var createdReceive = Factory.LoadTop1<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, pick.PK));
			var createdReceiveLine = createdReceive.Lines.Single();

			if (isFinalised)
			{
				wheelPickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var wheelTransferLine = (WhsTransferLine)(wheelPickLine.InventoryLine);
				WhsPickByBOMHelper.CreatePickByBOMTransferLinesIfNecessary(newFactory, data.Whs1.DefaultOutboundDockDoorLocation.PK, new WhsTransferLine[] { wheelTransferLine }, GlbStaff.CurrentUser.GS_Code);
				newFactory.Save();

				pick = newFactory.Load<WhsPick>(pick.PK);
				createdReceive = newFactory.Load<WhsReceive>(createdReceive.PK);
				createdReceiveLine = createdReceive.Lines.Single();
				kitPickLine = newFactory.Load<WhsPickLine>(kitPickLine.PK);
				AssertEquals("Precondition", false, pick.IsFinalised);
				AssertEquals("Precondition", false, createdReceive.IsFinalised);
				AssertEquals("Precondition", false, createdReceiveLine.IsFinalised);

				pick.FinalisePick();
				newFactory.Save();
			}

			AssertEquals("Precondition: PBB Receive is finalised.", isFinalised, createdReceive.IsFinalised);

			AssertNull("Should not include the Pick by BOM Receive.", LoadView(data.Whs1).SingleOrDefault(r => (ZString)r["DocketID"] == createdReceive.WD_DocketID));
		}

		#endregion

		#region Implementation

		#region AssertReportLine

		void AssertReportLine(DynamicBusinessObject dynamicObject, WhsDocket docket)
		{
			AssertEquals("WarehousePK", docket.Warehouse.PK, dynamicObject["WarehousePK"]);
			AssertEquals("WarehouseName", docket.Warehouse.WW_WarehouseName, dynamicObject["WarehouseName"]);
			AssertEquals("ClientCode", docket.Client.OH_Code, dynamicObject["ClientCode"]);
			AssertEquals("Client", docket.Client.OH_FullName, dynamicObject["Client"]);
			AssertEquals("TranType", docket.WD_DocketType, dynamicObject["TranType"]);
			AssertEquals(
				"Status",
				docket is WhsOrder order ? order.WarehouseOrderStatus : docket.WD_DocketStatus,
				dynamicObject["Status"]);
			AssertEquals("DocketID", docket.WD_DocketID, dynamicObject["DocketID"]);
			AssertEquals("Reference", docket.WD_ExternalReference, dynamicObject["Reference"]);
			AssertEquals("FinalisedDate", docket.WD_FinalisedDate.Date,
				((ZDateTimeOffset)dynamicObject["FinalisedDate"]).Date);
			AssertEquals("SystemCreateTime", docket.WD_SystemCreateTimeUtc.Date,
				((ZDateTime)dynamicObject["SystemCreateTime"]).Date);
			AssertEquals("IsFinalised", docket.IsFinalised ? "Y" : "N", dynamicObject["IsFinalised"]);
		}

		void AssertReportLine(DynamicBusinessObject dynamicObject, WhsAdHocServiceJob adhocServiceJob)
		{
			AssertEquals("WarehousePK", adhocServiceJob.Warehouse.PK, dynamicObject["WarehousePK"]);
			AssertEquals("WarehouseName", adhocServiceJob.Warehouse.WW_WarehouseName, dynamicObject["WarehouseName"]);
			AssertEquals("ClientCode", adhocServiceJob.Client.OH_Code, dynamicObject["ClientCode"]);
			AssertEquals("Client", adhocServiceJob.Client.OH_FullName, dynamicObject["Client"]);
			AssertEquals("DocketID", adhocServiceJob.WSJ_JobNumber, dynamicObject["DocketID"]);
			AssertEquals("Reference", adhocServiceJob.WSJ_CustomerReference, dynamicObject["Reference"]);
			AssertEquals("FinalisedDate", adhocServiceJob.WSJ_BillingDate,
				((ZDateTimeOffset)dynamicObject["FinalisedDate"]).Date);
			AssertEquals("SystemCreateTime", adhocServiceJob.WSJ_SystemCreateTimeUtc.Date,
				((ZDateTime)dynamicObject["SystemCreateTime"]).Date);
			AssertEquals("IsFinalised", adhocServiceJob.IsFinalised ? "Y" : "N", dynamicObject["IsFinalised"]);
			AssertReportLineCustomColumns(dynamicObject);
		}

		void AssertReportLine(DynamicBusinessObject dynamicObject, WhsVASOrder vasOrder,
			ZDateTimeOffset? expectedFinaliseDate = null)
		{
			AssertEquals("WarehousePK", vasOrder.Warehouse.PK, dynamicObject["WarehousePK"]);
			AssertEquals("WarehouseName", vasOrder.Warehouse.WW_WarehouseName, dynamicObject["WarehouseName"]);
			AssertEquals("ClientCode", vasOrder.Client.OH_Code, dynamicObject["ClientCode"]);
			AssertEquals("Client", vasOrder.Client.OH_FullName, dynamicObject["Client"]);
			AssertEquals("DocketID", vasOrder.WVO_JobID, dynamicObject["DocketID"]);
			AssertEquals("Reference", vasOrder.WVO_CustomerReferenceNo, dynamicObject["Reference"]);
			var finaliseDate = dynamicObject["FinalisedDate"];
			var reportFinaliseDate = string.IsNullOrEmpty(finaliseDate.ToString()) ? ZDateTimeOffset.Empty : (ZDateTimeOffset)finaliseDate;
			AssertEquals("FinalisedDate", expectedFinaliseDate ?? ZDateTimeOffset.Empty, reportFinaliseDate);
			AssertEquals("SystemCreateTime", vasOrder.WVO_SystemCreateTimeUtc.Date,
				((ZDateTime)dynamicObject["SystemCreateTime"]).Date);
			AssertEquals("IsFinalised", vasOrder.IsFinalised ? "Y" : "N", dynamicObject["IsFinalised"]);
			AssertReportLineCustomColumns(dynamicObject);
		}

		void AssertReportLineCustomColumns(DynamicBusinessObject dynamicObject)
		{
			AssertEquals("Custom Attribute1 should be empty.", "", dynamicObject["WhsDocket_CustomAttrib1"]);
			AssertEquals("Custom Attribute2 should be empty.", "", dynamicObject["WhsDocket_CustomAttrib2"]);
			AssertEquals("Custom Attribute3 should be empty.", "", dynamicObject["WhsDocket_CustomAttrib3"]);
			AssertEquals("Custom Attribute4 should be empty.", "", dynamicObject["WhsDocket_CustomAttrib4"]);
			AssertEquals("Custom Attribute5 should be empty.", "", dynamicObject["WhsDocket_CustomAttrib5"]);
			AssertEquals("Custom Date1 should be empty.", ZDateTime.Empty, dynamicObject["WhsDocket_CustomDate1"]);
			AssertEquals("Custom Date2 should be empty.", ZDateTime.Empty, dynamicObject["WhsDocket_CustomDate2"]);
			AssertEquals("Custom Decimal1 should be empty.", ZDecimal.Zero, dynamicObject["WhsDocket_CustomDecimal1"]);
			AssertEquals("Custom Decimal2 should be empty.", ZDecimal.Zero, dynamicObject["WhsDocket_CustomDecimal2"]);
			AssertEquals("Custom Decimal3 should be empty.", ZDecimal.Zero, dynamicObject["WhsDocket_CustomDecimal3"]);
			AssertEquals("Custom Decimal4 should be empty.", ZDecimal.Zero, dynamicObject["WhsDocket_CustomDecimal4"]);
			AssertEquals("Custom Decimal5 should be empty.", ZDecimal.Zero, dynamicObject["WhsDocket_CustomDecimal5"]);
			AssertEquals("Custom Flag1 should be empty.", 0, dynamicObject["WhsDocket_CustomFlag1"]);
			AssertEquals("Custom Flag2 should be empty.", 0, dynamicObject["WhsDocket_CustomFlag2"]);
			AssertEquals("Custom Flag3 should be empty.", 0, dynamicObject["WhsDocket_CustomFlag3"]);
			AssertEquals("Custom Flag4 should be empty.", 0, dynamicObject["WhsDocket_CustomFlag4"]);
			AssertEquals("Custom Flag5 should be empty.", 0, dynamicObject["WhsDocket_CustomFlag5"]);
		}

		#endregion

		#region LoadView

		DynamicBusinessObjectCollection LoadView(WhsWarehouse whs)
		{
			var result = new DynamicBusinessObjectCollection(Factory);
			var sql = @"
SELECT
	*
FROM
	dbo.WhsUninvoicedJobsReport
WHERE
	WarehousePK = @WarehousePK
ORDER BY
	reference";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WarehousePK", whs.PK, WhsWarehouseSchema.PK);

			result.Load(sql, sqlParams);
			return result;
		}

		#endregion

		#region SetupData

		void SetupData(WhsWarehouse whs1, WhsWarehouse whs2, ref List<WhsOrder> orders, ref List<WhsReceive> receives)
		{
			var date = ZDateTime.Now.AddMonths(-1);
			var year = date.Year;
			var month = date.Month;

			var client = Helper.CreateClient("TESFIR", "Test First Client");
			var consignee = Helper.CreateClient("TESSEC", "Test Second Client");
			var part = Helper.CreateProduct(client, "1");

			// setup rating
			client.OH_IsDebtor = true;
			whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;
			whs1.WW_UseArrivalDateForInwardsFinalisedDate = true;
			whs1.WW_UseRequiredDateForOutwardsFinalisedDate = true;

			var receiveHandlirngChargeCode =
				Helper.CreateChargeCode("WREC", "Receive Handling", ChargeCodeGroupList.Codes.WHSInwards, "");
			var orderHandlirngChargeCode =
				Helper.CreateChargeCode("WOUT", "Order Handling", ChargeCodeGroupList.Codes.WHSOutwards, "");

			var clientRate = Factory.New<ClientRate>();
			client.OH_IsDebtor = true;
			clientRate.TH_OH = client.PK;
			var rateEntry = Helper.CreateRateEntry(clientRate, new ZDate(year - 1, 1, 1), ZDate.Empty);
			Helper.CreateRateLine(rateEntry, receiveHandlirngChargeCode, Constants.PkgUnit.Unit, 5m);
			Helper.CreateRateLine(rateEntry, orderHandlirngChargeCode, Constants.PkgUnit.Unit, 10m);

			// setup whs jobs
			var receive1 = SetupReceive(client, whs1, "01", new ZDateTimeOffset(year, month, 1), part, 111, true,
				new ZDateTime(year, month, 1));
			var receive2 = SetupReceive(client, whs1, "02", new ZDateTimeOffset(year, month, 3), part, 121, true,
				new ZDateTime(year, month, 2));
			var receive3 = SetupReceive(client, whs1, "03", new ZDateTimeOffset(year, month, 8), part, 211, true,
				new ZDateTime(year, month, 7));
			var receive4 = SetupReceive(client, whs1, "04", new ZDateTimeOffset(year, month, 10), part, 221, true,
				new ZDateTime(year, month, 9));
			var receive5 = SetupReceive(client, whs1, "05", new ZDateTimeOffset(year, month, 15), part, 311, true,
				new ZDateTime(year, month, 14));
			var receive6 = SetupReceive(client, whs1, "06", new ZDateTimeOffset(year, month, 17), part, 321, true,
				new ZDateTime(year, month, 16));
			var receive7 = SetupReceive(client, whs2, "07", new ZDateTimeOffset(year, month, 18), part, 411, true,
				new ZDateTime(year, month, 17));
			var receive8 = SetupReceive(client, whs2, "08", new ZDateTimeOffset(year, month, 24), part, 511, false,
				new ZDateTime(year, month, 25));
			Factory.Save();
			var order1 = SetupOrder(client, consignee, whs1, "21", new ZDateTimeOffset(year, month, 9), part, 10, true);
			var order2 = SetupOrder(client, consignee, whs1, "23", new ZDateTimeOffset(year, month, 1), part, 6, true);
			var order3 = SetupOrder(client, consignee, whs1, "24", new ZDateTimeOffset(year, month, 12), part, 8, true);
			var order4 = SetupOrder(client, consignee, whs1, "25", new ZDateTimeOffset(year, month, 17), part, 20, true);
			var order5 = SetupOrder(client, consignee, whs1, "26", new ZDateTimeOffset(year, month, 28), part, 15, true);

			var adjustment1 = SetupAdjustment(client, whs1, "31", part, 9, true);

			// setup invoicing
			var invoiceType = ObjectFactory.GetType<IWhsInvoice>();
			var invoice = (JobStorage)Factory.NewWithValidTestData(invoiceType);
			invoice.ET_StorageType = "WHS";
			invoice.ET_OH_Client = client.PK;
			invoice.ET_WW = whs1.PK;
			invoice.ET_StorageFromDate = new ZDateTime(year, month, 8);
			invoice.ET_StorageToDate = new ZDateTime(year, month, 14);

			// Periodic with multiple Orders and Receives - All Posted. Pre-create Jobs for Receives here for proper AutoRating
			var jobHeader1 = Helper.CreateRatingJob(invoice);
			var jobHeader2 = Helper.CreateRatingJob(receive3, WhsDocketSchema.Constants.Prefix);
			Helper.CreateRatingJob(receive4, WhsDocketSchema.Constants.Prefix);

			Helper.AutoRateJob(invoice, jobHeader1,
				new IAutoRating[]
				{
					GetIAutoRating((IRatingSupporter)invoice), GetIAutoRating(receive3), GetIAutoRating(receive4),
					GetIAutoRating(order1), GetIAutoRating(order3)
				});
			Helper.PostInvoice(jobHeader1);
			AssertEquals("Precondition - ensure invoice was Autorated.", true, jobHeader1.Charges.Count > 0);

			// creating 0 value unposted charge for Receive3 to make sure it will be ignored and Receive still will be recognised as Posted.
			Helper.CreateJobCharge(jobHeader2, receiveHandlirngChargeCode, 0m);

			// Job Header for Receive with posted charge.
			var jobHeader3 = Helper.CreateRatingJob(receive2, WhsDocketSchema.Constants.Prefix);
			Helper.AutoRateJob(receive2, jobHeader3, new IAutoRating[] { GetIAutoRating(receive2) });
			Helper.PostInvoice(jobHeader3);
			AssertEquals("Precondition - ensure Receive2 was Autorated.", true, jobHeader3.Charges.Count > 0);

			// Job Header for Order with posted charge.
			var jobHeader4 = Helper.CreateRatingJob(order5, WhsDocketSchema.Constants.Prefix);
			var charge = Helper.CreateJobCharge(jobHeader4, orderHandlirngChargeCode, 5m);
			Helper.PostInvoice(jobHeader4);
			AssertEquals("Precondition - ensure Order6 has a posted charge.", true, charge.IsRevenuePosted);

			// Job Header for Receive with no charges.
			Helper.CreateRatingJob(receive6, WhsDocketSchema.Constants.Prefix);

			Factory.Save();
			CancelReceive(ref receive8);
			Factory.Save();

			orders.Add(order1);
			orders.Add(order2);
			orders.Add(order3);
			orders.Add(order4);
			orders.Add(order5);

			receives.Add(receive1);
			receives.Add(receive2);
			receives.Add(receive3);
			receives.Add(receive4);
			receives.Add(receive5);
			receives.Add(receive6);
			receives.Add(receive7);
			receives.Add(receive8);
		}

		void CancelReceive(ref WhsReceive receive)
		{
			receive.CancelReactivateDocket();
			AssertEquals(true, receive.IsCancelled);
		}

		WhsReceive SetupReceive(OrgHeader client, WhsWarehouse warehouse, ZString reference, ZDateTimeOffset arrivalDate,
			OrgSupplierPart product, ZDecimal units, bool finalise, ZDateTime createTime)
		{
			var receive = Helper.CreateWhsReceive(client.PK, warehouse.PK, reference, arrivalDate, Notify);
			receive.WD_SystemCreateTimeUtc = createTime;
			Helper.CreateWhsReceiveInventoryLine(receive, product, units);
			if (finalise)
			{
				receive.AllocateLocationsWithMock();
				receive.FinaliseDocket();
				AssertIsFinalisedPrecondition(receive);
			}

			return receive;
		}

		WhsOrder SetupOrder(OrgHeader client, OrgHeader consignee, WhsWarehouse warehouse, ZString reference,
			ZDateTimeOffset requiredDate, OrgSupplierPart product, ZDecimal units, bool finalise)
		{
			var order = Helper.CreateWhsOrder(client.PK, warehouse.PK, consignee.PK, reference, requiredDate, Notify);
			Helper.CreateWhsOrderLine(order, product, units);

			if (finalise)
			{
				var pick = Helper.CreatePickNew(order);
				pick.FinaliseAllOrders();
				pick.FinalisePick();
				AssertIsFinalisedPrecondition(order);
				AssertIsFinalisedPrecondition(pick);
			}

			return order;
		}

		WhsAdjustment SetupAdjustment(OrgHeader client, WhsWarehouse warehouse, ZString reference,
			OrgSupplierPart product, ZDecimal units, bool finalise)
		{
			var adjustment = Helper.CreateWhsAdjustment(client, warehouse, reference, Notify);
			Helper.CreateWhsAdjustmentLine(adjustment, product, units, warehouse.FindLocation("A-1-1"));

			if (finalise)
			{
				adjustment.FinaliseDocket();
				AssertIsFinalisedPrecondition(adjustment);
			}

			return adjustment;
		}

		IAutoRating GetIAutoRating(IRatingSupporter ratingSupporter)
		{
			return ratingSupporter.AdaptersProvider.GetAdapters(null, AutoRateOptions.AutorateRevenue)
				.FirstOrDefault();
		}

		AccChargeCode CreateVasOrderChargeCode() => Helper.CreateChargeCode("WVO", "VAS Order Job Handling",
			ChargeCodeGroupList.Codes.WHSAdHocServiceJob, "");

		bool FilterVASOrders(DynamicBusinessObject row) => (ZString)row["TranType"] == "VAS";

		#endregion

		#endregion
	}
}
