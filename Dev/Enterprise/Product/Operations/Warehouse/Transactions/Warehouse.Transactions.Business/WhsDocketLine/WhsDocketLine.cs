using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business.Common;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[UniversalCopyIgnoreElement("ParentDocketLine", "WhsDocket")]
	[CodeProperty(WhsDocketLine.Schema.Code), DescriptionProperty(WhsDocketLine.Schema.Description)]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_PreventUnPickedPickLinesOnFinalisedDocketLines, WhsValidationHelper.WhsCheckFinalisedDocketLineWithUnPickedPicklines, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckFinalisedDocketLineWithUnPickedPicklines_DeferTriggerStrategy))]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_PreventOverReduceOfStockViaInventoryLine, WhsValidationHelper.WhsCheckTotalUnits, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckTotalUnits_DeferTriggerStrategy), ExtraParamsForStoredProc = "0")]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStock, WhsValidationHelper.WhsCheckTotalUnits, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckTotalUnitsStrategy_BOMKitReceiveLineUpdate), ExtraParamsForStoredProc = "1")]
	[DeferTriggerAndRunBeforeCommit(WhsValidationHelper.TG_WhsDocketLine_EnsureSerialNumberQuantityMatchWithLine, WhsValidationHelper.WhsCheckSerialNumberMatchWithDocketLineQuantity, WhsDocketLineSchema.Constants.PK, typeof(IWhsCheckSerialNumberMatchWithDocketLineQuantity_DeferTriggerStrategy))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Trigger Identifier.")]
	public abstract class WhsDocketLine :
		AutoWhsDocketLine,
		IWhsDocketLine,
		ILineAttributes,
		ILineCustomAttributes,
		ICustomLabelsConfigOrgProvider,
		IPartAttributeValidationConsumer,
		IProcessHandlingInfoProvider,
		ISupportDataImporting,
		IEDocsProvider,
		ILocationCapacityLine,
		ICodeDescription
	{
		public const string PreventAdjustmentInLinesWithPickLinesTriggerID = "Adjustment In lines should not pick any stock.";
		public const string PreventDesynchronisingInterWhsTransferLines = "Attempt to desynchronise InterWhs Child!";
		public const string PreventIncorrectStatusDocketLineTriggerID = "Attempt to set incorrect WE_DocketLineStatus";
		public const string PreventIncorrectFinalisedDateDocketLineTriggerID = "Attempt to set incorrect WE_FinalisedDate";
		public const string PreventOverflowLocationQuantityTriggerID = "Attempt to overflow the location unit capacity.";
		public const string PreventLocationForIncorrectWarehouseTriggerException = "Locations should be in the warehouse specified on the parent Docket.";
		public const string PreventUnPickedPickLinesOnFinalisedDocketLine = "Attempt to save a Finalised Docket Line with an unpicked pickline.";
		public const string PreventAttemptToMakeAllocatedInventoryUnallocateable = "Attempt to make allocated inventory unallocateable.";
		public const string PreventFinalisationOfReceiveWithShortageOfStock = "Cannot Finalise while Cross Dock Orders Unfufilled.";
		public const string PreventMismatchedSerialNumbersWithQuantity = "Attempt to finalise docket line where the quantity does not match the number of serial numbers.";

		#region Constructors

		protected WhsDocketLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConfirmDocketTypeIsCorrect(factory, row);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WE_WHC_NKOriginalInventoryHeldCode), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WE_WHC_NKCurrentInventoryHeldCode), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WE_ExtendedLinePrice), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(WE_StockOnHand), ConcurrencyPolicy.Strict);
		}

		#endregion

		#region Schema

		public new abstract class Schema : AutoWhsDocketLine.Schema
		{
			public const string WE_ClientUQ = "WE_ClientUQ";
			public const string WE_PackQuantity = "WE_PackQuantity";
			public const string IsBOMProduct = "IsBOMProduct";
			public const string CustomsData = "CustomsData";
			public const string LocationString = "LocationString";
			public const string ConsigneePK = "ConsigneePK";
			public const string CommodityCode = "CommodityCode";
			public const string HeldCodeChangeQuantity = "HeldCodeChangeQuantity";
			public const string HeldCodeToChangeTo = "HeldCodeToChangeTo";
			public const string HoldReasonToChangeTo = "HoldReasonToChangeTo";
			public const string OriginalHeldCode = "OriginalHeldCode";
			public const string ProductCode = "ProductCode";
			public const string ProductDesc = "ProductDesc";
			public const string ProductUQ = "ProductUQ";
			public const string AvailableToPickQuantity = "AvailableToPickQuantity";
			public const string AvailableToTransferQuantity = nameof(AvailableToTransferQuantity);
			public const string ReceiptReference = "ReceiptReference";
			public const string HasEDocsOrNotesAttached = "HasEDocsOrNotesAttached";
			public const string StorageMain = "StorageMain";
			public const string ReservedQuantity = "ReservedQuantity";
			public const string Code = "Code";
			public const string Description = "Description";
			public const string CustomsTariffLookup = nameof(CustomsTariffLookup);
			public const string CustomsTariffItem = nameof(CustomsTariffItem);
			public const string CustomsTariffDesc = nameof(CustomsTariffDesc);

			public const int LocationStringMaxLength = 36;
		}

		public const int PartAttributeCount = 3;

		#endregion

		#region TypeDecider

		public static readonly WhsDocketLineTypeDecider TypeDecider = new WhsDocketLineTypeDecider();
		public abstract Type DocketType { get; }

		#endregion

		#region ConfirmDocketType

		void ConfirmDocketTypeIsCorrect(BusinessObjectFactory factory, DataRow row)
		{
			var docketLineType = TypeDecider.GetTypeForLoad(row, factory);
			if (docketLineType != null && !docketLineType.IsAssignableFrom(this.GetType()) && !docketLineType.IsSubclassOf(this.GetType()))
			{
				throw new NotSupportedException(string.Format(Culture.Invariant, "docketLineType did not match the type of the class. The class type was {0}, but the docketLineType was {1}", GetType().ToString(), docketLineType.ToString()));
			}
		}

		#endregion

		#region Customs Stuff

		#region CustomsData

		public WhsBondedWarehouseAttribute CustomsData
		{
			get
			{
				if (!IsDeleted && (customsData == null || customsData.IsDeleted))
				{
					customsData = WhsBondedWarehouseAttribute.LoadFromParentID(Factory, this);
					if (customsData == null)
					{
						customsData = Factory.New<WhsBondedWarehouseAttribute>();
						SetDefaultValueWhsBondedWarehouseAttribute(customsData);
						using (customsData.SuspendSettingHasChanges())
						{
							customsData.SetParent(this);
						}
						customsData.SetReadOnlyIncludingChildren(IsCustomsDataReadOnly);
					}
					RegisterEditableChildObject(customsData);
				}

				return customsData;
			}
		}

		IWhsBondedWarehouseAttribute IWhsDocketLine.CustomsData => CustomsData;

		protected virtual void SetDefaultValueWhsBondedWarehouseAttribute(WhsBondedWarehouseAttribute bondedWarehouseAttribute) { }

		protected virtual bool IsCustomsDataReadOnly => IsFinalised || IsDocketCancelled;

		WhsBondedWarehouseAttribute customsData;

		#endregion

		public bool IsCustomsTransaction => !IsDeleted && (Docket?.IsCustomsTransaction ?? false);

		public bool IsWarehouseBondEnabled => !IsDeleted && (Docket?.IsWarehouseBondEnabled ?? false);

		public bool IsWarehouseExciseEnabled => !IsDeleted && (Docket?.IsWarehouseExciseEnabled ?? false);

		protected virtual void CheckCustomsDataMatch(WhsBondedWarehouseAttribute bond)
		{
			if (bond == null && !WE_BondedEntryKey.IsEmpty)
			{
				ErrorReporter.ReportOnce("DocketLineCouldNotLoadCustomsData", $"This WhsDocketLine ({PK}) has a value in WE_BondedEntryKey yet the WhsBondedWarehouseAttribute row could NOT be loaded.");
			}
			else if (bond != null && WE_BondedEntryKey != WhsBondedWarehouseAttribute.BuildKey(bond.WB_EntryKey, bond.WB_EntryLineNo))
			{
				ErrorReporter.ReportOnce("DocketLineHadBadCustomsDataMatch", $"This WhsDocketLine ({PK}) WE_BondedEntryKey value does not match that in the related WhsBondedWarehouseAttribute row.");
			}
		}

		Enterprise.Integration.Customs.IBaseCusClassPartPivot[] CustomsClassificationPivots
		{
			get
			{
				var query = new ZQuery(CusClassPartPivotSchema.CI_OP, WE_OP);
				return Factory.Load<Enterprise.Integration.Customs.IBaseCusClassPartPivot>(query);
			}
		}

		Enterprise.Integration.Customs.IBaseCusClassification CustomsClassification
		{
			get
			{
				Enterprise.Integration.Customs.IBaseCusClassification result = null;
				var typeCode = IsWarehouseExciseEnabled ? "EXP" : "IMP";

				foreach (var pivot in CustomsClassificationPivots)
				{
					var classification = Factory.Load<Enterprise.Integration.Customs.IBaseCusClassification>(pivot.CI_CC);
					if (classification != null && classification.CC_ClassificationType == typeCode)
					{
						result = classification;
						break;
					}
				}

				return result;
			}
		}

		public ZString CustomsTariffLookup => CustomsClassification?.CC_LookupCode ?? ZString.Empty;

		public ZString CustomsTariffItem => CustomsClassification?.CC_TariffNum ?? ZString.Empty;

		public ZString CustomsTariffDesc => CustomsClassification?.CC_Description ?? ZString.Empty;

		public ZPropertyInfo CustomsTariffLookupInfo => GetZPropertyInfo(nameof(CustomsTariffLookup));

		public ZPropertyInfo CustomsTariffItemInfo => GetZPropertyInfo(nameof(CustomsTariffItem));

		public ZPropertyInfo CustomsTariffDescInfo => GetZPropertyInfo(nameof(CustomsTariffDesc));

		#endregion

		#region Business Object Overrides

		#region Data Refresh

		protected override void OnBeforeUpdatedByDataRefresh()
		{
			base.OnBeforeUpdatedByDataRefresh();
			if (IsInDatabase && IsInventoryLine && WE_StockOnHandInfo.HasChanges)
			{
				OriginalTotalUnitsBeforeDataRefresh = (ZDecimal)WE_StockOnHandInfo.OriginalValue;
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			ReloadInventory();
			AddRowErrorIfOriginalTotalUnitsChangedByDataRefreshBus();
		}

		/// <summary>
		/// If WE_StockOnHand is modified but the original value is being overriden by the data refresh bus with a different value
		/// then we need stop the user saving by adding a row error to the docketline that picked the Stock.
		/// We will also add a save exception if save is not done through a form.
		/// Tests in WhsInventoryViewTest.PreventInventoryTotalUnitsFromBeingReducedIncorrectlyTest
		/// </summary>
		void AddRowErrorIfOriginalTotalUnitsChangedByDataRefreshBus()
		{
			if (OriginalTotalUnitsBeforeDataRefresh.HasValue && OriginalTotalUnitsBeforeDataRefresh != (ZDecimal)WE_StockOnHandInfo.OriginalValue)
			{
				var query = new ZQuery(WhsPickLineSchema.WZ_PickedDateTime, SQLComparisonOperator.NotEqual, null);
				query.FetchOnlyFromLocalCache = true;
				var whsPickLines = Factory.Load<WhsPickLine>(query);

				foreach (var pickLine in whsPickLines.Where(l => l.WZ_PickedDateTimeInfo.HasChanges))
				{
					var line = pickLine.DocketLine;
					line.AddRowError(OverriddenInventoryUnitErrorMsg);
				}
			}
		}

		public static string OverriddenInventoryUnitErrorMsg
		{
			get { return Res.GetString("AA75E707-20EC-439D-849C-BC437BB1840E", "Units for Inventory that was committed have been changed by another user. Reload the current job to redo changes."); }
		}

		ZDecimal? OriginalTotalUnitsBeforeDataRefresh;

		#endregion

		#region RunPreSaveValidation

		protected sealed override void RunPreSaveValidationCore()
		{
			// if a property validated inside committer while doing changes it will not be run during base.RunPreSaveValidationCore() and
			// we want to run validation when everything is set rather than half way through change.
			using (GetValidationSuspender())
			{
				ClearPackageGroupIDAndPerPackageQuantityIfDocketIsNotABondedUSTransaction();
				CheckTemporaryProductsAreCreated();
				BeforeRunPreSaveValidationCore();
			}

			base.RunPreSaveValidationCore();
		}

		void ClearPackageGroupIDAndPerPackageQuantityIfDocketIsNotABondedUSTransaction()
		{
			var docket = Docket;
			if (docket == null || !docket.IsCustomsTransaction || docket.CountryCode != Constants.CountryCodes.UnitedStates)
			{
				WE_PackageGroupId = "";
				WE_PerPackageQty = 0m;
			}
		}

		void CheckTemporaryProductsAreCreated()
		{
			var errorMsg = Res.GetString("3240463d-5978-46c0-9c8f-87d890dea7b5", "Please Create Product Files for the line.");

			if (!WE_OP.IsValid) // cannot use Is Temporary product
			{
				AddRowError(errorMsg);
			}
			else
			{
				RemoveRowError(errorMsg);
			}
		}

		protected virtual void BeforeRunPreSaveValidationCore()
		{
			if (IsCustomsTransaction)
			{
				// poke CustomsData to create it and register as child object
				_ = CustomsData;
			}
		}

		#endregion

		#region SetDefaultValues

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			var row = ((IBusinessObjectInternals)this).Row;
			row[WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus] = DefaultInventoryStatus;
			row[WhsDocketLineSchema.Constants.WE_CurrentInventoryStatus] = DefaultInventoryStatus;

			WE_DocketLineType = DocketLineType;
		}

		protected abstract string DocketLineType { get; }

		protected abstract string DefaultInventoryStatus { get; }

		#endregion

		#region Delete

		#region Delete

		public override void Delete()
		{
			if (!IsDeleted) // because of tracking
			{
				PickLines.DeleteAll();

				//check CustomsData exist to avoid lazy creation
				var checkCustomsData = WhsBondedWarehouseAttribute.LoadFromParentID(Factory, this);
				if (checkCustomsData != null)
				{
					checkCustomsData.Delete();
				}

				var previousDocket = IsDeleted ? null : Docket;
				using (new SemaphoreManager(DocketLineDeletionSemaphore))
				{
					DeleteInventory(previousDocket);
				}

				if (previousDocket != null)
				{
					previousDocket.UpdateChangesCacheOnRemove(this);
				}
			}
			ClearLocationRequiredCapacityCache();
			base.Delete();
		}

		public Semaphore DocketLineDeletionSemaphore
		{
			get { return docketLineDeletionSemaphore ?? (docketLineDeletionSemaphore = new Semaphore()); }
		}
		Semaphore docketLineDeletionSemaphore;

		protected override void DeleteForDataRefresh()
		{
			Inventory.RemoveAndDeleteAll();
			base.DeleteForDataRefresh();
		}

		void DeleteInventory(WhsDocket previousDocket)
		{
			if (previousDocket != null)
			{
				using (new SemaphoreManager(previousDocket.UpdatingWeightAndVolumeSemaphore))
				{
					Inventory.RemoveAndDeleteAll();
				}
			}
			else
			{
				Inventory.RemoveAndDeleteAll();
			}

			DeleteInventoryHoldChangeLogs();
		}

		void DeleteInventoryHoldChangeLogs()
		{
			if (IsInventoryLine)
			{
				// FetchOnlyFromLocalCache = true as there should be no scenario where we delete an inventory in DB with hold change logs
				var query = new ZQuery(WhsInventoryHoldChangeLogSchema.WHL_WE_ParentDocketLine, PK) { FetchOnlyFromLocalCache = true };
				var holdChangeLogs = Factory.Load<WhsInventoryHoldChangeLog>(query);
				foreach (var holdChangeLog in holdChangeLogs)
				{
					((IBusinessObjectInternals)holdChangeLog).Row.Delete();
				}
			}
		}

		#endregion

		#region OnDeleteByInventory

		public void OnDeleteByInventory()
		{
			if (!IsDeleted && !IsDocketFinalisedOrCancelled)
			{
				DeleteByInventoryCore();
			}
		}

		/// <summary>
		/// Does nothing which is default behaviour. Override to implement special behaviour.
		/// </summary>
		protected virtual void DeleteByInventoryCore()
		{
		}

		#endregion

		#endregion

		#region HasChanges

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				base.HasChanges = value;

				if (value && HasChanges && IsInventoryEditForm && Inventory.Count > 0)
				{
					Inventory[0].HasChanges = true;
				}
			}
		}

		#endregion

		#region OnFactorySaving

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			ChangeInventoryHeldCode(IsInventoryEditForm);

			if (OriginalTotalUnitsBeforeDataRefresh.HasValue && OriginalTotalUnitsBeforeDataRefresh != (ZDecimal)WE_StockOnHandInfo.OriginalValue)
			{
				ThrowErrorThatInventoryChanged();
			}
		}

		void ThrowErrorThatInventoryChanged()
		{
			throw new ZCannotSaveException(OverriddenInventoryUnitErrorMsg, ResString.GetMultilingualString("ABC4E8A9-1BCA-493C-BE70-8F28F8A2E5ED", "Inventory changed"), ExceptionType.BusinessFailure);
		}

		#endregion

		#region ChangeInventoryHeldCode

		public bool ChangeInventoryHeldCode(bool shouldChangeHeldCode)
		{
			var result = false;

			if (shouldChangeHeldCode && SupportsHoldCodeChange && HeldCodeChangeQuantity > 0m && IsFinalised)
			{
				var inventory = (WhsInventoryView)Inventory.FirstOrDefault();
				if (inventory != null)
				{
					var origInventoryStatus = WE_CurrentInventoryStatus;
					var status = HeldCodeToChangeTo == string.Empty ? InventoryStatus.Codes.Available : InventoryStatus.Codes.Held;

					// Check against WE_TransactionQuantity rather than WE_StockOnHand to maintain history if any stock has been picked
					if (HeldCodeChangeQuantity == WE_TransactionQuantity)
					{
						var oldInventoryStatus = inventory.WI_InventoryStatus;
						var oldHeldCode = WE_WHC_NKCurrentInventoryHeldCode;

						inventory.WI_InventoryStatus = status;
						WE_CurrentInventoryStatus = status;
						WE_WHC_NKCurrentInventoryHeldCode = HeldCodeToChangeTo;
						WE_CurrentHoldReason = HoldReasonToChangeTo;
						LogHeldCodeChanged(oldInventoryStatus, oldHeldCode);
					}
					else
					{
						CloneInventoryForPartialHeldCodeChange(status, inventory);
						inventory.WI_TotalUnits -= HeldCodeChangeQuantity;
					}

					if (origInventoryStatus != status)
					{
						inventory.Location.UpdateWLV_LastAllocatedOrChangedDateUtc();
					}

					HeldCodeChangeQuantity = 0;
					result = true;
				}
			}

			return result;
		}

		protected abstract bool SupportsHoldCodeChange { get; }

		void CloneInventoryForPartialHeldCodeChange(string status, WhsInventoryView inventory)
		{
			// compare to available stock, which excludes both committed and reserved stock
			if (HeldCodeChangeQuantity > inventory.WI_AvailableToTransferQuantity)
			{
				HeldCodeChangeQuantity = inventory.WI_AvailableToTransferQuantity;
			}

			var excludedColumns = new[]
			{
				WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus,
				WhsDocketLineSchema.Constants.WE_CurrentInventoryStatus,
				WhsDocketLineSchema.Constants.WE_WHC_NKOriginalInventoryHeldCode,
			};

			var args = new BusinessObjectCloneArgs(excludedColumns);
			var newDocketLine = (WhsDocketLine)Clone(args);
			var docketLineRow = ((IBusinessObjectInternals)newDocketLine).Row;
			newDocketLine.WE_WE_ParentDocketLine = PK;
			docketLineRow[WhsDocketLineSchema.Constants.WE_OriginalInventoryStatus] = WE_CurrentInventoryStatus;
			docketLineRow[WhsDocketLineSchema.Constants.WE_CurrentInventoryStatus] = status;
			docketLineRow[WhsDocketLineSchema.Constants.WE_WHC_NKOriginalInventoryHeldCode] = WE_WHC_NKCurrentInventoryHeldCode;
			newDocketLine.WE_WHC_NKCurrentInventoryHeldCode = HeldCodeToChangeTo;
			newDocketLine.WE_IsOriginalInventory = false;

			newDocketLine.WE_WD = WE_WD;
			newDocketLine.WE_TransactionQuantity = HeldCodeChangeQuantity;
			newDocketLine.WE_DocketLineStatus = WE_DocketLineStatus;
			newDocketLine.WE_FinalisedDate = WE_FinalisedDate;
			newDocketLine.WE_StockOnHand = HeldCodeChangeQuantity;
			newDocketLine.WE_WE_OriginalDocketLineForRating = WE_WE_OriginalDocketLineForRating;
			newDocketLine.WE_CurrentHoldReason = HoldReasonToChangeTo;

			newDocketLine.LogHeldCodeChanged(WE_CurrentInventoryStatus, WE_WHC_NKCurrentInventoryHeldCode);

			if (newDocketLine.Inventory.Count == 0)
			{
				var clone = Factory.NewWithPrimaryKey<WhsInventoryView>(newDocketLine.PK.ToGuid());
				newDocketLine.Inventory.Add(clone);
				clone.WI_InDocketLineType = Inventory[0].WI_InDocketLineType;
				clone.WI_OH_Client = Inventory[0].WI_OH_Client;
				clone.WI_WW_Whs = Inventory[0].WI_WW_Whs;
				clone.WI_HeldCode = newDocketLine.WE_WHC_NKCurrentInventoryHeldCode;
				clone.WI_OP = WE_OP;
				clone.WI_InDocketLineUnits = 0m;
				clone.WI_TotalUnits = HeldCodeChangeQuantity;
				clone.WI_F3_NKPackType = WE_F3_NKPackType;
				clone.WI_WL = WE_WL;
				clone.WI_PalletID = WE_PalletID;
				clone.WI_PartAttrib1 = WE_PartAttrib1;
				clone.WI_PartAttrib2 = WE_PartAttrib2;
				clone.WI_PartAttrib3 = WE_PartAttrib3;
				clone.WI_ExpiryDate = WE_ExpiryDate;
				clone.WI_PackingDate = WE_PackingDate;
				clone.WI_BondedEntryKey = WE_BondedEntryKey;
				clone.WI_AllocationKey = WE_AllocationKey;
				clone.OriginalInventoryStatus = status;
				clone.WI_WE_InDocketLine = newDocketLine.PK;
				clone.WI_WE_OriginalInDocketLineForRating = WE_WE_OriginalDocketLineForRating;
				clone.WI_IsOriginalReceiptLine = false;
				clone.WI_ArrivalDate = WE_AdjustmentArrivalDate;
				clone.WI_ExpectedReceiptQuantity = WE_ClientOrderedUnits;
			}
			else
			{
				var clone = newDocketLine.Inventory[0];
				clone.WI_InventoryStatus = newDocketLine.WE_CurrentInventoryStatus;
				clone.WI_InDocketLineUnits = 0m;
				clone.WI_ExpectedReceiptQuantity = WE_ClientOrderedUnits;
			}
		}

		void LogHeldCodeChanged(ZString oldInventoryStatus, ZString oldHeldCode)
		{
			// Hold Code Change only gets called on Factory Save, therefore, cache the event time so that all hold code change events will have the same event time.
			// This is needed for matching the inventory that has changed hold code when we propagate to the parent Docket.
			var now = Factory.GetCachedValue("WhsDocketLine|ChangeInventoryHeldCode|Now" + "|" + Docket.WD_WW_Whs, () => Warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow), CacheStalenessPolicy.StaleOnFactorySave);
			var reference = WE_CurrentInventoryStatus != oldInventoryStatus
				? string.Format(Culture.Invariant, (NoResString)"Status Changed from '{0}' to '{1}'", oldInventoryStatus, WE_CurrentInventoryStatus) // Used in a non localisable log
				: "";

			Logs.AddNew(Events.ChangeOfIdentifier, reference, now,
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.HoldCode),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Old, oldHeldCode),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.New, WE_WHC_NKCurrentInventoryHeldCode),
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, WE_CurrentHoldReason),
				// need a time stamp to make event reference unique so that propagation does not replace the
				// Event on the Original Docket if they had previously made the same hold code change.
				new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.FlightDate, now.ToLocalZDateTime().ToISO8601String()));

			CreateWhsInventoryHoldChangeLog(now);
		}

		void CreateWhsInventoryHoldChangeLog(ZDateTimeOffset eventTime)
		{
			var log = Factory.New<WhsInventoryHoldChangeLog>();
			log.WHL_WE_ParentDocketLine = PK;
			log.WHL_LogVersion = (short)(GetPreviousInventoryHoldChangeLogVersion() + 1);
			log.WHL_WHC_NKCode = WE_WHC_NKCurrentInventoryHeldCode;
			log.WHL_Reason = WE_CurrentHoldReason;
			log.WHL_EventTime = eventTime;
		}

		short GetPreviousInventoryHoldChangeLogVersion()
		{
			var previousLogVersion = (short)0;
			var query = new ZQuery();
			query.AddToFilter(WhsInventoryHoldChangeLogSchema.WHL_WE_ParentDocketLine, PK);
			query.OrderBy = WhsInventoryHoldChangeLogSchema.Constants.WHL_LogVersion + OrderByClause.Descending;

			var holdChangeLog = Factory.LoadTop1<WhsInventoryHoldChangeLog>(query);
			if (holdChangeLog != null)
			{
				previousLogVersion = (short)holdChangeLog.WHL_LogVersion;
			}
			return previousLogVersion;
		}

		#endregion

		#region OnSaving

		public override void OnSaving()
		{
			base.OnSaving();

			if (IsInventoryLine && !IsFinalised && WE_WL.IsValid)
			{
				OnSavingLastAllocatedOrChangedDate();
			}
		}

		void OnSavingLastAllocatedOrChangedDate()
		{
			var now = ZDateTime.UtcNow;
			if (WE_WLInfo.HasChanges && WE_WLInfo.OriginalValue.IsValid)
			{
				UpdateLocationsLastAllocatedOrChangedDate(now, (ZGuid)WE_WLInfo.OriginalValue);
			}
			UpdateLocationsLastAllocatedOrChangedDate(now, WE_WL);
		}

		#endregion

		#region IsInventoryLine

		public bool IsInventoryLine => IsInventoryLineCore;

		protected virtual bool IsInventoryLineCore => false;

		#endregion

		#region OnSaved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				if (fInventory != null && fInventory.Count == 1)
				{
					// RowState does not get managed by the factory as we disabled IsSavedByFactory.
					IBusinessObjectInternals inventoryToSave = fInventory[0];
					inventoryToSave.Row.AcceptChanges();
				}
			}
		}

		#endregion

		#region EnableLightValidationIfAvailable

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		#endregion

		#region Cloning

		#region SupportsCloneCore

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region CloneInternal

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (WhsDocketLine)base.CloneInternal(args);

			if (clone.WE_LineNo > 0m)
			{
				clone.WE_SubLineNo = GetSubLineNo();
			}
			CloneCustomsData(clone);

			return clone;
		}

		public ZShort GetSubLineNo()
		{
			ZShort subLineNo = WE_SubLineNo;

			var docket = Docket;
			if (docket != null)
			{
				foreach (var line in docket.Lines)
				{
					if (line.WE_SubLineNo > subLineNo && line.WE_LineNo == WE_LineNo)
					{
						subLineNo = line.WE_SubLineNo;
					}
				}
			}

			return ++subLineNo;
		}

		void CloneCustomsData(WhsDocketLine docketLine)
		{
			var docket = Docket;
			if (docket != null)
			{
				bool isCustomsTransaction = docket.IsCustomsTransaction;
				if (isCustomsTransaction)
				{
					var newCustomsData = (WhsBondedWarehouseAttribute)CustomsData.Clone();
					newCustomsData.SetParent(docketLine);

					if (docket.CountryCode == Constants.CountryCodes.UnitedStates)
					{
						docketLine.WE_PerPackageQty = WE_PerPackageQty;
						docketLine.WE_PackageGroupId = WE_PackageGroupId;
					}
				}
			}
		}

		#endregion

		#region GetPropertiesToExcludeFromCloning

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var propertiesToExcludeFromCloning = new List<string>(base.GetPropertiesToExcludeFromCloning());
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_WD);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_GS_NKPutawayBy);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_PutawayTime);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_FinalisedDate);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_DocketLineStatus);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_WE_ParentDocketLine);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_WE_MatchingLine);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_SubLineNo);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_WE_OriginalDocketLineForRating);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_SystemCreateTimeUtc);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_SystemCreateUser);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_SystemLastEditTimeUtc);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_SystemLastEditUser);
			propertiesToExcludeFromCloning.Add(WhsDocketLineSchema.Constants.WE_P9_Task);

			return propertiesToExcludeFromCloning;
		}

		#endregion

		#region Clone

		public T Clone<T>() where T : WhsDocketLine
		{
			return (T)Clone(new BusinessObjectCloneArgs(Enumerable.Empty<string>(), typeof(T)));
		}

		#endregion

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsDocketLineFetchStrategy(this);
		}

		#endregion

		#region Reload

		protected override void ReloadCore()
		{
			base.ReloadCore();
			ReloadInventory();
		}

		void ReloadInventory()
		{
			if (fInventory != null && fInventory.Count == 1)
			{
				IBusinessObjectInternals inventoryToReload = fInventory[0];
				inventoryToReload.Row[WhsInventoryViewSchema.Constants.WI_TotalUnits] = (decimal)WE_StockOnHand;
				inventoryToReload.Row[WhsInventoryViewSchema.Constants.WI_WL] = WE_WL.IsValid ? WE_WL.ToGuid() : Guid.Empty;
				inventoryToReload.Row[WhsInventoryViewSchema.Constants.WI_InventoryStatus] = WE_OriginalInventoryStatus.ToString();
			}
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("E53323FD-6B89-488E-A530-B1EF96E0CBB9", "Docket Line"); }
		}

		#endregion

		#region HumanReadableShortcutName

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var shortCutName = string.Empty;
				var inventory = IsInventoryLine
					? (WhsInventoryView)Inventory.FirstOrDefault()
					: null;

				if (inventory != null)
				{
					shortCutName = inventory.HumanReadableShortcutName;
				}

				return string.IsNullOrEmpty(shortCutName)
					? Res.GetString("38feaf30-770e-495e-a0f4-569a1619a292", "Docket Line")
					: shortCutName;
			}
		}

		#endregion

		#endregion

		#region Related Business Objects

		#region Docket

		public WhsDocket Docket
		{
			get { return (WhsDocket)Factory.Load(DocketType, WE_WD); }
		}

		#endregion

		#region DocketOriginal

		public WhsDocket DocketOriginal
		{
			get
			{
				var originalDocketLine = Factory.Load<WhsDocketLine>(WE_WE_OriginalDocketLineForRating);
				return originalDocketLine != null ? originalDocketLine.Docket : Docket;
			}
		}

		#endregion

		#region Product

		public WhsProduct Product
		{
			get
			{
				if (WE_OP.IsValid)
				{
					if (product == null || WE_OP != product.Parent.PK)
					{
						product = WhsProduct.GetWhsProduct(Factory, WE_OP);
					}
				}
				else
				{
					product = null;
				}
				return product;
			}
		}

		WhsProduct product;

		#endregion

		#region Inventory

		[ChildEditable(true)]
		public WhsInventoryViewCollection Inventory
		{
			get
			{
				if (fInventory == null)
				{
					fInventory = GetNewWhsInventoryViewCollection();
					RegisterEditableChildObject(fInventory);
				}

				if (fInventory.Count == 0 && !IsDeleted)
				{
					if (IsInventoryLine)
					{
						// this will find it in memory if it exists, whereas the collection load won't, resulting in unnecessary db hits.
						var query = new ZQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, PK);
						query.FetchOnlyFromLocalCache = !IsInDatabase;

						var oneToOneInventory = LoadInventoryView(query);
						if (oneToOneInventory != null)
						{
							fInventory.Add(oneToOneInventory);
						}
					}
				}

				return fInventory;
			}
		}

		protected virtual WhsInventoryView LoadInventoryView(ZQuery query)
		{
			return Factory.LoadTop1<WhsInventoryView>(query);
		}

		protected virtual WhsInventoryViewCollection GetNewWhsInventoryViewCollection()
		{
			return new WhsInventoryViewCollection(Factory, this);
		}

		WhsInventoryViewCollection fInventory;

		#endregion

		#region ClearInventoryCache

		public void ClearInventoryCache()
		{
			fInventory = null;
		}

		#endregion

		#region PickLines

		[ChildEditableTestExclude]
		public WhsPickLineCollection PickLines
		{
			get
			{
				if (pickLines == null)
				{
					pickLines = GetPickLinesCollection();
					AddFetchHintsForDocketPickLines();
				}

				return pickLines;
			}
		}

		WhsPickLineCollection pickLines;

		protected bool IsPickLinesCreated
		{
			get { return pickLines != null; }
		}

		protected virtual WhsPickLineCollection GetPickLinesCollection()
		{
			return new WhsPickLineCollection(this);
		}

		protected virtual void AddFetchHintsForDocketPickLines()
		{
		}

		#endregion

		#region ReservedPickLines

		[ChildEditable(true)]
		public ReservedPickLineCollection ReservedPickLines
		{
			get
			{
				if (reservedPickLines == null)
				{
					reservedPickLines = GetReservedPickLinesCollection();
					LoadReservedLines(reservedPickLines);
					RegisterEditableChildObject(reservedPickLines);
				}
				return reservedPickLines;
			}
		}

		protected virtual ReservedPickLineCollection GetReservedPickLinesCollection()
		{
			return new ReservedPickLineCollection(this, WhsPickLineSchema.WZ_WE_InventoryLine);
		}

		ReservedPickLineCollection reservedPickLines;

		protected virtual void LoadReservedLines(ReservedPickLineCollection reservedPickLineCollection)
		{
		}

		#endregion

		#region LocationArea

		public WhsArea LocationArea => Location?.PickingArea;

		#endregion

		#region BOMComponentLinks

		public IEnumerable<WhsBOMInventoryPivot> BOMComponentLinks => BOMComponentLinksCore;

		protected virtual IEnumerable<WhsBOMInventoryPivot> BOMComponentLinksCore
		{
			get
			{
				IEnumerable<WhsBOMInventoryPivot> result = null;
				if (WE_WE_OriginalDocketLineForRating != Guid.Empty)
				{
					var query = new ZQuery(WhsBOMInventoryPivotSchema.WIP_WE_InventoryLine, WE_WE_OriginalDocketLineForRating);
					result = Factory.Load<WhsBOMInventoryPivot>(query);
				}

				return result ?? Enumerable.Empty<WhsBOMInventoryPivot>();
			}
		}

		#endregion

		#region ComponentInventoryLines

		public ActiveBusinessObjectCollection<WhsDocketLine> ComponentInventoryLines
		{
			get
			{
				if (componentInventoryLines == null)
				{
					componentInventoryLines = new ActiveBusinessObjectCollection<WhsDocketLine>(Factory, new AdhocCollectionRelationship(typeof(WhsDocketLine)));

					var distinctInventory = IEnumerableExtensions.DistinctBy(BOMComponentLinks.SelectMany(l => l.ComponentLine.PickLines), pl => pl.InventoryLinePKForAvailableInventory).Select(pl => pl.InventoryLineForAvailableInventory);
					componentInventoryLines.AddRange(distinctInventory);
				}
				return componentInventoryLines;
			}
		}

		ActiveBusinessObjectCollection<WhsDocketLine> componentInventoryLines;

		#endregion

		#endregion

		#region UpdateWeightAndVolumeOnRemove

		// Moved from dbo.WhsDocket, tested in WhsDocket.
		protected void UpdateWeightAndVolumeOnRemove(WhsDocket previousDocket)
		{
			if (previousDocket != null && ShouldUpdateWeightAndVolumeOfDocketFromDocketLine(previousDocket))
			{
				previousDocket.UpdateTotalWeightAndVolume(SupplierPart, -WE_TransactionQuantity);
			}
		}

		#endregion

		#region Properties

		#region Country Code

		public ZString CountryCode
		{
			get
			{
				var docket = Docket;
				return (docket != null) ? docket.CountryCode : ZString.Empty;
			}
		}

		#endregion

		#region HeldCodeToChangeTo

		[List("Lookups.InventoryHeldCodeCollection")]
		[ResourceStringData("WhsDocketLine|HeldCodeToChangeTo", Caption = "Hold Code Change")]
		[ReadOnlyMember(nameof(InventoryEditFormReadOnly))]
		public ZString HeldCodeToChangeTo
		{
			get { return heldCodeToChangeTo; }
			set
			{
				SetNonPersistentPropertyValue(HeldCodeToChangeToInfo, ref heldCodeToChangeTo, value, setValueOnlyIfDifferentToGetter: false);

				if (HeldCodeChangeQuantity == 0m && !ClearingHoldCodeChangeSemaphore.IsSuspended)
				{
					var inventory = (WhsInventoryView)Inventory.FirstOrDefault();
					HeldCodeChangeQuantity = inventory != null ? inventory.WI_AvailableToTransferQuantity : 0;
				}

				if (!HeldCodeToChangeTo.IsEmpty && HoldReasonToChangeTo.IsEmpty)
				{
					using (GetValidationSuspender())
					{
						HoldReasonToChangeTo = WE_CurrentHoldReason;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateHeldCodeToChangeTo();
					Validation.ValidateHeldCodeChangeQuantity();
					Validation.ValidateHoldReasonToChangeTo();
				}
			}
		}

		ZString heldCodeToChangeTo;

		public ZPropertyInfo HeldCodeToChangeToInfo
		{
			get { return GetZPropertyInfo(Schema.HeldCodeToChangeTo); }
		}

		#endregion

		#region HeldCodeChangeQuantity

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(InventoryEditFormReadOnly))]
		[ResourceStringData("WhsDocketLine|HeldCodeChangeQuantity", MediumCaption = "Hold Code Change Qty", Caption = "Hold Code Change Quantity")]
		public ZDecimal HeldCodeChangeQuantity
		{
			get { return heldCodeChangeQuantity; }
			set
			{
				if (value != heldCodeChangeQuantity)
				{
					SetNonPersistentPropertyValue(HeldCodeChangeQuantityInfo, ref heldCodeChangeQuantity, value);

					if (value == 0m)
					{
						using (new SemaphoreManager(ClearingHoldCodeChangeSemaphore))
						using (GetValidationSuspender())
						{
							HeldCodeToChangeTo = "";
							HoldReasonToChangeTo = "";
						}
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateHeldCodeChangeQuantity();
						Validation.ValidateHeldCodeToChangeTo();
						Validation.ValidateHoldReasonToChangeTo();
					}
				}
			}
		}

		ZDecimal heldCodeChangeQuantity;

		public ZPropertyInfo HeldCodeChangeQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.HeldCodeChangeQuantity); }
		}

		Semaphore ClearingHoldCodeChangeSemaphore
		{
			get { return clearingHoldCodeChangeSemaphore ?? (clearingHoldCodeChangeSemaphore = new Semaphore()); }
		}

		Semaphore clearingHoldCodeChangeSemaphore;

		#endregion

		#region HoldReasonToChangeTo

		[ActionField(ReadOnly = true, MaxLength = Schema.WE_CurrentHoldReasonMaxLength)]
		[MaxLength(Schema.WE_CurrentHoldReasonMaxLength)]
		[ReadOnlyMember(nameof(InventoryEditFormReadOnly))]
		[ResourceStringData("WhsDocketLine|HoldReasonToChangeTo", Caption = "Hold Change Reason")]
		public ZString HoldReasonToChangeTo
		{
			get => holdReasonToChangeTo;
			set
			{
				SetNonPersistentPropertyValue(HoldReasonToChangeToInfo, ref holdReasonToChangeTo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateHoldReasonToChangeTo();
				}
			}
		}

		ZString holdReasonToChangeTo;

		public ZPropertyInfo HoldReasonToChangeToInfo
		{
			get { return GetZPropertyInfo(Schema.HoldReasonToChangeTo); }
		}

		#endregion

		#region WE_AllocationKey

		[ReadOnly(true)]
		public override ZString WE_AllocationKey
		{
			get => base.WE_AllocationKey;
			set
			{
				base.WE_AllocationKey = value;
				SetValueToInventoryView(WhsInventoryViewSchema.WI_AllocationKey.Name, value);
			}
		}

		#endregion

		#region WE_P9_Task

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZGuid WE_P9_Task
		{
			get => base.WE_P9_Task;
			set => base.WE_P9_Task = value;
		}

		#endregion

		#region WE_OP

		[ReadOnlyMember(nameof(ProductReadOnly))]
		[List("Lookups.SupplierParts")]
		public override ZGuid WE_OP
		{
			get { return base.WE_OP; }
			set
			{
				if (base.WE_OP != value)
				{
					base.WE_OP = value;
					ClearLocationRequiredCapacityCache();
					SetValueToInventoryView(WhsInventoryViewSchema.WI_OP.Name, value);

					if (!IsCopying)
					{
						SetLineDataBasedOnProduct();
						SetPackTypeFromProductParams();
					}
				}
			}
		}

		#region SetLineDataBasedOnProduct

		protected void SetLineDataBasedOnProduct()
		{
			var part = SupplierPart;
			if (part != null)
			{
				if (WE_F3_NKPackType.IsEmpty)
				{
					WE_F3_NKPackType = part.OP_StockKeepingUnit;
				}

				CalcQuantityFromWE_PackQty();

				var client = Docket?.Client;
				if (client != null)
				{
					var manager = client.PartAttributeManager;
					if (!manager.IsExpiryDateUsedByProduct(part))
					{
						WE_ExpiryDate = ZDate.Empty;
					}

					if (!manager.IsPackingDateUsedByProduct(part))
					{
						WE_PackingDate = ZDate.Empty;
					}

					if (!manager.IsPartAttributeUsedByProduct(part, 1))
					{
						WE_PartAttrib1 = "";
					}

					if (!manager.IsPartAttributeUsedByProduct(part, 2))
					{
						WE_PartAttrib2 = "";
					}

					if (!manager.IsPartAttributeUsedByProduct(part, 3))
					{
						WE_PartAttrib3 = "";
					}

					if (!manager.IsSerialNumberUsedByProduct(part))
					{
						WE_SerialNumber = "";
					}
				}
			}
		}

		#endregion

		#region SetPackTypeFromProductParams

		protected void SetPackTypeFromProductParams()
		{
			if (!IsCopying)
			{
				var productParams = GetProductParams(Docket);
				if (productParams != null)
				{
					var packType = GetPackTypeFromProductParams(productParams);
					if (!packType.IsEmpty)
					{
						WE_F3_NKPackType = packType;
					}
				}
			}
		}

		protected WhsProductParamsByWhsAndClient GetProductParams(WhsDocket docket) => docket?.GetProductParams(Product);

		protected virtual ZString GetPackTypeFromProductParams(WhsProductParamsByWhsAndClient productParams)
		{
			return "";
		}

		#endregion

		#endregion

		#region WE_F3_NKPackType

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PackQuantityAndTypeAndRelatedQuantityInfoReadOnly))]
		[List("Lookups.PackTypes")]
		public override ZString WE_F3_NKPackType
		{
			get { return base.WE_F3_NKPackType; }
			set
			{
				if (WE_F3_NKPackType != value)
				{
					base.WE_F3_NKPackType = value;
					SetValueToInventoryView(WhsInventoryViewSchema.WI_F3_NKPackType.Name, value);
					CalcQuantityFromWE_PackQty();
				}
			}
		}

		#endregion

		#region PackUOM

		public ZString PackUOM => PackType?.F3_UOMType ?? string.Empty;

		#endregion

		#region WE_ClientOrderedUnits

		[ActionField(ReadOnly = true)]
		public override ZDecimal WE_ClientOrderedUnits
		{
			get { return base.WE_ClientOrderedUnits; }
			set
			{
				base.WE_ClientOrderedUnits = value;
				SetValueToInventoryView(WhsInventoryView.Schema.WI_ExpectedReceiptQuantity, value);
			}
		}

		#endregion

		#region WE_ClientUQ

		[MaxLength(OrgPartRelation.Schema.OU_ClientUQMaxLength)]
		public ZString WE_ClientUQ
		{
			get
			{
				ZString result = "";
				if (SupplierPart != null && SupplierPart.RelatedOrganisations != null && Docket != null && Docket.Client != null)
				{
					OrgPartRelation relation = SupplierPart.RelatedOrganisations.FindByOrganisationAndRelationship(Docket.Client, OrgPartRelation.RelationshipTypes.Owner);
					if (relation != null && !relation.OU_ClientUQ.IsEmpty)
					{
						result = relation.OU_ClientUQ;
					}
				}
				return result.IsEmpty ? ProductUQ : result;
			}
		}

		public ZPropertyInfo WE_ClientUQInfo
		{
			get { return GetZPropertyInfo(Schema.WE_ClientUQ); }
		}

		#endregion

		#region WE_PackageGroupId

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PackQuantityAndTypeAndRelatedQuantityInfoReadOnly))]
		public override ZString WE_PackageGroupId
		{
			get { return base.WE_PackageGroupId; }
			set
			{
				base.WE_PackageGroupId = value;
				if (!IsValidationSuspended)
				{
					ValidateUnitsProperty();
					Validation.ValidateWE_PerPackageQty();
				}
			}
		}

		#endregion

		#region WE_PackQuantity

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PackQuantityAndTypeAndRelatedQuantityInfoReadOnly))]
		public virtual ZDecimal WE_PackQuantity
		{
			get
			{
				if (!we_PackQuantity.HasValue)
				{
					var supplierPart = SupplierPart;
					if (supplierPart != null)
					{
						var transactionQty = WE_TransactionQuantity;
						var quantity = supplierPart.UnitConverter.Convert(transactionQty, ProductUQ, WE_F3_NKPackType);
						var packQty = (quantity != 0m) ? quantity : transactionQty;
						we_PackQuantity = Utilities.Round(packQty, 2);
					}
				}

				return we_PackQuantity ?? ZDecimal.Zero;
			}
			set
			{
				SetNonPersistentPropertyValue(WE_PackQuantityInfo, ref we_PackQuantity, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_PackQuantity();
				}

				CalcQuantityFromWE_PackQty();
				RefreshBinding();
			}
		}

		ZDecimal? we_PackQuantity;

		public void ClearPackQuantity() => we_PackQuantity = null;

		#endregion

		#region WE_PerPackageQty

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PackQuantityAndTypeAndRelatedQuantityInfoReadOnly))]
		public override ZDecimal WE_PerPackageQty
		{
			get { return base.WE_PerPackageQty; }
			set
			{
				base.WE_PerPackageQty = value;
				// validate related field
				if (!IsValidationSuspended)
				{
					ValidateUnitsProperty();
				}
			}
		}

		#endregion

		#region WE_PutawayTime

		[ReadOnly(true)]
		public override ZDateTimeOffset WE_PutawayTime
		{
			get { return base.WE_PutawayTime; }
			set { base.WE_PutawayTime = value; }
		}

		#endregion

		#region WE_TransactionQuantity

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(TransactionQtyReadOnly))]
		public override ZDecimal WE_TransactionQuantity
		{
			get { return base.WE_TransactionQuantity; }
			set
			{
				var oldValue = WE_TransactionQuantity;

				base.WE_TransactionQuantity = value;

				var docket = Docket;
				if (docket != null)
				{
					var delta = WE_TransactionQuantity - oldValue;
					if (delta != 0)
					{
						ClearLocationRequiredCapacityCache();

						if (!UpdatingTotalWeightAndVolumeSemaphore.IsSuspended && ShouldUpdateWeightAndVolumeOfDocketFromDocketLine(docket))
						{
							docket.UpdateTotalWeightAndVolume(SupplierPart, delta);
						}
					}
				}

				CalcWE_PackQtyFromWE_TransactionQuantity();

				if (docket != null && SettingWE_TransactionQuantityShouldRefreshTotalUnitsFromLines)
				{
					docket.WD_TotalUnitsFromLinesInfo.RefreshBinding();
				}
			}
		}

		void ClearLocationRequiredCapacityCache()
		{
			if (!IsDeleted && WE_WL.IsValid)
			{
				// delete cache to calculate required capacity again
				Docket?.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
			}
		}

		protected virtual bool SettingWE_TransactionQuantityShouldRefreshTotalUnitsFromLines
		{
			get { return true; }
		}

		protected virtual bool ShouldUpdateWeightAndVolumeOfDocketFromDocketLine(WhsDocket docket) => true;

		protected void CalcWE_PackQtyFromWE_TransactionQuantity()
		{
			if (!CalculateWE_TransactionQuantityAndWE_PackQtySemaphore.IsSuspended)
			{
				using (new SemaphoreManager(CalculateWE_TransactionQuantityAndWE_PackQtySemaphore))
				{
					var part = this.SupplierPart;
					if (part != null)
					{
						var result = WE_TransactionQuantity;
						this.ReplaceOverflowExceptionWithRowError(WE_PackQuantityInfo.HumanReadableName, () =>
						{
							if (part.UnitConverter.Convertible(ProductUQ, WE_F3_NKPackType))
							{
								result = Utilities.Round(part.UnitConverter.Convert(WE_TransactionQuantity, ProductUQ, WE_F3_NKPackType), 2);
							}
						});
						WE_PackQuantity = result;
					}
					else if (IsTemporaryProduct)
					{
						WE_PackQuantity = WE_TransactionQuantity;
						WE_F3_NKPackType = ProductUQ;
					}
				}
			}
		}

		protected virtual void CalcQuantityFromWE_PackQty()
		{
			UpdateQuantityFromWE_PackQty(WE_TransactionQuantityInfo);
		}

		protected void UpdateQuantityFromWE_PackQty(ZPropertyInfo propertyInfoToUpdate)
		{
			if (!CalculateWE_TransactionQuantityAndWE_PackQtySemaphore.IsSuspended)
			{
				using (new SemaphoreManager(CalculateWE_TransactionQuantityAndWE_PackQtySemaphore))
				{
					if (WE_OP.IsValid)
					{
						propertyInfoToUpdate.Value = GetOrderedQuantityFromPackageQuantity(WE_PackQuantity);
					}
					else if (IsTemporaryProduct)
					{
						propertyInfoToUpdate.Value = WE_PackQuantity;

						if (WE_F3_NKPackType.IsEmpty)
						{
							WE_F3_NKPackType = ProductUQ;
						}
						else
						{
							ProductUQ = WE_F3_NKPackType;
						}
					}
				}
			}
		}

		public ZDecimal GetOrderedQuantityFromPackageQuantity(ZDecimal packageQuantity)
		{
			var result = packageQuantity;
			this.ReplaceOverflowExceptionWithRowError(WE_TransactionQuantityInfo.HumanReadableName, () =>
			{
				var supplierPart = SupplierPart;
				if (supplierPart != null && supplierPart.UnitConverter.Convertible(WE_F3_NKPackType, ProductUQ))
				{
					result = Utilities.Round(supplierPart.UnitConverter.Convert(packageQuantity, WE_F3_NKPackType, ProductUQ), supplierPart.OP_CountDecimalPlaces);
				}
			});

			return result;
		}

		#region CalculateWE_TransactionQuantityAndWE_PackQtySemaphore

		Semaphore CalculateWE_TransactionQuantityAndWE_PackQtySemaphore
		{
			get { return calculateWE_TransactionQuantityAndWE_PackQtySemaphore ?? (calculateWE_TransactionQuantityAndWE_PackQtySemaphore = new Semaphore()); }
		}

		Semaphore calculateWE_TransactionQuantityAndWE_PackQtySemaphore;

		#endregion

		#region Updating Total Weight and Volume Semaphore

		internal void SuspendingUpdatingTotalWeightAndVolume()
		{
			((ISemaphoreItemInternals)UpdatingTotalWeightAndVolumeSemaphore).Increment();
		}

		internal void ResumeUpdatingTotalWeightAndVolume()
		{
			((ISemaphoreItemInternals)UpdatingTotalWeightAndVolumeSemaphore).Decrement();
		}

		protected Semaphore UpdatingTotalWeightAndVolumeSemaphore
		{
			get { return updatingTotalWeightAndVolumeSemaphore ?? (updatingTotalWeightAndVolumeSemaphore = new Semaphore()); }
		}

		Semaphore updatingTotalWeightAndVolumeSemaphore;

		#endregion

		#endregion

		#region DocketLine Flags

		#region IsBOMProduct

		public ZBool IsBOMProduct => SupplierPart?.BillOfMaterials.Count > 0;

		public virtual ZPropertyInfo IsBOMProductInfo => GetZPropertyInfo(Schema.IsBOMProduct);

		#endregion

		public bool IsDocketFinalising
		{
			get
			{
				var docket = Docket;
				return docket != null && docket.IsFinalising;
			}
		}

		public bool IsDocketFinalised
		{
			get
			{
				var docket = Docket;
				return docket != null && docket.IsFinalised;
			}
		}

		public bool IsDocketCancelled
		{
			get
			{
				var docket = Docket;
				return docket != null && docket.IsCancelled;
			}
		}

		public bool IsDocketFinalisedOrCancelled
		{
			get
			{
				var docket = Docket;
				return docket != null && docket.IsFinalisedOrCancelled;
			}
		}

		public bool CanUpdateFromInventory => !IsDocketFinalisedOrCancelled && CanUpdateFromInventoryCore;

		protected virtual bool CanUpdateFromInventoryCore => true;

		#region IsReadyForPlanningOrPlanned

		protected bool IsReadyForPlanningOrPlanned => Docket?.IsReadyForPlanningOrPlanned ?? false;

		#endregion

		#region IsFinalised

		public bool IsFinalised
		{
			get { return IsFinalisedCore; }
		}

		protected virtual bool IsFinalisedCore
		{
			get { return WE_FinalisedDate.IsValid; }
		}

		#endregion

		#region IsTemporaryProduct

		public bool IsTemporaryProduct
		{
			get { return IsTemporaryProductCore; }
		}

		protected virtual bool IsTemporaryProductCore
		{
			get { return false; }
		}

		#endregion

		#region IsAvailable

		public bool IsAvailable
		{
			get { return WE_CurrentInventoryStatus.EqualsIgnoringCase(InventoryStatus.Codes.Available); }
		}

		#endregion

		#region HasEDocsOrNotesAttached

		[ResourceStringData("WhsReceiveLine|HasEDocsOrNotesAttached", Caption = "eDocs/Notes")]
		public ZBool HasEDocsOrNotesAttached
		{
			get { return StorageMain != null || Notes.HasNotes; }
		}

		BusinessObject StorageMain
		{
			get { return storageMain ?? (storageMain = (BusinessObject)DocFactory.LoadTop1<IStorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, this.PK))); }
		}

		BusinessObject storageMain;

		public BusinessObjectFactory DocFactory
		{
			get
			{
				if (docFactory == null)
				{
					IDocumentFactoryProvider documentFactoryProvider = ObjectFactory.Get<IDocumentFactoryProvider>();
					docFactory = (BusinessObjectFactory)documentFactoryProvider.GetFactory(Factory);
				}
				return docFactory;
			}
			set
			{
				docFactory = value;
			}
		}

		BusinessObjectFactory docFactory;

		#endregion

		#region IsCreatedFromPickByBOM

		public bool IsCreatedFromPickByBOM => IsCreatedFromPickByBOMCore;

		protected virtual bool IsCreatedFromPickByBOMCore => false;

		#endregion

		#endregion

		#region QuantityNotMet

		public ZDecimal QuantityNotMet
		{
			get { return WE_TransactionQuantity - SumOfUnitsMet; }
		}

		#endregion

		#region SumOfUnitsMet

		[ResourceStringData("WhsDocketLine|SumOfUnitsMet", Caption = "Quantity Met")]
		public ZDecimal SumOfUnitsMet
		{
			get { return SumOfUnitsMetCore; }
		}

		protected virtual ZDecimal SumOfUnitsMetCore
		{
			get { return IsDocketFinalised ? WE_TransactionQuantity : ZDecimal.Zero; }
		}

		#endregion

		#region WE_WD

		public override ZGuid WE_WD
		{
			get { return base.WE_WD; }
			set
			{
				if (base.WE_WD != value)
				{
					var previousDocket = Docket;

					base.WE_WD = value;
					SetValueToInventoryView(WhsInventoryViewSchema.WI_WD.Name, value);

					UpdatePreviousDocketValues(previousDocket);
					UpdateNewDocketValues();
				}
			}
		}

		void UpdatePreviousDocketValues(WhsDocket previousDocket)
		{
			if (previousDocket != null)
			{
				previousDocket.MaxLineNoManager.LineNoDeleted(WE_LineNo);
				UpdateWeightAndVolumeOnRemove(previousDocket);
			}
		}

		void UpdateNewDocketValues()
		{
			var docket = Docket;
			if (docket != null)
			{
				if (ShouldUpdateWeightAndVolumeOfDocketFromDocketLine(docket))
				{
					docket.UpdateTotalWeightAndVolume(SupplierPart, WE_TransactionQuantity);
				}
				if (WE_LineNo == 0)
				{
					if (docket.NeedToAutoGenerateLineNumbersOnCreation && docket.MaxLineNoManager.CurrentMaxLineNo != short.MaxValue)
					{
						WE_LineNo = docket.MaxLineNoManager.CurrentMaxLineNo + 1;
					}
				}
				else
				{
					docket.MaxLineNoManager.LineNoSet(WE_LineNo);
				}
			}
		}

		#endregion

		#region Attributes

		#region WE_ExpiryDate

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(ExpiryDateReadOnly))]
		public override ZDate WE_ExpiryDate
		{
			get { return base.WE_ExpiryDate; }
			set
			{
				base.WE_ExpiryDate = value;
				SetValueToInventoryView(WhsInventoryViewSchema.WI_ExpiryDate.Name, value);
			}
		}

		#endregion

		#region WE_PackingDate

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PackingDateReadOnly))]
		public override ZDate WE_PackingDate
		{
			get => base.WE_PackingDate;
			set
			{
				base.WE_PackingDate = value;
				SetValueToInventoryView(WhsInventoryViewSchema.WI_PackingDate.Name, value);

				if (DocketLineCanCalculateExpiryDateFromPackingDate)
				{
					CalculateExpiryDateFromPackingDate();
				}
			}
		}

		protected virtual bool DocketLineCanCalculateExpiryDateFromPackingDate => false;

		void CalculateExpiryDateFromPackingDate()
		{
			if (WE_PackingDate.IsValid && !ExpiryDateReadOnly && WE_ExpiryDate.IsEmpty)
			{
				var productParam = GetProductParams(Docket);
				if (productParam != null && productParam.W3_MaximumShelfLife > 0)
				{
					WE_ExpiryDate = WE_PackingDate.AddDays(productParam.W3_MaximumShelfLife);
				}
			}
		}

		#endregion

		#region WE_PartAttrib1

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PartAttrib1InfoReadOnly))]
		public override ZString WE_PartAttrib1
		{
			get { return base.WE_PartAttrib1; }
			set
			{
				SetValueToInventoryView(WhsInventoryViewSchema.WI_PartAttrib1.Name, value); // should be set before base bacause of validation
				base.WE_PartAttrib1 = value;
				SetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed(WE_PartAttrib1, 1);
			}
		}

		#endregion

		#region WE_PartAttrib2

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PartAttrib2InfoReadOnly))]
		public override ZString WE_PartAttrib2
		{
			get { return base.WE_PartAttrib2; }
			set
			{
				SetValueToInventoryView(WhsInventoryViewSchema.WI_PartAttrib2.Name, value); // should be set before base bacause of validation
				base.WE_PartAttrib2 = value;
				SetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed(WE_PartAttrib2, 2);
			}
		}

		#endregion

		#region WE_PartAttrib3

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(PartAttrib3InfoReadOnly))]
		public override ZString WE_PartAttrib3
		{
			get { return base.WE_PartAttrib3; }
			set
			{
				SetValueToInventoryView(WhsInventoryViewSchema.WI_PartAttrib3.Name, value); // should be set before base bacause of validation
				base.WE_PartAttrib3 = value;
				SetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed(WE_PartAttrib3, 3);
			}
		}

		#endregion

		#region WE_SerialNumber

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(SerialNumberReadOnly))]
		public override ZString WE_SerialNumber
		{
			get => base.WE_SerialNumber;
			set
			{
				SetValueToInventoryView(WhsInventoryViewSchema.WI_SerialNumber.Name, value); // should be set before base bacause of validation
				base.WE_SerialNumber = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateWE_TransactionQuantity();
				}
			}
		}

		#endregion

		#region SetPackingDateAndExpiryDateIfJulianBatchNumberAttributeIsUsed

		void SetExpiryDateAndPackingDateIfJulianBatchNumberAttributeIsUsed(ZString partAttribute, int partAttributeNumber)
		{
			var whsProduct = Product;
			var docket = Docket;
			if (whsProduct != null && docket != null)
			{
				var client = docket.Client;
				if (client != null && whsProduct.IsPartAttributeAJulianBatchNumberAndUsed(client, partAttributeNumber))
				{
					WE_PackingDate = whsProduct.GetPackingDateFromJulianBatchNumber(client, docket.Warehouse, partAttribute);
					WE_ExpiryDate = whsProduct.CalculateExpiryDate(client, docket.Warehouse, partAttribute);
				}
			}
		}

		#endregion

		#endregion

		#region WE_OriginalInventoryStatus

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WE_OriginalInventoryStatus
		{
			get { return base.WE_OriginalInventoryStatus; }
			set
			{
				base.WE_OriginalInventoryStatus = value;
				WE_CurrentInventoryStatus = value;
				SetValueToInventoryView(WhsInventoryView.Schema.OriginalInventoryStatus, value);

				if (IsFinalised)
				{
					throw new InvalidOperationException("Original Inventory Status of Docket Line should not be modified after it is finalised.");
				}
			}
		}

		#endregion

		#region WE_CurrentInventoryStatus

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		[List("Lookups.InventoryStatuses")]
		public override ZString WE_CurrentInventoryStatus
		{
			get { return base.WE_CurrentInventoryStatus; }
			set
			{
				base.WE_CurrentInventoryStatus = value;
				SetValueToInventoryView(WhsInventoryViewSchema.WI_InventoryStatus.Name, value);
			}
		}

		#endregion

		#region WE_WHC_NKOriginalInventoryHeldCode

		[ReadOnlyMember(nameof(HeldCodeReadonly))]
		[List("Lookups.InventoryHeldCodeCollection")]
		[RelatedBusinessObject(Schema.OriginalHeldCode)]
		public override ZString WE_WHC_NKOriginalInventoryHeldCode
		{
			get { return base.WE_WHC_NKOriginalInventoryHeldCode; }
			set
			{
				if (ShouldSettingHeldCodeChangeStatus && WE_WHC_NKOriginalInventoryHeldCode != value)
				{
					var statusToChangeTo = !value.IsEmpty ? InventoryStatus.Codes.Held : InventoryStatus.Codes.Available;
					WE_OriginalInventoryStatus = statusToChangeTo;
				}

				base.WE_WHC_NKOriginalInventoryHeldCode = value;

				if (!IsBeingCloned)
				{
					WE_WHC_NKCurrentInventoryHeldCode = value;
				}

				if (IsFinalised)
				{
					throw new InvalidOperationException("Original Inventory Hold Code of Docket Line should not be modified after it is finalised.");
				}
			}
		}

		protected virtual bool ShouldSettingHeldCodeChangeStatus
		{
			get { return true; }
		}

		[ActionFieldFollow(false)]
		public WhsInventoryHeldCode OriginalHeldCode
		{
			get { return Factory.LoadFromNaturalKey<WhsInventoryHeldCode>(WhsInventoryHeldCodeSchema.WHC_Code, WE_WHC_NKOriginalInventoryHeldCode); }
		}

		protected virtual bool HeldCodeReadonly
		{
			get { return StandardReadOnly; }
		}

		#endregion

		#region WE_WHC_NKCurrentInventoryHeldCode

		[ReadOnly(true)]
		[List("Lookups.InventoryHeldCodeCollection")]
		public override ZString WE_WHC_NKCurrentInventoryHeldCode
		{
			get { return base.WE_WHC_NKCurrentInventoryHeldCode; }
			set
			{
				var prevHeldCode = WE_WHC_NKCurrentInventoryHeldCode;

				base.WE_WHC_NKCurrentInventoryHeldCode = value;

				if (SupportsHoldCodeChange)
				{
					SetValueToInventoryView(WhsInventoryViewSchema.WI_HeldCode.Name, value);

					if (prevHeldCode != InventoryHoldCodes.Codes.LostInCycleCount
						&& value == InventoryHoldCodes.Codes.LostInCycleCount)
					{
						PreviousHeldCodeForCycleCount = prevHeldCode;
					}
				}
			}
		}

		#endregion

		#region WE_WHC_NKOrderedHeldCode

		[ReadOnlyMember(nameof(IsOrderedHoldCodeReadOnly))]
		[List(nameof(Lookups) + "." + nameof(WhsDocketLineLookups.InventoryHeldCodeCollection))]
		public override ZString WE_WHC_NKOrderedHeldCode
		{
			get { return base.WE_WHC_NKOrderedHeldCode; }
			set { base.WE_WHC_NKOrderedHeldCode = value; }
		}

		#endregion

		#region PreviousHeldCodeForCycleCount

		public ZString PreviousHeldCodeForCycleCount
		{
			get
			{
				var result = ZString.Empty;
				if (IsInventoryLine)
				{
					result = this.GetSystemDefinedValue<ZString>(nameof(PreviousHeldCodeForCycleCount));
				}
				return result;
			}
			private set
			{
				if (IsInventoryLine)
				{
					this.SetSystemDefinedValue(nameof(PreviousHeldCodeForCycleCount), value);
				}
			}
		}

		#endregion

		#region WE_CurrentHoldReason

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZString WE_CurrentHoldReason
		{
			get => base.WE_CurrentHoldReason;
			set => base.WE_CurrentHoldReason = value;
		}

		#endregion

		#region WE_PalletID

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(DestPalletIDReadOnly))]
		public override ZString WE_PalletID
		{
			get { return base.WE_PalletID; }
			set
			{
				base.WE_PalletID = value;
				SetValueToInventoryView(WhsInventoryViewSchema.WI_PalletID.Name, value);
			}
		}

		#endregion

		#region WE_WE_OriginalDocketLineForRating

		public override ZGuid WE_WE_OriginalDocketLineForRating
		{
			get { return base.WE_WE_OriginalDocketLineForRating; }
			set
			{
				base.WE_WE_OriginalDocketLineForRating = value;
				SetValueToInventoryView(WhsInventoryViewSchema.WI_WE_OriginalInDocketLineForRating.Name, value);
			}
		}

		#endregion

		#region WE_BondedEntryKey

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(BondedEntryKeyReadOnly))]
		[ResourceStringData("WhsDocket|WE_BondedEntryKey", Caption = "Inwards Entry Key")]
		public override ZString WE_BondedEntryKey
		{
			get { return base.WE_BondedEntryKey; }
			set
			{
				base.WE_BondedEntryKey = value;
				SetValueToInventoryView(WhsInventoryViewSchema.WI_BondedEntryKey.Name, value);
			}
		}

		#endregion

		#region WE_IsOriginalInventory

		[ActionField(ReadOnly = true)]
		public override ZBool WE_IsOriginalInventory
		{
			get { return base.WE_IsOriginalInventory; }
			set
			{
				base.WE_IsOriginalInventory = value;
				SetValueToInventoryView(WhsInventoryViewSchema.WI_IsOriginalReceiptLine.Name, value);
			}
		}

		#endregion

		#region WE_AdjustmentArrivalDate

		[ActionField(ReadOnly = true)]
		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZDateTimeOffset WE_AdjustmentArrivalDate
		{
			get { return base.WE_AdjustmentArrivalDate; }
			set
			{
				base.WE_AdjustmentArrivalDate = value;
				SetValueToInventoryView(WhsInventoryViewSchema.WI_ArrivalDate.Name, value);
			}
		}

		#endregion

		#region Calculated Properties

		#region CommittedQuantityIncludingUnfinalisedReceipt

		public ZDecimal CommittedQuantityIncludingUnfinalisedReceipt
		{
			get { return Inventory.Count > 0 ? Inventory[0].CommittedQuantityIncludingUnfinalisedReceipt : ZDecimal.Zero; }
		}

		#endregion

		#region ProductCode

		[MaxLength(OrgSupplierPart.Schema.OP_PartNumMaxLength)]
		public ZString ProductCode
		{
			get
			{
				var part = this.SupplierPart;
				return part != null ? part.OP_PartNum : temporaryProductCode;
			}
			set
			{
				if (temporaryProductCode != value)
				{
					CheckMaximumLength(ProductCodeInfo, value);
					temporaryProductCode = value;
					SetValueToInventoryView(WhsInventoryView.Schema.WI_OP_PartNum, value);
					ProductCodeInfo.RefreshBinding();
				}
			}
		}

		ZString temporaryProductCode;

		public ZPropertyInfo ProductCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ProductCode); }
		}

		#endregion

		#region ProductDesc

		[ReadOnlyMember(nameof(TempProductReadOnly))]
		[MaxLength(OrgSupplierPart.Schema.OP_DescMaxLength)]
		public ZString ProductDesc
		{
			get
			{
				var supplierPart = SupplierPart;
				return supplierPart != null ? supplierPart.OP_Desc : temporaryProductDesc;
			}
			set
			{
				if (temporaryProductDesc != value)
				{
					CheckMaximumLength(ProductDescInfo, value);
					temporaryProductDesc = value;
					SetValueToInventoryView(WhsInventoryView.Schema.WI_OP_Desc, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateProductDesc();
					}
					ProductDescInfo.RefreshBinding();
				}
			}
		}

		ZString temporaryProductDesc;

		public virtual ZPropertyInfo ProductDescInfo
		{
			get { return GetZPropertyInfo(Schema.ProductDesc); }
		}

		#endregion

		#region ProductUQ

		[ReadOnlyMember(nameof(TempProductReadOnly))]
		[List("Lookups.PackTypesWithStandardUnits")]
		[MaxLength(OrgSupplierPart.Schema.OP_StockKeepingUnitMaxLength)]
		[ResourceStringData("WhsDocketLine|ProductUQ", Caption = "UQ")]
		public ZString ProductUQ
		{
			get
			{
				var part = this.SupplierPart;
				return part != null ? part.OP_StockKeepingUnit : temporaryProductUQ;
			}
			set
			{
				if (temporaryProductUQ != value)
				{
					CheckMaximumLength(ProductUQInfo, value);
					temporaryProductUQ = value;
					SetValueToInventoryView(WhsInventoryView.Schema.WI_UnitsUQ, value);
					CalcWE_PackQtyFromWE_TransactionQuantity();
					if (!IsValidationSuspended)
					{
						Validation.ValidateProductUQ();
					}
					ProductUQInfo.RefreshBinding();
				}
			}
		}

		ZString temporaryProductUQ = Constants.PkgUnit.Unit;

		public ZPropertyInfo ProductUQInfo
		{
			get { return GetZPropertyInfo(Schema.ProductUQ); }
		}

		#endregion

		#region CommodityCode

		[ReadOnlyMember(nameof(TempProductReadOnly))]
		[List("SupplierPart.Lookups.CommodityCodes")]
		[MaxLength(RefCommodityCode.Schema.RH_CodeMaxLength)]
		[ResourceStringData("WhsDocketLine|CommodityCode", Caption = "Commodity")]
		public ZString CommodityCode
		{
			get
			{
				var part = this.SupplierPart;
				return part != null ? part.OP_RH_NKCommodityCode : temporaryCommodityCode;
			}
			set
			{
				if (temporaryCommodityCode != value)
				{
					CheckMaximumLength(CommodityCodeInfo, value);
					temporaryCommodityCode = value;
					SetValueToInventoryView(WhsInventoryView.Schema.CommodityCode, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateCommodityCode();
					}
					CommodityCodeInfo.RefreshBinding();
				}
			}
		}

		ZString temporaryCommodityCode;

		public ZPropertyInfo CommodityCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CommodityCode); }
		}

		#endregion

		#region LocationAreaName

		[ResourceStringData("WhsDocketLine|LocationAreaName", Caption = "Picking Location Area")]
		public ZString LocationAreaName => LocationArea?.WA_NameMultilingual ?? ZString.Empty;

		#endregion

		#region LocationAreaType

		[ResourceStringData("WhsDocketLine|LocationAreaType", Caption = "Picking Area Type")]
		public ZString LocationAreaType => Location?.WLV_PickingAreaType ?? ZString.Empty;

		#endregion

		#region ReservedQuantity

		[ResourceStringData("WhsDocketLine|ReservedQuantity", Caption = "Reserved Quantity")]
		public ZDecimal ReservedQuantity
		{
			get { return ReservedPickLines.GetQtyCommitted(); }
		}

		public ZPropertyInfo ReservedQuantityInfo => GetZPropertyInfo(Schema.ReservedQuantity);

		#endregion

		#region AvailableToPickQuantity

		/// <summary>
		/// Quantity that is Available as Finalised Stock. This does not include
		/// Inventory that is Damaged or Held, as such inventory cannot be picked.
		/// </summary>
		public ZDecimal AvailableToPickQuantity
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_AvailableToPickQuantity : ZDecimal.Zero; }
		}

		public ZPropertyInfo AvailableToPickQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.AvailableToPickQuantity); }
		}

		#endregion

		#region AvailableToTransferQuantity

		public ZDecimal AvailableToTransferQuantity
		{
			get { return Inventory.Count > 0 ? Inventory[0].WI_AvailableToTransferQuantity : ZDecimal.Zero; }
		}

		public ZPropertyInfo AvailableToTransferQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.AvailableToTransferQuantity); }
		}

		#endregion

		#region ReceiptReference

		[ResourceStringData("WhsDocketLine|ReceiptReference", Caption = "Receipt Reference")]
		public ZString ReceiptReference
		{
			get
			{
				var docketOriginal = DocketOriginal;
				return docketOriginal != null ? docketOriginal.WD_ExternalReference : ZString.Empty;
			}
		}

		public ZPropertyInfo ReceiptReferenceInfo
		{
			get { return GetZPropertyInfo(Schema.ReceiptReference); }
		}

		#endregion

		#region ClientPK

		[List("Lookups.Clients")]
		[ResourceStringData("WhsDocketLine|ClientPK", Caption = "Client")]
		public ZGuid ClientPK
		{
			get
			{
				var docket = Docket;
				return docket != null ? docket.WD_OH_Client : ZGuid.Empty;
			}
		}

		#endregion

		#region OriginalInventoryStatusDescription

		[ResourceStringData("WhsDocketLine|OriginalInventoryStatusDescription", Caption = "Inventory Status Description", MediumCaption = "Inventory Status Desc.", ShortCaption = "Status Desc.")]
		public ZString OriginalInventoryStatusDescription
		{
			get { return !WE_OriginalInventoryStatus.IsEmpty ? Lookups.InventoryStatuses.GetDescriptionFromCode(WE_OriginalInventoryStatus) : ""; }
		}

		#endregion

		#endregion

		#region SetValueToInventoryView

		protected void SetValueToInventoryView(ZString propertyName, IZType newValue)
		{
			if (Inventory.Count > 0 && !Inventory[0][propertyName].Equals(newValue))
			{
				var semaphore = GetSyncingDocketLineInventoryViewSemaphore(propertyName);
				if (!semaphore.IsSuspended)
				{
					using (new SemaphoreManager(semaphore))
					{
						Inventory[0][propertyName] = newValue;
					}
				}
			}
		}

		#endregion

		#region GetSyncingDocketLineInventoryViewSemaphore

		internal Semaphore GetSyncingDocketLineInventoryViewSemaphore(ZString propertyName)
		{
			Semaphore semaphore;
			syncingDocketLineInventoryViewSemaphores = syncingDocketLineInventoryViewSemaphores ?? new Dictionary<ZString, Semaphore>();
			if (!syncingDocketLineInventoryViewSemaphores.TryGetValue(propertyName, out semaphore))
			{
				semaphore = new Semaphore();
				syncingDocketLineInventoryViewSemaphores.Add(propertyName, semaphore);
			}

			return semaphore;
		}

		Dictionary<ZString, Semaphore> syncingDocketLineInventoryViewSemaphores;

		#endregion

		#region Property Infos

		[ActionField(ReadOnly = true)]
		public override ZString WE_DocketLineStatus
		{
			get { return base.WE_DocketLineStatus; }
			set { base.WE_DocketLineStatus = value; }
		}

		#region CustomAttributes

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZString WE_CustomAttrib1
		{
			get { return base.WE_CustomAttrib1; }
			set { base.WE_CustomAttrib1 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZString WE_CustomAttrib2
		{
			get { return base.WE_CustomAttrib2; }
			set { base.WE_CustomAttrib2 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZString WE_CustomAttrib3
		{
			get { return base.WE_CustomAttrib3; }
			set { base.WE_CustomAttrib3 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZString WE_CustomAttrib4
		{
			get { return base.WE_CustomAttrib4; }
			set { base.WE_CustomAttrib4 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZString WE_CustomAttrib5
		{
			get { return base.WE_CustomAttrib5; }
			set { base.WE_CustomAttrib5 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZString WE_CustomAttrib6
		{
			get { return base.WE_CustomAttrib6; }
			set { base.WE_CustomAttrib6 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZDecimal WE_CustomDecimal1
		{
			get { return base.WE_CustomDecimal1; }
			set { base.WE_CustomDecimal1 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZDecimal WE_CustomDecimal2
		{
			get { return base.WE_CustomDecimal2; }
			set { base.WE_CustomDecimal2 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZDecimal WE_CustomDecimal3
		{
			get { return base.WE_CustomDecimal3; }
			set { base.WE_CustomDecimal3 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZDecimal WE_CustomDecimal4
		{
			get { return base.WE_CustomDecimal4; }
			set { base.WE_CustomDecimal4 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZDecimal WE_CustomDecimal5
		{
			get { return base.WE_CustomDecimal5; }
			set { base.WE_CustomDecimal5 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZDateTime WE_CustomDate1
		{
			get { return base.WE_CustomDate1; }
			set { base.WE_CustomDate1 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZDateTime WE_CustomDate2
		{
			get { return base.WE_CustomDate2; }
			set { base.WE_CustomDate2 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZDateTime WE_CustomDate3
		{
			get { return base.WE_CustomDate3; }
			set { base.WE_CustomDate3 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZDateTime WE_CustomDate4
		{
			get { return base.WE_CustomDate4; }
			set { base.WE_CustomDate4 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZDateTime WE_CustomDate5
		{
			get { return base.WE_CustomDate5; }
			set { base.WE_CustomDate5 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZBool WE_CustomFlag1
		{
			get { return base.WE_CustomFlag1; }
			set { base.WE_CustomFlag1 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZBool WE_CustomFlag2
		{
			get { return base.WE_CustomFlag2; }
			set { base.WE_CustomFlag2 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZBool WE_CustomFlag3
		{
			get { return base.WE_CustomFlag3; }
			set { base.WE_CustomFlag3 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZBool WE_CustomFlag4
		{
			get { return base.WE_CustomFlag4; }
			set { base.WE_CustomFlag4 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZBool WE_CustomFlag5
		{
			get { return base.WE_CustomFlag5; }
			set { base.WE_CustomFlag5 = value; }
		}

		[ReadOnlyMember(nameof(CustomAttribInfoReadOnly))]
		public override ZString WE_CustomTextBlob1
		{
			get { return base.WE_CustomTextBlob1; }
			set { base.WE_CustomTextBlob1 = value; }
		}

		#endregion

		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WE_FinalisedDate
		{
			get { return base.WE_FinalisedDate; }
			set { base.WE_FinalisedDate = value.IsValid ? ZDateTimeOffset.TruncateMilliseconds(value) : value; }
		}

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZString WE_LineComment
		{
			get { return base.WE_LineComment; }
			set { base.WE_LineComment = value; }
		}

		public virtual ZPropertyInfo WE_PackQuantityInfo
		{
			get { return GetZPropertyInfo(Schema.WE_PackQuantity); }
		}

		[ReadOnly(true)]
		public override ZShort WE_LineNo
		{
			get { return base.WE_LineNo; }
			set
			{
				var docket = Docket;
				if (WE_LineNo != value && docket != null)
				{
					docket.MaxLineNoManager.LineNoDeleted(WE_LineNo);
				}

				base.WE_LineNo = value;

				if (docket != null)
				{
					docket.MaxLineNoManager.LineNoSet(WE_LineNo);
				}
			}
		}

		[ReadOnly(true)]
		public override ZShort WE_SubLineNo
		{
			get { return base.WE_SubLineNo; }
			set { base.WE_SubLineNo = value; }
		}

		[ActionField(ReadOnly = true)]
		[ReadOnly(true)]
		public override ZDecimal WE_StockOnHand
		{
			get { return base.WE_StockOnHand; }
			set
			{
				base.WE_StockOnHand = value;
				SetValueToInventoryView(WhsInventoryViewSchema.WI_TotalUnits.Name, value);
			}
		}

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZDecimal WE_UnitDiscountAmount
		{
			get { return base.WE_UnitDiscountAmount; }
			set { base.WE_UnitDiscountAmount = value; }
		}

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZDecimal WE_UnitDiscountPercent
		{
			get { return base.WE_UnitDiscountPercent; }
			set { base.WE_UnitDiscountPercent = value; }
		}

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZDecimal WE_UnitPriceAfterDiscount
		{
			get { return base.WE_UnitPriceAfterDiscount; }
			set { base.WE_UnitPriceAfterDiscount = value; }
		}

		[ReadOnlyMember(nameof(AutoCalculatePriceReadOnly))]
		public override ZDecimal WE_ExtendedLinePrice
		{
			get { return base.WE_ExtendedLinePrice; }
			set
			{
				if (Docket != null)
				{
					var originalOrderLineTotal = Docket.GetTotalOrderLineValue();
					base.WE_ExtendedLinePrice = value;

					if (WE_RX_NKUnitPriceCurrency.IsEmpty)
					{
						WE_RX_NKUnitPriceCurrency = Docket.GetMostCommonOrderCurrency();
					}

					Docket.UpdateTotalOrderValue(originalOrderLineTotal);
				}
				else
				{
					base.WE_ExtendedLinePrice = value;
				}
			}
		}

		[ReadOnlyMember(nameof(StandardReadOnly))]
		public override ZDecimal WE_RecommendedUnitPrice
		{
			get { return base.WE_RecommendedUnitPrice; }
			set { base.WE_RecommendedUnitPrice = value; }
		}

		[ReadOnlyMember(nameof(StandardReadOnly))]
		[List("Lookups.UnitPriceCurrencies")]
		public override ZString WE_RX_NKUnitPriceCurrency
		{
			get { return base.WE_RX_NKUnitPriceCurrency; }
			set { base.WE_RX_NKUnitPriceCurrency = value; }
		}

		[ActionField(ReadOnly = true)]
		public override ZString WE_TransferFromPalletId
		{
			get { return base.WE_TransferFromPalletId; }
			set { base.WE_TransferFromPalletId = value; }
		}

		[ActionField(ReadOnly = true)]
		public override ZString WE_ReceiveCrossDockOrderNo
		{
			get { return base.WE_ReceiveCrossDockOrderNo; }
			set { base.WE_ReceiveCrossDockOrderNo = value; }
		}

		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WE_RequiredByDate
		{
			get { return base.WE_RequiredByDate; }
			set { base.WE_RequiredByDate = value; }
		}

		[ReadOnly(true)]
		public override ZGuid WE_WPL_PutawayLine
		{
			get { return base.WE_WPL_PutawayLine; }
			set { base.WE_WPL_PutawayLine = value; }
		}

		#region WE_GS_NKPutawayBy

		[ReadOnlyMember(nameof(IsTransferredReadOnly))]
		public override ZString WE_GS_NKPutawayBy
		{
			get { return base.WE_GS_NKPutawayBy; }
			set { base.WE_GS_NKPutawayBy = value; }
		}

		#endregion

		#region ReadOnly

		protected virtual bool IsJobFinalisedOrCancelledReadOnly
		{
			get { return IsDocketFinalisedOrCancelled; }
		}

		protected virtual bool StandardReadOnly
		{
			get { return IsJobFinalisedOrCancelledReadOnly; }
		}

		protected virtual bool AutoCalculatePriceReadOnly
		{
			get { return StandardReadOnly; }
		}

		protected virtual bool BondedEntryKeyReadOnly
		{
			get { return StandardReadOnly || !IsCustomsTransaction; }
		}

		protected bool CustomAttribInfoReadOnly
		{
			get { return WE_OP.IsEmpty || AfterPickProductAndAttribsReadOnly; }
		}

		protected virtual bool DestLocationReadOnly
		{
			get { return IsJobFinalisedOrCancelledReadOnly; }
		}

		protected virtual bool DestPalletIDReadOnly
		{
			get { return IsJobFinalisedOrCancelledReadOnly; }
		}

		protected virtual bool IsTransferredReadOnly
		{
			get { return IsJobFinalisedOrCancelledReadOnly; }
		}

		protected virtual bool IsOrderedHoldCodeReadOnly
		{
			get { return true; }
		}

		#region PartAttribsReadOnly

		protected virtual bool CommonPartAttribReadOnly => WE_WD.IsEmpty || WE_OP.IsEmpty || Product == null || AfterPickProductAndAttribsReadOnly;

		protected virtual bool PartAttrib1InfoReadOnly
		{
			get
			{
				var docket = Docket;
				return CommonPartAttribReadOnly || docket == null || !Product.IsPartAttributeUsed(docket.Client, 1);
			}
		}

		protected virtual bool PartAttrib2InfoReadOnly
		{
			get
			{
				var docket = Docket;
				return CommonPartAttribReadOnly || docket == null || !Product.IsPartAttributeUsed(docket.Client, 2);
			}
		}

		protected virtual bool PartAttrib3InfoReadOnly
		{
			get
			{
				var docket = Docket;
				return CommonPartAttribReadOnly || docket == null || !Product.IsPartAttributeUsed(docket.Client, 3);
			}
		}

		protected virtual bool SerialNumberReadOnly
		{
			get
			{
				var docket = Docket;
				return CommonPartAttribReadOnly || docket == null || !Product.IsSerialNumberUsed(docket.Client);
			}
		}

		protected virtual bool ExpiryDateReadOnly
		{
			get
			{
				var docket = Docket;
				var client = docket?.Client;
				return CommonPartAttribReadOnly || docket == null || client == null || !Product.IsExpiryDateUsed(client) || Product.IsAJulianBatchNumberAttributeUsed(client);
			}
		}

		protected virtual bool PackingDateReadOnly
		{
			get
			{
				var docket = Docket;
				var client = docket?.Client;
				return CommonPartAttribReadOnly || docket == null || client == null || !Product.IsPackingDateUsed(client) || Product.IsAJulianBatchNumberAttributeUsed(client);
			}
		}

		#endregion

		#region Product / Attribs / Quantity ReadOnly

		protected virtual bool PackQuantityAndTypeAndRelatedQuantityInfoReadOnly => StandardReadOnly;

		protected virtual bool AfterPickProductAndAttribsReadOnly => StandardReadOnly;

		protected virtual bool ProductReadOnly => StandardReadOnly;

		protected virtual bool TransactionQtyReadOnly => PackQuantityAndTypeAndRelatedQuantityInfoReadOnly;

		#endregion

		#region TempProductReadOnly

		protected virtual bool TempProductReadOnly
		{
			get { return true; }
		}

		#endregion

		#region InventoryEditFormReadOnly

		protected bool InventoryEditFormReadOnly
		{
			get { return !IsInventoryEditForm || !IsFinalised; }
		}

		public bool IsInventoryEditForm
		{
			get;
			set;
		}

		#endregion

		#endregion

		#endregion

		#region SynchroniseOffsetsToWarehouseTime

		public void SynchroniseOffsetsToWarehouseTime(ITimeZone calculationTimeZone)
		{
			SynchroniseOffsetsToWarehouseTimeCore(calculationTimeZone);
		}

		protected virtual void SynchroniseOffsetsToWarehouseTimeCore(ITimeZone calculationTimeZone)
		{
			WE_AdjustmentArrivalDateInfo.ConvertOffsetPropertyToTimeZone(calculationTimeZone);
		}

		#endregion

		#region ExpiryDateShouldBeInFuturePastYearsBeforeWarning

		protected virtual bool ShouldDisplayWarningForPastExpiryDates => false;

		#endregion

		#endregion

		#region Validation

		#region ShouldSuppressDateRangeValidationErrors

		protected virtual bool ShouldSuppressDateRangeValidationErrors => false;

		#endregion

		protected virtual void ValidateUnitsProperty()
		{
			Validation.ValidateWE_TransactionQuantity();
		}

		public TypeValidationLimits GetExpectedExpiryDateValidationRange()
		{
			var limits = new TypeValidationLimits { FutureYearsBeforeError = DateRangeValidation.MaximumFutureYears };

			if (ShouldDisplayWarningForPastExpiryDates)
			{
				limits.PastYearsBeforeWarning = 0;
			}

			if (ShouldSuppressDateRangeValidationErrors)
			{
				limits.PastYearsBeforeError = DateRangeValidation.MaximumPastYears;
			}

			return limits;
		}

		public TypeValidationLimits GetExpectedPackingDateValidationRange()
		{
			var limits = new TypeValidationLimits();

			if (ShouldSuppressDateRangeValidationErrors)
			{
				limits.PastYearsBeforeError = DateRangeValidation.MaximumPastYears;
			}

			return limits;
		}

		#endregion

		#region Location

		#region Warehouse

		public WhsWarehouse Warehouse
		{
			get { return Factory.Load<WhsWarehouse>(WarehousePK); }
		}

		#endregion

		#region WarehousePK

		[List("Lookups.Warehouses")]
		[ResourceStringData("WhsDocketLine|WarehousePK", Caption = "Warehouse")]
		public ZGuid WarehousePK
		{
			get
			{
				ZGuid result;

				var docket = Docket;
				if (docket != null)
				{
					result = GetWarehousePK(docket);
				}
				else
				{
					result = ZGuid.Empty;
				}

				return result;
			}
		}

		protected virtual ZGuid GetWarehousePK(WhsDocket docket) => docket.WD_WW_Whs;

		#endregion

		#region Location

		public WhsLocation Location
		{
			get { return Factory.Load<WhsLocation>(WE_WL); }
		}

		#endregion

		#region WE_WL

		[ReadOnlyMember(nameof(DestLocationReadOnly))]
		[RelatedBusinessObject("Location")]
		public override ZGuid WE_WL
		{
			get { return base.WE_WL; }
			set
			{
				SetValueToInventoryView(WhsInventoryViewSchema.WI_WL.Name, value);
				var oldValue = base.WE_WL;
				base.WE_WL = value;

				if (oldValue != value)
				{
					Docket?.LocationCapacityValidationManager.ClearLocationRequiredCapacity();
				}
			}
		}

		#endregion

		#region LocationString

		[ReadOnlyMember(nameof(DestLocationReadOnly))]
		[List("Lookups.Locations")]
		[MaxLength(Schema.LocationStringMaxLength)]
		[ResourceStringData("WhsDocketLine|LocationString", Caption = "Location")]
		public ZString LocationString
		{
			get { return LocationStringCore; }
			set { LocationStringCore = value; }
		}

		protected virtual ZString LocationStringCore
		{
			get
			{
				var location = Location;
				return location != null ? location.WLV_LocationString_UserFriendly : locationString;
			}
			set
			{
				SetNonPersistentPropertyValue(LocationStringInfo, ref locationString, value);
				WE_WL = GetLocationPK(Warehouse, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateLocationString();
				}

				LocationStringInfo.RefreshBinding();
			}
		}

		ZString locationString;

		#region CurrentLocation

		[ResourceStringData("WhsDocketLine|CurrentLocationString", Caption = "Location")]
		public ZString CurrentLocationString
		{
			get { return CurrentLocation?.WLV_LocationString_UserFriendly ?? ZString.Empty; }
		}

		public WhsLocation CurrentLocation
		{
			get { return Factory.Load<WhsLocation>(CurrentLocationPK); }
		}

		[ResourceStringData("WhsDocketLine|CurrentLocationPickAreaName", Caption = "Location Pick Area")]
		public ZString CurrentLocationPickAreaName
		{
			get { return CurrentLocationPickArea?.WA_NameMultilingual ?? ZString.Empty; }
		}

		[ResourceStringData("WhsDocketLine|CurrentLocationPickAreaType", Caption = "Pick Area Type")]
		public ZString CurrentLocationPickAreaType
		{
			get { return CurrentLocationPickArea?.WA_AreaType ?? ZString.Empty; }
		}

		[ResourceStringData("WhsDocketLine|CurrentLocationStatus", Caption = "Location Status")]
		public ZString CurrentLocationStatus
		{
			get
			{
				var currentLocation = CurrentLocation;
				return (currentLocation != null) ? currentLocation.LocationStatuses.GetDescriptionFromCode(currentLocation.WLV_LocationStatus) : "";
			}
		}

		[ResourceStringData("WhsDocketLine|CurrentLocationClass", Caption = "Location Class", ShortCaption = "Loc. Class")]
		public ZString CurrentLocationClass => CurrentLocation?.WLV_LocationClass ?? ZString.Empty;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required for reflection components")]
		bool IsStockInDockDoor => WE_OriginalInventoryStatus == InventoryStatus.Codes.Received;

		ZGuid CurrentLocationPK
		{
			get
			{
				switch (WE_OriginalInventoryStatus)
				{
					case InventoryStatus.Codes.InTransit:
					case InventoryStatus.Codes.PuttingAway:
						return ZGuid.Empty;

					default:
						return WE_WL;
				}
			}
		}

		WhsArea CurrentLocationPickArea => Factory.Load<WhsLocation>(CurrentLocationPK)?.PickingArea;

		#endregion

		protected static ZGuid GetLocationPK(WhsWarehouse whs, ZString docketLineLocationString)
		{
			if (docketLineLocationString.IsEmpty || whs == null)
			{
				return ZGuid.Empty;
			}

			return whs.FindLocation(docketLineLocationString)?.PK ?? ZGuid.Invalid;
		}

		#endregion

		#region LocationStringInfo

		public ZPropertyInfo LocationStringInfo
		{
			get { return GetZPropertyInfo(Schema.LocationString); }
		}

		#endregion

		#region UpdateLocationsLastAllocatedOrChangedDate

		void UpdateLocationsLastAllocatedOrChangedDate(ZDateTime date, ZGuid locationPK)
		{
			if (locationPK.IsValid)
			{
				var location = Factory.Load<WhsLocation>(locationPK);
				location.UpdateWLV_LastAllocatedOrChangedDateUtc(date);
			}
		}

		#endregion

		#endregion

		#region Find Attributes

		public void UseChosenInventoryRowFindAttributes(WhsInventoryView inventory)
		{
			Argument.NotNull(inventory, "inventory");

			if (CanUpdateFromInventory)
			{
				UseChosenInventoryRowFindAttributesCore(inventory);
			}
			else
			{
				Docket.NotificationSubscriber.Notify(new ErrorNotification(CannotUseChosenInventoryError));
			}
		}

		protected virtual void UseChosenInventoryRowFindAttributesCore(WhsInventoryView inventory)
		{
			WE_OP = inventory.WI_OP;
			SetAttributes(inventory);
			SetCustomAttributes(inventory);
		}

		protected virtual ErrorType CannotUseChosenInventoryError
		{
			get { return WhsErrorTypes.CannotPerformThisOperationBecauseJobIsFinalisedOrCancelled; }
		}

		#endregion

		#region Why Is This Inventory Committed

		public ZString WhyIsThisInventoryCommitted()
		{
			var result = ZString.Empty;

			if (CommittedQuantityIncludingUnfinalisedReceipt == 0m)
			{
				result = Res.GetString("c64607c1-b1a4-4518-a12a-875d843df034", "This inventory item does not have any committed units.");
			}
			else
			{
				var docket = Docket;
				if (docket != null)
				{
					if (!IsFinalised && !docket.IsCancelled
						&& docket.WD_WP_ParentPickForTransfer.IsEmpty) // Tested in WhsTransferLine
					{
						result = Res.GetString("9f83f6dc-6ba7-484f-ade6-676ab983fe4d", "This inventory item has committed units because Receive Goods Job: {0} is not finalized", docket.WD_DocketID);
					}
					else if (PickAllocations.Count > 0 || TransferAllocations.Length > 0 || AdjustmentAllocations.Length > 0)
					{
						var stringBuilder = new ZStringBuilder();
						stringBuilder.Append(Res.GetString("08dd2b45-a092-461e-b058-b40d060e2551", "This inventory item has units committed to:"));

						if (PickAllocations.Count > 0)
						{
							stringBuilder.Append(Res.GetString("22345e3b-4861-4972-b809-2df17615b63b", "Pick(s): {0}.", PickAllocationsAsString));
						}
						if (TransferAllocations.Length > 0)
						{
							stringBuilder.Append(Res.GetString("5d8af131-370f-40b5-a9eb-f008f8c3db59", "Transfer(s): {0}.", TransferAllocationsAsString));
						}
						if (AdjustmentAllocations.Length > 0)
						{
							stringBuilder.Append(Res.GetString("d56554d1-b209-4d2e-819a-92f50b113cac", "Adjustment(s): {0}.", AdjustmentAllocationsAsString));
						}

						result = stringBuilder.ToStringWithNewLineBetweenAppends();
					}
				}
			}

			if (result.IsEmpty)
			{
				result = Res.GetString("2b70f854-7277-463d-8073-4c142f6ec0e5", "The system cannot determine why this item has committed units. Please contact support.");
			}

			return result;
		}

		#region PickAllocationsAsString

		public ZString PickAllocationsAsString
		{
			get { return string.Join(", ", PickAllocations.Select(p => p.WP_PickNo)); }
		}

		public IReadOnlyCollection<WhsPick> PickAllocations
		{
			get { return pickAllocations ?? (pickAllocations = Factory.Load<WhsPick>(CommittedToPickLineQuery)); }
		}

		WhsPick[] pickAllocations;

		ZQuery CommittedToPickLineQuery
		{
			get
			{
				var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
				pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_WE_InventoryLine, PK);
				pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_Units, SQLComparisonOperator.GreaterThan, 0m);
				pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);

				var orderLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
				orderLineSubQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

				var orderSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
				orderSubQuery.AddSubQuery(orderLineSubQuery, JoinCondition.And);

				var pickQuery = new ZQuery();
				pickQuery.AddToFilter(WhsPickSchema.WP_PickStatus, SQLComparisonOperator.NotEqual, new[] { PickStatus.Codes.Cancelled, PickStatus.Codes.Finalised });

				var query = new ZDBOnlyQuery(typeof(WhsPick));
				query.AddToFilter(pickQuery);
				query.AddSubQuery(orderSubQuery, JoinCondition.And);
				query.OrderBy = WhsPickSchema.WP_PickNo.Name;

				return query;
			}
		}

		#region Test
#if DEBUG
		public void ClearPickAllocationsCache()
		{
			pickAllocations = null;
		}
#endif
		#endregion

		#endregion

		#region TransferAllocations

		WhsTransfer[] TransferAllocations
		{
			get { return transferAllocations ?? (transferAllocations = Factory.Load<WhsTransfer>(GetAllocationTransfersFilter())); }
		}

		WhsTransfer[] transferAllocations;

		ZQuery GetAllocationTransfersFilter()
		{
			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_WE_InventoryLine, PK);

			var transferLineSubQuery = new ZDBOnlySubQuery(typeof(WhsTransferLine), WhsDocketLineSchema.WE_WD);
			transferLineSubQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketLineStatus.Codes.Finalised);
			transferLineSubQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			var transferQuery = new ZDBOnlyQuery(typeof(WhsTransfer));
			transferQuery.AddToFilter(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Transfer);
			transferQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, new[] { DocketStatus.Codes.Cancelled, DocketStatus.Codes.Finalised });
			transferQuery.AddSubQuery(transferLineSubQuery, JoinCondition.And);
			transferQuery.OrderBy = WhsDocketSchema.Constants.WD_DocketID;

			return transferQuery;
		}

		ZString TransferAllocationsAsString
		{
			get { return string.Join(", ", TransferAllocations.Select(t => t.WD_DocketID)); }
		}

		#endregion

		#region AdjustmentAllocationsAsString

		ZString AdjustmentAllocationsAsString
		{
			get { return string.Join(", ", AdjustmentAllocations.Select(a => a.WD_DocketID)); }
		}

		WhsAdjustment[] AdjustmentAllocations
		{
			get { return adjustmentAllocations ?? (adjustmentAllocations = Factory.Load<WhsAdjustment>(GetAdjustmentAllocationsFilter())); }
		}

		WhsAdjustment[] adjustmentAllocations;

		ZQuery GetAdjustmentAllocationsFilter()
		{
			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_WE_InventoryLine, PK);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_PickedDateTime, null);

			var adjusmentLineSubQuery = new ZDBOnlySubQuery(typeof(WhsAdjustmentLine), WhsDocketLineSchema.WE_WD);
			adjusmentLineSubQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			var adjustmentQuery = new ZDBOnlyQuery(typeof(WhsAdjustment));
			adjustmentQuery.AddToFilter(WhsDocketSchema.WD_DocketType, CodeLists.DocketType.Codes.Adjustment);
			adjustmentQuery.AddToFilter(WhsDocketSchema.WD_DocketStatus, SQLComparisonOperator.NotEqual, new[] { DocketStatus.Codes.Cancelled, DocketStatus.Codes.Finalised });
			adjustmentQuery.AddSubQuery(adjusmentLineSubQuery, JoinCondition.And);
			adjustmentQuery.OrderBy = WhsDocketSchema.Constants.WD_DocketID;

			return adjustmentQuery;
		}

		#endregion

		#endregion

		#region Cancel / Reactivate Line

		#region CancelLine

		internal void CancelLine() => CancelLineCore();

		protected virtual void CancelLineCore()
		{
		}

		#endregion

		#region ReactivateLine

		internal void ReactivateLine() => ReactivateLineCore();

		protected virtual void ReactivateLineCore()
		{
		}

		#endregion

		#endregion

		#region ILineAttributes Members

		ZDate IPartAttributes.ExpiryDate => WE_ExpiryDate;
		ZDate IPartAttributes.PackingDate => WE_PackingDate;
		ZString IPartAttributes.PartAttrib1 => WE_PartAttrib1;
		ZString IPartAttributes.PartAttrib2 => WE_PartAttrib2;
		ZString IPartAttributes.PartAttrib3 => WE_PartAttrib3;
		ZString IPartAttributes.SerialNumber => WE_SerialNumber;
		ZString ILineAttributes.BondedEntryKey => WE_BondedEntryKey;
		ZString ILineAttributes.AllocationKey => WE_AllocationKey;

		public void SetAttributes(ILineAttributes a)
		{
			WE_BondedEntryKey = a.BondedEntryKey;
			WE_AllocationKey = a.AllocationKey;
			WE_ExpiryDate = a.ExpiryDate;
			WE_PackingDate = a.PackingDate;
			WE_PartAttrib1 = a.PartAttrib1;
			WE_PartAttrib2 = a.PartAttrib2;
			WE_PartAttrib3 = a.PartAttrib3;
			WE_SerialNumber = a.SerialNumber;
		}

		#endregion

		#region ILineCustomAttributes Members

		ZString ILineCustomAttributes.CustomAttrib1 => WE_CustomAttrib1;

		ZString ILineCustomAttributes.CustomAttrib2 => WE_CustomAttrib2;

		ZString ILineCustomAttributes.CustomAttrib3 => WE_CustomAttrib3;

		ZString ILineCustomAttributes.CustomAttrib4 => WE_CustomAttrib4;

		ZString ILineCustomAttributes.CustomAttrib5 => WE_CustomAttrib5;

		ZString ILineCustomAttributes.CustomAttrib6 => WE_CustomAttrib6;

		ZDecimal ILineCustomAttributes.CustomDecimal1 => WE_CustomDecimal1;

		ZDecimal ILineCustomAttributes.CustomDecimal2 => WE_CustomDecimal2;

		ZDecimal ILineCustomAttributes.CustomDecimal3 => WE_CustomDecimal3;

		ZDecimal ILineCustomAttributes.CustomDecimal4 => WE_CustomDecimal4;

		ZDecimal ILineCustomAttributes.CustomDecimal5 => WE_CustomDecimal5;

		ZDateTime ILineCustomAttributes.CustomDate1 => WE_CustomDate1;

		ZDateTime ILineCustomAttributes.CustomDate2 => WE_CustomDate2;

		ZDateTime ILineCustomAttributes.CustomDate3 => WE_CustomDate3;

		ZDateTime ILineCustomAttributes.CustomDate4 => WE_CustomDate4;

		ZDateTime ILineCustomAttributes.CustomDate5 => WE_CustomDate5;

		ZBool ILineCustomAttributes.CustomFlag1 => WE_CustomFlag1;

		ZBool ILineCustomAttributes.CustomFlag2 => WE_CustomFlag2;

		ZBool ILineCustomAttributes.CustomFlag3 => WE_CustomFlag3;

		ZBool ILineCustomAttributes.CustomFlag4 => WE_CustomFlag4;

		ZBool ILineCustomAttributes.CustomFlag5 => WE_CustomFlag5;

		ZString ILineCustomAttributes.CustomTextBlob1 => WE_CustomTextBlob1;

		public void SetCustomAttributes(ILineCustomAttributes a)
		{
			WE_CustomAttrib1 = a.CustomAttrib1;
			WE_CustomAttrib2 = a.CustomAttrib2;
			WE_CustomAttrib3 = a.CustomAttrib3;
			WE_CustomAttrib4 = a.CustomAttrib4;
			WE_CustomAttrib5 = a.CustomAttrib5;
			WE_CustomAttrib6 = a.CustomAttrib6;

			WE_CustomDecimal1 = a.CustomDecimal1;
			WE_CustomDecimal2 = a.CustomDecimal2;
			WE_CustomDecimal3 = a.CustomDecimal3;
			WE_CustomDecimal4 = a.CustomDecimal4;
			WE_CustomDecimal5 = a.CustomDecimal5;

			WE_CustomDate1 = a.CustomDate1;
			WE_CustomDate2 = a.CustomDate2;
			WE_CustomDate3 = a.CustomDate3;
			WE_CustomDate4 = a.CustomDate4;
			WE_CustomDate5 = a.CustomDate5;

			WE_CustomFlag1 = a.CustomFlag1;
			WE_CustomFlag2 = a.CustomFlag2;
			WE_CustomFlag3 = a.CustomFlag3;
			WE_CustomFlag4 = a.CustomFlag4;
			WE_CustomFlag5 = a.CustomFlag5;

			WE_CustomTextBlob1 = a.CustomTextBlob1;
		}

		#endregion

		#region IPartAttributeValidationConsumer Members

		public virtual bool IsRegisteredForUniqueSerialNumberChecking
		{
			get { return false; }
		}

		public bool IsValidForUniqueSerialNumberChecking(ZString serialNumber)
		{
			bool result = false;
			if (!serialNumber.IsEmpty)
			{
				var docket = Docket;
				var client = docket?.Client;
				if (client != null && SupplierPart != null && !docket.IsFinalisedOrCancelled)
				{
					var product = Product;
					result = product.IsSerialNumberUsed(client) && !product.IsSerialNumberReleaseCaptured(client);
				}
			}
			return result;
		}

		public bool IsSerialNumberUsedOnThis(ZString serialNumber)
		{
			return WE_SerialNumber == serialNumber && IsValidForUniqueSerialNumberChecking(serialNumber);
		}

		public virtual bool IsSerialNumberUsedOnSiblings(ZString serialNumber)
		{
			bool result = false;
			if (Docket != null)
			{
				foreach (WhsDocketLine line in Docket.Lines)
				{
					if (line.PK != PK && line.IsRegisteredForUniqueSerialNumberChecking)
					{
						result = WarehouseDataRegistry.Instance.EnforceSerialUniquenessByProduct ?
								 (line.WE_OP == WE_OP && line.IsSerialNumberUsedOnThis(serialNumber)) :
								 line.IsSerialNumberUsedOnThis(serialNumber);

						if (result)
						{
							break;
						}
					}
				}
			}
			return result;
		}

		public bool IsInventoryAdjustedOutOnSiblings(WhsInventoryView inventory)
		{
			return IsInventoryAdjustedOutOnSiblingsCore(inventory);
		}

		protected virtual bool IsInventoryAdjustedOutOnSiblingsCore(WhsInventoryView inventory)
		{
			return false;
		}

		#endregion

		#region IProcessHandlingInfoProvider Members

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return GetProcessHandlingInfo(); }
		}

		protected virtual ProcessHandlingInfo GetProcessHandlingInfo()
		{
			return new WhsDocketLineProcessHandlingInfo(this);
		}

		#endregion

		#region ICustomLabelsConfigOrgProvider Members

		OrgHeader ICustomLabelsConfigOrgProvider.ConfigOrg
		{
			get { return ((ICustomLabelsConfigOrgProvider)Docket)?.ConfigOrg; }
		}

		event EventHandler ICustomLabelsConfigOrgProvider.ConfigOrgChanged
		{
			add
			{
				var docketLineConfigOrgProvider = ((ICustomLabelsConfigOrgProvider)Docket);
				if (docketLineConfigOrgProvider != null)
				{
					docketLineConfigOrgProvider.ConfigOrgChanged += value;
				}
			}

			remove
			{
				var docketLineConfigOrgProvider = ((ICustomLabelsConfigOrgProvider)Docket);
				if (docketLineConfigOrgProvider != null)
				{
					docketLineConfigOrgProvider.ConfigOrgChanged -= value;
				}
			}
		}

		#endregion

		#region ICustomLabelsProvider Members

		public class CustomLabelsProvider : ICustomLabelsProvider
		{
			public CustomLabelsProvider(ICustomLabelsConfigOrgProvider orgProvider)
			{
				ConfigOrgProvider = orgProvider;
			}

			public ICustomLabelsConfigOrgProvider ConfigOrgProvider { get; }

			public CustomLabelInfoList GetCustomFields(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				if (configOrg != null)
				{
					var key = string.Format(Culture.Invariant, "WhsDocketLine.CustomLabelsProvider:{0}", configOrg.PK.ToStringKey()); // Key used for Factory Cache
					return configOrg.Factory.GetCachedValue(key, () => GetCustomFieldsNoCache(configOrg, factory));
				}
				else
				{
					return GetCustomFieldsNoCache(configOrg, factory);
				}
			}

			CustomLabelInfoList GetCustomFieldsNoCache(OrgHeader configOrg, BusinessObjectFactory factory)
			{
				var result = new CustomLabelInfoList(typeof(WhsDocketLine), configOrg, ResString.GetMultilingualString("1ade788c-034c-435f-ada1-e5a9835b299b", "the client"), factory);
				result.Capacity = 22;
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute1, WhsDocketLineSchema.WE_CustomAttrib1.Name, Constants.CustomLabels.Descriptions.CustomAttribute(1));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute2, WhsDocketLineSchema.WE_CustomAttrib2.Name, Constants.CustomLabels.Descriptions.CustomAttribute(2));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute3, WhsDocketLineSchema.WE_CustomAttrib3.Name, Constants.CustomLabels.Descriptions.CustomAttribute(3));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute4, WhsDocketLineSchema.WE_CustomAttrib4.Name, Constants.CustomLabels.Descriptions.CustomAttribute(4));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute5, WhsDocketLineSchema.WE_CustomAttrib5.Name, Constants.CustomLabels.Descriptions.CustomAttribute(5));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomAttribute6, WhsDocketLineSchema.WE_CustomAttrib6.Name, Constants.CustomLabels.Descriptions.CustomAttribute(6));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate1, WhsDocketLineSchema.WE_CustomDate1.Name, Constants.CustomLabels.Descriptions.CustomDate(1));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate2, WhsDocketLineSchema.WE_CustomDate2.Name, Constants.CustomLabels.Descriptions.CustomDate(2));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate3, WhsDocketLineSchema.WE_CustomDate3.Name, Constants.CustomLabels.Descriptions.CustomDate(3));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate4, WhsDocketLineSchema.WE_CustomDate4.Name, Constants.CustomLabels.Descriptions.CustomDate(4));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDate5, WhsDocketLineSchema.WE_CustomDate5.Name, Constants.CustomLabels.Descriptions.CustomDate(5));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal1, WhsDocketLineSchema.WE_CustomDecimal1.Name, Constants.CustomLabels.Descriptions.CustomNumber(1));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal2, WhsDocketLineSchema.WE_CustomDecimal2.Name, Constants.CustomLabels.Descriptions.CustomNumber(2));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal3, WhsDocketLineSchema.WE_CustomDecimal3.Name, Constants.CustomLabels.Descriptions.CustomNumber(3));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal4, WhsDocketLineSchema.WE_CustomDecimal4.Name, Constants.CustomLabels.Descriptions.CustomNumber(4));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomDecimal5, WhsDocketLineSchema.WE_CustomDecimal5.Name, Constants.CustomLabels.Descriptions.CustomNumber(5));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag1, WhsDocketLineSchema.WE_CustomFlag1.Name, Constants.CustomLabels.Descriptions.CustomFlag(1));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag2, WhsDocketLineSchema.WE_CustomFlag2.Name, Constants.CustomLabels.Descriptions.CustomFlag(2));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag3, WhsDocketLineSchema.WE_CustomFlag3.Name, Constants.CustomLabels.Descriptions.CustomFlag(3));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag4, WhsDocketLineSchema.WE_CustomFlag4.Name, Constants.CustomLabels.Descriptions.CustomFlag(4));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomFlag5, WhsDocketLineSchema.WE_CustomFlag5.Name, Constants.CustomLabels.Descriptions.CustomFlag(5));
				result.Add(Constants.CustomLabels.WhsDocketLine.CustomTextBlob1, WhsDocketLineSchema.WE_CustomTextBlob1.Name, Constants.CustomLabels.Descriptions.CustomText(1));

				return result;
			}
		}

		#endregion

		#region ILocationCapacityLine Members

		ZDecimal ILocationCapacityLine.GetQuantity(WhsLocation location) => GetQuantityCore(location);

		protected virtual ZDecimal GetQuantityCore(WhsLocation location) => WE_TransactionQuantity;

		ZString ILocationCapacityLine.PalletID => WE_PalletID;

		ZGuid ILocationCapacityLine.ProductPK => WE_OP;

		#endregion

		#region ISupportDataImporting Members

		public bool IsImportingData
		{
			get { return IsImportingDataCore; }
			set { IsImportingDataCore = value; }
		}

		protected virtual bool IsImportingDataCore
		{
			get;
			set;
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter()
		{
			return new JobInvoicingEDocsProviderSupporter(this);
		}

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return new WhsInventoryDocumentSupporter(Inventory.Count > 0 ? this.Inventory[0] : Factory.GetNull<WhsInventoryView>()); }
		}

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Constants.DocManagerCodes.WarehouseInventory)); }
		}

		DocManagerInfo docManagerInfo;

		#endregion

		#region IsValidationEnabled

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo)
		{
			return (!IsFinalised || !propertyInfo.ReadOnly) && base.IsValidationEnabledCore(propertyInfo);
		}

		#endregion

		#region ICodeDescription Members

		object ICodeDescription.PK
		{
			get { return PK; }
		}

		string ICodeDescription.Code
		{
			get { return Code; }
		}

		public ZString Code
		{
			get { return "!FINDBOX!" + PK.ToString(); } // May be an identifier
		}

		string ICodeDescription.Description
		{
			get { return Description; }
		}

		public ZString Description
		{
			get { return "UNUSED"; } // UNUSED
		}

		#endregion
	}
}
