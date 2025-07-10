using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Warehouse.Transactions.ServiceTasks.Testing
{
	class CycleCountVarianceProcessingManagerTest : WhsTestCaseWithFactory
	{
		#region TestAcceptVariances

		public void TestAcceptVariances()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1", authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.ProcessCycleCountVariances();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, location1, -10, palletID: "PLT1");

			AssertEquals("Should have creation log of cycle count",
$@"Information|Did not find any variances authorized for Rejection.
Information|Adjustment W00000002 was created successfully for Client '111', Warehouse '1'.", logger.ToString().Trim());
		}

		public void TestAcceptVariances_HasError()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			Helper.CreatePickNew(order);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, "PLT1", data.Org1, data.Part1, -10, expectedQty: 10, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.ProcessCycleCountVariances();

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create adjustment", 0, adjustments.Count());
			AssertMultilineASCIIEquals("Should log error message",
@"Information|Did not find any variances authorized for Rejection.
Error|Failed to create adjustment for Client '111', Warehouse '1' by cycle count for Location 'A-1' in Warehouse '1'.
Not enough inventory was found. The stock may be committed by existing job(s) or has been picked.
Error - WE_TransactionQuantity: Attempted to adjust 5 Units, but no Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.", logger.ToString().Trim());
		}

		public void TestAcceptVariances_SetsUserContext()
		{
			var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var warehouse2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			var client1 = Helper.CreateClient("CL1");
			var product1 = Helper.CreateProduct("PROD1", client1);
			var client2 = Helper.CreateClient("CL2");
			var product2 = Helper.CreateProduct("PROD2", client2);
			var client3 = Helper.CreateClient("CL3");
			var product3 = Helper.CreateProduct("PROD3", client3);
			Factory.Save();

			var location1 = warehouse1.FindLocation("A-1");
			Helper.CreateWhsReceiveWithInventory(client1, warehouse1, "REC1", product1, 10m, location1, "PLT1");
			var location2 = warehouse2.FindLocation("B-1");
			Helper.CreateWhsReceiveWithInventory(client2, warehouse2, "REC2", product2, 10m, location2, "PLT2");
			var location3 = warehouse1.FindLocation("A-2");
			Helper.CreateWhsReceiveWithInventory(client3, warehouse1, "REC3", product3, 10m, location3, "PLT3");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, varianceQty: -10, expectedQty: 10, client: client1, part: product1, palletID: "PLT1", authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, varianceQty: -10, expectedQty: 10, client: client2, part: product2, palletID: "PLT2", authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);

			var cycleCount3 = Helper.CreateWhsCycleCountLocation(location3, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount3, CycleCountVarianceStatus.Codes.Open, varianceQty: -10, expectedQty: 10, client: client3, part: product3, palletID: "PLT3", authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var contextChangeCount = 0;
			var branchContexts = new HashSet<ZGuid>();
			var testBranchCode = Env.CurrentBranch.Code;

			try
			{
				Env.Instance.UserContextChanged += OnContextChanged;

				var processingManager = new CycleCountVarianceProcessingManager(Logger);
				processingManager.ProcessCycleCountVariances();

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

				AssertEquals("Changed context twice.", 2, contextChangeCount);
				AssertContainsExactElementsInAnyOrder("Changed to 2 warehouse branch contexts.", new[] { warehouse1.WW_GB_RelatedCompanyBranch, warehouse2.WW_GB_RelatedCompanyBranch }, branchContexts);
			}
			finally
			{
				Env.Instance.UserContextChanged -= OnContextChanged;
			}

			void OnContextChanged(object sender, IUserContextChangingEventArgs e)
			{
				if (e.NewUserContext.Branch.Code != testBranchCode) // do not count if it's just reverting to the previous context on temp context dispose
				{
					contextChangeCount++;
					branchContexts.Add(e.NewUserContext.Branch.PK);
				}
			}
		}

		#endregion

		#region TestAcceptVariances_ZeroQty

		public void TestAcceptVariances_ZeroQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, varianceQty: 0, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1", authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open, CycleCountVarianceAuthorizedAction.Codes.Approved);

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.ProcessCycleCountVariances();

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Approved, CycleCountVarianceAuthorizedAction.Codes.Empty);
			var adjustments = FindAdjustments(data.Org1);

			AssertEquals("Should not have adjustments", 0, adjustments.Count());

			AssertEquals("Should have creation log of cycle count",
$@"Information|Did not find any variances authorized for Rejection.", logger.ToString().Trim());
		}

		#endregion

		#region TestAcceptVariances_ShouldLinkAdjustmentOutToOpenNegativeVarianceForTheSamePalletID

		public void TestAcceptVariances_ShouldLinkAdjustmentOutToOpenNegativeVarianceForTheSamePalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 5m, location2, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var cycleCount1Variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var cycleCount2Variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: -5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Empty, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.ProcessCycleCountVariances();

			AssertEquals("Should have creation log of cycle count",
$@"Information|Did not find any variances authorized for Rejection.
Information|Adjustment W00000003 was created successfully for Client '111', Warehouse '1'.
Information|Adjustment W00000002 was created successfully for Client '111', Warehouse '1'.", Logger.ToString().Trim());

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should have 2 adjustments", 2, adjustments.Count());

			var adjustmentIn = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location1.PK));
			AssertAdjustment(adjustmentIn, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(adjustmentIn, data.Part1, location1, 5, palletID: "PLT1");

			var adjustmentOut = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK));
			AssertAdjustment(adjustmentOut, data.Org1, data.Whs1, adjustmentIn.PK);
			AssertAdjustmentLine(adjustmentOut, data.Part1, location2, -5, palletID: "PLT1");

			var newFactory = new BusinessObjectFactory();
			var cycleCount2VarianceInNewFactory = newFactory.Load<WhsCycleCountLocationVariance>(cycleCount2Variance.PK);
			AssertEquals("Negative Variance should have an adjustment.", adjustmentOut.PK, cycleCount2VarianceInNewFactory.WCC_WD_RelatedAdjustment);

			cycleCount2Variance.WCC_AuthorizedAction = CycleCountVarianceAuthorizedAction.Codes.Approved;
			Factory.Save();

			Logger.ClearLog();
			var processingManager2 = new CycleCountVarianceProcessingManager(Logger);
			processingManager2.ProcessCycleCountVariances();

			AssertEquals("Should have creation log of cycle count",
$@"Information|Did not find any variances authorized for Rejection.", logger.ToString().Trim());
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount2VarianceInNewFactory.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount2VarianceInNewFactory.WCC_AuthorizedAction);
		}

		public void TestAcceptVariances_ShouldLinkAdjustmentOutToVariancesForTheSameSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, use: true, setReleaseCaptured: false);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location2);
			var serial = "SN1";
			inventory.WE_SerialNumber = serial;
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var cycleCount1Variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: 1, serialNumber: serial, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var cycleCount2Variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: -1, serialNumber: serial, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Empty, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.ProcessCycleCountVariances();

			AssertEquals("Should have creation log of cycle count",
$@"Information|Did not find any variances authorized for Rejection.
Information|Adjustment W00000003 was created successfully for Client '111', Warehouse '1'.
Information|Adjustment W00000002 was created successfully for Client '111', Warehouse '1'.", Logger.ToString().Trim());

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should have 2 adjustments", 2, adjustments.Count());

			var adjustmentIn = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location1.PK));
			AssertAdjustment(adjustmentIn, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(adjustmentIn, data.Part1, location1, 1, serialNumber: serial);

			var adjustmentOut = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK));
			AssertAdjustment(adjustmentOut, data.Org1, data.Whs1, adjustmentIn.PK);
			AssertAdjustmentLine(adjustmentOut, data.Part1, location2, -1, serialNumber: serial);

			var newFactory = new BusinessObjectFactory();
			var cycleCount2VarianceInNewFactory = newFactory.Load<WhsCycleCountLocationVariance>(cycleCount2Variance.PK);
			AssertEquals("Negative Variance should have an adjustment.", adjustmentOut.PK, cycleCount2VarianceInNewFactory.WCC_WD_RelatedAdjustment);

			cycleCount2Variance.WCC_AuthorizedAction = CycleCountVarianceAuthorizedAction.Codes.Approved;
			Factory.Save();

			Logger.ClearLog();
			var processingManager2 = new CycleCountVarianceProcessingManager(Logger);
			processingManager2.ProcessCycleCountVariances();

			AssertEquals("Should have creation log of cycle count",
$@"Information|Did not find any variances authorized for Rejection.", logger.ToString().Trim());
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount2VarianceInNewFactory.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount2VarianceInNewFactory.WCC_AuthorizedAction);
		}

		#endregion

		#region TestRejectVariancesBelongsToMultipleCycleCountLocations

		public void TestRejectVariancesBelongsToMultipleCycleCountLocations()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAllAttributes, ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now, "~E");
			var openVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2);
			var markedAsRejectedVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);

			var cycleCountWithAnotherOpenVariance = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithPalletID, ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now, "~E");
			var openVariance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCountWithAnotherOpenVariance, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.ProcessCycleCountVariances();

			openVariance.Reload();
			AssertVarianceStatus(openVariance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected, CycleCountVarianceAuthorizedAction.Codes.Empty);

			markedAsRejectedVariance.Reload();
			AssertVarianceStatus(markedAsRejectedVariance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected, CycleCountVarianceAuthorizedAction.Codes.Empty);

			openVariance2.Reload();
			AssertVarianceStatus(openVariance2, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected, CycleCountVarianceAuthorizedAction.Codes.Empty);

			AssertCycleCount(cycleCount, location1, CycleCountGranularity.Codes.ProductWithAllAttributes, cycleCount.WCL_Priority);
			AssertCycleCount(cycleCountWithAnotherOpenVariance, location2, CycleCountGranularity.Codes.ProductWithPalletID, cycleCountWithAnotherOpenVariance.WCL_Priority);

			AssertEquals("Should have creation log of cycle count",
$@"Information|Re-Count was created with Granularity 'PWS' and Priority '0' for Location 'A-1' in Warehouse '1'.
Information|Re-Count was created with Granularity 'PWP' and Priority '0' for Location 'A-2' in Warehouse '1'.
Information|Did not find any variances authorized for Approval.", logger.ToString().Trim());
		}

		#endregion

		#region TestServiceTask_MultipleCycleCountWithApprovedAndRejected

		public void TestServiceTask_MultipleCycleCountWithApprovedAndRejected()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 10m, location2, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var rejectedCycleCount = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "~E");
			Helper.CreateWhsCycleCountLocationVariance(rejectedCycleCount, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			var approvedCycleCount = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(approvedCycleCount, CycleCountVarianceStatus.Codes.Open, varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1", authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.ProcessCycleCountVariances();

			AssertCycleCount(rejectedCycleCount, location1, CycleCountGranularity.Codes.ProductWithAttributes, rejectedCycleCount.WCL_Priority);

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, location2, -10, palletID: "PLT1");

			AssertEquals("Should have logs",
$@"Information|Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-1' in Warehouse '1'.
Information|Adjustment W00000002 was created successfully for Client '111', Warehouse '1'.", logger.ToString().Trim());
		}

		public void TestServiceTask_MultipleCycleCountWithApprovedAndRejected_ApprovedFailed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location2, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var rejectedCycleCount = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT"); // leave start and end time empty to create invalid data in the system
			Helper.CreateWhsCycleCountLocationVariance(rejectedCycleCount, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			var approvedCycleCount = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(approvedCycleCount, CycleCountVarianceStatus.Codes.Open, varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1", authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location2.ToLocationString(), palletID: "PLT1");
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.ProcessCycleCountVariances();

			AssertCycleCount(rejectedCycleCount, location1, CycleCountGranularity.Codes.ProductWithAttributes, rejectedCycleCount.WCL_Priority);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create new adjustments", 0, adjustments.Count(adj => adj.PK != adjustment.PK));
			AssertMultilineASCIIEquals("Accept variance failed should not stop creating cycle counts of rejected variances",
$@"Information|Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-1' in Warehouse '1'.
Error|Failed to create adjustment for Client '111', Warehouse '1' by cycle count for Location 'A-2' in Warehouse '1'.
Not enough inventory was found. The stock may be committed by existing job(s) or has been picked.
Error - WE_TransactionQuantity: Attempted to adjust 5 Units, but no Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.", logger.ToString().Trim());
		}

		#endregion

		#region TestRejectVariance

		public void TestRejectVariance()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now, "~E");
			var openVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2);
			var markedAsRejectedVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.ProcessCycleCountVariances();
			AssertCycleCount(cycleCount, location, CycleCountGranularity.Codes.PalletIDOnly, 0);
			AssertEquals("Should have creation log of cycle count",
$@"Information|Re-Count was created with Granularity '{CycleCountGranularity.Codes.PalletIDOnly}' and Priority '0' for Location 'A-1' in Warehouse '1'.
Information|Did not find any variances authorized for Approval.", logger.ToString().Trim());
		}

		public void TestRejectVariance_SetsUserContext()
		{
			var warehouse1 = Helper.CreateWarehouse("WH1", "A", 2, 1);
			var warehouse2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			var client1 = Helper.CreateClient("CL1");
			var product1 = Helper.CreateProduct("PROD1", client1);
			var client2 = Helper.CreateClient("CL2");
			var product2 = Helper.CreateProduct("PROD2", client2);
			var client3 = Helper.CreateClient("CL3");
			var product3 = Helper.CreateProduct("PROD3", client3);
			Factory.Save();

			var location1 = warehouse1.FindLocation("A-1");
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now, "~E");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, client1, product1, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);

			var location2 = warehouse2.FindLocation("B-1");
			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now, "~E");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, client2, product2, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);

			var location3 = warehouse1.FindLocation("A-2");
			var cycleCount3 = Helper.CreateWhsCycleCountLocation(location3, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now, "~E");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount3, CycleCountVarianceStatus.Codes.Open, client3, product3, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			var contextChangeCount = 0;
			var branchContexts = new HashSet<ZGuid>();
			var testBranchCode = Env.CurrentBranch.Code;

			try
			{
				Env.Instance.UserContextChanged += OnContextChanged;

				var processingManager = new CycleCountVarianceProcessingManager(Logger);
				processingManager.ProcessCycleCountVariances();
				AssertCycleCount(cycleCount1, location1, CycleCountGranularity.Codes.PalletIDOnly, 0);
				AssertCycleCount(cycleCount2, location2, CycleCountGranularity.Codes.PalletIDOnly, 0);
				AssertCycleCount(cycleCount3, location3, CycleCountGranularity.Codes.PalletIDOnly, 0);

				AssertEquals("Should have creation log of cycle count",
	$@"Information|Re-Count was created with Granularity '{CycleCountGranularity.Codes.PalletIDOnly}' and Priority '0' for Location 'A-1' in Warehouse 'WH1'.
Information|Re-Count was created with Granularity '{CycleCountGranularity.Codes.PalletIDOnly}' and Priority '0' for Location 'A-2' in Warehouse 'WH1'.
Information|Re-Count was created with Granularity '{CycleCountGranularity.Codes.PalletIDOnly}' and Priority '0' for Location 'B-1' in Warehouse 'WH2'.
Information|Did not find any variances authorized for Approval.", logger.ToString().Trim());

				AssertEquals("Changed context twice.", 2, contextChangeCount);
				AssertContainsExactElementsInAnyOrder("Changed to 2 warehouse branch contexts.", new[] { warehouse1.WW_GB_RelatedCompanyBranch, warehouse2.WW_GB_RelatedCompanyBranch }, branchContexts);
			}
			finally
			{
				Env.Instance.UserContextChanged -= OnContextChanged;
			}

			void OnContextChanged(object sender, IUserContextChangingEventArgs e)
			{
				if (e.NewUserContext.Branch.Code != testBranchCode) // do not count if it's just reverting to the previous context on temp context dispose
				{
					contextChangeCount++;
					branchContexts.Add(e.NewUserContext.Branch.PK);
				}
			}
		}

		#endregion

		#region TestRejectVariance_ZeroQty

		public void TestRejectVariance_ZeroQty()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.PalletIDOnly, ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now, "~E");
			var openVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 0, 2);
			var markedAsRejectedVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 0, 2,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			AssertVarianceStatus(markedAsRejectedVariance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open, CycleCountVarianceAuthorizedAction.Codes.Rejected);

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.ProcessCycleCountVariances();

			AssertVarianceStatus(markedAsRejectedVariance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected, CycleCountVarianceAuthorizedAction.Codes.Empty);

			AssertCycleCount(cycleCount, location, CycleCountGranularity.Codes.PalletIDOnly, 0);
			AssertEquals("Should have creation log of cycle count",
$@"Information|Re-Count was created with Granularity 'PID' and Priority '0' for Location 'A-1' in Warehouse '1'.
Information|Did not find any variances authorized for Approval.", logger.ToString().Trim());
		}

		#endregion

		#region TestServiceTask_MultipleRejectedVariances_SaveException

		public void TestServiceTask_MultipleRejectedVariances_SaveException()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ""); // leave start and end time empty to create invalid data in the system
			var varianceNotMarkedAsRejected = Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2);
			var varianceMarkedAsRejected = Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now, "~E"); // leave start and end time empty to create invalid data in the system
			var varianceMarkedAsRejectedInCCL = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);

			// Suspend the trigger to allow the save to proceed and do further validation.
			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsCycleCountLocationVariance_PreventCreateIfParentCycleCountLocationIsNotCompleted", "WhsCycleCountLocationVariance"))
			{
				Factory.Save();
			}

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.ProcessCycleCountVariances();
			AssertEquals("Since there must be a sql error when creating records logger must log an error.",
$@"Error|An error occurred while saving rejected cycle count variances: The value of WhsCycleCountLocation|WCL_WL_Location must be unique on WhsCycleCountLocation. The duplicate value(s) are: ({location1.PK}).
Information|Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-2' in Warehouse '1'.
Information|Did not find any variances authorized for Approval.", logger.ToString().Trim());

			AssertCycleCount(cycleCount1, location1, CycleCountGranularity.Codes.ProductWithAttributes, 0, false);
			AssertCycleCount(cycleCount2, location2, CycleCountGranularity.Codes.ProductWithAttributes, 0);

			varianceNotMarkedAsRejected.Reload();
			AssertVarianceStatus(varianceNotMarkedAsRejected, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open, CycleCountVarianceAuthorizedAction.Codes.Error);

			varianceMarkedAsRejected.Reload();
			AssertVarianceStatus(varianceMarkedAsRejected, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open, CycleCountVarianceAuthorizedAction.Codes.Error);

			varianceMarkedAsRejectedInCCL.Reload();
			AssertVarianceStatus(varianceMarkedAsRejectedInCCL, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected, CycleCountVarianceAuthorizedAction.Codes.Empty);
		}

		#endregion

		#region TestServiceTask_MultipleApprovedVariances_SaveException

		public void TestServiceTask_MultipleApprovedVariances_SaveException()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var now = DateTimeOffset.Now;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: 1, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "PLT1", client: data.Org1, part: data.Part2, varianceQty: 10, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.CreateSaveExceptionForTest += factoryInServiceTask =>
			{
				factoryInServiceTask.Saving += (f) => throw new ZSaveException(new ZDataException(new Exception(), ((INeedRow)cycleCount1).Row, Db.Connection), f);
			};
			processingManager.ProcessCycleCountVariances();

			AssertEquals("Since there must be a sql error when creating records logger must log an error.",
$@"Information|Did not find any variances authorized for Rejection.
Error|An error occurred while saving approved cycle count variances with adjustments: 
Information|Adjustment W00000001 was created successfully for Client '111', Warehouse '1'.", logger.ToString().Trim());

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should only create 1 adjustment", 1, adjustments.Count());
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part2, location2, 10, palletID: "PLT1");

			variance1.Reload();
			AssertVarianceStatus(variance1, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open, CycleCountVarianceAuthorizedAction.Codes.Error);

			variance2.Reload();
			AssertVarianceStatus(variance2, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved, CycleCountVarianceAuthorizedAction.Codes.Empty);
		}

		#endregion

		#region TestServiceTask_MultipleCycleCountWithApprovedAndRejected_RejectedFailed

		public void TestServiceTask_MultipleCycleCountWithApprovedAndRejected_RejectedFailed()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var now = DateTimeOffset.Now;
			var rejectedCycleCount = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, ZDateTimeOffset.Empty, ZDateTimeOffset.Empty, ""); // leave start and end time empty to create invalid data in the system
			Helper.CreateWhsCycleCountLocationVariance(rejectedCycleCount, CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			var approvedCycleCount = Helper.CreateWhsCycleCountLocation(location2, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Helper.CreateWhsCycleCountLocationVariance(approvedCycleCount, CycleCountVarianceStatus.Codes.Open, varianceQty: 10, expectedQty: 0, client: data.Org1, part: data.Part1, palletID: "PLT1", authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);

			// Suspend the trigger to allow the save to proceed and do further validation.
			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsCycleCountLocationVariance_PreventCreateIfParentCycleCountLocationIsNotCompleted", "WhsCycleCountLocationVariance"))
			{
				Factory.Save();
			}

			var processingManager = new CycleCountVarianceProcessingManager(Logger);
			processingManager.ProcessCycleCountVariances();
			AssertCycleCount(rejectedCycleCount, location1, CycleCountGranularity.Codes.ProductWithAttributes, rejectedCycleCount.WCL_Priority, false);

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, location2, 10, palletID: "PLT1");

			AssertEquals("Reject variance failed should not stop creating adjustments of approved variances",
$@"Error|An error occurred while saving rejected cycle count variances: The value of WhsCycleCountLocation|WCL_WL_Location must be unique on WhsCycleCountLocation. The duplicate value(s) are: ({location1.PK}).
Information|Adjustment W00000001 was created successfully for Client '111', Warehouse '1'.", logger.ToString().Trim());
		}

		#endregion

		#region Implementation

		void AssertVarianceStatus(WhsCycleCountLocationVariance variance, ZGuid relatedAdjustmentPK, ZString status, ZString authorizedAction)
		{
			AssertEquals("RelatedAdjustment", relatedAdjustmentPK, variance.WCC_WD_RelatedAdjustment);
			AssertEquals("Variance Status", status, variance.WCC_Status);
			AssertEquals("Variance AuthorizedAction", authorizedAction, variance.WCC_AuthorizedAction);
		}

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

		void AssertCycleCount(WhsCycleCountLocation parentCycleCount, WhsLocation expectedLocation, ZString expectedGranularity, ZByte expectedPriority, bool isNewCycleCountCreated = true)
		{
			var query = new ZQuery(WhsCycleCountLocationSchema.WCL_WCL_RejectedCycleCount, parentCycleCount.PK);
			var newCycleCount = Factory.Load<WhsCycleCountLocation>(query).SingleOrDefault();
			if (isNewCycleCountCreated)
			{
				AssertNotNull("Must create a new cycle count", newCycleCount);
				AssertEquals("Cycle Count - Granularity", expectedGranularity, newCycleCount.WCL_Granularity);
				AssertEquals("Cycle Count - Location", expectedLocation.PK, newCycleCount.WCL_WL_Location);
				AssertEquals("Cycle Count - Priority", expectedPriority, newCycleCount.WCL_Priority);
			}
			else
			{
				AssertNull("Must not create a new cycle count", newCycleCount);
			}
		}

		TestServiceLogger Logger => logger ?? (logger = new TestServiceLogger());
		TestServiceLogger logger;

		#endregion
	}
}
