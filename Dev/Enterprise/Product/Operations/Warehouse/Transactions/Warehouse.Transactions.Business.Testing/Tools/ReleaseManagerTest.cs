using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business.Tools;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class ReleaseManagerTest : WhsTestCaseWithFactory
	{
		#region Constructors

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorWithNullWarehouse()
		{
			new ReleaseManager(Factory, null);
		}

		[ExpectNoExceptions]
		public void TestConstructor()
		{
			var warehouse = Helper.CreateWarehouse("WHS");
			var releaseManager = new ReleaseManager(Factory, warehouse);
			AssertNotNull(releaseManager);
		}

		#endregion

		#region Test for Job

		#region TestReleaseJobForReference

		#region TestReleaseJobForReference_ByExternalReference

		public void TestReleaseJobForReference_ByExternalReference()
		{
			SetupEnvironmentData();

			TestReleaseJobForReference(Order1InWhs1.WD_ExternalReference, true, PackageJob1ForOrder1);
		}

		public void TestReleaseJobForReference_ByExternalReference_NotFound_DifferentWarehouse()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleaseJobForReference_NotFound(Order2InWhs2.WD_ExternalReference, releaseManager, true);
		}

		#endregion

		#region TestReleaseJobForReference_ByCustomerReference

		public void TestReleaseJobForReference_ByCustomerReference()
		{
			SetupEnvironmentData();

			TestReleaseJobForReference(Order1InWhs1.WD_CustomerReference, true, PackageJob1ForOrder1);
		}

		public void TestReleaseJobForReference_ByCustomerReference_NotFound_DifferentWarehouse()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleaseJobForReference_NotFound(Order2InWhs2.WD_CustomerReference, releaseManager, true);
		}

		#endregion

		#region TestReleaseJobForReference_ByDocketId

		public void TestReleaseJobForReference_ByDocketId()
		{
			SetupEnvironmentData();

			TestReleaseJobForReference(Order1InWhs1.WD_DocketID, true, PackageJob1ForOrder1);
		}

		public void TestReleaseJobForReference_ByDocketId_NotFound_DifferentWarehouse()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleaseJobForReference_NotFound(Order2InWhs2.WD_DocketID, releaseManager, true);
		}

		#endregion

		#region TestReleaseJobForReference_ByDocketReference

		public void TestReleaseJobForReference_ByDocketReference()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse2);
			var expectedPackageJobs = new List<PkgPackageJob>();
			expectedPackageJobs.Add(PackageJob2ForOrder2);

			TestReleaseJobForReference(Order2InWhs2.References[0].WX_Reference, releaseManager, true,
				expectedPackageJobs);
		}

		public void TestReleaseJobForReference_ByDocketReference_NotFound_DifferentWarehouse()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleaseJobForReference_NotFound(Order2InWhs2.References[0].WX_Reference, releaseManager, true);
		}

		#endregion

		#region TestReleaseJobForReference_NotFound_DoesNotExist

		public void TestReleaseJobForReference_NotFound_DoesNotExist()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleaseJobForReference_NotFound("xxx", releaseManager, true);
		}

		#endregion

		#region TestRelease_ReleaseJobForReference_Multiple

		public void TestRelease_ReleaseJobForReference_Multiple()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);

			var expectedPackageJobs = new List<PkgPackageJob>();
			expectedPackageJobs.Add(PackageJob1ForOrder1);
			expectedPackageJobs.Add(PackageJob3ForOrder3);

			TestReleaseJobForReference(Order1InWhs1.References[0].WX_Reference, releaseManager, true,
				expectedPackageJobs);
		}

		#endregion

		#region TestReleaseJobForReference_WithoutPkgPackageJob_SingleJob

		public void TestReleaseJobForReference_WithoutPkgPackageJob_SingleJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateGlbStaff("A.A", "AAA");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var pickedUpEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.PickedUp.Code);

			// Without a PkgJob
			var order1 =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m); // single without PkgJob
			Factory.Save();
			AssertNull("Precondition", order1.PackageJob);
			AssertEquals("Precondition - should not have a PickedUp event.", 0,
				order1.Logs.Find(pickedUpEventQuery).Length);

			var result1 = releaseManager.ReleaseJobForReference("O1");
			var jobInfo1 = result1.Items.Single();
			AssertEquals(true, result1.IsSuccess);
			AssertEquals(order1.PackageJob.KJ_ParentID, jobInfo1.PK);
			AssertEquals(order1.PackageJob.KJ_JobID, jobInfo1.JobId);
			AssertEquals(order1.WD_ExternalReference, jobInfo1.ParentJobNo);
			AssertEquals(order1.Client.OH_Code, jobInfo1.ClientCode);
			AssertEquals(order1.WD_RequiredDate.ToZDateTime(), jobInfo1.RequiredDate);
			AssertEquals(order1.WD_DocketStatus, jobInfo1.JobStatus);

			AssertEquals("Temporary PackageJob should not be marked as released.", false,
				order1.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertNull("Temporary PackageJob should not be saved to DB.",
				new BusinessObjectFactory().Load<PkgPackageJob>(order1.PackageJob.PK));
			AssertEquals("Order should have a PickedUp event.", 1, order1.Logs.Find(pickedUpEventQuery).Length);

			// With a PkgJob but without Packages
			var order2 =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m); // single with PkgJob
			PkgPackageJob.LoadOrCreatePackageJob(order2);
			Factory.Save();
			AssertNotNull("Precondition", order2.PackageJob);
			AssertEquals("Precondition - should not have a PickedUp event.", 0,
				order2.Logs.Find(pickedUpEventQuery).Length);

			var result2 = releaseManager.ReleaseJobForReference("O2");
			var jobInfo2 = result2.Items.Single();
			AssertEquals(true, result2.IsSuccess);
			AssertEquals(order2.PackageJob.KJ_ParentID, jobInfo2.PK);
			AssertEquals(order2.PackageJob.KJ_JobID, jobInfo2.JobId);
			AssertEquals(order2.WD_ExternalReference, jobInfo2.ParentJobNo);
			AssertEquals(order2.Client.OH_Code, jobInfo2.ClientCode);
			AssertEquals(order2.WD_RequiredDate.ToZDateTime(), jobInfo2.RequiredDate);
			AssertEquals(order2.WD_DocketStatus, jobInfo2.JobStatus);

			AssertEquals("If existing PackageJob have no packages, it should not be marked as released.", false,
				order2.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Order should have a PickedUp event.", 1, order2.Logs.Find(pickedUpEventQuery).Length);

			// With a PkgJob and with Packages
			var order3 =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m); // single with PkgJob
			PkgPackageJob.LoadOrCreatePackageJob(order3).Packages.AddNew(Constants.PkgUnit.Box);
			Factory.Save();
			AssertEquals("Precondition", 1, order3.PackageJob.Packages.Count);
			AssertEquals("Precondition - should not have a PUP event.", 0, order3.Logs.Find(pickedUpEventQuery).Length);

			var result3 = releaseManager.ReleaseJobForReference("O3");
			var jobInfo3 = result3.Items.Single();
			AssertEquals(true, result3.IsSuccess);
			AssertEquals(string.Empty, result1.Result.ErrorMessage);
			AssertEquals(order3.PackageJob.KJ_ParentID, jobInfo3.PK);
			AssertEquals(order3.PackageJob.KJ_JobID, jobInfo3.JobId);
			AssertEquals(order3.WD_ExternalReference, jobInfo3.ParentJobNo);
			AssertEquals(order3.Client.OH_Code, jobInfo3.ClientCode);
			AssertEquals(order3.WD_RequiredDate.ToZDateTime(), jobInfo3.RequiredDate);
			AssertEquals(order3.WD_DocketStatus, jobInfo3.JobStatus);

			AssertEquals("If existing PackageJob have packages, it should be marked as released.", true,
				order3.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Order should have a PickedUp event.", 1, order3.Logs.Find(pickedUpEventQuery).Length);
		}

		#endregion

		#region TestReleaseJobForReference_WithoutPkgPackageJob_MultipleJobs

		public void TestReleaseJobForReference_WithoutPkgPackageJob_MultipleJobs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var staff = Helper.CreateGlbStaff("A.A", "AAA");
			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var pickedUpEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.PickedUp.Code);

			// Multiple jobs with same reference
			var order1 =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 1m); // without PkgJob
			var order2 =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1,
					2m); // with PkgJob but without Packages
			var order3 =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1,
					3m); // with PkgJob and with Packages
			order2.WD_ExternalReferenceSplit = 1;
			order3.WD_ExternalReferenceSplit = 2;
			PkgPackageJob.LoadOrCreatePackageJob(order2);
			PkgPackageJob.LoadOrCreatePackageJob(order3).Packages.AddNew(Constants.PkgUnit.Box);
			Factory.Save();
			AssertNull("Precondition", order1.PackageJob);
			AssertNotNull("Precondition", order2.PackageJob);
			AssertEquals("Precondition", 1, order3.PackageJob.Packages.Count);
			AssertEquals("Precondition - should not have a PickedUp event.", 0,
				order1.Logs.Find(pickedUpEventQuery).Length);
			AssertEquals("Precondition - should not have a PickedUp event.", 0,
				order2.Logs.Find(pickedUpEventQuery).Length);
			AssertEquals("Precondition - should not have a PickedUp event.", 0,
				order3.Logs.Find(pickedUpEventQuery).Length);

			var result = releaseManager.ReleaseJobForReference("O1");
			var orders = new[] { order1, order2, order3 };
			foreach (var order in orders)
			{
				var matchingJobInfo = result.Items.Single(jobInfo => jobInfo.PK == order.PackageJob.KJ_ParentID);
				AssertEquals(true, result.IsSuccess);
				AssertEquals("O1", matchingJobInfo.ParentJobNo);
				AssertEquals(order.PackageJob.KJ_JobID, matchingJobInfo.JobId);
				AssertEquals(order.Client.OH_Code, matchingJobInfo.ClientCode);
				AssertEquals(order.WD_RequiredDate.ToZDateTime(), matchingJobInfo.RequiredDate);
				AssertEquals(order.WD_DocketStatus, matchingJobInfo.JobStatus);
			}

			AssertNull("Temporary Package job should not be saved to DB.",
				new BusinessObjectFactory().Load<PkgPackageJob>(order1.PackageJob.PK));
			AssertEquals("When multiple PackageJobs returned, none of them should be marked as released.", false, order1.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("When multiple PackageJobs returned, none of them should be marked as released.", false, order2.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("When multiple PackageJobs returned, none of them should be marked as released.", false, order3.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("When multiple PackageJobs returned, none of their Orders should receive PickedUp event.", 0,
				order1.Logs.Find(pickedUpEventQuery).Length);
			AssertEquals("When multiple PackageJobs returned, none of their Orders should receive PickedUp event.", 0,
				order2.Logs.Find(pickedUpEventQuery).Length);
			AssertEquals("When multiple PackageJobs returned, none of their Orders should receive PickedUp event.", 0,
				order3.Logs.Find(pickedUpEventQuery).Length);
		}

		#endregion

		#region TestReleaseForJobReference_ByPickNumber

		public void TestReleaseForJobReference_ByPickNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickNo = "P1";

			var packageJob1 = order1.PackageJob;
			AssertNotNull("Precondition: order has package job.", packageJob1);
			var packageInPackageJob1 = packageJob1.Packages.AddNew();
			var packageJob2 = order2.PackageJob;
			AssertNotNull("Precondition: order has package job.", packageJob2);
			var packageInPackageJob2 = packageJob2.Packages.AddNew();
			Factory.Save();

			Assert("Precondition: package job has packages.", order1.PackageJob.Packages.Count > 0);
			Assert("Precondition: package job has packages.", order2.PackageJob.Packages.Count > 0);
			Assert("Precondition: pick has packages.", pick.OuterPackages.Count > 0);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var result = releaseManager.ReleaseJobForReference("P1");
			var jobInfos = result.Items;
			AssertEquals(true, result.IsSuccess);
			AssertEquals(1, jobInfos.Length);

			var jobInfo = jobInfos[0];
			AssertEquals("Job info only contains job id information.", "P1", jobInfo.JobId);
			AssertEquals("Job info only contains job id information.", true, string.IsNullOrEmpty(jobInfo.ParentJobNo));
			AssertEquals("Job info only contains job id information.", true, string.IsNullOrEmpty(jobInfo.JobStatus));
			AssertEquals("Job info only contains job id information.", Guid.Empty, jobInfo.PK);
			AssertEquals("Job info only contains job id information.", DateTime.MinValue, jobInfo.RequiredDate);
			AssertEquals("Job info only contains job id information.", true, string.IsNullOrEmpty(jobInfo.ClientCode));

			AssertEquals(true, packageJob1.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(true, packageInPackageJob1.KP_IsReleasedViaJob);
			AssertEquals(true, packageJob2.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(true, packageInPackageJob2.KP_IsReleasedViaJob);

			var pickedUpEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.PickedUp.Code);
			AssertEquals("Order should have a PickedUp event.", 1, order1.Logs.Find(pickedUpEventQuery).Length);
			AssertEquals("Order should have a PickedUp event.", 1, order2.Logs.Find(pickedUpEventQuery).Length);
		}

		public void TestReleaseForJobReference_ByPickNumber_OrderWithPickNumberAsReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "P1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickNo = "P1";

			var packageJob1 = order1.PackageJob;
			AssertNotNull("Precondition: order has package job.", packageJob1);
			var packageInPackageJob1 = packageJob1.Packages.AddNew();
			var packageJob2 = order2.PackageJob;
			AssertNotNull("Precondition: order has package job.", packageJob2);
			var packageInPackageJob2 = packageJob2.Packages.AddNew();
			Factory.Save();

			Assert("Precondition: package job has packages.", order1.PackageJob.Packages.Count > 0);
			Assert("Precondition: package job has packages.", order2.PackageJob.Packages.Count > 0);
			Assert("Precondition: pick has packages.", pick.OuterPackages.Count > 0);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var result = releaseManager.ReleaseJobForReference("P1");
			var jobInfos = result.Items;
			AssertEquals(true, result.IsSuccess);
			AssertEquals(1, jobInfos.Length);

			var jobInfo = jobInfos[0];
			AssertEquals("Job info only contains job id information.", "P1", jobInfo.JobId);
			AssertEquals("Job info only contains job id information.", true, string.IsNullOrEmpty(jobInfo.ParentJobNo));
			AssertEquals("Job info only contains job id information.", true, string.IsNullOrEmpty(jobInfo.JobStatus));
			AssertEquals("Job info only contains job id information.", Guid.Empty, jobInfo.PK);
			AssertEquals("Job info only contains job id information.", DateTime.MinValue, jobInfo.RequiredDate);
			AssertEquals("Job info only contains job id information.", true, string.IsNullOrEmpty(jobInfo.ClientCode));

			AssertEquals(true, packageJob1.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(true, packageInPackageJob1.KP_IsReleasedViaJob);
			AssertEquals(true, packageJob2.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(true, packageInPackageJob2.KP_IsReleasedViaJob);

			var pickedUpEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.PickedUp.Code);
			AssertEquals("Order should have a PickedUp event.", 1, order1.Logs.Find(pickedUpEventQuery).Length);
			AssertEquals("Order should have a PickedUp event.", 1, order2.Logs.Find(pickedUpEventQuery).Length);
		}

		public void TestReleaseForJobReference_ByPickNumber_DBHits()
		{
			var data = new TestDataSimpleEnvironment(Factory);

			var numberOfOrders = 10;
			var quantity = 10m;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, quantity * numberOfOrders);

			var ordersList = new List<WhsOrder>();
			for (var i = 0; i < numberOfOrders; i++)
			{
				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, i.ToString(), data.Part1,
					quantity);
				ordersList.Add(order);
			}

			Factory.Save();

			var pick = Helper.CreatePickNew(ordersList.ToArray());
			pick.WP_PickNo = "P1";

			foreach (var order in ordersList)
			{
				order.PackageJob.Packages.AddNew();
			}

			Factory.Save();

			Assert("Precondition: All orders have package job.", ordersList.All(order => order.PackageJob != null));
			Assert("Precondition: package jobs have packages.",
				ordersList.All(order => order.PackageJob.Packages.Count > 0));
			Assert("Precondition: pick has packages.", pick.OuterPackages.Count > 0);

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var expectedDbHits = new Dictionary<string, int>()
			{
				{ JobHeaderSchema.Constants.TableName, 10 },
				{ PkgPackageSchema.Constants.TableName, 1 },
				{ PkgPackageJobSchema.Constants.TableName, 1 },
				{ ProcessCompanyLinkRuleSchema.Constants.TableName, 1 },
				{ ProcessTaskNotificationSchema.Constants.TableName, 1 },
				{ ProcessTasksSchema.Constants.TableName, 2 }, // extra hit from PUP milestone
				{ ProcessTaskTemplateSchema.Constants.TableName, 2 },
				{ StmALogSchema.Constants.TableName, 2 },
				{ StmEventSchema.Constants.TableName, 1 },
				{ WhsDocketSchema.Constants.TableName, 2 }, // extra hit from Pick loading Orders
				{ WhsPickSchema.Constants.TableName, 1 },
				{ WhsWarehouseSchema.Constants.TableName, 1 },
				{ OrgMiscServSchema.Constants.TableName, 1 },
				{ OrgHeaderSchema.Constants.TableName, 1 },
				{ GlbStaffSchema.Constants.TableName, 1 }
			};

			var releaseManager = new ReleaseManager(newFactory, data.Whs1);
			using (AssertDbHitsWithUsefulQueryInformation(expectedDbHits, newFactory))
			{
				var result = releaseManager.ReleaseJobForReference("P1");
				var jobInfos = result.Items;
				AssertEquals(true, result.IsSuccess);
				AssertEquals(1, jobInfos.Length);
			}

			var pickInOtherFactory = newFactory.Load<WhsPick>(pick.PK);
			Assert("All package jobs are released.",
				pickInOtherFactory.Orders.Cast<WhsOrder>().All(order => order.PackageJob.KJ_ReleasedTimeUtc.IsValid));
		}

		#endregion

		#region TestReleaseJobForReference_DBHits

		public void TestReleaseJobForReference_DBHits()
		{
			const int NumberOfOrdersToCreate = 10;
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_IsPickByUOMEnabled = true;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Box)).F3_UOMType =
				UOMPackTypesList.Codes.Case;
			Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, Constants.PkgUnit.Unit)).F3_UOMType =
				UOMPackTypesList.Codes.SplitCase;
			data.Part1.PartUnits.RemoveAndDeleteAll();
			Helper.CreateProductUnit(data.Part1, data.Part1.OP_StockKeepingUnit, Constants.PkgUnit.Box, 10m);
			var whsGroup = Helper.CreateWhsCartonGroup("1", "WhsOrg");
			var whsSize = Helper.CreateWhsCartonSize("BIG", 10m, 10m, 10m, 0m, 100m, 999, 100, Constants.Length.Metres,
				Constants.Weight.Kilograms);
			whsGroup.CartonSizes.Add(whsSize);
			data.Whs1.WarehouseAddress.Header.MiscServ.OM_WCG_CartonGroup = whsGroup.PK;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, NumberOfOrdersToCreate * 25m);
			var today = ZDateTime.Today;
			var orders = new WhsOrder[NumberOfOrdersToCreate];
			for (int i = 0; i < NumberOfOrdersToCreate; i++)
			{
				orders[i] = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O" + i, today.AddDays(-10).ToOffset(),
					data.Part1, 25m);
			}

			Factory.Save();

			var pick = Helper.CreatePickNew(orders);
			pick.WP_CartoniseSplitCases = true;
			pick.WP_PickCasesByLabel = true;
			Factory.Save();
			pick.AllocatePackageLabels();
			foreach (var order in orders)
			{
				var packages = order.PackageJob.Packages;
				AssertEquals("Precondition - should create 2x cases and 1x split case.", 3, packages.Count);
				AssertEquals("Precondition", 2, packages.Count(p => p.KP_F3_NKPackType == Constants.PkgUnit.Box));
				AssertEquals("Precondition", 1, packages.Count(p => p.KP_F3_NKPackType == Constants.PkgUnit.Carton));
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var expectedHits = new Dictionary<string, int>
			{
				{ JobHeaderSchema.Constants.TableName, 10 },
			};

			using (AssertDbHitsWithUsefulQueryInformation(expectedHits, newFactory,
					   ignoredNotSpecifiedUnlessGreaterThan5Hits: true))
			{
				var releaseManager = new ReleaseManager(newFactory, newFactory.Load<WhsWarehouse>(data.Whs1.PK));
				releaseManager.ReleaseJobForReference(pick.WP_PickNo);
			}
		}

		#endregion

		#region TestReleaseForJobReference_LoadingStatus

		public void TestReleaseForJobReference_LoadingStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var carrierServicelevel = data.Org1.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";
			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			var load = Helper.CreateWhsLoad(data.Org1, data.Whs1.DefaultOutboundDockDoorLocation, transportUnit: truck, startTime: DateTimeOffset.Now);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 5m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 5m);
			Factory.Save();

			Helper.CreatePickNew(order1, order2);

			var pickLine1 = order1.Lines.Single().PickLines.Single();
			pickLine1.WZ_PickedDateTime = ZDateTimeOffset.Now;

			var pickLine2 = order2.Lines.Single().PickLines.Single();
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			var package1 = order1.PackageJob.Packages.AddNew("CTN");
			package1.Pack(order1.Lines[0].ReleaseLines[0], 5m);
			var loadPkgPackagePivot1 = Helper.CreateLoadPkgPackagePivot(package1.PK, load);
			loadPkgPackagePivot1.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot1.WLP_GS_NKLoadingUser = "E";

			var package2 = order2.PackageJob.Packages.AddNew("CTN");
			package2.Pack(order2.Lines[0].ReleaseLines[0], 2m);
			var loadPkgPackagePivot2 = Helper.CreateLoadPkgPackagePivot(package2.PK, load);
			loadPkgPackagePivot2.WLP_LoadedTime = ZDateTimeOffset.Now;
			loadPkgPackagePivot2.WLP_GS_NKLoadingUser = "E";

			var package3 = order2.PackageJob.Packages.AddNew("CTN");
			package3.Pack(order2.Lines[0].ReleaseLines[0], 3m);
			Factory.Save();

			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var result1 = releaseManager.ReleaseJobForReference("O1");
			var jobInfos1 = result1.Items;
			AssertEquals(true, result1.IsSuccess);
			AssertEquals(1, jobInfos1.Length);

			var jobInfo1 = jobInfos1[0];
			AssertEquals("Job info 1 only contains correct information.", "P00000001", jobInfo1.JobId);
			AssertEquals("Job info 1 only contains correct information.", "O1", jobInfo1.ParentJobNo);
			AssertEquals("Job info 1 only contains correct information.", "LOA", jobInfo1.JobStatus);
			AssertEquals("Job info 1 only contains correct information.", order1.PK, jobInfo1.PK);
			AssertEquals("Job info 1 only contains correct information.", order1.WD_RequiredDate.ToZDateTime(), jobInfo1.RequiredDate);
			AssertEquals("Job info 1 only contains correct information.", order1.Client.OH_Code, jobInfo1.ClientCode);

			var result2 = releaseManager.ReleaseJobForReference("O2");
			var jobInfos2 = result2.Items;
			AssertEquals(true, result2.IsSuccess);
			AssertEquals(1, jobInfos2.Length);

			var jobInfo2 = jobInfos2[0];
			AssertEquals("Job info 2 only contains correct information.", "P00000002", jobInfo2.JobId);
			AssertEquals("Job info 2 only contains correct information.", "O2", jobInfo2.ParentJobNo);
			AssertEquals("Job info 2 only contains correct information.", "LDG", jobInfo2.JobStatus);
			AssertEquals("Job info 2 only contains correct information.", order2.PK, jobInfo2.PK);
			AssertEquals("Job info 2 only contains correct information.", order2.WD_RequiredDate.ToZDateTime(), jobInfo2.RequiredDate);
			AssertEquals("Job info 2 only contains correct information.", order2.Client.OH_Code, jobInfo2.ClientCode);
		}

		#endregion

		#endregion

		#region TestUnreleaseJobForReference

		#region TestUnreleaseJobForReference_ByExternalReference

		public void TestUnreleaseJobForReference_ByExternalReference()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse2);
			var expectedPackageJobs = new List<PkgPackageJob>();
			expectedPackageJobs.Add(PackageJob2ForOrder2);

			TestReleaseJobForReference(Order2InWhs2.WD_ExternalReference, releaseManager, false, expectedPackageJobs);
		}

		public void TestUnreleaseJobForReference_ByExternalReference_NotFound_DifferentWarehouse()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleaseJobForReference_NotFound(Order2InWhs2.WD_ExternalReference, releaseManager, false);
		}

		#endregion

		#region TestUnreleaseJobForReference_ByCustomerReference

		public void TestUnreleaseJobForReference_ByCustomerReference()
		{
			SetupEnvironmentData();

			TestReleaseJobForReference(Order1InWhs1.WD_CustomerReference, false, PackageJob1ForOrder1);
		}

		public void TestUnreleaseJobForReference_ByCustomerReference_NotFound_DifferentWarehouse()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleaseJobForReference_NotFound(Order2InWhs2.WD_CustomerReference, releaseManager, false);
		}

		#endregion

		#region TestUnreleaseJobForReference_ByDocketId

		public void TestUnreleaseJobForReference_ByDocketId()
		{
			SetupEnvironmentData();

			TestReleaseJobForReference(Order1InWhs1.WD_DocketID, false, PackageJob1ForOrder1);
		}

		public void TestUnreleaseJobForReference_ByDocketId_NotFound_DifferentWarehouse()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleaseJobForReference_NotFound(Order2InWhs2.WD_DocketID, releaseManager, false);
		}

		#endregion

		#region TestUnreleaseJobForReference_ByDocketReference

		public void TestUnreleaseJobForReference_ByDocketReference()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse2);
			var expectedPackageJobs = new List<PkgPackageJob>();
			expectedPackageJobs.Add(PackageJob2ForOrder2);
			TestReleaseJobForReference(Order2InWhs2.References[0].WX_Reference, releaseManager, false,
				expectedPackageJobs);
		}

		public void TestUnreleaseJobForReference_ByDocketReference_NotFound_DifferentWarehouse()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleaseJobForReference_NotFound(Order2InWhs2.References[0].WX_Reference, releaseManager, false);
		}

		#endregion

		public void TestUnreleaseJobForReference_NotFound_DoesNotExist()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleaseJobForReference_NotFound("xxx", releaseManager, false);
		}

		public void TestRelease_UnreleaseJobForReference_Multiple()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);

			var expectedPackageJobs = new List<PkgPackageJob>();
			expectedPackageJobs.Add(PackageJob1ForOrder1);
			expectedPackageJobs.Add(PackageJob3ForOrder3);

			TestReleaseJobForReference(Order1InWhs1.References[0].WX_Reference, releaseManager, false,
				expectedPackageJobs);
		}

		#region TestReleaseForJobReference_WithPickNumber

		public void TestUnreleaseForJobReference_ByPickNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickNo = "P1";

			var packageJob1 = order1.PackageJob;
			AssertNotNull("Precondition: order has package job.", packageJob1);
			var packageInPackageJob1 = packageJob1.Packages.AddNew();
			packageJob1.KJ_ReleasedTimeUtc = DateTime.UtcNow;
			var packageJob2 = order2.PackageJob;
			AssertNotNull("Precondition: order has package job.", packageJob2);
			var packageInPackageJob2 = packageJob2.Packages.AddNew();
			packageJob2.KJ_ReleasedTimeUtc = DateTime.UtcNow;
			Factory.Save();

			Assert("Precondition: package job has packages.", order1.PackageJob.Packages.Count > 0);
			Assert("Precondition: package job is released.", packageJob1.KJ_ReleasedTimeUtc.IsValid);
			Assert("Precondition: package is released.", packageInPackageJob1.KP_IsReleasedViaJob);
			Assert("Precondition: package job has packages.", order2.PackageJob.Packages.Count > 0);
			Assert("Precondition: package job is released.", packageJob2.KJ_ReleasedTimeUtc.IsValid);
			Assert("Precondition: package is released.", packageInPackageJob2.KP_IsReleasedViaJob);
			Assert("Precondition: pick has packages.", pick.OuterPackages.Count > 0);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var result = releaseManager.UnreleaseJobForReference("P1");
			var jobInfos = result.Items;
			AssertEquals(true, result.IsSuccess);
			AssertEquals(1, jobInfos.Length);

			var jobInfo = jobInfos[0];
			AssertEquals("Job info only contains job id information.", "P1", jobInfo.JobId);
			AssertEquals("Job info only contains job id information.", true, string.IsNullOrEmpty(jobInfo.ParentJobNo));
			AssertEquals("Job info only contains job id information.", true, string.IsNullOrEmpty(jobInfo.JobStatus));
			AssertEquals("Job info only contains job id information.", Guid.Empty, jobInfo.PK);
			AssertEquals("Job info only contains job id information.", DateTime.MinValue, jobInfo.RequiredDate);
			AssertEquals("Job info only contains job id information.", true, string.IsNullOrEmpty(jobInfo.ClientCode));

			AssertEquals(false, packageJob1.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(false, packageJob2.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(false, packageInPackageJob1.KP_IsReleasedViaJob);
			AssertEquals(false, packageInPackageJob2.KP_IsReleasedViaJob);
		}

		public void TestUnreleaseForJobReference_ByPickNumber_OrderWithPickNumberAsReference()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "P1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickNo = "P1";

			var packageJob1 = order1.PackageJob;
			AssertNotNull("Precondition: order has package job.", packageJob1);
			var packageInPackageJob1 = packageJob1.Packages.AddNew();
			packageJob1.KJ_ReleasedTimeUtc = ZDateTime.UtcNow;
			var packageJob2 = order2.PackageJob;
			AssertNotNull("Precondition: order has package job.", packageJob2);
			var packageInPackageJob2 = packageJob2.Packages.AddNew();
			packageJob2.KJ_ReleasedTimeUtc = ZDateTime.UtcNow;
			Factory.Save();

			Assert("Precondition: package job has packages.", order1.PackageJob.Packages.Count > 0);
			Assert("Precondition: package job is released.", packageJob1.KJ_ReleasedTimeUtc.IsValid);
			Assert("Precondition: package is released.", packageInPackageJob1.KP_IsReleasedViaJob);
			Assert("Precondition: package job has packages.", order2.PackageJob.Packages.Count > 0);
			Assert("Precondition: package job is released.", packageJob2.KJ_ReleasedTimeUtc.IsValid);
			Assert("Precondition: package is released.", packageInPackageJob2.KP_IsReleasedViaJob);
			Assert("Precondition: pick has packages.", pick.OuterPackages.Count > 0);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var result = releaseManager.UnreleaseJobForReference("P1");
			var jobInfos = result.Items;
			AssertEquals(true, result.IsSuccess);

			AssertEquals(1, jobInfos.Length);

			var jobInfo = jobInfos[0];
			AssertEquals("Job info only contains job id information.", "P1", jobInfo.JobId);
			AssertEquals("Job info only contains job id information.", true, string.IsNullOrEmpty(jobInfo.ParentJobNo));
			AssertEquals("Job info only contains job id information.", true, string.IsNullOrEmpty(jobInfo.JobStatus));
			AssertEquals("Job info only contains job id information.", Guid.Empty, jobInfo.PK);
			AssertEquals("Job info only contains job id information.", DateTime.MinValue, jobInfo.RequiredDate);
			AssertEquals("Job info only contains job id information.", true, string.IsNullOrEmpty(jobInfo.ClientCode));

			AssertEquals(false, packageJob1.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(false, packageJob2.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(false, packageInPackageJob1.KP_IsReleasedViaJob);
			AssertEquals(false, packageInPackageJob2.KP_IsReleasedViaJob);
		}

		#endregion

		#endregion

		#region TestReleaseJobForPk

		#region TestReleaseJobForPk

		public void TestReleaseJobForPk()
		{
			SetupEnvironmentData();

			TestReleaseJobForPk(Order1InWhs1, true);
		}

		#endregion

		#region TestReleaseJobForPk_NotFound

		public void TestReleaseJobForPk_NotFound()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleaseJobForPk_NotFound(ZGuid.NewZGuid(), releaseManager, true);
		}

		#endregion

		#region TestReleaseJobForPK_WithoutPackageJob

		public void TestReleaseJobForPK_WithoutPackageJob()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.CreateGlbStaff("A.A", "AAA");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var pickedUpEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.PickedUp.Code);

			// Without a PkgJob
			var order1 =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 4m); // single without PkgJob
			Factory.Save();
			AssertNull("Precondition", order1.PackageJob);
			AssertEquals("Precondition - should not have a PickedUp event.", 0,
				order1.Logs.Find(pickedUpEventQuery).Length);

			releaseManager.ReleaseJobForPk(order1.PK);
			AssertNull("Temporary PackageJob should not be created.", order1.PackageJob);
			AssertEquals("Order should have a PickedUp event.", 1, order1.Logs.Find(pickedUpEventQuery).Length);

			// With a PkgJob but without Packages
			var order2 =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 3m); // single with PkgJob
			PkgPackageJob.LoadOrCreatePackageJob(order2);
			Factory.Save();
			AssertNotNull("Precondition", order2.PackageJob);
			AssertEquals("Precondition - should not have a PickedUp event.", 0,
				order2.Logs.Find(pickedUpEventQuery).Length);

			releaseManager.ReleaseJobForPk(order2.PK);
			AssertEquals("If existing PackageJob have no packages, it should not be marked as released.", false,
				order2.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Order should have a PickedUp event.", 1, order2.Logs.Find(pickedUpEventQuery).Length);

			// With a PkgJob and with Packages
			var order3 =
				Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O3", data.Part1, 3m); // single with PkgJob
			PkgPackageJob.LoadOrCreatePackageJob(order3).Packages.AddNew(Constants.PkgUnit.Box);
			Factory.Save();
			AssertEquals("Precondition", 1, order3.PackageJob.Packages.Count);
			AssertEquals("Precondition - should not have a PUP event.", 0, order3.Logs.Find(pickedUpEventQuery).Length);

			releaseManager.ReleaseJobForPk(order3.PK);
			AssertEquals("If existing PackageJob have packages, it should be marked as released.", true,
				order3.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Order should have a PickedUp event.", 1, order3.Logs.Find(pickedUpEventQuery).Length);
		}

		#endregion

		#endregion

		#region TestUnreleaseJobForPk

		public void TestUnreleaseJobForPk()
		{
			SetupEnvironmentData();

			TestReleaseJobForPk(Order1InWhs1, false);
		}

		public void TestUnreleaseJobForPk_NotFound()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleaseJobForPk_NotFound(ZGuid.NewZGuid(), releaseManager, false);
		}

		#endregion

		#endregion

		#region Test for package

		#region TestReleasePackageForReference

		public void TestReleasePackageForReference_Unique()
		{
			SetupEnvironmentData();

			var expectedPackages = new List<PkgPackage>();
			expectedPackages.Add(Package2ForPackageJob1);

			TestReleasePackageForReference("PK02", true, expectedPackages);
		}

		public void TestReleasePackageForReference_Multiple()
		{
			SetupEnvironmentData();

			var expectedPackages = new List<PkgPackage>();
			expectedPackages.Add(Package1ForPackageJob1);
			expectedPackages.Add(Package5ForPackageJob3);

			TestReleasePackageForReference("PK01", true, expectedPackages);
		}

		public void TestReleasePackageForReference_InnerPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var outerPackage = packageJob.Packages.AddNew("PLT", "Outer-1");
			var innerPackage = outerPackage.Packages.AddNew("CAS", "Inner-1");
			var staff = Helper.CreateGlbStaff("US1", "User1");
			Factory.Save();

			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			AssertEquals("Precondition:", false, outerPackage.IsReleased);
			AssertEquals("Precondition:", false, innerPackage.IsReleased);

			var actualPackages = releaseManager.ReleasePackageForReference(outerPackage.KP_PackageID).Items;
			AssertEquals(1, actualPackages.Length);
			AssertEquals(true, actualPackages[0].IsReleased);

			var result = releaseManager.ReleasePackageForReference(innerPackage.KP_PackageID);
			AssertEquals(false, result.IsSuccess);
			AssertEquals(result.Result.ErrorMessage,
				"Package 'Inner-1' is not found, not an outer or already released!");
			AssertNull(result.Items);

			AssertEquals("Inner package must not be released.", false, innerPackage.IsReleased);
		}

		public void TestReleasePackageForReference_SSCCBarCode()
		{
			SetupEnvironmentData();
			Client.CustomsCodes.AddNew(OrgCusCode.CodeTypes.GS1, "1234567");
			Package1ForPackageJob1.KP_PackageID = "012345670000000015";
			Factory.Save();

			AssertEquals(true, Package1ForPackageJob1.IsPackageIdValidSSCCBarCode);

			var expectedPackages = new List<PkgPackage>();
			expectedPackages.Add(Package1ForPackageJob1);

			TestReleasePackageForReference("00012345670000000015", true, expectedPackages);
		}

		public void TestReleasePackageForReference_NotFound_DoesNotExist()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleasePackageForReference_NotFound("xxx", releaseManager, true);
		}

		public void TestReleasePackageForReference_NotFound_DifferentWarehouse()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleasePackageForReference_NotFound("PK03", releaseManager, true);
		}

		public void TestReleasePackageForReference_CanNotReleaseBecauseIsHeld()
		{
			SetupEnvironmentData();
			Package2ForPackageJob1.KP_ReleasedTimeUtc = ZDateTime.Empty;
			Package2ForPackageJob1.KP_IsHeld = true;
			var packageReference = Package2ForPackageJob1.KP_PackageID;
			var releaseManager = new ReleaseManager(Factory, Warehouse1);

			var result1 = releaseManager.ReleasePackageForReference(packageReference);
			AssertEquals(false, result1.IsSuccess);
			AssertEquals(result1.Result.ErrorMessage, "A package that is held cannot be released");
			AssertNull(result1.Items);

			Package2ForPackageJob1.KP_IsHeld = false;
			var result2 = releaseManager.ReleasePackageForReference(packageReference);
			AssertEquals("The package is not held now so it should be released without problems", true,
				result2.IsSuccess);
			AssertEquals(1, result2.Items.Length);
		}

		public void TestRelease_ReleasePackagePKWhichWasReleasedBefore()
		{
			AssertAlreadyReleasedOrCancelledPackagePk(true);
		}

		public void TestRelease_ReleasePackageWhichWasReleasedBefore()
		{
			AssertAlreadyReleasedOrCancelledPackageId(true);
		}

		#endregion

		#region TestUnreleasePackageForReference

		public void TestUnreleasePackageForReference_Unique()
		{
			SetupEnvironmentData();

			var expectedPackages = new List<PkgPackage>();
			expectedPackages.Add(Package2ForPackageJob1);

			TestReleasePackageForReference("PK02", false, expectedPackages);
		}

		public void TestUnreleasePackageForReference_Multiple()
		{
			SetupEnvironmentData();

			var expectedPackages = new List<PkgPackage>();
			expectedPackages.Add(Package1ForPackageJob1);
			expectedPackages.Add(Package5ForPackageJob3);

			TestReleasePackageForReference("PK01", false, expectedPackages);
		}

		public void TestUnreleasePackageForReference_NotFound_DoesNotExist()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleasePackageForReference_NotFound("xxx", releaseManager, false);
		}

		public void TestUnreleasePackageForReference_NotFound_DifferentWarehouse()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleasePackageForReference_NotFound("PK03", releaseManager, false);
		}

		public void TestRelease_ReleasePackageWhichWasCancelledBefore()
		{
			AssertAlreadyReleasedOrCancelledPackageId(false);
		}

		public void TestRelease_ReleasePackagePKWhichWasCancelledBefore()
		{
			AssertAlreadyReleasedOrCancelledPackagePk(false);
		}

		#endregion

		#region TestReleasePackageForPk

		public void TestReleasePackageForPk()
		{
			SetupEnvironmentData();

			TestReleasePackageForPk(Package1ForPackageJob1, true);
		}

		public void TestReleasePackageForPk_NotFound()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleasePackageForPk_NotFound(ZGuid.NewZGuid(), releaseManager, true);
		}

		public void TestReleasePackageForPK_CanNotReleaseBecauseIsHeld()
		{
			SetupEnvironmentData();
			Package2ForPackageJob1.KP_ReleasedTimeUtc = ZDateTime.Empty;
			Package2ForPackageJob1.KP_IsHeld = true;
			var packagePK = Package2ForPackageJob1.PK;
			var releaseManager = new ReleaseManager(Factory, Warehouse1);

			var result1 = releaseManager.ReleasePackageForPk(packagePK);
			AssertEquals(false, result1.IsSuccess);
			AssertEquals(result1.ErrorMessage, $"A package that is held cannot be released");
			Package2ForPackageJob1.KP_IsHeld = false;
			var result2 = releaseManager.ReleasePackageForPk(packagePK);
			AssertEquals(true, result2.IsSuccess);
			AssertEquals("The package is not held now so it should be released without problems", "",
				result2.ErrorMessage);
		}

		#endregion

		#region TestUnreleasePackageForPk

		public void TestUnreleasePackageForPk()
		{
			SetupEnvironmentData();

			TestReleasePackageForPk(Package1ForPackageJob1, false);
		}

		public void TestUnreleasePackageForPk_NotFound()
		{
			SetupEnvironmentData();

			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			AssertReleasePackageForPk_NotFound(ZGuid.NewZGuid(), releaseManager, false);
		}

		#endregion

		#endregion

		public void TestGetCannotReleasePackageErrorMessage()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			Helper.CreatePickNew(order);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			Factory.Save();

			AssertEquals("Package 'ABC' can't be released because it has not been fully picked.",
				ReleaseManager.GetCannotReleasePackageErrorMessage(package));
		}

		#region TestRelease_PreventReleaseOfPackageIfNotPicked

		#region TestReleaseJob

		#region TestReleaseJob_PreventReleaseOfPackageIfNotPicked

		public void TestReleaseJobForPk_PackageNotPicked_PreventReleaseOfPackageIfNotPickedEnabled()
		{
			TestReleaseJob_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(preventReleaseSettingEnabled: true,
				(releaseManager, order) => releaseManager.ReleaseJobForPk(order.PK));
		}

		public void TestReleaseJobForPk_PackageNotPicked_PreventReleaseOfPackageIfNotPickedDisabled()
		{
			TestReleaseJob_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(preventReleaseSettingEnabled: false,
				(releaseManager, order) => releaseManager.ReleaseJobForPk(order.PK));
		}

		public void TestReleaseJobForReference_PackageNotPicked_PreventReleaseOfPackageIfNotPickedEnabled()
		{
			TestReleaseJob_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(preventReleaseSettingEnabled: true,
				(releaseManager, order) => releaseManager.ReleaseJobForReference(order.WD_DocketID).Result);
		}

		public void TestReleaseJobForReference_PackageNotPicked_PreventReleaseOfPackageIfNotPickedDisabled()
		{
			TestReleaseJob_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(preventReleaseSettingEnabled: false,
				(releaseManager, order) => releaseManager.ReleaseJobForReference(order.WD_DocketID).Result);
		}

		void TestReleaseJob_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(bool preventReleaseSettingEnabled,
			Func<ReleaseManager, WhsOrder, ActionResult> releasePackageAction)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_PreventReleaseOfPackageIfNotPicked = preventReleaseSettingEnabled;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			packingHelper.CreatePackageDivot(package, pickLine);
			Factory.Save();

			AssertEquals("Precondition: WW_PreventReleaseOfPackageIfNotPicked setting.", preventReleaseSettingEnabled,
				data.Whs1.WW_PreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: pick line on package is not picked.", true,
				PickLinePair.New(pickLine).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package job is not released.", false, order.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Precondition: package is not released.", false, package.KP_IsReleasedViaJob);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);

			if (preventReleaseSettingEnabled)
			{
				var result = releasePackageAction(releaseManager, order);
				AssertEquals(false, result.IsSuccess);
				AssertEquals("Release of job is not allowed.", result.ErrorMessage,
					"Job can't be released because some packages on the job are not yet fully picked.");
			}
			else
			{
				var result = releasePackageAction(releaseManager, order);
				AssertEquals(true, result.IsSuccess);
			}

			AssertEquals(!preventReleaseSettingEnabled, order.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(!preventReleaseSettingEnabled, package.KP_IsReleasedViaJob);
		}

		public void TestReleaseForJobReference_ByPickNumber_PackageNotPicked_PreventReleaseOfPackageIfNotPickedEnabled()
		{
			TestReleaseForJobReference_ByPickNumber_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: true);
		}

		public void
			TestReleaseForJobReference_ByPickNumber_PackageNotPicked_PreventReleaseOfPackageIfNotPickedDisabled()
		{
			TestReleaseForJobReference_ByPickNumber_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: false);
		}

		void TestReleaseForJobReference_ByPickNumber_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
			bool preventReleaseSettingEnabled)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Whs1.WW_PreventReleaseOfPackageIfNotPicked = preventReleaseSettingEnabled;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickNo = "P1";

			var packingHelper = new PackingTestHelper(Factory);

			var packageJob1 = order1.PackageJob;
			AssertNotNull("Precondition: order has package job.", packageJob1);
			var packageInPackageJob1 = packageJob1.Packages.AddNew();
			var pickLine1 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 10m);
			packingHelper.CreatePackageDivot(packageInPackageJob1, pickLine1);

			var packageJob2 = order2.PackageJob;
			AssertNotNull("Precondition: order has package job.", packageJob2);
			var packageInPackageJob2 = packageJob2.Packages.AddNew();
			var pickLine2 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 20m);
			packingHelper.CreatePackageDivot(packageInPackageJob2, pickLine2);
			Factory.Save();

			AssertEquals("Precondition: WW_PreventReleaseOfPackageIfNotPicked setting.", preventReleaseSettingEnabled,
				data.Whs1.WW_PreventReleaseOfPackageIfNotPicked);
			Assert("Precondition: package job has packages.", order1.PackageJob.Packages.Count > 0);
			Assert("Precondition: package job has packages.", order2.PackageJob.Packages.Count > 0);
			Assert("Precondition: pick has packages.", pick.OuterPackages.Count > 0);
			AssertEquals("Precondition: pick line on package1 is not picked.", true,
				PickLinePair.New(pickLine1).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: pick line on package2 is not picked.", true,
				PickLinePair.New(pickLine2).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package job is not released.", false, packageJob1.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Precondition: package is not released.", false, packageInPackageJob1.KP_IsReleasedViaJob);
			AssertEquals("Precondition: package job is not released.", false, packageJob2.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Precondition: package is not released.", false, packageInPackageJob2.KP_IsReleasedViaJob);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);

			if (preventReleaseSettingEnabled)
			{
				var result = releaseManager.ReleaseJobForReference("P1");
				AssertEquals(false, result.IsSuccess);
				AssertEquals("Release of package is not allowed.", result.Result.ErrorMessage,
					"Job can't be released because some packages on the job are not yet fully picked.");
			}
			else
			{
				var result = releaseManager.ReleaseJobForReference("P1");
				AssertEquals(true, result.IsSuccess);
			}

			AssertEquals(!preventReleaseSettingEnabled, packageJob1.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(!preventReleaseSettingEnabled, packageInPackageJob1.KP_IsReleasedViaJob);
			AssertEquals(!preventReleaseSettingEnabled, packageJob2.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(!preventReleaseSettingEnabled, packageInPackageJob2.KP_IsReleasedViaJob);
		}

		public void TestReleaseJob_SomePackageNotPicked_PreventReleaseOfPackageIfNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_PreventReleaseOfPackageIfNotPicked = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 4m);
			var package1 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package1.KP_PackageID = "ABC";
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickLine2 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 6m);
			var package2 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package2.KP_PackageID = "DEF";
			packingHelper.CreatePackageDivot(package2, pickLine2);
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: WW_PreventReleaseOfPackageIfNotPicked setting.", true,
				data.Whs1.WW_PreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: pick line 1 on package is not picked.", true,
				PickLinePair.New(pickLine1).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: pick line 2 on package is picked.", false,
				PickLinePair.New(pickLine2).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package job is not released.", false, order.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Precondition: package1 is not released.", false, package1.KP_IsReleasedViaJob);
			AssertEquals("Precondition: package2 is not released.", false, package2.KP_IsReleasedViaJob);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var result = releaseManager.ReleaseJobForReference(order.WD_DocketID);
			AssertEquals(false, result.IsSuccess);
			AssertEquals("Release of job is not allowed.", result.Result.ErrorMessage,
				"Job can't be released because some packages on the job are not yet fully picked.");
			AssertEquals("Package job is not released due to the enabled setting plus the package is not picked.",
				false, order.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Package job is not released due to the enabled setting plus the package is not picked.",
				false, package1.KP_IsReleasedViaJob);
			AssertEquals("Package job is not released due to the enabled setting plus the package is not picked.",
				false, package2.KP_IsReleasedViaJob);
		}

		public void TestReleaseJob_PackagePicked_PreventReleaseOfPackageIfNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_PreventReleaseOfPackageIfNotPicked = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			packingHelper.CreatePackageDivot(package, pickLine);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: WW_PreventReleaseOfPackageIfNotPicked setting.", true,
				data.Whs1.WW_PreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: pick line on package is picked.", false,
				PickLinePair.New(pickLine).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package job is not released.", false, order.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Precondition: package is not released.", false, package.KP_IsReleasedViaJob);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);

			releaseManager.ReleaseJobForReference(order.WD_DocketID);
			AssertEquals("Package job is released.", true, order.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Package job is released.", true, package.KP_IsReleasedViaJob);
		}

		#endregion

		#region TestReleaseJob_Org_PreventReleaseOfPackageIfNotPicked

		public void TestReleaseJobForPk_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedEnabled()
		{
			TestReleaseJob_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: true,
				(releaseManager, order) => releaseManager.ReleaseJobForPk(order.PK));
		}

		public void TestReleaseJobForPk_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedDisabled()
		{
			TestReleaseJob_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: false,
				(releaseManager, order) => releaseManager.ReleaseJobForPk(order.PK));
		}

		public void TestReleaseJobForReference_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedEnabled()
		{
			TestReleaseJob_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: true,
				(releaseManager, order) => releaseManager.ReleaseJobForReference(order.WD_DocketID).Result);
		}

		public void TestReleaseJobForReference_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedDisabled()
		{
			TestReleaseJob_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: false,
				(releaseManager, order) => releaseManager.ReleaseJobForReference(order.WD_DocketID).Result);
		}

		void TestReleaseJob_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
			bool preventReleaseSettingEnabled, Func<ReleaseManager, WhsOrder, ActionResult> releasePackageAction)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked = preventReleaseSettingEnabled;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			packingHelper.CreatePackageDivot(package, pickLine);
			Factory.Save();

			AssertEquals("Precondition: OM_WhsPreventReleaseOfPackageIfNotPicked setting.",
				preventReleaseSettingEnabled, data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: pick line on package is not picked.", true,
				PickLinePair.New(pickLine).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package job is not released.", false, order.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Precondition: package is not released.", false, package.KP_IsReleasedViaJob);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);

			if (preventReleaseSettingEnabled)
			{
				var result = releasePackageAction(releaseManager, order);
				AssertEquals(false, result.IsSuccess);
				AssertEquals("Release of job is not allowed.", result.ErrorMessage,
					"Job can't be released because some packages on the job are not yet fully picked.");
			}
			else
			{
				var result = releasePackageAction(releaseManager, order);
				AssertEquals(true, result.IsSuccess);
			}

			AssertEquals(!preventReleaseSettingEnabled, order.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(!preventReleaseSettingEnabled, package.KP_IsReleasedViaJob);
		}

		public void
			TestReleaseForJobReference_Org_ByPickNumber_PackageNotPicked_PreventReleaseOfPackageIfNotPickedEnabled()
		{
			TestReleaseForJobReference_Org_ByPickNumber_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: true);
		}

		public void
			TestReleaseForJobReference_Org_ByPickNumber_PackageNotPicked_PreventReleaseOfPackageIfNotPickedDisabled()
		{
			TestReleaseForJobReference_Org_ByPickNumber_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: false);
		}

		void TestReleaseForJobReference_Org_ByPickNumber_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
			bool preventReleaseSettingEnabled)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked = preventReleaseSettingEnabled;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 30m);
			var order1 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var order2 = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O2", data.Part1, 20m);
			Factory.Save();

			var pick = Helper.CreatePickNew(order1, order2);
			pick.WP_PickNo = "P1";

			var packingHelper = new PackingTestHelper(Factory);

			var packageJob1 = order1.PackageJob;
			AssertNotNull("Precondition: order has package job.", packageJob1);
			var packageInPackageJob1 = packageJob1.Packages.AddNew();
			var pickLine1 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 10m);
			packingHelper.CreatePackageDivot(packageInPackageJob1, pickLine1);

			var packageJob2 = order2.PackageJob;
			AssertNotNull("Precondition: order has package job.", packageJob2);
			var packageInPackageJob2 = packageJob2.Packages.AddNew();
			var pickLine2 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 20m);
			packingHelper.CreatePackageDivot(packageInPackageJob2, pickLine2);
			Factory.Save();

			AssertEquals("Precondition: OM_WhsPreventReleaseOfPackageIfNotPicked setting.",
				preventReleaseSettingEnabled, data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked);
			Assert("Precondition: package job has packages.", order1.PackageJob.Packages.Count > 0);
			Assert("Precondition: package job has packages.", order2.PackageJob.Packages.Count > 0);
			Assert("Precondition: pick has packages.", pick.OuterPackages.Count > 0);
			AssertEquals("Precondition: pick line on package1 is not picked.", true,
				PickLinePair.New(pickLine1).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: pick line on package2 is not picked.", true,
				PickLinePair.New(pickLine2).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package job is not released.", false, packageJob1.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Precondition: package is not released.", false, packageInPackageJob1.KP_IsReleasedViaJob);
			AssertEquals("Precondition: package job is not released.", false, packageJob2.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Precondition: package is not released.", false, packageInPackageJob2.KP_IsReleasedViaJob);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);

			if (preventReleaseSettingEnabled)
			{
				var result = releaseManager.ReleaseJobForReference("P1");
				AssertEquals(false, result.IsSuccess);
				AssertEquals("Release of package is not allowed.", result.Result.ErrorMessage,
					"Job can't be released because some packages on the job are not yet fully picked.");
			}
			else
			{
				var result = releaseManager.ReleaseJobForReference("P1");
				AssertEquals(true, result.IsSuccess);
			}

			AssertEquals(!preventReleaseSettingEnabled, packageJob1.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(!preventReleaseSettingEnabled, packageInPackageJob1.KP_IsReleasedViaJob);
			AssertEquals(!preventReleaseSettingEnabled, packageJob2.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals(!preventReleaseSettingEnabled, packageInPackageJob2.KP_IsReleasedViaJob);
		}

		public void TestReleaseJob_Org_SomePackageNotPicked_PreventReleaseOfPackageIfNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 4m);
			var package1 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package1.KP_PackageID = "ABC";
			packingHelper.CreatePackageDivot(package1, pickLine1);

			var pickLine2 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 6m);
			var package2 = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package2.KP_PackageID = "DEF";
			packingHelper.CreatePackageDivot(package2, pickLine2);
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: OM_WhsPreventReleaseOfPackageIfNotPicked setting.", true,
				data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: pick line 1 on package is not picked.", true,
				PickLinePair.New(pickLine1).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: pick line 2 on package is picked.", false,
				PickLinePair.New(pickLine2).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package job is not released.", false, order.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Precondition: package1 is not released.", false, package1.KP_IsReleasedViaJob);
			AssertEquals("Precondition: package2 is not released.", false, package2.KP_IsReleasedViaJob);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var result = releaseManager.ReleaseJobForReference(order.WD_DocketID);
			AssertEquals(false, result.IsSuccess);
			AssertEquals("Release of job is not allowed.", result.Result.ErrorMessage,
				"Job can't be released because some packages on the job are not yet fully picked.");
			AssertEquals("Package job is not released due to the enabled setting plus the package is not picked.",
				false, order.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Package job is not released due to the enabled setting plus the package is not picked.",
				false, package1.KP_IsReleasedViaJob);
			AssertEquals("Package job is not released due to the enabled setting plus the package is not picked.",
				false, package2.KP_IsReleasedViaJob);
		}

		public void TestReleaseJob_Org_PackagePicked_PreventReleaseOfPackageIfNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			packingHelper.CreatePackageDivot(package, pickLine);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: OM_WhsPreventReleaseOfPackageIfNotPicked setting.", true,
				data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: pick line on package is picked.", false,
				PickLinePair.New(pickLine).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package job is not released.", false, order.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Precondition: package is not released.", false, package.KP_IsReleasedViaJob);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);

			releaseManager.ReleaseJobForReference(order.WD_DocketID);
			AssertEquals("Package job is released.", true, order.PackageJob.KJ_ReleasedTimeUtc.IsValid);
			AssertEquals("Package job is released.", true, package.KP_IsReleasedViaJob);
		}

		#endregion

		#endregion

		#region TestReleasePackage

		#region TestReleasePackage_PreventReleaseOfPackageIfNotPicked

		public void TestReleasePackageForPk_PackageNotPicked_PreventReleaseOfPackageIfNotPickedEnabled()
		{
			TestReleasePackage_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: true,
				(releaseManager, package) => releaseManager.ReleasePackageForPk(package.PK));
		}

		public void TestReleasePackageForPk_PackageNotPicked_PreventReleaseOfPackageIfNotPickedDisabled()
		{
			TestReleasePackage_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: false,
				(releaseManager, package) => releaseManager.ReleasePackageForPk(package.PK));
		}

		public void TestReleasePackageForReference_PackageNotPicked_PreventReleaseOfPackageIfNotPickedEnabled()
		{
			TestReleasePackage_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: true,
				(releaseManager, package) => releaseManager.ReleasePackageForReference(package.KP_PackageID).Result);
		}

		public void TestReleasePackageForReference_PackageNotPicked_PreventReleaseOfPackageIfNotPickedDisabled()
		{
			TestReleasePackage_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: false,
				(releaseManager, package) => releaseManager.ReleasePackageForReference(package.KP_PackageID).Result);
		}

		void TestReleasePackage_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
			bool preventReleaseSettingEnabled, Func<ReleaseManager, PkgPackage, ActionResult> releasePackageAction)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_PreventReleaseOfPackageIfNotPicked = preventReleaseSettingEnabled;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			packingHelper.CreatePackageDivot(package, pickLine);
			Factory.Save();

			AssertEquals("Precondition: WW_PreventReleaseOfPackageIfNotPicked setting.", preventReleaseSettingEnabled,
				data.Whs1.WW_PreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: pick line on package is not picked.", true,
				PickLinePair.New(pickLine).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package is not released.", false, package.IsReleased);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);

			if (preventReleaseSettingEnabled)
			{
				var result = releasePackageAction(releaseManager, package);
				AssertEquals(false, result.IsSuccess);
				AssertEquals("Release of package is not allowed.", result.ErrorMessage,
					"Package 'ABC' can't be released because it has not been fully picked.");
			}
			else
			{
				var result = releasePackageAction(releaseManager, package);
				AssertEquals(true, result.IsSuccess);
			}

			AssertEquals(!preventReleaseSettingEnabled, package.IsReleased);
		}

		public void TestReleasePackage_SomePickLineNotPicked_PreventReleaseOfPackageIfNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_PreventReleaseOfPackageIfNotPicked = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 6m);
			var pickLine2 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 4m);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: WW_PreventReleaseOfPackageIfNotPicked setting.", true,
				data.Whs1.WW_PreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: pick line 1 on package is not picked.", true,
				PickLinePair.New(pickLine1).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: pick line 2 on package is picked.", false,
				PickLinePair.New(pickLine2).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package is not released.", false, package.IsReleased);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var result = releaseManager.ReleasePackageForPk(package.PK);
			AssertEquals(false, result.IsSuccess);
			AssertEquals("Release of package is not allowed.", result.ErrorMessage,
				"Package 'ABC' can't be released because it has not been fully picked.");
			AssertEquals("Package is not released as setting is enabled and not all picklines are picked.", false,
				package.IsReleased);
		}

		public void TestReleasePackage_PackagePicked_PreventReleaseOfPackageIfNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_PreventReleaseOfPackageIfNotPicked = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			packingHelper.CreatePackageDivot(package, pickLine);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: WW_PreventReleaseOfPackageIfNotPicked setting.", true,
				data.Whs1.WW_PreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: pick line on package is picked.", false,
				PickLinePair.New(pickLine).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package is not released.", false, package.IsReleased);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);

			releaseManager.ReleasePackageForPk(package.PK);
			AssertEquals("Package is released.", true, package.IsReleased);
		}

		public void TestReleasePackage_EmptyPackage_PreventReleaseOfPackageIfNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Whs1.WW_PreventReleaseOfPackageIfNotPicked = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			Factory.Save();

			AssertEquals("Precondition: WW_PreventReleaseOfPackageIfNotPicked setting.", true,
				data.Whs1.WW_PreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: package is empty.", false, package.PackedItemDivots.Count > 0);
			AssertEquals("Precondition: package is not released.", false, package.IsReleased);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var result = releaseManager.ReleasePackageForPk(package.PK);
			AssertEquals(false, result.IsSuccess);
			AssertEquals("Release of package is not allowed.", result.ErrorMessage,
				"Package 'ABC' can't be released because it has not been fully picked.");

			AssertEquals("Package is not released.", false, package.IsReleased);
		}

		#endregion

		#region TestReleasePackage_Org_PreventReleaseOfPackageIfNotPicked

		public void TestReleasePackageForPk_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedEnabled()
		{
			TestReleasePackage_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: true,
				(releaseManager, package) => releaseManager.ReleasePackageForPk(package.PK));
		}

		public void TestReleasePackageForPk_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedDisabled()
		{
			TestReleasePackage_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: false,
				(releaseManager, package) => releaseManager.ReleasePackageForPk(package.PK));
		}

		public void TestReleasePackageForReference_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedEnabled()
		{
			TestReleasePackage_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: true,
				(releaseManager, package) => releaseManager.ReleasePackageForReference(package.KP_PackageID).Result);
		}

		public void TestReleasePackageForReference_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedDisabled()
		{
			TestReleasePackage_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
				preventReleaseSettingEnabled: false,
				(releaseManager, package) => releaseManager.ReleasePackageForReference(package.KP_PackageID).Result);
		}

		void TestReleasePackage_Org_PackageNotPicked_PreventReleaseOfPackageIfNotPickedCore(
			bool preventReleaseSettingEnabled, Func<ReleaseManager, PkgPackage, ActionResult> releasePackageAction)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked = preventReleaseSettingEnabled;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			packingHelper.CreatePackageDivot(package, pickLine);
			Factory.Save();

			AssertEquals("Precondition: OM_WhsPreventReleaseOfPackageIfNotPicked setting.",
				preventReleaseSettingEnabled, data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: pick line on package is not picked.", true,
				PickLinePair.New(pickLine).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package is not released.", false, package.IsReleased);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);

			if (preventReleaseSettingEnabled)
			{
				var result = releasePackageAction(releaseManager, package);
				AssertEquals(false, result.IsSuccess);
				AssertEquals("Release of package is not allowed.", result.ErrorMessage,
					"Package 'ABC' can't be released because it has not been fully picked.");
			}
			else
			{
				var result = releasePackageAction(releaseManager, package);
				AssertEquals(true, result.IsSuccess);
			}

			AssertEquals(!preventReleaseSettingEnabled, package.IsReleased);
		}

		public void TestReleasePackage_Org_SomePickLineNotPicked_PreventReleaseOfPackageIfNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 6m);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 4m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine1 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 6m);
			var pickLine2 = pick.GetAllPickLines().Single(pickLine => pickLine.WZ_Units == 4m);
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			packingHelper.CreatePackageDivot(package, pickLine1);
			packingHelper.CreatePackageDivot(package, pickLine2);
			pickLine2.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: OM_WhsPreventReleaseOfPackageIfNotPicked setting.", true,
				data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: pick line 1 on package is not picked.", true,
				PickLinePair.New(pickLine1).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: pick line 2 on package is picked.", false,
				PickLinePair.New(pickLine2).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package is not released.", false, package.IsReleased);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var result = releaseManager.ReleasePackageForPk(package.PK);
			AssertEquals(false, result.IsSuccess);
			AssertEquals("Release of package is not allowed.", result.ErrorMessage,
				"Package 'ABC' can't be released because it has not been fully picked.");
			AssertEquals("Package is not released as setting is enabled and not all picklines are picked.", false,
				package.IsReleased);
		}

		public void TestReleasePackage_Org_PackagePicked_PreventReleaseOfPackageIfNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			packingHelper.CreatePackageDivot(package, pickLine);
			pickLine.WZ_PickedDateTime = ZDateTimeOffset.Now;
			Factory.Save();

			AssertEquals("Precondition: OM_WhsPreventReleaseOfPackageIfNotPicked setting.", true,
				data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: pick line on package is picked.", false,
				PickLinePair.New(pickLine).PickedDateTime.IsEmpty);
			AssertEquals("Precondition: package is not released.", false, package.IsReleased);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);

			releaseManager.ReleasePackageForPk(package.PK);
			AssertEquals("Package is released.", true, package.IsReleased);
		}

		public void TestReleasePackage_Org_EmptyPackage_PreventReleaseOfPackageIfNotPicked()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked = true;
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var packingHelper = new PackingTestHelper(Factory);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			var pick = Helper.CreatePickNew(order);
			var pickLine = pick.GetAllPickLines().Single();
			var package = packingHelper.CreatePackage(order.PackageJob, 1, "CAS");
			package.KP_PackageID = "ABC";
			Factory.Save();

			AssertEquals("Precondition: OM_WhsPreventReleaseOfPackageIfNotPicked setting.", true,
				data.Org1.MiscServ.OM_WhsPreventReleaseOfPackageIfNotPicked);
			AssertEquals("Precondition: package is empty.", false, package.PackedItemDivots.Count > 0);
			AssertEquals("Precondition: package is not released.", false, package.IsReleased);

			var releaseManager = new ReleaseManager(Factory, data.Whs1);
			var result = releaseManager.ReleasePackageForPk(package.PK);
			AssertEquals(false, result.IsSuccess);
			AssertEquals("Release of package is not allowed.", result.ErrorMessage,
				"Package 'ABC' can't be released because it has not been fully picked.");

			AssertEquals("Package is not released.", false, package.IsReleased);
		}

		#endregion

		#endregion

		#endregion

		#region ReleaseManagerResultTest

		public void TestReleaseManagerResult_Success()
		{
			var package = Factory.New<PkgPackage>();
			var result = ReleaseManagerResult.Success(new[] { package });
			AssertEquals(true, result.IsSuccess);
			AssertEquals(typeof(PkgPackage), result.Items[0].GetType());
			AssertEquals(string.Empty, result.Result.ErrorMessage);
			AssertEquals(true, result.Result.IsSuccess);
		}

		public void TestReleaseManagerResult_Failure()
		{
			var result = ReleaseManagerResult.Failure<PkgPackage>("Some error!");
			AssertEquals(false, result.IsSuccess);
			AssertNull(result.Items);
			AssertEquals("Some error!", result.Result.ErrorMessage);
			AssertEquals(false, result.Result.IsSuccess);
		}

		#endregion

		#region Implementation

		void AssertAlreadyReleasedOrCancelledPackageId(bool isReleased)
		{
			SetupEnvironmentData();
			Package2ForPackageJob1.KP_ReleasedTimeUtc = isReleased ? ZDateTime.UtcNow : ZDateTime.Empty;
			Factory.Save();

			var packageID = Package2ForPackageJob1.KP_PackageID;
			var releaseManager = new ReleaseManager(Factory, Warehouse1);

			var result = isReleased
				? releaseManager.ReleasePackageForReference(packageID)
				: releaseManager.UnreleasePackageForReference(packageID);
			AssertEquals(false, result.IsSuccess);
			AssertEquals(result.Result.ErrorMessage,
				$"Package '{packageID}' is not found, not an outer or {(isReleased ? "already released!" : "canceled!")}");
			AssertNull(result.Items);
		}

		void AssertAlreadyReleasedOrCancelledPackagePk(bool isReleased)
		{
			SetupEnvironmentData();
			Package2ForPackageJob1.KP_ReleasedTimeUtc = isReleased ? ZDateTime.UtcNow : ZDateTime.Empty;
			var packagePK = Package2ForPackageJob1.PK;
			var releaseManager = new ReleaseManager(Factory, Warehouse1);

			var result = isReleased
				? releaseManager.ReleasePackageForPk(packagePK)
				: releaseManager.UnreleasePackageForPk(packagePK);
			AssertEquals(false, result.IsSuccess);
			AssertEquals(result.ErrorMessage, $"The package is already {(isReleased ? "released." : "canceled.")}");
		}

		void TestReleaseJobForReference(string searchedReference, bool releaseAction,
			PkgPackageJob expectedFoundPackageJob)
		{
			var expectedPackageJobs = new List<PkgPackageJob>();
			expectedPackageJobs.Add(expectedFoundPackageJob);

			TestReleaseJobForReference(searchedReference, releaseAction, expectedPackageJobs);
		}

		void TestReleaseJobForReference(string searchedReference, bool releaseAction,
			List<PkgPackageJob> expectedPackageJobs)
		{
			var releaseManager = new ReleaseManager(Factory, Warehouse1);
			TestReleaseJobForReference(searchedReference, releaseManager, releaseAction, expectedPackageJobs);
		}

		void TestReleaseJobForReference(string searchedReference, ReleaseManager releaseManager, bool releaseAction,
			List<PkgPackageJob> expectedPackageJobs)
		{
			foreach (var expectedPackageJob in expectedPackageJobs)
			{
				if (releaseAction == expectedPackageJob.KJ_ReleasedTimeUtc.IsValid)
				{
					ReleaseOrUnReleasePackageJob(!releaseAction, expectedPackageJob);
				}

				AssertEquals("Precondition", !releaseAction, expectedPackageJob.KJ_ReleasedTimeUtc.IsValid);
			}

			IPackageJobInfoForRelease[] actualPackageJobInfos = null;

			if (releaseAction)
			{
				actualPackageJobInfos = releaseManager.ReleaseJobForReference(searchedReference).Items;
			}
			else
			{
				actualPackageJobInfos = releaseManager.UnreleaseJobForReference(searchedReference).Items;
			}

			AssertEquals("Count", expectedPackageJobs.Count, actualPackageJobInfos.Length);
			foreach (var expectedPackageJob in expectedPackageJobs)
			{
				var expectedPackageJobOrder = expectedPackageJob.ParentJob as WhsOrder;
				var matchingJobInfo =
					actualPackageJobInfos.Single(jobInfo => jobInfo.ParentJobNo == expectedPackageJob.ParentJob.JobNo);
				AssertEquals(expectedPackageJob.KJ_ParentID, matchingJobInfo.PK);
				AssertEquals(expectedPackageJob.KJ_JobID, matchingJobInfo.JobId);
				AssertEquals(expectedPackageJobOrder.Client.OH_Code, matchingJobInfo.ClientCode);
				AssertEquals(expectedPackageJobOrder.WD_RequiredDate.ToZDateTime(), matchingJobInfo.RequiredDate);
				AssertEquals(expectedPackageJobOrder.WD_DocketStatus, matchingJobInfo.JobStatus);
			}

			if (expectedPackageJobs.Count == 1)
			{
				AssertEquals(string.Format("Should be {0}", releaseAction ? "released" : "unreleased"), releaseAction,
					expectedPackageJobs[0].KJ_ReleasedTimeUtc.IsValid);
				foreach (var package in expectedPackageJobs[0].Packages)
				{
					AssertEquals(string.Format("Should be {0}", releaseAction ? "released via job" : "unreleased"),
						releaseAction, package.KP_IsReleasedViaJob);
				}
			}
		}

		void AssertReleaseJobForReference_NotFound(string searchedReference, ReleaseManager releaseManager,
			bool release)
		{
			var result = release
				? releaseManager.ReleaseJobForReference(searchedReference)
				: releaseManager.UnreleaseJobForReference(searchedReference);
			AssertEquals(false, result.IsSuccess);
			AssertEquals(result.Result.ErrorMessage, string.Format("Reference '{0}' not found!", searchedReference));
			AssertNull("No jobs return", result.Items);
		}

		void TestReleaseJobForPk(WhsOrder order, bool release)
		{
			var releaseManager = new ReleaseManager(Factory, Warehouse1);

			var packageJob = order.PackageJob;
			if (release == packageJob.KJ_ReleasedTimeUtc.IsValid)
			{
				ReleaseOrUnReleasePackageJob(!release, packageJob);
			}

			AssertEquals("Precondition", !release, packageJob.KJ_ReleasedTimeUtc.IsValid);

			ActionResult result;
			if (release)
			{
				result = releaseManager.ReleaseJobForPk(order.PK);
			}
			else
			{
				result = releaseManager.UnreleaseJobForPk(order.PK);
			}

			AssertEquals(true, result.IsSuccess);
			AssertEquals(release, packageJob.KJ_ReleasedTimeUtc.IsValid);
			foreach (var package in packageJob.Packages)
			{
				AssertEquals(release, package.KP_IsReleasedViaJob);
			}
		}

		static void ReleaseOrUnReleasePackageJob(bool isRelease, PkgPackageJob packageJob)
		{
			packageJob.KJ_ReleasedTimeUtc = isRelease ? ZDateTime.UtcNow : ZDateTime.Empty;
			packageJob.KJ_GS_NKReleasedBy = isRelease ? Env.CurrentUser.Initials : string.Empty;
		}

		void AssertReleaseJobForPk_NotFound(ZGuid packageJobPk, ReleaseManager releaseManager, bool release)
		{
			var result = release
				? releaseManager.ReleaseJobForPk(packageJobPk)
				: releaseManager.UnreleaseJobForPk(packageJobPk);
			AssertEquals(false, result.IsSuccess);
			AssertEquals(result.ErrorMessage, "The Job does not exist.");
		}

		void TestReleasePackageForReference(string searchedReference, bool release, List<PkgPackage> expectedPackages)
		{
			var releaseManager = new ReleaseManager(Factory, Warehouse1);

			foreach (var package in expectedPackages)
			{
				if (release == package.IsReleased)
				{
					ReleaseOrUnReleasePackage(!release, package);
				}

				AssertEquals("Precondition", !release, package.IsReleased);
			}

			Factory.Save();

			PkgPackage[] actualPackages = null;

			if (release)
			{
				actualPackages = releaseManager.ReleasePackageForReference(searchedReference).Items;
			}
			else
			{
				actualPackages = releaseManager.UnreleasePackageForReference(searchedReference).Items;
			}

			AssertEquals("Count", expectedPackages.Count, actualPackages.Length);
			foreach (var package in expectedPackages)
			{
				AssertCollectionContains(
					string.Format("Should contain {0}-{1}", package.KP_PackageID, package.PackageJob.KJ_JobID), package,
					actualPackages);
			}

			if (expectedPackages.Count == 1)
			{
				AssertEquals(string.Format("Should be {0}", release ? "released" : "unreleased"), release,
					expectedPackages[0].IsReleased);
			}
		}

		void AssertReleasePackageForReference_NotFound(string searchedReference, ReleaseManager releaseManager,
			bool release)
		{
			var result = release
				? releaseManager.ReleasePackageForReference(searchedReference)
				: releaseManager.UnreleasePackageForReference(searchedReference);
			AssertEquals(false, result.IsSuccess);
			AssertEquals(result.Result.ErrorMessage,
				$"Package '{searchedReference}' is not found, not an outer or {(release ? "already released!" : "canceled!")}");
			AssertNull(result.Items);
		}

		void TestReleasePackageForPk(PkgPackage package, bool release)
		{
			var releaseManager = new ReleaseManager(Factory, Warehouse1);

			if (release == package.IsReleased)
			{
				ReleaseOrUnReleasePackage(!release, package);
			}

			AssertEquals("Precondition", !release, package.IsReleased);

			if (release)
			{
				releaseManager.ReleasePackageForPk(package.PK);
			}
			else
			{
				releaseManager.UnreleasePackageForPk(package.PK);
			}

			AssertEquals(release, package.IsReleased);
		}

		static void ReleaseOrUnReleasePackage(bool isReleased, PkgPackage package)
		{
			package.KP_ReleasedTimeUtc = isReleased ? ZDateTime.UtcNow : ZDateTime.Empty;
			package.KP_GS_NKReleasedBy = isReleased ? Env.CurrentUser.Initials : string.Empty;
		}

		void AssertReleasePackageForPk_NotFound(ZGuid packagePk, ReleaseManager releaseManager, bool release)
		{
			var result = release
				? releaseManager.ReleasePackageForPk(packagePk)
				: releaseManager.UnreleasePackageForPk(packagePk);
			AssertEquals(false, result.IsSuccess);
			AssertEquals(result.ErrorMessage, "The package does not exist.");
		}

		void SetupEnvironmentData()
		{
			// Warehouses / Areas / Locations

			Warehouse1 = Helper.CreateWarehouse("Wh1");
			Warehouse2 = Helper.CreateWarehouse("Wh2");

			// Client

			Client = Helper.CreateClient("Client");

			// Order

			Order1InWhs1 = Helper.CreateWhsOrder(Client, Warehouse1);
			Order1InWhs1.WD_DocketID = "O0001";
			Order1InWhs1.WD_ExternalReference = "REF1";
			Order1InWhs1.WD_CustomerReference = "CUST1";
			Order1InWhs1.References.AddNew();
			Order1InWhs1.References[0].WX_RefType = Order1InWhs1.References[0].Lookups.ReferenceTypes[0].Code;
			Order1InWhs1.References[0].WX_Reference = "Order Ref1";

			Order2InWhs2 = Helper.CreateWhsOrder(Client, Warehouse2);
			Order2InWhs2.WD_DocketID = "O0002";
			Order2InWhs2.WD_ExternalReference = "REF2";
			Order2InWhs2.WD_CustomerReference = "CUST2";
			Order2InWhs2.References.AddNew();
			Order2InWhs2.References[0].WX_RefType = Order2InWhs2.References[0].Lookups.ReferenceTypes[0].Code;
			Order2InWhs2.References[0].WX_Reference = "Order Ref2";

			Order3InWhs1 = Helper.CreateWhsOrder(Client, Warehouse1);
			Order3InWhs1.WD_DocketID = "O0003";
			Order3InWhs1.WD_ExternalReference = "REF3";
			Order3InWhs1.WD_CustomerReference = "CUST3";
			Order3InWhs1.References.AddNew();
			Order3InWhs1.References[0].WX_RefType = Order3InWhs1.References[0].Lookups.ReferenceTypes[0].Code;
			Order3InWhs1.References[0].WX_Reference = "Order Ref1";

			// Package Job

			PackageJob1ForOrder1 = PkgPackageJob.LoadOrCreatePackageJob(Order1InWhs1);
			PackageJob2ForOrder2 = PkgPackageJob.LoadOrCreatePackageJob(Order2InWhs2);
			PackageJob3ForOrder3 = PkgPackageJob.LoadOrCreatePackageJob(Order3InWhs1);

			// Packages

			Package1ForPackageJob1 = PackageJob1ForOrder1.Packages.AddNew();
			Package1ForPackageJob1.KP_PackageID = "PK01";
			Package2ForPackageJob1 = PackageJob1ForOrder1.Packages.AddNew();
			Package2ForPackageJob1.KP_PackageID = "PK02";
			Package3ForPackageJob2 = PackageJob2ForOrder2.Packages.AddNew();
			Package3ForPackageJob2.KP_PackageID = "PK01";
			Package4ForPackageJob2 = PackageJob2ForOrder2.Packages.AddNew();
			Package4ForPackageJob2.KP_PackageID = "PK04";
			Package5ForPackageJob3 = PackageJob3ForOrder3.Packages.AddNew();
			Package5ForPackageJob3.KP_PackageID = "PK01";

			// Staff

			Staff1 = Helper.CreateGlbStaff("ST1", "ST1");

			Factory.Save();
		}

		WhsWarehouse Warehouse1;
		WhsWarehouse Warehouse2;
		OrgHeader Client;
		WhsOrder Order1InWhs1;
		WhsOrder Order2InWhs2;
		WhsOrder Order3InWhs1;
		PkgPackageJob PackageJob1ForOrder1;
		PkgPackageJob PackageJob2ForOrder2;
		PkgPackageJob PackageJob3ForOrder3;
		PkgPackage Package1ForPackageJob1;
		PkgPackage Package2ForPackageJob1;
		PkgPackage Package3ForPackageJob2;
		PkgPackage Package4ForPackageJob2;
		PkgPackage Package5ForPackageJob3;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Used in SetupEnvironmentData")]
		GlbStaff Staff1;

		#endregion
	}
}
