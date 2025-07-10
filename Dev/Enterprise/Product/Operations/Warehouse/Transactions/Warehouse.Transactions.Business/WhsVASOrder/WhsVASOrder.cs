using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transactions.Business
{
	[PreventDelete(true)]
	[CodeProperty(WhsVASOrderSchema.Constants.WVO_JobID)]
	[UniversalDataContext(DataContextType.WarehouseVASOrder)]
	public class WhsVASOrder :
		AutoWhsVASOrder,
		IWhsVASOrder,
		IWorkflowProvider,
		IJobNumber,
		ITemplateCopyable,
		IDocManagerSupport,
		IEDocsProvider,
		IHaveServices,
		IJobInvoicingPlugIn,
		IRatingSupporter,
		ILineToPutawayParent,
		INumberFountainConsumer,
		ICriticalChangesVersionID,
		ICancellable
	{
		#region Constructors

		public WhsVASOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WVO_CancelledTimeUtc), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WVO_GS_NKCancelledBy), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WVO_FinalizedTimeUtc), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WVO_GS_NKFinalizedBy), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WVO_WorkCompletedTimeUtc), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WVO_GS_NKWorkCompletedBy), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WVO_WD_TransferIntoServiceArea), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WVO_WD_TransferOutOfServiceArea), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WVO_CriticalChangesVersionID), ConcurrencyPolicy.Ignore);
		}

		#endregion

		#region Lines

		[ChildEditable(true)]
		public WhsVASOrderLineCollection Lines
		{
			get { return lines ?? (lines = GetLinesCollection()); }
		}

		WhsVASOrderLineCollection GetLinesCollection()
		{
			var collection = new WhsVASOrderLineCollection(this);
			RegisterEditableChildObject(collection);
			return collection;
		}

		WhsVASOrderLineCollection lines;

		#endregion

		#region ServiceArea

		public WhsArea ServiceArea
		{
			get { return Factory.Load<WhsArea>(WVO_WA_ServiceArea); }
		}

		#endregion

		#region TransferIntoServiceArea

		public WhsTransfer TransferIntoServiceArea
		{
			get { return Factory.Load<WhsTransfer>(WVO_WD_TransferIntoServiceArea); }
		}

		#endregion

		#region TransferOutOfServiceArea

		public WhsTransfer TransferOutOfServiceArea
		{
			get { return Factory.Load<WhsTransfer>(WVO_WD_TransferOutOfServiceArea); }
		}

		#endregion

		#region Warehouse

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WarehousePK); }
		}

		#endregion

		#region Properties

		#region Persistent

		#region WVO_FinalizedTimeUtc

		[BusinessObjectTestExclude] // the setter throws an exception when setting at the wrong time
		public override ZDateTime WVO_FinalizedTimeUtc
		{
			get { return base.WVO_FinalizedTimeUtc; }
			set
			{
				if (value.IsValid && !WVO_WorkCompletedTimeUtc.IsValid)
				{
					throw new InvalidOperationException("You cannot finalise the VAS Order without first completing it.");
				}

				if (!value.IsValid && WVO_FinalizedTimeUtc.IsValid)
				{
					throw new InvalidOperationException("Once a VAS Order is finalised, you cannot unfinalise it.");
				}

				base.WVO_FinalizedTimeUtc = value;
				UpdateRelatedUserIfNecessary(
					WVO_FinalizedTimeUtc,
					WVO_GS_NKFinalizedBy,
					user => WVO_GS_NKFinalizedBy = user);
				if (IsFinalised)
				{
					SetReadOnlyIncludingChildren(true);
				}
			}
		}

		void UpdateRelatedUserIfNecessary(
			ZDateTime time,
			ZString currentUser,
			Action<ZString> setUser)
		{
			var user = time.IsValid ? GlbStaff.CurrentUser.GS_Code : ZString.Empty;
			if (currentUser != user)
			{
				setUser(user);
			}
		}

		#endregion

		#region WVO_WorkCompletedTimeUtc

		[BusinessObjectTestExclude] // the setter throws an exception when setting at the wrong time
		public override ZDateTime WVO_WorkCompletedTimeUtc
		{
			get { return base.WVO_WorkCompletedTimeUtc; }
			set
			{
				if (value.IsValid && !WVO_WD_TransferIntoServiceArea.IsValid)
				{
					throw new InvalidOperationException("You cannot set work as completed without a Into Service Area Transfer.");
				}

				if (!value.IsValid && WVO_WorkCompletedTimeUtc.IsValid)
				{
					throw new InvalidOperationException("Once a VAS Order is completed, you cannot mark it as incomplete.");
				}

				base.WVO_WorkCompletedTimeUtc = value;
				UpdateRelatedUserIfNecessary(
					WVO_WorkCompletedTimeUtc,
					WVO_GS_NKWorkCompletedBy,
					user => WVO_GS_NKWorkCompletedBy = user);
			}
		}

		#endregion

		#region WVO_CancelledTimeUtc

		public override ZDateTime WVO_CancelledTimeUtc
		{
			get { return base.WVO_CancelledTimeUtc; }
			set
			{
				base.WVO_CancelledTimeUtc = value;
				UpdateRelatedUserIfNecessary(
					WVO_CancelledTimeUtc,
					WVO_GS_NKCancelledBy,
					user => WVO_GS_NKCancelledBy = user);
			}
		}

		#endregion

		#region WVO_OH_Client

		[List("Lookups.Clients")]
		public override ZGuid WVO_OH_Client
		{
			get { return base.WVO_OH_Client; }
			set
			{
				base.WVO_OH_Client = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWVO_CustomerReferenceNo(); // Tested in WhsVASOrderValidation
				}
				Lines.RefreshBinding();
			}
		}

		#endregion

		#region WVO_WA_ServiceArea

		[RelatedBusinessObject("ServiceArea")]
		[List("Lookups.Areas")]
		public override ZGuid WVO_WA_ServiceArea
		{
			get { return base.WVO_WA_ServiceArea; }
			set
			{
				if (!value.IsValid)
				{
					var previousArea = ServiceArea;
					if (previousArea != null)
					{
						warehousePK = previousArea.WA_WW_Whs;
					}
				}

				base.WVO_WA_ServiceArea = value;

				var area = ServiceArea;
				if (area != null)
				{
					warehousePK = area.WA_WW_Whs;
				}
				Lines.RefreshBinding();
			}
		}

		#endregion

		#region WVO_WD_TransferIntoServiceArea

		[BusinessObjectTestExclude] // the setter throws an exception if you try to set a valid guid twice
		public override ZGuid WVO_WD_TransferIntoServiceArea
		{
			get { return base.WVO_WD_TransferIntoServiceArea; }
			set
			{
				if (WVO_WD_TransferIntoServiceArea.IsValid && value.IsValid)
				{
					throw new InvalidOperationException("Cannot overwrite TransferIntoServiceArea with another one.");
				}

				base.WVO_WD_TransferIntoServiceArea = value;
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WVO_CriticalChangesVersionID), WVO_WD_TransferIntoServiceArea.IsValid ? ConcurrencyPolicy.Strict : ConcurrencyPolicy.Ignore);
				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsVASOrder>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);
			}
		}

		#endregion

		#region WVO_WD_TransferOutOfServiceArea

		[BusinessObjectTestExclude] // the setter throws an exception if you try to set a valid guid twice
		public override ZGuid WVO_WD_TransferOutOfServiceArea
		{
			get { return base.WVO_WD_TransferOutOfServiceArea; }
			set
			{
				if (WVO_WD_TransferOutOfServiceArea.IsValid && value.IsValid)
				{
					throw new InvalidOperationException("Cannot overwrite TransferOutOfServiceArea with another one.");
				}

				if (IsFinalised && value.IsValid)
				{
					throw new InvalidOperationException("Cannot create a TransferOutOfServiceArea if the VAS Order is already finalised.");
				}

				base.WVO_WD_TransferOutOfServiceArea = value;
			}
		}

		#endregion

		#endregion

		#region Non Persistent / Calculated

		#region ProductCode

		[ResourceStringData("WhsVASOrder|ProductCode", Caption = "Product Code", ShortCaption = "Product")]
		public ZString ProductCode => Lines.GetSingleValueOrManyText(l => l.Product?.OP_PartNum ?? ZString.Empty, wvl => wvl.WVL_OP_Product);

		#endregion

		#region ProductDescription

		[ResourceStringData("WhsVASOrder|ProductDescription", Caption = "Product Description", ShortCaption = "Product Desc.")]
		public ZString ProductDescription => Lines.GetSingleValueOrManyText(l => l.Product?.OP_Desc ?? ZString.Empty, wvl => wvl.WVL_OP_Product);

		#endregion

		#region ProductQty

		[ResourceStringData("WhsVASOrder|ProductQty", Caption = "Product Quantity", MediumCaption = "Quantity", ShortCaption = "Qty")]
		public ZDecimal ProductQty
		{
			get { return Lines.Sum(l => l.WVL_Quantity); }
		}

		#endregion

		#region Status

		[ResourceStringData("WhsVASOrder|Status", Caption = "Status")]
		public ZString Status
		{
			get
			{
				var result = ZString.Empty;
				if (!IsInDatabase)
				{
					result = WhsVASOrderStatuses.Descriptions.NewEntry;
				}
				else if (IsFinalised)
				{
					result = WhsVASOrderStatuses.Descriptions.Finalized;
				}
				else if (IsCancelled)
				{
					result = WhsVASOrderStatuses.Descriptions.Cancelled;
				}
				else if (WVO_WD_TransferOutOfServiceArea.IsValid)
				{
					result = WhsVASOrderStatuses.Descriptions.TransferringOut;
				}
				else if (WVO_WorkCompletedTimeUtc.IsValid)
				{
					result = WhsVASOrderStatuses.Descriptions.WorkCompleted;
				}
				else if (TransferIntoServiceArea != null)
				{
					result = TransferIntoServiceArea.IsFinalised ? WhsVASOrderStatuses.Descriptions.Working : WhsVASOrderStatuses.Descriptions.TransferringIn;
				}
				else
				{
					result = WhsVASOrderStatuses.Descriptions.Entered;
				}

				return result.ToUpper();
			}
		}

		#endregion

		#region WarehousePK

		[List("Lookups.Warehouses")]
		[RelatedBusinessObject("Warehouse")]
		[ResourceStringData("WhsVASOrder|WarehousePK", Caption = "Warehouse")]
		public ZGuid WarehousePK
		{
			get
			{
				var area = WVO_WA_ServiceArea.IsEmpty ? null : ServiceArea;
				return area != null ? area.WA_WW_Whs : warehousePK;
			}
			set
			{
				var previousValue = WarehousePK;
				SetNonPersistentPropertyValue(WarehousePKInfo, ref warehousePK, value);

				if (previousValue != value)
				{
					base.WVO_WA_ServiceArea = ZGuid.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateWarehousePK();
				}
				Lines.RefreshBinding();
			}
		}

		public ZPropertyInfo WarehousePKInfo
		{
			get { return GetZPropertyInfo(nameof(WarehousePK)); }
		}

		ZGuid warehousePK;

		#endregion

		#endregion

		#endregion

		// Overrides

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsVASOrderFetchStrategy(this);
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return WVO_JobID.IsEmpty ? HumanReadableNameWithoutID : ZString.Format("{0} {1}", HumanReadableNameWithoutID, WVO_JobID); }
		}

		ZString HumanReadableNameWithoutID
		{
			get { return Res.GetString("1cceaf36-c2bc-46ec-965e-0705f0023b19", "Warehouse VAS Order"); }
		}

		#endregion

		#region IsAutoLogged

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly || IsCancelled || IsTransferStarted; }
			set { base.ReadOnly = value; }
		}

		bool IsTransferStarted
		{
			get { return WVO_WD_TransferIntoServiceArea.IsValid; }
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			Lines.DeleteAll();
			WorkflowItems.RemoveAndDeleteAll();

			base.Delete();
		}

		#endregion

		#region INumberFountainConsumer

		ZString INumberFountainEntityWithID.ID
		{
			get => WVO_JobID;
			set => WVO_JobID = value;
		}

		INumberFountainProxy INumberFountainConsumer.Fountain => Env.NumberFountains.WarehouseVASOrderJobID;

		#endregion

		#region Save

		#region OnFactorySaved

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			ClearJobNoAndCustomerReferenceIfSaveFailed(saveSucceeded);
			RemoveServiceCommencedEventIfSaveFailed(saveSucceeded);
		}

		void ClearJobNoAndCustomerReferenceIfSaveFailed(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				if (WVO_CustomerReferenceNo.EqualsIgnoringCase(WVO_JobID))
				{
					WVO_CustomerReferenceNo = "";
				}

				WVO_JobID = "";
			}
		}

		void RemoveServiceCommencedEventIfSaveFailed(bool saveSucceeded)
		{
			if (!saveSucceeded && !IsInDatabase)
			{
				var serviceCommencedLog = Logs.LogsNotInDB.FirstOrDefault(l => l.SL_SE_NKEvent.EqualsIgnoringCase(Events.ServiceCommencedCode));
				if (serviceCommencedLog != null)
				{
					serviceCommencedLog.Delete();
				}
			}
		}

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this, TemplateApplicationParameters.ApplyIgnoreHasChanges(!IsInDatabase));
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();

			CheckForChangesAndPreventSaveIfNecessary();
			AddServiceCommencedEvent();
			if (WVO_CustomerReferenceNo.IsEmpty)
			{
				WVO_CustomerReferenceNo = WVO_JobID;
			}
		}

		void CheckForChangesAndPreventSaveIfNecessary()
		{
			var specifiedPropertiesChanged = IsInDatabase && (WVO_OH_ClientInfo.HasChanges || WVO_WA_ServiceAreaInfo.HasChanges);
			if (specifiedPropertiesChanged)
			{
				if (WVO_WD_TransferIntoServiceAreaInfo.OriginalValue.IsValid)
				{
					throw new ZCannotSaveException(Res.GetString("WhsVASOrder|PropertiesMustNotBeChanged", "Cannot save as fields are modified after Into Service Area Transfer is created."),
						Res.GetString("WhsVASOrder|VasOrderTransferringHasStarted", "VAS Order Transferring has Started."), ExceptionType.BusinessFailure);
				}

				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsVASOrder>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);
			}
		}

		void AddServiceCommencedEvent()
		{
			if (!IsInDatabase)
			{
				Logs.AddNew(Events.ServiceCommenced);
			}
		}

		#endregion

		#endregion

		#region Unique Index Failure Handler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get { yield return uniqueIndexFailureHandler ?? (uniqueIndexFailureHandler = new WhsVASOrderFountainUniqueIndexFailureHandler(this)); }
		}

		IUniqueIndexFailureHandler uniqueIndexFailureHandler;

		class WhsVASOrderFountainUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public WhsVASOrderFountainUniqueIndexFailureHandler(WhsVASOrder order)
				: base(WhsVASOrderSchema.Constants.Indexes.NR_UC__WVO_JobID, order)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix => Env.NumberFountains.WarehouseVASOrderJobID;
		}

		#endregion

		// Functions

		#region FinaliseVASOrder

		public bool FinaliseVASOrder(INotifications notifications, bool confirmFinalise = true)
		{
			var result = false;

			if (IsFinalised)
			{
				notifications.AddError(Res.GetString("4b1758f9-90d9-4680-b1c5-078b74b162b7", "VAS Order already Finalized."));
			}
			else if (HasChanges || !IsInDatabase)
			{
				notifications.AddError(Res.GetString("a8c84f50-46cd-4973-8500-9cc6b9f512cd", "Save all changes before Finalizing this VAS Order."));
			}
			else if (IsCancelled)
			{
				notifications.AddError(Res.GetString("487015e2-adf4-4e02-9070-f5ef1ac02d7d", "Cannot Finalize Canceled VAS Orders."));
			}
			else if (!WVO_WorkCompletedTimeUtc.IsValid)
			{
				notifications.AddError(Res.GetString("0183aae0-df9d-4f2e-a7f9-2a9e41808bab", "Cannot Finalize VAS Order until the Work has been Completed."));
			}
			else if (TransferOutOfServiceArea != null)
			{
				notifications.AddError(Res.GetString("1415b510-aec0-4c36-ac2d-16a0588f6b2a",
					"There is already a Transfer out of the Service Area for this VAS Order and the Order can no longer be Finalized this way. You must Finalize the Transfer instead."));
			}
			else
			{
				DoFinaliseOrTransferOut(notifications, Res.GetString("b3da6908-5c4e-4044-8689-845fedcc5ea5", "Cannot Finalize VAS Order until all Services have been Completed."), () =>
				{
					var initialTransfer = TransferIntoServiceArea;
					if (initialTransfer == null)
					{
						notifications.AddError(Res.GetString("a1f49b0d-7df4-47af-ab1b-04ece2f5a181", "Before finalizing the VAS Order, a Transfer into the Service Area must first be created."));
					}
					else if (!initialTransfer.IsFinalised)
					{
						notifications.AddError(Res.GetString("2e1e263d-29c3-48ae-91a6-599b5e69b71c", "Before finalizing the VAS Order, the Transfer into the Service Area must first be finalized."));
					}
					else if (!confirmFinalise || ConfirmFinalise(notifications))
					{
						FinaliseVASOrder(initialTransfer);
						result = true;
					}
				});
			}

			return result;
		}

		void DoFinaliseOrTransferOut(INotifications notifications, string errorMessage, Action action)
		{
			using (new SemaphoreManager(FinaliseOrTransferringOutSemaphore))
			{
				Services.Cast<WhsJobService>().ForEach(s => s.Validation.ValidateES_CompletedDateTimeOffset());
				if (Services.Cast<WhsJobService>().Any(s => !s.ES_CompletedDateTimeOffset.IsValid))
				{
					notifications.AddError(errorMessage);
				}
				else
				{
					action();
				}
			}
		}

		bool ConfirmFinalise(INotifications notify)
		{
			var dialogDefaultContext = new DialogDefaultContext(dialogIdentifier: new ZGuid("32ca2fd7-c55a-416b-8d13-a1712580821d"),
												caption: Res.GetString("b0fda45c-1507-4606-bd6b-3a521f3b94e7", "Finalize VAS Order"),
												buttons: ZMessageBoxButtons.YesNo,
												icon: ZMessageBoxIcon.Warning,
												context: null,
												resultsNotToSave: new[] { ZDialogResult.No },
												showCheckboxOnly: true);

			var message = Res.GetString("fa14f54b-8363-44c1-a1c2-a88a3a8086fa",
@"Finalizing the VAS Order will make the Stock Transferred into the Service Area available for Picking.
You will not be allowed to Create a Transfer out of the Service Area from this VAS Order if you Finalize it.
This process cannot be undone.

Are you sure you want to do this?");
			var e = new DefaultableQueryUserEventArgs(dialogDefaultContext, message, false);
			notify.QueryUser(e);
			return e.Response;
		}

		void FinaliseVASOrder(WhsTransfer initialTransfer)
		{
			// Force reload of cached inventories prior to carrying out finalise operation
			TransferIntoServiceArea.Lines.ForEach(l => l.Inventory.Reload(true));

			var query = new ZQuery();
			query.AddToFilter(WhsDocketLineSchema.WE_WD, initialTransfer.PK);

			var allTransferLines = Factory.Load<WhsTransferLine>(query);
			foreach (var transferLine in allTransferLines)
			{
				transferLine.WE_CurrentInventoryStatus = InventoryStatus.Codes.Available;
			}

			WVO_FinalizedTimeUtc = ZDateTime.UtcNow;
			Logs.AddNew(Events.ItemDocumentJobFinalised, "VAS Order");
		}

		#endregion

		#region GetOrCreateInitialTransfer

		public WhsTransfer GetOrCreateInitialTransfer(INotifications notifications)
		{
			WhsTransfer result = null;

			if (HasChanges || !IsInDatabase)
			{
				var message = TransferIntoServiceArea != null
					? Res.GetString("38f86577-70bf-4e21-9ac8-70865b60816c", "Save all changes before viewing the Transfer to Service Area.")
					: Res.GetString("161af878-5616-46e3-a9d1-ace171b96813", "Save all changes before creating the Transfer to Service Area.");

				notifications.AddError(message);
			}
			else if (IsCancelled)
			{
				notifications.AddError(CannotCreateTransferForCancelledVASOrderMessage);
			}
			else if (Lines.Count == 0)
			{
				notifications.AddError(Res.GetString("17a76c5d-b14c-4a9f-b1ce-3d9b69cc4247", "Cannot create Transfer as there are no Service Lines."));
			}
			else if (TransferIntoServiceArea != null)
			{
				result = TransferIntoServiceArea;
			}
			else
			{
				var availableInventoriesResult = CheckAvailableInventoryForVASOrderLines();
				if (availableInventoriesResult.ShortfallExists)
				{
					notifications.AddError(availableInventoriesResult.ShortfallError);
				}
				else
				{
					using (var mutex = new ZGlobalMutex(MutexIDs.WhsVasOrderCheckingLocationCapacity, WVO_WA_ServiceArea.ToString()))
					{
						if (mutex.Lock())
						{
							try
							{
								var serviceAreaLocations = GetValidNotFullServiceAreaLocations();
								if (!serviceAreaLocations.Any())
								{
									notifications.AddError(Res.GetString("753f54dd-81c0-4689-a090-c19de87e512f", "The Service Area must have at least one not full location with a {0} Status.", LocationStatus.Descriptions.Normal));
								}
								else
								{
									result = CreateInitialTransfer(availableInventoriesResult.AvailableInventories, serviceAreaLocations, notifications);
								}
							}
							finally
							{
								mutex.Unlock();
							}
						}
						else
						{
							notifications.AddError(Res.GetString("DD622CD6-3567-43A2-8056-128BA0D5FECA", "Another VAS order is currently creating a transfer in for the same service area. To avoid capacity overflow please try again in short time."));
						}
					}
				}
			}

			return result;
		}

		#region GetValidNotFullServiceAreaLocations

		IEnumerable<LocationWithCapacity> GetValidNotFullServiceAreaLocations()
		{
			var locations = WhsValidationHelper.GetLocationCapacityInfoForNormalLocations(ServiceArea).Where(loc => !loc.Quantity.HasValue || loc.Quantity > 0).ToArray();

			return locations.OrderBy(locationWithCapacity => locationWithCapacity.Location.RowPathSequence)
				.ThenBy(locationWithCapacity => locationWithCapacity.Location.WLV_PutawayPathSequence)
				.ThenBy(locationWithCapacity => locationWithCapacity.Location.RowName)
				.ThenBy(locationWithCapacity => locationWithCapacity.Location.WLV_Column)
				.ThenBy(locationWithCapacity => locationWithCapacity.Location.WLV_Level)
				.ThenBy(locationWithCapacity => locationWithCapacity.Location.WLV_Tray);
		}

		#endregion

		#region CheckAvailableInventoryForVASOrderLines

		AvailableInventoriesResult CheckAvailableInventoryForVASOrderLines()
		{
			var availableInventories = GetAvailableInventories();
			var shortfallBuilder = new ZStringBuilder();
			var shortfallsAlreadyProcessed = new HashSet<string>();

			foreach (DynamicBusinessObject availableInventory in availableInventories)
			{
				var attribKey = GetAttributeKey(availableInventory);
				if (!shortfallsAlreadyProcessed.Contains(attribKey))
				{
					shortfallsAlreadyProcessed.Add(attribKey);
					var availableQty = (ZDecimal)availableInventory["AvailableQuantityGrouped"];
					var orderedQty = (ZDecimal)availableInventory["OrderedQuantity"];
					if (availableQty < orderedQty)
					{
						shortfallBuilder.Append(BuildShortfallMessage(availableInventory, orderedQty - availableQty));
					}
				}
			}

			AvailableInventoriesResult result;

			if (shortfallBuilder.Length > 0)
			{
				shortfallBuilder.Prepend(Res.GetString("30d9c547-7d58-4a63-bbb3-d7f42e5e2b7b", "Cannot create Transfer because of the following Shortfalls:"));
				result = new AvailableInventoriesResult(shortfallBuilder.ToStringWithNewLineBetweenAppends());
			}
			else
			{
				result = new AvailableInventoriesResult(availableInventories.Cast<DynamicBusinessObject>());
			}

			return result;
		}

		#region GetAvailableInventories

		DynamicBusinessObjectCollection GetAvailableInventories()
		{
			string sql = $@"
;with AvailableInventories as (
	select
		WI_OP,
		WI_WL,
		WI_PalletID,
		WI_PartAttrib1,
		WI_PartAttrib2,
		WI_PartAttrib3,
		WI_SerialNumber,
		WI_ExpiryDate,
		WI_PackingDate,
		sum(WI_TotalUnits - isnull(CommittedUnits, 0)) as AvailableQuantity
	from
		dbo.WhsInventoryView
		join
		(
			select
				WVL_OP_Product,
				WVL_PartAttrib1,
				WVL_PartAttrib2,
				WVL_PartAttrib3,
				WVL_SerialNumber,
				WVL_ExpiryDate,
				WVL_PackingDate
			from
				dbo.WhsVASOrderLine
			where
				WVL_WVO_VASOrder = @VASOrderPK
			group by
				WVL_OP_Product,
				WVL_PartAttrib1,
				WVL_PartAttrib2,
				WVL_PartAttrib3,
				WVL_SerialNumber,
				WVL_ExpiryDate,
				WVL_PackingDate
		) as VASOrderLine on WVL_OP_Product = WI_OP
			and WVL_PartAttrib1 = WI_PartAttrib1
			and WVL_PartAttrib2 = WI_PartAttrib2
			and WVL_PartAttrib3 = WI_PartAttrib3
			and WVL_SerialNumber = WI_SerialNumber
			and ((WVL_ExpiryDate is null and WI_ExpiryDate is null) or WVL_ExpiryDate = WI_ExpiryDate)
			and ((WVL_PackingDate is null and WI_PackingDate is null) or WVL_PackingDate = WI_PackingDate)
		join dbo.WhsLocationView on WLV_PK = WI_WL
		cross apply
		(
			select
				sum(WZ_Units) as CommittedUnits
			from
				dbo.WhsPickLine CommittedPickLine
				join dbo.WhsDocketLine CommittedDocketLine on CommittedDocketLine.WE_PK = WZ_WE_TransactionLine
				join dbo.WhsDocket CommittedDocket on CommittedDocket.WD_PK = CommittedDocketLine.WE_WD and CommittedDocket.WD_DocketStatus <> 'CAN'
			where
				CommittedPickLine.WZ_PickedDateTime is null
				and CommittedDocket.WD_IsPutawayTransfer = 0
				and CommittedPickLine.WZ_WE_InventoryLine = WI_WE_InDocketLine
		) Committed
	where
		WLV_LocationStatus = '{LocationStatus.Codes.Normal}'
		and WLV_PickingAreaType = '{AreaTypes.Codes.FreeStore}'
		and WLV_WW_Whs = @WarehousePK
		and WI_InventoryStatus = '{InventoryStatus.Codes.Available}'
		and WI_OH_Client = @ClientPK
		and WI_TotalUnits > 0
	group by
		WI_OP,
		WI_WL,
		WI_PalletID,
		WI_PartAttrib1,
		WI_PartAttrib2,
		WI_PartAttrib3,
		WI_SerialNumber,
		WI_ExpiryDate,
		WI_PackingDate
	),
	VASOrderLines as (
		select
			WVL_OP_Product,
			WVL_PartAttrib1,
			WVL_PartAttrib2,
			WVL_PartAttrib3,
			WVL_SerialNumber,
			WVL_PackingDate,
			WVL_ExpiryDate,
			sum(WVL_Quantity) WVL_Quantity
		from
			dbo.WhsVASOrderLine
			join dbo.OrgPartRelation on OU_OP = WVL_OP_Product and OU_OH = @ClientPK and OU_Relationship in ('{OrgPartRelation.RelationshipTypes.Owner}', '{OrgPartRelation.RelationshipTypes.Both}')
		where
			WVL_WVO_VASOrder = @VASOrderPK
		group by
			WVL_OP_Product,
			WVL_PartAttrib1,
			WVL_PartAttrib2,
			WVL_PartAttrib3,
			WVL_SerialNumber,
			WVL_PackingDate,
			WVL_ExpiryDate
	)

select
	WVL_OP_Product as ProductPK,
	OP_PartNum as ProductCode,
	WI_WL as LocationPK,
	WI_PalletID as PalletID,
	WVL_PartAttrib1 as PartAttrib1,
	WVL_PartAttrib2 as PartAttrib2,
	WVL_PartAttrib3 as PartAttrib3,
	WVL_SerialNumber as SerialNumber,
	WVL_ExpiryDate as ExpiryDate,
	WVL_PackingDate as PackingDate,
	isnull(AvailableQuantity, 0) as AvailableQuantity,
	isnull(TotalPalletUnits, 0) as TotalPalletUnits,
	WVL_Quantity OrderedQuantity,
	sum(isnull(AvailableQuantity, 0)) over (partition by WVL_OP_Product, WVL_PartAttrib1, WVL_PartAttrib2, WVL_PartAttrib3, WVL_SerialNumber, WVL_ExpiryDate, WVL_PackingDate) as AvailableQuantityGrouped
from
	VASOrderLines
	join dbo.OrgSupplierPart on WVL_OP_Product = OP_PK
	left join AvailableInventories on WVL_OP_Product = WI_OP
		and WI_PartAttrib1 = WVL_PartAttrib1
		and WI_PartAttrib2 = WVL_PartAttrib2
		and WI_PartAttrib3 = WVL_PartAttrib3
		and WI_SerialNumber = WVL_SerialNumber
		and ((WVL_ExpiryDate is null and WI_ExpiryDate is null) or WVL_ExpiryDate = WI_ExpiryDate)
		and ((WVL_PackingDate is null and WI_PackingDate is null) or WVL_PackingDate = WI_PackingDate)
	cross apply
	(
		select
			sum(WE_StockOnHand) as TotalPalletUnits
		from
			dbo.WhsDocketLine
			join dbo.WhsDocket on WD_PK = WE_WD
		where
			WE_StockOnHand > 0
			and WD_WW_Whs = @WarehousePk
			and WE_WL is not null
			and WE_PalletID <> ''
			and WI_PalletID = WE_PalletID
	) as TotalPallet
order by
	OP_PartNum,
	AvailableQuantity desc";

			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add("@WarehousePK", WarehousePK, WhsLocationViewSchema.WLV_WW_Whs);
			sqlParams.Add("@ClientPK", WVO_OH_Client, WhsInventoryViewSchema.WI_OH_Client);
			sqlParams.Add("@VASOrderPK", PK, WhsVASOrderLineSchema.WVL_WVO_VASOrder);

			var availableInventories = new DynamicBusinessObjectCollection(Factory);
			availableInventories.Load(sql, sqlParams);

			return availableInventories;
		}

		#endregion

		#region GetAttributeKey

		string GetAttributeKey(DynamicBusinessObject availableInventory)
		{
			const string separator = "|";
			return
					((ZGuid)availableInventory["ProductPK"] + separator +
					(ZString)availableInventory["PartAttrib1"] + separator +
					(ZString)availableInventory["PartAttrib2"] + separator +
					(ZString)availableInventory["PartAttrib3"] + separator +
					(ZString)availableInventory["SerialNumber"] + separator +
					(ZDateTime)availableInventory["ExpiryDate"] + separator +
					(ZDateTime)availableInventory["PackingDate"]).ToUpper(Culture.Invariant);
		}

		#endregion

		#region BuildShortfallMessage

		string BuildShortfallMessage(DynamicBusinessObject availableInventory, ZDecimal shortfallQuantity)
		{
			var attributeBuilder = new ZStringBuilder();
			AppendPartAttributeMessage(attributeBuilder, availableInventory, 1);
			AppendPartAttributeMessage(attributeBuilder, availableInventory, 2);
			AppendPartAttributeMessage(attributeBuilder, availableInventory, 3);

			var serialNumber = (ZString)availableInventory["SerialNumber"];
			if (!serialNumber.IsEmpty)
			{
				attributeBuilder.Append(Res.GetString("49776865-17bb-4e75-8caf-788f28ddafad", "Serial Number - {0}", serialNumber));
			}

			var expiryDate = (ZDateTime)availableInventory["ExpiryDate"];
			if (!expiryDate.IsEmpty)
			{
				attributeBuilder.Append(Res.GetString("0bdf4452-6c8f-405c-b502-2f7b2d8b7a9b", "Expiry Date - {0}", expiryDate.ToShortDateString()));
			}

			var packingDate = (ZDateTime)availableInventory["PackingDate"];
			if (!packingDate.IsEmpty)
			{
				attributeBuilder.Append(Res.GetString("4c472d64-3c89-4838-b304-73a7b447b4f8", "Packing Date - {0}", packingDate.ToShortDateString()));
			}

			var attributeInfo = attributeBuilder.Length > 0 ? string.Format(Culture.Current, " {{ {0} }}", attributeBuilder.ToStringWithDelimiterBetweenAppends(", ")) : "";

			return string.Format(Culture.Current, (NoResString)"{0}x {1}{2}", shortfallQuantity.ToStringTrimZeros(), availableInventory[nameof(ProductCode)], attributeInfo); // false positive
		}

		void AppendPartAttributeMessage(ZStringBuilder attributeBuilder, DynamicBusinessObject shortfall, int attribNo)
		{
			var partAttrib1 = (ZString)shortfall["PartAttrib" + attribNo];
			if (!partAttrib1.IsEmpty)
			{
				attributeBuilder.Append(string.Format(Culture.Current, "{0} - {1}", Client.PartAttributeManager.PartAttributeName(attribNo), partAttrib1));
			}
		}

		#endregion

		#region class AvailableInventoriesResult

		class AvailableInventoriesResult
		{
			public AvailableInventoriesResult(IEnumerable<DynamicBusinessObject> availableInventories)
			{
				this.availableInventories = Argument.NotNull(availableInventories, "availableInventories");
			}

			public AvailableInventoriesResult(ZString shortfallError)
				: this(Enumerable.Empty<DynamicBusinessObject>())
			{
				ShortfallError = Argument.NotNullOrEmpty(shortfallError, "shortfallError");
			}

			readonly IEnumerable<DynamicBusinessObject> availableInventories;
			public readonly ZString ShortfallError;

			public IEnumerable<DynamicBusinessObject> AvailableInventories
			{
				get { return availableInventories; }
			}

			public bool ShortfallExists
			{
				get { return !ShortfallError.IsEmpty; }
			}
		}

		#endregion

		#endregion

		#region CreateInitialTransfer

		WhsTransfer CreateInitialTransfer(IEnumerable<DynamicBusinessObject> availableInventories, IEnumerable<LocationWithCapacity> serviceAreaLocations, INotifications notifications)
		{
			WhsTransfer result = null;

			var initialTransfer = CreateInitialTransferWithoutDestLocation(availableInventories);
			using (SuspendSettingHasChanges())
			{
				WVO_WD_TransferIntoServiceArea = initialTransfer.PK;
				ClearCachedTransferLineInventoryStatusLookup(initialTransfer.PK);
			}

			ZString errorMessage = "";
			bool foundLocationWithEnoughCapacity = SetDestinationLocationWithEnoughCapacity(initialTransfer, serviceAreaLocations);
			if (!foundLocationWithEnoughCapacity)
			{
				errorMessage = Res.GetString("76f9c801-0d9e-47b6-ae3e-21ad59401688", "Cannot find location in Service Area {0} with enough capacity for the transfer.", ServiceArea.WA_NameMultilingual);
			}
			else
			{
				errorMessage = ValidateTransferAndReturnError(Res.GetString("817f003b-a2cb-41eb-8a1a-987379fe5bde", "Into"), initialTransfer);
			}

			if (errorMessage.IsEmpty)
			{
				result = initialTransfer;

				HasChanges = true;
				ReadOnly = true;
				Lines.SetReadOnlyIncludingChildren(true);
			}
			else
			{
				using (SuspendSettingHasChanges())
				{
					WVO_WD_TransferIntoServiceArea = ZGuid.Empty;
				}

				initialTransfer.Delete();
				notifications.AddError(errorMessage);
			}

			return result;
		}

		#region CreateInitialTransferWithoutDestLocation

		WhsTransfer CreateInitialTransferWithoutDestLocation(IEnumerable<DynamicBusinessObject> availableInventories)
		{
			var result = CreateTransfer();
			var fulfilledOrderLines = new Dictionary<string, ZDecimal>();
			var fullPalletTransfers = new Dictionary<ZString, List<WhsTransferLine>>();

			foreach (var availableInventory in availableInventories)
			{
				var availableQty = (ZDecimal)availableInventory["AvailableQuantity"];
				var orderedQty = (ZDecimal)availableInventory["OrderedQuantity"];
				ZDecimal amountToCommitToTransferLine = 0m;

				ZDecimal currentAmount;
				var attribKey = GetAttributeKey(availableInventory);
				if (!fulfilledOrderLines.TryGetValue(attribKey, out currentAmount))
				{
					amountToCommitToTransferLine = Math.Min(availableQty, orderedQty);
					fulfilledOrderLines[attribKey] = amountToCommitToTransferLine;
				}
				else if (currentAmount < orderedQty)
				{
					fulfilledOrderLines[attribKey] = Math.Min(availableQty + currentAmount, orderedQty);
					amountToCommitToTransferLine = fulfilledOrderLines[attribKey] - currentAmount;
				}

				if (amountToCommitToTransferLine > 0m)
				{
					var transferLine = CreateTransferLineWithoutDestLocation(result, availableInventory, amountToCommitToTransferLine);

					SetDestinationPalletIDIfFullPalletTransfer(fullPalletTransfers, transferLine, (ZDecimal)availableInventory["TotalPalletUnits"]);
				}
			}

			return result;
		}

		WhsTransfer CreateTransfer()
		{
			var result = Factory.New<WhsTransfer>();
			result.WD_OH_Client = WVO_OH_Client;
			result.WD_WW_Whs = WarehousePK;
			result.WD_ExternalReference = WVO_JobID;

			return result;
		}

		WhsTransferLine CreateTransferLineWithoutDestLocation(WhsTransfer transfer, DynamicBusinessObject availableInventory, ZDecimal amountToCommitToTransferLine)
		{
			var transferLine = transfer.Lines.AddNew();
			transferLine.WE_OP = (ZGuid)availableInventory["ProductPK"];
			transferLine.WE_ExpiryDate = ((ZDateTime)availableInventory["ExpiryDate"]).Date;
			transferLine.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			transferLine.WE_PackingDate = ((ZDateTime)availableInventory["PackingDate"]).Date;
			transferLine.WE_PartAttrib1 = (ZString)availableInventory["PartAttrib1"];
			transferLine.WE_PartAttrib2 = (ZString)availableInventory["PartAttrib2"];
			transferLine.WE_PartAttrib3 = (ZString)availableInventory["PartAttrib3"];
			transferLine.WE_SerialNumber = (ZString)availableInventory["SerialNumber"];
			transferLine.WE_TransactionQuantity = amountToCommitToTransferLine;
			transferLine.WE_TransferFromPalletId = (ZString)availableInventory["PalletID"];
			transferLine.WE_WL_TransferFrom = (ZGuid)availableInventory["LocationPK"];

			return transferLine;
		}

		static void SetDestinationPalletIDIfFullPalletTransfer(Dictionary<ZString, List<WhsTransferLine>> fullPalletTransfers, WhsTransferLine transferLine, ZDecimal totalPalletUnits)
		{
			var transferringPalletID = transferLine.WE_TransferFromPalletId;
			if (!transferringPalletID.IsEmpty)
			{
				var key = transferringPalletID.ToUpper();

				List<WhsTransferLine> palletTransferLines;
				if (!fullPalletTransfers.TryGetValue(key, out palletTransferLines))
				{
					fullPalletTransfers[key] = palletTransferLines = new List<WhsTransferLine>();
				}

				palletTransferLines.Add(transferLine);

				// check to see if the sum of the transfer lines for a particular pallet id equals the number of items on the pallet (therefore full pallet transfer)
				var currentAmountToTransfer = palletTransferLines.Sum(t => t.WE_TransactionQuantity);
				if (currentAmountToTransfer == totalPalletUnits)
				{
					foreach (var palletTransferLine in palletTransferLines)
					{
						palletTransferLine.WE_PalletID = transferringPalletID;
					}
				}
				else if (currentAmountToTransfer > totalPalletUnits)
				{
					throw new InvalidOperationException("Cannot Transfer more Units than exists on Pallet " + transferringPalletID);
				}
			}
		}

		#endregion

		#region SetDestinationLocationWithEnoughCapacity

		static bool SetDestinationLocationWithEnoughCapacity(WhsTransfer initialTransfer, IEnumerable<LocationWithCapacity> serviceAreaLocations)
		{
			var transferLinesByPalletId = initialTransfer.Lines
				.Cast<WhsTransferLine>()
				.GroupBy(GetGroupIDKey)
				.OrderByDescending(g => g.Sum(l => l.QtyToMoveIncludingMatchingLines))
				.ToDictionary(g => g.Key);

			var transferLinesByDestLocation = new Dictionary<WhsLocation, List<WhsTransferLine>>();

			foreach (var transferLinesGroup in transferLinesByPalletId)
			{
				bool locationFound = false;

				foreach (var locationInfo in serviceAreaLocations)
				{
					var transferLinesForThisLocation = GetTransferLinesForThisLocation(transferLinesByDestLocation, transferLinesGroup.Value, locationInfo.Location);

					bool doesLocationHaveEnoughCapacity = TransferValidationHelper.GetMessageForLocationCapacity(transferLinesForThisLocation, locationInfo, processLinesWithEmptyDestLocation: true).IsNone;
					if (doesLocationHaveEnoughCapacity)
					{
						SetDestinationLocation(transferLinesByDestLocation, transferLinesGroup.Value, locationInfo.Location);
						locationFound = true;
						break;
					}
				}

				if (!locationFound)
				{
					return false;
				}
			}

			return true;
		}

		static ZString GetGroupIDKey(WhsTransferLine transferLine)
		{
			ZString result;

			if (transferLine.WE_PalletID.IsEmpty)
			{
				result = transferLine.PK.ToString();
			}
			else
			{
				result = transferLine.WE_PalletID;
			}

			return result;
		}

		static IEnumerable<WhsTransferLine> GetTransferLinesForThisLocation(Dictionary<WhsLocation, List<WhsTransferLine>> transferLinesByDestLocation, IEnumerable<WhsTransferLine> transferLinesToFit, WhsLocation location)
		{
			IEnumerable<WhsTransferLine> transferLinesForThisLocation;

			List<WhsTransferLine> linesForDestination;
			if (transferLinesByDestLocation.TryGetValue(location, out linesForDestination))
			{
				transferLinesForThisLocation = linesForDestination.Concat(transferLinesToFit);
			}
			else
			{
				transferLinesForThisLocation = transferLinesToFit;
			}

			return transferLinesForThisLocation;
		}

		static void SetDestinationLocation(Dictionary<WhsLocation, List<WhsTransferLine>> transferLinesByDestLocation, IEnumerable<WhsTransferLine> transferLinesToFit, WhsLocation location)
		{
			foreach (var transferLine in transferLinesToFit)
			{
				using (transferLine.GetValidationSuspender())
				{
					transferLine.WE_WL = location.PK;
				}
			}

			List<WhsTransferLine> linesForDestination;
			if (!transferLinesByDestLocation.TryGetValue(location, out linesForDestination))
			{
				transferLinesByDestLocation[location] = new List<WhsTransferLine>(transferLinesToFit);
			}
			else
			{
				linesForDestination.AddRange(transferLinesToFit);
			}
		}

		#endregion

		#region ValidateTransferAndReturnError

		ZString ValidateTransferAndReturnError(ZString transferPrefix, WhsTransfer transferToValidate)
		{
			transferToValidate.RunPreSaveValidation();

			#region Test
#if DEBUG
			if (AddErrorOnTransferForTesting)
			{
				transferToValidate.AddRowError("Test Error");
			}
#endif
			#endregion

			return transferToValidate.HasErrors
				? Res.GetString("59686121-fa27-4f71-b17a-fac1a9d3d680", "{0} Service Area Transfer could not be created:\r\n{1}", transferPrefix, transferToValidate.GetErrors().ToUniqueMessageListString())
				: "";
		}

		#region Test
#if DEBUG
		internal IDisposable CreateErrorOnTransferAfterValidationForTest()
		{
			return new DisposableAction(
				() => AddErrorOnTransferForTesting = true,
				() => AddErrorOnTransferForTesting = false);
		}

		bool AddErrorOnTransferForTesting;
#endif
		#endregion

		#endregion

		#endregion

		#region CannotCreateTransferForCancelledVASOrderMessage

		string CannotCreateTransferForCancelledVASOrderMessage
		{
			get { return Res.GetString("77596c68-5457-4c35-83b8-0dfffa8db6dc", "Cannot create Transfer for Canceled VAS Orders."); }
		}

		#endregion

		#region ClearCachedTransferLookup

		void ClearCachedTransferLineInventoryStatusLookup(ZGuid transferPK)
		{
			Factory.ClearCachedValue<CodeDescriptionPairList>($"WhsTransferLineLookups|InventoryStatuses|{transferPK}");
		}

		#endregion

		#endregion

		#region GetOrCreateReturnTransfer

		public WhsTransfer GetOrCreateReturnTransfer(INotifications notifications)
		{
			WhsTransfer result = null;

			if (HasChanges || !IsInDatabase)
			{
				var message = TransferOutOfServiceArea != null
					? Res.GetString("4fd28010-8880-4d07-b1bd-533a20876bfe", "Save all changes before viewing the Transfer out of the Service Area.")
					: Res.GetString("1e6183e5-a896-4afa-b2c1-99341877d5af", "Save all changes before creating the Transfer out of the Service Area.");

				notifications.AddError(message);
			}
			else if (TransferOutOfServiceArea != null)
			{
				result = TransferOutOfServiceArea;
			}
			else if (IsCancelled)
			{
				notifications.AddError(CannotCreateTransferForCancelledVASOrderMessage);
			}
			else if (!WVO_WorkCompletedTimeUtc.IsValid)
			{
				notifications.AddError(Res.GetString("01e5d059-92c7-4cd3-8ae0-2ef8bf5ce9de", "Cannot create Transfer out of Service Area as the VAS Order has not yet been Completed."));
			}
			else if (IsFinalised)
			{
				notifications.AddError(Res.GetString("aba6e16a-7617-40d9-be25-1dc073638760", "Cannot create Transfer out of Service Area as the VAS Order has been Finalized."));
			}
			else
			{
				DoFinaliseOrTransferOut(notifications, Res.GetString("9bc3871f-e1ef-4c0f-91f4-4e25f91154bc", "Cannot create Transfer out of Service Area until all Services have been Completed."), () =>
				{
					var initialTransfer = TransferIntoServiceArea;
					if (initialTransfer == null)
					{
						notifications.AddError(Res.GetString("7268161d-30f6-499f-a6bb-394772040157", "Before creating the out of Service Area Transfer, a Transfer into the Service Area must first be created."));
					}
					else if (!initialTransfer.IsFinalised)
					{
						notifications.AddError(Res.GetString("40e493d9-37ba-4fcf-bf46-afabedc64501", "Before creating the out of Service Area Transfer, the Transfer into the Service Area must first be finalized."));
					}
					else
					{
						result = CreateReturnTransfer(initialTransfer, notifications);
					}
				});
			}

			return result;
		}

		WhsTransfer CreateReturnTransfer(WhsTransfer initialTransfer, INotifications notifications)
		{
			var putawayNotifications = new NotificationBuffer();
			var returnTransfer = CreateReturnTransferCore(initialTransfer, putawayNotifications);
			using (SuspendSettingHasChanges())
			{
				WVO_WD_TransferOutOfServiceArea = returnTransfer.PK;
				ClearCachedTransferLineInventoryStatusLookup(returnTransfer.PK);
			}

			var errorMessage = ValidateTransferAndReturnError(Res.GetString("90c4e055-c1bd-4357-9afc-d98f0f5622d2", "Out of"), returnTransfer);
			if (errorMessage.IsEmpty)
			{
				ShowPutawayNotifications(notifications, putawayNotifications); // show putaway errors if any; should be done after return transfer is created and successfully validated.
				HasChanges = true;
			}
			else
			{
				using (SuspendSettingHasChanges())
				{
					WVO_WD_TransferOutOfServiceArea = ZGuid.Empty;
				}

				returnTransfer.Delete();
				returnTransfer = null;

				notifications.AddError(errorMessage);
			}

			return returnTransfer;
		}

		#region CreateReturnTransferCore

		WhsTransfer CreateReturnTransferCore(WhsTransfer initialTransfer, INotifications putawayNotifications)
		{
			var returnTransfer = CreateTransfer();

			foreach (WhsTransferLine transferLine in initialTransfer.Lines)
			{
				var returnTransferLine = returnTransfer.Lines.AddNew();
				CopyTransferInLineToTransferOutLine(transferLine, returnTransferLine);

				foreach (WhsTransferLine matchingLine in transferLine.MatchingLines)
				{
					var returnMatchingLine = returnTransferLine.MatchingLines.AddNew();
					returnMatchingLine.WE_WD = returnTransfer.PK;

					CopyTransferInLineToTransferOutLine(matchingLine, returnMatchingLine);
				}
			}

			var linesToPutaway = VASReturnTransferLine.GetLinesToPutawayFromTransfer(returnTransfer);

			var putawayManager = ObjectFactory.Get<IPutawayEngineManagerForVASTransferLine>();
			putawayManager.Putaway(new[] { this }, linesToPutaway, putawayNotifications, null);

			return returnTransfer;
		}

		static void CopyTransferInLineToTransferOutLine(WhsTransferLine transferInLine, WhsTransferLine transferOutLine)
		{
			transferOutLine.WE_OP = transferInLine.WE_OP;
			transferOutLine.WE_ExpiryDate = transferInLine.WE_ExpiryDate;
			transferOutLine.WE_PackingDate = transferInLine.WE_PackingDate;
			transferOutLine.WE_PartAttrib1 = transferInLine.WE_PartAttrib1;
			transferOutLine.WE_PartAttrib2 = transferInLine.WE_PartAttrib2;
			transferOutLine.WE_PartAttrib3 = transferInLine.WE_PartAttrib3;
			transferOutLine.WE_SerialNumber = transferInLine.WE_SerialNumber;
			transferOutLine.WE_TransferFromPalletId = transferInLine.WE_PalletID;
			transferOutLine.WE_PalletID = transferOutLine.WE_TransferFromPalletId;
			transferOutLine.WE_WL_TransferFrom = transferInLine.WE_WL;
			transferOutLine.WE_TransactionQuantity = transferInLine.WE_TransactionQuantity;

			CommitTransferInLineInventoryToTransferOutLine(transferInLine, transferOutLine);
		}

		/// <summary>
		/// Commit the exact same inventory that was previously transferred *into* the Service Area.
		/// </summary>
		static void CommitTransferInLineInventoryToTransferOutLine(WhsTransferLine transferInLine, WhsTransferLine transferOutLine)
		{
			var pickLine = transferOutLine.PickLines.AddNew();
			pickLine.WZ_Units = transferInLine.WE_StockOnHand;
			pickLine.WZ_WE_InventoryLine = transferInLine.PK;
		}

		#endregion

		#region ShowPutawayNotifications

		static void ShowPutawayNotifications(INotifications notifications, NotificationBuffer putawayNotifications)
		{
			// show putaway errors if any
			if (putawayNotifications.Events != null)
			{
				foreach (var notification in putawayNotifications.Events)
				{
					notifications.Add(notification);
				}
			}
		}

		#endregion

		#endregion

		#region MarkVASOrderAsCompleted

		public bool MarkVASOrderAsCompleted(INotifications notifications)
		{
			bool result = false;

			if (WVO_WorkCompletedTimeUtc.IsValid)
			{
				notifications.AddError(Res.GetString("e13f8f39-d878-425f-8676-0fbaf75fdfd7", "VAS Order already Complete."));
			}
			else if (HasChanges || !IsInDatabase)
			{
				notifications.AddError(Res.GetString("fdf35bb5-2f40-48a7-a412-e6bc9794bccd", "Save all changes before Completing this VAS Order."));
			}
			else if (IsCancelled)
			{
				notifications.AddError(Res.GetString("d9f2a2e8-8735-4789-a0e5-0a15cbd172f9", "Cannot Complete Canceled VAS Orders."));
			}
			else
			{
				var initialTransfer = TransferIntoServiceArea;
				if (initialTransfer == null)
				{
					notifications.AddError(Res.GetString("67bb8ee8-3ed0-4704-b8bf-230110f9c435", "Before completing the VAS Order, a Transfer into the Service Area must first be created."));
				}
				else if (!initialTransfer.IsFinalised)
				{
					notifications.AddError(Res.GetString("602eef2e-bb4d-4eb4-93f7-2392bbadb6c9", "Before completing the VAS Order, the Transfer into the Service Area must first be finalized."));
				}
				else
				{
					MarkVASOrderAsCompleted();
					result = true;
				}
			}

			return result;
		}

		void MarkVASOrderAsCompleted()
		{
			WVO_WorkCompletedTimeUtc = ZDateTime.UtcNow;
			Logs.AddNew(Events.ServiceCompleted);
		}

		#endregion

		// Interface Implementation

		#region IWorkflowProvider Members

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.WhsVASOrderWorkflowDescriptorCode; }
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

		[ChildEditable]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WhsVASOrderProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var selectionCriteria = new ColumnValueRanker();
			selectionCriteria.Add(ProcessTaskTemplateSchema.P0_OH_Client, WVO_OH_Client, ZGuid.Empty);
			selectionCriteria.Add(ProcessTaskTemplateSchema.P0_WW, WarehousePK, ZGuid.Empty);
			return selectionCriteria;
		}

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

		ProcessTaskCollection workflowItems;

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return WVO_JobID; }
		}

		#endregion

		#region ICancellable Members

		public string CanCancel()
		{
			string reason = null;

			var transferIntoServiceArea = TransferIntoServiceArea;
			if (transferIntoServiceArea != null && !transferIntoServiceArea.CanDelete)
			{
				reason = Res.GetString("WhsVASOrder|CanCancel", "Transfer / Work has already started. {0} cannot be canceled.", HumanReadableName);
			}

			return reason;
		}

		public bool IsCancelled
		{
			get { return WVO_CancelledTimeUtc.IsValid; }
			set
			{
				if (value)
				{
					WVO_CancelledTimeUtc = ZDateTime.UtcNow;
					TransferIntoServiceArea?.Delete();
				}
				else
				{
					WVO_CancelledTimeUtc = ZDateTime.Empty;
				}
			}
		}

		public bool IsCancelledHasChanged => WVO_CancelledTimeUtcInfo.HasChanges;

		public string CanReactivate() => null;

		#endregion

		#region IHaveServices

		public ZString TableCode => WhsVASOrderSchema.Constants.Prefix;

		public ZString TransportMode => ZString.Empty;

		public ZString ContainerMode => ZString.Empty;

		public IHaveServices[] DependentServiceParents => Array.Empty<IHaveServices>();

		public BusinessObject ServiceParent => this;

		public bool NeedsServiceEvents => false;

		[ChildEditable]
		public JobServiceDependentCollection Services
		{
			get
			{
				if (services == null)
				{
					services = new WhsJobServiceDependentCollection(this, Factory);
					services.Load();
					RegisterEditableChildObject(services);
					services.SetReadOnlyIncludingChildren(IsFinalised);
				}
				return services;
			}
		}
		WhsJobServiceDependentCollection services;

		void IHaveServices.JobServiceDeleted(ZGuid servicePK) { }

		IBranch IHaveServices.ServiceBranch => Warehouse?.RelatedCompanyBranch;

		#endregion

		#region IsFinalisingOrTransferringOut

		public bool IsFinalisingOrTransferringOut => FinaliseOrTransferringOutSemaphore.IsSuspended;

		Semaphore FinaliseOrTransferringOutSemaphore => finaliseOrTransferringOutSemaphore ??= new Semaphore();
		Semaphore finaliseOrTransferringOutSemaphore;

		#endregion

		#region IsFinalised

		public bool IsFinalised => WVO_FinalizedTimeUtc.IsValid;

		#endregion

		#region ITemplateCopyable

		public IBusiness TemplateCopy()
		{
			var columnsToExclude = new List<string>()
			{
				WhsVASOrderSchema.WVO_WD_TransferOutOfServiceArea.Name,
				WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea.Name,
				WhsVASOrderSchema.WVO_CustomerReferenceNo.Name,
				WhsVASOrderSchema.WVO_WorkCompletedTimeUtc.Name,
				WhsVASOrderSchema.WVO_GS_NKWorkCompletedBy.Name,
				WhsVASOrderSchema.WVO_FinalizedTimeUtc.Name,
				WhsVASOrderSchema.WVO_GS_NKFinalizedBy.Name,
				WhsVASOrderSchema.WVO_CancelledTimeUtc.Name,
				WhsVASOrderSchema.WVO_GS_NKCancelledBy.Name,
				WhsVASOrderSchema.WVO_JobID.Name,
				"STATUS"
			};
			var args = new BusinessObjectCloneArgs(columnsToExclude, true);
			var copy = (WhsVASOrder)this.Clone(args);

			TemplateCopyWhsVASOrderLines(copy);

			return copy;
		}

		void TemplateCopyWhsVASOrderLines(WhsVASOrder clone)
		{
			foreach (WhsVASOrderLine line in this.Lines.ToArray())
			{
				var lineClone = (WhsVASOrderLine)line.Clone();
				clone.Lines.Add(lineClone);
			}
		}

		#endregion

		#region SupportsCloneCore

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region IEDocsProvider	

		public DocumentSupporter DocumentSupporter => documentSupporter ?? (documentSupporter = new WhsVASOrderDocumentSupporter(this));
		DocumentSupporter documentSupporter;

		DocManagerInfo IDocManagerSupport.DocManagerInfo => docManagerInfo ?? (docManagerInfo = new WhsVASOrderDocManagerInfo(this));
		DocManagerInfo docManagerInfo;

		public EDocsProviderSupporter GetEDocsProviderSupporter() => new JobInvoicingEDocsProviderSupporter(this);

		#endregion

		#region JobHeader

		public JobHeader Job => job ?? (job = new JobHeader.Loader(this).Load());
		JobHeader job;

		#endregion

		#region NoteTypesCore

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var notes = base.NoteTypesCore;
				notes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);

				return notes;
			}
		}

		#endregion

		#region IJobInvoicingPlugIn

		IJobInvoicingSupporter IJobInvoicingPlugIn.InvoicingSupporter => invoicingSupporter ?? (invoicingSupporter = new WhsVASOrderInvoicingSupporter(this));
		IJobInvoicingSupporter invoicingSupporter;

		#endregion

		#region IJobHeaderParent

		bool IJobHeaderParent.AllowInvoiceDeletion => true;

		void IJobHeaderParent.SetJobNumberFieldOnSaving() { }

		void IJobHeaderParent.OnJobCreating(JobHeader jobHeader) { }

		void IJobHeaderParent.OnJobCreated(JobHeader jobHeader) { }

		void IJobHeaderParent.OnJobDeleting(JobHeader jobHeader) { }

		void IJobHeaderParent.OnJobDeleted(JobHeader jobHeader) { }

		#endregion

		#region IRatingSupporter Member

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider => new WhsVASOrderRatingAdaptersProvider(this);

		#endregion

		#region ICriticalChangesVersionID

		ZGuid ICriticalChangesVersionID.CriticalChangesVersionID
		{
			set => WVO_CriticalChangesVersionID = value;
			get => WVO_CriticalChangesVersionID;
		}

		bool ICriticalChangesVersionID.IsImmutableStatus => WVO_WD_TransferIntoServiceArea.IsValid;

		#endregion

		#region ILineToPutawayParent

		ZGuid ILineToPutawayParent.ClientPK => WVO_OH_Client;

		#endregion
	}

	#region Document Support

	public class WhsVASOrderDocumentSupporter : DocumentSupporter
	{
		public WhsVASOrderDocumentSupporter(WhsVASOrder vasOrder)
			: base(vasOrder)
		{
		}

		protected WhsVASOrder VASOrder => (WhsVASOrder)BusinessObject;

		public override BusinessContext BusinessContext => BusinessContext.WhsVASOrder;

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsVASOrderCustomizeDocuments;

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun) => DocumentWrapperFactory.GenerateGenericWrappers(dataContext, VASOrder);

		protected override Constants.DataContext[] GetSupportedDataContexts() => new Constants.DataContext[] { Constants.DataContext.WhsVASOrder };

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction) => new OrgHeaderContact(VASOrder.Client, null);

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun) => false;
	}

	#endregion
}
