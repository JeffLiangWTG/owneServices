using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Services.OperationalActions.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsCycleCountLocation))]
	public class WhsCycleCountLocationTest : WhsBusinessObjectTestCase
	{
		#region INumberFountain

		public void TestINumberFountainConsumer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			AssertEquals("WhsCycleCountLocation uses correct Fountain.", Env.NumberFountains.WhsCycleCountLocationID, ((INumberFountainConsumer)cycleCount).Fountain);

			cycleCount.WCL_JobID = "WC00000001";
			AssertEquals("ID refers to correct Field.", "WC00000001", ((INumberFountainConsumer)cycleCount).ID);

			((INumberFountainConsumer)cycleCount).ID = "WC00000002";
			AssertEquals("ID refers to correct Field.", "WC00000002", cycleCount.WCL_JobID);
		}

		public void TestINumberFountainConsumer_SavingSetsJobID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			AssertEquals("Precondition: Cycle Count Location Job ID is empty.", "", cycleCount.WCL_JobID);

			Factory.Save();
			AssertEquals("Cycle Count Location Job ID was set correctly.", "WC00000001", cycleCount.WCL_JobID);
		}

		#endregion

		#region TestReadOnlyProperties

		public void TestReadOnlyProperties()
		{
			var cycleCount = Factory.New<WhsCycleCountLocation>();
			var fieldsTester = ObjectFactory.Get<IOperationalActionFieldTester>();

			foreach (var property in cycleCount
										.ZPropertyInfoHash
										.Cast<ZPropertyInfo>()
										.Where(pi => !pi.Name.StartsWith("WCL_System")))
			{
				CombineAssertions(
					$"{property.Name} should be readonly.",
					() =>
					{
						AssertEquals("Should be readonly", true, property.ReadOnly);

						var actionFieldAttribute = ActionFieldAttribute.Get(typeof(WhsCycleCountLocation).GetProperty(property.Name));
						if (actionFieldAttribute != null)
						{
							AssertEquals($"Action field attribute for {property.Name} must be read-only.", true,
								actionFieldAttribute.ReadOnly);
						}
						else
						{
							AssertEquals(true, fieldsTester.IsFieldUnsupported(typeof(WhsCycleCountLocation), property.Name));
						}
					});
			}
		}

		#endregion

		#region TestVariances

		public void TestVariances()
		{
			var cycleCount = Factory.New<WhsCycleCountLocation>();
			AssertNotNull("Variance collection should not be null", cycleCount.Variances);
			AssertEquals("No variances in collection", 0, cycleCount.Variances.Count);

			var variance1 = cycleCount.Variances.AddNew();
			AssertEquals("Should has 1 variance in collection", 1, cycleCount.Variances.Count);
		}

		#endregion

		#region TestProperties

		public void TestHasOpenVariances()
		{
			var cycleCount = Factory.New<WhsCycleCountLocation>();
			AssertEquals("No variances in collection", false, cycleCount.HasOpenVariances);

			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount,
				CycleCountVarianceStatus.Codes.Rejected, null, null, 0, 5);
			AssertEquals("Variance is rejected", false, cycleCount.HasOpenVariances);

			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				null, null, 0, 5);
			AssertEquals("One of Variances is rejected", false, cycleCount.HasOpenVariances);

			variance1.WCC_Status = CycleCountVarianceStatus.Codes.Open;
			AssertEquals("All variance is open", true, cycleCount.HasOpenVariances);
		}

		public void TestIsBulkCycleCount()
		{
			var cycleCount = Factory.New<WhsCycleCountLocation>();
			cycleCount.WCL_Granularity = CycleCountGranularity.Codes.PalletCount;
			AssertEquals("Granularity is PLT", true, cycleCount.IsBulkCycleCount);

			cycleCount.WCL_Granularity = CycleCountGranularity.Codes.PalletIDOnly;
			AssertEquals("Granularity is PID", false, cycleCount.IsBulkCycleCount);

			cycleCount.WCL_Granularity = CycleCountGranularity.Codes.ProductOnly;
			AssertEquals("Granularity is PRD", true, cycleCount.IsBulkCycleCount);

			cycleCount.WCL_Granularity = CycleCountGranularity.Codes.ProductWithPalletID;
			AssertEquals("Granularity is PWP", false, cycleCount.IsBulkCycleCount);

			cycleCount.WCL_Granularity = CycleCountGranularity.Codes.ProductWithAttributes;
			AssertEquals("Granularity is PWA", false, cycleCount.IsBulkCycleCount);

			cycleCount.WCL_Granularity = CycleCountGranularity.Codes.ProductWithAllAttributes;
			AssertEquals("Granularity is PWS", false, cycleCount.IsBulkCycleCount);
		}

		public void TestCanApproveVariances()
		{
			var now = DateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var cycleCount = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, ZDateTimeOffset.Empty, "TTT");
			Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				client: data.Org1, part: data.Part1, varianceQty: 5,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			AssertEquals("Cannot approve variances when cycle count is not finished yet", false,
				cycleCount.CanApproveVariances);

			cycleCount.WCL_EndTime = now.AddHours(1);
			AssertEquals("Can approve variances when cycle count has been finished", true,
				cycleCount.CanApproveVariances);

			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount,
				CycleCountVarianceStatus.Codes.Approved, client: data.Org1, part: data.Part1, varianceQty: 5,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			AssertEquals("Cannot approve variances when the status of variance is not Open", false,
				cycleCount.CanApproveVariances);

			variance2.WCC_Status = CycleCountVarianceStatus.Codes.Open;
			var variance3 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				client: data.Org1, part: data.Part1, varianceQty: 5,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertEquals("Cannot approve variances when the AuthorizedAction of variance is not Approve", false,
				cycleCount.CanApproveVariances);

			variance3.WCC_AuthorizedAction = CycleCountVarianceAuthorizedAction.Codes.Approved;
			AssertEquals("Can approve variances when all Status and AuthorizedAction are valid", true,
				cycleCount.CanApproveVariances);
		}

		#endregion

		#region TestAcceptVariances

		#region TestAcceptVariances_CannotAccept

		#region TestAcceptVariances_CannotAccept_AuthorizedAction

		public void TestAcceptVariances_CannotAccept_AuthorizedActionIsReject()
		{
			TestAcceptVariances_CannotAcceptCore(CycleCountGranularity.Codes.ProductWithAttributes,
				CycleCountVarianceStatus.Codes.Open, CycleCountVarianceAuthorizedAction.Codes.Rejected);
		}

		public void TestAcceptVariances_CannotAccept_AuthorizedActionIsError()
		{
			TestAcceptVariances_CannotAcceptCore(CycleCountGranularity.Codes.ProductWithAttributes,
				CycleCountVarianceStatus.Codes.Open, CycleCountVarianceAuthorizedAction.Codes.Error);
		}

		public void TestAcceptVariances_CannotAccept_AuthorizedActionIsEmpty()
		{
			TestAcceptVariances_CannotAcceptCore(CycleCountGranularity.Codes.ProductWithAttributes,
				CycleCountVarianceStatus.Codes.Open, CycleCountVarianceAuthorizedAction.Codes.Empty);
		}

		public void TestAcceptVariances_CannotAccept_AuthorizedActionIsApprovedAndStatusIsApproved()
		{
			// Need to suspend the constraint here to temporarily allow the insertion of an invalid status
			using (WhsTestHelperFunctions.DisableConstraint("WhsCycleCountLocationVariance", "Constraint_ApprovedVarianceHasAnAdjustment"))
			using (WhsTestHelperFunctions.DisableConstraint("WhsCycleCountLocationVariance", "Constraint_AuthorizedAction2"))
			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsCycleCountLocationVariance_CannotBeModifiedOrDeleted", "WhsCycleCountLocationVariance"))
			{
				TestAcceptVariances_CannotAcceptCore(CycleCountGranularity.Codes.ProductWithAttributes,
					CycleCountVarianceStatus.Codes.Approved, CycleCountVarianceAuthorizedAction.Codes.Approved);
			}
		}

		#endregion

		#region TestAcceptVariances_CannotAccept_Granularity

		public void TestAcceptVariances_CannotAccept_GranularityIsPLT()
		{
			TestAcceptVariances_CannotAcceptCore(CycleCountGranularity.Codes.PalletCount,
				CycleCountVarianceStatus.Codes.Open, CycleCountVarianceAuthorizedAction.Codes.Approved);
		}

		public void TestAcceptVariances_CannotAccept_GranularityIsPRD()
		{
			TestAcceptVariances_CannotAcceptCore(CycleCountGranularity.Codes.ProductOnly,
				CycleCountVarianceStatus.Codes.Open, CycleCountVarianceAuthorizedAction.Codes.Approved);
		}

		#endregion

		public void TestAcceptVariances_MarkAsError_WhenLocationIsIncomplete()
		{
			// Need to suspend the trigger here to enable insertion of an invalid variance
			using (WhsTestHelperFunctions.SuspendTrigger("TG_WhsCycleCountLocationVariance_PreventCreateIfParentCycleCountLocationIsNotCompleted", "WhsCycleCountLocationVariance"))
			{
				TestAcceptVariances_CannotAcceptCore(CycleCountGranularity.Codes.ProductWithAttributes,
					CycleCountVarianceStatus.Codes.Open, CycleCountVarianceAuthorizedAction.Codes.Rejected, ZDateTimeOffset.Empty);
			}
		}

		void TestAcceptVariances_CannotAcceptCore(ZString granularity, ZString varianceStatus, ZString authorizedAction, ZDateTimeOffset? endTime = null)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);

			var now = DateTimeOffset.Now;
			var cycleCount =
				Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation, granularity, now, endTime ?? now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, varianceStatus, client: data.Org1, part: data.Part1,
				varianceQty: 5, authorizedAction: authorizedAction);

			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);

			var factory2 = new BusinessObjectFactory();
			var varianceInFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance.PK);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create adjustment", 0, adjustments.Count());
			AssertMultilineASCIIEquals("Should log error message",
				@"No variances have been approved for Location 'A-1' in Warehouse '1'.
Two possible reasons for this are the granularity of cycle count is 'PLT'/'PRD' or the variances of cycle count are not valid(Status must be 'OPN', Authorized Action must be 'APP').",
				logger.LogMessagesAllAppended);

			// Assert that the variance is marked in error
			AssertEquals("Variance Authorized", CycleCountVarianceAuthorizedAction.Codes.Error, varianceInFactory2.WCC_AuthorizedAction);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Open, varianceInFactory2.WCC_Status);
			AssertEquals("Variance Adjustment", ZGuid.Empty, varianceInFactory2.WCC_WD_RelatedAdjustment);
		}

		public void TestAcceptVariances_SaveError()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "REC1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			Factory.Saving += (f) =>
				throw new ZSaveException(
					new ZDataException(new ArgumentException("Test"), ((INeedRow)cycleCount).Row, Db.Connection),
					Factory);

			var logger = new DummyWhsDocketCreationLogger();
			AssertNoExceptionThrown("Should not throw exception", () => cycleCount.AcceptVariances(logger));
			AssertEquals("Should log error message", true,
				logger.LogMessagesAllAppended.Contains(
					"An error occurred while saving approved cycle count variances with adjustments: "));

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create adjustment", 0, adjustments.Count());
		}

		#endregion

		#region TestAcceptVariances_VarianceIsNegative

		public void TestAcceptVariances_VarianceIsNegative()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1", ZDate.Today, ZDate.Today, "AA",
				"BB", "CC", ZString.Empty);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -3, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "Plt1",
				partAttrib1: "aa", partAttrib2: "bB", partAttrib3: "Cc", expiryDate: ZDate.Today,
				packingDate: ZDate.Today, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertEquals("External Reference", "CYCLECOUNT ADJUSTMENT W00000002", finalAdjustment.WD_ExternalReference);
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -3, "PLT1", "AA", "BB", "CC",
				"", ZDate.Today, ZDate.Today, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_EmptyHeldCodeInventory()
		{
			TestAcceptVariances_VarianceIsNegative_AvailableInventory_Core(-10m);
		}

		public void TestAcceptVariances_VarianceIsNegative_PartialVariance_AvailableInventory()
		{
			TestAcceptVariances_VarianceIsNegative_AvailableInventory_Core(-4m);
		}

		void TestAcceptVariances_VarianceIsNegative_AvailableInventory_Core(ZDecimal varianceQty)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: varianceQty, expectedQty: 10m, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });
			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, varianceQty, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Available,
				originalHeldCode: string.Empty,
				currentAdjustmentStatus: InventoryStatus.Codes.Available,
				currentAdjustmentHeldCode: string.Empty);

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			AssertVarianceStatus(variance,
				finalAdjustment.PK,
				CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_HeldInventory_UserDefinedHeldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var heldCode = Helper.CreateInventoryHeldCode("SA", "SA");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, "PLT1", InventoryStatus.Codes.Held, heldCode.WHC_Code);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Held,
				originalInventoryHeldCode: heldCode.WHC_Code,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: heldCode.WHC_Code);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10m, expectedQty: 10m, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });
			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -10m, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: heldCode.WHC_Code,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: heldCode.WHC_Code);

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Held,
				originalInventoryHeldCode: heldCode.WHC_Code,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: heldCode.WHC_Code);

			AssertVarianceStatus(variance,
				finalAdjustment.PK,
				CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_AllDamaged_HeldInventory()
		{
			TestAcceptVariances_VarianceIsNegative_Damaged_HeldInventory_Core(-10m);
		}

		public void TestAcceptVariances_VarianceIsNegative_AllDamaged_PartialVariance_HeldInventory()
		{
			TestAcceptVariances_VarianceIsNegative_Damaged_HeldInventory_Core(-4m);
		}

		void TestAcceptVariances_VarianceIsNegative_Damaged_HeldInventory_Core(ZDecimal varianceQty)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, "PLT1", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Held,
				originalInventoryHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.Damaged);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: varianceQty, expectedQty: 10m, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });
			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, varianceQty, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged);

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Held,
				originalInventoryHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.Damaged);

			AssertVarianceStatus(variance,
				finalAdjustment.PK,
				CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_InconsistentCase_SerialNumber_VarianceIsNegative()
		{
			TestAcceptVariances_InconsistentCase_SerialNumberCore(initalSerial: "sn1", varianceSerial: "Sn1", adjustmentSerial: "SN1", varianceValue: -1);
		}

		public void TestAcceptVariances_InconsistentCase_SerialNumber_VarianceIsPositive()
		{
			TestAcceptVariances_InconsistentCase_SerialNumberCore(initalSerial: "sn1", varianceSerial: "Sn2", adjustmentSerial: "SN2", varianceValue: 1);
		}

		void TestAcceptVariances_InconsistentCase_SerialNumberCore(string initalSerial, string varianceSerial, string adjustmentSerial, int varianceValue)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "", ZDate.Today, ZDate.Today, "AA",
				"BB", "CC", ZString.Empty);
			receiveLine.WE_SerialNumber = initalSerial;
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: varianceValue, expectedQty: 1, client: data.Org1, part: data.Part1, palletID: "",
				partAttrib1: "aa", partAttrib2: "bB", partAttrib3: "Cc", serialNumber: varianceSerial,
				expiryDate: ZDate.Today, packingDate: ZDate.Today,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertEquals("External Reference", "CYCLECOUNT ADJUSTMENT W00000002", finalAdjustment.WD_ExternalReference);
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, varianceValue, "", "AA", "BB", "CC",
				adjustmentSerial, ZDate.Today, ZDate.Today, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_InconsistentCase_PalletID_VarianceIsNegative()
		{
			TestAcceptVariances_InconsistentCase_PalletIDCore(initalPalletID: "plt1", variancePalletID: "pLT1", adjustmentPalletID: "PLT1", varianceValue: -3);
		}

		public void TestAcceptVariances_InconsistentCase_PalletID_VarianceIsPositive()
		{
			TestAcceptVariances_InconsistentCase_PalletIDCore(initalPalletID: "plt1", variancePalletID: "Plt2", adjustmentPalletID: "PLT2", varianceValue: 3);
		}

		void TestAcceptVariances_InconsistentCase_PalletIDCore(string initalPalletID, string variancePalletID, string adjustmentPalletID, int varianceValue)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, initalPalletID, ZDate.Today, ZDate.Today, "AA",
				"BB", "CC", ZString.Empty);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: varianceValue, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: variancePalletID,
				partAttrib1: "aa", partAttrib2: "bB", partAttrib3: "Cc", expiryDate: ZDate.Today,
				packingDate: ZDate.Today, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertEquals("External Reference", "CYCLECOUNT ADJUSTMENT W00000002", finalAdjustment.WD_ExternalReference);
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, varianceValue, adjustmentPalletID, "AA", "BB", "CC",
				"", ZDate.Today, ZDate.Today, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_Damaged_To_LCC_HeldInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, "PLT1", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Held,
				originalInventoryHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.Damaged);

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Held,
				originalInventoryHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.LostInCycleCount);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10m, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });
			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -10, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged);

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Held,
				originalInventoryHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.Damaged);

			AssertVarianceStatus(variance,
				finalAdjustment.PK,
				CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_AllShortPicked_HeldInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.ShortPicked;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.ShortPicked);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10m, expectedQty: 10m, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -10m, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Available,
				originalHeldCode: string.Empty,
				currentAdjustmentStatus: InventoryStatus.Codes.Available,
				currentAdjustmentHeldCode: string.Empty);

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			AssertVarianceStatus(variance,
				finalAdjustment.PK,
				CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_LostCycleCount_HeldCode_Changed_To_Damaged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.Damaged);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10m, expectedQty: 10m, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });
			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -10m, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged);

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.Damaged);

			AssertVarianceStatus(variance,
				finalAdjustment.PK,
				CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_HeldInventory_HeldCodeIsNotLostInCycleCount()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1",
				InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -5, expectedQty: 20, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var cycleCountInFactory2 = factory2.Load<WhsCycleCountLocation>(cycleCount.PK);
			var logger = new DummyWhsDocketCreationLogger();
			cycleCountInFactory2.AcceptVariances(logger);
			factory2.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -5, palletID: "PLT1");

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_HeldInventory_AllStockAreDamaged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, location1, "PLT1",
				InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Held,
				originalInventoryHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.Damaged);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -5, expectedQty: 20, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var factory2 = new BusinessObjectFactory();
			var cycleCountInFactory2 = factory2.Load<WhsCycleCountLocation>(cycleCount.PK);
			var logger = new DummyWhsDocketCreationLogger();
			cycleCountInFactory2.AcceptVariances(logger);
			factory2.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, location1, -5m, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged);

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Held,
				originalInventoryHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.Damaged);

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_PrioritiseAdjustOutHeldStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -5, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var factory2 = new BusinessObjectFactory();
			var cycleCountInFactory2 = factory2.Load<WhsCycleCountLocation>(cycleCount.PK);
			var logger = new DummyWhsDocketCreationLogger();
			cycleCountInFactory2.AcceptVariances(logger);
			factory2.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -5, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Available,
				originalHeldCode: string.Empty,
				currentAdjustmentStatus: InventoryStatus.Codes.Available,
				currentAdjustmentHeldCode: string.Empty);

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_AllInventoryHaveBeenAdjustedOut()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -2, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -8m, location1.ToLocationString(), "PLT1",
				receiveLine.WE_AdjustmentArrivalDate);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -2, location1.ToLocationString(), "PLT1",
				receiveLine.WE_AdjustmentArrivalDate, InventoryHoldCodes.Codes.LostInCycleCount, "", "", "", "",
				ZDate.Empty, ZDate.Empty);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(adjustment);

			var factory2 = new BusinessObjectFactory();
			var cycleCountInFactory2 = factory2.Load<WhsCycleCountLocation>(cycleCount.PK);
			var logger = new DummyWhsDocketCreationLogger();
			cycleCountInFactory2.AcceptVariances(logger);
			AssertMultilineASCIIEquals("Error occurs.",
				@"Failed to create adjustment for Client '111', Warehouse '1' by cycle count for Location 'A-1' in Warehouse '1'.
Not enough inventory was found. The stock may be committed by existing job(s) or has been picked.
Error - WE_TransactionQuantity: Attempted to adjust 2 Units, but no Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.",
				logger.LogMessagesAllAppended);

			var factory3 = new BusinessObjectFactory();
			var varianceInFactory3 = factory3.Load<WhsCycleCountLocationVariance>(variance.PK);
			AssertVarianceStatus(varianceInFactory3, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open,
				CycleCountVarianceAuthorizedAction.Codes.Error);
		}

		public void TestAcceptVariances_VarianceIsNegative_AllInventoryHaveBeenAdjustedOutWithMultiParts()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 20m, location1, "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -2, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -5, expectedQty: 20, client: data.Org1, part: data.Part2, palletID: "PLT2",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -8m, location1.ToLocationString(), "PLT1",
				receiveLine1.WE_AdjustmentArrivalDate);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -2, location1.ToLocationString(), "PLT1",
				receiveLine1.WE_AdjustmentArrivalDate, InventoryHoldCodes.Codes.LostInCycleCount, "", "", "", "",
				ZDate.Empty, ZDate.Empty);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part2.PK, -15m, location1.ToLocationString(), "PLT2",
				receiveLine2.WE_AdjustmentArrivalDate);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part2.PK, -5, location1.ToLocationString(), "PLT2",
				receiveLine2.WE_AdjustmentArrivalDate, InventoryHoldCodes.Codes.LostInCycleCount, "", "", "", "",
				ZDate.Empty, ZDate.Empty);
			adjustment.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertIsFinalisedPrecondition(adjustment);

			var factory2 = new BusinessObjectFactory();
			var cycleCountInFactory2 = factory2.Load<WhsCycleCountLocation>(cycleCount.PK);
			var logger = new DummyWhsDocketCreationLogger();
			cycleCountInFactory2.AcceptVariances(logger);
			AssertMultilineASCIIEquals("Error occurs.",
				@"Failed to create adjustment for Client '111', Warehouse '1' by cycle count for Location 'A-1' in Warehouse '1'.
Not enough inventory was found. The stock may be committed by existing job(s) or has been picked.
Error - WE_TransactionQuantity: Attempted to adjust 2 Units, but no Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.
Error - WE_TransactionQuantity: Attempted to adjust 5 Units, but no Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.",
				logger.LogMessagesAllAppended);

			var factory3 = new BusinessObjectFactory();
			var variance1InFactory3 = factory3.Load<WhsCycleCountLocationVariance>(variance1.PK);
			var variance2InFactory3 = factory3.Load<WhsCycleCountLocationVariance>(variance2.PK);
			AssertVarianceStatus(variance1InFactory3, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open,
				CycleCountVarianceAuthorizedAction.Codes.Error);
			AssertVarianceStatus(variance2InFactory3, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open,
				CycleCountVarianceAuthorizedAction.Codes.Error);
		}

		public void TestAcceptVariances_VarianceIsNegative_HeldStockHasPartiallyCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -7, expectedQty: 20, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var holdCodeChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertEquals("Precondition: Should put held code for matched inventory", 7m,
				holdCodeChangeLines.Single().WE_StockOnHand);

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 3m, location1.ToLocationString(),
				"PLT1", location2.ToLocationString(), "PLT2", InventoryHoldCodes.Codes.LostInCycleCount);
			transferLine.RunPreSaveValidation();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var cycleCountInFactory2 = factory2.Load<WhsCycleCountLocation>(cycleCount.PK);
			var logger = new DummyWhsDocketCreationLogger();
			cycleCountInFactory2.AcceptVariances(logger);
			factory2.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertEquals("Should create 2 adjustment line", 2, finalAdjustment.Lines.Count);

			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -4, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Available,
				originalHeldCode: string.Empty,
				currentAdjustmentStatus: InventoryStatus.Codes.Available,
				currentAdjustmentHeldCode: string.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -3, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Available,
				originalHeldCode: string.Empty,
				currentAdjustmentStatus: InventoryStatus.Codes.Available,
				currentAdjustmentHeldCode: string.Empty);

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000003 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_HeldStockIsMoreThanExpected()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -5, expectedQty: 20, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			receiveLine.HeldCodeChangeQuantity = 12m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var holdCodeChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertEquals("Precondition: Should put held code for inventory", 2, holdCodeChangeLines.Length);
			var holdCodeChangeLine1 = holdCodeChangeLines.Single(l => l.WE_StockOnHand == 5);
			var holdCodeChangeLine2 = holdCodeChangeLines.Single(l => l.WE_StockOnHand == 12);

			var factory2 = new BusinessObjectFactory();
			var cycleCountInFactory2 = factory2.Load<WhsCycleCountLocation>(cycleCount.PK);
			var logger = new DummyWhsDocketCreationLogger();
			cycleCountInFactory2.AcceptVariances(logger);
			factory2.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -5, palletID: "PLT1");

			AssertAdjustmentLineStatus(finalAdjustment,
				originalStatus: InventoryStatus.Codes.Available,
				originalHeldCode: string.Empty,
				currentAdjustmentStatus: InventoryStatus.Codes.Available,
				currentAdjustmentHeldCode: string.Empty);

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);

			holdCodeChangeLine2.Reload();
			AssertEquals("Inventory Status", InventoryStatus.Codes.Available,
				holdCodeChangeLine2.WE_CurrentInventoryStatus);
			AssertEquals("Inventory Held Code", "", holdCodeChangeLine2.WE_WHC_NKCurrentInventoryHeldCode);
		}

		public void TestAcceptVariances_VarianceIsNegative_HeldStockIsMoreThanExpected_HasError()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 20m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -5, expectedQty: 20, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			receiveLine.HeldCodeChangeQuantity = 15m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var holdCodeChangeLines = Factory.Load<WhsDocketLine>(new ZQuery(
				WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode, InventoryHoldCodes.Codes.LostInCycleCount));
			AssertEquals("Precondition: Should put held code for inventory", 2, holdCodeChangeLines.Length);
			var holdCodeChangeLine1 = holdCodeChangeLines.Single(l => l.WE_StockOnHand == 5);
			var holdCodeChangeLine2 = holdCodeChangeLines.Single(l => l.WE_StockOnHand == 15);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -20m, location1.ToLocationString(), "PLT1",
				receiveLine.WE_AdjustmentArrivalDate, InventoryHoldCodes.Codes.LostInCycleCount, "", "", "", "",
				ZDate.Empty, ZDate.Empty);
			adjustment.RunPreSaveValidation(); // Commit held inventory
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var cycleCountInFactory2 = factory2.Load<WhsCycleCountLocation>(cycleCount.PK);
			var logger = new DummyWhsDocketCreationLogger();
			cycleCountInFactory2.AcceptVariances(logger);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create adjustment when inventory is committed", 0,
				adjustments.Count(adj => adj.PK != adjustment.PK));

			var factory3 = new BusinessObjectFactory();
			var varianceInFactory2 = factory3.Load<WhsCycleCountLocationVariance>(variance.PK);
			AssertVarianceStatus(varianceInFactory2, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open,
				CycleCountVarianceAuthorizedAction.Codes.Error);
			AssertMultilineASCIIEquals("Should log error message",
				@"Failed to create adjustment for Client '111', Warehouse '1' by cycle count for Location 'A-1' in Warehouse '1'.
Not enough inventory was found. The stock may be committed by existing job(s) or has been picked.
Error - WE_TransactionQuantity: Attempted to adjust 5 Units, but no Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.",
				logger.LogMessagesAllAppended);

			holdCodeChangeLine1.Reload();
			AssertEquals("Inventory Status should not be changed", InventoryStatus.Codes.Held,
				holdCodeChangeLine1.WE_CurrentInventoryStatus);
			AssertEquals("Inventory Held Code should not be changed", InventoryHoldCodes.Codes.LostInCycleCount,
				holdCodeChangeLine1.WE_WHC_NKCurrentInventoryHeldCode);

			holdCodeChangeLine2.Reload();
			AssertEquals("Inventory Status should not be changed", InventoryStatus.Codes.Held,
				holdCodeChangeLine2.WE_CurrentInventoryStatus);
			AssertEquals("Inventory Held Code should not be changed", InventoryHoldCodes.Codes.LostInCycleCount,
				holdCodeChangeLine2.WE_WHC_NKCurrentInventoryHeldCode);
		}

		public void TestAcceptVariances_VarianceIsNegative_IgnoreUnrelatedStock()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 4m, location1, "PLT1");
			Helper.CreateWhsReceiveLine(receive, data.Part1, 6m, location1, "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -3, expectedQty: 4, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertEquals("Should only have 1 adjustment line", 1, finalAdjustment.Lines.Count);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -3, palletID: "PLT1");

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_MultipleInventories()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: false);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 4m, location1, "PLT1", ZDate.Today, ZDate.Today, "AA",
				"BB", "CC", ZString.Empty);
			Helper.CreateWhsReceiveLine(receive, data.Part1, 6m, location1, "PLT1", ZDate.Today, ZDate.Today, "AA",
				"BB", "CC", ZString.Empty);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				partAttrib1: "AA", partAttrib2: "BB", partAttrib3: "CC", expiryDate: ZDate.Today,
				packingDate: ZDate.Today, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -6, "PLT1", "AA", "BB", "CC",
				"", ZDate.Today, ZDate.Today);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -4, "PLT1", "AA", "BB", "CC",
				"", ZDate.Today, ZDate.Today);

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_InventoryIsPartialCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT1");
			var receiveLine2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = Helper.CreateWhsOrderLine(order, data.Part1, 4m);
			orderLine1.ReserveStockIfAbleTo(receiveLine1);
			var orderLine2 = Helper.CreateWhsOrderLine(order, data.Part1, 6m);
			orderLine2.ReserveStockIfAbleTo(receiveLine2);
			Helper.CreatePickNew(order);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -4, palletID: "PLT1");
			AssertAdjustmentLine(finalAdjustment, data.Part1, data.Whs1.DefaultLocation, -6, palletID: "PLT1");

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000003 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_DifferentClients()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var org1 = data.Org1;
			var org2 = Helper.CreateClient("ORG2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT1");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -3, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -5, expectedQty: 10, client: org2, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);

			var adjustments = FindAdjustments(org1, org2);
			AssertEquals("Should create 2 adjustments for each client", 2, adjustments.Count());

			var finalAdjustment1 = adjustments.Single(adj => adj.WD_OH_Client == org1.PK);
			AssertAdjustment(finalAdjustment1, org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location1, -3, palletID: "PLT1");

			var finalAdjustment2 = adjustments.Single(adj => adj.WD_OH_Client == org2.PK);
			AssertAdjustment(finalAdjustment2, org2, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -5, palletID: "PLT1");

			var factory2 = new BusinessObjectFactory();
			var variance1InFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance1.PK);
			var variance2InFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance2.PK);
			AssertVarianceStatus(variance1InFactory2, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertVarianceStatus(variance2InFactory2, finalAdjustment2.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
				$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
				$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client 'ORG2', Warehouse '1'."
			};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsNegative_DifferentClients_HasError()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var org1 = data.Org1;
			var org2 = Helper.CreateClient("ORG2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT1");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			var transferLine = Helper.CreateWhsTransferLine(transfer, data.Part1, 5m, location1.ToLocationString(),
				"PLT1", location2.ToLocationString(), "PLT2");
			transferLine.RunPreSaveValidation();

			transferLine.PickedTime = ZDateTimeOffset.Now;
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_OriginalInventoryStatus);
			AssertEquals("Precondition - should be InTransit", InventoryStatus.Codes.InTransit,
				transferLine.WE_CurrentInventoryStatus);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -6, expectedQty: 10, client: org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -5, expectedQty: 10, client: org2, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);

			var adjustments = FindAdjustments(org1, org2);
			AssertEquals("Should not create adjustment when inventory is committed", 0, adjustments.Count());

			var factory2 = new BusinessObjectFactory();
			var variance1InFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance1.PK);
			var variance2InFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance2.PK);
			AssertVarianceStatus(variance1InFactory2, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open,
				CycleCountVarianceAuthorizedAction.Codes.Error);
			AssertVarianceStatus(variance2InFactory2, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open,
				CycleCountVarianceAuthorizedAction.Codes.Error);
			AssertMultilineASCIIEquals("Should log error message",
				@"Failed to create adjustment for Client '111', Warehouse '1' by cycle count for Location 'A-1' in Warehouse '1'.
Not enough inventory was found. The stock may be committed by existing job(s) or has been picked.
Error - WE_TransactionQuantity: Attempted to adjust 1 Unit, but no Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.",
				logger.LogMessagesAllAppended);
		}

		public void TestAcceptVariances_VarianceIsNegative_InventoryIsCommitted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var pick = Helper.CreatePickNew(order);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -6, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create adjustment when inventory is committed", 0, adjustments.Count());

			var factory2 = new BusinessObjectFactory();
			var varianceInFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance.PK);
			AssertVarianceStatus(varianceInFactory2, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open,
				CycleCountVarianceAuthorizedAction.Codes.Error);
			AssertMultilineASCIIEquals("Should log error message",
				@"Failed to create adjustment for Client '111', Warehouse '1' by cycle count for Location 'A-1' in Warehouse '1'.
Not enough inventory was found. The stock may be committed by existing job(s) or has been picked.
Error - WE_TransactionQuantity: Attempted to adjust 1 Unit, but no Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.",
				logger.LogMessagesAllAppended);
		}

		public void TestAcceptVariances_VarianceIsNegative_InventoryIsNotEnough()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive1 =
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -6, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustmentLine(adjustment, data.Part1, -5m, location1.ToLocationString(), palletID: "PLT1");
			adjustment.FinaliseDocketWithoutUserConfirmation();
			AssertIsFinalisedPrecondition(adjustment);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create adjustment when inventory is not enough to adjust", 0,
				adjustments.Count(adj => adj.PK != adjustment.PK));

			var factory2 = new BusinessObjectFactory();
			var varianceInFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance.PK);
			AssertVarianceStatus(varianceInFactory2, ZGuid.Empty, CycleCountVarianceStatus.Codes.Open,
				CycleCountVarianceAuthorizedAction.Codes.Error);
			AssertMultilineASCIIEquals("Should log error message",
				@"Failed to create adjustment for Client '111', Warehouse '1' by cycle count for Location 'A-1' in Warehouse '1'.
Not enough inventory was found. The stock may be committed by existing job(s) or has been picked.
Error - WE_TransactionQuantity: Attempted to adjust 1 Unit, but no Units are available for adjustment out of this location.
Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the adjustment line.
Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to adjust.
If you are trying to adjust stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.
If you are trying to adjust stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.",
				logger.LogMessagesAllAppended);
		}

		#endregion

		#region TestAcceptVariances_VarianceIsPositive

		public void TestAcceptVariances_VarianceIsPositive()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 3, partAttrib1: "AA",
				partAttrib2: "BB", partAttrib3: "CC", expiryDate: ZDate.Today, packingDate: ZDate.Today,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, location1, 3, "PLT1", "AA", "BB", "CC", "", ZDate.Today,
				ZDate.Today, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000001 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsPositive_PalletIDInDifferentLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 6, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment1 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity == 6));
			AssertAdjustment(finalAdjustment1, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 6, palletID: "PLT1",
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var finalAdjustment2 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity == -10));
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -10, palletID: "PLT1",
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertVarianceStatus(variance, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
				$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
				$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
			};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsPositive_PalletIDInDifferentLocation_HeldPallet()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 6, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment1 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity == 6));
			AssertAdjustment(finalAdjustment1, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 6, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var finalAdjustment2 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity == -10));
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -10, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertVarianceStatus(variance, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
					$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
					$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
				};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsPositive_PalletIDInDifferentLocation_HeldPallet_CompletelyDifferentInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part2, varianceQty: 6, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment1 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity == 6));
			AssertAdjustment(finalAdjustment1, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part2, location2, 6, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var finalAdjustment2 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity == -10));
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -10, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertVarianceStatus(variance, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
					$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
					$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
				};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsPositive_PalletIDInDifferentLocation_HeldPallet_CompletelyDifferentInventory_MixedHoldCodes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 7m, location1, "PLT1", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 2m, location1, "PLT1", InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Held);
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "PLT1", InventoryStatus.Codes.Available, "");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine1.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine1.ChangeInventoryHeldCode(true);
			receiveLine2.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine2.ChangeInventoryHeldCode(true);
			receiveLine3.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine3.ChangeInventoryHeldCode(true);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part2, varianceQty: 6, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment1 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity == 6));
			AssertAdjustment(finalAdjustment1, data.Org1, data.Whs1, ZGuid.Empty);

			// Completely new inventory found, default to held as old inventory is mixed hold codes
			AssertAdjustmentLine(finalAdjustment1, data.Part2, location2, 6, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Held,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Held,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var finalAdjustment2 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity == -7));
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);

			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -7, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -2, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Held,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Held,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -1, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Available,
				originalHeldCode: "",
				currentAdjustmentStatus: InventoryStatus.Codes.Available,
				currentAdjustmentHeldCode: "",
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertVarianceStatus(variance, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
					$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
					$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
				};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_MultipleHoldCodeChanges()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 6, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment1 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity == 6));
			AssertAdjustment(finalAdjustment1, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 6, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var finalAdjustment2 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity == -10));
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -10, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertVarianceStatus(variance, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
					$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
					$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
				};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_MultipleHoldCodeChanges_FinalIsAvailable()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			receiveLine.HeldCodeToChangeTo = string.Empty;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 6, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment1 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity == 6));
			AssertAdjustment(finalAdjustment1, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 6, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Available,
				originalHeldCode: string.Empty,
				currentAdjustmentStatus: InventoryStatus.Codes.Available,
				currentAdjustmentHeldCode: string.Empty,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var finalAdjustment2 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity == -10));
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -10, palletID: "PLT1",
				originalStatus: InventoryStatus.Codes.Available,
				originalHeldCode: string.Empty,
				currentAdjustmentStatus: InventoryStatus.Codes.Available,
				currentAdjustmentHeldCode: string.Empty,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertVarianceStatus(variance, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
					$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
					$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
				};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsPositive_PalletIDInDifferentLocation_InDifferentWarehouse()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = whs2.FindLocation("B-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 6, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, whs2, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, location2, 6, palletID: "PLT1");

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse 'WH2'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsPositive_PalletIDInDifferentLocation_InDifferentClient()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var org2 = Helper.CreateClient("ORG2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: org2, part: data.Part1, varianceQty: 6, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1, org2);
			AssertEquals("Should create adjustment for each client", 2, adjustments.Count());

			var finalAdjustment1 = adjustments.Single(adj => adj.WD_OH_Client == org2.PK);
			AssertAdjustment(finalAdjustment1, org2, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 6, palletID: "PLT1");

			var finalAdjustment2 = adjustments.Single(adj => adj.WD_OH_Client == data.Org1.PK);
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -10, palletID: "PLT1");

			AssertVarianceStatus(variance, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
				$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client 'ORG2', Warehouse '1'.",
				$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
			};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsPositive_PalletIDInDifferentLocation_HasMultipleClients()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var org1 = data.Org1;
			var org2 = Helper.CreateClient("ORG2");
			Helper.CreateProductClientRelationShip(org2, data.Part2);
			Factory.Save();

			Helper.CreateWhsReceiveWithInventory(org1, data.Whs1, "R1", data.Part1, 10m, location1, "PLT1");
			Helper.CreateWhsReceiveWithInventory(org2, data.Whs1, "R2", data.Part2, 8m, location1, "PLT1");
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: org1, part: data.Part1, varianceQty: 6, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: org2, part: data.Part2, varianceQty: 3, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(org1, org2);
			AssertEquals("Should create adjustment for each client and each PalletID", 4, adjustments.Count());

			var finalAdjustment1 = adjustments.Single(adj =>
				adj.WD_OH_Client == org1.PK && adj.Lines.Any(l => l.WE_TransactionQuantity == 6));
			AssertAdjustment(finalAdjustment1, org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 6, palletID: "PLT1");

			var finalAdjustment2 = adjustments.Single(adj =>
				adj.WD_OH_Client == org2.PK && adj.Lines.Any(l => l.WE_TransactionQuantity == 3));
			AssertAdjustment(finalAdjustment2, org2, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment2, data.Part2, location2, 3, palletID: "PLT1");

			var finalAdjustment3 = adjustments.Single(adj =>
				adj.WD_OH_Client == org1.PK && adj.Lines.Any(l => l.WE_TransactionQuantity == -10));
			AssertAdjustment(finalAdjustment3, org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment3, data.Part1, location1, -10, palletID: "PLT1");

			var finalAdjustment4 = adjustments.Single(adj =>
				adj.WD_OH_Client == org2.PK && adj.Lines.Any(l => l.WE_TransactionQuantity == -8));
			AssertAdjustment(finalAdjustment4, org2, data.Whs1, finalAdjustment2.PK);
			AssertAdjustmentLine(finalAdjustment4, data.Part2, location1, -8, palletID: "PLT1");

			AssertVarianceStatus(variance1, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertVarianceStatus(variance2, finalAdjustment2.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new string[]
			{
				$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
				$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client 'ORG2', Warehouse '1'.",
				$"Adjustment {finalAdjustment3.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
				$"Adjustment {finalAdjustment4.WD_DocketID} was created successfully for Client 'ORG2', Warehouse '1'."
			};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		#region TestAcceptVariances_VarianceIsPositive_SerialNumberAndPalletIDAreInDifferentLocation

		public void TestAcceptVariances_VarianceIsPositive_SerialNumberAndPalletIDAreInDifferentLocation()
		{
			var serial = "S1";
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var location3 = data.Whs1.FindLocation("A-3");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "PLT2", ZDate.Today,
				ZDate.Today, "S1", "S2", "S3", ZString.Empty);
			receiveLine1.WE_SerialNumber = serial;
			Helper.CreateWhsReceiveLine(receive, data.Part2, 5m, location2, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location3,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "S1",
				partAttrib2: "S2", partAttrib3: "S3", serialNumber: serial, expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should create adjustments for adjust in&out", 2, adjustments.Count());

			var finalAdjustment1 = adjustments.Single(adj => adj.Lines.Count == 1);
			AssertAdjustment(finalAdjustment1, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location3, 1, "PLT1", "S1", "S2", "S3", serial,
				ZDate.Today, ZDate.Today, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var finalAdjustment2 = adjustments.Single(adj => adj.Lines.Count == 2);
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -1, "PLT2", "S1", "S2", "S3", serial,
				ZDate.Today, ZDate.Today, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			AssertAdjustmentLine(finalAdjustment2, data.Part2, location2, -5, palletID: "PLT1",
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertVarianceStatus(variance, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
				$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
				$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
			};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		#endregion

		#region TestAcceptVariances_VarianceIsPositive_SerialNumberInDifferentLocation_DifferentClients

		public void TestAcceptVariances_VarianceIsPositive_SerialNumberInDifferentLocation_DifferentClients()
		{
			var serial = "S1";
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var org1 = data.Org1;
			var org2 = Helper.CreateClient("ORG2");
			Helper.CreateProductClientRelationShip(org2, data.Part1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAllAttributeType(org1, true);
			Helper.SetProductAllAttributeUse(org1, data.Part1, use: true, useSerialNumber: true);

			Helper.SetClientAllAttributeType(org2, true);
			Helper.SetProductAllAttributeUse(org2, data.Part1, use: true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "", ZDate.Today,
				ZDate.Today, "S1", "S2", "S3", ZString.Empty);
			receiveLine1.WE_SerialNumber = serial;
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "", client: org2, part: data.Part1, varianceQty: 1, partAttrib1: "S1", partAttrib2: "S2",
				partAttrib3: "S3", serialNumber: serial, expiryDate: ZDate.Today, packingDate: ZDate.Today,
				expectedLocation: location1, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(org1, org2);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, org2, data.Whs1, ZGuid.Empty);
			AssertEquals("Should only have 1 adjustment line", 1, finalAdjustment.Lines.Count);
			AssertAdjustmentLine(finalAdjustment, data.Part1, location2, 1, "", "S1", "S2", "S3", serial, ZDate.Today,
				ZDate.Today);

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client 'ORG2', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		#endregion

		public void TestAcceptVariances_VarianceIsPositive_SerialNumberInDifferentLocation_DifferentProducts()
		{
			var serial = "S1";
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, useSerialNumber: true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part2, use: true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "", ZDate.Today,
				ZDate.Today, "S1", "S2", "S3", ZString.Empty);
			receiveLine1.WE_SerialNumber = serial;
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "", client: data.Org1, part: data.Part2, varianceQty: 1, partAttrib1: "S1", partAttrib2: "S2",
				partAttrib3: "S3", serialNumber: serial, expiryDate: ZDate.Today, packingDate: ZDate.Today,
				expectedLocation: location1, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertEquals("Should only have 1 adjustment line", 1, finalAdjustment.Lines.Count);
			AssertAdjustmentLine(finalAdjustment, data.Part2, location2, 1, "", "S1", "S2", "S3", serial, ZDate.Today,
				ZDate.Today);

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		#region TestAcceptVariances_VarianceIsPositive_SerialNumberInDifferentLocation

		public void TestAcceptVariances_VarianceIsPositive_SerialNumberInDifferentLocation()
		{
			var org1Serial = "S11";
			var org2Serial = "S21";
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "PLT1", ZDate.Today,
				ZDate.Today, "S11", "S12", "S13", ZString.Empty);
			receiveLine1.WE_SerialNumber = org1Serial;
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, ZString.Empty,
				ZDate.Today, ZDate.Today, "S21", "S22", "S23", ZString.Empty);
			receiveLine2.WE_SerialNumber = org2Serial;
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "S11",
				partAttrib2: "S12", partAttrib3: "S13", serialNumber: org1Serial, expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: ZString.Empty, client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "S21",
				partAttrib2: "S22", partAttrib3: "S23", serialNumber: org2Serial, expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should create adjustment for adjust in & out", 2, adjustments.Count());

			var finalAdjustment1 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK));
			AssertAdjustment(finalAdjustment1, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 1, "PLT1", "S11", "S12", "S13", org1Serial,
				ZDate.Today, ZDate.Today, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 1, ZString.Empty, "S21", "S22", "S23",
				org2Serial, ZDate.Today, ZDate.Today,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var finalAdjustment2 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location1.PK));
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -1, "PLT1", "S11", "S12", "S13", org1Serial,
				ZDate.Today, ZDate.Today, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -1, ZString.Empty, "S21", "S22", "S23",
				org2Serial, ZDate.Today, ZDate.Today,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertVarianceStatus(variance1, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertVarianceStatus(variance2, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
				$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
				$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
			};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsPositive_SerialNumberInDifferentLocation_HeldInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, useSerialNumber: true);

			var serial1 = "S11";
			var serial2 = "S21";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "PLT1", ZDate.Today,
				ZDate.Today, "S11", "S12", "S13", ZString.Empty);
			receiveLine1.WE_SerialNumber = serial1;
			receiveLine1.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, ZString.Empty,
				ZDate.Today, ZDate.Today, "S21", "S22", "S23", ZString.Empty);
			receiveLine2.WE_SerialNumber = serial2;
			receiveLine2.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
			receiveLine2.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine1.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine1.ChangeInventoryHeldCode(true);
			receiveLine2.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine2.ChangeInventoryHeldCode(true);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "S11",
				partAttrib2: "S12", partAttrib3: "S13", serialNumber: serial1, expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: ZString.Empty, client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "S21",
				partAttrib2: "S22", partAttrib3: "S23", serialNumber: serial2, expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment1 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK));
			AssertAdjustment(finalAdjustment1, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 1, "PLT1", "S11", "S12", "S13", serial1,
				ZDate.Today, ZDate.Today,
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);

			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 1, ZString.Empty, "S21", "S22", "S23",
				serial2, ZDate.Today, ZDate.Today,
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var finalAdjustment2 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location1.PK));
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -1, "PLT1", "S11", "S12", "S13", serial1,
				ZDate.Today, ZDate.Today,
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);

			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -1, ZString.Empty, "S21", "S22", "S23",
				serial2, ZDate.Today, ZDate.Today,
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertVarianceStatus(variance1, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertVarianceStatus(variance2, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
					$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
					$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
				};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_VarianceIsPositive_SerialNumberInDifferentLocation_HeldInventory_DifferentHoldCodes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, useSerialNumber: true);

			var serial1 = "S11";
			var serial2 = "S21";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, ZString.Empty,
				ZDate.Today, ZDate.Today, "S11", "S12", "S13", ZString.Empty);
			receiveLine1.WE_SerialNumber = serial1;
			receiveLine1.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
			receiveLine1.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Held;

			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, ZString.Empty,
				ZDate.Today, ZDate.Today, "S21", "S22", "S23", ZString.Empty);
			receiveLine2.WE_SerialNumber = serial2;
			receiveLine2.WE_OriginalInventoryStatus = InventoryStatus.Codes.Held;
			receiveLine2.WE_WHC_NKOriginalInventoryHeldCode = InventoryHoldCodes.Codes.Damaged;

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine1.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine1.ChangeInventoryHeldCode(true);
			receiveLine2.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine2.ChangeInventoryHeldCode(true);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "S11",
				partAttrib2: "S12", partAttrib3: "S13", serialNumber: serial1, expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: ZString.Empty, client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "S21",
				partAttrib2: "S22", partAttrib3: "S23", serialNumber: serial2, expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment1 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK));
			AssertAdjustment(finalAdjustment1, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 1, "PLT1", "S11", "S12", "S13", serial1,
				ZDate.Today, ZDate.Today,
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Held,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Held,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);

			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 1, ZString.Empty, "S21", "S22", "S23",
				serial2, ZDate.Today, ZDate.Today,
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var finalAdjustment2 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location1.PK));
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -1, ZString.Empty, "S11", "S12", "S13", serial1,
				ZDate.Today, ZDate.Today,
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Held,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Held,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);

			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -1, ZString.Empty, "S21", "S22", "S23",
				serial2, ZDate.Today, ZDate.Today,
				originalStatus: InventoryStatus.Codes.Held,
				originalHeldCode: InventoryHoldCodes.Codes.Damaged,
				currentAdjustmentStatus: InventoryStatus.Codes.Held,
				currentAdjustmentHeldCode: InventoryHoldCodes.Codes.Damaged,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			AssertVarianceStatus(variance1, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertVarianceStatus(variance2, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
					$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
					$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
				};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		#endregion

		#region TestAcceptVariances_VarianceIsPositive_SerialNumberInDifferentLocation_HasMultipleClients

		public void TestAcceptVariances_VarianceIsPositive_SerialNumberInDifferentLocation_HasMultipleClients()
		{
			var serial = "S1";
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var org2 = Helper.CreateClient("ORG2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, ZString.Empty,
				ZDate.Today, ZDate.Today, "S1", "S2", "S3", ZString.Empty);
			receiveLine.WE_SerialNumber = serial;
			receive.FinaliseDocketWithoutUserConfirmation();

			var receive2 = Helper.CreateWhsReceive(org2, data.Whs1, "R1");
			Helper.CreateWhsReceiveLine(receive2, data.Part1, 1m, location1, ZString.Empty, ZDate.Empty, ZDate.Empty,
				ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty);
			receive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				"", data.Org1, data.Part1, 1, "S1", "S2", "S3", serial, ZDate.Today, ZDate.Today,
				expectedLocation: location1, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should create adjustment for adjust in & out", 2, adjustments.Count());

			var finalAdjustment1 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity > 0));
			AssertAdjustment(finalAdjustment1, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 1, ZString.Empty, "S1", "S2", "S3", serial,
				ZDate.Today, ZDate.Today);

			var finalAdjustment2 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_TransactionQuantity < 0));
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -1, ZString.Empty, "S1", "S2", "S3", serial,
				ZDate.Today, ZDate.Today);

			AssertVarianceStatus(variance, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
				$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse '1'.",
				$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
			};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		#endregion

		#region TestAcceptVariances_VarianceIsPositive_SerialNumberInDifferentWarehouse

		public void TestAcceptVariances_VarianceIsPositive_SerialNumberInDifferentWarehouse()
		{
			var serial = "S1";
			var data = new TestDataSimpleEnvironment(Factory, 3, 1);
			var whs2 = Helper.CreateWarehouse("WH2", "B", 2, 1);
			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = whs2.FindLocation("B-2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "PLT1", ZDate.Today,
				ZDate.Today, "S1", "S2", "S3", ZString.Empty);
			receiveLine.WE_SerialNumber = serial;
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "S1",
				partAttrib2: "S2", partAttrib3: "S3", serialNumber: serial, expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should create adjustment for each warehouse", 2, adjustments.Count());

			var finalAdjustment1 = adjustments.Single(adj => adj.WD_WW_Whs == whs2.PK);
			AssertAdjustment(finalAdjustment1, data.Org1, whs2, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 1, "PLT1", "S1", "S2", "S3", serial,
				ZDate.Today, ZDate.Today);
			AssertVarianceStatus(variance, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var finalAdjustment2 = adjustments.Single(adj => adj.WD_WW_Whs == data.Whs1.PK);
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -1, "PLT1", "S1", "S2", "S3", serial,
				ZDate.Today, ZDate.Today);

			AssertVarianceStatus(variance, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
				$"Adjustment {finalAdjustment1.WD_DocketID} was created successfully for Client '111', Warehouse 'WH2'.",
				$"Adjustment {finalAdjustment2.WD_DocketID} was created successfully for Client '111', Warehouse '1'."
			};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		#endregion

		public void TestAcceptVariances_VarianceIsPositive_FixedLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var fixLocationType = Helper.CreateLocationType("TE1", "Test 1", false, 1, LocationClasses.Codes.FIX);
			location2.WLV_WLT_LocationType = fixLocationType.PK;

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: true);
			Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var inventory = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "", ZDate.Today,
				ZDate.Today, "AA", "BB", "CC", ZString.Empty);
			inventory.WE_SerialNumber = "S1";
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "", client: data.Org1, part: data.Part1, varianceQty: 1, partAttrib1: "AA", partAttrib2: "BB",
				partAttrib3: "CC", serialNumber: "S1", expiryDate: ZDate.Today, packingDate: ZDate.Today,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should create 2 adjustments", 2, adjustments.Count());

			var finalAdjustment1 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK));
			AssertAdjustment(finalAdjustment1, data.Org1, data.Whs1, ZGuid.Empty);
			AssertEquals("Should only have 1 adjustment line", 1, finalAdjustment1.Lines.Count);
			AssertAdjustmentLine(finalAdjustment1, data.Part1, location2, 1, "", "AA", "BB", "CC", "S1", ZDate.Today,
				ZDate.Today);

			var finalAdjustment2 = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location1.PK));
			AssertAdjustment(finalAdjustment2, data.Org1, data.Whs1, finalAdjustment1.PK);
			AssertEquals("Should only have 1 adjustment line", 1, finalAdjustment2.Lines.Count);
			AssertAdjustmentLine(finalAdjustment2, data.Part1, location1, -1, "", "AA", "BB", "CC", "S1", ZDate.Today,
				ZDate.Today);

			AssertVarianceStatus(variance, finalAdjustment1.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);

			var expectedLogs = new[]
			{
				$"Adjustment W00000002 was created successfully for Client '111', Warehouse '1'.",
				$"Adjustment W00000003 was created successfully for Client '111', Warehouse '1'."
			};
			AssertContainsExactElementsInAnyOrder("Should log success message", expectedLogs,
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_InvalidAdjustmentReasonCode()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Factory.Save();

			var invalidAdjustmentCode = "ZZZ";
			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 3, partAttrib1: "AA",
				partAttrib2: "BB", partAttrib3: "CC", expiryDate: ZDate.Today, packingDate: ZDate.Today,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: invalidAdjustmentCode);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, location1, 3, "PLT1", "AA", "BB", "CC", "", ZDate.Today,
				ZDate.Today);

			AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000001 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

		public void TestAcceptVariances_CustomAdjustmentReasonCode()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			var adjustmentReasons =
				(SystemDefinableCodeDescriptionBoolCollection)WarehouseDataRegistry.Instance.AdjustmentReasonCodes
					.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			var customAdjustmentReason = adjustmentReasons.AddNew();
			customAdjustmentReason.Code = "XXX";
			Factory.Save();

			using (WarehouseDataRegistry.Instance.AdjustmentReasonCodes.SetTemporaryValue(Guid.Empty, Guid.Empty,
					   Guid.Empty, adjustmentReasons))
			{
				var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var refAdjustmentLine = newFactory.New<WhsAdjustmentLine>();
				Assert("Precondition", refAdjustmentLine.Lookups.AdjustmentReasonCodes.ContainsCode("XXX"));

				var now = DateTimeOffset.Now;
				var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
					CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
				var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount,
					CycleCountVarianceStatus.Codes.Open, palletID: "PLT1", client: data.Org1, part: data.Part1,
					varianceQty: 3, partAttrib1: "AA", partAttrib2: "BB", partAttrib3: "CC", expiryDate: ZDate.Today,
					packingDate: ZDate.Today, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
					adjustmentReasonCode: "XXX");
				Factory.Save();

				var logger = new DummyWhsDocketCreationLogger();
				cycleCount.AcceptVariances(logger);
				Factory.Save();

				var adjustments = FindAdjustments(data.Org1);
				var finalAdjustment = adjustments.Single();
				AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
				AssertAdjustmentLine(finalAdjustment, data.Part1, location1, 3, "PLT1", "AA", "BB", "CC", "",
					ZDate.Today, ZDate.Today, adjustmentReasonCode: "XXX");

				AssertVarianceStatus(variance, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
					CycleCountVarianceAuthorizedAction.Codes.Empty);
				AssertContainsExactElementsInAnyOrder("Should log success message",
					new[] { "Adjustment W00000001 was created successfully for Client '111', Warehouse '1'." },
					logger.AllIndividualLogMessages);
			}
		}

		public void TestAcceptVariances_MultipleAdjustmentReasonCodes()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, use: true, setReleaseCaptured: false,
				useSerialNumber: false);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 3, partAttrib1: "AA",
				partAttrib2: "BB", partAttrib3: "CC", expiryDate: ZDate.Today, packingDate: ZDate.Today,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT2", client: data.Org1, part: data.Part1, varianceQty: 4, partAttrib1: "DD",
				partAttrib2: "EE", partAttrib3: "FF", expiryDate: ZDate.Today, packingDate: ZDate.Today,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			var finalAdjustment = adjustments.Single();
			AssertAdjustment(finalAdjustment, data.Org1, data.Whs1, ZGuid.Empty);
			AssertAdjustmentLine(finalAdjustment, data.Part1, location1, 3, "PLT1", "AA", "BB", "CC", "", ZDate.Today,
				ZDate.Today, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			AssertAdjustmentLine(finalAdjustment, data.Part1, location1, 4, "PLT2", "DD", "EE", "FF", "", ZDate.Today,
				ZDate.Today, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);

			AssertVarianceStatus(variance1, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertVarianceStatus(variance2, finalAdjustment.PK, CycleCountVarianceStatus.Codes.Approved,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000001 was created successfully for Client '111', Warehouse '1'." },
				logger.AllIndividualLogMessages);
		}

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
			var cycleCount2Variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: -5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var logger1 = new DummyWhsDocketCreationLogger();
			cycleCount1.AcceptVariances(logger1);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount1Variance.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount1Variance.WCC_AuthorizedAction);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'.", "Adjustment W00000003 was created successfully for Client '111', Warehouse '1'." },
				logger1.AllIndividualLogMessages);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should have 2 adjustments", 2, adjustments.Count());

			var adjustmentIn = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location1.PK));
			AssertAdjustmentLine(adjustmentIn, data.Part1, location1, 5, "PLT1", "", "", "", "", null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var adjustmentOut = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK));
			AssertAdjustmentLine(adjustmentOut, data.Part1, location2, -5, "PLT1", "", "", "", "", null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var newFactory = new BusinessObjectFactory();
			var cycleCount2VarianceInNewFactory = newFactory.Load<WhsCycleCountLocationVariance>(cycleCount2Variance.PK);
			AssertEquals("Negative Variance should have an adjustment.", adjustmentOut.PK, cycleCount2VarianceInNewFactory.WCC_WD_RelatedAdjustment);

			var logger2 = new DummyWhsDocketCreationLogger();
			var cycleCount2InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount2.PK);
			cycleCount2.AcceptVariances(logger2);

			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount2VarianceInNewFactory.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount2VarianceInNewFactory.WCC_AuthorizedAction);
		}

		public void TestAcceptVariances_ShouldLinkAdjustmentOutToOpenNegativeVarianceForTheSamePalletID_AcceptNegativeVarianceFirst()
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
			var cycleCount2Variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: -5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var logger1 = new DummyWhsDocketCreationLogger();
			cycleCount2.AcceptVariances(logger1);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount2Variance.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount2Variance.WCC_AuthorizedAction);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger1.AllIndividualLogMessages);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should have 1 adjustment", 1, adjustments.Count());

			var adjustmentOut = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK));
			AssertAdjustmentLine(adjustmentOut, data.Part1, location2, -5, "PLT1", "", "", "", "", null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);

			var logger2 = new DummyWhsDocketCreationLogger();
			cycleCount1.AcceptVariances(logger2);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount1Variance.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount1Variance.WCC_AuthorizedAction);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000003 was created successfully for Client '111', Warehouse '1'." },
				logger2.AllIndividualLogMessages);

			var adjustments2 = FindAdjustments(data.Org1);
			AssertEquals("Should have 2 adjustments this time", 2, adjustments2.Count());

			var adjustmentIn = adjustments2.Single(adj => adj.Lines.Any(l => l.WE_WL == location1.PK));
			AssertAdjustmentLine(adjustmentIn, data.Part1, location1, 5, "PLT1", "", "", "", "", null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
		}

		public void TestAcceptVariances_ShouldLinkAdjustmentOutToOpenNegativeVarianceForTheSamePalletID_WithSiblingVariance()
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
			var cycleCount2Variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: -5, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			var cycleCount2Variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "PLT2", client: data.Org1, part: data.Part1, varianceQty: 10, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var logger1 = new DummyWhsDocketCreationLogger();
			cycleCount1.AcceptVariances(logger1);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount1Variance.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount1Variance.WCC_AuthorizedAction);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'.", "Adjustment W00000003 was created successfully for Client '111', Warehouse '1'." },
				logger1.AllIndividualLogMessages);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should have 2 adjustments", 2, adjustments.Count());

			var adjustmentIn = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location1.PK));
			AssertAdjustmentLine(adjustmentIn, data.Part1, location1, 5, "PLT1", "", "", "", "", null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			var adjustmentOut = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK && l.WE_PalletID == "PLT1"));
			AssertAdjustmentLine(adjustmentOut, data.Part1, location2, -5, "PLT1", "", "", "", "", null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var newFactory = new BusinessObjectFactory();
			var cycleCount2VarianceInNewFactory = newFactory.Load<WhsCycleCountLocationVariance>(cycleCount2Variance1.PK);
			AssertEquals("Negative Variance should have an adjustment.", adjustmentOut.PK, cycleCount2VarianceInNewFactory.WCC_WD_RelatedAdjustment);

			var logger2 = new DummyWhsDocketCreationLogger();
			var cycleCount2InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount2.PK);
			cycleCount2.AcceptVariances(logger2);

			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount2VarianceInNewFactory.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount2VarianceInNewFactory.WCC_AuthorizedAction);

			var adjustments2 = FindAdjustments(data.Org1);
			AssertEquals("Should have 3 adjustments this time, new adjustment added for sibling variance for another pallet id.", 3, adjustments2.Count());
			var newAdjustment = adjustments2.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK && l.WE_PalletID == "PLT2"));
			AssertAdjustmentLine(newAdjustment, data.Part1, location2, 10, "PLT2", "", "", "", "", null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
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
			var cycleCount2Variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: -1, serialNumber: serial, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var logger1 = new DummyWhsDocketCreationLogger();
			cycleCount1.AcceptVariances(logger1);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount1Variance.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount1Variance.WCC_AuthorizedAction);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'.", "Adjustment W00000003 was created successfully for Client '111', Warehouse '1'." },
				logger1.AllIndividualLogMessages);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should have 2 adjustments", 2, adjustments.Count());

			var adjustmentIn = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location1.PK));
			AssertAdjustmentLine(adjustmentIn, data.Part1, location1, 1, "", "", "", "", serial, null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var adjustmentOut = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK));
			AssertAdjustmentLine(adjustmentOut, data.Part1, location2, -1, "", "", "", "", serial, null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var newFactory = new BusinessObjectFactory();
			var cycleCount2VarianceInNewFactory = newFactory.Load<WhsCycleCountLocationVariance>(cycleCount2Variance.PK);
			AssertEquals("Negative Variance should have an adjustment.", adjustmentOut.PK, cycleCount2VarianceInNewFactory.WCC_WD_RelatedAdjustment);

			var logger2 = new DummyWhsDocketCreationLogger();
			var cycleCount2InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount2.PK);
			cycleCount2.AcceptVariances(logger2);

			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount2VarianceInNewFactory.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount2VarianceInNewFactory.WCC_AuthorizedAction);
		}

		public void TestAcceptVariances_ShouldLinkAdjustmentOutToVariancesForTheSameSerialNumber_AcceptNegativeVarianceFirst()
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
			var cycleCount2Variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: -1, serialNumber: serial, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var logger1 = new DummyWhsDocketCreationLogger();
			cycleCount2.AcceptVariances(logger1);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount2Variance.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount2Variance.WCC_AuthorizedAction);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'." },
				logger1.AllIndividualLogMessages);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should have 1 adjustment", 1, adjustments.Count());

			var adjustmentOut = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK));
			AssertAdjustmentLine(adjustmentOut, data.Part1, location2, -1, "", "", "", "", serial, null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);

			var logger2 = new DummyWhsDocketCreationLogger();
			cycleCount1.AcceptVariances(logger2);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount1Variance.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount1Variance.WCC_AuthorizedAction);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000003 was created successfully for Client '111', Warehouse '1'." },
				logger2.AllIndividualLogMessages);

			var adjustments2 = FindAdjustments(data.Org1);
			AssertEquals("Should have 2 adjustments this time", 2, adjustments2.Count());

			var adjustmentIn = adjustments2.Single(adj => adj.Lines.Any(l => l.WE_WL == location1.PK));
			AssertAdjustmentLine(adjustmentIn, data.Part1, location1, 1, "", "", "", "", serial, null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
		}

		public void TestAcceptVariances_ShouldLinkAdjustmentOutToVariancesForTheSameSerialNumber_WithSiblingVariance()
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
			var cycleCount2Variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: -1, serialNumber: serial, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			var cycleCount2Variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: 1, serialNumber: "SN2", authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
			Factory.Save();

			var logger1 = new DummyWhsDocketCreationLogger();
			cycleCount1.AcceptVariances(logger1);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount1Variance.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount1Variance.WCC_AuthorizedAction);
			AssertContainsExactElementsInAnyOrder("Should log success message",
				new[] { "Adjustment W00000002 was created successfully for Client '111', Warehouse '1'.", "Adjustment W00000003 was created successfully for Client '111', Warehouse '1'." },
				logger1.AllIndividualLogMessages);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should have 2 adjustments", 2, adjustments.Count());

			var adjustmentIn = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location1.PK));
			AssertAdjustmentLine(adjustmentIn, data.Part1, location1, 1, "", "", "", "", serial, null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var adjustmentOut = adjustments.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK));
			AssertAdjustmentLine(adjustmentOut, data.Part1, location2, -1, "", "", "", "", serial, null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);

			var newFactory = new BusinessObjectFactory();
			var cycleCount2VarianceInNewFactory = newFactory.Load<WhsCycleCountLocationVariance>(cycleCount2Variance1.PK);
			AssertEquals("Negative Variance should have an adjustment.", adjustmentOut.PK, cycleCount2VarianceInNewFactory.WCC_WD_RelatedAdjustment);

			var logger2 = new DummyWhsDocketCreationLogger();
			var cycleCount2InNewFactory = newFactory.Load<WhsCycleCountLocation>(cycleCount2.PK);
			cycleCount2.AcceptVariances(logger2);

			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Approved, cycleCount2VarianceInNewFactory.WCC_Status);
			AssertEquals("Variance AuthorizedAction", CycleCountVarianceAuthorizedAction.Codes.Empty, cycleCount2VarianceInNewFactory.WCC_AuthorizedAction);

			var adjustments2 = FindAdjustments(data.Org1);
			AssertEquals("Should have 3 adjustments this time, new adjustment added for sibling variance for another pallet id.", 3, adjustments2.Count());
			var newAdjustment = adjustments2.Single(adj => adj.Lines.Any(l => l.WE_WL == location2.PK && l.WE_SerialNumber == "SN2"));
			AssertAdjustmentLine(newAdjustment, data.Part1, location2, 1, "", "", "", "", "SN2", null, null, adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.DamagedStock);
		}

		#endregion

		public void TestAcceptVariances_InventoryWithPalletId_UnexpectedInventoryInOutboundLocation_DockDoor()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var stockLocation = data.Whs1.FindLocation("A-1");

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, stockLocation, "PLT1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Inventory in dockdoor location.", "PLT1", pickLine.InventoryLine.WE_PalletID);
			AssertEquals("Inventory in dockdoor location.", data.Whs1.DefaultOutboundDockDoorLocation.PK, pickLine.InventoryLine.WE_WL);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(stockLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 10, expectedLocation: data.Whs1.DefaultOutboundDockDoorLocation,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create adjustment", 0, adjustments.Count());
			AssertMultilineASCIIEquals("Should log error message",
				@"No variances have been approved for Location 'A-1' in Warehouse '1'.
There are unexpected inventories in a dock door, packing station or consolidation location. Please finalize the associated picking job(s) first before accepting the variance(s).",
				logger.LogMessagesAllAppended);

			var factory2 = new BusinessObjectFactory();
			var varianceInFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance.PK);
			AssertEquals("Variance Authorized", CycleCountVarianceAuthorizedAction.Codes.Error, varianceInFactory2.WCC_AuthorizedAction);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Open, varianceInFactory2.WCC_Status);
			AssertEquals("Variance Adjustment", ZGuid.Empty, varianceInFactory2.WCC_WD_RelatedAdjustment);
		}

		public void TestAcceptVariances_InventoryWithPalletId_UnexpectedInventoryInOutboundLocation_PackingStation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var stockLocation = data.Whs1.FindLocation("A-1");

			var packingStationType = Helper.CreateLocationType("PST", "Packing Station", isPalletIDNeutral: false, 0, LocationClasses.Codes.PST);
			var packingStation = Helper.CreateRowAndGenerateLocations(data.Whs1, "Packing");
			var packingStationLocation = packingStation.Locations.Single();
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, stockLocation, "PLT1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingStationLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Inventory in packing location.", "PLT1", pickLine.InventoryLine.WE_PalletID);
			AssertEquals("Inventory in packing location.", packingStationLocation.PK, pickLine.InventoryLine.WE_WL);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(stockLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 10, expectedLocation: packingStationLocation,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create adjustment", 0, adjustments.Count());
			AssertMultilineASCIIEquals("Should log error message",
				@"No variances have been approved for Location 'A-1' in Warehouse '1'.
There are unexpected inventories in a dock door, packing station or consolidation location. Please finalize the associated picking job(s) first before accepting the variance(s).",
				logger.LogMessagesAllAppended);

			var factory2 = new BusinessObjectFactory();
			var varianceInFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance.PK);
			AssertEquals("Variance Authorized", CycleCountVarianceAuthorizedAction.Codes.Error, varianceInFactory2.WCC_AuthorizedAction);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Open, varianceInFactory2.WCC_Status);
			AssertEquals("Variance Adjustment", ZGuid.Empty, varianceInFactory2.WCC_WD_RelatedAdjustment);
		}

		public void TestAcceptVariances_InventoryWithPalletId_UnexpectedInventoryInOutboundLocation_ConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var stockLocation = data.Whs1.FindLocation("A-1");

			var consolidationLocationType = Helper.CreateLocationType("CON", "Consolidation Location", isPalletIDNeutral: false, 0, LocationClasses.Codes.PST);
			var consolidation = Helper.CreateRowAndGenerateLocations(data.Whs1, "Consolidation");
			var consolidationLocation = consolidation.Locations.Single();
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, stockLocation, "PLT1");
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = consolidationLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Inventory in consolidation location.", "PLT1", pickLine.InventoryLine.WE_PalletID);
			AssertEquals("Inventory in consolidation location.", consolidationLocation.PK, pickLine.InventoryLine.WE_WL);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(stockLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 10, expectedLocation: consolidationLocation,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create adjustment", 0, adjustments.Count());
			AssertMultilineASCIIEquals("Should log error message",
				@"No variances have been approved for Location 'A-1' in Warehouse '1'.
There are unexpected inventories in a dock door, packing station or consolidation location. Please finalize the associated picking job(s) first before accepting the variance(s).",
				logger.LogMessagesAllAppended);

			var factory2 = new BusinessObjectFactory();
			var varianceInFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance.PK);
			AssertEquals("Variance Authorized", CycleCountVarianceAuthorizedAction.Codes.Error, varianceInFactory2.WCC_AuthorizedAction);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Open, varianceInFactory2.WCC_Status);
			AssertEquals("Variance Adjustment", ZGuid.Empty, varianceInFactory2.WCC_WD_RelatedAdjustment);
		}

		public void TestAcceptVariances_InventoryWithSerialNumber_UnexpectedInventoryInOutboundLocation_DockDoor()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var stockLocation = data.Whs1.FindLocation("A-1");

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, stockLocation);
			receiveLine.WE_SerialNumber = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Inventory in dockdoor location.", "SN1", pickLine.InventoryLine.WE_SerialNumber);
			AssertEquals("Inventory in dockdoor location.", data.Whs1.DefaultOutboundDockDoorLocation.PK, pickLine.InventoryLine.WE_WL);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(stockLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: 1, expectedQty: 0, client: data.Org1, part: data.Part1, palletID: "", serialNumber: "SN1",
				expectedLocation: data.Whs1.DefaultOutboundDockDoorLocation,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create adjustment", 0, adjustments.Count());
			AssertMultilineASCIIEquals("Should log error message",
				@"No variances have been approved for Location 'A-1' in Warehouse '1'.
There are unexpected inventories in a dock door, packing station or consolidation location. Please finalize the associated picking job(s) first before accepting the variance(s).",
				logger.LogMessagesAllAppended);

			var factory2 = new BusinessObjectFactory();
			var varianceInFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance.PK);
			AssertEquals("Variance Authorized", CycleCountVarianceAuthorizedAction.Codes.Error, varianceInFactory2.WCC_AuthorizedAction);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Open, varianceInFactory2.WCC_Status);
			AssertEquals("Variance Adjustment", ZGuid.Empty, varianceInFactory2.WCC_WD_RelatedAdjustment);
		}

		public void TestAcceptVariances_InventoryWithSerialNumber_UnexpectedInventoryInOutboundLocation_PackingStation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var stockLocation = data.Whs1.FindLocation("A-1");

			var packingStationType = Helper.CreateLocationType("PST", "Packing Station", isPalletIDNeutral: false, 0, LocationClasses.Codes.PST);
			var packingStation = Helper.CreateRowAndGenerateLocations(data.Whs1, "Packing");
			var packingStationLocation = packingStation.Locations.Single();
			packingStationLocation.WLV_WLT_LocationType = packingStationType.PK;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, stockLocation);
			receiveLine.WE_SerialNumber = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = packingStationLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Inventory in packing location.", "SN1", pickLine.InventoryLine.WE_SerialNumber);
			AssertEquals("Inventory in packing location.", packingStationLocation.PK, pickLine.InventoryLine.WE_WL);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(stockLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "PLT1", client: data.Org1, part: data.Part1, varianceQty: 10, expectedLocation: packingStationLocation,
				serialNumber: "SN1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create adjustment", 0, adjustments.Count());
			AssertMultilineASCIIEquals("Should log error message",
				@"No variances have been approved for Location 'A-1' in Warehouse '1'.
There are unexpected inventories in a dock door, packing station or consolidation location. Please finalize the associated picking job(s) first before accepting the variance(s).",
				logger.LogMessagesAllAppended);

			var factory2 = new BusinessObjectFactory();
			var varianceInFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance.PK);
			AssertEquals("Variance Authorized", CycleCountVarianceAuthorizedAction.Codes.Error, varianceInFactory2.WCC_AuthorizedAction);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Open, varianceInFactory2.WCC_Status);
			AssertEquals("Variance Adjustment", ZGuid.Empty, varianceInFactory2.WCC_WD_RelatedAdjustment);
		}

		public void TestAcceptVariances_InventoryWithSerialNumber_UnexpectedInventoryInOutboundLocation_ConsolidationLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var stockLocation = data.Whs1.FindLocation("A-1");

			var consolidationLocationType = Helper.CreateLocationType("CON", "Consolidation Location", isPalletIDNeutral: false, 0, LocationClasses.Codes.PST);
			var consolidation = Helper.CreateRowAndGenerateLocations(data.Whs1, "Consolidation");
			var consolidationLocation = consolidation.Locations.Single();
			consolidationLocation.WLV_WLT_LocationType = consolidationLocationType.PK;

			Helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Serial, true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, stockLocation);
			receiveLine.WE_SerialNumber = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 1m);
			Factory.Save();

			Helper.CreatePickNew(order);
			var pickLine = orderLine.PickLines.Single();
			var transferLine = Helper.PickAndMakeInTransitTransfer(pickLine, ZDateTimeOffset.Now);
			transferLine.WE_WL = consolidationLocation.PK;
			transferLine.FinaliseDocketLine();
			Factory.Save();

			AssertEquals("Inventory in consolidation location.", "SN1", pickLine.InventoryLine.WE_SerialNumber);
			AssertEquals("Inventory in consolidation location.", consolidationLocation.PK, pickLine.InventoryLine.WE_WL);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(stockLocation,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				client: data.Org1, part: data.Part1, varianceQty: 1, expectedLocation: consolidationLocation, palletID: "",
				serialNumber: "SN1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.Shrinkage);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);
			Factory.Save();

			var adjustments = FindAdjustments(data.Org1);
			AssertEquals("Should not create adjustment", 0, adjustments.Count());
			AssertMultilineASCIIEquals("Should log error message",
				@"No variances have been approved for Location 'A-1' in Warehouse '1'.
There are unexpected inventories in a dock door, packing station or consolidation location. Please finalize the associated picking job(s) first before accepting the variance(s).",
				logger.LogMessagesAllAppended);

			var factory2 = new BusinessObjectFactory();
			var varianceInFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance.PK);
			AssertEquals("Variance Authorized", CycleCountVarianceAuthorizedAction.Codes.Error, varianceInFactory2.WCC_AuthorizedAction);
			AssertEquals("Variance Status", CycleCountVarianceStatus.Codes.Open, varianceInFactory2.WCC_Status);
			AssertEquals("Variance Adjustment", ZGuid.Empty, varianceInFactory2.WCC_WD_RelatedAdjustment);
		}

		public void TestAcceptVariances_FinaliseAdjustmentsFail()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			Helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			Helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, true);

			var expiryDate = ZDate.Today;
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var inventory = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m,
				location1, expiryDate, ZDate.Empty, "", "", "", "");
			receive.FinaliseDocket();
			AssertIsFinalisedPrecondition(receive);
			Factory.Save();

			AssertInventoryStatus(inventory,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "", client: data.Org1, part: data.Part1, varianceQty: -10, partAttrib1: "",
				partAttrib2: "", partAttrib3: "", expiryDate: ZDate.Today, packingDate: null,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.StocktakeAdjustment,
				expectedQty: 10);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				palletID: "", client: data.Org1, part: data.Part1, varianceQty: 10, partAttrib1: "",
				partAttrib2: "", partAttrib3: "", expiryDate: null, packingDate: null,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved,
				adjustmentReasonCode: AdjustmentReasonCodesCodeList.Codes.StocktakeAdjustment,
				expectedQty: 0);

			inventory.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			inventory.ChangeInventoryHeldCode(true);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);

			var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var inventoryInOtherFactory = otherFactory.Load<WhsReceiveLine>(inventory.PK);
			var variance1InOtherFactory = otherFactory.Load<WhsCycleCountLocationVariance>(variance1.PK);
			var variance2InOtherFactory = otherFactory.Load<WhsCycleCountLocationVariance>(variance2.PK);
			CombineAssertions(() =>
			{
				AssertEquals("Error occurs.",
					@"Failed to create adjustment for Client '111', Warehouse '1' by cycle count for Location 'A-1' in Warehouse '1'.
Error - WE_ExpiryDate: Please enter an Expiry date.
", logger.LogMessagesAllAppended);
				AssertEquals(InventoryStatus.Codes.Held, inventoryInOtherFactory.WE_CurrentInventoryStatus);
				AssertEquals(InventoryHoldCodes.Codes.LostInCycleCount, inventoryInOtherFactory.WE_WHC_NKCurrentInventoryHeldCode);
				AssertEquals(CycleCountVarianceAuthorizedAction.Codes.Error, variance1InOtherFactory.WCC_AuthorizedAction);
				AssertEquals(CycleCountVarianceAuthorizedAction.Codes.Error, variance2InOtherFactory.WCC_AuthorizedAction);
			});
		}

		IEnumerable<WhsAdjustment> FindAdjustments(params OrgHeader[] clients)
		{
			var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Adjustment);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, clients.Select(c => c.PK));

			return newFactory.Load<WhsAdjustment>(query);
		}

		void AssertVarianceStatus(WhsCycleCountLocationVariance variance, ZGuid relatedAdjustmentPK, ZString status,
			ZString authorizedAction)
		{
			AssertEquals("RelatedAdjustment", relatedAdjustmentPK, variance.WCC_WD_RelatedAdjustment);
			AssertEquals("Variance Status", status, variance.WCC_Status);
			AssertEquals("Variance AuthorizedAction", authorizedAction, variance.WCC_AuthorizedAction);
		}

		void AssertAdjustment(WhsAdjustment adjustment, OrgHeader expectedClient, WhsWarehouse expectedWarehouse,
			ZGuid parentDocketPK)
		{
			AssertEquals("Adjustment - Client", expectedClient.PK, adjustment.WD_OH_Client);
			AssertEquals("Adjustment - Warehouse", expectedWarehouse.PK, adjustment.WD_WW_Whs);
			AssertEquals("Adjustment - ParentDocket", parentDocketPK, adjustment.WD_WD_ParentDocket);
		}

		void AssertAdjustmentLine(WhsAdjustment adjustment,
			OrgSupplierPart part, WhsLocation location, ZDecimal adjustmentQuantity, string palletID = "",
			string partAttrib1 = "", string partAttrib2 = "", string partAttrib3 = "", string serial = "",
			ZDate? expiryDate = null, ZDate? packingDate = null,
			string originalStatus = InventoryStatus.Codes.Available,
			string originalHeldCode = "",
			string adjustmentReasonCode = AdjustmentReasonCodesCodeList.Codes.StocktakeAdjustment,
			string currentAdjustmentStatus = InventoryStatus.Codes.Available,
			string currentAdjustmentHeldCode = "")
		{
			AssertNotNull("Should find matched Adjustment Line", adjustment.Lines.SingleOrDefault(l =>
				l.WE_OP == part.PK
				&& l.WE_F3_NKPackType == part.OP_StockKeepingUnit
				&& l.WE_WL == location.PK
				&& l.WE_PalletID == palletID
				&& l.WE_PartAttrib1 == partAttrib1
				&& l.WE_PartAttrib2 == partAttrib2
				&& l.WE_PartAttrib3 == partAttrib3
				&& l.WE_SerialNumber == serial
				&& l.WE_ExpiryDate == (expiryDate ?? ZDate.Empty)
				&& l.WE_PackingDate == (packingDate ?? ZDate.Empty)
				&& l.WE_TransactionQuantity == adjustmentQuantity
				&& l.WE_OriginalInventoryStatus == originalStatus
				&& l.WE_WHC_NKOriginalInventoryHeldCode == originalHeldCode
				&& l.WE_ReasonCode == adjustmentReasonCode
				&& l.WE_CurrentInventoryStatus == currentAdjustmentStatus
				&& l.WE_WHC_NKOriginalInventoryHeldCode == currentAdjustmentHeldCode
				&& l.IsFinalised));
		}

		void AssertInventoryStatus(WhsDocketLine docketLine, string originalInventoryStatus, string originalInventoryHeldCode, string currentInventoryStatus, string currentInventoryHeldCode)
		{
			AssertEquals("Original Held Code", originalInventoryHeldCode, docketLine.WE_WHC_NKOriginalInventoryHeldCode);
			AssertEquals("Current Held Code", currentInventoryHeldCode, docketLine.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Original Status", originalInventoryStatus, docketLine.WE_OriginalInventoryStatus);
			AssertEquals("Current Status", currentInventoryStatus, docketLine.WE_CurrentInventoryStatus);
		}

		void AssertAdjustmentLineStatus(WhsAdjustment adjustment, string originalStatus, string originalHeldCode, string currentAdjustmentStatus, string currentAdjustmentHeldCode)
		{
			var adjustmentLine = adjustment.Lines.Single();
			AssertInventoryStatus(adjustmentLine,
				originalInventoryHeldCode: originalHeldCode,
				originalInventoryStatus: originalStatus,
				currentInventoryHeldCode: currentAdjustmentHeldCode,
				currentInventoryStatus: currentAdjustmentStatus);
		}

		class DummyWhsDocketCreationLogger : IWhsDocketCreationLogger
		{
			public IEnumerable<string> AllIndividualLogMessages => Logs.AllIndividualLogMessages;

			public string LogMessagesAllAppended
			{
				get
				{
					var builder = new ZStringBuilder();
					foreach (var log in Logs.AllIndividualLogMessages)
					{
						builder.AppendLine(log);
					}

					return builder.ToString();
				}
			}

			LogsWrapper Logs => logs ?? (logs = new LogsWrapper());
			LogsWrapper logs;

			class LogsWrapper
			{
				public void AppendLine(string message)
				{
					Logs.Add(message);
				}

				List<string> Logs { get; } = new List<string>();

				public IEnumerable<string> AllIndividualLogMessages => Logs;
			}

			public void LogFailure(string message)
			{
				// find the validation error messages and sort them in a consistent order.
				var escapedSequence = "^" + Regex.Escape("Error - W") + ".{1,2}_.+:" + Regex.Escape(" ");
				var regex = new Regex(escapedSequence, RegexOptions.Multiline);

				var preValidationErrorsMessage = "";
				var validationErrorMessages = new List<string>();
				int previousMatchIndex = -1;

				while (previousMatchIndex < message.Length)
				{
					var currentMatch = regex.Match(message, previousMatchIndex + 1);
					if (currentMatch.Success)
					{
						if (previousMatchIndex > -1)
						{
							// Another match found, grab everything between the previous match and the current match.
							// Remove trailing newline so we can join validation errors again with newlines in between
							validationErrorMessages.Add(message
								.Substring(previousMatchIndex, currentMatch.Index - previousMatchIndex).Trim());
						}
						else
						{
							// this is the first match, grab anything before the current match as the preValidationMessage.
							preValidationErrorsMessage = message.Substring(0, currentMatch.Index);
						}

						previousMatchIndex = currentMatch.Index;
					}
					else
					{
						if (previousMatchIndex > -1)
						{
							// this was the last Validation Error Message, grab the rest of the string
							validationErrorMessages.Add(message.Substring(previousMatchIndex));
						}
						else
						{
							// no Validation Error Messages
							preValidationErrorsMessage = message;
						}

						break;
					}
				}

				Logs.AppendLine(preValidationErrorsMessage +
								string.Join("\n", validationErrorMessages.OrderBy(m => m)));
			}

			public void LogHyperLinkSuccess(WhsDocket docket, string message)
			{
				Logs.AppendLine($"{docket.WD_ExternalReference}: {message}");
			}

			public void LogSuccess(string message)
			{
				Logs.AppendLine(message);
			}

			public void LogWarning(string message)
			{
				Logs.AppendLine(message);
			}
		}

		#endregion

		#region TestRejectVariance

		public void TestRejectVarianceBelongsToPIDCycleCountLocation()
		{
			TestRejectVarianceCore(CycleCountGranularity.Codes.PalletIDOnly, CycleCountGranularity.Codes.PalletIDOnly);
		}

		public void TestRejectVarianceBelongsToPRDCycleCountLocation()
		{
			TestRejectVarianceCore(CycleCountGranularity.Codes.ProductOnly,
				CycleCountGranularity.Codes.ProductWithAttributes);
		}

		public void TestRejectVarianceBelongsToPWPCycleCountLocation()
		{
			TestRejectVarianceCore(CycleCountGranularity.Codes.ProductWithPalletID,
				CycleCountGranularity.Codes.ProductWithPalletID);
		}

		public void TestRejectVarianceBelongsToPLTCycleCountLocation()
		{
			TestRejectVarianceCore(CycleCountGranularity.Codes.PalletCount, CycleCountGranularity.Codes.PalletIDOnly);
		}

		public void TestRejectVarianceBelongsToPWACycleCountLocation()
		{
			TestRejectVarianceCore(CycleCountGranularity.Codes.ProductWithAttributes,
				CycleCountGranularity.Codes.ProductWithAttributes);
		}

		public void TestRejectVarianceBelongsToPWSCycleCountLocation()
		{
			TestRejectVarianceCore(CycleCountGranularity.Codes.ProductWithAllAttributes,
				CycleCountGranularity.Codes.ProductWithAllAttributes);
		}

		void TestRejectVarianceCore(string originalGranularity, string newGranularity)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1, originalGranularity,
				ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now, "~E");
			var openVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount,
				CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2);
			var markedAsRejectedVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount,
				CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			openVariance.Reload();
			AssertVarianceStatus(openVariance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			markedAsRejectedVariance.Reload();
			AssertVarianceStatus(markedAsRejectedVariance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location1, newGranularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					$"Re-Count was created with Granularity '{newGranularity}' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		public void TestRejectVariances_VarianceIsNegative_HeldInventory_UserDefinedHeldCode()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");

			var heldCode = Helper.CreateInventoryHeldCode("SA", "SA");
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location, "PLT1", InventoryStatus.Codes.Held, heldCode.WHC_Code);

			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Held,
				originalInventoryHeldCode: heldCode.WHC_Code,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: heldCode.WHC_Code);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Held,
				originalInventoryHeldCode: heldCode.WHC_Code,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.LostInCycleCount);

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Held,
				originalInventoryHeldCode: heldCode.WHC_Code,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: heldCode.WHC_Code);
			AssertRecountWasCreated(cycleCount, location, CycleCountGranularity.Codes.ProductWithAttributes, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					$"Re-Count was created with Granularity '{CycleCountGranularity.Codes.ProductWithAttributes}' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		public void TestRejectVariances_VarianceIsNegative_AllShortPicked_HeldInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Available,
				currentInventoryHeldCode: string.Empty);

			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.ShortPicked;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.ShortPicked);

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10m, expectedQty: 10m, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.LostInCycleCount);

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertInventoryStatus(receiveLine,
				originalInventoryStatus: InventoryStatus.Codes.Available,
				originalInventoryHeldCode: string.Empty,
				currentInventoryStatus: InventoryStatus.Codes.Held,
				currentInventoryHeldCode: InventoryHoldCodes.Codes.ShortPicked);
			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected, CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location1, CycleCountGranularity.Codes.ProductWithAttributes, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					$"Re-Count was created with Granularity '{CycleCountGranularity.Codes.ProductWithAttributes}' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		#region TestRejectVariances_RevertInventoryStatus

		public void TestRejectVariances_RevertInventoryStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			AssertEquals("Precondition: ReceiveLine has been held", InventoryStatus.Codes.Held,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: LCC has been set", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine has been reverted", InventoryStatus.Codes.Available,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been reverted", "", receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location1, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		public void TestRejectVariances_RevertInventoryStatus_LCCInventoryInOtherLocation()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, location1, "");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, location2, "");
			receive1.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			receiveLine2.HeldCodeChangeQuantity = 10m;
			receiveLine2.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine2.ChangeInventoryHeldCode(true);
			Factory.Save();

			AssertEquals("Precondition: ReceiveLine has been held", InventoryStatus.Codes.Held,
				receiveLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: LCC has been set", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Precondition: ReceiveLine has been held", InventoryStatus.Codes.Held,
				receiveLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: LCC has been set", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine1 status should be reverted", InventoryStatus.Codes.Available,
				receiveLine1.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine1 HeldCode should be changed", "", receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("ReceiveLine2 status should not be reverted", InventoryStatus.Codes.Held,
				receiveLine2.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine2 HeldCode should not be changed", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location1, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		public void TestRejectVariances_RevertInventoryStatus_LCCInventoryCommittedPartially()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			AssertEquals("Precondition: ReceiveLine has been held", InventoryStatus.Codes.Held,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: LCC has been set", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			var adjustmentLine = Helper.CreateWhsAdjustmentLine(adjustment, data.Part1.PK, -6m,
				location1.ToLocationString(), "PLT1", receiveLine.WE_AdjustmentArrivalDate,
				InventoryHoldCodes.Codes.LostInCycleCount, "", "", "", "", ZDate.Empty, ZDate.Empty);
			adjustment.RunPreSaveValidation(); // Commit held inventory
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			var docketLines = Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.PK,
				SQLComparisonOperator.NotEqual, adjustmentLine.PK));
			AssertEquals("Should find 2 docket line", 2, docketLines.Length);
			var docketLine1 = docketLines.Single(l => l.PK == receiveLine.PK);
			AssertEquals("ReceiveLine has been reverted", InventoryStatus.Codes.Held,
				docketLine1.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been reverted", InventoryHoldCodes.Codes.LostInCycleCount,
				docketLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("ReceiveLine - StockOnHand", 6m, docketLine1.WE_StockOnHand);
			var docketLine2 = docketLines.Single(l => l.PK != receiveLine.PK);
			AssertEquals("The rest inventory should be reverted", InventoryStatus.Codes.Available,
				docketLine2.WE_CurrentInventoryStatus);
			AssertEquals("The rest inventory should be reverted", "", docketLine2.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("The rest inventory should be reverted", 4m, docketLine2.WE_StockOnHand);

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location1, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		public void TestRejectVariances_RevertInventoryStatus_InventoryNotMatched()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive1, data.Part1, 10m, location1, "PLT1");
			receive1.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			AssertEquals("Precondition: ReceiveLine has been held", InventoryStatus.Codes.Held,
				receiveLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: LCC has been set", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);

			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive2, data.Part2, 10m, location1, "PLT2");
			receiveLine2.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine2.HeldCodeChangeQuantity = 10m;
			receiveLine2.ChangeInventoryHeldCode(true);
			receive2.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine1 status should be reverted", InventoryStatus.Codes.Available,
				receiveLine1.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine1 HeldCode should be changed", "", receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("ReceiveLine2 status should be reverted", InventoryStatus.Codes.Available,
				receiveLine2.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine2 HeldCode should be changed", "", receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location1, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		public void TestRejectVariances_RevertInventoryStatus_NonLostInCycleCount()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1",
				InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine status should not be reverted", InventoryStatus.Codes.Held,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine HeldCode should not be changed", InventoryHoldCodes.Codes.Damaged,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location1, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		public void TestRejectVariances_RevertInventoryStatus_OriginalStatusIsNotAvailable()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1",
				InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine has been reverted", InventoryStatus.Codes.Held,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been reverted", InventoryHoldCodes.Codes.Damaged,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location1, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
						"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		public void TestRejectVariances_RevertInventoryStatus_OriginalStatusIsInvalid()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1",
				InventoryHoldCodes.Codes.LostInCycleCount);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = "ABC";
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine has been defaulted to Held", InventoryStatus.Codes.Held,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been reverted to invalid code", "ABC",
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location1, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
						"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		public void TestRejectVariances_RevertInventoryStatus_OriginalStatusChanged()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1",
				InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine has been defaulted to Held", InventoryStatus.Codes.Held,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been defaulted to Held", InventoryHoldCodes.Codes.Damaged,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			var newCycleCount = Factory.Load<WhsCycleCountLocation>(new ZQuery(WhsCycleCountLocationSchema.WCL_WCL_RejectedCycleCount, cycleCount.PK)).Single();
			newCycleCount.WCL_StartTime = now.AddHours(2);
			newCycleCount.WCL_EndTime = now.AddHours(3);
			newCycleCount.WCL_GS_NKAssignedTo = "TTT";

			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = string.Empty;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var newVariance = Helper.CreateWhsCycleCountLocationVariance(newCycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);

			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			newCycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine has been reverted to Available", InventoryStatus.Codes.Available,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been reverted to Available", string.Empty,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);
		}

		public void TestRejectVariances_RevertInventoryStatus_OriginalStatusIsNotAvailable_PartialVariance()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1",
				InventoryStatus.Codes.Held, InventoryHoldCodes.Codes.Damaged);
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -5, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 5m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);

			var clonedReceiveLine = Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, receiveLine.PK)).Single();
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine has been reverted", InventoryStatus.Codes.Held,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been reverted", InventoryHoldCodes.Codes.Damaged,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			var newLine =
				Factory.LoadTop1<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, receiveLine.PK));
			AssertEquals("New ReceiveLine has been reverted", InventoryStatus.Codes.Held,
				newLine.WE_CurrentInventoryStatus);
			AssertEquals("New ReceiveLine has been reverted", InventoryHoldCodes.Codes.Damaged,
				newLine.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location1, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
						"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		public void TestRejectVariances_RevertInventoryStatus_MultipleHoldCodeChangeLogs()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Damaged;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine has been reverted", InventoryStatus.Codes.Held,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been reverted", InventoryHoldCodes.Codes.Damaged,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location1, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
						"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		public void TestRejectVariances_RevertInventoryStatus_MultipleHoldCodeChangeLogs_FinalIsAvailable()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.Held;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = string.Empty;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1",
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			receiveLine.HeldCodeChangeQuantity = 10m;
			receiveLine.HeldCodeToChangeTo = InventoryHoldCodes.Codes.LostInCycleCount;
			receiveLine.ChangeInventoryHeldCode(true);
			Factory.Save();

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine has been reverted", InventoryStatus.Codes.Available,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been reverted", string.Empty,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location1, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
						"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-1' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		public void TestRejectVariances_RevertInventoryStatus_OnExpectedLocation_WithPalletID()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: 10, expectedQty: 0, client: data.Org1, part: data.Part1, palletID: "PLT1",
				expectedLocation: location1, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			AssertEquals("Precondition: ReceiveLine has been held", InventoryStatus.Codes.Held,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: LCC has been set", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine has been reverted", InventoryStatus.Codes.Available,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been reverted", "", receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location2, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-2' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		#region TestRejectVariances_RevertInventoryStatus_OnExpectedLocation_WithSerialNumber

		public void TestRejectVariances_RevertInventoryStatus_OnExpectedLocation_WithSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "", ZDate.Today,
				ZDate.Today, "S1", "S2", "S3", "");
			receiveLine.WE_SerialNumber = "SN1";
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: 1, expectedQty: 0, client: data.Org1, part: data.Part1, palletID: "", partAttrib1: "S1",
				partAttrib2: "S2", partAttrib3: "S3", serialNumber: "SN1", expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			AssertEquals("Precondition: ReceiveLine has been held", InventoryStatus.Codes.Held,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: LCC has been set", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine has been reverted", InventoryStatus.Codes.Available,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been reverted", "", receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location2, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-2' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		#endregion

		#region TestRejectVariances_RevertInventoryStatus_OnExpectedLocation_WithPalletIDAndSerialNumber

		public void TestRejectVariances_RevertInventoryStatus_OnExpectedLocation_WithPalletIDAndSerialNumber()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "", ZDate.Today,
				ZDate.Today, "S1", "S2", "S3", "");
			receiveLine1.WE_SerialNumber = "SN1";
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part2, 10m, location1, "PLT2");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: 1, expectedQty: 0, client: data.Org1, part: data.Part1, palletID: "", partAttrib1: "S1",
				partAttrib2: "S2", partAttrib3: "S3", serialNumber: "SN1", expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				varianceQty: 10, expectedQty: 0, client: data.Org1, part: data.Part2, palletID: "PLT2",
				partAttrib1: "S1", partAttrib2: "S2", partAttrib3: "S3", expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount.PK });

			AssertEquals("Precondition: ReceiveLine has been held", InventoryStatus.Codes.Held,
				receiveLine1.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: LCC has been set", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("Precondition: ReceiveLine has been held", InventoryStatus.Codes.Held,
				receiveLine2.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: LCC has been set", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			AssertEquals("ReceiveLine has been reverted", InventoryStatus.Codes.Available,
				receiveLine1.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been reverted", "", receiveLine1.WE_WHC_NKCurrentInventoryHeldCode);
			AssertEquals("ReceiveLine has been reverted", InventoryStatus.Codes.Available,
				receiveLine2.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine has been reverted", "", receiveLine2.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance1, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertVarianceStatus(variance2, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount, location2, cycleCount.WCL_Granularity, cycleCount.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-2' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		#endregion

		public void TestRejectVariances_RevertInventoryStatus_OnExpectedLocation_WithPalletID_HasOpenVariance()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 10m, location1, "PLT1");
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open,
				varianceQty: 10, expectedQty: 0, client: data.Org1, part: data.Part1, palletID: "PLT1",
				expectedLocation: location1, authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -10, expectedQty: 10, client: data.Org1, part: data.Part1, palletID: "PLT1");
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount1.PK });

			AssertEquals("Precondition: ReceiveLine has been held", InventoryStatus.Codes.Held,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: LCC has been set", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount1.RejectVariances(logger);

			AssertEquals("ReceiveLine Status should not be reverted when expected location has open variance",
				InventoryStatus.Codes.Held, receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine HeldCode should not be reverted when expected location has open variance",
				InventoryHoldCodes.Codes.LostInCycleCount, receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance1, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount1, location2, cycleCount1.WCL_Granularity, cycleCount1.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-2' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		#region TestRejectVariances_RevertInventoryStatus_OnExpectedLocation_WithSerialNumber_HasOpenVariance

		public void TestRejectVariances_RevertInventoryStatus_OnExpectedLocation_WithSerialNumber_HasOpenVariance()
		{
			var serial = "SN1";
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");

			Helper.SetClientAllAttributeType(data.Org1, true);
			Helper.SetProductAllAttributeUse(data.Org1, data.Part1, true, useSerialNumber: true);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var receiveLine = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "PLT1", ZDate.Today,
				ZDate.Today, "S1", "S2", "S3", "");
			receiveLine.WE_SerialNumber = serial;
			receive.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();

			var now = DateTimeOffset.Now;
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance1 = Helper.CreateWhsCycleCountLocationVariance(cycleCount1, CycleCountVarianceStatus.Codes.Open,
				varianceQty: 1, expectedQty: 0, client: data.Org1, part: data.Part1, palletID: "PLT1",
				partAttrib1: "S1", partAttrib2: "S2", partAttrib3: "S3", serialNumber: serial, expiryDate: ZDate.Today,
				packingDate: ZDate.Today, expectedLocation: location1,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			var variance2 = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open,
				varianceQty: -1, expectedQty: 1, client: data.Org1, part: data.Part1, palletID: "PLT1",
				partAttrib1: "S1", partAttrib2: "S2", partAttrib3: "S3", expiryDate: ZDate.Today,
				packingDate: ZDate.Today);
			Factory.Save();

			CycleCountService.MarkInventoriesLostInCycleCount(new[] { cycleCount1.PK });

			AssertEquals("Precondition: ReceiveLine has been held", InventoryStatus.Codes.Held,
				receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("Precondition: LCC has been set", InventoryHoldCodes.Codes.LostInCycleCount,
				receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount1.RejectVariances(logger);

			AssertEquals("ReceiveLine Status should not be reverted when expected location has open variance",
				InventoryStatus.Codes.Held, receiveLine.WE_CurrentInventoryStatus);
			AssertEquals("ReceiveLine HeldCode should not be reverted when expected location has open variance",
				InventoryHoldCodes.Codes.LostInCycleCount, receiveLine.WE_WHC_NKCurrentInventoryHeldCode);

			AssertVarianceStatus(variance1, ZGuid.Empty, CycleCountVarianceStatus.Codes.Rejected,
				CycleCountVarianceAuthorizedAction.Codes.Empty);
			AssertRecountWasCreated(cycleCount1, location2, cycleCount1.WCL_Granularity, cycleCount1.WCL_Priority);
			AssertContainsExactElementsInAnyOrder("Should have creation log of cycle count",
				new[]
				{
					"Re-Count was created with Granularity 'PWA' and Priority '0' for Location 'A-2' in Warehouse '1'."
				}, logger.AllIndividualLogMessages);
		}

		#endregion

		#endregion

		void AssertRecountWasCreated(WhsCycleCountLocation parentCycleCount, WhsLocation expectedLocation,
			ZString expectedGranularity, ZByte expectedPriority)
		{
			var query = new ZQuery(WhsCycleCountLocationSchema.WCL_WCL_RejectedCycleCount, parentCycleCount.PK);
			var newCycleCount = Factory.Load<WhsCycleCountLocation>(query).SingleOrDefault();
			AssertNotNull("Must create a new cycle count", newCycleCount);
			AssertEquals("Cycle Count - Granularity", expectedGranularity, newCycleCount.WCL_Granularity);
			AssertEquals("Cycle Count - Location", expectedLocation.PK, newCycleCount.WCL_WL_Location);
			AssertEquals("Cycle Count - Priority", expectedPriority, newCycleCount.WCL_Priority);
		}

		#endregion

		#region TestWhenRelatedAdjustmentPKIsSet

		public void TestWhenRelatedAdjusmentPKIsSet_NegativeVarianceWithPalletID_CanBeSaved()
		{
			TestWhenRelatedAdjustmentPKIsSet((variance) =>
			{
				variance.WCC_PalletID = "PLT1";
			}, canBeSaved: true);
		}

		public void TestWhenRelatedAdjusmentPKIsSet_NegativeVarianceWithSerialNumber_CanBeSaved()
		{
			TestWhenRelatedAdjustmentPKIsSet((variance) =>
			{
				variance.WCC_SerialNumber = "SN1";
			}, canBeSaved: true);
		}

		public void TestWhenRelatedAdjusmentPKIsSet_NegativeVarianceWithPalletID_CannotBeSaved_IfAuthorizeActionIsError()
		{
			TestWhenRelatedAdjustmentPKIsSet((variance) =>
			{
				variance.WCC_PalletID = "PLT1";
				variance.WCC_AuthorizedAction = CycleCountVarianceAuthorizedAction.Codes.Error;
			}, canBeSaved: false);
		}

		public void TestWhenRelatedAdjusmentPKIsSet_NegativeVarianceWithSerialNumber_CannotBeSaved_IfAuthorizeActionIsError()
		{
			TestWhenRelatedAdjustmentPKIsSet((variance) =>
			{
				variance.WCC_SerialNumber = "SN1";
				variance.WCC_AuthorizedAction = CycleCountVarianceAuthorizedAction.Codes.Error;
			}, canBeSaved: false);
		}

		public void TestWhenRelatedAdjusmentPKIsSet_NegativeVarianceWithEmptyPalletIDAndEmptySerialNumber_CannotBeSaved()
		{
			TestWhenRelatedAdjustmentPKIsSet((variance) =>
			{
				variance.WCC_PalletID = "";
				variance.WCC_SerialNumber = "";
			}, canBeSaved: false);
		}

		void TestWhenRelatedAdjustmentPKIsSet(Action<WhsCycleCountLocationVariance> setDataForTriggerToPass, bool canBeSaved)
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");

			var now = DateTimeOffset.Now;
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes, now, now.AddHours(1), "TTT");
			Factory.Save();

			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open, palletID: "", client: data.Org1, part: data.Part1, varianceQty: -1);
			var adjustment = Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			variance.WCC_WD_RelatedAdjustment = adjustment.PK;

			setDataForTriggerToPass(variance);

			if (canBeSaved)
			{
				AssertNoExceptionThrown("Should have no exception", () => Factory.Save());
			}
			else
			{
				AssertExceptionThrown<ZSaveException>("Should throw an exception", () => Factory.Save());
			}
		}

		#endregion

		#region TestITaskPlanningJob

		public void TestITaskPlanningJob_BasicProperties()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes);

			var job = (ITaskPlanningJob)cycleCount;
			CombineAssertions(() =>
			{
				AssertEquals(cycleCount.PK, job.PK);
				AssertEquals(data.Whs1.PK, job.WarehousePK);
				AssertEquals(Factory, job.Factory);
				AssertEquals("A-1", job.JobID);
				AssertEquals("Cycle Count", job.HumanReadableNameWithoutID);
			});
		}

		public void TestITaskPlanningJob_TaskPlanningStatus()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes);

			var job = (ITaskPlanningJob)cycleCount;
			AssertEquals(string.Empty, job.TaskPlanningStatus);

			job.TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals("RFP", job.TaskPlanningStatus);
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes);
			Factory.Save();

			var job = (ITaskPlanningJob)cycleCount;
			CombineAssertions(() =>
			{
				AssertEquals(true, job.IsInDatabase);
				AssertEquals(false, job.HasChanges);
				AssertEquals(false, job.IsFinalisedOrCancelled);
				AssertEquals(string.Empty, job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
				AssertEquals(string.Empty, job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
			});
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason_NotInDatabase()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes);

			var job = (ITaskPlanningJob)cycleCount;
			AssertEquals(false, job.IsInDatabase);
			AssertEquals("Cannot change Task Planning Status as the Cycle Count is not saved.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals("Cannot change Task Planning Status as the Cycle Count is not saved.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason_ValueChange()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes);
			Factory.Save();

			var job = (ITaskPlanningJob)cycleCount;
			AssertEquals(string.Empty, job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals(string.Empty, job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));

			job.TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
			AssertEquals(true, job.HasChanges);
			AssertEquals("Cannot change Task Planning Status as the Cycle Count is not saved.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals("Cannot change Task Planning Status as the Cycle Count is not saved.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestITaskPlanningJob_GetCannotUpdateTaskPlanningStatusReason_Finalised()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location = data.Whs1.FindLocation("A-1");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes);
			cycleCount.WCL_StartTime = ZDateTimeOffset.Now;
			cycleCount.WCL_EndTime = ZDateTimeOffset.Now;
			cycleCount.WCL_GS_NKAssignedTo = "AAA";
			Factory.Save();

			var job = (ITaskPlanningJob)cycleCount;
			AssertEquals(true, job.IsFinalisedOrCancelled);
			AssertEquals("Cannot change Task Planning Status as the Cycle Count is finalized or canceled.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: true));
			AssertEquals("Cannot change Task Planning Status as the Cycle Count is finalized or canceled.", job.GetCannotUpdateTaskPlanningStatusReason(changeStatusToReady: false));
		}

		public void TestITaskPlanningJob_ClearFKForProcessTasks()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");
			var location = data.Whs1.FindLocation("A-1");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes);
			var wave = Factory.New<WhsCycleCountWave>();
			Factory.Save();

			var processTask = Helper.CreateProcessTaskForCycleCountWave(wave, staff);
			cycleCount.WCL_P9_Task = processTask.PK;

			var job = (ITaskPlanningJob)cycleCount;

			AssertEquals("Precondition", processTask.PK, cycleCount.WCL_P9_Task);
			job.ClearFKForProcessTasks(new HashSet<ZGuid> { processTask.PK });
			AssertEquals("The FK should be cleared.", ZGuid.Empty, cycleCount.WCL_P9_Task);
		}

		public void TestITaskPlanningJob_GetRelatedProcessTasks()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("US1", "User");
			var location = data.Whs1.FindLocation("A-1");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location, CycleCountGranularity.Codes.ProductWithAttributes);
			var wave = Factory.New<WhsCycleCountWave>();
			Factory.Save();

			var job = (ITaskPlanningJobWithExternalTasks)cycleCount;

			AssertEquals("Precondition", 0, job.GetRelatedProcessTasks().Length);

			var processTask = Helper.CreateProcessTaskForCycleCountWave(wave, staff);
			cycleCount.WCL_P9_Task = processTask.PK;

			AssertEquals("Get the correct process Task", processTask.PK, job.GetRelatedProcessTasks().Single().PK);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var data = new TestDataSimpleEnvironment(factory, 2, 1);
			return Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.PalletCount);
		}

		WhsCycleCountLocationService CycleCountService =>
			(cycleCountService ?? (cycleCountService = new WhsCycleCountLocationService()));

		WhsCycleCountLocationService cycleCountService;

		#endregion
	}

	#region CycleCountVarianceProcessingManagerNonTransactionalTest

	class CycleCountVarianceProcessingManagerNonTransactionalTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestAcceptVariances_SavingErrorVariancesThrowException()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAllAttributes, ZDateTimeOffset.Now.AddDays(-1),
				ZDateTimeOffset.Now, "~E");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount, CycleCountVarianceStatus.Codes.Open,
				data.Org1, data.Part1, 1, expectedQty: 0,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Approved);
			Factory.Save();

			string triggerSQL = $@"
CREATE TRIGGER TestRejectVariance on {WhsCycleCountLocationVarianceSchema.Constants.TableName} AFTER UPDATE
AS
BEGIN
	RAISERROR('Error in the trigger', 16, 1)
END";
			Db.Connection.ExecuteNonQuery(triggerSQL);

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.AcceptVariances(logger);

			var factory2 = new BusinessObjectFactory();
			var varianceInFactory2 = factory2.Load<WhsCycleCountLocationVariance>(variance.PK);
			AssertEquals(CycleCountVarianceStatus.Codes.Open, varianceInFactory2.WCC_Status);
			AssertEquals(CycleCountVarianceAuthorizedAction.Codes.Approved, varianceInFactory2.WCC_AuthorizedAction);
			AssertEquals("Should have creation log of cycle count",
				@"An error occurred while saving approved cycle count variances with adjustments: Error in the trigger
An error occurred while marking and saving cycle count variances as Error: Error in the trigger",
				logger.LogMessage.Trim());
		}

		[UseSnapshotProtection]
		public void TestRejectVariances_SavingErrorVariancesThrowException()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1,
				CycleCountGranularity.Codes.ProductWithAllAttributes, ZDateTimeOffset.Now.AddDays(-1),
				ZDateTimeOffset.Now, "~E");
			var openVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount,
				CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2);
			var markedAsRejectedVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount,
				CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, expectedQty: 0,
				authorizedAction: CycleCountVarianceAuthorizedAction.Codes.Rejected);
			Factory.Save();

			string triggerSQL = $@"
CREATE TRIGGER TestRejectVariance on {WhsCycleCountLocationVarianceSchema.Constants.TableName} AFTER UPDATE
AS
BEGIN
	RAISERROR('Error in the trigger', 16, 1)
END";
			Db.Connection.ExecuteNonQuery(triggerSQL);

			var logger = new DummyWhsDocketCreationLogger();
			cycleCount.RejectVariances(logger);

			openVariance.Reload();
			markedAsRejectedVariance.Reload();
			AssertEquals(CycleCountVarianceStatus.Codes.Open, openVariance.WCC_Status);
			AssertEquals(CycleCountVarianceStatus.Codes.Open, markedAsRejectedVariance.WCC_Status);
			AssertEquals("", openVariance.WCC_AuthorizedAction);
			AssertEquals(CycleCountVarianceAuthorizedAction.Codes.Rejected,
				markedAsRejectedVariance.WCC_AuthorizedAction);

			AssertEquals("Should have creation log of cycle count",
				@"An error occurred while saving rejected cycle count variances: Error in the trigger
An error occurred while marking and saving cycle count variances as Error: Error in the trigger",
				logger.LogMessage.Trim());
		}

		#region Implementation

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}

		BusinessObjectFactory factory;

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		class DummyWhsDocketCreationLogger : IWhsDocketCreationLogger
		{
			public ZString LogMessage => logs.ToString();

			ZStringBuilder Logs => logs ?? (logs = new ZStringBuilder());
			ZStringBuilder logs;

			public void LogFailure(string message)
			{
				Logs.AppendLine(message);
			}

			public void LogHyperLinkSuccess(WhsDocket docket, string message)
			{
				Logs.AppendLine($"{docket.WD_ExternalReference}: {message}");
			}

			public void LogSuccess(string message)
			{
				Logs.AppendLine(message);
			}

			public void LogWarning(string message)
			{
				Logs.AppendLine(message);
			}
		}

		#endregion
	}

	#endregion

	#region Triggers_WhsCycleCountLocationVarianceTest class

	public class Triggers_WhsCycleCountLocationTest : WhsTestCaseWithFactory
	{
		#region TestTG_WhsCycleCountLocation_CannotChangeKeyFields_Update

		[ExpectNoExceptions]
		public void TestTG_WhsCycleCountLocation_CannotChangeKeyFields_Update()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var cycleCount = Helper.CreateWhsCycleCountLocation(location1, "PLT");
			Factory.Save();

			cycleCount.WCL_WL_Location = location2.PK;

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsCycleCountLocation.PreventModifyOfLocationTriggerError, true), "Trigger should prevent modify of Location field.");
		}

		#endregion

		#region TestTG_WhsCycleCountLocation_PreventStartWhenVarianceExpectLocationExisted_Update

		[ExpectNoExceptions]
		public void TestTG_WhsCycleCountLocation_PreventStartWhenVarianceExpectLocationExisted_Update()
		{
			var today = ZDateTimeOffset.Now;
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var location1 = data.Whs1.FindLocation("A-1");
			var location2 = data.Whs1.FindLocation("A-2");
			var cycleCount =
				Helper.CreateWhsCycleCountLocation(location1, CycleCountGranularity.Codes.ProductWithPalletID);

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(location2,
				CycleCountGranularity.Codes.ProductWithPalletID, today, today.AddMinutes(2), "AAA");
			var variance = Helper.CreateWhsCycleCountLocationVariance(cycleCount2, CycleCountVarianceStatus.Codes.Open,
				data.Org1, data.Part1, 10m, 1m);
			variance.WCC_PalletID = "PLT1";
			variance.WCC_WL_ExpectedStockLocation = location1.PK;

			Factory.Save();

			cycleCount.WCL_StartTime = today;
			cycleCount.WCL_GS_NKAssignedTo = "BBB";
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsCycleCountLocation.PreventStartCycleCountHasOpenExpectStockVarianceTriggerError, true), "Trigger should prevent modify of AssignedTo field.");
		}

		#endregion
	}

	#endregion

	#region Triggers_PreventCreatingCycleCountLocationsWithOpenVariance

	[TestedType(typeof(WhsCycleCountLocation))]
	public class
		Triggers_PreventCreatingCycleCountLocationsWithOpenVariance : DeferrableTriggerTestCase<WhsCycleCountLocation>
	{
		#region TestTG_PreventCreatingCycleCountLocationsWithOpenVariance

		[ExpectNoExceptions]
		public void TestTG_PreventCreatingCycleCountLocationsWithOpenVariance_Insert()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var cycleCount1 = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithPalletID, ZDateTimeOffset.Now.AddDays(-1), ZDateTimeOffset.Now,
				"~BS");
			var openVariance = Helper.CreateWhsCycleCountLocationVariance(cycleCount1,
				CycleCountVarianceStatus.Codes.Open, data.Org1, data.Part1, 1, 2);
			Factory.Save();

			var cycleCount2 = Helper.CreateWhsCycleCountLocation(data.Whs1.DefaultLocation,
				CycleCountGranularity.Codes.ProductWithPalletID);
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), WhsCycleCountLocation.PreventCreatingCycleCountLocationsWithOpenVarianceTriggerError, true), "Trigger should prevent inserting cycle count location for same location with open variance.");
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}

	#endregion
}
