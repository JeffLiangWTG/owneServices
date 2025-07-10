using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static System.FormattableString;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.Business
{
	/// <summary>
	/// Represents each row of a WiseEntry that is shown in the WiseRatesForm
	/// The properties contained herein are part of the columns that are displayed in the
	/// Multimodal search form.
	///
	/// When adding a new column to display in the Multimodal search form, you also need to add it
	/// to RateEntryCollectionGUIInfo.cs, WiseRatesRateEntryCollectionGUIInfo.cs and
	/// WiseRatesRateEntryCollectionGUIInfoTest.cs so that it can appear in the
	/// Multimodal search form results grid.
	/// </summary>
	public class WiseEntryView : NonPersistentBusinessObject, IRateEntry, IGetZPropertyInfo, INeedCodeMappings
	{
		public WiseEntryView()
		{
		}

		public WiseEntryView(IRateEntry wiseEntry, RatesSearchResponse searchResponse) : base(wiseEntry.Factory)
		{
			Argument.NotNull(wiseEntry, nameof(wiseEntry));

			this.wiseEntry = wiseEntry;
			this.searchResponse = searchResponse;
		}

		readonly IRateEntry wiseEntry;
		readonly RatesSearchResponse searchResponse;

		public WiseEntry UnderlyingWiseEntry => wiseEntry as WiseEntry;
		public IRatingHeader ParentRatingHeader => wiseEntry.ParentRatingHeader;

		public string RateId => wiseEntry.RateId;

		[List("Lookups.TransportModes")]
		public ZString TI_Mode => wiseEntry.TI_Mode;
		public ZPropertyInfo TI_ModeInfo => GetZPropertyInfo(nameof(TI_Mode));

		[List("Lookups.Locations")]
		public ZString TI_OriginLRC => wiseEntry.TI_OriginLRC;
		public ZPropertyInfo TI_OriginLRCInfo => GetZPropertyInfo(nameof(TI_OriginLRC));

		[List("Lookups.Locations")]
		public ZString TI_DestinationLRC => wiseEntry.TI_DestinationLRC;
		public ZPropertyInfo TI_DestinationLRCInfo => GetZPropertyInfo(nameof(TI_DestinationLRC));

		[List("Lookups.Containers")]
		public ZGuid TI_RC => wiseEntry.TI_RC;
		public ZPropertyInfo TI_RCInfo => GetZPropertyInfo(nameof(TI_RC));

		[List("Lookups.Publishers")]
		public ZGuid TI_GC_Publisher => wiseEntry.TI_GC_Publisher;

		public ZString TI_RateCategory => wiseEntry.TI_RateCategory;
		public ZPropertyInfo TI_RateCategoryInfo => GetZPropertyInfo(nameof(TI_RateCategory));

		[List("Lookups.Suppliers")]
		public ZGuid TI_OH_Supplier => wiseEntry.TI_OH_Supplier;
		public ZGuid TI_OH_TransportProvider => UnderlyingWiseEntry?.Carrier?.PK ?? ZGuid.Empty;
		public ZPropertyInfo TI_OH_TransportProviderInfo => GetZPropertyInfo(nameof(TI_OH_TransportProvider));
		public ZGuid TI_TZ_OriginZone => wiseEntry.TI_TZ_OriginZone;
		public ZGuid TI_TZ_DestinationZone => wiseEntry.TI_TZ_DestinationZone;
		public ZBool TI_MatchContainerRateClass => wiseEntry.TI_MatchContainerRateClass;
		public ZDate TI_RateEndDate => wiseEntry.TI_RateEndDate;
		public ZString TI_TransitTime => wiseEntry.TI_TransitTime;

		[List("Lookups.FrequencyUnits")]
		public ZString TI_FrequencyUnit => wiseEntry.TI_FrequencyUnit;

		public ZInt TI_Frequency => wiseEntry.TI_Frequency;

		public ZGuid TI_WW_Warehouse => wiseEntry.TI_WW_Warehouse;

		public ZGuid TI_CYC_WW_Facility => wiseEntry.TI_CYC_WW_Facility;

		[List("Lookups.CommodityCodes")]
		public ZString TI_RH_NKCommodityCode => wiseEntry.TI_RH_NKCommodityCode;
		public ZPropertyInfo TI_RH_NKCommodityCodeInfo => GetZPropertyInfo(nameof(TI_RH_NKCommodityCode));

		public ZString TI_FMCTariffID => wiseEntry.TI_FMCTariffID;
		public ZPropertyInfo TI_FMCTariffIDInfo => GetZPropertyInfo(nameof(TI_FMCTariffID));

		public ZDate TI_RateStartDate => wiseEntry.TI_RateStartDate;

		[List("Lookups.Locations")]
		public ZString TI_ViaLRC => wiseEntry.TI_ViaLRC;

		public ZBool TI_IsCrossTrade => wiseEntry.TI_IsCrossTrade;

		[List("Lookups.ServiceLevel_NIs")]
		public ZString TI_RS_NKServiceLevel_NI => wiseEntry.TI_RS_NKServiceLevel_NI;
		public ZPropertyInfo TI_RS_NKServiceLevel_NIInfo => GetZPropertyInfo(nameof(TI_RS_NKServiceLevel_NI));

		public ZString TI_PL_NKCarrierServiceLevel => wiseEntry.TI_PL_NKCarrierServiceLevel;
		public ZPropertyInfo TI_PL_NKCarrierServiceLevelInfo => GetZPropertyInfo(nameof(TI_PL_NKCarrierServiceLevel));

		public ZString TI_RS_NKGatewayServiceLevel => wiseEntry.TI_RS_NKGatewayServiceLevel;
		public ZPropertyInfo TI_RS_NKGatewayServiceLevelInfo => GetZPropertyInfo(nameof(TI_RS_NKGatewayServiceLevel));

		public ZString TI_RS_NKShipmentGatewayServiceLevel => wiseEntry.TI_RS_NKShipmentGatewayServiceLevel;
		public ZPropertyInfo TI_RS_NKShipmentGatewayServiceLevelInfo => GetZPropertyInfo(nameof(TI_RS_NKShipmentGatewayServiceLevel));

		[List("Lookups.ControllingCustomers")]
		public ZGuid TI_OH_ControllingCustomer => wiseEntry.TI_OH_ControllingCustomer;
		public ZPropertyInfo TI_OH_ControllingCustomerInfo => GetZPropertyInfo(nameof(TI_OH_ControllingCustomer));

		[List("Lookups.Consignors")]
		public ZGuid TI_OH_Consignor => wiseEntry.TI_OH_Consignor;

		[List("Lookups.Consignees")]
		public ZGuid TI_OH_Consignee => wiseEntry.TI_OH_Consignee;

		public ZGuid TI_OA_CartageDeliveryAddressOverride => wiseEntry.TI_OA_CartageDeliveryAddressOverride;

		public ZGuid TI_OA_CartagePickupAddressOverride => wiseEntry.TI_OA_CartagePickupAddressOverride;

		public ZString TI_CartagePickupAddressPostCode => wiseEntry.TI_CartagePickupAddressPostCode;

		public ZString TI_CartageDeliveryAddressPostCode => wiseEntry.TI_CartageDeliveryAddressPostCode;

		public ZString TI_ContractNumber => wiseEntry.TI_ContractNumber;

		public ZString CGReference => (string)(UnderlyingWiseEntry?.GetCustomFieldValue(Rate.CustomFields.Cargoguide.Reference) ?? string.Empty);

		public ZString UniversalCarrierServiceLevel => UnderlyingWiseEntry?.UniversalServiceLevel ?? ZString.Empty;

		public ZString ContainerQuality => UnderlyingWiseEntry?.ContainerQuality ?? ZString.Empty;

		public ZString CargoguideProductCode => (string)(UnderlyingWiseEntry?.GetCustomFieldValue(Rate.CustomFields.Cargoguide.ProductCode) ?? string.Empty);

		public ZGuid TI_R9_FromSuburb => wiseEntry.TI_R9_FromSuburb;

		public ZGuid TI_R9_ToSuburb => wiseEntry.TI_R9_ToSuburb;

		public ZBool TI_IsTact => wiseEntry.TI_IsTact;

		public ZBool IsPublished => false;

		public RateEntryLookups Lookups => fLookups ?? (fLookups = new RateEntryLookups(this, Factory));
		RateEntryLookups fLookups;

		public bool IsSpotEntry => wiseEntry.IsSpotEntry;
		public bool IsJobServiceSpotEntry => wiseEntry.IsJobServiceSpotEntry;
		public JobServiceInfo JobServiceForSpotEntry => null;
		public ZGuid ContainerPKForSpotEntry => wiseEntry.ContainerPKForSpotEntry;

		public RefContainer Container => wiseEntry.Container;

		public ZGuid OriginSuburbPK => wiseEntry.OriginSuburbPK;

		public ZGuid DestinationSuburbPK => wiseEntry.DestinationSuburbPK;

		public RateTransportZone OriginZone => wiseEntry.OriginZone;

		public RateTransportZone DestinationZone => wiseEntry.DestinationZone;

		public OrgHeader TransportProvider => wiseEntry.TransportProvider;

		public OrgHeader ControllingCustomer => wiseEntry.ControllingCustomer;

		public OrgHeader Consignor => wiseEntry.Consignor;

		public OrgHeader Consignee => wiseEntry.Consignee;

		public OrgAddress CartagePickupAddressOverride => wiseEntry.CartagePickupAddressOverride;

		public OrgAddress CartageDeliveryAddressOverride => wiseEntry.CartageDeliveryAddressOverride;

		public ZString RateProvider => WRConstants.RateProviders.GetDescription(wiseEntry.RateProvider);

		public OrgHeader Supplier => wiseEntry.Supplier;

		public string InvalidReason => wiseEntry.InvalidReason;
		public IEnumerable<IRateLine> ChildRateLines => wiseEntry.ChildRateLines;

		/// <summary>
		/// This is the listing of rate line views shown on the multi modal search form
		/// </summary>
		public WiseLineViewsCollection ChildWiseRateLineViews
		{
			get
			{
				if (childWiseRateLineViews == null)
				{
					childWiseRateLineViews = new WiseLineViewsCollection(wiseEntry?.ChildRateLines ?? Enumerable.Empty<IRateLine>());
					RegisterEditableChildObject(childWiseRateLineViews);
				}

				return childWiseRateLineViews;
			}
		}

		WiseLineViewsCollection childWiseRateLineViews;

		public Directions JobDirection => this.JobDirection();
		ZPropertyInfo IGetZPropertyInfo.GetZPropertyInfo(string propertyName)
		{
			return this.GetZPropertyInfo(propertyName);
		}

		public ZGuid TI_ParentID => wiseEntry.TI_ParentID;

		public ZString TI_ParentTableCode => wiseEntry.TI_ParentTableCode;

		public ZString TI_PaymentTerm => wiseEntry.TI_PaymentTerm;

		public ZString TI_GatewayAgentType => wiseEntry.TI_GatewayAgentType;

		public ZString TI_ShipmentConsolidationStatus => wiseEntry.TI_ShipmentConsolidationStatus;

		public ZString TI_HBLDeliveryMode => wiseEntry.TI_HBLDeliveryMode;

		public ZString TI_IsNonOperatedReefer => wiseEntry.TI_IsNonOperatedReefer;

		public IEnumerable<string> ReservedForJobIDs => wiseEntry.ReservedForJobIDs;

		public IEnumerable<string> NamedAccounts => wiseEntry.NamedAccounts;

		[ResourceStringData("f7caed45-0b73-4175-9b3d-0e691824327e", ShortCaption = "Product", Caption = "Product Name", FullDescription = "Carrier Product Name")]
		public string ProductName => productName ?? (productName = wiseEntry.ProductName());
		string productName;

		[ResourceStringData("4760CECE-58C7-45A1-B2E6-143B7306EA47", ShortCaption = "Comm. Group", Caption = "Commodity Group")]
		public ZString CommodityGroup => UnderlyingWiseEntry.CommodityGroup;

		[ResourceStringData("1ec832aa-0dfa-419b-a0ec-de326c709601", ShortCaption = "Commodities", Caption = "Commodities")]
		public string Commodities => commodities ?? (commodities = wiseEntry.Commodities());
		string commodities;

		public ZDecimal ContainerPayloadWeight => wiseEntry.ContainerPayloadWeight;
		public ZString ContainerPayloadWeightForBinding => Invariant($"{ContainerPayloadWeight:0.##} KG");
		public ZDecimal ContainerPayloadVolume => wiseEntry.ContainerPayloadVolume;
		public ZString ContainerPayloadVolumeForBinding => Invariant($"{ContainerPayloadVolume:0.##} M3");

		#region Business Object Overrides

		public WiseEntryViewValidation Validation => new WiseEntryViewValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			Validation.ValidateAll();
		}

		#endregion

		#region INeedCodeMappings

		List<UnmappedForeignCode> INeedCodeMappings.UnmappedCodes => unmappedCodes;
		readonly List<UnmappedForeignCode> unmappedCodes = new List<UnmappedForeignCode>();

		bool INeedCodeMappings.NeedsCodeMapping => unmappedCodes.Any();
		ZGuid INeedCodeMappings.CarrierOrgHeaderPK => TI_OH_TransportProvider;

		void INeedCodeMappings.SetUnmappedCodes(IEnumerable<UnmappedForeignCode> codesNeedsMapping)
		{
			unmappedCodes.AddRange(codesNeedsMapping);
		}

		#endregion

		#region FreightRatePerChargeable

		[ResourceStringData("bf8a5818-6810-49a1-a1da-bb3a0cd75ecc", ShortCaption = "All In Cost", Caption = "All In Cost", FullDescription = "All In Cost for a single chargeable unit")]
		public string AllInCost => allInCost ?? (allInCost = this.AllInCost());
		string allInCost;

		[ResourceStringData("199752b1-c31f-4768-bdd9-2d375dcde92a", ShortCaption = "Freight Rate", Caption = "Freight Rate Per Unit", FullDescription = "Freight rate per single chargeable unit, calculated with unit rates only")]
		public string FreightRatePerChargeableUnit => freightRatePerChargeableUnit ?? (freightRatePerChargeableUnit = this.FreightRatePerChargeableUnit());
		string freightRatePerChargeableUnit;

		#endregion

		public RefCarrier RefCarrier
		{
			get
			{
				RefCarrier result = null;
				var carrierCode = UnderlyingWiseEntry?.WiseRate?.Carrier;
				if (!string.IsNullOrWhiteSpace(carrierCode))
				{
					result = searchResponse.Carriers.FirstOrDefault(x => string.Equals(x.Code, carrierCode, StringComparison.OrdinalIgnoreCase));
				}

				return result;
			}
		}

		[ResourceStringData("78EA345B-E11B-4E14-B155-DCAEC9B14BD7", Caption = "Carrier")]
		public ZString CarrierCode
		{
			get
			{
				if (carrierCode == null)
				{
					var org = wiseEntry.ParentRatingHeader?.Header;
					carrierCode = org?.OH_Code.ToString();
					if (carrierCode == null)
					{
						var refCarrier = RefCarrier;
						if (refCarrier != null)
						{
							carrierCode = string.Join(", ", new[] { refCarrier.SCACCode, refCarrier.IATACode }.Where(x => !string.IsNullOrEmpty(x)));
						}
						else
						{
							carrierCode = string.Empty;
						}
					}
				}
				return carrierCode;
			}
		}

		string carrierCode;

		public ZPropertyInfo CarrierCodeInfo => GetWrappedZPropertyInfo(nameof(CarrierCode), x => TI_OH_TransportProviderInfo);

		public ZString TI_PlannedLoadLRC => wiseEntry.TI_PlannedLoadLRC;

		public ZString TI_PlannedDischargeLRC => wiseEntry.TI_PlannedDischargeLRC;

		public ZString TI_FirstLoadLRC => wiseEntry.TI_FirstLoadLRC;

		public ZString TI_LastDischargeLRC => wiseEntry.TI_LastDischargeLRC;

		public ZString TI_FirstRouteSetLoadPortLRC => wiseEntry.TI_FirstRouteSetLoadPortLRC;

		public ZString TI_LastRouteSetDischargePortLRC => wiseEntry.TI_LastRouteSetDischargePortLRC;

		public ZString TI_RateOrigin => wiseEntry.TI_RateOrigin;

		public ZString TI_RateDestination => wiseEntry.TI_RateDestination;

		public ZString TI_AircraftType => wiseEntry.TI_AircraftType;

		public ZString TI_YardUnitType => wiseEntry.TI_YardUnitType;

		public ZString TI_YardUnitLoad => wiseEntry.TI_YardUnitLoad;

		public ZBool TI_IsExcludedFromAutoRating => wiseEntry.TI_IsExcludedFromAutoRating;

		public ZString TI_ContainerUnitSection => TI_ContainerUnitSection;

		public ZGuid TI_RRC_RepairCode => TI_RRC_RepairCode;

		public ZGuid TI_RCC_ComponentCode => TI_RCC_ComponentCode;

		public ZGuid TI_RMC_Material => wiseEntry.TI_RMC_Material;

		public ZString TI_EstimateType => wiseEntry.TI_EstimateType;

		public ZGuid TI_REG_EquipmentGrade => wiseEntry.TI_REG_EquipmentGrade;

		public ZString TI_MNRGroup => wiseEntry.TI_MNRGroup;
	}
}
