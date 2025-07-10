using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business
{
	[DebuggerDisplay(
		"{" + RateEntry.Schema.TI_RateCategory +
		"}-{" + RateEntry.Schema.TI_Mode +
		"}-{" + RateEntry.Schema.TI_OriginLRC +
		"}-{" + RateEntry.Schema.TI_ViaLRC +
		"}-{" + RateEntry.Schema.TI_DestinationLRC +
		"}")]
	public class RateEntry : AutoRateEntry, ISupportDataImporting, IRateEntry, ISalesValueAssociatedEntity
	{
		#region Schema

		public new abstract class Schema : AutoRateEntry.Schema
		{
			public const string TransportProviderCarrierCode = "TransportProviderCarrierCode";
			public const string LocalPortCode = "LocalPortCode";
			public const string OverseasPortPortCode = "OverseasPortPortCode";
			public const string FreightPage = "FreightPage";
			public const string ImportExport = "ImportExport";
			public const string AllWarehouses = "AllWarehouses";
			public const string TI_WW_Warehouse = "TI_WW_Warehouse";
			public const string RateType = "RateType";
			public const string OriginSuburbPK = "OriginSuburbPK";
			public const string DestinationSuburbPK = "DestinationSuburbPK";
			public const string Unit = "Unit";
			public const string IsPublished = "IsPublished";
			public const string ContainerClass = nameof(ContainerClass);
			public const string TI_CYC_WW_Facility = "TI_CYC_WW_Facility";
			public const string CommodityDescription = "CommodityCode+RH_DescriptionMultilingual";
			public const string CommodityLocalCode = "CommodityCode+RatingLocalCode+LC_LocalCode";
		}

		#endregion

		public RateEntry(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
			SetCreationSource();
		}

		#region Properties

		public override bool ReadOnly
		{
			get { return IsDeleted ? base.ReadOnly : base.ReadOnly || DeniedSecurityCheckPoint != null || IsReadOnlyDueToGlobalPublisher; }
			set { base.ReadOnly = value; }
		}

		#region TI_ShipmentConsolidationStatus

		[List("Lookups.ShipmentConsolidationStatusList")]
		public override ZString TI_ShipmentConsolidationStatus
		{
			get { return base.TI_ShipmentConsolidationStatus; }
			set { base.TI_ShipmentConsolidationStatus = value; }
		}

		#endregion

		#region TI_HBLDeliveryMode

		[List("Lookups.HBLDeliveryModeList")]
		public override ZString TI_HBLDeliveryMode
		{
			get { return base.TI_HBLDeliveryMode; }
			set { base.TI_HBLDeliveryMode = value; }
		}

		protected bool TI_HBLDeliveryMode_ReadOnly
		{
			get { return !validRateModesForHBLDeliveryMode.Contains(base.TI_Mode); }
		}

		readonly List<string> validRateModesForHBLDeliveryMode = new List<string>()
		{
			RateMode.FCL, RateMode.LCL, RateMode.BCN, RateMode.BLK, RateMode.BBK, RateMode.ROR, RateMode.LSE, RateMode.ULD ,
			RateMode.SEA, RateMode.ROA, RateMode.RAI, RateMode.FRO, RateMode.FRA,RateMode.AIR, RateMode.LRO, RateMode.LRA
		};

		void UpdateHBLDeliveryMode()
		{
			if (TI_HBLDeliveryMode_ReadOnly)
			{
				TI_HBLDeliveryMode = string.Empty;
			}
		}

		#endregion

		#region TI_FrequencyUnit

		[List("Lookups.FrequencyUnits")]
		public override ZString TI_FrequencyUnit
		{
			get { return base.TI_FrequencyUnit; }
			set { base.TI_FrequencyUnit = value; }
		}

		#endregion

		#region TI_TH

		[RelatedBusinessObject("Parent")]
		public override ZGuid TI_TH
		{
			get { return base.TI_TH; }
			set
			{
				base.TI_TH = value;
				parentOverride = null;
				parentActual = null;
				RateLines.MarkAsNeedingValidationIncludingChildren();
			}
		}

		#endregion

		#region TI_CrossTrade

		public override ZBool TI_IsCrossTrade
		{
			get { return base.TI_IsCrossTrade; }
			set
			{
				base.TI_IsCrossTrade = value;
				if (TI_IsCrossTrade)
				{
					TI_OriginLRC = "";
					TI_DestinationLRC = "";
				}

				Validation.ValidateTI_OriginLRC();
				Validation.ValidateTI_DestinationLRC();
				Validation.ValidateTI_ViaLRC();
			}
		}

		#endregion

		#region TI_Mode

		[List("Lookups.TransportModes")]
		public override ZString TI_Mode
		{
			get { return base.TI_Mode; }
			set
			{
				if (TI_Mode != value)
				{
					base.TI_Mode = value;
					ResetContainerType();
					RateLines.MarkAsNeedingValidation();
					InvalidateRelatedRateLines();

					ReloadCartageZones();

					if (this.IsULD())
					{
						Unit = QuantityUnit.CN;
					}
					else if (TI_Mode == Constants.RateMode.LSE)
					{
						Unit = QuantityUnit.KG;
					}

					if (this.IsContainerTypeAllowed())
					{
						Validation.ValidateTI_RC();
					}

					if (!this.IsAir())
					{
						TI_AircraftType = ZString.Empty;
					}

					if (TI_IsNonOperatedReefer_ReadOnly)
					{
						TI_IsNonOperatedReefer = "";
					}

					UpdateHBLDeliveryMode();
				}
				Validation.ValidateTI_TZ_OriginZone();
				Validation.ValidateTI_TZ_DestinationZone();
			}
		}

		protected bool TI_Mode_ReadOnly
		{
			get { return RateLines.Cast<RateLine>().Any(x => x.Uses(CalculatorType.Equalization) && !this.IsFCL()); }
		}

		void ResetContainerType()
		{
			// Container spot entry can be applied to LCL. Let's keep RefContainer in this case.
			// Without the container, we don't have enough information for merging charges and the first charge found will be picked instead.
			if (!IsSpotEntry && !this.IsContainerTypeAllowed())
			{
				TI_RC = ZGuid.Empty;
			}
		}

		#endregion

		#region TI_RateCategory

		public override ZString TI_RateCategory
		{
			get { return base.TI_RateCategory; }
			set
			{
				if (TI_RateCategory != value)
				{
					base.TI_RateCategory = value;
					RateLines.MarkAsNeedingValidation();
					foreach (RateLine rateLine in RateLines)
					{
						rateLine.RateLineItems.MarkAsNeedingValidation();
					}
				}
			}
		}

		#endregion

		#region TI_OriginLRC

		[List("Lookups.Locations")]
		public override ZString TI_OriginLRC
		{
			get { return base.TI_OriginLRC; }
			set
			{
				if (base.TI_OriginLRC != value)
				{
					base.TI_OriginLRC = value;
					SetSalesCurrency();
					InvalidateRelatedRateLines();
					Validation.ValidateTI_DestinationLRC();
					Validation.ValidateTI_RateOrigin();
					Validation.ValidateTI_RateDestination();
					if (this.IsIntercompanyTariff())
					{
						Validation.ValidateTI_PlannedLoadLRC();
						Validation.ValidateTI_PlannedDischargeLRC();
					}
					else
					{
						Validation.ValidateTI_ViaLRC();
					}

					Validation.ValidateLocation();
					RateLines.MarkAsNeedingValidation();

					if (this.IsOriginEntry())
					{
						ReloadCartageZones();
					}
				}
			}
		}

		protected bool TI_OriginLRC_ReadOnly
		{
			get { return !IsDeleted && TI_IsCrossTrade; }
		}

		#endregion

		#region TI_IsNonOperatedReefer

		/// <summary>
		/// This can be either blank, "Y", or "N".
		/// </summary>
		[List("Lookups.IsNonOperationalReefer")]
		public override ZString TI_IsNonOperatedReefer
		{
			get
			{
				return base.TI_IsNonOperatedReefer;
			}
			set
			{
				if (TI_IsNonOperatedReefer != value)
				{
					base.TI_IsNonOperatedReefer = value;
					InvalidateRelatedRateLines();
				}
			}
		}

		protected bool TI_IsNonOperatedReefer_ReadOnly
		{
			get
			{
				var fWDAndCUSValidModes = new List<string> { "FCL", "BCN", "SCN", "FRO", "FRA" };
				var cFSPackingValidModes = new List<string> { "FCL", "FRO", "ROR", "FRA" };
				if (this.IsForwarding() || this.IsCustoms())
				{
					return (!IsDeleted && !fWDAndCUSValidModes.Any(c => c == TI_Mode)) && !this.IsFCL();
				}
				if (this.IsShipping())
				{
					return (this.IsOriginEntry() || this.IsDestinationEntry()) && (!IsDeleted && TI_Mode != "FCL");
				}
				if (this.IsCFS())
				{
					return !this.IsFCL() && !IsDeleted && !cFSPackingValidModes.Any(c => c == TI_Mode);
				}
				return false;
			}
		}

		#endregion

		#region TI_ContractNumber

		public override ZString TI_ContractNumber
		{
			get { return base.TI_ContractNumber; }
			set
			{
				if (TI_ContractNumber != value)
				{
					base.TI_ContractNumber = value;
					InvalidateRelatedRateLines();
				}
			}
		}

		protected bool TI_ContractNumber_ReadOnly => TI_ContractNumberLinked;

		#endregion

		#region TI_FMCTariffID

		public override ZString TI_FMCTariffID
		{
			get { return base.TI_FMCTariffID; }
			set
			{
				if (TI_FMCTariffID != value)
				{
					base.TI_FMCTariffID = value;
					InvalidateRelatedRateLines();
				}
			}
		}

		#endregion

		#region TI_ContractNumberLinked

		protected bool TI_ContractNumberLinked_ReadOnly =>
			ContractNumberLinkedSavedSinceLinking || !ObjectFactory.Get<IContractPermissions>().IsCarrierAndClientContractModulesEnabled();

		// 1. When the user makes a new RateEntry and sets TI_ContractNumberLinked = true; then it is still write-able.
		// 2. When the user edits an existing RateEntry and sets TI_ContractNumberLinked = true; then it is still write-able.
		// 3. Extending 1 or 2, if the user then presses Save, it is no longer writable.
		bool ContractNumberLinkedSavedSinceLinking => !TI_ContractNumberLinkedInfo.HasChangesOrIsNew() && TI_ContractNumberLinked;

		#endregion

		#region TI_DestinationLRC

		[List("Lookups.Locations")]
		public override ZString TI_DestinationLRC
		{
			get { return base.TI_DestinationLRC; }
			set
			{
				if (base.TI_DestinationLRC != value)
				{
					base.TI_DestinationLRC = value;
					SetSalesCurrency();
					InvalidateRelatedRateLines();
					Validation.ValidateTI_OriginLRC();
					Validation.ValidateTI_RateOrigin();
					Validation.ValidateTI_RateDestination();
					if (this.IsIntercompanyTariff())
					{
						Validation.ValidateTI_PlannedLoadLRC();
						Validation.ValidateTI_PlannedDischargeLRC();
					}
					else
					{
						Validation.ValidateTI_ViaLRC();
					}

					RateLines.MarkAsNeedingValidation();

					foreach (RateLine rateLine in RateLines)
					{
						rateLine.RateLineItems.MarkAsNeedingValidation();
					}

					if (this.IsDestinationEntry())
					{
						ReloadCartageZones();
					}
				}
			}
		}

		protected bool TI_DestinationLRC_ReadOnly
		{
			get { return !IsDeleted && TI_IsCrossTrade; }
		}

		#endregion

		#region TI_TZ_OriginZone, TI_TZ_DestinationZone

		[List("Lookups.TransportZones")]
		public override ZGuid TI_TZ_OriginZone
		{
			get { return base.TI_TZ_OriginZone; }
			set { base.TI_TZ_OriginZone = value; }
		}

		[List("Lookups.TransportZones")]
		public override ZGuid TI_TZ_DestinationZone
		{
			get { return base.TI_TZ_DestinationZone; }
			set { base.TI_TZ_DestinationZone = value; }
		}

		#endregion

		#region OriginSuburbPK, DestinationSuburbPK

		[List("Lookups.Suburbs")]
		[ResourceStringData("RateEntry|OriginSuburbPK", Caption = "From Suburb")]
		public ZGuid OriginSuburbPK
		{
			get => GetLocation(isOrigin: true);
			set => SetLocation(isOrigin: true, value, OriginSuburbPKInfo);
		}

		public ZPropertyInfo OriginSuburbPKInfo => GetZPropertyInfo(Schema.OriginSuburbPK);

		[List("Lookups.Suburbs")]
		[ResourceStringData("RateEntry|DestinationSuburbPK", Caption = "To Suburb")]
		public ZGuid DestinationSuburbPK
		{
			get => GetLocation(isOrigin: false);
			set => SetLocation(isOrigin: false, value, DestinationSuburbPKInfo);
		}

		public ZPropertyInfo DestinationSuburbPKInfo => GetZPropertyInfo(Schema.DestinationSuburbPK);

		void SetLocation(bool isOrigin, ZGuid value, ZPropertyInfo info)
		{
			if (isOrigin)
			{
				TI_R9_FromSuburb = value;
			}
			else
			{
				TI_R9_ToSuburb = value;
			}

			info.RefreshBinding();
			if (!IsValidationSuspended)
			{
				Validation.ValidateLocation(info);
			}
		}

		ZGuid GetLocation(bool isOrigin) => isOrigin ? TI_R9_FromSuburb : TI_R9_ToSuburb;

		#endregion

		#region Sales Currency

		class SalesCurrencySetDelayer : IDisposable
		{
			public SalesCurrencySetDelayer(RateEntry entry)
			{
				this.entry = entry;
				entry.skipSalesCurrencySetter = true;
			}

			readonly RateEntry entry;

			public void Dispose()
			{
				entry.skipSalesCurrencySetter = false;
				entry.SetSalesCurrency();
			}
		}

		bool skipSalesCurrencySetter;

		public IDisposable DelaySettingSalesCurrency()
		{
			return new SalesCurrencySetDelayer(this);
		}

		void SetSalesCurrency()
		{
			if (!skipSalesCurrencySetter && !IsCopying)
			{
				switch (TI_RateCategory)
				{
					case RatingConstants.RateCategory.AIR:
					case RatingConstants.RateCategory.ORG:
					case RatingConstants.RateCategory.CAI:
					case RatingConstants.RateCategory.COR:
					case RatingConstants.RateCategory.SOR:
					case RatingConstants.RateCategory.SED:
						SetSalesCurrency(this.Origin());
						break;

					case RatingConstants.RateCategory.FCL:
					case RatingConstants.RateCategory.LCL:
					case RatingConstants.RateCategory.CFC:
					case RatingConstants.RateCategory.CLC:
						if (TI_RX_NKCurrency.IsEmpty)
						{
							TI_RX_NKCurrency = Constants.CurrencyCodes.UnitedStates;
						}

						break;

					case RatingConstants.RateCategory.DST:
					case RatingConstants.RateCategory.CDS:
					case RatingConstants.RateCategory.SDE:
					case RatingConstants.RateCategory.SID:
						SetSalesCurrency(this.Destination());
						break;
				}
			}
		}

		void SetSalesCurrency(ILocation location)
		{
			if (location != null)
			{
				TI_RX_NKCurrency = location.Country != null
									   ? location.Country.RN_RX_NKLocalCurrency
									   : LocalCurrency.RX_Code;
			}
		}

		RefCurrency LocalCurrency
		{
			get { return this.Company()?.LocalCurrency ?? RefCurrency.LoadFromCurrencyCode(Factory, Constants.CurrencyCodes.UnitedStates); }
		}

		#endregion

		#region TI_ViaLRC

		[List("Lookups.Locations")]
		public override ZString TI_ViaLRC
		{
			get { return base.TI_ViaLRC; }
			set
			{
				base.TI_ViaLRC = value;
				InvalidateRelatedRateLines();
				Validation.ValidateTI_OriginLRC();
				Validation.ValidateTI_DestinationLRC();
				Validation.ValidateTI_RateOrigin();
				Validation.ValidateTI_RateDestination();
			}
		}

		#endregion

		#region TI_PlannedLoadLRC

		[List("Lookups.Locations")]
		public override ZString TI_PlannedLoadLRC
		{
			get { return base.TI_PlannedLoadLRC; }
			set
			{
				if (base.TI_PlannedLoadLRC != value)
				{
					base.TI_PlannedLoadLRC = value;
					if (this.IsIntercompanyTariff())
					{
						Validation.ValidateTI_OriginLRC();
						Validation.ValidateTI_DestinationLRC();
						Validation.ValidateTI_PlannedDischargeLRC();
					}
					Validation.ValidateTI_RateOrigin();
					Validation.ValidateTI_RateDestination();

					Validation.ValidateLocation();
					RateLines.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region TI_PlannedDischargeLRC

		[List("Lookups.Locations")]
		public override ZString TI_PlannedDischargeLRC
		{
			get { return base.TI_PlannedDischargeLRC; }
			set
			{
				if (base.TI_PlannedDischargeLRC != value)
				{
					base.TI_PlannedDischargeLRC = value;
					if (this.IsIntercompanyTariff())
					{
						Validation.ValidateTI_OriginLRC();
						Validation.ValidateTI_DestinationLRC();
						Validation.ValidateTI_PlannedLoadLRC();
					}
					Validation.ValidateTI_RateOrigin();
					Validation.ValidateTI_RateDestination();

					Validation.ValidateLocation();
					RateLines.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region RateEntryLocations

		[ChildEditable(true)]
		public RateEntryLocationCollection RateEntryLocations
		{
			get
			{
				if (rateEntryLocations == null)
				{
					rateEntryLocations = new RateEntryLocationCollection(this);
					RegisterEditableChildObject(RateEntryLocations);
				}

				return rateEntryLocations;
			}
		}

		internal bool RateEntryLocationFieldsSuspended { get; set; }
		bool IsRateEntryLocationsLoaded => rateEntryLocations != null;
		bool CanUpdateRateEntryLocationFields =>
			IsRateEntryLocationsLoaded && !RateEntryLocationFieldsSuspended;

		RateEntryLocationCollection rateEntryLocations;

		#endregion

		#region TI_FirstLoadLRC

		[List("Lookups.Locations")]
		public override ZString TI_FirstLoadLRC
		{
			get => base.TI_FirstLoadLRC;
			set
			{
				if (base.TI_FirstLoadLRC != value)
				{
					base.TI_FirstLoadLRC = value;

					if (CanUpdateRateEntryLocationFields)
					{
						RateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.FirstLoad, base.TI_FirstLoadLRC);
					}
				}
			}
		}

		#endregion

		#region TI_LastDischargeLRC

		[List("Lookups.Locations")]
		public override ZString TI_LastDischargeLRC
		{
			get => base.TI_LastDischargeLRC;
			set
			{
				if (base.TI_LastDischargeLRC != value)
				{
					base.TI_LastDischargeLRC = value;

					if (CanUpdateRateEntryLocationFields)
					{
						RateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.LastDischarge, base.TI_LastDischargeLRC);
					}
				}
			}
		}

		#endregion

		#region TI_FirstRouteSetLoadPortLRC

		[List("Lookups.Locations")]
		public override ZString TI_FirstRouteSetLoadPortLRC
		{
			get => base.TI_FirstRouteSetLoadPortLRC;
			set
			{
				if (base.TI_FirstRouteSetLoadPortLRC != value)
				{
					base.TI_FirstRouteSetLoadPortLRC = value;

					if (CanUpdateRateEntryLocationFields)
					{
						RateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.FirstRouteSetLoad, base.TI_FirstRouteSetLoadPortLRC);
					}
				}
			}
		}

		#endregion

		#region TI_LastRouteSetDischargePortLRC

		[List("Lookups.Locations")]
		public override ZString TI_LastRouteSetDischargePortLRC
		{
			get => base.TI_LastRouteSetDischargePortLRC;
			set
			{
				if (base.TI_LastRouteSetDischargePortLRC != value)
				{
					base.TI_LastRouteSetDischargePortLRC = value;

					if (CanUpdateRateEntryLocationFields)
					{
						RateEntryLocations.UpdateLocation(RateEntryLookups.LocationSourceOption.Code.LastRouteSetDischarge, base.TI_LastRouteSetDischargePortLRC);
					}
				}
			}
		}

		#endregion

		#region TI_RateOrigin

		[List("Lookups.Locations")]
		public override ZString TI_RateOrigin
		{
			get { return base.TI_RateOrigin; }
			set
			{
				if (base.TI_RateOrigin != value)
				{
					base.TI_RateOrigin = value;

					if (this.IsIntercompanyTariff())
					{
						Validation.ValidateTI_PlannedLoadLRC();
						Validation.ValidateTI_PlannedDischargeLRC();
					}
					Validation.ValidateTI_OriginLRC();
					Validation.ValidateTI_DestinationLRC();
					Validation.ValidateTI_RateDestination();
					Validation.ValidateTI_ViaLRC();

					Validation.ValidateLocation();
					RateLines.MarkAsNeedingValidation();
				}
			}
		}

		/// <summary>
		/// A child RateLine has changed such that this entry and all its RateLines should be marked as needing validation.
		/// Similary purpose as the BusinessObject.MarkAsNeedingValidationIncludingChildren, but only marks RateLines (once).
		/// </summary>
		internal void MarkAsNeedingValidationForRateLineChange()
		{
			MarkAsNeedingValidation();
			rateLines?.MarkAsNeedingValidationIncludingChildren();
		}

		#endregion

		#region TI_RateDestination

		[List("Lookups.Locations")]
		public override ZString TI_RateDestination
		{
			get { return base.TI_RateDestination; }
			set
			{
				if (base.TI_RateDestination != value)
				{
					base.TI_RateDestination = value;

					if (this.IsIntercompanyTariff())
					{
						Validation.ValidateTI_PlannedLoadLRC();
						Validation.ValidateTI_PlannedDischargeLRC();
					}

					Validation.ValidateTI_OriginLRC();
					Validation.ValidateTI_DestinationLRC();
					Validation.ValidateTI_RateOrigin();
					Validation.ValidateTI_ViaLRC();

					Validation.ValidateLocation();
					RateLines.MarkAsNeedingValidation();
				}
			}
		}

		#endregion

		#region TI_RC
		[List("Lookups.Containers")]
		public override ZGuid TI_RC
		{
			get { return base.TI_RC; }
			set
			{
				if (base.TI_RC != value)
				{
					RateLines.MarkAsNeedingValidation();
					base.TI_RC = value;
					InvalidateRelatedRateLines();
					Validation.ValidateTI_MatchContainerRateClass();
				}
			}
		}

		protected bool TI_RC_ReadOnly
		{
			get
			{
				return !this.IsContainerTypeAllowed();
			}
		}

		#endregion

		#region TI_MatchContainerRateClass

		protected bool TI_MatchContainerRateClass_ReadOnly
		{
			get { return !this.IsContainerTypeAllowed(); }
		}

		#endregion

		#region TI_RX_NKCurrency

		[List("Lookups.Currencies")]
		public override ZString TI_RX_NKCurrency
		{
			get { return base.TI_RX_NKCurrency; }
			set
			{
				base.TI_RX_NKCurrency = value;
				foreach (RateLine line in RateLines)
				{
					line.TL_RX_NKCurrency = TI_RX_NKCurrency;
				}
			}
		}

		#endregion

		#region TI_OH_Consignor / TI_OH_Consignee

		[List("Lookups.Consignors")]
		public override ZGuid TI_OH_Consignor
		{
			get { return base.TI_OH_Consignor; }
			set
			{
				base.TI_OH_Consignor = value;
				Lookups.InvalidateCartageAddresses(RateEntrySchema.TI_OH_Consignor);
				TI_OA_CartagePickupAddressOverride_ZAddress.SetOrgWithoutSettingDefaultAddress(TI_OH_Consignor);
				if (TI_OH_Consignor.IsEmpty)
				{
					TI_OA_CartagePickupAddressOverride = ZGuid.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateTI_OriginLRC();
					Validation.ValidateTI_DestinationLRC();
				}
			}
		}

		[List("Lookups.Consignees")]
		public override ZGuid TI_OH_Consignee
		{
			get { return base.TI_OH_Consignee; }
			set
			{
				base.TI_OH_Consignee = value;
				Lookups.InvalidateCartageAddresses(RateEntrySchema.TI_OH_Consignee);
				TI_OA_CartageDeliveryAddressOverride_ZAddress.SetOrgWithoutSettingDefaultAddress(TI_OH_Consignee);
				if (TI_OH_Consignee.IsEmpty)
				{
					TI_OA_CartageDeliveryAddressOverride = ZGuid.Empty;
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateTI_OriginLRC();
					Validation.ValidateTI_DestinationLRC();
				}
			}
		}

		#endregion

		#region TI_RH_NKCommodityCode

		[List("Lookups.CommodityCodes")]
		public override ZString TI_RH_NKCommodityCode
		{
			get { return base.TI_RH_NKCommodityCode; }
			set
			{
				if (base.TI_RH_NKCommodityCode != value)
				{
					base.TI_RH_NKCommodityCode = value;
					RateLines.MarkAsNeedingValidation();
					InvalidateRelatedRateLines();
				}
			}
		}

		#endregion

		#region TI_RS_NKServiceLevel_NI

		[List("Lookups.ServiceLevel_NIs")]
		public override ZString TI_RS_NKServiceLevel_NI
		{
			get { return base.TI_RS_NKServiceLevel_NI; }
			set
			{
				base.TI_RS_NKServiceLevel_NI = value;
				InvalidateRelatedRateLines();
			}
		}

		#endregion

		#region TI_PL_NKCarrierServiceLevel

		[List("Lookups.CarrierServiceLevels")]
		public override ZString TI_PL_NKCarrierServiceLevel
		{
			get { return base.TI_PL_NKCarrierServiceLevel; }
			set { base.TI_PL_NKCarrierServiceLevel = value; }
		}

		#endregion

		#region TI_RateEndDate

		public override ZDate TI_RateEndDate
		{
			get { return base.TI_RateEndDate; }
			set
			{
				base.TI_RateEndDate = value;
				RateLines.MarkAsNeedingValidation();
				InvalidateRelatedRateLines();
			}
		}

		#endregion

		#region TI_TransitTime

		[List("Lookups.AirTransitTimes")]
		public override ZString TI_TransitTime
		{
			get { return base.TI_TransitTime; }
			set { base.TI_TransitTime = value; }
		}

		#endregion

		#region TI_OH_TransportProvider

		[List("Lookups.ShippingProviders")]
		public override ZGuid TI_OH_TransportProvider
		{
			get { return base.TI_OH_TransportProvider; }
			set
			{
				base.TI_OH_TransportProvider = value;
				InvalidateRelatedRateLines();
				if (!IsValidationSuspended)
				{
					Validation.ValidateTI_OriginLRC();
					Validation.ValidateTI_DestinationLRC();
				}
			}
		}

		#endregion

		#region TI_OH_Supplier

		[List("Lookups.Suppliers")]
		public override ZGuid TI_OH_Supplier
		{
			get { return base.TI_OH_Supplier; }
			set
			{
				base.TI_OH_Supplier = value;
				InvalidateRelatedRateLines();
				ReloadCartageZones();
				if (!IsValidationSuspended)
				{
					Validation.ValidateTI_OriginLRC();
					Validation.ValidateTI_DestinationLRC();
					Validation.ValidateTI_TZ_OriginZone();
					Validation.ValidateTI_TZ_DestinationZone();
				}
			}
		}

		#endregion

		#region TI_OH_AgentOverride

		[List("Lookups.AgentOverrides")]
		public override ZGuid TI_OH_AgentOverride
		{
			get { return base.TI_OH_AgentOverride; }
			set { base.TI_OH_AgentOverride = value; }
		}

		#endregion

		#region TI_QuotePageIncoTerm

		[List("Lookups.IncoTerms")]
		public override ZString TI_QuotePageIncoTerm
		{
			get { return base.TI_QuotePageIncoTerm; }
			set { base.TI_QuotePageIncoTerm = value; }
		}

		protected bool TI_QuotePageIncoTerm_ReadOnly =>
			!this.IsQuote()
			|| this.IsOneOffQuote()
			|| !TI_RateCategory.ToString().In(RatingConstants.RateCategory.AIR, RatingConstants.RateCategory.FCL, RatingConstants.RateCategory.LCL, RatingConstants.RateCategory.SCO, RatingConstants.RateCategory.SNC);

		#endregion

		#region TI_GC_Publisher

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		public sealed override ZGuid TI_GC_Publisher
		{
			get { return base.TI_GC_Publisher; }
			set
			{
				if (!value.IsEmpty)
				{
					base.TI_GC_Publisher = value;

					#region SuppressResourceStringsCheckRegion

					if (Parent != null && !Parent.TH_GC.IsEmpty && Parent.TH_GC != value)
					{
						var message = Invariant($@"Rate Entry Publisher should never be a different company to the RatingHeader's Company when the TH_GC is set.
TH_GC = {Parent.Company?.GC_Code ?? "Invalid"} ('{Parent.TH_GC}')
Publisher= {Publisher?.GC_Code ?? "Invalid"} ('{value}')");

						throw new DeveloperNotificationException(message);
					}

					#endregion
				}
			}
		}

		public bool IsReadOnlyDueToGlobalPublisher
		{
			get { return this.IsGlobal() && TI_GC_Publisher != Env.CurrentCompanyPK && Parent != null && !CanEditFromAnyCompany; }
		}

		bool CanEditFromAnyCompany => Factory.GetCachedValue(CanEditFromAnyCompanyKey, SecurityAllowsEditingFromAnyCompany);
		bool SecurityAllowsEditingFromAnyCompany()
		{
			if (Parent.IsGlobal())
			{
				if (Parent.IsTariff())
				{
					return Env.Security.GlobalTariffRatesEditFromAnyCompany.IsAllowed;
				}

				if (Parent.IsCosting())
				{
					return Env.Security.GlobalCostingRatesEditFromAnyCompany.IsAllowed;
				}

				if (Parent.IsClientRate())
				{
					return Env.Security.GlobalClientRatesEditFromAnyCompany.IsAllowed;
				}
			}

			return true;
		}

		#region SuppressResourceStringsCheckRegion

		string CanEditFromAnyCompanyKey
		{
			get { return Invariant($"CanEditFromAnyCompany.{TI_TH}"); }
		}

		#endregion

		#endregion

		#region TI_GatewayAgentType

		[List("Lookups.GatewayAgentTypes")]
		public override ZString TI_GatewayAgentType
		{
			get { return base.TI_GatewayAgentType; }
			set { base.TI_GatewayAgentType = value; }
		}

		#endregion

		#region TI_PaymentTerm
		[List("Lookups.PaymentTerms")]
		public override ZString TI_PaymentTerm
		{
			get { return base.TI_PaymentTerm; }
			set { base.TI_PaymentTerm = value; }
		}
		#endregion

		#region TI_AircraftType

		[List("Lookups.AircraftTypes")]
		public override ZString TI_AircraftType
		{
			get { return base.TI_AircraftType; }
			set { base.TI_AircraftType = value; }
		}

		protected bool TI_AircraftType_ReadOnly
		{
			get { return !this.IsAir(); }
		}

		#endregion

		#region TI_YardUnitType

		[List("Lookups.YardUnitTypes")]
		public override ZString TI_YardUnitType
		{
			get { return base.TI_YardUnitType; }
			set
			{
				base.TI_YardUnitType = value;
				if (value != ContainerYardConstants.YardUnitType.Codes.CNT)
				{
					TI_YardUnitLoad = string.Empty;
				}
				if (this.IsYardUnitTypeEmpty())
				{
					TI_RC = ZGuid.Empty;
				}
			}
		}

		#endregion

		#region TI_YardUnitType

		[List("Lookups.ContainerUnitSections")]
		public override ZString TI_ContainerUnitSection
		{
			get { return base.TI_ContainerUnitSection; }
			set
			{
				base.TI_ContainerUnitSection = value;
			}
		}

		#endregion

		#region TI_YardUnitLoad

		[List("Lookups.YardUnitLoads")]
		public override ZString TI_YardUnitLoad
		{
			get { return base.TI_YardUnitLoad; }
			set { base.TI_YardUnitLoad = value; }
		}

		protected bool TI_YardUnitLoad_ReadOnly
		{
			get { return TI_YardUnitType != ContainerYardConstants.YardUnitType.Codes.CNT; }
		}

		#endregion

		#region IsPublished

		[BusinessObjectTestExclude]
		public ZBool IsPublished
		{
			get
			{
				return TI_TH == Parent?.GlobalRatingHeader?.PK;
			}
			set
			{
				if (value)
				{
					if (Parent.GlobalRatingHeader == null || Parent.GlobalRatingHeader.IsDeleted)
					{
						new RateCreator(Parent).LoadOrCreateGlobalRatingHeader(Factory);
					}

					if (Parent.GlobalRatingHeader != null)
					{
						TI_TH = Parent.GlobalRatingHeader.PK;
						foreach (var rateLine in RateLines.OfType<RateLine>())
						{
							rateLine.ConvertChargeCode(value);
						}
					}
				}
				else
				{
					TI_TH = Parent.PK;
					foreach (var rateLine in RateLines.OfType<RateLine>())
					{
						rateLine.ConvertChargeCode(value);
					}
				}

				IsPublishedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsPublishedInfo
		{
			get { return GetZPropertyInfo(Schema.IsPublished); }
		}

		protected bool IsPublished_ReadOnly
		{
			get { return Factory.GetCachedValue(IsPublishedReadOnlyKey, IsRateEntryPublishDisallowed); }
		}

		bool IsRateEntryPublishDisallowed()
		{
			if (!Parent.SupportsRateEntryPublish())
			{
				return true;
			}

			if (Parent.IsCosting() && !Env.Security.GlobalCostingRatesPublishAndUnpublish.IsAllowed)
			{
				return true;
			}

			if (Parent.IsClientRate() && !Env.Security.GlobalClientRatesPublishAndUnpublish.IsAllowed)
			{
				return true;
			}

			if (Parent.IsLevelOneTariff() && !Env.Security.GlobalTariffRatesPublishAndUnpublish.IsAllowed)
			{
				return true;
			}

			return false;
		}

		#region SuppressResourceStringsCheckRegion

		string IsPublishedReadOnlyKey
		{
			get { return Invariant($"IsRateEntryPublishDisallowed.{TI_TH}"); }
		}

		#endregion

		#endregion

		#region ContainerClass

		public ZString ContainerClass
		{
			get
			{
				if (Container == null)
				{
					return ZString.Empty;
				}

				return this.IsFreightEntry() ? Container.RC_FreightRateClass : (TI_RateCategory == RatingConstants.RateCategory.CST ? Container.RC_StorageClass : Container.RC_HandlingRateClass);
			}
		}

		#endregion

		#region Pickup / Delivery Address

		[List("Lookups.CartageDeliveryAddressOverrides")]
		public override ZGuid TI_OA_CartageDeliveryAddressOverride
		{
			get { return base.TI_OA_CartageDeliveryAddressOverride; }
			set
			{
				base.TI_OA_CartageDeliveryAddressOverride = value;
				TI_CartageDeliveryAddressPostCode = ZString.Empty;

				if (this.IsDestinationEntry())
				{
					ReloadCartageZones();
				}
			}
		}

		protected bool TI_OA_CartageDeliveryAddressOverride_ReadOnly
		{
			get { return Consignee == null; }
		}

		public override ZString TI_CartageDeliveryAddressPostCode
		{
			get
			{
				return CartageDeliveryAddressOverride != null
							? CartageDeliveryAddressOverride.OA_PostCode
							: base.TI_CartageDeliveryAddressPostCode;
			}
			set
			{
				base.TI_CartageDeliveryAddressPostCode = value;
			}
		}

		protected bool TI_CartageDeliveryAddressPostCode_ReadOnly
		{
			get { return CartageDeliveryAddressOverride != null; }
		}

		[List("Lookups.CartagePickupAddressOverrides")]
		public override ZGuid TI_OA_CartagePickupAddressOverride
		{
			get { return base.TI_OA_CartagePickupAddressOverride; }
			set
			{
				base.TI_OA_CartagePickupAddressOverride = value;
				TI_CartagePickupAddressPostCode = ZString.Empty;

				if (this.IsOriginEntry())
				{
					ReloadCartageZones();
				}
			}
		}

		protected bool TI_OA_CartagePickupAddressOverride_ReadOnly
		{
			get { return Consignor == null; }
		}

		public override ZString TI_CartagePickupAddressPostCode
		{
			get
			{
				return CartagePickupAddressOverride != null
							? CartagePickupAddressOverride.OA_PostCode
							: base.TI_CartagePickupAddressPostCode;
			}
			set
			{
				base.TI_CartagePickupAddressPostCode = value;
			}
		}

		protected bool TI_CartagePickupAddressPostCode_ReadOnly
		{
			get { return CartagePickupAddressOverride != null; }
		}

		protected override ZAddress GetNewTI_OA_CartageDeliveryAddressOverride_ZAddress()
		{
			var result = base.GetNewTI_OA_CartageDeliveryAddressOverride_ZAddress();
			result.DefaultAddressType = AddressType.DLV;
			return result;
		}

		protected override ZAddress GetNewTI_OA_CartagePickupAddressOverride_ZAddress()
		{
			var result = base.GetNewTI_OA_CartagePickupAddressOverride_ZAddress();
			result.DefaultAddressType = AddressType.PIC;
			return result;
		}

		#endregion

		#region TransportProviderCarrierCode

		public ZString TransportProviderCarrierCode
		{
			get
			{
				ZString result = "";
				if (TI_OH_TransportProvider.IsValid)
				{
					if (TransportProvider.MiscServ.Airline != null)
					{
						result = TransportProvider.MiscServ.Airline.RM_TwoCharacterCode;
					}
				}
				return result;
			}
		}

		public ZPropertyInfo TransportProviderCarrierCodeInfo
		{
			get { return GetZPropertyInfo(Schema.TransportProviderCarrierCode); }
		}

		#endregion

		#region OriginZone

		public RateTransportZone OriginZone
		{
			get { return Factory.Load<RateTransportZone>(TI_TZ_OriginZone); }
		}

		#endregion

		#region DestinationZone

		public RateTransportZone DestinationZone
		{
			get { return Factory.Load<RateTransportZone>(TI_TZ_DestinationZone); }
		}

		#endregion

		#region Via

		[List("Lookups.Locations")]
		public ILocation Via
		{
			get
			{
				if (!TI_ViaLRC.IsValid || TI_ViaLRC.IsEmpty)
				{
					via = null;
				}
				else if (via == null || TI_ViaLRC != via.Code)
				{
					via = LocationHelper.GetCachedLocationFromString(TI_ViaLRC, Factory);
				}
				return via;
			}
		}
		ILocation via;

		#endregion

		#region Local/Overseas Port

		public ILocation LocalPort
		{
			get
			{
				var isOriginLocal = !this.OriginCountryCode().IsEmpty && ImportExportHelper.IsBranchCountry(this.OriginCountryCode());
				var isDestinationLocal = !DestinationCountry.IsEmpty && ImportExportHelper.IsBranchCountry(DestinationCountry);

				if (isOriginLocal && isDestinationLocal)
				{
					return null;
				}

				if (isOriginLocal)
				{
					return this.Origin();
				}

				if (isDestinationLocal)
				{
					return this.Destination();
				}

				return null;
			}
		}

		public ILocation OverseasPort
		{
			get
			{
				var isOriginNotLocal = !this.OriginCountryCode().IsEmpty && !ImportExportHelper.IsBranchCountry(this.OriginCountryCode());
				var isDestinationNotLocal = !DestinationCountry.IsEmpty && !ImportExportHelper.IsBranchCountry(DestinationCountry);

				if (isOriginNotLocal && isDestinationNotLocal)
				{
					return null;
				}

				if (isOriginNotLocal)
				{
					return this.Origin();
				}

				if (isDestinationNotLocal)
				{
					return this.Destination();
				}

				return null;
			}
		}

		public ZString LocalPortCode
		{
			get { return LocalPort == null ? ZString.Empty : LocalPort.Code; }
		}

		public ZString OverseasPortCode
		{
			get { return OverseasPort == null ? ZString.Empty : OverseasPort.Code; }
		}

		#endregion

		#region Unit

		[ReadOnly(true)]
		[BusinessObjectTestExclude]
		[List("Lookups.Units")]
		[MaxLength(3)]
		public ZString Unit
		{
			get
			{
				if (unit.IsEmpty)
				{
					unit = GetDefaultUnit(TI_RateCategory);
				}

				return unit;
			}
			set
			{
				if (unit != value)
				{
					SetNonPersistentPropertyValue(UnitInfo, ref unit, value);

					foreach (RateLine line in RateLines)
					{
						if (!line.TL_WeightVolumeInfo.ReadOnly)
						{
							line.TL_WeightVolume = unit;
						}
					}
				}
			}
		}

		ZString unit;

		public ZPropertyInfo UnitInfo
		{
			get { return GetZPropertyInfo(Schema.Unit); }
		}

		public static ZString GetDefaultUnit(string rateCategory)
		{
			switch (rateCategory)
			{
				case RatingConstants.RateCategory.AIR:
				case RatingConstants.RateCategory.CAI:
					return Env.Registry.FreightWeightUnit;

				case RatingConstants.RateCategory.SCO:
				case RatingConstants.RateCategory.FCL:
				case RatingConstants.RateCategory.CFC:
					return RatingConstants.Units.CN;

				case RatingConstants.RateCategory.SNC:
				case RatingConstants.RateCategory.LCL:
				case RatingConstants.RateCategory.CLC:
					return Env.Registry.FreightVolumeUnit;

				case RatingConstants.RateCategory.SID:
				case RatingConstants.RateCategory.SED:
					return RatingConstants.Units.DY;

				default:
					return ZString.Empty;
			}
		}

		#endregion

		#region CompanyTariffDiscount

		public ZDecimal GetCompanyTariffDiscount(RatingHeader header)
		{
			var tariff = header as CompanyTariff;
			if (tariff != null)
			{
				return tariff.Discounts.GetDiscount(TI_RateCategory, TI_RS_NKServiceLevel_NI);
			}

			return 0m;
		}

		public ZDecimal GetCompanyTariffDiscountForLevel(ZByte level)
		{
			var filter = new ZQuery(RatingHeaderSchema.TH_GlobalRateLevel, level);
			filter.AddToFilter(RatingHeaderSchema.TH_RateType, (ZString)RatingConstants.RatingHeaderTypes.Tariff);

			if (Parent.TH_GC.IsEmpty)
			{
				filter.AddToFilter(RatingHeaderSchema.TH_GC, null);
			}
			else
			{
				filter.AddToFilter(RatingHeaderSchema.TH_GC, Parent.TH_GC);
			}

			var tariff = Factory.LoadTop1<CompanyTariff>(filter);

			return GetCompanyTariffDiscount(tariff);
		}

		#endregion

		#region Properites For Printing

		#region FreightType

		public ZString FreightType
		{
			get
			{
				var code = FreightTypeList.CodeFromRateMode(TI_RateCategory, TI_Mode);
				return code == null ? TI_Mode : (ZString)Factory.GetCachedValue<FreightTypeList>().GetDescriptionFromCode(code);
			}
		}

		#endregion

		#region Index Page Description

		public ZString QuotationIndexDescription
		{
			get { return QuotationIndexDescriptionInternal(ZString.Empty); }
		}

		protected ZString QuotationIndexDescriptionInternal(ZString prefix)
		{
			var result = prefix;
			if (OriginForDescription != null)
			{
				result += LocationHelper.GetFullName(OriginForDescription, true);
			}

			if (DestinationForDescription != null)
			{
				if (!result.IsEmpty)
				{
					result += " " + Res.GetString("30a4cc41-edce-42aa-9ea9-abe06d7f1392", "to") + " ";
				}

				result += LocationHelper.GetFullName(DestinationForDescription, true);
			}
			if (ViaForDescription != null)
			{
				result += " " + Res.GetString("39301a41-9dee-4fa0-a576-d190c2b00355", "via") + " " + LocationHelper.GetFullName(ViaForDescription, false);
			}

			return result;
		}

		protected virtual ILocation OriginForDescription
		{
			get { return this.Origin(); }
		}

		protected virtual ILocation DestinationForDescription
		{
			get { return this.Destination(); }
		}

		#region Via

		protected virtual ILocation ViaForDescription
		{
			get { return Via; }
		}

		#endregion

		#endregion

		#endregion

		#region AllWarehouses

		public ZBool AllWarehouses
		{
			get
			{
				if (!fAllWarehousesInitialized)
				{
					if (IsInDatabase)
					{
						fAllWarehouses = TI_WW_Warehouse.IsEmpty;
					}
					else
					{
						fAllWarehouses = false;
					}
					fAllWarehousesInitialized = true;
				}
				return fAllWarehouses;
			}
			set
			{
				if (fAllWarehouses != value || !fAllWarehousesInitialized)
				{
					fAllWarehouses = value;
					fAllWarehousesInitialized = true;
					if (fAllWarehouses)
					{
						TI_WW_Warehouse = ZGuid.Empty;
					}
					AllWarehousesInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo AllWarehousesInfo
		{
			get { return GetZPropertyInfo(Schema.AllWarehouses); }
		}

		bool fAllWarehousesInitialized;
		ZBool fAllWarehouses;

		#endregion

		#region TI_WW_Warehouse - proxy to TI_ParentID

		[ReadOnlyMember(nameof(AllWarehouses))]
		[List("Lookups.Warehouses")]
		public ZGuid TI_WW_Warehouse
		{
			get { return base.TI_ParentID; }
			set
			{
				base.TI_ParentID = value;
				base.TI_ParentTableCode = WhsWarehouseSchema.Constants.Prefix;
				if (!IsValidationSuspended)
				{
					Validation.ValidateTI_WW_Warehouse();
				}

				TI_WW_WarehouseInfo.RefreshBinding();
				InvalidateRelatedRateLines();
			}
		}

		public ZPropertyInfo TI_WW_WarehouseInfo => GetZPropertyInfo(Schema.TI_WW_Warehouse);

		#endregion

		#region TI_CYC_WW_Facility - proxy to TI_ParentID

		[List("Lookups.Yards")]
		public ZGuid TI_CYC_WW_Facility
		{
			get { return base.TI_ParentID; }
			set
			{
				base.TI_ParentID = value;
				base.TI_ParentTableCode = WhsWarehouseSchema.Constants.Prefix;
				if (!IsValidationSuspended)
				{
					Validation.ValidateTI_CYC_WW_Facility();
				}

				TI_CYC_WW_FacilityInfo.RefreshBinding();
				InvalidateRelatedRateLines();
			}
		}

		public ZPropertyInfo TI_CYC_WW_FacilityInfo => GetZPropertyInfo(Schema.TI_CYC_WW_Facility);

		#endregion

		#region BulkUpdater Properties

		#region EntryType

		public ZString EntryType
		{
			get { return this.ParentRatingHeader.RatingHeaderTypeDescription; }
		}

		public ZPropertyInfo EntryTypeInfo
		{
			get { return GetZPropertyInfo(nameof(EntryType)); }
		}

		#endregion

		#region Organisation Code or Company Tariff Description

		public ZString Organisation
		{
			get { return ParentRatingHeader?.Header?.OH_Code ?? ZString.Empty; }
		}

		public ZPropertyInfo OrganisationInfo
		{
			get { return GetZPropertyInfo(nameof(Organisation)); }
		}

		#endregion

		#region IncludeInUpdate

		public ZBool IncludeInUpdate
		{
			get { return fIncludeInUpdate; }
			set
			{
				fIncludeInUpdate = value;
				IncludeInUpdateInfo.RefreshBinding();
			}
		}

		ZBool fIncludeInUpdate;

		public ZPropertyInfo IncludeInUpdateInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeInUpdate)); }
		}

		#endregion

		#endregion

		#region Carrier Service Levels

		public override OrgCarrierServiceLevel CarrierServiceLevel
		{
			get
			{
				return TI_PL_NKCarrierServiceLevel.Length > 0 && this.CarrierServiceLevelParent() != null
						? Lookups.CarrierServiceLevels.Find(carrierServiceLevel => carrierServiceLevel.PL_Code == TI_PL_NKCarrierServiceLevel).FirstOrDefault()
						: null;
			}
		}

		#endregion

		#region Creation Source

		/// <summary>
		/// The creation source is set only by the code and NOT the UI
		/// </summary>
		protected bool TI_CreationSource_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region PageHeader

		[MaxLength(AutoRateEntry.Schema.TI_PageHeadingMaxLength)]
		public virtual ZString PageHeader
		{
			get { return TI_PageHeading; }
			set
			{
				CheckMaximumLength(PageHeaderInfo, value);
				TI_PageHeading = value;
				PageHeaderInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PageHeaderInfo
		{
			get { return GetZPropertyInfo(nameof(PageHeader)); }
		}

		#endregion

		public string RateId => PK.ToString();
		public ZString CommodityGroup { get; set; }
		public bool IsSpotEntry { get; set; }
		public JobServiceInfo JobServiceForSpotEntry { get; set; }
		public bool IsJobServiceSpotEntry => JobServiceForSpotEntry != null;
		public ZGuid ContainerPKForSpotEntry { get; set; }

		public ZString RateProvider => ZString.Empty;

		public ZDecimal ContainerPayloadWeight => Container?.RC_NetWeight ?? 0m;

		public ZDecimal ContainerPayloadVolume => Container?.RC_CubicCapacity ?? 0m;

		#endregion

		#region Security

		public SecurityCheckpoint DeniedSecurityCheckPoint
		{
			get { return !IsInDatabase ? null : Factory.GetCached(ref deniedSecurityCheckPoint, GetDeniedSecurityCheckpoint); }
		}

		SecurityCheckpoint GetDeniedSecurityCheckpoint()
		{
			var orgHeaderPKs = new[]
				{
					Parent?.Header?.PK ?? ZGuid.Empty,
					TI_OH_Consignor,
					TI_OH_Consignee,
					TI_OH_Supplier
				}
				.Where(x => !x.IsEmpty)
				.ToList();

			var result = RateSecurityHelper.GetFirstDeniedSecurityCheckPoint(orgHeaderPKs, Factory);
			return result?.SecurityCheckPoint?.IsAllowed ?? true ? null : result.SecurityCheckPoint;
		}

		CachedProperty<SecurityCheckpoint> deniedSecurityCheckPoint;

		public bool RateLinesAccessDenied
		{
			get { return DeniedSecurityCheckPoint != null; }
		}

		public string SecurityMessage
		{
			get { return DeniedSecurityCheckPoint == null ? "" : DeniedSecurityCheckPoint.ErrorMessageForNotAllowed; }
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return false;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			return base.GetPropertiesToExcludeFromCloning().Concat(new[]
			{
				RateEntrySchema.Constants.TI_TH,
				RateEntrySchema.Constants.TI_GC_Publisher,
				RateEntrySchema.Constants.TI_DataChecked,
				RateEntrySchema.Constants.TI_IsTact,
				RateEntrySchema.Constants.TI_ContractNumberLinked
			});
		}

		public RateEntry Clone(BusinessObjectCollection destinationRateEntryCollection, IRateEntrySecurityUIIntractor uiIntractor = null)
		{
			if (DeniedSecurityCheckPoint != null)
			{
				if (uiIntractor != null)
				{
					uiIntractor.ShowMessage(this);
				}

				return null;
			}

			var result = (RateEntry)destinationRateEntryCollection.AddNew(destinationRateEntryCollection.TypeOfElements);

			if (result.TI_TH.IsEmpty)
			{
				var rateEntryCollection = destinationRateEntryCollection as RateEntryCollection;
				var parent = rateEntryCollection?.Parent;
				if (parent != null)
				{
					result.TI_TH = parent.PK;
					result.TI_GC_Publisher = !parent.TH_GC.IsEmpty ? parent.TH_GC : Env.CurrentCompanyPK;
				}
				else
				{
					result.TI_TH = TI_TH;
					result.TI_GC_Publisher = Env.CurrentCompanyPK;
				}
			}

			result.RateLines.RemoveAndDeleteAll();

			using (result.SuspendSettingHasChanges())
			{
				var sourceRateType = ParentRatingHeader.RateTypeSafe();
				var destinationRateType = result.ParentRatingHeader.RateTypeSafe();
				var excludedColumns = new List<string>();
				var sourceAndDestinationDifferent = sourceRateType != destinationRateType;
				var notQuotationGoingToClientRate = !(sourceRateType == RatingConstants.RatingHeaderTypes.Quote && destinationRateType == RatingConstants.RatingHeaderTypes.ClientRate);

				excludedColumns.Add(RateEntrySchema.Constants.TI_CreationSource);
				if (sourceAndDestinationDifferent && notQuotationGoingToClientRate)
				{
					excludedColumns.Add(RateEntrySchema.Constants.TI_ContractNumber);
					excludedColumns.Add(RateEntrySchema.Constants.TI_ContractNumberLinked);
				}

				var args = new BusinessObjectCloneArgs();
				args.AddExcludedColumns(excludedColumns);
				result.CopyPersistentValuesFrom(this, args);
			}

			return result;
		}

		public RateEntry DeepClone(BusinessObjectCollection destinationRateEntryCollection, IRateEntrySecurityUIIntractor uiIntractor = null, RowFactory rowFactory = null)
		{
			return DeepClone(destinationRateEntryCollection, false, uiIntractor, rowFactory);
		}

		RateEntry DeepClone(BusinessObjectCollection destinationRateEntryCollection, bool isTACT, IRateEntrySecurityUIIntractor uiIntractor = null, RowFactory rowFactory = null)
		{
			var clonedEntry = Clone(destinationRateEntryCollection, uiIntractor);

			if (clonedEntry == null)
			{
				return null;
			}

			foreach (RateLine line in RateLines)
			{
				if (rowFactory != null)
				{
					var clonedLine = line.Clone(rowFactory, clonedEntry);
					clonedLine.RateCalculatorChanged = false;
					clonedLine.RateLineItems.Clone(line, rowFactory);
					clonedEntry.RateLines.Add(clonedLine);
				}
				else
				{
					var clonedLine = line.Clone(clonedEntry.RateLines);
					clonedLine.TL_TI = clonedEntry.PK;
					clonedLine.RateCalculatorChanged = false;
					clonedLine.RateLineItems.Clone(line);
				}
			}

			if (!isTACT && destinationRateEntryCollection is RateEntryCollection)
			{
				clonedEntry.TI_LineOrder = (ZShort)RatingHelper.MaxPlus1(destinationRateEntryCollection, RateEntrySchema.TI_LineOrder.Name);
				clonedEntry.InvalidateRelatedRateLines();
			}

			return clonedEntry;
		}

		public void CopyPersistentValuesFrom(QuoteEntry quoteEntry)
		{
			using (GetValidationSuspender())
			{
				base.CopyPersistentValuesFrom(quoteEntry);
			}
		}

		#endregion

		#region Validation

		protected override RateEntryValidation GetNewValidation()
		{
			return new ClientAndCostRateEntryValidation(this);
		}

		#endregion

		#region Default End Date

		internal ZDate DefaultEndDate(int validityPeriod)
		{
			if (TI_RateStartDate.IsValid && validityPeriod != 0)
			{
				if (validityPeriod < 0)
				{
					var endDate = TI_RateStartDate.AddMonths(-validityPeriod + 1);
					endDate = new ZDate(endDate.Year, endDate.Month, 1).AddDays(-1);
					return endDate;
				}
				return TI_RateStartDate.AddMonths(validityPeriod);
			}

			return ZDate.Empty;
		}

		public ZDate DefaultRateEndDate
		{
			get { return DefaultEndDate(this.IsQuote() ? Env.Registry.Rating.QuoteValidityPeriod.Value : Env.Registry.Rating.RateValidityPeriod); }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			TI_RateStartDate = ZDate.Today;
			TI_RateEndDate = DefaultRateEndDate;
		}

		#endregion

		#region Utility Methods and Properties

		#region Matching Entry Validation

		public enum RelatedEntriesToFindType
		{
			Freight,
			Origin,
			Destination
		}

		/// <summary>
		/// If "this" is a freight rate entry, this method returns a list of origin/destination rate entries
		/// that "match" this freight entry.
		/// If "this" is an origin/destination rate entry, this method returns a list of matching freight entries.
		/// </summary>
		/// <param name="type">The type of entries to find.</param>
		/// <returns>List of matching entries</returns>
		public List<RateEntry> GetRelatedEntries(RelatedEntriesToFindType type)
		{
			var results = new List<RateEntry>();

			if (SkipGetRelatedEntries(type))
			{
				return results;
			}

			var freightEntryFilter = GetRelatedEntriesFilter(type);

			//TODO this filters in memory, but should filter on disk to cope with rates with millions of entries
			var matchedEntries = Parent.AllEntriesCollection.Find(freightEntryFilter);
			var thisOrigin = this.Origin();
			var thisDestination = this.Destination();
			foreach (RateEntry matchedEntry in matchedEntries)
			{
				if (matchedEntry.PK == PK)
				{
					continue;
				}

				var matchEntryOrigin = matchedEntry.Origin();
				var originOverlaps = thisOrigin == null
					|| matchEntryOrigin == null
					|| thisOrigin.CompletelyCovers(matchEntryOrigin)
					|| matchEntryOrigin.CompletelyCovers(thisOrigin);

				var matchEntryDestination = matchedEntry.Destination();
				var destinationOverlaps = thisDestination == null
					|| matchEntryDestination == null
					|| thisDestination.CompletelyCovers(matchEntryDestination)
					|| matchEntryDestination.CompletelyCovers(thisDestination);

				if (originOverlaps && destinationOverlaps)
				{
					results.Add(matchedEntry);
				}
			}

			return results;
		}

		ZQuery GetRelatedEntriesFilter(RelatedEntriesToFindType type)
		{
			var freightEntryFilter = new ZQuery();

			var typeFilter = new ZQuery();
			if (type == RelatedEntriesToFindType.Freight)
			{
				typeFilter.AddToFilter(RatingHelper.GetFreightModeAndFCL_LCLInclusiveFilter(TI_Mode, this.RateType()));
			}
			else if (type == RelatedEntriesToFindType.Origin)
			{
				typeFilter.AddToFilter(RateEntrySchema.TI_RateCategory, GetOriginCategory(this.RateType()));
				typeFilter.AddToFilter(RatingHelper.GetOriginDestinationModeExclusiveFilter(this.FreightMode()));
			}
			else if (type == RelatedEntriesToFindType.Destination)
			{
				typeFilter.AddToFilter(RateEntrySchema.TI_RateCategory, GetDestinationCategory(this.RateType()));
				typeFilter.AddToFilter(RatingHelper.GetOriginDestinationModeExclusiveFilter(this.FreightMode()));
			}
			freightEntryFilter.AddToFilter(typeFilter);

			freightEntryFilter.AddToFilter(GetContainerFilter(type));

			if (type == RelatedEntriesToFindType.Freight)   // freight CAN be more specific than origin/dest
			{
				freightEntryFilter.AddToFilter(ServiceLevelFilter);
				freightEntryFilter.AddToFilter(CommodityCodeFilter);
				freightEntryFilter.AddToFilter(TransportProviderFilter);
			}
			else    // origin/dest CANT be more specific than freight
			{
				freightEntryFilter.AddToFilter(RateEntrySchema.TI_RS_NKServiceLevel_NI, SQLComparisonOperator.Equal, TI_RS_NKServiceLevel_NI);
				freightEntryFilter.AddToFilter(RateEntrySchema.TI_RH_NKCommodityCode, SQLComparisonOperator.Equal, TI_RH_NKCommodityCode);
				if (!TI_OH_TransportProvider.IsEmpty)
				{
					freightEntryFilter.AddToFilter(RateEntrySchema.TI_OH_TransportProvider, SQLComparisonOperator.Equal, TI_OH_TransportProvider);
				}
				else
				{
					freightEntryFilter.AddToFilter(RateEntrySchema.TI_OH_TransportProvider, SQLComparisonOperator.Equal, null);
				}
			}

			return freightEntryFilter;
		}

		string GetOriginCategory(RateType rateType)
			=> rateType == RateType.Shipping
				? RatingConstants.RateCategory.SOR
				: rateType == RateType.Customs
					? RatingConstants.RateCategory.COR
					: RatingConstants.RateCategory.ORG;

		string GetDestinationCategory(RateType rateType)
			=> rateType == RateType.Shipping
				? RatingConstants.RateCategory.SDE
				: rateType == RateType.Customs
					? RatingConstants.RateCategory.CDS
					: RatingConstants.RateCategory.DST;

		bool SkipGetRelatedEntries(RelatedEntriesToFindType type)
		{
			if (this.IsForwarding())
			{
				if (type == RelatedEntriesToFindType.Origin)
				{
					if (TI_RateCategory == RatingConstants.RateCategory.ORG)
					{
						return true;
					}
				}
				else if (type == RelatedEntriesToFindType.Destination)
				{
					if (TI_RateCategory == RatingConstants.RateCategory.DST)
					{
						return true;
					}
				}
			}
			else if (this.IsCustoms())
			{
				if (type == RelatedEntriesToFindType.Origin)
				{
					if (TI_RateCategory == RatingConstants.RateCategory.COR)
					{
						return true;
					}
				}
				else if (type == RelatedEntriesToFindType.Destination)
				{
					if (TI_RateCategory == RatingConstants.RateCategory.CDS)
					{
						return true;
					}
				}
			}
			else if (this.IsShipping())
			{
				if (type == RelatedEntriesToFindType.Origin)
				{
					if (TI_RateCategory == RatingConstants.RateCategory.SOR)
					{
						return true;
					}
				}
				else if (type == RelatedEntriesToFindType.Destination)
				{
					if (TI_RateCategory == RatingConstants.RateCategory.SDE)
					{
						return true;
					}
				}
			}
			else
			{
				return true;
			}

			return false;
		}

		#region Filters For Matching Freight Entry Validation

		ZQuery GetContainerFilter(RelatedEntriesToFindType type)
		{
			var result = new ZQuery();
			if (!TI_RC.IsEmpty)
			{
				if (type != RelatedEntriesToFindType.Freight)
				{
					result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RC, SQLComparisonOperator.Equal, null);
				}
				result.AddToFilter(JoinCondition.Or, RateEntrySchema.TI_RC, SQLComparisonOperator.Equal, TI_RC);
			}

			return result;
		}

		ZQuery ServiceLevelFilter
		{
			get
			{
				var serviceLevelFilter = new ZQuery();
				if (!TI_RS_NKServiceLevel_NI.IsEmpty)
				{
					serviceLevelFilter.AddToFilter(RateEntrySchema.TI_RS_NKServiceLevel_NI, SQLComparisonOperator.Equal, TI_RS_NKServiceLevel_NI);
				}
				return serviceLevelFilter;
			}
		}

		ZQuery CommodityCodeFilter
		{
			get
			{
				var commodityCodeFilter = new ZQuery();
				if (!TI_RH_NKCommodityCode.IsEmpty)
				{
					commodityCodeFilter.AddToFilter(RateEntrySchema.TI_RH_NKCommodityCode, SQLComparisonOperator.Equal, TI_RH_NKCommodityCode);
				}
				return commodityCodeFilter;
			}
		}

		ZQuery TransportProviderFilter
		{
			get
			{
				var transportProviderFilter = new ZQuery();
				if (!TI_OH_TransportProvider.IsEmpty)
				{
					transportProviderFilter.AddToFilter(RateEntrySchema.TI_OH_TransportProvider, SQLComparisonOperator.Equal, TI_OH_TransportProvider);
				}
				return transportProviderFilter;
			}
		}

		#endregion

		#endregion

		public bool IsAccepting
		{
			get { return HasChanges && isAccepting; }
			set { isAccepting = value; }
		}

		bool isAccepting;

		public ZString FreightPage
		{
			get
			{
				if (this.IsULD())
				{
					return Res.GetString("e0669e09-3035-4603-8938-7a4d7d046011", "ULD Air");
				}

				if (this.IsAir())
				{
					return Res.GetString("d3398566-e839-4246-b09f-7c6a1cf21a1c", "Air");
				}
				else if (this.IsSea())
				{
					return Res.GetString("95523dac-1b9c-435f-8afc-de29d469692a", "Sea");
				}
				else if (this.IsRoad())
				{
					return Res.GetString("692c52ae-d131-4f03-8558-de35c6a04616", "Road");
				}
				else if (this.IsRail())
				{
					return Res.GetString("d408508f-5255-43a2-ab0e-55e33974fb62", "Rail");
				}
				else if (this.IsMail())
				{
					return Res.GetString("cf6b2b7e-e036-4ff3-91f7-1fba8a023f65", "Post");
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		public bool HasRateLines
		{
			get
			{
				var query = new ZQuery(RateLinesSchema.TL_TI, PK);
				query.FetchOnlyFromLocalCache = !IsInDatabase;
				return Factory.LoadTop1<RateLine>(query) != null;
			}
		}

		#endregion

		#region IImportExport Members

		public Directions JobDirection => this.JobDirection();

		ZString DestinationCountry
		{
			get { return this.Destination() != null && this.Destination().Country != null ? this.Destination().Country.Code : ZString.Empty; }
		}

		#endregion

		#region Is Duplicate

		IEnumerable<string> DuplicateSearchColumns
		{
			get
			{
				var defaultColumns = new List<string>
				{
					nameof(TI_TH),
					nameof(TI_RateCategory),
					nameof(TI_Mode),
					nameof(TI_OriginLRC),
					nameof(TI_DestinationLRC),
					nameof(TI_IsCrossTrade),
					nameof(TI_ViaLRC),
					nameof(TI_PlannedLoadLRC),
					nameof(TI_PlannedDischargeLRC),
					nameof(TI_FirstLoadLRC),
					nameof(TI_LastDischargeLRC),
					nameof(TI_FirstRouteSetLoadPortLRC),
					nameof(TI_LastRouteSetDischargePortLRC),
					nameof(TI_OH_TransportProvider),
					nameof(TI_RS_NKServiceLevel_NI),
					nameof(TI_PL_NKCarrierServiceLevel),
					nameof(TI_RS_NKGatewayServiceLevel),
					nameof(TI_RS_NKShipmentGatewayServiceLevel),
					nameof(TI_RH_NKCommodityCode),
					nameof(TI_RMC_Material),
					nameof(TI_RCC_ComponentCode),
					nameof(TI_RRC_RepairCode),
					nameof(TI_EstimateType),
					nameof(TI_REG_EquipmentGrade),
					nameof(TI_MNRGroup),
					nameof(TI_ContainerUnitSection),
					nameof(TI_FMCTariffID),
					nameof(TI_OH_Supplier),
					nameof(TI_OH_Consignor),
					nameof(TI_OH_Consignee),
					nameof(TI_OH_ControllingCustomer),
					nameof(TI_CartagePickupAddressPostCode),
					nameof(TI_CartageDeliveryAddressPostCode),
					nameof(TI_OA_CartagePickupAddressOverride),
					nameof(TI_OA_CartageDeliveryAddressOverride),
					nameof(TI_RateOrigin),
					nameof(TI_RateDestination),
					nameof(TI_AircraftType),
					nameof(TI_TransitTime),
					nameof(TI_Frequency),
					nameof(TI_FrequencyUnit),
					nameof(TI_ParentID),
					nameof(TI_TZ_OriginZone),
					nameof(TI_TZ_DestinationZone),
					nameof(TI_R9_FromSuburb),
					nameof(TI_R9_ToSuburb),
					nameof(TI_IsTact),
					nameof(TI_PaymentTerm),
					nameof(TI_GatewayAgentType),
					nameof(TI_ContractNumber),
					nameof(TI_ShipmentConsolidationStatus),
					nameof(TI_HBLDeliveryMode),
					nameof(TI_IsNonOperatedReefer),
					nameof(TI_YardUnitType),
					nameof(TI_YardUnitLoad),
				};

				if (TI_MatchContainerRateClass)
				{
					defaultColumns.Add(nameof(ContainerClass));
				}
				else
				{
					defaultColumns.Add(nameof(TI_RC));
				}

				return defaultColumns;
			}
		}

		static bool IsDuplicate(RateEntry entry1, RateEntry entry2, IEnumerable<string> columns)
		{
			foreach (var column in columns)
			{
				if (!entry1[column].Equals(entry2[column]))
				{
					return false;
				}
			}
			return true;
		}

		public bool IsDuplicate(RateEntry entry)
		{
			return IsDuplicate(this, entry, DuplicateSearchColumns);
		}

		internal bool IsDuplicateExcludingColumns(RateEntry entry, params string[] columnToExclude)
		{
			var columns = DuplicateSearchColumns.Where(column => !columnToExclude.Contains(column));
			return IsDuplicate(this, entry, columns);
		}

		public string GetKeyForDuplicateSearch(bool excludingContainerClass = false)
		{
			var searchColumns = DuplicateSearchColumns;
			if (excludingContainerClass)
			{
				searchColumns = searchColumns
					.Where(c => c != nameof(ContainerClass) && c != nameof(TI_RC))
					.Append(nameof(TI_RC));
			}
			return string.Join("|", searchColumns.Select(c => this[c]?.ToString()));
		}

		#endregion

		#region Delete

		/// <summary>
		/// Delete this Rate Entry AND all child Rate Lines
		/// </summary>
		public override void Delete()
		{
			if (IsDeleted)
			{
				return;      // HACK - DataRefreshBus shit
			}

			var inMemoryQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
			inMemoryQuery.AddToFilter(RateLinesSchema.TL_TI, PK);
			var inMemoryLines = Factory.Load<RateLine>(inMemoryQuery);

			// DELETING ORDER
			//
			// Logically, lines should be deleted before entry as they are children to entry. Deleting entry first may cause side effects which is hard to reproduce and debug.
			// If you have an issue and think that changing the order (i.e. delete entry then lines) fill fix it - please think twice. It may fix your issue but will cause other
			// issues like the one behind this changeset.

			foreach (var line in inMemoryLines)
			{
				line.Delete();
			}

			base.Delete();
		}

		public override MultilingualString GetWarningBeforeBeingDeleted()
		{
			// Note: Currently contract numbers are linked only for costing.
			// Extra code must be added here, if ever, client contract numbers are linked, so that an appropriate message is displayed.
			if (TI_ContractNumberLinked)
			{
				return ResString.GetMultilingualString("e0788ae1-fe88-407a-8c69-9008ea9ca5f4", "You are about to delete rates that have been linked to Carrier Contract & Allocation record(s).");
			}

			return base.GetWarningBeforeBeingDeleted();
		}

		#endregion

		#region Rate Lines Collection

		[ChildEditable(true)]
		public RateLinesCollection RateLines
		{
			get
			{
				if (rateLines == null)
				{
					rateLines = new RateLinesCollection(this);
					rateLines.Load();
					rateLines.Sort(RateLinesSchema.TL_LineOrder.Name, System.ComponentModel.ListSortDirection.Ascending);
					RegisterEditableChildObject(RateLines);

					rateLines.CountChanged += RateLine_CountChanged;
				}
				return rateLines;
			}
		}
		RateLinesCollection rateLines;
		public bool IsRateLinesLoaded => rateLines != null;

		[ChildEditable(true)]
		public BusinessObjectCollectionView<RateLine> FilteredRateLinesForBinding
		{
			get
			{
				if (filteredRateLinesForBinding == null)
				{
					filteredRateLinesForBinding = new FilteredRateLinesCollection(RateLines, currentLineFilter);
					RegisterEditableChildObject(filteredRateLinesForBinding);
				}
				return filteredRateLinesForBinding;
			}
		}
		FilteredRateLinesCollection filteredRateLinesForBinding;
#if DEBUG
		public bool IsFilteredRateLinesForBindingLoaded_ForTest => filteredRateLinesForBinding != null;
#endif

		internal void SetLineFilter(ZQuery lineFilter)
		{
			currentLineFilter = lineFilter;
			if (filteredRateLinesForBinding != null)
			{
				filteredRateLinesForBinding.SetFilter(lineFilter);
			}
		}
		ZQuery currentLineFilter;

		#endregion

		public void ReloadCartageZones()
		{
			var cartageZoneCalculators = RateLines
				.Cast<RateLine>()
				.Where(x => x.Uses(CalculatorType.CartageZoneDistance))
				.Select(x => x.Calculator);

			foreach (var calculator in cartageZoneCalculators)
			{
				calculator.ReloadCartageZones();
			}
		}

		#region Related Rate Lines

		public ZBool HeaderShowCosting
		{
			get { return fHeaderShowCosting; }
		}

		public ZBool HeaderShowCompanyTariff
		{
			get { return fHeaderShowCompanyTariff; }
		}

		public ZBool HeaderShowClientRates
		{
			get { return fHeaderShowClientRates; }
		}

		public RelatedRateLinesCollection RelatedRateLines
		{
			get
			{
				if (fRelatedRateLines == null)
				{
					fRelatedRateLines = new RelatedRateLinesCollection(Factory, this);
					fRelatedRateLines.Load();
					fHeaderShowCosting = Parent == null ? ZBool.False : Parent.ShowCosting;
					fHeaderShowCompanyTariff = Parent == null ? ZBool.False : Parent.ShowCompanyTariff;
					fHeaderShowClientRates = Parent == null ? ZBool.False : Parent.ShowClientRates;
				}
				else if (fInvalidateRelatedRateLines || (Parent != null && RelatedRateLinesViewOptionsChanged))
				{
					fInvalidateRelatedRateLines = false;
					fRelatedRateLines.Load();
					fHeaderShowCosting = Parent == null ? ZBool.False : Parent.ShowCosting;
					fHeaderShowCompanyTariff = Parent == null ? ZBool.False : Parent.ShowCompanyTariff;
					fHeaderShowClientRates = Parent == null ? ZBool.False : Parent.ShowClientRates;
				}

				return fRelatedRateLines;
			}
		}

		public void InvalidateRelatedRateLines()
		{
			fInvalidateRelatedRateLines = true;
			if (Parent != null && fRelatedRateLines != null)
			{
				((IBusinessObjectCollectionInternals)RelatedRateLines).FireListResetEvent();
			}
		}

		ZBool fHeaderShowCosting;
		ZBool fHeaderShowCompanyTariff;
		ZBool fHeaderShowClientRates;
		RelatedRateLinesCollection fRelatedRateLines;
		ZBool fInvalidateRelatedRateLines;

#if DEBUG

		internal RelatedRateLinesCollection fRelatedRateLinesForTest { get { return fRelatedRateLines; } }

#endif

		/// <summary>
		/// Invalidates any related Costings, Company Tariffs or Client Rate lines,
		/// if the user has changed what they want to view.
		/// </summary>
		/// <returns></returns>
		public bool InvalidateRelatedRateLinesIfValuesChanged()
		{
			var result = false;
			if (Parent != null)
			{
				if (RelatedRateLinesViewOptionsChanged)
				{
					InvalidateRelatedRateLines();
					result = true;
				}
			}
			return result;
		}

		bool RelatedRateLinesViewOptionsChanged
		{
			get
			{
				return HeaderShowCosting != Parent.ShowCosting ||
					HeaderShowCompanyTariff != Parent.ShowCompanyTariff ||
					HeaderShowClientRates != Parent.ShowClientRates;
			}
		}

		public bool ContainsLineWithChargeCode(ZString chargeCode)
		{
			var result = false;
			foreach (RateLine line in RateLines)
			{
				if (line.ChargeCode != null && line.ChargeCode.AC_Code == chargeCode)
				{
					result = true;
				}
			}
			return result;
		}

		public IRateLine InsertRelatedRateLine(RelatedRateLine relatedRateLine, bool removeExistingMatchingChargeCodes)
		{
			if (ReadOnly)
			{
				return null;
			}

			if (removeExistingMatchingChargeCodes)
			{
				for (var i = RateLines.Count - 1; i >= 0; i--)
				{
					var line = RateLines[i];
					if (line.ChargeCode != null && line.ChargeCode.AC_Code == relatedRateLine.ChargeCode.AC_Code)
					{
						RateLines.RemoveAndDelete(line);
					}
				}
			}

			var createdLine = relatedRateLine.Clone(RateLines);
			createdLine.TL_TI = PK;
			createdLine.RateCalculatorChanged = true;

			if (relatedRateLine.IsTariff)
			{
				createdLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CompanyTariffBasedCode;
			}
			else if (relatedRateLine.IsCosting)
			{
				createdLine.TL_RateCalculator = CompanyTariffOrCostBasedCalculator.CostBasedCode;
			}
			else if (relatedRateLine.IsClientRate)
			{
				createdLine.InitializeCalculator();
				createdLine.RateLineItems.Clone(relatedRateLine);
			}

			if (createdLine.Calculator.ShowEquipmentType && relatedRateLine.Calculator.ShowEquipmentType)
			{
				createdLine.Calculator.EquipmentType = relatedRateLine.Calculator.EquipmentType;
			}

			if (createdLine.Calculator.ShowMessageTypeSubType && relatedRateLine.Calculator.ShowMessageTypeSubType)
			{
				createdLine.Calculator.MessageType = relatedRateLine.Calculator.MessageType;
				createdLine.Calculator.MessageSubType = relatedRateLine.Calculator.MessageSubType;
			}

			createdLine.RunPreSaveValidation();

			if (!relatedRateLine.IsClientRate)
			{
				TI_OH_Supplier = relatedRateLine.Supplier;
			}

			if (TI_OriginLRC.IsEmpty)
			{
				TI_OriginLRC = relatedRateLine.Origin;
			}

			if (TI_DestinationLRC.IsEmpty)
			{
				TI_DestinationLRC = relatedRateLine.Destination;
			}

			if (TI_ViaLRC.IsEmpty)
			{
				TI_ViaLRC = relatedRateLine.Via;
			}

			if (TI_PlannedLoadLRC.IsEmpty)
			{
				TI_PlannedLoadLRC = relatedRateLine.PlannedLoad;
			}

			if (TI_PlannedDischargeLRC.IsEmpty)
			{
				TI_PlannedDischargeLRC = relatedRateLine.PlannedDischarge;
			}

			if (TI_RateOrigin.IsEmpty)
			{
				TI_RateOrigin = relatedRateLine.RateOrigin;
			}

			if (TI_RateDestination.IsEmpty)
			{
				TI_RateDestination = relatedRateLine.RateDestination;
			}

			if (TI_RC.IsEmpty)
			{
				TI_RC = relatedRateLine.ContainerPK;
			}

			if (TI_RS_NKServiceLevel_NI.IsEmpty)
			{
				TI_RS_NKServiceLevel_NI = relatedRateLine.ServiceLevel;
			}
			if (TI_PL_NKCarrierServiceLevel.IsEmpty)
			{
				TI_PL_NKCarrierServiceLevel = relatedRateLine.CarrierServiceLevel;
			}
			if (TI_RH_NKCommodityCode.IsEmpty)
			{
				TI_RH_NKCommodityCode = relatedRateLine.CommodityCode;
			}

			if (TI_OH_TransportProvider.IsEmpty)
			{
				if (relatedRateLine.SupplierIsCarrier)
				{
					TI_OH_TransportProvider = relatedRateLine.Supplier;
				}
				else if (!relatedRateLine.SupplierIsCarrier && !relatedRateLine.Carrier.IsEmpty)
				{
					TI_OH_TransportProvider = relatedRateLine.Carrier;
				}
			}

			if (TI_TransitTime.IsEmpty && TI_Frequency.IsEmpty && TI_FrequencyUnit.IsEmpty)
			{
				TI_TransitTime = relatedRateLine.TransitTime;
				TI_Frequency = relatedRateLine.Frequency;
				TI_FrequencyUnit = relatedRateLine.FrequencyUnit;
			}

			if (TI_OH_Consignee.IsEmpty && TI_OA_CartagePickupAddressOverride.IsEmpty && TI_CartagePickupAddressPostCode.IsEmpty)
			{
				TI_OH_Consignee = relatedRateLine.Parent.TI_OH_Consignee;
				TI_OA_CartagePickupAddressOverride = relatedRateLine.Parent.TI_OA_CartagePickupAddressOverride;
				TI_CartagePickupAddressPostCode = relatedRateLine.Parent.TI_CartagePickupAddressPostCode;
			}

			if (TI_OH_Consignor.IsEmpty && TI_OA_CartageDeliveryAddressOverride.IsEmpty && TI_CartageDeliveryAddressPostCode.IsEmpty)
			{
				TI_OH_Consignor = relatedRateLine.Parent.TI_OH_Consignor;
				TI_OA_CartageDeliveryAddressOverride = relatedRateLine.Parent.TI_OA_CartageDeliveryAddressOverride;
				TI_CartageDeliveryAddressPostCode = relatedRateLine.Parent.TI_CartageDeliveryAddressPostCode;
			}

			// We can only populate contract number if both lines have carrier contract number (Costing) 
			// or both have a contract number that is not the carrier contract number (which is the client contract number)
			if (TI_ContractNumber.IsEmpty && createdLine.IsCosting() == relatedRateLine.IsCosting())
			{
				TI_ContractNumber = relatedRateLine.Parent.TI_ContractNumber;
			}

			if (TI_FMCTariffID.IsEmpty && RatingHeader.IsFMCTariffAllowed(Parent.RateTypeSafe()))
			{
				TI_FMCTariffID = relatedRateLine.Parent.TI_FMCTariffID;
			}

			if (TI_IsNonOperatedReefer.IsEmpty && !TI_IsNonOperatedReefer_ReadOnly)
			{
				TI_IsNonOperatedReefer = relatedRateLine.Parent.TI_IsNonOperatedReefer;
			}

			RunPreSaveValidation();
			createdLine.RefreshBinding();

			return createdLine;
		}

		ZGuid fSelectedLineChargeCode;

		public ZGuid SelectedLineChargeCode
		{
			get { return fSelectedLineChargeCode; }
			set
			{
				fSelectedLineChargeCode = value;
				if (Parent != null && Parent.HasSelectedRateLineChangedHandlers)
				{
					for (var i = 0; i < RelatedRateLines.Count; i++)
					{
						if (RelatedRateLines[i].TL_AC == fSelectedLineChargeCode)
						{
							Parent.OnSelectedRateLineChanged(this, i);
							break;
						}
					}
				}
			}
		}

		#endregion

		#region Group Validation

		RateLineGroupValidation RateLineGroupValidation
		{
			get
			{
				if (rateLineGroupValidation == null)
				{
					ProcessGroupValidation();
				}

				return rateLineGroupValidation;
			}
		}
		RateLineGroupValidation rateLineGroupValidation;

		RateLineItemGroupValidation RateLineItemGroupValidation
		{
			get
			{
				if (rateLineItemGroupValidation == null)
				{
					ProcessGroupValidation();
				}

				return rateLineItemGroupValidation;
			}
		}
		RateLineItemGroupValidation rateLineItemGroupValidation;

#if DEBUG
		public bool IsGroupValidationCacheMarkedForRefreshForTest => rateLineItemGroupValidation == null;
#endif

		public void InvalidateGroupValidation()
		{
			rateLineGroupValidation = null;
			rateLineItemGroupValidation = null;
		}

		void RateLine_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			InvalidateGroupValidation();
		}

		void ProcessGroupValidation()
		{
			rateLineGroupValidation = new RateLineGroupValidation();
			rateLineItemGroupValidation = new RateLineItemGroupValidation();

			foreach (RateLine line in RateLines)
			{
				rateLineGroupValidation.AddItem(line);
				rateLineItemGroupValidation.AddItem(line);
			}
		}

		public bool HasSameChargeCodeWithOverlappingCurrencies(RateLine rateLine) => RateLineGroupValidation.HasSameChargeCodeWithOverlappingCurrencies(rateLine);

		public bool IsJobLevelAvailableIsConsistent(RateLine rateLine) => RateLineGroupValidation.IsJobLevelAvailableIsConsistent(rateLine);

		public bool HasFRTCalculatorItemsHaveSameChargeCode(RateLine rateLine) => RateLineItemGroupValidation.HasFRTCalculatorItemsHaveSameChargeCode(rateLine);

		#endregion

		#region Costs Search

		[List("Lookups.Suppliers")]
		public override OrgHeader Supplier
		{
			get { return base.Supplier; }
		}

		[List("Lookups.GeneralOrganizations")]
		public override OrgHeader ControllingCustomer
		{
			get { return base.ControllingCustomer; }
		}

		[List("Lookups.Consignees")]
		public override OrgHeader Consignee
		{
			get { return base.Consignee; }
		}

		[List("Lookups.Consignors")]
		public override OrgHeader Consignor
		{
			get { return base.Consignor; }
		}

		#region Data Import

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData || (Parent != null && ((ISupportDataImporting)Parent).IsImportingData); }
			set { fIsImportingData = value; }
		}
		bool fIsImportingData;

		#endregion

		public RatingCriteria Criteria
		{
			get { return Factory.GetCached(ref criteria, GetCriteria); }
		}
		CachedProperty<RatingCriteria> criteria;

		RatingCriteria GetCriteria()
		{
			var rateEntryCriteria = new RateEntryAdapter(this);

			return new RatingCriteria(rateEntryCriteria, Factory);
		}

		#endregion

		#region Parent Rating Header

		public RatingHeader Parent
		{
			get
			{
				if (parentOverride != null)
				{
					return parentOverride;
				}
				if (IsDeleted)
				{
					return null;
				}

				foreach (var rateEntryCollection in ParentCollections)
				{
					if (rateEntryCollection != null && rateEntryCollection.Parent != null && !rateEntryCollection.Parent.IsDeleted)
					{
						return rateEntryCollection.Parent;
					}
				}

				if (parentActual == null)
				{
					parentActual = Factory.Load<RatingHeader>(TI_TH);
				}
				return parentActual;
			}
			set { parentOverride = value; }
		}
		RatingHeader parentOverride;
		RatingHeader parentActual;

		IEnumerable<RateEntryCollection> ParentCollections => ((IBusinessObjectInternals)this).ParentCollections.OfType<RateEntryCollection>();

		#endregion

		#region Event Logging

		/// <summary>
		/// RateEntry and child objects (RateLine and RateLineItems) call this method whenever they are
		/// created or updated. This method then logs the event under the Rating Header.
		/// 
		/// Only logs one event per save.
		/// </summary>
		public void AddLog()
		{
			if (!IsDeleted)
			{
				AddLog(IsInDatabase ? AutoEvents.EditedARecord : AutoEvents.AddedARecordToTheSystem);
			}
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				IsAccepting = false;
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			CartageZoneDistanceCalculator.DeleteOrphanedZonesIfNeeded(this);
			AddLog();

			if (IsRateEntryLocationsLoaded)
			{
				RateEntryLocations.PopulateBackToRateEntry();
			}
		}

		protected override void BeforeSuccessfulDelete()
		{
			if (IsDeleted)
			{
				return;
			}

			base.BeforeSuccessfulDelete();
			if (IsInDatabase && Parent != null)
			{
				AddLog(AutoEvents.DeletedARecordInTheSystem);
			}
		}

		#region SuppressResourceStringsCheckRegion

		void AddLog(Event eventType)
		{
			Parent?.AddRateEntryLogEvent(this, eventType);
		}

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!IsDeleted && HasChanges)
			{
				TI_SystemLastEditTimeUtc = ZDateTime.Now;
			}
		}

		#endregion

		void SetCreationSource()
		{
			if (!IsInDatabase)
			{
				var stack = ObjectFactory.Get<IBusinessObjectCreationSourceStack>();
				var source = stack.CurrentSource?.CreationSourceCode ?? RateEntryCreator.Sources.Manual;
				TI_CreationSource = source;
			}
		}

		#region Pre-Fetch

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new RateEntryFetchStrategy(this);
		}

		#endregion

		#region SupportsNotes

		public override bool SupportsNotes
		{
			get
			{
				return false;
			}
		}

		#endregion

		public bool SuspendSettingRateLineTariff { get; set; }

		#region Testing Methods
#if DEBUG

		public RateLine AddRateLine(ZString chargeCode, string calculatorCode = "", string lineUnit = "", string currencyCode = "")
		{
			var filter = new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode);
			if (this.Company() == null)
			{
				filter.AddToFilter(AccChargeCodeSchema.AC_GC, null);
			}
			else
			{
				filter.AddToFilter(AccChargeCodeSchema.AC_GC, this.Company().PK);
			}

			var foundCode = Factory.LoadTop1<AccChargeCode>(filter);

			var result = AddRateLine(foundCode, calculatorCode, lineUnit, currencyCode);

			return result;
		}

		public RateLine AddRateLine(AccChargeCode chargeCode, string calculatorCode = "", string lineUnit = "", string currencyCode = "")
		{
			var rateLine = RateLines.AddNew();
			rateLine.TL_AC = chargeCode?.PK ?? ZGuid.Empty;
			rateLine.TL_TI = PK;
			rateLine.TL_RateCalculator = !string.IsNullOrEmpty(calculatorCode) ? (ZString)calculatorCode : chargeCode.AC_RateCalculator;

			if (!string.IsNullOrEmpty(lineUnit))
			{
				rateLine.TL_WeightVolume = lineUnit;
			}

			var currency = !string.IsNullOrEmpty(currencyCode)
				? currencyCode
				: !string.IsNullOrEmpty(TI_RX_NKCurrency)
					? (string)TI_RX_NKCurrency
					: Constants.CurrencyCodes.Australia;

			rateLine.TL_RX_NKCurrency = currency;

			if (rateLine.RequiresWeightVolume() && string.IsNullOrWhiteSpace(lineUnit))
			{
				throw new InvalidOperationException(Invariant($"The test should set unit because {rateLine.TL_RateCalculator} calculator requires one"));
			}

			return rateLine;
		}

		public RateLine AddUnitRateLine(ZString chargeCode, decimal unitPrice, string lineUnit = default, string currency = default, string description = default, ZGuid warehouseProduct = default)
		{
			var rateLine = AddRateLine(chargeCode, UnitCalculator.Code, lineUnit, currencyCode: currency);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = unitPrice;

			if (!string.IsNullOrEmpty(currency))
			{
				rateLine.TL_RX_NKCurrency = currency;
			}

			if (!string.IsNullOrEmpty(description))
			{
				rateLine.OverrideChargeDescription = true;
				rateLine.TL_RateDesc = description;
			}

			if (warehouseProduct != default)
			{
				rateLine.TL_OP_ProductNumber = warehouseProduct;
			}

			return rateLine;
		}

		public RateLine AddFlatRateLine(ZString chargeCode, ZDecimal amount, string currency = "", string container = "", string description = "", string unitFactor = "", ZGuid warehouseProduct = default)
		{
			var rateLine = AddRateLine(chargeCode, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = amount;

			if (!string.IsNullOrEmpty(currency))
			{
				rateLine.TL_RX_NKCurrency = currency;
			}

			if (!string.IsNullOrEmpty(container))
			{
				TI_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, container).PK;
			}

			if (!string.IsNullOrEmpty(description))
			{
				rateLine.OverrideChargeDescription = true;
				rateLine.TL_RateDesc = description;
			}

			if (!string.IsNullOrEmpty(unitFactor))
			{
				rateLine.TL_UnitFactor = unitFactor;
			}

			if (warehouseProduct != default)
			{
				rateLine.TL_OP_ProductNumber = warehouseProduct;
			}

			return rateLine;
		}

		public RateLine AddCMBRateLine(ZString chargeCode, string unit, params (string breakValue, decimal rate)[] rateBreaks)
		{
			var rateLine = AddRateLine(chargeCode, CombinedCalculator.Code, unit);
			var calculator = rateLine.GetCalculator<CombinedCalculator>();

			foreach (var rateBreak in rateBreaks)
			{
				calculator[rateBreak.breakValue] = (ZDecimal)rateBreak.rate;
			}

			return rateLine;
		}

#endif
		#endregion

		public IRatingHeader ParentRatingHeader
		{
			get { return Parent; }
		}

		public IEnumerable<IRateLine> ChildRateLines
		{
			get { return RateLines.Select(x => x as IRateLine); }
		}

		public string InvalidReason => string.Empty;

		internal static RateEntry GetBO(IRateEntry entry)
		{
			return entry as RateEntry;      //ToDo: fix this
		}

		public IEnumerable<string> ReservedForJobIDs
		{
			get => reservedForJobIDs ?? (reservedForJobIDs = new List<string>());
			set => reservedForJobIDs = value;
		}
		IEnumerable<string> reservedForJobIDs;

		public IEnumerable<string> NamedAccounts
		{
			get => namedAccounts ?? (namedAccounts = new List<string>());
			set => namedAccounts = value;
		}
		IEnumerable<string> namedAccounts;

		#region ISalesValueAssociatedEntity Members

		ZString ISalesValueAssociatedEntity.ID => ParentRatingHeader?.TH_QuoteNumber ?? ZString.Empty;

		ZString ISalesValueAssociatedEntity.EntityType
		{
			get
			{
				var parentActivity = Parent as IRelatableActivity;
				return parentActivity != null ? parentActivity.ActivityType : ZString.Empty;
			}
		}

		ControllerID ISalesValueAssociatedEntity.ControllerID
		{
			get
			{
				var parentActivity = Parent as IRelatableActivity;
				if (parentActivity == null)
				{
					return null;
				}

				switch (parentActivity.ActivityType)
				{
					case RelatableActivityTypeList.Codes.Quotations:
						return ControllerIDs.Quotations;

					case RelatableActivityTypeList.Codes.ClientRates:
						return ControllerIDs.ClientRates;
				}

				return null;
			}
		}

		ZGuid? ISalesValueAssociatedEntity.CompanyPk
		{
			get { return Parent != null ? Parent.TH_GC : ZGuid.Empty; }
		}

		ZString ISalesValueAssociatedEntity.CompanyCode
		{
			get { return Parent?.Company?.GC_Code ?? ZString.Empty; }
		}

		ZPropertyInfo ISalesValueAssociatedEntity.OrgPkInfo
		{
			get { return Parent?.TH_OHInfo; }
		}

		ZString ISalesValueAssociatedEntity.Summary
		{
			get { return string.Join("; ", AssociatedEntitySummaryParts); }
		}

		IEnumerable<ZString> AssociatedEntitySummaryParts
		{
			get
			{
				var container = Container;
				if (container != null)
				{
					yield return container.RC_Code;
				}

				var transportProvider = TransportProvider;
				if (transportProvider != null)
				{
					yield return transportProvider.OH_Code;
				}

				if (!TI_RS_NKServiceLevel_NI.IsEmpty)
				{
					yield return TI_RS_NKServiceLevel_NI;
				}

				var consignor = Consignor;
				if (consignor != null)
				{
					yield return consignor.OH_Code;
				}

				var consignee = Consignee;
				if (consignee != null)
				{
					yield return consignee.OH_Code;
				}

				if (TI_RateStartDate.IsValid)
				{
					yield return Res.GetString("1e65c7c0-5f52-4596-8a63-31ea3bc20385", "Start {0}", TI_RateStartDate.ToShortDateString());
				}

				if (TI_RateEndDate.IsValid)
				{
					yield return Res.GetString("866b4cd6-1ba7-4d6b-8a30-5ab2d9382f1b", "Expiry {0}", TI_RateEndDate.ToShortDateString());
				}
			}
		}

		ZString ISalesValueAssociatedEntity.ValueCurrency
		{
			get { return TI_RX_NKCurrency; }
		}

		ZDateTime ISalesValueAssociatedEntity.DateForExchangeRate
		{
			get { return TI_RateStartDate; }
		}

		ZDateTime ISalesValueAssociatedEntity.GetDateAssociatedToSalesValue(ISalesValue salesValue)
		{
			return TI_SystemCreateTimeUtc.ToLocalBranchTime();
		}

		ZString ISalesValueAssociatedEntity.GetUserThatAssociatedToSalesValue(ISalesValue salesValue)
		{
			return TI_SystemCreateUser;
		}

		ISalesHeaderCollection ISalesValueAssociatedEntity.ActualAndProspectiveSalesHeaderCollection
		{
			get
			{
				if (salesHeaderCollection == null)
				{
					salesHeaderCollection = ObjectFactory.Get<ISalesHeaderCollectionBuilder>().New(this, true);
				}

				return salesHeaderCollection;
			}
		}
		ISalesHeaderCollection salesHeaderCollection;

		public ISalesHeaderCollection ProspectiveSalesHeaderCollection
		{
			get
			{
				if (prospectiveSalesHeaderCollection == null)
				{
					prospectiveSalesHeaderCollection = ObjectFactory.Get<ISalesHeaderCollectionBuilder>().New(this, false);
				}
				return prospectiveSalesHeaderCollection;
			}
		}
		ISalesHeaderCollection prospectiveSalesHeaderCollection;

		#endregion

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			if (TI_RateCategory.IsEmpty)
			{
				TI_RateCategory = RatingConstants.RateCategory.AIR;
			}
			if (TI_Mode.IsEmpty)
			{
				TI_Mode = Core.Constants.RateMode.LSE;
			}

			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif

		internal bool ShouldRatingHeaderSkipFetchHints;

#if DEBUG
		public int ValidationCountForTest;
#endif
	}
}
