using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DeferTriggerAndRunBeforeCommit("TG_WhsCycleCountLocation_PreventCreateIfLocationHasOpenVariance", WhsValidationHelper.WhsCheckIfLocationHasOpenVariance, WhsCycleCountLocationSchema.Constants.PK, typeof(IWhsCycleCountLocationPreventCreateIfLocationHasOpenVarianceDeferTriggerStrategy))]
	[CodeProperty(WhsCycleCountLocationSchema.Constants.WCL_JobID)]
	public class WhsCycleCountLocation :
		AutoWhsCycleCountLocation,
		INumberFountainConsumer,
		ITaskPlanningJobWithExternalTasks
	{
		public WhsCycleCountLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventModifyOfLocationTriggerError = "Attempted to change Location on Cycle Count task.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventModifyOfTimeTriggerError = "Attempted to change StartTime/EndTime when Cycle Count has completed.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventModifyOfAssignedToTriggerError = "Attempted to change AssignedTo when Cycle Count has started/completed.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventStartCycleCountHasOpenExpectStockVarianceTriggerError = "Attempted to start a Cycle Count that is blocked by open variance from another cycle count.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventCreateCycleCountWhenOldOneNotFinishedTriggerError = "The previous Cycle Count for this Location has not yet been completed, cannot create a new one.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventCreatingCycleCountLocationsWithOpenVarianceTriggerError = "The previous Cycle Count for this Location has not yet been completed, cannot create a new one.";

		#region Location

		public WhsLocation Location
		{
			get { return Factory.Load<WhsLocation>(WCL_WL_Location); }
		}

		[RelatedBusinessObject("Location")]
		[ReadOnly(true)]
		public override ZGuid WCL_WL_Location { get => base.WCL_WL_Location; set => base.WCL_WL_Location = value; }

		#endregion

		#region Variances

		public WhsCycleCountLocationVarianceCollection Variances
		{
			get
			{
				if (variances == null)
				{
					variances = new WhsCycleCountLocationVarianceCollection(this);
				}

				return variances;
			}
		}

		WhsCycleCountLocationVarianceCollection variances;

		#endregion

		#region Properties

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WCL_TaskPlanningStatus
		{
			get => base.WCL_TaskPlanningStatus;
			set => base.WCL_TaskPlanningStatus = value;
		}

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZGuid WCL_P9_Task
		{
			get => base.WCL_P9_Task;
			set => base.WCL_P9_Task = value;
		}

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZDateTimeOffset WCL_EndTime
		{
			get => base.WCL_EndTime;
			set => base.WCL_EndTime = value;
		}

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WCL_JobID
		{
			get => base.WCL_JobID;
			set => base.WCL_JobID = value;
		}

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WCL_Granularity
		{
			get => base.WCL_Granularity;
			set => base.WCL_Granularity = value;
		}

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WCL_GS_NKAssignedTo
		{
			get => base.WCL_GS_NKAssignedTo;
			set => base.WCL_GS_NKAssignedTo = value;
		}

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZByte WCL_Priority
		{
			get => base.WCL_Priority;
			set => base.WCL_Priority = value;
		}

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZDateTimeOffset WCL_StartTime
		{
			get => base.WCL_StartTime;
			set => base.WCL_StartTime = value;
		}

		[ReadOnly(true)]
		public override ZGuid WCL_WCL_RejectedCycleCount
		{
			get => base.WCL_WCL_RejectedCycleCount;
			set => base.WCL_WCL_RejectedCycleCount = value;
		}

		public ZBool HasOpenVariances => Variances.Count > 0 && Variances.All(v => v.WCC_Status == CycleCountVarianceStatus.Codes.Open);

		public ZBool IsBulkCycleCount => WCL_Granularity == CycleCountGranularity.Codes.PalletCount || WCL_Granularity == CycleCountGranularity.Codes.ProductOnly;

		public ZBool CanApproveVariances => !WCL_EndTime.IsEmpty && Variances.All(v => v.WCC_AuthorizedAction == CycleCountVarianceAuthorizedAction.Codes.Approved && v.WCC_Status == CycleCountVarianceStatus.Codes.Open);

		ZGuid WarehousePK => Location?.WLV_WW_Whs ?? ZGuid.Empty;

		ZString HumanReadableNameWithoutID => Res.GetString("b6609d4d-df9c-4f57-824e-9cf5f5a68de8", "Cycle Count");

		#region INumberFountainConsumer

		ZString INumberFountainEntityWithID.ID
		{
			get => WCL_JobID;
			set => WCL_JobID = value;
		}

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.WhsCycleCountLocationID;

		#endregion

		#region ITaskPlanningJob

		ZGuid ITaskPlanningJob.PK => PK;

		ZString ITaskPlanningJob.TaskPlanningStatus
		{
			get => WCL_TaskPlanningStatus;
			set => WCL_TaskPlanningStatus = value;
		}

		ZGuid ITaskPlanningJob.WarehousePK => Location.WLV_WW_Whs;

		string ITaskPlanningJob.JobID => Location.WLV_LocationString_UserFriendly;

		ZString ITaskPlanningJob.HumanReadableNameWithoutID => HumanReadableNameWithoutID;

		bool ITaskPlanningJob.IsFinalisedOrCancelled => WCL_EndTime.IsValid;

		void ITaskPlanningJob.ClearFKForProcessTasks(ISet<ZGuid> processTaskPKs)
		{
			if (processTaskPKs.Contains(WCL_P9_Task))
			{
				WCL_P9_Task = ZGuid.Empty;
			}
		}

		string ITaskPlanningJob.SpecialCannotUpdateTaskPlanningStatusReason => string.Empty;

		IProcessTask[] ITaskPlanningJobWithExternalTasks.GetRelatedProcessTasks()
		{
			var task = WCL_P9_Task.IsValid ? Factory.Load<ProcessTask>(WCL_P9_Task) : null;
			return task != null ? [task] : Array.Empty<IProcessTask>();
		}

		#endregion

		#endregion

		#region AcceptVariances

		public void AcceptVariances(IWhsDocketCreationLogger logger)
		{
			if (!IsBulkCycleCount && CanApproveVariances)
			{
				var adjustmentsForOtherLocations = new List<WhsAdjustment>();
				var adjustmentsForCycleCountLocation = new List<WhsAdjustment>();

				var unexpectedInventoriesWithPalletID = WhsCycleCountLocationHelper.GetUnexpectedInventoriesWithPalletID(Factory, this);
				var unexpectedInventoriesWithSerialNumber = WhsCycleCountLocationHelper.GetUnexpectedInventoriesWithSerialNumber(Factory, this);
				var hasErrors = unexpectedInventoriesWithPalletID
					.Union(unexpectedInventoriesWithSerialNumber)
					.Any(inv => inv.LocationClass == LocationClasses.Codes.CON || inv.LocationClass == LocationClasses.Codes.DDL || inv.LocationClass == LocationClasses.Codes.PST);

				if (!hasErrors)
				{
					hasErrors = AcceptVariancesCore(logger, adjustmentsForOtherLocations, adjustmentsForCycleCountLocation, unexpectedInventoriesWithPalletID, unexpectedInventoriesWithSerialNumber);
				}
				else
				{
					logger.LogFailure(GetUnableToAcceptVariancesErrorMessage());
					logger.LogFailure(Res.GetString("d7079da6-42d8-4a78-a0e2-a0f06d357424", "There are unexpected inventories in a dock door, packing station or consolidation location. Please finalize the associated picking job(s) first before accepting the variance(s)."));
					MarkAllVariancesAsErrorInNewFactory(logger);
				}

				if (!hasErrors)
				{
					try
					{
						ApproveVariances(Variances);
						Factory.Save();
						CreateAdjustmentSuccessLogs(adjustmentsForOtherLocations, logger);
						CreateAdjustmentSuccessLogs(adjustmentsForCycleCountLocation, logger);
					}
					catch (ZSaveException ex)
					{
						logger.LogFailure(Res.GetString("3f9d660f-102e-462b-840f-a1ac06c74cf2", "An error occurred while saving approved cycle count variances with adjustments: {0}", ex.FriendlyMessage));
						MarkAllVariancesAsErrorInNewFactory(logger);
					}
				}
			}
			else
			{
				logger.LogFailure(GetUnableToAcceptVariancesErrorMessage());
				logger.LogFailure(
					Res.GetString("88c7ea53-215d-4d2c-b3bc-d0bd38f43fb2",
					"Two possible reasons for this are the granularity of cycle count is '{0}'/'{1}' or the variances of cycle count are not valid(Status must be '{2}', Authorized Action must be '{3}').",
					CycleCountGranularity.Codes.PalletCount,
					CycleCountGranularity.Codes.ProductOnly,
					CycleCountVarianceStatus.Codes.Open,
					CycleCountVarianceAuthorizedAction.Codes.Approved)
				);

				MarkAllVariancesAsErrorInNewFactory(logger);
			}

			string GetUnableToAcceptVariancesErrorMessage() => Res.GetString("bb948541-5228-4e00-8c0e-087b62979c24", "No variances have been approved for Location '{0}' in Warehouse '{1}'.", Location.WLV_LocationString, Location.Warehouse.WW_WarehouseNameMultilingual);
		}

		bool AcceptVariancesCore(
			IWhsDocketCreationLogger logger,
			List<WhsAdjustment> adjustmentsForOtherLocations,
			List<WhsAdjustment> adjustmentsForCycleCountLocation,
			WhsInventoryView[] unexpectedInventoriesWithPalletID,
			WhsInventoryView[] unexpectedInventoriesWithSerialNumber)
		{
			Factory.GetCachedValue<WhsAdjustmentValidationStrategy>(nameof(WhsAdjustmentValidationStrategy), () => new WhsAdjustmentValidationStrategy_CycleCount());

			var previousHoldCodeCache = new CycleCountPreviousHoldCodeCache();
			var previousInventoryHoldCodesCache = new Dictionary<ZGuid, ZString>();

			var updatedNegativeVariancesForUnexpectedInventoriesWithPalletID = AcceptUnexpectedInventoriesWithPalletID(adjustmentsForOtherLocations, previousHoldCodeCache, previousInventoryHoldCodesCache, unexpectedInventoriesWithPalletID);
			var updatedNegativeVariancesForUnexpectedInventoriesWithSerialNumber = AcceptUnexpectedInventoriesWithSerialNumbers(adjustmentsForOtherLocations, previousHoldCodeCache, previousInventoryHoldCodesCache, unexpectedInventoriesWithSerialNumber);
			var updatedNegativeVariancesToRollbackOnError = updatedNegativeVariancesForUnexpectedInventoriesWithPalletID.Concat(updatedNegativeVariancesForUnexpectedInventoriesWithSerialNumber);

			var hasErrors = FinaliseAdjustments(adjustmentsForOtherLocations, logger);
			if (!hasErrors)
			{
				AcceptNegativeVariances(adjustmentsForCycleCountLocation, previousInventoryHoldCodesCache);
				AcceptPositiveVariances(adjustmentsForCycleCountLocation, adjustmentsForOtherLocations, previousHoldCodeCache);

				hasErrors = FinaliseAdjustments(adjustmentsForCycleCountLocation, logger);

				if (!hasErrors)
				{
					var heldInventories = GetLostInCycleCountInventory().Where(i => i.WI_AvailableToTransferQuantity > 0);
					foreach (var inventory in heldInventories)
					{
						WhsCycleCountLocationHelper.HoldInventory(inventory, ZString.Empty, inventory.WI_AvailableToTransferQuantity);
					}
				}
				else
				{
					adjustmentsForOtherLocations.ForEach(adj => adj.Delete());
					RollbackChangesOnRelatedNegativeVariances(updatedNegativeVariancesToRollbackOnError);
					MarkAllVariancesAsErrorInNewFactory(logger);
				}
			}
			else
			{
				RollbackChangesOnRelatedNegativeVariances(updatedNegativeVariancesToRollbackOnError);
			}

			return hasErrors;
		}

		IEnumerable<WhsInventoryView> GetLostInCycleCountInventory()
		{
			var inventorySQL = @"
SELECT 
	WI_PK 
FROM 
	dbo.WhsInventoryView
	JOIN dbo.WhsDocketLine ON WI_WE_InDocketLine = WE_PK
WHERE
	WI_WL = @LocationPK AND
	WE_WHC_NKCurrentInventoryHeldCode <> '' AND
	WE_WHC_NKCurrentInventoryHeldCode = @HeldCode AND
	WI_TotalUnits > 0";

			var parameter = new ZSqlParameterCollection();
			parameter.Add("@LocationPK", WCL_WL_Location, WhsCycleCountLocationSchema.WCL_WL_Location);
			parameter.Add("@HeldCode", InventoryHoldCodes.Codes.LostInCycleCount, WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode);

			var inventoryQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			inventoryQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", WhsInventoryViewSchema.PK.Name, inventorySQL), parameter);

			return Factory.Load<WhsInventoryView>(inventoryQuery);
		}

		IEnumerable<WhsCycleCountLocationVariance> AcceptUnexpectedInventoriesWithPalletID(
			List<WhsAdjustment> adjustmentsForOtherLocations,
			CycleCountPreviousHoldCodeCache previousHoldCodeCache,
			Dictionary<ZGuid, ZString> previousInventoryHoldCodesCache,
			WhsInventoryView[] unexpectedInventoriesWithPalletID)
		{
			var groupedInventories = unexpectedInventoriesWithPalletID
				.GroupBy(i =>
				new
				{
					Client = i.WI_OH_Client,
					Location = i.WI_WL,
					Product = i.SupplierPart,
					PalletID = i.WI_PalletID.ToUpper(),
					PartAttrib1 = i.WI_PartAttrib1.ToUpper(),
					PartAttrib2 = i.WI_PartAttrib2.ToUpper(),
					PartAttrib3 = i.WI_PartAttrib3.ToUpper(),
					SerialNumber = i.WI_SerialNumber.ToUpper(),
					ExpiryDate = i.WI_ExpiryDate,
					PackingDate = i.WI_PackingDate,
					InventoryStatus = i.WI_InventoryStatus,
					HeldCode = i.WI_HeldCode,
					PreviousHeldCodeForCycleCount = GetPreviousInventoryHoldCode(i.InDocketLine, previousInventoryHoldCodesCache),
				});

			var relatedNegativeVariances = new List<WhsCycleCountLocationVariance>();
			foreach (var groupedInventory in groupedInventories)
			{
				var key = groupedInventory.Key;
				var totalUnits = groupedInventory.Sum(g => g.WI_TotalUnits);
				var openNegativeVariance = WhsCycleCountLocationHelper.GetOpenNegativeVariancesMatchingInventory(Factory, groupedInventory.First(), -totalUnits);
				var adjustmentReasonCode = openNegativeVariance?.WCC_AdjustmentReasonCode ?? AdjustmentReasonCodesCodeList.Codes.StocktakeAdjustment;

				if (key.HeldCode.EqualsIgnoringCase(InventoryHoldCodes.Codes.LostInCycleCount))
				{
					groupedInventory.ForEach(inv => SetHoldCodeForRevertInventoryStatus(inv.InDocketLine, key.PreviousHeldCodeForCycleCount));
				}

				var (holdCode, status) = CalculateHoldCodeAndStatusFromPreviousHoldCode(key.PreviousHeldCodeForCycleCount);

				var adjustment = FindOrCreateAdjustment(adjustmentsForOtherLocations, key.Client, WarehousePK);
				CreateAdjustmentLine(adjustment, key.Product, key.Location, key.PalletID, key.PartAttrib1, key.PartAttrib2, key.PartAttrib3, key.SerialNumber, key.ExpiryDate, key.PackingDate, status, holdCode, -totalUnits, adjustmentReasonCode);
				UpdateRelatedNegativeVariance(relatedNegativeVariances, openNegativeVariance, adjustment);

				if (!key.SerialNumber.IsEmpty)
				{
					previousHoldCodeCache.CacheSerialNumberPreviousHoldCode(
						key.Client,
						key.Product.PK,
						key.SerialNumber,
						key.PreviousHeldCodeForCycleCount);
				}
				else
				{
					previousHoldCodeCache.CachePalletIDPreviousHoldCode(
						key.Client,
						WarehousePK,
						key.Product.PK,
						key.PalletID,
						key.PartAttrib1,
						key.PartAttrib2,
						key.PartAttrib3,
						key.ExpiryDate,
						key.PackingDate,
						key.PreviousHeldCodeForCycleCount);
				}
			}

			return relatedNegativeVariances;
		}

		static void UpdateRelatedNegativeVariance(List<WhsCycleCountLocationVariance> relatedNegativeVariances, WhsCycleCountLocationVariance openNegativeVariance, WhsAdjustment adjustment)
		{
			if (openNegativeVariance != null)
			{
				openNegativeVariance.WCC_WD_RelatedAdjustment = adjustment.PK;
				relatedNegativeVariances.Add(openNegativeVariance);
			}
		}

		IEnumerable<WhsCycleCountLocationVariance> AcceptUnexpectedInventoriesWithSerialNumbers(
			List<WhsAdjustment> adjustmentsForOtherLocations,
			CycleCountPreviousHoldCodeCache previousHoldCodeCache,
			Dictionary<ZGuid, ZString> previousInventoryHoldCodesCache,
			WhsInventoryView[] unexpectedInventoriesWithSerialNumber)
		{
			var relatedNegativeVariances = new List<WhsCycleCountLocationVariance>();
			foreach (var inventory in unexpectedInventoriesWithSerialNumber)
			{
				var adjustment = FindOrCreateAdjustment(adjustmentsForOtherLocations, inventory.WI_OH_Client, inventory.WI_WW_Whs);
				var isAdjustmentLineExisted = CheckAdjustmentLineAlreadyExisted(adjustment, inventory.SupplierPart, inventory.WI_WL, inventory.WI_PalletID, inventory.WI_PartAttrib1, inventory.WI_PartAttrib2, inventory.WI_PartAttrib3, inventory.WI_SerialNumber, inventory.WI_ExpiryDate, inventory.WI_PackingDate, inventory.WI_InventoryStatus, inventory.WI_HeldCode, -inventory.WI_TotalUnits);
				if (!isAdjustmentLineExisted)
				{
					var openNegativeVariance = WhsCycleCountLocationHelper.GetOpenNegativeVariancesMatchingInventory(Factory, inventory, -inventory.WI_TotalUnits);
					var adjustmentReasonCode = openNegativeVariance?.WCC_AdjustmentReasonCode ?? AdjustmentReasonCodesCodeList.Codes.StocktakeAdjustment;
					CreateAdjustmentLineByInventory(adjustment, inventory, -inventory.WI_TotalUnits, adjustmentReasonCode, previousInventoryHoldCodesCache);
					UpdateRelatedNegativeVariance(relatedNegativeVariances, openNegativeVariance, adjustment);

					var previousHeldCode = GetPreviousInventoryHoldCode(inventory.InDocketLine, previousInventoryHoldCodesCache);
					if (!previousHeldCode.IsEmpty)
					{
						previousHoldCodeCache.CacheSerialNumberPreviousHoldCode(inventory.WI_OH_Client, inventory.WI_OP, inventory.WI_SerialNumber, previousHeldCode);
					}
				}
			}

			return relatedNegativeVariances;
		}

		void AcceptNegativeVariances(List<WhsAdjustment> adjustmentsForCycleCountLocation, Dictionary<ZGuid, ZString> previousInventoryHoldCodesCache)
		{
			var inventories = WhsCycleCountLocationHelper.GetInventoriesMatchingNegativeVariances(Factory, this);
			var missedVariances = Variances.Where(v => v.WCC_VarianceQty < 0);
			foreach (var variance in missedVariances)
			{
				if (variance.WCC_WD_RelatedAdjustment.IsEmpty)
				{
					var matchedInventories = inventories.Where(i =>
						i.WI_OP == variance.WCC_OP_Product
						&& i.WI_OH_Client == variance.WCC_OH_Client
						&& i.WI_PalletID.EqualsIgnoringCase(variance.WCC_PalletID)
						&& i.WI_PartAttrib1.EqualsIgnoringCase(variance.WCC_PartAttrib1)
						&& i.WI_PartAttrib2.EqualsIgnoringCase(variance.WCC_PartAttrib2)
						&& i.WI_PartAttrib3.EqualsIgnoringCase(variance.WCC_PartAttrib3)
						&& i.WI_SerialNumber.EqualsIgnoringCase(variance.WCC_SerialNumber)
						&& i.WI_ExpiryDate == variance.WCC_ExpiryDate
						&& i.WI_PackingDate == variance.WCC_PackingDate
						&& i.WI_AvailableToTransferQuantity > 0);

					var adjustment = FindOrCreateAdjustment(adjustmentsForCycleCountLocation, variance.WCC_OH_Client, WarehousePK);
					CreateAdjustmentOutLines(adjustment, variance, matchedInventories, previousInventoryHoldCodesCache);
					variance.WCC_WD_RelatedAdjustment = adjustment.PK;
				}
			}
		}

		void CreateAdjustmentOutLines(WhsAdjustment adjustment, WhsCycleCountLocationVariance variance, IEnumerable<WhsInventoryView> matchedInventories, Dictionary<ZGuid, ZString> previousInventoryHoldCodesCache)
		{
			var adjustOutQty = (ZDecimal)Math.Abs(variance.WCC_VarianceQty);
			var matchedLCCHeldInventories = matchedInventories.Where(i => i.WI_InventoryStatus == InventoryStatus.Codes.Held && i.WI_HeldCode == InventoryHoldCodes.Codes.LostInCycleCount).ToArray();
			adjustOutQty = CreateAdjustmentOutLineByInventories(matchedLCCHeldInventories, adjustment, adjustOutQty, variance.WCC_AdjustmentReasonCode, previousInventoryHoldCodesCache);

			if (adjustOutQty > 0)
			{
				var availableInventories = matchedInventories.Where(i => i.WI_InventoryStatus == InventoryStatus.Codes.Available);
				adjustOutQty = CreateAdjustmentOutLineByInventories(availableInventories, adjustment, adjustOutQty, variance.WCC_AdjustmentReasonCode, previousInventoryHoldCodesCache);
			}

			if (adjustOutQty > 0)
			{
				var nonLCCHeldInventories = matchedInventories.Where(i => i.WI_InventoryStatus == InventoryStatus.Codes.Held && i.WI_HeldCode != InventoryHoldCodes.Codes.LostInCycleCount).ToArray();
				adjustOutQty = CreateAdjustmentOutLineByInventories(nonLCCHeldInventories, adjustment, adjustOutQty, variance.WCC_AdjustmentReasonCode, previousInventoryHoldCodesCache);
			}

			if (adjustOutQty > 0)
			{
				CreateAdjustmentLineByVariance(adjustment, variance, -adjustOutQty, ZString.Empty);
			}
		}

		ZDecimal CreateAdjustmentOutLineByInventories(IEnumerable<WhsInventoryView> matchedInventories, WhsAdjustment adjustment, ZDecimal adjustOutQty, string adjustmentReasonCode, Dictionary<ZGuid, ZString> previousInventoryHoldCodesCache)
		{
			foreach (var inventory in matchedInventories)
			{
				var qty = Math.Min(adjustOutQty, inventory.WI_AvailableToTransferQuantity);
				CreateAdjustmentLineByInventory(adjustment, inventory, -qty, adjustmentReasonCode, previousInventoryHoldCodesCache);

				adjustOutQty -= qty;
				if (adjustOutQty == 0)
				{
					break;
				}
			}

			return adjustOutQty;
		}

		void AcceptPositiveVariances(List<WhsAdjustment> adjustmentsForCycleCountLocation, List<WhsAdjustment> adjustmentsForOtherLocations, CycleCountPreviousHoldCodeCache previousHoldCodeCache)
		{
			var matchedVariances = Variances.Where(v => v.WCC_VarianceQty > 0);
			foreach (var variance in matchedVariances)
			{
				var adjustment = FindOrCreateAdjustment(adjustmentsForCycleCountLocation, variance.WCC_OH_Client, WarehousePK);

				var previousHoldCode = RetrievePreviousHoldCodeForVariance(variance, previousHoldCodeCache);

				CreateAdjustmentLineByVariance(adjustment, variance, variance.WCC_VarianceQty, previousHoldCode);
				variance.WCC_WD_RelatedAdjustment = adjustment.PK;

				if (!variance.WCC_PalletID.IsEmpty)
				{
					var childAdjustmentsWithSamePalletID = adjustmentsForOtherLocations.Where(adj => adj.WD_OH_Client == adjustment.WD_OH_Client && adj.Lines.Any(l => l.WE_PalletID.EqualsIgnoringCase(variance.WCC_PalletID)));
					UpdateChildAdjustments(childAdjustmentsWithSamePalletID, adjustment, variance, (line, refVariance) => line.WE_PalletID.EqualsIgnoringCase(refVariance.WCC_PalletID));
				}

				var product = variance.WhsProduct;
				var client = variance.Client;
				if (product != null && product.IsSerialNumberUsedAndNotReleaseCaptured(client))
				{
					var childAdjustmentsWithSerialNumber = adjustmentsForOtherLocations.Where(adj => adj.WD_OH_Client == adjustment.WD_OH_Client && HasAdjustOutSerialNumber(adj, variance, client, product));
					UpdateChildAdjustments(childAdjustmentsWithSerialNumber, adjustment, variance, (line, refVariance) => AdjustLineAndVarianceHasSameSerialNumber(line, refVariance));
				}
			}

			SetParentDocketFallback(matchedVariances, adjustmentsForOtherLocations);
		}

		ZString RetrievePreviousHoldCodeForVariance(WhsCycleCountLocationVariance variance, CycleCountPreviousHoldCodeCache previousHoldCodeCache)
		{
			var result = ZString.Empty;
			if (!variance.WCC_SerialNumber.IsEmpty)
			{
				result = previousHoldCodeCache.RetrieveHoldCodeForSerialNumber(
					variance.WCC_OH_Client,
					variance.WCC_OP_Product,
					variance.WCC_SerialNumber);
			}
			else if (!variance.WCC_PalletID.IsEmpty)
			{
				result = previousHoldCodeCache.RetrieveHoldCodeForPalletID(
					variance.WCC_OH_Client,
					WarehousePK,
					variance.WCC_OP_Product,
					variance.WCC_PalletID,
					variance.WCC_PartAttrib1,
					variance.WCC_PartAttrib2,
					variance.WCC_PartAttrib3,
					variance.WCC_ExpiryDate,
					variance.WCC_PackingDate);
			}

			return result;
		}

		void UpdateChildAdjustments(IEnumerable<WhsAdjustment> childAdjustments, WhsAdjustment adjustment, WhsCycleCountLocationVariance variance, Func<WhsDocketLine, WhsCycleCountLocationVariance, bool> adjustmentLineToVarianceComparer)
		{
			childAdjustments
				.Where(adj => adj.WD_WD_ParentDocket.IsEmpty)
				.ForEach(adj => adj.WD_WD_ParentDocket = adjustment.PK);
			childAdjustments
				.SelectMany(adj => adj.Lines)
				.Where(line => adjustmentLineToVarianceComparer(line, variance))
				.ForEach(line => line.WE_ReasonCode = GetValidAdjustmentReasonCode((WhsAdjustmentLine)line, variance.WCC_AdjustmentReasonCode));
		}

		void SetParentDocketFallback(IEnumerable<WhsCycleCountLocationVariance> matchedVariances, List<WhsAdjustment> adjustmentsForOtherLocations)
		{
			var childAdjustmentWithoutParents = adjustmentsForOtherLocations.Where(adj => adj.WD_WD_ParentDocket.IsEmpty);
			foreach (var childAdjustment in childAdjustmentWithoutParents)
			{
				foreach (var childAdjustmentLine in childAdjustment.Lines)
				{
					if (!childAdjustmentLine.WE_PalletID.IsEmpty)
					{
						var variance = matchedVariances.FirstOrDefault(v => v.WCC_PalletID.EqualsIgnoringCase(childAdjustmentLine.WE_PalletID));
						childAdjustment.WD_WD_ParentDocket = variance.WCC_WD_RelatedAdjustment;
						break;
					}
				}
			}
		}

		ZBool HasAdjustOutSerialNumber(WhsAdjustment adjustment, WhsCycleCountLocationVariance variance, OrgHeader client, WhsProduct product)
		{
			var result = false;
			if (client.PK == adjustment.WD_OH_Client)
			{
				result = product.IsSerialNumberUsedAndNotReleaseCaptured(client) && adjustment.Lines.Any(l => AdjustLineAndVarianceHasSameSerialNumber(l, variance));
			}

			return result;
		}

		ZBool AdjustLineAndVarianceHasSameSerialNumber(WhsDocketLine line, WhsCycleCountLocationVariance variance)
			=> line.WE_OP == variance.WCC_OP_Product && line.WE_SerialNumber.EqualsIgnoringCase(variance.WCC_SerialNumber);

		ZBool FinaliseAdjustments(List<WhsAdjustment> adjustments, IWhsDocketCreationLogger logger)
		{
			var hasErrors = false;
			foreach (var adjustment in adjustments.Where(adj => !adj.IsFinalised))
			{
				adjustment.FinaliseDocketWithoutUserConfirmation();
				if (!adjustment.IsFinalised)
				{
					var hasCommittedInventory = adjustment.Lines.Cast<WhsAdjustmentLine>().Any(l =>
					{
						var transactionLine = ((ILineWithCommittedPickLines)l).CommittedStrategy;
						return transactionLine.TotalQtyCommitted < transactionLine.TotalTransactionQty;
					});

					var errors = new List<INotification>();
					if (hasCommittedInventory)
					{
						errors.Add(new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("1dd3451d-19c3-4e02-88fb-7ad1f35d4611", "Not enough inventory was found. The stock may be committed by existing job(s) or has been picked.")));
					}

					errors.AddRange(adjustment.GetErrors());
					CreateAdjustmentErrorLogs(adjustment, logger, errors.ToUniqueMessageListString());

					hasErrors = true;
					break;
				}
			}

			if (hasErrors)
			{
				foreach (var adjustment in adjustments)
				{
					adjustment.Delete();
				}
			}

			return hasErrors;
		}

		void CreateAdjustmentErrorLogs(WhsAdjustment adjustment, IWhsDocketCreationLogger logger, ZString errorMessage)
		{
			var warehouseName = Location.Warehouse.WW_WarehouseNameMultilingual;
			var locationString = Location.WLV_LocationString;

			logger.LogFailure(
				Res.GetString("0159d9a0-0d21-4f77-9d70-d01fa7dd31ab",
				"Failed to create adjustment for Client '{0}', Warehouse '{1}' by cycle count for Location '{2}' in Warehouse '{3}'.\r\n{4}",
				adjustment.Client.OH_Code,
				adjustment.Warehouse.WW_WarehouseNameMultilingual,
				locationString,
				warehouseName,
				errorMessage)
			);
		}

		void RollbackChangesOnRelatedNegativeVariances(IEnumerable<WhsCycleCountLocationVariance> relatedNegativeVariances)
		{
			relatedNegativeVariances.ForEach(relatedVariance => relatedVariance.WCC_WD_RelatedAdjustment = ZGuid.Empty);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Adjustment external reference")]
		WhsAdjustment FindOrCreateAdjustment(List<WhsAdjustment> cachedAdjustments, ZGuid clientPK, ZGuid warehousePK)
		{
			var matchedAdjustment = cachedAdjustments.FirstOrDefault(adj => adj.WD_OH_Client == clientPK && adj.WD_WW_Whs == warehousePK && !adj.IsFinalised);
			if (matchedAdjustment == null)
			{
				const string cycleCountReferencePrefix = "CYCLECOUNT ADJUSTMENT";

				matchedAdjustment = Factory.New<WhsAdjustment>();
				matchedAdjustment.WD_OH_Client = clientPK;
				matchedAdjustment.WD_WW_Whs = warehousePK;
				matchedAdjustment.WD_ExternalReference = cycleCountReferencePrefix;

				cachedAdjustments.Add(matchedAdjustment);
			}

			return matchedAdjustment;
		}

		void CreateAdjustmentLineByInventory(WhsAdjustment adjustment, WhsInventoryView inventory, ZDecimal adjustmentQuantity, string adjustmentReasonCode, Dictionary<ZGuid, ZString> previousInventoryHoldCodesCache)
		{
			var inventoryLine = inventory.InDocketLine;
			var previousHoldCode = GetPreviousInventoryHoldCode(inventoryLine, previousInventoryHoldCodesCache);

			SetHoldCodeForRevertInventoryStatus(inventoryLine, previousHoldCode);

			CreateAdjustmentLine(adjustment,
				inventory.SupplierPart,
				inventory.WI_WL,
				inventory.WI_PalletID,
				inventory.WI_PartAttrib1,
				inventory.WI_PartAttrib2,
				inventory.WI_PartAttrib3,
				inventory.WI_SerialNumber,
				inventory.WI_ExpiryDate,
				inventory.WI_PackingDate,
				inventory.WI_InventoryStatus,
				inventory.WI_HeldCode,
				adjustmentQuantity,
				adjustmentReasonCode);
		}

		void SetHoldCodeForRevertInventoryStatus(WhsDocketLine inventoryLine, ZString holdCode)
		{
			var (newHoldCode, newStatus) = CalculateHoldCodeAndStatusFromPreviousHoldCode(holdCode);
			inventoryLine.WE_WHC_NKCurrentInventoryHeldCode = newHoldCode;
			inventoryLine.WE_CurrentInventoryStatus = newStatus;
		}

		(ZString HoldCode, ZString Status) CalculateHoldCodeAndStatusFromPreviousHoldCode(ZString previousHoldCode)
		{
			var holdCode = previousHoldCode.EqualsIgnoringCase(InventoryHoldCodes.Codes.ShortPicked)
					? ZString.Empty
					: previousHoldCode;

			var status = holdCode.IsEmpty
				? InventoryStatus.Codes.Available
				: InventoryStatus.Codes.Held;

			return (holdCode, status);
		}

		void CreateAdjustmentLineByVariance(WhsAdjustment adjustment, WhsCycleCountLocationVariance variance, ZDecimal adjustmentQuantity, ZString inventoryHeldCode)
		{
			var inventoryStatus = inventoryHeldCode.IsEmpty ? InventoryStatus.Codes.Available : InventoryStatus.Codes.Held;

			CreateAdjustmentLine(adjustment,
				variance.Product, WCL_WL_Location,
				variance.WCC_PalletID,
				variance.WCC_PartAttrib1,
				variance.WCC_PartAttrib2,
				variance.WCC_PartAttrib3,
				variance.WCC_SerialNumber,
				variance.WCC_ExpiryDate,
				variance.WCC_PackingDate,
				inventoryStatus: inventoryStatus,
				heldCode: inventoryHeldCode,
				adjustmentQuantity,
				variance.WCC_AdjustmentReasonCode);
		}

		void CreateAdjustmentLine(WhsAdjustment adjustment,
			OrgSupplierPart part,
			ZGuid locationPK,
			ZString palletID,
			ZString partAttrib1,
			ZString partAttrib2,
			ZString partAttrib3,
			ZString serialNumber,
			ZDate expiryDate,
			ZDate packingDate,
			ZString inventoryStatus,
			ZString heldCode,
			ZDecimal adjustmentQuantity,
			string adjustmentReasonCode)
		{
			var adjLine = adjustment.Lines.AddNew();
			adjLine.WE_OP = part.PK;
			adjLine.WE_WL = locationPK;
			adjLine.WE_F3_NKPackType = part.OP_StockKeepingUnit;
			adjLine.WE_ReasonCode = GetValidAdjustmentReasonCode(adjLine, adjustmentReasonCode);
			adjLine.WE_TransactionQuantity = adjustmentQuantity;
			adjLine.WE_PartAttrib1 = partAttrib1.ToUpper();
			adjLine.WE_PartAttrib2 = partAttrib2.ToUpper();
			adjLine.WE_PartAttrib3 = partAttrib3.ToUpper();
			adjLine.WE_SerialNumber = serialNumber.ToUpper();
			adjLine.WE_ExpiryDate = expiryDate;
			adjLine.WE_PackingDate = packingDate;
			adjLine.WE_PalletID = palletID.ToUpper();
			adjLine.WE_OriginalInventoryStatus = inventoryStatus;
			adjLine.WE_WHC_NKOriginalInventoryHeldCode = heldCode;
		}

		string GetValidAdjustmentReasonCode(WhsAdjustmentLine adjustmentLine, string reasonCode)
		{
			if (AdjustmentReasonCodes == null)
			{
				AdjustmentReasonCodes = adjustmentLine.Lookups.AdjustmentReasonCodes;
			}

			return AdjustmentReasonCodes.ContainsCode(reasonCode)
						 ? reasonCode
						 : AdjustmentReasonCodesCodeList.Codes.StocktakeAdjustment;
		}

		CodeDescriptionPairList AdjustmentReasonCodes;

		ZBool CheckAdjustmentLineAlreadyExisted(WhsAdjustment adjustment, OrgSupplierPart part, ZGuid locationPK, ZString palletID, ZString partAttrib1, ZString partAttrib2, ZString partAttrib3, ZString serialNumber, ZDate expiryDate, ZDate packingDate, ZString inventoryStatus, ZString heldCode, ZDecimal adjustmentQuantity)
		{
			return adjustment.Lines.Any(l =>
							l.WE_OP == part.PK
							&& l.WE_F3_NKPackType == part.OP_StockKeepingUnit
							&& l.WE_WL == locationPK
							&& l.WE_PalletID.EqualsIgnoringCase(palletID)
							&& l.WE_PartAttrib1.EqualsIgnoringCase(partAttrib1)
							&& l.WE_PartAttrib2.EqualsIgnoringCase(partAttrib2)
							&& l.WE_PartAttrib3.EqualsIgnoringCase(partAttrib3)
							&& l.WE_SerialNumber.EqualsIgnoringCase(serialNumber)
							&& l.WE_ExpiryDate == expiryDate
							&& l.WE_PackingDate == packingDate
							&& l.WE_OriginalInventoryStatus == inventoryStatus
							&& l.WE_WHC_NKOriginalInventoryHeldCode == heldCode
							&& l.WE_TransactionQuantity == adjustmentQuantity);
		}

		void CreateAdjustmentSuccessLogs(List<WhsAdjustment> adjustments, IWhsDocketCreationLogger logger)
		{
			foreach (var adjustment in adjustments)
			{
				logger.LogSuccess(
					Res.GetString("dab1a13c-f675-42f6-a89c-4c942e6bc58f",
					"Adjustment {0} was created successfully for Client '{1}', Warehouse '{2}'.",
					adjustment.WD_DocketID,
					adjustment.Client.OH_Code,
					adjustment.Warehouse.WW_WarehouseNameMultilingual)
				);
			}
		}

		static void ChangeVarianceStatus(WhsCycleCountLocationVariance variance, string status)
		{
			variance.WCC_AuthorizedAction = CycleCountVarianceAuthorizedAction.Codes.Empty;
			variance.WCC_Status = status;
		}

		static void MarkVarianceAsError(WhsCycleCountLocationVariance variance)
		{
			variance.WCC_AuthorizedAction = CycleCountVarianceAuthorizedAction.Codes.Error;
			variance.WCC_Status = CycleCountVarianceStatus.Codes.Open;
			variance.WCC_WD_RelatedAdjustment = ZGuid.Empty;
		}

		void MarkAllVariancesAsErrorInNewFactory(IWhsDocketCreationLogger logger)
		{
			try
			{
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var cycleCountInNewFactory = newFactory.Load<WhsCycleCountLocation>(PK);
				cycleCountInNewFactory.Variances.ForEach(v => MarkVarianceAsError(v));

				newFactory.Save();
			}
			catch (ZSaveException errorEx)
			{
				logger.LogFailure(Res.GetString("0a05f6d2-5ba3-4e07-938b-2af7561de0ba", "An error occurred while marking and saving cycle count variances as Error: {0}", errorEx.FriendlyMessage));
			}
		}

		#region WhsAdjustmentValidationStrategy_CycleCount Class

		class WhsAdjustmentValidationStrategy_CycleCount : WhsAdjustmentValidationStrategy
		{
			protected override void CheckLocationString_ProductAssignedToCorrectFixedOrDynamicLocationCore(WhsAdjustmentLine adjustmentLine)
			{
				// Because this function is used for Cycle Count Variance Service, we don't need to run the validation for this case.
				// If we find a non Pick Face product really in a Pick Face Location, we should allow it be adjusted in.
				// Test in WhsCycleCountLocationTest.TestAcceptVariances_VarianceIsPostive_FixedLocation
			}
		}

		#endregion

		#endregion

		#region RejectVariances

		public void RejectVariances(IWhsDocketCreationLogger logger)
		{
			try
			{
				RejectVariances(Variances);
				RevertInventoryStatus();
				var newCycleCountLocation = CreateCycleCountLocation();
				Factory.Save();

				logger.LogSuccess(
					Res.GetString("4d3683bc-706e-45e4-86b8-62dc61fdc236",
					"Re-Count was created with Granularity '{0}' and Priority '{1}' for Location '{2}' in Warehouse '{3}'.",
					newCycleCountLocation.WCL_Granularity, newCycleCountLocation.WCL_Priority, newCycleCountLocation.Location.WLV_LocationString, newCycleCountLocation.Location.Warehouse.WW_WarehouseNameMultilingual));
			}
			catch (ZSaveException ex)
			{
				logger.LogFailure(Res.GetString("be56cf35-964f-4d6c-9493-630e2c58f268", "An error occurred while saving rejected cycle count variances: {0}", ex.FriendlyMessage));

				MarkAllVariancesAsErrorInNewFactory(logger);
			}
		}

		WhsCycleCountLocation CreateCycleCountLocation()
		{
			var newCycleCountLocation = Factory.New<WhsCycleCountLocation>();
			newCycleCountLocation.WCL_WL_Location = WCL_WL_Location;
			newCycleCountLocation.WCL_Granularity = WCL_Granularity == CycleCountGranularity.Codes.PalletCount
				? CycleCountGranularity.Codes.PalletIDOnly
				: (WCL_Granularity == CycleCountGranularity.Codes.ProductOnly ? CycleCountGranularity.Codes.ProductWithAttributes : WCL_Granularity.ToString());
			newCycleCountLocation.WCL_WCL_RejectedCycleCount = PK;
			newCycleCountLocation.WCL_Priority = WCL_Priority;
			return newCycleCountLocation;
		}

		static void ApproveVariances(WhsCycleCountLocationVarianceCollection variances)
		{
			foreach (var variance in variances)
			{
				ChangeVarianceStatus(variance, CycleCountVarianceStatus.Codes.Approved);
			}
		}

		static void RejectVariances(WhsCycleCountLocationVarianceCollection variances)
		{
			foreach (var variance in variances)
			{
				ChangeVarianceStatus(variance, CycleCountVarianceStatus.Codes.Rejected);
			}
		}

		#region RevertInventoryStatus

		void RevertInventoryStatus()
		{
			var heldInventories = GetHeldInventoriesInCurrentLocation()
							.Concat(GetUnexpectedHeldInventories(WhsCycleCountLocationHelper.UnexpectedInventoryWithPalletIDSQL, true))
							.Concat(GetUnexpectedHeldInventories(WhsCycleCountLocationHelper.UnexpectedInventoryWithSerialNumberSQL, false));

			var checkedInventory = new Dictionary<WhsInventoryView, ZString>();
			var previousInventoryHoldCodesCache = new Dictionary<ZGuid, ZString>();

			foreach (var heldInventory in heldInventories)
			{
				WhsCycleCountLocationHelper.HoldInventory(heldInventory, GetHoldCodeForRevert(heldInventory, checkedInventory, previousInventoryHoldCodesCache), heldInventory.WI_AvailableToPickQuantity);
			}
		}

		ZString GetHoldCodeForRevert(WhsInventoryView heldInventory, Dictionary<WhsInventoryView, ZString> checkedInventory, Dictionary<ZGuid, ZString> previousInventoryHoldCodesCache)
		{
			if (!checkedInventory.TryGetValue(heldInventory, out var holdCodeForRevert))
			{
				holdCodeForRevert = GetPreviousInventoryHoldCode(heldInventory.InDocketLine, previousInventoryHoldCodesCache);

				checkedInventory[heldInventory] = holdCodeForRevert;
			}

			return holdCodeForRevert;
		}

		IEnumerable<WhsInventoryView> GetHeldInventoriesInCurrentLocation()
		{
			var inventorySQL = @"
SELECT
	WI_PK
FROM
	dbo.WhsInventoryView
	JOIN dbo.WhsDocketLine ON WI_WE_InDocketLine = WE_PK
WHERE
	WI_WL = @LocationPK AND
	WE_WHC_NKCurrentInventoryHeldCode <> '' AND
	WE_WHC_NKCurrentInventoryHeldCode = @HeldCode AND
	WI_TotalUnits > 0";
			var parameter = new ZSqlParameterCollection();
			parameter.Add("@LocationPK", WCL_WL_Location, WhsCycleCountLocationSchema.WCL_WL_Location);
			parameter.Add("@HeldCode", InventoryHoldCodes.Codes.LostInCycleCount, WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode);

			var inventoryQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			inventoryQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", WhsInventoryViewSchema.PK.Name, inventorySQL), parameter);

			return Factory.Load<WhsInventoryView>(inventoryQuery);
		}

		IEnumerable<WhsInventoryView> GetUnexpectedHeldInventories(ZString unexpectedHeldInventorySQL, ZBool requireWhsPK)
		{
			var inventorySQL = string.Format(CultureInfo.InvariantCulture, @"
SELECT
	WI_PK
FROM
	dbo.WhsInventoryView
	JOIN dbo.WhsDocketLine ON WI_PK = WE_PK
WHERE
	WI_PK IN ({0}) AND
	WE_WHC_NKCurrentInventoryHeldCode <> '' AND
	WE_WHC_NKCurrentInventoryHeldCode = @HeldCode AND
	NOT EXISTS
	(
		SELECT NULL
		FROM
			dbo.WhsCycleCountLocation
			JOIN dbo.WhsCycleCountLocationVariance ON WCC_WCL_CycleCountLocation = WCL_PK
		WHERE
			WCL_WL_Location = WI_WL AND
			WCC_Status = 'OPN'
	)", unexpectedHeldInventorySQL);
			var parameter = new ZSqlParameterCollection();
			parameter.Add("@CycleCountPK", PK, WhsCycleCountLocationSchema.PK);
			parameter.Add("@HeldCode", InventoryHoldCodes.Codes.LostInCycleCount, WhsDocketLineSchema.WE_WHC_NKCurrentInventoryHeldCode);
			if (requireWhsPK)
			{
				parameter.Add("@WhsPK", Location.WLV_WW_Whs, WhsLocationViewSchema.WLV_WW_Whs);
			}

			var inventoryQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			inventoryQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} IN ({1})", WhsInventoryViewSchema.PK.Name, inventorySQL), parameter);

			return Factory.Load<WhsInventoryView>(inventoryQuery);
		}

		#endregion

		#endregion

		ZString GetPreviousInventoryHoldCode(WhsDocketLine inventory, Dictionary<ZGuid, ZString> previousInventoryHoldCodesCache)
			=> GetPreviousInventoryHoldCodeFromInventoryHoldChangeLogTable(inventory, previousInventoryHoldCodesCache);

		ZString GetPreviousInventoryHoldCodeFromInventoryHoldChangeLogTable(WhsDocketLine inventory, Dictionary<ZGuid, ZString> previousInventoryHoldCodesCache)
		{
			if (!previousInventoryHoldCodesCache.TryGetValue(inventory.PK, out var previousHoldCode))
			{
				var previousHoldCodeLog = Factory.LoadTop1<WhsInventoryHoldChangeLog>(GetPreviousInventoryHoldCodeQuery(inventory.PK));
				previousHoldCode = previousHoldCodeLog?.WHL_WHC_NKCode ?? inventory.WE_WHC_NKOriginalInventoryHeldCode;
				previousInventoryHoldCodesCache[inventory.PK] = previousHoldCode;
			}

			return previousHoldCode;
		}

		ZQuery GetPreviousInventoryHoldCodeQuery(ZGuid inventoryPK)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsInventoryHoldChangeLogSchema.WHL_WE_ParentDocketLine, inventoryPK);
			query.AddToFilter(WhsInventoryHoldChangeLogSchema.WHL_WHC_NKCode, SQLComparisonOperator.NotEqual, InventoryHoldCodes.Codes.LostInCycleCount);
			query.OrderBy = WhsInventoryHoldChangeLogSchema.Constants.WHL_LogVersion + OrderByClause.Descending;

			return query;
		}
	}
}
