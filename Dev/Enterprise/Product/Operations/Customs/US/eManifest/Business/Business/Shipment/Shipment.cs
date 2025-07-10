using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.eManifest.Business
{
	[SystemDefinedValues]
	[UniversalDataContext(DataContextType.USeManifestShipment)]
	public class Shipment : AutoCusInBondBill
		, INoteSource
		, IWorkflowTriggerFieldChangeSource
		, ICusInBondCargoDescTypeProvider
		, Integration.Customs.US.eManifest.ICusInBondBill
	{
		public Shipment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusInBondBill.Schema
		{
			public const string B0_DescriptionOfCargo = "B0_DescriptionOfCargo";
			public const string B0_IsLodged = "B0_IsLodged";
			public const string B0_ReleaseStatusCodeDescription = "B0_ReleaseStatusCodeDescription";

			public const string HasMultipleItemLine = "HasMultipleItemLine";
			public const string FirstCommodityPieceCount = "FirstCommodityPieceCount";
			public const string FirstCommodityManifestUnitCode = "FirstCommodityManifestUnitCode";
			public const string FirstCommodityWeight = "FirstCommodityWeight";
			public const string FirstCommodityWeightUnit = "FirstCommodityWeightUnit";
			public const string FirstCommodityDescription = "FirstCommodityDescription";
			public const string FirstCommodityEquipment = "FirstCommodityEquipment";
			public const string FirstCommodityMarksAndNumbers = "FirstCommodityMarksAndNumbers";
			public const string FirstCommodityMonetaryValue = "FirstCommodityMonetaryValue";
			public const string FirstCommodityCountryOfOrigin = "FirstCommodityCountryOfOrigin";
			public const string FirstCommodityHarmonizedNumbers = "FirstCommodityHarmonizedNumbers";
			public const string FirstCommodityHazardousGoodsIdentifier = "FirstCommodityHazardousGoodsIdentifier";
			public const string FirstCommodityHazardousGoodsContact = "FirstCommodityHazardousGoodsContact";
			public const string FirstCommodityHazardousGoodsContactPhone = "FirstCommodityHazardousGoodsContactPhone";
			public const string FirstCommodityVehicleIdentificationNumbers = "FirstCommodityVehicleIdentificationNumbers";
			public const string FirstCommodityC4Codes = "FirstCommodityC4Codes";

			public const int FirstCommodityManifestUnitCodeMaxLength = Commodity.Schema.BY_ManifestUnitCodeMaxLength;
			public const int FirstCommodityDescriptionMaxLength = Commodity.Schema.BY_DescriptionMaxLength;
			public const int FirstCommodityMarksAndNumbersMaxLength = Commodity.Schema.BY_MarksAndNumbersMaxLength;
			public const int FirstCommodityCountryOfOriginMaxLength = Commodity.Schema.BY_RN_NKCountryOfOriginMaxLength;
			public const int FirstCommodityWeightUnitMaxLength = Commodity.Schema.BY_GrossWeightUnitMaxLength;
		}

		#endregion

		#region Properties

		#region B0_BH

		[RelatedBusinessObject("Trip")]
		public override ZGuid B0_BH
		{
			get { return base.B0_BH; }
			set { base.B0_BH = value; }
		}

		public Trip Trip
		{
			get { return Factory.Load<Trip>(B0_BH); }
		}

		public ZString ShipmentControlNumber
		{
			get { return B0_IssuerSCAC + B0_MasterBillNumber; }
		}

		#endregion

		#region B0_DescriptionOfCargo

		public ZString B0_DescriptionOfCargo
		{
			get { return DescriptionNoteManager.Value; }
			set
			{
				DescriptionNoteManager.Value = value;
				B0_DescriptionOfCargoInfo.RefreshBinding();
				if (!populateCommodityValueSuspended)
				{
					FirstCommodityDescription = value.Left(CusInBondCargoDesc.Schema.BY_DescriptionMaxLength);
				}
			}
		}

		public ZPropertyInfo B0_DescriptionOfCargoInfo
		{
			get { return GetZPropertyInfo(Schema.B0_DescriptionOfCargo); }
		}

		public int B0_DescriptionOfCargo_MaxLength
		{
			get { return PredefinedNoteTypes.Instance.DetailedGoodsDescription.TextOnlyMaxLength; }
		}

		ProxiedNotePropertyManager DescriptionNoteManager
		{
			get { return descriptionNoteManager ?? (descriptionNoteManager = new ProxiedNotePropertyManager(this, PredefinedNoteTypes.Instance.DetailedGoodsDescription)); }
		}

		ProxiedNotePropertyManager descriptionNoteManager;

		#endregion

		#region B0_Firms

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.FIRMSCodes))]
		public override ZString B0_Firms
		{
			get { return base.B0_Firms; }
			set { base.B0_Firms = value; }
		}

		#endregion

		#region B0_IsLodged

		public ZBool B0_IsLodged
		{
			get
			{
				return !B0_ReleaseStatus.IsEmpty
							 && B0_ReleaseStatus != EntryStatusList.Codes.Error
							 && B0_ReleaseStatus != EntryStatusList.Codes.Cancelled;
			}
			set
			{
				B0_ReleaseStatus = value ? (ZString)ShipmentEntryStatusList.Codes.LodgedWithOtherTrip : (ZString)B0_ReleaseStatusInfo.OriginalValue;
				if (!value && B0_IsLodged)
				{
					B0_ReleaseStatus = ZString.Empty;
				}

				B0_IsLodgedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo B0_IsLodgedInfo
		{
			get { return GetZPropertyInfo(Schema.B0_IsLodged); }
		}

		protected bool B0_IsLodged_ReadOnly
		{
			get { return B0_IsLodged && B0_ReleaseStatus != ShipmentEntryStatusList.Codes.LodgedWithOtherTrip; }
		}

		#endregion

		#region B0_ManifestUQ

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.QuantityUnits))]
		public override ZString B0_ManifestUQ
		{
			get { return base.B0_ManifestUQ; }
			set
			{
				base.B0_ManifestUQ = value;

				PopulateCommodityValueIfRequired(
					CusInBondCargoDescSchema.Constants.BY_ManifestUnitCode,
					value,
					AutoCusInBondCargoDesc.Schema.BY_ManifestUnitCodeMaxLength);
			}
		}

		#endregion

		#region B0_MasterBillNumber

		[ReadOnlyMember(Schema.B0_IsLodged)]
		public override ZString B0_MasterBillNumber
		{
			get { return base.B0_MasterBillNumber; }
			set { base.B0_MasterBillNumber = value; }
		}

		#endregion

		#region B0_RL_NKPortOfLading

		public override ZString B0_RL_NKPortOfLading
		{
			get { return base.B0_RL_NKPortOfLading; }
			set
			{
				var oldValue = B0_RL_NKPortOfLading;
				base.B0_RL_NKPortOfLading = value;
				if (!IsCopying && oldValue != B0_RL_NKPortOfLading)
				{
					B0_PortOfLadingKCode = USScheduleResolver.GetScheduleCode(Schedule.K, B0_RL_NKPortOfLading, USLocoMapSystemUsageList.Codes.SCK, Factory).Left(B0_PortOfLadingKCodeInfo.MaxLength);
				}
			}
		}

		#endregion

		#region B0_PortOfLadingKCode

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.ScheduleKPortCodes))]
		public override ZString B0_PortOfLadingKCode
		{
			get { return base.B0_PortOfLadingKCode; }
			set { base.B0_PortOfLadingKCode = value; }
		}

		#endregion

		#region B0_ReleaseStatus

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.ReleaseStatusList))]
		public override ZString B0_ReleaseStatus
		{
			get { return base.B0_ReleaseStatus; }
			set
			{
				var hasChanges = base.B0_ReleaseStatus != value;
				base.B0_ReleaseStatus = value;

				if (hasChanges && !IsCopying)
				{
					MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		public ZString B0_ReleaseStatusCodeDescription
		{
			get { return Lookups.ReleaseStatusList.GetCodeDescription(B0_ReleaseStatus); }
		}

		public bool IsLinked
		{
			get
			{
				return B0_IsLodged
							 && B0_ReleaseStatus != ShipmentEntryStatusList.Codes.Accepted
							 && B0_ReleaseStatus != ShipmentEntryStatusList.Codes.LodgedWithOtherTrip;
			}
		}

		#endregion

		#region B0_ReleaseStatusDate

		[ReadOnly(true)]
		public override ZDateTime B0_ReleaseStatusDate
		{
			get { return base.B0_ReleaseStatusDate; }
			set
			{
				var hasChanges = base.B0_ReleaseStatusDate != value;
				base.B0_ReleaseStatusDate = value;
				if ((hasChanges || B0_ReleaseStatusInfo.HasChanges) && !B0_ReleaseStatus.IsEmpty)
				{
					LogManager.AddStatusChangedLog(B0_ReleaseStatusCodeDescription, value.ToOffset());
				}
			}
		}

		ShipmentLogManager LogManager
		{
			get { return logManager ?? (logManager = new ShipmentLogManager(Logs)); }
		}

		ShipmentLogManager logManager;

		#endregion

		#region B0_ServiceType

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.ServiceTypes))]
		public override ZString B0_ServiceType
		{
			get { return base.B0_ServiceType; }
			set { base.B0_ServiceType = value; }
		}

		#endregion

		#region B0_ShipmentType

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.ShipmentTypes))]
		public override ZString B0_ShipmentType
		{
			get { return base.B0_ShipmentType; }

			set
			{
				var hasChanges = base.B0_ShipmentType != value;
				base.B0_ShipmentType = value;
				if (hasChanges && !IsCopying)
				{
					UpdateShipmentTypeDependentsAsNeeded();
					MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		public bool IsSplit
		{
			get { return B0_ShipmentType == ShipmentTypes.Codes.SplitShipment; }
		}

		#endregion

		#region B0_Volume

		[MeasureUnit(Schema.B0_VolumeUQ, MeasureUnitType.Volume)]
		public override ZDecimal B0_Volume
		{
			get { return base.B0_Volume; }
			set
			{
				var oldValue = B0_Volume;
				base.B0_Volume = value;
				if (!IsCopying && oldValue != B0_Volume && B0_VolumeUQ.IsEmpty)
				{
					B0_VolumeUQ = Constants.Volume.CubicMetres;
				}
			}
		}

		#endregion

		#region B0_VolumeUQ

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.VolumeUnits))]
		public override ZString B0_VolumeUQ
		{
			get { return base.B0_VolumeUQ; }
			set { base.B0_VolumeUQ = value; }
		}

		#endregion

		#region B0_Weight

		[MeasureUnit(Schema.B0_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal B0_Weight
		{
			get { return base.B0_Weight; }
			set
			{
				var oldValue = B0_Weight;
				base.B0_Weight = value;
				if (!IsCopying && oldValue != B0_Weight && B0_WeightUQ.IsEmpty)
				{
					B0_WeightUQ = Constants.Weight.Kilograms;
				}

				PopulateCommodityValueIfRequired(CusInBondCargoDescSchema.Constants.BY_GrossWeight, value);
			}
		}

		#endregion

		#region B0_WeightUQ

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.WeightUnits))]
		public override ZString B0_WeightUQ
		{
			get { return base.B0_WeightUQ; }
			set
			{
				base.B0_WeightUQ = value;
				PopulateCommodityValueIfRequired(CusInBondCargoDescSchema.Constants.BY_GrossWeightUnit, value);
			}
		}

		#endregion

		#region B0_IssuerSCAC

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.SCACCarrierCodes))]
		[ReadOnlyMember(Schema.B0_IsLodged)]
		public override ZString B0_IssuerSCAC
		{
			get { return base.B0_IssuerSCAC; }
			set
			{
				base.B0_IssuerSCAC = value;
			}
		}

		#endregion

		#region Shipment Value

		[DecimalPlaces(0)]
		public override ZDecimal B0_GoodsValue
		{
			get { return base.B0_GoodsValue; }
			set
			{
				var hasChanges = base.B0_GoodsValue != value;
				base.B0_GoodsValue = value;
				if (hasChanges && !IsCopying)
				{
					PopulateCommodityValueIfRequired(CusInBondCargoDescSchema.Constants.BY_MonetaryValue, value);
				}
			}
		}

		protected bool B0_RX_NKGoodsValueCurrency_ReadOnly => true;

		#endregion

		#region InBond

		public InBond InBond
		{
			get
			{
				if (inBond == null || inBond.IsDeleted)
				{
					inBond = InBond.LoadOrCreate(this);
					RegisterEditableChildObject(inBond);
				}
				return inBond;
			}
		}
		InBond inBond;

		#endregion

		#region B0_ManifestQty

		public override ZInt B0_ManifestQty
		{
			get => base.B0_ManifestQty;
			set
			{
				base.B0_ManifestQty = value;
				PopulateCommodityValueIfRequired(CusInBondCargoDescSchema.Constants.BY_PieceCount, value);
			}
		}

		#endregion

		#region B0_RN_NKCountryOfExport

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.Countries))]
		public override ZString B0_RN_NKCountryOfExport
		{
			get => base.B0_RN_NKCountryOfExport;
			set
			{
				base.B0_RN_NKCountryOfExport = value;
				PopulateCommodityValueIfRequired(CusInBondCargoDescSchema.Constants.BY_RN_NKCountryOfOrigin, value);
			}
		}

		#endregion

		#endregion

		#region Collections

		#region Parties

		public Party Consignee
		{
			get
			{
				if (consignee == null || consignee.IsDeleted)
				{
					consignee = Party.LoadOrCreate(this, PartyTypes.Codes.Consignee);
					RegisterEditableChildObject(consignee);
				}
				return consignee;
			}
		}
		Party consignee;

		public Party Shipper
		{
			get
			{
				if (shipper == null || shipper.IsDeleted)
				{
					shipper = Party.LoadOrCreate(this, PartyTypes.Codes.Shipper);
					RegisterEditableChildObject(shipper);
				}
				return shipper;
			}
		}
		Party shipper;

		[ChildEditable(true)]
		public PartyCollection Parties
		{
			get
			{
				if (parties == null)
				{
					parties = new PartyCollection(this);
					RegisterEditableChildObject(parties);
				}
				return parties;
			}
		}
		PartyCollection parties;

		#endregion

		#region Commodities

		[ChildEditable]
		public CommodityCollection Commodities
		{
			get
			{
				if (commodities == null)
				{
					commodities = new CommodityCollection(this);
					RegisterEditableChildObject(commodities);
				}
				return commodities;
			}
		}
		CommodityCollection commodities;

		#endregion

		#endregion

		#region Overrides

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			licenceLogRequired = B0_ReleaseStatusInfo.HasChanges && IsClearedStatus(B0_ReleaseStatus) && !Logs.Find(l => l.IsInDatabase && l.SL_SE_NKEvent == AutoEvents.CustomsEntryStatusCode && IsClearedReference(l.SL_Reference)).Any();

			if (inBond != null && B0_ShipmentType != ShipmentTypes.Codes.Inbond)
			{
				inBond.Delete();
			}

			ZBool IsClearedReference(string reference)
			{
				return reference == Lookups.ReleaseStatusList.GetCodeDescription(ShipmentEntryStatusList.Codes.Clear)
					|| reference == Lookups.ReleaseStatusList.GetCodeDescription(ShipmentEntryStatusList.Codes.Linked);
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded && licenceLogRequired && !IsDeleted)
			{
				ObjectFactory.Get<ILicenceConsumptionLogCreator>().CreateLog(Env.Licence.USeManifest, true);
				licenceLogRequired = false;
			}
		}
		bool licenceLogRequired;

		public static ZBool IsClearedStatus(string status)
		{
			return status == ShipmentEntryStatusList.Codes.Clear
				|| status == ShipmentEntryStatusList.Codes.Linked;
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			B0_RX_NKGoodsValueCurrency = Constants.CurrencyCodes.UnitedStates;
			base.B0_ShipmentType = ShipmentTypes.Codes.PAPS;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Shipment: " + B0_MasterBillNumber; }
		}

		protected override NoteTypeCollection NoteTypesCore
		{
			get
			{
				var types = base.NoteTypesCore;
				types.Add(PredefinedNoteTypes.Instance.DetailedGoodsDescription);
				return types;
			}
		}

		#region Delete

		public override void Delete()
		{
			Parties.DeleteAll();
			Commodities.DeleteAll();
			if (inBond != null)
			{
				inBond.Delete();
			}

			base.Delete();
		}

		public override bool CanDelete
		{
			get { return !B0_IsLodged; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("8894401C-AF4C-47D5-B000-99A65D365BF8", "This Shipment is lodged to Customs, you have to send cancellation message for this shipment before deleting it."); }
		}

		#endregion

		#region Validation

		public new ShipmentValidation Validation
		{
			get { return (ShipmentValidation)base.Validation; }
		}

		protected override CusInBondBillValidation GetNewValidation()
		{
			return new ShipmentValidation(this);
		}

		internal bool ValidateAllHasBeenRun { get; set; }

		#endregion

		#region Lookups

		public new ShipmentLookups Lookups
		{
			get { return (ShipmentLookups)base.Lookups; }
		}

		protected override CusInBondBillLookups GetNewLookups()
		{
			return new ShipmentLookups(this);
		}

		#endregion

		#endregion

		#region Implementation of INoteSource

		ZString INoteSource.NoteSourceName
		{
			get { return HumanReadableName; }
		}

		#endregion

		#region Implementation of IWorkflowTriggerFieldChangeSource

		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders
		{
			get { return new IWorkflowProvider[] { Trip }; }
		}

		#endregion

		public string SelectionDescription => B0_ReferenceID;

		public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.USeManifest));
		DocManagerInfo docManagerInfo;

		#region ICusInBondCargoDescTypeProvider Members
		Type ICusInBondCargoDescTypeProvider.CusInBondCargoDescType => typeof(Commodity);
		#endregion

		#region Methods

		public void UpdateShipmentTypeDependentsAsNeeded()
		{
			B0_RX_NKGoodsValueCurrency = Constants.CurrencyCodes.UnitedStates;

			if (B0_ShipmentType != ShipmentTypes.Codes.BRASS)
			{
				Commodities.ForEach(x => x.C4Codes.RemoveAndDeleteAll());
			}

			if (B0_GoodsValueInfo.ReadOnly)
			{
				B0_GoodsValueInfo.ClearValue();
			}
		}

		void PopulateCommodityValueIfRequired(string commodityFieldName, ZString value, int maxLength)
		{
			PopulateCommodityValueIfRequired(commodityFieldName, value.Left(maxLength));
		}

		void PopulateCommodityValueIfRequired(string commodityFieldName, IZType value)
		{
			if (!populateCommodityValueSuspended)
			{
				Commodity firstCommodityToBeUpdated = null;
				var countOfCommodities = Commodities.Count;
				if (countOfCommodities == 0)
				{
					firstCommodityToBeUpdated = GetOrCreateFirstCommodity();
				}
				else if (countOfCommodities == 1)
				{
					firstCommodityToBeUpdated = Commodities[0];
				}

				if (firstCommodityToBeUpdated != null)
				{
					firstCommodityToBeUpdated[commodityFieldName] = value;
				}
			}
		}

		public IDisposable SuspendPopulateCommodityValue() =>
			new DisposableAction(
			() => populateCommodityValueSuspended = true,
			() => populateCommodityValueSuspended = false
		);

		bool populateCommodityValueSuspended;

		[DecimalPlaces(0)]
		internal ZDecimal TotalCommoditiesValue
		{
			get
			{
				return Commodities.Sum(x => x.BY_MonetaryValue);
			}
		}

		#endregion

		#region First Commodity
		public Commodity FirstCommodity
		{
			get
			{
				if (firstCommodity == null)
				{
					Commodities.CollectionCountChange -= Commodities_CountChanged;
					Commodities.CollectionCountChange += Commodities_CountChanged;
					firstCommodity = new RecalculableCachedValue<Commodity>(() => Commodities.Count == 1 ? Commodities[0] : Factory.GetNull<Commodity>());
				}
				return firstCommodity.Value;
			}
		}
		RecalculableCachedValue<Commodity> firstCommodity;

		Commodity GetOrCreateFirstCommodity()
		{
			var result = FirstCommodity;
			if (result == null && !IsCopying)
			{
				Commodities.AddNew();
				firstCommodity.InvalidateCache();
				Commodities.RefreshBinding();
			}
			return result;
		}

		void Commodities_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (firstCommodity != null)
			{
				var shouldRefresh = false;
				var oldItemCount = Commodities.Count;
				if (e.ItemAdded)
				{
					shouldRefresh = oldItemCount < 3;
					oldItemCount -= 1;
				}
				else
				{
					shouldRefresh = oldItemCount < 2;
					oldItemCount += 1;
				}

				if (shouldRefresh)
				{
					var oldItem = firstCommodity.Value;
					firstCommodity.InvalidateCache();
					var newItem = firstCommodity.Value;

					if (!IsCopying)
					{
						if (!object.ReferenceEquals(oldItem, newItem))
						{
							RefreshBindingForFirstCommodityProperty(oldItem, newItem);
						}
					}
				}

				HasMultipleItemLineInfo.RefreshBinding((ZBool)(oldItemCount > 1));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		void RefreshBindingForFirstCommodityProperty(Commodity oldItem, Commodity newItem)
		{
			RefreshBinding(oldItem, newItem, (x) => x.BY_PieceCount, () => FirstCommodityPieceCountInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_ManifestUnitCode, () => FirstCommodityManifestUnitCodeInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_GrossWeight, () => FirstCommodityWeightInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_GrossWeightUnit, () => FirstCommodityWeightUnitInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_Description, () => FirstCommodityDescriptionInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_BJ_Equipment, () => FirstCommodityEquipmentInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_MarksAndNumbers, () => FirstCommodityMarksAndNumbersInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_MonetaryValue, () => FirstCommodityMonetaryValueInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_RN_NKCountryOfOrigin, () => FirstCommodityCountryOfOriginInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_HarmonizedNumbers, () => FirstCommodityHarmonizedNumbersInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_HazardousGoodsIdentifier, () => FirstCommodityHazardousGoodsIdentifierInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_HazardousGoodsContact, () => FirstCommodityHazardousGoodsContactInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_HazardousGoodsContactPhone, () => FirstCommodityHazardousGoodsContactPhoneInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_VehicleIdentificationNumbers, () => FirstCommodityVehicleIdentificationNumbersInfo);
			RefreshBinding(oldItem, newItem, (x) => x.BY_C4Codes, () => FirstCommodityC4CodesInfo);
		}
		void RefreshBinding(Commodity oldItem, Commodity newItem, Func<Commodity, IZType> getValue, Func<ZPropertyInfo> getInfo)
		{
			if (!oldItem.IsDeleted)
			{
				var oldValue = getValue(oldItem);
				if (oldValue != getValue(newItem))
				{
					getInfo().RefreshBinding(oldValue);
				}
			}
			else
			{
				getInfo().RefreshBinding();
			}
		}

		#region FirstCommodityPieceCount

		[ReadOnlyMember(nameof(HasMultipleItemLine))]
		public ZInt FirstCommodityPieceCount
		{
			get => FirstCommodity.BY_PieceCount;
			set
			{
				GetOrCreateFirstCommodity().BY_PieceCount = value;
			}
		}

		public ZPropertyInfo FirstCommodityPieceCountInfo => GetZPropertyInfo(Schema.FirstCommodityPieceCount);

		#endregion

		#region FirstCommodityManifestUnitCode

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.QuantityUnits))]
		[MaxLength(Schema.FirstCommodityManifestUnitCodeMaxLength)]
		[ReadOnlyMember(nameof(HasMultipleItemLine))]
		public ZString FirstCommodityManifestUnitCode
		{
			get => FirstCommodity.BY_ManifestUnitCode;
			set
			{
				GetOrCreateFirstCommodity().BY_ManifestUnitCode = value;
			}
		}

		public ZPropertyInfo FirstCommodityManifestUnitCodeInfo => GetZPropertyInfo(Schema.FirstCommodityManifestUnitCode);

		#endregion

		#region FirstCommodityWeight

		[MeasureUnit(Schema.FirstCommodityWeightUnit, MeasureUnitType.Weight)]
		[ReadOnlyMember(nameof(HasMultipleItemLine))]
		public ZDecimal FirstCommodityWeight
		{
			get => FirstCommodity.BY_GrossWeight;
			set
			{
				GetOrCreateFirstCommodity().BY_GrossWeight = value;
			}
		}

		public ZPropertyInfo FirstCommodityWeightInfo => GetZPropertyInfo(Schema.FirstCommodityWeight);

		#endregion

		#region FirstCommodityWeightUnit

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.WeightUnits))]
		[ReadOnlyMember(nameof(HasMultipleItemLine))]
		[MaxLength(Schema.FirstCommodityWeightUnitMaxLength)]
		public ZString FirstCommodityWeightUnit
		{
			get => FirstCommodity.BY_GrossWeightUnit;
			set
			{
				GetOrCreateFirstCommodity().BY_GrossWeightUnit = value;
			}
		}

		public ZPropertyInfo FirstCommodityWeightUnitInfo => GetZPropertyInfo(Schema.FirstCommodityWeightUnit);

		#endregion

		#region FirstCommodityDescription

		[MaxLength(Schema.FirstCommodityDescriptionMaxLength)]
		[ReadOnlyMember(nameof(HasMultipleItemLine))]
		public ZString FirstCommodityDescription
		{
			get => FirstCommodity.BY_Description;
			set
			{
				GetOrCreateFirstCommodity().BY_Description = value.Left(Schema.FirstCommodityDescriptionMaxLength);
			}
		}

		public ZPropertyInfo FirstCommodityDescriptionInfo => GetZPropertyInfo(Schema.FirstCommodityDescription);

		#endregion

		#region FirstCommodityEquipment

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.Equipment))]
		[ReadOnlyMember(nameof(HasMultipleItemLine))]
		public ZGuid FirstCommodityEquipment
		{
			get => FirstCommodity.BY_BJ_Equipment;
			set
			{
				GetOrCreateFirstCommodity().BY_BJ_Equipment = value;
			}
		}

		public ZPropertyInfo FirstCommodityEquipmentInfo => GetZPropertyInfo(Schema.FirstCommodityEquipment);

		#endregion

		#region FirstCommodityMarksAndNumbers

		[MaxLength(Schema.FirstCommodityMarksAndNumbersMaxLength)]
		[ReadOnlyMember(nameof(HasMultipleItemLine))]
		public ZString FirstCommodityMarksAndNumbers
		{
			get => FirstCommodity.BY_MarksAndNumbers;
			set
			{
				GetOrCreateFirstCommodity().BY_MarksAndNumbers = value;
			}
		}

		public ZPropertyInfo FirstCommodityMarksAndNumbersInfo => GetZPropertyInfo(Schema.FirstCommodityMarksAndNumbers);

		#endregion

		#region FirstCommodityMonetaryValue

		[DecimalPlaces(0)]
		[ReadOnlyMember(nameof(HasMultipleItemLine))]
		public ZDecimal FirstCommodityMonetaryValue
		{
			get => FirstCommodity.BY_MonetaryValue;
			set
			{
				GetOrCreateFirstCommodity().BY_MonetaryValue = value;
			}
		}

		public ZPropertyInfo FirstCommodityMonetaryValueInfo => GetZPropertyInfo(Schema.FirstCommodityMonetaryValue);

		#endregion

		#region FirstCommodityCountryOfOrigin

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.Countries))]
		[MaxLength(Schema.FirstCommodityCountryOfOriginMaxLength)]
		[ReadOnlyMember(nameof(FirstCommodityCountryOfOrigin_ReadOnly))]
		public ZString FirstCommodityCountryOfOrigin
		{
			get => FirstCommodity.BY_RN_NKCountryOfOrigin;
			set
			{
				GetOrCreateFirstCommodity().BY_RN_NKCountryOfOrigin = value;
			}
		}

		public ZPropertyInfo FirstCommodityCountryOfOriginInfo => GetZPropertyInfo(Schema.FirstCommodityCountryOfOrigin);

		bool FirstCommodityCountryOfOrigin_ReadOnly
		{
			get { return HasMultipleItemLine || FirstCommodity.BY_RN_NKCountryOfOrigin_ReadOnly; }
		}

		#endregion

		#region FirstCommodityHarmonizedNumbers

		[BusinessObjectMaxLengthTestExclude]
		[ReadOnlyMember(nameof(HasMultipleItemLine))]
		public ZString FirstCommodityHarmonizedNumbers
		{
			get => FirstCommodity.BY_HarmonizedNumbers;
			set
			{
				GetOrCreateFirstCommodity().BY_HarmonizedNumbers = value;
			}
		}

		public ZPropertyInfo FirstCommodityHarmonizedNumbersInfo => GetZPropertyInfo(Schema.FirstCommodityHarmonizedNumbers);

		#endregion

		#region FirstCommodityHazardousGoodsIdentifier

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.UNDGSubs))]
		[ReadOnlyMember(nameof(FirstCommodityHazardousGoodsIdentifier_ReadOnly))]
		public ZGuid FirstCommodityHazardousGoodsIdentifier
		{
			get => FirstCommodity.BY_HazardousGoodsIdentifier;
			set
			{
				GetOrCreateFirstCommodity().BY_HazardousGoodsIdentifier = value;
			}
		}

		public ZPropertyInfo FirstCommodityHazardousGoodsIdentifierInfo => GetZPropertyInfo(Schema.FirstCommodityHazardousGoodsIdentifier);

		bool FirstCommodityHazardousGoodsIdentifier_ReadOnly
		{
			get { return HasMultipleItemLine || FirstCommodity.HazardousGoodsReadOnly; }
		}

		#endregion

		#region FirstCommodityHazardousGoodsContact

		[List(nameof(Lookups) + "." + nameof(ShipmentLookups.Contacts))]
		[ReadOnlyMember(nameof(FirstCommodityHazardousGoodsContact_ReadOnly))]
		public ZGuid FirstCommodityHazardousGoodsContact
		{
			get => FirstCommodity.BY_HazardousGoodsContact;
			set
			{
				GetOrCreateFirstCommodity().BY_HazardousGoodsContact = value;
			}
		}

		public ZPropertyInfo FirstCommodityHazardousGoodsContactInfo => GetZPropertyInfo(Schema.FirstCommodityHazardousGoodsContact);

		bool FirstCommodityHazardousGoodsContact_ReadOnly
		{
			get { return HasMultipleItemLine || FirstCommodity.HazardousGoodsReadOnly; }
		}

		#endregion

		#region FirstCommodityHazardousGoodsContactPhone

		public ZString FirstCommodityHazardousGoodsContactPhone
		{
			get => FirstCommodity.BY_HazardousGoodsContactPhone;
		}

		public ZPropertyInfo FirstCommodityHazardousGoodsContactPhoneInfo => GetZPropertyInfo(Schema.FirstCommodityHazardousGoodsContactPhone);

		#endregion

		#region FirstCommodityVehicleIdentificationNumbers

		[BusinessObjectMaxLengthTestExclude]
		[ReadOnlyMember(nameof(HasMultipleItemLine))]
		public ZString FirstCommodityVehicleIdentificationNumbers
		{
			get => FirstCommodity.BY_VehicleIdentificationNumbers;
			set
			{
				GetOrCreateFirstCommodity().BY_VehicleIdentificationNumbers = value;
			}
		}

		public ZPropertyInfo FirstCommodityVehicleIdentificationNumbersInfo => GetZPropertyInfo(Schema.FirstCommodityVehicleIdentificationNumbers);

		#endregion

		#region FirstCommodityC4Codes

		[BusinessObjectMaxLengthTestExclude]
		[ReadOnlyMember(nameof(FirstCommodityC4Codes_ReadOnly))]
		public ZString FirstCommodityC4Codes
		{
			get => FirstCommodity.BY_C4Codes;
			set
			{
				GetOrCreateFirstCommodity().BY_C4Codes = value;
			}
		}

		public ZPropertyInfo FirstCommodityC4CodesInfo => GetZPropertyInfo(Schema.FirstCommodityC4Codes);

		bool FirstCommodityC4Codes_ReadOnly
		{
			get { return HasMultipleItemLine || FirstCommodity.BY_C4Codes_ReadOnly; }
		}

		#endregion

		#region HasMultipleItemLine

		public ZBool HasMultipleItemLine => Commodities.Count > 1;

		public ZPropertyInfo HasMultipleItemLineInfo => GetZPropertyInfo(Schema.HasMultipleItemLine);

		#endregion

		#endregion

		#region Fetch Strategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new ShipmentFetchStrategy(this);

		#endregion
	}
}
