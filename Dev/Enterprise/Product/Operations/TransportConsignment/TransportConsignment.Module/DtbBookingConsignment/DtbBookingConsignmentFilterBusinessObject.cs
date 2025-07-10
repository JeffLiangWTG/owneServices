using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Packing.Business;
using Enterprise.Rating.Business;
using Enterprise.Security;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Module;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbBookingConsignmentFilterBusinessObject : DtbTransportFilterBusinessObject<DtbBookingConsignment>
	{
		#region FilterConstants

		public static class FilterConstants
		{
			public const string ActualDelivery = "ActualDelivery";
			public const string ActualPickUp = "ActualPickUp";
			public const string BookingID = "BookingID";
			public const string BookingParty = "BookingParty";
			public const string BillingParty = "BillingParty";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
			public const string Carrier = "Carrier";
			public const string ConsignmentID = "ConsignmentID";
			public const string ConsignmentJobType = "ConsignmentJobType";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
			public const string Consignee = "Consignee";
			public const string ConsigneeDropMode = "ConsigneeDropMode";
			public const string ConsigneeRef = "ConsigneeRef";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
			public const string Consignor = "Consignor";
			public const string ConsignorDropMode = "ConsignorDropMode";
			public const string ConsignorRef = "ConsignorRef";
			public const string CreateUser = "CreateUser";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
			public const string Depot = "Depot";
			public const string DestinationCity = "DestinationCity";
			public const string DestinationPostcode = "DestinationPostcode";
			public const string DestinationState = "DestinationState";
			public const string PickupRequestedFrom = "PickupRequestedFrom"; // Filter name
			public const string PickupRequestedTo = "PickupRequestedTo"; // Filter name
			public const string PickupEstimated = "PickupEstimated"; // Filter name
			public const string DeliveryRequestedFrom = "DeliveryRequestedFrom"; // Filter name
			public const string DeliveryRequestedTo = "DeliveryRequestedTo"; // Filter name
			public const string DeliveryEstimated = "DeliveryEstimated"; // Filter name
			public const string HasDelivery = "HasDelivery"; // Filter name
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
			public const string Hazardous = "Hazardous";
			public const string LastEditTime = "LastEditTime";
			public const string LastEditUser = "LastEditUser";
			public const string OriginCity = "OriginCity";
			public const string OriginPostcode = "OriginPostcode";
			public const string OriginState = "OriginState";
			public const string PackageID = "PackageID";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
			public const string Refrigerated = "Refrigerated";
			public const string ServiceLevel = "ServiceLevel";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
			public const string Status = "Status";
			public const string TransportReference = "TransportReference";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
			public const string Zone = "Zone";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
			public const string ConsignmentAdditionalReference = "Additional Reference #";
			public const string SpecialInstructionsExist = "SpecialInstructionsExist"; // Filter name
		}

		#endregion

		#region ModuleFilters

		protected override void AddModuleFilters(ModuleFilterCollection filters)
		{
			AddDateFilters(filters);
			AddLocationFilters(filters);
			AddNumbersAndReferencesFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddModesAndTypesFilters(filters);
		}

		#region AddAccountingFilters

		protected override SecurityCheckpoint GetSecurityCheckPoint()
		{
			return Env.Security.None;
		}

		protected override bool IsAddRevenueFilters
		{
			get { return true; }
		}

		#endregion

		#region Dates Filters

		void AddDateFilters(ModuleFilterCollection filters)
		{
			var actualDeliveryFilter = filters.AddDateFilter(FilterConstants.ActualDelivery, GetActualDeliveryQuery);
			actualDeliveryFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|ActualDelivery", "Actual Delivery");

			var actualPickUpFilter = filters.AddDateFilter(FilterConstants.ActualPickUp, GetActualPickUpQuery);
			actualPickUpFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|ActualPickUp", "Actual Pickup");

			var pickupRequestedFromFilter = filters.AddDateFilter(FilterConstants.PickupRequestedFrom, GetPickupRequestedFromQuery);
			pickupRequestedFromFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|PickupRequestedFrom", "Pickup Requested From");

			var pickupRequestedToFilter = filters.AddDateFilter(FilterConstants.PickupRequestedTo, GetPickupRequestedToQuery);
			pickupRequestedToFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|PickupRequestedTo", "Pickup Requested To");

			var deliveryRequestedFromFilter = filters.AddDateFilter(FilterConstants.DeliveryRequestedFrom, GetDeliveryRequestedFromQuery);
			deliveryRequestedFromFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|DeliveryRequestedFrom", "Delivery Requested From");

			var deliveryRequestedToFilter = filters.AddDateFilter(FilterConstants.DeliveryRequestedTo, GetDeliveryRequestedToQuery);
			deliveryRequestedToFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|DeliveryRequestedTo", "Delivery Requested To");

			var estimatedDeliveryFilter = filters.AddDateFilter(FilterConstants.DeliveryEstimated, GetEstimatedDeliveryQuery);
			estimatedDeliveryFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|DeliveryEstimated", "Delivery Estimated");

			var estimatedPickUpFilter = filters.AddDateFilter(FilterConstants.PickupEstimated, GetEstimatedPickUpQuery);
			estimatedPickUpFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|PickupEstimated", "Pickup Estimated");
		}

		ZQuery GetActualDeliveryQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(InstructionTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_Actual, comparisonOperator, date1, date2);
		}

		ZQuery GetActualPickUpQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(InstructionTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_Actual, comparisonOperator, date1, date2);
		}

		ZQuery GetPickupRequestedFromQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(InstructionTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_RequiredFrom, comparisonOperator, date1, date2);
		}

		ZQuery GetPickupRequestedToQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(InstructionTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_RequiredTo, comparisonOperator, date1, date2);
		}

		ZQuery GetEstimatedPickUpQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(InstructionTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_Estimated, comparisonOperator, date1, date2);
		}

		ZQuery GetDeliveryRequestedFromQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(InstructionTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_RequiredFrom, comparisonOperator, date1, date2);
		}

		ZQuery GetDeliveryRequestedToQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(InstructionTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_RequiredTo, comparisonOperator, date1, date2);
		}

		ZQuery GetEstimatedDeliveryQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(InstructionTypes.Codes.Delivery, DtbBookingConfirmationSchema.KK_Estimated, comparisonOperator, date1, date2);
		}

		ZQuery GetConfirmationDateQuery(string instructionType, SchemaDateTimeColumn schemaColumn, DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var confirmationSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentConfirmation), DtbBookingConfirmationSchema.KK_KN_BookingInstruction);
			AddDateRange(confirmationSubQuery, comparisonOperator, JoinCondition.And, schemaColumn, fromDate.Date, toDate.Date);

			var instructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
			instructionSubQuery.AddSubQuery(confirmationSubQuery, JoinCondition.And);
			instructionSubQuery.AddToFilter(DtbBookingInstructionSchema.KN_InstructionType, instructionType);

			var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));
			consignmentQuery.AddSubQuery(instructionSubQuery, JoinCondition.And);
			return consignmentQuery;
		}

		#endregion

		#region Location Filters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var cityMaxLength = Math.Min(OrgAddressSchema.OA_City.MaxLength, JobDocAddressSchema.E2_City.MaxLength);
			var postcodeMaxLength = Math.Min(OrgAddressSchema.OA_PostCode.MaxLength, JobDocAddressSchema.E2_Postcode.MaxLength);
			var stateMaxLength = Math.Min(OrgAddressSchema.OA_State.MaxLength, JobDocAddressSchema.E2_State.MaxLength);

			var destinationCityFilter = filters.AddTextFilter(FilterConstants.DestinationCity, GetDestinationCityQuery);
			destinationCityFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|DestinationCity", "Destination City");
			destinationCityFilter.Category = FilterCategories.Locations;
			destinationCityFilter.MaxLength = cityMaxLength;

			var destinationPostcodeFilter = filters.AddTextFilter(FilterConstants.DestinationPostcode, GetDestinationPostcodeQuery);
			destinationPostcodeFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|DestinationPostcode", "Destination Postcode");
			destinationPostcodeFilter.Category = FilterCategories.Locations;
			destinationPostcodeFilter.MaxLength = postcodeMaxLength;

			var destinationStateFilter = filters.AddTextFilter(FilterConstants.DestinationState, GetDestinationStateQuery);
			destinationStateFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|DestinationState", "Destination State");
			destinationStateFilter.Category = FilterCategories.Locations;
			destinationStateFilter.MaxLength = stateMaxLength;

			var originCityFilter = filters.AddTextFilter(FilterConstants.OriginCity, GetOriginCityQuery);
			originCityFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|OriginCity", "Origin City");
			originCityFilter.Category = FilterCategories.Locations;
			originCityFilter.MaxLength = cityMaxLength;

			var originPostcodeFilter = filters.AddTextFilter(FilterConstants.OriginPostcode, GetOriginPostcodeQuery);
			originPostcodeFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|OriginPostcode", "Origin Postcode");
			originPostcodeFilter.Category = FilterCategories.Locations;
			originPostcodeFilter.MaxLength = postcodeMaxLength;

			var originStateFilter = filters.AddTextFilter(FilterConstants.OriginState, GetOriginStateQuery);
			originStateFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|OriginState", "Origin State");
			originStateFilter.Category = FilterCategories.Locations;
			originStateFilter.MaxLength = stateMaxLength;

			var zoneFilter = filters.AddGuidFilter(FilterConstants.Zone, ModuleIDs.RateTransportZone, GetZoneQuery, Zones);
			zoneFilter.Category = FilterCategories.Locations;
			zoneFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|Zone", "Zone");
			zoneFilter.SubGroup = ConsignmentInstructionSubGroup;
		}

		ZQuery GetDestinationCityQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(InstructionTypes.Codes.Delivery, OrgAddressSchema.OA_City, JobDocAddressSchema.E2_City, comparisonOperator, value);
		}

		ZQuery GetDestinationPostcodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(InstructionTypes.Codes.Delivery, OrgAddressSchema.OA_PostCode, JobDocAddressSchema.E2_Postcode, comparisonOperator, value);
		}

		ZQuery GetDestinationStateQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(InstructionTypes.Codes.Delivery, OrgAddressSchema.OA_State, JobDocAddressSchema.E2_State, comparisonOperator, value);
		}

		ZQuery GetOriginCityQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(InstructionTypes.Codes.PickUp, OrgAddressSchema.OA_City, JobDocAddressSchema.E2_City, comparisonOperator, value);
		}

		ZQuery GetOriginPostcodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(InstructionTypes.Codes.PickUp, OrgAddressSchema.OA_PostCode, JobDocAddressSchema.E2_Postcode, comparisonOperator, value);
		}

		ZQuery GetOriginStateQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(InstructionTypes.Codes.PickUp, OrgAddressSchema.OA_State, JobDocAddressSchema.E2_State, comparisonOperator, value);
		}

		ZQuery GetAddressQuery(string instructionType, SchemaStringColumn orgAddressColumn, SchemaStringColumn jobDocAddressColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var instructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
			instructionSubQuery.AddToFilter(DtbBookingInstructionSchema.KN_InstructionType, instructionType);

			var jobDocAddressQuery = JobDocAddressQueryHelper.JobDocAddressFieldSearchDBSubQuery(comparisonOperator, orgAddressColumn, jobDocAddressColumn, value);
			instructionSubQuery.AddSubQuery(jobDocAddressQuery, JoinCondition.And);

			var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));
			consignmentQuery.AddSubQuery(instructionSubQuery, JoinCondition.And);
			return consignmentQuery;
		}

		ZQuery GetZoneQuery(ZGuid zonePK)
		{
			var instructionQuery = new ZQuery();
			instructionQuery.AddToFilter(DtbBookingInstructionSchema.KN_TZ_DomesticZone, zonePK);

			return instructionQuery;
		}

		#endregion

		#region Modes And Types Filters

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			var serviceLevelFilter = filters.AddNkFilter(FilterConstants.ServiceLevel, DtbBookingSchema.KM_RS_NKServiceLevel, ModuleIDs.ServiceLevel, ServiceLevels);
			serviceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|ServiceLevel", "Service Level");
			serviceLevelFilter.Category = FilterCategories.ModesAndTypes;

			var consignorDropModeFilter = filters.AddTextFilter(FilterConstants.ConsignorDropMode, GetConsignorDropModeQuery, DropModeList);
			consignorDropModeFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|ConsignorDropMode", "Consignor Drop Mode");
			consignorDropModeFilter.Category = FilterCategories.ModesAndTypes;

			var consigneeDropModeFilter = filters.AddTextFilter(FilterConstants.ConsigneeDropMode, GetConsigneeDropModeQuery, DropModeList);
			consigneeDropModeFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|ConsigneeDropMode", "Consignee Drop Mode");
			consigneeDropModeFilter.Category = FilterCategories.ModesAndTypes;
		}

		ZQuery GetConsignorDropModeQuery(ZString dropMode)
		{
			return GetDropModesQuery(InstructionTypes.Codes.PickUp, dropMode);
		}

		ZQuery GetConsigneeDropModeQuery(ZString dropMode)
		{
			return GetDropModesQuery(InstructionTypes.Codes.Delivery, dropMode);
		}

		static ZQuery GetDropModesQuery(ZString instructionType, ZString dropMode)
		{
			var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));

			if (dropMode != "ANY")
			{
				var instructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
				instructionSubQuery.AddToFilter(DtbBookingInstructionSchema.KN_InstructionType, instructionType);
				instructionSubQuery.AddToFilter(DtbBookingInstructionSchema.KN_DropMode, dropMode);

				consignmentQuery.AddSubQuery(instructionSubQuery, JoinCondition.And);
			}
			return consignmentQuery;
		}

		#endregion

		#region Numbers And References Filters

		void AddNumbersAndReferencesFilters(ModuleFilterCollection moduleFilters)
		{
			var bookingIDFilter = moduleFilters.AddFountainFilter(FilterConstants.BookingID, GetBookingIDQuery, "TB");
			bookingIDFilter.Category = FilterCategories.NumbersAndReferences;
			bookingIDFilter.SubGroup = ConsignmentConsolidationSubGroup;
			bookingIDFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|BookingID", "Booking ID");
			bookingIDFilter.MaxLength = DtbBookingSchema.KM_JobID.MaxLength;

			var consignmentFilter = moduleFilters.AddTextFilter(FilterConstants.ConsignmentID, DtbBookingSchema.KM_JobID);
			consignmentFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|ConsignmentID", "Consignment ID");
			consignmentFilter.Category = FilterCategories.NumbersAndReferences;
			consignmentFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			consignmentFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			var consigneeRefFilter = moduleFilters.AddTextFilter(FilterConstants.ConsigneeRef, GetConsigneeRefQuery);
			consigneeRefFilter.Category = FilterCategories.NumbersAndReferences;
			consigneeRefFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|ConsigneeRef", "Consignee Ref");
			consigneeRefFilter.MaxLength = DtbBookingConfirmationSchema.KK_ReferenceNum.MaxLength;

			var consignorRefFilter = moduleFilters.AddTextFilter(FilterConstants.ConsignorRef, GetConsignorRefQuery);
			consignorRefFilter.Category = FilterCategories.NumbersAndReferences;
			consignorRefFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|ConsignorRef", "Consignor Ref");
			consignorRefFilter.MaxLength = DtbBookingConfirmationSchema.KK_ReferenceNum.MaxLength;

			var packageIDFilter = moduleFilters.AddTextFilter(FilterConstants.PackageID, GetPackageIDQuery);
			packageIDFilter.Category = FilterCategories.NumbersAndReferences;
			packageIDFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|PackageID", "Package ID");
			packageIDFilter.MaxLength = PkgPackageHeaderSchema.KPH_PackageID.MaxLength;

			var transportReferenceFilter = moduleFilters.AddTextFilter(FilterConstants.TransportReference, DtbBookingSchema.KM_TransportReference);
			transportReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			transportReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|Connote No", "Connote No.");

			moduleFilters.AddCustomFilter(new ReferenceNumberFilter(
				FilterConstants.ConsignmentAdditionalReference,
				new ReferenceNumberFilterHelper<DtbBookingConsignment>().GetReferenceNumberFilter,
				new RefCountryCollection(Factory),
				GetAdditionalReferenceNumberTypes()
				)
			{ MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|ReferenceNumbers", "Additional Reference #") });
		}

		#region GetBookingIDQuery

		ZQuery GetBookingIDQuery(SQLComparisonOperator @operator, ZString value)
		{
			var bookingConsolidationSubQuery = new ZDBOnlySubQuery(typeof(DtbTransportConsolidation), DtbBookingSchema.KM_KB_Booking);
			bookingConsolidationSubQuery.AddToFilter(DtbBookingConsolidationSchema.KB_JobType, TransportConsolidationJobTypes.Codes.Booking);

			var bookingSubQuery = new ZDBOnlySubQuery(typeof(DtbTransport), DtbBookingConsolidationSchema.KB_ParentID);
			bookingSubQuery.AddToFilter_PossiblyCommaSeparated(DtbBookingSchema.KM_JobID, @operator, value);
			bookingSubQuery.AddSubQuery(bookingConsolidationSubQuery, JoinCondition.And);

			var consignmentQuery = new ZDBOnlyQuery(typeof(DtbConsignmentConsolidation));
			consignmentQuery.AddSubQuery(bookingSubQuery, JoinCondition.And);
			return consignmentQuery;
		}

		#endregion

		#region ConfirmationRefQuery

		ZQuery GetConsigneeRefQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetConfirmationRefQuery(InstructionTypes.Codes.Delivery, comparisonOperator, value);
		}

		ZQuery GetConsignorRefQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetConfirmationRefQuery(InstructionTypes.Codes.PickUp, comparisonOperator, value);
		}

		ZQuery GetConfirmationRefQuery(string instructionType, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var confirmationSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentConfirmation), DtbBookingConfirmationSchema.KK_KN_BookingInstruction);
			confirmationSubQuery.AddToFilter(DtbBookingConfirmationSchema.KK_ReferenceNum, comparisonOperator, value);

			var instructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
			instructionSubQuery.AddSubQuery(confirmationSubQuery, JoinCondition.And);
			instructionSubQuery.AddToFilter(DtbBookingInstructionSchema.KN_InstructionType, instructionType);

			var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));
			consignmentQuery.AddSubQuery(instructionSubQuery, JoinCondition.And);
			return consignmentQuery;
		}

		#endregion

		#region GetPackageIDQuery

		ZQuery GetPackageIDQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var packageIdSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHeader), PkgPackageSchema.KP_KPH_PackageHeader);
			packageIdSubQuery.AddToFilter(PkgPackageHeaderSchema.KPH_PackageID, comparisonOperator, value);

			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
			packageSubQuery.AddSubQuery(packageIdSubQuery, JoinCondition.And);

			var packageJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
			packageJobSubQuery.AddSubQuery(packageSubQuery, JoinCondition.And);

			var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));
			consignmentQuery.AddSubQuery(packageJobSubQuery, JoinCondition.And);
			return consignmentQuery;
		}

		#endregion

		#endregion

		#region Organisation Filters

		protected override void AddOrganisationFiltersCore(ModuleFilterCollection filters)
		{
			var carrierFilter = filters.AddGuidFilter(FilterConstants.Carrier, ModuleIDs.Organisation, GetCarrierQuery, new OrgHeaderCollection(Factory));
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|Carrier", "Carrier");

			var bookingPartyFilter = filters.AddGuidFilter(FilterConstants.BookingParty, ModuleIDs.Organisation, GetBookingPartyQuery, new OrgHeaderCollection(Factory));
			bookingPartyFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|BookingParty", "Booking Party");

			var billingPartyFilter = filters.AddGuidFilter(FilterConstants.BillingParty, ModuleIDs.Organisation, GetBillingPartyQuery, new OrgHeaderCollection(Factory));
			billingPartyFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|BillingParty", "Billing Party");

			var consigneeFilter = filters.AddGuidFilter(FilterConstants.Consignee, ModuleIDs.Organisation, GetConsigneeQuery, new OrgHeaderCollection(Factory));
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|Consignee", "Consignee");

			var consignorFilter = filters.AddGuidFilter(FilterConstants.Consignor, ModuleIDs.Organisation, GetConsignorQuery, new OrgHeaderCollection(Factory));
			consignorFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|Consignor", "Consignor");

			var depotFilter = filters.AddGuidFilter(FilterConstants.Depot, ModuleIDs.Organisation, GetDepotQuery, new OrgHeaderCollection(Factory));
			depotFilter.Category = FilterCategories.Locations;
			depotFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|Depot", "Depot");
			depotFilter.SubGroup = ConsignmentInstructionSubGroup;
		}

		ZQuery GetCarrierQuery(ZGuid orgGuid)
		{
			var runSheetSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheet), DtbConsignmentRunSheetInstructionSchema.K1_KG_RunSheet);
			runSheetSubQuery.AddToFilter(DtbConsignmentRunSheetSchema.KG_OH_TransportCo, orgGuid);

			var runSheetInstructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentRunSheetInstruction), DtbBookingConfirmationSchema.KK_K1_RunSheetInstruction);
			runSheetInstructionSubQuery.AddSubQuery(runSheetSubQuery, JoinCondition.And);

			var confirmationSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentConfirmation), DtbBookingConfirmationSchema.KK_KN_BookingInstruction);
			confirmationSubQuery.AddSubQuery(runSheetInstructionSubQuery, JoinCondition.And);

			var instructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
			instructionSubQuery.AddSubQuery(confirmationSubQuery, JoinCondition.And);

			var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));
			consignmentQuery.AddSubQuery(instructionSubQuery, JoinCondition.And);
			return consignmentQuery;
		}

		ZQuery GetBookingPartyQuery(ZGuid orgGuid)
		{
			var jobDocAddressQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(DocAddressTypes.Codes.BookingPartyDocumentaryAddress, orgGuid);

			var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));
			var consolidationSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentConsolidation), DtbBookingSchema.KM_KB_Booking);
			consolidationSubQuery.AddSubQuery(jobDocAddressQuery, JoinCondition.And);
			consignmentQuery.AddSubQuery(consolidationSubQuery, JoinCondition.And);
			return consignmentQuery;
		}

		ZQuery GetBillingPartyQuery(ZGuid orgGuid)
		{
			// Job header client
			var jobHeaderClientQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobHeaderSchema.JH_OA_LocalChargesAddr);
			jobHeaderClientQuery.AddToFilter(OrgAddressSchema.OA_OH, orgGuid);

			var jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID);
			jobHeaderQuery.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			jobHeaderQuery.AddSubQuery(jobHeaderClientQuery, JoinCondition.And);

			// CRB
			var jobDocAddressQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(DocAddressTypes.Codes.ClientRequestedBillingParty, orgGuid);

			var headerNotInFilter = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.JH_ParentID, true);
			headerNotInFilter.AddToFilter(JoinCondition.And, JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			jobDocAddressQuery.AddSubQuery(JobDocAddressSchema.E2_ParentID, headerNotInFilter, JoinCondition.And);

			var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));
			consignmentQuery.AddSubQuery(jobHeaderQuery, JoinCondition.And);
			consignmentQuery.AddSubQuery(jobDocAddressQuery, JoinCondition.Or);
			return consignmentQuery;
		}

		ZQuery GetConsigneeQuery(ZGuid orgGuid)
		{
			return GetConsigneeConsignorQuery(InstructionTypes.Codes.Delivery, orgGuid);
		}

		ZQuery GetConsignorQuery(ZGuid orgGuid)
		{
			return GetConsigneeConsignorQuery(InstructionTypes.Codes.PickUp, orgGuid);
		}

		ZQuery GetConsigneeConsignorQuery(string instructionType, ZGuid orgGuid)
		{
			var jobDocAddressQuery = JobDocAddressQueryHelper.JobDocAddressOrgHeaderParentSubQuery(orgGuid);

			var instructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
			instructionSubQuery.AddToFilter(DtbBookingInstructionSchema.KN_InstructionType, instructionType);
			instructionSubQuery.AddSubQuery(jobDocAddressQuery, JoinCondition.And);

			var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));
			consignmentQuery.AddSubQuery(instructionSubQuery, JoinCondition.And);
			return consignmentQuery;
		}

		ZQuery GetDepotQuery(ZGuid depotPK)
		{
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), PortHubSelectionSchema.TY_OA_DepotAddress);
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_OH, depotPK);

			var portHubSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IPortHubSelection>(), PortHubZonePivotSchema.TX_TY_Hub);
			portHubSubQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);

			var pivotSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IPortHubZonePivot>(), PortHubZonePivotSchema.TX_TZ_Zone);
			pivotSubQuery.AddSubQuery(portHubSubQuery, JoinCondition.And);

			var instructionQuery = new ZDBOnlyQuery(typeof(DtbConsignmentInstruction));
			instructionQuery.AddSubQuery(DtbBookingInstructionSchema.KN_TZ_DomesticZone, pivotSubQuery, JoinCondition.And);

			return instructionQuery;
		}

		#endregion

		#region Status And Flags Filters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var specialInstructionsExistFilter = filters.AddTextFilter(FilterConstants.SpecialInstructionsExist, SpecialInstuctionsExistQuery, SpecialInstructionsExistList);
			specialInstructionsExistFilter.MultilingualDescription = (ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|SpecialInstructionsExist", "Special Instructions Exist"));
			specialInstructionsExistFilter.Category = FilterCategories.StatusAndFlags;

			var hasDeliveryFilter = filters.AddTextFilter(FilterConstants.HasDelivery, HasDeliveryQuery, HasDeliveryList);
			hasDeliveryFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|HasDelivery", "Has Delivery");
			hasDeliveryFilter.Category = FilterCategories.StatusAndFlags;
			hasDeliveryFilter.SubGroup = ConsignmentInstructionSubGroup;

			var isHazardousFilter = filters.AddTextFilter(FilterConstants.Hazardous, HazardousQuery, IsHazardousList);
			isHazardousFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|IsHazardous", "Hazardous");
			isHazardousFilter.Category = FilterCategories.StatusAndFlags;

			var isRefrigeratedFilter = filters.AddTextFilter(FilterConstants.Refrigerated, RefrigeratedQuery, IsRefrigeratedList);
			isRefrigeratedFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|IsRefrigerated", "Refrigeration");
			isRefrigeratedFilter.Category = FilterCategories.StatusAndFlags;

			var consignmentStatusFilter = filters.AddTextFilter(FilterConstants.Status, DtbBookingSchema.KM_Status, ConsignmentStatusList);
			consignmentStatusFilter.MultilingualDescription = ResString.GetMultilingualString("DtbConsignmentFilterBusinessObject|Status", "Consignment Status");
			consignmentStatusFilter.Category = FilterCategories.StatusAndFlags;
		}

		#region Special Instuctions Exist Query

		ZQuery SpecialInstuctionsExistQuery(ZString showConsignmentsWithInstructions)
		{
			var query = new ZQuery();

			if (showConsignmentsWithInstructions.EqualsIgnoringCase(SpecialInstructionsExistStatuses.Codes.Yes))
			{
				query = ConsignmentInstructionsQuery(true);
			}
			else if (showConsignmentsWithInstructions.EqualsIgnoringCase(SpecialInstructionsExistStatuses.Codes.No))
			{
				query = ConsignmentInstructionsQuery(false);
			}

			return query;
		}

		ZQuery ConsignmentInstructionsQuery(bool isTrue)
		{
			var instructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement, notIn: !isTrue);
			instructionSubQuery.AddToFilter(DtbBookingInstructionSchema.KN_ServiceInstruction, SQLComparisonOperator.NotEqual, "");

			var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));
			consignmentQuery.AddSubQuery(instructionSubQuery, JoinCondition.And);

			return consignmentQuery;
		}

		#endregion

		#region HasDeliveryQuery

		ZQuery HasDeliveryQuery(ZString value)
		{
			ZQuery result;

			if (value.EqualsIgnoringCase(HasDeliveryStatuses.Codes.HasDelivery))
			{
				var instructionsQuery = new ZDBOnlyQuery(typeof(DtbConsignmentInstruction));
				AddAddressQuery(instructionsQuery, JoinCondition.And, hasAddress: true);

				result = instructionsQuery;
			}
			else if (value.EqualsIgnoringCase(HasDeliveryStatuses.Codes.HasNoDelivery))
			{
				var instructionsQuery = new ZDBOnlyQuery(typeof(DtbConsignmentInstruction));
				instructionsQuery.AddFilterAndZSQLParameterCollection(@"
NOT EXISTS
(
	SELECT NULL
	FROM dbo.JobDocAddress WHERE E2_ParentID = KN_PK
)", new ZSqlParameterCollection());

				AddAddressQuery(instructionsQuery, JoinCondition.Or, hasAddress: false);

				result = instructionsQuery;
			}
			else
			{
				result = new ZQuery();
			}

			return result;
		}

		void AddAddressQuery(ZDBOnlyQuery instructionsQuery, JoinCondition subQueryJoinCondition, bool hasAddress)
		{
			// we check for address with 'In' and check for no address with 'NotIn'
			var jobDocAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn: !hasAddress);
			var hasAddressQuery = new ZQuery();
			hasAddressQuery.AddToFilter(JobDocAddressSchema.E2_OA_Address, SQLComparisonOperator.NotEqual, null);
			hasAddressQuery.AddToFilter(JoinCondition.Or, JobDocAddressSchema.E2_AddressOverride, true);
			jobDocAddressSubQuery.AddToFilter(hasAddressQuery);

			instructionsQuery.AddSubQuery(jobDocAddressSubQuery, subQueryJoinCondition);
			instructionsQuery.AddToFilter(DtbBookingInstructionSchema.KN_InstructionType, InstructionTypes.Codes.Delivery);
		}

		#endregion

		#region Hazardous / Refrigerated Query

		ZQuery HazardousQuery(ZString hazardousStatus)
		{
			return StatusQuery(DtbBookingSchema.KM_IsHazardous, hazardousStatus != IsHazardousStatuses.Codes.Both, hazardousStatus == IsHazardousStatuses.Codes.Hazardous);
		}

		ZQuery RefrigeratedQuery(ZString refrigeratedStatus)
		{
			return StatusQuery(DtbBookingSchema.KM_RequiresRefrigeration, refrigeratedStatus != IsRefrigeratedStatuses.Codes.Both, refrigeratedStatus == IsRefrigeratedStatuses.Codes.Refrigerated);
		}

		ZQuery StatusQuery(SchemaBoolColumn column, bool notBoth, bool isTrue)
		{
			var query = new ZQuery();
			if (notBoth)
			{
				query.AddToFilter(column, isTrue);
			}
			return query;
		}

		#endregion

		#endregion

		#endregion

		#region SubGroups

		#region ConsignmentConsolidationSubGroup

		ConsignmentConsolidationFilterSubGroup ConsignmentConsolidationSubGroup
		{
			get { return consignmentConsolidationSubGroup ?? (consignmentConsolidationSubGroup = new ConsignmentConsolidationFilterSubGroup()); }
		}

		ConsignmentConsolidationFilterSubGroup consignmentConsolidationSubGroup;

		class ConsignmentConsolidationFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var consolidationSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentConsolidation), DtbBookingSchema.KM_KB_Booking);
				consolidationSubQuery.AddToFilter(filter);

				var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));
				consignmentQuery.AddSubQuery(consolidationSubQuery, JoinCondition.And);

				return consignmentQuery;
			}
		}

		#endregion

		#region ConsignmentInstructionSubGroup

		ConsignmentInstructionFilterSubGroup ConsignmentInstructionSubGroup
		{
			get { return consignmentInstructionSubGroup ?? (consignmentInstructionSubGroup = new ConsignmentInstructionFilterSubGroup()); }
		}

		ConsignmentInstructionFilterSubGroup consignmentInstructionSubGroup;

		class ConsignmentInstructionFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var instructionSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentInstruction), DtbBookingInstructionSchema.KN_KM_BookingMovement);
				instructionSubQuery.AddToFilter(filter);

				var consignmentQuery = new ZDBOnlyQuery(typeof(DtbBookingConsignment));
				consignmentQuery.AddSubQuery(instructionSubQuery, JoinCondition.And);

				return consignmentQuery;
			}
		}

		#endregion

		#endregion

		#region Lists

		#region ConsignmentStatusList

		CodeDescriptionPairList ConsignmentStatusList
		{
			get { return Factory.GetCachedValue("DtbConsignmentFilterBusinessObject|ConsignmentStatusList", () => GetConsignmentStatusList()); }
		}

		CodeDescriptionPairList GetConsignmentStatusList()
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(TransportStatuses.Codes.Available, TransportStatuses.Descriptions.Available);
			list.AddPair(TransportStatuses.Codes.Delivered, TransportStatuses.Descriptions.Delivered);
			list.AddPair(TransportStatuses.Codes.DeliveryAllocated, TransportStatuses.Descriptions.DeliveryAllocated);
			list.AddPair(TransportStatuses.Codes.PickedUp, TransportStatuses.Descriptions.PickedUp);
			list.AddPair(TransportStatuses.Codes.PickUpAllocated, TransportStatuses.Descriptions.PickUpAllocated);
			return list;
		}

		#endregion

		#region SpecialInstructionsExistList

		CodeDescriptionPairList SpecialInstructionsExistList
		{
			get { return Factory.GetCachedValue("DtbConsignmentFilterBusinessObject|SpecialInstructionsExistList", () => new SpecialInstructionsExistStatuses()); }
		}

		#endregion

		#region HasDeliveryList

		CodeDescriptionPairList HasDeliveryList
		{
			get { return Factory.GetCachedValue("DtbConsignmentFilterBusinessObject|HasDeliveryList", () => new HasDeliveryStatuses()); }
		}

		#endregion

		#region IsHazardousList

		CodeDescriptionPairList IsHazardousList
		{
			get { return Factory.GetCachedValue("DtbConsignmentFilterBusinessObject|IsHazardousList", () => new IsHazardousStatuses()); }
		}

		#endregion

		#region IsRefrigeratedList

		CodeDescriptionPairList IsRefrigeratedList
		{
			get { return Factory.GetCachedValue("DtbConsignmentFilterBusinessObject|IsRefrigeratedList", () => new IsRefrigeratedStatuses()); }
		}

		#endregion

		#region DropModeList

		CodeDescriptionPairList DropModeList
		{
			get { return Factory.GetCachedValue("DtbConsignmentFilterBusinessObject|DropModeList", () => new CombinedEquipmentNeededList()); }
		}

		#endregion

		#region ServiceLevels

		RefServiceLevelCollection ServiceLevels
		{
			get { return new RefServiceLevelCollection(Factory); }
		}

		#endregion

		#region Zones

		RateTransportZonesCollection Zones
		{
			get { return new RateTransportZonesCollection(Factory); }
		}

		#endregion

		#region GetAdditionalReferenceNumberTypes

		CodeDescriptionPairList GetAdditionalReferenceNumberTypes()
		{
			var list = new CodeDescriptionPairList();
			list.AddRange(Registry.LandTransportRegistry.Instance.LandTransportConsignmentAdditionalReferenceNumbers.Value);
			return list;
		}

		#endregion

		#endregion
	}
}
