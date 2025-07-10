using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DataTransfer.Integration;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Shared;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.PermitService;
using Enterprise.Packing.Business;
using Enterprise.Packing.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Security;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.BondedWarehouse;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.Warehouse.Transactions.Business.Printing;
using Enterprise.Warehouse.Transactions.Business.Tools;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static CargoWise.EventReference.Constants;
using Constants = Enterprise.Core.Constants;
using GlowRegistry = Enterprise.Registry.Business.GlowRegistry;

namespace Enterprise.Warehouse.Transactions.Business
{
	[GlowDataDefinition("IWhsOrder")]
	[UniversalDataContext(DataContextType.WarehouseOrder)]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsOrder_EnsureDDLIsEnteredOnPick, WhsValidationHelper.WhsCheckOrderDockDoorLocationIsMatchWithPick, WhsDocketSchema.Constants.PK, typeof(IWhsCheckOrderDockDoorLocationIsMatchWithPick_DeferTriggerStrategy), ValueToRunStoredProcWith = ValueVersion.Current)]
	[Metadata.Integration.MetadataContext(Enterprise.Metadata.Integration.MetadataContext.WhsOrder)]
	public class WhsOrder :
		WhsPickableDocket,
		IWhsOrderWrapperStrategy,
		IWhsOrderWrapperCallback,
		IAttachedOrder,
		IAutoCreateAddressOnUnmatch,
		IBuyerSupplierRelationshipConsumer,
		ICartageLooseCargo,
		ICartageParent,
		IDtbBookingParent,
		IModuleToModule,
		IOrdersDocumentSupport,
		IPackingParentDefaultPackageType,
		IPackingParentWithPackableItems,
		IProcessHandlingInfoProvider,
		IRatingSupporter,
		IRelatableActivity,
		ITransportJobLinkProvider,
		IScreeningPartyProvider,
		IWhsOrder,
		IShouldUpdateScreeningStatus,
		IDeniedPartyProvider,
		ICustomizableNumberFountainConsumer,
		IConversationProvider,
		IConversationAdditionalParticipantProvider,
		IConversationParentHyperlinkProvider,
		IAllowAttachEmailsToEDocs
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
		public const string PreventChangeOfWarehouse = "Attempt to change warehouse for an order with Cross Dock Location/Reserved Stock or is processing/processed.";

		#region Schema

		public new abstract class Schema : WhsPickableDocket.Schema
		{
			public const string WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations = "WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations";
			public const string WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations = "WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations";
		}

		#endregion

		public WhsOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WD_WLO_PlannedLoad), ConcurrencyPolicy.Strict);
		}

		#region Strategies

		protected IWhsOrderWrapperStrategy wrapperStrategy;
		public IWhsOrderWrapperStrategy WrapperStrategy
		{
			get
			{
				if (wrapperStrategy == null)
				{
					var builder = ObjectFactory.Get<ITrackingOrderStrategyBuilder>();
					wrapperStrategy = builder.Build(this);
				}

				return wrapperStrategy;
			}
		}

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsOrderFetchStrategy(this);
		}

		#endregion

		#endregion

		#region BuyerSupplierLinksHelper

		BuyerSupplierLinksHelper<WhsOrder> BuyerSupplierLinksHelper
		{
			get
			{
				if (buyerSupplierLinksHelper == null)
				{
					buyerSupplierLinksHelper = new BuyerSupplierLinksHelper<WhsOrder>(this);
					buyerSupplierLinksHelper.Register();
				}

				return buyerSupplierLinksHelper;
			}
		}

		BuyerSupplierLinksHelper<WhsOrder> RegisterBuyerSupplierLink()
		{
			return BuyerSupplierLinksHelper;
		}

		BuyerSupplierLinksHelper<WhsOrder> buyerSupplierLinksHelper;

		#endregion

		#region BuyerSupplierLinks

		public bool ShouldPromptToSaveSupplierBuyerRelationship
		{
			get { return BuyerSupplierLinksHelper.ShouldPromptToSaveSupplierBuyerRelationship; }
		}

		public void AddNewBuyerSupplierLink()
		{
			using (new SemaphoreManager(IsAddingNewLinkSemaphore))
			{
				BuyerSupplierLinksHelper.AddNewBuyerSupplierLink();
			}
		}

		#region IsAddingNewLinkSemaphore

		Semaphore IsAddingNewLinkSemaphore
		{
			get { return isAddingNewLinkSemaphore ?? (isAddingNewLinkSemaphore = new Semaphore()); }
		}

		Semaphore isAddingNewLinkSemaphore;

		#endregion

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
				PrimaryTarget = new OrderIDGeneratorTarget(this),
			};
			generator.ValueProviders.AddRange(new StandardValueSource().Concat(new WarehouseJobValueSource(this)).Concat(new WarehouseOrderValueSource(this)));

			return generator;
		}

		#endregion

		#region Invoice Link

		public IInvoiceLink InvoiceLink
		{
			get { return fIInvoiceLink; }
			set
			{
				if (fIInvoiceLink != null)
				{
					throw new NotSupportedException("Cannot change InvoiceLinkProvider once set");
				}

				fIInvoiceLink = value;
			}
		}
		IInvoiceLink fIInvoiceLink;

		#endregion

		#region Customs Stuff

		public override bool IsCustomsTransaction
		{
			get { return WD_DocketSubType == OrderType.Codes.Customs || WD_DocketSubType == OrderType.Codes.CustomsReleaseWithPermit; }
		}

		protected override ZString DefaultDocketSubTypeForCustomsTransactionCore
		{
			get { return NeedCheckCustomsPermit ? OrderType.Codes.CustomsReleaseWithPermit : OrderType.Codes.Customs; }
		}

		bool NeedCheckCustomsPermit => !IsImportingData && BondedHelper.IsCountrySupportedForFTZPermits(CountryCode);

		#endregion

		#region Business Object Overrides

		#region Data Refresh

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			base.OnBeforeUpdatedByDataRefresh();
			StatusBeforeRefresh = WD_DocketStatus;
			PickBeforeRefresh = Pick;
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			if (IsConsigneeDocAddressInitialised && StatusBeforeRefresh.HasValue && StatusBeforeRefresh.Value != WarehouseOrderStatus)
			{
				ConsigneeDocAddress.RefreshBinding();
			}

			StatusBeforeRefresh = null;
			UpdateOrderedInventories();
			ClearPickAndReleaseLines();
		}

		ZString? StatusBeforeRefresh;
		WhsPick PickBeforeRefresh;

		void UpdateOrderedInventories()
		{
			var pick = Pick;
			if (pick != null)
			{
				pick.OrderedInventoriesNeedRefresh = true;
			}
		}

		void ClearPickAndReleaseLines()
		{
			if (PickBeforeRefresh != null && Pick == null)
			{
				PickPKJustBeforeDataRefreshForClearingCache = PickBeforeRefresh.PK;
				try
				{
					PickBeforeRefresh.Orders.Remove(this);
				}
				finally
				{
					PickBeforeRefresh = null;
					PickPKJustBeforeDataRefreshForClearingCache = null;
				}
			}
		}

		ZGuid? PickPKJustBeforeDataRefreshForClearingCache;

		internal ZGuid CurrentPickPKOrPickPKJustBeforeDataRefreshForClearingCache => PickPKJustBeforeDataRefreshForClearingCache ?? WD_WP;

		#endregion

		#region DocAddress Requirements

		protected override WhsDocketDocAddressValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			switch (CountryCode)
			{
				case Core.Constants.CountryCodes.UnitedStates:
					return new US.WhsOrderDocAddressValidation(addressToValidate);
				default:
					return new WhsOrderDocAddressValidation(addressToValidate);
			}
		}

		protected override SecurityCheckpoint GetCanOverrideAddressCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.WhsOrder;
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			WD_AddPalletWeightToOrder = WarehouseDataRegistry.Instance.AddPalletWeightToOrder.Value;

			WD_DocketType = CodeLists.DocketType.Codes.Order;
			WD_DocketSubType = CodeLists.OrderType.Codes.Order;
		}

		#endregion

		#region OnFactorySaving

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!WD_FinalisedDate.IsEmpty && WD_RequiredDate.IsEmpty)
			{
				RequiredDate = WD_FinalisedDate.ToZDateTime();
			}

			UpdateScreeningStatusFromScreeningParties();
		}

		void UpdateScreeningStatusFromScreeningParties()
		{
			ScreeningStatusUpdater.UpdateJobStatusFromItsScreeningParties(this, this, WD_ScreeningStatusInfo.OriginalValue?.ToString());
		}

		protected override bool ReloadRelatedJobsOnNextSave
		{
			get => BOM.ReloadRelatedJobsOnNextSave;
			set => BOM.ReloadRelatedJobsOnNextSave = value;
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();
			SubscribeToRelinquishPermitForDetachedOrder();

			WrapperStrategy.OrderOnSaving(); //All WhsOrder specific OnSaving code should be implemented in OrderOnSaving(). As WhsOrder OnSaving is run for both TrackingWhsOrder And WhsOrder
		}

		void SubscribeToRelinquishPermitForDetachedOrder()
		{
			if (IsFTZCustomsOrderWithPermit && IsInDatabase && WD_WPInfo.HasChanges && WD_WP.IsEmpty)
			{
				var relinquishPermitForDetachOrderService = Factory.ServiceContainer.GetAfterOnSavingService<RelinquishPermitForDetachOrderService>();
				if (relinquishPermitForDetachOrderService == null)
				{
					relinquishPermitForDetachOrderService = new RelinquishPermitForDetachOrderService();
					Factory.ServiceContainer.AddAfterOnSavingService(relinquishPermitForDetachOrderService);
				}

				relinquishPermitForDetachOrderService.SubscribeOrderThatNeedsToBeRelinquished(this);
			}
		}

		#region RelinquishPermitForDetachOrderService

		class RelinquishPermitForDetachOrderService : IAfterOnSavingBOProcessingService
		{
			public RelinquishPermitForDetachOrderService()
			{
				ReferenceNumbers = new Dictionary<ZString, WhsOrderLine>();
			}

			Dictionary<ZString, WhsOrderLine> ReferenceNumbers { get; }

			bool RollbackIfSaveFails;

			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				if (ReferenceNumbers.Count > 0 && ObjectFactory.Get<IPermitService>().RelinquishPermitTransactions(ReferenceNumbers.Values.Select(x => new WhsPermitTransactionDetail(x))) != SuccessOrFailure.Success)
				{
					Clear();
					throw new ZCannotSaveException(ResString.GetMultilingualString("87DD6EA1-4493-45CB-9340-55F0CE84C296", "The Order(s) you detached could not relinquish its Permits. Close and Re-open the form and try again."),
						Res.GetString("026708CE-8E46-475B-97F4-E3F38C95A44A", "Relinquish Permits Failed."), ExceptionType.BusinessFailure);
				}
				else
				{
					RollbackIfSaveFails = (ReferenceNumbers.Count > 0);
				}
			}

			public void SubscribeOrderThatNeedsToBeRelinquished(WhsOrder order)
			{
				foreach (WhsOrderLine orderLine in order.GetLinesToPick())
				{
					ReferenceNumbers.Add(WhsPermitWithdrawalRequestDetail.GetPermitTransactionRefNumber(orderLine), orderLine);
				}
			}

			public void Clear()
			{
				RollbackIfSaveFails = false;
				ReferenceNumbers.Clear();
			}

			public void Rollback()
			{
				try
				{
					if (RollbackIfSaveFails)
					{
						if (ReferenceNumbers.Count > 0)
						{
							int times = 0;
							const int retryTimes = 3;
							while (times < retryTimes && !ObjectFactory.Get<IPermitService>().TryGetPermits(ReferenceNumbers.Values.Select(l => new WhsPermitWithdrawRequest(l))).Success)
							{
								times++;
							}
						}
					}
				}
				finally
				{
					Clear();
				}
			}
		}

		#endregion

		#endregion

		#region OnSaved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			var relinquishPermitForDetachOrderService = Factory.ServiceContainer.GetAfterOnSavingService<RelinquishPermitForDetachOrderService>();
			if (relinquishPermitForDetachOrderService != null)
			{
				if (saveSucceeded)
				{
					relinquishPermitForDetachOrderService.Clear();
				}
				else
				{
					relinquishPermitForDetachOrderService.Rollback();
				}
			}

			if (saveSucceeded && PrintLabelsOnSave)
			{
				PrintAllLabelsOnSave();
			}
		}

		void PrintAllLabelsOnSave()
		{
			try
			{
				GetPackageJob()?.PrintAllLabels();
			}
			finally
			{
				PrintLabelsOnSave = false;
			}
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return WD_DocketID.IsEmpty ? HumanReadableNameWithoutID : ZString.Format("{0} {1}", HumanReadableNameWithoutID, WD_DocketID); }
		}

		protected override ZString HumanReadableNameWithoutID => Res.GetString("355ae270-e707-48cf-84a5-358e7dc26d8a", "Warehouse Order");

		#endregion

		#region RunPreSaveValidationCore

		protected override void RunPreSaveValidationCore()
		{
			// Tested in ReleaseEntryFormBasherTest.TestFinaliseOrderAndPickButtonPerformance_Click_WithStockCommittedToAdjustments
			// This prevents FetchForLoad() on Receive Lines adding GenAddOnColumn Fetch Hints.
			// Since we are never going to load the Original Hold Reason for Release Lines, we don't need these extra loads.
			using (CustomsValuesBusinessObjectStrategyHelper.SuspendCustomsValuesFetchHint(Factory, typeof(WhsReceiveLine)))
			{
				LoadAllReleaseLines();
			}

			var pickLinesQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_InventoryLine);
			var transactionLineQuery = new ZQuery { AllowTableValuedParameters = true };
			transactionLineQuery.AddToFilter(WhsPickLineSchema.WZ_WE_TransactionLine, Lines.Select(l => l.PK).ToArray());
			pickLinesQuery.AddToFilter(transactionLineQuery);

			var inventoryLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WL);
			inventoryLineSubQuery.AddSubQuery(pickLinesQuery, JoinCondition.And);

			var locationQuery = new ZDBOnlyQuery(typeof(WhsLocation));
			locationQuery.AddSubQuery(inventoryLineSubQuery, JoinCondition.And);

			Factory.AddFetchHint(WhsLocationViewSchema.Instance, locationQuery);

			Lines.Cast<WhsPickableDocketLine>().ForEach(l =>
			{
				l.ReservedPickLines.ForEach(pl =>
				{ Factory.AddFetchHint(WhsPickLineSchema.WZ_WE_InventoryLine, pl.WZ_WE_InventoryLine); });
			});

			if (!((IAttachedOrder)this).ShouldSkipAllValidations)
			{
				base.RunPreSaveValidationCore();
				Validation.ValidateConsigneeNameOrPK();
			}

			if (WD_DocketStatus == DocketStatus.Codes.Error && this.IsInDatabase && !HasErrorsIncludingLines)
			{
				WD_DocketStatus = DocketStatus.Codes.Entered;
			}

			if (!HasErrors && IsSavedFromOrderForm)
			{
				var pick = Pick;
				if (pick != null && !pick.IsMultiOrderPick && !IsUSBonded)
				{
					SyncPickWithOrder();
				}
			}

			var packageJob = Factory.LoadTop1<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, PK));
			if (packageJob != null)
			{
				var packagePKs = packageJob.Packages.Select(p => p.PK).ToArray();
				Factory.AddFetchHint(WhsPickTrolleySlotSchema.Instance, new ZQuery(WhsPickTrolleySlotSchema.WTS_KP_Package, packagePKs));
				Factory.AddFetchHint(WhsPickByLabelLabelSchema.Instance, new ZQuery(WhsPickByLabelLabelSchema.WTL_KP_Package, packagePKs));

				foreach (var packagePK in packagePKs)
				{
					var pivotQuery = new ZQuery(WhsLoadPkgPackagePivotSchema.WLP_KP_Package, packagePK);
					pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_UnloadedTime, SQLComparisonOperator.Equal, null);
					Factory.AddFetchHint(WhsLoadPkgPackagePivotSchema.Instance, pivotQuery);
				}
			}
		}

		/// <summary>
		/// This will load all the Non-Persistent Release Lines for Validation during Order Finalisation.
		/// </summary>
		void LoadAllReleaseLines()
		{
			if (IsFinalising)
			{
				foreach (WhsOrderLine orderLine in ParentLines)
				{
					// We need to build all the release lines for Validation to work. This will build the Release Lines even if they have been cleared.
					orderLine.BuildReleaseLines();
				}
			}
		}

		#endregion

		#region Notes

		protected override StmNoteContexts NoteContextsForRelatedNotes
		{
			get
			{
				StmNoteContexts result = base.NoteContextsForRelatedNotes;

				result.Module |= StmNoteContextModule.W;
				result.Direction |= StmNoteContextDirection.O;
				result.FreightMode |= StmNoteContextFreightMode.O;

				return result;
			}
		}

		public override BusinessObject[] BusinessObjectsWithRelatedNotes
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedNotes);

				var consigneeOrg = ConsigneeDocAddress.Organisation;
				if (consigneeOrg != null)
				{
					result.Add(consigneeOrg);
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

		#region BusinessObjectsWithRelatedEvents

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);

				result.AddRange(TransportBookingLoader.GetRelatedTransportBookingEvents(this));

				var cartageJob = CartageJob;
				if (cartageJob != null)
				{
					result.Add((BusinessObject)cartageJob);
				}

				result.AddRange(GetRelatedParents());

				var packageJob = PackageJob;
				if (packageJob != null)
				{
					result.AddRange(PackageJob.Packages);
				}

				return result.ToArray();
			}
		}

		#endregion

		#region Overrided Events

		protected override bool DoesNotHaveJobEnteredEvent
		{
			get
			{
				return WrapperStrategy.DoesNotHaveJobEnteredEvent;
			}
		}

		#endregion

		#endregion

		#region Related Entities

		#region CarrierServiceLevel

		public override OrgCarrierServiceLevel CarrierServiceLevel => this.GetCarrierServiceLevel(WD_PL_NKCarrierServiceLevel);

		#endregion

		#region Lines

		[ChildEditable]
		public new WhsOrderLineCollection Lines
		{
			get { return (WhsOrderLineCollection)base.Lines; }
		}

		[ChildEditable]
		public new WhsOrderLineCollection LinesToPickForBinding
		{
			get { return (WhsOrderLineCollection)base.LinesToPickForBinding; }
		}

		[ChildEditable]
		public new WhsOrderLineCollection AllLines
		{
			get { return (WhsOrderLineCollection)base.AllLines; }
		}

		[ChildEditable]
		public WhsOrderLineCollection ParentLines
		{
			get
			{
				if (parentLines == null)
				{
					parentLines = GetNewWhsOrderLineCollectionWithoutChildLines();
					RegisterEditableChildObject(parentLines);
				}
				return parentLines;
			}
		}

		WhsOrderLineCollection parentLines;

		WhsOrderLineCollection GetNewWhsOrderLineCollectionWithoutChildLines()
		{
			var result = GetNewParentLinesCollection();
			result.CollectionCountChange += Lines_CountChanged;
			return result;
		}

		void Lines_CountChanged(object sender, EventArgs e)
		{
			UpdatePackableItemParents();
			if (!IsUpdatingPickOrderedInventoriesSuspended
				&& !IsDeleted) // Cannot delete the order functionally but may be possible in code and done in some tests
			{
				UpdateOrderedInventories();
			}
		}

		void UpdatePackableItemParents()
		{
			if (packableItemParents != null)
			{
				using (packableItemParents.SuspendCountChangedForPacking())
				{
					var currentReleaseLines = new HashSet<WhsReleaseLine>();

					foreach (var releaseLine in GetAllReleaseLines())
					{
						packableItemParents.Add(releaseLine); // will only add if the collection does not contain the release line
						currentReleaseLines.Add(releaseLine);
					}

					// remove release lines no longer valid
					for (int index = packableItemParents.Count - 1; index >= 0; index--)
					{
						var releaseLine = packableItemParents[index];
						if (!currentReleaseLines.Contains(releaseLine))
						{
							packableItemParents.Remove(releaseLine);
						}
					}
				}
			}
		}

		protected virtual WhsOrderLineCollection GetNewParentLinesCollection()
		{
			return new WhsOrderLineCollectionWithoutChildLines(this);
		}

		protected override WhsPickableDocketLine[] LinesForSelectedOrderLines()
		{
			return WrapperStrategy.LinesForSelectedOrderLines();
		}

		protected override sealed WhsPickableDocketLineCollection GetNewPickableDocketLineCollection()
		{
			return new WhsOrderLineCollection(this);
		}

		protected override WhsPickableDocketLineCollection GetNewAllLines()
		{
			return new WhsOrderLineCollection(this);
		}

		protected override WhsPickableDocketLineCollection GetLinesToPickCore()
		{
			return Lines;
		}

		protected override WhsPickableDocketLineCollection GetParentLinesToPickForBinding()
		{
			return ParentLines;
		}

		#endregion

		#region AllocatedReceipts

		public WhsDocketCollectionAdHoc AllocatedReceipts
		{
			get
			{
				if (allocatedReceipts == null)
				{
					allocatedReceipts = new WhsDocketCollectionAdHoc(Factory);

					if (WD_WP.IsEmpty)
					{
						allocatedReceipts.AddRange(Factory.Load<WhsDocket>(AllocatedReceiptsQuery));
					}

					allocatedReceipts.SetReadOnlyIncludingChildren(true);
				}

				return allocatedReceipts;
			}
		}

		ZQuery AllocatedReceiptsQuery
		{
			get
			{
				var orderLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsPickLineSchema.WZ_WE_TransactionLine);
				orderLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, PK);

				var pickLineQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_InventoryLine);
				pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_OriginalReservedQty, SQLComparisonOperator.GreaterThan, 0m);
				pickLineQuery.AddSubQuery(orderLineQuery, JoinCondition.And);

				var inventoryLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
				inventoryLineQuery.AddSubQuery(pickLineQuery, JoinCondition.And);

				var query = new ZDBOnlyQuery(typeof(WhsDocket));
				query.AddSubQuery(inventoryLineQuery, JoinCondition.And);

				return query;
			}
		}

		WhsDocketCollectionAdHoc allocatedReceipts;

		#endregion

		#region CartonGroup

		public CartonGroupResult CartonGroup => GetFirstCartonGroupFKOffOrgs();

		CartonGroupResult GetFirstCartonGroupFKOffOrgs()
		{
			var cartonGroupSequence = WarehouseDataRegistry.Instance.CartonGroupSequence.GetValueWithoutFallback(Guid.Empty, Warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty);
			CartonGroupResult cartonGroupResult = null;
			var orgGetters = GetCartonGroupGetters(cartonGroupSequence);
			foreach (var orgGetter in orgGetters.OrderBy(g => g.Key))
			{
				var org = orgGetter.Value();
				var miscServ = org?.MiscServ;

				if (miscServ != null && !miscServ.OM_WCG_CartonGroup.IsEmpty)
				{
					var fkToCartonGroup = miscServ.OM_WCG_CartonGroup;
					cartonGroupResult = new CartonGroupResult(fkToCartonGroup, orgGetter.Key > cartonGroupSequence.Product);
					break;
				}
			}

			return cartonGroupResult ?? new CartonGroupResult(ZGuid.Empty, cartonGroupSequence.Product > 0);
		}

		Dictionary<int, Func<OrgHeader>> GetCartonGroupGetters(CartonGroupSequence cartonGroupSequence)
		{
			var orgGetters = new Dictionary<int, Func<OrgHeader>>();

			if (cartonGroupSequence.Carrier > 0)
			{
				orgGetters[cartonGroupSequence.Carrier] = this.GetTransportCo;
			}

			if (cartonGroupSequence.Consignee > 0)
			{
				orgGetters[cartonGroupSequence.Consignee] = () => Consignee;
			}

			if (cartonGroupSequence.Client > 0)
			{
				orgGetters[cartonGroupSequence.Client] = () => Client;
			}

			if (cartonGroupSequence.Warehouse > 0)
			{
				orgGetters[cartonGroupSequence.Warehouse] = () => Warehouse?.WarehouseAddress?.Header;
			}

			return orgGetters;
		}

		#endregion

		#region CartageJob

		public ICommonCartage CartageJob
		{
			get { return Factory.LoadTop1<ICommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, PK)); }
		}

		#endregion

		#region ConsigneeDocAddress

		protected override void BeforeOnConsigneeDocAddressChanged()
		{
			base.BeforeOnConsigneeDocAddressChanged();
			RegisterBuyerSupplierLink();
		}

		protected override void ConsigneeOrgAddressBeforeChangeCore(JobDocAddress docAddress)
		{
			SetShouldDefaultTransportAndTransportBillToCompanies();
			SetShouldDefaultDistributionCenter();
		}

		protected override void OnConsigneeDocAddressChangedCore()
		{
			base.OnConsigneeDocAddressChangedCore();

			SetDefaultINCOTerm();
			SetDefaultTransportAndTransportBillToCompanies();
			SetDefaultDistributionCentre();
			SetDefaultDropMode();
			SetDefaultContainerMode();
			SetDefaultFreightForwarder();
			SetRateTransportZone();
		}

		protected override bool IsOnlyTheConsigneeOrgReadOnly
		{
			get { return ConsigneeDocAddress.ReadOnly || ((IsAttachedToPick || IsFinalised) && UsesExpiryDate); }
		}

		protected override IJobDocAddressReadOnlyStrategy GetConsigneeDocAddressReadOnlyStrategy()
		{
			Func<bool> readOnly = () => ReadOnly || NonStandardReadOnly1;
			Func<bool> organisationPKReadOnly = () => IsOnlyTheConsigneeOrgReadOnly;
			return new JobDocAddressReadOnlyStrategy(readOnly, organisationPKReadOnly);
		}

		bool UsesExpiryDate
		{
			get
			{
				var client = Client;
				return client != null && Lines.Any(l => l.Product != null && l.Product.IsExpiryDateUsed(client));
			}
		}

		#endregion

		#region PackageJob

		public PkgPackageJob PackageJob
		{
			get { return PkgPackageJob.LoadPackageJob(this); }
		}

		PkgPackageJob GetPackageJob() => Factory.LoadTop1<PkgPackageJob>(new ZQuery(PkgPackageJobSchema.KJ_ParentID, PK));

		void Packages_CollectionCountChange(object sender, CollectionCountChangedEventArgs e)
		{
			var packageJob = GetPackageJob();
			if (packageJob != null)
			{
				if (e.ItemRemoved || e.ItemAdded) // occurs when adding, removing or changing outer to inner or inner to outer
				{
					if (!PreviousPackageCount.HasValue)
					{
						// if we're removing a package without Count initialised we can assume it was the current Count + 1
						if (e.ItemRemoved)
						{
							PreviousPackageCount = packageJob.Packages.Count + 1;
						}
						// if we're adding a package without Count initialised and the package is not yet in the collection, Current Count is correct
						else if (e.ItemAdded && e.BizObject != null && !packageJob.Packages.Contains((PkgPackage)e.BizObject))
						{
							PreviousPackageCount = packageJob.Packages.Count;
						}
					}

					UpdatePackageData(packageJob);
				}

				// just adding a package will not change the weight or volume, setting the weight or volume on the new package will
				if (e.ItemRemoved)
				{
					UpdateWeightFromPackages(packageJob, packageBeingAdded: null);
					UpdateVolumeFromPackages(packageJob, packageBeingAdded: null);
				}
			}
		}

		void PackageJob_PackageDataChanged(object sender, PackageDataChangeEventArgs e)
		{
			if (e.DataChangeType == PackageDataChangeType.PackageContent) // occurs when changing package data or adding a new package
			{
				UpdatePackageData(GetPackageJob());
			}
			else if (e.DataChangeType == PackageDataChangeType.Weight)
			{
				UpdateWeightFromPackages(GetPackageJob(), e.Package);
			}
			else if (e.DataChangeType == PackageDataChangeType.Volume)
			{
				UpdateVolumeFromPackages(GetPackageJob(), e.Package);
			}
		}

		void PackageJob_MassPackageProcessFinished(object sender, EventArgs e)
		{
			UpdatePackageDataAndWeightAndVolume();
		}

		void UpdateWeightFromPackages(PkgPackageJob packageJob, PkgPackage packageBeingAdded)
		{
			if (IsPackingUpdatesAllowed(packageJob))
			{
				GrossWeightSent = GetGrossWeight(packageJob, packageBeingAdded);

				if (UsePackingWeightAndVolume)
				{
					WD_WeightSent = GrossWeightSent;
				}
			}
		}

		void UpdateVolumeFromPackages(PkgPackageJob packageJob, PkgPackage packageBeingAdded)
		{
			if (IsPackingUpdatesAllowed(packageJob))
			{
				GrossVolumeSent = GetGrossVolumeSent(packageJob, packageBeingAdded);

				if (UsePackingWeightAndVolume)
				{
					WD_CubicSent = Math.Max(NetVolumeSent, GrossVolumeSent);
				}
			}
		}

		public void UpdatePackageDataAndWeightAndVolume()
		{
			var packageJob = GetPackageJob();
			UpdatePackageData(packageJob);
			UpdateWeightFromPackages(packageJob, packageBeingAdded: null);
			UpdateVolumeFromPackages(packageJob, packageBeingAdded: null);
		}

		void UpdatePackageData(PkgPackageJob pkgJob)
		{
			if (IsPackingUpdatesAllowed(pkgJob))
			{
				var packages = pkgJob.Packages;
				if (packages.Count > 0 || PreviousPackageCount > 0)
				{
					UpdatePackageSentInfo();
				}

				// default UsePackingWeightAndVolume to true if they add their first Package(s)
				if (PreviousPackageCount == 0 && packages.Count > 0)
				{
					UsePackingWeightAndVolume = true;
				}
				// reset WD_AddPalletWeightToOrder if they remove the last Package(s)
				else if (PreviousPackageCount > 0 && packages.Count == 0)
				{
					WD_AddPalletWeightToOrder = WarehouseDataRegistry.Instance.AddPalletWeightToOrder.Value;
					WD_CubicSent = NetVolumeSent;
				}

				PreviousPackageCount = packages.Count;

				void UpdatePackageSentInfo()
				{
					var packTypeIsPalletCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
					var packagesSum = 0;
					var palletsSum = (short)0;
					var packTypesList = new List<ZString>(2);

					foreach (var package in packages)
					{
						var packType = package.KP_F3_NKPackType;
						if (IsPalletPackType(packTypeIsPalletCache, packType))
						{
							palletsSum += (short)package.KP_PackageQty;
						}
						else
						{
							if (packTypesList.Count < 2 && !packTypesList.Any(p => packType.EqualsIgnoringCase(p)))
							{
								packTypesList.Add(packType);
							}

							packagesSum += package.KP_PackageQty;
						}
					}

					WD_PackagesSent = packagesSum;
					WD_PalletsSent = palletsSum;
					WD_F3_NKTotalPackType = packTypesList.Count == 1 ? packTypesList[0] : ZString.Empty;
				}

				bool IsPalletPackType(Dictionary<string, bool> packTypeIsPalletCache, ZString packType)
				{
					if (!packTypeIsPalletCache.TryGetValue(packType, out var result))
					{
						packTypeIsPalletCache[packType] = result = CheckIsPalletPackType(packType);
					}

					return result;
				}
			}
		}

		int? PreviousPackageCount;

		bool IsPackingUpdatesAllowed(PkgPackageJob packageJob) => packageJob != null && !packageJob.IsMassPackageProcessRunning && !SuspendPackingUpdateSemaphore.IsSuspended;

		public Semaphore SuspendPackingUpdateSemaphore => suspendPackingUpdateSemaphore ?? (suspendPackingUpdateSemaphore = new Semaphore());
		Semaphore suspendPackingUpdateSemaphore;

		#endregion

		#region CheckIsPalletPackType

		public bool CheckIsPalletPackType(ZString packType)
		{
			var result = packType.EqualsIgnoringCase(Constants.PkgUnit.Pallet);

			if (!result)
			{
				var refPackType = Factory.LoadFromNaturalKey<RefPackType>(RefPackTypeSchema.F3_Code, packType);
				result = refPackType != null && refPackType.F3_UOMType == UOMPackTypesList.Codes.Pallet;
			}

			return result;
		}

		#endregion

		#region RelatedBackOrder

		public WhsOrder RelatedBackOrder
		{
			get { return Factory.LoadTop1<WhsOrder>(GetRelatedBackOrdersFilter()); }
		}

		ZQuery GetRelatedBackOrdersFilter()
		{
			var query = new ZQuery();
			query.AddToFilter(WhsDocketSchema.WD_WD_ParentDocket, PK);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, WD_OH_Client);
			query.AddToFilter(WhsDocketSchema.WD_ExternalReference, WD_ExternalReference);
			query.AddToFilter(WhsDocketSchema.WD_DocketSubType, OrderType.Codes.BackOrder);
			query.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);

			return query;
		}

		#endregion

		#region RelatedReceives

		internal WhsReceive[] RelatedReceives => Factory.Load<WhsReceive>(GetRelatedReceivesQuery(PK));

		public static ZQuery GetRelatedReceivesQuery(ZGuid orderPK)
		{
			var query = new ZQuery(WhsDocketSchema.WD_DocketType, DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.WD_WD_ParentDocket, orderPK);
			return query;
		}

		public bool HasRelatedReturnReceive => RelatedReceives.Length > 0;

		#endregion

		#region SupplierBuyerLink

		public OrgSupplierBuyerLink SupplierBuyerLink
		{
			get
			{
				var destination = InvoicingSupporter.Destination;
				return OrgSupplierBuyerLink.GetExistingOrgSupplierBuyerLink(InvoicingSupporter.Consignor, Consignee, destination != null ? destination.RL_RN_NKCountryCode : ZString.Empty);
			}
		}

		#endregion

		#region TransportCoDocAddress

		void OnTransportCoDocAddressChanged(object sender, EventArgs e)
		{
			transportCoDocAddress.DocAddressChanged -= OnTransportCoDocAddressChanged;
			transportCoDocAddress = this.GetTransportCoDocAddress(ref transportCoDocAddress);
			SetRateTransportZone();
			transportCoDocAddress.DocAddressChanged += OnTransportCoDocAddressChanged;
		}

		protected override JobDocAddress TransportCoDocAddressCore
		{
			get
			{
				if (transportCoDocAddress == null || transportCoDocAddress.IsDeleted)
				{
					transportCoDocAddress = this.GetTransportCoDocAddress(ref transportCoDocAddress);
					previousTranportCoPK = transportCoDocAddress.OrganisationPK;
					transportCoDocAddress.DocAddressChanged += OnTransportCoDocAddressChanged;
				}
				Func<bool> readOnly = () => IsOrderAssignedToLoad || WarehouseOrderStatus == WhsOrderStatus.Codes.ReadyToPack;
				transportCoDocAddress.OrganisationPKInfo.ValueChanged += OnTranportCoPKValueChanged;
				transportCoDocAddress.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(readOnly);
				return transportCoDocAddress;
			}
		}
		JobDocAddress transportCoDocAddress;

		#endregion

		#region DistributionCentreAddressPK

		public ZGuid DistributionCentreAddressPK
		{
			get => !DistributionCentreDocAddress.E2_AddressOverride ? DistributionCentreDocAddress.E2_OA_Address : ZGuid.Empty;
			set
			{
				DistributionCentreDocAddress.E2_OA_Address = value;
				ValidateDistributionCentrePK();
			}
		}

		void ValidateDistributionCentrePK()
		{
			if (!base.IsValidationSuspended && !DistributionCentreDocAddress.IsValidationSuspended)
			{
				DistributionCentreDocAddress.Validation.ValidateOrganisationPK();
			}
		}

		#endregion

		#region LoadID

		[ResourceStringData("WhsOrder|LoadID", Caption = "Load ID", ShortCaption = "ID")]
		public ZString LoadID
		{
			get
			{
				var loadOrderQuery = new ZQuery(WhsLoadOrderSchema.WOV_WD_Docket, PK) { MaximumRows = 2 };
				var loadOrders = Factory.Load<WhsLoadOrder>(loadOrderQuery);
				var result = ZString.Empty;
				if (loadOrders.Length == 1)
				{
					result = loadOrders[0].Load.WLO_JobID;
				}
				else if (loadOrders.Length > 1)
				{
					result = Res.GetString("b3e98ae0-2184-d802-b5f3-439317137c6c", "Many");
				}
				return result;
			}
		}

		public ZPropertyInfo LoadIDInfo => GetZPropertyInfo(nameof(LoadID));

		#endregion

		#region CarrierBookingAgent

		public JobDocAddress CarrierBookingAgentDocAddress
		{
			get
			{
				if (carrierBookingAgentDocAddress == null || carrierBookingAgentDocAddress.IsDeleted)
				{
					carrierBookingAgentDocAddress = DocAddresses.FindOrCreateWithRequirement(CarrierBookingAgentDocAddressRequirement);
				}
				return carrierBookingAgentDocAddress;
			}
		}
		JobDocAddress carrierBookingAgentDocAddress;

		public OrgHeader CarrierBookingAgent => CarrierBookingAgentDocAddress.Organisation;

		public ZGuid CarrierBookingAgentPK => CarrierBookingAgentDocAddress.OrganisationPK;

		#endregion

		#region DestinationWarehouseDocAddress

		public JobDocAddress DestinationWarehouseDocAddress
		{
			get
			{
				if (destinationWarehouseDocAddress == null || destinationWarehouseDocAddress.IsDeleted)
				{
					destinationWarehouseDocAddress = DocAddresses.FindOrCreateWithRequirement(DestinationWarehouseDocAddressRequirement);
				}
				return destinationWarehouseDocAddress;
			}
		}
		JobDocAddress destinationWarehouseDocAddress;

		#endregion

		#region ReturnDocAddress

		public JobDocAddress ReturnDocAddress
		{
			get
			{
				if (returnDocAddress == null || returnDocAddress.IsDeleted)
				{
					returnDocAddress = DocAddresses.FindOrCreateWithRequirement(ReturnDocAddressRequirement);
				}
				return returnDocAddress;
			}
		}
		JobDocAddress returnDocAddress;

		#endregion

		#region SalesChannel

		public WhsSalesChannel SalesChannel => Factory.Load<WhsSalesChannel>(WD_WSH_SalesChannel);

		protected override ZString SalesChannelCodeCore => SalesChannel?.WSH_Code ?? string.Empty;

		#endregion

		#region PlannedLoad

		public WhsLoad PlannedLoad => Factory.Load<WhsLoad>(WD_WLO_PlannedLoad);

		#endregion

		#endregion

		#region Validation

		public new WhsOrderValidation Validation
		{
			get { return (WhsOrderValidation)base.Validation; }
		}

		protected override WhsDocketValidation GetNewValidation()
		{
			return WrapperStrategy.GetNewValidation();
		}

		void OnTranportCoPKValueChanged(object sender, EventArgs e)
		{
			previousTranportCoPK = e is ValueChangedEventArgs args ? (ZGuid?)args.OldValue : ZGuid.Empty;
		}

		#region ValidationForFinalisationWithFutureFinalisedDate

		class ValidationForFinalisationWithFutureFinalisedDate : ZValidation
		{
			public ValidationForFinalisationWithFutureFinalisedDate(WhsOrder order)
				: base(order)
			{
			}

			WhsOrder Parent => (WhsOrder)ParentFilter;

			public void ValidateWD_DocketStatusDescription()
			{
				ValidateCalculatedProperty(Parent.WD_DocketStatusDescriptionInfo);
			}

			protected void CheckWD_DocketStatusDescription()
			{
				if (!Parent.HasErrors)
				{
					var warehouse = Parent.Warehouse;
					if (warehouse.WW_UseRequiredDateForOutwardsFinalisedDate)
					{
						var registryOn = WarehouseDataRegistry.Instance.AllowWarehouseOrderFinalisationDateUpTo30DaysInTheFuture.GetFallBackValueAtAllLevels(Guid.Empty, warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), Guid.Empty);
						var parentFinalisedDate = Parent.GetFinalisedDate().ToZDateTime();
						var today = ZDateTime.Today;
						if ((!registryOn && parentFinalisedDate >= today.AddDays(One)) || (registryOn && parentFinalisedDate >= today.AddDays(ThirtyOne)))
						{
							Parent.WD_DocketStatusDescriptionInfo.AddError(Res.GetString("19f0d612-11c9-43ca-8565-50351383cacb", @"Finalized Date is currently an incorrect future date.
Finalized Date is being set by Required Date since the Warehouse option 'Use Required Date For Outwards Finalized Date' is enabled for the current Warehouse.
For valid future finalization dates less than 30 days in the future the registry setting 'Allow Finalization Date Up To 30 Days In The Future' can be enabled for this warehouse's branch."));
						}
					}
				}
			}

			const int One = 1;
			const int ThirtyOne = 31;

			public override Type AutoValidationType => typeof(ValidationForFinalisationWithFutureFinalisedDate);

			public override void ValidateAll()
			{
				throw new InvalidOperationException("Should not be calling ValidateAll() on ValidationForFinalisationWithFutureFinalisedDate.");
			}
		}

		#endregion

		#endregion

		#region Lookups

		protected override WhsDocketLookups GetNewLookups()
		{
			return WrapperStrategy.GetNewLookups();
		}

		public new WhsOrderLookups Lookups
		{
			get { return (WhsOrderLookups)base.Lookups; }
		}

		#endregion

		#region Properties

		#region ClientPickingParams

		public WhsClientPickPackParamsByWhs ClientPickingParams
		{
			get
			{
				WhsClientPickPackParamsByWhs result = null;

				var client = Client;
				if (client != null)
				{
					result = WhsClientPickingParams.GetClientPickingParams(client).WarehousePickPackParams
						.Where(p => p.WPP_WW_Warehouse == WD_WW_Whs && (p.WPP_WSH_SalesChannel.IsEmpty || p.WPP_WSH_SalesChannel == WD_WSH_SalesChannel))
						.OrderBy(p => p.WPP_WSH_SalesChannel.IsEmpty)
						.FirstOrDefault();
				}

				return result;
			}
		}

		#endregion

		#region Description

		protected override ZString DescriptionCore
		{
			get { return Res.GetString("86be1873-8964-4950-96d7-983a4da24c9c", "Order"); }
		}

		#endregion

		#region WD_AddPalletWeightToOrder

		public override ZBool WD_AddPalletWeightToOrder
		{
			get => base.WD_AddPalletWeightToOrder;
			set
			{
				base.WD_AddPalletWeightToOrder = value;

				if (!HasPackages)
				{
					UpdateWeightSent();
				}
			}
		}

		#endregion

		#region WD_FinalisedDate

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WD_FinalisedDate
		{
			get => base.WD_FinalisedDate;
			set
			{
				base.WD_FinalisedDate = value;
				SetCustomsDataReadOnly();
			}
		}

		#endregion

		#region WD_CubicSent

		protected override bool CubicSentReadOnly => base.CubicSentReadOnly || HasPackages;

		bool HasPackages => WD_WP.IsValid && GetPackageJob()?.Packages.Count > 0;

		#endregion

		#region WD_IsLoadingRequired

		[ReadOnlyMember(nameof(IsFinalisedOrCancelled))]
		public override ZBool WD_IsLoadingRequired
		{
			get => base.WD_IsLoadingRequired;
			set => base.WD_IsLoadingRequired = value;
		}

		#endregion

		#region CarrierServiceLevelReadOnly

		protected override bool CarrierServiceLevelReadOnly => base.CarrierServiceLevelReadOnly || IsOrderAssignedToLoad || WarehouseOrderStatus == WhsOrderStatus.Codes.ReadyToPack;

		#endregion

		#region WD_PalletsSent

		public override ZShort WD_PalletsSent
		{
			get => base.WD_PalletsSent;
			set
			{
				base.WD_PalletsSent = value;

				if (!HasPackages)
				{
					UpdateWeightSent();
				}
			}
		}

		#endregion

		#region WD_WeightSentUserEntered

		public override ZDecimal WD_WeightSentUserEntered
		{
			get => base.WD_WeightSentUserEntered;
			set
			{
				base.WD_WeightSentUserEntered = value;

				if (!UsePackingWeightAndVolume || !HasPackages)
				{
					UpdateWeightSent();
				}
			}
		}

		#endregion

		#region WD_WeightSent

		void UpdateWeightSent()
		{
			if (!IsDetachingFromPick)
			{
				if (WD_AddPalletWeightToOrder)
				{
					WD_WeightSent = WD_WeightSentUserEntered + GetPalletsWeight();
				}
				else
				{
					WD_WeightSent = WD_WeightSentUserEntered;
				}
			}
		}

		ZDecimal GetPalletsWeight()
		{
			var packType = Factory.LoadFromUniqueKey<RefPackType>(RefPackTypeSchema.F3_Code, new ZString("PLT"));
			ZDecimal unitPalletWeight = Enterprise.Core.Constants.Weight.Convert(packType.F3_Weight, packType.F3_UnitOfWeight, WD_TotalWeightUnit);
			return WD_PalletsSent * unitPalletWeight;
		}

		#endregion

		#region UsePackingWeightAndVolume

		protected override void OnUsePackingWeightAndVolumeSet(bool value)
		{
			base.OnUsePackingWeightAndVolumeSet(value);

			if (value)
			{
				WD_WeightSent = GrossWeightSent;
				WD_CubicSent = Math.Max(NetVolumeSent, GrossVolumeSent);
			}
			else
			{
				WD_WeightSent = WD_WeightSentUserEntered;
				WD_CubicSent = NetVolumeSent;
			}
		}

		#endregion

		#region GrossWeight

		PkgPackageJob PackageJobForAllocatedOrder => WD_WP.IsValid ? PackageJob : null;

		protected override decimal GetGrossWeightSent() => GetGrossWeight(PackageJobForAllocatedOrder, packageBeingAdded: null);

		decimal GetGrossWeight(PkgPackageJob pkgJob, PkgPackage packageBeingAdded)
		{
			decimal result;

			if (pkgJob != null && (pkgJob.Packages.Count > 0 || packageBeingAdded != null))
			{
				var weightAmounts = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

				// add weight of anything not yet packed
				foreach (PackableItemParentWrapper wrapper in pkgJob.PackableItemParents)
				{
					if (wrapper.PackableItemParent != null)
					{
						MemoiseMeasure(weightAmounts, wrapper.PackableItemParent.WeightUQ, wrapper.UnpackedWeight);
					}
				}

				var packages = packageBeingAdded != null && !pkgJob.Packages.Contains(packageBeingAdded)
					? pkgJob.Packages.Append(packageBeingAdded)
					: pkgJob.Packages;

				foreach (var package in packages)
				{
					MemoiseMeasure(weightAmounts, package.KP_WeightUQ, package.KP_Weight);
				}

				result = CalculateGrossResult(weightAmounts, WD_TotalWeightUnit, Constants.Weight.Codes, (item) => Constants.Weight.Convert(item.Value, item.Key, WD_TotalWeightUnit), WhsDocketSchema.WD_WeightSent.Scale);
			}
			else
			{
				result = WD_WeightSentUserEntered;
			}

			return result;
		}

		void MemoiseMeasure(Dictionary<string, decimal> valueCacheByUQ, string uq, decimal value)
		{
			if (valueCacheByUQ.TryGetValue(uq, out var amount))
			{
				valueCacheByUQ[uq] = amount + value;
			}
			else
			{
				valueCacheByUQ.Add(uq, value);
			}
		}

		decimal CalculateGrossResult(Dictionary<string, decimal> amounts, string totalUnit, string[] validUnitCodes, Func<KeyValuePair<string, decimal>, decimal> conversionFunc, byte scale)
		{
			var enteredUnits = amounts.Keys.ToList();
			enteredUnits.Add(totalUnit);
			enteredUnits.RemoveAll(s => string.IsNullOrWhiteSpace(s));
			return !enteredUnits.Except(validUnitCodes).Any()
				? Utilities.Round(amounts.Sum(item => conversionFunc(item)), scale)
				: 0m;
		}

		#endregion

		#region NetVolumeSent

		decimal NetVolumeSent
		{
			get => netVolumeSent ?? (netVolumeSent = GetNetVolumeSent()).Value;
			set => netVolumeSent = value;
		}

		decimal GetNetVolumeSent()
		{
			var lines = Lines.Where(line => !line.HasErrors && line.WE_OP.IsValid).Select(line => new LineWithProductAndQuantity(line.WE_OP, line.SumOfUnitsMet)).ToArray();
			return UnitOfMeasureConverter.GetTotalQuantityFromLines(this, this.GetVolumeMeasure(), lines);
		}

		decimal? netVolumeSent;

		#endregion

		#region GrossVolumeSent

		decimal GrossVolumeSent
		{
			get => grossVolumeSent ?? (grossVolumeSent = GetGrossVolumeSent(PackageJobForAllocatedOrder, packageBeingAdded: null)).Value;
			set => grossVolumeSent = value;
		}

		decimal? grossVolumeSent;

		decimal GetGrossVolumeSent(PkgPackageJob pkgJob, PkgPackage packageBeingAdded)
		{
			decimal result;

			if (pkgJob != null && (pkgJob.Packages.Count > 0 || packageBeingAdded != null))
			{
				var volumeAmounts = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

				var packages = packageBeingAdded != null && !pkgJob.Packages.Contains(packageBeingAdded)
					? pkgJob.Packages.Append(packageBeingAdded)
					: pkgJob.Packages;

				foreach (var package in packages)
				{
					MemoiseMeasure(volumeAmounts, package.KP_VolumeUQ, package.KP_Volume);
				}

				result = CalculateGrossResult(volumeAmounts, WD_TotalCubicUnit, Constants.Volume.Codes, (item) => Constants.Volume.Convert(item.Value, item.Key, WD_TotalCubicUnit), WhsDocketSchema.WD_CubicSent.Scale);
			}
			else
			{
				result = 0m;
			}

			return result;
		}

		#endregion

		#region WD_TotalCubicUnit, WD_TotalWeightUnit

		public override ZString WD_TotalCubicUnit
		{
			get { return base.WD_TotalCubicUnit; }
			set
			{
				if (base.WD_TotalCubicUnit != value)
				{
					base.WD_TotalCubicUnit = value;
					CalculateTotals();
				}
			}
		}

		public override ZString WD_TotalWeightUnit
		{
			get { return base.WD_TotalWeightUnit; }
			set
			{
				if (base.WD_TotalWeightUnit != value)
				{
					base.WD_TotalWeightUnit = value;
					CalculateTotals();
				}
			}
		}

		void CalculateTotals()
		{
			// Strange that we do these calculations on the Order when it is not allocated since all fields are only seen on Release.
			// This should be looked into and perhaps this functionality should only run if the Order has a Pick.
			if (CalculateTotalsEnabled)
			{
				var lines = Lines.Where(line => !line.HasErrors && line.WE_OP.IsValid).Select(line => new LineWithProductAndQuantity(line.WE_OP, line.SumOfUnitsMet)).ToArray();
				WD_UnitsSent = lines.Sum(line => line.Quantity);

				var packageJob = PackageJobForAllocatedOrder;
				var netVolume = UnitOfMeasureConverter.GetTotalQuantityFromLines(this, this.GetVolumeMeasure(), lines);
				var grossVolume = GetGrossVolumeSent(packageJob, packageBeingAdded: null);
				NetVolumeSent = netVolume;
				GrossVolumeSent = grossVolume;

				var usePackingWeightAndVolume = UsePackingWeightAndVolume && HasPackages;
				WD_CubicSent = usePackingWeightAndVolume ? Math.Max(netVolume, grossVolume) : netVolume;
				WD_WeightSentUserEntered = UnitOfMeasureConverter.GetTotalQuantityFromLines(this, this.GetWeightMeasure(), lines);
				GrossWeightSent = GetGrossWeight(packageJob, packageBeingAdded: null);

				if (usePackingWeightAndVolume)
				{
					WD_WeightSent = GrossWeightSent;
				}
			}
		}

		#region DelayUpdatingReleaseTotals

		protected override IDisposable DelayUpdatingReleaseTotalsCore() => new UpdatingReleaseTotalsDelayer(this);

		#region UpdatingReleaseTotalsDelayer

		class UpdatingReleaseTotalsDelayer : IDisposable
		{
			public UpdatingReleaseTotalsDelayer(WhsOrder order)
			{
				Order = Argument.NotNull(order, nameof(order));
				SemaphoreSuspender = new SemaphoreManager(order.DelayUpdatingReleaseTotalsSemaphore);
				_ = Order.NetVolumeSent; // in case NetVolume hasn't been initialised, calculate it here so the Dispose method updates it correctly
			}

			readonly WhsOrder Order;
			readonly IDisposable SemaphoreSuspender;

			void IDisposable.Dispose()
			{
				SemaphoreSuspender.Dispose();

				if (!Order.DelayUpdatingReleaseTotalsSemaphore.IsSuspended && !Order.IsUpdatingReleaseTotalsSuspended)
				{
					try
					{
						foreach (var qtyDelta in Order.QtyChangesPerProductForPickingCache)
						{
							Order.UpdateReleaseTotals(qtyDelta.Key, qtyDelta.Value);
						}
					}
					finally
					{
						Order.QtyChangesPerProductForPickingCache.Clear();
					}
				}
			}
		}

		Semaphore DelayUpdatingReleaseTotalsSemaphore
		{
			get { return delayUpdatingReleaseTotalsSemaphore ?? (delayUpdatingReleaseTotalsSemaphore = new Semaphore()); }
		}

		Semaphore delayUpdatingReleaseTotalsSemaphore;

		#endregion

		#endregion

		#region UpdateReleaseTotals

		protected override void UpdateReleaseTotalsCore(OrgSupplierPart part, ZDecimal quantity)
		{
			base.UpdateReleaseTotalsCore(part, quantity);

			if (DelayUpdatingReleaseTotalsSemaphore.IsSuspended)
			{
				UpdateChangesCache(part, quantity);
			}
			else
			{
				UpdateReleaseQuantities(part, quantity);
			}
		}

		/// <summary>
		/// If Updating Release Totals is Suspended, we add the qty to update to the total sum.
		/// This is then stored in the cache, per product.
		/// This is because unit conversions are costly, and so they will be suspended until the Delayer (semaphore) is released.
		/// Once Suspension has finished, the cache is then used to unit convert each unique product qty to the Release Totals on the Docket.
		/// </summary>
		void UpdateChangesCache(OrgSupplierPart part, ZDecimal quantity)
		{
			if (QtyChangesPerProductForPickingCache.TryGetValue(part, out var result))
			{
				QtyChangesPerProductForPickingCache[part] = result + quantity;
			}
			else
			{
				QtyChangesPerProductForPickingCache[part] = quantity;
			}
		}

		void UpdateReleaseQuantities(OrgSupplierPart part, ZDecimal quantity)
		{
			WD_UnitsSent = Math.Max(0m, WD_UnitsSent + quantity);

			if (!WD_WeightVolSetFromImport)
			{
				var lineWithProductAndQty = new LineWithProductAndQuantity(part.PK, quantity);

				var volumeQuantity = UnitOfMeasureConverter.GetQuantityFromLine(this, this.GetVolumeMeasure(), lineWithProductAndQty);
				NetVolumeSent = Math.Max(0m, NetVolumeSent + volumeQuantity);

				var hasPackages = HasPackages;
				if (hasPackages && UsePackingWeightAndVolume)
				{
					WD_CubicSent = Math.Max(GrossVolumeSent, NetVolumeSent);
				}
				else
				{
					WD_CubicSent = Math.Max(0m, WD_CubicSent + volumeQuantity);
				}

				var weightQuantity = UnitOfMeasureConverter.GetQuantityFromLine(this, this.GetWeightMeasure(), lineWithProductAndQty);
				WD_WeightSentUserEntered = Math.Max(0m, WD_WeightSentUserEntered + weightQuantity);

				if (!hasPackages)
				{
					GrossWeightSent = WD_WeightSentUserEntered;
				}
			}
		}

		#region ReleaseTotalsDictionary

		class ReleaseTotalsDictionary : IEnumerable<KeyValuePair<OrgSupplierPart, ZDecimal>>
		{
			public ReleaseTotalsDictionary(WhsOrder order)
			{
				Order = Argument.NotNull(order, nameof(order));
				QtyCache = new Dictionary<OrgSupplierPart, ZDecimal>();
			}

			readonly WhsOrder Order;
			readonly Dictionary<OrgSupplierPart, ZDecimal> QtyCache;

			public void Clear()
			{
				if (Order.DelayUpdatingReleaseTotalsSemaphore.IsSuspended)
				{
					throw new InvalidOperationException("Should not clear Release Totals Cache until Calculation Delay is removed.");
				}

				QtyCache.Clear();
			}

			public bool TryGetValue(OrgSupplierPart key, out ZDecimal total)
			{
				return QtyCache.TryGetValue(key, out total);
			}

			public ZDecimal this[OrgSupplierPart key]
			{
				set
				{
					if (!Order.DelayUpdatingReleaseTotalsSemaphore.IsSuspended)
					{
						throw new InvalidOperationException("Should not set Quantities in the Release Totals Cache unless Calculation is Delayed.");
					}

					QtyCache[key] = value;
				}
			}

			IEnumerator<KeyValuePair<OrgSupplierPart, ZDecimal>> IEnumerable<KeyValuePair<OrgSupplierPart, ZDecimal>>.GetEnumerator()
			{
				return QtyCache.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return QtyCache.GetEnumerator();
			}
		}

		ReleaseTotalsDictionary QtyChangesPerProductForPickingCache
		{
			get { return qtyChangesPerProductForPickingCache ?? (qtyChangesPerProductForPickingCache = new ReleaseTotalsDictionary(this)); }
		}

		ReleaseTotalsDictionary qtyChangesPerProductForPickingCache;

		#endregion

		#endregion

		#endregion

		#region WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations, WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations

		public ZDecimal WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations
		{
			get { return (CrossDockLocation != null) ? CrossDockLocation.WLV_MaxCubic - WhsValidationHelper.GetLocationCapacityInfo(CrossDockLocation, null).Volume.GetValueOrDefault(0) + WD_CalcVolumeAllocatedToCrossDockLocation : 0m; }
		}

		public ZDecimal WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations
		{
			get { return (CrossDockLocation != null) ? WhsValidationHelper.GetLocationCapacityInfo(CrossDockLocation, null).Volume.GetValueOrDefault(0) - WD_CalcVolumeAllocatedToCrossDockLocation : 0m; }
		}

		public ZPropertyInfo WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocationsInfo
		{
			get { return GetZPropertyInfo(Schema.WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocations); }
		}

		public ZPropertyInfo WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocationsInfo
		{
			get { return GetZPropertyInfo(Schema.WD_CalcCrossDockAvailableVolumeIncludingCrossDockAllocations); }
		}

		ZDecimal WD_CalcVolumeAllocatedToCrossDockLocation
		{
			get
			{
				ZDecimal result = 0m;

				var crossDockLocation = CrossDockLocation;
				if (crossDockLocation != null)
				{
					var pickLines = Factory.Load<WhsPickLine>(QueryForAllReservedPickLinesInThisCrossDockLocation);

					var linesWithQuantities =
						from pl in pickLines
						let part = pl.DocketLine?.SupplierPart
						where part != null
						let inventory = pl.Inventory
						// ignore picklines to inventory already located in the cross dock location
						// because this inventory volume is already included in the locations's calculation
						where inventory.Location == null || inventory.WI_WL != WD_WL_CrossDock
						select new LineWithProductAndQuantity(part.PK, pl.WZ_Units);

					result = UnitOfMeasureConverter.GetTotalQuantityFromLines(this, new VolumeMeasure(WD_CalcCrossDockCurrentVolumeIncludingCrossDockAllocationsInfo, crossDockLocation.WLV_MaxCubicUnit), linesWithQuantities);
				}

				return result;
			}
		}

		ZQuery QueryForAllReservedPickLinesInThisCrossDockLocation
		{
			get
			{
				var docketQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketLineSchema.WE_WD);
				docketQuery.AddToFilter(WhsDocketSchema.WD_WL_CrossDock, WD_WL_CrossDock);
				docketQuery.AddToFilter(WhsDocketSchema.WD_WP, null);

				var docketLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsPickLineSchema.WZ_WE_TransactionLine);
				docketLineQuery.AddSubQuery(docketQuery, JoinCondition.And);

				var pickLineQuery = new ZDBOnlyQuery(typeof(WhsPickLine));
				pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_OriginalReservedQty, SQLComparisonOperator.GreaterThan, 0m);
				pickLineQuery.AddSubQuery(docketLineQuery, JoinCondition.And);

				return pickLineQuery;
			}
		}

		#endregion

		#region WD_WP

		public override ZGuid WD_WP
		{
			get { return base.WD_WP; }
			set
			{
				var previousPickPK = WD_WP;
				base.WD_WP = value;

				var pickChanged = previousPickPK != WD_WP;
				if (pickChanged)
				{
					CreateOrDeletePackageJobOnPickChange();
				}
			}
		}

		void CreateOrDeletePackageJobOnPickChange()
		{
			if (WD_WP.IsValid)
			{
				PkgPackageJob.LoadOrCreatePackageJob(this);
			}
			else
			{
				DeletePackageJob();
			}
		}

		#endregion

		#region WD_WW_Whs

		[ActionField(CollectionType = typeof(WhsWarehouseCollectionWithSecurityCheck))]
		public override ZGuid WD_WW_Whs
		{
			get { return base.WD_WW_Whs; }
			set
			{
				SetShouldDefaultTransportAndTransportBillToCompanies();
				base.WD_WW_Whs = value;
				SetDefaultINCOTerm();
				SetDefaultTransportAndTransportBillToCompanies();
				SetDefaultUseDirectedPackageConsolidation();

				// tested in WhsOrderValidationTest
				if (!WD_WL_CrossDock.IsEmpty && !IsValidationSuspended)
				{
					Validation.ValidateWD_WL_CrossDock();
				}

				if (WD_WW_Whs != value)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, WD_ScreeningStatus, Factory, WD_WW_Whs);
				}
			}
		}

		#endregion

		#region WD_OH_Client

		public override ZGuid WD_OH_Client
		{
			get { return base.WD_OH_Client; }
			set
			{
				SetShouldDefaultTransportAndTransportBillToCompanies();
				RegisterBuyerSupplierLink();
				base.WD_OH_Client = value;

				SetDefaultINCOTerm();
				SetDefaultTransportAndTransportBillToCompanies();
				SetDefaultContainerMode();
				SetDefaultFreightForwarder();
				SetDefaultUseDirectedPackageConsolidation();

				var consignee = Consignee;
				if (consignee != null)
				{
					SetDefaultAuthorisedToLeave(consignee);
				}

				if (WD_OH_Client != value)
				{
					ScreeningStatusUpdater.SetShouldUpdateScreeningStatusByOrgHeader(this, WD_ScreeningStatus, Factory, WD_OH_Client);
				}
			}
		}

		#endregion

		#region WD_OH_Forwarder

		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		[List("Lookups.Forwarders")]
		public override ZGuid WD_OH_Forwarder
		{
			get { return base.WD_OH_Forwarder; }
			set { base.WD_OH_Forwarder = value; }
		}

		#endregion

		#region RequiredDate

		protected override ZDateTimeOffset AppendWarehouseOffset(ZDateTime value)
		{
			var result = ZDateTimeOffset.Empty;
			if (value.IsValid && !value.IsEmpty)
			{
				var offset = Warehouse.GetWarehouseBranchLocalDateTimeOffset(value.ToUniversalBranchTime());
				var (hour, minute, second) = (!IsImportingData && WD_RequiredDate.IsEmpty) ? (23, 59, 00) : (value.Hour, value.Minute, value.Second);
				result = new ZDateTimeOffset(value.Year, value.Month, value.Day, hour, minute, second, offset.Offset);
			}

			return result;
		}

		#endregion

		#region WD_QualityAuditRequired

		protected bool WD_QualityAuditRequired_ReadOnly
		{
			get { return IsFinalisedOrCancelled; }
		}

		#endregion

		#region WD_TransportMode

		public override ZString WD_TransportMode
		{
			get { return base.WD_TransportMode; }
			set
			{
				RegisterBuyerSupplierLink();
				var previousValue = WD_TransportMode;
				// When clearing the value of the transport Mode, the Buyer Supplier Relationship
				// tries to update the transport mode and effectively does not allow the user to clear it.
				// This suspends the transport mode update if setting to empty.
				using (value.IsEmpty ? BuyerSupplierLinksHelper.SuspendTransportModeRestoration() : null)
				{
					base.WD_TransportMode = value;
				}

				if (previousValue != WD_TransportMode)
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateWD_ContainerMode();
					}
				}
			}
		}

		#endregion

		#region WD_ContainerMode

		public override ZString WD_ContainerMode
		{
			get { return base.WD_ContainerMode; }
			set
			{
				RegisterBuyerSupplierLink();
				base.WD_ContainerMode = value;
			}
		}

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

		#region WD_TotalUnitsFromLinesCore

		protected override ZDecimal WD_TotalUnitsFromLinesCore
		{
			get
			{
				var total = 0m;
				foreach (WhsPickableDocketLine line in Lines)
				{
					if (!line.IsComponentLineOnSalesOrder)
					{
						total += line.WE_TransactionQuantity;
					}
				}
				return total;
			}
		}

		#endregion

		#region IsFinaliseAllowed

		IDPSSecurityProvider DPSSecurityProvider
			=> dpsSecurityProvider ?? (dpsSecurityProvider = Globals.CanShowDialogs
				? new DPSSecurityProvider(Pick?.NotificationSubscriber ?? NotificationSubscriber)
				: new DPSSecurityNonInteractiveProvider());
		IDPSSecurityProvider dpsSecurityProvider;

		protected override bool CheckIsHeldByCustomsWhenFinalising
		{
			get { return !WhsCustomsHelper.CanFinaliseWhsOrderWithoutCustomsClearance(this); }
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

		#region PortOfOrigin

		public ZString PortOfOrigin
		{
			get
			{
				var result = ZString.Empty;
				var warehouse = Warehouse;
				if (warehouse != null && warehouse.WarehouseAddress != null)
				{
					result = warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode;
					if (result.IsEmpty)
					{
						result = warehouse.WarehouseAddress.Header.OH_RL_NKClosestPort;
					}
				}

				return result;
			}
		}

		#endregion

		#region PortOfDestination

		public ZString PortOfDestination
		{
			get
			{
				var result = ZString.Empty;
				var consigneeAddress = ConsigneeAddress;
				if (consigneeAddress != null)
				{
					result = consigneeAddress.OA_RL_NKRelatedPortCode;
					if (result.IsEmpty)
					{
						result = Consignee.OH_RL_NKClosestPort;
					}
				}
				else
				{
					var portCode = RefUNLOCO.LookupPortCodeFromCountryCityOrAUState(Factory, ConsigneeDocAddress.E2_RN_NKCountryCode, ConsigneeDocAddress.E2_City, ConsigneeDocAddress.E2_State);
					var portUNLOCO = new RefUNLOCO.Loader(Factory).Load(portCode);
					if (portUNLOCO != null)
					{
						result = portCode;
					}
				}

				return result;
			}
		}

		#endregion

		#region GoodsDescriptionWithFallback

		public ZString GoodsDescriptionWithFallback
		{
			get { return WD_GoodsDescription.IsEmpty ? CartageHelper.OrderLinesGoodsDescription : WD_GoodsDescription; }
		}

		#endregion

		#region DistributionCentreDocAddressReadOnly

		protected override bool DistributionCentreDocAddressReadOnly()
		{
			return ReadOnly || NonStandardReadOnly1;
		}

		#endregion

		#region DistributionCentreNameOrPK For Grid Binding

		public ZString DistributionCentreFieldType => DistributionCentreDocAddress.E2_AddressOverride ? nameof(FieldType.Text) : nameof(FieldType.Guid);

		[List(nameof(Lookups) + "." + nameof(Lookups.DistributionCentres))]
		[ResourceStringData("WhsOrder|DistributionCentreNameOrPK", Caption = "Distribution Center", MediumCaption = "Dist. Center")]
		public ZString DistributionCentreNameOrPK
		{
			get
			{
				ZString result;

				var docAddress = DistributionCentreDocAddress;
				if (docAddress.E2_AddressOverride)
				{
					result = docAddress.E2_CompanyNameTruncated;
				}
				else
				{
					result = docAddress.Organisation?.PK.ToString() ?? ZGuid.Empty.ToString();
				}

				return result;
			}
		}

		#endregion

		#region TransportJobNumber

		[ResourceStringData("WhsOrder|TransportJobNumber", Caption = "Transport Job Number", MediumCaption = "Transport Job")]
		public ZString TransportJobNumber
		{
			get
			{
				var result = string.Empty;
				var transportJobResult = ((ITransportJobLinkProvider)this).TransportJobResult;
				var transportJob = transportJobResult.TransportJob;

				if (transportJob != null || transportJobResult.ReasonForEmptyOverride != null)
				{
					result = transportJobResult.ReasonForEmptyOverride ?? transportJob.JobNumber;
				}

				return result;
			}
		}

		#endregion

		#region TrolleyNumber

		[ResourceStringData("WhsOrder|TrolleyNumber", Caption = "Trolley Number", MediumCaption = "Trolley")]
		public ZString TrolleyNumber
		{
			get
			{
				var query = new ZQuery(WhsOrderTrolleyViewSchema.WTR_WD_PK, PK);
				var whsOrderTrolleyView = Factory.Load<WhsOrderTrolleyView>(query);
				return string.Join(", ", whsOrderTrolleyView.Select(s => s.WTR_RQ_Registration));
			}
		}

		#endregion

		#region AuditStatus

		public ZString AuditStatus
		{
			get
			{
				var packageJob = PackageJob;
				var allOuters = new Lazy<ReadOnlyCollection<PkgPackage>>(() => packageJob.GetAllNonContainerOutersAndFirstLevelPackagesOnContainers());

				if (WD_QualityAuditRequired)
				{
					if (packageJob == null || packageJob.Packages.Count == 0 || HasAnyUnauditedPackage(allOuters.Value))
					{
						return OrderAuditStatus.Descriptions.Pending;
					}
					else if (HasAnyFailingPackage(allOuters.Value))
					{
						return OrderAuditStatus.Descriptions.Failed;
					}
					else
					{
						return OrderAuditStatus.Descriptions.Passed;
					}
				}
				else
				{
					if (packageJob != null && HasAnyFailingPackage(allOuters.Value))
					{
						return OrderAuditStatus.Descriptions.Failed;
					}
					else
					{
						return OrderAuditStatus.Descriptions.NotRequired;
					}
				}
			}
		}

		#endregion

		#region WD_WL_CrossDock

		[List("Lookups.CrossDockLocations")]
		public override ZGuid WD_WL_CrossDock
		{
			get { return base.WD_WL_CrossDock; }
			set { base.WD_WL_CrossDock = value; }
		}

		#endregion

		#region WD_IsAuthorisedToLeave

		[ReadOnlyMember(nameof(IsPickFinalised))]
		public override ZBool WD_IsAuthorisedToLeave
		{
			get => base.WD_IsAuthorisedToLeave;
			set => base.WD_IsAuthorisedToLeave = value;
		}

		#endregion

		#region WD_PackingAfterPickingRequired

		[ReadOnlyMember(nameof(IsPickFinalised))]
		public override ZBool WD_PackingAfterPickingRequired
		{
			get => base.WD_PackingAfterPickingRequired;
			set => base.WD_PackingAfterPickingRequired = value;
		}

		#endregion

		#region OrderGoodsHandlingInstructions

		public ZString OrderGoodsHandlingInstructions => GetGoodsHandlingInstructionsNote();

		#endregion

		#region ProductCount

		protected override ZInt ProductCountCore => IEnumerableExtensions.DistinctBy(Lines.Where(ol => ol.WE_OP.IsValid), l => l.WE_OP).Count();

		#endregion

		#region WD_WSH_SalesChannel

		[RelatedBusinessObject(nameof(SalesChannel))]
		[List("Lookups.SalesChannels")]
		[ReadOnlyMember(nameof(NonStandardReadOnly1))]
		public override ZGuid WD_WSH_SalesChannel
		{
			get => base.WD_WSH_SalesChannel;
			set
			{
				base.WD_WSH_SalesChannel = value;
				SetDefaultUseDirectedPackageConsolidation();
			}
		}

		#endregion

		#region WhsOrderStatus

		public ZString WarehouseOrderStatus
		{
			get
			{
				var result = WD_DocketStatus;
				if (IsInDatabase && result.EqualsIgnoringCase(DocketStatus.Codes.Picking))
				{
					result = orderStatus ?? (orderStatus = GetWhsOrderStatus());
				}
				return result;
			}
		}
		string orderStatus;

		string GetWhsOrderStatus()
		{
			var query = new ZQuery(WhsOrderStatusViewSchema.PK, PK);
			var whsOrderStatusView = Factory.Load<WhsOrderStatusView>(query).Single();
			return whsOrderStatusView.WOS_OrderStatus;
		}

		[ResourceStringData("WhsOrder|WarehouseOrderStatusDescription", Caption = "Status")]
		public ZString WarehouseOrderStatusDescription
			=> WhsOrderHelper.OrderStatuses.GetMultilingualDescriptionFromCode(WarehouseOrderStatus);

		#endregion

		#region WD_ExcludeFromTotePicking

		[ReadOnlyMember(nameof(IsPickFinalised))]
		public override ZBool WD_ExcludeFromTotePicking
		{
			get => base.WD_ExcludeFromTotePicking;
			set => base.WD_ExcludeFromTotePicking = value;
		}

		#endregion

		#region PropagatesFinalisedStatusChangeToLines

		protected override void PropagatesFinalisedStatusChangeToLinesCore()
		{
			if (IsFinalised || IsDepartedFromPickFinalisation)
			{
				LinesToPropagateStatusAndFinDateChange.ForEach(l => l.WE_DocketLineStatus = IsDepartedFromPickFinalisation ? DocketLineStatus.Codes.Departed : DocketLineStatus.Codes.Finalised);
			}
			else
			{
				ResetDocketLineStatusIfRequired();
			}
		}

		#endregion

		#region PropagateFinalisedDateChangeToLines

		protected override void PropagateFinalisedDateChangeToLinesCore(IEnumerable<WhsDocketLine> lines, ZDateTimeOffset docketFinalisedDate)
		{
			if (docketFinalisedDate.IsValid)
			{
				lines.ForEach(l => l.WE_DocketLineStatus = IsDepartedFromPickFinalisation ? DocketLineStatus.Codes.Departed : DocketLineStatus.Codes.Finalised);
			}
			else
			{
				lines.ForEach(l => l.WE_DocketLineStatus = string.Empty);
			}
			base.PropagateFinalisedDateChangeToLinesCore(lines, docketFinalisedDate);
		}

		#endregion

		#region DocketTypeSupportsAllocationKeyCore

		protected override bool DocketTypeSupportsAllocationKeyCore => true;

		#endregion

		#region WD_UseDirectedPackingConsolidation

		[ReadOnlyMember(nameof(WD_UseDirectedPackingConsolidation_ReadOnly))]
		public override ZBool WD_UseDirectedPackingConsolidation
		{
			get => base.WD_UseDirectedPackingConsolidation;
			set => base.WD_UseDirectedPackingConsolidation = value;
		}

		#region WD_UseDirectedPackingConsolidation_ReadOnly

		bool WD_UseDirectedPackingConsolidation_ReadOnly => IsFinalisedOrCancelled || (Pick?.IsReadyForPlanningOrPlanned ?? false);

		#endregion

		#endregion

		#endregion

		#region WD_DocketStatusDescription

		protected override ZString WD_DocketStatusDescriptionCore
			=> WarehouseOrderStatusDescription;

		#endregion

		#region Flags

		#region IsDepartedFromPickFinalisation

		public bool IsDepartedFromPickFinalisation => WD_DocketStatus.EqualsIgnoringCase(WhsOrderStatus.Codes.Departed);

		#endregion

		#region HasChangesForLock

		public bool HasChangesForLock
		{
			get
			{
				return ((IBusiness)this).HasChangesNotIncludingChildren || HasChangesOnLines();

				bool HasChangesOnLines()
				{
					IEnumerable<IBusiness> orderLines;

					if (IsLinesInitialised)
					{
						orderLines = Lines;
					}
					else
					{
						var memoryQuery = new ZQuery(WhsDocketLineSchema.WE_WD, PK) { FetchOnlyFromLocalCache = true };
						orderLines = Factory.Load<WhsOrderLine>(memoryQuery);
					}

					return orderLines.Any(l => l.HasChangesNotIncludingChildren);
				}
			}
		}

		#endregion

		#region IsPostFinalizeEditAllowedCore

		protected override bool IsPostFinalizeEditAllowedCore
		{
			get { return base.IsPostFinalizeEditAllowedCore && !IsAtleastOneInvoiceLinePosted; }
		}

		#endregion

		#region IsOrderHeld

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public ZBool IsOrderHeld
		{
			get => WD_DocketStatus.EqualsIgnoringCase(DocketStatus.Codes.Held);
			set
			{
				if (!WD_DocketStatus.EqualsIgnoringCase(DocketStatus.Codes.Entered) &&
					!WD_DocketStatus.EqualsIgnoringCase(DocketStatus.Codes.New) &&
					!WD_DocketStatus.EqualsIgnoringCase(DocketStatus.Codes.Held))
				{
					throw new InvalidOperationException($"Cannot change the status via IsOrderHeld when the Docket Status is '{WD_DocketStatus}'.");
				}
				else if (value)
				{
					WD_DocketStatus = DocketStatus.Codes.Held;
				}
				else
				{
					WD_DocketStatus = IsInDatabase ? DocketStatus.Codes.Entered : DocketStatus.Codes.New;
				}

				IsOrderHeldInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsOrderHeldInfo => GetZPropertyInfo(nameof(IsOrderHeld));

		#endregion

		#region IsDomesticFreight

		public override bool IsDomesticFreight
		{
			get
			{
				bool result = base.IsDomesticFreight;

				var warehouse = Warehouse;
				if (warehouse != null && warehouse.WarehouseAddress != null)
				{
					var consigneeDocAddress = ConsigneeDocAddress;
					if (consigneeDocAddress != null)
					{
						result = warehouse.CountryCode == consigneeDocAddress.E2_RN_NKCountryCode;
					}
				}

				return result;
			}
		}

		#endregion

		#region IsSavedFromOrderForm

		public bool IsSavedFromOrderForm
		{
			get;
			set;
		}

		#endregion

		#region DocketSubTypeReadOnly

		protected override bool DocketSubTypeReadOnly
		{
			get { return base.DocketSubTypeReadOnly || IsLinkedBackOrder; }
		}

		bool IsLinkedBackOrder
		{
			get { return WD_DocketSubType == OrderType.Codes.BackOrder && !WD_WD_ParentDocket.IsEmpty; }
		}

		#endregion

		#region WarehouseReadOnly

		protected override bool WarehouseReadOnly => !WD_WP.IsEmpty;

		#endregion

		#region RequiredDateReadOnly

		protected override bool RequiredDateReadOnly
		{
			get
			{
				return base.RequiredDateReadOnly || (IsFinaliseAllowed && IsFinalised && IsPostFinalizeEditAllowed && UseRequiredDateForFinalisedDate);
			}
		}

		bool UseRequiredDateForFinalisedDate
		{
			get
			{
				var warehouse = Warehouse;
				return warehouse != null && warehouse.WW_UseRequiredDateForOutwardsFinalisedDate;
			}
		}

		#endregion

		#region IsPossibleChangeOfInventoryJob

		protected override bool IsPossibleChangeOfInventoryJob => IsCustomsTransaction && IsImportingForChangeOfInventory;

		#endregion

		#region HasAnyFailingPackage

		bool HasAnyFailingPackage(ReadOnlyCollection<PkgPackage> allOuters)
		{
			return allOuters.Any(p => WhsPackageAuditManager.HasAuditFailures(p));
		}

		#endregion

		#region HasAnyUnauditedPackage

		bool HasAnyUnauditedPackage(ReadOnlyCollection<PkgPackage> allOuters)
		{
			return allOuters.Any(p => !WhsPackageAuditManager.HasBeenAudited(p));
		}

		#endregion

		#region IsPickingFTZCustomsOrderWithPermit

		public bool IsPickingFTZCustomsOrderWithPermit => Pick != null && IsFTZCustomsOrderWithPermit;

		public bool IsFTZCustomsOrderWithPermit
		{
			get
			{
				var warehouse = Warehouse;
				return WD_DocketSubType == OrderType.Codes.CustomsReleaseWithPermit && warehouse != null && warehouse.IsFTZWarehouseInCountryThatUsesPermits;
			}
		}

		#endregion

		#region HasReservedStock

		public bool HasReservedStock => Lines.Any(l => l.IsInDatabase && l.ReservedQuantity > 0m);

		#endregion

		#region AreLinesUpdateAllowedAfterPick

		public bool AreLinesUpdateDisabledAfterPick => IsAttachedToPickButNotFinalised && WarehouseDataRegistry.Instance.PreventOrderLinesUpdateWhenOrderIsInPicking.Value;

		#endregion

		#region IsSalesChannelAllowed

		protected override bool SalesChannelAllowed => true;

		#endregion

		#region IsOrderAssignedToLoad

		public bool IsOrderAssignedToLoad => WD_WLO_PlannedLoad.IsValid || IsOrderAttachedToLoadThroughPackage;

		bool IsOrderAttachedToLoadThroughPackage => Factory.LoadTop1<WhsLoadOrder>(new ZQuery(WhsLoadOrderSchema.WOV_WD_Docket, PK)) != null;

		#endregion

		#region IsAnyPackageInConsolidationLocationOrOnConsolidationHU

		public bool IsAnyPackageInConsolidationLocationOrOnConsolidationHU()
		{
			var isAnyPackageInConsolidationLocationOrOnConsolidationHU = false;
			if (WD_WP.IsValid)
			{
				var packageJob = GetPackageJob();
				if (packageJob != null)
				{
					var packages = packageJob.Packages;
					if (packages.Any(pkg => !pkg.KP_KP_TopHandlingUnitPackage.IsEmpty))
					{
						isAnyPackageInConsolidationLocationOrOnConsolidationHU = true;
					}
					else if (WarehouseOrderStatus == DocketStatus.Codes.Picking)
					{
						var packagePKs = packages.Select(p => p.PK).ToArray();
						var query = new ZQuery { AllowTableValuedParameters = true };
						query.AddToFilter(WhsPackageLocationViewSchema.WPK_KP_Package, packagePKs);
						query.AddToFilter(WhsPackageLocationViewSchema.WPK_LocationClass, LocationClasses.Codes.CON);

						isAnyPackageInConsolidationLocationOrOnConsolidationHU = Factory.LoadTop1<WhsPackageLocationView>(query) != null;
					}
				}
			}
			return isAnyPackageInConsolidationLocationOrOnConsolidationHU;
		}

		internal bool TranportCoPKHasChanges => previousTranportCoPK != TransportCoPK;

		ZGuid? previousTranportCoPK = ZGuid.Empty;

		#endregion

		#region IsBondedEntryKeyVisibleForCustomsTransactionsCore

		protected override bool IsBondedEntryKeyVisibleForCustomsTransactionsCore => true;

		#endregion

		#region IsLoadingOrLoadedOrDeparted

		public bool IsLoadingOrLoadedOrDeparted
		{
			get
			{
				var status = WarehouseOrderStatus;
				return status == WhsOrderStatus.Codes.Loading || status == WhsOrderStatus.Codes.Loaded || status == WhsOrderStatus.Codes.Departed;
			}
		}

		#endregion

		#endregion

		#region Cancellation

		protected override string CanCancelCore()
		{
			var result = base.CanCancelCore();

			if (result == ZString.Empty && IsOrderAssignedToLoad)
			{
				result = Res.GetString("b6ba193d-6bc1-4905-afa2-41b02e2fc70b", "The order cannot be canceled while it is assigned to a Load.");
			}

			return result;
		}

		#endregion

		#region AutoPackAndPrintAllLabels

		public bool AutoPackAndPrintAllLabelsWithNoFactorySave(INotifications notify, bool disablePrinting)
		{
			var options = disablePrinting
				? AutoPackOptions.NoFactorySave | AutoPackOptions.DisablePrinting
				: AutoPackOptions.NoFactorySave;

			return AutoPackAndPrintAllLabels(notify, options);
		}

		public bool AutoPackAndPrintAllLabels(INotifications notify, bool disablePrinting)
		{
			return AutoPackAndPrintAllLabels(notify, disablePrinting ? AutoPackOptions.DisablePrinting : AutoPackOptions.Default);
		}

		[Flags]
		enum AutoPackOptions
		{
			Default = 1,
			NoFactorySave = 2,
			DisablePrinting = 4,
		}

		bool AutoPackAndPrintAllLabels(INotifications notify, AutoPackOptions options)
		{
			Argument.GreaterThan(WD_DocketID.Length, 0, (NoResString)"Auto Pack Requires Valid ID"); // Dev Warning
			var success = false;

			if (Pick == null)
			{
				notify.AddMessageError(Res.GetString("0fc70583-d6f7-4179-81a7-a2f1b2bfcb36", "Order {0} cannot be Auto-Packed as it has not been Picked.", WD_DocketID));
			}
			else
			{
				var packageJob = PackageJob;
				if (packageJob == null)
				{
					// Safeguard -- this should not actually happen because a PackingJob is created on Pick.
					notify.AddMessageError(Res.GetString("ae3cf7fa-0ed7-42dd-a50c-a996565c2040",
	@"Order {0} does not have a Packing Job.
To rectify, open the Release, click on the Packing tab and then click Save.
This will create the necessary Packing information.", WD_DocketID));
				}
				else if (packageJob.ReadOnly)
				{
					notify.AddMessageError(Res.GetString("4f379f1c-57d8-4643-a84f-a21512c7cd36",
						"Order {0} cannot be Auto-Packed as it has a read only Packing Job.", WD_DocketID));
				}
				else if (!IsAutoPackAllowed)
				{
					notify.AddMessageError(Res.GetString("202cb644-4979-402a-b37e-9c80813e0157",
						"Order {0} cannot be Auto-Packed as it is not enabled on Consignee {1}.", WD_DocketID, ConsigneeDocAddress.E2_CompanyName));
				}
				else if (packageJob.Packages.Count > 0)
				{
					notify.AddMessageError(Res.GetString("0fc70583-d7f7-4179-81a7-a2f1b2bfcb36",
						"Order {0} cannot be Auto-Packed as it already has Packages.", WD_DocketID));
				}
				else if (PackableItemParents.Count == 0)
				{
					notify.AddMessageError(Res.GetString("efd3a1a7-fcb9-4b9a-aa63-f5c03844ddfd",
						"Order {0} cannot be Auto-Packed as it has no Packable Items caused by a shortfall.", WD_DocketID));
				}
				else
				{
					success = AutoPackAndPrintAllLabelsCore(notify, packageJob, options);
				}
			}

			return success;
		}

		bool AutoPackAndPrintAllLabelsCore(INotifications notify, PkgPackageJob packageJob, AutoPackOptions options)
		{
			var result = false;

			// auto-pack / generate ids / print labels
			packageJob.AutoPack(notify);
			if (packageJob.Packages.Count > 0)
			{
				var printLabels = !options.HasFlag(AutoPackOptions.DisablePrinting);

				if (options.HasFlag(AutoPackOptions.NoFactorySave) || !printLabels)
				{
					AutoPackAndPrintAllLabelsWithNoFactorySaveCore(packageJob, printLabels);
					result = true;
				}
				else
				{
					result = AutoPackAndPrintAllLabelsWithFactorySaveCore(notify, packageJob);
				}
			}

			return result;
		}

		bool AutoPackAndPrintAllLabelsWithFactorySaveCore(INotifications notify, PkgPackageJob packageJob)
		{
			var result = false;
			if (HasErrors)
			{
				var errorMessage = new StringBuilder();
				errorMessage.AppendLine(Res.GetString("72813520-3bd1-44e0-bb90-793ba6e62422",
					"Order {0} cannot be saved due to the following validation errors:", WD_DocketID));
				errorMessage.Append(string.Join("\r\n", NotificationsIncludingChildren.GetErrors().GetUniqueMessageList()));
				notify.AddError(errorMessage.ToString());
			}
			else
			{
				packageJob.CloseAndGenerateIDs(notify);
				Factory.Save();
				packageJob.PrintAllLabels();

				result = true;
			}
			return result;
		}

		void AutoPackAndPrintAllLabelsWithNoFactorySaveCore(PkgPackageJob packageJob, bool printLabels)
		{
			PrintLabelsOnSave = printLabels;
			foreach (var package in packageJob.Packages)
			{
				package.KP_ClosedTimeUtc = ZDateTime.UtcNow;
				var iSupportPackageIDGeneration = (ISupportPackageIDGeneration)package;
				iSupportPackageIDGeneration.ShouldGenerateIDOnSaving = true;
			}
		}

		bool PrintLabelsOnSave { get; set; }

		#endregion

		#region DocAddress Requirements

		protected override JobDocAddressRequirement GetPickUpDocAddressRequirement()
		{
			JobDocAddressRequirement requirement = new JobDocAddressRequirement(DocAddressType.PickUpAddress, AddressType.PIC, ContactType.Consignee);
			return requirement;
		}

		protected override JobDocAddressRequirement GetDropOffDocAddressRequirement()
		{
			JobDocAddressRequirement requirement = new JobDocAddressRequirement(DocAddressType.DropOffAddress, AddressType.DLV, ContactType.All);
			return requirement;
		}

		protected override JobDocAddressRequirement GetConsigneeDocAddressRequirement()
		{
			JobDocAddressRequirement requirement = new JobDocAddressRequirement(DocAddressType.ConsigneeAddress, AddressType.DLV, ContactType.Consignee);
			return requirement;
		}

		internal JobDocAddressRequirement TransportBillToDocAddressRequirement
		{
			get { return transportBillToDocAddressRequirement ?? (transportBillToDocAddressRequirement = GetTransportBillToDocAddressRequirement()); }
		}

		JobDocAddressRequirement GetTransportBillToDocAddressRequirement()
		{
			return new JobDocAddressRequirement(DocAddressType.TransportBillToAddress, ContactType.TransportServices);
		}

		JobDocAddressRequirement transportBillToDocAddressRequirement;

		internal JobDocAddressRequirement DistributionCentreDocAddressRequirement
		{
			get
			{
				return distributionCentreDocAddressRequirement ??
					(distributionCentreDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.DistributionCentreAddress, ContactType.NoContactType));
			}
		}
		JobDocAddressRequirement distributionCentreDocAddressRequirement;

		#region CarrierBookingAgentRequirement

		internal JobDocAddressRequirement CarrierBookingAgentDocAddressRequirement
		{
			get { return carrierBookingAgentDocAddressRequirement ?? (carrierBookingAgentDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.CarrierBookingAgent) { CanOverride = false }); }
		}

		JobDocAddressRequirement carrierBookingAgentDocAddressRequirement;

		#endregion

		internal JobDocAddressRequirement DestinationWarehouseDocAddressRequirement => destinationWarehouseDocAddressRequirement
			?? (destinationWarehouseDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.DestinationWarehouse, ContactType.NoContactType));
		JobDocAddressRequirement destinationWarehouseDocAddressRequirement;

		#region ReturnAddressRequirement

		internal JobDocAddressRequirement ReturnDocAddressRequirement
		{
			get { return returnDocAddressRequirement ?? (returnDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ReturnAddress)); }
		}

		JobDocAddressRequirement returnDocAddressRequirement;

		#endregion

		protected override DocAddressType[] SupportedAddressTypesCore()
		{
			var result = new List<DocAddressType>(base.SupportedAddressTypesCore());
			result.Add(DocAddressType.TransportBillToAddress);
			result.Add(DocAddressType.DistributionCentreAddress);
			result.Add(DocAddressType.CarrierBookingAgent);
			result.Add(DocAddressType.DestinationWarehouse);
			result.Add(DocAddressType.ReturnAddress);

			return result.ToArray();
		}

		protected override JobDocAddressRequirement GetDocAddressRequirementCore(DocAddressType addressType)
		{
			switch (addressType)
			{
				case DocAddressType.TransportBillToAddress:
					return TransportBillToDocAddressRequirement;
				case DocAddressType.DistributionCentreAddress:
					return DistributionCentreDocAddressRequirement;
				case DocAddressType.CarrierBookingAgent:
					return CarrierBookingAgentDocAddressRequirement;
				case DocAddressType.DestinationWarehouse:
					return DestinationWarehouseDocAddressRequirement;
				case DocAddressType.ReturnAddress:
					return ReturnDocAddressRequirement;
				default:
					return base.GetDocAddressRequirementCore(addressType);
			}
		}

		#endregion

		#region SetDefaults

		#region SetDefaultFreightForwarder

		void SetDefaultFreightForwarder()
		{
			if (WD_OH_Forwarder.IsEmpty && WD_OH_Client.IsValid)
			{
				var consignee = Consignee;
				if (consignee != null || ConsigneeDocAddress.E2_AddressOverride)
				{
					var forwarder = GetRelatedWarehouseForwarder(consignee) ?? GetRelatedWarehouseForwarder(Client);
					if (forwarder != null)
					{
						WD_OH_Forwarder = forwarder.PK;
					}
				}
			}
		}

		OrgHeader GetRelatedWarehouseForwarder(OrgHeader relatedOrganisation)
		{
			return relatedOrganisation != null
				? relatedOrganisation.GetRelatedParty(RelatedPartyTypeList.Codes.WarehouseForwarder, RelatedPartyDirectionList.Codes.Forwarder)
				: null;
		}

		#endregion

		#region SetClientOrderOptions

		protected override void SetClientOrderOptionsCore(OrgMiscServ clientMiscServ)
		{
			base.SetClientOrderOptionsCore(clientMiscServ);
			WD_WhsOrderFulfillmentRule = clientMiscServ.OM_WhsOrderFulfillmentRule;
			WD_PickPriority = clientMiscServ.OM_WhsOrderDefaultPickPriority;
		}

		#endregion

		#region SetDefaultContainerMode

		void SetDefaultContainerMode()
		{
			if (!WD_TransportMode.IsEmpty && WD_ContainerMode.IsEmpty)
			{
				var supplierBuyerLink = SupplierBuyerLink;
				if (supplierBuyerLink?.OrgSupBuyLinkTrnModes.Count == 1)
				{
					((IBuyerSupplierRelationshipConsumer)this).ContainerMode = supplierBuyerLink.OrgSupBuyLinkTrnModes[0].PF_ContainerMode;
				}
			}
		}

		#endregion

		#region SetDefaultINCOTerm

		void SetDefaultINCOTerm()
		{
			if (!Lookups.INCOTerms.ContainsCode(WD_INCO))
			{
				WD_INCO = ZString.Empty;
			}

			if (WD_INCO.IsEmpty)
			{
				if (Consignee != null && Consignee.MiscServ != null && !Consignee.MiscServ.OM_IMDefaultINCOTerm.IsEmpty && Lookups.INCOTerms.ContainsCode(Consignee.MiscServ.OM_IMDefaultINCOTerm))
				{
					WD_INCO = Consignee.MiscServ.OM_IMDefaultINCOTerm;
				}
				else if (Client != null && Client.MiscServ != null && !Client.MiscServ.OM_IMDefaultINCOTerm.IsEmpty && Lookups.INCOTerms.ContainsCode(Client.MiscServ.OM_IMDefaultINCOTerm))
				{
					WD_INCO = Client.MiscServ.OM_IMDefaultINCOTerm;
				}
			}
		}

		#endregion

		#region SetShouldDefaultTransportDistributionCenterAndTransportBillToCompanies

		void SetShouldDefaultTransportAndTransportBillToCompanies()
		{
			var transportDefaultsProviders = TransportDefaultsProviders.ToArray();

			shouldSetDefaultTransportCo = !IsImportingData;
			shouldSetDefaultTransportBillTo = !IsImportingData;

			if (shouldSetDefaultTransportCo)
			{
				var transportCo = this.GetTransportCo();
				shouldSetDefaultTransportCo = ((transportCo?.PK == GetDefaultCompanyPK(transportDefaultsProviders, RelatedPartyTypeList.Codes.LocalTransport))
					|| (transportCo == null && !TransportCoDocAddress.E2_AddressOverride));
			}

			if (shouldSetDefaultTransportBillTo)
			{
				var transportBillTo = TransportBillTo;
				shouldSetDefaultTransportBillTo = ((transportBillTo?.PK == GetDefaultCompanyPK(transportDefaultsProviders, RelatedPartyTypeList.Codes.LocalTransportBillTo))
					|| (transportBillTo == null && !TransportBillToDocAddress.E2_AddressOverride));
			}
		}

		void SetShouldDefaultDistributionCenter()
		{
			var distributionCentre = DistributionCentre;
			shouldSetDefaultDistributionCenter =
				!IsImportingData &&
				((distributionCentre?.PK == GetDefaultCompanyPK(TransportDefaultsProviders.ToArray(), RelatedPartyTypeList.Codes.NationalDistributionCentre))
					|| (distributionCentre == null && !DistributionCentreDocAddress.E2_AddressOverride));
		}

		bool shouldSetDefaultTransportCo;
		bool shouldSetDefaultTransportBillTo;
		bool shouldSetDefaultDistributionCenter;

		#endregion

		#region SetDefaultTransportDistributionCenterAndTransportBillToCompanies

		void SetDefaultTransportAndTransportBillToCompanies()
		{
			var transportDefaultsProviders = TransportDefaultsProviders.ToArray();

			if (shouldSetDefaultTransportCo)
			{
				SetDefaultCompany(transportDefaultsProviders, TransportCoDocAddress, RelatedPartyTypeList.Codes.LocalTransport);
			}

			if (shouldSetDefaultTransportBillTo)
			{
				SetDefaultCompany(transportDefaultsProviders, TransportBillToDocAddress, RelatedPartyTypeList.Codes.LocalTransportBillTo);
			}
		}

		void SetDefaultDistributionCentre()
		{
			if (shouldSetDefaultDistributionCenter)
			{
				SetDefaultCompany(TransportDefaultsProviders.ToArray(), DistributionCentreDocAddress, RelatedPartyTypeList.Codes.NationalDistributionCentre);
			}
		}

		#endregion

		#region SetDefaultCompany

		void SetDefaultCompany(IEnumerable<OrgHeader> transportDefaultsProviders, JobDocAddress docAddressToDefault, string relatedPartyTypeList)
		{
			var companyToDefaultToPK = GetDefaultCompanyPK(transportDefaultsProviders, relatedPartyTypeList);
			if (companyToDefaultToPK.IsValid)
			{
				docAddressToDefault.OrganisationPK = companyToDefaultToPK;
			}
		}

		ZGuid GetDefaultCompanyPK(IEnumerable<OrgHeader> transportDefaultsProviders, string relatedPartyType)
		{
			var relatedPartyPK = ZGuid.Empty;
			foreach (var org in transportDefaultsProviders)
			{
				var relatedParty = GetRelatedParty(org, relatedPartyType);

				if (relatedParty != null)
				{
					relatedPartyPK = relatedParty.PR_OH_RelatedParty;
					break;
				}
			}

			return relatedPartyPK;
		}

		OrgRelatedParty GetRelatedParty(OrgHeader org, string relatedPartyType)
		{
			OrgRelatedParty result = null;

			if (org.PK == ConsigneePK)
			{
				result = relatedPartyType == RelatedPartyTypeList.Codes.NationalDistributionCentre
							? org.AllRelatedParties.GetRelatedParty(ConsigneeDocAddress.E2_OA_Address, relatedPartyType, RelatedPartyDirectionList.Codes.Delivery, ZString.Empty, ZString.Empty)
							: org.AllRelatedParties.GetRelatedParty(ConsigneeDocAddress.E2_OA_Address, relatedPartyType, RelatedPartyDirectionList.Codes.Delivery, Constants.TransportModes.All, ZString.Empty);
			}
			else
			{
				result = org.AllRelatedParties.GetRelatedParty(relatedPartyType, RelatedPartyDirectionList.Codes.Pickup, Constants.TransportModes.All, ZString.Empty);
			}

			return result;
		}

		#endregion

		#region SetDefaultDropMode

		void SetDefaultDropMode()
		{
			var result = ZString.Empty;

			var consigneeOrgAddress = ConsigneeDocAddress?.Address;
			if (consigneeOrgAddress != null)
			{
				result = Containers.Count == 0 ? consigneeOrgAddress.OA_LCLEquipmentNeeded : consigneeOrgAddress.OA_FCLEquipmentNeeded;
			}

			WD_DropMode = result;
		}

		#endregion

		#region SetDefaultServiceLevel

		void SetDefaultServiceLevel()
		{
			if (WD_PL_NKCarrierServiceLevel.IsEmpty)
			{
				var clientEXDefaultServiceLevel = Client?.MiscServ?.EXDefaultServiceLevel;
				var consigneeIMDefaultServiceLevel = new Lazy<RefServiceLevel>(() => Consignee?.MiscServ?.IMDefaultServiceLevel);
				if (clientEXDefaultServiceLevel != null)
				{
					WD_PL_NKCarrierServiceLevel = clientEXDefaultServiceLevel.RS_Code;
				}
				else if (consigneeIMDefaultServiceLevel.Value != null)
				{
					WD_PL_NKCarrierServiceLevel = consigneeIMDefaultServiceLevel.Value.RS_Code;
				}
			}
		}

		#endregion

		#region SetDefaultAuthorisedToLeave

		protected override void OnConsigneeAddressOverrideChanged(EventArgs e)
		{
			base.OnConsigneeAddressOverrideChanged(e);

			// if address override changed, set ATL to false
			if (e is ValueChangedEventArgs args && !args.OldValue.Equals(args.NewValue))
			{
				WD_IsAuthorisedToLeave = ZBool.False;
			}
		}

		protected override void OnConsigneeAddressChanged(EventArgs e)
		{
			base.OnConsigneeAddressChanged(e);

			if (e is ValueChangedEventArgs args && !args.OldValue.Equals(args.NewValue))
			{
				var consignee = Consignee;
				if (consignee != null)
				{
					// if the previous Org is different to the current Org, default ATL
					var previousAddress = !args.OldValue.IsEmpty ? Factory.Load<OrgAddress>((ZGuid)args.OldValue) : null;
					if (previousAddress == null || previousAddress.OA_OH != consignee.PK)
					{
						SetDefaultAuthorisedToLeave(consignee);
					}
				}
				// if organisation removed, set ATL to false
				else
				{
					WD_IsAuthorisedToLeave = ZBool.False;
				}
			}
		}

		void SetDefaultAuthorisedToLeave(OrgHeader consignee)
		{
			var client = Client;
			if (client != null)
			{
				WD_IsAuthorisedToLeave = AuthorityToLeaveHelper.GetConsigneeAuthorityToLeave(consignee.MainAddress, client.MainAddress);
			}
		}

		#endregion

		#region SetRateTransportZone

		public void SetRateTransportZone()
		{
			var consigneeDocAddress = this.LoadJobDocAddressQuickly(ConsigneeDocAddressRequirement.DefaultDocAddressType);
			if (consigneeDocAddress != null && !consigneeDocAddress.IsDeleted)
			{
				var transportCo = TransportCo;
				var transportZone = RateTransportZone.GetOperationZone(Factory, transportCo, consigneeDocAddress, consigneeDocAddress.E2_RN_NKCountryCode, null, consigneeDocAddress.Postcode, consigneeDocAddress.City);
				if (transportZone == null || transportCo == null)
				{
					WD_TZ_TransportZone = ZGuid.Empty;
				}
				else
				{
					WD_TZ_TransportZone = transportZone.PK;
				}
			}
		}

		#endregion

		#region TransportDefaultsProviders

		IEnumerable<OrgHeader> TransportDefaultsProviders
		{
			get
			{
				var transportCoDefaultingRules = WarehouseDataRegistry.Instance.TransportCoDefaultingRules.Value;

				var orgs = new OrgHeader[3];

				var consignee = Consignee;
				if (transportCoDefaultingRules.Consignee > 0 && consignee != null)
				{
					orgs[transportCoDefaultingRules.Consignee - 1] = consignee;
				}

				var client = Client;
				if (transportCoDefaultingRules.Client > 0 && client != null)
				{
					orgs[transportCoDefaultingRules.Client - 1] = client;
				}

				var warehouseAddressOrg = Warehouse?.WarehouseAddress?.Header;
				if (transportCoDefaultingRules.Warehouse > 0 && warehouseAddressOrg != null)
				{
					orgs[transportCoDefaultingRules.Warehouse - 1] = warehouseAddressOrg;
				}

				return orgs.WhereNotNull();
			}
		}

		#endregion

		#region SetDefaultUseDirectedPackageConsolidation

		void SetDefaultUseDirectedPackageConsolidation()
		{
			WD_UseDirectedPackingConsolidation = ClientPickingParams?.WPP_UseDirectedPackingConsolidation ?? false;
		}

		#endregion

		#endregion

		#region TransportPayerCode

		public ZString GetTransportBillToAccountCodeWithTransportCo()
		{
			var result = ZString.Empty;

			var transportCo = this.GetTransportCo();
			var transportBillTo = TransportBillTo;
			if (transportCo != null && transportBillTo != null)
			{
				var filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, transportCo.PK)
				.AddToFilter(OrgPatternMatchOverrideSchema.OO_LocalGuid, transportBillTo.PK)
				.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Constants.OrgPatternMatchOverrideRelationships.Organisation);

				result = Factory.LoadTop1<OrgPatternMatchOverride>(filter)?.OO_ForeignCode ?? ZString.Empty;
			}

			return result;
		}

		#endregion

		#region FulfillmentRules

		#region SupportsWhsOrderFulfillmentRule

		protected override bool SupportsWhsOrderFulfillmentRule
		{
			get { return true; }
		}

		#endregion

		#region IsFulfillmentRuleMet

		protected override bool IsFulfillmentRuleMetCore
		{
			get { return !(WD_WhsOrderFulfillmentRule == WhsOrderFulfillmentRuleList.Codes.All && ShortfallExists); }
		}

		#endregion

		#region OverrideFulfillmentRuleAndCreateLog

		public void OverrideFulfillmentRuleAndCreateLog(INotifications notify)
		{
			if (WD_WhsOrderFulfillmentRule == WhsOrderFulfillmentRuleList.Codes.None)
			{
				notify.AddMessageError(Res.GetString("79a1cd73-3561-4842-814e-efe173c83ce5", "The Fulfillment Rule is already set to 'None' for this Order: {0}", WD_DocketID));
			}
			else
			{
				ZString reference = GetOverrideFulfillmentRuleReference(notify);
				if (reference != ZString.Empty)
				{
					OverrideFulfillmentRuleAndCreateLog(reference, notify);
					if (Pick != null)
					{
						Pick.UpdatePickStatusIfRequired();
					}
				}
			}
		}

		public void OverrideFulfillmentRuleAndCreateLog(string reference, INotifications notify)
		{
			if (WD_WhsOrderFulfillmentRule == WhsOrderFulfillmentRuleList.Codes.None)
			{
				notify.AddMessageError(Res.GetString("79a1cd73-3561-4842-814e-efe173c83ce5", "The Fulfillment Rule is already set to 'None' for this Order: {0}", WD_DocketID));
			}
			else
			{
				WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, string.Format(Culture.Invariant, "Fulfillment Rule Overridden - Ref: {0}", reference));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		#endregion

		#region GetValidReferenceForOverrideFulfillmentRule

		public ZString GetOverrideFulfillmentRuleReference(INotifications notify)
		{
			const int MAX_REFERENCE_LENGTH = 80;
			var args = new UserResponseArgument
			{
				Caption = Res.GetString("80cd9339-6f47-4cfc-8ebf-db23283ae830", "Override Order Fulfillment Rule(s)"),
				Message = Res.GetString("fa04b069-f9a6-4ffc-b69f-192dce1ccc6f", "Please enter a Reference that will be logged against each order that has its Fulfillment Rule overridden. Maximum length is 80 characters."),
				Buttons = ZMessageBoxButtons.OKCancel,
				DefaultButton = ZMessageBoxDefaultButton.Button1,
				Icon = ZMessageBoxIcon.Information,
				MinimumResponseLength = 0
			};

			var reference = Globals.Message.QueryUserResponse(args);
			if (reference.Length > MAX_REFERENCE_LENGTH)
			{
				notify.AddMessageError(Res.GetString("198a7d87-e674-46d6-82d0-fc7c643fcbeb", "Please enter a Reference that is {0} characters or less in length.", MAX_REFERENCE_LENGTH));
				reference = ZString.Empty;
			}

			return reference;
		}

		#endregion

		#endregion

		#region Picking

		protected override void GetPickabilityCore(WhsPick.DocketPickabilityEventArgs pickability, ZStringBuilder errorMessage)
		{
			base.GetPickabilityCore(pickability, errorMessage);

			if (ConsigneeNameOrPK.IsEmpty || ConsigneeNameOrPK == ZGuid.Empty.ToString()) // Generating Orders from Rcv/Inv -- may not have a consignee on the inv line.
			{
				errorMessage.Append(Res.GetString("36be9e7b-3656-419b-84a1-f10b986381e6", "This {0} has no Consignee.", Description));
			}
			else if (IsDefaultDockDoorMissing())
			{
				errorMessage.Append(Res.GetString("7CB7F658-E2AE-404E-93B0-7975F07BDE81", "Cannot create Pick. Default outbound dock door location is not specified for this warehouse."));
			}
			else if (HasCustomsValidationErrorsInLines())
			{
				errorMessage.Append(Res.GetString("f126aec3-2614-4e81-a4d1-90d11a3fbe62", "This {0} has Customs validation errors on its Lines.", Description));
			}
			else
			{
				var isPermitAvailableResult = IsFTZCustomsOrderWithPermit ? FTZOrderPermitManager.IsPermitAvailable(this, Lines.Cast<WhsOrderLine>()) : ActionResult.Success();
				if (!isPermitAvailableResult.IsSuccess)
				{
					errorMessage.Append(isPermitAvailableResult.ErrorMessage);
				}
				else
				{
					ConsigneeDocAddress.Validation.ValidateAll();
					if (ConsigneeDocAddress.HasErrors())
					{
						errorMessage.Append(Res.GetString("fe2e72e2-3c3a-4270-b8c3-4b4f347559ec", "Consignee on this {0} has validation errors.", Description));
					}
				}
			}
		}

		protected override bool GetPickabilityForNewPickCore(WhsPick.DocketPickabilityEventArgs pickability, ZStringBuilder errorMessage, WhsPick newPick)
		{
			var result = base.GetPickabilityForNewPickCore(pickability, errorMessage, newPick);
			if (result && !newPick.WP_WA_DynamicPickAreaOverride.IsEmpty && Lines.Any(l => !l.WE_PalletID.IsEmpty))
			{
				errorMessage.Append(Res.GetString("1f5dabdb-b3cf-41ca-be49-7960fb304ad3", "Pick is using Dynamic Pick Face Replenishment and cannot attach Order with Ordered Pallet ID."));
				result = false;
			}
			return result;
		}

		bool HasCustomsValidationErrorsInLines()
		{
			var hasErrorsInCustomsData = false;
			if (IsCustomsTransaction)
			{
				hasErrorsInCustomsData = Lines.Any(l =>
				{
					var customsData = l.CustomsData;
					customsData.Validation.ValidateAll();
					return customsData.HasErrors;
				});
			}
			return hasErrorsInCustomsData;
		}

		bool IsDefaultDockDoorMissing()
		{
			var warehouse = Warehouse;
			return warehouse != null
				&& warehouse.IsInDatabase
				&& warehouse.WW_IsActive
				&& !warehouse.WW_IsVirtualWarehouse
				&& (warehouse.WW_WarehouseType == WarehouseTypes.Codes.Product || warehouse.IsFTZWarehouse)
				&& warehouse.DefaultOutboundDockDoorLocation == null;
		}

		#endregion

		#region ResetOrderAfterDetachingFromPick

		protected override void ResetOrderAfterDetachingFromPickCore()
		{
			base.ResetOrderAfterDetachingFromPickCore();

			WD_UnitsSent = 0m;
			WD_CubicSent = 0m;
			WD_WeightSent = 0m;
			WD_WeightSentUserEntered = 0m;
			WD_PalletsSent = 0;
			WD_GS_NKAssignedPacker = ZString.Empty;
			GrossWeightSent = 0;
			grossVolumeSent = null;
			netVolumeSent = null;
			PreviousPackageCount = null;
			ResetOrderLinesCustomData();
		}

		void ResetOrderLinesCustomData()
		{
			foreach (var line in GetLinesToPick())
			{
				var customsData = line.CustomsData;
				if (customsData != null)
				{
					customsData.WB_EntryKey = "";
					customsData.WB_EntryLineNo = 0;
				}
			}
		}

		#endregion

		#region Finalisation

		protected override bool RunPreFinaliseValidationCore()
		{
			new ValidationForFinalisationWithFutureFinalisedDate(this).ValidateWD_DocketStatusDescription();
			var result = base.RunPreFinaliseValidationCore();
			if (!IsFulfillmentRuleMet)
			{
				AddRowError(Res.GetString("f6cc6f9d-0309-4a9e-875d-d18ca7781c42", "Order cannot be finalized until Fulfillment Rule is met"));
				result = false;
			}

			var packageJob = PackageJob;
			if (packageJob != null)
			{
				result = HasAuditError(packageJob) && result; // don't short circuit
			}

			if (WD_IsLoadingRequired)
			{
				result = HasNonLoadPackages(packageJob) && result; // don't short circuit;
			}

			return result;
		}

		bool HasNonLoadPackages(PkgPackageJob packageJob)
		{
			var result = true;

			if (packageJob == null || packageJob.Packages.Count == 0)
			{
				AddRowError(Res.GetString("a9d86526-2793-44bd-b1f6-4fd8f3019349", "Order {0} requires Loading and Order has no Packages.", WD_DocketID));
				result = false;
			}
			else
			{
				var loadedPackageInfoLookup = Pick.GetLoadedPackagesInfo(packageJob);
				var nonLoadedPackages = new List<(PkgPackage Package, ZString LoadID)>();

				foreach (var package in packageJob.Packages)
				{
					if (!loadedPackageInfoLookup.TryGetValue(package.PK, out var packageInfo) || !packageInfo.IsLoaded)
					{
						nonLoadedPackages.Add((package, packageInfo?.LoadID ?? Res.GetString("cc688172-c3ea-45f1-8bae-d436431ca0be", "Unassigned")));
					}
				}

				if (nonLoadedPackages.Count > 0)
				{
					var packageErrors = string.Join("\r\n", nonLoadedPackages.Select(p => Res.GetString("bd49def8-ee65-40af-93ce-6fc1e242dd44", "Package '{0}' for Load '{1}'", p.Package.PackageIDWithFallback, p.LoadID)));

					AddRowError(Res.GetString("efd4d1b4-2282-4251-8674-16132f9485f8", "Order {0} requires Loading and not all Packages have been Loaded:\r\n{1}", WD_DocketID, packageErrors));
					result = false;
				}
			}

			return result;
		}

		bool HasAuditError(PkgPackageJob packageJob)
		{
			var result = true;

			var allPackages = packageJob.GetAllPackagesOnJob();
			if (allPackages.Any(p => p.KP_IsHeld))
			{
				AddRowError(Res.GetString("0bf20c3b-d14e-40a1-9883-461d0a78e30a", "There is a failed audit for Order {0}, or one or more Packages are Held. Remove the Hold Status on Held Packages to finalize the Order.", WD_DocketID));
				result = false;
			}
			else if (WD_QualityAuditRequired)
			{
				var allOuters = packageJob.GetAllNonContainerOutersAndFirstLevelPackagesOnContainers();
				if (!allOuters.Any(p => p.KP_PackageID.IsEmpty))
				{
					Factory.AddFetchHint(WhsPackageAuditSchema.Instance, new ZQuery(WhsPackageAuditSchema.WPA_WD_Order, PK));

					var auditsPKs = allPackages.Select(p => WhsPackageAuditManager.GetLastWhsPackageAudit(p)?.PK).WhereNotNull();
					Factory.AddFetchHint(WhsPackageAuditLineFailureSchema.Instance, new ZQuery(WhsPackageAuditLineFailureSchema.WPF_WPA_WhsPackageAudit, auditsPKs));

					if (HasAnyUnauditedPackage(allOuters))
					{
						AddRowError(Res.GetString("a6e62074-cccc-495d-aeaf-60ad5f6d6f23", "Order {0} requires a Package audit and not all Packages have been audited or it has audit failures.", WD_DocketID));
						result = false;
					}
				}
				else
				{
					AddRowError(Res.GetString("e886307f-f218-4969-b611-f2c115e7380f", "The Order {0} requires Quality Audit but not all the outer packages have an ID. All the outer packages must have an ID to complete the audits.", WD_DocketID));
					result = false;
				}
			}

			return result;
		}

		protected override void OnFinaliseSucceeded()
		{
			base.OnFinaliseSucceeded();

			DeleteEmptyPackagesIfPickIsCartonised();

			if (Warehouse.WW_AutoPrintPackingSlip)
			{
				PrintPackingSlip();
			}

			WD_IsOrderSelectedForFinalisation = false;
		}

		#region DeleteEmptyPackagesIfPickIsCartonised

		void DeleteEmptyPackagesIfPickIsCartonised()
		{
			var pick = Pick;
			if (pick.WP_IsCartonised)
			{
				var packageJob = PackageJob;
				if (packageJob != null)
				{
					var unpack = new UnpackItemsBusinessObject(packageJob, Array.Empty<PkgPackageItemDivotsWrapperAndBarcode>(), packageJob.Packages.Where(pkg => IsPackageEmpty(pkg)));
					unpack.RunValidationAndApplyChanges();
				}
			}
		}

		bool IsPackageEmpty(PkgPackage package)
		{
			return package.PackedItemDivots.Count == 0 && package.Packages.Count == 0;
		}

		#endregion

		protected override ZDateTimeOffset GetFinalisedDate()
		{
			var warehouse = Warehouse;
			return warehouse != null && warehouse.WW_UseRequiredDateForOutwardsFinalisedDate ? WD_RequiredDate : base.GetFinalisedDate();
		}

		#endregion

		#region SplitOrderLinesByAllocatedInventoriesForOrder

		public void SplitOrderLinesByAllocatedInventoriesForOrder()
		{
			short nextLineNumber = 0;
			var existingLineNumbers = Lines.Select(l => l.WE_LineNo).ToHashSet();

			foreach (WhsOrderLine orderLine in Lines)
			{
				if (orderLine.PickLines.Count > 1)
				{
					orderLine.ClearReleaseLines();

					decimal remainingQtyForLine = orderLine.WE_TransactionQuantity;

					var pickLinesToSplitBy = orderLine.PickLines.OrderBy(pl => pl.WZ_Units).Take(orderLine.PickLines.Count - 1);
					foreach (var pickLine in pickLinesToSplitBy)
					{
						nextLineNumber = GetNextLineNumber(existingLineNumbers, nextLineNumber);
						var newLine = SplitLineByAllocatedQuantity(orderLine, pickLine.WZ_Units, nextLineNumber);

						remainingQtyForLine -= pickLine.WZ_Units;
						pickLine.WZ_WE_TransactionLine = newLine.PK;
					}
					((IBusinessObjectInternals)orderLine).Row[WhsDocketLineSchema.Constants.WE_TransactionQuantity] = remainingQtyForLine;
					orderLine.ClearPackQuantity();
					orderLine.HasChanges = true;
				}
			}
		}

		WhsOrderLine SplitLineByAllocatedQuantity(WhsOrderLine lineToSplit, decimal qty, short lineNumber)
		{
			var splitLine = (WhsOrderLine)lineToSplit.Clone(new BusinessObjectCloneArgs(new[] { WhsDocketLineSchema.Constants.WE_TransactionQuantity, WhsDocketLineSchema.Constants.WE_LineNo }, typeof(WhsOrderLine)));
			splitLine.WE_WD = PK.ToGuid();
			((IBusinessObjectInternals)splitLine).Row[WhsDocketLineSchema.Constants.WE_TransactionQuantity] = qty;
			splitLine.WE_LineNo = lineNumber;
			splitLine.WE_DocketLineStatus = lineToSplit.WE_DocketLineStatus;
			splitLine.WE_FinalisedDate = lineToSplit.WE_FinalisedDate;
			return splitLine;
		}

		short GetNextLineNumber(HashSet<ZShort> existingLineNumbers, short currentLineNumber)
		{
			do
			{
				currentLineNumber++;
			}
			while (existingLineNumbers.Contains(currentLineNumber));

			return currentLineNumber;
		}

		#endregion

		#region BOM

		public BOMAutoCreateHelper BOM => bom ?? (bom = new BOMAutoCreateHelper(this));
		BOMAutoCreateHelper bom;

		#endregion

		#region Printing

		#region GetRFPickPackPrinter

		/// <summary>
		/// This will get the Printer to use for printing labels in RF Pick and Pack. First tries to
		/// get the printer assigned to the Staff directly, then the Printer assigned to the Staging Area,
		/// then the printer assigned to the Warehouse and lastly tries to get the printer for the Label Document.
		/// </summary>
		public IStmPrintQueue GetRFPickPackPrinter(IUser currentUser, IStmMenuItem documentToPrint)
		{
			var warehouse = Warehouse;
			var printer = warehouse != null ? GetPrinterFromDockDoorLocationOrWarehouse(warehouse) : null;
			if (documentToPrint != null && printer == null) // fall back to Printer stored against the Document if we couldn't get a printer
			{
				var defaultPrinter = StmDefaultPrinter.LoadDefaultPrinter(Factory, (GlbStaff)currentUser, documentToPrint);
				printer = defaultPrinter != null ? Factory.Load<StmPrintQueue>(defaultPrinter.SDP_SQ_Printer) : null;
			}

			return printer;
		}

		StmPrintQueue GetPrinterFromDockDoorLocationOrWarehouse(WhsWarehouse warehouse)
		{
			var ddlArea = Pick?.DockDoorLocation?.PickingArea;
			return (ddlArea != null ? Factory.Load<StmPrintQueue>(ddlArea.RFPickPackPrinterPK) : null)
				?? Factory.Load<StmPrintQueue>(warehouse.RFPickPackPrinterPK);
		}

		#endregion

		#region PrintPackingSlip

		public void PrintPackingSlip()
		{
			if (Globals.IsUserInteractive && !IsImportingData)
			{
				var pick = Pick;
				var notifications = (pick != null) ? pick.NotificationSubscriber : NotificationSubscriber;
				var packingSlipPrinter = new WhsPackingSlipDocumentPrinter(this, notifications);
				packingSlipPrinter.PrintDocument();
			}
		}

		#endregion

		#region Printing Events

		public event EventHandler<WhsOrderToPrintEventArgs> OnWhsOrderToPrint;

		public void CallOnWhsOrderToPrint(object sender, WhsOrderToPrintEventArgs e)
		{
			OnWhsOrderToPrint?.Invoke(sender, e);
		}

		#endregion

		#region Override Events

		protected override void OnClientChanged()
		{
			base.OnClientChanged();
			((IBusinessObjectState)ParentLines).RefreshBindingIncludingChildren();
		}

		protected override void OnWarehouseChanged()
		{
			base.OnWarehouseChanged();
			((IBusinessObjectState)ParentLines).RefreshBindingIncludingChildren();
		}

		protected override void OnDocketSubTypeChangedCore(ZString previousSubType)
		{
			base.OnDocketSubTypeChangedCore(previousSubType);
			SetDefaultOutwardTypeForCustomsJob();
		}

		#endregion

		#region SetDefaultOutwardTypeForCustomsJob

		void SetDefaultOutwardTypeForCustomsJob()
		{
			if (IsCustomsTransaction)
			{
				foreach (var customsData in Lines.Select(ol => ol.CustomsData).Where(c => c.WB_OutwardType.IsEmpty))
				{
					customsData.SetDefaultOutwardTypeIfEmpty();
				}
			}
		}

		#endregion

		#endregion

		#region Generating Data from Containers

		public void GenerateDataFromContainers()
		{
			// generate the lines from receive jobs with same container numbers
			int linesGenerated = PopulateNewLinesFromContainers();

			// notify user how many lines were generated
			string count = (linesGenerated > 0) ? linesGenerated.ToString(Culture.Current) : Res.GetString("40ae759b-9d59-48e8-bc0c-1c2a863e71e1", "No");
			NotificationSubscriber.Notify(new InfoNotification(GetGenerateDataFromContainersNotification(count).Message));
		}

		#region Implementation

		int PopulateNewLinesFromContainers()
		{
			int origLineCount = Lines.Count;
			var tempFactory = new BusinessObjectFactory();
			var receipts = new WhsReceiveCollection(tempFactory, GetReceiptsFromContainersQuery());

			foreach (WhsReceive receipt in receipts)
			{
				bool found = false;
				foreach (var orderContainer in Containers)
				{
					foreach (var receiptContainer in receipt.Containers)
					{
						if (orderContainer.WC_ContainerNum == receiptContainer.WC_ContainerNum)
						{
							found = true;
							break;
						}
					}
					if (found)
					{
						break;
					}
				}

				if (found)
				{
					foreach (WhsReceiveLine receiveLine in receipt.Lines)
					{
						var newOrderLine = this.Lines.AddNew();
						newOrderLine.WE_OP = receiveLine.WE_OP;
						newOrderLine.WE_TransactionQuantity = receiveLine.WE_TransactionQuantity;
						newOrderLine.WE_BondedEntryKey = receiveLine.WE_BondedEntryKey;
						newOrderLine.WE_ExpiryDate = receiveLine.WE_ExpiryDate;
						newOrderLine.WE_PackingDate = receiveLine.WE_PackingDate;
						newOrderLine.WE_PartAttrib1 = receiveLine.WE_PartAttrib1;
						newOrderLine.WE_PartAttrib2 = receiveLine.WE_PartAttrib2;
						newOrderLine.WE_PartAttrib3 = receiveLine.WE_PartAttrib3;
					}
				}
			}

			return Lines.Count - origLineCount;
		}

		ZDBOnlyQuery GetReceiptsFromContainersQuery()
		{
			var containerSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketContainer), WhsDocketContainerSchema.WC_WD);
			var query = new ZDBOnlyQuery(typeof(WhsDocket));
			query.AddSubQuery(containerSubQuery, JoinCondition.And);
			query.AddToFilter(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Receive);
			query.AddToFilter(WhsDocketSchema.WD_OH_Client, this.WD_OH_Client);
			query.AddToFilter(WhsDocketSchema.WD_WW_Whs, this.WD_WW_Whs);
			return query;
		}

		WhsErrorTypes GetGenerateDataFromContainersNotification(string count)
		{
			WhsErrorTypes result = OrderErrorTypes.DataGeneratedFromContainers(count);
			if (Lines.HasErrors())
			{
				result = OrderErrorTypes.DataGeneratedFromContainersHasErrors(count);
			}
			return result;
		}

		#endregion

		#endregion

		#region GetLinesToCalculateShortfall

		protected override WhsDocketLine[] GetLinesToCalculateShortfall()
		{
			return GetLinesToPick().Where(l => l.WE_WE_ParentDocketLine == ZGuid.Empty).ToArray();
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			base.Delete();
			DeletePackageJob();
			ViewRelatedActivityPivot.DeleteAllPivots(this);
			if (transportCoDocAddress != null)
			{
				transportCoDocAddress.DocAddressChanged -= OnTransportCoDocAddressChanged;
				transportCoDocAddress.OrganisationPKInfo.ValueChanged -= OnTransportCoDocAddressChanged;
			}
		}

		void DeletePackageJob()
		{
			var packageJob = PackageJob;
			if (packageJob != null)
			{
				packageJob.Delete();
			}
		}

		#endregion

		#region TemplateCopy

		protected override void TemplateCopyCore(WhsPickableDocket copy)
		{
			base.TemplateCopyCore(copy);

			copy.Validation.ValidateWD_ExternalReference();
		}

		protected override void TemplateCopyLines(WhsPickableDocket copy)
		{
			WrapperStrategy.TemplateCopyLines(copy);
		}

		#endregion

		#region Workflow Template

		protected override void GetTemplateSelectionCriteriaCore(ColumnValueRanker result)
			=> result.Add(ProcessTaskTemplateSchema.P0_SubType1, SalesChannel?.WSH_Code ?? ZString.Empty, ZString.Empty);

		#endregion

		#region CreateOperationalStatusChangeEvents

		protected override void CreateOperationalStatusChangeEventsCore()
		{
			base.CreateOperationalStatusChangeEventsCore();
			if (WD_DocketStatus == DocketStatus.Codes.Entered && ((ZGuid)WD_WPInfo.OriginalValue).IsValid)
			{
				CreateEventLog(Events.WarehousePickCancelled, Res.GetString("6a0c2357-a1cc-4843-942c-025beb920011", "Pick Canceled: ") + CancelledPickNo);
				CancelEvents(Events.WarehouseOrderPicking);
			}
			else if (WD_WP.IsValid && (!IsInDatabase || ((ZGuid)WD_WPInfo.OriginalValue).IsEmpty))
			{
				CreateEventLog(Events.WarehouseOrderPicking);
			}
		}

		#endregion

		#region SyncPickWithOrder

		// To make this work on MOPs we need to be able to access Order.OrderedInventories, because Pick.OrderedInventories contains the WhsPickOrderedInventory objects for *all* Orders on the Pick,
		// and it may deallocate from Order 1 and then reallocate to Order 2 (if Order 2 had a shortfall).
		//
		// An alternative would be to modify the Update the WhsPickOrderedInventory.PickLineQuantity property to be able to accept a Docket.

		/// <summary>
		/// Reallocates stock, call this method when the OrderLines are modified after the Order is Picked.
		/// NOT SUPPORTED ON A MULTI-ORDER PICK or US Customs Jobs.
		/// </summary>
		public void SyncPickWithOrder()
		{
			if (!IsFinalisedOrCancelled)
			{
				var pick = this.Pick;

				if (pick != null && !pick.IsFinalisedOrCancelled)
				{
					if (pick.IsMultiOrderPick)
					{
						throw new NotSupportedException("Order.SyncPickWithOrder() is not supported on a Multi-Order Pick.");
					}
					else if (IsUSBonded)
					{
						throw new NotSupportedException("Order.SyncPickWithOrder() is not supported for US Customs Orders.");
					}

					if (IsSyncPickWithOrderRequired())
					{
						pick.ClearInventoryCache();
						pick.ClearOrderedInventoriesCache();
						pick.ClearOrdersCache();

						foreach (IWhsPickOrderedInventoryInternals orderedInventory in pick.OrderedInventories)
						{
							orderedInventory.AdjustDownPickAndAttributeMetLinesToMatchOrderQty();
						}
					}

					foreach (var line in Lines.ToArray())
					{
						if (line.WE_TransactionQuantity == 0)
						{
							line.Delete();
						}
					}
				}
			}
		}

		bool IsSyncPickWithOrderRequired()
		{
			foreach (WhsOrderLine line in Lines)
			{
				if (!line.IsInDatabase || line.PickLineQuantity > line.WE_TransactionQuantity || line.SumOfUnitsMet > line.WE_TransactionQuantity)
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region OrderLineFromInventory

		OrderLineFromInventoryHelper OrderLineFromInventoryHelper
		{
			get { return orderLineFromInventoryHelper ?? (orderLineFromInventoryHelper = new OrderLineFromInventoryHelper(NotificationSubscriber, this)); }
		}
		OrderLineFromInventoryHelper orderLineFromInventoryHelper;

		protected override WhsDocketLine CreateDocketLineFromInventoryCore(WhsInventoryView inventoryList)
		{
			return OrderLineFromInventoryHelper.CreateDocketLineFromInventory(this.Lines, inventoryList);
		}

		protected override void AcceptInventoryLinesFromSearchGridCore(BusinessObject[] inventoryList)
		{
			OrderLineFromInventoryHelper.AcceptInventoryLinesFromSearchGrid(this.Lines, inventoryList);
		}

		#endregion

		#region IAttachedOrder Members

		public const string AttachedOrderType = "WHS";

		ZString IAttachedOrder.JobNo
		{
			get { return WD_ExternalReference; }
		}

		ZString IAttachedOrder.JobDescription
		{
			get { return Res.GetString("355ae270-e707-48cf-84a5-358e7dc26d8a", "Warehouse Order"); }
		}

		ZString IAttachedOrder.JobStatus
		{
			get { return WD_DocketStatus; }
		}

		ZString IAttachedOrder.GoodsDescription
		{
			get { return GoodsDescriptionWithFallback; }
		}

		ZDateTime IAttachedOrder.JobDate
		{
			get { return WD_RequiredDate.ToZDateTime(); }
		}

		ZString IAttachedOrder.JobType
		{
			get { return AttachedOrderType; }
		}

		ModuleIdentifier IAttachedOrder.ModuleID
		{
			get { return ModuleIDs.WhsOrder; }
		}

		ControllerID IAttachedOrder.ControllerID
		{
			get { return ControllerIDs.WhsOrder; }
		}

		bool IAttachedOrder.ShouldSkipAllValidations
		{
			get { return shouldSkipAllValidations && !Env.Security.WhsOrder.IsAllowed; }
			set { shouldSkipAllValidations = value; }
		}
		bool shouldSkipAllValidations;

		ZString IAttachedOrder.TransportMode => ZString.Empty;

		ZDateTime IAttachedOrder.ETD => ZDateTime.Empty;

		ZDateTime IAttachedOrder.ETA => ZDateTime.Empty;

		ZDateTime IAttachedOrder.RequiredExWorks => ZDateTime.Empty;

		ZDateTime IAttachedOrder.RequiredInStore => ZDateTime.Empty;

		ZDecimal IAttachedOrder.TotalWeight => ZDecimal.Zero;

		ZString IAttachedOrder.WeightUnit => ZString.Empty;

		ZDecimal IAttachedOrder.TotalVolume => ZDecimal.Zero;

		ZString IAttachedOrder.VolumeUnit => ZString.Empty;

		ZDecimal IAttachedOrder.QuantityRemaining => ZDecimal.Zero;

		ZDecimal IAttachedOrder.QuantityInvoiced => ZDecimal.Zero;

		ZDecimal IAttachedOrder.QuantityOrdered => ZDecimal.Zero;

		ZDecimal IAttachedOrder.QuantityReceived => ZDecimal.Zero;

		ZString IAttachedOrder.BuyerOrgCode => ZString.Empty;

		ZString IAttachedOrder.SupplierOrgCode => ZString.Empty;

		ZInt IAttachedOrder.TotalPacks => ZInt.Zero;

		ZString IAttachedOrder.PacksType => ZString.Empty;

		#endregion

		#region IAutoCreateAddressOnUnmatch Members

		bool IAutoCreateAddressOnUnmatch.CreateOrgAddressOnUnmatch
		{
			get { return WarehouseDataRegistry.Instance.CreateNewAddressOnUnmatchedAddressForOrders.Value; }
		}

		#endregion

		#region IRatingSupporter Members

		RatingAdaptersProvider IRatingSupporter.AdaptersProvider
		{
			get { return new WhsOrderRatingAdaptersProvider(this); }
		}

		#endregion

		#region IBuyerSupplierRelationshipConsumer Members

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignee
		{
			get { return Consignee; }
			set
			{
				if (!ConsigneeDocAddress.E2_AddressOverride)
				{
					ConsigneePK = value != null ? value.PK : ZGuid.Empty;
				}
			}
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsigneeChanged
		{
			add { ConsigneeDocAddressChanged += value; }
			remove { ConsigneeDocAddressChanged -= value; }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.ConsigneeDeliveryAddress
		{
			get { return null; }
		}

		public void RestoreImportBrokerFallback()
		{
		}

		OrgHeader IBuyerSupplierRelationshipConsumer.Consignor
		{
			get { return Client; }
			set { WD_OH_Client = value != null ? value.PK : ZGuid.Empty; }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ConsignorChanged
		{
			add { WD_OH_ClientInfo.ValueChanged += value; }
			remove { WD_OH_ClientInfo.ValueChanged -= value; }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.ConsignorPickupAddress
		{
			get { return null; }
		}

		ZString IBuyerSupplierRelationshipConsumer.ContainerMode
		{
			get
			{
				var result = WD_ContainerMode;
				if (IsAddingNewLinkSemaphore.IsSuspended && result.IsEmpty)
				{
					result = Constants.ContainerModes.LTL;
				}
				return result;
			}
			set
			{
				if (Lookups.ContainerModes.ContainsCode(value))
				{
					WD_ContainerMode = value;
				}
			}
		}

		ZGuid IBuyerSupplierRelationshipConsumer.DeliveryCartageCoPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.Destination
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.DischargePort
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsCurrency
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.GoodsDescription
		{
			get { return WD_GoodsDescription; }
			set { WD_GoodsDescription = value; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ImportBrokerPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZBool IBuyerSupplierRelationshipConsumer.IsSettingDefaultValues { get; set; }

		ZString IBuyerSupplierRelationshipConsumer.LoadPort
		{
			get { return ZString.Empty; }
			set { }
		}

		event EventHandler IBuyerSupplierRelationshipConsumer.ModesChanged
		{
			add
			{
				WD_ContainerModeInfo.ValueChanged += value;
				WD_TransportModeInfo.ValueChanged += value;
			}
			remove
			{
				WD_ContainerModeInfo.ValueChanged -= value;
				WD_TransportModeInfo.ValueChanged -= value;
			}
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoCopyBills
		{
			get { return ZByte.Zero; }
			set { }
		}

		ZByte IBuyerSupplierRelationshipConsumer.NoOriginalBills
		{
			get { return ZByte.Zero; }
			set { }
		}

		JobDocAddress IBuyerSupplierRelationshipConsumer.NotifyPartyDocumentaryAddress
		{
			get { return null; }
		}

		ZString IBuyerSupplierRelationshipConsumer.Origin
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.PaymentTerms
		{
			get { return WD_INCO; }
			set
			{
				if (Lookups.INCOTerms.ContainsCode(value))
				{
					WD_INCO = value;
				}
			}
		}

		ZGuid IBuyerSupplierRelationshipConsumer.PickupCartageCoPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ReceivingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.ReleaseType
		{
			get { return ZString.Empty; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreGoodsCurrencyFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreNumberOfBillsWithFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreReceivingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreSendingAgentFallback()
		{
		}

		void IBuyerSupplierRelationshipConsumer.RestoreServiceLevelFallback()
		{
			SetDefaultServiceLevel();
		}

		ZGuid IBuyerSupplierRelationshipConsumer.SendingAgentPK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZString IBuyerSupplierRelationshipConsumer.ServiceLevel
		{
			get { return WD_PL_NKCarrierServiceLevel; }
			set { WD_PL_NKCarrierServiceLevel = value; }
		}

		ZGuid IBuyerSupplierRelationshipConsumer.ShippingLinePK
		{
			get { return ZGuid.Empty; }
			set { }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldPromptToSaveBuyerSupplierRelationship
		{
			get { return Env.Registry.UseBuyerSupplierRelationships; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePaymentTerm
		{
			get { return true; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestoreHandlingInformation
		{
			get { return true; }
		}

		ZString IBuyerSupplierRelationshipConsumer.TransportMode
		{
			get
			{
				var result = WD_TransportMode;
				if (IsAddingNewLinkSemaphore.IsSuspended && result.IsEmpty)
				{
					result = Constants.TransportModes.Road;
				}

				return result;
			}
			set { WD_TransportMode = value; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldRestorePickupDeliveryAndNotifyPartyAddress
		{
			get { return true; }
		}

		void IBuyerSupplierRelationshipConsumer.RestoreEFreightStatusFallback(ZString defaultStatus)
		{
		}

		ZBool IBuyerSupplierRelationshipConsumer.PreventBuyerSupplierRelationships
		{
			get { return false; }
		}

		ZBool IBuyerSupplierRelationshipConsumer.ShouldDefaultContainerModeAndIsContainerised(ZString containerMode) => false;

		#endregion

		#region ICartageLooseCargo Members

		ZString ICartageLooseCargo.BookedDimensionUnit
		{
			get { return "M"; }
		}

		ZDecimal ICartageLooseCargo.BookedHeight
		{
			get { return 0m; }
		}

		ZDecimal ICartageLooseCargo.BookedLength
		{
			get { return 0m; }
		}

		ZString ICartageLooseCargo.BookedPackType
		{
			get { return CartageHelper.OrderLinesGoodsTypeCode; }
		}

		ZInt ICartageLooseCargo.BookedPackages
		{
			get { return CartageHelper.OrderLinesPackageCount; }
		}

		ZDecimal ICartageLooseCargo.BookedVolume
		{
			get { return WD_CubicSent; }
		}

		ZString ICartageLooseCargo.BookedVolumeUnit
		{
			get { return WD_TotalCubicUnit; }
		}

		ZDecimal ICartageLooseCargo.BookedWeight
		{
			get { return WD_WeightSent; }
		}

		ZString ICartageLooseCargo.BookedWeightUnit
		{
			get { return WD_TotalWeightUnit; }
		}

		ZDecimal ICartageLooseCargo.BookedWidth
		{
			get { return 0m; }
		}

		IReadOnlyCollection<UNDGDataItem> ICartageLooseCargo.DangerousGoods
		{
			get { return Array.Empty<UNDGDataItem>(); }
		}

		#endregion

		#region ICartageParent Members

		ZGuid ICartageParent.BranchPK
		{
			get { return ZGuid.Empty; }
		}

		ZGuid ICartageParent.CartageParentID
		{
			get { return PK; }
		}

		ZString ICartageParent.CartageParentTableCode
		{
			get { return WhsDocketSchema.Constants.Prefix; }
		}

		IReadOnlyCollection<CartageType> ICartageParent.CartageTypes
		{
			get { return new CartageType[] { new WhsCartageType(this) }; }
		}

		event EventHandler ICartageParent.CartageTypesChanged
		{
			add { }
			remove { }
		}

		ControllerID ICartageParent.ControllerID
		{
			get { return ControllerIDs.WhsOrder; }
		}

		CartageType ICartageParent.GetLocalCartageType
		{
			get { return new WhsCartageType(this); }
		}

		ZString ICartageParent.GoodsDescription
		{
			get { return GoodsDescriptionWithFallback; }
		}

		ZGuid ICartageParent.JobHeaderPK
		{
			get { return JobHeader != null ? JobHeader.PK : ZGuid.Empty; }
		}

		ZGuid ICartageParent.LocalClientAddressPK
		{
			get { return Client.MainAddress.PK; }
		}

		ZString ICartageParent.OrderReferenceNumber
		{
			get { return WD_ExternalReference; }
		}

		ZString ICartageParent.ServiceLevel
		{
			get { return WD_PL_NKCarrierServiceLevel; }
		}

		ZString ICartageParent.TotalPackType
		{
			get { return CartageHelper.OrderLinesGoodsTypeCode; }
		}

		ZInt ICartageParent.TotalPackages
		{
			get { return CartageHelper.OrderLinesPackageCount; }
		}

		ZDecimal ICartageParent.TotalVolume
		{
			get { return WD_CubicSent; }
		}

		ZString ICartageParent.TotalVolumeUnit
		{
			get { return WD_TotalCubicUnit; }
		}

		ZDecimal ICartageParent.TotalWeight
		{
			get { return WD_WeightSent; }
		}

		ZString ICartageParent.TotalWeightUnit
		{
			get { return WD_TotalWeightUnit; }
		}

		ZString ICartageParent.UniqueConsignmentID
		{
			get { return WD_DocketID; }
		}

		ZString ICartageParent.WayBillNumber
		{
			get { return WD_BOLNo; }
		}

		bool ICartageParent.RebuildLocalCartageMenuOnClick
		{
			get { return true; }
		}

		bool ICartageParent.UseJobTotals
		{
			get { return true; }
		}

		void ICartageParent.CartageCreatedAndSaved()
		{
			OnTransportCreatedAndSaved();
		}

		IStmALogParent ICartageParent.BusinessObjectForRelatedEvents
		{
			get { return this; }
		}

		IDocManagerSupport ICartageParent.BusinessObjectForRelatedEDocs
		{
			get { return this; }
		}

		public WhsOrderCartageHelper CartageHelper
		{
			get { return cartageHelper ?? (cartageHelper = new WhsOrderCartageHelper(this)); }
		}

		WhsOrderCartageHelper cartageHelper;

		#region class WhsCartageType

		public class WhsCartageType : CartageType
		{
			public WhsCartageType(WhsOrder order)
				: base(order)
			{
				Order = order;
			}

			protected readonly WhsOrder Order;

			/// <summary>
			/// Fired when cartage is created / sent to cartage company.
			/// </summary>
			public override void CartageAdvised(BusinessObjectFactory factoryToSaveIn)
			{
				ICommonCartage cartageJob = factoryToSaveIn.LoadTop1<ICommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, Order.PK));
				if (cartageJob != null)
				{
					Order.WD_TransportReference = (ZString)((BusinessObject)cartageJob)[JobCartageSchema.JJ_ConsignmentID];
				}
			}

			public override IReadOnlyCollection<ICartageContainer> CartageContainers
			{
				get { return Order.Containers.ToArray(); }
			}

			public override ZString CartageJobType
			{
				get { return (Order.Containers.Count > 0) ? Core.Constants.CartageJobType.NEW_WarehouseContainerizedDelivery : Core.Constants.CartageJobType.NEW_WarehouseLooseDelivery; }
			}

			public override IReadOnlyCollection<ICartageLooseCargo> CartageLooseCargo
			{
				get { return new ICartageLooseCargo[] { Order }; } // we only want one line for Warehouse Cartage
			}

			public override OrgAddress LocalTransportProviderAddress
			{
				get { return Order.TransportCoDocAddress.Address; }
			}

			public override ZPropertyInfo CartageAddressInfo
			{
				get { return Order.TransportBillToDocAddress.E2_OA_AddressInfo; }
			}

			public override void DeliveryCompleted(DocAddressType addressType, ZDateTime timeOut)
			{
				// once cartage is complete (eg. delivery date is entered for given addressType (eg Warehouse or Consignee)
			}

			public override void DeliveryCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
			{
				// same as above but for containers
			}

			public override MultilingualString Description
			{
				get { return (NoResString)Order.GoodsDescriptionWithFallback; }
			}

			public override ZString DropMode
			{
				get { return Order.WD_DropMode; }
			}

			public override ZDateTime E_ARV
			{
				get { return ZDateTime.Empty; } // est. arrival of cartage
			}

			public override ZDateTime E_DEP
			{
				get { return ZDateTime.Empty; }
			}

			public override ZDateTime A_ARV
			{
				get { return ZDateTime.Empty; } // act. arrival of cartage
			}

			public override ZDateTime A_DEP
			{
				get { return ZDateTime.Empty; }
			}

			public override ZDateTime EstimatedCartageDelivery
			{
				get { return Order.WD_RequiredDate.ToZDateTime(); }
			}

			public override ZDateTime EstimatedCartagePickup
			{
				get { return Order.WD_FinalisedDate.IsValid ? Order.WD_FinalisedDate.ToZDateTime() : ZDateTime.Empty; }
			}

			public override ZDateTime FCLAvailabilityDate
			{
				get { return ZDateTime.Empty; }
			}

			public override ZDateTime FCLStorageDate
			{
				get { return ZDateTime.Empty; }
			}

			public override ZDateTime FCLCutOff
			{
				get { return ZDateTime.Empty; }
			}

			public override ZDateTime FCLReceivalCommences
			{
				get { return ZDateTime.Empty; }
			}

			public override ZDateTime LCLCutOff
			{
				get { return ZDateTime.Empty; }
			}

			public override ZDateTime LCLReceivalCommences
			{
				get { return ZDateTime.Empty; }
			}

			public override JobDocAddress GetCartageAddress(ZString orgType)
			{
				JobDocAddress result = null;

				if (orgType == LocalCartageJobOrgTypeList.Codes.WHS)
				{
					result = JobDocAddress.GetOrCreateNonPersistantDocAddress(Order.Warehouse, DocAddressType.PickUpAddress, Order.Warehouse.WarehouseAddress.PK);
				}
				else if (orgType == LocalCartageJobOrgTypeList.Codes.CNE)
				{
					result = Order.ConsigneeDocAddress;
				}

				return result;
			}

			protected override ZPropertyInfo[] GetCartageAddressInfosToMonitor()
			{
				return Array.Empty<ZPropertyInfo>();
			}

			public override ZDateTime LCLAvailabilityDate
			{
				get { return ZDateTime.Empty; }
			}

			public override ZDateTime LCLStorageDate
			{
				get { return ZDateTime.Empty; }
			}

			public override void PickupCompleted(DocAddressType addressType, ZDateTime timeOut)
			{
				// add a log or something to say the pickup was completed?
			}

			public override void PickupCompleted(AutoJobContainer container, DocAddressType addressType, ZDateTime timeOut)
			{
				// same as above but for containers
			}

			public override void SetTotalDemurrage(TimeSpan demurrage)
			{
			}

			public override ZString PortOfDischarge
			{
				get { return ""; }
			}

			public override ZString PortOfLoading
			{
				get { return ""; }
			}

			public override ZString Vessel
			{
				get { return ""; }
			}

			public override ZString VoyageFlight
			{
				get { return ""; }
			}

			protected override MultilingualString GetDescriptionForMenu()
			{
				return MultilingualString.Join(": ", (NoResString)Order.WD_ExternalReference, Description);
			}

			public override IEnumerable<ZString> GetMatchingDirectionCodes()
			{
				return new ZString[]
				{
					Constants.CartageDirection.Export,
					Constants.CartageDirection.Import,
					Constants.CartageDirection.Origin,
					Constants.CartageDirection.Destination,
					Constants.CartageDirection.Local,
					Constants.CartageDirection.LineHaul,
				};
			}
		}

		#endregion

		#endregion

		#region IDocManagerSupport Members

		protected override DocManagerInfo GetNewDocManagerInfo()
		{
			return new WhsOrderDocManagerInfo(this, Constants.DocManagerCodes.WarehouseOrderDocket);
		}

		#endregion

		#region IDocumentSupportable Members

		protected override DocumentSupporter GetNewDocumentSupporter()
		{
			return documentSupporter ?? (documentSupporter = new WhsOrderDocumentSupporter(this));
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
			return new DtbBookingDirection[] { DtbBookingDirection.DLV };
		}

		IJobInvoicingPlugIn IDtbBookingParent.InvoicingJob
		{
			get { return this; }
		}

		void IDtbBookingParent.TransportBookingCreatedOrUpdated(IEnumerable<IDtbBooking> bookings)
		{
			OnTransportCreatedAndSaved();
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

		#region IDocketInternals Members Override

		protected override void FlagDocketAsFinalisedCore() => WD_FinalisedDate = GetFinalisedDate();

		#endregion

		#region ISendEmailSource Members

		protected override AddressBookSelection GetAddressBookSelection()
		{
			var result = base.GetAddressBookSelection();
			result.AddRecipient(Consignee);
			result.AddRecipient(GoodsBillTo);
			result.AddRecipient(this.GetTransportCo());
			result.AddRecipient(TransportBillTo);
			return result;
		}

		// Note: The templates shown on ReleaseOrdersModuleButtonGrid's Send E-mail form
		// were in the None category.
		protected override string GetTemplateCategory()
		{
			return MailTemplateCategoryList.Codes.WarehouseOrder;
		}

		#endregion

		#region IJobInvoicingPlugin Members

		protected override WhsDocketInvoicingSupporter GetNewInvoicingSupporter()
		{
			return new WhsOrderInvoicingSupporter(this);
		}

		#endregion

		#region IJobInvoicingAdditionalData Members

		protected override CustomPropertyContainer<JobCharge> GetAdditionalProperties()
		{
			return new WhsOrderJobInvoicingAdditionalDataPropertyProvider().GetAdditionalProperties();
		}

		#endregion

		#region IModuleToModule Members

		IOrgHeader IModuleToModule.RecipientOrganisation
		{
			get { return Forwarder; }
		}

		bool IModuleToModule.CanExportData(out ZString errorMessage)
		{
			errorMessage = "";

			if (Forwarder == null)
			{
				errorMessage = Res.GetString("8edc566f-8ea2-4ba0-be1f-3505d2e99bc9", "Cannot create a Shipment as no Freight Forwarder is specified.");
			}
			else if (PortOfDestination.IsEmpty)
			{
				errorMessage = Res.GetString("d4b6129f-2e0e-400c-8bb1-a808842bc0ab", "Cannot create a Shipment as a Consignee Location Port/UNLOCO could not be determined. Check the City, State and Country/Region for a valid combination.");
			}
			else if (WD_ContainerMode.IsEmpty && !WD_TransportMode.IsEmpty && !Lookups.ContainerModes.ContainsCode(Constants.ContainerModes.LTL))
			{
				errorMessage = Res.GetString("fdc9d810-7fc9-4374-8902-03d4c097033f",
					"Cannot create a Shipment as the Container Mode is empty and the entered Transport Mode '{0}' does not support Container Mode '{1}'",
					Lookups.TransportModes.GetDescriptionFromCode(WD_TransportMode), Lookups.ContainerModesFromTransportMode(Constants.TransportModes.Road).GetDescriptionFromCode(Constants.ContainerModes.LTL));
			}
			else if (WD_TransportMode.IsEmpty && !WD_ContainerMode.IsEmpty && !Lookups.ContainerModesFromTransportMode(Constants.TransportModes.Road).ContainsCode(WD_ContainerMode))
			{
				errorMessage = Res.GetString("aeb1f797-d6e8-4b67-8433-609c69778533",
					"Cannot create a Shipment as the Transport Mode is empty and Transport Mode '{0}' does not support the entered Container Mode '{1}'",
					Lookups.TransportModes.GetDescriptionFromCode(Constants.TransportModes.Road), Lookups.ContainerModes.GetDescriptionFromCode(WD_ContainerMode));
			}

			return errorMessage.IsEmpty;
		}

		BusinessObject IModuleToModule.GetRelatedObject()
		{
			return GetRelatedShipment();
		}

		BusinessObject GetRelatedShipment()
		{
			if (relatedShipment == null)
			{
				var pivotQuery = new ZQuery(WhsDocketJobPivotSchema.WV_WD_Docket, PK);
				pivotQuery.AddToFilter(WhsDocketJobPivotSchema.WV_DocketType, DocketType.Codes.Order);

				var pivots = Factory.Load<WhsDocketJobPivot>(pivotQuery);

				foreach (var pivot in pivots)
				{
					var relatedJob = Factory.Load(pivot.WV_ParentTableCode, pivot.WV_ParentId);
					if (relatedJob != null && relatedJob.TableName == JobShipmentSchema.Constants.TableName)
					{
						relatedShipment = relatedJob;
						break;
					}
				}
			}

			return relatedShipment;
		}
		BusinessObject relatedShipment;

		void IModuleToModule.AddToRelatedJobs(BusinessObject loadedJob)
		{
			RelatedJobs.Add(loadedJob);
		}

		#endregion

		#region IOrdersDocumentSupport Members

		public DocumentWrapper[] GetPackageLabelDocumentWrappers()
		{
			WhsDocketLabelControl legacyDocketLabelControl = new WhsDocketLabelControl(this, this.WD_PackagesSent);
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.WhsOrder, legacyDocketLabelControl) };
		}

		public DocumentWrapper[] GetDeliveryLabelDocumentWrappers(Action<object, WhsOrderToPrintEventArgs> onPrintEvent)
		{
			List<DocumentWrapper> wrappers = new List<DocumentWrapper>();
			DeliveryLabelLineCollection deliveryLabelLines = new DeliveryLabelLineCollection(this, Factory);
			WhsDocketsLabelControl ordersLabelControl = new WhsDocketsLabelControl(deliveryLabelLines);
			WhsOrderToPrintEventArgs eventArgs = new WhsOrderToPrintEventArgs(ordersLabelControl, Core.Constants.DataContext.WhsDeliveryLabels);
			onPrintEvent(this, eventArgs);
			if (eventArgs.ContinueToPrint)
			{
				foreach (WhsDocketLabelLine line in eventArgs.DocketsLabelControl.Lines)
				{
					if (line.NumberOfLabelsToPrint > 0)
					{
						wrappers.AddRange(new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.WhsOrder, line.LegacyDocketLabelControl) });
					}
				}
			}
			return wrappers.ToArray();
		}

		#endregion

		#region IShouldPackTrackedPackagesViaDivot

		bool IShouldPackTrackedPackagesViaDivot.ShouldPackTrackedPackagesViaDivot => false;

		#endregion

		#region IPackingParentDefaultPackageType Members

		ZString IPackingParentDefaultPackageType.DefaultOuterPackType
		{
			get { return ClientPickingParams?.WPP_F3_NKPackType ?? ZString.Empty; }
		}

		#endregion

		#region IPackingParentWithPackableItems Members

		IEnumerable<IPackableItemParent> IPackingParentWithPackableItems.PackableItemParents => PackableItemParents.Typed;

		void IPackingParentWithPackableItems.LoadAllPackableItemParentsInOneHit(PkgPackageJob packageJob)
		{
			// poke collection to build it which will load relevant fetch hints
			_ = packageJob?.PackableItemParents;
		}

		event EventHandler IPackingParentWithPackableItems.PackableItemParentsCountChanged
		{
			add { PackableItemParents.CountChanged += value; }
			remove { PackableItemParents.CountChanged -= value; }
		}

		#region IPackingParent.GetPackageActionStrategy

		IPackageActionStrategy IPackingParent.GetPackageActionStrategy(PkgPackage package)
		{
			foreach (IWhsPackageStrategy packageStrategy in PackageStrategies)
			{
				var strategy = packageStrategy.GetPackageActionStrategy(this, package);
				if (strategy != null)
				{
					return strategy;
				}
			}

			return new PackageActionStrategy(package);
		}

		#endregion

		ControllerID IPackingParent.ControllerID => JobControllerId;

		DocumentOptions IPackingParent.DocumentOptions => DocumentOptions.None;

		ZString IPackingParent.JobDescription => Res.GetString("876f9247-5c1b-4018-a159-d6511fe3e477", "Warehouse {0}", Description);

		ZString IPackingParent.JobNo => WD_ExternalReference;

		ZString IPackingParent.ConnoteNo => ZString.Empty;

		void IPackingParent.OnPackageJobReleased()
		{
			Logs.AddNew(AutoEvents.PickedUp);
		}

		void IPackingParent.OnPackageJobCreatedOrLoaded(PkgPackageJob packageJob)
		{
			packageJob.MassPackageProcessFinished -= PackageJob_MassPackageProcessFinished;
			packageJob.MassPackageProcessFinished += PackageJob_MassPackageProcessFinished;
			packageJob.PackageDataChanged -= PackageJob_PackageDataChanged;
			packageJob.PackageDataChanged += PackageJob_PackageDataChanged;
			packageJob.Packages.CollectionCountChange -= Packages_CollectionCountChange;
			packageJob.Packages.CollectionCountChange += Packages_CollectionCountChange;
		}

		void IPackingParent.OnPackageDelete(PkgPackage package)
		{
			foreach (IWhsPackageStrategy strategy in PackageStrategies)
			{
				strategy.OnPackageDelete(this, package);
			}
		}

		void IPackingParent.OnContainerIDChanged(PkgPackage container)
		{
		}

		public YesNoWithReasonForNo IsAutoPackAllowed
		{
			get
			{
				var consignee = Consignee;
				return (consignee != null && !consignee.MiscServ.OM_IsAutoPackAllowed)
					? YesNoWithReasonForNo.No(Res.GetString("13c8564a-85f0-41e1-b7f5-9648187d5b8d", "Auto-Pack is not enabled for the current Consignee."))
					: YesNoWithReasonForNo.Yes;
			}
		}

		bool IPackingParent.IsAutoPrintAllowed
		{
			get
			{
				var result = false;
				var consignee = Consignee;
				if (consignee != null)
				{
					result = consignee.MiscServ.OM_IsLabelPrintedOnClosePackage;
				}
				else if (ConsigneeDocAddress?.E2_AddressOverride ?? false)
				{
					result = PackingRegistry.Instance.AutoPrintLabelOnPackageCloseForOverriddenConsigneeAddresses.Value;
				}

				return result;
			}
		}

		bool IPackingParent.IsParentJobFinalised => IsPickFinalised;

		IEnumerable<KeyValuePair<TypeWithDescription, IZType>> IPackingParent.GetAdditionalEventContextValuesFromParent()
		{
			return ObjectFactory.Get<IWarehousePackingJobParentEventContextValueProvider>().GetAdditionalEventContextValues(this);
		}

		bool IPackingParent.IsPackingJobReadOnly
		{
			get
			{
				var pick = Pick;
				var isReadOnly = pick != null && (pick.IsFinalised || pick.IsReadyForPlanningOrPlanned);
				if (!isReadOnly && (Warehouse?.WW_GG_ReleaseGroup.IsValid ?? false))
				{
					var query = new ZDBOnlyQuery(typeof(WhsLoad));
					var loadOrderQuery = new ZDBOnlySubQuery(typeof(WhsLoadOrder), WhsLoadOrderSchema.WOV_WLO_Load);
					loadOrderQuery.AddToFilter(WhsLoadOrderSchema.WOV_WD_Docket, SQLComparisonOperator.Equal, PK);
					query.AddSubQuery(loadOrderQuery, JoinCondition.And);
					query.AddToFilter(WhsLoadSchema.WLO_TaskPlanningStatus, SQLComparisonOperator.Equal, new[] { TaskPlanningStatus.Codes.Ready, TaskPlanningStatus.Codes.Planned });

					isReadOnly = Factory.Exists(typeof(WhsLoad), query);
				}

				return isReadOnly;
			}
		}

		bool IPackingParent.IsScanEventsVisible => false;

		ZString IPackingParent.CarrierServiceLevelCode(PkgPackage package) => WD_PL_NKCarrierServiceLevel;

		OrgHeader IPackingParent.GetCarrier(PkgPackage package) => this.GetTransportCo();

		ZString IPackingParent.TransportReference
		{
			get => WD_TransportReference;
			set => WD_TransportReference = value;
		}

		bool IPackingParent.IsLoosePackageIDsSupported => false;

		void IPackingParent.BeforeUnpackingPackages(IReadOnlyList<PkgPackage> packages)
		{
			var packagePKs = packages.Select(p => p.PK).ToArray();
			Factory.AddFetchHint(WhsPickTrolleySlotSchema.Instance, new ZQuery(WhsPickTrolleySlotSchema.WTS_KP_Package, packagePKs));
			Factory.AddFetchHint(StmUniversalCopySchema.Instance, new ZQuery(StmUniversalCopySchema.SUC_CopyObjectId, packagePKs));
			Factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentID, packagePKs));
			Factory.AddFetchHint(StmDocDataOverrideSchema.Instance, new ZQuery(StmDocDataOverrideSchema.DD_ParentRelatedID, packagePKs));
			Factory.AddFetchHint(JobDocumentDeliverySchema.Instance, new ZQuery(JobDocumentDeliverySchema.JDC_ParentID, packagePKs));
			Factory.AddFetchHint(JobDocumentExclusionSchema.Instance, new ZQuery(JobDocumentExclusionSchema.JDE_ParentID, packagePKs));
		}

		bool IPackingParent.IsUXMLEventParent(IXmlEventValueObject xmlEvent)
		{
			bool isParent = false;
			if (xmlEvent.Context.OrderNumber == WD_DocketID)
			{
				isParent = true;
			}
			else
			{
				if (xmlEvent.Context.ClientReference == WD_ExternalReference
					&& xmlEvent.Context.ClientCode == Client.OH_Code)
				{
					isParent = true;
				}
			}

			return isParent;
		}

		YesNoWithReasonForNo IPackingParentWithPackableItems.IsScanQtyAllowed
		{
			get
			{
				var consignee = Consignee;
				return (consignee != null && !consignee.MiscServ.OM_IsScanPackQtyAllowed)
					? YesNoWithReasonForNo.No(Res.GetString("4fdd06f3-231f-4626-8f36-b4268791475a", "Scan Qty Mode is not enabled for the current Consignee."))
					: YesNoWithReasonForNo.Yes;
			}
		}

		ParentJobType IPackingParent.ParentJobType => ParentJobType.WarehouseOrder;

		PackageSequenceType IPackingParent.PackageSequenceType => (Pick?.WP_IsCartonised ?? false) ? PackageSequenceType.Consolidated : PackageSequenceType.Outer;

		bool IPackingParent.CanReleasePackage(PkgPackage package)
		{
			return ReleaseManager.CanReleasePackage(package, Warehouse);
		}

		ZString IPackingParent.GetCannotReleasePackageMessage(PkgPackage package)
		{
			return ReleaseManager.GetCannotReleasePackageErrorMessage(package);
		}

		void IPackingParent.OnPackageBookedViaRTUS(ZDateTime sentDateTime)
		{
			if (WD_BookedWithCBADateTimeUtc.IsEmpty)
			{
				WD_BookedWithCBADateTimeUtc = sentDateTime;
			}
		}

		#region SSCCPrefix

		ZString IPackingParent.GetSSCCPrefix(INotifications notify, SSCCGenerationContext context) => SSCCPrefixFinder.GetSSCCPrefix(context, () => Client, () => Warehouse, notify, shouldPrompt: true);

		ISSCCPrefixFinder SSCCPrefixFinder
		{
			get => ssccPrefixFinder ?? (ssccPrefixFinder = ObjectFactory.Get<ISSCCPrefixFinder>());
		}

		ISSCCPrefixFinder ssccPrefixFinder;

		#endregion

		NotificationTypes IPackingParent.NotificationTypeForInvalidContainerNumber => NotificationTypes.None;

		#region PackableItems

		public IReleaseLinesOnOrderCollection PackableItemParents
		{
			get
			{
				if (packableItemParents == null)
				{
					packableItemParents = new ReleaseLinesOnOrderCollection();
					packableItemParents.AddRange(GetAllReleaseLines());
				}

				return packableItemParents;
			}
		}

		ReleaseLinesOnOrderCollection packableItemParents;

		#region GetAllReleaseLines

		IEnumerable<WhsReleaseLine> GetAllReleaseLines()
		{
			var pick = Pick;
			if (pick != null && pick.IsAlterPick) // only Validate to check release lines when on the Release form
			{
				foreach (WhsPickableDocketLine orderLine in Lines)
				{
					using (orderLine.GetValidationSuspender()) // any order line specific validation run by the release lines is not necessary here
					{
						foreach (var releaseLine in orderLine.ReleaseLines.Cast<WhsReleaseLine>().Where(IsValidForPacking))
						{
							yield return releaseLine;
						}
					}
				}
			}
			else
			{
				foreach (WhsReleaseLine releaseLine in Lines.Cast<WhsPickableDocketLine>().SelectMany(l => l.ReleaseLines))
				{
					yield return releaseLine;
				}
			}
		}

		/// <summary>
		/// If we have release lines that are in error then the Packable Items Grid should not
		/// show those Release Line as they cannot be Packed. Note that release lines that are not
		/// in error will be validated here to make sure we only get valid Release lines.
		/// </summary>
		static bool IsValidForPacking(WhsReleaseLine releaseLine)
		{
			bool result = !releaseLine.HasErrors;
			if (result || releaseLine.InvalidQuantityThatWasReversed.HasValue)
			{
				releaseLine.Quantity = releaseLine.Quantity; // clears out InvalidQuantityThatWasReversed on ReleaseLine

				bool originalValue = releaseLine.IgnoreValidationSuspended;
				try
				{
					releaseLine.IgnoreValidationSuspended = true;
					releaseLine.Validation.ValidateAll();
				}
				finally
				{
					releaseLine.IgnoreValidationSuspended = originalValue;
				}

				result = !releaseLine.HasErrors;
			}

			return result;
		}

		IEnumerable PackageStrategies
		{
			get => packageStrategies ?? (packageStrategies = ObjectFactory.Get<IEnumerable>("WhsPackageStrategyList"));
		}

		IEnumerable packageStrategies;

		#endregion

		#region ReleaseLinesOnOrderCollection

#if DEBUG
		public
#endif
		class ReleaseLinesOnOrderCollection : NonPersistentBusinessObjectCollection<WhsReleaseLine>, IReleaseLinesOnOrderCollection, ICollection, IEnumerable, IEnumerable<BusinessObject>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				throw new NotImplementedException();
			}

			protected override bool AllowNewCore => false;

			internal IDisposable SuspendCountChangedForPacking()
			{
				return SuspendCountChanged(args => OnCountChanged(new CollectionCountChangedEventArgs(false, null)));
			}

			internal void RegisterOrderLineWithClearedReleaseLines(WhsPickableDocketLine orderLine)
			{
				if (RegisteredOrderLinesToAddReleaseLinesFrom == null)
				{
					RegisteredOrderLinesToAddReleaseLinesFrom = new HashSet<WhsPickableDocketLine>();
				}

				RegisteredOrderLinesToAddReleaseLinesFrom.Add(orderLine);
			}

			HashSet<WhsPickableDocketLine> RegisteredOrderLinesToAddReleaseLinesFrom;

			#region IReleaseLinesOnOrderCollection Members

			IEnumerable<WhsReleaseLine> IReleaseLinesOnOrderCollection.Typed
			{
				get
				{
					foreach (WhsReleaseLine releaseLine in (IEnumerable)this)
					{
						yield return releaseLine;
					}
				}
			}

			event EventHandler IReleaseLinesOnOrderCollection.CountChanged
			{
				add { CountChanged += new CollectionCountChangedEventHandler(value); }
				remove { CountChanged -= new CollectionCountChangedEventHandler(value); }
			}

			#endregion

			#region ICollection

			int ICollection.Count
			{
				get { return Count; }
			}

			public new int Count
			{
				get
				{
					if (RegisteredOrderLinesToAddReleaseLinesFrom?.Count > 0)
					{
						AddNewReleaseLines();
					}

					return base.Count;
				}
			}

			#endregion

			#region IEnumerable

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator<BusinessObject> IEnumerable<BusinessObject>.GetEnumerator()
			{
				return GetEnumerator();
			}

			IEnumerator<BusinessObject> GetEnumerator()
			{
				if (RegisteredOrderLinesToAddReleaseLinesFrom?.Count > 0)
				{
					AddNewReleaseLines();
				}

				return new BusinessObjectCollectionEnumerator(this);
			}

			#endregion

			#region AddNewReleaseLines

			void AddNewReleaseLines()
			{
				try
				{
					// only trigger Count Changed once
					using (SuspendCountChangedForPacking())
					{
						// Clearing Release Lines will remove all Release Lines from the PackableItemParents collection
						// We need to add back Release Lines from these OrderLines when Enumeration is requested.
						foreach (var orderLine in RegisteredOrderLinesToAddReleaseLinesFrom)
						{
							foreach (WhsReleaseLine releaseLine in orderLine.ReleaseLines)
							{
								Add(releaseLine);
							}
						}
					}
				}
				finally
				{
					RegisteredOrderLinesToAddReleaseLinesFrom.Clear();
				}
			}

			#endregion
		}

		#endregion

		#region FireReleaseLineAddedOrRemoved

		// tested in WhsReleaseLineCollection.cs
		internal void FireReleaseLineAddedOrRemoved(WhsReleaseLine releaseLine, bool releaseLineWasAdded)
		{
			if (releaseLine != null && packableItemParents != null)
			{
				if (releaseLineWasAdded)
				{
					packableItemParents.Add(releaseLine);
				}
				else
				{
					packableItemParents.Remove(releaseLine);
				}
			}
		}

		#endregion

		#region SuspendPackableItemParentsCountChanged

		internal IDisposable SuspendPackableItemParentsCountChanged()
		{
			return packableItemParents?.SuspendCountChangedForPacking();
		}

		#endregion

		#region RegisterOrderLineWithClearedReleaseLines

		internal void RegisterOrderLineWithClearedReleaseLines(WhsPickableDocketLine orderLine)
		{
			if (orderLine.WE_WD != PK)
			{
				throw new InvalidOperationException("Only register OrderLines on this Order.");
			}

			packableItemParents?.RegisterOrderLineWithClearedReleaseLines(orderLine);
		}

		#endregion

		#endregion

		#endregion

		#region IProcessHandlingInfoProvider Members

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo => new WhsOrderProcessHandlingInfo(this);

		#endregion

		#region IRelatableActivity Members

		ZBool IRelatableActivity.ShouldIgnoreSuperAndSubActivityRelationships
		{
			get { return false; }
		}

		ZString IRelatableActivity.ActivityType
		{
			get { return RelatableActivityTypeList.Codes.WarehouseOrders; }
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
			get { return string.Join("; ", new[] { WD_ExternalReference, WD_CustomerReference, WD_DocketStatusDescription }.Where(x => !x.IsEmpty)); }
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

		#region IRelatedJob Members

		protected override bool CanHaveChildWorkOrders => true;

		protected override List<IRelatedJob> GetRelatedJobsCore()
		{
			var jobs = base.GetRelatedJobsCore();

			var cartageJob = CartageJob;
			if (cartageJob != null)
			{
				jobs.Add((IRelatedJob)cartageJob);
			}

			jobs.AddRange(GetRelatedParents().Cast<IRelatedJob>());

			//Back Orders
			var relatedBackOrder = RelatedBackOrder;
			if (relatedBackOrder != null)
			{
				jobs.Add(relatedBackOrder);
			}
			//Related Receives
			var relatedReceives = RelatedReceives;
			if (relatedReceives != null && relatedReceives.Length > 0)
			{
				jobs.AddRange(relatedReceives);
			}

			jobs.AddRange(GetRelatedParentOrders());
			jobs.AddRange(TransportBookingLoader.GetRelatedTransportBookingEvents(this).Cast<IRelatedJob>());

			jobs.AddRange(GetRelatedJobsFromPertinentJobs(jobs.OfType<ISupportRelatedJobs>().ToArray()));
			jobs.AddRange(GetRelatedAdjustmentsAndTransfers());

			return jobs;
		}

		IRelatedJob[] GetRelatedJobsFromPertinentJobs(ISupportRelatedJobs[] jobs)
		{
			var relatedJobs = new List<IRelatedJob>();
			foreach (var job in jobs)
			{
				relatedJobs.AddRange(job.RelatedJobs.Cast<IRelatedJob>().Where(r => r.JobNumber != WD_DocketID));
			}
			return relatedJobs.ToArray();
		}

		#region GetRelatedParentOrders

		IEnumerable<IRelatedJob> GetRelatedParentOrders()
		{
			return Factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.PK, WD_WD_ParentDocket));
		}

		#endregion

		#region GetRelatedAdjustments

		internal IEnumerable<IRelatedJob> GetRelatedAdjustmentsAndTransfers()
		{
			var query = new ZQuery(WhsDocketSchema.WD_WD_ParentDocket, PK);
			query.AddToFilter(WhsDocketSchema.WD_DocketType, new[] { DocketType.Codes.Adjustment, DocketType.Codes.Transfer });
			return Factory.Load<WhsDocket>(query);
		}

		#endregion

		protected override ControllerID JobControllerId
		{
			get { return ControllerIDs.WhsOrder; }
		}

		#endregion

		#region RelatedOrgPartyScreeningStatus Collection

		public IRelatedOrgPartyScreeningStatusCollection RelatedOrgPartyScreeningStatusCollection
		{
			get
			{
				return RelatedOrgPartyScreeningStatusHelper.GetRelatedOrgPartyScreeningStatusCollection(Factory, ((IScreeningPartyProvider)this).ScreeningParties, relatedJobPKs: PK);
			}
		}

		#endregion

		#region IWhsOrder Members

		bool IWhsOrder.IncludeInRelatedShipmentCharges
		{
			get
			{
				bool result = false;

				var client = Client;
				if (client != null)
				{
					var companyData = client.CompanyData;
					if (companyData != null)
					{
						result = companyData.OB_WhsIncludeReleaseChargesOnShipment;
					}
				}

				return result;
			}
		}

		int IWhsOrder.TotalOrderLines => Lines.Count;

		ZDate IWhsOrder.CreateDate
		{
			get
			{
				var result = ZDateTimeOffset.Empty;
				var warehouse = Warehouse;
				if (warehouse != null && WD_SystemCreateTimeUtc.IsValid)
				{
					var branch = warehouse.RelatedCompanyBranch;
					var utc = WD_SystemCreateTimeUtc;
					var currentBranchZoneSet = branch.HomePort.TimeZoneSet;
					if (currentBranchZoneSet != null)
					{
						var calculationTimeZone = currentBranchZoneSet.GetCalculationTimeZone();
						result = calculationTimeZone.ToLocalTime(utc.ToDateTime());
					}
				}

				return result.Date;
			}
		}

		#endregion

		#region IWorkflowProvider Members

		protected override ZString GetWorkflowType()
		{
			return WorkflowDescriptors.WhsOrderWorkflowDescriptorCode;
		}

		protected override WhsDocketProcessTasksCollection GetNewProcessTasksCollection()
		{
			return new WhsOrderProcessTasksCollection(this);
		}

		#endregion

		#region IWhsLogEventParent Members

		protected override string GetDocketEventReferenceParameterType
		{
			get { return Constants.EventReferenceParameterTypes.Order; }
		}

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return false; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("1dc3fc86-a9fb-424d-b46e-20fc03c331b0", @"Orders cannot be deleted."); }
		}

		#endregion

		#region ITransportJobLinkProvider Members

		void OnTransportCreatedAndSaved()
		{
			if (TransportJobCreatedAndSaved != null)
			{
				TransportJobCreatedAndSaved(this, EventArgs.Empty);
			}
		}

		public event EventHandler TransportJobCreatedAndSaved;

		TransportJobResult ITransportJobLinkProvider.TransportJobResult
		{
			get
			{
				var result = GetTransportJobResult<ICommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, PK));

				if (result == null)
				{
					OrderListForDelayedJobNumberFetchHints.GetInstanceForFactory(Factory).LoadFetchHints(Factory);
					var bookings = TransportBookingLoader.GetRelatedTransportBookingEvents(this);

					if (bookings.Any())
					{
						var bookingPks = bookings.Select(b => b.PK);

						result =
							GetTransportJobResult<ICommonCartage>(new ZQuery(JobCartageSchema.JJ_ParentID, bookingPks)) ??
							GetTransportJobResult(bookings.ToArray());
					}
					else
					{
						result = TransportJobResult.Empty;
					}
				}

				return result;
			}
		}

		TransportJobResult GetTransportJobResult<T>(ZQuery query)
			where T : class
		{
			query.MaximumRows = 2; // If > 1 result is 'Multiple Jobs', don't load any more than 2 results into memory
			var results = Factory.Load<T>(query);
			return GetTransportJobResult(results);
		}

		TransportJobResult GetTransportJobResult<T>(T[] bizos)
			where T : class
		{
			TransportJobResult result = null;

			if (bizos.Length > 0)
			{
				result = bizos.Length == 1 ? new TransportJobResult((IRelatedJob)bizos[0]) : TransportJobResult.MultipleJobs;
			}

			return result;
		}

		#endregion

		#region Implementation of IWhsOrderWrapperCallback

		bool IWhsOrderWrapperCallback.EventLogExists(ZString code)
		{
			return EventLogExists(code);
		}

		bool IWhsOrderWrapperCallback.DocketSubTypeReadOnly
		{
			get { return DocketSubTypeReadOnly; }
		}

		#endregion

		#region Implementation of IWhsOrderWrapperStrategy

		WhsDocketValidation IWhsOrderWrapperStrategy.GetNewValidation()
		{
			return new WhsOrderValidation(this);
		}

		WhsDocketLookups IWhsOrderWrapperStrategy.GetNewLookups()
		{
			return new WhsOrderLookups(this);
		}

		WhsPickableDocketLine[] IWhsOrderWrapperStrategy.LinesForSelectedOrderLines()
		{
			return ParentLines.ToArray<WhsPickableDocketLine>();
		}

		void IWhsOrderWrapperStrategy.TemplateCopyLines(WhsPickableDocket copy)
		{
			foreach (var line in ParentLines.ToArray())
			{
				var copiedLine = (WhsPickableDocketLine)line.Clone();
				copy.Lines.Add(copiedLine);
			}
		}

		void IWhsOrderWrapperStrategy.OrderOnSaving()
		{
		}

		bool IWhsOrderWrapperStrategy.DoesNotHaveJobEnteredEvent
		{
			get { return base.DoesNotHaveJobEnteredEvent; }
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

		protected override ScreeningParty[] GetScreeningPartiesCore()
		{
			var parties = this.GetScreeningParties();

			var forwarder = Forwarder;
			if (forwarder != null)
			{
				parties.Add(new ScreeningParty(this, Res.GetString("e3abcdf3-2341-4a2f-bc72-e541b010754e", "Forwarder"), forwarder));
			}

			return parties.ToArray();
		}

		protected override bool IsDPSMovementRestrictedCore() => this.IsDPSMovementRestricted();

		#endregion

		#region IsPropertyUpdatableViaXueAdditionalFieldsCore

		protected override bool IsPropertyUpdatableViaXueAdditionalFieldsCore(PropertyInfo propertyInfo, object proposedValue, out string errorMessage)
		{
			errorMessage = null;
			var result = true;

			if (propertyInfo.Name == nameof(IsOrderHeld) && IsOrderHeldInfo.ReadOnly)
			{
				errorMessage = Res.GetString("871277c8-796c-4c18-8850-c13a92bc120", "Is Order Held can only be set for Entered, New or Held Orders.");
				result = false;
			}

			return result;
		}

		#endregion

		#region CalculateAndSetOrderClassification

		public void CalculateAndSetOrderClassification()
		{
			var packages = PackageJob?.Packages;
			var fallback = packages.IsNullOrEmpty();
			var isBulk = false;
			if (packages != null)
			{
				foreach (var pkg in packages)
				{
					if (pkg.RTUSBookedType == string.Empty || pkg.RTUSBookedType == OrderTypes.Codes.Unknown)
					{
						fallback = true;
						break;
					}
					else if (pkg.RTUSBookedType == OrderTypes.Codes.Bulk)
					{
						isBulk = true;
					}
				}
			}

			if (fallback)
			{
				CalculateAndSetOrderClassificationCore();
			}
			else
			{
				WD_OrderClassification = isBulk ? OrderClassification.Codes.Bulk : OrderClassification.Codes.ECommerce;
			}
		}

		void CalculateAndSetOrderClassificationCore()
		{
			var eCommerceScore = 0;
			var bulkScore = 0;
			var unknownScore = 0;

			foreach (var calcOrderClassificationFunc in OrderClassificationCalculatorFunctions)
			{
				var result = calcOrderClassificationFunc(this);
				if (result == OrderClassificationResult.Bulk)
				{
					bulkScore++;
				}
				else if (result == OrderClassificationResult.Ecommerce)
				{
					eCommerceScore++;
				}
				else
				{
					unknownScore++;
				}

				if (bulkScore >= 3 || eCommerceScore >= 3 || unknownScore >= 3)
				{
					break;
				}
			}

			if (eCommerceScore >= 3)
			{
				WD_OrderClassification = OrderClassification.Codes.ECommerce;
			}
			else if (bulkScore >= 3)
			{
				WD_OrderClassification = OrderClassification.Codes.Bulk;
			}
			else
			{
				WD_OrderClassification = OrderClassification.Codes.Unknown;
			}
		}

		readonly Func<WhsOrder, OrderClassificationResult>[] OrderClassificationCalculatorFunctions = new Func<WhsOrder, OrderClassificationResult>[]
		{
			CalculateOrderScore_ByProductCount,
			CalculateOrderScore_ByProductQuantity,
			CalculateOrderScore_ByVolume,
			CalculateOrderScore_ByWeight,
			CalculateOrderScore_ByConsigneeAddress
		};

		static OrderClassificationResult CalculateOrderScore_ByVolume(WhsOrder order)
		{
			return CalculateOrderScore_ByMeasurement(
				order,
				(product) => product.OP_Cubic,
				(whsOrder) => Constants.Volume.Convert(whsOrder.WD_CubicSent, whsOrder.WD_TotalCubicUnit, Constants.Volume.CubicMetres),
				minBulkDimension: 1m,
				maxEcommerceDimension: 0.5m);
		}

		static OrderClassificationResult CalculateOrderScore_ByWeight(WhsOrder order)
		{
			return CalculateOrderScore_ByMeasurement(
				order,
				(product) => product.OP_Weight,
				(whsOrder) => Constants.Weight.Convert(whsOrder.WD_WeightSent, whsOrder.WD_TotalWeightUnit, Constants.Weight.Kilograms),
				minBulkDimension: 20m,
				maxEcommerceDimension: 10m);
		}

		static OrderClassificationResult CalculateOrderScore_ByMeasurement(
			WhsOrder order,
			Func<OrgSupplierPart, ZDecimal> getProductDimension,
			Func<WhsOrder, ZDecimal> getOrderDimensionConvertedToStandardUnit,
			ZDecimal minBulkDimension,
			ZDecimal maxEcommerceDimension)
		{
			var score = OrderClassificationResult.Unknown;
			var products = IEnumerableExtensions.DistinctBy(order.Lines, line => line.WE_OP).Select(dl => dl.SupplierPart);
			if (products.Any(product => getProductDimension(product) == 0))
			{
				score = OrderClassificationResult.Bulk;
			}
			else
			{
				var dimensionInStandardUnit = getOrderDimensionConvertedToStandardUnit(order);
				if (dimensionInStandardUnit > minBulkDimension)
				{
					score = OrderClassificationResult.Bulk;
				}
				else if (dimensionInStandardUnit > 0m && dimensionInStandardUnit < maxEcommerceDimension)
				{
					score = OrderClassificationResult.Ecommerce;
				}
			}

			return score;
		}

		static OrderClassificationResult CalculateOrderScore_ByProductCount(WhsOrder order)
		{
			var score = OrderClassificationResult.Unknown;
			var productPKsCount = IEnumerableExtensions.DistinctBy(order.Lines, line => line.WE_OP).Count();
			if (productPKsCount > 4)
			{
				score = OrderClassificationResult.Bulk;
			}
			else if (productPKsCount < 3)
			{
				score = OrderClassificationResult.Ecommerce;
			}

			return score;
		}

		static OrderClassificationResult CalculateOrderScore_ByProductQuantity(WhsOrder order)
		{
			var score = OrderClassificationResult.Unknown;
			var productQtyDictionary = new Dictionary<ZGuid, ZDecimal>();
			foreach (var line in order.Lines.OrderByDescending(line => line.WE_TransactionQuantity))
			{
				if (productQtyDictionary.TryGetValue(line.WE_OP, out var productQty))
				{
					productQty += line.WE_TransactionQuantity;
					productQtyDictionary[line.WE_OP] = productQty;
				}
				else
				{
					productQty = line.WE_TransactionQuantity;
					productQtyDictionary.Add(line.WE_OP, line.WE_TransactionQuantity);
				}

				if (productQty > 4)
				{
					score = OrderClassificationResult.Bulk;
					break;
				}
			}

			if (score != OrderClassificationResult.Bulk)
			{
				var maxProdQty = productQtyDictionary.Values.Max();
				if (maxProdQty < 3)
				{
					score = OrderClassificationResult.Ecommerce;
				}
			}

			return score;
		}

		static OrderClassificationResult CalculateOrderScore_ByConsigneeAddress(WhsOrder order)
		{
			return order.Pick.IsConsigneeOnOrderUsedOnOtherWarehouseJobs(order) ? OrderClassificationResult.Bulk : OrderClassificationResult.Ecommerce;
		}

		enum OrderClassificationResult
		{
			Unknown,
			Bulk,
			Ecommerce
		}

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

		ModuleIdentifier IConversationProvider.ParentModule => ModuleIDs.WhsOrder;

		ControllerID IConversationProvider.ParentController => ControllerIDs.WhsOrder;

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

		[ResourceStringData("WhsOrder|OutboundLocation", Caption = "Outbound Location", ShortCaption = "Outbound")]
		public ZString OutboundLocation
		{
			get
			{
				var locationQuery = new ZQuery(WhsOutboundLocationViewSchema.WOU_WD_Order, PK);
				var locations = Factory.Load<WhsOutboundLocationView>(locationQuery);
				return string.Join(", ", locations.Select(l => l.WOU_OutboundLocationString).OrderBy(s => s));
			}
		}

		#region PreSaveValidationCache

		protected override IDisposable GetValidationDataSuspender()
		{
			return new DisposableList(new[] {
				base.GetValidationDataSuspender(),
				new DisposableAction(
					() => orderValidationCache = new WhsOrderValidationCache(AllLines),
					() => orderValidationCache = null)
			});
		}

		internal WhsOrderValidationCache PreSaveValidationCache => orderValidationCache;
		WhsOrderValidationCache orderValidationCache;

		#endregion
	}

	#region Class WhsOrderDocumentSupporter

	public class WhsOrderDocumentSupporter : DocumentSupporter, IPackingParentDocumentSupporter
	{
		public WhsOrderDocumentSupporter(WhsOrder order)
			: base(order)
		{
		}

		protected WhsOrder Order
		{
			get { return (WhsOrder)BusinessObject; }
		}

		protected override void InitialiseCore(IDocumentEvents documentEventSource)
		{
			base.InitialiseCore(documentEventSource);

			bookingDeliveryCancelled = false;
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.WhsOrder; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.WhsOrderCustomiseDocuments; }
		}

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[]
			{
				Constants.DataContext.WhsOrder,
				Constants.DataContext.WhsPickableDocket,
				Constants.DataContext.WhsPackageLabels,
				Constants.DataContext.WhsDeliveryLabels,
				Constants.DataContext.GenericFreightJob
			};
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			switch (dataContext)
			{
				case Constants.DataContext.WhsOrder:
				case Constants.DataContext.Service:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.WhsOrder, Order) };
					break;
				case Core.Constants.DataContext.WhsPickableDocket:
					result = new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Core.Constants.DataContext.WhsPickableDocket, Order) };
					break;

				case Constants.DataContext.WhsPackageLabels:
					var packageLabelsDocumentSupport = Order as IOrdersDocumentSupport;
					if (packageLabelsDocumentSupport != null)
					{
						result = packageLabelsDocumentSupport.GetPackageLabelDocumentWrappers();
					}
					else
					{
						result = Array.Empty<DocumentWrapper>();
					}
					break;

				case Constants.DataContext.WhsDeliveryLabels:
					var deliveryLabelsDocumentSupport = Order as IOrdersDocumentSupport;
					if (deliveryLabelsDocumentSupport != null)
					{
						result = deliveryLabelsDocumentSupport.GetDeliveryLabelDocumentWrappers(Order.CallOnWhsOrderToPrint);
					}
					else
					{
						result = Array.Empty<DocumentWrapper>();
					}
					break;

				case Constants.DataContext.GenericFreightJob:
					result = DocumentWrapperFactory.GenerateGenericWrappers(dataContext, Order);
					break;

				default:
					break;
			}
			return result;
		}

		public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contactType, DocumentDirection direction)
		{
			IDocumentDeliveryContact result;
			OrgAddress address = null;

			if (contactType != null)
			{
				if (contactType == ContactType.LocalTransport)
				{
					if (!Order.TransportCoDocAddress.E2_AddressOverride)
					{
						address = Order.TransportCoDocAddress.Address;
					}
					result = new OrgHeaderContact(Order.GetTransportCo(), address);
				}
				else
				{
					if (contactType.BrandingType == ContactBrandingType.Client)
					{
						if (!Order.ConsigneeDocAddress.E2_AddressOverride)
						{
							address = Order.ConsigneeDocAddress.Address;
						}
						result = new OrgHeaderContact(Order.Consignee, address);
					}
					else
					{
						result = new OrgHeaderContact(Order.Client, Order.Consignee, null);
					}
				}
			}
			else
			{
				result = base.GetContactOrganisation(menuName, contactType, direction);
			}

			return result;
		}

		protected override IDocumentEventsHandler[] GetDocumentEventsHandlers()
		{
			var result = new List<IDocumentEventsHandler>(base.GetDocumentEventsHandlers());
			result.Add(new WhsOrderCartageAdviceDocumentEventsHandler() { DocumentSupporter = this });
			return result.ToArray();
		}

		#region GetChildCollection

		public override IDocumentSupportable[] GetChildCollection(IStmMenuItem menuToBeRun, BusinessContext businessContext, IStmMenuItem childCommand)
		{
			IDocumentSupportable[] result;

			switch (businessContext)
			{
				case BusinessContext.Packing:
					var packageJob = Order.PackageJob;
					result = (packageJob != null) ? new[] { packageJob } : Array.Empty<IDocumentSupportable>();
					break;
				case BusinessContext.DtbBooking:
					bookings = TransportBookingLoader.GetBookingsToDeliver(Order, menuToBeRun);
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
					var factory = Order.Factory;
					var reloadedBooking = (IDocumentSupportable)factory.Load<IDtbBooking>(b.PK);
					if (reloadedBooking == null)
					{
						ErrorReporter.ReportOnce("WhsOrderDocumentSupporter_NullBooking", $"Booking {b.HumanReadableName} from factory '{b.Factory.NameForDebugging}' could not be loaded by factory '{factory.NameForDebugging}' belonging to {Order.HumanReadableName}");
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

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var message = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			switch (dataContextValue.DataContext)
			{
				case Constants.DataContext.WhsOrder:
				case Constants.DataContext.Service:
				case Constants.DataContext.WhsPickableDocket:
				case Constants.DataContext.GenericFreightJob:
					message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoOrder", "Cannot find Warehouse Order.");
					break;

				case Constants.DataContext.WhsPackageLabels:
					message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoPackageLabel", "Cannot find Package Label to print.");
					break;

				case Constants.DataContext.WhsDeliveryLabels:
					message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoDeliveryLabel", "Cannot find Delivery Label to print.");
					break;

				default:
					break;
			}

			return message;
		}

		public override BusinessContext[] SupportedChildBusinessContexts
		{
			get { return new[] { BusinessContext.Packing, BusinessContext.DtbBooking }; }
		}

		#endregion

		#region IPackingParentDocumentSupporter

		IEnumerable<Constants.DataContext> IPackingParentDocumentSupporter.GetModuleSpecificPackageSupportedDataContexts()
		{
			var result = Array.Empty<Constants.DataContext>();

			if (GlbStaff.CurrentUser.IsSupportUser)    // Remove this condition to Expose Labels
			{
				result = new[]
				{
					Constants.DataContext.GenericAuditVarianceLabel,
					Constants.DataContext.GenericAuditVarianceLabelAll
				};
			}

			return result;
		}

		ZString IPackingParentDocumentSupporter.GetModuleSpecificNotFoundMessage(Constants.DataContext dataContext)
		{
			string message;

			switch (dataContext)
			{
				case Constants.DataContext.GenericAuditVarianceLabel:
					message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoPackageVariance", "No variance was found for selected package(s)");
					break;
				case Constants.DataContext.GenericAuditVarianceLabelAll:
					message = Res.GetString("GetBODocDataProvidersNotFoundMessage-NoJobVariance", "No variance was found for any packages in this job.");
					break;
				default:
					message = "";
					break;
			}

			return message;
		}

		bool IPackingParentDocumentSupporter.IsPrintablePackageForSpecificModuleDataContext(Constants.DataContext dataContext, PkgPackage package)
		{
			var result = true;

			if (dataContext == Constants.DataContext.GenericAuditVarianceLabel || dataContext == Constants.DataContext.GenericAuditVarianceLabelAll)
			{
				var packageAudit = package.GetMostRecentAudit();
				result = packageAudit != null && packageAudit.PackageAuditFailureLines.Count > 0;
			}

			return result;
		}

		bool IPackingParentDocumentSupporter.IsAllPackLevelsEnabled(Constants.DataContext dataContext)
		{
			return dataContext == Constants.DataContext.GenericAuditVarianceLabelAll;
		}

		#endregion
	}

	#endregion
}
