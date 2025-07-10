using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Numerics;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.AsnMapping;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.DialogDefault;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;

namespace Enterprise.Warehouse.Transactions.Business
{
	[GlowDataDefinition(nameof(IWhsReceive))]
	[UniversalDataContext(DataContextType.WarehouseReceive)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.WhsReceive)]
	public class WhsReceive : WhsDocket,
		IWhsReceive,
		IWhsReceiveWrapperStrategy,
		IDocumentSupportable,
		IEDocsProvider,
		ITemplateCopyable,
		IRelatableActivity,
		IRelatedJob,
		IDtbBookingParent,
		IRatingSupporter,
		IScreeningPartyProvider,
		IJobWithTransportCompany,
		IShouldUpdateScreeningStatus,
		ILineToPutawayParent,
		IDeniedPartyProvider,
		ICustomizableNumberFountainConsumer,
		ICriticalChangesVersionID,
		IValidateParentWithLines,
		IConversationProvider,
		IConversationAdditionalParticipantProvider,
		IConversationParentHyperlinkProvider,
		IAllowAttachEmailsToEDocs,
		ITaskPlanningJob
	{
		#region Schema

		public new abstract class Schema : WhsDocket.Schema
		{
			public const string TotalPalletsReceived = "TotalPalletsReceived";
			public const string VehicleNo = "VehicleNo";
		}

		#endregion

		#region Constructors

		public WhsReceive(BusinessObjectFactory factory, DataRow row)
				 : base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WD_FinalisedDate), ConcurrencyPolicy.Strict);
		}

		#endregion

		#region Strategies

		protected IWhsReceiveWrapperStrategy wrapperStrategy;
		public IWhsReceiveWrapperStrategy WrapperStrategy
		{
			get
			{
				if (wrapperStrategy == null)
				{
					var builder = ObjectFactory.Get<IWhsReceiveStrategyBuilder>();
					wrapperStrategy = builder.Build(this);
				}

				return wrapperStrategy;
			}
		}

		#endregion

		#region Events

		public event EventHandler<WhsReceiveToPrintEventArgs> OnPrint;
		public event EventHandler ProductChanged;
		public event EventHandler<WhsDocumentInventoryEventArgs> OnInventoryPrint;

		public void CallOnPrint(object sender, WhsReceiveToPrintEventArgs e)
		{
			OnPrint?.Invoke(sender, e);
		}

		public void CallOnProductChanged(object sender, EventArgs args)
		{
			ProductChanged?.Invoke(sender, args);
		}

		public void CallOnInventoryPrint(object sender, WhsDocumentInventoryEventArgs e)
		{
			OnInventoryPrint?.Invoke(sender, e);
		}

		protected override void OnClientChanged()
		{
			base.OnClientChanged();
			Inventory.RefreshBinding();
		}

		protected override void OnWarehouseChanged()
		{
			base.OnWarehouseChanged();
			Inventory.RefreshBinding();
		}

		#endregion

		#region Customs Stuff

		public override bool IsCustomsTransaction
		{
			get { return WD_DocketSubType == ReceiveType.Codes.Customs; }
		}

		protected override ZString DefaultDocketSubTypeForCustomsTransactionCore
		{
			get { return ReceiveType.Codes.Customs; }
		}

		#endregion

		#region CustomizableNumber

		ICustomizableNumberFormatter ICustomizableNumberFountainConsumer.NumberFormatter => GetGenerator();

		NumberGenerator GetGenerator()
		{
			var generator = new NumberGenerator
			{
				Factory = this.Factory,
				Context = new NumberGeneratorContext(),
				BaseFountain = Env.NumberFountains.WarehouseDocketID,
				FountainGetter = Env.NumberFountains.GetDocketIDGeneratorFountain,
				PrimaryTarget = new ReceiveIDGeneratorTarget(this),
			};
			generator.ValueProviders.AddRange(new StandardValueSource().Concat(new WarehouseJobValueSource(this)).Concat(new WarehouseReceiveValueSource(this)));

			return generator;
		}

		#endregion

		#region Business Object Overrides

		protected override WhsDocketDocAddressValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new WhsReceiveDocAddressValidation(addressToValidate);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			WD_DocketType = DocketType.Codes.Receive;
			WD_DocketSubType = ReceiveType.Codes.Receipt;
		}

		#region Delete

		public override void Delete()
		{
			ListManager.UnhookListChangedEvent(Inventory);
			Inventory.CountChanged -= InventoryCountChanged;
			Inventory.RemoveAndDeleteAll();
			ViewRelatedActivityPivot.DeleteAllPivots(this);
			UnlinkChildSplits();
			((IBusinessObjectState)Lines).HasChangesChanged -= HandleReceiveProductSummaryChanges;
			((IBusinessObjectState)AsnLines).HasChangesChanged -= HandleReceiveProductSummaryChanges;

			base.Delete();
		}

		void UnlinkChildSplits()
		{
			if (!IsDeleted) // HACK: Should not occur functionally, problem with WebTracker loading two bizos around one row
			{
				RelatedSplits.CollectionCountChange -= RelatedSplits_CollectionCountChange;
				var childSplits = RelatedSplits.Cast<WhsReceive>().Where(d => d.WD_WD_Split == PK).ToArray();

				foreach (var childSplit in childSplits)
				{
					childSplit.WD_WD_Split = ZGuid.Empty;
				}
			}
		}

		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			Array.ForEach(Inventory.Cast<WhsInventoryView>().ToArray(), i =>
			{
				Factory.AddFetchHint(WhsProductParamsByWhsAndClientSchema.W3_OP, i.WI_OP);
				Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, i.WI_WE_InDocketLine);
			});

			if (IsCustomsTransaction)
			{
				Lines.ForEach(line => Factory.AddFetchHint(WhsBondedWarehouseAttributeSchema.WB_ParentID, line.PK));
			}

			if (WD_DocketSubType == ReceiveType.Codes.Returns && WD_WD_ParentDocket.IsEmpty)
			{
				SetOrderToReturnRelatedInfo();
			}

			// To make sure ReceiveProductSummary's PreSaveValidation is triggered when WhsReceive is saved,
			// ReceiveProductSummaryCollection has to be built before WhsReceive's PreSaveValidation
			_ = ReceiveProductSummaryCollection;

			ReceiveValidationStrategy.ClearCheckSerialNumberIsUniqueCache();

			base.RunPreSaveValidationCore();

			AddErrorIfRelatedSplitsHasErrors();
			Validation.ValidateWD_TotalPallets();

			if (WD_DocketStatus == DocketStatus.Codes.Error && this.IsInDatabase && !HasErrorsIncludingLines)
			{
				WD_DocketStatus = DocketStatus.Codes.Entered;
			}
		}

		void AddErrorIfRelatedSplitsHasErrors()
		{
			if (relatedSplits != null && RelatedSplits.HasErrors())
			{
				var builder = new ZStringBuilder();
				foreach (var split in RelatedSplits)
				{
					builder.Append(Res.GetString("a8e4baf4-16c9-4ceb-8c46-1ec571b085e6", "Reference: {0}-{1}:", split.WD_ExternalReference, split.WD_ExternalReferenceSplit));
					var errorMsg = string.Join(System.Environment.NewLine, split.GetErrors().GetUniqueMessageList());
					builder.Append(errorMsg);
				}
				AddRowError(builder.ToStringWithNewLineBetweenAppends());
			}
		}

		void SetOrderToReturnRelatedInfo()
		{
			var orderToReturn = OrderToReturn;
			if (orderToReturn != null && !IsOrderToReturnFullyReturned(orderToReturn))
			{
				var existingReceivesWithOrderReference = GetExistingReturnReceivesForParentOrderWithOrderReference(orderToReturn);
				WD_ExternalReferenceSplit = GetNextMaxExternalReferenceSplitFromReceiveCollection(existingReceivesWithOrderReference);
				WD_WD_ParentDocket = orderToReturn.PK;

				BuildReturnLinesIfNecessary();
			}
		}

		void BuildReturnLinesIfNecessary()
		{
			if (!IsAutoCreatingReceive
				&& !WhsEnvironment.IsRF
				&& Lines.Count == 0)
			{
				OrderToReturnInfoCache.OrderLinesToReturn.Where(line => line.AvailableQtyToReturn > 0m)
					.ForEach(line =>
					{
						line.QuantityToReturn = line.AvailableQtyToReturn;
						WhsRMAHelper.BuildReceiveLine(this, line);
					});
			}
		}

		internal bool IsReturnReceiveLineQtyValid(WhsReceiveLine receiveLine)
		{
			if (WD_WD_ParentDocket.IsEmpty)
			{
				return true;
			}

			var inventoryKey = WhsRMAHelper.GetKey(receiveLine);
			var linesWithSameKeyTotals = OrderToReturnInfoCache.GetReceiveLinesWithInventoryKey(inventoryKey).Sum(line => line.WE_TransactionQuantity);

			var availableQtyToReturn = OrderToReturnInfoCache.GetOrderLinesWithInventoryKey(inventoryKey).Sum(line => line.AvailableQtyToReturn);
			return linesWithSameKeyTotals <= availableQtyToReturn;
		}

		internal bool IsOrderToReturnFullyReturned(WhsOrder orderToReturn) =>
			orderToReturn != null
			&& orderToReturn.HasRelatedReturnReceive
			&& !OrderToReturnInfoCache.AnyOrderLinesToReturnWithAvailableQuantity;

		internal bool IsValidProductForReturnReceive(ZGuid productPK)
			=> OrderToReturnInfoCache.IsValidProductToReturn(productPK);

		protected override IDisposable GetValidationDataSuspender()
		{
			return new DisposableList(new[]
			{
				base.GetValidationDataSuspender(),
				PalletIDLocationValidationCacheManager.UseLocationCache(Factory),
				PalletIDPutawayTransferCacheManager.UseHasPutawayTransferCache(Factory),
				PendingOrdersCacheManager.UseInventoryPendingOrdersCache(Factory, () => Lines.Select(line => line.WE_OP)),
				UseReceiveValidationCache()
			});
		}

		IDisposable UseReceiveValidationCache()
		{
			return new DisposableAction(
				() =>
				{
					bondedWarehouseAttributeCache = new WhsBondedWarehouseAttributeCache(Factory, Lines);
					packageGroupIdsCache = new PackageGroupIdsCache(Factory, this);
					cachedProductPackageTotals = GetProductPackageTotals(Lines.Cast<WhsReceiveLine>());
					docketUNDGValidationCache = DocketUNDGValidationHelper.BuildDocketUNDGValidationCache(this);
					orderToReturnInfoCache = new OrderToReturnInfoCache(Factory, this);
					serialNumberUniquenessCheckerCache = new SerialNumberUniquenessChecker(this);
				},
				() =>
				{
					bondedWarehouseAttributeCache = null;
					packageGroupIdsCache = null;
					cachedProductPackageTotals = null;
					docketUNDGValidationCache = null;
					orderToReturnInfoCache = null;
					serialNumberUniquenessCheckerCache = null;
				});
		}

		#endregion

		#region RunPreFinaliseValidationCore

		protected override bool RunPreFinaliseValidationCore()
		{
			return base.RunPreFinaliseValidationCore()
				&& CheckAllLinesHaveSameAreaType()
				&& CheckAllPutawayTransferLinesAreFinalised();
		}

		bool CheckAllLinesHaveSameAreaType()
		{
			var result = true;
			var linesWithLocation = Lines.Where(line => !line.WE_WL.IsEmpty).Cast<WhsReceiveLine>().ToArray();
			if (linesWithLocation.Length > 1)
			{
				var areaType = linesWithLocation[0].PutawayLocationAreaType;
				var locationsPKs = linesWithLocation.Select(l => l.WE_WL).Distinct();

				var areaTypes = areaType == AreaTypes.Codes.FreeStore || areaType == AreaTypes.Codes.DockDoor
					? new[] { AreaTypes.Codes.FreeStore, AreaTypes.Codes.DockDoor }
					: new[] { (string)areaType };

				var locationQuery = new ZQuery(WhsLocationViewSchema.PK, locationsPKs);
				locationQuery.AddToFilter(WhsLocationViewSchema.WLV_PutawayAreaType, SQLComparisonOperator.NotEqual, areaTypes);

				result = !Factory.Exists(typeof(WhsLocation), locationQuery);

				if (!result)
				{
					AddRowError(CannotPutawayIntoDifferentAreaTypesErrorMsg);
				}
			}

			return result;
		}

		bool CheckAllPutawayTransferLinesAreFinalised()
		{
			var result = true;

			if (!HasErrorsIncludingLines && Lines.Any(line => !line.WE_PalletID.IsEmpty))
			{
				var pickLineQuery = new ZQuery(WhsPickLineSchema.WZ_WE_InventoryLine, Lines.Select(l => l.PK));
				Factory.AddFetchHint(typeof(WhsPickLine), pickLineQuery);

				foreach (WhsReceiveLine receiveLine in Lines)
				{
					var putawayTransfer = receiveLine.PutawayTransfer;
					if (putawayTransfer != null && !putawayTransfer.IsFinalised)
					{
						putawayTransfer.FinaliseDocketWithoutUserConfirmation();
						if (putawayTransfer.HasErrors)
						{
							receiveLine.AddRowError(ContainsUnfinalizedPutawayTransfersErrorMessage);
							result = false;
						}
					}
				}
			}

			return result;
		}

		protected override void SynchroniseOffsetsToWarehouseTimeCore(ITimeZone calculationTimeZone)
		{
			base.SynchroniseOffsetsToWarehouseTimeCore(calculationTimeZone);

			WD_ETDInfo.ConvertOffsetPropertyToTimeZone(calculationTimeZone);
			WD_ETAInfo.ConvertOffsetPropertyToTimeZone(calculationTimeZone);
			WD_ArrivalDateInfo.ConvertOffsetPropertyToTimeZone(calculationTimeZone);
		}

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			UpdateScreeningStatusFromScreeningParties();
		}

		void UpdateScreeningStatusFromScreeningParties()
		{
			ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(this, this, WD_ScreeningStatusInfo.OriginalValue?.ToString());
		}

		#region OnSaving

		bool failedToAutoCreatePutawayTransfers;

		string UnableToSaveCaption => ResString.GetMultilingualString("b10967ab-fd4f-4aa9-89b2-8b4abf64caf3", "Unable to Save");

		public override void OnSaving()
		{
			base.OnSaving();
			UpdateTaskPlanningStatus();
			PreCreatePutawayTransfer();

			if ((!IsInDatabase || WD_DocketStatusInfo.HasChanges) && IsFinalised)
			{
				CheckAllPutawayTransferLinesHasValidInventoryStatus();
			}

			if (LinesToGeneratePkgGroupID != null && LinesToGeneratePkgGroupID.Count > 0)
			{
				foreach (var (line, value) in LinesToGeneratePkgGroupID)
				{
					var generatedGroupID = PackageGroupIdGenerator.GetPackageGroupId(Factory, this, value);
					if (generatedGroupID.HasValue)
					{
						line.WE_PackageGroupId = generatedGroupID.Value;
					}
				}
			}
			LinesToGeneratePkgGroupID = null;

			// tested in WhsWorkOrderTest
			if (WD_IsInwardsProcessingJob && !IsInDatabase && IsCreatedFromWorkOrder)
			{
				var allocationKeyCount = 1;

				foreach (var line in Lines)
				{
					line.WE_AllocationKey = $"{WD_DocketID}-{allocationKeyCount++.ToString(Culture.Invariant).PadLeft(4, '0')}";
				}
			}
		}

		void CheckAllPutawayTransferLinesHasValidInventoryStatus()
		{
			var validFinalisedPutawayTransferInventoryStatusList = new ZString[] { InventoryStatus.Codes.Available, InventoryStatus.Codes.Held };
			foreach (var putawayTransferLine in IEnumerableExtensions.DistinctBy(Lines.Cast<WhsReceiveLine>().Where(line => line.CanHavePutawayTransfer).SelectMany(l => l.Inventory[0].AllPickLines), pl => pl.WZ_WE_TransactionLine)
						 .Select(l => l.DocketLine).OfType<WhsTransferLine>().WhereNotNull())
			{
				if (!validFinalisedPutawayTransferInventoryStatusList.Contains(putawayTransferLine.WE_CurrentInventoryStatus))
				{
					throw new ZCannotSaveException(Res.GetString("WhsReceive|PutawayTransferLinesInvalidInventoryStatus", "Cannot save as putaway transfer lines have invalid inventory status."),
						Res.GetString("WhsTransfer|PutawayTransferLinesMustHaveValidInventoryStatus", "Related putaway transfer lines must have valid inventory status."), ExceptionType.BusinessFailure);
				}
			}
		}

		void UpdateTaskPlanningStatus()
		{
			var lazyWarehouse = new Lazy<WhsWarehouse>(() => Warehouse);
			if (IsFinalisedOrCancelled)
			{
				WD_TaskPlanningStatus = string.Empty;
			}
			else if (WD_TaskPlanningStatus.IsEmpty || WD_TaskPlanningStatus == TaskPlanningStatus.Codes.NotReady)
			{
				if (StartedReceiving
					&& (!IsInDatabase || WD_StartedReceivingTimeUtcInfo.HasChanges)
					&& (lazyWarehouse.Value?.WW_GG_ReleaseGroup.IsValid ?? false)
					&& WarehouseDataRegistry.Instance.AutomaticallySetReceiveTaskPlanningStatusToReadyForPlanning.GetFallBackValueAtAllLevels(Guid.Empty, lazyWarehouse.Value.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty))
				{
					WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Ready;
				}
				else if (!IsInDatabase || WD_WW_WhsInfo.HasChanges)
				{
					WD_TaskPlanningStatus = (lazyWarehouse.Value?.WW_GG_ReleaseGroup.IsValid ?? false) ? TaskPlanningStatus.Codes.NotReady : string.Empty;
				}
			}
		}

		void PreCreatePutawayTransfer()
		{
			if (failedToAutoCreatePutawayTransfers)
			{
				throw new ZCannotSaveException(
					ResString.GetMultilingualString("80fce8f1-2c52-4009-8a47-981000f4be9d", "Cannot save as an error occurred during Saving. Please reload the Receive."),
					UnableToSaveCaption, ExceptionType.BusinessFailure);
			}

			var shouldCreate = (!IsInDatabase || IsInDatabase && (WD_UnloadCompletedTimeInfo.HasChanges || WD_HoldPalletIDPutawayInfo.HasChanges))
				&& WD_UnloadCompletedTime.IsValid
				&& !WD_HoldPalletIDPutaway
				&& !IsFinalisedOrCancelled
				&& Warehouse.WW_GG_ReleaseGroup.IsValid;

			if (shouldCreate)
			{
				var inventoryWithPalletIdAndLocation = Inventory.Where(w => !w.WI_PalletID.IsEmpty && w.WI_WL.IsValid).ToArray();
				var locationPKs = inventoryWithPalletIdAndLocation.Select(s => s.WI_WL).Distinct();
				foreach (var locationPK in locationPKs)
				{
					Factory.AddFetchHint(typeof(WhsInventoryView), WhsInventoryViewSchema.WI_WL, locationPK);
				}

				var staffCode = GlbStaff.CurrentUser.GS_Code;
				var inventoryToDockDoor = inventoryWithPalletIdAndLocation.Where(w => w.Location.IsDockDoorLocation).ToArray();
				var palletIDs = inventoryToDockDoor.Select(s => s.WI_PalletID.ToString().ToUpper()).Distinct().ToArray();
				if (palletIDs.Length > 0)
				{
					var inventories = Factory
						.Load<WhsInventoryView>(PutawayHelper.FindInventoryQuery(palletIDs, WD_WW_Whs))
						.Where(w => w.WI_WD != PK)
						.Concat(inventoryToDockDoor);
					var helper = ObjectFactory.Get<ICreatePutawayTransferHelper>();
					var errorMessage = helper.CreateTransfer(Factory, Warehouse, inventories, palletIDs.Length > 1, staffCode);
					if (!errorMessage.IsNullOrEmpty())
					{
						failedToAutoCreatePutawayTransfers = true;
						throw new ZCannotSaveException(errorMessage, UnableToSaveCaption, ExceptionType.BusinessFailure);
					}
					else
					{
						var transfer = inventories.First()?.PutawayTransferLine?.Docket;
						if (transfer != null && Warehouse.WW_GG_ReleaseGroup.IsValid)
						{
							transfer.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
							CreatePutawayTransferTask(transfer, staffCode);
						}
					}
				}
			}
		}

		void CreatePutawayTransferTask(WhsTransfer transfer, string staffCode)
		{
			var task = transfer.Factory.New<WhsTransferProcessTasks>();
			task.P9_ParentID = transfer.PK;
			task.P9_ParentTableCode = WhsDocketSchema.Constants.Prefix;
			task.P9_FormFlowType = WarehouseTaskFormFlowTypes.PutawayJob;
			task.P9_GS_NKAssignedStaffMember = staffCode;

			var description = Res.GetString("b5ccdfdc-e43e-435e-9e98-12d551d83e8b", "Temporary Putaway Task Description");
			SetDefaultTaskDetails(task, description, staffCode, isWorking: false);
		}

		void SetDefaultTaskDetails(ProcessTask task, string description, ZString userNK, bool isWorking)
		{
			task.P9_Description = description;
			task.P9_Type = Constants.Workflow.UndefinedTaskType;

			var originalUserInteractive = Globals.IsUserInteractive;
			try
			{
				// Current code does not allow to set user if user is system admin (support user is also system admin)
				Globals.IsUserInteractive = true;
				task.P9_GS_NKAssignedStaffMember = userNK;
			}
			finally
			{
				Globals.IsUserInteractive = originalUserInteractive;
			}

			task.P9_Status = isWorking ? ProcessTaskStatusCodeList.Codes.Working : ProcessTaskStatusCodeList.Codes.Assigned;
		}

		#region PackageGroupIdGenerator

		class PackageGroupIdGenerator
		{
			PackageGroupIdGenerator()
			{
			}

			public static ZString? GetPackageGroupId(BusinessObjectFactory factory, WhsReceive receive, ZString? universalPackId)
			{
				var generator = factory.GetCachedValue(receive.PK.ToString(), () => new PackageGroupIdGenerator());

				ZString? result = null;
				var value = universalPackId.GetValueOrDefault();
				if (!value.IsEmpty && !generator.PackageGroupIds.TryGetValue(value, out result))
				{
					generator.PackageGroupIds[value] = result = GetPackageId(receive, generator.PackageGroupIds.Count + 1);
				}

				return result;
			}

			static ZString GetPackageId(WhsReceive receive, int count)
				=> ZString.Format("{0}-{1}", receive.WD_DocketID, count.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0'));

			Dictionary<ZString, ZString?> PackageGroupIds
				=> packageGroupIds ?? (packageGroupIds = new Dictionary<ZString, ZString?>());
			Dictionary<ZString, ZString?> packageGroupIds;
		}

		#region AddReceiveLineToGeneratePackageGroupId

		// Tested in usage through WhsReceiveLineDataObjectReaderTest.TestPackageGroupId
		public void AddReceiveLineToGeneratePackageGroupId(WhsReceiveLine line, ZString? value)
		{
			if (IsImportingData && line.WE_WD == PK)
			{
				if (LinesToGeneratePkgGroupID == null)
				{
					LinesToGeneratePkgGroupID = new List<(WhsReceiveLine, ZString?)>();
				}
				LinesToGeneratePkgGroupID.Add((line, value));
			}
		}

		List<(WhsReceiveLine line, ZString? value)> LinesToGeneratePkgGroupID;

		#endregion

		#endregion

		#endregion

		#region LockReceiveAndCheckCriticalChanges

		protected override ZString HumanReadableNameCore
		{
			get { return WD_DocketID.IsEmpty ? HumanReadableNameWithoutID : ZString.Format("{0} {1}", HumanReadableNameWithoutID, WD_DocketID); }
		}

		protected override ZString HumanReadableNameWithoutID => Res.GetString("f78555f7-ff43-4b50-8cb6-8f211c5ebb79", "Warehouse Receipt");

		#endregion

		#region Notes

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = base.NoteContextsForRelatedNotes;

				result.Module |= StmNoteContextModule.W;
				result.Direction |= StmNoteContextDirection.R;
				result.FreightMode |= StmNoteContextFreightMode.R;

				return result;
			}
		}

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);

				var supplierOrg = SupplierDocAddress.Organisation;
				if (supplierOrg != null)
				{
					result.Add(supplierOrg);
				}

				var transportOrg = TransportCoDocAddress.Organisation;
				if (transportOrg != null)
				{
					result.Add(transportOrg);
				}

				return result.ToArray();
			}
		}

		#endregion

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsReceiveFetchStrategy(this);
		}

		#endregion

		#region Related Business Objects

		#region CarrierServiceLevel

		public override OrgCarrierServiceLevel CarrierServiceLevel => this.GetCarrierServiceLevel(WD_PL_NKCarrierServiceLevel);

		#endregion

		#region RelatedJobs

		protected override List<IRelatedJob> GetRelatedJobsCore()
		{
			var result = base.GetRelatedJobsCore();

			var componentOrder = ComponentOrder;
			if (componentOrder != null)
			{
				result.Add(componentOrder);
			}

			var orderToReturn = OrderToReturn;
			if (orderToReturn != null)
			{
				result.Add(orderToReturn);
			}

			result.AddRange(GetRelatedParents().Cast<IRelatedJob>());
			result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEvents(this).Cast<IRelatedJob>());

			return result;
		}

		WhsComponentOrder ComponentOrder
		{
			get { return Factory.LoadTop1<WhsComponentOrder>(ComponentOrderQuery); }
		}

		ZQuery ComponentOrderQuery
		{
			get
			{
				var result = new ZQuery(WhsDocketSchema.WD_DocketType, new[] { DocketType.Codes.WorkOrder, DocketType.Codes.DynamicWorkOrder });
				result.AddToFilter(WhsDocketSchema.PK, WD_WD_ParentDocket);
				return result;
			}
		}

		internal WhsOrder OrderToReturn => Factory.LoadTop1<WhsOrder>(OrderToReturnQuery);

		ZQuery OrderToReturnQuery
		{
			get
			{
				var orderToReturnQuery = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);
				if (WD_WD_ParentDocket.IsEmpty)
				{
					orderToReturnQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, WD_ExternalReference);
					orderToReturnQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, WD_OH_Client);
					orderToReturnQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, WhsOrderStatus.Codes.Departed);
				}
				else
				{
					orderToReturnQuery.AddToFilter(WhsDocketSchema.PK, this.WD_WD_ParentDocket);
				}
				return orderToReturnQuery;
			}
		}

		#endregion

		#region AsnLines

		[ChildEditable(true)]
		public WhsAsnLineCollection AsnLines
		{
			get
			{
				if (asnLines == null)
				{
					asnLines = new WhsAsnLineCollection(this, Factory);
					asnLines.Load();
					RegisterEditableChildObject(asnLines);
				}
				return asnLines;
			}
		}

		WhsAsnLineCollection asnLines;

		#endregion

		#region ReceiveProductSummaries

		[ChildEditable(true)]
		public WhsReceiveProductSummaryCollection ReceiveProductSummaryCollection
		{
			get
			{
				if (receiveProductSummaries == null)
				{
					receiveProductSummaries = new WhsReceiveProductSummaryCollection(Factory, this);
					((IBusinessObjectState)Lines).HasChangesChanged += HandleReceiveProductSummaryChanges;
					RegisterEditableChildObject(receiveProductSummaries);
				}

				return receiveProductSummaries;
			}
		}

		void HandleReceiveProductSummaryChanges(object sender, HasChangesChangedEventArgs e)
		{
			if (e.ObjectJustWasChanged && !receiveProductSummaries.NeedsRefresh)
			{
				receiveProductSummaries.NeedsRefresh = true;
			}
		}

		WhsReceiveProductSummaryCollection receiveProductSummaries;

		#endregion

		#region Lines

		[ChildEditable(true)]
		public new WhsReceiveLineCollection Lines
		{
			get { return (WhsReceiveLineCollection)base.Lines; }
		}

		protected override WhsDocketLineCollection GetNewDocketLineCollection()
		{
			return new WhsReceiveLineCollection(this, new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, ZBool.True));
		}

		#endregion

		#region Inventory

		[ChildEditable(false)]
		public WhsInventoryViewCollection Inventory
		{
			get
			{
				if (inventory == null)
				{
					inventory = WrapperStrategy.GetNewWhsInventoryCollection();

					foreach (var receiveLine in Lines)
					{
						Factory.AddFetchHint(typeof(WhsInventoryView), WhsInventoryViewSchema.WI_WE_InDocketLine, receiveLine.PK);
					}

					foreach (WhsReceiveLine line in Lines)
					{
						foreach (WhsInventoryView inv in line.Inventory)
						{
							if (inv.WI_IsOriginalReceiptLine)
							{
								inventory.Add(inv);
							}
						}
					}

					RegisterEditableChildObject(inventory);
					inventory.CountChanged += InventoryCountChanged;
					ListManager.HookListChangedEvent(inventory);
				}
				return inventory;
			}
		}

		protected virtual WhsInventoryViewCollection GetNewWhsInventoryCollection()
		{
			return new WhsInventoryViewCollection(Factory, this);
		}

		WhsInventoryViewCollection inventory;

		#region InventoryCountChanged

		void InventoryCountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemRemoved)
			{
				if (Inventory.Count == 0)
				{
					UpdateDocketStatus();
				}

				UpdateTotalPalletsReceived();
			}
		}

		#endregion

		#endregion

		#region Logs

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEvents(this));
				result.AddRange(this.Lines);
				result.AddRange(GetRelatedParents());
				return result.ToArray();
			}
		}

		#endregion

		#region ReceiveValidationStrategy

		public WhsReceiveValidationStrategy ReceiveValidationStrategy
		{
			get { return receiveValidationStrategy ?? (receiveValidationStrategy = (WhsEnvironment.IsRF) ? new WhsReceiveValidationRFStrategy(this) : new WhsReceiveValidationStrategy(this)); }
		}

		WhsReceiveValidationStrategy receiveValidationStrategy;

		#endregion

		#region TransportCoDocAddress

		public JobDocAddress TransportCoDocAddress => this.GetTransportCoDocAddress(ref transportCoDocAddress);
		JobDocAddress transportCoDocAddress;

		#endregion

		#region TransportCo

		protected override OrgHeader TransportCoCore => this.GetTransportCo();

		#endregion

		#region References

		protected override void AfterReferencesLoaded() => References.SetReadOnlyIncludingChildren(IsCreatedFromPickByBOM);

		#endregion

		#region Containers

		protected override void AfterContainersLoaded() => Containers.SetReadOnlyIncludingChildren(IsCreatedFromPickByBOM);

		#endregion

		#region InboundDockDoor

		public WhsLocation InboundDockDoor
		{
			get { return Factory.Load<WhsLocation>(WD_WL_InboundDockDoor); }
		}

		#endregion

		#endregion

		#region Related Splits

		public WhsReceiveCollection RelatedSplits
		{
			get
			{
				if (relatedSplits == null)
				{
					var filter = new ZQuery(WhsDocketSchema.WD_WD_Split, PK);
					if (WD_WD_Split.IsValid)
					{
						filter.AddToFilter(JoinCondition.Or, WhsDocketSchema.PK, WD_WD_Split);
					}

					relatedSplits = new WhsReceiveCollection(Factory, filter);
					relatedSplits.CollectionCountChange += RelatedSplits_CollectionCountChange;
					foreach (WhsReceive split in relatedSplits)
					{
						SetRelatedSplitType(split);
					}
					relatedSplits.ApplySort(WhsDocketSchema.WD_ExternalReferenceSplit.Name, ListSortDirection.Ascending);
				}
				return relatedSplits;
			}
		}

		WhsReceiveCollection relatedSplits;

		void RelatedSplits_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded && e.BizObject != null)
			{
				var split = (WhsReceive)e.BizObject;
				SetRelatedSplitType(split);
			}
		}

		void SetRelatedSplitType(WhsReceive split)
		{
			using (split.SuspendSettingHasChanges())
			{
				if (split.WD_WD_Split == PK)
				{
					split.SplitRelationshipTypeDescription = Res.GetString("6bec47d7-8cf1-4220-bb9f-f756e6453278", "CHILD");
				}
				else
				{
					split.SplitRelationshipTypeDescription = Res.GetString("78f6378c-d59e-4ba4-a740-50d571617530", "PARENT");
				}
			}
		}

		#endregion

		#region UpdateDocketStatus

		public void UpdateDocketStatus()
		{
			if (!IsFinalisedOrCancelled)
			{
				WD_DocketStatus = IsThereAnyPutawayInventoryLine()
						 ? DocketStatus.Codes.Putaway
						 : IsInDatabase
								? DocketStatus.Codes.Entered
								: DocketStatus.Codes.New;
			}
		}

		bool IsThereAnyPutawayInventoryLine()
		{
			var inventoryView = Inventory.Cast<WhsInventoryView>();
			var inventoryWithPutawayStatus = inventoryView.Any(inv => inv.WI_InventoryStatus == InventoryStatus.Codes.Putaway);
			var inventoryHasFinalisedPutawayTransferLine = new Lazy<bool>(() => inventoryView.Any(i => ((WhsReceiveLine)i.InDocketLine)?.PutawayTransferLine?.IsFinalised ?? false));

			return inventoryWithPutawayStatus || inventoryHasFinalisedPutawayTransferLine.Value;
		}

		#endregion

		#region Validation

		protected override WhsDocketValidation GetNewValidation()
		{
			return WrapperStrategy.GetNewValidation();
		}

		#endregion

		#region Lookups

		protected override WhsDocketLookups GetNewLookups()
		{
			return WrapperStrategy.GetNewLookups();
		}

		#endregion

		#region ReceiveCategory

		[ReadOnlyMember(nameof(ReceivingCategoryReadonly))]
		[List(nameof(Lookups) + "." + nameof(WhsReceiveLookups.ReceiveCategories))]
		public override ZString WD_ReceiveCategory
		{
			get { return base.WD_ReceiveCategory; }
			set { base.WD_ReceiveCategory = value; }
		}

		bool ReceivingCategoryReadonly => StartedReceiving || IsFinalisedOrCancelled;

		#endregion

		#region WD_F3_NKTotalPackType

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZString WD_F3_NKTotalPackType
		{
			get => base.WD_F3_NKTotalPackType;
			set => base.WD_F3_NKTotalPackType = value;
		}

		#endregion

		#region WD_HoldPalletIDPutaway

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZBool WD_HoldPalletIDPutaway
		{
			get => base.WD_HoldPalletIDPutaway;
			set => base.WD_HoldPalletIDPutaway = value;
		}

		#endregion

		#region WD_UnloadCompletedTime 

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZDateTimeOffset WD_UnloadCompletedTime
		{
			get => base.WD_UnloadCompletedTime;
			set => base.WD_UnloadCompletedTime = value;
		}

		#endregion

		#region WD_PackagesSent

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZInt WD_PackagesSent
		{
			get => base.WD_PackagesSent;
			set => base.WD_PackagesSent = value;
		}

		#endregion

		#region WD_TotalPallets

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZShort WD_TotalPallets
		{
			get => base.WD_TotalPallets;
			set => base.WD_TotalPallets = value;
		}

		#endregion

		#region WD_TotalUnits

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZDecimal WD_TotalUnits
		{
			get => base.WD_TotalUnits;
			set => base.WD_TotalUnits = value;
		}

		#endregion

		[ReadOnlyMember(nameof(IsCreatedFromPickByBOM))]
		public override ZString WD_TransportReference
		{
			get => base.WD_TransportReference;
			set => base.WD_TransportReference = value;
		}

		[ReadOnlyMember(nameof(ReceiveReferenceReadOnly))]
		public override ZString WD_ExternalReference
		{
			get => base.WD_ExternalReference;
			set => base.WD_ExternalReference = value;
		}

		public override ZString WD_DocketSubType
		{
			get => base.WD_DocketSubType;
			set
			{
				base.WD_DocketSubType = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWD_ExternalReference();
				}
			}
		}

		#region Flags

		#region HasOversAndUnders

		public ZBool HasOversAndUnders
		{
			get { return Lines.Any(l => l.WE_ClientOrderedUnits != l.WE_TransactionQuantity); }
		}

		#endregion

		#region Generate Serial Numbers Semaphore

		internal Semaphore GenerateSerialNumbersSemaphore
		{
			get { return generateSerialNumbersSemaphore ?? (generateSerialNumbersSemaphore = new Semaphore()); }
		}

		Semaphore generateSerialNumbersSemaphore;

		public bool IsGeneratingSerialNumbers
		{
			get { return GenerateSerialNumbersSemaphore.IsSuspended; }
		}

		#endregion

		#region IsPossibleChangeOfInventoryJob

		protected override bool IsPossibleChangeOfInventoryJob => IsCustomsTransaction;

		#endregion

		public bool IsReturnReceive => WD_DocketSubType == ReceiveType.Codes.Returns;

		bool IsLinkedReturnReceive => IsReturnReceive && WD_WD_ParentDocket.IsValid;

		public bool IsAutoCreatingReceive { get; set; }

		public bool IsClearingCompletionAllowed => !IsFinalisedOrCancelled && WD_UnloadCompletedTime.IsValid && Lines.Cast<WhsReceiveLine>().All(line => !line.HasPutawayTransfer);

		#endregion

		#region Properties

		#region Bool

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly && !IsCreatedFromWorkOrder; }
			set { base.ReadOnly = value; }
		}

		protected override bool StandardReadOnly
		{
			get { return StandardReadOnlyWithoutWorkOrderCheck || IsCreatedFromWorkOrder || IsCreatedFromPickByBOM; }
		}

		protected bool StandardReadOnlyWithoutWorkOrderCheck
		{
			get { return IsPuttingAway || IsFinalisedOrCancelled; }
		}

		protected override bool NonStandardReadOnly1
		{
			get { return (base.NonStandardReadOnly1 && !IsPostFinalizeEditAllowed) || IsCreatedFromWorkOrder || IsCreatedFromPickByBOM; }
		}

		protected override bool TotalWeightAndVolumnReadOnly => base.TotalWeightAndVolumnReadOnly || IsCreatedFromPickByBOM;

		bool ReceiveReferenceReadOnly => NonStandardReadOnly1 || IsLinkedReturnReceive;

		#endregion

		#region IsPuttingAway

		public bool IsPuttingAway
		{
			get { return WD_DocketStatus == DocketStatus.Codes.Putaway; }
		}

		#endregion

		#region IsPuttingAwayOrFinalised

		public bool IsPuttingAwayOrFinalised
		{
			get { return IsPuttingAway || IsFinalised; }
		}

		#endregion

		#region IsPostFinalizeEditAllowedCore

		protected override bool IsPostFinalizeEditAllowedCore
		{
			get { return Env.Security.WhsReceivePostFinaliseEdit.IsAllowed && !IsAtleastOneInvoiceLinePosted; }
		}

		#endregion

		#region IsCreatedFromWorkOrder

		public bool IsCreatedFromWorkOrder
		{
			get { return ParentDocket is WhsComponentOrder; }
		}

		#endregion

		#region IsCreatedFromPickByBOM

		protected override bool IsCreatedFromPickByBOMCore => WD_WP_ParentPickForReceive.IsValid;

		#endregion

		#region IsSameAsFinalisedOrCancelled

		protected override bool IsSameAsFinalisedOrCancelled => IsCreatedFromPickByBOM;

		#endregion

		#region IsLocationSetOnAllInventoryLines

		public bool IsLocationSetOnAllInventoryLines
		{
			get
			{
				foreach (WhsInventoryView whsInventory in Inventory)
				{
					SetDefaultLocationForUnders(whsInventory);
					if (whsInventory.WI_WL.IsEmpty)
					{
						return false;
					}
				}

				return true;
			}
		}

		void SetDefaultLocationForUnders(WhsInventoryView whsInventory)
		{
			if (whsInventory.WI_WL.IsEmpty && whsInventory.WI_InDocketLineUnits == 0m && whsInventory.WI_ExpectedReceiptQuantity > 0m)
			{
				if (Warehouse.DefaultLocation != null)
				{
					whsInventory.WI_WL = Warehouse.DefaultLocation.PK;
				}
			}
		}

		#endregion

		#region IsSplitQuantitySetOnAtleastOneInventoryLine

		bool IsSplitQuantitySetOnAtleastOneInventoryLine
		{
			get
			{
				foreach (WhsInventoryView whsInventory in Inventory)
				{
					if (whsInventory.WI_SplitQuantity > 0m)
					{
						return true;
					}
				}

				return false;
			}
		}

		#endregion

		#region IsSplitQuantityFullySetOnAllLines

		bool IsSplitQuantityFullySetOnAllLines
		{
			get
			{
				foreach (WhsInventoryView whsInventory in Inventory)
				{
					if (whsInventory.WI_SplitQuantity < whsInventory.WI_InDocketLineUnits)
					{
						return false;
					}
				}

				return true;
			}
		}

		#endregion

		#region IsAreaTypeTheSameOnAllInventoryLines

		bool IsAreaTypeTheSameOnAllInventoryLines => IEnumerableExtensions.DistinctBy(Inventory.Cast<WhsInventoryView>(), i => i.LocationPutawayAreaType).IsCountLessThan(2);

		#endregion

		#region HasInventoryWithTemporaryProducts

		public bool HasInventoryWithTemporaryProducts
		{
			get
			{
				foreach (WhsInventoryView whsInventory in Inventory)
				{
					if (whsInventory.IsTemporaryProduct)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region HasInventoryWithStockAndTemporaryProducts

		public bool HasInventoryWithStockAndTemporaryProducts
		{
			get
			{
				foreach (WhsInventoryView whsInventory in Inventory)
				{
					if (whsInventory.IsTemporaryProduct && whsInventory.WI_InDocketLineUnits > 0m && !whsInventory.WI_UnitsUQ.IsEmpty)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region StartedReceiving

		public bool StartedReceiving => !WD_StartedReceivingTimeUtc.IsEmpty;

		#endregion

		#endregion

		#region Description

		protected override ZString DescriptionCore
		{
			get { return Res.GetString("08e25379-c74d-4de6-86e5-1c1d7aed4171", "Receive"); }
		}

		#endregion

		#region NeedToAutoGenerateLineNumbersOnCreation

		public override bool NeedToAutoGenerateLineNumbersOnCreation
		{
			get
			{
				return AsnLines.Count == 0;
			}
		}

		#endregion

		#region WD_ArrivalDate

		public override ZDateTimeOffset WD_ArrivalDate
		{
			get { return base.WD_ArrivalDate; }
			set
			{
				if (WD_ArrivalDate != value || OffsetHasChanged(WD_ArrivalDate, value))
				{
					base.WD_ArrivalDate = value;
					SynchronizeInventoryArrival();
				}
			}
		}

		bool OffsetHasChanged(ZDateTimeOffset offset1, ZDateTimeOffset offset2)
			=> offset1.IsValid && offset2.IsValid && offset1.Offset != offset2.Offset;

		#endregion

		#region WD_WW_Whs

		public override ZGuid WD_WW_Whs
		{
			get { return base.WD_WW_Whs; }
			set
			{
				if (WD_WW_Whs != value)
				{
					base.WD_WW_Whs = value;
					SynchroniseWarehouseOnInventory();
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, WD_ScreeningStatus, Factory, WD_WW_Whs);
				}
			}
		}

		void SynchroniseWarehouseOnInventory()
		{
			foreach (WhsInventoryView inv in Inventory)
			{
				inv.WI_WW_Whs = WD_WW_Whs;
			}
		}

		protected override bool WarehouseReadOnly => IsReadyForPlanningOrPlanned || base.WarehouseReadOnly;

		#endregion

		#region WD_OH_Client

		public override ZGuid WD_OH_Client
		{
			get { return base.WD_OH_Client; }
			set
			{
				if (WD_OH_Client != value)
				{
					base.WD_OH_Client = value;
					SynchroniseClientOnInventory();
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, WD_ScreeningStatus, Factory, WD_OH_Client);
				}
			}
		}

		void SynchroniseClientOnInventory()
		{
			foreach (WhsInventoryView inv in Inventory)
			{
				inv.WI_OH_Client = WD_OH_Client;
			}
		}

		protected override bool ClientReadOnly => StartedReceiving || IsReadyForPlanningOrPlanned || base.ClientReadOnly;

		#endregion

		#region WD_ScreeningStatus

		[ReadOnly(true)]
		[List("Lookups.ScreeningStatusesList")]
		public override ZString WD_ScreeningStatus
		{
			get => base.WD_ScreeningStatus;
			set => base.WD_ScreeningStatus = value;
		}

		#endregion

		#region WD_FirstScannedInboundDockDoorUtc

		public override ZDateTime WD_FirstScannedInboundDockDoorUtc
		{
			get { return base.WD_FirstScannedInboundDockDoorUtc; }
			set
			{
				base.WD_FirstScannedInboundDockDoorUtc = value;
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WD_WL_InboundDockDoor), WD_FirstScannedInboundDockDoorUtc.IsValid ? ConcurrencyPolicy.Strict : ConcurrencyPolicy.Default);
			}
		}

		#endregion

		#region TransportCoPK

		public ZGuid TransportCoPK
		{
			get => this.GetTransportCoPK();
			set => this.SetTransportCoPK(value);
		}

		#endregion

		#region TransportCoName

		protected override ZString TransportCoNameCore => this.GetTransportCoName();

		#endregion

		#region TransportCoNameOrPK For Grid Binding

		public ZString TransportCoFieldType => this.GetTransportCoFieldType();

		protected override ZString TransportCoNameOrPKCore
		{
			get => this.GetTransportCoNameOrPK();
			set => this.SetTransportCoNameOrPK(value);
		}

		public ZPropertyInfo TransportCoNameOrPKInfo => this.GetTransportCoNameOrPKInfo(GetZPropertyInfo);

		protected override int TransportCoNameOrPKMaxLength => this.GetTransportCoNameOrPKMaxLength();

		#endregion

		#region TotalPalletsReceived

		[ResourceStringData("WhsReceive|TotalPalletsReceived", Caption = "Total Line PLTs", ShortCaption = "Line PLTs")]
		public ZInt TotalPalletsReceived
		{
			get
			{
				return IEnumerableExtensions.DistinctBy(Lines.Where(l => !l.WE_PalletID.IsEmpty), l => l.WE_PalletID).Count();
			}
		}

		public ZPropertyInfo TotalPalletsReceivedInfo
		{
			get { return GetZPropertyInfo(Schema.TotalPalletsReceived); }
		}

		public IDisposable DeferUpdateTotalPalletsReceived()
		{
			var manager = new SemaphoreManager(UpdateTotalPalletsReceivedSemaphore);
			return new DisposableAction(() =>
			{
				manager.Dispose();
				UpdateTotalPalletsReceived();
			});
		}

		public void UpdateTotalPalletsReceived()
		{
			if (!UpdateTotalPalletsReceivedSemaphore.IsSuspended)
			{
				// Test in WhsReceive and WhsReceiveLine
				TotalPalletsReceivedInfo.RefreshBinding();
			}
		}

		Semaphore UpdateTotalPalletsReceivedSemaphore => updateTotalPalletsReceivedSemaphore ?? (updateTotalPalletsReceivedSemaphore = new Semaphore());
		Semaphore updateTotalPalletsReceivedSemaphore;

		#endregion

		#region SplitRelationshipTypeDescription

		[ReadOnly(true)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString SplitRelationshipTypeDescription
		{
			get { return splitRelationshipTypeDescription; }
			set { SetNonPersistentPropertyValue(SplitRelationshipTypeDescriptionInfo, ref splitRelationshipTypeDescription, value); }
		}
		ZString splitRelationshipTypeDescription;

		public ZPropertyInfo SplitRelationshipTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.SplitRelationshipTypeDescription); }
		}

		#endregion

		#region TotalASNLineUnits

		public ZDecimal TotalASNLineUnits
		{
			get
			{
				ZDecimal result = 0m;
				foreach (WhsAsnLine asnLine in AsnLines)
				{
					result += asnLine.WN_Quantity;
				}
				return result;
			}
		}

		#endregion

		#region WD_TotalExpectedQuantity

		public ZDecimal WD_TotalExpectedQuantity => Lines.Sum(l => l.WE_ClientOrderedUnits);

		#endregion

		#region IsFinaliseAllowed

		IDPSSecurityProvider DPSSecurityProvider
			=> dpsSecurityProvider ?? (dpsSecurityProvider = Globals.CanShowDialogs
				? new DPSSecurityProvider(NotificationSubscriber)
				: new DPSSecurityNonInteractiveProvider());
		IDPSSecurityProvider dpsSecurityProvider;

		protected override bool CheckIsHeldByCustomsWhenFinalising
		{
			get { return true; }
		}

		protected override bool CanFinalizeDPS
		{
			get
			{
				if (IsNotDPSScreened)
				{
					UpdateScreeningStatusFromScreeningParties();
				}

				return DPSSecurityProvider.ValidateDPS(this);
			}
		}

		#endregion

		#region CanCreateInventoryCore

		protected override bool CanCreateInventoryCore
		{
			get { return true; }
		}

		#endregion

		#region ReceiveGoodsHandlingInstructions

		public ZString ReceiveGoodsHandlingInstructions => GetGoodsHandlingInstructionsNote();

		#endregion

		#region HasBeenLoadedInRF

		public bool HasBeenLoadedInRF() => Logs.Find(log => log.SL_SE_NKEvent == Events.EditedARecord.Code && log.SL_Reference.StartsWith("RF", StringComparison.CurrentCultureIgnoreCase)).Any();

		#endregion

		#region ProductCount

		protected override ZInt ProductCountCore => IEnumerableExtensions.DistinctBy(Lines.Where(rl => rl.WE_OP.IsValid), l => l.WE_OP).Count();

		#endregion

		#region VehicleNo

		[ResourceStringData("WhsReceive|VehicleNumber", Caption = "Vehicle Number", MediumCaption = "Vehicle No")]
		[ActionField(MaxLength = 25)]
		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		[MaxLength(25)]
		public ZString VehicleNo
		{
			get { return GetReference("VHN"); }
			set
			{
				SetReference("VHN", value);
				VehicleNoInfo.RefreshBinding();
			}
		}

		#endregion

		#region VehicleNoInfo

		public ZPropertyInfo VehicleNoInfo => GetZPropertyInfo(Schema.VehicleNo);

		public override void RefreshProxyProperties()
		{
			base.RefreshProxyProperties();
			VehicleNoInfo.RefreshBinding();
		}

		#endregion

		#region WD_FinalisedDate

		public override ZDateTimeOffset WD_BookingDate
		{
			get => base.WD_BookingDate;
			set
			{
				base.WD_BookingDate = value;
				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsReceive>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);
			}
		}

		public override ZDateTimeOffset WD_FinalisedDate
		{
			get => base.WD_FinalisedDate;
			set
			{
				base.WD_FinalisedDate = value;
				PopulateUnloadCompletedTime();
				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsReceive>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, PK);
				ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WD_CriticalChangesVersionID), WD_FinalisedDate.IsValid ? ConcurrencyPolicy.Strict : ConcurrencyPolicy.Ignore);
			}
		}

		void PopulateUnloadCompletedTime()
		{
			if (WD_UnloadCompletedTime.IsEmpty && WD_FinalisedDate.IsValid)
			{
				WD_UnloadCompletedTime = WD_FinalisedDate;
			}
		}

		#endregion

		#region DocketTypeSupportsAllocationKeyCore

		protected override bool DocketTypeSupportsAllocationKeyCore => true;

		#endregion

		#region SupportsDocketPlanningStatus

		protected override bool SupportsDocketPlanningStatus => true;

		#endregion

		#region LockReceiveAfterUnloadCompletedAndPutawayHoldCleared

		public bool LockReceiveAfterUnloadCompletedAndPutawayHoldCleared
			=> WD_UnloadCompletedTime.IsValid
			&& !WD_HoldPalletIDPutaway;

		#endregion

		#endregion

		#region Action Menu Functionality

		#region Receipt Splitting

		#region AutoFillSplitQuantities

		public void AutoFillSplitQuantities(IEnumerable<WhsInventoryView> selectedInventory)
		{
			PerformActionOnSelectedLines(AutoFillSplitQuantitiesCore, selectedInventory);
		}

		void AutoFillSplitQuantitiesCore(IEnumerable<WhsInventoryView> selectedInventory)
		{
			if (IsCreatedFromWorkOrder)
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromWorkOrder));
			}
			else
			{
				foreach (var whsInventory in selectedInventory)
				{
					if (whsInventory.WI_SplitQuantity == 0m)
					{
						whsInventory.WI_SplitQuantity = whsInventory.WI_InDocketLineUnits;
					}
				}
			}
		}

		#endregion

		#region SplitReceiptByQuantity

		public void SplitReceiptByQuantity()
		{
			PerformActionOnAllLines(
					() =>
					{
						if (!IsSplitQuantitySetOnAtleastOneInventoryLine)
						{
							NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.SplitByQuantityRequiresLinesWithSplitQuantities));
						}
						else if (IsSplitQuantityFullySetOnAllLines)
						{
							NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.SplitByQuantitiesFullySet));
						}
						else if (IsSplitQuantityNotDivisibleByPerPackageQty)
						{
							NotificationSubscriber.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("521a2796-86cc-4b83-8956-415f35dd2ddf",
											"This Receipt cannot be split because one or more inventory's Split Quantity is not divisible by its Per Package Quantity.")));
						}
						else if (IsAPackageGroupPartiallySplit)
						{
							NotificationSubscriber.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("fbcd2151-836f-4849-8df4-580b6d5400ca",
											"This Receipt cannot be split because one or more Package Groups are not fully split.")));
						}
						else if (HasErrors)
						{
							NotificationSubscriber.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("fc8d2bb9-7a78-4672-9070-6d60d30014d3",
											"This Receipt cannot be split because it has some errors. Please correct the errors and try again.")));
						}
						else if (HasChanges)
						{
							NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.SplitByQuantityRequiresReceiptToBeSaved));
						}
						else
						{
							SplitReceiptByQuantityCore();
						}
					});
		}

		bool IsSplitQuantityNotDivisibleByPerPackageQty
		{
			get { return Inventory.Cast<WhsInventoryView>().Any(i => i.PerPackageQty > 0m && i.WI_SplitQuantity % i.PerPackageQty != 0m); }
		}

		#region IsAPackageGroupPartiallySplit

		bool IsAPackageGroupPartiallySplit
		{
			get
			{
				var zeroSplitPackageGroupIDs = new HashSet<ZString>();
				var fullSplitPackageGroupIDs = new HashSet<ZString>();

				foreach (WhsInventoryView whsInventory in Inventory)
				{
					if (!whsInventory.PackageGroupId.IsEmpty
							 && IsPackageGroupPartiallySplit(whsInventory, zeroSplitPackageGroupIDs, fullSplitPackageGroupIDs))
					{
						return true;
					}
				}

				return false;
			}
		}

		static bool IsPackageGroupPartiallySplit(WhsInventoryView inventory, HashSet<ZString> zeroSplitPackageGroupIDs, HashSet<ZString> fullSplitPackageGroupIDs)
		{
			bool result = true;
			var packageGroupID = inventory.PackageGroupId.ToUpper();

			if (inventory.WI_SplitQuantity == 0m)
			{
				if (!fullSplitPackageGroupIDs.Contains(packageGroupID))
				{
					result = false;
					zeroSplitPackageGroupIDs.Add(packageGroupID);
				}
			}
			else if (inventory.WI_SplitQuantity == inventory.WI_InDocketLineUnits)
			{
				if (!zeroSplitPackageGroupIDs.Contains(packageGroupID))
				{
					result = false;
					fullSplitPackageGroupIDs.Add(packageGroupID);
				}
			}

			return result;
		}

		#endregion

		#region SplitReceiptByQuantityCore

		void SplitReceiptByQuantityCore()
		{
			SplitReceipt(new ReceiveSplitterByQuantity());
		}

		#endregion

		#endregion

		#region SplitReceiptByAreaType

		public void SplitReceiptByAreaType()
		{
			PerformActionOnAllLines(
					() =>
					{
						if (!IsLocationSetOnAllInventoryLines)
						{
							NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.NotEveryLineHasLocationSetForSplit));
						}
						else if (IsAreaTypeTheSameOnAllInventoryLines)
						{
							NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.SplitByAreaTypeRequiresDifferentAreaTypes));
						}
						else if (IsAPackageGroupSplitAcrossMultipleAreaTypes)
						{
							NotificationSubscriber.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("b9c685fc-476b-4f0d-9926-2b43bd81578c",
											"This Receipt cannot be split by area because one or more Package Groups will become partially split.")));
						}
						else if (HasErrors)
						{
							NotificationSubscriber.Notify(new ErrorNotification(ErrorType.Error, Res.GetString("1cac3acf-c471-4979-8bb1-946c6a7d9e90",
											"This Receipt cannot be split because it has some errors. Please correct the errors and try again.")));
						}
						else if (HasChanges)
						{
							NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.SplitByAreaTypeRequiresReceiptToBeSaved));
						}
						else
						{
							SplitReceiptByAreaTypeCore();
						}
					});
		}

		bool IsAPackageGroupSplitAcrossMultipleAreaTypes
		{
			get
			{
				var areaTypeByPackageGroup = new Dictionary<ZString, ZString>();

				foreach (WhsInventoryView whsInventory in Inventory)
				{
					var packageGroupId = whsInventory.PackageGroupId.ToUpper();
					if (!packageGroupId.IsEmpty)
					{
						var areaTypeForThisInventory = whsInventory.LocationPutawayAreaType;

						if (!areaTypeByPackageGroup.TryGetValue(packageGroupId, out var areaTypeForPackageGroupId))
						{
							areaTypeByPackageGroup[packageGroupId] = areaTypeForThisInventory;
						}
						else if (!areaTypeForThisInventory.EqualsIgnoringCase(areaTypeForPackageGroupId))
						{
							return true;
						}
					}
				}

				return false;
			}
		}

		void SplitReceiptByAreaTypeCore()
		{
			SplitReceipt(new ReceiveSplitterByAreaType());
			HasChanges = true;
		}

		void SplitReceipt(ReceiveSplitter splitter)
		{
			var result = splitter.Split(this);
			if (result.Success)
			{
				NotifyUserOfNewReceiveCreatedBySplit(splitter.NewDocketSplits);
			}
			else
			{
				NotifyUserOfErrorFromReceiptSplit(result.Message, splitter.NewDocketSplits);
			}
		}

		#endregion

		void NotifyUserOfNewReceiveCreatedBySplit(List<ZByte> splits)
		{
			NotificationSubscriber.Notify(new InfoNotification(GetNewReceiveCreatedBySplitNotification(splits).ToString()));
		}

		void NotifyUserOfErrorFromReceiptSplit(string errorMessage, List<ZByte> splits)
		{
			var message = "";
			if (splits.Count > 0)
			{
				var newReceivesCreatedMessage = GetNewReceiveCreatedBySplitNotification(splits);
				newReceivesCreatedMessage.Prepend(Res.GetString("d00710da-5f95-4ed5-b858-c280c351cd44", "The action encountered an error while performing the split: {0}", errorMessage));
				message = newReceivesCreatedMessage.ToStringWithNewLineBetweenAppends();
			}

			NotificationSubscriber.Notify(new ErrorNotification(ErrorType.Error, string.IsNullOrEmpty(message) ? errorMessage : message));
		}

		ZStringBuilder GetNewReceiveCreatedBySplitNotification(List<ZByte> splits)
		{
			var message = new ZStringBuilder();
			message.Append(splits.Count > 1
					? Res.GetString("32580be3-35b8-4ed4-8147-bd8b5303edc7", "The following new Receipts have been created as a result of this Split Action")
					: Res.GetString("052ad967-60fb-4ae0-8dc1-2135b6c5037c", "The following new Receipt has been created as a result of this Split Action"));
			message.Append("\r\n\r\n");
			foreach (ZByte split in splits)
			{
				message.Append(Res.GetString("4a2aea38-c8a3-4b11-a39d-0d6a7970407d", "Reference: {0}-{1}", WD_ExternalReference, split.ToString())).Append("\r\n");
			}
			message.Append("\r\n");
			message.Append(Res.GetString("6725a993-0d0c-42ac-b418-c4dbc235e48e", "You can open the new Receipt(s) from the Related Splits tab"));

			return message;
		}

		#endregion

		#region Palletize Lines

		public void PalletizeLines(IEnumerable<WhsReceiveLine> selectedInventory)
		{
			PerformActionOnSelectedLines(
				(whsInventory) =>
				{
					var splitter = new PalletReceiveSplitter();
					splitter.SplitLines(this, whsInventory);
				}, selectedInventory);
		}

		#endregion

		#region PalletizeLinesAndGeneratePalletIDs

		public void PalletizeLinesAndGeneratePalletIDs(IEnumerable<WhsReceiveLine> selectedLines)
		{
			PerformActionOnSelectedLines(
				lines =>
				{
					var groupedReceiveLines = lines.GroupBy(line => line.WE_OP);

					foreach (var receiveLineGroup in groupedReceiveLines)
					{
						var palletQty = Factory.Load<OrgSupplierPart>(receiveLineGroup.Key)?.OP_StockKeepingUnitPerPallet ?? 0m;
						if (palletQty > 0)
						{
							var splitter = new PalletReceiveSplitter();
							var linesNeedGeneratePalletIDs = splitter.SplitLines(this, receiveLineGroup);

							GeneratePalletIDsForPalletizedLines(linesNeedGeneratePalletIDs, palletQty);
						}
						else
						{
							GenerateSequentialPalletIDs(receiveLineGroup);
						}
					}
				}, selectedLines);
		}

		#endregion

		#region Generate Pallet IDs

		public void GenerateSequentialPalletIDs(IEnumerable<WhsReceiveLine> selectedLines)
		{
			PerformActionOnSelectedLines(lines => GeneratePalletIDsHelper.GenerateIDs(PalletIDGenerator, this, lines, (WhsDocketLine line) => line.SupplierPart != null && !line.HasErrors && !line.IsAvailable, ErrorMessage_InvalidLine), selectedLines);
		}

		void GeneratePalletIDsForPalletizedLines(IEnumerable<WhsReceiveLine> resultCollection, ZDecimal palletQty)
		{
			var receiveLinesEqualToPalletQty = new List<WhsReceiveLine>();
			var linesWithLessThanPalletQty = new SortedList<ZDecimal, List<WhsReceiveLine>>();

			foreach (var line in resultCollection.Where(l => l.WE_PalletID.IsEmpty))
			{
				if (line.WE_TransactionQuantity == palletQty)
				{
					receiveLinesEqualToPalletQty.Add(line);
				}
				else
				{
					if (linesWithLessThanPalletQty.TryGetValue(line.WE_TransactionQuantity, out var receiveLineList))
					{
						receiveLineList.Add(line);
					}
					else
					{
						linesWithLessThanPalletQty.Add(line.WE_TransactionQuantity, new List<WhsReceiveLine> { line });
					}
				}
			}

			var mergedLinesGroups = MergeLinesForLessThanPalletQty(linesWithLessThanPalletQty, palletQty);

			var idCount = receiveLinesEqualToPalletQty.Count + mergedLinesGroups.Count;

			if (idCount > 0)
			{
				var ids = PalletIDGenerator.GenerateIDs(this, idCount, shouldPrompt: true).ToArray();
				var idIndex = 0;
				foreach (var line in receiveLinesEqualToPalletQty)
				{
					line.WE_PalletID = ids[idIndex++];
				}
				foreach (var group in mergedLinesGroups)
				{
					foreach (var line in group)
					{
						line.WE_PalletID = ids[idIndex];
					}
					idIndex++;
				}
			}
		}

		List<List<WhsReceiveLine>> MergeLinesForLessThanPalletQty(SortedList<ZDecimal, List<WhsReceiveLine>> linesWithLessThanPalletQty, ZDecimal palletQty)
		{
			var palletGroups = new List<List<WhsReceiveLine>>();

			while (linesWithLessThanPalletQty.Count > 0)
			{
				var lastIndexOfLinesWithLessThanPalletQty = linesWithLessThanPalletQty.Count - 1;
				var linesWithLargestQty = linesWithLessThanPalletQty.Values[lastIndexOfLinesWithLessThanPalletQty];

				var lastIndexOfLinesWithLargestQty = linesWithLargestQty.Count - 1;
				var lineWithLargestQty = linesWithLargestQty[lastIndexOfLinesWithLargestQty];
				linesWithLargestQty.RemoveAt(lastIndexOfLinesWithLargestQty);
				if (linesWithLargestQty.Count == 0)
				{
					linesWithLessThanPalletQty.RemoveAt(lastIndexOfLinesWithLessThanPalletQty);
				}
				var palletGroup = new List<WhsReceiveLine>() { lineWithLargestQty };

				if (linesWithLessThanPalletQty.Count > 0)
				{
					if (GroupLinesIntoPalletQuantitiesAndReturnIfGroupIsFulfilled(linesWithLessThanPalletQty, palletGroup, lineWithLargestQty, palletQty))
					{
						palletGroups.Add(palletGroup);
					}
				}
				else
				{
					palletGroups.Add(palletGroup);
				}
			}

			return palletGroups;
		}

		bool GroupLinesIntoPalletQuantitiesAndReturnIfGroupIsFulfilled(
			SortedList<ZDecimal, List<WhsReceiveLine>> linesWithLessThanPalletQty,
			List<WhsReceiveLine> palletGroup,
			WhsReceiveLine lineWithLargestQty,
			ZDecimal palletQty)
		{
			var linesWithSmallestQty = linesWithLessThanPalletQty.Values[0];
			var currentQty = lineWithLargestQty.WE_TransactionQuantity;

			for (var index = linesWithSmallestQty.Count - 1; index >= 0; index--)
			{
				var lineWithSmallestQty = linesWithSmallestQty[index];
				linesWithSmallestQty.RemoveAt(index);
				if (linesWithSmallestQty.Count == 0)
				{
					linesWithLessThanPalletQty.RemoveAt(0);
				}

				if (currentQty + lineWithSmallestQty.WE_TransactionQuantity > palletQty)
				{
					var newLine = new PalletReceiveSplitter().CreateNewLineForSelectLine(this, lineWithSmallestQty, palletQty - currentQty);

					if (linesWithLessThanPalletQty.TryGetValue(newLine.WE_TransactionQuantity, out var receiveLineList))
					{
						receiveLineList.Add(newLine);
					}
					else
					{
						linesWithLessThanPalletQty.Add(newLine.WE_TransactionQuantity, new List<WhsReceiveLine> { newLine });
					}
				}
				currentQty += lineWithSmallestQty.WE_TransactionQuantity;
				palletGroup.Add(lineWithSmallestQty);

				if (currentQty == palletQty)
				{
					return true;
				}

				if (linesWithSmallestQty.Count == 0)
				{
					if (linesWithLessThanPalletQty.Count == 0)
					{
						return true;
					}
					else
					{
						linesWithSmallestQty = linesWithLessThanPalletQty.Values[0];
						index = linesWithSmallestQty.Count;
					}
				}
			}

			return false;
		}

		public void GenerateIdenticalPalletIDs(IEnumerable<WhsReceiveLine> selectedLines)
		{
			PerformActionOnSelectedLines(
				lines =>
				{
					var (id, validLines) = TryGetIdAndValidLinesForIdenticalPalletIDGeneration(lines.ToArray());

					if (id.IsEmpty)
					{
						foreach (var line in validLines)
						{
							line.AddRowWarning(GeneratePalletIDsHelper.ErrorMessage_NoMoreIDs);
						}
					}
					else
					{
						foreach (var line in validLines)
						{
							line.WE_PalletID = id;
						}
					}
				}, selectedLines);
		}

		(ZString, IEnumerable<WhsReceiveLine>) TryGetIdAndValidLinesForIdenticalPalletIDGeneration(WhsReceiveLine[] lines)
		{
			var id = ZString.Empty;
			var validLines = new List<WhsReceiveLine>(lines.Length);
			foreach (var line in lines)
			{
				if (line.SupplierPart != null && !line.HasErrors && !line.IsAvailable)
				{
					validLines.Add(line);

					if (id.IsEmpty && !line.WE_PalletID.IsEmpty)
					{
						id = line.WE_PalletID;
					}
				}
				else
				{
					line.AddRowWarning(ErrorMessage_InvalidLine);
				}
			}

			if (validLines.Count > 0)
			{
				if (id.IsEmpty)
				{
					id = PalletIDGenerator.GenerateIDs(this, numberOfIDs: 1, shouldPrompt: true).SingleOrDefault();
				}
				else
				{
					var dialogDefaultContext = new DialogDefaultContext(dialogIdentifier: new ZGuid("ee39c03c-8683-4b38-85f3-0404713771f7"),
																		caption: Res.GetString("b3e15f57-c295-4575-92f7-d6fe8d919d02", "Warning"),
																		buttons: ZMessageBoxButtons.YesNo,
																		icon: ZMessageBoxIcon.Warning,
																		context: null,
																		resultsNotToSave: new[] { ZDialogResult.No },
																		showCheckboxOnly: true);

					var message = Res.GetString("d53db5fe-d632-47f7-89e4-7afa6178b1b0", "The system will use the first Pallet ID you have selected: {0}. Do you want to proceed?", id);
					var e = new DefaultableQueryUserEventArgs(dialogDefaultContext, message, true);
					NotificationSubscriber.QueryUser(e);

					if (!e.Response)
					{
						id = ZString.Empty;
					}
				}
			}
			return (id, validLines);
		}

		ZString ErrorMessage_InvalidLine
		{
			get => Res.GetString(
				"f71882e4-95d0-4c88-8607-5cecad693da9",
				"The system has not generated a Pallet ID for this line because it does not have a product, or it has errors or it is finalized");
		}

		IPalletIDGenerator PalletIDGenerator
		{
			get => palletIDGenerator ?? (palletIDGenerator = ObjectFactory.Get<IPalletIDGenerator>());
		}

		IPalletIDGenerator palletIDGenerator;

		#endregion

		#region Clear Pallet IDs

		public void ClearPalletIDs(IEnumerable<WhsInventoryView> selectedInventory)
		{
			PerformActionOnSelectedLines(
					lines =>
					{
						lines.ForEach(l => l.WI_PalletID = ZString.Empty);
					}, selectedInventory);
		}

		public void ClearPalletIDs(IEnumerable<WhsReceiveLine> selectedLines)
		{
			PerformActionOnSelectedLines(
					lines =>
					{
						lines.ForEach(l => l.WE_PalletID = ZString.Empty);
					}, selectedLines);
		}

		#endregion

		#region Change Hold Code

		#region Change Hold Code For Product

		public void ChangeHoldCodeForProduct(IEnumerable<WhsReceiveLine> selectedLines)
		{
			PerformActionOnSelectedLines(ChangeHoldCodeForProductCore, selectedLines);
		}

		void ChangeHoldCodeForProductCore(IEnumerable<WhsReceiveLine> selectedLines)
		{
			if (AllowToChangeHoldCodeOnSelectedLines(selectedLines))
			{
				selectedLines.GroupBy(item => item.WE_OP).ForEach(ChangeHoldCodeCore);
			}
		}

		#endregion

		#region Change Hold Code For Receipt

		public void ChangeHoldCodeForReceipt()
		{
			PerformActionOnSelectedLines(ChangeHoldCodeForReceiptCore, Lines.Cast<WhsReceiveLine>());
		}

		void ChangeHoldCodeForReceiptCore(IEnumerable<WhsReceiveLine> lines)
		{
			if (AllowToChangeHoldCodeOnSelectedLines(lines))
			{
				ChangeHoldCodeCore(lines);
			}
		}

		#endregion

		#region Clear Hold Codes

		public void ClearHoldCodes(IEnumerable<WhsReceiveLine> selectedLines)
		{
			PerformActionOnSelectedLines(ClearHoldCodesCore, selectedLines);
		}

		void ClearHoldCodesCore(IEnumerable<WhsReceiveLine> selectedLines)
		{
			if (AllowToChangeHoldCodeOnSelectedLines(selectedLines))
			{
				selectedLines.ForEach(l => l.WE_WHC_NKOriginalInventoryHeldCode = ZString.Empty);
			}
		}

		#endregion

		bool AllowToChangeHoldCodeOnSelectedLines(IEnumerable<WhsReceiveLine> lines)
		{
			Argument.NotNull(lines, "selectedLines");

			var result = true;
			var validLines = lines.Where(IsOkToChangeHoldCode).ToList();
			if (validLines.Count != lines.Count())
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.NoValidLines));
				result = false;
			}

			return result;
		}

		void ChangeHoldCodeCore(IEnumerable<WhsReceiveLine> lines)
		{
			var firstValidLine = lines.FirstOrDefault(line => !line.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty);
			if (firstValidLine == null)
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.NoLinesWithHoldCode));
			}
			else
			{
				lines.ForEach(line => line.WE_WHC_NKOriginalInventoryHeldCode = firstValidLine.WE_WHC_NKOriginalInventoryHeldCode);
			}
		}

		bool IsOkToChangeHoldCode(WhsReceiveLine line) => line.SupplierPart != null && !line.HasErrors && !line.WE_WHC_NKOriginalInventoryHeldCodeInfo.ReadOnly;

		#endregion

		#region Generate Serial Numbers

		public void GenerateSerialNumbers(IEnumerable<WhsReceiveLine> selectedLines)
		{
			PerformActionOnSelectedLines(
				lines =>
				{
					var generator = new SerialNumberGenerator();
					generator.Generate(this, lines);
				}, selectedLines);
		}

		#endregion

		#region Create Product Files

		public void CreateProductFiles()
		{
			if (Env.Security.WhsReceiveCreateProductFiles.IsAllowed || IsImportingData)
			{
				// For every Temporary product find list of inventories that use it.
				var lines = CreateProductFilesList();

				foreach (var receiveLineList in lines.Values)
				{
					if (receiveLineList != null && receiveLineList.Count > 0)
					{
						CreateNewProduct(receiveLineList);
					}
				}
			}
		}

		#region CreateProductFilesList

		Dictionary<ZString, List<WhsReceiveLine>> CreateProductFilesList()
		{
			var result = new Dictionary<ZString, List<WhsReceiveLine>>();

			foreach (WhsReceiveLine line in Lines)
			{
				if (line.IsTemporaryProduct)
				{
					AddLineToCreateProductFileList(result, line);
				}
			}

			return result;
		}

		void AddLineToCreateProductFileList(Dictionary<ZString, List<WhsReceiveLine>> lines, WhsReceiveLine receiveLine)
		{
			List<WhsReceiveLine> listedReceiveLine;
			if (!lines.TryGetValue(receiveLine.ProductCode, out listedReceiveLine))
			{
				listedReceiveLine = new List<WhsReceiveLine>();
				lines.Add(receiveLine.ProductCode, listedReceiveLine);
			}
			listedReceiveLine.Add(receiveLine);
		}

		#endregion

		#region CreateNewProduct

		void CreateNewProduct(List<WhsReceiveLine> receiveLineList)
		{
			if (IsNewProductDataCorrect(receiveLineList, out var isPartAttrib1Used, out var isPartAttrib2Used, out var isPartAttrib3Used, out var isSerialUsed, out var isExpiryDateUsed, out var isPackingDateUsed))
			{
				var newProduct = Factory.LoadTop1<OrgSupplierPart>(new ZQuery(OrgSupplierPartSchema.OP_PartNum, receiveLineList[0].ProductCode)); // if new product was created in other factory.
				if (newProduct == null)
				{
					newProduct = receiveLineList[0].Lookups.SupplierParts.AddNew();
					receiveLineList[0].SetupSupplierPart(newProduct);
					SetNewProductAttributesUse(newProduct, isPartAttrib1Used, isPartAttrib2Used, isPartAttrib3Used, isSerialUsed, isExpiryDateUsed, isPackingDateUsed); // need to setup from all inventory lines not just the first one.
				}

				AssignNewProductForAllRelatedInventory(receiveLineList, newProduct);

				if (IsImportingData)
				{
					CreateOrUpdateOrgPartRelation(isPartAttrib1Used, isPartAttrib2Used, isPartAttrib3Used, isSerialUsed, isExpiryDateUsed, isPackingDateUsed, newProduct);
				}
			}
		}

		#region CreateOrUpdateOrgPartRelation

		void CreateOrUpdateOrgPartRelation(bool isPartAttrib1Used, bool isPartAttrib2Used, bool isPartAttrib3Used, bool isSerialUsed, bool isExpiryDateUsed, bool isPackingDateUsed, OrgSupplierPart newProduct)
		{
			var relations = newProduct.RelatedOrganisations.Cast<OrgPartRelation>().Where(r => r.OU_OH == Client.PK);
			if (!relations.Any() || relations.All(r => r.OU_Relationship == OrgPartRelation.RelationshipTypes.WarehouseConsignee))
			{
				CreateOwnerRelationForClient(isPartAttrib1Used, isPartAttrib2Used, isPartAttrib3Used, isSerialUsed, isExpiryDateUsed, isPackingDateUsed, newProduct);
			}
			else if (!relations.Any(r => r.IsOwner))
			{
				var relation = relations.FirstOrDefault(r => r.OU_Relationship == OrgPartRelation.RelationshipTypes.Supplier);
				if (relation != null)
				{
					relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
					SetProductAttributesUse(relation, isPartAttrib1Used, isPartAttrib2Used, isPartAttrib3Used, isSerialUsed, isExpiryDateUsed, isPackingDateUsed);
				}
			}
		}

		void CreateOwnerRelationForClient(bool isPartAttrib1Used, bool isPartAttrib2Used, bool isPartAttrib3Used, bool isSerialUsed, bool isExpiryDateUsed, bool isPackingDateUsed, OrgSupplierPart newProduct)
		{
			var newRelation = newProduct.RelatedOrganisations.AddOwner(Client);
			SetProductAttributesUse(newRelation, isPartAttrib1Used, isPartAttrib2Used, isPartAttrib3Used, isSerialUsed, isExpiryDateUsed, isPackingDateUsed);
		}

		#endregion

		#region IsNewProductDataCorrect

		bool IsNewProductDataCorrect(List<WhsReceiveLine> receiveLineList, out bool isPartAttrib1Used, out bool isPartAttrib2Used, out bool isPartAttrib3Used, out bool isSerialUsed, out bool isExpiryDateUsed, out bool isPackingDateUsed)
		{
			var canCreateProduct = true;
			isPartAttrib1Used = isPartAttrib2Used = isPartAttrib3Used = isSerialUsed = isExpiryDateUsed = isPackingDateUsed = false;

			var firstReceiveLine = receiveLineList[0];
			foreach (var receiveLine in receiveLineList)
			{
				// if any mandatory data NOT entered or any mandatory fields does not mutch for all collection, then do NOT auto-create product.
				if (receiveLine.ProductCode.IsEmpty || receiveLine.ProductCode != firstReceiveLine.ProductCode ||
						 receiveLine.ProductDesc.IsEmpty || receiveLine.ProductDesc != firstReceiveLine.ProductDesc ||
						 receiveLine.ProductUQ.IsEmpty || receiveLine.ProductUQ != firstReceiveLine.ProductUQ || receiveLine.ProductUQInfo.HasErrors() ||
						 receiveLine.CommodityCode != firstReceiveLine.CommodityCode || receiveLine.CommodityCodeInfo.HasErrors())
				{
					canCreateProduct = false;
					break;
				}
				else
				{
					isPartAttrib1Used = isPartAttrib1Used || !receiveLine.WE_PartAttrib1.IsEmpty;
					isPartAttrib2Used = isPartAttrib2Used || !receiveLine.WE_PartAttrib2.IsEmpty;
					isPartAttrib3Used = isPartAttrib3Used || !receiveLine.WE_PartAttrib3.IsEmpty;
					isExpiryDateUsed = isExpiryDateUsed || !receiveLine.WE_ExpiryDate.IsEmpty;
					isPackingDateUsed = isPackingDateUsed || !receiveLine.WE_PackingDate.IsEmpty;
					isSerialUsed = isSerialUsed || !receiveLine.WE_SerialNumber.IsEmpty;
				}
			}

			return canCreateProduct;
		}

		#endregion

		#region SetNewProductAttributesUse

		void SetNewProductAttributesUse(OrgSupplierPart newProduct, bool isPartAttrib1Used, bool isPartAttrib2Used, bool isPartAttrib3Used, bool isSerialUsed, bool isExpiryDateUsed, bool isPackingDateUsed)
		{
			foreach (OrgPartRelation relatedOrganisation in newProduct.RelatedOrganisations)
			{
				SetProductAttributesUse(relatedOrganisation, isPartAttrib1Used, isPartAttrib2Used, isPartAttrib3Used, isSerialUsed, isExpiryDateUsed, isPackingDateUsed);
			}
		}

		void SetProductAttributesUse(OrgPartRelation relation, bool isPartAttrib1Used, bool isPartAttrib2Used, bool isPartAttrib3Used, bool isSerialUsed, bool isExpiryDateUsed, bool isPackingDateUsed)
		{
			if (relation != null)
			{
				var miscServ = Client.MiscServ;
				relation.OU_UsePartAttrib1 = isPartAttrib1Used && !miscServ.OM_IMPartAttrib1Type.IsEmpty;
				relation.OU_UsePartAttrib2 = isPartAttrib2Used && !miscServ.OM_IMPartAttrib2Type.IsEmpty;
				relation.OU_UsePartAttrib3 = isPartAttrib3Used && !miscServ.OM_IMPartAttrib3Type.IsEmpty;
				relation.OU_UseSerialNumber = isSerialUsed && miscServ.OM_IMUseSerialNumber;
				relation.OU_UseExpiryDate = isExpiryDateUsed && miscServ.OM_IMUseExpiryDate;
				relation.OU_UsePackingDate = isPackingDateUsed && miscServ.OM_IMUsePackingDate;
				if (isPartAttrib1Used || isPartAttrib2Used || isPartAttrib3Used || isSerialUsed)
				{
					relation.OU_PickMode = Client.MiscServ.OM_WhsDefaultWarehousePickMode;
				}
			}
		}

		#endregion

		#region AssignNewProductForAllRelatedInventory

		void AssignNewProductForAllRelatedInventory(List<WhsReceiveLine> receiveLineList, OrgSupplierPart newProduct)
		{
			foreach (var receive in receiveLineList)
			{
				receive.WE_OP = newProduct.PK;
			}
		}

		#endregion

		#endregion

		#endregion

		#region Populate ASN Lines

		public void PopulateASNLines()
		{
#if DEBUG
			PopulateASNHasBeenCalled_TestsOnly = true;
#endif
			if (IsCreatedFromPickByBOM)
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromPickOrder));
			}
			else if (IsFinalisedOrCancelled)
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			}
			else if (HasChanges)
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CreateASNLineRequiresReceiptToBeSaved));
			}
			else if (IsCreatedFromWorkOrder)
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromWorkOrder));
			}
			else if (IsCustomsTransaction && !Warehouse.WW_IsVirtualWarehouse && IsDocketHeldByCustoms)
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseDocketIsHeldByCustoms));
			}
			else
			{
				WD_StartedReceivingTimeUtc = ZDateTime.UtcNow;
				if (this.AsnLines.Count == 0)
				{
					PopulateASNLinesCore();
				}
			}
		}

#if DEBUG
		public bool PopulateASNHasBeenCalled_TestsOnly;
#endif

		void PopulateASNLinesCore()
		{
			var registryEnabled = new Lazy<bool>(() => WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value);

			foreach (var docketLine in Lines.Cast<WhsReceiveLine>().Where(i => i.WE_OP.IsValid))
			{
				var asnLine = this.AsnLines.Add(docketLine);

				if (registryEnabled.Value)
				{
					foreach (var serialNumber in docketLine.SerialNumbers)
					{
						var pivot = asnLine.SerialNumbers.AddNew();
						pivot.WSV_WSN_SerialNumber = serialNumber.WSV_WSN_SerialNumber;
					}
				}
			}
		}

		#endregion

		#region Reconcile ASN

		internal void ReconcileASN()
		{
#if DEBUG
			ReconcileASNHasBeenCalled_TestsOnly = true;
#endif
			if (!IsFinalisedOrCancelled)
			{
				if (HasErrorsIncludingLines)
				{
					NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CalculateOversAndUndersRequiresReceiptToHaveNoErrors));
				}
				else
				{
					WhsReceiveASNHelper.ReconcileASN(this);
				}
			}
			else
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			}
		}

#if DEBUG
		public bool ReconcileASNHasBeenCalled_TestsOnly;
#endif

		#endregion

		#region Putaway

		public void ClearLocations()
		{
			ClearLocations(Lines.Cast<WhsReceiveLine>());
		}

		public void ClearLocations(IEnumerable<WhsReceiveLine> selectedInventory)
		{
			PerformActionOnSelectedLines(
					lines =>
					{
						lines.ForEach(l => l.WE_WL = ZGuid.Empty);
					}, selectedInventory);
		}

		#endregion

		#region Update Actual Quantity with Expected Quantity

		public void UpdateActualQuantityWithExpectedQuantity(IEnumerable<WhsReceiveLine> selectedReceiveLines)
		{
			PerformActionOnSelectedLines(
					lines =>
					{
						lines.Where(l => l.WE_TransactionQuantity == 0m).ForEach(l => l.WE_TransactionQuantity = l.WE_ClientOrderedUnits);
					}, selectedReceiveLines);
		}

		#endregion

		#region SetUnloadCompleteTimeAndCloseOrCancelActiveUnloadTasksIfPossible

		public void SetUnloadCompleteTimeAndCloseOrCancelActiveUnloadTasksIfPossible()
		{
			if (!WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned))
			{
				WD_UnloadCompletedTime = DateTimeOffset.Now;
			}
			else
			{
				var unloadTasks = UnloadTaskManagementTasks;
				if (unloadTasks.Where(t => t.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working)).IsCountEqualTo(0) || CloseActiveWorkingUnloadTasksConfirmationNotification())
				{
					CloseOrCancelActiveTasks(unloadTasks);
					WD_UnloadCompletedTime = DateTimeOffset.Now;
				}
			}
		}

		void CloseOrCancelActiveTasks(ProcessTask[] taskManagementUnloadJobTasks)
		{
			foreach (var task in taskManagementUnloadJobTasks)
			{
				if (task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working) || task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Suspended))
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				}
				else if (task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Assigned) || task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Open))
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
				}
			}
		}

		#endregion

		#endregion

		#region Finalisation

		protected override bool FinaliseDocketCore()
		{
			ReconcileASN();

			foreach (WhsReceiveLine line in Lines)
			{
				var putawayTransferLine = line.PutawayTransferLine;
				if (putawayTransferLine != null)
				{
					putawayTransferLine.WE_CurrentInventoryStatus = putawayTransferLine.WE_WHC_NKCurrentInventoryHeldCode.IsEmpty
						? InventoryStatus.Codes.Available
						: InventoryStatus.Codes.Held;
				}
				else
				{
					line.WE_OriginalInventoryStatus = line.WE_WHC_NKOriginalInventoryHeldCode.IsEmpty
						? InventoryStatus.Codes.Available
						: InventoryStatus.Codes.Held;
				}

				// Keep arrival date on over-picked Pick by BOM components, those lines are created in Pick Finalisation stage.
				if (!IsCreatedFromPickByBOM || line.Product.IsBOMProductPickedOnSalesOrder)
				{
					line.WE_AdjustmentArrivalDate = WD_ArrivalDate;
				}

				if (!line.WE_CurrentHoldReason.IsEmpty)
				{
					line.SetSystemDefinedValue(nameof(WhsReceiveLine.OriginalHoldReason), line.WE_CurrentHoldReason);
				}
			}

			if (WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned))
			{
				CloseOrCancelActiveTasks(UnloadTaskManagementTasks);
				CloseOrCancelActiveTasks(PutawayTaskManagementTasks);
			}

			FinaliseCompletedAssociatedPutawayJobs();
			return !HasErrors;
		}

		void FinaliseCompletedAssociatedPutawayJobs()
		{
			if (!HasErrorsIncludingLines && Lines.Any(line => !line.WE_PalletID.IsEmpty))
			{
				var palletIDs = Lines.Select(l => l.WE_PalletID).Distinct().ToArray();
				var finalisedPutawayJobsFromDB = FinalisePutawayJobsInDB(palletIDs);
				FinalisePutawayJobsFromLocalCache(palletIDs, finalisedPutawayJobsFromDB);
			}
		}

		IEnumerable<WhsPutawayJob> FinalisePutawayJobsInDB(IEnumerable<ZString> palletIDs)
		{
			var query = $@"
WPJ_PK IN
(
	SELECT
		WPJ_PK
	FROM
		dbo.WhsPutawayJob
		JOIN dbo.WhsPutawayLine ON WPL_WPJ_PutawayJob = WPJ_PK
	WHERE (1=1)
		AND WPJ_FinalizedTimeUtc IS NULL
		AND WPJ_WW_Warehouse = @WhsPK
		AND WPL_PalletID IN (SELECT VALUE FROM @PalletIDs)
		AND WPL_IsPuttingAway = 0
		AND NOT EXISTS
		(
			SELECT
				NULL
			FROM
				dbo.WhsPutawayLine
				JOIN dbo.WhsDocketLine ON WE_PalletID = WPL_PalletID
				JOIN dbo.WhsDocket ON WE_WD = WD_PK
			WHERE (1=1)
				AND WPL_WPJ_PutawayJob = WPJ_PK
				AND WD_IsPutawayTransfer = 1
				AND WD_WW_Whs = @WhsPK
				AND WE_PalletID <> ''
				AND WE_DocketLineType = 'TFR'
				AND WE_DocketLineStatus != '{DocketStatus.Codes.Finalised}'
				AND WE_DocketLineStatus != '{DocketStatus.Codes.Cancelled}'
		)
)
";
			var parameters = new ZSqlParameterCollection();
			parameters.Add(ZSqlParameter.New("@PalletIDs", palletIDs.ToArray(), WhsPutawayLineSchema.WPL_PalletID, isTableValued: true));
			parameters.Add("@WhsPK", WD_WW_Whs, WhsDocketSchema.WD_WW_Whs);

			var jobQuery = new ZDBOnlyQuery(typeof(WhsPutawayJob));
			jobQuery.AddFilterAndZSQLParameterCollection(query, parameters);

			var putawayJobs = Factory.Load<WhsPutawayJob>(jobQuery);
			putawayJobs.SelectMany(j => j.Lines).ForEach(l => l.WPL_IsFinalized = true);
			putawayJobs.ForEach(p => p.WPJ_FinalizedTimeUtc = ZDateTime.UtcNow);

			return putawayJobs;
		}

		void FinalisePutawayJobsFromLocalCache(IEnumerable<ZString> palletIDs, IEnumerable<WhsPutawayJob> finalisedPutawayJobs)
		{
			var localLineQuery = new ZQuery(WhsPutawayLineSchema.WPL_PalletID, palletIDs);
			localLineQuery.AddToFilter(WhsPutawayLineSchema.WPL_IsPuttingAway, false);
			localLineQuery.FetchOnlyFromLocalCache = true;
			var localPutawayLines = Factory.Load<WhsPutawayLine>(localLineQuery);

			var localJobQuery = new ZQuery(WhsPutawayJobSchema.WPJ_FinalizedTimeUtc, null);
			localJobQuery.AddToFilter(WhsPutawayJobSchema.WPJ_WW_Warehouse, WD_WW_Whs);
			localJobQuery.AddToFilter(WhsPutawayJobSchema.PK, localPutawayLines.Select(l => l.WPL_WPJ_PutawayJob));
			localJobQuery.AddToFilter(WhsPutawayJobSchema.PK, SQLComparisonOperator.NotEqual, finalisedPutawayJobs.Select(l => l.PK));
			localJobQuery.FetchOnlyFromLocalCache = true;
			var localPutawayJobs = Factory.Load<WhsPutawayJob>(localJobQuery);

			var putawayLinesQuery = new ZQuery(WhsPutawayLineSchema.WPL_WPJ_PutawayJob, localPutawayJobs.Select(j => j.PK));
			var putawayLines = Factory.Load<WhsPutawayLine>(putawayLinesQuery);

			var palletIDsOnLocalUnfinalisedPutawayJobs = putawayLines.Select(l => l.WPL_PalletID).ToArray();
			var putawayTransferLineQuery = new ZQuery(WhsDocketLineSchema.WE_DocketLineType, DocketType.Codes.Transfer);
			putawayTransferLineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketStatus.Codes.Finalised);
			putawayTransferLineQuery.AddToFilter(WhsDocketLineSchema.WE_TransferFromPalletId, palletIDsOnLocalUnfinalisedPutawayJobs);
			putawayTransferLineQuery.AddToFilter(WhsDocketLineSchema.WE_TransferFromPalletId, SQLComparisonOperator.NotEqual, "");
			var putawayTransferLines = Factory.Load<WhsDocketLine>(putawayTransferLineQuery);

			var putawayTransferQuery = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
			putawayTransferQuery.AddToFilter(WhsDocketSchema.WD_IsPutawayTransfer, true);
			putawayTransferQuery.AddToFilter(WhsDocketSchema.PK, putawayTransferLines.Select(line => line.WE_WD).Distinct());
			var putawayTransferPKs = Factory.Load<WhsDocket>(putawayTransferQuery).Select(d => d.PK).ToHashSet();

			var unputawayPalletIDs = putawayTransferLines.Where(u => putawayTransferPKs.Contains(u.WE_WD)).Select(u => u.WE_PalletID).ToHashSet();
			var palletIDsPerJob = putawayLines.GroupBy(pj => pj.WPL_WPJ_PutawayJob);
			var finalisablePutawayJobs = palletIDsPerJob.Where(o => o.All(pl => !unputawayPalletIDs.Contains(pl.WPL_PalletID))).Select(o => o.Key).ToHashSet();

			var localJobsToFinalise = localPutawayJobs.Where(p => finalisablePutawayJobs.Contains(p.PK)).ToArray();
			localJobsToFinalise.SelectMany(j => j.Lines).ForEach(l => l.WPL_IsFinalized = true);
			localJobsToFinalise.ForEach(p => p.WPJ_FinalizedTimeUtc = ZDateTime.UtcNow);
		}

		public static string CannotPutawayIntoDifferentAreaTypesErrorMsg
			=> Res.GetString("18035b52-812c-49a0-895f-25c0998166fd", "Cannot putaway stock into different area types. You can use the Split By Area Type action to generate a new Receipt for each Area Type.");

		public static string ContainsUnfinalizedPutawayTransfersErrorMessage
			=> Res.GetString("97774064-efcf-4302-91d4-33d66cd01e16", "This Receive cannot be finalized because the Putaway Transfer is not finalized.");

		#region FinaliseConfirmationNotification

		protected override ZGuid FinaliseConfirmationDialogIdentifier => new ZGuid("b7b3022a-a00d-4a90-bc5a-5759f9ceccac");

		protected override string FinaliseConfirmationMessage
		{
			get
			{
				var result = Res.GetString("5dd38865-6cdb-44bd-8505-bfb2d8d14a98", "Finalizing this Receipt will update the inventory, making it available for Picking / Release.\r\nOn finalization, this Receipt will become read-only so that it cannot be modified.\r\nThis finalization process cannot be undone once saved.") + "\r\n\r\n";
				if (HasWarnings)
				{
					result += Res.GetString("2403647e-433d-4472-8051-c3eae54c0109", "Note: There are warnings. Click Cancel to review these warnings before finalizing.") + "\r\n\r\n";
				}

				result += Res.GetString("c9445f3f-62b9-4d0f-ab70-9d08a3f79013", "Do you wish to Finalize this Receipt?");
				return result;
			}
		}

		#endregion

		protected override bool CanFinaliseWithActiveWorkingTasks(bool withUserConfirmations)
			=> !WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned)
			|| (ConfirmFinaliseWithActiveTasksOnPlay(UnloadTaskManagementTasks, withUserConfirmations, CloseActiveWorkingUnloadTasksConfirmationNotification)
				&& ConfirmFinaliseWithActiveTasksOnPlay(PutawayTaskManagementTasks, withUserConfirmations, CloseActiveWorkingPutawayTasksConfirmationNotification));

		bool ConfirmFinaliseWithActiveTasksOnPlay(ProcessTask[] processTasks, bool withUserConfirmations, Func<bool> confirmFinaliseNotification)
			=> processTasks.Where(t => t.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working) && !t.P9_GS_NKAssignedStaffMember.EqualsIgnoringCase(Env.CurrentUser.Initials)).IsCountEqualTo(0) || (withUserConfirmations && confirmFinaliseNotification());

		#region CloseActiveWorkingUnloadTasksConfirmationNotification

		bool CloseActiveWorkingUnloadTasksConfirmationNotification()
		{
			var dialogDefaultContext = new DialogDefaultContext(dialogIdentifier: CompleteUnloadWithActiveWorkingUnloadTasksConfirmationDialogIdentifier,
				caption: Res.GetString("e8dfccaf-8659-4d91-9d63-848a3cc8a083", "Completing unload with active unload tasks set to working confirmation"),
				buttons: ZMessageBoxButtons.YesNoCancel,
				icon: ZMessageBoxIcon.Warning,
				context: null,
				resultsNotToSave: new[] { ZDialogResult.No, ZDialogResult.Cancel },
				showCheckboxOnly: true);

			var e = new DefaultableQueryUserEventArgs(dialogDefaultContext, CompleteUnloadWithActiveWorkingUnloadTasksConfirmationMessage, false);
			NotificationSubscriber.QueryUser(e);
			return e.Response;
		}

		static ZGuid CompleteUnloadWithActiveWorkingUnloadTasksConfirmationDialogIdentifier => new ZGuid("bc9f8a97-d8d6-4127-ae0d-09d6862d508d");

		static string CompleteUnloadWithActiveWorkingUnloadTasksConfirmationMessage => Res.GetString("e1d85cfe-53a2-4357-b1b7-db4421521233", @"This Receipt has active unload job tasks set to working. The tasks need to be completed to complete the unload for this Receipt.

Do you wish to close these active tasks set to working to complete the unload for this Receipt?");

		#endregion

		#region CloseActiveWorkingPutawayTasksConfirmationNotification

		bool CloseActiveWorkingPutawayTasksConfirmationNotification()
		{
			var dialogDefaultContext = new DialogDefaultContext(dialogIdentifier: FinaliseWithActiveWorkingPutawayTasksConifrmationDialogIdentifier,
				caption: Res.GetString("e0b5d7d2-f85e-4496-96df-7a8682f23798", "Finalizing receive with active putaway tasks set to working confirmation"),
				buttons: ZMessageBoxButtons.YesNoCancel,
				icon: ZMessageBoxIcon.Warning,
				context: null,
				resultsNotToSave: new[] { ZDialogResult.No, ZDialogResult.Cancel },
				showCheckboxOnly: true);

			var e = new DefaultableQueryUserEventArgs(dialogDefaultContext, FinaliseWithActiveWorkingPutawayTasksConifrmationMessage, false);
			NotificationSubscriber.QueryUser(e);
			return e.Response;
		}

		static ZGuid FinaliseWithActiveWorkingPutawayTasksConifrmationDialogIdentifier => new ZGuid("5d4f7686-3221-49f7-97a5-0bc9b7dfe651");

		static string FinaliseWithActiveWorkingPutawayTasksConifrmationMessage => Res.GetString("493e1cd6-19fd-46dc-8321-1c9336f818cc", @"This Receipt has related active putaway job tasks set to working. The tasks need to be completed to finalize this Receipt.

Do you wish to close these active tasks set to working to finalize this Receipt?");

		#endregion

		protected override ZDateTimeOffset GetFinalisedDate() => Provider?.GetFinalisationTimeOffset() ?? GetFinalisedDateCore();

		ZDateTimeOffset GetFinalisedDateCore()
		{
			var warehouse = Warehouse;
			return warehouse != null && warehouse.WW_UseArrivalDateForInwardsFinalisedDate ? WD_ArrivalDate : base.GetFinalisedDate();
		}

		internal IFinalisedDateProvider GetFinalisedDateProvider()
		{
			if (Provider != null && !IsFinalising)
			{
				throw new InvalidOperationException("Attempt to call GetFinalisedDateProvider() without finalising the Receive.");
			}

			return Provider;
		}

		internal IDisposable SetFinalisedDateProvider(IFinalisedDateProvider provider)
		{
			Argument.NotNull(provider, nameof(provider));

			if (!IsCreatedFromPickByBOM)
			{
				throw new InvalidOperationException("Can only use a IFinalisedDateProvider for Receives created from Pick by BOM.");
			}

			return new DisposableAction(
				() => Provider = provider,
				() => Provider = null);
		}

		IFinalisedDateProvider Provider;

		protected override void OnFinaliseSucceeded()
		{
			base.OnFinaliseSucceeded();

			WD_TaskPlanningStatus = string.Empty;
			WD_HoldPalletIDPutaway = false;

			var order = ComponentOrder?.ParentDocket as WhsOrder;
			if (order != null && order.IsAttachedToPickButNotFinalised && order.Pick.OrderedInventories.Count > 0)
			{
				order.Pick.ClearInventoryCache();

				// if inventories for this receive will be reloaded from DB at this stage, they will have status = 'PUT', as the
				// finalised docket line status has not been saved yet. We have to override it back to 'AVL' before trying to allocate again.
				PokeAllInventories(order);
				foreach (WhsInventoryView whsInventory in Inventory)
				{
					whsInventory.WI_InventoryStatus = whsInventory.InDocketLine.WE_CurrentInventoryStatus;
				}

				order.Pick.AutoAllocateItems(NotificationSubscriber);

				NotificationSubscriber.Notify(new InfoNotification(Res.GetString("45706741-3d13-4f69-8fcf-7875ac573c64",
						 "Stock received was automatically allocated to Order {0}.", order.WD_ExternalReference)));
			}
		}

		IEnumerable<IWhsInventoryView> PokeAllInventories(WhsOrder order) => order.Pick.AllInventories;

		protected override bool AddFetchHintsForInventory
		{
			get { return true; }
		}

		protected override void AddFetchHintsForPreFinaliseValidation_Core()
		{
			base.AddFetchHintsForPreFinaliseValidation_Core();

			var pksForSerialNumberFetchHints = new List<ZGuid>();
			Lines.Cast<WhsReceiveLine>().ForEach(rl =>
			{
				rl.Inventory.Cast<WhsInventoryView>().ForEach(i => { Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, i.WI_WE_InDocketLine); });
				if (IsInDatabase && WarehouseDataRegistry.Instance.EnableSchemaRedesignChanges.Value && rl.IsSerialNumberUsed)
				{
					pksForSerialNumberFetchHints.Add(rl.PK);
				}
			});

			if (pksForSerialNumberFetchHints.Count > 0)
			{
				WhsSerialNumberHelper.AddWhsSerialNumberAndPivotFetchHint(Factory, pksForSerialNumberFetchHints);
			}
		}

		protected override IDisposable GetFinalisationDataCache()
		{
			return new DisposableList(new[]
			{
				base.GetFinalisationDataCache(),
				UseReceiveFinalisationDataCache()
			});
		}

		IDisposable UseReceiveFinalisationDataCache()
		{
			return new DisposableAction(
				() =>
				{
					unloadTaskManagementTasks = GetUnloadTaskManagementTasks();
					putawayTransferTaskManagementTasks = GetPutawayTransferTaskManagementTasks();
				},
				() =>
				{
					unloadTaskManagementTasks = null;
					putawayTransferTaskManagementTasks = null;
				});
		}

		ProcessTask[] UnloadTaskManagementTasks => unloadTaskManagementTasks ?? GetUnloadTaskManagementTasks();
		ProcessTask[] unloadTaskManagementTasks;

		ProcessTask[] GetUnloadTaskManagementTasks()
		{
			var unloadJobTasksQuery = new ZQuery(ProcessTasksSchema.P9_ParentID, PK);
			unloadJobTasksQuery.AddToFilter(ProcessTasksSchema.P9_FormFlowType, WarehouseTaskFormFlowTypes.UnloadJob);
			return Factory.Load<ProcessTask>(unloadJobTasksQuery);
		}

		ProcessTask[] PutawayTaskManagementTasks => putawayTransferTaskManagementTasks ?? GetPutawayTransferTaskManagementTasks();
		ProcessTask[] putawayTransferTaskManagementTasks;

		ProcessTask[] GetPutawayTransferTaskManagementTasks()
		{
			var putawayTransferPKs = Lines
				.Cast<WhsReceiveLine>()
				.Select(line => line.PutawayTransferLine?.WE_WD ?? ZGuid.Empty)
				.Where(id => !id.IsEmpty)
				.ToHashSet();

			var putawayTasks = Array.Empty<ProcessTask>();
			if (putawayTransferPKs.Count > 0)
			{
				var putawayJobTasksQuery = new ZQuery(ProcessTasksSchema.P9_ParentID, putawayTransferPKs);
				putawayJobTasksQuery.AddToFilter(ProcessTasksSchema.P9_FormFlowType, WarehouseTaskFormFlowTypes.PutawayJob);
				putawayTasks = Factory.Load<ProcessTask>(putawayJobTasksQuery);
			}

			return putawayTasks;
		}

		#endregion

		#region Cancellation

		static string CantCancelPutawayTransferReasonMsg
		{
			get { return Res.GetString("c7f28462-847c-4d77-946a-3315c4322eba", "You cannot cancel receives that have an active putaway transfer."); }
		}

		protected override string CanCancelCore()
		{
			var result = base.CanCancelCore();

			if (result == ZString.Empty)
			{
				if (IsCreatedFromWorkOrder)
				{
					result = Res.GetString("7185e168-470d-4fa4-b252-bbbb40e2f47a", "You cannot cancel receives created from Work Orders.");
				}
				else if (IsCreatedFromPickByBOM)
				{
					result = Res.GetString("54a35b24-0f38-4657-b970-585c3a8e44b0", "You cannot cancel receives created from Pick Orders.");
				}
				else
				{
					result = Lines.Cast<WhsReceiveLine>().Any(line => !line.PutawayTransfer?.IsFinalised ?? false) ? CantCancelPutawayTransferReasonMsg : string.Empty;
				}
			}

			return result;
		}

		#endregion

		#region Reactivation

		protected override string ReactivedDocketLineDefaultStatus(WhsDocketLine line)
		{
			var result = base.ReactivedDocketLineDefaultStatus(line);
			var receiveLine = (WhsReceiveLine)line;
			if (receiveLine?.PutawayTransferLine?.IsPicked ?? false)
			{
				result = DocketLineStatus.Codes.PickedForUnload;
			}
			return result;
		}

		#endregion

		#region Workflow Template

		#region GetTemplateSelectionCriteria

		protected override void GetTemplateSelectionCriteriaCore(ColumnValueRanker result)
		{
			result.Add(ProcessTaskTemplateSchema.P0_SubType1, WD_ReceiveCategory, ZString.Empty);
		}

		#endregion

		#endregion

		#region RelatedOrgPartyScreeningStatus Collection

		public IRelatedOrgPartyScreeningStatusCollection RelatedOrgPartyScreeningStatusCollection
			=> RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, ((IScreeningPartyProvider)this).ScreeningParties, relatedJobPKs: PK);

		#endregion

		#region IJobInvoicingPlugin

		protected override WhsDocketInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new WhsReceiveInvoicingSupporter(this);
		}

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new WhsReceiveRatingAdaptersProvider(this); }
		}

		#endregion

		#region IDocAddresses

		protected override JobDocAddressRequirement GetDocAddressRequirementCore(DocAddressType addressType)
		{
			return this.GetTransportCoRequirement(addressType) ?? base.GetDocAddressRequirementCore(addressType);
		}

		protected override DocAddressType[] SupportedAddressTypesCore()
		{
			return base.SupportedAddressTypesCore().Concat(TransportCoConstants.GetSupportedAddressTypes()).ToArray();
		}

		#endregion

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new WhsReceiveDocManagerInfo(this)); }
		}

		WhsReceiveDocManagerInfo docManagerInfo;

		#endregion

		#region IDocumentSupportable Members

		public DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new WhsReceiveDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;

		#endregion

		#region IDtbBookingParent Members

		ZString IDtbBookingParent.JobTypeDescription
		{
			get { return HumanReadableNameWithoutID; }
		}

		ZString IDtbBookingParent.JobType
		{
			get { return ((IWorkflowProvider)this).WorkflowType; }
		}

		DtbBookingDirection[] IDtbBookingParent.GetSupportedDirections()
		{
			return new DtbBookingDirection[] { DtbBookingDirection.PIC };
		}

		IJobInvoicingPlugIn IDtbBookingParent.InvoicingJob
		{
			get { return this; }
		}

		void IDtbBookingParent.TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings)
		{
		}

		ZBool IDtbBookingParent.IsSupportsDirectSchedule
		{
			get { return false; }
		}

		public ZQuery TransportBookingTemplateFilters => new ZQuery();

		bool IDtbBookingParent.RequiresMultiContainerBooking => true;

		public bool CanCreateTransportBooking => true;

		public ZGuid BookingParentPK => PK;

		public string BookingParentTablePrefix => TablePrefix;

		public (bool isShouldShow, string caption, string message, string confirmation) GetExtendingConfirmMessageBeforeCreateTransportBooking() => (false, null, null, null);

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		#endregion

		#region IJobWithTransportCompany

		bool IJobWithTransportCompany.GetTransportCoDocAddressReadOnly() => NonStandardReadOnly1;

		#endregion

		#region IWorkflowProvider Members

		protected override ZString GetWorkflowType()
		{
			return WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode;
		}

		protected override WhsDocketProcessTasksCollection GetNewProcessTasksCollection()
		{
			return new WhsReceiveProcessTasksCollection(this);
		}

		#endregion

		#region ITemplateCopyable Members

		IBusiness ITemplateCopyable.TemplateCopy()
		{
			var clone = (WhsReceive)this.Clone();
			clone.WD_DocketStatus = DocketStatus.Codes.New;
			clone.WD_ArrivalDate = ZDateTimeOffset.Empty;
			clone.WD_FinalisedDate = ZDateTimeOffset.Empty;
			clone.WD_TransportReference = "";
			clone.WD_WD_Split = ZGuid.Empty;
			clone.WD_ExternalReferenceSplit = 0;

			clone.Containers.DeleteAll();
			clone.Pallets.DeleteAll();

			TemplateCopyInventory(clone);

			clone.WD_DocketStatus = DocketStatus.Codes.New;
			clone.Validation.ValidateWD_ExternalReference();

			return clone;
		}

		void TemplateCopyInventory(WhsReceive clone)
		{
			using (new SemaphoreManager(this.UpdatingWeightAndVolumeSemaphore))
			using (new SemaphoreManager(clone.UpdatingWeightAndVolumeSemaphore))
			{
				foreach (WhsInventoryView line in Inventory.ToArray())
				{
					var lineClone = (WhsInventoryView)line.Clone();
					lineClone.WI_WD = clone.PK;
					lineClone.WI_WL = ZGuid.Empty;
					lineClone.WI_ArrivalDate = ZDateTimeOffset.Empty;
					lineClone.OriginalInventoryStatus = InventoryStatus.Codes.Pending;
					lineClone.OriginalInventoryHeldCode = ZString.Empty;
					lineClone.WI_PalletID = ZString.Empty;

					if (lineClone.Product.IsSerialNumberUsed(clone.Client))
					{
						lineClone.WI_SerialNumber = ZString.Empty;
					}

					var receiveLine = lineClone.InDocketLine;
					if (receiveLine != null)
					{
						receiveLine.WE_StockOnHand = receiveLine.WE_ClientOrderedUnits = receiveLine.WE_TransactionQuantity;
					}
				}
			}
		}

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.WarehouseReceive; }
		}

		IOrgHeader IRelatableActivity.Client
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ClientHasChanges
		{
			get { return false; }
		}

		IOrgContact IRelatableActivity.Contact
		{
			get { return null; }
		}

		ZBool IRelatableActivity.ContactHasChanges
		{
			get { return false; }
		}

		ZString IRelatableActivity.Summary
		{
			get { return string.Join("; ", new[] { WD_DocketSubType, WD_CustomerReference, WD_DocketStatusDescription }.Where(x => !x.IsEmpty)); }
		}

		void IRelatableActivity.OnRelatedActivitySaving(IRelatableActivity relatedActivity)
		{
		}

		ZBool IRelatableActivity.SupportViewRelatedCommunications => ZBool.True;

		public IRelatedChildActivityPivotCollection RelatedChildActivityPivotCollection
		{
			get
			{
				if (relatedChildActivityPivotCollection == null)
				{
					relatedChildActivityPivotCollection = new RelatedChildActivityPivotCollection(this);
				}
				return relatedChildActivityPivotCollection;
			}
		}
		RelatedChildActivityPivotCollection relatedChildActivityPivotCollection;

		public IRelatedParentActivityPivotCollection RelatedParentActivityPivotCollection
		{
			get
			{
				if (relatedParentActivityPivotCollection == null)
				{
					relatedParentActivityPivotCollection = new RelatedParentActivityPivotCollection(this);
				}
				return relatedParentActivityPivotCollection;
			}
		}
		RelatedParentActivityPivotCollection relatedParentActivityPivotCollection;

		#endregion

		#region IRelatedJobs Members

		protected override ControllerID JobControllerId
		{
			get { return ControllerIDs.WhsReceive; }
		}

		#endregion

		#region ISendEmailSource Members

		protected override AddressBookSelection GetAddressBookSelection()
		{
			var result = base.GetAddressBookSelection();
			result.AddRecipient(Supplier);
			result.AddRecipient(this.GetTransportCo());
			return result;
		}

		protected override string GetTemplateCategory()
		{
			return MailTemplateCategoryList.Codes.WarehouseReceive;
		}

		#endregion

		#region IWhsLogEventParent Members

		protected override string GetDocketEventReferenceParameterType
		{
			get { return Constants.EventReferenceParameterTypes.Receive; }
		}

		#endregion

		#region IShouldUpdateScreeningStatus

		ZBool IShouldUpdateScreeningStatus.ShouldUpdateScreeningStatus { get; set; }

		#endregion

		#region IDeniedPartyProvider Members

		ZString IDeniedPartyProvider.ReferenceId => WD_DocketID;

		#endregion

		#region IScreeningPartyProvider

		ZString IScreeningPartyProvider.GetWorstScreeningStatus() => this.GetWorstScreeningStatus();

		ZString IScreeningPartyProvider.GetWorstScreeningStatusUnlessManuallyCleared() =>
			this.GetWorstScreeningStatusUnlessManuallyCleared();

		ScreeningParty[] IScreeningPartyProvider.ScreeningParties => GetScreeningPartiesCore();

		ZString IScreeningStatusProvider.ScreeningStatus
		{
			get => WD_ScreeningStatus;
			set => WD_ScreeningStatus = value;
		}

		protected override ScreeningParty[] GetScreeningPartiesCore() => this.GetScreeningParties().ToArray();

		protected override bool IsDPSMovementRestrictedCore() => this.IsDPSMovementRestricted();

		#endregion

		#region ILineToPutawayParent Members

		ZGuid ILineToPutawayParent.WarehousePK => WD_WW_Whs;
		ZGuid ILineToPutawayParent.ClientPK => WD_OH_Client;

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return false; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("e316f682-13d9-4be1-a47d-f1b8dfaba96b", @"Receives cannot be deleted."); }
		}

		#endregion

		#region ICriticalChangesVersionID members

		ZGuid ICriticalChangesVersionID.CriticalChangesVersionID
		{
			set => WD_CriticalChangesVersionID = value;
			get => WD_CriticalChangesVersionID;
		}

		bool ICriticalChangesVersionID.IsImmutableStatus => IsFinalised;

		#endregion

		#region ITaskPlanningJob

		protected override string SpecialCannotUpdateTaskPlanningStatusReasonCore() => string.Empty;

		#endregion

		#region EConversations

		JobConversation IConversationProvider.eConversation
		{
			get
			{
				if (!IsInDatabase)
				{
					return null;
				}
				return conversation ?? (conversation = GetOrCreateConversation());
			}
		}
		JobConversation conversation;

		JobConversation GetOrCreateConversation()
		{
			var result = JobConversation.GetOrCreate(this);
			RegisterEditableChildObject(result);
			return result;
		}

		ModuleIdentifier IConversationProvider.ParentModule => ModuleIDs.WhsReceive;

		ControllerID IConversationProvider.ParentController => ControllerIDs.WhsReceive;

		IEnumerable<EConversation.Business.RelatedParty> IConversationProvider.AdditionalParticipants => Enumerable.Empty<EConversation.Business.RelatedParty>();

		bool IConversationProvider.SendEmailNotificationsOnSave => true;

		string IConversationProvider.EmailSubjectContentOverride => default;

		string IConversationProvider.FromAddressOverride => GlowRegistry.Instance.NeoEnableConversations.Value ? GlowRegistry.Instance.NeoConversationsEmailAddress.Value : default;

		NotificationEmailTemplate IConversationProvider.NotificationEmailTemplateOverride => default;

		string IAllowAttachEmailsToEDocs.ReferenceNumber => WD_DocketID;

		void IConversationProvider.RunConversationUpdateActionBeforeSaving()
		{
		}

		IEnumerable<IConversationParticipant> IConversationAdditionalParticipantProvider.GetAdditionalParticipants(IReadOnlyCollection<IConversationParticipant> subscribedParticipants, JobConversationParticipant sender)
		{
			if (!GlowRegistry.Instance.NeoEnableConversations.Value ||
				subscribedParticipants.Any(p => !(p is IOrgContact)))
			{
				return Enumerable.Empty<IConversationParticipant>();
			}

			var provider = ObjectFactory.Get<IWarehouseConversationParticipantProvider>();

			return provider.GetAdditionalParticipants(this, sender);
		}

		bool IConversationParentHyperlinkProvider.ShouldUseThisProviderForHyperlink(IConversationParticipant participant) => participant == null || participant is OrgContact;

		string IConversationParentHyperlinkProvider.GetHyperlinkToConversationParent() => GlowRegistry.GetNeoDefaultFormFlowUrl(this);

		#endregion

		#region Implementation

		internal void PerformActionOnSelectedLines<T>(Action<IEnumerable<T>> actionToPerform, IEnumerable<T> selectedInventory, INotifications notifications = null, bool canAllocateLocationOnPutawayTransfers = false)
				 where T : ICanPerformReceiveActions
		{
			Argument.NotNull(selectedInventory, nameof(selectedInventory));

			if (notifications == null)
			{
				notifications = NotificationSubscriber;
			}

			if (IsCreatedFromPickByBOM)
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromPickOrder));
			}
			else if (!IsFinalisedOrCancelled && (canAllocateLocationOnPutawayTransfers || selectedInventory.All(i => !i.HasPutawayTransfer)))
			{
				actionToPerform(selectedInventory);
			}
			else if (IsFinalisedOrCancelled)
			{
				notifications.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			}
			else
			{
				notifications.Notify(new ErrorNotification(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
			}
		}

		void PerformActionOnAllLines(Action actionToPerform)
		{
			if (IsCreatedFromPickByBOM)
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromPickOrder));
			}
			else if (IsFinalisedOrCancelled)
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled));
			}
			else if (IsCreatedFromWorkOrder)
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.CannotPerformThisOperationBecauseReceiveWasCreatedFromWorkOrder));
			}
			else if (Lines.Cast<WhsReceiveLine>().All(i => !i.HasPutawayTransfer))
			{
				actionToPerform();
			}
			else
			{
				NotificationSubscriber.Notify(new ErrorNotification(ReceiveErrorTypes.ContainsReceiveLinesWithPutawayTransfers));
			}
		}

		protected override void CreateOperationalStatusChangeEventsCore()
		{
			base.CreateOperationalStatusChangeEventsCore();
			if ((WD_DocketStatus == DocketStatus.Codes.Putaway || WD_DocketStatus == DocketStatus.Codes.Finalised) && !EventLogExists(Events.WarehouseReceiptPuttingAway.Code))
			{
				CreateEventLog(Events.WarehouseReceiptPuttingAway);
			}
			if (WD_ETA != ZDateTimeOffset.Empty && !EventLogExists(Events.WarehouseReceiptETANotification.Code))
			{
				CreateEventLog(Events.WarehouseReceiptETANotification);
			}
			if (WD_ArrivalDate != ZDateTimeOffset.Empty && !EventLogExists(Events.WarehouseReceiptArrived.Code))
			{
				CreateEventLog(Events.WarehouseReceiptArrived);
			}
		}

		public void CreateUnloadTime(ZDateTimeOffset? dateTime = null)
		{
			CreateEventLog(Events.WarehouseReceiptUnloaded, dateTime);
		}

		void SynchronizeInventoryArrival()
		{
			foreach (var line in Lines)
			{
				line.WE_AdjustmentArrivalDate = WD_ArrivalDate;
			}
		}

		#endregion

		#region Implementation of IWhsReceiveWrapperStrategy

		WhsDocketValidation IWhsReceiveWrapperStrategy.GetNewValidation()
		{
			return new WhsReceiveValidation(this);
		}

		WhsDocketLookups IWhsReceiveWrapperStrategy.GetNewLookups()
		{
			return new WhsReceiveLookups(this);
		}

		WhsInventoryViewCollection IWhsReceiveWrapperStrategy.GetNewWhsInventoryCollection()
		{
			return GetNewWhsInventoryCollection();
		}

		#endregion

		#region ReceiveValidationCache

		internal bool IsMismatchedEntryKey(WhsBondedWarehouseAttribute bondedWarehouseAttribute) => GetBondedWarehouseAttributeCache().IsMismatchedEntryKey(bondedWarehouseAttribute);

		WhsBondedWarehouseAttributeCache GetBondedWarehouseAttributeCache() => bondedWarehouseAttributeCache ?? new WhsBondedWarehouseAttributeCache(Factory, Lines);
		WhsBondedWarehouseAttributeCache bondedWarehouseAttributeCache;

		internal bool IsPackageGroupAlreadyAssigned(string packageGroupId) => GetPackageGroupIdsCache().IsPackageGroupAlreadyAssigned(packageGroupId);

		PackageGroupIdsCache GetPackageGroupIdsCache() => packageGroupIdsCache ?? new PackageGroupIdsCache(Factory, this);
		PackageGroupIdsCache packageGroupIdsCache;

		ProductPackageTotals IValidateParentWithLines.GetParentProductPackageTotals(IEnumerable<ICalculateProductPackageTotals> lines) => cachedProductPackageTotals ?? GetProductPackageTotals(lines);
		ProductPackageTotals cachedProductPackageTotals;

		ProductPackageTotals GetProductPackageTotals(IEnumerable<ICalculateProductPackageTotals> lines) => new ProductPackageTotals(lines);

		internal DocketUNDGValidationCache DocketUNDGValidationCache => docketUNDGValidationCache ?? DocketUNDGValidationHelper.BuildDocketUNDGValidationCache(this);
		DocketUNDGValidationCache docketUNDGValidationCache;

		OrderToReturnInfoCache OrderToReturnInfoCache => orderToReturnInfoCache ?? new OrderToReturnInfoCache(Factory, this);
		OrderToReturnInfoCache orderToReturnInfoCache;

		internal bool IsSerialNumberAlreadyInUse(WhsSerialNumberPivot pivot) => SerialNumberUniquenessCheckerCache.IsSerialNumberAlreadyInUse(pivot);

		internal SerialNumberUniquenessChecker SerialNumberUniquenessCheckerCache => serialNumberUniquenessCheckerCache ?? new SerialNumberUniquenessChecker();
		SerialNumberUniquenessChecker serialNumberUniquenessCheckerCache;

		#endregion

		public static IReadOnlyCollection<WhsReceive> GetExistingReturnReceivesForParentOrderWithOrderReference(WhsOrder order)
		{
			var receiveQuery = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive);
			receiveQuery.AddToFilter(WhsDocketSchema.WD_ExternalReference, order.WD_ExternalReference);
			receiveQuery.AddToFilter(WhsDocketSchema.WD_OH_Client, order.WD_OH_Client);
			receiveQuery.AddToFilter(WhsDocketSchema.WD_DocketSubType, ReceiveType.Codes.Returns);
			receiveQuery.AddToFilter(WhsDocketSchema.WD_WD_ParentDocket, order.PK);

			return order.Factory.Load<WhsReceive>(receiveQuery);
		}

		public static ZByte GetNextMaxExternalReferenceSplitFromReceiveCollection(IReadOnlyCollection<WhsReceive> existingReceivesWithTheSameReference)
		{
			var splitNumber = ZByte.Zero;
			if (existingReceivesWithTheSameReference.Count > 0)
			{
				var maxSplitNumber = existingReceivesWithTheSameReference.Max(receive => receive.WD_ExternalReferenceSplit);
				splitNumber = ++maxSplitNumber;
			}

			return splitNumber;
		}

		public static string ReturnReceivesRequireAnOrderToReturnWarning
			=> Res.GetString("d1e34e7d-afb6-4325-bca3-5f7770a65151", "Return receives require a reference with the same order number of a departed order to be considered a valid return receive.");
	}

	#region Document Supporter

	public class WhsReceiveDocumentSupporter : DocumentSupporter
	{
		public WhsReceiveDocumentSupporter(WhsReceive receive)
				 : base(receive)
		{
		}

		protected WhsReceive Receive
		{
			get { return (WhsReceive)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WhsInwards; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint => Env.Security.WhsReceiveCustomiseDocuments;

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] {
							 Core.Constants.DataContext.WhsReceive,
							 Core.Constants.DataContext.WhsPalletIDLabels,
							 Core.Constants.DataContext.GenericFreightJob
					 };
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] genericWrappers = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Receive);
			if (genericWrappers != null)
			{
				Receive.InventoryToPrintPalletLabelFor = null;
				return genericWrappers;
			}

			DocumentWrapper[] result = null;
			switch (dataContext)
			{
				case Core.Constants.DataContext.GenericProductLabel:
					result = GetProductLabelDocumentWrappers();
					break;
				case Core.Constants.DataContext.WhsPalletIDLabels:
					var receiveDocument = new WhsDocketLabelControl(Receive, Receive.WD_TotalPallets);
					var eventArgs = new WhsReceiveToPrintEventArgs(receiveDocument, dataContext);
					Receive.CallOnPrint(this, eventArgs);
					if (eventArgs.ContinueToPrint)
					{
						result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.WhsReceive, receiveDocument) };
					}
					break;

				default:
					Receive.InventoryToPrintPalletLabelFor = null;
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.WhsReceive, Receive) };
					break;
			}

			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var message = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			switch (dataContextValue.DataContext)
			{
				case Constants.DataContext.WhsReceive:
				case Constants.DataContext.GenericFreightJob:
					message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoReceive", "Cannot find Warehouse Receive.");
					break;
				case Constants.DataContext.GenericProductLabel:
					message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoProductLabelToPrint", "Cannot find Product Label to print.");
					break;
				case Constants.DataContext.WhsPalletIDLabels:
					message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoPalletIDLabelToPrint", "Cannot find Pallet ID Label to print.");
					break;
				default:
					break;
			}

			return message;
		}

		DocumentWrapper[] GetProductLabelDocumentWrappers()
		{
			var result = Array.Empty<DocumentWrapper>();
			var options = new WhsDocumentInventoryOptions(Receive.Inventory);
			var optionArgs = new WhsDocumentInventoryEventArgs(options);

			Receive.CallOnInventoryPrint(this, optionArgs);
			if (optionArgs.ContinueToPrint)
			{
				var wrappers = new List<DocumentWrapper>();

				foreach (WhsDocumentInventory documentInventory in options.DocumentInventories)
				{
					if (documentInventory.LabelsToPrint > 0)
					{
						var wrapper = DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, Receive)[0];
						var iWrapper = (IWhsDocumentInventory)wrapper;
						iWrapper.SetInventory(documentInventory);
						wrappers.Add(wrapper);
					}
				}

				result = wrappers.ToArray();
			}

			return result;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			IDocumentDeliveryContact result;
			OrgAddress address = null;

			if (contactType == ContactType.LocalTransport)
			{
				if (!Receive.TransportCoDocAddress.E2_AddressOverride)
				{
					address = Receive.TransportCoDocAddress.Address;
				}
				result = new OrgHeaderContact(Receive.GetTransportCo(), address);
			}
			else if (contactType == ContactType.Consignor)
			{
				if (!Receive.SupplierDocAddress.E2_AddressOverride)
				{
					address = Receive.SupplierDocAddress.Address;
				}
				result = new OrgHeaderContact(Receive.Supplier, address);
			}
			else
			{
				result = new OrgHeaderContact(Receive.Client, null);
			}

			return result;
		}

		protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			var result = new List<IDocumentEventsHandler>(base.GetDocumentEventsHandlers());
			result.Add(new WhsReceiveCartageAdviceDocumentEventsHandler() { DocumentSupporter = this });
			return result.ToArray();
		}

		#region GetChildCollection

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result;

			switch (businessContext)
			{
				case BusinessContext.DtbBooking:
					bookings = TransportBookingLoader.GetBookingsToDeliver(Receive, menuToBeRun);
					bookingDeliveryCancelled = !bookings.Any();
					result = bookings.Cast<IDocumentSupportable>().ToArray();
					break;
				default:
					result = base.GetChildCollection(menuToBeRun, businessContext, childCommand);
					break;
			}

			return result;
		}

		public IDocumentSupportable[] Bookings
		{
			get
			{
				return bookings.Select(b =>
				{
					// Use factory belonging to the booking parent, as it has the factory that gets saved at the end of a non user-interactive process
					var factory = Receive.Factory;
					var reloadedBooking = (IDocumentSupportable)factory.Load<IDtbBooking>(b.PK);
					if (reloadedBooking == null)
					{
						ErrorReporter.ReportOnce("WhsReceiveDocumentSupporter_NullBooking", $"Booking {b.HumanReadableName} from factory '{b.Factory.NameForDebugging}' could not be loaded by factory '{factory.NameForDebugging}' belonging to {Receive.HumanReadableName}");
					}
					return reloadedBooking;
				}).Where(b => b != null).ToArray();
			}
		}
		IEnumerable<IDtbBooking> bookings = Array.Empty<IDtbBooking>();

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return base.ShowReasonForNotPrinting(dataContext, commandBeingRun) && !bookingDeliveryCancelled;
		}

		bool bookingDeliveryCancelled;

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get { return new[] { BusinessContext.DtbBooking }; }
		}

		#endregion
	}

	#endregion

	#region Docket Splitter

	public abstract class ReceiveSplitter
	{
		protected WhsReceiveLineCollection InventoryLineList => Receive.Lines;

		WhsInventoryView CreateNewInventoryFromSplit(WhsReceiveLine line, WhsReceive owner)
		{
			var result = (WhsInventoryView)line.Inventory[0].Clone();
			owner.Inventory.Add(result);
			result.WI_WD = owner.PK;
			result.WI_SplitQuantity = 0m;

			return result;
		}

		protected WhsInventoryView SplitInventoryPartially(WhsReceiveLine oldLine, WhsReceive newReceive)
		{
			var newLine = CreateNewInventoryFromSplit(oldLine, newReceive);
			newLine.WI_ExpectedReceiptQuantity = oldLine.SplitQuantity;
			newLine.WI_InDocketLineUnits = oldLine.SplitQuantity;

			var originalQty = oldLine.Inventory[0].WI_InDocketLineUnits;
			using (new SemaphoreManager(oldLine.UpdatingTransactionQtyFromExpectedQtySemaphore))
			{
				oldLine.Inventory[0].WI_ExpectedReceiptQuantity -= oldLine.SplitQuantity;
				oldLine.Inventory[0].WI_InDocketLineUnits -= oldLine.SplitQuantity;
				oldLine.SplitQuantity = 0m;
			}

			if (Receive.IsCustomsTransaction)
			{
				WhsCustomsHelper.SplitCustomsValuesAndQuantities(oldLine, originalQty);
				WhsCustomsHelper.SplitCustomsValuesAndQuantities(newLine.InDocketLine, originalQty);
			}

			return newLine;
		}

		protected WhsInventoryView SplitInventoryFully(WhsReceiveLine oldLine, WhsReceive newReceive)
		{
			var newLine = CreateNewInventoryFromSplit(oldLine, newReceive);

			RemoveFromAllCollections(oldLine);
			oldLine.Delete();

			return newLine;
		}

		void RemoveFromAllCollections(BusinessObject bizo)
		{
			foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)bizo).ParentCollections)
			{
				collection.Remove(bizo);
			}
		}

		public ReturnResult Split(WhsReceive receive)
		{
			Argument.NotNull(receive, "receive");
			return Split(receive.Factory, receive);
		}

		public ReturnResult Split(BusinessObjectFactory factory, WhsReceive receive)
		{
			Argument.NotNull(factory, "factory");
			Argument.NotNull(receive, "receive");

			Receive = receive;
			Factory = factory;

			NewDocketSplits = new List<ZByte>();

			return SplitCore();
		}

		protected WhsReceive SplitDocket(ZByte nextSplitNumber)
		{
			var newDocket = (WhsReceive)Receive.Clone();
			newDocket.WD_ExternalReferenceSplit = nextSplitNumber;
			newDocket.WD_WD_Split = Receive.PK;

			NewDocketSplits.Add(newDocket.WD_ExternalReferenceSplit);
			return newDocket;
		}

		protected ZByte GetHighestSplitDocketExternalReference()
		{
			var query = new ZQuery(WhsDocketSchema.WD_ExternalReference, Receive.WD_ExternalReference);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, Receive.WD_OH_Client);
			query.AddToFilter(WhsDocketSchema.WD_DocketType, Receive.WD_DocketType);
			query.OrderBy = WhsDocketSchema.WD_ExternalReferenceSplit.Name + " DESC";

			var highestSplitDocket = Factory.LoadTop1<WhsDocket>(query);
			return highestSplitDocket != null ? highestSplitDocket.WD_ExternalReferenceSplit : ZByte.Zero;
		}

		protected static string MaximumAllowedSplitsReachedErrorMessage => ResString.GetMultilingualString("90f622c7-1ab5-427b-85f4-ca4670264660", "The receipt has reached the maximum number of allowable splits.");
		protected abstract ReturnResult SplitCore();

		public List<ZByte> NewDocketSplits { get; set; }
		protected WhsReceive Receive { get; set; }
		BusinessObjectFactory Factory { get; set; }
	}

	public class ReceiveSplitterByQuantity : ReceiveSplitter
	{
		protected override ReturnResult SplitCore()
		{
			var errorMessage = "";

			var highestSplitNumber = GetHighestSplitDocketExternalReference();
			if (highestSplitNumber >= byte.MaxValue)
			{
				errorMessage = MaximumAllowedSplitsReachedErrorMessage;
			}
			else
			{
				var newReceive = SplitDocket(++highestSplitNumber);
				newReceive.WD_WD_Split = Receive.PK;
				newReceive.WD_ArrivalDate = ZDateTimeOffset.Empty;
				newReceive.WD_TotalUnits = 0m;
				var inventories = InventoryLineList.Cast<WhsReceiveLine>().ToArray();
				var inventoriesToSplit = inventories.Where(l => l.SplitQuantity > 0m).ToArray();

				if (inventoriesToSplit.Length > 0)
				{
					var palletIdsCannotBeCopied = new HashSet<ZString>(inventories.Where(l => l.SplitQuantity != l.WE_TransactionQuantity).Select(l => l.WE_PalletID));

					foreach (var line in inventoriesToSplit)
					{
						var shouldClearPalletId = palletIdsCannotBeCopied.Contains(line.WE_PalletID);

						var newInventory = (line.SplitQuantity == line.WE_TransactionQuantity)
								 ? SplitInventoryFully(line, newReceive)
								 : SplitInventoryPartially(line, newReceive);

						if (shouldClearPalletId)
						{
							newInventory.WI_PalletID = ZString.Empty;
						}
						newInventory.WI_WL = ZGuid.Empty;
						newInventory.WI_ArrivalDate = ZDateTimeOffset.Empty;
						newInventory.WI_WE_OriginalInDocketLineForRating = newInventory.WI_WE_InDocketLine;
					}
				}
			}

			return new ReturnResult { Success = string.IsNullOrEmpty(errorMessage), Message = errorMessage };
		}
	}

	public class ReceiveSplitterByAreaType : ReceiveSplitter
	{
		protected override ReturnResult SplitCore()
		{
			var areaTypes = GetDistinctAreaTypes().ToArray();
			var errorMessage = "";

			for (int areaTypeIndex = 1; areaTypeIndex < areaTypes.Length; areaTypeIndex++)
			{
				var highestSplitNumber = GetHighestSplitDocketExternalReference();
				if (highestSplitNumber >= byte.MaxValue)
				{
					if (areaTypeIndex > 1)
					{
						errorMessage += Res.GetString("f3a8f46a-946e-43c1-b5b1-b2ee143c3e35", "Not all area types have been split into new receipts. ");
					}

					errorMessage += MaximumAllowedSplitsReachedErrorMessage;
					break;
				}

				var newReceive = SplitDocket(areaTypes[areaTypeIndex], ++highestSplitNumber);
				newReceive.WD_WD_Split = Receive.PK;

				for (int i = InventoryLineList.Count - 1; i > 0; i--)
				{
					var line = InventoryLineList[i];
					if (line.PutawayLocationAreaType == areaTypes[areaTypeIndex])
					{
						var newInventory = SplitInventoryFully(line, newReceive);
						newInventory.WI_WE_OriginalInDocketLineForRating = newInventory.WI_WE_InDocketLine;
					}
				}
			}

			return new ReturnResult { Success = string.IsNullOrEmpty(errorMessage), Message = errorMessage };
		}

		WhsReceive SplitDocket(string areaType, ZByte nextSplitNumber)
		{
			var newReceive = SplitDocket(nextSplitNumber);
			if (areaType == AreaTypes.Codes.Bonded ||
					areaType == AreaTypes.Codes.Excise)
			{
				newReceive.WD_DocketSubType = ReceiveType.Codes.Customs;
			}
			return newReceive;
		}

		IEnumerable<ZString> GetDistinctAreaTypes() => IEnumerableExtensions.DistinctBy(InventoryLineList.Cast<WhsReceiveLine>(), l => l.WE_WL).Select(l => l.PutawayLocationAreaType).Distinct();
	}

	#endregion

	#region Receive Splitters

	#region ReceiveSplitterBase

	internal abstract class ReceiveSplitterBase
	{
		public IEnumerable<WhsReceiveLine> SplitLines(WhsReceive receive, IEnumerable<WhsReceiveLine> linesToSplit)
		{
			var resultCollection = new List<WhsReceiveLine>();
			if (receive == null)
			{
				throw new ArgumentNullException(nameof(receive));
			}
			if (linesToSplit == null)
			{
				throw new ArgumentNullException(nameof(linesToSplit));
			}

			this.Receive = receive;
			resultCollection.AddRange(linesToSplit);

			var (allSplitCount, linesOkToSplit) = GetSplitCountForAllLines(linesToSplit);

			if (allSplitCount > WarehouseDataRegistry.Instance.MaxSplitCountForPalletizeLines.Value)
			{
				Receive.NotificationSubscriber.Notify(new ErrorNotification(ErrorType.Error,
					Res.GetString("3b38b544-8991-4f71-86c9-38c59c69c254", "Palletize lines will result in a quantity exceeding the max counts of lines.\r\nYou can increase the threshold in Registry -> Warehouse -> Receive -> Max Split Count For Palletize Lines.")));
			}
			else
			{
				foreach (var line in linesOkToSplit)
				{
					resultCollection.AddRange(SplitLine(line.Line, line.SplitQty, line.SplitCountForLine));
				}
			}

			return resultCollection;
		}

		(int AllSplitCount, IEnumerable<(WhsReceiveLine Line, ZDecimal SplitQty, int SplitCountForLine)> LinesOkToSplit) GetSplitCountForAllLines(IEnumerable<WhsReceiveLine> linesToSplit)
		{
			var result = 0;
			var linesOkToSplit = new List<(WhsReceiveLine, ZDecimal, int)>();

			foreach (var line in linesToSplit)
			{
				result += 1;
				var splitQty = SplitQuantity(line);
				if (IsLineOkToSplit(line, splitQty) && QuantityToSplit(line) > splitQty)
				{
					var splitCount = GetSplitCountForSingleLine(line, splitQty);
					linesOkToSplit.Add((line, splitQty, splitCount));
					result += splitCount;
				}
			}

			return (result, linesOkToSplit);
		}

		int GetSplitCountForSingleLine(WhsReceiveLine line, ZDecimal splitQty)
		{
			var ratio = new ZDecimal(QuantityToSplit(line) / splitQty);
			var splitCount = ratio % 1 > 0 ? ratio.ToZInt() : new ZDecimal(ratio - 1m).ToZInt();

			return splitCount;
		}

		IEnumerable<WhsReceiveLine> SplitLine(WhsReceiveLine line, ZDecimal splitQty, int splitCountForLine)
		{
			var receiveLines = (IBusinessObjectCollection)Receive.Lines;
			using (receiveLines.SuspendListChanged())
			using (Receive.Inventory.SuspendListChanged())
			using (line.Inventory.SuspendListChanged())
			using (new SemaphoreManager(Receive.UpdatingWeightAndVolumeSemaphore))
			using (new SemaphoreManager(Receive.RefreshTotalUnitsFromLinesSemaphore))
			{
				var addedReceiveLineList = new List<WhsReceiveLine>();
				var bomInfoFromLinks = GetBomInfoFromLinks(line);

				for (var i = 0; i < splitCountForLine; i++)
				{
					addedReceiveLineList.Add(CreateNewLine(line, bomInfoFromLinks, splitQty, isSplitBySpecificQuantity: false));
				}
				return addedReceiveLineList;
			}
		}

		public WhsReceiveLine CreateNewLineForSelectLine(WhsReceive receive, WhsReceiveLine line, ZDecimal splitQty)
		{
			if (receive == null)
			{
				throw new ArgumentNullException(nameof(receive));
			}
			if (line == null)
			{
				throw new ArgumentNullException(nameof(line));
			}
			if (splitQty <= 0 || splitQty >= line.WE_TransactionQuantity)
			{
				throw new ArgumentException("The split quantity should between 0 and WE_transactionQuantity of select line.");
			}
			Receive = receive;

			return CreateNewLine(line, GetBomInfoFromLinks(line), splitQty, isSplitBySpecificQuantity: true);
		}

		Dictionary<ZGuid, decimal> GetBomInfoFromLinks(WhsReceiveLine line)
		{
			Dictionary<ZGuid, decimal> bomInfoFromLinks = null;

			if (Receive.IsCreatedFromWorkOrder)
			{
				bomInfoFromLinks = new Dictionary<ZGuid, decimal>();
				foreach (var link in line.BOMComponentLinks)
				{
					bomInfoFromLinks.Add(link.WIP_WE_ComponentLine, link.WIP_ComponentQuantity / line.WE_TransactionQuantity);
				}
			}

			return bomInfoFromLinks;
		}

		WhsReceiveLine CreateNewLine(WhsReceiveLine line, Dictionary<ZGuid, decimal> bomInfoFromLinks, decimal splitQty, bool isSplitBySpecificQuantity)
		{
			WhsReceiveLine newReceiveLine = null;
			var originalQty = QuantityToSplit(line);
			if (originalQty > splitQty)
			{
				newReceiveLine = (WhsReceiveLine)line.Clone();
				Receive.Lines.Add(newReceiveLine);
				ClearPropertiesOnNewLine(newReceiveLine);
				UpdateQuantities(line, newReceiveLine, splitQty, isSplitBySpecificQuantity);
				UpdateCrossDockLinks(line, newReceiveLine);
				UpdateBOMLinks(line, newReceiveLine, bomInfoFromLinks);

				if (Receive.IsCustomsTransaction)
				{
					WhsCustomsHelper.SplitCustomsValuesAndQuantities(line, originalQty);
					WhsCustomsHelper.SplitCustomsValuesAndQuantities(newReceiveLine, originalQty);
				}
			}
			return newReceiveLine;
		}

		void UpdateCrossDockLinks(WhsReceiveLine line, WhsReceiveLine newLine)
		{
			var availableQtyToReserve = line.CanReserveClientOrderedUnits ? line.WE_ClientOrderedUnits : line.WE_TransactionQuantity;
			if (line.ReservedQuantity > availableQtyToReserve)
			{
				var crossDockUnitsToTransfer = line.ReservedQuantity - availableQtyToReserve; // the total cross dock units that we must transfer to the new inventory record

				foreach (var pickLine in line.ReservedPickLines.ToArray())
				{
					if (pickLine.ReservedQuantity <= crossDockUnitsToTransfer) // we can transer the whole reserved pick line to new inventory record
					{
						pickLine.WZ_WE_InventoryLine = newLine.PK;
						crossDockUnitsToTransfer -= pickLine.ReservedQuantity;
					}
					else    // else reduce the quantity on this divot and create a new divot pointing to the new inventory record
					{
						pickLine.ReservedQuantity -= crossDockUnitsToTransfer;
						((WhsOrderLine)pickLine.DocketLine).ReserveStockIfAbleTo(newLine.Inventory[0], crossDockUnitsToTransfer);

						// divot qty was more than we needed to transfer so we can break
						crossDockUnitsToTransfer = 0;
					}
					if (crossDockUnitsToTransfer == 0)
					{
						break;
					}
				}
			}
		}

		void UpdateQuantities(WhsReceiveLine line, WhsReceiveLine newLine, decimal quantity, bool isSplitBySpecificQuantity)
		{
			var saveExpectedReceiptQuantity = line.WE_ClientOrderedUnits;
			var inDocketLineUnitsDiff = line.WE_TransactionQuantity - quantity;

			using (new SemaphoreManager(line.UpdatingTransactionQtyFromExpectedQtySemaphore))
			{
				line.WE_ClientOrderedUnits = saveExpectedReceiptQuantity;
				var expectedReceiptQuantityDiff = line.WE_ClientOrderedUnits - quantity;

				if (isSplitBySpecificQuantity)
				{
					line.WE_ClientOrderedUnits = quantity;
					line.WE_TransactionQuantity = quantity;
					newLine.WE_ClientOrderedUnits = expectedReceiptQuantityDiff;
					newLine.WE_TransactionQuantity = inDocketLineUnitsDiff;
				}
				else
				{
					if (expectedReceiptQuantityDiff > 0m)
					{
						line.WE_ClientOrderedUnits -= System.Math.Min(expectedReceiptQuantityDiff, quantity);
					}

					if (inDocketLineUnitsDiff > 0m)
					{
						line.WE_TransactionQuantity -= System.Math.Min(inDocketLineUnitsDiff, quantity);
					}

					newLine.WE_ClientOrderedUnits = System.Math.Max(0m, System.Math.Min(expectedReceiptQuantityDiff, quantity));
					newLine.WE_TransactionQuantity = System.Math.Max(0m, System.Math.Min(inDocketLineUnitsDiff, quantity));
				}
				line.WE_StockOnHand = line.WE_TransactionQuantity;
				newLine.WE_StockOnHand = newLine.WE_TransactionQuantity;
			}
		}

		void UpdateBOMLinks(WhsReceiveLine line, WhsReceiveLine newLine, Dictionary<ZGuid, decimal> bomInfoFromLinks)
		{
			if (Receive.IsCreatedFromWorkOrder)
			{
				foreach (var link in line.BOMComponentLinks)
				{
					var factor = bomInfoFromLinks[link.WIP_WE_ComponentLine];
					var newLinkQuantity = newLine.WE_TransactionQuantity * factor;
					var newLink = line.Factory.New<WhsBOMInventoryPivot>();
					newLink.WIP_WE_InventoryLine = newLine.WE_WE_OriginalDocketLineForRating;
					newLink.WIP_WE_ComponentLine = link.WIP_WE_ComponentLine;
					newLink.WIP_ComponentQuantity = newLinkQuantity;
					link.WIP_ComponentQuantity -= newLinkQuantity;
				}
			}
		}

		bool IsLineOkToSplit(WhsReceiveLine line, ZDecimal splitQty)
		{
			var msg = IsLineOkToSplitCore(line, splitQty);
			var result = string.IsNullOrEmpty(msg);
			if (!result)
			{
				line.AddRowWarning(msg);
			}
			return result;
		}

		string IsLineOkToSplitCore(WhsReceiveLine line, ZDecimal splitQty)
		{
			var result = string.Empty;
			if (line.SupplierPart == null || line.HasErrors || line.IsAvailable)
			{
				result = ErrorMessage_InvalidLine;
			}
			else if (splitQty <= 0m)
			{
				result = ErrorMessage_NoQuantityDefined;
			}
			return result;
		}

		protected virtual ZDecimal QuantityToSplit(WhsReceiveLine line)
		{
			return System.Math.Max(line.WE_TransactionQuantity, line.WE_ClientOrderedUnits);
		}

		protected abstract ZDecimal SplitQuantity(WhsReceiveLine line);
		protected abstract void ClearPropertiesOnNewLine(WhsReceiveLine line);
		protected abstract void SetWarningMessage(WhsReceiveLine line, string message);
		public abstract string ErrorMessage_InvalidLine { get; }
		public abstract string ErrorMessage_NoQuantityDefined { get; }

		protected WhsReceive Receive { get; set; }
	}

	#endregion

	#region PalletReceiveSplitter

	internal class PalletReceiveSplitter : ReceiveSplitterBase
	{
		protected override ZDecimal SplitQuantity(WhsReceiveLine line)
		{
			return line?.SupplierPart?.OP_StockKeepingUnitPerPallet ?? 0m;
		}

		protected override void ClearPropertiesOnNewLine(WhsReceiveLine line)
		{
			line.WE_WL = ZGuid.Empty;
			line.WE_PalletID = ZString.Empty;
		}

		protected override void SetWarningMessage(WhsReceiveLine line, string message)
		{
		}

		public override string ErrorMessage_InvalidLine { get { return Res.GetString("9772855a-437e-4cbc-bccc-15531bb0bacf", "The system has not palletized this line because it does not have a product, or it has errors, or it is finalized"); } }
		public override string ErrorMessage_NoQuantityDefined { get { return Res.GetString("eb56cfd0-ee78-4b9a-ac76-14e4c04214fc", "The system has not palletized this line because the product does not have a pallet definition"); } }
	}

	#endregion

	#endregion

	#region ID Generators

	class SerialNumberGenerator
	{
		public void Generate(WhsReceive receive, IEnumerable<WhsReceiveLine> selectedLines)
		{
			Argument.NotNull(receive, nameof(receive));
			Argument.NotNull(selectedLines, nameof(selectedLines));

			using (new SemaphoreManager(receive.GenerateSerialNumbersSemaphore))
			{
				foreach (var line in selectedLines)
				{
					GenerateSerialNumbers(line);
				}
			}
		}

		void GenerateSerialNumbers(WhsReceiveLine line)
		{
			if (ResetWarningsAndValidate(line))
			{
				if (IsOkToSerializeLine(line) && line.WE_TransactionQuantity > 1m)
				{
					var splitLines = line.SplitWhenSerialNumberExists();
					UpdateSerialNumbers(line, splitLines);
				}
			}
		}

		bool IsOkToSerializeLine(WhsReceiveLine line)
		{
			var client = line.Docket.Client;
			return IsOkToSerializeLineWithSerialNumber(line, client);
		}

		bool IsOkToSerializeLineWithSerialNumber(WhsReceiveLine line, OrgHeader client)
		{
			return client != null
				&& line.SupplierPart != null
				&& line.Product.IsSerialNumberUsedAndNotReleaseCaptured(client)
				&& IsOkToSerialize(line, line.WE_SerialNumberInfo);
		}

		bool IsOkToSerialize(WhsReceiveLine line, ZPropertyInfo serialNumberAttribInfo)
		{
			return line.WE_OP.IsValid
				&& !line.HasErrors
				&& line.WE_CurrentInventoryStatus != InventoryStatus.Codes.Available
				&& !serialNumberAttribInfo.Value.IsEmpty;
		}

		void UpdateSerialNumbers(WhsReceiveLine line, IEnumerable<WhsReceiveLine> receiveLines)
		{
			var client = line.Docket.Client;
			UpdateSerialNumbers(line, receiveLines, client);
		}

		void UpdateSerialNumbers(WhsReceiveLine line, IEnumerable<WhsReceiveLine> receiveLines, OrgHeader client)
		{
			if (line.Product.IsSerialNumberUsedAndNotReleaseCaptured(client)
				&& IsOkToSerialize(line, line.WE_SerialNumberInfo))
			{
				SetPartAttributeSerialNumber(line.WE_SerialNumber,
					receiveLines,
					line.WE_SerialNumberInfo.MaxLength,
					(receiveLine, serialNumber) => receiveLine.WE_SerialNumber = serialNumber,
					(receiveLine, message) => receiveLine.Inventory[0].ValidationSerialNumberWarningMessage = message);
			}
		}

		void SetPartAttributeSerialNumber(ZString initialSerialNumber,
			IEnumerable<WhsReceiveLine> receiveLines,
			int serialAttributeMaxLength,
			Action<WhsReceiveLine, string> serialNumberSetter,
			Action<WhsReceiveLine, string> serialNumberWarningSetter)
		{
			ParseSerialNumber(initialSerialNumber, out serialNumberPrefix, out serialNumberSuffix);

			if (BigInteger.TryParse(serialNumberSuffix, out var suffix))
			{
				foreach (var line in receiveLines)
				{
					var zeroLeftPaddedSuffix = (++suffix).ToString(Culture.Invariant).PadLeft(serialNumberSuffix.Length, '0');
					var serialNumber = serialNumberPrefix + zeroLeftPaddedSuffix;

					if (serialNumber.Length <= serialAttributeMaxLength)
					{
						using (line.GetValidationSuspender())
						{
							serialNumberSetter(line, serialNumber);
						}
					}
					else
					{
						// this can never happen since the MaxLength validation prevents an oversized serial number being set in the first place,
						//  and previous checks will detect a serial number without a suffix.
						serialNumberSetter(line, "");
						serialNumberWarningSetter(line, ErrorMessage_SerialNumberTooLarge);
					}
				}
			}
		}

		void ParseSerialNumber(ZString serialNumber, out string prefix, out string suffix)
		{
			var i = serialNumber.Length - 1;
			while (i >= 0 && serialNumber.Substring(i, 1).IsNumbersOnlyOrEmpty)
			{
				i--;
			}

			prefix = serialNumber.Left(i + 1);
			suffix = serialNumber.Substring(i + 1);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for reflection components")]
		ZPropertyInfo GetAttributeInfo(WhsReceiveLine line, int attributeNumber)
		{
			ZPropertyInfo result;
			switch (attributeNumber)
			{
				case 2:
					result = line.WE_PartAttrib2Info;
					break;
				case 3:
					result = line.WE_PartAttrib3Info;
					break;
				default:
					result = line.WE_PartAttrib1Info;
					break;
			}
			return result;
		}

		bool ResetWarningsAndValidate(WhsReceiveLine line)
		{
			var client = line.Docket.Client;
			return ResetWarningsAndValidateSerialNumber(line, client);
		}

		bool ResetWarningsAndValidateSerialNumber(WhsReceiveLine line, OrgHeader client)
		{
			var isValid = true;
			var inventory = line.Inventory[0];
			inventory.ValidationSerialNumberWarningMessage = "";

			if (line.SupplierPart == null)
			{
				inventory.ValidationProductWarningMessage = ErrorMessage_ProductRequired;
				isValid = false;
			}
			else if (line.Product.IsSerialNumberUsedAndNotReleaseCaptured(client)
				&& line.WE_CurrentInventoryStatus != InventoryStatus.Codes.Available
				&& !IsStartingNumberValid(inventory,
						(invLine) => invLine.WI_SerialNumber,
						(invLine, message) => invLine.ValidationSerialNumberWarningMessage = message))
			{
				isValid = false;
			}

			return isValid;
		}

		bool IsStartingNumberValid(WhsInventoryView line,
			Func<WhsInventoryView, ZString> getSerialNumber,
			Action<WhsInventoryView, string> setSerialNumberAttributeWarningMessage)
		{
			var isValid = true;
			var serialNumber = getSerialNumber(line);

			if (serialNumber.IsEmpty)
			{
				if (line.WI_InDocketLineUnits > 1m)
				{
					setSerialNumberAttributeWarningMessage(line, ErrorMessage_FirstNumberIsEmpty);
				}
				else
				{
					setSerialNumberAttributeWarningMessage(line, ErrorMessage_WontProcessOneUnitLines);
				}
				isValid = false;
			}
			else
			{
				ParseSerialNumber(serialNumber, out serialNumberPrefix, out serialNumberSuffix);
				if (serialNumberSuffix.Length < 1)
				{
					if (line.WI_InDocketLineUnits > 1m)
					{
						setSerialNumberAttributeWarningMessage(line, ErrorMessage_InvalidNumericSuffix);
					}
					else
					{
						setSerialNumberAttributeWarningMessage(line, ErrorMessage_UnusualSuffixOnOneUnitLine);
					}
					isValid = false;
				}
			}

			return isValid;
		}

		static string ErrorMessage_SerialNumberTooLarge
			=> Res.GetString("56b37ae7-fa93-41ad-86fd-78b9b1a30308", "The Generate Serial Numbers action cannot use the serial number calculated for this field because it is too large");

		static string ErrorMessage_ProductRequired
			=> Res.GetString("739b89b5-0f3c-487b-ab23-3941c198fcfe", "The Generate Serial Numbers action cannot process lines with no product");

		static string ErrorMessage_FirstNumberIsEmpty
			=> Res.GetString("df90d35b-6caf-42ca-a630-d5aad0c8b450", "The Generate Serial Numbers action requires you to enter the first serial number in the sequence. This will allow the system to calculate the remaining serial numbers");

		static string ErrorMessage_WontProcessOneUnitLines
			=> Res.GetString("34b1c339-11b4-4eca-b1cc-9bf8afc124e5", "The Generate Serial Number action will only work for lines that have more than one unit. For lines with one unit (like this one), just enter the serial number directly into this field");

		static string ErrorMessage_InvalidNumericSuffix
			=> Res.GetString("bb0e5cc1-a21c-4db9-acca-54f624c5c588", "The Generate Serial Numbers action requires you to enter the first serial number in the sequence and this serial number must have a valid numeric suffix otherwise the system cannot calculate the remaining numbers. e.g. ABC100, SER-10, PS4232323");

		static string ErrorMessage_UnusualSuffixOnOneUnitLine
			=> Res.GetString("f896d26f-cde7-4dd7-8b20-28dd15faaaa5", "The Generate Serial Number action will only work for lines that have more than one unit. For lines with one unit (like this one), just enter the serial number directly into this field. The serial number you have entered does not have a numeric suffix, which is unusual. Please check it is entered correctly");

		string serialNumberSuffix;
		string serialNumberPrefix;
	}

	#endregion
}
