using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.Warehouse;
using Enterprise.Warehouse.Transactions.Business.Bonded;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using StmMenuTemplatePivotBase = Enterprise.DocumentEngine.Business.StmMenuTemplatePivotBase;

namespace Enterprise.Warehouse.Transactions.Business
{
	#region Pick Rules

	/* Pick Form
	 *
	 * User can create a new Pick from Pick Module or open an existing Pick.
	 *
	 * If they create a new Pick the pick status will = NEW
	 * If they open an existing Pick the pick status can equal NEW, PIS, FIN or CAN.
	 *
	 *
	 * Rules if Pick Status = NEW
	 *
	 *		(All orders saved or not will have Pick Status = NEW if they are not Finalised/Cancelled and the Pick Slip has not been printed).
	 *
	 *		User clicks the Attach button and searches for orders to pick. An error icon will show reasons why an order cannot be attached.
	 *
	 *		User selects and attaches one or more valid orders. On attach they are validated.
	 *			If an order cannot be attached an error is shown and the order is not attached.
	 *			If an order can be attached but has an alternative setting, the user is prompted to either abort the attach or allow the system to change the order setting to match the pick.
	 *
	 *		User goes to Pick Slip tab and allocates inventory.
	 *		User can go back and forth from Orders tab to Pick Slip tab to attach more orders / change allocations.
	 *
	 *		Uunsaved picks will have an empty Pick No until they are saved.
	 *		If user prints the Pick Slip.
	 *			User is prompted to save the form if HasChanges = true
	 *			On successful print the Pick status is changed to PIS and PIS rules below now apply.
	 *
	 *
	 *	Rules for Pick Status = PIS
	 *
	 *		Users cannot attach more orders
	 *		Users can change existing allocations.
	 *		Users can reprint the Pick Slip
	 *
	 *
	 *	Rules for Pick Status = FIN or CAN
	 *
	 *		Form is read only and no changes are allowed.
	 *		Users can reprint Pick Slip (if FIN, not CAN).
	 *
	 *
	 * If user tries to Print Pick slip they are told to save first
	 * User saves form, a Pick No is allocated (if not already), Pick Status is unchanged on save.
	 *
	 * As long as the Pick Status == NEW user can go back and add more orders/add/change allocations but they cannot use the Auto Pick button (no need to call pick.PickOrders() again)
	 * If the newly added order has a picktype of Auto, then what do we do?
	 *
	 * If user prints the pick slip, Pick Status now = PIS and users can no longer attach more orders, but they can still change the allocations for existin orders.
	 *
	 *
	 *
	 * Auto Pick from Pick Form
	 *
	 * User creates a New pick from Pick Module - Pick Status = NEW
	 * User attaches order(s) and the order is validated at this time
	 * If the order cannot be added an error is shown and the order is not added
	 * User then clicks the Auto Pick button on lower left of form. Stock is allocated and the Pick Slip is printed if option set (a factory.save is necessary before printing).
	 *
	 * Form is auto-saved -- a Pick No is allocated, Pick Status still == NEW.
	 *
	 * As long as the Pick Status == NEW user can go back and add more orders/add/change allocations but they cannot use the Auto Pick button (no need to call pick.PickOrders() again)
	 * If the newly added order has a picktype of Auto, then what do we do?
	 *
	 * If user prints the pick slip, Pick Status now = PIS and users can no longer attach more orders, but they can still change the allocations for existing orders.

	 *
	 * Pick from Order Form
	 *
	 * User clicks the Pick button.
	 *		User told to save Order.
	 *
	 *		If the order is already picked (WD_WP is not empty)
	 *			the pick form is opened to the Pick Slip tab with the existing pick and nothing else happens	 *
	 *
	 *		else
	 *			If Order Pick Type = 'AUT' the Pick is created, the order is attached, the stock is allocated, the factory is saved (allocating a Pick No),
				and the Pick Slip is printed (if option set) and THE PICK FORM IS NOT OPENED.
	 *
	 *		else if Order Pick Type = 'MAT'
	 *			the Pick is created, the order is attached, the Pick form is opened to the Pick Slip tab and the stock is auto-allocated.
	 *			The Pick form remains for users to make changes.
	 *
	 *		else if Order PickType = 'MAN'
	 *			the Pick is created, the order is attached, the Pick form is opened to the Pick Slip tab and nothing else happens
	 *			The Pick form remains for users to make changes.
	 *
	 *
	 *
	 */

	#endregion

	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_PreventUnPickedPickLinesOnFinalisedPicks, WhsValidationHelper.WhsCheckFinalisedPickWithUnPickedPicklines, WhsPickSchema.Constants.PK, typeof(IWhsCheckFinalisedPickWithUnPickedPicklines_DeferTriggerStrategy))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsPick_PackingStationIsValid, WhsValidationHelper.WhsCheckPickPackingStationCorrect, WhsPickSchema.Constants.PK, typeof(IPackingStationIsSetCorrectlyDeferTriggerStrategy))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsPick_EnsureDocketStatusDepartedOnPickFinalisation, WhsValidationHelper.WhsCheckDocketStatusDepartedOnPickFinalisation, WhsPickSchema.Constants.PK, typeof(IWhsCheckDocketStatusDepartedOnPickFinalisation_DeferTriggerStrategy))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick, WhsValidationHelper.WhsCheckForUnreferencedWhsDockDoorAssignment, WhsPickSchema.Constants.WP_WDA_DockDoorAssignment, typeof(IWhsCheckForUnreferencedWhsDockDoorAssignment_WhsPick_DeferTriggerStrategy), ValueToRunStoredProcWith = ValueVersion.Original)]
	[CodeProperty(WhsPickSchema.Constants.WP_PickNo), DescriptionProperty(WhsPickSchema.Constants.WP_PickNo)]
	public sealed partial class WhsPick : AutoWhsPick,
		ICartageLooseCargo,
		ICartageParent,
		ICreditControlledDocumentDelivery,
		ITransportJobLinkProvider,
		ICustomLabelsConfigOrgProvider,
		IDocManagerSupport,
		IDocumentSupportable,
		IMasterStaffAssigner,
		IProcessHandlingInfoProvider,
		IWhsPick,
		IWorkflowProvider,
		IWhsLogEventParent,
		IPickLineUpdatesRefreshable,
		INumberFountainConsumer,
		ICriticalChangesVersionID,
		ITaskPlanningJob
	{
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventUnPickedPickLinesOnFinalisedPicks = "Attempt to save a Finalised Pick which has unpicked pickedline.";

		public new class Schema : AutoWhsPick.Schema
		{
			public const string DockDoorLocationString = "DockDoorLocationString";
			public const string NumberOfOrders = nameof(NumberOfOrders);
			public const string Picker = nameof(Picker);
			public const string TotalWeight = nameof(TotalWeight);
			public const string TotalVolume = nameof(TotalVolume);
			public const string TotalLineQty = nameof(TotalLineQty);
			public const string TotalComponentQty = nameof(TotalComponentQty);
			public const string TotalWeightUnit = nameof(TotalWeightUnit);
			public const string TotalVolumeUnit = nameof(TotalVolumeUnit);
		}

		#region Events

		#region Printing Events

		public event EventHandler<WhsOrderToPrintEventArgs> OnWhsOrderToPrint;

		public void CallOnWhsOrderToPrint(object sender, WhsOrderToPrintEventArgs e)
		{
			OnWhsOrderToPrint?.Invoke(sender, e);
		}

		#endregion

		#endregion

		#region Constructors

		public WhsPick(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			NotificationManager = new NotificationManager();
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WP_FinalizedDateUtc), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WP_CriticalChangesVersionID), ConcurrencyPolicy.Ignore);
			RegisterDataRefresh();
		}

		#endregion

		#region Customs Stuff

		public bool IsCustomsTransaction => Orders.Count > 0 && Orders.Cast<WhsPickableDocket>().Any(o => o.IsCustomsTransaction);

		#endregion

		#region Business Object Overrides

		public override void Delete()
		{
			Orders.CountChanged -= OrdersCountChanged;

			OrderedInventories.RemoveAndDeleteAll();

			WorkflowItems.RemoveAndDeleteAll();

			DeleteLooseDockDoorAssignment();

			base.Delete();
		}

		void DeleteLooseDockDoorAssignment()
		{
			if (WP_WDA_DockDoorAssignment.IsValid)
			{
				var query = new ZQuery(WhsPickSchema.WP_WDA_DockDoorAssignment, WP_WDA_DockDoorAssignment);
				query.AddToFilter(WhsPickSchema.PK, SQLComparisonOperator.NotEqual, PK);
				if (!Factory.Exists(typeof(WhsPick), query))
				{
					DockDoorAssignment.DeleteLooseForPick();
				}
			}
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsPickFetchStrategy(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WP_PickStatus = PickStatus.Codes.Building;
		}

		public override void OnSaving()
		{
			base.OnSaving();

			UpdateTaskPlanningStatus();
			UpdateAllocateLogNote();

			if (!IsInDatabase && WP_PickType.IsEmpty)
			{
				WP_PickType = PickType.Codes.Order;
			}
		}

		void UpdateAllocateLogNote()
		{
			if (orderedInventories != null)
			{
				var inventoriesWithAllocationLogs = orderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(o => o.AvailableInventoriesWithAllocationLogs).ToArray();

				if (inventoriesWithAllocationLogs.Length > 0)
				{
					var noteText = GetAllocationLogText(inventoriesWithAllocationLogs);

					var allocationLog = Notes.FindByDescription(PredefinedNoteTypes.Instance.AllocationLog.Description).SingleOrDefault();
					if (allocationLog == null)
					{
						Notes.AddNew(false, PredefinedNoteTypes.Instance.AllocationLog.Description, noteText);
					}
					else
					{
						allocationLog.ST_NoteText += System.Environment.NewLine + System.Environment.NewLine + noteText;
					}
				}
			}
		}

		ZString GetAllocationLogText(WhsPickAvailableInventory[] inventories)
		{
			var result = new StringBuilder();
			result.AppendLine(Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow).ToString("[yyyy-MM-dd HH:mm:ss]") + ":");

			foreach (var inventory in inventories)
			{
				result.AppendLine(GetInventoryInfo(inventory));
				result.AppendLine(inventory.AllocationLog.Trim());
				inventory.AllocationLog = string.Empty;
			}

			return result.ToString().Trim();
		}

		ZString GetInventoryInfo(WhsPickAvailableInventory inventory)
		{
			var result = new StringBuilder(Res.GetString("f9a356ae-23e8-4974-ad69-0e8a5345a582", "Inventory Info:"));

			var inventoryView = inventory.Inventory[0];
			var orderedInventory = inventory.OrderedInventory;
			var isCustoms = inventoryView.Docket.WD_DocketSubType == ReceiveType.Codes.Customs;
			var customsData = isCustoms ? inventoryView.CustomsData : null;

			var inventoryInfos = new[]
			{
				(Caption: Res.GetString("833ceefa-009e-45e0-a3c5-02e300c8abf4", "Client:"), Value: inventoryView.Client?.OH_Code ?? ZString.Empty),
				(Caption: Res.GetString("f995d1f7-ea9a-4eb4-b2ae-752583ad7067", "Product:"), Value: inventoryView.Product?.Parent.OP_PartNum ?? ZString.Empty),
				(Caption: Res.GetString("92f2a37b-ac89-44f4-903d-0935257f717a", "Location:"), Value: inventoryView.Location?.WLV_LocationString ?? ZString.Empty),
				(Caption: Res.GetString("48459c01-4f9c-482b-bb68-cb8a4b91e2a6", "Arrival Date:"), Value: inventoryView.WI_ArrivalDate.IsValid ? inventoryView.WI_ArrivalDate.ToString("[yyyy-MM-dd HH:mm:ss]") : ZString.Empty),
				(Caption: Res.GetString("c4d4f250-05e8-4974-a777-9bc99effc72a", "Inventory Status:"), Value: inventoryView.WI_InventoryStatus),
				(Caption: Res.GetString("8113c727-6648-4507-9a80-cffba481faeb", "Pallet Id:"), Value: inventoryView.WI_PalletID),
				(Caption: Res.GetString("6d70b9af-aa18-4408-a6ef-5a398bfb5ee8", "Bonded Entry Key:"), Value: inventoryView.WI_BondedEntryKey),
				(Caption: Res.GetString("24b4aa16-c68a-48d1-8bda-ca959affad51", "Allocation Key:"), Value: inventoryView.WI_AllocationKey),
				(Caption: Res.GetString("e96fa749-4165-42c1-8c56-69dc720b3468", "Attribute 1:"), Value: inventoryView.WI_PartAttrib1),
				(Caption: Res.GetString("14acd5f3-9f40-41ea-ad68-d5edb0d513fc", "Attribute 2:"), Value: inventoryView.WI_PartAttrib2),
				(Caption: Res.GetString("0f0e5e50-fec2-494d-8439-01b005e6a5b8", "Attribute 3:"), Value: inventoryView.WI_PartAttrib3),
				(Caption: Res.GetString("accbbed5-fbf8-47b4-906f-b87d93fc1ce6", "Serial Number:"), Value: inventory.SerialNumber),
				(Caption: Res.GetString("4f3b9c61-915f-44d7-a37f-d15cc18dba53", "Packing Date:"), Value: inventoryView.WI_PackingDate.IsValid ? inventoryView.WI_PackingDate.ToString("[yyyy-MM-dd HH:mm:ss]") : ZString.Empty),
				(Caption: Res.GetString("a1958447-3169-44d2-8f13-0ff1f897d7c2", "Expiry Date:"), Value: inventoryView.WI_ExpiryDate.IsValid ? inventoryView.WI_ExpiryDate.ToString("[yyyy-MM-dd HH:mm:ss]") : ZString.Empty),
				(Caption: Res.GetString("d6f73f00-5dd5-4257-abf8-84b1ff98a9a9", "Customs Info: Package Group Id:"), Value: isCustoms ? inventoryView.PackageGroupId : ZString.Empty),
				(Caption: Res.GetString("7891324a-e7e0-4e54-8604-828909418cb6", "Per Package Qty:"), Value: isCustoms ? inventoryView.PerPackageQty.ToString() : ZString.Empty),
				(Caption: Res.GetString("1877ee7e-d473-4d5f-8c2e-2426fbabe537", "Value For Duty:"), Value: isCustoms ? customsData.WB_ValueForDuty.ToString() : ZString.Empty),
				(Caption: Res.GetString("a04ea12f-4802-44ac-81d5-4a2a3b0052d8", "Bonded Warehouse Qty:"), Value: isCustoms ? customsData.WB_BondedWhsQty.ToString() : ZString.Empty)
			};

			result.Append(string.Join(", ", inventoryInfos.Where(info => !info.Value.IsEmpty).Select(i => $"{i.Caption}{i.Value}")));

			return result.ToString().Trim();
		}

		void UpdateTaskPlanningStatus()
		{
			var lazyWarehouse = new Lazy<WhsWarehouse>(() => Warehouse);
			if (IsFinalisedOrCancelled)
			{
				WP_TaskPlanningStatus = string.Empty;
			}
			else if (WP_TaskPlanningStatus.IsEmpty || WP_TaskPlanningStatus == TaskPlanningStatus.Codes.NotReady)
			{
				if ((!IsInDatabase || WP_PickStatusInfo.HasChanges || WP_IsAwaitingReplenishmentInfo.HasChanges || WP_IsCartonisedInfo.HasChanges)
					&& WP_PickStatus != PickStatus.Codes.Building
					&& !WP_IsAwaitingReplenishment
					&& (!(WP_CartoniseSplitCases || WP_PickPalletsByLabel || WP_PickCasesByLabel) || WP_IsCartonised)
					&& (lazyWarehouse.Value?.WW_GG_ReleaseGroup.IsValid ?? false)
					&& WarehouseDataRegistry.Instance.AutomaticallySetPickTaskPlanningStatusToReadyForPlanning.GetFallBackValueAtAllLevels(Guid.Empty, lazyWarehouse.Value.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty))
				{
					WP_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				}
				else if (!IsInDatabase || WP_WW_WhsInfo.HasChanges)
				{
					WP_TaskPlanningStatus = (lazyWarehouse.Value?.WW_GG_ReleaseGroup.IsValid ?? false) ? TaskPlanningStatus.Codes.NotReady : string.Empty;
				}
			}
		}

		protected override void OnFactorySaving()
		{
			if (CannotSaveAfterFailedCancelOperation)
			{
				throw new ZCannotSaveException(
					ResString.GetMultilingualString("8e7314c9-c3f8-47ea-bf86-19efc736bf5b", "Cannot save Pick {0} as an error occurred during the Cancel Pick operation. Please reload the Pick.", WP_PickNo),
					ResString.GetMultilingualString("986b0c8e-8f91-490e-8ca8-1daf7ef26815", "Unable to Save"), ExceptionType.BusinessFailure);
			}

			base.OnFactorySaving();
			UpdatePickStatusIfRequired(updateWaitingReplenishmentStatus: false);

			// always reset the policy in case the pick is being saved again
			ConcurrencyInfo.SetConcurrencyPolicy(this, WhsPickSchema.Constants.WP_IsCartonised, ConcurrencyPolicy.Default);

			if (IsInDatabase && HasChangesForLock)
			{
				if (!WP_IsCartonised && !WP_IsCartonisedInfo.HasChanges)
				{
					if (CartonisationMutex.IsLocked && !CartonisationMutex.HasLock)
					{
						throw new ZCannotSaveException(
							"This Pick is in the process of having Package Labels allocated in another instance. Reload the Pick to make changes",
							"Package Labels Allocated", false, ExceptionType.BusinessFailure);
					}

					ConcurrencyInfo.SetConcurrencyPolicy(this, WhsPickSchema.Constants.WP_IsCartonised, ConcurrencyPolicy.Strict);
				}
			}

			// This cannot move to WhsPick.OnSaving() because the Pick won't necessarily have changes.
			// Not really an issue as we'd normally only have one pick loaded.
			GetPermitsForAttachedFTZOrderLines();
			ConfirmPermitsOfFTZOrderLinesIfPickFinalised();
		}

		bool HasChangesForLock => ((IBusiness)this).HasChangesNotIncludingChildren
			|| (orders != null && Orders.Any(o => o is WhsOrder order ? order.HasChangesForLock : ((IBusiness)o).HasChangesNotIncludingChildren));

		#region OnFactorySaving_TakeAllLocksAtOnce

		public const string WhsOnFactorySaving_TakeAllLocksAtOnce = "WhsOnFactorySaving_TakeAllLocksAtOnce";

		// This is still used in WebTracker
		public static Dictionary<ZGuid, IZType> OnFactorySaving_TakeAllLocksAtOnce(BusinessObjectFactory factory)
		{
			// quick and dirty solution to stop client's service tasks from crashing with deadlocks
			// to be replaced with better solution once we came up with it
			var picks = new List<WhsPick>();
			var orders = new List<WhsOrder>();
			foreach (var bizO in ((IBusinessObjectFactoryInternals)factory).AllBusinessObjects)
			{
				var pick = bizO as WhsPick;
				if (pick != null)
				{
					_ = pick.Orders; // poking Orders to register child editable for HasChanges
					if (bizO.IsInDatabase && pick.HasChangesForLock)
					{
						picks.Add(pick);
					}
				}
				else if (bizO is WhsOrder order && bizO.IsInDatabase && order.HasChangesForLock)
				{
					orders.Add(order);
				}
			}

			var result = new Dictionary<ZGuid, IZType>();
			foreach (var pick in picks.OrderBy(p => p.IsDeleted ? ZString.Empty : p.WP_PickNo))
			{
				var rawQuery = "SELECT WP_PK, WP_IsCartonised FROM dbo.WhsPick WHERE WP_PK = @PKValue";
				var @params = new ZSqlParameterCollection();
				@params.Add(ZSqlParameter.New("@PKValue", pick.PK, WhsPickSchema.PK));

				var resultSet = new DynamicBusinessObjectCollection(factory);
				resultSet.Load(rawQuery, @params);

				if (resultSet.Count == 1)
				{
					result.Add(new ZGuid(resultSet[0][WhsPickSchema.Constants.PK]), new ZBool(resultSet[0][WhsPickSchema.Constants.WP_IsCartonised]));
				}
			}

			foreach (var order in orders.OrderBy(o => o.IsDeleted ? ZString.Empty : o.WD_DocketID))
			{
				var rawQuery = "SELECT WD_PK, WD_WP FROM dbo.WhsDocket WITH(UPDLOCK, ROWLOCK) WHERE WD_PK = @PKValue";
				var @params = new ZSqlParameterCollection();
				@params.Add(ZSqlParameter.New("@PKValue", order.PK, WhsDocketSchema.PK));

				var resultSet = new DynamicBusinessObjectCollection(factory);
				resultSet.Load(rawQuery, @params);

				if (resultSet.Count == 1)
				{
					result.Add(new ZGuid(resultSet[0][WhsDocketSchema.Constants.PK]), new ZGuid(resultSet[0][WhsDocketSchema.Constants.WD_WP]));
				}
			}

			return result;
		}

		#endregion

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			if (IsFinalised && (!IsInDatabase || WP_PickStatusInfo.HasChanges))
			{
				CreateAutoInventoryAccuracyManagementTasks();
			}

			if (HasChanges)
			{
				AddFetchHintsForOrderWorkflows();
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}
		}

		void AddFetchHintsForOrderWorkflows()
		{
			foreach (var order in Orders)
			{
				Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.PK);
			}
			foreach (var workflow in Orders.Cast<WhsPickableDocket>().SelectMany(o => o.WorkflowItems).ToArray())
			{
				Factory.AddFetchHint(ProcessTaskNotificationSchema.PQ_P9, workflow.PK);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			try
			{
				if (!saveSucceeded)
				{
					RollbackPermits();
				}
			}
			finally
			{
				RollbackPermitsIfSaveFails = false;
			}

			AutoInventoryAccuracyManager.ClearAutoCreatedTasks(!saveSucceeded);
		}

		void RollbackPermits()
		{
			if (RollbackPermitsIfSaveFails)
			{
				var ftzOrderLines = OrdersRequiredCheckPermits
					.SelectMany(o => o.GetLinesToPick()).Cast<WhsOrderLine>().ToArray();

				if (ftzOrderLines.Length > 0)
				{
					int times = 0;
					const int retryTimes = 3;
					while (times < retryTimes && PermitService.RelinquishPermitTransactions(ftzOrderLines.Select(l => new WhsPermitTransactionDetail(l))) != SuccessOrFailure.Success)
					{
						times++;
					}
				}
			}
		}

		public void UpdatePickStatusIfRequired(bool updateWaitingReplenishmentStatus = true)
		{
			if (!IsDeleted && (HasChanges || !IsInDatabase))
			{
				var isFulfilmentRulesMet = IsFulfillmentRulesMet();

				if (WP_PickStatus == PickStatus.Codes.Building && isFulfilmentRulesMet)
				{
					WP_PickStatus = PickStatus.Codes.Created;
				}
				else if (!IsFinalisedOrCancelled && !isFulfilmentRulesMet)
				{
					WP_PickStatus = PickStatus.Codes.Building;
				}

				if (updateWaitingReplenishmentStatus && WP_IsAwaitingReplenishment)
				{
					var whsOrderedInventories = OrderedInventories.Cast<WhsPickOrderedInventory>().ToArray();
					AssignOrResetPickWaitingReplenishmentStatus(whsOrderedInventories, validateShortfall: false);
				}
			}
		}

		protected override WhsPickLookups GetNewLookups()
		{
			return new WhsPickLookups(this);
		}

		protected override WhsPickValidation GetNewValidation()
		{
			return new WhsPickValidation(this);
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		/// <summary>
		/// This will load all the Non-Persistent Release Lines for Validation on the Release Screen.
		/// It is important that the OrderedInventories is used to enumerate the Order Lines because they
		/// load appropriate fetch hints, they are always loaded for Pick Validation anyway.
		/// </summary>
		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			if (IsAlterPick) // this means the release form is open.
			{
				foreach (WhsPickOrderedInventory orderedInventory in OrderedInventories)
				{
					if (!orderedInventory.IsComponentOrderedInventoryOnSalesOrder)
					{
						foreach (var orderLine in orderedInventory.Owners)
						{
							orderLine.BuildReleaseLines();
						}
					}
				}
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return WP_PickNo.IsEmpty ? HumanReadableNameWithoutID : ZString.Format("{0} {1}", HumanReadableNameWithoutID, WP_PickNo); }
		}

		ZString HumanReadableNameWithoutID
		{
			get { return IsAlterPick ? Res.GetString("467E9180-D4E8-4CB1-B4C0-CE881CFB46B3", "Release") : Res.GetString("08A72371-3D82-47A1-ABC9-FE1840705291", "Pick"); }
		}

		#region SupportsClone

		protected override bool SupportsCloneCore() => true;

		#endregion

		#region GetPropertiesToExcludeFromCloning

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var propertiesToExcludeFromCloning = new List<string>(base.GetPropertiesToExcludeFromCloning());
			propertiesToExcludeFromCloning.Add(WhsPickSchema.Constants.WP_PickNo);
			propertiesToExcludeFromCloning.Add(WhsPickSchema.Constants.WP_IsAwaitingReplenishment);
			propertiesToExcludeFromCloning.Add(WhsPickSchema.Constants.WP_TaskPlanningStatus);

			return propertiesToExcludeFromCloning;
		}

		#endregion

		#endregion

		#region DataRefresh

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			base.OnBeforeUpdatedByDataRefresh();
			IsCancelledCache = IsCancelled;
			pickStatusBeforeUpdatedByDataRefresh = WP_PickStatus;
			pickStatusWasChangedBeforeUpdatedByDataRefresh = WP_PickStatusInfo.HasChanges;

			if (!WP_FinalizedDateUtc.IsEmpty)
			{
				pickFinalisedTimeBeforeUpdatedByDataRefresh = WP_FinalizedDateUtc;
				pickFinalisedByBeforeUpdatedByDataRefresh = WP_GS_NKFinalizedBy;
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			if (pickStatusBeforeUpdatedByDataRefresh != PickStatus.Codes.Finalised && WP_PickStatus == PickStatus.Codes.Finalised)
			{
				Orders.ForEach(o => o.RefreshBindingIncludingChildren());
			}

			if (IsCancelledCache != null && !IsCancelledCache.Value && IsCancelled)
			{
				ClearOrdersCache(); // Cancelling the order would have unpicked all Orders, so we should Refresh the collection
			}

			var row = ((IBusinessObjectInternals)this).Row;

			if (pickStatusWasChangedBeforeUpdatedByDataRefresh &&
				(pickStatusBeforeUpdatedByDataRefresh == PickStatus.Codes.Finalised || pickStatusBeforeUpdatedByDataRefresh == PickStatus.Codes.Cancelled))
			{
				row[WhsPickSchema.Constants.WP_PickStatus] = pickStatusBeforeUpdatedByDataRefresh;
			}

			if (pickFinalisedTimeBeforeUpdatedByDataRefresh.HasValue)
			{
				row[WhsPickSchema.Constants.WP_FinalizedDateUtc] = pickFinalisedTimeBeforeUpdatedByDataRefresh;
			}

			if (!pickFinalisedByBeforeUpdatedByDataRefresh.IsEmpty)
			{
				row[WhsPickSchema.Constants.WP_GS_NKFinalizedBy] = pickFinalisedByBeforeUpdatedByDataRefresh;
			}

			pickFinalisedTimeBeforeUpdatedByDataRefresh = null;
			pickFinalisedByBeforeUpdatedByDataRefresh = "";
			pickStatusWasChangedBeforeUpdatedByDataRefresh = false;
			IsCancelledCache = null;
		}

		ZBool? IsCancelledCache;
		ZString pickStatusBeforeUpdatedByDataRefresh;
		bool pickStatusWasChangedBeforeUpdatedByDataRefresh;
		ZDateTime? pickFinalisedTimeBeforeUpdatedByDataRefresh;
		ZString pickFinalisedByBeforeUpdatedByDataRefresh;

		#region DataRefreshSubscriber

		PickLinesDataRefreshBusSubscriber RegisterDataRefresh()
		{
			return new PickLinesDataRefreshBusSubscriber(Factory, this);
		}

		void IPickLineUpdatesRefreshable.Refresh(IEnumerable<WhsPickLine> pickLines)
		{
			var shouldUpdateTotalsByDataRefresh = false;
			if (orders != null && totalWeight != null)
			{
				var alreadyCheckedPKs = new HashSet<ZGuid>();
				shouldUpdateTotalsByDataRefresh = pickLines.Any(pl =>
				{
					var shouldUpdate = false;
					if (alreadyCheckedPKs.Add(pl.WZ_WE_TransactionLine))
					{
						var docketLine = pl.DocketLine;
						shouldUpdate = alreadyCheckedPKs.Add(docketLine.WE_WD) && docketLine.Docket.WD_WP == PK;
					}

					return shouldUpdate;
				});
			}

			if (shouldUpdateTotalsByDataRefresh)
			{
				CalculateOrderTotals();
			}
		}

		#endregion

		#endregion

		#region GetPermitsForAttachedOrderLines

		void GetPermitsForAttachedFTZOrderLines()
		{
			if ((!IsInDatabase || !IsFinalised || WP_PickStatusInfo.HasChanges) // WP_PickStatusInfo.HasChanges will mean FIN in memory
				&& !IsWorkOrderPick)
			{
				var ordersToCheck = OrdersRequiredCheckPermits.ToArray();
				var ftzOrderLines = ordersToCheck.SelectMany(o => o.GetLinesToPick()).Cast<WhsOrderLine>().ToArray();
				var permitAquiredResult = FTZOrderPermitManager.TryAquireFTZPermit(ordersToCheck, ftzOrderLines);

				if (!permitAquiredResult.IsSuccess)
				{
					var exceptionMessage = Res.GetString("2590c2ed-50f6-42ce-9042-a50d72ef902b",
							"{0}\r\n\r\nClose and reopen the form before making changes to the Order.", permitAquiredResult.ErrorMessage);

					throw new ZCannotSaveException(exceptionMessage,
						Res.GetString("7b75f1a2-18cc-46ce-974d-6b33ceb293ff", "Each Order Line must have a weekly estimate."), ExceptionType.BusinessFailure);
				}
				else
				{
					RollbackPermitsIfSaveFails = true;
				}
			}
		}

		IEnumerable<WhsOrder> OrdersRequiredCheckPermits => Orders.Cast<WhsOrder>().Where(o => o.WD_DocketSubType == OrderType.Codes.CustomsReleaseWithPermit && (!o.IsInDatabase || o.WD_WPInfo.HasChanges) && o.IsPickingFTZCustomsOrderWithPermit);

		IPermitService PermitService => ObjectFactory.Get<IPermitService>();

		bool RollbackPermitsIfSaveFails;

		#endregion

		#region ConfirmPermitsOfFTZOrderLinesIfPickFinalised

		void ConfirmPermitsOfFTZOrderLinesIfPickFinalised()
		{
			if (IsFinalised
				&& (!IsInDatabase || WP_PickStatusInfo.HasChanges)
				&& !IsWorkOrderPick)
			{
				var permitTransactions = Orders.Cast<WhsOrder>()
					.Where(o => o.IsPickingFTZCustomsOrderWithPermit)
					.SelectMany(o => o.GetLinesToPick()).Cast<WhsOrderLine>()
					.Select(l => new WhsPermitWithdrawalRequestDetail(l, l.PickLineQuantity)).ToArray();

				if (permitTransactions.Length > 0 && !PermitService.ConfirmPermitTransactions(permitTransactions))
				{
					throw new ZCannotSaveException(ResString.GetMultilingualString("1548D1FC-A89B-41C6-8ABD-1284ADBD487B", "The Order(s) you detached could not confirm its Permits. Close and Re-open the form and try again."),
						Res.GetString("5677157B-CFE1-4257-A456-F5A60816CFF9", "Confirm Permits Failed."), ExceptionType.BusinessFailure);
				}
			}
		}

		#endregion

		#region Related Entities

		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(WP_WW_Whs);

		#region OuterPackages

		public WhsPackageCollection OuterPackages => outerPackages ?? (outerPackages = new WhsPackageCollection(Factory, GetOuterPackagesQuery()));

		ZQuery GetOuterPackagesQuery()
		{
			ZQuery query;

			if (IsWorkOrderPick)
			{
				query = ZQuery.NoResultQuery;
			}
			else
			{
				query = new ZQuery(PkgPackageSchema.KP_KP_ParentPackage, null);
				query.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, Orders.Cast<WhsOrder>().Select(o => o.PackageJob).WhereNotNull().Select(pj => pj.PK).ToArray());
			}

			return query;
		}

		#endregion

		[ChildEditable(true)]
		public WhsLegacyPickableDocketCollection Orders
		{
			get
			{
				if (orders == null)
				{
					var query = new ZQuery(WhsDocketSchema.WD_WP, PK);
					query.FetchOnlyFromLocalCache = !IsInDatabase;

					var lorders = new WhsLegacyPickableDocketCollection(Factory, query);
					lorders.Load();
					orders = lorders;

					foreach (WhsPickableDocket order in orders)
					{
						Factory.AddFetchHint(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD, order.PK);
					}

					orders.CountChanged += new CollectionCountChangedEventHandler(OrdersCountChanged);
					RegisterEditableChildObject(orders);
				}
				return orders;
			}
		}

		[ChildEditable(true)]
		public WhsTransferOnPickCollection Transfers => transfers ?? (transfers = GetTransfersCollection());
		WhsTransferOnPickCollection transfers;

		WhsTransferOnPickCollection GetTransfersCollection()
		{
			var collection = new WhsTransferOnPickCollection(this);
			RegisterEditableChildObject(collection);
			return collection;
		}

		public ZByte PickPriority
		{
			get
			{
				var orderPriorities = Orders.Cast<WhsPickableDocket>().Select(o => o.WD_PickPriority).Where(p => !p.IsEmpty);
				return orderPriorities.Any() ? orderPriorities.Min() : ZByte.Zero;
			}
			set
			{
				foreach (WhsPickableDocket order in Orders)
				{
					order.WD_PickPriority = value;
				}
			}
		}

		public void ClearOrdersCache()
		{
			if (orders != null)
			{
				orders.CountChanged -= new CollectionCountChangedEventHandler(OrdersCountChanged);
				UnRegisterEditableChildObject(orders);
				orders = null;
			}
		}

		public WhsOrder CurrentOrder
		{
			get { return CurrentPickableDocket as WhsOrder; }
		}

		public WhsPickableDocket CurrentPickableDocket
		{
			get
			{
				WhsPickableDocket result = null;
				if (Orders.Count == 1)
				{
					result = Orders[0];
				}
				else if (ParentForm != null)
				{
					result = ParentForm.CurrentOrder;
				}
				return result;
			}
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				NoteTypeCollection types = base.NoteTypesCore;
				types.Add(PredefinedNoteTypes.Instance.ClientVisibleJobNotes);
				types.Add(PredefinedNoteTypes.Instance.DeliveryInstructionsNote);
				types.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
				types.Add(PredefinedNoteTypes.Instance.PickingInstructions);
				types.Add(PredefinedNoteTypes.Instance.AllocationLog);
				return types;
			}
		}

		#region OrderedInventories

		[ChildEditable(true)]
		public WhsPickOrderedInventoryCollection OrderedInventories
		{
			get
			{
				if (orderedInventories == null)
				{
					orderedInventories = new WhsPickOrderedInventoryCollection(Factory, this);
					BuildOrderedInventories();
				}
				else
				{
					RebuildOrderedInventoriesIfNecessary();
				}

				return orderedInventories;
			}
		}

		#endregion

		WhsPickOrderedInventory[] GetOrderedInventoriesWithOwners() => OrderedInventories.Cast<WhsPickOrderedInventory>().Where(oi => oi.Owners.Count > 0).ToArray();

		internal void RebuildOrderedInventoriesIfNecessary()
		{
			if (OrderedInventoriesNeedRefresh)
			{
				orderedInventories.ClearCollectionAndRelatedCache();
				BuildOrderedInventories();
				orderedInventories.RefreshBinding();
			}
		}

		void BuildOrderedInventories()
		{
			using (orderedInventories.SuspendRefreshingCollection())
			{
				RegisterEditableChildObject(orderedInventories);
				BuildOrderedInventoriesCore();
			}
		}

		void BuildOrderedInventoriesCore()
		{
			AddFetchHints(Orders.Cast<WhsPickableDocket>());

			using (orderedInventories.SuspendListChanged())
			using (orderedInventories.DeferValidationItemsOnAddOrRemove())
			{
				foreach (WhsPickableDocket pickableDocket in Orders)
				{
					orderedInventories.AddNewFromOrder(pickableDocket);
				}

				OrderedInventoriesNeedRefresh = false;
			}

			foreach (WhsPickOrderedInventory orderedInventory in orderedInventories)
			{
				// tested by AllocatePicksAwaitingReplenishmentWithExistingInventoryProcessingManagerTest.TestDBHitsForPickAllocation_WorkOrder
				var query = new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, orderedInventory.Owners.Select(o => o.PK));
				Factory.AddFetchHint(WhsPickLineSchema.Instance, query);
			}

			orderedInventories.Sort(new SortItemsForPicking());
		}

		internal bool OrderedInventoriesNeedRefresh { get; set; }

		void AddFetchHints(IEnumerable<WhsPickableDocket> whsOrders)
		{
			foreach (var order in whsOrders)
			{
				Factory.AddFetchHint(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID, order.PK);
				if (IsMultiOrderPick) // fetch hint below is important only for multiorder picks. When order loads its lines we have a fetch hint for picklines as well.
				{
					foreach (var orderLine in order.GetLinesToPick())
					{
						Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_TransactionLine, orderLine.PK);
					}
				}
			}

			foreach (var clientPk in whsOrders.Select(o => o.WD_OH_Client).Distinct())
			{
				Factory.AddFetchHint(OrgHeaderSchema.PK, clientPk);
				Factory.AddFetchHint(OrgMiscServSchema.OM_OH, clientPk);
				Factory.AddFetchHint(OrgCustomLabelsSchema.OT_OH, clientPk);
				Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, clientPk);
				Factory.AddFetchHint(OrgContactSchema.OC_OH, clientPk);
				Factory.AddFetchHint(OrgAddressSchema.OA_OH, clientPk);
			}
		}

		#endregion

		#region GetPickTypeToMatchDockets

		public static string GetPickTypeToMatchDockets(IReadOnlyList<WhsPickableDocket> dockets)
		{
			string result;

			var firstDocketType = dockets[0].WD_DocketType;

			result = (string)firstDocketType switch
			{
				DocketType.Codes.WorkOrder => PickType.Codes.WorkOrder,
				DocketType.Codes.DynamicWorkOrder => PickType.Codes.DynamicWorkOrder,
				DocketType.Codes.Order => dockets.FirstOrDefault(d => d.Lines.Count > 0)?.IsHeldInventoryOrder ?? false
					? PickType.Codes.HeldInventoryOrder
					: PickType.Codes.Order,
				_ => throw new ArgumentException($"Invalid docket type {firstDocketType}, cannot determine pick type"),
			};

			return result;
		}

		#endregion

		#region ClearPickLinesCacheForInventory

		internal void ClearPickLinesCacheForInventory(WhsPickableDocketLine line, ZGuid inventoryLinePK)
		{
			if (orderedInventories != null)
			{
				var orderedInventoryFromPickableDocketLine = orderedInventories.GetOrderedInventoryForLine(line);
				Argument.NotNull(orderedInventoryFromPickableDocketLine, nameof(orderedInventoryFromPickableDocketLine));

				orderedInventoryFromPickableDocketLine.ClearPickLinesCacheForInventory(inventoryLinePK);
			}
		}

		#endregion

		#region ClearOrderedInventoriesCache

		public void ClearOrderedInventoriesCache()
		{
			if (orderedInventories != null)
			{
				UnRegisterEditableChildObject(orderedInventories);
				orderedInventories = null;
			}
		}

		#endregion

		#region ClearInventoryCache

		/// <summary>
		/// The data refresh bus is turned off for Inventory. Calling this method will force a
		/// reload/resort of each OrderedInventories[n].Inventory collection.
		/// </summary>
		public void ClearInventoryCache()
		{
			ClearAllInventoryAndAvailableInventoryCache();
		}

		void ClearAllInventoryAndAvailableInventoryCache()
		{
			ClearAllInventoriesCache();
			foreach (WhsPickOrderedInventory orderedInventory in OrderedInventories)
			{
				orderedInventory.ClearAvailableInventoriesCache();
			}
		}

		#endregion

		#region GetFilteredInventories

		internal IEnumerable<WhsInventoryView> GetFilteredInventories(ZQuery query)
		{
			var inventoryQuery = GetAllInventoriesQuery();
			inventoryQuery.AddToFilter(query);

			var inventories = Factory.Load<WhsInventoryView>(inventoryQuery);
			inventories.ForEach(AddFetchHintForDocketRelatedToInventory);
			return inventories;
		}

		#endregion

		#region AllInventories

		public IEnumerable<WhsInventoryView> AllInventories
		{
			get
			{
				if (allInventories == null)
				{
					allInventories = Factory.Load<WhsInventoryView>(GetAllInventoriesQuery());
					allInventories.ForEach(AddFetchHintForDocketRelatedToInventory);
				}

				return allInventories;
			}
		}

		void AddFetchHintForDocketRelatedToInventory(WhsInventoryView inventory)
		{
			Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, inventory.WI_WE_InDocketLine);
			Factory.AddFetchHint(WhsDocketSchema.PK, inventory.WI_WD);
			Factory.AddFetchHint(WhsDocketLineSchema.PK, inventory.WI_WE_InDocketLine);

			if (IsCustomsTransaction)
			{
				Factory.AddFetchHint(WhsBondedWarehouseAttributeSchema.WB_ParentID, inventory.PK);
			}

			if (IsWorkOrderPick && Orders[0] is WhsComponentOrder componentOrder && !componentOrder.IsAssembly)
			{
				Factory.AddFetchHint(WhsBOMInventoryPivotSchema.WIP_WE_InventoryLine, inventory.WI_WE_OriginalInDocketLineForRating);
			}
		}

		public void ClearAllInventoriesCache()
		{
			// to make Factory reload WhsInventoryView collections from DB rather then from local cache we have to do this
			Factory.ClearQueryCache(WhsInventoryViewSchema.Constants.TableName);
			if (allInventories != null)
			{
				allInventories = null;
			}

			if (InventoryCache != null)
			{
				InventoryCache = null;
			}
		}

		public void RemoveDeletedPickByBOMInventoryFromCaches(WhsDocketLine deletedInventory, ZGuid partPK, ZGuid clientPK)
		{
			if (!deletedInventory.IsDeleted)
			{
				throw new ArgumentException("We can only remove deleted inventory from the caches.");
			}

			if (allInventories != null)
			{
				allInventories = allInventories.Where(inv => inv.PK != deletedInventory.PK).ToArray();
			}

			if (InventoryCache != null && InventoryCache.TryGetValue((partPK, clientPK), out var inventoryCache))
			{
				inventoryCache.RemoveInventoryFromAll(deletedInventory.PK);
			}
		}

		ZQuery GetAllInventoriesQuery()
		{
			return IsFinalised || IsFinalising ? GetInventoriesQueryForFinalisedPick() : GetInventoriesQueryForNonFinalisedPick(Orders.ToArray<WhsPickableDocket>());
		}

		/// If pick is finalised we only need to load inventory records that are attached to the pick lines
		ZQuery GetInventoriesQueryForFinalisedPick()
		{
			var sqlParams = new ZSqlParameterCollection { { "@Pick", PK, WhsDocketSchema.WD_WP } };

			var result = new ZDBOnlyQuery(typeof(WhsInventoryView));

			// To achieve the same result with ZQueries is quite complicated and not as performant, therefore we use Direct SQL
			result.AddFilterAndZSQLParameterCollection(@"
WI_WE_InDocketLine IN
(
	SELECT
		ISNULL(WZ_WE_OriginalPickedInventoryLine, WZ_WE_InventoryLine)
	FROM dbo.WhsPickLine
	WHERE
		WZ_WE_TransactionLine IN
		(
			SELECT WE_PK
			FROM dbo.WhsDocketLine
			WHERE
				WE_WD IN
				(
					SELECT WD_PK
					FROM dbo.WhsDocket
					WHERE WD_WP = @Pick
				)
		)
)", sqlParams);

			return result;
		}

		/// If pick is not finalised we need to load all the available inventory for this product to use for allocation
		ZQuery GetInventoriesQueryForNonFinalisedPick(WhsPickableDocket[] ordersForQuery, ZGuid[] products = null)
		{
			var allClients = GetAllClients(ordersForQuery);
			var allSupplierParts = products ?? GetAllSupplierParts(ordersForQuery);
			var allHeldCodes = WarehouseDataRegistry.Instance.EnableHeldGoodsForOrders.Value
				? ordersForQuery.SelectMany(orders => orders.Lines).Where(line => !line.WE_WHC_NKOrderedHeldCode.IsEmpty).Select(line => line.WE_WHC_NKOrderedHeldCode).Distinct().ToArray()
				: [];

			var orderTypes = GetOrderTypes();
			var warehouse = Warehouse;
			var needsDbOnlyQuery = warehouse != null || !orderTypes.HasCustomsOrder || !orderTypes.HasInwardProcessingOrders || orderTypes.HasDisassemblyComponentOrder;
			var result = !needsDbOnlyQuery ? new ZQuery() : new ZDBOnlyQuery(typeof(WhsInventoryView));

			if (allClients.Length > 0)
			{
				result.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, allClients);
			}

			if (allSupplierParts.Length > 0)
			{
				result.AddToFilter(WhsInventoryViewSchema.WI_OP, allSupplierParts);
			}

			AddQuantityOrAllocationFilter(result);
			AddPartAttributesFilter(result, ordersForQuery);

			var locationFilter = new Lazy<ZDBOnlySubQuery>(() => new ZDBOnlySubQuery(typeof(WhsLocation), WhsInventoryViewSchema.WI_WL));

			if (warehouse != null)
			{
				locationFilter.Value.AddToFilter(WhsLocationViewSchema.WLV_WW_Whs, warehouse.PK);

				if (InventoriesFilterDate.HasValue)
				{
					var orderLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsInventoryViewSchema.WI_WE_InDocketLine);
					orderLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_FinalisedDate, SQLComparisonOperator.LessThanOrEqualTo, InventoriesFilterDate.Value);
					((ZDBOnlyQuery)result).AddSubQuery(orderLineSubQuery, JoinCondition.And);
				}
			}

			if (allHeldCodes.Length > 0)
			{
				result.AddToFilter(WhsInventoryViewSchema.WI_HeldCode, allHeldCodes);
				locationFilter.Value.AddToFilter(WhsLocationViewSchema.WLV_LocationClass, SQLComparisonOperator.NotEqual, new [] { LocationClasses.Codes.FIX, LocationClasses.Codes.DPF });
			}

			AddAreaFilters();

			if (needsDbOnlyQuery)
			{
				((ZDBOnlyQuery)result).AddSubQuery(locationFilter.Value, JoinCondition.And);

				if (orderTypes.HasDisassemblyComponentOrder)
				{
					AddFilterToIgnoreKitsWithSecondaryProducts((ZDBOnlyQuery)result, warehouse.PK, allClients, allSupplierParts);
				}
			}

			if (ordersForQuery.Length > 0)
			{
				result.AddToFilter(GetInventoriesQueryForPickByBOMKitLines(ordersForQuery), JoinCondition.Union);
			}

			result.OrderBy = WhsInventoryViewSchema.WI_ArrivalDate.Name;

			return result;

			(bool HasCustomsOrder, bool HasInwardProcessingOrders, bool HasNonCustomsOrders, bool HasDisassemblyComponentOrder) GetOrderTypes()
			{
				var hasCustomsOrder = false;
				var hasInwardProcessingOrders = false;
				var hasNonCustomsOrder = false;
				var hasDisassemblyComponentOrder = false;

				foreach (var order in ordersForQuery)
				{
					var isCustomsTransaction = order.IsCustomsTransaction;
					var isInwardProcessingJob = order.WD_IsInwardsProcessingJob;

					hasCustomsOrder |= isCustomsTransaction && !isInwardProcessingJob;
					hasInwardProcessingOrders |= isInwardProcessingJob;
					hasNonCustomsOrder |= !isCustomsTransaction;

					hasDisassemblyComponentOrder |= order is WhsComponentOrder componentOrder && !componentOrder.IsAssembly;
				}

				return (hasCustomsOrder, hasInwardProcessingOrders, hasNonCustomsOrder, hasDisassemblyComponentOrder);
			}

			void AddAreaFilters()
			{
				if (!orderTypes.HasInwardProcessingOrders)
				{
					locationFilter.Value.AddToFilter(WhsLocationViewSchema.WLV_PickingAreaType, SQLComparisonOperator.NotEqual, AreaTypes.Codes.InwardProcessing);
				}

				if (!orderTypes.HasCustomsOrder)
				{
					if (!orderTypes.HasNonCustomsOrders)
					{
						locationFilter.Value.AddToFilter(WhsLocationViewSchema.WLV_PickingAreaType, SQLComparisonOperator.Equal, AreaTypes.Codes.InwardProcessing);
					}
					else
					{
						locationFilter.Value.AddToFilter(WhsLocationViewSchema.WLV_PickingAreaType, SQLComparisonOperator.NotEqual, AreaTypes.Codes.Bonded);
					}
				}
			}
		}

		static ZQuery GetTVPQuery<T>(SchemaColumn column, IEnumerable<T> values)
		{
			var query = new ZQuery { AllowTableValuedParameters = true };
			query.AddToFilter(column, values);
			return query;
		}

		ZQuery GetInventoriesQueryForPickByBOMKitLines(WhsPickableDocket[] ordersForQuery)
		{
			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsOrderLine), WhsDocketLineSchema.PK);
			docketLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_WD, ordersForQuery.Select(o => o.PK));

			var bomLinkSubQuery = new ZDBOnlySubQuery(typeof(WhsBOMInventoryPivot), WhsBOMInventoryPivotSchema.WIP_WE_InventoryLine);
			bomLinkSubQuery.AddSubQuery(WhsBOMInventoryPivotSchema.WIP_WE_ComponentLine, docketLineSubQuery, JoinCondition.And);

			var inventoryQuery = new ZDBOnlyQuery(typeof(WhsInventoryView));
			inventoryQuery.AddSubQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, bomLinkSubQuery, JoinCondition.And);

			return inventoryQuery;
		}

		WhsInventoryView[] allInventories;

		#region GetAllClients

		ZGuid[] GetAllClients(IEnumerable<WhsPickableDocket> ordersForQuery)
			=> ordersForQuery.Select(o => o.WD_OH_Client).Distinct().ToArray();

		#endregion

		#region GetAllSupplierParts

		ZGuid[] GetAllSupplierParts(IEnumerable<WhsPickableDocket> ordersForQuery)
			=> ordersForQuery.SelectMany(o => o.AllLines.Select(l => l.WE_OP).Where(op => op.IsValid)).Distinct().ToArray();

		#endregion

		#region AddQuantityOrAllocationFilter

		/// <summary>
		/// Filter all inventory with WI_TotalUnits > 0 and also All Inventory that is attached to All PickLines of All Orders.
		/// This is to alleviate a bug whereby inventory goes negative and previously could not be seen/fixed by the user (we had to datafix).
		/// </summary>
		void AddQuantityOrAllocationFilter(ZQuery filter)
		{
			var quantityOrAllocationQuery = new ZQuery();
			quantityOrAllocationQuery.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);

			var allocationQuery = GetTVPQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, GetAllPickLines().Select(pickLine => pickLine.InventoryLinePKForAvailableInventory));
			quantityOrAllocationQuery.AddToFilter(allocationQuery, JoinCondition.Or);
			filter.AddToFilter(quantityOrAllocationQuery);
		}

		#endregion

		#region AddPartAttributesFilter

		void AddPartAttributesFilter(ZQuery query, IEnumerable<WhsPickableDocket> ordersForQuery)
		{
			GetAllAttributes(out var allPartAttributes1, out var allPartAttributes2, out var allPartAttributes3, out var allExpiryDates, out var allPackingDates, ordersForQuery);

			if (allPartAttributes1 != null)
			{
				query.AddToFilter(GetTVPQuery(WhsInventoryViewSchema.WI_PartAttrib1, allPartAttributes1.ToArray()));
			}

			if (allPartAttributes2 != null)
			{
				query.AddToFilter(GetTVPQuery(WhsInventoryViewSchema.WI_PartAttrib2, allPartAttributes2.ToArray()));
			}

			if (allPartAttributes3 != null)
			{
				query.AddToFilter(GetTVPQuery(WhsInventoryViewSchema.WI_PartAttrib3, allPartAttributes3.ToArray()));
			}

			if (allExpiryDates != null)
			{
				query.AddToFilter(GetTVPQuery(WhsInventoryViewSchema.WI_ExpiryDate, allExpiryDates.ToArray()));
			}

			if (allPackingDates != null)
			{
				query.AddToFilter(GetTVPQuery(WhsInventoryViewSchema.WI_PackingDate, allPackingDates.ToArray()));
			}
		}

		void GetAllAttributes(out HashSet<ZString> allPartAttributes1, out HashSet<ZString> allPartAttributes2, out HashSet<ZString> allPartAttributes3, out HashSet<ZDateTime> allExpiryDates, out HashSet<ZDateTime> allPackingDates, IEnumerable<WhsPickableDocket> ordersForQuery)
		{
			allPartAttributes1 = new HashSet<ZString>();
			allPartAttributes2 = new HashSet<ZString>();
			allPartAttributes3 = new HashSet<ZString>();
			allExpiryDates = new HashSet<ZDateTime>();
			allPackingDates = new HashSet<ZDateTime>();

			var hasEmptyAttribute1 = false;
			var hasEmptyAttribute2 = false;
			var hasEmptyAttribute3 = false;
			var hasEmptyExpiry = false;
			var hasEmptyPacking = false;

			foreach (var order in ordersForQuery)
			{
				foreach (var orderLine in order.AllLines)
				{
					if (!hasEmptyAttribute1)
					{
						hasEmptyAttribute1 = AddUniqueToCollectionWithEmptyValueCheck(ref allPartAttributes1, orderLine.WE_PartAttrib1.ToUpper());
					}

					if (!hasEmptyAttribute2)
					{
						hasEmptyAttribute2 = AddUniqueToCollectionWithEmptyValueCheck(ref allPartAttributes2, orderLine.WE_PartAttrib2.ToUpper());
					}

					if (!hasEmptyAttribute3)
					{
						hasEmptyAttribute3 = AddUniqueToCollectionWithEmptyValueCheck(ref allPartAttributes3, orderLine.WE_PartAttrib3.ToUpper());
					}

					if (!hasEmptyExpiry)
					{
						hasEmptyExpiry = AddUniqueToCollectionWithEmptyValueCheck(ref allExpiryDates, orderLine.WE_ExpiryDate);
					}

					if (!hasEmptyPacking)
					{
						hasEmptyPacking = AddUniqueToCollectionWithEmptyValueCheck(ref allPackingDates, orderLine.WE_PackingDate);
					}

					// all attribs contain at least one empty value, no point enumerating further
					if (hasEmptyAttribute1 && hasEmptyAttribute2 && hasEmptyAttribute3 && hasEmptyExpiry && hasEmptyPacking)
					{
						return;
					}
				}
			}
		}

		// Add all unique values of the attribute into collection for future load from DB. If exist attribute with empty value, than we would need to load all posible inventories for this attribute,
		// and all other values has no meaning to us.
		bool AddUniqueToCollectionWithEmptyValueCheck<T>(ref HashSet<T> collection, T newItem)
			where T : IZType
		{
			bool result;

			if (newItem.IsEmpty)
			{
				collection = null;
				result = true;
			}
			else
			{
				collection.Add(newItem);
				result = false;
			}

			return result;
		}

		#endregion

		#region AddFilterToIgnoreKitsWithSecondaryProducts

		void AddFilterToIgnoreKitsWithSecondaryProducts(ZDBOnlyQuery query, ZGuid warehousePK, ZGuid[] allClients, ZGuid[] allSupplierParts)
		{
			var rawSql = $@"
	SELECT
		WI_PK
	FROM 
		dbo.WhsInventoryView as Inventory
		JOIN dbo.WhsDocketLine as InDocketLine on WI_WE_InDocketLine = InDocketLine.WE_PK 
		JOIN dbo.WhsBOMInventoryPivot as mainProductPivot on mainProductPivot.WIP_WE_InventoryLine = InDocketLine.WE_WE_OriginalDocketLineForRating
		JOIN dbo.WhsDocketLine as ComponentLine on mainProductPivot.WIP_WE_ComponentLine = ComponentLine.WE_PK
		JOIN dbo.WhsDocketLine as ComponentOrderLine on ComponentOrderLine.WE_PK = ComponentLine.WE_WE_ParentDocketLine
		JOIN dbo.WhsDocketLine as ChildComponentLines on ChildComponentLines.WE_WE_ParentDocketLine = ComponentOrderLine.WE_PK
		JOIN dbo.WhsBOMInventoryPivot as ChildPivot on ChildPivot.WIP_WE_ComponentLine = ChildComponentLines.WE_PK
		JOIN dbo.WhsDocketLine as ChildInventoryLine on ChildPivot.WIP_WE_InventoryLine = ChildInventoryLine.WE_PK
	WHERE
		ChildInventoryLine.WE_OP <> Inventory.WI_OP
		AND WI_TotalUnits > 0
		AND WI_WW_Whs = @WhsPK
		AND WI_OH_Client IN (SELECT value FROM @ClientPKs)
		AND WI_OP IN (SELECT value FROM @PartPKs)

	UNION ALL

	SELECT
		WI_PK
	FROM 
		dbo.WhsInventoryView as Inventory
		JOIN dbo.WhsBondedWarehouseAttribute as CustomsData on Inventory.WI_PK = CustomsData.WB_ParentID AND CustomsData.WB_ParentTableCode = 'WE'
	WHERE
		WB_IsSecondaryInwardsProcessedItem = 1
		AND WI_TotalUnits > 0
		AND WI_WW_Whs = @WhsPK
		AND WI_OH_Client IN (SELECT value FROM @ClientPKs)
		AND WI_OP IN (SELECT value FROM @PartPKs)";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WhsPK", warehousePK, WhsInventoryViewSchema.WI_WW_Whs);
			sqlParams.Add(ZSqlParameter.New("@ClientPKs", allClients, WhsInventoryViewSchema.WI_OH_Client, true));
			sqlParams.Add(ZSqlParameter.New("@PartPKs", allSupplierParts, WhsInventoryViewSchema.WI_OP, true));

			query.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, "{0} NOT IN ({1})", WhsInventoryViewSchema.PK.Name, rawSql), sqlParams);
		}

		#endregion

		#endregion

		#region GetAllPickLines

		public IEnumerable<WhsPickLine> GetAllPickLines()
		{
			return OrderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(o => o.Owners).SelectMany(l => l.PickLines);
		}

		#endregion

		#region CurrentPickStrategy

		public PickStrategy CurrentPickStrategy
		{
			get { return pickStrategy ?? (pickStrategy = GetNewPickStrategy()); }
		}

		PickStrategy pickStrategy;

		PickStrategy GetNewPickStrategy() => Warehouse.IsBondedEnabledAndUSJurisdiction() ? new PickStrategyUS(this) : new PickStrategy(this);

		#endregion

		#region DockDoorLocation

		public WhsLocation DockDoorLocation => DockDoorPK.IsValid ? Factory.Load<WhsLocation>(DockDoorPK) : null;

		[ReadOnlyMember(nameof(IsDockDoorReadOnly))]
		[List("Lookups.DockDoorLocations")]
		[MaxLength(WhsDocketLine.Schema.LocationStringMaxLength)]
		public ZString DockDoorLocationString
		{
			get => DockDoorLocation?.ToLocationString() ?? dockDoorLocationString;
			set
			{
				SetNonPersistentPropertyValue(DockDoorLocationStringInfo, ref dockDoorLocationString, value);
				DockDoorPK = GetLocationPK(Warehouse, value);
			}
		}

		ZString dockDoorLocationString;

		ZGuid GetLocationPK(WhsWarehouse whs, ZString ddlString) => ddlString.IsEmpty ? ZGuid.Empty : whs?.FindLocation(ddlString)?.PK ?? ZGuid.Invalid;

		public ZPropertyInfo DockDoorLocationStringInfo => GetWrappedZPropertyInfo(Schema.DockDoorLocationString, _ => DockDoorPKInfo);

		#endregion

		#region DockDoorAssignment

		public WhsDockDoorAssignment DockDoorAssignment => WP_WDA_DockDoorAssignment.IsValid ? Factory.Load<WhsDockDoorAssignment>(WP_WDA_DockDoorAssignment) : null;

		#endregion

		#region PackingStationLocation

		public WhsLocation PackingStationLocation => WP_WL_PackingStation.IsValid ? Factory.Load<WhsLocation>(WP_WL_PackingStation) : null;

		#endregion

		#region DynamicPickAreaOverride

		public WhsArea DynamicPickAreaOverride => WP_WA_DynamicPickAreaOverride.IsValid ? Factory.Load<WhsArea>(WP_WA_DynamicPickAreaOverride) : null;

		#endregion

		#region Order Attachment

		#region AddOrders

		public void AddOrders(IEnumerable<WhsPickableDocket> ordersToAdd)
		{
			var ordersToAddEvaluated = ordersToAdd.ToArray();

			var existingProducts = OrderedInventories.Cast<WhsPickOrderedInventory>().Select(ordInv => ordInv.SupplierPartPK).Distinct().ToArray();
			var productsOnNewOrders = new HashSet<ZGuid>();

			var orderPKs = ordersToAddEvaluated.Select(o => o.PK).ToArray();
			Factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.WE_WD, orderPKs));
			Factory.AddFetchHint(PkgPackageJobSchema.Instance, new ZQuery(PkgPackageJobSchema.KJ_ParentID, orderPKs));
			Factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, ordersToAddEvaluated.SelectMany(o => o.GetLinesToPick()).Select(l => l.PK)));

			SetPickPropertiesFromOrders(ordersToAddEvaluated);

			using (new IsAttachingPickSetter(ordersToAddEvaluated))
			{
				var newOrderPks = new HashSet<ZGuid>();

				using (Orders.SuspendListChanged())
				using (OrderedInventories.DeferValidationItemsOnAddOrRemove())
				using (new SemaphoreManager(BulkAttachingPickSemaphore))
				// since cross dock allocation is also handled when attaching orders (through count changed event)
				// we will suspend it here as we want to allocate reserved stock at the end when we have rolled up
				// all orders into ordered inventory.
				using (new SemaphoreManager(SuspendCrossDockAllocationSemaphore))
				{
					foreach (var order in ordersToAddEvaluated)
					{
						if (!Orders.Contains(order))
						{
							Orders.Add(order);

							if (order.WD_WP == PK)
							{
								order.AllLines.Select(l => l.WE_OP).ForEach(l => productsOnNewOrders.Add(l));
								newOrderPks.Add(order.PK);
							}
						}
					}
				}

				OrderedInventories.Sort(new SortItemsForPicking());

				// Reserved Pick Lines are dependent on whether the Docket is Picked or not.
				ReservedPickLineCollection.InvalidateAll(Factory);
				isCartonising_WithCacheForAttachingOrders = null;
				ClearAllInventoriesCache();

				AllocateCrossDockLines(newOrderPks);

				UpdatePickStatusIfRequired();
				CheckAndLogUnitConversionErrorsForNewProducts(existingProducts, productsOnNewOrders);
			}
		}

		#endregion

		#region RemoveOrders

		public void RemoveOrders(IEnumerable<WhsPickableDocket> pickableDocketsToRemove)
		{
			var pickableDocketsToRemoveEvaluated = pickableDocketsToRemove.ToArray();
			PackagePackingHelper.AddFetchHintsWhenDeletingPackageJobFromPickableDockets(Factory, pickableDocketsToRemoveEvaluated);

			using (new IsDetachingPickSetter(pickableDocketsToRemoveEvaluated))
			{
				var anyOrderHasStockAllocated = pickableDocketsToRemoveEvaluated.Any(o => IsStockAllocatedToOrder(o));

				using (Orders.SuspendListChanged())
				using (OrderedInventories.DeferValidationItemsOnAddOrRemove())
				using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
				using (new SemaphoreManager(SuspendStockWasUnAllocatedMessagePrompt))
				using (new SemaphoreManager(SuspendPickPercentageRecalculationSemaphore))
				using (new SemaphoreManager(BulkDetachingPickSemaphore))
				using (SuspendPackingUpdate())
				// since cross dock allocation is also handled when detaching orders (through count changed event)
				// we will suspend it here as we want to allocate reserved stock at the end when we have rolled up
				// all orders into ordered inventory.
				using (new SemaphoreManager(SuspendCrossDockAllocationSemaphore))
				{
					foreach (var order in pickableDocketsToRemoveEvaluated)
					{
						if (Orders.Contains(order))
						{
							Orders.Remove(order);
						}
					}
				}

				// Reserved Pick Lines are dependent on whether the Docket is Picked or not.
				ReservedPickLineCollection.InvalidateAll(Factory);
				ClearAllInventoriesCache();

				ReCalculatePercentageComplete();
				UpdatePickStatusIfRequired();

				if (anyOrderHasStockAllocated && !SuspendStockWasUnAllocatedMessagePrompt.IsSuspended)
				{
					NotificationSubscriber.Notify(new InfoNotification(PickErrorTypes.StockWasUnAllocatedFromOrders.Message));
				}

				CalculateOrderTotals();
			}
		}

		#endregion

		void OrdersCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (!((BusinessObjectCollection)sender).IsLoading)
			{
				var order = (WhsPickableDocket)e.BizObject;

				if (order != null)
				{
					ClearAllInventoriesCache();

					if (e.ItemAdded)
					{
						AttachOrder(order);
					}
					else
					{
						DetachOrder(order);
						ReCalculatePercentageComplete();
					}

					NumberOfOrdersInfo.RefreshBinding();
				}
			}
		}

		#region AttachOrder

		void AttachOrder(WhsPickableDocket order)
		{
			SetPickPropertiesFromOrders([order]);

			if (IsOrderOkToAttach(order))
			{
				using (order.SetIsAttachingToPick())
				{
					SetOrderProperties(order);

					if (orderedInventories == null)
					{
						PokeOrderedInventories(); // touching the collection will add inventories from all orders and sort
					}
					else
					{
						OrderedInventories.AddNewFromOrder(order);

						if (!BulkAttachingPickSemaphore.IsSuspended)
						{
							OrderedInventories.Sort(new SortItemsForPicking());
						}
					}

					if (order.IsRecalculateOrderPricing)
					{
						order.Lines.ForEach(l => l.WE_ExtendedLinePrice = 0m);
					}

					// If order had reserved stock we need to allocate this stock.
					// During picking, reserved stock is allocated automatically, so we only
					// allocate here if the cross dock allocation semaphore is not suspended.
					if (!SuspendCrossDockAllocationSemaphore.IsSuspended)
					{
						AllocateCrossDockLines(order);
					}
				}

				order.Validation.ValidateWD_WhsOrderFulfillmentRule();

				if (!BulkAttachingPickSemaphore.IsSuspended)
				{
					UpdatePickStatusIfRequired();
					CheckAndLogUnitConversionErrorsForNewProducts(order);
				}

				outerPackages = null;

				// DDL are not used by work orders.
				if (IsWorkOrderPick)
				{
					WP_WL_DockDoor = ZGuid.Empty;
				}
			}
			else
			{
				using (Orders.SuspendCountChanged(null))
				{
					Orders.Remove(order);
				}

				TestHandler(Res.GetString("a7d7c3f1-dca2-4a52-b36c-249d50c9c8d9", "Order: {0} could not be successfully attached to Pick: {1}. You may need to fix your test.", order.WD_ExternalReference, WP_PickNo));
			}
		}

		WhsPickOrderedInventoryCollection PokeOrderedInventories() => OrderedInventories;

		#region AddLinesForComponentIfRequired

		IEnumerable<WhsPickableDocketLine> AddLinesForComponentIfRequired(WhsPickableDocket order, HashSet<ZGuid> componentProductsForNewOrderLines)
		{
			var addedLines = new List<WhsPickableDocketLine>();
			var linesThatRequireChildLines = order.Lines.Cast<WhsPickableDocketLine>().Where(l => l.IsBOMProductPickedOnSalesOrder && l.WE_ShortfallQuantityCached > 0 && !LineHasAnyOrderedAttributes(l));
			var productGroups = linesThatRequireChildLines.GroupBy(l => l.WE_OP).ToList();

			foreach (var productGroup in productGroups)
			{
				var kitProduct = Factory.Load<OrgSupplierPart>(productGroup.Key);
				var possibleProductCanBeAssembledFromItsComponent = ShortfallQuantityHelper.PossibleProductQtyCanBeAssembledFromItsComponents(kitProduct, order.WD_OH_Client, order.WD_WW_Whs);
				foreach (var line in productGroup)
				{
					var numberOfKitsWeWantToBeBuilt = line.WE_TransactionQuantity - line.TotalQuantityOrderedFromComponents - line.PickLines.Where(pl => !pl.IsPickByBOMKitPickLine()).Sum(l => l.WZ_Units);
					var numberOfKitsToBeBuiltFromComponents = Math.Min(possibleProductCanBeAssembledFromItsComponent, numberOfKitsWeWantToBeBuilt);
					if (numberOfKitsToBeBuiltFromComponents > 0)
					{
						addedLines.AddRange(CreateOrderLinesForComponents(line, numberOfKitsToBeBuiltFromComponents, componentProductsForNewOrderLines));
						possibleProductCanBeAssembledFromItsComponent -= numberOfKitsToBeBuiltFromComponents;
					}
					else
					{
						break;
					}
				}
			}
			return addedLines;
		}

		bool LineHasAnyOrderedAttributes(WhsPickableDocketLine line)
		{
			return !line.WE_PartAttrib1.IsEmpty ||
				!line.WE_PartAttrib2.IsEmpty ||
				!line.WE_PartAttrib3.IsEmpty ||
				!line.WE_SerialNumber.IsEmpty ||
				line.WE_ExpiryDate.IsValid ||
				line.WE_PackingDate.IsValid ||
				!line.WE_BondedEntryKey.IsEmpty ||
				!line.WE_PalletID.IsEmpty;
		}

		#region CreateOrderLinesForComponents

		IEnumerable<WhsPickableDocketLine> CreateOrderLinesForComponents(WhsPickableDocketLine line, ZDecimal productQuantity, HashSet<ZGuid> componentProductsForNewLines)
		{
			var newLines = new List<WhsPickableDocketLine>();
			foreach (OrgPartBOM bomPart in line.SupplierPart.BillOfMaterials)
			{
				var docket = (WhsPickableDocket)line.Docket;
				using (docket.SuspendUpdatingPickOrderedInventories())
				{
					var componentLine = docket.AllLines.AddNew();

					componentLine.WE_OP = bomPart.OE_OP_Component;
					componentLine.WE_WE_ParentDocketLine = line.PK;
					componentLine.WE_F3_NKPackType = bomPart.OE_F3_NKPackType;
					componentLine.WE_TransactionQuantity = BOMComponentQuantityHelper.GetComponentsQuantityToBuildKits(bomPart, productQuantity);
					newLines.Add(componentLine);
					componentProductsForNewLines.Add(bomPart.OE_OP_Component);
				}
			}
			return newLines;
		}

		#endregion

		#endregion

		#region CheckAndLogUnitConversionErrorsForNewProducts

		void CheckAndLogUnitConversionErrorsForNewProducts(WhsPickableDocket newOrder)
		{
			var productsAlreadyInThePick = Orders.Except(newOrder).Cast<WhsPickableDocket>().SelectMany(x => x.AllLines.Select(l => l.WE_OP));
			var productsInTheOrder = newOrder.AllLines.Select(l => l.WE_OP);
			CheckAndLogUnitConversionErrorsForNewProducts(productsAlreadyInThePick, productsInTheOrder);
		}

		void CheckAndLogUnitConversionErrorsForNewProducts(IEnumerable<ZGuid> productsAlreadyOnThePick, IEnumerable<ZGuid> productsOnNewOrders)
		{
			var newProductsForThePick = productsOnNewOrders.Except(productsAlreadyOnThePick).Distinct();

			var eventDateTime = ZDateTimeOffset.Now;
			foreach (var product in Factory.Load<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.PK, newProductsForThePick)))
			{
				var conversionTable = new ConversionsToSKUTable(product);
				if (conversionTable.HasErrors || conversionTable.HasWarnings)
				{
					var typeParameter = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.Product + " " + product.OP_PartNum);
					var reasonParameter = new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, Constants.EventReferenceParameterReasons.BadUnitConversion);

					Logs.CreateOrRecreateEventLog(Events.ErrorReport, EstimateActual.Actual, eventDateTime, "", typeParameter, reasonParameter);
				}
			}
		}

		#endregion

		#region SetPickPropertiesFromOrders

		void SetPickPropertiesFromOrders(WhsPickableDocket[] orders)
		{
			if (!WP_WW_Whs.IsValid)
			{
				WP_WW_Whs = orders[0].WD_WW_Whs;
			}
			if (WP_PickOption.IsEmpty)
			{
				WP_PickOption = orders[0].WD_PickOption;
			}
			if (WP_PickType.IsEmpty)
			{
				WP_PickType = GetPickTypeToMatchDockets(orders);
			}
		}

		#endregion

		#region SetOrderProperties

		void SetOrderProperties(WhsPickableDocket order)
		{
			if (order.WD_WP != PK) // compare so HasChanges is not set when loading existing picks
			{
				order.WD_WP = PK;
			}

			if (order.WD_DocketStatus == DocketStatus.Codes.New || order.WD_DocketStatus == DocketStatus.Codes.Entered)
			{
				order.WD_DocketStatus = DocketStatus.Codes.AttachedToPick;
			}
		}

		#endregion

		#region IsOrderOkToAttach

		bool IsOrderOkToAttach(WhsPickableDocket order)
		{
			bool result = true;

			if (!order.WD_WP.IsEmpty && order.WD_WP != PK)
			{
				// must not attach, it is already attached to another pick
				result = false;
			}
			else if (IsReadyForPlanningOrPlanned)
			{
				// Can't attach new orders as Pick is Ready For Planning or Planned
				result = false;
				NotificationSubscriber.Notify(new ErrorNotification(PickErrorTypes.CannotPerformThisOperationBecausePickIsReadyForPlanningOrPlanned));
			}
			else if (WP_IsCartonised || IsCartonising_WithCacheForAttachingOrders)
			{
				// Can't attach new orders to a Cartonised or Cartonising Pick as it will modify allocations
				result = false;
			}
			else if (Orders.Any(x => x != order && x is WhsComponentOrder))
			{
				// Cannot attach new orders to a Pick which has WorkOrders
				result = false;
			}
			else if (!IsDocketTypeValidToAttach(order))
			{
				result = false;

				var expectedDocketTypeDesc = new PickType().GetDescriptionFromCode(WP_PickType == PickType.Codes.HeldInventoryOrder ? PickType.Codes.Order : WP_PickType);
				var errorType = ((string)WP_PickType) switch
				{
					PickType.Codes.Order => order.WD_DocketType == DocketType.Codes.Order ? PickErrorTypes.DocketMismatchExpectAvailableInventory : PickErrorTypes.DocketTypeMismatch(expectedDocketTypeDesc),
					PickType.Codes.HeldInventoryOrder => order.WD_DocketType == DocketType.Codes.Order ? PickErrorTypes.DocketMismatchExpectHeldInventory : PickErrorTypes.DocketTypeMismatch(expectedDocketTypeDesc),
					PickType.Codes.WorkOrder or PickType.Codes.DynamicWorkOrder => PickErrorTypes.DocketTypeMismatch(expectedDocketTypeDesc),
					_ => throw new ArgumentException($"Invalid pick type {WP_PickType}, cannot determine if docket is valid to attach"),
				};
				NotificationSubscriber.Notify(new ErrorNotification(errorType));
			}
			else if (order.WD_PickOption != WP_PickOption)
			{
				var e = GetOrderHadWrongPickOptionQueryArgs(order);
				NotificationSubscriber.QueryUser(e);

				#region Test
#if DEBUG
				LastNotificationMessage = e.Message;
#endif
				#endregion

				result = e.Response;
				if (result)
				{
					order.WD_PickOption = WP_PickOption;
				}
			}

			return result;
		}

		bool IsDocketTypeValidToAttach(WhsPickableDocket docket)
		{
			return (string)WP_PickType switch
			{
				PickType.Codes.Order => docket.WD_DocketType == DocketType.Codes.Order && (docket.Lines.Count == 0 || !docket.IsHeldInventoryOrder),
				PickType.Codes.HeldInventoryOrder => docket.WD_DocketType == DocketType.Codes.Order && (docket.Lines.Count == 0 || docket.IsHeldInventoryOrder),
				PickType.Codes.WorkOrder => docket.WD_DocketType == DocketType.Codes.WorkOrder,
				PickType.Codes.DynamicWorkOrder => docket.WD_DocketType == DocketType.Codes.DynamicWorkOrder,
				_ => throw new ArgumentException($"Invalid pick type {WP_PickType}, cannot determine if docket is valid to attach"),
			};
		}

		QueryUserYesNoEventArgs GetOrderHadWrongPickOptionQueryArgs(WhsPickableDocket order)
		{
			string msg = "\r\n" +
				Res.GetString("130cbbf8-e352-4078-b1c0-a7cf30119760",
					"Order '{0}' has a Pick Option of {1}, however the Pick Option on the Pick is {2}.\r\nDo you want to change the Order's Pick Option to {2} and attach the Order?",
				order.WD_ExternalReference, order.WD_PickOption, WP_PickOption);

			return new QueryUserYesNoEventArgs(msg, false);
		}

		public bool PickContainsMixedInventory(WhsPickableDocket attachedOrUpdatedOrder)
		{
			var result = false;
			var firstOrderLine = Orders.Where(o => o.PK != attachedOrUpdatedOrder.PK).SelectMany(o => o.Lines).FirstOrDefault();

			if (attachedOrUpdatedOrder is WhsOrder && firstOrderLine != null)
			{
				result = firstOrderLine.WE_WHC_NKOrderedHeldCode.IsEmpty != attachedOrUpdatedOrder.Lines[0].WE_WHC_NKOrderedHeldCode.IsEmpty;
			}

			return result;
		}

		#endregion

		#endregion

		#region DetachOrder

		void DetachOrder(WhsPickableDocket order)
		{
			if (IsOrderOkToDetach(order))
			{
				bool orderHadStockAllocated = IsStockAllocatedToOrder(order);

				using (order.SetIsDetachingToPick())
				{
					using (order.DelayUpdatingReleaseTotals())
					{
						ClearAllocations(order);
					}

					if (!IsWorkOrderPick)
					{
						DeleteChildComponentLinesIfAny(order);
					}

					order.WD_WP = ZGuid.Empty; // clear out pick before reinstating cross dock allocations, as reservation can only be done for non-picked orders

					ReinstateCrossDockAllocations(order);

					OrderedInventories.RemoveFromOrder(order);
					ResetOrder(order);
					if (!BulkDetachingPickSemaphore.IsSuspended)
					{
						UpdatePickStatusIfRequired();
					}
					outerPackages = null;

					if (orderHadStockAllocated && !SuspendStockWasUnAllocatedMessagePrompt.IsSuspended)
					{
						NotificationSubscriber.Notify(new InfoNotification(PickErrorTypes.StockWasUnAllocatedFromOrders.Message));
					}
				}
				if (Orders.Count == 0)
				{
					WP_IsCartonised = false;
				}

				if (order.IsRecalculateOrderPricing)
				{
					order.Lines.Cast<WhsPickableDocketLine>().ForEach(l => l.CalculateExtendedLinePrice());
				}
			}
			else
			{
				using (Orders.SuspendCountChanged(null))
				{
					Orders.Add(order);
				}
			}
		}

		bool IsOrderOkToDetach(WhsPickableDocket order)
		{
			var canDetach = !IsCartonising || IsCurrentInstanceCartonising;
			if (canDetach)
			{
				if (IsReadyForPlanningOrPlanned)
				{
					canDetach = false;
					NotificationSubscriber.Notify(new ErrorNotification(PickErrorTypes.CannotPerformThisOperationBecausePickIsReadyForPlanningOrPlanned));
				}
				else if (order.IsPartiallyOrFullyPickedFromPutawayLocation)
				{
					canDetach = false;
					NotificationSubscriber.Notify(new ErrorNotification(OrderErrorTypes.CannotPerformThisOperationBecauseOrderIsPartiallyOrFullyPicked));
				}
				else if (order.IsCurrentlyBeingPickedFromPutawayLocation)
				{
					canDetach = false;
					NotificationSubscriber.Notify(new ErrorNotification(OrderErrorTypes.CannotPerformThisOperationBecauseOrderIsBeingPicked));
				}
				foreach (var strategy in PickStrategies)
				{
					if (!strategy.IsOrderActionAllowed(order as WhsOrder, PickOrderAction.DetachOrder, out var reasonNotAllowedNotification))
					{
						canDetach = false;
						NotificationSubscriber.Notify(reasonNotAllowedNotification);
						break;
					}
				}
			}
			else
			{
				NotificationSubscriber.Notify(new ErrorNotification(OrderErrorTypes.CannotPerformThisOperationBecauseOrderIsCartonisedOrBeingCartonised));
			}

			return canDetach;
		}

		bool IsCurrentInstanceCartonising => CartonisationMutex.HasLock;

		Semaphore SuspendStockWasUnAllocatedMessagePrompt
		{
			get { return suspendStockWasUnAllocatedMessagePrompt ?? (suspendStockWasUnAllocatedMessagePrompt = new Semaphore()); }
		}

		Semaphore suspendStockWasUnAllocatedMessagePrompt;

		IDisposable SuspendPackingUpdate()
		{
			var semaphores = Orders.OfType<WhsOrder>().Select(o => new SemaphoreManager(o.SuspendPackingUpdateSemaphore)).ToArray();
			return new DisposableAction(() => semaphores.ForEach(s => s.Dispose()));
		}

		void DeleteChildComponentLinesIfAny(WhsPickableDocket order)
		{
			var childLines = order.Lines.Where(l => !l.WE_WE_ParentDocketLine.IsEmpty).ToArray();
			if (childLines.Length > 0)
			{
				DeleteReceiveCreatedFromPickByBOM(order, childLines);
			}

			foreach (var line in childLines)
			{
				line.Delete();
			}
		}

		void DeleteReceiveCreatedFromPickByBOM(WhsDocket order, WhsDocketLine[] componentLines)
		{
			var query = new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, PK);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, order.WD_OH_Client);
			var receiveCreatedFromPickByBOM = Factory.LoadTop1<WhsReceive>(query);
			if (receiveCreatedFromPickByBOM != null)
			{
				var componentLinesPKHashSet = componentLines.Select(l => l.PK).ToHashSet();
				foreach (var line in receiveCreatedFromPickByBOM.Lines.ToArray())
				{
					var links = line.BOMComponentLinks;
					if (links.Any(l => componentLinesPKHashSet.Contains(l.WIP_WE_ComponentLine)))
					{
						line.Delete();
					}
				}

				if (receiveCreatedFromPickByBOM.Lines.Count == 0)
				{
					receiveCreatedFromPickByBOM.Delete();
				}
			}
		}

		#region IsStockAllocatedToOrder

		bool IsStockAllocatedToOrder(WhsPickableDocket order)
		{
			return order.Lines.Any(l => l.PickLines.Any(pl => pl.WZ_Units > 0));
		}

		#endregion

		#region ClearAllocations

		/// <summary>
		/// We cannot go through ordered inventory when detaching an Order as they are rolled up across orders.
		/// We must therefore delete the picklines directly attached to the orderlines on the detaching Order.
		/// </summary>
		void ClearAllocations(WhsPickableDocket order)
		{
			foreach (WhsPickableDocketLine line in order.GetLinesToPick())
			{
				foreach (var pickLine in line.PickLines.ToArray())
				{
					ReleaseLinesOnPickManager.NotifyChange(Factory, pickLine, -pickLine.WZ_Units);

					if (pickLine.IsReserveLine)
					{
						pickLine.ClearOutPickingValuesForReservedLine();
					}
					else
					{
						pickLine.Delete();
					}
				}

				// Unrelease, Clear and Unregister release lines before we lose reference to the pick
				if (line.IsReleaseLineCollectionBuilt)
				{
					foreach (WhsReleaseLine releaseLine in line.ReleaseLines)
					{
						// we need to update unreleased qtys with the release lines that are being removed
						ReleaseLinesOnPickManager.NotifyChange(Factory, releaseLine, line, -releaseLine.Quantity);
					}

					line.ClearReleaseLines();
				}
			}
		}

		#endregion

		#region ReinstateCrossDockAllocations

		void ReinstateCrossDockAllocations(WhsPickableDocket order)
		{
			foreach (WhsPickableDocketLine line in order.GetLinesToPick())
			{
				var orderLine = line as WhsOrderLine;
				if (orderLine != null)
				{
					var reservedPickLines = GetReservedPickLines(orderLine);

					foreach (var pickLine in reservedPickLines)
					{
						// try to re-reserve the original reserved quantity
						orderLine.ReserveStockIfAbleTo(pickLine.Inventory, pickLine.WZ_OriginalReservedQty);
					}
				}
			}
		}

		WhsPickLine[] GetReservedPickLines(WhsOrderLine line)
		{
			var query = new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, line.PK);
			query.AddToFilter(WhsPickLineSchema.WZ_OriginalReservedQty, SQLComparisonOperator.GreaterThan, 0m);
			return Factory.Load<WhsPickLine>(query);
		}

		#endregion

		#region ResetOrder

		void ResetOrder(WhsPickableDocket pickableDocket)
		{
			pickableDocket.WD_DocketStatus = DocketStatus.Codes.Entered;
			pickableDocket.WD_FinalisedDate = ZDateTimeOffset.Empty;
			pickableDocket.WD_PackagesSent = 0;
			pickableDocket.ResetOrderAfterDetachingFromPick();
		}

		#endregion

		#endregion

		#endregion

		#region Properties

		[ResourceStringData("WhsPick|SalesChannelDescriptions", Caption = "Sales Channel", FullDescription = "Sales Channel Description")]
		public ZString SalesChannelDescriptions => Orders.OfType<WhsOrder>().GetSingleValueOrManyText(o => o.SalesChannel?.WSH_Description ?? ZString.Empty, o => o.WD_WSH_SalesChannel);

		#region ClientCodes

		public ZString ClientCodes => Orders.Cast<WhsPickableDocket>().GetSingleValueOrManyText(x => x.Client.OH_Code, x => x.WD_OH_Client, x => x.WD_OH_Client.IsValid);

		#endregion

		#region ClientNames

		public ZString ClientNames => Orders.Cast<WhsPickableDocket>().GetSingleValueOrManyText(x => x.Client.OH_FullName, x => x.WD_OH_Client, x => x.WD_OH_Client.IsValid);

		#endregion

		#region InventoriesFilterDate

		[BusinessObjectTestExclude]
		public ZDateTimeOffset? InventoriesFilterDate
		{
			get { return inventoriesFilterDate; }
			private set
			{
				if (allInventories != null)
				{
					throw new InvalidOperationException("Pass inventory date before all inventories are loaded");
				}

				inventoriesFilterDate = value;
			}
		}

		ZDateTimeOffset? inventoriesFilterDate;

		#endregion

		#region EarliestRequiredDate

		public ZDateTimeOffset EarliestRequiredDate => Orders.Cast<WhsPickableDocket>().Select(x => x.WD_RequiredDate).OrderBy(x => x).FirstOrDefault();

		#endregion

		#region ConsigneeCodes

		public ZString ConsigneeCodes
		{
			get
			{
				return Orders.Cast<WhsPickableDocket>().GetSingleValueOrManyText(x => x.ConsigneeDocAddress.E2_AddressOverride
					? x.ConsigneeDocAddress.E2_CompanyName : x.ConsigneeDocAddress.Organisation?.OH_Code ?? ZString.Empty);
			}
		}

		#endregion

		#region ConsigneeNames

		public ZString ConsigneeNames
		{
			get
			{
				return Orders.Cast<WhsPickableDocket>().GetSingleValueOrManyText(x => x.ConsigneeDocAddress.E2_AddressOverride
					? x.ConsigneeDocAddress.E2_CompanyName : x.ConsigneeDocAddress.Organisation?.OH_FullNameTruncated ?? ZString.Empty);
			}
		}

		#endregion

		#region TransportCodes

		public ZString TransportCodes
		{
			get
			{
				return Orders.Cast<WhsPickableDocket>().GetSingleValueOrManyText(x => x.TransportCoDocAddress.E2_AddressOverride
						? x.TransportCoDocAddress.E2_CompanyName : x.TransportCoDocAddress.Organisation?.OH_Code ?? ZString.Empty);
			}
		}

		#endregion

		#region TransportReferences

		public ZString TransportReferences => Orders.Cast<WhsPickableDocket>().Where(x => !x.WD_TransportReference.IsEmpty).GetSingleValueOrManyText(x => x.WD_TransportReference);

		#endregion

		#region DistributionCentreCodes

		[ResourceStringData("WhsPick|DistributionCentreCodes", Caption = "Distribution Center", ShortCaption = "Dist. Center")]
		public ZString DistributionCentreCodes => Orders.Cast<WhsPickableDocket>().GetSingleValueOrManyText(
			x => x.DistributionCentreDocAddress.E2_AddressOverride ? x.DistributionCentreDocAddress.E2_CompanyName : x.DistributionCentreDocAddress.Organisation?.OH_Code ?? ZString.Empty);

		#endregion

		#region ServiceLevels

		public ZString ServiceLevels => Orders.Cast<WhsPickableDocket>().GetSingleValueOrManyText(x => x.WD_RS_NKServiceLevel);

		#endregion

		#region HasNonAllocatedOrderedInventory

		public ZBool HasNonAllocatedOrderedInventory
		{
			get
			{
				foreach (WhsPickOrderedInventory orderedInventory in OrderedInventories)
				{
					if (orderedInventory.PickLineQuantity == 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region HasShortfallItems

		public ZBool HasShortfallItems
		{
			get
			{
				foreach (WhsPickOrderedInventory orderedInventory in OrderedInventories)
				{
					if (orderedInventory.QuantityShort != 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region WP_CartoniseSplitCases

		[ReadOnlyMember(nameof(CartonisationReadOnly))]
		public override ZBool WP_CartoniseSplitCases
		{
			get { return base.WP_CartoniseSplitCases; }
			set
			{
				base.WP_CartoniseSplitCases = value;

				if (value)
				{
					WP_PickCasesByLabel = true;
					WP_PickPalletsByLabel = true;
				}
			}
		}

		#endregion

		#region WP_ForcePickByCaseUOMTypeAllocation

		[ReadOnlyMember(nameof(ForcePickByCaseUOMTypeAllocationReadOnly))]
		public override ZBool WP_ForcePickByCaseUOMTypeAllocation
		{
			get { return base.WP_ForcePickByCaseUOMTypeAllocation; }
			set
			{
				if (value && WP_ForceSplitCaseUOMTypeAllocation)
				{
					throw new InvalidOperationException("Cannot set WP_ForcePickByCaseUOMTypeAllocation to TRUE if WP_ForceSplitCaseUOMTypeAllocation is already true.");
				}
				else
				{
					base.WP_ForcePickByCaseUOMTypeAllocation = value;
				}
			}
		}

		#endregion

		#region WP_ForceSplitCaseUOMTypeAllocation

		[ReadOnlyMember(nameof(ForceSplitCaseUOMTypeAllocationReadOnly))]
		public override ZBool WP_ForceSplitCaseUOMTypeAllocation
		{
			get { return base.WP_ForceSplitCaseUOMTypeAllocation; }
			set
			{
				if (value && WP_ForcePickByCaseUOMTypeAllocation)
				{
					throw new InvalidOperationException("Cannot set WP_ForceSplitCaseUOMTypeAllocation to TRUE if WP_ForcePickByCaseUOMTypeAllocation is already true.");
				}
				else
				{
					base.WP_ForceSplitCaseUOMTypeAllocation = value;
				}
			}
		}

		#endregion

		#region WP_WW_Whs

		[ReadOnlyMember(nameof(StandardReadOnly))]
		[List("Lookups.Warehouses")]
		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WP_WW_Whs
		{
			get { return base.WP_WW_Whs; }
			set
			{
				var warehouseHasChanged = WP_WW_Whs != value;
				base.WP_WW_Whs = value;

				if (warehouseHasChanged)
				{
					SetDefaultDockDoorLocation();
					WP_WA_DynamicPickAreaOverride = ZGuid.Empty;
				}
			}
		}

		#region SetDefaultDockDoorLocation

		void SetDefaultDockDoorLocation()
		{
			var whs = Warehouse;
			if (whs != null && !IsWorkOrderPick)
			{
				DockDoorPK = whs.WW_DefaultOutboundDockDoor;
			}
			else if (DockDoorPK.IsValid)
			{
				DockDoorPK = ZGuid.Empty;
			}
		}

		#endregion

		#endregion

		#region WP_PickOption

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[List("Lookups.PickOptions")]
		public override ZString WP_PickOption
		{
			get { return base.WP_PickOption; }
			set { base.WP_PickOption = value; }
		}

		#endregion

		#region WP_PickStatus

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WP_PickStatus
		{
			get { return base.WP_PickStatus; }
			set
			{
				var previousPickStatus = base.WP_PickStatus;
				base.WP_PickStatus = value;

				if (!previousPickStatus.EqualsIgnoringCase(WP_PickStatus))
				{
					ConcurrencyPolicy versionIDConcurrencyPolicy;

					if (IsFinalisedOrCancelled)
					{
						ReCalculatePercentageComplete();
						versionIDConcurrencyPolicy = ConcurrencyPolicy.Strict;
					}
					else
					{
						versionIDConcurrencyPolicy = ConcurrencyPolicy.Ignore;
					}

					ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WP_CriticalChangesVersionID), versionIDConcurrencyPolicy);
					UpdateWhsPickCriticalChangesVersionIDHelper.UpdatePickVersionAndSubscribeToFactory(Factory, PK);
				}
			}
		}

		#endregion

		#region WP_PickCasesByLabel

		[ReadOnlyMember(nameof(CartonisationReadOnly))]
		public override ZBool WP_PickCasesByLabel
		{
			get { return base.WP_PickCasesByLabel; }
			set { base.WP_PickCasesByLabel = value; }
		}

		#endregion

		#region WP_TaskPlanningStatus

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WP_TaskPlanningStatus
		{
			get => base.WP_TaskPlanningStatus;
			set => base.WP_TaskPlanningStatus = value;
		}

		#endregion

		#region DockDoorPK

		[ReadOnlyMember(nameof(IsDockDoorReadOnly))]
		[List("Lookups.DockDoorLocations")]
		[RelatedBusinessObject("DockDoorLocation")]
		[ResourceStringData("WhsPick|DockDoorPK", Caption = "Dock Door")]
		public ZGuid DockDoorPK
		{
			get { return WP_WDA_DockDoorAssignment.IsValid ? DockDoorAssignment.WDA_WL_AssignedDockDoor : WP_WL_DockDoor; }
			set
			{
				if (WP_WDA_DockDoorAssignment.IsEmpty)
				{
					WP_WL_DockDoor = value;
				}
				else if (!DockDoorAssignment.WDA_FirstPutawayToDockDoorUtc.IsValid)
				{
					DockDoorAssignment.WDA_WL_AssignedDockDoor = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateDockDoorPK();
				}

				DockDoorPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DockDoorPKInfo => GetZPropertyInfo(nameof(DockDoorPK));

		#endregion

		#region WP_WL_DockDoor

		public override ZGuid WP_WL_DockDoor
		{
			get { return base.WP_WL_DockDoor; }
			set
			{
				SetWarehouseFromDockDoorLocation(value);
				base.WP_WL_DockDoor = value;
			}
		}

		#region SetWarehouseFromDockDoorLocation

		void SetWarehouseFromDockDoorLocation(ZGuid locationPK)
		{
			if (WP_WW_Whs.IsEmpty && !locationPK.IsEmpty)
			{
				var dockDoorLocation = Factory.Load<WhsLocation>(locationPK);
				if (dockDoorLocation != null)
				{
					WP_WW_Whs = dockDoorLocation.WLV_WW_Whs;
				}
			}
		}

		#endregion

		#endregion

		#region WP_WL_PackingStation

		[ReadOnly(true)]
		[List("Lookups.PackingStationLocations")]
		public override ZGuid WP_WL_PackingStation
		{
			get { return base.WP_WL_PackingStation; }
			set { base.WP_WL_PackingStation = value; }
		}

		#endregion

		#region WP_WA_DynamicPickAreaOverride

		[ReadOnlyMember(nameof(PickAreaOverrideReadOnly))]
		[List("Lookups.DynamicPickAreas")]
		public override ZGuid WP_WA_DynamicPickAreaOverride
		{
			get { return base.WP_WA_DynamicPickAreaOverride; }
			set { base.WP_WA_DynamicPickAreaOverride = value; }
		}

		#endregion

		#region WP_PickPalletsByLabel

		[ReadOnlyMember(nameof(CartonisationReadOnly))]
		public override ZBool WP_PickPalletsByLabel
		{
			get { return base.WP_PickPalletsByLabel; }
			set { base.WP_PickPalletsByLabel = value; }
		}

		#endregion

		#region WP_IsAwaitingReplenishment

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZBool WP_IsAwaitingReplenishment
		{
			get { return base.WP_IsAwaitingReplenishment; }
			set { base.WP_IsAwaitingReplenishment = value; }
		}

		#endregion

		#region WP_IsCartonised

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZBool WP_IsCartonised
		{
			get { return base.WP_IsCartonised; }
			set { base.WP_IsCartonised = value; }
		}

		#endregion

		#region WP_FinalizedDateUtc

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZDateTime WP_FinalizedDateUtc
		{
			get => base.WP_FinalizedDateUtc;
			set
			{
				base.WP_FinalizedDateUtc = value;
				UpdateFinalizedByIfNecessary();
			}
		}

		void UpdateFinalizedByIfNecessary()
		{
			var finalizedBy = WP_FinalizedDateUtc.IsValid ? GlbStaff.CurrentUser.GS_Code : ZString.Empty;
			if (WP_GS_NKFinalizedBy != finalizedBy)
			{
				WP_GS_NKFinalizedBy = finalizedBy;
			}
		}

		#endregion

		#region ParentForm

		public IPickForm ParentForm
		{
			get;
			set;
		}

		#endregion

		#region Flags

		#region IsAlterPick

		public bool IsAlterPick
		{
			get;
			set;
		}

		#endregion

		#region IsPickSlipPrinted

		public bool IsPickSlipPrinted
		{
			get { return WP_PickStatus == PickStatus.Codes.PickSlip; }
		}

		#endregion

		#region IsPicking

		public bool IsPicking
		{
			get { return GetAllPickLines().Any(line => line.WZ_IsPicking); }
		}

		#endregion

		#region IsFinalised

		public bool IsFinalised
		{
			get { return WP_PickStatus == PickStatus.Codes.Finalised; }
		}

		#endregion

		#region IsFinalising

		public bool IsFinalising
		{
			get { return FinalisePickSemaphore.IsSuspended; }
		}

		#endregion

		#region IsCancelled

		public bool IsCancelled
		{
			get { return WP_PickStatus == PickStatus.Codes.Cancelled; }
		}

		#endregion

		#region IsCartonising

		public bool IsCartonising => CartonisationMutex.IsLocked;

		bool IsCartonising_WithCacheForAttachingOrders
		{
			get
			{
				// This field is safe to cache as we check if the pick has started cartonising when saving.
				var result = false;

				if (BulkAttachingPickSemaphore.IsSuspended)
				{
					result = (bool)(isCartonising_WithCacheForAttachingOrders ?? (isCartonising_WithCacheForAttachingOrders = IsCartonising));
				}
				else
				{
					result = IsCartonising;
				}

				return result;
			}
		}

		bool? isCartonising_WithCacheForAttachingOrders;

		ZGlobalMutex CartonisationMutex
		{
			get { return cartonisationMutex ?? (cartonisationMutex = new ZGlobalMutex(MutexIDs.WhsPickAllocatingPackageLabels, PK.ToString())); }
		}
		ZGlobalMutex cartonisationMutex;

		#endregion

		#region IsFinalisedOrCancelled

		public bool IsFinalisedOrCancelled
		{
			get { return IsFinalised || IsCancelled; }
		}

		#endregion

		#region IsReadyForPlanningOrPlanned

		public bool IsReadyForPlanningOrPlanned => WP_TaskPlanningStatus == TaskPlanningStatus.Codes.Ready || WP_TaskPlanningStatus == TaskPlanningStatus.Codes.Planned;

		#endregion

		#region IsDockDoorReadOnly

		public bool IsDockDoorReadOnly => IsFinalisedOrCancelled || WP_WDA_DockDoorAssignment.IsValid;

		#endregion

		#region IsFinalisedOrCancelledOrAllocated

		public bool IsFinalisedOrCancelledOrAllocated => IsFinalisedOrCancelled || IsAllocated;

		#endregion

		#region IsAllocated

		public bool IsAllocated => isAllocated ?? (isAllocated = CalculateIsAllocated()).Value;
		bool? isAllocated;

		bool CalculateIsAllocated()
		{
			return GetAllPickLines().Any();
		}

		internal void ClearIsAllocated()
		{
			isAllocated = null;
		}

		#endregion

		#region IsPartiallyOrFullyPickedFromPutawayLocation

		public bool IsPartiallyOrFullyPickedFromPutawayLocation => Orders.Cast<WhsPickableDocket>().Any(o => o.IsPartiallyOrFullyPickedFromPutawayLocation);

		#endregion

		#region IsPickByUOMEnabled

		public bool IsPickByUOMEnabled
		{
			get
			{
				if (isPickByUOMEnabled == null)
				{
					var warehouse = Warehouse;
					if (warehouse != null)
					{
						// We will determine whether or not 'PickByUOM' is enabled based on the existing pick lines attached to the pick.
						// If we have any picklines and the Allocated Pack Type is set then it is enabled. We check it this way because, Pick By UOM
						// can be disabled on the Warehouse at any time, e.g. after the pick is allocated or finalised, but the picklines
						// still need to be displayed as if they had been picked by UOM.
						// If there are no pick lines on the Pick, then we simply use the Warehouse setting. (c) MMC
						// OrderedInventories is used as this will ensure all PickLines for all Orders are loaded once for future use.
						var pickLineNotReservedLine = OrderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(ord => ord.Owners.Where(ol => IsExistingOrderInPick(ol))).SelectMany(line => line.PickLines).FirstOrDefault();
						if (pickLineNotReservedLine != null)
						{
							isPickByUOMEnabled = !pickLineNotReservedLine.WZ_F3_NKAllocatedPackType.IsEmpty;
						}
						else
						{
							isPickByUOMEnabled = warehouse.WW_IsPickByUOMEnabled;
						}
					}
				}
				return isPickByUOMEnabled ?? false;
			}
		}

		static bool IsExistingOrderInPick(WhsPickableDocketLine orderline)
		{
			var order = orderline.Docket as WhsOrder;
			var pick = (order)?.Pick;
			return pick != null && pick.IsInDatabase && !order.IsAttachingToPick;
		}

		bool? isPickByUOMEnabled;

		#endregion

		#region IsWorkOrderPick

		public bool IsWorkOrderPick => WP_PickType == PickType.Codes.WorkOrder || WP_PickType == PickType.Codes.DynamicWorkOrder; // cannot have a Pick with both Orders + WorkOrders/DynamicWorkOrders

		#endregion

		#region IsHeldInventoryPick

		public bool IsHeldInventoryOrderPick => WP_PickType == PickType.Codes.HeldInventoryOrder;

		#endregion

		#region IsDisassemblyDynamicWorkOrderPick

		public bool IsDisassemblyDynamicWorkOrderPick =>
			IsWorkOrderPick
			&& Orders[0] is WhsDynamicWorkOrder dynamicWorkOrder
			&& !dynamicWorkOrder.IsAssembly;

		#endregion

		#region IsPrintingWithoutSeparatorLabels

		public bool IsPrintingWithoutSeparatorLabels
		{
			get { return enablePrintingWithoutSeparatorLabelsSemaphore != null && enablePrintingWithoutSeparatorLabelsSemaphore.IsSuspended; }
		}

		public IDisposable EnablePrintingWithoutSeparatorLabels() => new SemaphoreManager(EnablePrintingWithoutSeparatorLabelsSemaphore);

		Semaphore EnablePrintingWithoutSeparatorLabelsSemaphore
		{
			get { return enablePrintingWithoutSeparatorLabelsSemaphore ?? (enablePrintingWithoutSeparatorLabelsSemaphore = new Semaphore()); }
		}

		Semaphore enablePrintingWithoutSeparatorLabelsSemaphore;

		#endregion

		#endregion

		#region CannotCancelPick

		public bool CannotCancelPick
		{
			get { return IsFinalisedOrCancelled; }
		}

		#endregion

		#region StatusDesc

		public ZString StatusDesc
		{
			get { return Statuses.GetDescriptionFromCode(WP_PickStatus); }
		}

		public ZPropertyInfo StatusDescInfo
		{
			get { return GetZPropertyInfo(nameof(StatusDesc)); }
		}

		#endregion

		#region Statuses

		public PickStatus Statuses
		{
			get { return new PickStatus(); }
		}

		#endregion

		#region PickingInstructions

		public ZString PickingInstructions
		{
			get
			{
				ZString result = "";

				StmNote[] notes = Notes.FindByDescription(PredefinedNoteTypes.Instance.PickingInstructions.Description);
				if (notes.Length > 0)
				{
					result = notes[0].ST_NoteText;
				}

				return result;
			}
		}

		#endregion

		#region WP_PickNo

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WP_PickNo
		{
			get { return base.WP_PickNo; }
			set { base.WP_PickNo = value; }
		}

		#endregion

		#region WP_PercentageComplete

		/// <summary>
		/// Shows the percent of locations from which the items were Picked
		/// Do NOT set Percentage Complete manualy anywhere, it ALWAYS should be calculated automatically. Use ReCalculatePercentageComplete to recalculate the value where needed.
		/// </summary>
		[ActionField(ReadOnly = true)]
		public override ZByte WP_PercentageComplete
		{
			get { return base.WP_PercentageComplete; }
			set { base.WP_PercentageComplete = value; }
		}

		#region ReCalculatePercentageComplete

		/// <summary>
		/// Always use this when WP_PercentageComplete need to be modified. It's not only recalculate the property to show correct data, but also redraw pie charts on Pick and Release forms.
		/// </summary>
		public void ReCalculatePercentageComplete()
		{
			if (!SuspendPickPercentageRecalculationSemaphore.IsSuspended)
			{
				if (IsCancelled)
				{
					WP_PercentageComplete = 0;
				}
				else if (IsFinalised)
				{
					WP_PercentageComplete = 100;
				}
				else
				{
					WP_PercentageComplete = ReCalculatePercentageCompleteCore();
				}

				CreateCompletedEventIfRequired();
			}
		}

		#region CreateCompletedEventIfRequired

		void CreateCompletedEventIfRequired()
		{
			if (WP_PercentageComplete == 100)
			{
				if (!IsCompletedEventAlreadyExistingOnPick)
				{
					this.AddEvents(Events.ServiceCompleted);
					AddServiceCompleteEventToOrders();
				}
			}
		}

		void AddServiceCompleteEventToOrders()
		{
			var orders = Orders;
			foreach (var order in orders)
			{
				Factory.AddFetchHint(ProcessTasksSchema.Instance, FetchHintsHelper.GetProcessTasksQuery(order.PK, Events.ServiceCompletedCode,
					new[] { Constants.Workflow.MilestoneType, Constants.Workflow.WorkflowTriggerType }, ZString.Empty));
			}

			foreach (WhsPickableDocket order in orders)
			{
				order.AddEvents(Events.ServiceCompleted, WP_PickNo, Constants.EventReferenceParameterTypes.Pick);
			}
		}

		bool IsCompletedEventAlreadyExistingOnPick
		{
			get { return Logs.Find(l => l.SL_SE_NKEvent == Events.ServiceCompletedCode && !l.SL_IsCancelled).Any(); }
		}

		#endregion

		ZByte ReCalculatePercentageCompleteCore()
		{
			var result = ZByte.Zero;
			var allLocationsToPickFrom = new HashSet<ZGuid>();
			var locationsLeftToPickFrom = new HashSet<ZGuid>();

			foreach (WhsPickableDocket order in Orders)
			{
				foreach (var pickLine in order.AllLines.SelectMany(l => l.PickLines.Where(pl => pl.WZ_Units != 0 && !pl.IsPickByBOMKitPickLine())))
				{
					var locationPK = pickLine.InventoryLineForAvailableInventory.WE_WL;
					allLocationsToPickFrom.Add(locationPK);
					if (!pickLine.IsPickedFromPutawayLocation)
					{
						locationsLeftToPickFrom.Add(locationPK);
					}
				}
			}

			if (allLocationsToPickFrom.Count != 0)
			{
				ZInt allLocations = allLocationsToPickFrom.Count;
				ZInt pickedFromLocations = allLocations - locationsLeftToPickFrom.Count;
				result = (ZByte)(100 * pickedFromLocations / allLocations);
			}

			return result;
		}

		#endregion

		#region SuspendPickPercentageRecalculation

		public IDisposable SuspendPickPercentageRecalculation()
		{
			var manager = new SemaphoreManager(SuspendPickPercentageRecalculationSemaphore);
			return new DisposableAction(() =>
			{
				manager.Dispose();
				ReCalculatePercentageComplete();
			});
		}

		Semaphore SuspendPickPercentageRecalculationSemaphore => suspendPickPercentageRecalculationSemaphore ?? (suspendPickPercentageRecalculationSemaphore = new Semaphore());
		Semaphore suspendPickPercentageRecalculationSemaphore;

		#endregion

		#endregion

		#region ReadOnly

		#region StandardReadOnly

		bool StandardReadOnly
		{
			get { return IsPickSlipPrinted || IsFinalisedOrCancelled || Orders.Count > 0; }
		}

		#endregion

		#region CartonisationReadOnly

		bool CartonisationReadOnly => IsFinalisedOrCancelled || WP_IsCartonised || IsWorkOrderPick || IsCartonising;

		#endregion

		#region ForcePickUOMTypeAllocationReadOnlys

		bool ForcePickByCaseUOMTypeAllocationReadOnly
		{
			get
			{
				return WP_ForceSplitCaseUOMTypeAllocation || CartonisationReadOnly || DoesAnyPickLineHaveAllocatedPackType();
			}
		}

		bool ForceSplitCaseUOMTypeAllocationReadOnly
		{
			get
			{
				return WP_ForcePickByCaseUOMTypeAllocation || CartonisationReadOnly || DoesAnyPickLineHaveAllocatedPackType();
			}
		}

		bool DoesAnyPickLineHaveAllocatedPackType()
		{
			return forcePickByCaseUOMTypeAllocationReadOnly ?? (forcePickByCaseUOMTypeAllocationReadOnly = GetAllPickLines().Any(p => p.AllocatedPackType != null)).Value;
		}
		bool? forcePickByCaseUOMTypeAllocationReadOnly;

		#endregion

		#region ClearPackTypeAllocatedReadOnlyCache

		internal void ClearPackTypeAllocatedReadOnlyCache()
		{
			forcePickByCaseUOMTypeAllocationReadOnly = null;
			WP_ForcePickByCaseUOMTypeAllocationInfo.RefreshBinding();
			WP_ForceSplitCaseUOMTypeAllocationInfo.RefreshBinding();
		}

		#endregion

		#endregion

		#region AutoPackOrdersAndPrintLabels

		public bool AutoPackOrdersAndPrintLabels(INotifications notify)
		{
			Argument.GreaterThan(Orders.Count, 0, "Orders.Count");

			var success = true;
			var decoratedNotify = new NotificationsDecorator(notify);
			var disablePrinting = WarehouseDataRegistry.Instance.DisablePrintingLabelsOnAutoPack.Value;

			// auto-pack orders
			foreach (WhsOrder order in Orders)
			{
				success &= order.AutoPackAndPrintAllLabels(decoratedNotify, disablePrinting);

				if (decoratedNotify.ReportedFatalError)
				{
					break;
				}
			}

			return success;
		}

		#endregion

		#region ForceWhsPickUOMTypeAllocation

		public ForceWhsPickUOMTypeAllocation ForceWhsPickUOMTypeAllocation
		{
			get
			{
				var result = ForceWhsPickUOMTypeAllocation.None;
				if (WP_ForcePickByCaseUOMTypeAllocation)
				{
					result = ForceWhsPickUOMTypeAllocation.Case;
				}
				else if (WP_ForceSplitCaseUOMTypeAllocation)
				{
					result = ForceWhsPickUOMTypeAllocation.SplitCase;
				}

				return result;
			}
		}

		#endregion

		#region PickAreaOverrideReadOnly

		bool PickAreaOverrideReadOnly => !WP_WW_Whs.IsValid || IsFinalisedOrCancelledOrAllocated;

		#endregion

		#region Totals

		#region NumberOfOrders

		[ResourceStringData("WhsPick|NumberOfOrders", Caption = "Order Count")]
		public ZInt NumberOfOrders => Orders.Count;

		public ZPropertyInfo NumberOfOrdersInfo => GetZPropertyInfo(Schema.NumberOfOrders);

		#endregion

		#region TotalWeight

		[ResourceStringData("WhsPick|TotalWeight", Caption = "Total Weight")]
		public ZDecimal TotalWeight
		{
			get
			{
				if (totalWeight == null)
				{
					CalculateOrderTotals();
				}
				return totalWeight.Value;
			}
		}

		ZDecimal? totalWeight;

		public ZPropertyInfo TotalWeightInfo => GetZPropertyInfo(Schema.TotalWeight);

		#endregion

		#region TotalVolume

		[ResourceStringData("WhsPick|TotalVolume", Caption = "Total Volume")]
		public ZDecimal TotalVolume
		{
			get
			{
				if (totalVolume == null)
				{
					CalculateOrderTotals();
				}
				return totalVolume.Value;
			}
		}

		ZDecimal? totalVolume;

		public ZPropertyInfo TotalVolumeInfo => GetZPropertyInfo(Schema.TotalVolume);

		#endregion

		#region TotalLineQty

		[ResourceStringData("WhsPick|TotalLineQty", Caption = "Total Quantity that have been allocated, excluding component Qty", ShortCaption = "Total Qty")]
		public ZDecimal TotalLineQty
		{
			get
			{
				if (totalLineQty == null)
				{
					CalculateOrderTotals();
				}
				return totalLineQty.Value;
			}
		}

		ZDecimal? totalLineQty;

		public ZPropertyInfo TotalLineQtyInfo => GetZPropertyInfo(Schema.TotalLineQty);

		#endregion

		#region TotalComponentQty

		[ResourceStringData("WhsPick|TotalComponentQty", Caption = "Total Component Qty")]
		public ZDecimal TotalComponentQty
		{
			get
			{
				if (totalComponentQty == null)
				{
					CalculateOrderTotals();
				}
				return totalComponentQty.Value;
			}
		}

		ZDecimal? totalComponentQty;

		public ZPropertyInfo TotalComponentQtyInfo => GetZPropertyInfo(Schema.TotalComponentQty);

		#endregion

		#region TotalWeightUnit

		[ResourceStringData("WhsPick|TotalWeightUnit", Caption = "Total Weight Unit")]
		public ZString TotalWeightUnit => PackingRegistry.Instance.WeightUnit.Value;

		#endregion

		#region TotalVolumeUnit

		[ResourceStringData("WhsPick|TotalVolumeUnit", Caption = "Total Volume Unit")]
		public ZString TotalVolumeUnit => PackingRegistry.Instance.VolumeUnit.Value;

		#endregion

		#region Calculate Totals

		#region CalculateOrderTotals

		public void CalculateOrderTotals()
		{
			WhsPickByBOMHelper.AddFetchHintsForIsPickByBOMKitPickLine(Factory, OrderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(oi => oi.PickLines));

			var weightSum = 0m;
			var volumeSum = 0m;
			var lineQtySum = 0m;
			var componentQtySum = 0m;
			var qtyOfProductForTotalsCache = new Dictionary<OrgSupplierPart, ZDecimal>();

			foreach (var orderedInventory in OrderedInventories.Cast<WhsPickOrderedInventory>())
			{
				var qty = orderedInventory.PickLines.Where(pl => !pl.IsPickByBOMKitPickLine()).Sum(pl => pl.WZ_Units);
				if (qty > 0)
				{
					if (ShouldUpdateTotalComponentQty(orderedInventory))
					{
						componentQtySum += qty;
					}
					else
					{
						lineQtySum += qty;
					}

					var part = orderedInventory.SupplierPart;
					if (!qtyOfProductForTotalsCache.TryGetValue(part, out var result))
					{
						qtyOfProductForTotalsCache[part] = qty;
					}
					else
					{
						qtyOfProductForTotalsCache[part] = result + qty;
					}
				}
			}

			foreach (var qtyDelta in qtyOfProductForTotalsCache)
			{
				var supplierPart = qtyDelta.Key;
				weightSum += supplierPart.UnitConverter.Convert(qtyDelta.Value * supplierPart.OP_Weight, supplierPart.OP_WeightUQ, TotalWeightUnit);
				volumeSum += supplierPart.UnitConverter.Convert(qtyDelta.Value * supplierPart.OP_Cubic, supplierPart.OP_CubicUQ, TotalVolumeUnit);
			}

			totalWeight = weightSum;
			totalVolume = volumeSum;
			totalLineQty = lineQtySum;
			totalComponentQty = componentQtySum;
			RefreshBindingForTotals();
		}

		#endregion

		#region ShouldUpdateTotalComponentQty

		bool ShouldUpdateTotalComponentQty(WhsPickOrderedInventory orderedInventory) => orderedInventory.Owners.FirstOrDefault()?.WE_WE_ParentDocketLine.IsValid ?? false;

		#endregion

		#region UpdateTotals

		public void UpdateTotals(ZDecimal adjustQty, OrgSupplierPart part, WhsPickOrderedInventory orderedInventory)
		{
			if (totalWeight != null)
			{
				totalWeight += part.UnitConverter.Convert(adjustQty * part.OP_Weight, part.OP_WeightUQ, TotalWeightUnit);
				totalVolume += part.UnitConverter.Convert(adjustQty * part.OP_Cubic, part.OP_CubicUQ, TotalVolumeUnit);

				if (ShouldUpdateTotalComponentQty(orderedInventory))
				{
					totalComponentQty += adjustQty;
				}
				else
				{
					totalLineQty += adjustQty;
				}

				RefreshBindingForTotals();
			}
		}

		#endregion

		#region RefreshBindingForTotals

		void RefreshBindingForTotals()
		{
			TotalWeightInfo.RefreshBinding();
			TotalVolumeInfo.RefreshBinding();
			TotalLineQtyInfo.RefreshBinding();
			TotalComponentQtyInfo.RefreshBinding();
		}

		#endregion

		#endregion

		#endregion

		#region HasUnpickedPickLines

		public bool HasUnpickedPickLines => GetAllPickLines().Any(pl => pl.WZ_Units > 0 && !pl.IsPickedFromPutawayLocation && !pl.IsPickByBOMKitPickLine());

		#endregion

		#endregion

		#region Picking

		#region PickOrders

		#region Pick Orders

		public bool PickOrders()
		{
			return PickOrdersCore(Orders.ToArray<WhsPickableDocket>(), false);
		}

		public bool PickOrdersWithoutAutoAllocate(bool saveFactory = true)
		{
			return PickOrdersCore(Orders.ToArray<WhsPickableDocket>(), false, autoAllocate: false, saveFactory: saveFactory);
		}

		public bool PickOrdersOnlyIfPickNotInDb()
		{
			return PickOrdersCore(Orders.ToArray<WhsPickableDocket>(), true);
		}

		public bool PickOrders(WhsPickableDocket unattachedPickableDocket, bool saveFactory = true)
		{
			return PickOrders(new[] { unattachedPickableDocket }, saveFactory);
		}

		public bool PickOrders(WhsPickableDocket[] unattachedOrders, bool saveFactory = true)
		{
			if (Orders.Count > 0)
			{
				throw new InvalidOperationException("Cannot use pick.PickOrders(orders) on a pick that already has Orders. Use pick.PickOrders() instead.");
			}

			return PickOrdersCore(unattachedOrders, false, saveFactory: saveFactory);
		}

		bool PickOrdersCore(WhsPickableDocket[] whsOrders, bool onlyIfPickNotInDb, bool autoAllocate = true, bool saveFactory = true)
		{
			var pickability = RunPrePickValidation(whsOrders);
			var success = pickability.IsDocketPickable && (!onlyIfPickNotInDb || !pickability.PickAlreadyExists);

			// only add the orders once we pass prePick validation
			if (success)
			{
				AddOrders(whsOrders);

				if (WP_PickOption == WhsPickOption.Codes.Auto)
				{
					AutoPickOrders(autoAllocate, saveFactory);
				}
				else
				{
					ManuallyPickOrders(saveFactory);
				}

				ValidateFulfillmentRuleOnOrders();
			}
			else
			{
				OnPickOrdersFailed(pickability);
			}

			return success;
		}

		#region ShortfallQuantityHelper

		WhsOrderLineShortfallQuantityHelper ShortfallQuantityHelper
		{
			get { return shortfallQuantityHelper ?? (shortfallQuantityHelper = new WhsOrderLineShortfallQuantityHelper(Factory)); }
		}
		WhsOrderLineShortfallQuantityHelper shortfallQuantityHelper;

		#endregion

		#region IsAttachingPickSetter class

		class IsAttachingPickSetter : IDisposable
		{
			public IsAttachingPickSetter(IEnumerable<WhsPickableDocket> orders)
			{
				Disposables = orders.Select(o => o.SetIsAttachingToPick()).ToArray();
			}

			readonly IEnumerable<IDisposable> Disposables;

			void IDisposable.Dispose()
			{
				foreach (var disposable in Disposables)
				{
					disposable.Dispose();
				}
			}
		}

		#endregion

		#region IsDetachingPickSetter class

		class IsDetachingPickSetter : IDisposable
		{
			public IsDetachingPickSetter(IEnumerable<WhsPickableDocket> orders)
			{
				Disposables = orders.Select(o => o.SetIsDetachingToPick()).ToArray();
			}

			readonly IEnumerable<IDisposable> Disposables;

			void IDisposable.Dispose()
			{
				foreach (var disposable in Disposables)
				{
					disposable.Dispose();
				}
			}
		}

		#endregion

		#region BulkAttachingPickSemaphore

		Semaphore BulkAttachingPickSemaphore
		{
			get { return bulkAttachingPickSemaphore ?? (bulkAttachingPickSemaphore = new Semaphore()); }
		}

		Semaphore bulkAttachingPickSemaphore;

		#endregion

		#region BulkDetachingPickSemaphore

		public IDisposable SetBulkDetachingFromPick()
		{
			return new SemaphoreManager(BulkDetachingPickSemaphore);
		}

		Semaphore BulkDetachingPickSemaphore
		{
			get { return bulkDetachingPickSemaphore ?? (bulkDetachingPickSemaphore = new Semaphore()); }
		}

		Semaphore bulkDetachingPickSemaphore;

		#endregion

		#region SuspendCrossDockAllocationSemaphore

		Semaphore SuspendCrossDockAllocationSemaphore
		{
			get { return suspendCrossDockAllocationSemaphore ?? (suspendCrossDockAllocationSemaphore = new Semaphore()); }
		}

		Semaphore suspendCrossDockAllocationSemaphore;

		#endregion

		#region IsUpdatingAllocationLogSuspended

		public bool IsUpdatingAllocationLogSuspended
		{
			get { return SuspendUpdateAllocationLogSemaphore.IsSuspended; }
		}

		public IDisposable SuspendUpdatingAllocationLog() => new SemaphoreManager(SuspendUpdateAllocationLogSemaphore);

		Semaphore SuspendUpdateAllocationLogSemaphore
		{
			get { return suspendUpdateAllocationLogSemaphore ?? (suspendUpdateAllocationLogSemaphore = new Semaphore()); }
		}

		Semaphore suspendUpdateAllocationLogSemaphore;

		#endregion

		#region ValidateFulfillmentRuleOnOrders

		void ValidateFulfillmentRuleOnOrders()
		{
			foreach (WhsPickableDocket order in Orders)
			{
				order.Validation.ValidateWD_WhsOrderFulfillmentRule();
			}
		}

		#endregion

		#endregion

		#region IsFulfillmentRulesMet

		public bool IsFulfillmentRulesMet()
		{
			foreach (WhsPickableDocket order in Orders)
			{
				if (!order.IsFulfillmentRuleMet)
				{
					return false;
				}
			}
			return true;
		}

		#endregion

		#region RunPrePickValidation

		DocketPickabilityEventArgs RunPrePickValidation(WhsPickableDocket[] whsOrders)
		{
			DocketPickabilityEventArgs result = null;

			if (whsOrders.Length == 0)
			{
				result = new DocketPickabilityEventArgs(false, Res.GetString("37ef0bea-1f6a-459d-9fcc-b05cd4fd9700", "No Orders are attached to the Pick."), NotificationTypes.Error);
			}

			foreach (WhsPickableDocket order in whsOrders)
			{
				result = order.GetPickability(this);
				if (!result.IsDocketPickable)
				{
					break;
				}
			}

			return result;
		}

		void OnPickOrdersFailed(DocketPickabilityEventArgs e)
		{
			if (PickOrdersFailed != null)
			{
				PickOrdersFailed(this, e);
			}
		}

		public event EventHandler<DocketPickabilityEventArgs> PickOrdersFailed;

		#endregion

		#region class DocketPickabilityEventArgs

		public class DocketPickabilityEventArgs : EventArgs
		{
			public DocketPickabilityEventArgs()
				: this(true, "", NotificationTypes.None)
			{
			}

			public DocketPickabilityEventArgs(bool isDocketPickable, ZString message, NotificationTypes messageType)
			{
				Message = message;
				MessageType = messageType;
				IsDocketPickable = isDocketPickable;
			}

			public ZString Message { get; set; }
			public bool IsDocketPickable { get; set; }
			public bool PickAlreadyExists { get; set; }
			public NotificationTypes MessageType { get; set; }
		}

		#endregion

		#region AutoPickOrders

		void AutoPickOrders(bool autoAllocate, bool saveFactory)
		{
			const bool includeBizOsWithChanges = true;
			var autoAllocateErrorMessage = "";

			if (autoAllocate)
			{
				Factory.SynchroniseCachedBusinessObjectsWithDB<WhsInventoryView>(includeBizOsWithChanges, false); // if Inventory is modified in another factory, the picking will fail
				Factory.SynchroniseCachedBusinessObjectsWithDB<WhsDocketLine>(false, false);

				var notifications = new NotificationsDecorator(NotificationSubscriber);
				AutoAllocateItems(notifications);

				var notificationsMessage = notifications.MessagesAsString;
				if (!string.IsNullOrEmpty(notificationsMessage))
				{
					autoAllocateErrorMessage = Res.GetString("ce23062b-b077-4cd8-98ca-b37b78b8d0d7", "Issue occurred during allocation: {0}", notificationsMessage);
				}
			}

			var fulFillmentRulesMet = IsFulfillmentRulesMet();

			if (WP_IsAwaitingReplenishment || IsAnyStockAllocated)
			{
				if (!WP_IsAwaitingReplenishment && Warehouse.WW_AutoPrintPickingSlip && fulFillmentRulesMet)
				{
					WP_PickStatus = PickStatus.Codes.PickSlip;
				}

				ReduceOverpickedStockForWorkOrders();

				if (HasErrors || IsThereAnyFatalNotification())
				{
					GenerateOnPickOrdersFailedArgs();
				}
				else
				{
					if (!saveFactory)
					{
						UpdatePickStatusIfRequired(); // Status is usually updated on saving
					}

					var saved = !saveFactory || AttemptSave(fulFillmentRulesMet);
					if (saved && !WP_IsAwaitingReplenishment && WP_PickStatus != PickStatus.Codes.Building)
					{
						AllocatePackageLabels(saveFactory);
					}
				}
			}
			else
			{
				OnAutoPickAttempt(NoStockWarningMessage, false, fulFillmentRulesMet, autoAllocateErrorMessage);
			}
		}

		#region IsAnyStockAllocated

		bool IsAnyStockAllocated
		{
			get
			{
				foreach (WhsPickOrderedInventory orderedInventory in OrderedInventories)
				{
					if (orderedInventory.PickLineQuantity > 0)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region ReduceOverpickedStockForWorkOrders

		void ReduceOverpickedStockForWorkOrders()
		{
			if (IsWorkOrderPick)
			{
				foreach (var workOrder in Orders.OfType<WhsWorkOrder>())
				{
					foreach (WhsWorkOrderLine line in workOrder.AllLines)
					{
						line.ReduceOverpickedStock();
					}
				}
			}
		}

		#endregion

		#region IsThereAnyFatalNotification

		bool IsThereAnyFatalNotification()
		{
			bool result = false;
			if (NotificationsIncludingChildren.Any())
			{
				IEnumerable<INotification> fatalNotifications = new ZNotificationCollector(this, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetFatalNotifications();
				if (fatalNotifications.Any())
				{
					result = true;
				}
			}
			return result;
		}

		#endregion

		#region GenerateOnPickOrdersFailedArgs

		void GenerateOnPickOrdersFailedArgs()
		{
			if (IsOrderHavingErrorsInWeightSentOrCubicSent())
			{
				var failedEventArgs = new DocketPickabilityEventArgs(false, Res.GetString("WhsPick|AutoPickOrders|OrderHasErrorInWeightSentOrCubicSent",
@"Please check your order lines. One or multiple products have excessive Weight / Volume.
To fix the Weight / Volume go to Maintain > Warehouse > Products and amend the gross Weight / Volume for these products."), NotificationTypes.Error);
				OnPickOrdersFailed(failedEventArgs);
			}
			else
			{
				var failedEventArgs = new DocketPickabilityEventArgs(false,
				Res.GetString("WhsPick|AutoPickOrders|ErrorMessageForOrderHasError", @"Please check your Order Lines."), NotificationTypes.Error);
				OnPickOrdersFailed(failedEventArgs);
			}
		}

		bool IsOrderHavingErrorsInWeightSentOrCubicSent()
		{
			return NotificationsIncludingChildren.Any(n => IsNotificationErrorInWeightSentOrCubicSent(n));
		}

		bool IsNotificationErrorInWeightSentOrCubicSent(INotification notification)
		{
			var propertyNotification = notification as PropertyNotification;
			return propertyNotification != null && propertyNotification.Type == CargoWise.ComponentModel.NotificationType.Error
				&& (propertyNotification.PropertyName == WhsDocketSchema.WD_WeightSent.Name || propertyNotification.PropertyName == WhsDocketSchema.WD_CubicSent.Name);
		}

		#endregion

		#region AttemptSave

		bool AttemptSave(bool fulFillmentRulesMet)
		{
			var success = true;
			Exception saveException = null;
			var notificationBuffer = new NotificationHandlerBuffer();

			try
			{
				Factory.Save(); // needed for the generation of WP_PickNo + the printing of the Pick docs
			}
			catch (ZSaveException exception)
			{
				saveException = exception;
				ZExceptionReporting.HandleSaveException(exception, notificationBuffer);
			}
			catch (ZCannotSaveException exception)
			{
				saveException = exception;
				ZExceptionReporting.HandleSaveException(exception, notificationBuffer);
			}

			bool savedOk = saveException == null;
			if (savedOk)
			{
				string message = Res.GetString("8e99ef44-3667-4b79-85c4-7ecbd1f0deb6", "Pick {0} has been created.", WP_PickNo);
				if (!fulFillmentRulesMet)
				{
					message += "\r\n\r\n" + Res.GetString("668a7911-9c9c-489f-b6ab-60c2afbda41d",
@"This Pick has been created with a status of 'BUILDING'.
The Fulfillment Rules for all Orders on the Pick have not been met.
Pick Documentation cannot be printed until the Fulfillment Rules have been satisfied or overridden.");
				}
				else if (WP_IsAwaitingReplenishment)
				{
					message = Res.GetString("5A7CA184-23B9-4E50-B990-8D3E0E7E63AE", "Pick {0} has been created and is awaiting replenishment.", WP_PickNo);
				}

				if (WP_PickOption == WhsPickOption.Codes.Auto)
				{
					OnAutoPickAttempt(message, true, fulFillmentRulesMet);
				}
				else
				{
					OnManualPickSucceeded();
				}
			}
			else
			{
				success = false;

				// only PickLines have changes, therefore non-PickLine triggers (eg. Inventory, DocketLine etc.) will not fail  thus only check for the PickLine trigger failure.
				var saveErrorMessage = saveException.IsPreventOverCommitStockViaPickLineTriggerException()
					? WhsExceptionHandler.PreventOverCommitOfStockViaPickLineTriggerMsgForUser
					: notificationBuffer.GetErrors();

				if (SaveFailureEvent != null)
				{
					SaveFailureEvent(this, new SaveFailureEventArgs(saveErrorMessage));
				}
			}

			return success;
		}

		public event EventHandler<SaveFailureEventArgs> SaveFailureEvent;

		#region class SaveFailureEventArgs

		public class SaveFailureEventArgs : EventArgs
		{
			public SaveFailureEventArgs(ZString message)
			{
				Message = message;
			}
			public readonly ZString Message;
		}

		#endregion

		#region class NotificationHandlerBuffer

		class NotificationHandlerBuffer : INotificationHandler
		{
			void INotificationHandler.ReportError(string message, string caption, string errorContext, Exception exception)
			{
				ReportInformation(message, caption);
			}

			public void ReportInformation(string message, string caption)
			{
				Errors.Add(message);
			}

			List<string> Errors
			{
				get { return errors ?? (errors = new List<string>()); }
			}

			List<string> errors;

			public string GetErrors()
			{
				return string.Join(System.Environment.NewLine, Errors);
			}
		}

		#endregion

		#endregion

		#region OnAutoPickAttempt

		void OnAutoPickAttempt(string message, bool stockWasAllocated, bool fulfillmentRulesMet, string autoAllocateErrorMessage = null)
		{
			if (AutoPickAttempt != null)
			{
				AutoPickAttempt(this, new AutoPickEventArgs(message, stockWasAllocated, fulfillmentRulesMet, WP_IsAwaitingReplenishment, autoAllocateErrorMessage));
			}
		}

		public event EventHandler<AutoPickEventArgs> AutoPickAttempt;

		#region class AutoPickEventArgs

		public class AutoPickEventArgs : EventArgs
		{
			public AutoPickEventArgs(ZString message, bool stockWasAllocated, bool fulfillmentRulesMet, bool isPickWaitingReplenishment, ZString autoAllocateErrorMessage)
			{
				Message = message;
				StockWasAllocated = stockWasAllocated;
				FulfillmentRulesMet = fulfillmentRulesMet;
				IsPickWaitingReplenishment = isPickWaitingReplenishment;
				AutoAllocateErrorMessage = autoAllocateErrorMessage;
			}

			public readonly ZString Message;
			public readonly bool StockWasAllocated;
			public readonly bool FulfillmentRulesMet;
			public readonly bool IsPickWaitingReplenishment;
			public readonly ZString AutoAllocateErrorMessage;
		}

		#endregion

		#endregion

		#region AllStockAlreadyAllocatedWarningMessage

		static string AllStockAlreadyAllocatedWarningMessage
			=> Res.GetString("WhsPick|AllStockAlreadyAllocatedWarningMessage", "No new stock could be allocated for this pick as all stock has already been allocated.");

		#endregion

		#region NoStockAllocatedWarningMessage

		static string NoStockAllocatedWarningMessage
			=> Res.GetString("WhsPick|NoStockAllocatedWarningMessage", "No stock could be found for this pick either because there is no available stock in the warehouse or there was no stock which meets configured allocation rules.");

		#endregion

		#region NoStockWarningMessage

		public static string NoStockWarningMessage
		{
			get
			{
				return Res.GetString("WhsPick|NoStockWarningMessage",
@"{0}

You have two options:
1. You can Save this Order and the Pick will be created with nothing allocated.
2. You can Cancel this Order (without Saving) and no Pick will be created.", NoStockAllocatedWarningMessage);
			}
		}

		#endregion

		#endregion

		#region ManuallyPickOrders

		void ManuallyPickOrders(bool saveFactory)
		{
			if (WP_PickOption == WhsPickOption.Codes.ManualWithAutoAllocate)
			{
				AutoAllocateItems();
			}

			if (HasErrors || IsThereAnyFatalNotification())
			{
				GenerateOnPickOrdersFailedArgs();
			}
			else if (saveFactory)
			{
				AttemptSave(IsFulfillmentRulesMet()); // needed for the generation of WP_PickNo + opening the pick form in a diff factory
			}
		}

		void OnManualPickSucceeded()
		{
			if (ManualPickSucceeded != null)
			{
				ManualPickSucceeded(this, EventArgs.Empty);
			}
		}

		public event EventHandler ManualPickSucceeded;

		#endregion

		#endregion

		#region PickOrdersForBond

		/// <summary>
		/// This method will not print pick slips nor save the pick as necessary for UXML import.
		/// </summary>
		public void PickOrdersForBond(WhsOrder unattachedOrder, IEnumerable<WhsInventoryView> additionalInventoryToMakeAvailableForPick)
		{
			PickOrdersForBondCore(unattachedOrder, additionalInventoryToMakeAvailableForPick);
		}

		void PickOrdersForBondCore(WhsOrder unattachedOrder, IEnumerable<WhsInventoryView> additionalInventoryToMakeAvailableForPick)
		{
			DocketPickabilityEventArgs pickability = RunPrePickValidation(new[] { unattachedOrder });
			if (pickability.IsDocketPickable)
			{
				Orders.Add(unattachedOrder);
				AutoAllocateItemsAndIncludeStockFromInventory(additionalInventoryToMakeAvailableForPick);
			}
			else
			{
				OnPickOrdersFailed(pickability);
			}
		}

		void AutoAllocateItemsAndIncludeStockFromInventory(IEnumerable<WhsInventoryView> additionalInventoryToMakeAvailableForPick)
		{
			try
			{
				allInventories = AllInventories.Concat(additionalInventoryToMakeAvailableForPick ?? Enumerable.Empty<WhsInventoryView>()).ToArray();
				AutoAllocateItems();
			}
			finally
			{
				ClearAllInventoriesCache();
			}
		}

		#endregion

		#region AllocateCrossDockLines

		void AllocateCrossDockLines()
		{
			foreach (WhsPickOrderedInventory orderedInventory in OrderedInventories)
			{
				PickFromCrossDockedLines(orderedInventory, orderedInventory.Owners);
			}
		}

		void AllocateCrossDockLines(WhsPickableDocket order)
		{
			AllocateCrossDockLines(new HashSet<ZGuid>(new[] { order.PK }));
		}

		void AllocateCrossDockLines(HashSet<ZGuid> newOrderPks)
		{
			foreach (WhsPickOrderedInventory orderedInventory in OrderedInventories)
			{
				PickFromCrossDockedLines(orderedInventory, orderedInventory.Owners.Where(pickableDocketLine => newOrderPks.Contains(pickableDocketLine.WE_WD)));
			}
		}

		static void PickFromCrossDockedLines(WhsPickOrderedInventory orderedInventory, IEnumerable<WhsPickableDocketLine> lines)
		{
			orderedInventory.ValidationQuantityShortWarningMessage = "";

			bool isPickByUOM = orderedInventory.Pick.IsPickByUOMEnabled;

			var availableInventoryUsed = new HashSet<WhsPickAvailableInventory>();

			foreach (var line in lines)
			{
				// if order was unpicked, pick lines could only have been created for these order lines through cross docking.
				var pickLineQuery = new ZQuery();
				pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_WE_TransactionLine, line.PK);

				foreach (var pickLine in line.Factory.Load<WhsPickLine>(pickLineQuery))
				{
					if (pickLine.IsReserveLine)
					{
						var availableInventory = PickInventoryDirectFromCrossDock(orderedInventory, pickLine.Inventory, pickLine);
						if (availableInventory != null)
						{
							availableInventoryUsed.Add(availableInventory);
						}

						if (!isPickByUOM)
						{
							pickLine.WZ_F3_NKAllocatedPackType = "";
						}
					}
					else
					{
						// if a pick line exists before picking that is not reserved, something is wrong and we should just delete the offending pickline.
						pickLine.Delete();
					}
				}
			}

			if (isPickByUOM)
			{
				foreach (var availableInventory in availableInventoryUsed)
				{
					availableInventory.SplitByPackType();
				}
			}
		}

		/// <summary>
		/// Invoked when the pick is first created or attaching an order. Not invoked for manual allocation.
		/// </summary>
		static WhsPickAvailableInventory PickInventoryDirectFromCrossDock(WhsPickOrderedInventory orderedInventory, WhsInventoryView inventory, WhsPickLine reservedPickLine)
		{
			// inventory.WI_AvailableToPickQuantity takes into account reserved inventory, including the currently
			// reserved pickLine therefore WI_AvailableToPickQuantity only needs to be greater than or equal to Zero (provided the inventory is available).
			// reserved pickLines become picked automatically as soon as order is attached to pick, we just need to deallocate stock if it cannot be picked.
			var quantityToPick = inventory.IsAvailable && inventory.WI_AvailableToPickQuantity >= 0m
				? reservedPickLine.ReservedQuantity
				: ZDecimal.Zero;

			var availableInventory = FindAvailableInventory(orderedInventory, inventory);
			bool isInventoryAvailable = (availableInventory != null);
			if (!isInventoryAvailable || quantityToPick == 0m)
			{
				// no inventory to commit to pick, do not delete the reserving pickline so that we can retain the reservation history.
				reservedPickLine.ClearOutPickingValuesForReservedLine();
				if (isInventoryAvailable) // this means quantityToPick was zero
				{
					// if we can pick some, but not all that was reserved, simply pick as much as we can.
					if (inventory.IsAvailable)
					{
						// Could not pick whole amount, so just pick what is available.
						// Now that the reserved line has been reduced to zero, we can see the actual available quantity.
						reservedPickLine.WZ_Units = inventory.WI_AvailableToPickQuantity;
					}
				}
			}

			// add validation error if we did not pick the full amount.
			if (quantityToPick == 0m)
			{
				orderedInventory.ValidationQuantityShortWarningMessage =
					Res.GetString("bae9dc17-bb76-4f4d-929d-3ed6ced0f211",
					"Some product could not be picked from cross dock allocations because either the product has not yet arrived (and the receipt is not finalized), the product is not available for picking, or it has already been picked");
			}

			if (isInventoryAvailable)
			{
				// when the user allocates stock manually (via pick screen allocate checkbox or qty adjustment), that process
				// invokes the below Allocate/Validate method. We should do the same here for consistency.
				if (reservedPickLine.ReservedQuantity > 0m)
				{
					availableInventory.AllocateAndValidateOrderedInventory(reservedPickLine.ReservedQuantity);
					ReleaseLinesOnPickManager.NotifyChange(reservedPickLine.Factory, reservedPickLine, reservedPickLine.ReservedQuantity);
				}
			}

			return availableInventory;
		}

		static WhsPickAvailableInventory FindAvailableInventory(WhsPickOrderedInventory orderedInventory, WhsInventoryView inventory)
		{
			foreach (WhsPickAvailableInventory availableInventory in orderedInventory.AvailableInventories)
			{
				if (availableInventory.Inventory.Contains(inventory))
				{
					return availableInventory;
				}
			}

			return null;
		}

		#endregion

		#region AutoAllocateItems, ClearAllocatedItems

		public string AutoAllocateItems(INotifications notifications = null, bool shouldAddFetchHints = true)
		{
			var result = AutoAllocateItems(ZDateTimeOffset.Empty, notifications: notifications, shouldAddFetchHints: shouldAddFetchHints);

			if (!IsWorkOrderPick)
			{
				var bomProductsOnPick = GetBOMProductsOnPick();
				if (bomProductsOnPick.Any())
				{
					var bomResult = RebuildOrderedInventoriesAndReAllocateForComponents();

					// If bom allocation succeeded, display no warning
					if (!string.IsNullOrEmpty(result) && string.IsNullOrEmpty(bomResult))
					{
						result = string.Empty;
					}
				}
			}

			return result;
		}

		public AllocationResult AutoAllocateItems(IEnumerable<WhsPickOrderedInventory> whsOrderedInventories, INotifications notifications = null)
		{
			Argument.NotNull(whsOrderedInventories, nameof(whsOrderedInventories));
			return AutoAllocateItemsCore(
				ZDateTimeOffset.Empty,
				() =>
				{
					var result = whsOrderedInventories.ToArray();
					Array.Sort(result, new SortItemsForPicking());
					return result;
				},
				notifications: notifications,
				shouldAddFetchHints: true);
		}

		AllocationResult AutoAllocateItemsCore(ZDateTimeOffset dateToFilterInventory, Func<WhsPickOrderedInventory[]> getSortedOrderedInventories, INotifications notifications = null, bool shouldAddFetchHints = true)
		{
			if (!dateToFilterInventory.IsEmpty)
			{
				InventoriesFilterDate = dateToFilterInventory;
			}

			var sortedOrderedInventories = getSortedOrderedInventories();

			if (shouldAddFetchHints)
			{
				AddFetchHintsForAutoAllocateItemsCore(sortedOrderedInventories);
			}

			AllocationResult allocationResult;
			using (new UpdatingTotalsSemaphoreManager(this))
			using (new AutoAllocatingItemsSessionManager(this))
			{
				allocationResult = AllocateUsingAllocationRules(sortedOrderedInventories, notifications);
			}

			ValidatePickLineQuantityForAvailableInventories(sortedOrderedInventories);
			AssignOrResetPickWaitingReplenishmentStatus(sortedOrderedInventories, validateShortfall: true);

			return allocationResult;
		}

		public string AutoAllocateItems(ZDateTimeOffset dateToFilterInventory, INotifications notifications = null, bool shouldAddFetchHints = true)
		{
			var whsOrderedInventories = GetOrderedInventoriesWithOwners();
			var allocationResult = AutoAllocateItemsCore(
				dateToFilterInventory,
				() =>
				{
					OrderedInventories.Sort(new SortItemsForPicking());
					return GetOrderedInventoriesWithOwners();
				},
				notifications,
				shouldAddFetchHints);

			string result;

			if (allocationResult == AllocationResult.NoStockAllocated)
			{
				if (whsOrderedInventories.All(i => i.QuantityShort == 0))
				{
					result = AllStockAlreadyAllocatedWarningMessage;
				}
				else
				{
					result = NoStockAllocatedWarningMessage;
				}
			}
			else
			{
				result = string.Empty;
			}

			return result;
		}

		public void AddFetchHintsForAutoAllocateItems()
		{
			AddFetchHintsForAutoAllocateItemsCore(GetOrderedInventoriesWithOwners());
		}

		void AddFetchHintsForAutoAllocateItemsCore(IEnumerable<WhsPickOrderedInventory> whsOrderedInventories)
		{
			var locationPKs = AllInventories.Select(i => i.WI_WL).Distinct().ToArray();
			var locationSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsLocationViewSchema.WLV_WR) { AllowTableValuedParameters = true };
			locationSubQuery.AddToFilter(WhsLocationViewSchema.PK, SQLComparisonOperator.Equal, locationPKs);

			var rowQuery = new ZDBOnlyQuery(typeof(WhsRow));
			rowQuery.AddSubQuery(locationSubQuery, JoinCondition.And);
			Factory.AddFetchHint(WhsRowSchema.Instance, rowQuery);

			var distinctProducts = IEnumerableExtensions.DistinctBy(whsOrderedInventories, inv => inv.SupplierPartPK).Select(inv => inv.Product).Where(product => product != null);
			distinctProducts.ForEach(product => Factory.AddFetchHint(WhsProductParamsByWhsAndClientSchema.W3_OP, product.Parent.PK));
		}

		void AssignOrResetPickWaitingReplenishmentStatus(WhsPickOrderedInventory[] whsOrderedInventories, bool validateShortfall)
		{
			if (whsOrderedInventories.Any(o => o.IsWaitingReplenishment()))
			{
				WP_IsAwaitingReplenishment = true;

				if (WP_PickStatus != PickStatus.Codes.Created && WP_PickStatus != PickStatus.Codes.Building)
				{
					WP_PickStatus = PickStatus.Codes.Created;
				}
			}
			else
			{
				WP_IsAwaitingReplenishment = false;
			}

			if (validateShortfall)
			{
				foreach (var owner in whsOrderedInventories.SelectMany(ordInv => ordInv.Owners))
				{
					using (owner.SuspendTotalUnitsValidationOnSettingShortfallQuantity())
					{
						owner.Validation.ValidateWE_ShortfallQuantityCached();
					}
				}

				foreach (WhsPickableDocket order in Orders)
				{
					order.Validation.ValidateWD_TotalUnits();
				}
			}
		}

		AllocationResult AllocateUsingAllocationRules(IEnumerable<WhsPickOrderedInventory> orderedInventories, INotifications notifications)
		{
			var allocationEngine = ObjectFactory.Get<IAllocationEngineManager>();
			var newPickStrategy = GetNewPickStrategy();

			using (newPickStrategy.CacheQuantityUnPicked())
			{
				return allocationEngine.Allocate(this, notifications ?? NotificationSubscriber, newPickStrategy, orderedInventories);
			}
		}

		void ValidatePickLineQuantityForAvailableInventories(IEnumerable<WhsPickOrderedInventory> whsOrderedInventories)
		{
			// we need this for bonded orders, because some errors might be added when we pick line-by-line
			foreach (var orderedInventory in whsOrderedInventories)
			{
				foreach (var availableInventory in orderedInventory.AvailableInventories.Cast<WhsPickAvailableInventory>().Where(i => i.HasErrors))
				{
					availableInventory.Validation.Validate_PickLineQuantity();
				}
			}
		}

		public void ClearAllocatedItems()
		{
			bool originalIsAlterPick = IsAlterPick;

			try
			{
				IsAlterPick = false;

				using (new UpdatingTotalsSemaphoreManager(this))
				{
					foreach (WhsPickOrderedInventory orderedInventory in OrderedInventories)
					{
						orderedInventory.ClearAllocations();
					}
				}
			}
			finally
			{
				IsAlterPick = originalIsAlterPick;
			}
		}

		#region RebuildOrderedInventoriesAndReAllocateForComponents

		string RebuildOrderedInventoriesAndReAllocateForComponents()
		{
			var itemAllocatedMessage = NoStockAllocatedWarningMessage;
			var orderLinesToAdd = new List<WhsPickableDocketLine>();
			var componentProductsForNewLines = new HashSet<ZGuid>();
			var orders = Orders.Cast<WhsPickableDocket>().ToArray();
			foreach (var order in orders.Where(o => !o.IsCustomsTransaction))
			{
				var newLines = AddLinesForComponentIfRequired(order, componentProductsForNewLines);
				orderLinesToAdd.AddRange(newLines);
			}

			if (orderLinesToAdd.Count > 0)
			{
				if (componentProductsForNewLines.Count > 0)
				{
					AddComponentsToAllInventories(componentProductsForNewLines);
				}
				OrderedInventories.AddNewFromOrderLines(orderLinesToAdd);
				itemAllocatedMessage = AutoAllocateItems(ZDateTimeOffset.Empty);
				foreach (var order in orders)
				{
					CondenseChildComponentLinesToAllocatedStock(order);
				}
			}

			if (orders.Any(order => order.Lines.Cast<WhsPickableDocketLine>().Where(line => line.IsComponentLineOnSalesOrder).Any(l => l.PickLines.Any(pl => !pl.IsInDatabase))))
			{
				CreateReceives(orders);
			}

			return itemAllocatedMessage;
		}

		void AddComponentsToAllInventories(IEnumerable<ZGuid> componentProductsForNewLines)
		{
			var bomPartsOnOrderedInventories = OrderedInventories.Cast<WhsPickOrderedInventory>().Select(o => o.SupplierPartPK).Distinct();
			var bomPartsToAdd = componentProductsForNewLines.Except(bomPartsOnOrderedInventories).ToArray();

			if (bomPartsToAdd.Length > 0)
			{
				var bomInventoryQuery = GetInventoriesQueryForNonFinalisedPick(Orders.ToArray<WhsPickableDocket>(), bomPartsToAdd);
				var bomInventories = Factory.Load<WhsInventoryView>(bomInventoryQuery);
				allInventories = AllInventories.Append(bomInventories).ToArray();
				AppendToInventoryCacheForPickByBOM(bomInventories);
				bomInventories.ForEach(AddFetchHintForDocketRelatedToInventory);
			}
		}

		void CondenseChildComponentLinesToAllocatedStock(WhsPickableDocket order)
		{
			// Find all orderlines with child component lines
			var parentlinesToBeChecked = order.Lines.Cast<WhsPickableDocketLine>().Where(l => l.IsBOMProductPickedOnSalesOrder && !LineHasAnyOrderedAttributes(l)).ToList();
			// For each parent line find child lines
			foreach (var parentLine in parentlinesToBeChecked)
			{
				var childProductGroups = parentLine.ChildComponentLines.GroupBy(l => l.WE_OP).ToArray();
				foreach (var childProductGroup in childProductGroups)
				{
					// merge child lines + point picklines to remaining child line
					if (childProductGroup.Count() > 1)
					{
						var firstLine = childProductGroup.FirstOrDefault(l => l.IsInDatabase) ?? childProductGroup.First();
						var pickLineQty = firstLine.PickLines.Sum(pl => pl.WZ_Units);
						var childLinesToRemove = childProductGroup.Where(li => li.PK != firstLine.PK).ToArray();
						foreach (var pickLine in childLinesToRemove.SelectMany(l => l.PickLines).ToArray())
						{
							pickLineQty += pickLine.WZ_Units;
							pickLine.WZ_WE_TransactionLine = firstLine.PK;
						}
						childLinesToRemove.ForEach(ctr => ctr.Delete());
						firstLine.WE_TransactionQuantity = pickLineQty;
					}
				}

				// reduce qty on child component lines to fullyallocatedkits as long as the ordered kit qty (including already completed kits) <= picked kit qty
				var requiredKits = parentLine.QuantityNotMet;
				if (requiredKits < parentLine.TotalPickLineQuantityFromComponents)
				{
					foreach (WhsOrderLine childOrderLine in parentLine.ChildComponentLines)
					{
						childOrderLine.ReduceOverpickedChildStock(requiredKits.ToZInt());
					}
				}
			}
		}

		void CreateReceives(WhsPickableDocket[] orders)
		{
			var existingReceives = Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, PK)).ToDictionary(d => d.WD_OH_Client);
			var pickLinesByProduct = new Dictionary<(ZGuid, ZGuid), List<WhsPickLine>>();
			var orderedInventoriesLookup = OrderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(oi => oi.Owners.Select(o => (o, oi))).ToDictionary(v => v.o, v => v.oi);

			foreach (var clientGroup in orders.GroupBy(o => o.WD_OH_Client))
			{
				var newReceiveLines = new List<WhsReceiveLine>();
				var (receive, existingReceiveLineMap, componentInventoryMap) = GetOrCreateReceive(existingReceives, clientGroup.Key);

				foreach (var order in clientGroup)
				{
					var parentLines = order.Lines.Cast<WhsPickableDocketLine>().Where(l => l.SupplierPart.OP_IsComponentPickedOnSalesOrder && l.ChildComponentLines.Count > 0);
					foreach (var parentLine in parentLines)
					{
						var kitQuantity = parentLine.TotalPickLineQuantityFromComponents;
						if (kitQuantity > 0)
						{
							var componentLines = parentLine.ChildComponentLines;
							var (receiveLine, isNewLine, additionalQuantity) = GetOrCreateReceiveLine(receive, parentLine, existingReceiveLineMap, componentInventoryMap, componentLines.First().PK, kitQuantity);
							if (additionalQuantity > 0)
							{
								if (isNewLine)
								{
									var newInventories = new WhsInventoryView[] { (WhsInventoryView)receiveLine.Inventory.FirstOrDefault() };
									orderedInventoriesLookup[parentLine].AvailableInventories.MergeInventoryIntoThisCollection(newInventories);
									newReceiveLines.Add(receiveLine);
								}

								var pickLine = CreatePickLine(additionalQuantity, receiveLine, parentLine);
								if (!pickLinesByProduct.TryGetValue((order.PK, receiveLine.WE_OP), out var pickLines))
								{
									pickLines = new List<WhsPickLine>();
									pickLinesByProduct[(order.PK, receiveLine.WE_OP)] = pickLines;
								}
								pickLines.Add(pickLine);

								RecreateBOMInventoryPivots(componentLines, componentInventoryMap, receiveLine);

								parentLine.ClearReleaseLines();
								parentLine.Validation.ValidateWE_ShortfallQuantityCached();
								parentLine.WE_ShortfallQuantityCachedInfo.RefreshBinding();
							}
						}
					}
				}

				receive.Lines.AddRange(newReceiveLines);
			}

			if (IsPickByUOMEnabled)
			{
				foreach (var pickLines in pickLinesByProduct.Values)
				{
					PickLinePackAssigner.AssignPackTypes(pickLines, ForceWhsPickUOMTypeAllocation);
				}
			}
		}

		(WhsReceive Receive, Dictionary<ZGuid, WhsReceiveLine> ExistingReceiveLineMap, Dictionary<ZGuid, WhsBOMInventoryPivot> ComponentInventoryMap) GetOrCreateReceive(
			Dictionary<ZGuid, WhsReceive> existingReceives,
			ZGuid clientGroupKey)
		{
			Dictionary<ZGuid, WhsReceiveLine> existingReceiveLineMap;
			Dictionary<ZGuid, WhsBOMInventoryPivot> componentInventoryMap;
			if (existingReceives.TryGetValue(clientGroupKey, out var receive))
			{
				existingReceiveLineMap = receive.Lines.Cast<WhsReceiveLine>().ToDictionary(l => l.PK);
				componentInventoryMap = receive.Lines.SelectMany(l => l.BOMComponentLinks).ToDictionary(l => l.WIP_WE_ComponentLine);
			}
			else
			{
				existingReceiveLineMap = new Dictionary<ZGuid, WhsReceiveLine>();
				componentInventoryMap = new Dictionary<ZGuid, WhsBOMInventoryPivot>();
				receive = Factory.New<WhsReceive>();
				receive.WD_OH_Client = clientGroupKey;
				receive.WD_WW_Whs = WP_WW_Whs;
				receive.WD_ArrivalDate = ZDateTimeOffset.Now;
				receive.WD_WP_ParentPickForReceive = PK;
			}
			return (receive, existingReceiveLineMap, componentInventoryMap);
		}

		(WhsReceiveLine Line, bool IsNewLine, decimal AdditionalQuantity) GetOrCreateReceiveLine(
			WhsReceive receive,
			WhsPickableDocketLine parentLine,
			Dictionary<ZGuid, WhsReceiveLine> existingReceiveLineMap,
			Dictionary<ZGuid, WhsBOMInventoryPivot> componentInventoryMap,
			ZGuid firstComponentPK,
			decimal kitQuantity)
		{
			WhsReceiveLine receiveLine;
			var isNewLine = false;
			if (componentInventoryMap.TryGetValue(firstComponentPK, out var existingLink))
			{
				receiveLine = existingReceiveLineMap[existingLink.WIP_WE_InventoryLine];
			}
			else
			{
				receiveLine = WhsPickByBOMHelper.NewKitReceiveLine(Factory, receive.PK, parentLine.WE_OP, 0m);
				isNewLine = true;
			}

			var quantityAllocated = receiveLine.WE_TransactionQuantity;
			if (quantityAllocated != kitQuantity)
			{
				receiveLine.WE_ClientOrderedUnits = kitQuantity;
				receiveLine.WE_StockOnHand = kitQuantity;
			}

			return (receiveLine, isNewLine, kitQuantity - quantityAllocated);
		}

		WhsPickLine CreatePickLine(decimal units, WhsReceiveLine inventoryLine, WhsPickableDocketLine transactionLine)
		{
			var pickLine = Factory.New<WhsPickLine>();
			pickLine.WZ_Units = units;
			pickLine.WZ_WE_InventoryLine = inventoryLine.PK;
			pickLine.WZ_WE_TransactionLine = transactionLine.PK;
			return pickLine;
		}

		void RecreateBOMInventoryPivots(
			IReadOnlyCollection<WhsPickableDocketLine> componentLines,
			Dictionary<ZGuid, WhsBOMInventoryPivot> componentInventoryMap,
			WhsReceiveLine receiveLine)
		{
			foreach (var componentLine in componentLines)
			{
				if (componentInventoryMap.TryGetValue(componentLine.PK, out var link))
				{
					link.Delete();
				}

				link = Factory.New<WhsBOMInventoryPivot>();
				link.WIP_WE_ComponentLine = componentLine.PK;
				link.WIP_WE_InventoryLine = receiveLine.PK;
				link.WIP_ComponentQuantity = componentLine.WE_TransactionQuantity;
			}
		}

		#endregion

		#region GetBOMProductsOnPick

		IEnumerable<OrgSupplierPart> GetBOMProductsOnPick()
		{
			return IEnumerableExtensions.DistinctBy(OrderedInventories.Cast<WhsPickOrderedInventory>().Where(o => !o.SupplierPartPK.IsEmpty), o => o.SupplierPartPK)
					.Select(o => o.SupplierPart)
					.Where(o => o.OP_IsComponentPickedOnSalesOrder && o.BillOfMaterials.Count > 0);
		}

		#endregion

		#region IsAutoAllocatingItemsSemaphore

		class AutoAllocatingItemsSessionManager : IDisposable
		{
			public AutoAllocatingItemsSessionManager(WhsPick pick)
			{
				Pick = Argument.NotNull(pick, nameof(pick));
				SemaphoreSuspender = new SemaphoreManager(pick.IsAutoAllocatingItemsSemaphore);
			}

			readonly WhsPick Pick;
			readonly IDisposable SemaphoreSuspender;

			void IDisposable.Dispose()
			{
				SemaphoreSuspender.Dispose();
				if (!Pick.IsAutoAllocatingItemsSemaphore.IsSuspended)
				{
					Pick.CalculateAutoAllocatedLineTotals();
				}
			}
		}

		// Tested in PickingSlipBuilderTest
		internal Semaphore IsAutoAllocatingItemsSemaphore => isAutoAllocatingItemsSemaphore ?? (isAutoAllocatingItemsSemaphore = new Semaphore());
		Semaphore isAutoAllocatingItemsSemaphore;

		void CalculateAutoAllocatedLineTotals()
		{
			try
			{
				AutoAllocatedLinesCache.Values.ForEach(l => WhsReleaseLine.UpdateOrderTotalsAndCalculateExtendedLinePrice(l.Line, l.Qty));
			}
			finally
			{
				AutoAllocatedLinesCache.Clear();
			}
		}

		internal void AddAutoAllocatedPickableDocketLine(WhsPickableDocketLine pickableDocketLine, ZDecimal difference)
		{
			if (!AutoAllocatedLinesCache.TryGetValue(pickableDocketLine.PK, out var pickableDocketLineAndQty))
			{
				AutoAllocatedLinesCache.Add(pickableDocketLine.PK, (pickableDocketLine, difference));
			}
			else
			{
				AutoAllocatedLinesCache[pickableDocketLine.PK] = (pickableDocketLineAndQty.Line, pickableDocketLineAndQty.Qty + difference);
			}
		}

		Dictionary<ZGuid, (WhsPickableDocketLine Line, ZDecimal Qty)> AutoAllocatedLinesCache { get; } = new Dictionary<ZGuid, (WhsPickableDocketLine, ZDecimal)>();

		#endregion

		#region UpdatingReleaseTotalsAndPickTotalsDelayer

		class UpdatingTotalsSemaphoreManager : IDisposable
		{
			public UpdatingTotalsSemaphoreManager(WhsPick pick)
			{
				Pick = Argument.NotNull(pick, "pick");
				OrderCalculationDelayers = new Stack<IDisposable>();

				foreach (WhsPickableDocket order in pick.Orders)
				{
					var delayer = order.DelayUpdatingReleaseTotals();
					if (delayer != null)
					{
						OrderCalculationDelayers.Push(delayer);
					}
				}

				UpdateTotalsSemaphoreSuspender = new SemaphoreManager(pick.UpdatingTotalsSemaphore);
			}

			readonly Stack<IDisposable> OrderCalculationDelayers;
			readonly WhsPick Pick;
			readonly IDisposable UpdateTotalsSemaphoreSuspender;

			void IDisposable.Dispose()
			{
				foreach (var delayer in OrderCalculationDelayers)
				{
					delayer.Dispose();
				}
				UpdateTotalsSemaphoreSuspender.Dispose();
				if (Pick.totalWeight != null)
				{
					Pick.CalculateOrderTotals();
				}
			}
		}

		// Tested in WhsPickTest.TestTotals_UpdateTotals_ChangePickLineQuantity and TestUpdatePickTotals_Suspend
		public bool IsUpdatingTotalsSuspended => updatingTotalsSemaphore != null && updatingTotalsSemaphore.IsSuspended;

		Semaphore UpdatingTotalsSemaphore => updatingTotalsSemaphore ?? (updatingTotalsSemaphore = new Semaphore());
		Semaphore updatingTotalsSemaphore;

		#endregion

		#endregion

		#region FindInventoryByOrderedInventory

		internal IEnumerable<WhsInventoryView> FindInventoryByOrderedInventoryFromAllInventories(WhsPickOrderedInventory orderedInventory)
		{
			return FindInventoryByOrderedInventoryCore(orderedInventory, () =>
			{
				if (InventoryCache == null)
				{
					InventoryCache = CacheInventoryByAttribs(AllInventories);
				}

				return InventoryCache;
			});
		}

		internal IEnumerable<WhsInventoryView> FindInventoryByOrderedInventory(WhsPickOrderedInventory orderedInventory, Func<WhsPick, IEnumerable<WhsInventoryView>> loadInventories)
		{
			return FindInventoryByOrderedInventoryCore(orderedInventory, () => CacheInventoryByAttribs(loadInventories(this)));
		}

		IEnumerable<WhsInventoryView> FindInventoryByOrderedInventoryCore(WhsPickOrderedInventory orderedInventory, Func<Dictionary<(ZGuid ProductPK, ZGuid ClientPK), InventoryCacheByAttribs>> getCacheToUse)
		{
			var result = Enumerable.Empty<WhsInventoryView>();

			var firstOwner = orderedInventory.Owners.Count > 0 ? orderedInventory.Owners[0] : null;
			if (firstOwner != null)
			{
				var orderedInventorySupplierPartPK = firstOwner.WE_OP;
				if (orderedInventorySupplierPartPK.IsValid)
				{
					var orderedInventoryClientPK = orderedInventory.GetClientPKFast(firstOwner);
					if (orderedInventoryClientPK.IsValid)
					{
						var cache = getCacheToUse();
						if (cache.TryGetValue((orderedInventorySupplierPartPK, orderedInventoryClientPK), out var inventoryCacheByAttribs))
						{
							result = inventoryCacheByAttribs.FindMatchingInventoryByAttribs(firstOwner);
						}
					}
				}
			}

			return result;
		}

		Dictionary<(ZGuid ProductPK, ZGuid ClientPK), InventoryCacheByAttribs> CacheInventoryByAttribs(IEnumerable<WhsInventoryView> inventories)
		{
			var inventoryCache = new Dictionary<(ZGuid ProductPK, ZGuid ClientPK), InventoryCacheByAttribs>();

			WhsSerialNumberHelper.AddWhsSerialNumberAndPivotFetchHint(Factory, inventories.Select(i => i.PK));

			foreach (var inventory in inventories)
			{
				var key = (inventory.WI_OP, inventory.WI_OH_Client);
				if (!inventoryCache.TryGetValue(key, out var attribCache))
				{
					inventoryCache[key] = attribCache = new InventoryCacheByAttribs();
				}

				attribCache.CacheByAttributes(inventory);

				// at this point we are about to load available inventories, so we will add a fetch hint for Location
				Factory.AddFetchHint(WhsLocationViewSchema.PK, inventory.WI_WL);
			}

			return inventoryCache;
		}

		void AppendToInventoryCacheForPickByBOM(IEnumerable<WhsInventoryView> inventories)
		{
			if (InventoryCache == null)
			{
				if (allInventories != null)
				{
					InventoryCache = CacheInventoryByAttribs(AllInventories);
				}
				else
				{
					throw new InvalidOperationException("Cannot append to cache if it hasn't yet been created.");
				}
			}

			foreach (var inventory in inventories)
			{
				var key = (inventory.WI_OP, inventory.WI_OH_Client);
				if (!InventoryCache.TryGetValue(key, out var attribCache))
				{
					InventoryCache[key] = attribCache = new InventoryCacheByAttribs();
				}

				// can't order pick by BOM components by attribute, just directly add Inventory.
				attribCache.AddInventory(inventory);
			}
		}

		Dictionary<(ZGuid ProductPK, ZGuid ClientPK), InventoryCacheByAttribs> InventoryCache;

		#endregion

		#region SortItemsForManualPicking

		public void SortItemsForManualPicking()
		{
			OrderedInventories.Sort(new SortItemsForPicking());

			foreach (WhsPickOrderedInventory orderedInventory in OrderedInventories)
			{
				if (!orderedInventory.AvailableInventories.IsProductUsingSerialNumberAttribute)
				{
					orderedInventory.AvailableInventories.Sort(new SortPickInventoryForPicking());
				}
			}
		}

		#endregion

		#region SuspendBuildingAllReleaseLinesForPick

		internal bool IsBuildingAllReleaseLinesForPickSuspended => BuildingAllReleaseLinesForPickSemaphore.IsSuspended;

		public IDisposable SuspendBuildingAllReleaseLinesForPick() => new SemaphoreManager(BuildingAllReleaseLinesForPickSemaphore);

		Semaphore BuildingAllReleaseLinesForPickSemaphore => buildingAllReleaseLinesForPickSemaphore ?? (buildingAllReleaseLinesForPickSemaphore = new Semaphore());
		Semaphore buildingAllReleaseLinesForPickSemaphore;

		#endregion

		#region Cancel Pick

		public ZString GetIsCancellableErrorMessage()
		{
			var errorMessage = string.Empty;

			if (!IsFinalised && !IsCancelled)
			{
				if (IsReadyForPlanningOrPlanned)
				{
					errorMessage = Res.GetString("433991cd-c71c-42f9-96ee-fbb3aa025969", "You cannot cancel pick because this pick is Ready For Planning or Planned.");
				}
				else if (AnyPackagesOnOrdersHasActiveTrolleyJob)
				{
					errorMessage = Res.GetString("e2b661bb-2334-46af-8fe5-6c3790956b38", "You cannot cancel pick because this pick has a package with an active Trolley job or was picked by Trolley.");
				}
				else if (AnyPackagesOnOrdersHasActivePickByLabelJob)
				{
					errorMessage = Res.GetString("8e155a0a-4b16-4ce0-8a71-70bba4d386bd", "You cannot cancel pick because this pick has a package with an active Pick By Label job.");
				}
				else if (IsPartiallyOrFullyPickedFromPutawayLocation)
				{
					errorMessage = OrderErrorTypes.CannotPerformThisOperationBecauseOrderIsPartiallyOrFullyPicked.Message;
				}
				else if (Transfers.SelectMany(l => l.Lines).Any(l => !l.IsFinalised))
				{
					errorMessage = Res.GetString("24845c2e-9660-48b0-8e40-a6078766f4f8", "You cannot cancel the pick because there are un-finalized Outbound Dock Door Transfer lines.");
				}
			}

			return errorMessage;
		}

		public bool CancelPick()
		{
			var errorMessage = GetIsCancellableErrorMessage();
			var isCancellable = errorMessage.IsEmpty;

			return isCancellable && CancelCancellablePick();
		}

		public bool CancelCancellablePick()
		{
			var result = false;

			var finaliseTime = Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
			var finalisedTimeProvider = new FinalisedDateFromFinaliseEvent(finaliseTime);
			var transfersSuccessfullyFinalised = FinaliseInTransitTransfers(finalisedTimeProvider, GlbStaff.CurrentUser.GS_Code, shouldFinaliseLines: false);
			if (transfersSuccessfullyFinalised)
			{
				using (OrderedInventories.SuspendListChanged())
				{
					using (SuspendUpdatingAllocationLog())
					// Cancelling the Pick will just set all sent quantities to 0, no point doing any release totals calculations.
					using (new DisposableList(Orders.Cast<WhsPickableDocket>().Select(o => o.SuspendUpdatingReleaseTotals_DoNotUse())))
					{
						ClearAllocatedItems();
					}

					WP_TaskPlanningStatus = string.Empty;
					ClearAllAvailableInventories();
					CancelPackageLabelAllocations();
					SetCancelledPickNoToAllOrders();
					ResetOrders();
				}

				if (Orders.Count > 0)
				{
					CannotSaveAfterFailedCancelOperation = true;
				}
				else
				{
					WP_IsAwaitingReplenishment = false;
					WP_PickStatus = PickStatus.Codes.Cancelled;
					this.AddEvents(Events.ServiceCancelled);
					result = true;
				}
			}
			else
			{
				new ValidationForInTransitTransferFinalisation(this, Res.GetString("e6dac4b6-e698-439a-a87c-d47b753b39f4", "Cancel")).ValidateStatusDesc();
			}

			return result;
		}

		void ClearAllAvailableInventories()
		{
			foreach (WhsPickOrderedInventory orderedInventory in OrderedInventories)
			{
				using (orderedInventory.AvailableInventories.SuspendListChanged())
				{
					orderedInventory.AvailableInventories.RemoveAll();
				}
			}
		}

		void SetCancelledPickNoToAllOrders()
		{
			foreach (WhsPickableDocket order in Orders)
			{
				order.CancelledPickNo = WP_PickNo;
			}
		}

		void ResetOrders()
		{
			for (int i = Orders.Count - 1; i >= 0; i--)
			{
				var docket = Orders[i];
				ClearShortfallQuantityCached(docket);
				Orders.Remove(docket);
			}
		}

		void ClearShortfallQuantityCached(WhsPickableDocket pickableDocket)
		{
			foreach (WhsPickableDocketLine pickableDocketLine in pickableDocket.Lines)
			{
				pickableDocketLine.ClearWE_ShortfallQuantityCached();
			}
		}

		bool CannotSaveAfterFailedCancelOperation;

		#endregion

		#region IsLineInventoryAwaitingReplenishment

		public bool IsLineInventoryAwaitingReplenishment(WhsOrderLine line)
		{
			var orderedInventoryLine = OrderedInventories.GetOrderedInventoryForLine(line);
			Argument.NotNull(orderedInventoryLine, nameof(orderedInventoryLine));

			return orderedInventoryLine.IsWaitingReplenishment();
		}

		#endregion

		#endregion

		#region OverrideFulfillmentRuleForOrders

		public void OverrideFulfillmentRuleForOrders(WhsOrder[] whsOrders, INotifications notify)
		{
			if (whsOrders.Length == 0)
			{
				notify.AddMessageError(Res.GetString("14068ba2-4653-4856-b8c8-a937f56e8a21", "Pick must have at least one order."));
			}
			else
			{
				EnsureAllOrdersAreOnThisPick(whsOrders);

				var reference = whsOrders[0].GetOverrideFulfillmentRuleReference(notify);
				if (!reference.IsEmpty)
				{
					foreach (var order in whsOrders)
					{
						order.OverrideFulfillmentRuleAndCreateLog(reference, notify);
					}

					UpdatePickStatusIfRequired();
				}
			}
		}

		void EnsureAllOrdersAreOnThisPick(WhsOrder[] whsOrders)
		{
			foreach (WhsOrder order in whsOrders)
			{
				if (!Orders.Contains(order.PK))
				{
					throw new InvalidOperationException("Cannot override fulfillment rule for an order that is not on this pick.");
				}
			}
		}

		#endregion

		#region FinalisePick

		public event EventHandler<FinalisePickEventArgs> FinalisePickFailure;

		internal IEnumerable<IWhsPickStrategy> PickStrategies => pickStrategies ?? (pickStrategies = new IWhsPickStrategy[] { ObjectFactory.Get<IWhsPickByLabelPickStrategy>(), ObjectFactory.Get<IWhsPickByTrolleyPickStrategy>() });
		IEnumerable<IWhsPickStrategy> pickStrategies;

		public bool IsConsigneeOnOrderUsedOnOtherWarehouseJobs(WhsOrder order) => (ConsigneeAddressOnOrdersCache ?? new ConsigneeECommerceScoreCache(this)).IsConsigneeOnOrderUsedOnOtherWarehouseJobs(order);

		bool ConfirmUserWishesToFinalizePickWithUnpickedPickLines()
		{
			var dialogDefaultContext = new DialogDefaultContext(dialogIdentifier: new ZGuid("39d82e16-b144-4f0a-acc6-bcbc0e7bdfbc"),
												caption: Res.GetString("dc49d54d-e84c-472f-8ee8-a21238c8208e", "Continue?"),
												buttons: ZMessageBoxButtons.YesNo,
												icon: ZMessageBoxIcon.Warning,
												context: null,
												resultsNotToSave: new[] { ZDialogResult.No },
												showCheckboxOnly: true);

			var message = Res.GetString("ed686cc1-cbde-494f-a465-dde9d77b3421", "Not all allocated lines have been picked, please confirm that you want to finalize the pick which will automatically mark any un-picked lines as picked.");
			var e = new DefaultableQueryUserEventArgs(dialogDefaultContext, message, true);
			NotificationSubscriber.QueryUser(e);
			return e.Response;
		}

		public void FinalisePick()
		{
			var finalisePickStatus = GetFinalisableStatus();
			if (finalisePickStatus.IsFinalisable)
			{
				if (IsWorkOrderPick || !HasUnpickedPickLines || ConfirmUserWishesToFinalizePickWithUnpickedPickLines())
				{
					using (new SemaphoreManager(FinalisePickSemaphore))
					{
						FinalisePickCore();

						if (HasErrors)
						{
							finalisePickStatus = new FinalisePickEventArgs(this, "");
						}
					}
				}
			}

			if (!finalisePickStatus.IsFinalisable && FinalisePickFailure != null)
			{
				FinalisePickFailure(this, finalisePickStatus);
			}
		}

		void FinalisePickCore()
		{
			using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
			{
				AddFetchHintsForFinalisePick();

				ClearPickLineCache();

				RunPreFinaliseValidation();

				if (!HasErrors)
				{
					var finaliseTime = Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);
					var finalisedTimeProvider = new FinalisedDateFromFinaliseEvent(finaliseTime);
					var currentUser = GlbStaff.CurrentUser.GS_Code;
					var transfersSuccessfullyFinalised = FinaliseInTransitTransfers(finalisedTimeProvider, currentUser, shouldFinaliseLines: true);
					if (transfersSuccessfullyFinalised)
					{
						if (HandleOverPickedComponentsAndFinalisePickByBOMReceives(finalisedTimeProvider, currentUser))
						{
							SetPickedTimeOnUnpickedPickLines(finaliseTime, currentUser);
							SetDepartedStatusOnAttachedOrders();
							SplitOrderLinesByAllocatedInventoriesForCustomsOrders();
							SetGateOutTimeForAllLoadsInOrders(finaliseTime);

							Logs.AddNew(Events.ItemDocumentJobFinalised, finaliseTime);
							WP_IsAwaitingReplenishment = false;
							WP_PickStatus = PickStatus.Codes.Finalised;
							WP_TaskPlanningStatus = string.Empty;
							WP_FinalizedDateUtc = ZDateTime.UtcNow;
							CalculateOrderClassificationOnOrders();
							OnFinaliseSucceeded();
						}
						else
						{
							new ValidationForReceiveFinalisation(this, Res.GetString("98463353-e522-4c4e-897d-6256d8bdd026", "Finalize")).ValidateStatusDesc();
							OnFinaliseFailed();
						}
					}
					else
					{
						new ValidationForInTransitTransferFinalisation(this, Res.GetString("98463353-e522-4c4e-897d-6256d8bdd026", "Finalize")).ValidateStatusDesc();
						OnFinaliseFailed();
					}
				}
				else
				{
					OnFinaliseFailed();
				}
			}
		}

		void AddFetchHintsForFinalisePick()
		{
			if (!IsWorkOrderPick)
			{
				AddFetchHintsForFinaliseOrders(Orders.ToArray<WhsOrder>());
			}

			foreach (var transfer in Transfers)
			{
				foreach (var transferLine in transfer.Lines)
				{
					Factory.AddFetchHint(JobDocAddressSchema.Instance, FetchHintsHelper.GetJobDocAddressQuery(WhsDocketLineSchema.Constants.Prefix, transferLine.PK));

					var isOriginalInventorySubQuery = new ZQuery();
					isOriginalInventorySubQuery.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, true);
					isOriginalInventorySubQuery.AddToFilter(new ZQuery()); // required to wrap subquery in brackets (bla = bla)

					var matchingLinesQuery = new ZQuery();
					matchingLinesQuery.AddToFilter(WhsDocketLineSchema.WE_WE_MatchingLine, transferLine.PK);
					matchingLinesQuery.AddToFilter(isOriginalInventorySubQuery);
					Factory.AddFetchHint(WhsDocketLineSchema.Instance, matchingLinesQuery);

					var childTransferLineQuery = new ZQuery();
					childTransferLineQuery.AddToFilter(WhsDocketLineSchema.WE_WE_ParentDocketLine, transferLine.PK);
					childTransferLineQuery.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, true);
					Factory.AddFetchHint(WhsDocketLineSchema.Instance, childTransferLineQuery);

					var reservedPickLinesSubQuery = new ZQuery();
					reservedPickLinesSubQuery.AddToFilter(WhsPickLineSchema.WZ_OriginalReservedQty, SQLComparisonOperator.GreaterThan, 0m);
					reservedPickLinesSubQuery.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);

					var reservedPickLinesQuery = new ZQuery();
					reservedPickLinesQuery.AddToFilter(WhsPickLineSchema.WZ_WE_InventoryLine, transferLine.PK);
					reservedPickLinesQuery.AddToFilter(reservedPickLinesSubQuery);
					Factory.AddFetchHint(WhsPickLineSchema.Instance, reservedPickLinesQuery);
				}
			}
		}

		void ClearPickLineCache()
		{
			foreach (WhsPickOrderedInventory orderedInventory in OrderedInventories)
			{
				orderedInventory.ClearPickLinesCache();
			}
		}

		void CalculateOrderClassificationOnOrders()
		{
			if (!IsWorkOrderPick)
			{
				ConsigneeAddressOnOrdersCache = new ConsigneeECommerceScoreCache(this);

				try
				{
					foreach (WhsOrder order in Orders)
					{
						order.CalculateAndSetOrderClassification();
					}
				}
				finally
				{
					ConsigneeAddressOnOrdersCache = null;
				}
			}
		}

		ConsigneeECommerceScoreCache ConsigneeAddressOnOrdersCache;

		void SetPickedTimeOnUnpickedPickLines(ZDateTimeOffset finaliseTime, ZString currentUser)
		{
			using (SuspendPickPercentageRecalculation())
			{
				foreach (var pickLine in GetAllPickLines().ToArray())
				{
					SetPickedTimeOrDeletePickLine(pickLine, currentUser, finaliseTime);
				}
			}
		}

		void SetDepartedStatusOnAttachedOrders()
		{
			var ordersToSetDeparted = Orders.Where(
				o => o.WD_DocketType == DocketType.Codes.Order
				&& !o.WD_DocketStatus.EqualsIgnoringCase(WhsOrderStatus.Codes.Departed)).ToArray();
			ordersToSetDeparted.ForEach(p => p.WD_DocketStatus = WhsOrderStatus.Codes.Departed);
		}

		void SetPickedTimeOrDeletePickLine(WhsPickLine pickLine, ZString currentUser, ZDateTimeOffset finaliseTime)
		{
			if (pickLine.WZ_Units == 0)
			{
				pickLine.Delete();
			}
			else
			{
				if (pickLine.WZ_GS_NKAssignedTo.IsEmpty)
				{
					pickLine.WZ_GS_NKAssignedTo = currentUser;
				}

				if (!pickLine.IsPicked)
				{
					pickLine.WZ_PickedDateTime = finaliseTime;
				}
			}
		}

		void SetLocationOnPickByBOMKitReceiveLinesIfNecessary(WhsPickLine pickLine, ZString currentUser, ZDateTimeOffset finaliseTime, Dictionary<ZGuid, ZGuid> componentInventoryLocationCache)
		{
			var originalInventory = pickLine.InventoryLineForAvailableInventory;
			if (originalInventory.WE_WL.IsEmpty && originalInventory.IsCreatedFromPickByBOM)
			{
				var componentInventoryLocationPK = componentInventoryLocationCache[pickLine.WZ_WE_TransactionLine];
				WhsPickByBOMHelper.UpdateKitReceiveLineToPUT(originalInventory, componentInventoryLocationPK, currentUser, finaliseTime);
			}
		}

		bool HandleOverPickedComponentsAndFinalisePickByBOMReceives(IFinalisedDateProvider finalisedTimeProvider, ZString currentUser)
		{
			if (!IsWorkOrderPick)
			{
				var receives = Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, PK));
				if (receives.Length > 0)
				{
					var componentInventoryLocationCache = new Dictionary<ZGuid, ZGuid>();
					var firstComponentLineByParent = OrderedInventories.Where(oi => oi.IsComponentOrderedInventoryOnSalesOrder).SelectMany(oi => oi.Owners)
						.GroupBy(l => l.WE_WE_ParentDocketLine, (key, components) => components.First());
					foreach (var firstComponentLine in firstComponentLineByParent)
					{
						var componentPickLines = firstComponentLine.PickLines;
						if (componentPickLines.Count > 0)
						{
							componentInventoryLocationCache[firstComponentLine.WE_WE_ParentDocketLine] = componentPickLines[0].InventoryLineForAvailableInventory.WE_WL;
						}
					}

					if (componentInventoryLocationCache.Count > 0)
					{
						foreach (var pickLine in GetAllPickLines().Where(pl => pl.WZ_Units > 0m))
						{
							SetLocationOnPickByBOMKitReceiveLinesIfNecessary(pickLine, currentUser, finalisedTimeProvider.GetFinalisationTimeOffset(), componentInventoryLocationCache);
						}
					}

					HandleOverPickedComponents(receives.ToDictionary(d => d.WD_OH_Client));

					foreach (var receive in receives)
					{
						if (receive.Lines.Count > 0)
						{
							using (receive.SetFinalisedDateProvider(finalisedTimeProvider))
							{
								receive.FinaliseDocketWithoutUserConfirmation();
								if (!receive.IsFinalised)
								{
									return false;
								}
							}
						}
						else
						{
							receive.Delete();
						}
					}
				}
			}

			return true;
		}

		void HandleOverPickedComponents(Dictionary<ZGuid, WhsReceive> receiveLookup)
		{
			var partsCached = new HashSet<ZGuid>();
			var bomParts = new Dictionary<BOMPartCacheKey, OrgPartBOM>();

			var kitOrderLines = OrderedInventories.Where(oi => oi.IsBOMProductPickedOnSalesOrder).SelectMany(oi => oi.Owners).Cast<WhsOrderLine>().ToArray();
			if (kitOrderLines.Length > 0)
			{
				kitOrderLines.ForEach(l => Factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, l.PK)));

				foreach (var orderLine in kitOrderLines)
				{
					var componentOrderLines = orderLine.ChildComponentLines;
					if (componentOrderLines.Count > 0)
					{
						CacheBOMParts(orderLine);
						var assembledKitQuantity = orderLine.PickLines.Where(pl => pl.IsPickByBOMKitPickLine()).Sum(pl => pl.WZ_Units);
						HandleOverPickedComponentsCore(assembledKitQuantity, componentOrderLines, receiveLookup[orderLine.Docket.WD_OH_Client], bomParts, orderLine.WE_OP);
					}
				}
			}

			void CacheBOMParts(WhsOrderLine kitOrderLine)
			{
				if (partsCached.Add(kitOrderLine.WE_OP))
				{
					foreach (var bomPartDef in kitOrderLine.SupplierPart.BillOfMaterials)
					{
						var key = new BOMPartCacheKey(bomPartDef.OE_OP_MainProduct, bomPartDef.OE_OP_Component, bomPartDef.OE_F3_NKPackType);
						if (!bomParts.ContainsKey(key))
						{
							bomParts[key] = bomPartDef;
						}
					}
				}
			}
		}

		void HandleOverPickedComponentsCore(ZDecimal assembledKitQuantity,
			IReadOnlyCollection<WhsPickableDocketLine> componentOrderLines,
			WhsReceive receive,
			Dictionary<BOMPartCacheKey, OrgPartBOM> bomParts,
			ZGuid mainProductPK)
		{
			foreach (var componentOrderLine in componentOrderLines)
			{
				var bomPart = bomParts[new BOMPartCacheKey(mainProductPK, componentOrderLine.WE_OP, componentOrderLine.WE_F3_NKPackType)];
				var quantityUsed = assembledKitQuantity * bomPart.OE_ComponentQty;
				var overPickedQty = componentOrderLine.PickLineQuantity - quantityUsed;
				if (overPickedQty > 0)
				{
					foreach (var componentPickLine in componentOrderLine.PickLines.Where(pl => pl.WZ_Units > 0m).OrderByDescending(pl => pl.WZ_Units))
					{
						var qtyReturnedOnInventoryLine = Math.Min(overPickedQty, componentPickLine.WZ_Units);
						var inventoryLine = componentPickLine.InventoryLineForAvailableInventory;

						var newReceiveLine = receive.Lines.AddNew();
						newReceiveLine.WE_OP = componentOrderLine.WE_OP;
						newReceiveLine.WE_WL = inventoryLine.WE_WL;
						newReceiveLine.WE_F3_NKPackType = inventoryLine.WE_F3_NKPackType;
						newReceiveLine.WE_ClientOrderedUnits = qtyReturnedOnInventoryLine;
						newReceiveLine.WE_StockOnHand = qtyReturnedOnInventoryLine;
						newReceiveLine.WE_AdjustmentArrivalDate = inventoryLine.WE_AdjustmentArrivalDate;
						newReceiveLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
						newReceiveLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
						newReceiveLine.WE_WE_OriginalDocketLineForRating = inventoryLine.WE_WE_OriginalDocketLineForRating;

						newReceiveLine.SetAttributes(inventoryLine);

						overPickedQty -= componentPickLine.WZ_Units;
						if (overPickedQty <= 0)
						{
							break;
						}
					}
				}
			}
		}

		#region FinaliseInTransitTransfers

		bool FinaliseInTransitTransfers(IFinalisedDateProvider finalisedTimeProvider, ZString currentUser, bool shouldFinaliseLines)
		{
			var dockDoorLocationPk = DockDoorPK;

			// Use Lazy<> since the pick can exist without a warehouse, e.g. 'Generate Pick' Operational Action will create a Pick and cancel it if there are errors.
			// If a Transfer is somehow attached to a Pick without a Warehouse, accessing the Lazy Value with throw an exception, which we want to happen.
			var stagingLocationHelper = new Lazy<WorkOrderStagingLocationHelper>(() => new WorkOrderStagingLocationHelper(Warehouse));

			foreach (var transfer in Transfers)
			{
				var unFinalisedLines = transfer.Lines.Where(tl => !tl.IsFinalised).ToArray();
				if (!shouldFinaliseLines && unFinalisedLines.Length > 0)
				{
					throw new InvalidOperationException("Attempted to finalise an In-Transit Transfer with unfinalized lines.");
				}

				// destination location should already be set, but just in case something went wrong earlier we set it now (this has actually happened in defects)
				SetDestinationLocationOnUnfinalisedLines(unFinalisedLines, stagingLocationHelper.Value, dockDoorLocationPk);

				using (transfer.SetFinalisedDateProvider(finalisedTimeProvider))
				{
					transfer.FinaliseDocketWithoutUserConfirmation();
				}

				if (!transfer.IsFinalised)
				{
					return false;
				}
				else
				{
					foreach (var transferLine in unFinalisedLines)
					{
						transferLine.WE_GS_NKPutawayBy = currentUser;
					}
				}
			}

			return true;
		}

		void SetGateOutTimeForAllLoadsInOrders(ZDateTimeOffset finaliseTime)
		{
			if (!IsWorkOrderPick)
			{
				var orders = Orders;
				var orderPks = orders.Select(o => o.PK).ToArray();
				var loadOrderCollection = GetWhsLoadOrders(orderPks);

				var orderPKsAssignedToLoad = loadOrderCollection.Select(loadOrder => loadOrder.WOV_WD_Docket).ToHashSet();
				orders.Where(o => !orderPKsAssignedToLoad.Contains(o.PK)).ForEach(o => o.Logs.AddNew(Events.FreightLoaded, finaliseTime));

				if (loadOrderCollection.Length > 0)
				{
					var picksOnLoadInfos = Factory.Load<WhsLoadOrder>(new ZQuery(WhsLoadOrderSchema.WOV_WLO_Load, loadOrderCollection.Select(loadOrder => loadOrder.WOV_WLO_Load).Distinct()))
						.GroupBy(loadOrder => loadOrder.WOV_WLO_Load)
						.ToDictionary(g => g.Key, g => g.ToArray());

					var loads = GetLoads(loadOrderCollection).ToArray();
					foreach (var load in loads)
					{
						if (load.WLO_GateOutTime.IsEmpty
							&& picksOnLoadInfos.TryGetValue(load.PK, out var picks)
							&& picks.All(pick => !pick.WOV_PickNo.IsEmpty && (pick.WOV_PickStatus == PickStatus.Codes.Finalised || pick.WOV_PickNo == WP_PickNo)))
						{
							load.WLO_GateOutTime = finaliseTime;
						}
					}

					FinalisePackageJobsForGateOutLoad(loads.Where(load => load.WLO_GateOutTime.IsValid).Select(l => l.PK).ToArray());
				}
			}
		}

		void FinalisePackageJobsForGateOutLoad(IEnumerable<ZGuid> loadPKs)
		{
			var loadPivotsSubQuery = new ZDBOnlySubQuery(typeof(WhsLoadPkgPackagePivot), WhsLoadPkgPackagePivotSchema.WLP_KP_Package);
			loadPivotsSubQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_WLO_Load, loadPKs);

			var loadPackageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
			loadPackageSubQuery.AddToFilter(PkgPackageSchema.KP_ClosedTimeUtc, SQLComparisonOperator.Equal, null);
			loadPackageSubQuery.AddSubQuery(loadPivotsSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(PkgPackageJob));
			query.AddToFilter(PkgPackageJobSchema.KJ_ParentTableCode, SQLComparisonOperator.Equal, PkgHandlingUnitSchema.Constants.Prefix);
			query.AddSubQuery(loadPackageSubQuery, JoinCondition.And);

			var loadPackageJobs = Factory.Load<PkgPackageJob>(query);
			loadPackageJobs.ForEach(j =>
			{
				using (j.SuspendClosingPackagesOnFinalize())
				{
					j.KJ_IsFinalized = true;
				}
			});
		}

		WhsLoadOrder[] GetWhsLoadOrders(IEnumerable<ZGuid> orderPks)
		{
			return Factory.Load<WhsLoadOrder>(new ZQuery(WhsLoadOrderSchema.WOV_WD_Docket, orderPks));
		}

		IEnumerable<WhsLoad> GetLoads(IEnumerable<WhsLoadOrder> loadOrders)
		{
			loadOrders.ForEach(loadOrder => Factory.AddFetchHint(WhsLoadSchema.PK, loadOrder.WOV_WLO_Load));
			return loadOrders.Select(l => l.Load);
		}

		const string WhsLoadPK = nameof(WhsLoadPK);
		const string WD_PKAsFK = nameof(WD_PKAsFK);

		void SetDestinationLocationOnUnfinalisedLines(WhsDocketLine[] unFinalisedLines, WorkOrderStagingLocationHelper stagingLocationHelper, ZGuid dockDoorLocationPk)
		{
			if (IsWorkOrderPick)
			{
				foreach (var transferLine in unFinalisedLines)
				{
					var workOrderLine = GetWorkOrderLineFromTransferLine(transferLine);
					transferLine.WE_WL = stagingLocationHelper.GetStagingLocationForWorkOrderLine(workOrderLine);
				}
			}
			else
			{
				foreach (var transferLine in unFinalisedLines)
				{
					transferLine.WE_WL = dockDoorLocationPk;
				}
			}
		}

		WhsComponentOrderLine GetWorkOrderLineFromTransferLine(WhsDocketLine transferLine)
		{
			var query = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, transferLine.PK);
			var pickLines = Factory.Load<WhsPickLine>(query);

			// tested in TestFinaliseDocket_CreatesReceive_SplitsAndLinksInventory_TwoLinesPickSameInventoryWhenSplit
			var firstWorkOrderLine = (WhsComponentOrderLine)pickLines[0].DocketLine;
			foreach (var pickLine in pickLines.Skip(1))
			{
				var workOrderLine = pickLine.DocketLine;
				if (workOrderLine.WE_WE_ParentDocketLine.IsEmpty
					|| workOrderLine.WE_WD != firstWorkOrderLine.WE_WD
					|| workOrderLine.WE_OP != firstWorkOrderLine.WE_OP
					|| ((WhsComponentOrderLine)workOrderLine).ParentLine.WE_OP != firstWorkOrderLine.ParentLine.WE_OP)
				{
					throw new InvalidOperationException("Should not have the same TransferLine for different Component or Kit.");
				}
			}

			return firstWorkOrderLine;
		}

		#endregion

		#region SplitOrderLinesByAllocatedInventoriesForCustomsOrders

		void SplitOrderLinesByAllocatedInventoriesForCustomsOrders()
		{
			if (Warehouse.WW_IsVirtualWarehouse && !IsWorkOrderPick)
			{
				foreach (WhsOrder order in Orders)
				{
					if (order.IsCustomsTransaction)
					{
						order.SplitOrderLinesByAllocatedInventoriesForOrder();
					}
				}
			}
		}

		#endregion

		abstract class ValidationOverrideForPickFinalisation : ZValidation
		{
			protected ValidationOverrideForPickFinalisation(WhsPick pick, string action)
				: base(pick)
			{
				Action = action;
			}

			protected WhsPick Parent => (WhsPick)ParentFilter;
			protected readonly string Action;

			public void ValidateStatusDesc()
			{
				ValidateCalculatedProperty(Parent.StatusDescInfo);
			}

			protected abstract void CheckStatusDesc();

			public override void ValidateAll()
			{
				throw new InvalidOperationException($"Should not be calling ValidateAll() on {GetType().Name}.");
			}
		}

		class ValidationForInTransitTransferFinalisation : ValidationOverrideForPickFinalisation
		{
			public ValidationForInTransitTransferFinalisation(WhsPick pick, string action)
				: base(pick, action)
			{
			}

			protected override void CheckStatusDesc()
			{
				var transferFinalisationErrors = Parent.Transfers.Where(t => !t.IsFinalised && t.HasErrors).SelectMany(t => t.GetErrors()).ToUniqueMessageListString();
				if (!transferFinalisationErrors.IsNullOrEmpty())
				{
					var errorMessage = Res.GetString("4f7b84ac-350a-4034-8021-3fdd223aec14", "Unable to {0} Pick because of the following Errors when attempting to finalize all In-Transit Transfers:\r\n{1}", Action, transferFinalisationErrors);
					Parent.StatusDescInfo.AddError(errorMessage);
				}
			}

			public override Type AutoValidationType => typeof(ValidationForInTransitTransferFinalisation);
		}

		class ValidationForReceiveFinalisation : ValidationOverrideForPickFinalisation
		{
			public ValidationForReceiveFinalisation(WhsPick pick, string action)
				: base(pick, action)
			{
			}

			protected override void CheckStatusDesc()
			{
				var receives = Parent.Factory.Load<WhsReceive>(new ZQuery(WhsDocketSchema.WD_WP_ParentPickForReceive, Parent.PK));
				var receiveFinalisationErrors = receives.Where(r => !r.IsFinalised && r.HasErrors).SelectMany(r => r.GetErrors()).ToUniqueMessageListString();
				if (!receiveFinalisationErrors.IsNullOrEmpty())
				{
					var errorMessage = Res.GetString("1ae0653f-e42c-4b44-a6b4-21656a9e1da1", "Unable to {0} Pick because of the following Errors when attempting to finalize all Assembled Kits:\r\n{1}", Action, receiveFinalisationErrors);
					Parent.StatusDescInfo.AddError(errorMessage);
				}
			}

			public override Type AutoValidationType => typeof(ValidationForReceiveFinalisation);
		}

		void OnFinaliseSucceeded()
		{
			if (!IsWorkOrderPick) // we need a strategy for Order vs WorkOrder, whoever reads this can do it :P
			{
				UpdatePackingReadOnly();
				RunPickStrategies();
				CreateBackOrders();
			}
		}

		void OnFinaliseFailed()
		{
			ClearAllInventoryAndAvailableInventoryCache();
		}

		void UpdatePackingReadOnly()
		{
			foreach (WhsOrder order in Orders)
			{
				var packageJob = order.PackageJob;
				if (packageJob != null)
				{
					packageJob.KJ_IsFinalized = true;
				}
			}

			var canFinalisedJobs = GetCanFinalisedPackageJobs();

			if (canFinalisedJobs.Count > 0)
			{
				var jobPKs = new HashSet<ZGuid>();
				var pickInfosGroupByJob = canFinalisedJobs.GroupBy(r => r["ConHUPK"]);
				foreach (var pickInfo in pickInfosGroupByJob)
				{
					if (pickInfo.All(p => p[WhsPickSchema.Constants.WP_PickStatus].ToString() == PickStatus.Codes.Finalised || (ZGuid)p[WhsPickSchema.Constants.PK] == PK))
					{
						jobPKs.Add(new ZGuid(pickInfo.First()["ConHUPackageJob"]));
					}
				}

				var jobs = Factory.Load<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.PK, jobPKs));
				jobs.ForEach(j => j.KJ_IsFinalized = true);
			}
		}

		DynamicBusinessObjectCollection GetCanFinalisedPackageJobs()
		{
			var result = new DynamicBusinessObjectCollection(Factory);

			var sql = GetCanFinalisedConsolidationPackageJob();
			var sqlParams = new ZSqlParameterCollection();
			var orderPKs = Orders.Select(o => o.PK);
			sqlParams.Add(ZSqlParameter.New("@OrderPKs", orderPKs.ToArray(), WhsDocketSchema.PK, isTableValued: true));

			result.Load(sql, sqlParams);
			return result;
		}

		ZString GetCanFinalisedConsolidationPackageJob()
		{
			var sql = $@"
SELECT DISTINCT
	WP_PK,
	WP_PickStatus,
	ConHUPK,
	ConHUPackageJob
FROM
	WhsDocket CurrentOrder
	JOIN PkgPackageJob ON KJ_ParentID = WD_PK
	CROSS APPLY
	(
		SELECT
			TOP 1 ConsolidationHU.KP_PK as ConHUPK,
			ConsolidationHU.KP_KJ_ParentPackageJob as ConHUPackageJob
		FROM
			PkgPackage OrderPackage
			JOIN PkgPackageHandlingUnitDivot ON KPD_KP_Package = KP_PK
			JOIN PkgPackage ConsolidationHU ON KPD_KP_HandlingUnit = ConsolidationHU.KP_PK
		WHERE
			OrderPackage.KP_KJ_ParentPackageJob = KJ_PK
			AND ConsolidationHU.KP_ClosedTimeUtc IS NOT NULL
	) as ConsolidationHU
	JOIN PkgPackageHandlingUnitDivot ON KPD_KP_HandlingUnit = ConHUPK
	JOIN PkgPackage ON KPD_KP_Package = KP_PK AND KP_KJ_ParentPackageJob <> KJ_PK
	JOIN PkgPackageJob ConsolidatedPackageJob ON KP_KJ_ParentPackageJob = ConsolidatedPackageJob.KJ_PK
	JOIN WhsDocket OtherOrder ON OtherOrder.WD_PK = ConsolidatedPackageJob.KJ_ParentID
	JOIN WhsPick ON OtherOrder.WD_WP = WP_PK
WHERE
	CurrentOrder.WD_PK IN (SELECT value FROM @OrderPKs)
	AND CurrentOrder.WD_DocketType = 'ORD'
";
			return sql;
		}

		void RunPickStrategies()
		{
			foreach (var strategy in PickStrategies)
			{
				strategy.OnFinalised(Factory, this);
			}
		}

		#region CreateBackOrders

#if DEBUG
		public
#endif
		void CreateBackOrders()
		{
			foreach (WhsOrder order in Orders)
			{
				if (order.Client.MiscServ.OM_WhsGenerateBackOrdersOnShortfalls && !order.IsCustomsTransaction && order.GetShortfallExistsStatus())
				{
					CreateBackOrder(order);
				}
			}
		}

		#region Back Orders

#if DEBUG
		public
#endif
		void CreateBackOrder(WhsOrder order)
		{
			WhsOrder backOrder;
			var oldBackOrder = order.RelatedBackOrder;
			if (oldBackOrder != null)
			{
				if (oldBackOrder.WD_DocketStatus == DocketStatus.Codes.Entered)
				{
					oldBackOrder.Delete();
				}
			}
			backOrder = (WhsOrder)((IWhsJobTemplateCopyable)order).TemplateCopyWithoutLines();
			SetBackOrderProperties(backOrder, order);
		}

		void SetBackOrderProperties(WhsOrder backOrder, WhsOrder order)
		{
			if (backOrder == null)
			{
				throw new ArgumentNullException(nameof(backOrder));
			}

			backOrder.WD_WD_ParentDocket = order.PK;
			backOrder.WD_DocketSubType = OrderType.Codes.BackOrder;
			backOrder.WD_ExternalReferenceSplit = GetNextAvailableSplitNo(order);
			backOrder.WD_DocketStatus = DocketStatus.Codes.Entered;
			backOrder.WD_TotalWeight = 0m;
			backOrder.WD_TotalCubic = 0m;

			if (!backOrder.WD_PickPriority.IsEmpty)
			{
				var shortPriority = backOrder.WD_PickPriority - WarehouseDataRegistry.Instance.ShortPickingPriorityIncrement.Value;
				backOrder.WD_PickPriority = (ZByte)Math.Max(1, shortPriority);
			}

			ClearAllReleaseRelatedData(backOrder);
			SetBackOrderLines(backOrder, order);
		}

		ZByte GetNextAvailableSplitNo(WhsOrder order)
		{
			// get order with same WD_ExternalReference
			var query = new ZQuery(WhsDocketSchema.WD_OH_Client, order.WD_OH_Client);
			query.AddToFilter(WhsDocketSchema.WD_ExternalReference, order.WD_ExternalReference);
			query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
			query.OrderBy = WhsDocketSchema.Constants.WD_ExternalReferenceSplit + OrderByClause.Descending;

			var order1 = Factory.LoadTop1<WhsOrder>(query);
			return (order1 != null) ? (ZByte)(order1.WD_ExternalReferenceSplit + 1) : ZByte.Zero;
		}

		void ClearAllReleaseRelatedData(WhsOrder backOrder)
		{
			backOrder.WD_TransportReference = "";
			backOrder.VehicleNo = "";
			backOrder.WD_ShipperCODAmount = 0m;
			backOrder.WD_CODPayMethod = "";
			backOrder.Containers.DeleteAll();
		}

		void SetBackOrderLines(WhsOrder backOrder, WhsOrder order)
		{
			var isRecalculateOrderPricing = backOrder.IsRecalculateOrderPricing;
			foreach (WhsOrderLine line in order.ParentLines.Cast<WhsPickableDocketLine>().Where(l => !l.IsBOMProductPickedOnSalesOrder).ToArray())
			{
				var difference = line.WE_ShortfallQuantityCached;
				if (difference != 0)
				{
					var backOrderLine = line.Clone();
					backOrderLine.WE_TransactionQuantity = difference;
					if (isRecalculateOrderPricing)
					{
						backOrderLine.WE_ExtendedLinePrice = backOrderLine.WE_UnitPriceAfterDiscount * backOrderLine.WE_TransactionQuantity;
					}
					backOrder.Lines.Add(backOrderLine);
				}
			}
		}

		#endregion

		#endregion

		void RunPreFinaliseValidation()
		{
			MarkAsNeedingValidationIncludingChildren();
			RunPreSaveValidation();

			foreach (WhsPickLine pickLine in GetAllPickLines())
			{
				pickLine.Validation.ValidateWZ_Units();
			}

			if (!HasErrors)
			{
				FinalDoubleCheckOfQuantitiesAgainstLowLevelPersistentBusinessObjects();
			}
		}

		void FinalDoubleCheckOfQuantitiesAgainstLowLevelPersistentBusinessObjects()
		{
			ZDecimal quantityMet = 0m;
			ZDecimal pickLineQuantity = 0m;

			foreach (WhsPickableDocket order in Orders)
			{
				foreach (WhsPickableDocketLine orderLine in order.GetLinesToPick())
				{
					if (!orderLine.IsComponentLineOnSalesOrder)
					{
						quantityMet += orderLine.SumOfUnitsMet;

						foreach (var pickLine in orderLine.PickLines)
						{
							pickLineQuantity += pickLine.WZ_Units;
						}
					}
				}
			}

			if (quantityMet != pickLineQuantity)
			{
				AddRowError(Res.GetString("440ba56c-3a67-4615-97d8-f92441b3c00d", "Cannot Finalize this Pick because Quantity Met does not equal Quantity Picked"));
			}
		}

		#region IsFinalisable

		public FinalisePickEventArgs GetFinalisableStatus()
		{
			ZString errorMessage = "";

			if (IsFinalised)
			{
				errorMessage = Res.GetString("8d698025-01b6-4e38-a52d-63bab645440f", "This pick is already finalized.");
			}
			else if (IsCancelled)
			{
				errorMessage = Res.GetString("77a47b66-2ee8-40e4-9389-d3b7f0328683", "This pick is canceled.");
			}
			else if (IsAnyOrderNotFinalised)
			{
				errorMessage = (Orders.Count == 1)
					? Res.GetString("2e4c7631-1c32-4b65-a4b2-a51745707a91", "Please finalize the order before finalizing the pick.")
					: Res.GetString("3047704f-78f0-48c2-a201-1ef69f406f20", "Please finalize all the orders on the pick before finalizing the pick.");
			}
			else if (IsPickByLabelOrCartonizeSplitCasesAndLabelsHaveNotBeenCreated)
			{
				errorMessage = Res.GetString("ba01fce1-2264-4f21-9744-1a3777472005", "Cannot Finalize this pick, because it requires Package Labels and no Package Labels have been allocated.");
			}
			else if (!IsWorkOrderPick && IsAnyLoadNotComplete())
			{
				errorMessage = Res.GetString("2FF0C7E0-0275-4D89-B5DE-A8FD53D37BE7", "Cannot Finalize this pick, because there are some orders that require loading but have not completed loading.");
			}
			else if (!IsWorkOrderPick)
			{
				var ordersNotStaged = GetNonStagedOrdersUsingConsolidation();
				if (!string.IsNullOrEmpty(ordersNotStaged))
				{
					errorMessage = Res.GetString("B12AD22D-FAA3-4491-9017-EBD88D6BF503", "Cannot Finalize this pick as the following Orders are using directed Consolidation and are not yet Staged:\r\n{0}", ordersNotStaged);
				}
				else
				{
					var lazyPackageJob = new Lazy<Dictionary<ZGuid, PkgPackageJob>>(() => GetOrderPKWithPackageJobs());
					var ordersWithTote = GetOrdersWithUnfinishedTotes(lazyPackageJob);
					if (!string.IsNullOrEmpty(ordersWithTote))
					{
						errorMessage = Res.GetString("E9AE1819-F2FE-45E8-965B-6460EC1D6681", "Cannot Finalize this pick as the following Orders have not finished Packing Totes:\r\n{0}", ordersWithTote);
					}
					else
					{
						var ordersWithIncompletePacking = GetOrdersThatRequirePackingButPackingIsIncomplete(lazyPackageJob);
						if (!string.IsNullOrEmpty(ordersWithIncompletePacking))
						{
							errorMessage = Res.GetString("9b5b892b-a03a-40a3-b890-3a1a600f390d", @"Cannot Finalize this pick, because there are some orders that require packing but have not completed packing.

Packing is required as the orders are marked on the order screen as 'Required Packing'. To address this error, all orders must be packed, all packages must be closed and all items must be packed into non-container packages.

If packing is not used, the orders can have 'Required Packing' unchecked to proceed with finalization.

Order(s) that require packing, closing packages or have items packed into containers:
{0}", ordersWithIncompletePacking);
						}
					}
				}
			}

			return new FinalisePickEventArgs(this, errorMessage);
		}

		Dictionary<ZGuid, PkgPackageJob> GetOrderPKWithPackageJobs()
		{
			return Orders.Cast<WhsOrder>()
				.Select(o => new { OrderPK = o.PK, PackageJob = Factory.LoadTop1<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, o.PK)) })
				.Where(o => o.PackageJob != null)
				.ToDictionary(o => o.OrderPK, o => o.PackageJob);
		}

		string GetOrdersThatRequirePackingButPackingIsIncomplete(Lazy<Dictionary<ZGuid, PkgPackageJob>> lazyOrdersWithPackageJobs)
		{
			var ordersRequiringPacking = Orders.OfType<WhsOrder>()
				.Where(order => order.WD_PackingAfterPickingRequired);

			var orderRefsWithIncompletePacking = ordersRequiringPacking
				.Where(order =>
				{
					if (lazyOrdersWithPackageJobs.Value.TryGetValue(order.PK, out var packageJob))
					{
						return packageJob.PackableItemParents
							.Where(item => item.UnpackedQty != 0).Any() ||
								packageJob.Packages.Any(package => !package.IsClosed ||
								(package.IsContainer && package.PackedItems.Count != 0));
					}

					return false;
				})
				.Select(o => o.WD_ExternalReference)
				.OrderBy(r => r);

			return string.Join("\r\n", orderRefsWithIncompletePacking);
		}

		bool IsPickByLabelOrCartonizeSplitCasesAndLabelsHaveNotBeenCreated => (!WP_IsCartonised && (WP_CartoniseSplitCases || WP_PickPalletsByLabel || WP_PickCasesByLabel));

		bool IsAnyOrderNotFinalised => Orders.Cast<WhsPickableDocket>().Any(o => !o.IsFinalised);

		bool IsAnyLoadNotComplete()
		{
			var loads = GetLoads(GetWhsLoadOrders((orders.Select(o => o.PK))));
			return loads.Any(l => l.WLO_CompleteTime.IsEmpty);
		}

		string GetNonStagedOrdersUsingConsolidation()
		{
			Factory.AddFetchHint(WhsOrderStatusViewSchema.Instance, new ZQuery(WhsOrderStatusViewSchema.PK, Orders.Select(o => o.PK)));

			return string.Join("\r\n",
				Orders
					.Cast<WhsOrder>()
					.Where(o =>
						o.WD_UseDirectedPackingConsolidation &&
						o.WarehouseOrderStatus != WhsOrderStatus.Codes.Staged &&
						o.WarehouseOrderStatus != WhsOrderStatus.Codes.Loading &&
						o.WarehouseOrderStatus != WhsOrderStatus.Codes.Loaded &&
						o.WarehouseOrderStatus != WhsOrderStatus.Codes.Departed)
					.Select(o => o.WD_ExternalReference)
					.OrderBy(r => r)
			);
		}

		string GetOrdersWithUnfinishedTotes(Lazy<Dictionary<ZGuid, PkgPackageJob>> lazyOrdersWithPackageJobs)
		{
			AddFetchHintsForPackageJobs();
			Factory.AddFetchHint(OrgHeaderSchema.Instance, new ZQuery(OrgHeaderSchema.PK, orders.Select(o => o.WD_OH_Client).Distinct()));

			var packageJobsDictionary = lazyOrdersWithPackageJobs.Value;

			var ordersWithTotePackages = Orders
				.Cast<WhsOrder>()
				.Where(order => !CanFinaliseUnpackedTotes(order)
					&& packageJobsDictionary.TryGetValue(order.PK, out var packageJob)
					&& packageJob.Packages.Any(p => p.GetIsTote()))
				.Select(order => order.WD_ExternalReference)
				.OrderBy(r => r);

			return string.Join("\r\n", ordersWithTotePackages);
		}

		bool CanFinaliseUnpackedTotes(WhsOrder order)
		{
			var clientPickingParams = order.ClientPickingParams;
			return clientPickingParams is not null && clientPickingParams.WPP_AllowPickFinalizationWithUnpackedTotes;
		}

		#endregion

		#region FinalisePickEventArgs

		public class FinalisePickEventArgs : EventArgs
		{
			public FinalisePickEventArgs(WhsPick pick, ZString errorMessage)
			{
				Pick = pick;
				ErrorMessage = errorMessage;
			}

			public bool IsFinalisable
			{
				get
				{
					var isFinalisable = ErrorMessage.IsEmpty && !Pick.HasErrors;
					if (isFinalisable)
					{
						isFinalisable = FinaliseCheckpoint.IsAllowed || IsVirtualWarehouseAndAllOrdersCustomsType || AllOrdersAreIsImportingForChangeOfInventory;
					}
					return isFinalisable;
				}
			}

			bool IsVirtualWarehouseAndAllOrdersCustomsType
			{
				get { return (Pick.Warehouse?.WW_IsVirtualWarehouse ?? false) && Pick.Orders.Cast<WhsPickableDocket>().All(o => o.WD_DocketSubType == OrderType.Codes.Customs); }
			}

			bool AllOrdersAreIsImportingForChangeOfInventory
			{
				get
				{
					var orders = Pick.Orders.Cast<WhsPickableDocket>();
					return orders.Any() && orders.All(o => o.IsImportingForChangeOfInventory);
				}
			}

			public SecurityCheckpoint FinaliseCheckpoint
			{
				get { return Env.Security.WhsReleaseFinalise; }
			}

			public readonly ZString ErrorMessage;
			public readonly WhsPick Pick;
		}

		#endregion

		#region Finalise Pick Semaphore

#if DEBUG
		public
#endif
		Semaphore FinalisePickSemaphore
		{
			get { return finalisePickSemaphore ?? (finalisePickSemaphore = new Semaphore()); }
		}

		Semaphore finalisePickSemaphore;

		#endregion

		#endregion

		#region FinaliseOrder

		public void FinaliseOrder(WhsOrder order)
		{
			FinaliseOrders(order);
		}

		public void FinaliseAllOrders()
		{
			FinaliseOrders(Orders.ToArray<WhsPickableDocket>());
		}

		public void FinaliseSelectedOrders()
		{
			var pickableDockets = Orders.Cast<WhsPickableDocket>().Where(o => o.WD_IsOrderSelectedForFinalisation).ToArray();
			FinaliseOrders(pickableDockets);
		}

		void FinaliseOrders(params WhsPickableDocket[] pickableDockets)
		{
			var orders = IsWorkOrderPick ? Array.Empty<WhsOrder>() : pickableDockets.Cast<WhsOrder>().ToArray();
			if (!IsWorkOrderPick)
			{
				AddFetchHintsForFinaliseOrders(orders);
			}

			try
			{
				LoadedPackagesCache = new LoadedPackageInfoCache(Factory, orders.Select(o => o.PackageJob).WhereNotNull());

				foreach (var pickableDocket in pickableDockets)
				{
					pickableDocket.FinaliseDocket();
					OnFinalisingSelectedOrdersForTest(pickableDocket);
				}
			}
			finally
			{
				LoadedPackagesCache = null;
			}

			RefreshBinding();
		}

		void AddFetchHintsForFinaliseOrders(WhsOrder[] orders)
		{
			var company = GlbCompany.CurrentCompany;

			foreach (var order in orders)
			{
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, order.PK);

				var jobHeaderQuery = new ZQuery();
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_ParentID, order.PK);
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, company.PK);

				Factory.AddFetchHint(JobHeaderSchema.Instance, jobHeaderQuery);

				Factory.AddFetchHint(JobServiceSchema.ES_ParentID, order.PK);
				Factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.PK);
				Factory.AddFetchHint(WhsDocketContainerSchema.WC_WD, order.PK);
				Factory.AddFetchHint(WhsDocketPalletSchema.W2_WD, order.PK);
				Factory.AddFetchHint(WhsDocketReferenceSchema.WX_WD, order.PK);
				Factory.AddFetchHint(WhsOrderStatusViewSchema.PK, order.PK);
				Factory.AddFetchHint(WhsLoadOrderSchema.WOV_WD_Docket, order.PK);

				Factory.AddFetchHint(PkgPackageJobSchema.KJ_ParentID, order.PK);
				Factory.AddFetchHint(JobCartageSchema.JJ_ParentID, order.PK);
				Factory.AddFetchHint(StmALogSchema.SL_Parent, order.PK);

				var bookingConsolQuery = new ZQuery(DtbBookingConsolidationSchema.KB_ParentID, order.PK);
				bookingConsolQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, SQLComparisonOperator.NotEqual, TransportConsolidationJobTypes.Codes.Consignment);
				Factory.AddFetchHint(DtbBookingConsolidationSchema.Instance, bookingConsolQuery);

				var pivotQuery = new ZQuery(WhsDocketJobPivotSchema.WV_WD_Docket, order.PK);
				pivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_DocketType, DocketType.Codes.Order);
				Factory.AddFetchHint(WhsDocketJobPivotSchema.Instance, pivotQuery);
			}

			var originalInventoryPKs = GetAllPickLines().Where(pl => pl.WZ_WE_OriginalPickedInventoryLine.IsValid).Select(pl => pl.WZ_WE_OriginalPickedInventoryLine);
			Factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.PK, originalInventoryPKs));

			var packageJobCache = new Dictionary<WhsOrder, PkgPackageJob>();

			foreach (var order in orders)
			{
				var packageJob = order.PackageJob;
				if (packageJob != null)
				{
					packageJobCache.Add(order, packageJob);
					Factory.AddFetchHint(PkgPackageJobPackageHeaderPivotSchema.KPJ_KJ_PackageJob, packageJob.PK);
					Factory.AddFetchHint(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob.PK);
				}

				foreach (JobDocAddress jobAddress in order.DocAddresses)
				{
					Factory.AddFetchHint(GenCustomAddOnRuleAckSchema.XK_ParentID, jobAddress.PK);
					Factory.AddFetchHint(OrgAddressSchema.Constants.TableName, jobAddress.E2_OA_Address);
				}

				foreach (var workflowItem in order.WorkflowItems.Cast<ProcessTask>())
				{
					Factory.AddFetchHint(ProcessTaskNotificationSchema.PQ_P9, workflowItem.PK);
					Factory.AddFetchHint(StmALogSchema.SL_Parent, workflowItem.PK);

					if (workflowItem.IsMilestone)
					{
						workflowItem.ShouldAddMilestoneCompletionPivotFetchHintsForLoadChildEditableObjects = true;
					}

					if (workflowItem.IsException)
					{
						workflowItem.ShouldAddProcessWorkflowExceptionFetchHintsForLoadChildEditableObjects = true;
					}
				}
			}

			foreach (var order in orders)
			{
				if (packageJobCache.TryGetValue(order, out var packageJob))
				{
					foreach (var package in packageJob.GetAllPackagesOnJob())
					{
						Factory.AddFetchHint(StmDefaultPrinterSchema.SDP_SubjectID, package.PK);
						Factory.AddFetchHint(WhsPickByLabelLabelSchema.WTL_KP_Package, package.PK);
						Factory.AddFetchHint(WhsPickTrolleySlotSchema.WTS_KP_Package, package.PK);

						var pivotQuery = new ZQuery(WhsLoadPkgPackagePivotSchema.WLP_KP_Package, package.PK);
						pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_UnloadedTime, SQLComparisonOperator.Equal, null);
						Factory.AddFetchHint(WhsLoadPkgPackagePivotSchema.Instance, pivotQuery);

						Factory.AddFetchHint(StmALogSchema.SL_Parent, package.PK);
					}
				}

				foreach (var notification in order.WorkflowItems.SelectMany(p => ((ProcessTask)p).ProcessTaskNotifications))
				{
					Factory.AddFetchHint(ProcessTaskNotificationSchema.PK, notification.PQ_SourceTemplateNotification);
					Factory.AddFetchHint(ProcessTasksSchema.PK, notification.PQ_P9);
				}

				foreach (var address in order.DocAddresses.Cast<JobDocAddress>().Where(d => d.HasRealAddress).Select(d => d.Address))
				{
					Factory.AddFetchHint(OrgAddressSchema.OA_OH, address.OA_OH);
					Factory.AddFetchHint(OrgContactSchema.OC_OH, address.OA_OH);
					Factory.AddFetchHint(OrgCusCodeSchema.OK_OH, address.OA_OH);
				}
			}
		}

		public void SetSelectColumnForAllOrders(bool selectAllOrders)
		{
			foreach (WhsOrder order in Orders)
			{
				if (!order.IsFinalisedOrCancelled)
				{
					order.WD_IsOrderSelectedForFinalisation = selectAllOrders;
				}
			}
		}

		public bool AllOrdersAreFinalised
		{
			get { return !IsAnyOrderNotFinalised && Orders.Count > 0; }
		}

		internal IReadOnlyDictionary<ZGuid, LoadedPackageInfo> GetLoadedPackagesInfo(PkgPackageJob packageJob)
		{
			Argument.NotNull(packageJob, nameof(packageJob));

			var loadedPackageInfo = LoadedPackagesCache ?? new LoadedPackageInfoCache(Factory, new[] { packageJob });
			return loadedPackageInfo.GetLoadedPackageInfo(packageJob);
		}

		LoadedPackageInfoCache LoadedPackagesCache;

		#endregion

		#region CreateAutoInventoryAccuracyManagementTasks

		void CreateAutoInventoryAccuracyManagementTasks()
		{
			using (UseVerifiedNonEmptyCacheHolder())
			{
				this.UpdateIsLocationEmptyAfterFinalisingPickOnPickLines(GetAllPickLines());
				var availableInventoryLines = OrderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(o => o.AvailableInventories).Cast<WhsPickAvailableInventory>().ToArray();

				AutoInventoryAccuracyManager.CreateAutoInventoryAccuracyManagementTasks(availableInventoryLines, Warehouse);
			}
		}

		IWhsAutoInventoryAccuracyManager AutoInventoryAccuracyManager => autoInventoryAccuracyManager ?? (autoInventoryAccuracyManager = ObjectFactory.Get<IWhsAutoInventoryAccuracyManager>());
		IWhsAutoInventoryAccuracyManager autoInventoryAccuracyManager;

		internal VerifiedNonEmptyCacheHolder VerifiedNonEmptyCacheManager => verifiedNonEmptyCacheManager;
		VerifiedNonEmptyCacheHolder verifiedNonEmptyCacheManager;

		IDisposable UseVerifiedNonEmptyCacheHolder()
		{
			if (VerifiedNonEmptyCacheManager != null)
			{
				throw new InvalidOperationException("Nested use of the VerifiedNonEmpty cache is not supported.");
			}

			return new DisposableAction(
				() => verifiedNonEmptyCacheManager = new VerifiedNonEmptyCacheHolder(OrderedInventories, this),
				() => verifiedNonEmptyCacheManager = null);
		}

		#endregion

		#region CartonizationProgressForm

		public void SetCartonizationProgressForm(Func<string, IDisposable> getProgressForm)
		{
			getCartonizationProgressForm = Argument.NotNull(getProgressForm, nameof(getProgressForm));
		}

		IDisposable GetNewCartonizationProgressForm(string status)
		{
			return getCartonizationProgressForm != null ? getCartonizationProgressForm(status) : new DisposableObject();
		}

		Func<string, IDisposable> getCartonizationProgressForm;

		#endregion

		#region AllocatePackageSequence

		public void AllocatePackageSequence()
		{
			var sorter = new PackageComparerForLabelPrinting(this);
			var packagesQueue = new Queue<PkgPackage>(OuterPackages.Where(p => p.PackedItems.Count > 0).OrderBy(p => p, sorter));
			var index = (short)1;
			while (packagesQueue.Count > 0)
			{
				var package = packagesQueue.Dequeue();
				package.KP_Sequence = index++;
			}
		}

		#endregion

		#region AllocatePackageLabels

		public void AllocatePackageLabels()
		{
			AllocatePackageLabels(saveFactory: true);
		}

		void AllocatePackageLabels(bool saveFactory)
		{
			using (GetNewCartonizationProgressForm(Res.GetString("66377e71-45ca-4c2e-b6be-5a3221f79c45", "Allocating Package Labels...")))
			using (new SemaphoreManager(SuspendStockWasUnAllocatedMessagePrompt))
			{
				if (CanAllocatePackageLabels && AllocatePackageLabelsStrategy.RunChecksPriorToCartonisingOrPickingByLabel(this, saveFactory))
				{
					if (!IsInDatabase)
					{
						AllocatePackageLabelsCore(saveFactory);
					}
					else
					{
						var mutex = TryToTakeAllocatingPackageLabelsMutex();
						if (mutex != null)
						{
							using (mutex)
							{
								AllocatePackageLabelsCore(saveFactory);
							}
						}
					}
				}
			}
		}

		public bool CanAllocatePackageLabels => !IsFinalisedOrCancelled && IsPickByLabelOrCartonizeSplitCasesAndLabelsHaveNotBeenCreated && !IsWorkOrderPick && !AnyPackagesOnOrders;

		void AllocatePackageLabelsCore(bool saveFactory)
		{
			var allocatedLabels = false;
			var algorithmHadFatalError = false; // i.e. Cartonise Split Cases ticked, but not a single order/line could be cartonised

			using (SuspendPackingUpdate())
			{
				if (WP_CartoniseSplitCases)
				{
					var cartonisationResult = AllocatePackageLabelsStrategy.CartoniseSplitCases(this, saveFactory);
					algorithmHadFatalError = cartonisationResult == CartonisationResult.Error;
					allocatedLabels = cartonisationResult == CartonisationResult.Cartonised;
				}

				if (!algorithmHadFatalError)
				{
					if (WP_PickCasesByLabel)
					{
						allocatedLabels = AllocatePackageLabelsStrategy.PickCasesByLabel(this) || allocatedLabels;
					}

					if (WP_PickPalletsByLabel)
					{
						allocatedLabels = AllocatePackageLabelsStrategy.PickPalletsByLabel(this) || allocatedLabels;
					}
				}
			}

			if (!algorithmHadFatalError)
			{
				if (allocatedLabels)
				{
					WP_IsCartonised = true;
					Orders.Cast<WhsOrder>().ForEach(o => o.UpdatePackageDataAndWeightAndVolume()); //for each order calculate package sent and pallet sent.
					LogSuccessfulAllocationOfPackageLabels();

					if (saveFactory)
					{
						FactorySaveSafe();
					}
				}
				else
				{
					NotificationSubscriber.AddError(Res.GetString("829712d2-f199-4bd0-bbe5-b0b11cf2be51",
						"No Packages could be created for the Allocations on this Pick. Check that the correct flags are selected for the UOM Types allocated to this Pick."));
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "EDI008:LogReferenceValuesInEnglishOnly", Justification = "Baseline")]
		void LogSuccessfulAllocationOfPackageLabels()
		{
			var reference = Res.GetString("FBC6ACCD-6C5E-4DC2-BCD3-0A04630A8669", "Pick Cartonized: {0}", WP_PickNo);
			Logs.AddNew(Events.PackingCommenced, reference, ZDateTimeOffset.Now,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.Cartonization));
		}

		void FactorySaveSafe()
		{
			try
			{
				Factory.Save();
			}
			catch (ZSaveException exception)
			{
				ZExceptionReporting.HandleSaveException(exception);
			}
			catch (ZCannotSaveException exception)
			{
				ZExceptionReporting.HandleSaveException(exception);
			}
		}

		public bool AnyPackagesOnOrders
		{
			get
			{
				AddFetchHintsForPackageJobs();
				return Orders.Cast<WhsOrder>().Any(o => (o.PackageJob?.Packages.Count ?? 0) > 0);
			}
		}

		internal bool AnyPackagesOnOrdersHasActiveTrolleyJob
		{
			get
			{
				var strategy = ObjectFactory.Get<IWhsPickByTrolleyPickStrategy>("IWhsPickByTrolleyPickStrategy");
				return !IsWorkOrderPick && Orders != null && Orders.Cast<WhsOrder>().Any(o => o.PackageJob != null && strategy.HasAnyPackageAssignedToATrolleyJob(o));
			}
		}

		internal bool AnyPackagesOnOrdersHasActivePickByLabelJob
		{
			get
			{
				var strategy = ObjectFactory.Get<IWhsPickByLabelPickStrategy>("IWhsPickByLabelPickStrategy");
				return !IsWorkOrderPick && Orders != null && Orders.Cast<WhsOrder>().Any(o => o.PackageJob != null && strategy.IsOrderAssociatedWithPickByLabelJob(o));
			}
		}

		public bool AnyPrintedPackageLabels
		{
			get
			{
				AddFetchHintsForPackageJobs();
				return Orders.Cast<WhsOrder>().Any(order => order.PackageJob?.Packages.Any(package => package.IsLabelPrinted) ?? false);
			}
		}

		public bool AnyPackageSentToRTUS
		{
			get
			{
				AddFetchHintsForPackageJobs();
				return Orders.Cast<WhsOrder>().Any(order => order.PackageJob?.Packages.Any(package => package.IsSentToRTUS) ?? false);
			}
		}

		void AddFetchHintsForPackageJobs()
		{
			var packageJobQuery = new ZQuery(PkgPackageJobSchema.KJ_ParentID, Orders.Select(o => o.PK).ToArray());
			Factory.AddFetchHint(typeof(PkgPackageJob), packageJobQuery); // Deep for PkgPackage hints
		}

		public bool AnyOrderWithCarrierBookingAgent
		{
			get
			{
				foreach (var order in Orders)
				{
					Factory.AddFetchHint(typeof(JobDocAddress), FetchHintsHelper.GetJobDocAddressQuery(WhsDocketSchema.Constants.Prefix, order.PK));
				}

				return Orders.Cast<WhsOrder>().Any(order => order.CarrierBookingAgent != null);
			}
		}

		ZGlobalMutex TryToTakeAllocatingPackageLabelsMutex()
		{
			var deletedOrCartonisedInAnotherInstance = false;

			var gotLock = CartonisationMutex.Lock();
			if (!gotLock)
			{
				NotificationSubscriber.AddError(Res.GetString("e9fbb6e2-8ac9-4ea7-ba38-8c0d1b298d1a", "This Pick is already having Package Labels allocated in another instance."));
			}
			else
			{
				// Check if Pick has been deleted or already cartonised within the critical region
				var otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
				var pickInOtherFactory = otherFactory.Load<WhsPick>(PK);

				deletedOrCartonisedInAnotherInstance = pickInOtherFactory == null || pickInOtherFactory.WP_IsCartonised;
				if (deletedOrCartonisedInAnotherInstance)
				{
					CartonisationMutex.Unlock();
					NotificationSubscriber.AddError(Res.GetString("f4219166-26a1-4d89-aa72-0ef94d6c3d4c", "This Pick has already had Package Labels allocated or been deleted in another instance."));
				}
			}

			return gotLock && !deletedOrCartonisedInAnotherInstance ? CartonisationMutex : null;
		}

		IAllocatePackageLabelsStrategy AllocatePackageLabelsStrategy
		{
			get { return allocatePackageLabelsStrategy ?? (allocatePackageLabelsStrategy = new AllocatePackageLabelsStrategy()); }
		}

		IAllocatePackageLabelsStrategy allocatePackageLabelsStrategy;

		#endregion

		#region CancelPackageLabelAllocations

		public void CancelPackageLabelAllocations()
		{
			if (CanCancelPackageLabelAllocations)
			{
				using (ActiveBusinessObjectCollection.DelayListChangedEvents(Factory))
				using (SuspendPackingUpdate())
				{
					Orders.Cast<WhsOrder>().ForEach(o => o.PackageJob?.Packages.DeleteAll());

					foreach (var availInv in
						OrderedInventories.Cast<WhsPickOrderedInventory>()
						.SelectMany(orderedInv => orderedInv.AvailableInventories)
						.Cast<WhsPickAvailableInventory>())
					{
						availInv.SplitByPackType();
					}

					WP_IsCartonised = false;
				}

				Orders.Cast<WhsOrder>().ForEach(o => o.UpdatePackageDataAndWeightAndVolume()); //for each order calculate package sent and pallet sent.
			}
		}

		public bool CanCancelPackageLabelAllocations => !IsFinalisedOrCancelled && !IsReadyForPlanningOrPlanned && WP_IsCartonised && GetAllPickLines().All(pl => !pl.IsPickedFromPutawayLocation);

		#endregion

		#region IDocManagerSupport Members

		public DocManagerInfo DocManagerInfo
		{
			get
			{
				if (docManagerInfo == null)
				{
					docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.WarehousePick);
				}
				return docManagerInfo;
			}
		}
		DocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new WhsPickDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
			=> WarehouseDataRegistry.Instance.GroupOrderedInventoryByCustomAttributes.GetFallBackValueAtAllLevels(Guid.Empty, Warehouse?.WW_GB_RelatedCompanyBranch.ToGuid() ?? Guid.Empty, Guid.Empty)
			? customLabelsGridLayoutPersister_ConfigOrg
			: null;

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add { }
			remove { }
		}

		public OrgHeader CustomLabelsGridLayoutPersister_ConfigOrg
		{
			get { return customLabelsGridLayoutPersister_ConfigOrg; }
			set { customLabelsGridLayoutPersister_ConfigOrg = value; }
		}
		OrgHeader customLabelsGridLayoutPersister_ConfigOrg;

		#endregion

		#region INumberFountainConsumer

		ZString INumberFountainEntityWithID.ID
		{
			get => WP_PickNo;
			set => WP_PickNo = value;
		}

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.WarehousePickNo;

		#endregion

		#region Implementation

		#region Test Handler

		void TestHandler(string desc)
		{
#if DEBUG
			if (Globals.IsTest && ThrowExceptionIfTest)
			{
				throw new WhsException(desc + ". " + LastNotificationMessage);
			}
#endif
		}

#if DEBUG
		public bool ThrowExceptionIfTest = true;
		string LastNotificationMessage;
#endif
		#endregion

		WhsLegacyPickableDocketCollection orders;
		WhsPickOrderedInventoryCollection orderedInventories;
		WhsPackageCollection outerPackages;

		#endregion

		#region Obsolete Code - Will probably never be removed ffs - do not use for new code

		public bool IsMultiOrderPick
		{
			get { return Orders.Count > 1; }
		}

		public IReadOnlyList<WhsPickableDocket> OrdersToPick => ordersToPick ?? (ordersToPick = Array.Empty<WhsPickableDocket>());

		public void CreatePick_OBSOLETE(params WhsPickableDocket[] ordersToSet)
		{
			if (ordersToSet == null)
			{
				throw new ArgumentNullException(nameof(ordersToSet), "Cannot call CreatePick(Orders) with a null Orders object.");
			}

			//#warning this needs refactoring, (is not encapsulated etc).
			ClearAllNotifications();

			SetOrdersToPick(ordersToSet);
			ValidateDataBeforeCreatingThePick();

			if (PickCanBeCreated)
			{
				ObtainNecessaryPickDataFromOrders();

				// wrap pick process in a global mutex to ensure inventory is not committed to more than one pick
				using (ZGlobalMutex mutex = new ZGlobalMutex(MutexIDs.WhsNewPick, NewPickMutexKey))
				{
					if (mutex.Lock())
					{
						try
						{
							var pickProcess = new PickProcess_OBSOLETE(this);
							AllocateCrossDockLines(); // since cross docking now creates pick lines, it needs to be handled correctly in the obsolete process as well.
							pickProcess.CreatePick();
						}
						catch (PickFailedException)
						{
							ResetOrdersOnPickFailure();
							throw;
						}
						finally
						{
							mutex.Unlock();
						}
					}
					else
					{
						throw new MutexLockDeniedException();
					}
				}
			}
			else
			{
				ResetOrdersOnPickFailure();
				NotificationSubscriber.Notify(new ErrorNotification(PickErrorTypes.PickZErrorMessageBox));
			}
		}

		void ResetOrdersOnPickFailure()
		{
			foreach (WhsPickableDocket order in OrdersToPick)
			{
				Orders.Remove(order);
			}
		}

		string NewPickMutexKey
		{
			// for Multiple Order Pick the mutex has been modified to lock at the warehouse level.
			// An improvement to this locking mechanism is to lock all distinct clients that are
			// in the current pick.
			get { return Warehouse.PK.ToString(); }
		}

		bool OrdersBelongToOneWarehouse
		{
			get
			{
				if (OrdersToPick.Count > 1)
				{
					ZGuid warehousePK = OrdersToPick[0].WD_WW_Whs;

					foreach (WhsPickableDocket order in OrdersToPick)
					{
						if (warehousePK != order.WD_WW_Whs)
						{
							return false;
						}
					}
				}
				return true;
			}
		}

		bool PickCanBeCreated
		{
			get { return !HasErrors; }
		}

		void ValidateDataBeforeCreatingThePick()
		{
			ValidateAtleastThereIsOneOrderToPick();
			ValidatePickIsNotCancelledOrFinalised();
			ValidateOrdersBelongToOneWarehouse();
		}

		void ValidateOrdersBelongToOneWarehouse()
		{
			if (!OrdersBelongToOneWarehouse)
			{
				AddRowError(Res.GetString("8d0ae3be-1161-4e36-bad9-c6cf5f2afdf1", "The orders you have selected belong to more than one warehouse."));
			}
		}

		void ValidatePickIsNotCancelledOrFinalised()
		{
			if (IsFinalisedOrCancelled)
			{
				AddRowError(Res.GetString("60346a70-f6a7-4e9e-9db6-6289693a4514", "This pick is either Finalized or Canceled."));
			}
		}

		void ValidateAtleastThereIsOneOrderToPick()
		{
			if (OrdersToPick.Count < 1)
			{
				AddRowError(Res.GetString("93dae5ab-3de6-42bb-9be1-a9b0f500abda", "At least one order must be selected to pick."));
			}
		}

		void ObtainNecessaryPickDataFromOrders()
		{
			WP_WW_Whs = OrdersToPick[0].WD_WW_Whs;
			WP_PickOption = OrdersToPick[0].WD_PickOption;
		}

		void SetOrdersToPick(WhsPickableDocket[] whsOrders)
		{
			// ensure orders are in pick factory
			ordersToPick = new WhsPickableDocket[whsOrders.Length];
			for (int i = 0; i < whsOrders.Length; i++)
			{
				ordersToPick[i] = (WhsPickableDocket)Factory.Load(whsOrders[i].GetType(), whsOrders[i].PK);
				if (!Orders.Contains(ordersToPick[i]))
				{
					Orders.Add(ordersToPick[i]);
				}
			}
		}

		WhsPickableDocket[] ordersToPick;

		#endregion

		#region ICartageParent Members

		ICartageParent CurrentOrderCartageParent
		{
			get { return CurrentOrder; }
		}

		ZGuid ICartageParent.BranchPK
		{
			get { return CurrentOrderCartageParent.BranchPK; }
		}

		ZGuid ICartageParent.CartageParentID
		{
			get { return CurrentOrderCartageParent.CartageParentID; }
		}

		ZString ICartageParent.CartageParentTableCode
		{
			get { return CurrentOrderCartageParent.CartageParentTableCode; }
		}

		IReadOnlyCollection<CartageType> ICartageParent.CartageTypes => (CurrentOrderCartageParent != null) ? CurrentOrderCartageParent.CartageTypes : Array.Empty<CartageType>();

		event EventHandler ICartageParent.CartageTypesChanged
		{
			add { CurrentOrderCartageParent.CartageTypesChanged += value; }
			remove { CurrentOrderCartageParent.CartageTypesChanged -= value; }
		}

		ControllerID ICartageParent.ControllerID
		{
			get { return CurrentOrderCartageParent.ControllerID; }
		}

		BusinessObjectFactory ICartageParent.Factory
		{
			get { return CurrentOrderCartageParent.Factory; }
		}

		CartageType ICartageParent.GetLocalCartageType
		{
			get { return CurrentOrderCartageParent.GetLocalCartageType; }
		}

		ZString ICartageParent.GoodsDescription
		{
			get { return CurrentOrderCartageParent.GoodsDescription; }
		}

		bool ICartageParent.HasChanges
		{
			get { return CurrentOrderCartageParent.HasChanges; }
		}

		ZString ICartageParent.HumanReadableName
		{
			get { return CurrentOrderCartageParent.HumanReadableName; }
		}

		bool ICartageParent.IsInDatabase
		{
			get { return CurrentOrderCartageParent.IsInDatabase; }
		}

		ZGuid ICartageParent.JobHeaderPK
		{
			get { return CurrentOrderCartageParent.JobHeaderPK; }
		}

		ZGuid ICartageParent.LocalClientAddressPK
		{
			get { return CurrentOrderCartageParent.LocalClientAddressPK; }
		}

		ZString ICartageParent.OrderReferenceNumber
		{
			get { return CurrentOrderCartageParent.OrderReferenceNumber; }
		}

		ZString ICartageParent.ServiceLevel
		{
			get { return CurrentOrderCartageParent.ServiceLevel; }
		}

		ZString ICartageParent.TotalPackType
		{
			get { return CurrentOrderCartageParent.TotalPackType; }
		}

		ZInt ICartageParent.TotalPackages
		{
			get { return CurrentOrderCartageParent.TotalPackages; }
		}

		ZDecimal ICartageParent.TotalVolume
		{
			get { return CurrentOrderCartageParent.TotalVolume; }
		}

		ZString ICartageParent.TotalVolumeUnit
		{
			get { return CurrentOrderCartageParent.TotalVolumeUnit; }
		}

		ZDecimal ICartageParent.TotalWeight
		{
			get { return CurrentOrderCartageParent.TotalWeight; }
		}

		ZString ICartageParent.TotalWeightUnit
		{
			get { return CurrentOrderCartageParent.TotalWeightUnit; }
		}

		ZString ICartageParent.UniqueConsignmentID
		{
			get { return CurrentOrderCartageParent.UniqueConsignmentID; }
		}

		ZString ICartageParent.WayBillNumber
		{
			get { return CurrentOrderCartageParent.WayBillNumber; }
		}

		bool ICartageParent.RebuildLocalCartageMenuOnClick
		{
			get { return true; }
		}

		bool ICartageParent.UseJobTotals
		{
			get { return CurrentOrderCartageParent.UseJobTotals; }
		}

		void ICartageParent.CartageCreatedAndSaved()
		{
			OnCartageCreatedAndSaved();
		}

		IStmALogParent ICartageParent.BusinessObjectForRelatedEvents
		{
			get { return CurrentOrderCartageParent.BusinessObjectForRelatedEvents; }
		}

		IDocManagerSupport ICartageParent.BusinessObjectForRelatedEDocs
		{
			get { return CurrentOrderCartageParent.BusinessObjectForRelatedEDocs; }
		}

		#endregion

		#region Notifications

		public NotificationManager NotificationManager
		{
			get;
			private set;
		}

		public INotifications NotificationSubscriber
		{
			get { return NotificationManager.Peek; }
		}

		#endregion

		#region ICartageLooseCargo Members

		ICartageLooseCargo CurrentOrderCartageLooseCargo
		{
			get { return CurrentOrder; }
		}

		ZString ICartageLooseCargo.BookedDimensionUnit
		{
			get { return CurrentOrderCartageLooseCargo.BookedDimensionUnit; }
		}

		ZDecimal ICartageLooseCargo.BookedHeight
		{
			get { return CurrentOrderCartageLooseCargo.BookedHeight; }
		}

		ZDecimal ICartageLooseCargo.BookedLength
		{
			get { return CurrentOrderCartageLooseCargo.BookedLength; }
		}

		ZString ICartageLooseCargo.BookedPackType
		{
			get { return CurrentOrderCartageLooseCargo.BookedPackType; }
		}

		ZInt ICartageLooseCargo.BookedPackages
		{
			get { return CurrentOrderCartageLooseCargo.BookedPackages; }
		}

		ZDecimal ICartageLooseCargo.BookedVolume
		{
			get { return CurrentOrderCartageLooseCargo.BookedVolume; }
		}

		ZString ICartageLooseCargo.BookedVolumeUnit
		{
			get { return CurrentOrderCartageLooseCargo.BookedVolumeUnit; }
		}

		ZDecimal ICartageLooseCargo.BookedWeight
		{
			get { return CurrentOrderCartageLooseCargo.BookedWeight; }
		}

		ZString ICartageLooseCargo.BookedWeightUnit
		{
			get { return CurrentOrderCartageLooseCargo.BookedWeightUnit; }
		}

		ZDecimal ICartageLooseCargo.BookedWidth
		{
			get { return CurrentOrderCartageLooseCargo.BookedWidth; }
		}

		IReadOnlyCollection<UNDGDataItem> ICartageLooseCargo.DangerousGoods
		{
			get { return CurrentOrderCartageLooseCargo.DangerousGoods; }
		}

		#endregion

		#region ITransportJobLinkProvider Members

		public event EventHandler TransportJobCreatedAndSaved
		{
			add
			{
				transportJobCreatedAndSaved += value;

				var currentOrder = CurrentOrder;
				if (currentOrder != null)
				{
					currentOrder.TransportJobCreatedAndSaved += value;
				}
			}
			remove
			{
				transportJobCreatedAndSaved -= value;

				var currentOrder = CurrentOrder;
				if (currentOrder != null)
				{
					currentOrder.TransportJobCreatedAndSaved -= value;
				}
			}
		}

		event EventHandler transportJobCreatedAndSaved;

		void OnCartageCreatedAndSaved()
		{
			if (transportJobCreatedAndSaved != null)
			{
				transportJobCreatedAndSaved(this, EventArgs.Empty);
			}
		}

		TransportJobResult ITransportJobLinkProvider.TransportJobResult
		{
			get
			{
				var currentOrder = CurrentOrder;
				return (currentOrder != null) ? ((ITransportJobLinkProvider)currentOrder).TransportJobResult : null;
			}
		}

		#endregion

		#region IMasterAssigner

		IEnumerable<ILineStaffAssigner> IMasterStaffAssigner.Lines
			=> OrderedInventories.Cast<WhsPickOrderedInventory>().SelectMany(l => l.AvailableInventories).Cast<WhsPickAvailableInventory>();

		bool IMasterStaffAssigner.IsJobAssignable => !IsFinalisedOrCancelled;

		bool IMasterStaffAssigner.CanAssignOrUnAssignAnyLines => !IsFinalisedOrCancelled
			&& OrderedInventories.Cast<WhsPickOrderedInventory>().Any(l => l.AvailableInventories.Cast<ILineStaffAssigner>().Any(x => x.CanAssignOrUnAssignLine()));

		#endregion

		#region IWorkflowProvider Members

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowType; }
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable]
		[ChildEditableTestExclude]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WhsPickProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var selectionCriteria = new ColumnValueRanker();

			var singleClientPK = Orders.Cast<WhsDocket>().SameOrDefault(o => o.WD_OH_Client);
			if (!singleClientPK.IsEmpty)
			{
				selectionCriteria.Add(ProcessTaskTemplateSchema.P0_OH_Client, singleClientPK, ZGuid.Empty);
			}
			else
			{
				selectionCriteria.Add(ProcessTaskTemplateSchema.P0_OH_Client, ZGuid.Empty);
			}

			selectionCriteria.Add(ProcessTaskTemplateSchema.P0_WW, WP_WW_Whs, ZGuid.Empty);
			return selectionCriteria;
		}

		public ZString WorkflowType
		{
			get { return WorkflowDescriptors.WhsPickWorkflowDescriptorCode; }
		}

		#endregion

		#region IWhsLogEventParent Members

		ZString IWhsLogEventParent.EventFreeTextReference
		{
			get { return WP_PickNo; }
		}

		string IWhsLogEventParent.EventReferenceParameterType
		{
			get { return Constants.EventReferenceParameterTypes.Pick; }
		}

		#endregion

		partial void OnFinalisingSelectedOrdersForTest(WhsPickableDocket order);

		#region Unique Index Failure Handler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new WarehousePickNumberFountainUniqueIndexFailureHandler(this)); }
		}

		class WarehousePickNumberFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public WarehousePickNumberFountainUniqueIndexFailureHandler(WhsPick pick)
				: base(WhsPickSchema.Constants.Indexes.NR_UX__WP_PickNo, pick)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix
			{
				get { return Env.NumberFountains.WarehousePickNo; }
			}
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		#endregion

		#region	IProcessHandlingInfoProvider Members

		public ProcessHandlingInfo ProcessHandlingInfo => new WhsPickProcessHandlingInfo(this);

		#endregion

		#region ICreditControlledDocumentDelivery

		ScreeningParty[] ICreditControlledDocumentDelivery.GetScreeningParties()
		{
			var orders = Orders;
			return orders.Cast<ICreditControlledDocumentDelivery>().SelectMany(order => order.GetScreeningParties()).ToArray();
		}

		void ICreditControlledDocumentDelivery.RaiseOnGetDocumentLogin(SecurityLoginEventArgs e)
		{
			GetDocumentLogin?.Invoke(this, e);
		}

		public event EventHandler<SecurityLoginEventArgs> GetDocumentLogin;

		bool ICreditControlledDocumentDelivery.IsDPSFreightMovementRestricted
		{
			get
			{
				var orders = Orders;
				return orders.Cast<ICreditControlledDocumentDelivery>().Any(order => order.IsDPSFreightMovementRestricted);
			}
		}

		bool ICreditControlledDocumentDelivery.IsAviationSecurityFreightMovementRestricted => false;

		OrgHeader[] ICreditControlledDocumentDelivery.OrganisationsForCreditChecks
		{
			get
			{
				var orders = Orders;
				return orders.Cast<ICreditControlledDocumentDelivery>().SelectMany(order => order.OrganisationsForCreditChecks).Distinct().ToArray();
			}
		}

		string ICreditControlledDocumentDelivery.DescriptionOfOrganisationBeingCheckedForCredit => Res.GetString("2e451b8d-f041-4b18-851c-d237e0727855", "Warehouse Client");

		CustomMessageBoxCallback ICreditControlledDocumentDelivery.DocumentLoginMessageBoxCallback { get; set; }

		string[] IRelatedJobNumber.JobNumber => Array.Empty<string>();

		#endregion

		#region ICriticalChangesVersionID

		ZGuid ICriticalChangesVersionID.CriticalChangesVersionID
		{
			set => WP_CriticalChangesVersionID = value;
			get => WP_CriticalChangesVersionID;
		}

		bool ICriticalChangesVersionID.IsImmutableStatus => IsFinalisedOrCancelled;

		#endregion

		#region ITaskPlanningJob

		ZGuid ITaskPlanningJob.PK => PK;

		ZGuid ITaskPlanningJob.WarehousePK => WP_WW_Whs;

		ZString ITaskPlanningJob.TaskPlanningStatus
		{
			get => WP_TaskPlanningStatus;
			set => WP_TaskPlanningStatus = value;
		}

		string ITaskPlanningJob.JobID => WP_PickNo;

		ZString ITaskPlanningJob.HumanReadableNameWithoutID => HumanReadableNameWithoutID;

		string ITaskPlanningJob.SpecialCannotUpdateTaskPlanningStatusReason => string.Empty;

		void ITaskPlanningJob.ClearFKForProcessTasks(ISet<ZGuid> processTaskPKs)
		{
			var pickByLabelJobs = Factory.Load<IWhsPickByLabelJob>(new ZQuery(WhsPickByLabelJobSchema.WTK_P9_Task, processTaskPKs));
			pickByLabelJobs.ForEach(job => job.WTK_P9_Task = ZGuid.Empty);

			Orders.Where(order => processTaskPKs.Contains(order.WD_P9_PackingTask)).ForEach(order => order.WD_P9_PackingTask = ZGuid.Empty);

			var pickLines = Factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_P9_Task, processTaskPKs));
			pickLines.ForEach(line => line.WZ_P9_Task = ZGuid.Empty);
		}

		#endregion

		#region VerifiedNonEmptyCacheHolder

		internal sealed class VerifiedNonEmptyCacheHolder
		{
			public VerifiedNonEmptyCacheHolder(WhsPickOrderedInventoryCollection orderedInventories, WhsPick pick)
			{
				VerifiedNonEmptyCache = new Lazy<HashSet<(ZGuid ClientPK, ZGuid SupplierPartPK, ZGuid LocationPK)>>(() =>
				{
					if (pick.Warehouse.WW_VerifyEmptyLocations)
					{
						var productAndLocationByDocketLine = new Dictionary<ZGuid, (ZGuid ClientPK, ZGuid SupplierPartPK, ZGuid LocationPK)>();
						return orderedInventories.Cast<WhsPickOrderedInventory>()
							.SelectMany(oi => oi.PickLines)
							.Where(pl => pl.WZ_VerifiedEmpty == "N")
							.Select(pl =>
							{
								var inventoryPK = pl.WZ_WE_OriginalPickedInventoryLine.IsValid ? pl.WZ_WE_OriginalPickedInventoryLine : pl.WZ_WE_InventoryLine;
								if (!productAndLocationByDocketLine.TryGetValue(inventoryPK, out var result))
								{
									var inventory = pl.Factory.Load<WhsInventoryView>(inventoryPK);
									productAndLocationByDocketLine[inventoryPK] = result = (inventory.WI_OH_Client, inventory.WI_OP, inventory.WI_WL);
								}
								return result;
							}).ToHashSet();
					}
					else
					{
						return new HashSet<(ZGuid ClientPK, ZGuid SupplierPartPK, ZGuid LocationPK)>();
					}
				});
			}

			public bool GetVerifiedNonEmpty(ZGuid clientPK, ZGuid supplierPartPK, ZGuid locationPK) => VerifiedNonEmptyCache.Value.Contains((clientPK, supplierPartPK, locationPK));

			Lazy<HashSet<(ZGuid ClientPK, ZGuid SupplierPartPK, ZGuid LocationPK)>> VerifiedNonEmptyCache { get; }
		}

		#endregion

		#region DistinctClientProductPickFacesInWhsAwaitingReplenishment

		public PickFaceProductsAwaitingReplenishmentCollection DistinctClientProductPickFacesInWhsAwaitingReplenishment
		{
			get
			{
				return distinctClientProductPickFacesInWhsAwaitingReplenishment ??= BuildDistinctClientProductPickFacesInWhsAwaitingReplenishments();
			}
		}

		PickFaceProductsAwaitingReplenishmentCollection BuildDistinctClientProductPickFacesInWhsAwaitingReplenishments()
		{
			var query = new ZDBOnlyQuery(typeof(WhsPickFaceAwaitingReplenishmentView));
			query.AddToFilter(WhsPickFaceAwaitingReplenishmentViewSchema.WWP_WW_Whs, WP_WW_Whs);
			query.AddToFilter(WhsPickFaceAwaitingReplenishmentViewSchema.WWP_OP, OrderedInventories.Select(o => o.SupplierPartPK));

			var pickFaceAwaitingReplenishmentViews = new WhsPickFaceAwaitingReplenishmentViewCollection(Factory, query);
			distinctClientProductPickFacesInWhsAwaitingReplenishment = new PickFaceProductsAwaitingReplenishmentCollection(Factory);
			var pickFaceProductsDict = new Dictionary<(ZGuid, ZGuid), PickFaceProductsAwaitingReplenishment>();

			foreach (var pickFaceAwaitingReplenishmentView in pickFaceAwaitingReplenishmentViews)
			{
				var clientPK = pickFaceAwaitingReplenishmentView.WWP_OH_Client;
				var productPK = pickFaceAwaitingReplenishmentView.WWP_OP;
				if (!pickFaceProductsDict.TryGetValue((clientPK, productPK), out var pickFaceProductsAwaitingReplenishment))
				{
					Factory.AddFetchHint(OrgHeaderSchema.PK, clientPK);
					Factory.AddFetchHint(OrgSupplierPartSchema.PK, productPK);

					pickFaceProductsAwaitingReplenishment = new PickFaceProductsAwaitingReplenishment(clientPK, productPK, Factory);
					pickFaceProductsDict.Add((clientPK, productPK), pickFaceProductsAwaitingReplenishment);
					distinctClientProductPickFacesInWhsAwaitingReplenishment.Add(pickFaceProductsAwaitingReplenishment);
				}

				pickFaceProductsAwaitingReplenishment.PickFacesAwaitingReplenishmentWithSameWhsClientProduct.Add(pickFaceAwaitingReplenishmentView);
			}

			var comparer = (IComparer<ISupportPickPriority>)ObjectFactory.Get<ISupportObjectPickPriorityComparer>();
			foreach (var pickFaceProductsAwaitingReplenishment in distinctClientProductPickFacesInWhsAwaitingReplenishment.Cast<PickFaceProductsAwaitingReplenishment>())
			{
				pickFaceProductsAwaitingReplenishment.PickFacesAwaitingReplenishmentWithSameWhsClientProduct.ApplySort(comparer);
			}

			return distinctClientProductPickFacesInWhsAwaitingReplenishment;
		}

		PickFaceProductsAwaitingReplenishmentCollection distinctClientProductPickFacesInWhsAwaitingReplenishment;

		#endregion
	}

	#region Document Supporter

	public class WhsPickDocumentSupporter : DocumentSupporter, ISupportCustomizedDocumentPrintSet
	{
		public WhsPickDocumentSupporter(WhsPick pick)
			: base(pick)
		{
		}

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);
			documentEventSource.DocumentPrinted += DocumentEventSource_DocumentPrinted;
			documentEventSource.DocumentPrintRequested += DocumentEventSource_DocumentPrintRequested;
			bookingDeliveryCancelled = false;
		}

		protected WhsPick Pick
		{
			get { return (WhsPick)BusinessObject; }
		}

		#region BusinessContext

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WhsDespatch; }
		}

		#endregion

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.WhsReleaseCustomiseDocuments; }
		}

		#region DocumentPrintRequested

		[SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This event will be triggered only if GUI is present")]
		void DocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			DocumentCommand command = (DocumentCommand)e.MenuItem;
			Constants.DataContext dataContext = GetDataContext(command);

			if ((dataContext == Constants.DataContext.GenericFreightJobWhsPick || dataContext == Core.Constants.DataContext.WhsPick) && Pick.WP_PickStatus == PickStatus.Codes.Building)
			{
				Globals.Message.ShowInformation(string.Format(Culture.Invariant, (NoResString)"{0} cannot be printed until Fulfillment Rules are met or overridden.", command.SU_MenuName), (NoResString)"Document Printing"); // This event will be triggered only if GUI is present
				e.Cancel = true;
			}
			else if (dataContext == Core.Constants.DataContext.WhsPackageLabels && Pick.Orders.ContainsWorkOrders)
			{
				Globals.Message.ShowInformation((NoResString)"Work Orders do not require Package Labels.", (NoResString)"Package Labels"); // This event will be triggered only if GUI is present
				e.Cancel = true;
			}
			else if (dataContext == Core.Constants.DataContext.WhsDeliveryLabels && Pick.Orders.ContainsWorkOrders)
			{
				Globals.Message.ShowInformation((NoResString)"Work Orders do not require Delivery Labels.", (NoResString)"Delivery Labels"); // This event will be triggered only if GUI is present
				e.Cancel = true;
			}
		}

		#endregion

		#region DocumentEventSource_DocumentPrinted

		void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
		{
			var command = e.MenuItem as DocumentCommand;
			var dataContext = GetDataContext(command);
			if (dataContext == Core.Constants.DataContext.WhsPick || dataContext == Core.Constants.DataContext.GenericFreightJobWhsPick)
			{
				if (Pick.WP_PickStatus == PickStatus.Codes.Created && !Pick.WP_IsAwaitingReplenishment && Pick.IsFulfillmentRulesMet())
				{
					Pick.WP_PickStatus = PickStatus.Codes.PickSlip;
				}
			}

			if (command.SU_MenuName == PrintAllPackageLabelMenuItemName)
			{
				foreach (var package in Pick.OuterPackages)
				{
					if (!package.IsLabelPrinted)
					{
						package.IsLabelPrinted = true;
					}
				}
			}

			// Hacky save in event. Should be handled by DocEngine architecture. We raised this issue with them, hopefully will be addressed.
			TryToSaveChanges(Factory);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "hard-coded document name")]
		const string PrintAllPackageLabelMenuItemName = "All Package Labels for a Pick.";

		[SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This event will be triggered only if GUI is present")]
		void TryToSaveChanges(BusinessObjectFactory factoryToSave)
		{
			var needSave = false;
			var errors = new ZStringBuilder();
			foreach (var bizO in ((IBusinessObjectFactoryInternals)factoryToSave).AllBusinessObjects.Where(x => x.HasChanges))
			{
				needSave = true;
				if (bizO.HasErrors)
				{
					errors.Append(bizO.GetErrors().ToUniqueMessageListString());
				}
			}

			try
			{
				if (errors.Length > 0)
				{
					Globals.Message.ShowError(errors.ToStringWithNewLineBetweenAppends()); // This event will be triggered only if GUI is present
				}
				else if (needSave)
				{
					factoryToSave.Save();
				}
			}
			catch (ZCannotSaveException ex)
			{
				Globals.Message.ShowError(ex.Message); // This event will be triggered only if GUI is present
			}
			catch (ZSaveException ex)
			{
				Globals.Message.ShowError(ex.Message); // This event will be triggered only if GUI is present
			}
		}

		#endregion

		#region Document Wrappers

		#region GetDocumentWrappersInternal

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			var result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Pick);
			if (result == null)
			{
				switch (dataContext)
				{
					case Core.Constants.DataContext.GenericFreightJobOrder:
						result = new DocumentWrapper[] { (DocumentWrapper)ObjectFactory.New<IFreightWrapperFromWhsBO>(Pick, Pick.Factory) };
						break;
					case Core.Constants.DataContext.GenericFreightJobOrders:
						result = GetResultForGenericFreightJobOrders();
						break;
					case Core.Constants.DataContext.WhsPickNonPickedItems:
						if (Pick.HasNonAllocatedOrderedInventory)
						{
							result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Enterprise.Core.Constants.DataContext.WhsPick, Pick) };
						}
						break;
					case Core.Constants.DataContext.WhsPickShortfallItems:
						if (Pick.HasShortfallItems)
						{
							result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Enterprise.Core.Constants.DataContext.WhsPick, Pick) };
						}
						break;
					case Core.Constants.DataContext.WhsPick:
						{
							result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(dataContext, Pick) };
						}
						break;
					case Core.Constants.DataContext.WhsOrder:
						if (!Pick.Orders.ContainsWorkOrders)
						{
							result = GetOrderDocumentWrappers(Core.Constants.DataContext.WhsOrder);
						}
						else
						{
							result = Array.Empty<DocumentWrapper>();
						}
						break;
					case Core.Constants.DataContext.WhsPickableDocket:
						result = GetPickableDocketDocumentWrappers();
						break;
					case Core.Constants.DataContext.WhsPackageLabels:
						IOrdersDocumentSupport packageLabelsDocumentSupport = Pick.Orders;
						if (packageLabelsDocumentSupport != null)
						{
							result = packageLabelsDocumentSupport.GetPackageLabelDocumentWrappers();
						}
						else
						{
							result = Array.Empty<DocumentWrapper>();
						}
						break;

					case Core.Constants.DataContext.WhsDeliveryLabels:
						IOrdersDocumentSupport deliveryLabelsDocumentSupport = Pick.Orders;
						if (deliveryLabelsDocumentSupport != null)
						{
							result = deliveryLabelsDocumentSupport.GetDeliveryLabelDocumentWrappers(Pick.CallOnWhsOrderToPrint);
						}
						else
						{
							result = Array.Empty<DocumentWrapper>();
						}
						break;

					case Core.Constants.DataContext.GenericFreightJob:
						if (DocumentType(commandBeingRun) == "WPA" && !Pick.Orders.ContainsWorkOrders)
						{
							result = GetOrderDocumentWrappers(Core.Constants.DataContext.GenericFreightJob);
						}
						else
						{
							result = Array.Empty<DocumentWrapper>();
						}
						break;
					case Core.Constants.DataContext.GenericFreightJobWhsPick:
						result = DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, Pick);
						break;
					default:
						break;
				}
			}

			return result;
		}

		DocumentWrapper[] GetResultForGenericFreightJobOrders()
		{
			var result = new DocumentWrapper[Pick.Orders.Count];
			var i = 0;
			foreach (var order in Pick.Orders)
			{
				var a = DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, order);
				result[i] = a[0];
				i++;
			}

			return result;
		}

		ZString DocumentType(IStmMenuItem commandBeingRun)
		{
			ZString result = ZString.Empty;
			RefDocType docType = DocumentTypeCore(commandBeingRun);
			if (docType != null)
			{
				result = docType.RT_DocType;
			}
			return result;
		}

		RefDocType DocumentTypeCore(IStmMenuItem commandBeingRun)
		{
			DocumentCommand command = commandBeingRun as DocumentCommand;

			if (command != null)
			{
				ZQuery filter = new ZQuery(StmMenuTemplatePivotSchema.SI_SU, command.PK);
				StmMenuTemplatePivot firstPivot = Factory.LoadTop1<StmMenuTemplatePivot>(filter);

				if (firstPivot != null)
				{
					return firstPivot.DocType;
				}
			}
			return null;
		}

		#endregion

		#region GetOrderDocumentWrappers

		DocumentWrapper[] GetOrderDocumentWrappers(Constants.DataContext dataContext)
		{
			List<DocumentWrapper> wrappers = new List<DocumentWrapper>();

			foreach (WhsPickableDocket order in Pick.Orders)
			{
				if (dataContext == Core.Constants.DataContext.GenericFreightJob)
				{
					wrappers.AddRange(DocumentWrapperFactory.GenerateGenericWrappers(dataContext, (WhsOrder)order));
				}
				else
				{
					wrappers.Add(DocumentWrapperFactory.CreateWrapper(Enterprise.Core.Constants.DataContext.WhsOrder, order));
				}
			}

			return wrappers.ToArray();
		}
		#endregion

		#region GetPickableDocketDocumentWrapper

		DocumentWrapper[] GetPickableDocketDocumentWrappers()
		{
			List<DocumentWrapper> wrappers = new List<DocumentWrapper>();

			foreach (WhsPickableDocket order in Pick.Orders)
			{
				wrappers.Add(DocumentWrapperFactory.CreateWrapper(Enterprise.Core.Constants.DataContext.WhsPickableDocket, order));
			}

			return wrappers.ToArray();
		}

		#endregion

		#endregion

		#region DataContext

		Constants.DataContext GetDataContext(DocumentCommand command)
		{
			var result = Constants.DataContext.None;

			var menuItems = new List<StmMenuItem>(command.ChildMenus.Cast<StmMenuMenuPivotBase>().Select(m => m.Outward));
			menuItems.Add(command);

			var documentPivots = Factory.Load<StmMenuTemplatePivotBase>(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, menuItems.Select(m => m.PK))).ToList();
			var templates = documentPivots.Select(d => d.Template);
			var configs = new List<StmMenuDocumentConfig>();
			documentPivots.ForEach(d => configs.AddRange(d.DocConfigs.Cast<StmMenuDocumentConfig>()));

			var dataContexts = new List<ZString>();
			dataContexts.AddRange(configs.Select(c => c.S3_OverrideDataContext));
			dataContexts.AddRange(templates.Select(t => t.SO_DataContext));

			var validDataContexts = new[] { nameof(Constants.DataContext.GenericFreightJob),
				nameof(Constants.DataContext.GenericFreightJobWhsPick),
				nameof(Constants.DataContext.WhsPick),
				nameof(Constants.DataContext.WhsOrder),
				nameof(Constants.DataContext.WhsDeliveryLabels),
				nameof(Constants.DataContext.WhsPackageLabels) };

			foreach (string dataContext in dataContexts)
			{
				if (validDataContexts.Contains(dataContext))
				{
					Constants.DataContext validContext;
					if (Enum.TryParse(dataContext, out validContext))
					{
						result = validContext;
						break;
					}
				}
			}

			return result;
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
			{
				Core.Constants.DataContext.WhsPick,
				Core.Constants.DataContext.WhsPickShortfallItems,
				Core.Constants.DataContext.WhsPickNonPickedItems,
				Core.Constants.DataContext.WhsOrder,
				Core.Constants.DataContext.WhsPackageLabels,
				Core.Constants.DataContext.WhsDeliveryLabels,
				Core.Constants.DataContext.WhsPickableDocket,
				Core.Constants.DataContext.GenericFreightJob,
				Core.Constants.DataContext.GenericFreightJobOrder,
				Core.Constants.DataContext.GenericFreightJobWhsPick
			};
		}

		#endregion

		#region GetChildCollection

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result;

			switch (businessContext)
			{
				case BusinessContext.Packing:
					result = GetPackageJobs(menuToBeRun);
					break;

				case BusinessContext.DtbBooking:
					var tbParents = Pick.Orders.OfType<IDtbBookingParent>().ToArray();

					if (tbParents.Length > 0)
					{
						var bookings = TransportBookingLoader.GetBookingsToDeliver(tbParents, menuToBeRun);
						bookingDeliveryCancelled = !bookings.Any();
						result = bookings.Cast<IDocumentSupportable>().ToArray();
					}
					else
					{
						result = null;
					}

					break;

				case BusinessContext.WhsOrder:
					result = GetOrders(menuToBeRun);
					break;

				default:
					result = base.GetChildCollection(menuToBeRun, businessContext, childCommand);
					break;
			}

			return result;
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return base.ShowReasonForNotPrinting(dataContext, commandBeingRun) && !bookingDeliveryCancelled;
		}

		bool bookingDeliveryCancelled;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a menu Path")]
		IDocumentSupportable[] GetPackageJobs(IStmMenuItem menuToBeRun)
		{
			const string OrdersMenuPath = "Orders";

			var useSelectedOrder = menuToBeRun.SU_MenuPath.Contains(OrdersMenuPath, StringComparison.Ordinal);
			var orders = (useSelectedOrder && Pick.CurrentOrder != null) ? new[] { Pick.CurrentOrder } : Pick.Orders.ToArray();

			var result = new List<IDocumentSupportable>();
			foreach (WhsPickableDocket order in orders)
			{
				var iPackingParent = order as IPackingParent;
				if (iPackingParent != null)
				{
					var packingJob = PkgPackageJob.LoadPackageJob(iPackingParent);
					if (packingJob != null)
					{
						result.Add(packingJob);
					}
				}
			}

			return result.ToArray();
		}

		IDocumentSupportable[] GetOrders(IStmMenuItem menuToBeRun) => Pick.Orders.Cast<WhsPickableDocket>().ToArray();

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get { return new[] { BusinessContext.Packing, BusinessContext.DtbBooking, BusinessContext.WhsOrder }; }
		}

		#endregion

		#region GetContactOrganisation

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			IDocumentDeliveryContact result = null;
			if (Pick.Orders.Count > 0)
			{
				if (Pick.CurrentOrder != null)
				{
					result = GetContactOrganisationCore(menuName, Pick.CurrentPickableDocket);
				}
				else //required when printing from outside of the form
				{
					result = GetContactOrganisationCore(menuName, Pick.Orders[0]);
				}
			}
			return result;
		}

		OrgHeaderContact GetContactOrganisationCore(ZString menuName, WhsPickableDocket whsOrder)
		{
			OrgHeaderContact result = null;
			if (whsOrder != null)
			{
				if (menuName.StartsWith((NoResString)"Cartage Advice", StringComparison.Ordinal) && whsOrder.TransportCoDocAddress.Organisation != null) // It's inside data
				{
					result = new OrgHeaderContact(whsOrder.TransportCoDocAddress.Organisation, whsOrder.Consignee, whsOrder.TransportCoDocAddress.Address);
				}
				else
				{
					result = new OrgHeaderContact(whsOrder.Client, whsOrder.Consignee, null);
				}
			}
			return result;
		}

		#endregion

		//Tests in PackageLabelsHelper
		public DocumentEngineIntegration.IPrintTask GetCustomizedDocumentPrintSet(IDocumentCommand command)
		{
			var commandAsDocCommand = (DocumentCommand)command;
			var docPack = PackageLabelsHelper.GetDocumentPack(Pick, commandAsDocCommand);
			var documentPrintSet = new DocumentPrintSet(commandAsDocCommand, null);

			documentPrintSet.Add(docPack);
			return documentPrintSet;
		}

		public bool ShouldCustomizedDocumentPrintSet(IDocumentCommand command)
		{
			return command.SU_MenuName == PrintAllPackageLabelMenuItemName;
		}

		#region GetBODocDataProvidersNotFoundMessage

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			if ((dataContextValue.DataContext == Core.Constants.DataContext.WhsOrder ||
					dataContextValue.DataContext == Core.Constants.DataContext.WhsDeliveryLabels ||
					dataContextValue.DataContext == Core.Constants.DataContext.WhsPickableDocket ||
					dataContextValue.DataContext == Core.Constants.DataContext.GenericFreightJobOrders ||
					dataContextValue.DataContext == Core.Constants.DataContext.WhsPackageLabels) && Pick.Orders.Count == 0)
			{
				return Res.GetString("GetBODocDataProvidersNotFoundMessage-NotExists-Order", "Pick does not have any order.");
			}
			else if (dataContextValue.DataContext == Core.Constants.DataContext.WhsPickNonPickedItems && !Pick.HasNonAllocatedOrderedInventory)
			{
				return Res.GetString("GetBODocDataProvidersNotFoundMessage-NotExists-HasNonPickedItems", "Pick does not have non picked item.");
			}
			else if (dataContextValue.DataContext == Core.Constants.DataContext.WhsPickShortfallItems && !Pick.HasShortfallItems)
			{
				return Res.GetString("GetBODocDataProvidersNotFoundMessage-NotExists-HasShortfallItems", "Pick does not have short fall item.");
			}
			else
			{
				return base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			}
		}

		#endregion

	}

	#endregion

	#region Obsolete Code - Will soon be removed - do not use for new code

	class PickProcess_OBSOLETE
	{
		#region Constructors

		public PickProcess_OBSOLETE(WhsPick pick)
		{
			this.Pick = pick;
		}

		#endregion

		#region Public

		public void CreatePick()
		{
			CreatePickCore();
		}

		#endregion

		#region Picking

		void CreatePickCore()
		{
			Pick.AutoAllocateItems();
			ProcessOrders();
		}

		#endregion

		#region Process Orders

		void ProcessOrders()
		{
			if (Pick.GetAllPickLines().Any())
			{
				// once the stock is picked it must be allocated (assembled) to orders
				PostAssembleOrderProcessing();
				CalcOrderTotals();
				SetPickFKOnOrders();
			}
			else
			{
				// error no stock in warehouse could be found for any order on the pick
				Pick.AddRowError(Pick.OrdersToPick.Count == 1 ?
					Res.GetString("3cb5626b-ad02-412c-b098-3cd6accdaab0", "No stock could be found for the selected order.") :
					Res.GetString("0022236f-8e7f-4b45-ac5e-d2f1c8e26310", "No stock could be found for any of the selected orders."));

				throw new PickFailedException();
			}
		}

		void PostAssembleOrderProcessing()
		{
			PostProcessOrderLines();
			BuildCustomsData();
		}

		void PostProcessOrderLines()
		{
			foreach (WhsPickableDocket order in Pick.OrdersToPick)
			{
				foreach (WhsOrderLine orderLine in order.Lines)
				{
					orderLine.Validation.ValidateAll();
				}
			}
		}

		void BuildCustomsData()
		{
			foreach (WhsPickableDocket order in Pick.OrdersToPick)
			{
				foreach (WhsOrderLine orderLine in order.Lines)
				{
					if (!orderLine.WE_BondedEntryKey.IsEmpty && orderLine.IsWarehouseBondEnabled)
					{
						BuildCustomsDataCore(orderLine);
					}
				}
			}
		}

		[SuppressMessage("Enterprise", "EDI003", Justification = "Will remove after translation has been changed.")]
		void BuildCustomsDataCore(WhsOrderLine orderLine)
		{
			// Tested in WhsBondedTransactionProcessorInbound & WhsBondedTransactionProcessorOutbound
			using (orderLine.CustomsData.SuspendUpdatingDocketLineEntryKey())
			{
				orderLine.CustomsData.WB_EntryKey = WhsBondedWarehouseAttribute.WaitingForCustomsPaymentMessage;
				orderLine.CustomsData.WB_EntryLineNo = (short)0m;
			}

			orderLine.CustomsData.WB_DeclarationReference = WhsCustomsHelper.SafelyRemoveSuffixValue(orderLine.Order.WD_ExternalReference);

			WhsBondedCalculator.SetCustomsData(orderLine);
		}

		void CalcOrderTotals()
		{
			// these are not calced getters because user can override on Release.
			foreach (WhsPickableDocket order in Pick.OrdersToPick)
			{
				ZDecimal unitsSent = 0m;
				ZDecimal weightSent = 0m;
				ZDecimal cubicSent = 0m;

				foreach (WhsOrderLine orderLine in order.Lines)
				{
					unitsSent += orderLine.SumOfUnitsMet;
					weightSent += ZArchitecture.Core.Utilities.Round(orderLine.SupplierPart.UnitConverter.Convert(orderLine.SupplierPart.OP_Weight, orderLine.SupplierPart.OP_WeightUQ, order.WD_TotalWeightUnit) * orderLine.SumOfUnitsMet, 2);
					cubicSent += ZArchitecture.Core.Utilities.Round(orderLine.SupplierPart.UnitConverter.Convert(orderLine.SupplierPart.OP_Cubic, orderLine.SupplierPart.OP_CubicUQ, order.WD_TotalCubicUnit) * orderLine.SumOfUnitsMet, 2);
				}

				order.WD_UnitsSent = unitsSent;
				order.WD_TotalWeight = weightSent;
				order.WD_TotalCubic = cubicSent;
			}
		}

		void SetPickFKOnOrders()
		{
			Pick.ClearOrdersCache();
		}

		#endregion

		#region Implementation

		readonly WhsPick Pick;

		#endregion
	}

	#endregion
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed partial class WhsPick
	{
		partial void OnFinalisingSelectedOrdersForTest(WhsPickableDocket order)
		{
			OrdersBeingFinalisedForTest.Add(order);
		}
		public IList<WhsPickableDocket> OrdersBeingFinalisedForTest = new List<WhsPickableDocket>();

		public void SetAllocatePackageLabelsStrategyForTest(IAllocatePackageLabelsStrategy strategy)
		{
			allocatePackageLabelsStrategy = strategy;
		}

		public MockProgressFormForTest MockProgressForm
		{
			get { return mockProgressForm ?? (mockProgressForm = new MockProgressFormForTest()); }
		}
		MockProgressFormForTest mockProgressForm;
	}
}

#endif
#endregion
