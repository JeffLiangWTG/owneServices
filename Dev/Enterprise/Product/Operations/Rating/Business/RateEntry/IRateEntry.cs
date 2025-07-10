using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public interface IRateEntry : IRate, IImportExport, IFactoryProvider
	{
		ZString TI_Mode { get; }
		IRatingHeader ParentRatingHeader { get; }
		ZString TI_OriginLRC { get; }
		ZString TI_DestinationLRC { get; }
		bool IsSpotEntry { get; }
		bool IsJobServiceSpotEntry { get; }
		JobServiceInfo JobServiceForSpotEntry { get; }
		ZGuid ContainerPKForSpotEntry { get; }
		IEnumerable<IRateLine> ChildRateLines { get; }
		RefContainer Container { get; }
		ZGuid TI_RC { get; }
		ZString TI_RateCategory { get; }
		ZGuid TI_GC_Publisher { get; }
		ZGuid TI_OH_Supplier { get; }

		/// <summary>
		/// Used by: Container Yard (CYD) team for container section identification for Estimate of Repair jobs.
		/// Location: Found in Yard Maintenance tab under Container Yard Client Rates.
		/// 
		/// Represents the specific section of a container unit being repaired or rated.
		/// 
		/// Rate Matching:
		/// - Must match the job's container section for the rate to be considered.
		/// </summary>
		ZString TI_ContainerUnitSection { get; }

		/// <summary>
		/// Used by: Container Yard (CYD) team for repair code identification for Estimate of Repair jobs.
		/// Location: Found in Yard Maintenance tab under Container Yard Client Rates.
		/// 
		/// Represents the repair code associated with a specific type of repair.
		/// 
		/// Rate Matching:
		/// - Must match the job's repair code for the rate to be considered.
		/// </summary>
		ZGuid TI_RRC_RepairCode { get; }

		/// <summary>
		/// Used by: Container Yard (CYD) team for component identification for Estimate of Repair jobs.
		/// Location: Found in Yard Maintenance tab under Container Yard Client Rates.
		/// 
		/// Represents the specific component of a container being repaired.
		/// 
		/// Rate Matching:
		/// - Must match the job's component for the rate to be considered.
		/// </summary>
		ZGuid TI_RCC_ComponentCode { get; }

		/// <summary>
		/// Used by: Container Yard (CYD) team for material identification for Estimate of Repair jobs.
		/// Location: Found in Yard Maintenance tab under Container Yard Client Rates.
		/// 
		/// Represents the material used in a repair or maintenance job.
		/// 
		/// Rate Matching:
		/// - Must match the job's material for the rate to be considered.
		/// </summary>
		ZGuid TI_RMC_Material { get; }

		/// <summary>
		/// Used by: Container Yard (CYD) team for Estimate of Repair jobs.
		/// Location: Found in Labour Rates tab under Container Yard Client Rates.
		/// 
		/// Determines the type of repair estimate being performed on container equipment.
		/// Values: 'structural' or 'machinery'.
		/// 
		/// Rate Matching:
		/// - Used during auto-rating of M&amp;R jobs and EOR service lines.
		/// - Must match between job's repair type and rate entry for the rate to be considered.
		/// - Part of the composite key for rate lookup along with component code and repair code.
		/// 
		/// Example:
		/// 1. Job: Container wall repair
		///    - EstimateType = 'structural'
		///    - Matches rates in Labour Rates tab with structural repair rates.
		///    - Labor calculation uses area-based or linear measurements.
		/// 
		/// 2. Job: Reefer unit maintenance
		///    - EstimateType = 'machinery'
		///    - Matches specialized machinery repair rates.
		///    - Labor calculation typically uses fixed or time-based rates.
		/// </summary>
		ZString TI_EstimateType { get; }

		/// <summary>
		/// Used by: Container Yard (CYD) team for equipment quality classification.
		/// Location: Displayed in both Labour Rates and Material Rates tabs in Container Yard Client Rates.
		/// 
		/// References equipment grade from RefEquipmentGrade table to determine repair standards and pricing.
		/// 
		/// Rate Matching:
		/// - Matched against container's assigned equipment grade during rate search.
		/// - Critical for warranty repairs and premium equipment handling.
		/// - Used in both labor and material rate calculations.
		/// 
		/// Business Rules:
		/// - Higher grades require certified technicians (affects labor rates).
		/// - Specific material requirements based on grade (affects material costs).
		/// - May override standard repair procedures for premium equipment.
		/// 
		/// Example:
		/// - Job: Premium reefer container repair (Grade A)
		///   - Only matches rates with matching equipment grade.
		///   - Requires certified technician rates (higher labor cost).
		///   - Mandates OEM parts usage (specific material rates).
		///   - May have special warranty implications.
		/// </summary>
		ZGuid TI_REG_EquipmentGrade { get; }

		/// <summary>
		/// Used by: Container Yard (CYD) team for repair standard classification.
		/// Location: Available in Labour Rates and Material Rates tabs under Container Yard Client Rates.
		/// 
		/// Values:
		/// - 'CEDEX': Container Equipment Data Exchange standard.
		/// - 'MERC': Merchant-specific standards.
		/// 
		/// Rate Matching Process:
		/// 1. Job Creation:
		///    - M&R job or EOR specifies CEDEX/MERC classification.
		///    - System filters rates based on matching MNRGroup.
		/// 
		/// 2. Rate Application:
		///    - CEDEX:
		///      - Uses industry-standard repair codes (e.g., 'RPN' for Replace Panel).
		///      - Standard labor hours from CEDEX guidelines.
		///      - Fixed material specifications.
		///      - Example: Panel replacement on 20ft container
		///        - Labor: 2.5 hours (CEDEX standard).
		///        - Material: 2m² steel panel (CEDEX spec).
		/// 
		///    - MERC:
		///      - Custom repair standards.
		///      - Depot-specific labor rates.
		///      - Flexible material specifications.
		///      - Example: Custom reefer repair
		///        - Labor: Depot-specific hourly rate.
		///        - Materials: Preferred supplier parts.
		/// </summary>
		ZString TI_MNRGroup { get; }

		ZGuid TI_OH_TransportProvider { get; }
		ZGuid TI_TZ_OriginZone { get; }
		ZGuid TI_TZ_DestinationZone { get; }
		ZGuid OriginSuburbPK { get; }
		ZGuid DestinationSuburbPK { get; }
		ZBool TI_MatchContainerRateClass { get; }
		RateTransportZone OriginZone { get; }
		RateTransportZone DestinationZone { get; }
		ZDate TI_RateEndDate { get; }
		OrgHeader TransportProvider { get; }
		ZString TI_TransitTime { get; }
		ZString TI_FrequencyUnit { get; }
		ZInt TI_Frequency { get; }
		OrgHeader Supplier { get; }
		ZGuid TI_WW_Warehouse { get; }
		ZGuid TI_CYC_WW_Facility { get; }
		ZString TI_RH_NKCommodityCode { get; }
		ZString TI_FMCTariffID { get; }
		ZDate TI_RateStartDate { get; }
		ZString TI_ViaLRC { get; }
		ZBool TI_IsCrossTrade { get; }
		ZString TI_RS_NKServiceLevel_NI { get; }
		ZString TI_PL_NKCarrierServiceLevel { get; }
		ZString TI_RS_NKGatewayServiceLevel { get; }
		ZString TI_RS_NKShipmentGatewayServiceLevel { get; }
		ZGuid TI_OH_ControllingCustomer { get; }
		ZGuid TI_OH_Consignor { get; }
		ZGuid TI_OH_Consignee { get; }
		ZGuid TI_OA_CartageDeliveryAddressOverride { get; }
		ZGuid TI_OA_CartagePickupAddressOverride { get; }
		ZString TI_CartagePickupAddressPostCode { get; }
		ZString TI_CartageDeliveryAddressPostCode { get; }
		ZString TI_ContractNumber { get; }
		ZGuid PK { get; }
		OrgHeader ControllingCustomer { get; }
		OrgHeader Consignor { get; }
		OrgHeader Consignee { get; }
		ZGuid TI_R9_FromSuburb { get; }
		ZGuid TI_R9_ToSuburb { get; }
		ZString RateProvider { get; }
		ZBool TI_IsTact { get; }
		ZBool IsPublished { get; }
		ZGuid TI_ParentID { get; }
		ZString TI_ParentTableCode { get; }
		ZString TI_PaymentTerm { get; }
		ZString TI_GatewayAgentType { get; }
		IEnumerable<string> ReservedForJobIDs { get; }
		OrgAddress CartagePickupAddressOverride { get; }
		OrgAddress CartageDeliveryAddressOverride { get; }
		IEnumerable<string> NamedAccounts { get; }
		ZString TI_PlannedLoadLRC { get; }
		ZString TI_PlannedDischargeLRC { get; }
		ZString TI_FirstLoadLRC { get; }
		ZString TI_LastDischargeLRC { get; }
		ZString TI_FirstRouteSetLoadPortLRC { get; }
		ZString TI_LastRouteSetDischargePortLRC { get; }
		ZString CommodityGroup { get; }
		ZString TI_RateOrigin { get; }
		ZString TI_RateDestination { get; }
		ZBool TI_IsExcludedFromAutoRating { get; }
		ZString TI_AircraftType { get; }
		ZString TI_ShipmentConsolidationStatus { get; }
		ZString TI_HBLDeliveryMode { get; }
		ZString TI_IsNonOperatedReefer { get; }
		ZString TI_YardUnitType { get; }
		ZString TI_YardUnitLoad { get; }

		/// <summary>
		///		Container Payload Weight in KG
		/// </summary>
		ZDecimal ContainerPayloadWeight { get; }

		/// <summary>
		///		Container Payload Volume in M3
		/// </summary>
		ZDecimal ContainerPayloadVolume { get; }

		/// <summary>
		///		A string uniquely identifying the rate. It can be PK for CW1 rate and any rate ID provided by CargoSphere/Cargoguide in
		///		case of rates service rates. It must be no longer than <see cref="AutoJobChargeAttrib.Schema.EC_ValueMaxLength"/>.
		/// </summary>
		string RateId { get; }
	}

	public static class RateEntryInterfaceExtensions
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public static IZType GetValue(this IRateEntry entry, SchemaColumn column)
		{
			var bo = entry as BusinessObject;

			if (bo != null)
			{
				return (IZType)bo[column];
			}

			switch (column.Name)
			{
				case RateEntrySchema.Constants.TI_RC:
					return entry.TI_RC;
				case RateEntrySchema.Constants.TI_OH_ControllingCustomer:
					return entry.TI_OH_ControllingCustomer;
				case RateEntrySchema.Constants.TI_OH_Consignor:
					return entry.TI_OH_Consignor;
				case RateEntrySchema.Constants.TI_OH_Consignee:
					return entry.TI_OH_Consignee;
				case RateEntrySchema.Constants.TI_Mode:
					return entry.TI_Mode;
				case RateEntrySchema.Constants.TI_OriginLRC:
					return entry.TI_OriginLRC;
				case RateEntrySchema.Constants.TI_DestinationLRC:
					return entry.TI_DestinationLRC;
				case RateEntrySchema.Constants.TI_OH_Supplier:
					return entry.TI_OH_Supplier;
				case RateEntrySchema.Constants.TI_OA_CartageDeliveryAddressOverride:
					return entry.TI_OA_CartageDeliveryAddressOverride;
				case RateEntrySchema.Constants.TI_OA_CartagePickupAddressOverride:
					return entry.TI_OA_CartagePickupAddressOverride;
				case RateEntrySchema.Constants.TI_CartageDeliveryAddressPostCode:
					return entry.TI_CartageDeliveryAddressPostCode;
				case RateEntrySchema.Constants.TI_CartagePickupAddressPostCode:
					return entry.TI_CartagePickupAddressPostCode;
				case RateEntrySchema.Constants.TI_R9_FromSuburb:
					return entry.TI_R9_FromSuburb;
				case RateEntrySchema.Constants.TI_R9_ToSuburb:
					return entry.TI_R9_ToSuburb;
				case RateEntrySchema.Constants.TI_ParentID:
					return entry.TI_ParentID;
				case RateEntrySchema.Constants.TI_ParentTableCode:
					return entry.TI_ParentTableCode;
				case RateEntrySchema.Constants.TI_RH_NKCommodityCode:
					return entry.TI_RH_NKCommodityCode;
				case RateEntrySchema.Constants.TI_FMCTariffID:
					return entry.TI_FMCTariffID;
				case RateEntrySchema.Constants.TI_RS_NKServiceLevel_NI:
					return entry.TI_RS_NKServiceLevel_NI;
				case RateEntrySchema.Constants.TI_PL_NKCarrierServiceLevel:
					return entry.TI_PL_NKCarrierServiceLevel;
				case RateEntrySchema.Constants.TI_RS_NKGatewayServiceLevel:
					return entry.TI_RS_NKGatewayServiceLevel;
				case RateEntrySchema.Constants.TI_RS_NKShipmentGatewayServiceLevel:
					return entry.TI_RS_NKShipmentGatewayServiceLevel;
				case RateEntrySchema.Constants.TI_OH_TransportProvider:
					return entry.TI_OH_TransportProvider;
				case RateEntrySchema.Constants.TI_ViaLRC:
					return entry.TI_ViaLRC;
				case RateEntrySchema.Constants.TI_IsTact:
					return entry.TI_IsTact;
				case RateEntrySchema.Constants.TI_PaymentTerm:
					return entry.TI_PaymentTerm;
				case RateEntrySchema.Constants.TI_GatewayAgentType:
					return entry.TI_GatewayAgentType;
				case RateEntrySchema.Constants.TI_PlannedLoadLRC:
					return entry.TI_PlannedLoadLRC;
				case RateEntrySchema.Constants.TI_PlannedDischargeLRC:
					return entry.TI_PlannedDischargeLRC;
				case RateEntrySchema.Constants.TI_FirstLoadLRC:
					return entry.TI_FirstLoadLRC;
				case RateEntrySchema.Constants.TI_LastDischargeLRC:
					return entry.TI_LastDischargeLRC;
				case RateEntrySchema.Constants.TI_FirstRouteSetLoadPortLRC:
					return entry.TI_FirstRouteSetLoadPortLRC;
				case RateEntrySchema.Constants.TI_LastRouteSetDischargePortLRC:
					return entry.TI_LastRouteSetDischargePortLRC;
				case RateEntrySchema.Constants.TI_RateOrigin:
					return entry.TI_RateOrigin;
				case RateEntrySchema.Constants.TI_RateDestination:
					return entry.TI_RateDestination;
				case RateEntrySchema.Constants.TI_AircraftType:
					return entry.TI_AircraftType;
				case RateEntrySchema.Constants.TI_ShipmentConsolidationStatus:
					return entry.TI_ShipmentConsolidationStatus;
				case RateEntrySchema.Constants.TI_IsNonOperatedReefer:
					return entry.TI_IsNonOperatedReefer;
				case RateEntrySchema.Constants.TI_YardUnitType:
					return entry.TI_YardUnitType;
				case RateEntrySchema.Constants.TI_YardUnitLoad:
					return entry.TI_YardUnitLoad;
				case RateEntrySchema.Constants.TI_RCC_ComponentCode:
					return entry.TI_RCC_ComponentCode;
				case RateEntrySchema.Constants.TI_ContainerUnitSection:
					return entry.TI_ContainerUnitSection;
				case RateEntrySchema.Constants.TI_RRC_RepairCode:
					return entry.TI_RRC_RepairCode;
				case RateEntrySchema.Constants.TI_RMC_Material:
					return entry.TI_RMC_Material;
				case RateEntrySchema.Constants.TI_ContractNumber:
					return entry.TI_ContractNumber;
				case RateEntrySchema.Constants.TI_EstimateType:
					return entry.TI_EstimateType;
				case RateEntrySchema.Constants.TI_REG_EquipmentGrade:
					return entry.TI_REG_EquipmentGrade;
				case RateEntrySchema.Constants.TI_MNRGroup:
					return entry.TI_MNRGroup;
			}

			throw new NotImplementedException(column.Name);
		}
	}
}
