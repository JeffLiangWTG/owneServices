using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.NumberFountain;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.eTail.Integration.HVLVConstants;
using EventReferenceCodes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.eTail.Business
{
	[CodeProperty(AutoHVLVItem.Schema.HVI_ItemId)]
	[DependentBusinessObject(typeof(HVLVConsignment), nameof(HVLVConsignment.Items))]
	[UniversalDataContext(DataContextType.HVLVItem)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]

	public class HVLVItem : AutoHVLVItem,
		IHVLVItemForDocument,
		IJobNumber,
		IWorkflowTriggerEventSource,
		IDocManagerSupportCore,
		IHVLVFWBDGCodeProvider,
		IDocumentSupportable,
		IUNDGDataItemProvider,
		ISupportUXMLDataImporting,
		IHVLVISFItemInfoProvider
	{
		#region Schema

		public new class Schema : AutoHVLVItem.Schema
		{
			public static class Triggers
			{
				public const string PopulateUsageTimes_InsertUpdate = "TG_HVLVItem_PopulateUsageTimes_InsertUpdate";
				public const string CalculateMeasurements_Update = "TG_HVLVItem_CalculateMeasurements_Update";
				public const string CalculateMeasurements_Insert = "TG_HVLVItem_CalculateMeasurements_Insert";
				public const string CalculateMeasurements_Delete = "TG_HVLVItem_CalculateMeasurements_Delete";
			}
		}

		#endregion

		public HVLVItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			SetConcurrencyPolicyOnTable(ConcurrencyPolicy.Ignore);
			factory.SetBulkCopyOnTable(HVLVItemSchema.Constants.TableName,
				batchSize: HVLVDataRegistry.Instance.HVLVDataBulkCopyBatchSize.Value,
				checkConstraints: true,
				onBulkCopyingHandler: BulkCopyingHandler);
		}

		internal void MarkForReloadItemsFromLocalCache() => markForReloadItemsFromLocalCache = true;
		bool markForReloadItemsFromLocalCache;

		void BulkCopyingHandler(object sender, EventArgs e)
		{
			HVLVConsignment.CalculateTotalProperties(Factory);
		}

		#region Related Business Objects

		public HVLVConsignment Consignment => Factory.Load<HVLVConsignment>(HVI_HVC_Consignment);

		[RelatedBusinessObject("Consignment")]
		public override ZGuid HVI_HVC_Consignment
		{
			get { return base.HVI_HVC_Consignment; }
			set
			{
				var oldValue = HVI_HVC_Consignment;
				if (oldValue != value)
				{
					base.HVI_HVC_Consignment = value;
					if (Consignment is HVLVConsignment consignment)
					{
						consignment.MarkForReloadItemsFromLocalCache();
						HVI_ClusterKey = consignment.HVC_ClusterKey;
						if (consignment.ManagingShipment is ForwardingShipment managingShipment)
						{
							HVI_JS_LoadedOnShipment = managingShipment.PK;
						}
					}

					if (!IsCopying)
					{
						Lines.Cast<HVLVItemLine>().ForEach(x => x.RefreshPartSyncManagerActiveDeciderPK());
					}
				}
			}
		}

		[ChildEditable(true)]
		public HVLVItemLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new HVLVItemLineCollection(this);
					lines.Load();
					RegisterEditableChildObject(lines);
				}

				if (markForReloadItemsFromLocalCache)
				{
					lines.ReloadFromLocalCache();
					markForReloadItemsFromLocalCache = false;
				}

				return lines;
			}
		}

		HVLVItemLineCollection lines;

		public ForwardingShipment Shipment => Factory.Load<ForwardingShipment>(HVI_JS_LoadedOnShipment);

		public HVLVOriginLoadList LoadList => Factory.Load<HVLVOriginLoadList>(HVI_HVL_LoadList);

		public HVLVOuterPackage OuterPackage => Factory.Load<HVLVOuterPackage>(HVI_HVO_OuterPackage);

		public OrgHeader ETailer => Shipment?.Consignor ?? Consignment?.BookingHeader?.BillToParty?.Header;

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			HVI_UnitOfDimension = Env.Registry.OuterPacklinesMeasurementDefaultUnit;
			HVI_ShipperFirstUsageTimeUtc = ZDateTime.Empty;
			HVI_OriginFirstUsageTimeUtc = ZDateTime.Empty;
			HVI_DestinationFirstUsageTimeUtc = ZDateTime.Empty;
		}

		#endregion

		#region Properties

		public override ZInt HVI_ClusterKey
		{
			get { return base.HVI_ClusterKey; }
			set
			{
				if (!value.IsEmpty)
				{
					Lines.OfType<HVLVItemLine>().ForEach(x => x.HVS_ClusterKey = value);
					base.HVI_ClusterKey = value;
				}
			}
		}

		[List("Lookups.HVI_JS_LoadedOnShipment_List")]
		[RelatedBusinessObject(nameof(Shipment))]
		public override ZGuid HVI_JS_LoadedOnShipment
		{
			get { return base.HVI_JS_LoadedOnShipment; }
			set
			{
				if (value != HVI_JS_LoadedOnShipment)
				{
					base.HVI_JS_LoadedOnShipment = value;
					ReviseStatusAfterShipmentLinking();

					if (HVI_ContainerNumber.IsEmpty && Shipment != null && Shipment.IsSea && Lookups.ContainerNumber_List.Count == 1)
					{
						HVI_ContainerNumber = Lookups.ContainerNumber_List[0].Code;
					}

					if (IsPlusUsageType)
					{
						HVI_UsageType = HVLVItemUsageTypes.Codes.Plus;
					}
					else
					{
						HVI_UsageType = HVLVItemUsageTypes.Codes.Standard;
					}
				}
			}
		}

		List<ZString> ValidPlusUsageCountries => new List<ZString> { CountryCodes.UnitedStates, CountryCodes.SouthAfrica };

		public bool IsPlusUsageType
		{
			get
			{
				var shipmentDestinationCustomsJurisdictionCountry = CountryCodes.GetCustomsCountryOfJurisdiction(Shipment?.Destination?.Country?.Code);
				var branchCustomsJurisdictionCountry = CountryCodes.GetCustomsCountryOfJurisdiction(GlbBranch.CurrentBranch.Country?.Code);
				var isSameCustomsJurisdictionCountry = (branchCustomsJurisdictionCountry == shipmentDestinationCustomsJurisdictionCountry);
				var isValidPlusUsageCountry = ValidPlusUsageCountries.Contains(shipmentDestinationCustomsJurisdictionCountry);

				if (shipmentDestinationCustomsJurisdictionCountry == CountryCodes.UnitedStates)
				{
					return isValidPlusUsageCountry && (isSameCustomsJurisdictionCountry || HVLVDataRegistry.Instance.EnableSecurityFilingsForNonUSACompanies.Value);
				}

				return isValidPlusUsageCountry && isSameCustomsJurisdictionCountry;
			}
		}

		protected bool HVI_JS_LoadedOnShipment_ReadOnly => (Consignment?.HasManagingShipment ?? false) || Consignment?.ConsignmentHeader == null;

		[List("Lookups.HVI_HVL_LoadList_List")]
		[RelatedBusinessObject(nameof(LoadList))]
		public override ZGuid HVI_HVL_LoadList
		{
			get { return base.HVI_HVL_LoadList; }
			set
			{
				if (value != HVI_HVL_LoadList)
				{
					base.HVI_HVL_LoadList = value;
					if (LoadList != null && HVI_Status == HVLVItemStatus.Codes.ManifestedByETailer)
					{
						HVI_Status = HVLVItemStatus.Codes.LoadListAllocated;
					}
					else if (LoadList == null && HVI_Status == HVLVItemStatus.Codes.LoadListAllocated)
					{
						HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
					}
				}
			}
		}

		protected bool HVI_HVL_LoadList_ReadOnly =>
			HVI_Status != HVLVItemStatus.Codes.ManifestedByETailer &&
			HVI_Status != HVLVItemStatus.Codes.LoadListAllocated;

		[List("Lookups.HVI_HVO_OuterPackage_List")]
		[RelatedBusinessObject(nameof(OuterPackage))]
		public override ZGuid HVI_HVO_OuterPackage
		{
			get => base.HVI_HVO_OuterPackage;
			set
			{
				base.HVI_HVO_OuterPackage = value;
				if (Shipment?.ArrivalConsol?.PK != null && OuterPackage != null)
				{
					OuterPackage.HVO_JK_LoadedOnConsol = Shipment.ArrivalConsol.PK;
				}
			}
		}

		protected bool HVI_ItemId_ReadOnly => true;

		protected bool HVI_DestinationFirstUsageTimeUtc_ReadOnly => true;

		protected bool HVI_OriginFirstUsageTimeUtc_ReadOnly => true;

		protected bool HVI_ShipperFirstUsageTimeUtc_ReadOnly => true;

		protected bool HVI_IsScannedAtDestination_ReadOnly => true;

		public override ZDecimal HVI_ManifestedWeight
		{
			get { return base.HVI_ManifestedWeight; }
			set
			{
				base.HVI_ManifestedWeight = value;
				if (Consignment != null && HVI_IsActive)
				{
					Consignment.CalculateManifestedWeight();
					CalculateChargeableInformation();
					Consignment.BookingHeader?.MarkForWeightRecalculation();
				}
			}
		}

		public decimal EffectiveWeight => HVI_ActualWeight.IsEmpty ? HVI_ManifestedWeight : HVI_ActualWeight;

		internal ZDecimal ItemLinesTotalWeight
		{
			get
			{
				var result = ZDecimal.Zero;
				foreach (HVLVItemLine line in Lines)
				{
					result += Weight.ConvertSafe(line.HVS_GrossWeight, line.HVS_WeightUnit, Consignment.HVC_WeightUQ);
				}

				return result;
			}
		}

		public void CalculateManifestedWeight()
		{
			if (HasItemLines)
			{
				HVI_ManifestedWeight = ItemLinesTotalWeight;
			}
		}

		public override ZDecimal HVI_ManifestedVolume
		{
			get { return base.HVI_ManifestedVolume; }
			set
			{
				base.HVI_ManifestedVolume = value;
				if (Consignment != null && HVI_IsActive)
				{
					Consignment.CalculateManifestedVolume();
					CalculateChargeableInformation();
					Consignment.BookingHeader?.MarkForVolumeRecalculation();
				}
			}
		}

		public decimal EffectiveVolume => HVI_ActualVolume.IsEmpty ? HVI_ManifestedVolume : HVI_ActualVolume;

		public ZBool HasItemLines => Lines.Any();

		public override ZDecimal HVI_ActualWeight
		{
			get { return base.HVI_ActualWeight; }
			set
			{
				base.HVI_ActualWeight = value;
				if (Consignment != null && HVI_IsActive)
				{
					Consignment.CalculateActualWeight();
					CalculateChargeableInformation();
					Consignment.BookingHeader?.MarkForWeightRecalculation();
				}
			}
		}

		public override ZDecimal HVI_ActualVolume
		{
			get { return base.HVI_ActualVolume; }
			set
			{
				base.HVI_ActualVolume = value;
				if (Consignment != null && HVI_IsActive)
				{
					Consignment.CalculateActualVolume();
					CalculateChargeableInformation();
					Consignment.BookingHeader?.MarkForVolumeRecalculation();
				}
			}
		}

		[MeasureUnit(AutoHVLVItem.Schema.HVI_UnitOfDimension, MeasureUnitType.Length)]
		public override ZDecimal HVI_Height
		{
			get => base.HVI_Height;
			set
			{
				base.HVI_Height = value;
				RecalculateVolume(HVLVItemSchema.HVI_ActualVolume);
				RecalculateVolume(HVLVItemSchema.HVI_ManifestedVolume, true);
			}
		}

		[MeasureUnit(AutoHVLVItem.Schema.HVI_UnitOfDimension, MeasureUnitType.Length)]
		public override ZDecimal HVI_Width
		{
			get => base.HVI_Width;
			set
			{
				base.HVI_Width = value;
				RecalculateVolume(HVLVItemSchema.HVI_ActualVolume);
				RecalculateVolume(HVLVItemSchema.HVI_ManifestedVolume, true);
			}
		}

		[MeasureUnit(AutoHVLVItem.Schema.HVI_UnitOfDimension, MeasureUnitType.Length)]
		public override ZDecimal HVI_Length
		{
			get => base.HVI_Length;
			set
			{
				base.HVI_Length = value;
				RecalculateVolume(HVLVItemSchema.HVI_ActualVolume);
				RecalculateVolume(HVLVItemSchema.HVI_ManifestedVolume, true);
			}
		}

		[List("Lookups.HVI_UnitOfDimensionList")]
		public override ZString HVI_UnitOfDimension
		{
			get
			{
				return base.HVI_UnitOfDimension;
			}
			set
			{
				var oldValue = base.HVI_UnitOfDimension;
				base.HVI_UnitOfDimension = value.ToUpper();

				RecalculateVolume(HVLVItemSchema.HVI_ActualVolume);
				if (oldValue != HVI_UnitOfDimension || HVI_ManifestedVolume.IsEmpty)
				{
					RecalculateVolume(HVLVItemSchema.HVI_ManifestedVolume);
				}
			}
		}

		public void RecalculateVolume(SchemaDecimalColumn schemaDecimalColumn, bool checkEmpty = false)
		{
			if (Consignment is HVLVConsignment consignment)
			{
				var currentValue = this.GetValue<ZDecimal>(schemaDecimalColumn);
				if ((!checkEmpty || currentValue.IsEmpty)
					&& !HVI_Height.IsEmpty
					&& !HVI_Width.IsEmpty
					&& !HVI_Length.IsEmpty)
				{
					this[schemaDecimalColumn] = new ZDecimal(FreightUtilities.CalculateVolume(
						currentValue,
						1,
						HVI_Length,
						HVI_Width,
						HVI_Height,
						HVI_UnitOfDimension,
						consignment.HVC_VolumeUQ,
						schemaDecimalColumn.Scale));
				}
			}
		}

		[BusinessObjectTestExclude]
		public override ZString HVI_GoodsDescription
		{
			get
			{
				return Factory.StripNonWesternEuropeanCharactersIfRequired(base.HVI_GoodsDescription);
			}
			set
			{
				base.HVI_GoodsDescription = value;

				if (Consignment != null)
				{
					Consignment.ClearPreScreeningStatus();
				}
			}
		}

		public override ZString HVI_Status
		{
			get { return base.HVI_Status; }
			set
			{
				if (value != HVI_Status)
				{
					var oldStatus = HVI_Status;
					base.HVI_Status = value;
					HVI_Status_DescriptionInfo.RefreshBinding();

					if (!HVI_IsScannedAtDestination && ScannedAtDestinationStatus.Contains(HVI_Status))
					{
						HVI_IsScannedAtDestination = true;
					}

					if (!AddStatusUpdatedEventSuspended && IsInDatabase)
					{
						var parameters = new List<KeyValuePair<string, string>>
						{
							new(EventReferenceCodes.New, HVI_Status),
							new(EventReferenceCodes.ReferenceNumber, GetItemEventReferenceNumber()),
							new(EventReferenceCodes.Type, GetItemEventReferenceType()),
							new(EventReferenceCodes.Old, oldStatus)
						};

						var isEstimate = value == HVLVItemStatus.Codes.ShipmentArrived || value == HVLVItemStatus.Codes.ShipmentDeparted;
						Logs.AddNew(AutoEvents.StatusUpdated, ZDateTimeOffset.Now, isEstimate, [.. parameters]);
					}

					if (HVI_Status == HVLVItemStatus.Codes.Delivered)
					{
						Consignment.HVC_Status = HVLVConsignmentStatus.Codes.Delivered;
					}
				}
			}
		}

		[ResourceStringData("HVLVItem|ServiceLevel", Caption = "Service Level")]
		public ZString BookingServiceLevel => Consignment?.BookingHeader?.HVH_RS_NKBookingServiceLevel ?? string.Empty;

		[ResourceStringData("HVLVItem|DestinationCountry", Caption = "Destination Country")]
		public ZString ConsigneeCountry => Consignment?.ConsigneeCountryCode?.Description ?? string.Empty;

		public IDisposable SuspendAddStatusUpdatedEvent()
		{
			return new DisposableAction(
				() => addStatusUpdatedEventSuspendedCount++,
				() => addStatusUpdatedEventSuspendedCount--);
		}

#if DEBUG
		internal
#endif
		bool AddStatusUpdatedEventSuspended => addStatusUpdatedEventSuspendedCount > 0;

		int addStatusUpdatedEventSuspendedCount;

		public ZBool IsScannedCleared => HVI_IsScannedAtDestination && HVI_ReleaseStatus == HVLVReleaseStatus.ShortVersion.Cleared;

		public ZBool IsScannedHeld => HVI_IsScannedAtDestination && HVI_ReleaseStatus == HVLVReleaseStatus.ShortVersion.Held;

		public ZBool IsScannedSurplus => HVI_IsScannedAtDestination && HVI_Status == HVLVItemStatus.Codes.SurplusAtDestinationDepot;

		public ZBool IsScannedNotReported => HVI_IsScannedAtDestination && HVI_ReleaseStatus == HVLVReleaseStatus.ShortVersion.None;

		public string GetItemEventReferenceNumber()
		{
			if (!HVI_CurrentBarcode.IsEmpty)
			{
				return HVI_CurrentBarcode;
			}
			else if (!HVI_ShipperReference.IsEmpty)
			{
				return HVI_ShipperReference;
			}

			return HVI_ItemId;
		}

		public string GetItemEventReferenceType()
		{
			if (!HVI_CurrentBarcode.IsEmpty)
			{
				return ItemReferenceTypes.Barcode;
			}
			else if (!HVI_ShipperReference.IsEmpty)
			{
				return ItemReferenceTypes.ShipperReference;
			}

			return ItemReferenceTypes.ItemId;
		}

		[ResourceStringData("HVLVItem|HVI_Status_Description", Caption = "Status")]
		public ZString HVI_Status_Description => Lookups.HVI_Status_List.GetDescriptionFromCode(HVI_Status);

		public ZPropertyInfo HVI_Status_DescriptionInfo => GetZPropertyInfo(nameof(HVI_Status_Description));

		[List("Lookups.ContainerNumber_List")]
		public override ZString HVI_ContainerNumber
		{
			get => base.HVI_ContainerNumber;
			set => base.HVI_ContainerNumber = value;
		}

		protected bool HVI_ContainerNumber_ReadOnly => Shipment == null;

		[List("Lookups.LastMileTransportBooking_List")]
		[RelatedBusinessObject("LastMileTransportBooking")]
		public override ZGuid HVI_KM_LastMileTransportBooking
		{
			get => base.HVI_KM_LastMileTransportBooking;
			set => base.HVI_KM_LastMileTransportBooking = value;
		}

		public DtbBooking LastMileTransportBooking => Factory.Load<DtbBooking>(HVI_KM_LastMileTransportBooking);

		public ZBool IsLastMileCarrierBooked
		{
			get
			{
				var lmcQuery = new ZQuery(StmALogSchema.SL_Parent, PK);
				lmcQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, RefDocTypes.BookingConfirmation);
				lmcQuery.AddToFilter(StmALogSchema.SL_IsEstimate, false);
				lmcQuery.AddToFilter(StmALogSchema.SL_IsCancelled, false);

				return Factory.Exists(typeof(StmALog), lmcQuery);
			}
		}

		[ReadOnly(true)]
		public ZString HVI_CarrierBookingStatus_Description => Lookups.HVI_CarrierBookingStatus_List.GetDescriptionFromCode(HVI_CarrierBookingStatus);

		public ZPropertyInfo HVI_CarrierBookingStatus_DescriptionInfo => GetZPropertyInfo(nameof(HVI_CarrierBookingStatus_Description));

		#region Chargeable

		public ZDecimal VolumeWeight => Consignment.CalculateVolumeWeightFunction(EffectiveWeight, EffectiveVolume);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		public ZString VolumeWeightForDisplay
		{
			get
			{
				return string.Format("{0} {1}", Math.Round(VolumeWeight, 3), Consignment.ChargeableUQ);
			}
		}

		public ZPropertyInfo VolumeWeightForDisplayInfo => GetZPropertyInfo(nameof(VolumeWeightForDisplay));

		public ZDecimal Chargeable => Consignment.CalculateChargeableFunction(EffectiveWeight, EffectiveVolume);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1068:DoNotUseMathRound", Justification = "Baseline")]
		public ZString ChargeableForDisplay
		{
			get
			{
				return string.Format(Culture.Invariant, "{0} {1}", Math.Round(Chargeable, 3), Consignment.ChargeableUQ);
			}
		}

		public ZPropertyInfo ChargeableForDisplayInfo => GetZPropertyInfo(nameof(ChargeableForDisplay));

		public void CalculateChargeableInformation()
		{
			VolumeWeightForDisplayInfo.RefreshBinding();
			ChargeableForDisplayInfo.RefreshBinding();

			CalculateDensityInformation();
		}

		#endregion

		#region Density Factor

		public ZDecimal DensityFactor => Density.DensityFactor;

		HVLVItemDensity Density
		{
			get
			{
				if (density == null)
				{
					density = new HVLVItemDensity(this);
					density.RefreshAllValues();
				}

				return density;
			}
		}

		HVLVItemDensity density;

		void CalculateDensityInformation()
		{
			Density.RefreshAllValues();
		}

		#endregion

		#endregion

		#region Active Filter

		public static ZQuery ActiveFilter => new ZQuery(HVLVItemSchema.HVI_IsActive, SQLComparisonOperator.Equal, true);

		#endregion

		#region Status

		void ReviseStatusAfterShipmentLinking()
		{
			var currentStatusOrdinal = GetStatusOrdinal(HVI_Status);

			if (currentStatusOrdinal <= LastShipmentStatusOrdinal.Value)
			{
				if (Shipment != null)
				{
					var transports = Shipment.TransportsIncludingRelated;
					if (transports.ArrivalTransport != null && transports.ArrivalTransport.JW_ATA.IsValid)
					{
						HVI_Status = HVLVItemStatus.Codes.ShipmentArrived;
					}
					else if (transports.DepartureTransport != null && transports.DepartureTransport.JW_ATD.IsValid)
					{
						HVI_Status = HVLVItemStatus.Codes.ShipmentDeparted;
					}
					else
					{
						HVI_Status = HVLVItemStatus.Codes.ShipmentAllocated;
					}
				}
				else
				{
					// Todo: For now we can't distinguish between LoadListLodged and LoadListAllocated - to choose which one we should fallback to.
					// Let's leave it as is for now.
					HVI_Status = LoadList == null ?
						HVLVItemStatus.Codes.ManifestedByETailer :
						HVLVItemStatus.Codes.LoadListAllocated;
				}
			}
		}

		public bool HasArrivedAtDestinationDepot
		{
			get
			{
				return HVI_Status != HVLVItemStatus.Codes.ShortShippedAtDestinationDepot
					&& GetStatusOrdinal(HVI_Status) > LastShipmentStatusOrdinal.Value;
			}
		}

		Lazy<int> LastShipmentStatusOrdinal => new Lazy<int>(() => GetStatusOrdinal(HVLVItemStatus.Codes.ShipmentArrived));

		#endregion

		#region GS1 Prefix

		internal bool HasValidGS1Prefix => Consignment?.HasValidGS1Prefix ?? false;

		#endregion

		#region IsPackageIdValidSSCCBarCode

		public bool IsPackageIdValidSSCCBarCode
		{
			get
			{
				const string zeroPrefix = "00";
				const int barCodeLength = 20;

				var reference = HVI_CurrentBarcode;

				if (reference.SubstringSafe(0, 2) == zeroPrefix && reference.Length == barCodeLength)
				{
					reference = reference.SubstringSafe(2);
				}

				var result = SSCCBarCodeChecker.IsSSCCBarCode(reference);
				return result;
			}
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = Res.GetString("bb43d947-4ba6-4fc7-aea8-500bf8a0009f", "HVLV Item");

				if (!HVI_ItemId.IsEmpty)
				{
					result += " " + HVI_ItemId;
				}

				return result;
			}
		}

		protected override bool DeferFiringWorkflowCore
		{
			get
			{
				return true;
			}
		}

		public override void Delete()
		{
			Lines.DeleteAll();
			base.Delete();
		}

		#endregion

		#region IJobNumber

		string IJobNumber.JobNumber => HVI_ItemId;

		#endregion

		#region Loading / Saving / Deleting

		public override void OnSaving()
		{
			base.OnSaving();

			PopuldateItemIdIfNeeded();
			PopulateFirstUsageTimeIfNeeded();

			if (HVI_IsActiveInfo.HasChanges)
			{
				Logs.AddNew(HVI_IsActive ? AutoEvents.SetToActive : AutoEvents.SetToInactive);
			}

			if (HVI_CurrentBarcode.IsEmpty)
			{
				HVI_CurrentBarcode = HVI_ItemId;
			}
		}

		void PopuldateItemIdIfNeeded()
		{
			if (HVI_ItemId.IsEmpty)
			{
				if (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.Value)
				{
					var items = Factory.GetAddedBusinessObjects<HVLVItem>(false);
					if (items.Any())
					{
						if (Consignment.ConsignmentHeader?.GS1Info != null)
						{
							AllocateItemIDsIfNeeded(items, Consignment.ConsignmentHeader.GS1Info.SSCCNumberFountain);
						}
						else if (Consignment.BookingHeader?.GS1Info != null)
						{
							AllocateItemIDsIfNeeded(items, Consignment.BookingHeader.GS1Info.SSCCNumberFountain);
						}
						else
						{
							AllocateItemIDsIfNeeded(items, Env.NumberFountains.HVLVItemId);
						}
					}
				}
				else
				{
					HVI_ItemId = GetItemIdCandidates(this).First(x => !x.IsEmpty);
				}
			}
		}

		void AllocateItemIDsIfNeeded(IEnumerable<HVLVItem> items, INumberFountainProxy numberFountain)
		{
			var newItems = items.Where(item => item.HVI_ItemId.IsEmpty);
			if (newItems.Any())
			{
				var itemIDs = new Queue<string>(numberFountain.GetNextsFormatted(Factory, newItems.Count()));
				newItems.ForEach(item =>
				{
					item.HVI_ItemId = itemIDs.Dequeue();
					item.HVI_IsValidatedForUniqueness = true;
				});
			}
		}

		void PopulateFirstUsageTimeIfNeeded()
		{
			if (HVI_ShipperFirstUsageTimeUtc.IsEmpty && HVI_HVL_LoadList.IsEmpty && HVI_JS_LoadedOnShipment.IsEmpty)
			{
				HVI_ShipperFirstUsageTimeUtc = ZDateTime.UtcNow;
			}
			else if (HVI_OriginFirstUsageTimeUtc.IsEmpty && !HVI_HVL_LoadList.IsEmpty)
			{
				HVI_OriginFirstUsageTimeUtc = ZDateTime.UtcNow;
			}
			else if (HVI_DestinationFirstUsageTimeUtc.IsEmpty && !HVI_JS_LoadedOnShipment.IsEmpty)
			{
				HVI_DestinationFirstUsageTimeUtc = ZDateTime.UtcNow;
			}

			if (!IsInDatabase)
			{
				HVI_LastUsageCode = GlbBranch.CurrentBranch.GB_Code;
			}
		}

		internal static IEnumerable<ZString> GetItemIdCandidates(HVLVItem item, bool isFirstItem = false)
		{
			yield return item.HVI_CurrentBarcode;
			yield return item.HVI_ShipperReference;

			if (isFirstItem)
			{
				yield return item.Consignment.HVC_ConsignmentId;
			}

			yield return item.Consignment.ConsignmentHeader?.GS1Info?.GenerateSSCCNumber(item.Factory);
			yield return item.Consignment.BookingHeader?.GS1Info?.GenerateSSCCNumber(item.Factory);
			yield return Env.NumberFountains.HVLVItemId.GetNextFormatted(item.Factory);
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				PopulateUsageTableIfNeeded();
			}

			if (!saveSucceeded && !IsInDatabase)
			{
				HVI_ItemId = ZString.Empty;
			}
		}

		void PopulateUsageTableIfNeeded()
		{
			if (CargoWise.Data.DbRegistry.BiDisableChangeDataCapture.LoadValue(CargoWise.Data.Db.Connection))
			{
				if (IsInDatabase && HVI_IsActive)
				{
					PopulateUsageTableIfEditedByOtherCompany();
				}
			}
		}

		public void PopulateUsageTableIfEditedByOtherCompany()
		{
			if (!InvalidUsers.Contains(HVI_SystemCreateUser) && !InvalidUsers.Contains(GlbStaff.CurrentUser.GS_Code))
			{
				var creatingBranch = Factory.GetBranchFromStaffCode(HVI_SystemCreateUser);
				var lastEditBranch = GlbBranch.CurrentBranch;

				if (creatingBranch != null && lastEditBranch != null)
				{
					var lastEditCompany = lastEditBranch.Company;
					var creatingCompany = creatingBranch.Company;

					if (lastEditCompany != null && creatingCompany != null && lastEditCompany.PK != creatingCompany.PK)
					{
						LogUsageWhenEditedByOtherCompany();
					}
				}
			}
		}

		public void LogUsageWhenEditedByOtherCompany()
		{
			LogHVLVUsage(PK.ToString(), UsageCategories.CargoWiseOneUsage, UsageCodes.CargoWiseUsageByOtherCompany, GlbCompany.CurrentCompany.GC_Code, GlbBranch.CurrentBranch.GB_Code, GlbStaff.CurrentUser.GS_Code);
		}

		void LogHVLVUsage(string itemPK, string usageCategory, string usageCode, string companyCode, string branchCode, string userCode)
		{
			var sql = $@"
IF NOT EXISTS (SELECT * FROM dbo.HVLVUsage WHERE HXU_HVI_ParentItem = '{itemPK}' AND HXU_Code = '{usageCode}' AND HXU_GC_NKCompany = '{companyCode}')
BEGIN
	INSERT INTO dbo.HVLVUsage (HXU_PK, HXU_HVI_ParentItem, HXU_Category, HXU_GS_NKUser, HXU_Code, HXU_GC_NKCompany, HXU_BranchCode, HXU_UsageTimeUtc)
	VALUES (NEWID(), '{itemPK}', '{usageCategory}', '{userCode}', '{usageCode}', '{companyCode}', '{branchCode}', GETUTCDATE())
END";

			((CargoWise.Data.IDbConnected)Factory).Connection.ExecuteNonQuery(sql);
		}

		public override ZBool HVI_IsActive
		{
			get => base.HVI_IsActive;
			set
			{
				base.HVI_IsActive = value;
				if (Consignment != null)
				{
					Consignment.CalculateItemTotals();
					Consignment.Items?.RunPreSaveValidation();
					Consignment.BookingHeader?.MarkForItemCountRecalculation();
					Consignment.BookingHeader?.MarkForVolumeRecalculation();
					Consignment.BookingHeader?.MarkForWeightRecalculation();
				}
			}
		}

		protected bool HVI_IsActive_ReadOnly => !IsInDatabase;

		public override bool CanDelete => !IsInDatabase;

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			if (Shipment != null && !Consignment.HasManagingShipment)
			{
				return ResString.GetMultilingualString("6ae3bae9-14c9-494f-baf7-d5f6dc10328d", "Item(s) is attached to a Shipment.");
			}

			return base.GetWarningBeforeBeingDeleted();
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				return CanDelete ? base.ReasonForNotAbleToDelete : ResString.GetMultilingualString("c8187cdc-792b-4dc6-9e43-d915948dc851", "Existing items cannot be deleted. Mark them as inactive instead.");
			}
		}

		#endregion

		#region IWorkflowTriggerEventSource

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders => Consignment is IWorkflowProviderCore provider ?
			new[] { provider } :
			Array.Empty<IWorkflowProviderCore>();

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany => GlbCompany.CurrentCompany;

		#endregion

		#region DG Codes

		IReadOnlyCollection<UNDGSubstance> GetUNDGSubstances()
		{
			var query = new ZQuery();
			query.AddToFilter(UNDGDataItemSchema.DI_ParentTableCode, HVLVItemSchema.Constants.Prefix);
			query.AddToFilter(UNDGDataItemSchema.DI_ParentID, PK);

			var dataItems = Factory.Load<UNDGDataItem>(query);
			var substances = Factory.Load<UNDGSubstance>(new ZQuery(UNDGSubstanceSchema.PK, dataItems.Select(d => d.DI_DG)));

			return substances.ToArray();
		}

		public IReadOnlyCollection<ZString> DGUNNOValues()
		{
			return GetUNDGSubstances().Select(s => s.DG_UNNO.Trim()).Distinct().ToArray();
		}

		public IReadOnlyCollection<ZString> DGCodes
		{
			get
			{
				return GetUNDGSubstances().Select(s => s.DG_Code.Trim()).Distinct().ToArray();
			}
		}

		#endregion DG Codes

		#region IDocManagerSupportCore Members

		public IDocManagerInfoCore DocManagerInfo => Consignment.DocManagerInfo();

		public DocumentSupporter DocumentSupporter => new HVLVConsignmentDocumentSupporter(this);

		#endregion

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			var result = false;
			if (!HVI_IsActive && property.Name != AutoHVLVItem.Schema.HVI_IsActive)
			{
				result = true;
			}

			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		#region IHVLVItemForDocument

		IDocManagerInfo IHVLVItemForDocument.DocManagerInfo => DocManagerInfo;

		IOrgHeader IHVLVItemForDocument.ETailer => ETailer;

		Enterprise.Integration.TransportBooking.IDtbBooking IHVLVItemForDocument.LastMileTransportBooking => LastMileTransportBooking;

		Enterprise.Integration.Forwarding.IForwardingShipment IHVLVItemForDocument.Shipment => Shipment;

		IHVLVOuterPackage IHVLVItemForDocument.OuterPackage => OuterPackage;

		IHVLVOriginLoadList IHVLVItemForDocument.LoadList => LoadList;

		IHVLVConsignment IHVLVItemForDocument.Consignment => Consignment;

		IHVLVItemLineCollection IHVLVItemForDocument.Lines => Lines;

		#endregion

		#region IUNDGDataItemProvider Members

		[ChildEditable(true)]
		public UNDGDataItemCollection UNDGs
		{
			get
			{
				if (fUNDGs == null)
				{
					fUNDGs = new ForwardingUNDGDataItemCollection(this);
					RegisterEditableChildObject(fUNDGs);
				}

				return fUNDGs;
			}
		}

		UNDGDataItemCollection fUNDGs;

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => false;

		#endregion

		#region HVLVItemStatus

		int GetStatusOrdinal(string code)
		{
			if (!StatusOrdinalDictionary.TryGetValue(code, out var result))
			{
				throw new ArgumentException(FormattableString.Invariant($"Invalid code: {code}"));
			}

			return result;
		}

		Dictionary<string, int> StatusOrdinalDictionary
		{
			get
			{
				if (statusOrdinalDictionary == null)
				{
					statusOrdinalDictionary = new Dictionary<string, int>();
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.ManifestedByETailer, 1);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.PickUpFromShipper, 2);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.ReceivedAtOrigin, 3);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.ReceivedAtOriginWithException, 4);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.LoadListAllocated, 5);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.LoadListLodged, 6);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.ShipmentAllocated, 7);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.ShipmentDeparted, 8);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.ShipmentArrived, 9);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.ShortShippedAtDestinationDepot, 10);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.SurplusAtDestinationDepot, 10);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot, 10);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.DiscardedAtDestinationDepot, 10);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.ReceivedAtDestinationDepotWithException, 10);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.SeizedByCustoms, 10);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.DirectedToHeightenedSecurity, 10);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.ReadyForLastMileDelivery, 11);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.DispatchedToLastMileCarrier, 12);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.PickedUpByLastMileCarrier, 13);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.SuspendedByLastMileCarrier, 13);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.DamagedLostStolenByLastMileCarrier, 13);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.TrackingCancelledOrDeleted, 13);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.ProcessedAtFacility, 14);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.OnboardForDelivery, 15);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.ReturnToSender, 15);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.RedeliveryPending, 15);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.Delivered, 16);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.ProofOfDeliveryReceived, 17);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.DeliveryFailure, 18);
					statusOrdinalDictionary.Add(HVLVItemStatus.Codes.AllUnknownEvents, 19);
				}

				return statusOrdinalDictionary;
			}
		}

		Dictionary<string, int> statusOrdinalDictionary;

		static HashSet<string> ScannedAtDestinationStatus => new HashSet<string>
		{
			HVLVItemStatus.Codes.ReadyForLastMileDelivery,
			HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot,
			HVLVItemStatus.Codes.ReceivedAtDestinationDepotWithException,
			HVLVItemStatus.Codes.SurplusAtDestinationDepot,
			HVLVItemStatus.Codes.DispatchedToLastMileCarrier
		};

		#endregion

		#region UniqueIndexFailureHandler

		protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
		{
			get
			{
				if (uniqueIndexFailureHandlers == null)
				{
					uniqueIndexFailureHandlers = new List<IUniqueIndexFailureHandler>();
					uniqueIndexFailureHandlers.Add(new HVLVItemShipperReferenceUniqueIndexFailureHandler(this));
					uniqueIndexFailureHandlers.Add(new HVLVItemIdUniqueIndexFailureHandler(this));
				}

				return uniqueIndexFailureHandlers;
			}
		}

		List<IUniqueIndexFailureHandler> uniqueIndexFailureHandlers;

		class HVLVItemShipperReferenceUniqueIndexFailureHandler : IUniqueIndexFailureHandler
		{
			public HVLVItemShipperReferenceUniqueIndexFailureHandler(HVLVItem item)
			{
				this.item = item;
			}
			readonly HVLVItem item;

			public IEnumerable<string> HandledUniqueIndexNames
			{
				get { yield return HVLVItemSchema.Constants.Indexes.NR_UX__HVI_ShipperReference_HVI_ClusterKey; }
			}

			public void NotifyUserAndAttemptToResolve(INotificationHandler notifier, string indexName)
			{
				notifier?.ReportError(Res.GetString("684ebdb6-223f-496d-a2ab-abab17cd300e", "Error saving record. Item Shipper Reference must be unique on the Shipment or Booking Header. The duplicate value is ({0}).", item.HVI_ShipperReference), Res.GetString("9e105a15-064e-4e30-b772-dfaa1bfddbe4", "Duplicate Item Shipper Reference"));
			}
		}

		class HVLVItemIdUniqueIndexFailureHandler : NumberFountainUniqueIndexFailureHandler
		{
			public HVLVItemIdUniqueIndexFailureHandler(HVLVItem item)
				: base(HVLVItemSchema.Constants.Indexes.NR_UX__HVI_ItemId, item)
			{
			}

			protected override INumberFountainProxy NumberFountainToFix
			{
				get
				{
					var item = BizObjCausingError as HVLVItem;
					var consignment = item.Consignment;

					var gs1Info = consignment.ConsignmentHeader?.GS1Info ?? consignment.BookingHeader?.GS1Info;
					var usingSSCCFountainThatWeCannotFix = gs1Info != null;
					if (!usingSSCCFountainThatWeCannotFix)
					{
						var theFallbackFountainForSSCCFountain = Env.NumberFountains.HVLVItemId;
						return theFallbackFountainForSSCCFountain;
					}

					return null;
				}
			}
		}

		#endregion

		#region Validation

		protected override HVLVItemValidation GetNewValidation()
		{
			if (validatingForSeaCargoReport)
			{
				return new HVLVItemValidationForSeaCargoReport(this);
			}

			return new HVLVItemValidation(this);
		}

		bool validatingForSeaCargoReport;

		public IDisposable MarkValidatingForSeaCargoReport()
		{
			return new DisposableAction(
				() => validatingForSeaCargoReport = true,
				() => validatingForSeaCargoReport = false);
		}

		#endregion

		#region ISupportUXMLDataImporting

		bool ISupportUXMLDataImporting.IsUXMLImportingData { get; set; }

		#endregion

		#region UpdateFromScan

		public void UpdateFromScan(UniversalEvent eventObject)
		{
			PopulateItemFromEvent(eventObject);
			UpdateItemStatus();
			UpdateConsignmentHeaderScanStartTime(eventObject.EventTime);
		}

		void PopulateItemFromEvent(UniversalEvent valueObject)
		{
			var context = ((IXmlEventValueObject)valueObject).Context;
			if (context.FailureReason.IsEmpty)
			{
				var referenceParameters = StmALog.GetParametersFromReference(valueObject.EventReference);

				referenceParameters.TryGetValue(EventReferenceCodes.Length, out var rawLength);
				referenceParameters.TryGetValue(EventReferenceCodes.Width, out var rawWidth);
				referenceParameters.TryGetValue(EventReferenceCodes.Height, out var rawHeight);

				referenceParameters.TryGetValue(EventReferenceCodes.Weight, out var rawWeight);

				var length = new MeasurementValueAndUnit(rawLength);
				var width = new MeasurementValueAndUnit(rawWidth);
				var height = new MeasurementValueAndUnit(rawHeight);

				var weight = new MeasurementValueAndUnit(rawWeight);
				var zWeight = new ZWeight(weight.Value, weight.Unit);

				var isValid = zWeight.IsValid && Length.ContainsCode(length.Unit) && Length.ContainsCode(width.Unit) && Length.ContainsCode(height.Unit);

				if (isValid)
				{
					var consignmentWeightUQ = Consignment.HVC_WeightUQ.IsEmpty ? (ZString)HVLVConsignmentSchema.HVC_WeightUQ.SqlDbDefault.ToString() : Consignment.HVC_WeightUQ;
					var itemDimensionUQ = HVI_UnitOfDimension.IsEmpty ? (ZString)Env.Registry.OuterPacklinesMeasurementDefaultUnit : HVI_UnitOfDimension;

					HVI_ActualWeight = zWeight.ConvertTo(consignmentWeightUQ);

					HVI_Length = Length.Convert(length.Value, length.Unit, itemDimensionUQ);
					HVI_Width = Length.Convert(width.Value, width.Unit, itemDimensionUQ);
					HVI_Height = Length.Convert(height.Value, height.Unit, itemDimensionUQ);

					HVI_UnitOfDimension = itemDimensionUQ;
					Consignment.HVC_WeightUQ = consignmentWeightUQ;
				}
			}
		}

		void UpdateItemStatus()
		{
			if (ItemHasEventAdded(AutoEvents.SpecialHandlingRequestedCode) &&
				!ItemHasEventAdded(AutoEvents.SpecialHandlingCompletedCode))
			{
				HVI_Status = HVLVItemStatus.Codes.ReceivedAtDestinationDepotWithException;
			}
			else
			{
				switch (Consignment.HVC_ImportReleaseStatus)
				{
					case HVLVReleaseStatus.Cleared:
						HVI_Status = HVLVItemStatus.Codes.ReadyForLastMileDelivery;
						break;
					case HVLVReleaseStatus.Held:
					case HVLVReleaseStatus.None:
						HVI_Status = HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot;
						break;
					default:
						break;
				}
			}
		}

		void UpdateConsignmentHeaderScanStartTime(ZDateTimeOffset? timeStamp)
		{
			if (timeStamp.HasValue)
			{
				var header = Factory.LoadTop1<HVLVConsignmentHeader>(new ZQuery(HVLVConsignmentHeaderSchema.HCH_ClusterKey, HVI_ClusterKey));
				if (header != null)
				{
					if (header.HCH_ScanStartTime.IsEmpty || header.HCH_ScanStartTime > timeStamp.Value)
					{
						header.HCH_ScanStartTime = timeStamp.Value;
					}
				}
			}
		}

		bool ItemHasEventAdded(string eventCode)
		{
			return Logs.HasLogWith(x => x.SL_SE_NKEvent == eventCode);
		}

		#endregion

		#region MeasureValueAndUnit

		class MeasurementValueAndUnit
		{
			public MeasurementValueAndUnit(string rawValueAndUnit)
			{
				Value = GetValueFromValueAndUnit(rawValueAndUnit);
				Unit = GetUnitFromValueAndUnit(rawValueAndUnit);
			}

			public MeasurementValueAndUnit(ZDecimal value, ZString unit)
			{
				Value = value;
				Unit = unit;
			}

			public ZDecimal Value { get; }
			public ZString Unit { get; }

			static ZDecimal GetValueFromValueAndUnit(ZString valueAndUnit)
			{
				var value = valueAndUnit.KeepCharsUntil("0123456789.", " ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray());
				return ZDecimal.ParseSafe(value, 0);
			}

			static ZString GetUnitFromValueAndUnit(ZString valueAndUnit)
			{
				var value = GetValueFromValueAndUnit(valueAndUnit);
				return valueAndUnit.ReplaceIgnoringCase(value.ToString(), "").Trim();
			}
		}

		#endregion

		#region IHVLVISFItemInfoProvider

		IHVLVISFBillInfoProvider IHVLVISFItemInfoProvider.Consignment => Consignment;

		IHVLVItemLineCollection IHVLVISFItemInfoProvider.Lines => Lines;

		#endregion
	}
}
