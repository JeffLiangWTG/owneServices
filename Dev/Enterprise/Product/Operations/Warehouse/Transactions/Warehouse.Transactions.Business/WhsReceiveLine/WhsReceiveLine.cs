using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DependentBusinessObject(typeof(WhsReceive), "Lines")]
	[SystemDefinedValues]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced, WhsValidationHelper.WhsCheckStockOnHandIsBalanced, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_ReceiveLine))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced, WhsValidationHelper.WhsCheckStockOnHandIsBalanced, WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine, typeof(IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineUpdate_Parent), ValueToRunStoredProcWith = ValueVersion.Both)]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced_Insert, WhsValidationHelper.WhsCheckStockOnHandIsBalanced_ForInsert, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ReceiveLine))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent, WhsValidationHelper.WhsCheckStockOnHandIsBalanced, WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine, typeof(IWhsCheckStockOnHandIsBalancedStrategy_InventoryLineInsert_ForParent))]
	public class WhsReceiveLine : WhsDocketLine,
		IWhsReceiveLineWrapperStrategy,
		IWhsReceiveLine,
		ISupportTemporaryProduct,
		ISerialSplittableLine,
		ICanPerformReceiveActions,
		ICusAddInfoTypeSupporter,
		IDocAddresses,
		ILineToPutaway,
		ISerialNumberParent,
		ICalculateProductPackageTotals
	{
		#region Constructors

		public WhsReceiveLine(BusinessObjectFactory factory, DataRow row)
				 : base(factory, row)
		{
		}

		#endregion

		#region Strategies

		protected IWhsReceiveLineWrapperStrategy wrapperStrategy;
		public IWhsReceiveLineWrapperStrategy WrapperStrategy
		{
			get
			{
				if (wrapperStrategy == null)
				{
					var builder = ObjectFactory.Get<IWhsReceiveLineStrategyBuilder>();
					wrapperStrategy = builder.Build(this);
				}

				return wrapperStrategy;
			}
		}

		#endregion

		#region Schema

		public new abstract class Schema : WhsDocketLine.Schema
		{
			public const string SplitQuantity = "SplitQuantity";
			public const string InDocketLineUnits = "InDocketLineUnits";
			public const string PutawayTransfer = "PutawayTransfer";
			public const string PutawayTransferID = "PutawayTransferID";
			public const string PutawayTransferLine = "PutawayTransferLine";
			public const string DestLocation = "DestLocation";
			public const string ConsigneeNameOrPK = "ConsigneeNameOrPK";
			public const string ConsigneeFieldType = "ConsigneeFieldType";
		}

		#endregion

		#region Type Decider

		public override Type DocketType
		{
			get { return typeof(WhsReceive); }
		}

		#endregion

		#region Public Static Methods

		public static WhsReceiveLine New(BusinessObjectFactory factory)
		{
			return factory.
					// Split so find replace will ignore
					New<WhsReceiveLine>();
		}

		#endregion

		#region Related Entities

		#region Docket

		public new WhsReceive Docket
		{
			get { return (WhsReceive)base.Docket; }
		}

		#endregion

		#region PickLines

		protected override WhsPickLineCollection GetPickLinesCollection()
		{
			return new WhsPickLineCollection(Factory); // Currently no PickLines are ever attached to a Receive Line
		}

		#endregion

		#region ReceiveLineValidationStrategy

		public WhsReceiveLineValidationStrategy ReceiveLineValidationStrategy
		{
			get { return receiveLineValidationStrategy ?? (receiveLineValidationStrategy = WhsEnvironment.IsRF ? new WhsReceiveLineValidationRFStrategy(this) : new WhsReceiveLineValidationStrategy(this)); }
		}

		WhsReceiveLineValidationStrategy receiveLineValidationStrategy;

		#endregion

		#region PutawayTransfer

		[ActionFieldFollow(false)]
		public WhsTransfer PutawayTransfer
		{
			get { return PutawayTransferLine?.Docket; }
		}

		[ActionFieldFollow(false)]
		public WhsTransferLine PutawayTransferLine
		{
			get
			{
				WhsTransferLine transferLine = null;
				var inventories = Inventory;
				if (inventories != null && inventories.Count > 0)
				{
					transferLine = inventories[0].PutawayTransferLine;
				}

				return transferLine;
			}
		}

		#endregion

		#region BOMComponentLinksForBinding

		/// <summary>
		/// This collection is Adhoc and is only correct at time of creation. This is only used
		/// once, when displaying the proposed Assembled Inventory when Finalising a Work Order.
		/// </summary>
		public NonPersistentBusinessObjectCollection<ComponentLineForAssembly> BOMComponentLinksForBinding
		{
			get
			{
				if (bomComponentLinksForBinding == null)
				{
					bomComponentLinksForBinding = new ComponentLineForAssemblyCollection();

					var componentLines = new List<ComponentLineForAssembly>();

					foreach (var pickLineGroup in BOMComponentLinks.SelectMany(l => l.ComponentLine.PickLines).GroupBy(pl => pl.InventoryLinePKForAvailableInventory))
					{
						var units = pickLineGroup.Sum(pl => pl.WZ_Units);
						var pickLine = pickLineGroup.First();

						var componentLine = new ComponentLineForAssembly(pickLine.InventoryLineForAvailableInventory, units);
						componentLines.Add(componentLine);
					}

					bomComponentLinksForBinding.AddRange(componentLines);
				}

				return bomComponentLinksForBinding;
			}
		}

		internal class ComponentLineForAssemblyCollection : NonPersistentBusinessObjectCollection<ComponentLineForAssembly>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				throw new NotImplementedException();
			}

			protected override bool AllowNewCore => false;

			protected override bool AllowRemoveCore => false;
		}

		NonPersistentBusinessObjectCollection<ComponentLineForAssembly> bomComponentLinksForBinding;

		#endregion

		#endregion

		#region Business Object Overrides

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			var inventory = (Inventory.Count > 0) ? Inventory[0] : null;

			if (inventory == null)
			{
				inventory = WrapperStrategy.GetNewInventoryView();
				inventory.WI_InDocketLineType = CodeLists.DocketType.Codes.Receive;
				inventory.OriginalInventoryStatus = InventoryStatus.Codes.Pending;
				inventory.WI_WE_InDocketLine = PK;
				inventory.WI_IsOriginalReceiptLine = WE_IsOriginalInventory;
				inventory.WI_WD = WE_WD;
			}

			WE_WE_OriginalDocketLineForRating = PK;
		}

		protected override string DefaultInventoryStatus
		{
			get { return InventoryStatus.Codes.Pending; }
		}

		protected override string DocketLineType => CodeLists.DocketType.Codes.Receive;

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();
			UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsReceive>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, WE_WD);
		}

		#endregion

		#region Delete

		protected override void DeleteByInventoryCore()
		{
			Delete();
		}

		public override void Delete()
		{
			if (IsInDatabase)
			{
				UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsReceive>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, WE_WD);
			}
			// tested in WhsReceiveTest.TestDataRefreshOnDeletedInventory
			BOMComponentLinks.ForEach(l => l.Delete());
			if (!IsDeleted)
			{
				ObjectFactory.Get<IChildrenDeletionHelper>().DeleteChildren(this);
			}
			base.Delete();

			if (!IsDeleted)
			{
				UnHookAdditionalValidation();
				DocAddresses.RemoveAndDeleteAll();
			}
		}

		void UnHookAdditionalValidation()
		{
			if (consigneeDocAddress != null)
			{
				consigneeDocAddress.OrganisationPKInfo.AdditionalValidation -= new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				consigneeDocAddress.E2_OA_AddressInfo.AdditionalValidation -= new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				consigneeDocAddress.E2_CompanyNameInfo.AdditionalValidation -= new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				consigneeDocAddress.E2_Address1Info.AdditionalValidation -= new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				consigneeDocAddress.E2_PostcodeInfo.AdditionalValidation -= new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				consigneeDocAddress.E2_CityInfo.AdditionalValidation -= new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				consigneeDocAddress.E2_StateInfo.AdditionalValidation -= new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				consigneeDocAddress.E2_RN_NKCountryCodeInfo.AdditionalValidation -= new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				consigneeDocAddress.E2_EmailInfo.AdditionalValidation -= new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
			}
		}

		public override bool CanDelete
		{
			get
			{
				return base.CanDelete && (!IsInDatabase || CanDeleteCore(Docket));

				bool CanDeleteCore(WhsReceive receive) => receive == null || (!IsFinalised && !HasPutawayTransfer && !receive.IsCreatedFromWorkOrder);
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var result = base.ReasonForNotAbleToDelete;
				if (HasPutawayTransfer)
				{
					result = ResString.GetMultilingualString("31e172e7-a751-488c-93ca-7987929E7130", "Putaway receive lines with a putaway transfer must not be deleted.");
				}
				else if (IsFinalised)
				{
					result = ResString.GetMultilingualString("621d51e2-9661-4e80-af21-f13a084085f0", "Finalized receive lines must not be deleted.");
				}
				else if (Docket?.IsCreatedFromWorkOrder ?? false)
				{
					result = ResString.GetMultilingualString("186902fe-e298-4363-9022-1d161b4d6a35", "Assembled Inventory cannot be deleted.");
				}
				else
				{
					return base.ReasonForNotAbleToDelete;
				}

				return result;
			}
		}

		protected override void BeforeRunPreSaveValidationCore()
		{
			base.BeforeRunPreSaveValidationCore();
			RemoveRowError(WhsReceive.ContainsUnfinalizedPutawayTransfersErrorMessage); // removing row error to allow re-validation in WhsReceive's PreFinaliseValidation
			if (WE_WL.IsValid && Location != null && IsStockChangedInLocation)
			{
				Location.OnStockOnHandChanged();
			}
			if (IsInDatabase && ((ZDecimal)WE_TransactionQuantityInfo.OriginalValue) > 0 && WE_WLInfo.HasChanges)
			{
				Factory.Load<WhsLocation>((ZGuid)WE_WLInfo.OriginalValue)?.OnStockOnHandChanged();
			}

			SyncQuantityToCustomsData();
		}

		bool IsStockChangedInLocation => WE_TransactionQuantity > 0 && (!IsInDatabase || WE_WLInfo.HasChanges || WE_OPInfo.HasChanges || WE_TransactionQuantityInfo.HasChanges);

		void SyncQuantityToCustomsData()
		{
			if (IsCustomsTransaction && !Docket.IsCreatedFromWorkOrder)
			{
				var customsData = CustomsData;
				if (WE_IsOriginalInventory && customsData.WB_BondedWhsQty != WE_TransactionQuantity)
				{
					customsData.WB_BondedWhsQty = WE_TransactionQuantity;
				}
			}
		}

		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsReceiveLineFetchStrategy(this);
		}

		#endregion

		#region CloneInternal

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (WhsReceiveLine)base.CloneInternal(args);

			var address = this.LoadJobDocAddressQuickly(DocAddressType.ConsigneeAddress);
			if (address != null && !address.IsEmpty)
			{
				clone.ConsigneeDocAddress.SetActualFieldValuesFromParent(address);
			}

			if (IsTemporaryProduct)
			{
				clone.ProductCode = ProductCode;
				clone.ProductDesc = ProductDesc;
				clone.CommodityCode = CommodityCode;
				clone.ProductUQ = ProductUQ;
			}

			if (!clone.WE_IsOriginalInventory)
			{
				clone.WE_WE_OriginalDocketLineForRating = WE_WE_OriginalDocketLineForRating;
			}

			if (HasDocketStartedReceiving)
			{
				var transactionQuantityBeforeSettingClientOrderedUnits = clone.WE_TransactionQuantity;
				clone.WE_ClientOrderedUnits = 0m;
				clone.WE_TransactionQuantity = transactionQuantityBeforeSettingClientOrderedUnits;
			}

			return clone;
		}

		#endregion

		#region IsInventoryLineCore

		protected override bool IsInventoryLineCore
		{
			get { return true; }
		}

		#endregion

		#region Consignee

		#region Consignee

		public OrgHeader Consignee => this.LoadJobDocAddressQuickly(DocAddressType.ConsigneeAddress)?.GetOrganisation();

		#endregion

		#region ConsigneePK

		public ZGuid ConsigneePK
		{
			get
			{
				var consigneeDocAddressWithoutCreate = this.LoadJobDocAddressQuickly(DocAddressType.ConsigneeAddress);
				return consigneeDocAddressWithoutCreate?.OrganisationPK ?? ZGuid.Empty;
			}

			set { ConsigneeDocAddress.OrganisationPK = value; }
		}

		public ZPropertyInfo ConsigneePKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneePK, x => ConsigneeDocAddress.OrganisationPKInfo); }
		}

		#endregion

		#region ConsigneeNameOrPK

		#region ConsigneeNameOrPK

		[List("Lookups.Consignees")]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[ResourceStringData("WhsDocketLine|ConsigneeNameOrPK", Caption = "Consignee", FullDescription = "Press Ctrl-D to override the address.")]
		public ZString ConsigneeNameOrPK
		{
			get { return ConsigneeDocAddress.OrganisationNameOrPK; }
			set
			{
				if (ConsigneeDocAddress.E2_AddressOverride && value == "")
				{
					ConsigneeDocAddress.E2_AddressOverride = false;
					ConsigneeDocAddress.OrganisationPK = ZGuid.Empty;
				}
				else
				{
					ConsigneeDocAddress.OrganisationNameOrPK = value;
				}
				ConsigneeNameOrPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConsigneeNameOrPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ConsigneeNameOrPK, o => ConsigneeDocAddress.OrganisationNameOrPKInfo); }
		}

		#endregion

		#region ConsigneeFieldType

		public ZString ConsigneeFieldType
		{
			get { return ConsigneeDocAddress.E2_AddressOverride ? nameof(FieldType.Text) : nameof(FieldType.Guid); }
		}

		public ZPropertyInfo ConsigneeFieldTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ConsigneeFieldType); }
		}

		#endregion

		#endregion

		#region ConsigneeDocAddress

		public JobDocAddress ConsigneeDocAddress
		{
			get
			{
				if (consigneeDocAddress == null || consigneeDocAddress.IsDeleted)
				{
					consigneeDocAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ConsigneeAddress);
					AddAdditionalValidation(consigneeDocAddress);
					consigneeDocAddress.ReadOnlyStrategy = new JobDocAddressReadOnlyStrategy(() => IsDocketFinalisedOrCancelled);
				}

				return consigneeDocAddress;
			}
		}

		JobDocAddress consigneeDocAddress;

		#region AddAdditionalValidation

		void AddAdditionalValidation(JobDocAddress jobDocAddress)
		{
			if (jobDocAddress != null)
			{
				// All properties that can be set wrong on JobDocAddress.
				jobDocAddress.OrganisationPKInfo.AdditionalValidation += new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				jobDocAddress.E2_OA_AddressInfo.AdditionalValidation += new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				// Overriden properties
				jobDocAddress.E2_CompanyNameInfo.AdditionalValidation += new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				jobDocAddress.E2_Address1Info.AdditionalValidation += new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				jobDocAddress.E2_PostcodeInfo.AdditionalValidation += new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				jobDocAddress.E2_CityInfo.AdditionalValidation += new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				jobDocAddress.E2_StateInfo.AdditionalValidation += new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				jobDocAddress.E2_RN_NKCountryCodeInfo.AdditionalValidation += new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
				jobDocAddress.E2_EmailInfo.AdditionalValidation += new RunValidationInvoker(ConsigneeDocAddress_AdditionalValidation);
			}
		}

		void ConsigneeDocAddress_AdditionalValidation()
		{
			if (!IsAdditionalValidationRunning.IsSuspended)
			{
				ConsigneeDocAddress.Validation.ValidateOrganisationNameOrPK();
				ConsigneeNameOrPKInfo.RefreshBinding();
			}
		}

		public Semaphore IsAdditionalValidationRunning
		{
			get { return isAdditionalValidationRunning ?? (isAdditionalValidationRunning = new Semaphore()); }
		}

		Semaphore isAdditionalValidationRunning;

		#endregion

		#region ShouldValidateConsignee

		public bool ShouldValidateConsignee
		{
			get
			{
				return consigneeDocAddress != null &&
					(ConsigneeNameOrPKInfo.HasErrors() ||
					!consigneeDocAddress.OrganisationPK.IsEmpty ||
					consigneeDocAddress.E2_AddressOverride ||
					!consigneeDocAddress.E2_OA_Address.IsEmpty);
			}
		}

		#endregion

		#endregion

		#endregion

		protected override void SynchroniseOffsetsToWarehouseTimeCore(ITimeZone calculationTimeZone)
		{
			base.SynchroniseOffsetsToWarehouseTimeCore(calculationTimeZone);

			WE_RequiredByDateInfo.ConvertOffsetPropertyToTimeZone(calculationTimeZone);
		}

		#endregion

		#region Properties

		protected override bool ShouldSettingHeldCodeChangeStatus
		{
			// Inventory becomes held when the Receive is finalised, should be set if the receive is finalised (i.e. HeldCode change)
			get { return IsDocketFinalised; }
		}

		protected override bool SettingWE_TransactionQuantityShouldRefreshTotalUnitsFromLines
		{
			// Receives currently base their total units from Inventory and setting inventory units calls RefreshBinding().
			get { return false; }
		}

		#region SupportsHoldCodeChange

		protected override bool SupportsHoldCodeChange
		{
			get { return true; }
		}

		#endregion

		#region Readonly

		#region HeldCodeReadonly

		protected override bool HeldCodeReadonly => IsPickedForUnload
			|| IsInventoryEditForm
			|| IsFinalised
			|| IsReceiveFinalisedCancelledOrCreatedFromWorkOrder(Docket)
			|| WE_OriginalInventoryStatus == InventoryStatus.Codes.PuttingAway
			|| InventoryStatusReceivedReadOnly;

		bool IsReceiveFinalisedCancelledOrCreatedFromWorkOrder(WhsReceive receive) => receive == null
			|| receive.IsFinalisedOrCancelled
			|| receive.IsCreatedFromWorkOrder;

		#endregion

		protected override bool StandardReadOnly => ReceiveEditFormReadonly || (Docket?.IsCreatedFromWorkOrder ?? false);

		protected override bool DestLocationReadOnly => ReceiveEditFormReadonly || Warehouse == null || InventoryStatusReceivedReadOnly;

		protected override bool DestPalletIDReadOnly => ReceiveEditFormReadonly || InventoryStatusReceivedReadOnly;

		protected override bool ProductReadOnly => base.ProductReadOnly || InventoryStatusReceivedReadOnly;

		protected override bool PackQuantityAndTypeAndRelatedQuantityInfoReadOnly => base.PackQuantityAndTypeAndRelatedQuantityInfoReadOnly || InventoryStatusReceivedReadOnly;

		protected override bool TransactionQtyReadOnly => base.TransactionQtyReadOnly || !HasDocketStartedReceiving;

		protected bool ExpectedQtyReadOnly => StandardReadOnly || HasDocketStartedReceiving;

		#region PartAttributes

		protected override bool PartAttrib1InfoReadOnly => GetPartAttributeReadonly(WE_PartAttrib1Info, (c) => c.PartAttributeManager.IsPartAttributeUsedByOrganisation(1), (c, p) => p.IsPartAttributeUsed(c, 1)) || InventoryStatusReceivedReadOnly;

		protected override bool PartAttrib2InfoReadOnly => GetPartAttributeReadonly(WE_PartAttrib2Info, (c) => c.PartAttributeManager.IsPartAttributeUsedByOrganisation(2), (c, p) => p.IsPartAttributeUsed(c, 2)) || InventoryStatusReceivedReadOnly;

		protected override bool PartAttrib3InfoReadOnly => GetPartAttributeReadonly(WE_PartAttrib3Info, (c) => c.PartAttributeManager.IsPartAttributeUsedByOrganisation(3), (c, p) => p.IsPartAttributeUsed(c, 3)) || InventoryStatusReceivedReadOnly;

		protected override bool SerialNumberReadOnly => GetPartAttributeReadonly(WE_SerialNumberInfo, (c) => c.PartAttributeManager.IsSerialNumberUsedByOrganisation, (c, p) => p.IsSerialNumberUsed(c)) || InventoryStatusReceivedReadOnly;

		protected override bool ExpiryDateReadOnly => GetPartAttributeReadonly(WE_ExpiryDateInfo, (c) => c.PartAttributeManager.IsExpiryDateUsedByOrganisation, (c, p) => p.IsExpiryDateUsed(c), checkForJulianBatchNumber: true) || InventoryStatusReceivedReadOnly;

		protected override bool PackingDateReadOnly => GetPartAttributeReadonly(WE_PackingDateInfo, (c) => c.PartAttributeManager.IsPackingDateUsedByOrganisation, (c, p) => p.IsPackingDateUsed(c), checkForJulianBatchNumber: true) || InventoryStatusReceivedReadOnly;

		bool GetPartAttributeReadonly(ZPropertyInfo info, Func<OrgHeader, bool> isAttributeUsedByOrganisation, Func<OrgHeader, WhsProduct, bool> isAttributeUsedByProduct, bool checkForJulianBatchNumber = false)
			=> IsCreatedFromPickByBOM || GetPartAttributeReadonlyCore(info, isAttributeUsedByOrganisation, isAttributeUsedByProduct, checkForJulianBatchNumber);

		bool GetPartAttributeReadonlyCore(ZPropertyInfo info, Func<OrgHeader, bool> isAttributeUsedByOrganisation, Func<OrgHeader, WhsProduct, bool> isAttributeUsedByProduct, bool checkForJulianBatchNumber)
		{
			var result = true;
			var client = Docket?.Client;
			var product = Product;

			var attributeUsedByClient = client != null && isAttributeUsedByOrganisation(client);

			if (!ReceiveEditFormReadonly && attributeUsedByClient)
			{
				result = !IsTemporaryProduct
				&&
				(
					IsAttributeNotUsedByProduct(info, isAttributeUsedByProduct, client, product)
					|| IsJulianBatchNumberNotUsedByProduct(client, product, checkForJulianBatchNumber)
				)
				&& (info.Value.IsEmpty || checkForJulianBatchNumber);
			}

			return result;
		}

		bool IsAttributeNotUsedByProduct(ZPropertyInfo info, Func<OrgHeader, WhsProduct, bool> isAttributeUsedByProduct, OrgHeader client, WhsProduct product)
		{
			Argument.NotNull(client, nameof(client));
			return product == null || (info.Value.IsEmpty && !isAttributeUsedByProduct(client, product));
		}

		bool IsJulianBatchNumberNotUsedByProduct(OrgHeader client, WhsProduct product, bool checkForJulianBatchNumber)
		{
			Argument.NotNull(client, nameof(client));
			return product == null || (checkForJulianBatchNumber && product.IsAJulianBatchNumberAttributeUsed(client));
		}

		#endregion

		#region TempProductReadOnly

		protected override bool TempProductReadOnly
		{
			get { return StandardReadOnly || !IsTemporaryProduct; }
		}

		#endregion

		#region ReceiveEditFormReadonly

		protected bool ReceiveEditFormReadonly => IsInventoryEditForm || IsFinalised || IsDocketFinalisedOrCancelled || HasPutawayTransfer || IsCreatedFromPickByBOM || (Docket?.LockReceiveAfterUnloadCompletedAndPutawayHoldCleared ?? false);

		#endregion

		bool InventoryStatusReceivedReadOnly => IsInDatabase && WE_OriginalInventoryStatus == InventoryStatus.Codes.Received && !WE_OriginalInventoryStatusInfo.HasChanges;

		#endregion

		// Persistant properties

		#region WE_TransactionQuantity

		public override ZDecimal WE_TransactionQuantity
		{
			get { return base.WE_TransactionQuantity; }
			set
			{
				base.WE_TransactionQuantity = value;
				SetUnitsToInventoryView(value);

				// tested in WhsReceiveLineValidationUS
				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_PerPackageQty();
					Validation.ValidateWE_ClientOrderedUnits();
					ValidateSerialNumber();
				}
			}
		}

		void SetUnitsToInventoryView(ZDecimal value)
		{
			var receive = Docket;
			if (receive != null)
			{
				using (new SemaphoreManager(receive.UpdatingWeightAndVolumeSemaphore))
				{
					SetValueToInventoryView(WhsInventoryViewSchema.WI_InDocketLineUnits.Name, value);
				}
			}
			else
			{
				SetValueToInventoryView(WhsInventoryViewSchema.WI_InDocketLineUnits.Name, value);
			}
		}

		void ValidateSerialNumber()
		{
			if (!WE_SerialNumber.IsEmpty)
			{
				var client = Docket?.Client;
				var part = Product;
				if (client != null && part != null && part.IsSerialNumberUsed(client))
				{
					Validation.ValidateWE_SerialNumber();
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for reflection components")]
		bool IsPartAttributeSpecified => !WE_PartAttrib1.IsEmpty || !WE_PartAttrib2.IsEmpty || !WE_PartAttrib3.IsEmpty;

		#endregion

		#region WE_OP

		public override ZGuid WE_OP
		{
			get { return base.WE_OP; }
			set
			{
				if (base.WE_OP != value)
				{
					base.WE_OP = value;
					DefaultHoldCodeFromProduct();
				}

				Docket?.CallOnProductChanged(this, EventArgs.Empty);
			}
		}

		void DefaultHoldCodeFromProduct()
		{
			if (!IsFinalised && !HoldCodeDefaultingSemaphore.IsSuspended)
			{
				var receive = Docket;
				var product = SupplierPart;
				if (receive != null && product != null && !IsTemporaryProductGuid)
				{
					WE_WHC_NKOriginalInventoryHeldCode = product.GetProductDefaultHoldCode(receive.WD_OH_Client);
				}
			}
		}

		Semaphore HoldCodeDefaultingSemaphore => holdCodeDefaultingSemaphore ?? (holdCodeDefaultingSemaphore = new Semaphore());
		Semaphore holdCodeDefaultingSemaphore;

		#endregion

		#region WE_ClientOrderedUnits

		[ReadOnlyMember(nameof(ExpectedQtyReadOnly))]
		[ResourceStringData("WhsReceiveLine|WE_ClientOrderedUnits", Caption = "Expected Quantity", ShortCaption = "Exp Qty", MediumCaption = "Expected Qty")]
		public override ZDecimal WE_ClientOrderedUnits
		{
			get => base.WE_ClientOrderedUnits;
			set
			{
				base.WE_ClientOrderedUnits = value;
				if (CanAssignClientOrderedUnitsToTransactionQty)
				{
					WE_TransactionQuantity = value;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_TransactionQuantity();
					ValidateSerialNumber();
				}
			}
		}

		bool CanAssignClientOrderedUnitsToTransactionQty => !IsFinalised && !UpdatingTransactionQtyFromExpectedQtySemaphore.IsSuspended && !HasDocketStartedReceiving;

		#region UpdatingTransactionQtyFromExpectedQtySemaphore

		internal Semaphore UpdatingTransactionQtyFromExpectedQtySemaphore
		{
			get { return updatingTransactionQtyFromExpectedQtySemaphore ?? (updatingTransactionQtyFromExpectedQtySemaphore = new Semaphore()); }
		}

		Semaphore updatingTransactionQtyFromExpectedQtySemaphore;

		#endregion

		bool HasDocketStartedReceiving => Docket?.StartedReceiving ?? false;

		#endregion

		#region WE_WL

		[List("Lookups.Locations")]
		[ReadOnlyMember(nameof(DestLocationReadOnly))]
		public override ZGuid WE_WL
		{
			get => base.WE_WL;
			set
			{
				base.WE_WL = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_PalletID();
				}
			}
		}

		#endregion

		#region WE_LineNo

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZShort WE_LineNo
		{
			get { return base.WE_LineNo; }
			set { base.WE_LineNo = value; }
		}

		#endregion

		#region WE_SubLineNo

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZShort WE_SubLineNo
		{
			get { return base.WE_SubLineNo; }
			set { base.WE_SubLineNo = value; }
		}

		#endregion

		#region WE_ReceiveCrossDockOrderNo

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZString WE_ReceiveCrossDockOrderNo
		{
			get { return base.WE_ReceiveCrossDockOrderNo; }
			set { base.WE_ReceiveCrossDockOrderNo = value; }
		}

		#endregion

		#region WE_RequiredByDate

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZDateTimeOffset WE_RequiredByDate
		{
			get { return base.WE_RequiredByDate; }
			set { base.WE_RequiredByDate = value; }
		}

		#endregion

		#region WE_PalletID

		public override ZString WE_PalletID
		{
			get => base.WE_PalletID;
			set
			{
				base.WE_PalletID = value;
				Docket?.UpdateTotalPalletsReceived();

				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_WL();
				}
			}
		}

		#endregion

		#region WE_OriginalInventoryStatus

		public override ZString WE_OriginalInventoryStatus
		{
			get => base.WE_OriginalInventoryStatus;
			set
			{
				base.WE_OriginalInventoryStatus = value;
				if (value == InventoryStatus.Codes.Received && WE_UnloadedTime.IsEmpty)
				{
					WE_UnloadedTime = ZDateTimeOffset.Now;
				}
				else if (value != InventoryStatus.Codes.Received && !IsInDatabase)
				{
					WE_UnloadedTime = ZDateTimeOffset.Empty;
				}
			}
		}

		#endregion

		#region WE_UnloadedTime

		public override ZDateTimeOffset WE_UnloadedTime
		{
			get => base.WE_UnloadedTime;
			set
			{
				base.WE_UnloadedTime = value;
				UpdateUnloadedByIfNecessary();
			}
		}

		void UpdateUnloadedByIfNecessary()
		{
			var unloadedBy = WE_UnloadedTime.IsValid ? GlbStaff.CurrentUser.GS_Code : ZString.Empty;
			if (WE_GS_NKUnloadedBy != unloadedBy)
			{
				WE_GS_NKUnloadedBy = unloadedBy;
			}
		}

		#endregion

		#region WE_PackQuantity

		protected override void CalcQuantityFromWE_PackQty()
		{
			var docket = Docket;
			if (docket != null && docket.StartedReceiving)
			{
				base.CalcQuantityFromWE_PackQty();
			}
			else
			{
				UpdateQuantityFromWE_PackQty(WE_ClientOrderedUnitsInfo);
			}
		}

		#endregion

		#region WE_WHC_NKOriginalInventoryHeldCode

		public override ZString WE_WHC_NKOriginalInventoryHeldCode
		{
			get => base.WE_WHC_NKOriginalInventoryHeldCode;
			set
			{
				base.WE_WHC_NKOriginalInventoryHeldCode = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_CurrentHoldReason();
				}
			}
		}

		#endregion

		#region DocketLineCanCalculateExpiryDateFromPackingDate

		protected override bool DocketLineCanCalculateExpiryDateFromPackingDate => !IsTemporaryProduct;

		#endregion

		// Calculated Properties

		#region OriginalHoldReason

		[ActionField(ReadOnly = true, MaxLength = Schema.WE_CurrentHoldReasonMaxLength)]
		[MaxLength(Schema.WE_CurrentHoldReasonMaxLength)]
		[ResourceStringData("WhsReceiveLine|OriginalHoldReason", Caption = "Hold Reason")]
		[ReadOnlyMember(nameof(HeldCodeReadonly))]
		public ZString OriginalHoldReason
		{
			get => IsFinalised && IsInDatabase ? this.GetSystemDefinedValue<ZString>(nameof(OriginalHoldReason)) : WE_CurrentHoldReason;
			set
			{
				if (!IsFinalised && !IsPickedForUnload)
				{
					WE_CurrentHoldReason = value;
				}
				else
				{
					throw new InvalidOperationException("This property is only used to set the Hold Reason before Finalization or Putaway.");
				}
			}
		}

		public ZPropertyInfo OriginalHoldReasonInfo => GetWrappedZPropertyInfo(nameof(OriginalHoldReason), x => WE_CurrentHoldReasonInfo);

		#endregion

		#region SplitQuantity

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		[ResourceStringData("WhsReceiveLine|SplitQuantity", Caption = "Split Quantity")]
		public ZDecimal SplitQuantity
		{
			get { return Inventory.Count == 1 ? Inventory[0].WI_SplitQuantity : ZDecimal.Zero; }
			set
			{
				if (Inventory.Count > 0)
				{
					Inventory[0].WI_SplitQuantity = value;
					((WhsReceiveLineValidation)Validation).ValidateSplitQuantity();
				}

				SplitQuantityInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SplitQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.SplitQuantity); }
		}

		#endregion

		#region Putaway Transfer

		public ZString PutawayTransferID => PutawayTransfer?.WD_DocketID ?? ZString.Empty;

		public ZPropertyInfo PutawayTransferIDInfo => GetZPropertyInfo(Schema.PutawayTransferID);

		#endregion

		#region PutawayLocationAreaName

		[ResourceStringData("WhsDocketLine|PutawayLocationAreaName", ShortCaption = "Put. Area", MediumCaption = "Putaway Area", Caption = "Putaway Location Area")]
		public ZString PutawayLocationAreaName => Location?.PutawayArea?.WA_NameMultilingual ?? ZString.Empty;

		#endregion

		#region PutawayLocationAreaType

		[ResourceStringData("WhsDocketLine|PutawayLocationAreaType", MediumCaption = "Put. Area Type", Caption = "Putaway Area Type")]
		public ZString PutawayLocationAreaType => Location?.WLV_PutawayAreaType ?? ZString.Empty;

		#endregion

		#region DestLocation

		[ResourceStringData("WhsReceiveLine|DestLocation", Caption = "Destination Location", ShortCaption = "Dest. Loc.")]
		[ReadOnly(true)]
		public ZString DestLocation => DestinationLocation?.WLV_LocationString_UserFriendly ?? ZString.Empty;

		public WhsLocation DestinationLocation
		{
			get
			{
				WhsLocation destinationLocation;
				if (HasPutawayTransfer)
				{
					destinationLocation = PutawayTransferLine?.Location;
				}
				else
				{
					var location = Location;
					var locationClass = location?.LocationType?.WLT_LocationClass ?? ZString.Empty;
					destinationLocation = locationClass != LocationClasses.Codes.DDL ? location : null;
				}

				return destinationLocation;
			}
		}

		#endregion

		#region ShouldIgnoreOutOfRangeDates

		internal bool ShouldIgnoreOutOfRangeDates => InventoryStatusReceivedReadOnly || ReceiveEditFormReadonly;

		#endregion

		// Flags

		#region IsDocketPuttingAway

		public bool IsDocketPuttingAway
		{
			get
			{
				var docket = Docket;
				return (docket != null && docket.IsPuttingAway);
			}
		}

		#endregion

		#region IsTemporaryProductCore

		protected override bool IsTemporaryProductCore
		{
			get { return IsTemporaryProductGuid && !ProductCode.IsEmpty; }
		}

		public bool IsTemporaryProductGuid => !WE_OP.IsValid;

		#endregion

		#region CanHavePutawayTransfer

		public bool CanHavePutawayTransfer
		{
			get
			{
				return IsPickedForUnload
					|| (WE_WL.IsValid && WE_OriginalInventoryStatus == InventoryStatus.Codes.Received);
			}
		}

		#endregion

		#region HasPutawayTransfer

		public bool HasPutawayTransfer => IsPickedForUnload || (CanHavePutawayTransfer && PutawayTransfer != null);

		#endregion

		#region IsPickedForUnload

		public bool IsPickedForUnload => WE_DocketLineStatus == DocketLineStatus.Codes.PickedForUnload;

		#endregion

		#region IsPutaway

		internal bool IsCrossDockLocation(WhsLocation location)
		{
			return ReservedPickLines.Any(pl => ((WhsPickableDocketLine)pl.DocketLine)?.PickableDocket?.CrossDockLocation == location);
		}

		public bool IsPutaway => WE_OriginalInventoryStatus == InventoryStatus.Codes.Putaway;

		#endregion

		#region CanReserveClientOrderedUnits

		public bool CanReserveClientOrderedUnits => !IsFinalised && WE_ClientOrderedUnits > WE_TransactionQuantity;

		#endregion

		#region IsCreatedFromPickByBOMCore

		protected override bool IsCreatedFromPickByBOMCore
		{
			get
			{
				var result = false;
				if (WE_WE_OriginalDocketLineForRating == PK)
				{
					result = IsInDatabase
						? Factory.GetCachedValue("WhsReceiveLine|IsCreatedFromPickByBOMCore|" + WE_WD, () => IsReceiveLineCreatedFromPickByBOM)
						: IsReceiveLineCreatedFromPickByBOM;
				}
				return result;
			}
		}

		bool IsReceiveLineCreatedFromPickByBOM => Docket?.IsCreatedFromPickByBOM ?? false;

		#endregion

		#endregion

		#region Validation

		#region ExpiryDateShouldBeInFuturePastYearsBeforeWarning

		protected override bool ShouldDisplayWarningForPastExpiryDates => true;

		#endregion

		#region ShouldSuppressDateRangeValidationErrors

		protected override bool ShouldSuppressDateRangeValidationErrors => base.ShouldSuppressDateRangeValidationErrors || ShouldIgnoreOutOfRangeDates;

		#endregion

		protected override WhsDocketLineValidation GetNewValidation()
		{
			switch (CountryCode)
			{
				case Core.Constants.CountryCodes.UnitedStates:
					return new US.WhsReceiveLineValidationUS(this);

				default:
					return new WhsReceiveLineValidation(this);
			}
		}

		#endregion

		#region Find Attributes

		protected override void UseChosenInventoryRowFindAttributesCore(WhsInventoryView inventory)
		{
			using (new SemaphoreManager(HoldCodeDefaultingSemaphore))
			{
				base.UseChosenInventoryRowFindAttributesCore(inventory);
				WE_PalletID = inventory.WI_PalletID;
			}
		}

		#endregion

		#region WhsReceiveLineLookups

		protected override WhsDocketLineLookups GetNewLookups()
		{
			return new WhsReceiveLineLookups(this);
		}

		#endregion

		#region IWhsReceiveLine

		IWhsInventoryView IWhsReceiveLine.Inventory => Inventory[0];

		#endregion

		#region ISupportTemporaryProduct

		#region IsTemporaryProduct

		bool ISupportTemporaryProduct.IsTemporaryProduct
		{
			get { return IsTemporaryProduct; }
		}

		#endregion

		#region ISupportTemporaryProduct Members

		ISupportTemporaryProduct[] ISupportTemporaryProduct.GetSiblingsWithTheSameProduct()
		{
			var docket = Docket;
			return docket != null
					? Docket.Lines.Cast<WhsReceiveLine>().Where(i => i.ProductCode == ProductCode).Cast<ISupportTemporaryProduct>().ToArray()
					: Array.Empty<ISupportTemporaryProduct>();
		}

		#endregion

		#endregion

		#region ISerialSplittableLine

		ZDecimal ISerialSplittableLine.Units
		{
			get { return WE_TransactionQuantity; }
		}

		void ISerialSplittableLine.SplitWhenSerialNumberExists()
		{
			SplitWhenSerialNumberExists();
		}

		bool ISerialSplittableLine.IsSplittableProduct
		{
			get { return IsSerialisedProduct() && !IsCreatedFromPickByBOM; }
		}

		/// <summary>
		/// Split up all inventory lines that have a serial number
		/// </summary>
		/// <returns>WhsInventoryViewCollection</returns>
		public IEnumerable<WhsReceiveLine> SplitWhenSerialNumberExists()
		{
			var collection = new List<WhsReceiveLine>();
			var inventory = Inventory[0];

			if (WE_TransactionQuantity > 1 && IsSerialisedProduct())
			{
				var receive = Docket;

				// there is no change in units, no need to run validation or update weight & volume
				using (new SemaphoreManager(receive.UpdatingWeightAndVolumeSemaphore))
				using (receive.GetValidationSuspender())
				using (GetValidationSuspender())
				{
					for (var i = 1; i < WE_TransactionQuantity; i++)
					{
						var newReceiveLine = (WhsReceiveLine)this.Clone(new BusinessObjectCloneArgs(new[] { WhsDocketLineSchema.WE_SerialNumber.Name }));
						var newInventory = newReceiveLine.Inventory[0];
						using (newInventory.GetValidationSuspender())
						{
							UpdateInventoryQuantities(newInventory, newReceiveLine, 1m);
						}
						newInventory.WI_WD = receive.PK;
						collection.Add(newReceiveLine);
					}

					UpdateInventoryQuantities(inventory, (WhsReceiveLine)inventory.InDocketLine, 1m);
				}
			}
			return collection;
		}

		#region UpdateInventoryQuantities

		static void UpdateInventoryQuantities(WhsInventoryView newInventory, WhsReceiveLine newReceiveLine, ZDecimal newQuantity)
		{
			// do not want to call setter
			SetValueOnRow(newInventory, WhsInventoryViewSchema.WI_InDocketLineUnits, (decimal)newQuantity);
			SetValueOnRow(newReceiveLine, WhsDocketLineSchema.WE_TransactionQuantity, (decimal)newQuantity);
			newReceiveLine.CalcWE_PackQtyFromWE_TransactionQuantity();

			newInventory.WI_TotalUnits = newQuantity;
			newInventory.WI_ExpectedReceiptQuantity = newQuantity;
		}

		static void SetValueOnRow(IBusinessObjectInternals entity, SchemaColumn column, IConvertible value)
		{
			entity.Row[column.Name] = value;
		}

		#endregion

		bool IsSerialisedProduct()
		{
			var client = Docket?.Client;
			var product = Product;
			var result = false;
			if (client != null && product != null)
			{
				result = product.IsSerialNumberUsed(client);
			}
			return result;
		}

		#endregion

		#region CancelLineCore

		protected override void CancelLineCore()
		{
			foreach (WhsInventoryView whsInventory in Inventory)
			{
				whsInventory.ReservedPickLines.DeleteAll();
				whsInventory.WI_TotalUnits = 0m;
			}
		}

		#endregion

		#region ReactivateLineCore

		protected override void ReactivateLineCore()
		{
			if (!PutawayTransferLine?.IsPicked ?? true)
			{
				WE_StockOnHand = WE_TransactionQuantity;
			}
		}

		#endregion

		#region IOrgSupplierPartDefaults

		public void SetupSupplierPart(OrgSupplierPart newPart)
		{
			Inventory[0]?.SetupSupplierPart(newPart);
		}

		#endregion

		#region ICusAddInfoTypeSupporter

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return (IBusinessObjectFetchStrategy)Activator.CreateInstance(ObjectFactory.GetType<ICusAddInfoTypeSupporterFetchStrategy>(), this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.WarehouseCustomsAddInfo, ObjectFactory.GetType<IWarehouseCustomsAddInfo>());
			return result;
		}

		public static class CusAddInfoTypeAttribute
		{
			public static class Codes
			{
				public const string WarehouseCustomsAddInfo = "WCA";
			}
		}

		#endregion

		#region ILineToPutaway Members

		ZGuid ILineToPutawayInfo.DocketPK => WE_WD;

		WhsLocation ILineToPutaway.Location => Factory.Load<WhsLocation>(((ILineToPutaway)this).LocationPK);

		ZGuid ILineToPutawayInfo.LocationPK
		{
			get
			{
				return PutawayTransferLine?.WE_WL ?? WE_WL;
			}
			set
			{
				var putawaytransferLine = PutawayTransferLine;
				if (putawaytransferLine != null)
				{
					putawaytransferLine.WE_WL = value;
				}
				else
				{
					WE_WL = value;
				}
			}
		}

		ZGuid ILineToPutawayInfo.ProductPK => WE_OP;

		ZGuid ILineToPutawayInfo.WarehousePK => Docket.WD_WW_Whs;

		OrgSupplierPart ILineToPutaway.Product => SupplierPart;

		ZString ILineToPutawayInfo.InventoryStatus => WE_CurrentInventoryStatus;

		ZString ILineToPutawayInfo.InventoryHeldCode => WE_WHC_NKOriginalInventoryHeldCode;

		ZString ILineToPutawayInfo.PalletID
		{
			get => PutawayTransferLine?.WE_PalletID ?? WE_PalletID;
			set
			{
				var putawaytransferLine = PutawayTransferLine;
				if (putawaytransferLine != null)
				{
					putawaytransferLine.WE_PalletID = value;
				}
				else
				{
					WE_PalletID = value;
				}
			}
		}

		ZDecimal ILineToPutawayInfo.QuantityToPutaway => WE_TransactionQuantity;
		ZDecimal ILineToPutawayInfo.PackQuantity => WE_PackQuantity;

		ZString ILineToPutawayInfo.PackType
		{
			get { return WE_F3_NKPackType; }
			set { WE_F3_NKPackType = value; }
		}

		bool ILineToPutawayInfo.CheckPalletIDExists => true;

		bool ILineToPutawayInfo.IsValidToPutaway => (ReservedPickLines.Count == 0 || !((ILineToPutaway)this).LocationPK.IsEmpty);

		ILineToPutaway ILineToPutaway.Split(ZDecimal quantityForNewLine)
		{
			return Split(quantityForNewLine);
		}

		internal WhsReceiveLine Split(ZDecimal quantityForNewLine)
		{
			if (quantityForNewLine >= WE_TransactionQuantity)
			{
				throw new ArgumentException("Should not be splitting Receive Line by an amount greater than or equal to its Quantity.");
			}

			var newLine = (WhsReceiveLine)Clone();
			Docket.Lines.Add(newLine);
			newLine.WE_ClientOrderedUnits = newLine.WE_StockOnHand = newLine.WE_TransactionQuantity = quantityForNewLine;

			ZDecimal saveExpectedReceiptQuantity = WE_ClientOrderedUnits;

			// Splitting should have validation suspended because invalid errors may occur such as:
			// When Splitting Reserved Receive Line, we re-allocate the Reserved Pick Lines after setting
			// the Receive Line Quantity so the Validation will be run too early. Thus we need to suspend.
			using (GetValidationSuspender())
			{
				WE_TransactionQuantity -= quantityForNewLine;
				WE_StockOnHand = WE_TransactionQuantity;
				WE_ClientOrderedUnits = saveExpectedReceiptQuantity - quantityForNewLine;
				if (WE_ClientOrderedUnits < 0)
				{
					newLine.WE_ClientOrderedUnits += WE_ClientOrderedUnits;
					WE_ClientOrderedUnits = 0;
				}
			}

			return newLine;
		}

		#endregion

		#region IDocAddresses Members

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new ConsigneeJobDocAddressValidation(this, addressToValidate);
		}

		// Normally subclasses of ZValidation are auto-generated.
		// However we have manually created this class to avoid rerunning validation methods as this class is only used as piggyback validation.
		class ConsigneeJobDocAddressValidation : ZValidation
		{
			public ConsigneeJobDocAddressValidation(WhsReceiveLine line, JobDocAddress addressToValidate)
				: base(addressToValidate)
			{
				Line = line;
			}

			public override Type AutoValidationType
			{
				get { return typeof(ConsigneeJobDocAddressValidation); }
			}

			protected void CheckOrganisationNameOrPK()
			{
				((WhsReceiveLineValidation)Line.Validation).ValidateConsigneeNameOrPK();
			}

			public override void ValidateAll()
			{
				throw new InvalidOperationException("Should not be calling ValidateAll() on PiggyBacked Validation.");
			}

			protected JobDocAddress Parent => (JobDocAddress)ParentFilter;

			readonly WhsReceiveLine Line;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new[] { DocAddressType.ConsigneeAddress };
			}
		}

		#region GetDocAddressRequirement

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
			=> addressType == DocAddressType.ConsigneeAddress ? ConsigneeDocAddressRequirement : null;

		#region ConsigneeDocAddressRequirement

		internal JobDocAddressRequirement ConsigneeDocAddressRequirement
		{
			get { return consigneeDocAddressRequirement ?? (consigneeDocAddressRequirement = new JobDocAddressRequirement(DocAddressType.ConsigneeAddress, ContactType.Consignee)); }
		}

		JobDocAddressRequirement consigneeDocAddressRequirement;

		#endregion

		#endregion

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Enterprise.Environment.Env.Security.None;

		#region DocAddresses

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}

		JobDocAddressDependentCollection docAddresses;

		#endregion

		#region CanDeleteAddress

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		#endregion

		#region GetOrgHeaderList

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#endregion

		#region ICalculateProductPackageTotals

		ZString ICalculateProductPackageTotals.PackageGroupID => WE_PackageGroupId;

		ZDecimal ICalculateProductPackageTotals.PerPackageQty => WE_PerPackageQty;

		ZGuid ICalculateProductPackageTotals.ProductPK => WE_OP;

		ZGuid ICalculateProductPackageTotals.LocationPK => WE_WL;

		ZGuid ICalculateProductPackageTotals.ClientPK => Docket?.WD_OH_Client ?? ZGuid.Empty;

		ZDecimal ICalculateProductPackageTotals.Units => WE_TransactionQuantity;

		#endregion

		#region SerialNumbers

		[ChildEditable]
		public WhsSerialNumberPivotCollection SerialNumbers
		{
			get
			{
				if (serialNumbers == null)
				{
					serialNumbers = new WhsSerialNumberPivotCollection(this);
					RegisterEditableChildObject(serialNumbers);
				}
				return serialNumbers;
			}
		}

		WhsSerialNumberPivotCollection serialNumbers;

		#region ISerialNumberParent

		ZGuid ISerialNumberParent.ProductPK => WE_OP;

		bool ISerialNumberParent.SerialNumberReadOnly => SerialNumberReadOnly;

		public bool IsSerialNumberUsed => Product?.IsSerialNumberUsed(Docket.Client) ?? false;

		bool ISerialNumberParent.IsAllowedToCreateOriginalSerialNumberRecord => true;

		bool ISerialNumberParent.IsSerialNumberAlreadyInUse(WhsSerialNumberPivot pivot) => Docket.IsSerialNumberAlreadyInUse(pivot);

		#endregion

		#endregion

		protected override WhsInventoryViewCollection GetNewWhsInventoryViewCollection()
		{
			return WrapperStrategy.GetNewWhsInventoryCollection();
		}

		protected override WhsInventoryView LoadInventoryView(ZQuery query)
		{
			return WrapperStrategy.LoadInventoryView(query);
		}

		WhsInventoryViewCollection IWhsReceiveLineWrapperStrategy.GetNewWhsInventoryCollection()
		{
			return base.GetNewWhsInventoryViewCollection();
		}

		WhsInventoryView IWhsReceiveLineWrapperStrategy.LoadInventoryView(ZQuery query)
		{
			return base.LoadInventoryView(query);
		}

		WhsInventoryView IWhsReceiveLineWrapperStrategy.GetNewInventoryView()
		{
			return Factory.NewWithPrimaryKey<WhsInventoryView>(PK.ToGuid());
		}
	}
}
