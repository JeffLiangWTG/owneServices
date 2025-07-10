using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	[TestedType(typeof(CycleCountVarianceServiceTask))]
	class CycleCountVarianceServiceTaskTest : ServiceTaskTestCase<CycleCountVarianceServiceTask>
	{
		public void TestInitialiseTask()
		{
			AssertEquals("15minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestServiceTask()
		{
			var logger = RunServiceTask();

			AssertMultilineASCIIEquals("Since there are no variances to reject/approve, we should show information messages.",
@"Information|Did not find any variances authorized for Rejection.
Information|Did not find any variances authorized for Approval.", logger.ToString().Trim());
		}

		public void TestServiceTask_EndToEnd_AcceptedVariance()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse1 = helper.CreateWarehouse("WH1", "A", 2, 1);
			var warehouse2 = helper.CreateWarehouse("WH2", "B", 2, 1);
			var client1 = helper.CreateClient("CL1");
			var product1 = helper.CreateProduct("PROD1", client1);
			var client2 = helper.CreateClient("CL2");
			var product2 = helper.CreateProduct("PROD2", client2);
			var client3 = helper.CreateClient("CL3");
			var product3 = helper.CreateProduct("PROD3", client3);
			Factory.Save();

			var location1 = warehouse1.FindLocation("A-1");
			helper.CreateWhsReceiveWithInventory(client1, warehouse1, "REC1", product1, 10m, location1, "PLT1");
			var location2 = warehouse2.FindLocation("B-1");
			helper.CreateWhsReceiveWithInventory(client2, warehouse2, "REC2", product2, 10m, location2, "PLT2");
			var location3 = warehouse1.FindLocation("A-2");
			helper.CreateWhsReceiveWithInventory(client3, warehouse1, "REC3", product3, 10m, location3, "PLT3");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount1 = helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, varianceQty: -10, expectedQty: 10, client: client1, part: product1, palletID: "PLT1", authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);

			var cycleCount2 = helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, varianceQty: -10, expectedQty: 10, client: client2, part: product2, palletID: "PLT2", authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);

			var cycleCount3 = helper.CreateWhsCycleCountLocation(location3, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			helper.CreateWhsCycleCountLocationVariance(cycleCount3, CycleCountVarianceStatus.Codes.Open, varianceQty: -10, expectedQty: 10, client: client3, part: product3, palletID: "PLT3", authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = RunServiceTask();

			var adjustmentsClient1 = FindAdjustments(client1);
			var finalAdjustmentClient1 = adjustmentsClient1.Single();
			AssertAdjustment(finalAdjustmentClient1, client1, warehouse1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustmentClient1, product1, location1, -10, palletID: "PLT1");

			var adjustmentsClient2 = FindAdjustments(client2);
			var finalAdjustmentClient2 = adjustmentsClient2.Single();
			AssertAdjustment(finalAdjustmentClient2, client2, warehouse2, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustmentClient2, product2, location2, -10, palletID: "PLT2");

			var adjustmentsClient3 = FindAdjustments(client3);
			var finalAdjustmentClient3 = adjustmentsClient3.Single();
			AssertAdjustment(finalAdjustmentClient3, client3, warehouse1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustmentClient3, product3, location3, -10, palletID: "PLT3");

			IEnumerable<WhsAdjustment> FindAdjustments(params OrgHeader[] clients)
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Adjustment);
				query.AddToFilter(WhsDocketSchema.WD_OH_Client, clients.Select(c => c.PK));

				return newFactory.Load<WhsAdjustment>(query);
			}

			void AssertAdjustment(WhsAdjustment adjustment, OrgHeader expectedClient, WhsWarehouse expectedWarehouse, ZGuid parentDocketPK)
			{
				AssertEquals("Adjustment - Client", expectedClient.PK, adjustment.WD_OH_Client);
				AssertEquals("Adjustment - Warehouse", expectedWarehouse.PK, adjustment.WD_WW_Whs);
				AssertEquals("Adjustment - ParentDocket", parentDocketPK, adjustment.WD_WD_ParentDocket);
			}

			void AssertAdjustmentLine(WhsAdjustment adjustment, OrgSupplierPart part, WhsLocation location, ZDecimal adjustmentQuantity, string palletID = "", string partAttrib1 = "", string partAttrib2 = "", string partAttrib3 = "", string serialNumber = "", ZDate? expiryDate = null, ZDate? packingDate = null)
			{
				AssertNotNull("Should find matched Adjustment Line", adjustment.Lines.SingleOrDefault(l =>
					l.WE_OP == part.PK
					&& l.WE_F3_NKPackType == part.OP_StockKeepingUnit
					&& l.WE_WL == location.PK
					&& l.WE_PalletID == palletID
					&& l.WE_PartAttrib1 == partAttrib1
					&& l.WE_PartAttrib2 == partAttrib2
					&& l.WE_PartAttrib3 == partAttrib3
					&& l.WE_SerialNumber == serialNumber
					&& l.WE_ExpiryDate == (expiryDate ?? ZDate.Empty)
					&& l.WE_PackingDate == (packingDate ?? ZDate.Empty)
					&& l.WE_TransactionQuantity == adjustmentQuantity
					&& l.IsFinalised));
			}
		}

		public void TestServiceTask_EndToEnd_RejectedVariance()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var warehouse1 = helper.CreateWarehouse("WH1", "A", 2, 1);
			var warehouse2 = helper.CreateWarehouse("WH2", "B", 2, 1);
			var client1 = helper.CreateClient("CL1");
			var product1 = helper.CreateProduct("PROD1", client1);
			var client2 = helper.CreateClient("CL2");
			var product2 = helper.CreateProduct("PROD2", client2);
			var client3 = helper.CreateClient("CL3");
			var product3 = helper.CreateProduct("PROD3", client3);
			Factory.Save();

			var location1 = warehouse1.FindLocation("A-1");
			var cycleCount1 = helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now, "~E");
			helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, client1, product1, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);

			var location2 = warehouse2.FindLocation("B-1");
			var cycleCount2 = helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now, "~E");
			helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, client2, product2, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);

			var location3 = warehouse1.FindLocation("A-2");
			var cycleCount3 = helper.CreateWhsCycleCountLocation(location3, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now, "~E");
			helper.CreateWhsCycleCountLocationVariance(cycleCount3, CycleCountVarianceStatus.Codes.Open, client3, product3, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			RunServiceTask();

			AssertCycleCount(cycleCount1, location1, CycleCountGranularity.Codes.PalletIDOnly, 0);
			AssertCycleCount(cycleCount2, location2, CycleCountGranularity.Codes.PalletIDOnly, 0);
			AssertCycleCount(cycleCount3, location3, CycleCountGranularity.Codes.PalletIDOnly, 0);

			void AssertCycleCount(WhsCycleCountLocation parentCycleCount, WhsLocation expectedLocation, ZString expectedGranularity, ZByte expectedPriority)
			{
				var query = new ZQuery(WhsCycleCountLocationSchema.WCL_WCL_RejectedCycleCount, parentCycleCount.PK);
				var newCycleCount = Factory.Load<WhsCycleCountLocation>(query).SingleOrDefault();
				AssertNotNull("Must create a new cycle count", newCycleCount);
				AssertEquals("Cycle Count - Granularity", expectedGranularity, newCycleCount.WCL_Granularity);
				AssertEquals("Cycle Count - Location", expectedLocation.PK, newCycleCount.WCL_WL_Location);
				AssertEquals("Cycle Count - Priority", expectedPriority, newCycleCount.WCL_Priority);
			}
		}

		public void TestCanRunInAnyBranch()
		{
			var attribute = typeof(CycleCountVarianceServiceTask)
				.Assembly
				.GetCustomAttributes(true)
				.OfType<HostedServiceAttribute>()
				.Single(x => x.Code == "CCV");
			AssertEquals("CanRunInAnyBranch", true, attribute.CanRunInAnyBranch);
		}

		TestServiceLogger RunServiceTask()
		{
			var logger = new TestServiceLogger();
			var serviceTask = new CycleCountVarianceServiceTask() { ServiceLogger = logger };
			InitialiseTaskSchedule(serviceTask);
			using (EnvProxy.Instance.TemporaryServiceTaskContext("CCV", canRunInAnyBranch: true))
			{
				serviceTask.RunTask();
			}

			return logger;
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						WhsCycleCountLocationVarianceSchema.Constants.TableName,
						null,
						WhsCycleCountLocationVarianceSchema.Constants.WCC_AuthorizedAction + "=" + CycleCountVarianceAuthorizedAction.Codes.Approved),

					new TaskNudgeInformationForTest(
						WhsCycleCountLocationVarianceSchema.Constants.TableName,
						null,
						WhsCycleCountLocationVarianceSchema.Constants.WCC_AuthorizedAction + "=" + CycleCountVarianceAuthorizedAction.Codes.Rejected),
				};
			}
		}
	}
}
