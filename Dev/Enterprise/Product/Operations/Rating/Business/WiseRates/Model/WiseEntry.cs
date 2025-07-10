using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Urs.Api.Integration.Interfaces;
using WiseRates.Api.Model;
using Rate = WiseRates.Api.Model.Rate;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.Business
{
	public class WiseEntry : IRateEntry
	{
		public Rate WiseRate { get; }

		public ITradeServiceDto TradeService { get; }

		public WiseEntry(Rate wiseRate, BusinessObjectFactory factory)
		{
			Factory = factory;
			PK = ZGuid.NewZGuid();
			WiseRate = wiseRate;
			ChildRateLines = System.Array.Empty<IRateLine>();
			UniversalServiceLevel = wiseRate?.ServiceLevel;
		}

		public WiseEntry(ITradeServiceDto tradeService, BusinessObjectFactory factory)
		{
			Factory = factory;
			PK = ZGuid.NewZGuid();
			TradeService = tradeService;
			ChildRateLines = System.Array.Empty<IRateLine>();
			UniversalServiceLevel = UrsRatesParseHelper.GetUniversalServiceLevel(tradeService);
		}

		public string InvalidReason
		{
			get
			{
				var errors = new StringBuilder();

				foreach (var error in ErrorsIncludingChildren)
				{
					errors.AppendLine(error);
				}

				return errors.ToString();
			}
		}

		public IDictionary<SchemaColumn, string> Errors { get; } = new Dictionary<SchemaColumn, string>();

		public IEnumerable<string> ErrorsIncludingChildren
		{
			get
			{
				var errors = new List<string>();
				var header = ParentRatingHeader as WiseHeader;

				if (header?.Errors?.Any() ?? false)
				{
					errors.AddRange(header.Errors.Select(e => e.Value));
				}

				errors.AddRange(Errors.Select(e => e.Value));

				foreach (WiseLine line in ChildRateLines)
				{
					errors.AddRange(line.Errors.Select(e => e.Value));
				}

				return errors;
			}
		}

		public ZGuid PK { get; }

		public OrgHeader Carrier => TransportProvider ?? ParentRatingHeader?.Header;

		public ZString ProductName { get; set; }

		public ZString TI_RateCategory { get; set; }

		public ZString TI_Mode { get; set; }

		public ZGuid TI_GC_Publisher => ZGuid.Empty;

		public IRatingHeader ParentRatingHeader { get; set; }

		public ZString TI_OriginLRC { get; set; }

		public ZString TI_DestinationLRC { get; set; }

		#region SpotEntry

		public bool IsSpotEntry => false;

		public ZString CarrierQuoteNumber => IsSpotEntry ? TradeService?.ExternalReference : ZString.Empty;

		public bool IsJobServiceSpotEntry => false;
		public JobServiceInfo JobServiceForSpotEntry => null;

		public ZGuid ContainerPKForSpotEntry => ZGuid.Empty;

		#endregion

		public RefContainer Container
		{
			get { return Factory.Load<RefContainer>(TI_RC); }
		}

		public ZGuid TI_RC { get; set; }

		public ZGuid TI_OH_Supplier => ZGuid.Empty;

		public OrgHeader Supplier => null;

		public ZGuid TI_OH_TransportProvider { get; set; }

		public OrgHeader TransportProvider
		{
			get { return Factory.Load<OrgHeader>(TI_OH_TransportProvider); }
		}

		public ZGuid TI_TZ_OriginZone => ZGuid.Empty;

		public ZGuid TI_TZ_DestinationZone => ZGuid.Empty;

		public ZGuid OriginSuburbPK => ZGuid.Empty;

		public ZGuid DestinationSuburbPK => ZGuid.Empty;

		public ZBool TI_MatchContainerRateClass { get; set; }

		public RateTransportZone OriginZone => null;

		public RateTransportZone DestinationZone => null;

		public ZDate TI_RateStartDate { get; set; }

		public ZDate TI_RateEndDate { get; set; }

		public ZString TI_TransitTime { get; set; }

		public ZString TI_FrequencyUnit => ZString.Empty;

		public ZInt TI_Frequency => ZInt.Zero;

		public ZGuid TI_WW_Warehouse => ZGuid.Empty;

		public ZGuid TI_CYC_WW_Facility => ZGuid.Empty;

		public ZString TI_RH_NKCommodityCode { get; set; }

		public ZString TI_FMCTariffID { get; set; }

		public ZString CommodityGroup { get; set; }

		public ZString[] Commodities { get; set; }

		public ZString TI_ViaLRC { get; set; }

		public ZBool TI_IsCrossTrade => false;

		public ZString TI_RS_NKServiceLevel_NI { get; set; }

		public ZString TI_PL_NKCarrierServiceLevel { get; set; }

		public ZString TI_RS_NKGatewayServiceLevel { get; set; }

		public ZString TI_RS_NKShipmentGatewayServiceLevel { get; set; }

		public ZGuid TI_OH_Consignor => ZGuid.Empty;

		public OrgHeader Consignor => null;

		public ZGuid TI_OH_ControllingCustomer { get; set; }

		public OrgHeader ControllingCustomer
		{
			get { return Factory.Load<OrgHeader>(TI_OH_ControllingCustomer); }
		}

		public ZGuid TI_OH_Consignee => ZGuid.Empty;

		public OrgHeader Consignee => null;

		public OrgAddress CartagePickupAddressOverride => null;

		public OrgAddress CartageDeliveryAddressOverride => null;

		public ZGuid TI_OA_CartageDeliveryAddressOverride => ZGuid.Empty;

		public ZGuid TI_OA_CartagePickupAddressOverride => ZGuid.Empty;

		public ZString TI_CartagePickupAddressPostCode => ZString.Empty;

		public ZString TI_CartageDeliveryAddressPostCode => ZString.Empty;

		public BusinessObjectFactory Factory { get; }

		public ZString TI_ContractNumber { get; set; }

		public ZGuid TI_R9_FromSuburb => ZGuid.Empty;

		public ZGuid TI_R9_ToSuburb => ZGuid.Empty;

		public ZBool IsPublished => false;

		public ZBool TI_IsTact { get; }

		public ZString RateProvider { get; set; }

		public Directions JobDirection => this.JobDirection();

		public ZGuid TI_ParentID => ZGuid.Empty;

		public ZString TI_ParentTableCode => ZString.Empty;

		public ZString TI_PaymentTerm { get; set; }

		public ZString TI_GatewayAgentType => ZString.Empty;

		public IEnumerable<string> ReservedForJobIDs { get; set; }

		public IEnumerable<string> NamedAccounts { get; set; }

		public ZString TI_PlannedLoadLRC => ZString.Empty;

		public ZString TI_PlannedDischargeLRC => ZString.Empty;

		public ZString TI_FirstLoadLRC => ZString.Empty;

		public ZString TI_LastDischargeLRC => ZString.Empty;

		public ZString TI_FirstRouteSetLoadPortLRC => ZString.Empty;

		public ZString TI_LastRouteSetDischargePortLRC => ZString.Empty;

		public ZString TI_RateOrigin => ZString.Empty;

		public ZString TI_RateDestination => ZString.Empty;

		public ZString TI_AircraftType { get; set; }

		public ZString TI_ShipmentConsolidationStatus => ZString.Empty;

		public ZString TI_HBLDeliveryMode => ZString.Empty;

		public ZString TI_IsNonOperatedReefer => ZString.Empty;

		public ZString TI_YardUnitType => ZString.Empty;

		public ZString TI_YardUnitLoad => ZString.Empty;

		public ZBool TI_IsExcludedFromAutoRating { get; }

		public ZString UniversalServiceLevel { get; set; }

		public string ContainerQuality
			=> (GetCustomFieldValue(Rate.CustomFields.CargoSphere.ContainerQuality) as string) ?? string.Empty;

		#region Payload Weight Properties

		public ZDecimal ContainerPivotWeight { get; set; }

		public ZDecimal ContainerPayloadWeightOverride { get; set; }

		public ZDecimal ContainerPayloadWeight => ContainerPayloadWeightOverride != default ? ContainerPayloadWeightOverride : Container?.RC_NetWeight ?? 0m;

		#endregion

		#region Payload Volume Properties

		public ZDecimal ContainerPayloadVolumeOverride { get; set; }

		public ZDecimal ContainerPayloadVolume => ContainerPayloadVolumeOverride != default ? ContainerPayloadVolumeOverride : Container?.RC_CubicCapacity ?? 0m;

		#endregion

		public IEnumerable<IRateLine> ChildRateLines
		{
			get
			{
				return lines;
			}
			set
			{
				foreach (var line in value)
				{
					if (line is WiseLine wiseLine)
					{
						wiseLine.ParentRateEntry = this;
					}
				}

				lines = value;
			}
		}

		IEnumerable<IRateLine> lines;

		public IEnumerable<CustomField> CustomFields { get; set; }

		public string RateId => WiseRate?.ProviderRateId ?? TradeService?.Key;

		#region Custom Field Properties

		public ZString Remarks => GetCustomFieldValue(Rate.CustomFields.Cargoguide.Remarks)?.ToString();

		public ZString Deck => GetCustomFieldValue(Rate.CustomFields.Cargoguide.DeckType)?.ToString();

		public ZString ChargeableFactor => GetCustomFieldValue(Rate.CustomFields.Cargoguide.Ratio)?.ToString();

		public ZString ProductCode => GetCustomFieldValue(Rate.CustomFields.Cargoguide.ProductCode)?.ToString();

		public ZString RateType => GetCustomFieldValue(Rate.CustomFields.CargoSphere.RateType)?.ToString();

		public ZString RateType2 => GetCustomFieldValue(Rate.CustomFields.CargoSphere.RateType2)?.ToString();

		public ZString Vessel => GetCustomFieldValue(Rate.CustomFields.CargoSphere.Vessel)?.ToString();

		public ZString AddOn => GetCustomFieldValue(Rate.CustomFields.CargoSphere.ArbitraryPermission)?.ToString();

		public ZString Tradelane => GetCustomFieldValue(Rate.CustomFields.CargoSphere.TradeLane)?.ToString();

		public ZString ServiceString => GetCustomFieldValue(Rate.CustomFields.CargoSphere.ServiceString)?.ToString();

		#endregion

		public ZString TI_ContainerUnitSection { get; }
		public ZGuid TI_RRC_RepairCode { get; }
		public ZGuid TI_RCC_ComponentCode { get; }
		public ZGuid TI_RMC_Material { get; }
		public ZString TI_EstimateType { get; }
		public ZGuid TI_REG_EquipmentGrade { get; }
		public ZString TI_MNRGroup { get; }

		/// <summary>
		/// Returns null if no code is found.
		/// </summary>
		public object GetCustomFieldValue(string code)
		{
			return CustomFields?.FirstOrDefault(c => string.Equals(code, c?.Code))?.Value;
		}
	}
}
