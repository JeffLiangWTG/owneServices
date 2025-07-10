using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.TransportCommon.Module;
using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;

namespace Enterprise.TransportConsignment.Module
{
	public class DtbRoutePlannerFilterBusinessObject : FilterStripBusinessObject, IDtbRoutePlannerFilterBusinessObject
	{
		public DtbRoutePlannerFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = ModuleIDs.DtbRoutePlanner.Name;
		}

		#region FilterConstants

		public static class FilterConstants
		{
			public const string BookingID = "BookingID";
			public const string ConsignmentID = "ConsignmentID";
			public const string TransportReference = "TransportReference";
			public const string ConsignorDropMode = "ConsignorDropMode";
			public const string ConsignorReference = "ConsignorReference";
			public const string ConsigneeDropMode = "ConsigneeDropMode";
			public const string ConsigneeReference = "ConsigneeReference";
			public const string ServiceLevel = "ServiceLevel";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter name")]
			public const string Hazardous = "Hazardous";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter name")]
			public const string Refrigerated = "Refrigerated";
			public const string DeliveryRequestedFrom = "DeliveryRequestedFrom";
			public const string DeliveryRequestedTo = "DeliveryRequestedTo";
			public const string PlannedDelivery = "PlannedDelivery";
			public const string PickUpRequestedFrom = "PickUpRequestedFrom";
			public const string PickUpRequestedTo = "PickUpRequestedTo";
			public const string PlannedPickUp = "PlannedPickUp";
			public const string DestinationCity = "DestinationCity";
			public const string DestinationPostcode = "DestinationPostcode";
			public const string DestinationState = "DestinationState";
			public const string DestinationZone = "DestinationZone";
			public const string OriginCity = "OriginCity";
			public const string OriginPostcode = "OriginPostcode";
			public const string OriginState = "OriginState";
			public const string OriginZone = "OriginZone";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
			public const string DepotAddress = "Depot Address";
			public const string BillToParty = "BillToParty";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "filter name")]
			public const string Branch = "Branch";
			public const string PickUpAndDelivery = "PickUpAndDelivery";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
			public const string Zone = "Zone";
			public const string SpecialInstructionsExist = "SpecialInstructionsExist"; // Filter name
			public const string ServicesExist = "ServicesExist"; // Filter name
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter name")]
			public const string CompanyName = "Company Name";
		}

		enum FilterType
		{
			PickUp,
			Delivery,
		}

		#endregion

		#region ModuleFilters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddDatesFilters(result);
			AddLocationFilters(result);
			AddModesAndTypesFilters(result);
			AddNumbersAndReferencesFilters(result);
			AddOrganisationFilters(result);
			AddStatusAndFlagsFilters(result);

			return result;
		}

		HashSet<ModuleFilter> ParentModuleFilters;

		#region Dates Filters

		void AddDatesFilters(ModuleFilterCollection filters)
		{
			var deliveryRequestedFromFilter = filters.AddDateFilter(FilterConstants.DeliveryRequestedFrom, GetDeliveryRequestedFromQuery);
			deliveryRequestedFromFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|DeliveryRequestedFrom", "Delivery Requested From");

			var deliveryRequestedToFilter = filters.AddDateFilter(FilterConstants.DeliveryRequestedTo, GetDeliveryRequestedToQuery);
			deliveryRequestedToFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|DeliveryRequestedTo", "Delivery Requested To");

			var estimatedDeliveryFilter = filters.AddDateFilter(FilterConstants.PlannedDelivery, GetEstimatedDeliveryQuery);
			estimatedDeliveryFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|DeliveryEstimated", "Delivery Estimated");

			var pickupRequestedFromFilter = filters.AddDateFilter(FilterConstants.PickUpRequestedFrom, GetPickupRequestedFromQuery);
			pickupRequestedFromFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|PickupRequestedFrom", "Pickup Requested From");

			var pickupRequestedToFilter = filters.AddDateFilter(FilterConstants.PickUpRequestedTo, GetPickupRequestedToQuery);
			pickupRequestedToFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|PickupRequestedTo", "Pickup Requested To");

			var estimatedPickupFilter = filters.AddDateFilter(FilterConstants.PlannedPickUp, GetPickupEstimatedQuery);
			estimatedPickupFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|PickupEstimated", "Pickup Estimated");
		}

		ZQuery GetDeliveryRequestedFromQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(FilterType.Delivery, FilterConstants.DeliveryRequestedFrom, DtbBookingConfirmationSchema.KK_RequiredFrom, comparisonOperator, date1, date2);
		}

		ZQuery GetDeliveryRequestedToQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(FilterType.Delivery, FilterConstants.DeliveryRequestedTo, DtbBookingConfirmationSchema.KK_RequiredTo, comparisonOperator, date1, date2);
		}

		ZQuery GetEstimatedDeliveryQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(FilterType.Delivery, FilterConstants.PlannedDelivery, DtbBookingConfirmationSchema.KK_Estimated, comparisonOperator, date1, date2);
		}

#if DEBUG
		public
#endif
 ZQuery GetPickupRequestedFromQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(FilterType.PickUp, FilterConstants.PickUpRequestedFrom, DtbBookingConfirmationSchema.KK_RequiredFrom, comparisonOperator, date1, date2);
		}

		ZQuery GetPickupRequestedToQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(FilterType.PickUp, FilterConstants.PickUpRequestedTo, DtbBookingConfirmationSchema.KK_RequiredTo, comparisonOperator, date1, date2);
		}

		ZQuery GetPickupEstimatedQuery(DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			return GetConfirmationDateQuery(FilterType.PickUp, FilterConstants.PlannedPickUp, DtbBookingConfirmationSchema.KK_Estimated, comparisonOperator, date1, date2);
		}

		ZQuery GetConfirmationDateQuery(FilterType filterType, string columnNameFromPickUpDeliveryJoin, SchemaDateTimeColumn dateTimeColumn, DateComparisonOperator comparisonOperator, ZDateTime date1, ZDateTime date2)
		{
			var queryToBuildDateFilter = new ZQuery();
			AddDateTimeRange(queryToBuildDateFilter, comparisonOperator, JoinCondition.And, dateTimeColumn, date1, date2);
			var dateQuery = new ZQuery();
			var confirmationType = filterType == FilterType.PickUp ? ConfirmationTypes.Codes.PickUp : ConfirmationTypes.Codes.Delivery;
			// If we are looking for a Pickup Time and we are a Pickup Confirmation it needs to look at itself,
			// otherwise it should look at all Pickups. Vice versa for deliveries.
			var sql = Invariant($@"(({DtbBookingConfirmationSchema.KK_ConfirmationType.Name} = '{confirmationType}' AND 
{queryToBuildDateFilter.LiteralTextSqlFormatted}) OR ({DtbBookingConfirmationSchema.KK_ConfirmationType.Name} <> '{confirmationType}' AND 
{queryToBuildDateFilter.LiteralTextSqlFormatted.Replace(dateTimeColumn.Name, columnNameFromPickUpDeliveryJoin)}))"); // SQL Query for filter
			dateQuery.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());
			return dateQuery;
		}

		#endregion

		#region Location Filters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var cityMaxLength = Math.Min(OrgAddressSchema.OA_City.MaxLength, JobDocAddressSchema.E2_City.MaxLength);
			var postcodeMaxLength = Math.Min(OrgAddressSchema.OA_PostCode.MaxLength, JobDocAddressSchema.E2_Postcode.MaxLength);
			var stateMaxLength = Math.Min(OrgAddressSchema.OA_State.MaxLength, JobDocAddressSchema.E2_State.MaxLength);

			var destinationCityFilter = filters.AddTextFilter(FilterConstants.DestinationCity, GetDestinationCityQuery);
			destinationCityFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|DestinationCity", "Destination City");
			destinationCityFilter.Category = FilterCategories.Locations;
			destinationCityFilter.MaxLength = cityMaxLength;

			var destinationPostcodeFilter = filters.AddTextFilter(FilterConstants.DestinationPostcode, GetDestinationPostcodeQuery);
			destinationPostcodeFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|DestinationPostcode", "Destination Postcode");
			destinationPostcodeFilter.Category = FilterCategories.Locations;
			destinationPostcodeFilter.MaxLength = postcodeMaxLength;

			var destinationStateFilter = filters.AddTextFilter(FilterConstants.DestinationState, GetDestinationStateQuery);
			destinationStateFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|DestinationState", "Destination State");
			destinationStateFilter.Category = FilterCategories.Locations;
			destinationStateFilter.MaxLength = stateMaxLength;

			var originCityFilter = filters.AddTextFilter(FilterConstants.OriginCity, GetOriginCityQuery);
			originCityFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|OriginCity", "Origin City");
			originCityFilter.Category = FilterCategories.Locations;
			originCityFilter.MaxLength = cityMaxLength;

			var originPostcodeFilter = filters.AddTextFilter(FilterConstants.OriginPostcode, GetOriginPostcodeQuery);
			originPostcodeFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|OriginPostcode", "Origin Postcode");
			originPostcodeFilter.Category = FilterCategories.Locations;
			originPostcodeFilter.MaxLength = postcodeMaxLength;

			var originStateFilter = filters.AddTextFilter(FilterConstants.OriginState, GetOriginStateQuery);
			originStateFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|OriginState", "Origin State");
			originStateFilter.Category = FilterCategories.Locations;
			originStateFilter.MaxLength = stateMaxLength;

			var zoneFilter = filters.AddGuidFilter(FilterConstants.Zone, ModuleIDs.RateTransportZone, GetZoneQuery, Zones);
			zoneFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|Zone", "Zone");
			zoneFilter.Category = FilterCategories.Locations;
		}

		ZQuery GetDestinationCityQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(OrgAddressSchema.OA_City, FilterType.Delivery, JobDocAddressSchema.E2_City, comparisonOperator, value);
		}

		ZQuery GetDestinationPostcodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(OrgAddressSchema.OA_PostCode, FilterType.Delivery, JobDocAddressSchema.E2_Postcode, comparisonOperator, value);
		}

		ZQuery GetDestinationStateQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(OrgAddressSchema.OA_State, FilterType.Delivery, JobDocAddressSchema.E2_State, comparisonOperator, value);
		}

		ZQuery GetOriginCityQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(OrgAddressSchema.OA_City, FilterType.PickUp, JobDocAddressSchema.E2_City, comparisonOperator, value);
		}

		ZQuery GetOriginPostcodeQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(OrgAddressSchema.OA_PostCode, FilterType.PickUp, JobDocAddressSchema.E2_Postcode, comparisonOperator, value);
		}

		ZQuery GetOriginStateQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetAddressQuery(OrgAddressSchema.OA_State, FilterType.PickUp, JobDocAddressSchema.E2_State, comparisonOperator, value);
		}

		ZQuery GetAddressQuery(SchemaStringColumn orgAddressColumn, FilterType filterType, SchemaStringColumn jobDocAddressColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();

			var orgAddressTable = filterType == FilterType.PickUp ? "PickUpOrgAddress" : "DeliveryOrgAddress";
			var docAddressTable = filterType == FilterType.PickUp ? "PickUpJobDocAddress" : "DeliveryJobDocAddress";

			var notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);
			var joinCondition = (notIn || comparisonOperator == SpecialComparisonOperator.IsBlank) ? JoinCondition.And : JoinCondition.Or;
			if (notIn)
			{
				var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK, notIn);
				orgAddressQuery.AddToFilter(orgAddressColumn, comparisonOperator, value);
				var orgAddressPKQuery = new ZDBOnlyQuery(typeof(OrgAddress));
				orgAddressPKQuery.AddSubQuery(orgAddressQuery, JoinCondition.And);
				query.AddFilterAndZSQLParameterCollection(Invariant($"{orgAddressTable}.{orgAddressPKQuery.LiteralTextSqlFormatted}"), new ZSqlParameterCollection());

				var jobDocAddressQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.PK, notIn);
				jobDocAddressQuery.AddToFilter(jobDocAddressColumn, comparisonOperator, value);
				var jobDocAddressPKQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
				jobDocAddressPKQuery.AddSubQuery(jobDocAddressQuery, JoinCondition.And);
				query.AddFilterAndZSQLParameterCollection(Invariant($"{docAddressTable}.{jobDocAddressPKQuery.LiteralTextSqlFormatted}"), new ZSqlParameterCollection(), joinCondition);
			}
			else
			{
				var orgAddressTableAndColumn = Invariant($"{orgAddressTable}.{orgAddressColumn.Name}"); // a part of SQL expression
				var jobDocAddressTableAndColumn = Invariant($"{docAddressTable}.{jobDocAddressColumn.Name}"); // a part of SQL expression

				if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
				{
					orgAddressTableAndColumn = Invariant($"ISNULL({orgAddressTableAndColumn}, '')"); // a part of SQL expression
					jobDocAddressTableAndColumn = Invariant($"ISNULL({jobDocAddressTableAndColumn}, '')"); // a part of SQL expression
				}

				query.AddFilterAndZSQLParameterCollection(Invariant($"{orgAddressTableAndColumn} {comparisonOperator.ComparisonText(value)} '{comparisonOperator.ValueForLiteralADO(value)}'"), new ZSqlParameterCollection());

				// Choose doc address table for column select.
				query.AddFilterAndZSQLParameterCollection(Invariant($"{jobDocAddressTableAndColumn} {comparisonOperator.ComparisonText(value)} '{comparisonOperator.ValueForLiteralADO(value)}'"), new ZSqlParameterCollection(), joinCondition);
			}

			return query;
		}

		ZQuery GetZoneQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			bool notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparisonOperator);
			var zoneQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentInstruction), DtbBookingInstructionSchema.PK, notIn);
			zoneQuery.AddToFilter(DtbBookingInstructionSchema.KN_TZ_DomesticZone, comparisonOperator, value);

			var instructionQuery = new ZDBOnlyQuery(typeof(DtbConsignmentInstruction));
			instructionQuery.AddSubQuery(zoneQuery, JoinCondition.And);

			var query = new ZQuery();
			query.AddToFilter(instructionQuery);

			return query;
		}

		#endregion

		#region Modes And Types Filters

		void AddModesAndTypesFilters(ModuleFilterCollection filters)
		{
			var serviceLevelFilter = filters.AddNkFilter(FilterConstants.ServiceLevel, DtbBookingSchema.KM_RS_NKServiceLevel, ModuleIDs.ServiceLevel, ServiceLevels);
			serviceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|ServiceLevel", "Service Level");
			serviceLevelFilter.Category = FilterCategories.ModesAndTypes;

			var consingorDropModeFilter = filters.AddTextFilter(FilterConstants.ConsignorDropMode, GetConsignorDropModeQuery, DropModeList);
			consingorDropModeFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|ConsignorDropMode", "Consignor Drop Mode");
			consingorDropModeFilter.Category = FilterCategories.ModesAndTypes;

			var consingeeDropModeFilter = filters.AddTextFilter(FilterConstants.ConsigneeDropMode, GetConsigneeDropModeQuery, DropModeList);
			consingeeDropModeFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|ConsigneeDropMode", "Consignee Drop Mode");
			consingeeDropModeFilter.Category = FilterCategories.ModesAndTypes;
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
			var query = new ZQuery();

			if (dropMode != "ANY")
			{
				var zoneQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentInstruction), DtbBookingInstructionSchema.PK);
				zoneQuery.AddToFilter(DtbBookingInstructionSchema.KN_InstructionType, instructionType);
				zoneQuery.AddToFilter(DtbBookingInstructionSchema.KN_DropMode, dropMode);

				var instructionQuery = new ZDBOnlyQuery(typeof(DtbConsignmentInstruction));
				instructionQuery.AddSubQuery(zoneQuery, JoinCondition.And);

				query.AddToFilter(instructionQuery);
			}
			return query;
		}

		#endregion

		#region Numbers And References Filters

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			var bookingIDfilter = filters.AddFountainFilter(FilterConstants.BookingID, GetBookingIDQuery, "TB");
			bookingIDfilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|BookingID", "Booking ID");
			bookingIDfilter.Category = FilterCategories.NumbersAndReferences;
			bookingIDfilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			bookingIDfilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			bookingIDfilter.MaxLength = DtbBookingSchema.KM_JobID.MaxLength;
			bookingIDfilter.UseMultiSearch = false; //TODO: Needs to work with multiple values. (GetBookingIDQuery)

			var consignmentFilter = filters.AddTextFilter(FilterConstants.ConsignmentID, DtbBookingSchema.KM_JobID);
			consignmentFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|ConsignmentID", "Consignment ID");
			consignmentFilter.Category = FilterCategories.NumbersAndReferences;
			consignmentFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			consignmentFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			var transportReferenceFilter = filters.AddTextFilter(FilterConstants.TransportReference, DtbBookingSchema.KM_TransportReference);
			transportReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|TransportReference", "Connote No.");
			transportReferenceFilter.Category = FilterCategories.NumbersAndReferences;

			var consigneeReferenceFilter = filters.AddTextFilter(FilterConstants.ConsigneeReference, GetConsigneeReferenceQuery);
			consigneeReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|ConsigneeReference", "Consignee Reference");
			consigneeReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			consigneeReferenceFilter.MaxLength = DtbBookingConfirmationSchema.KK_ReferenceNum.MaxLength;

			var consignorReferenceFilter = filters.AddTextFilter(FilterConstants.ConsignorReference, GetConsignorReferenceQuery);
			consignorReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|ConsignorReference", "Consignor Reference");
			consignorReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			consignorReferenceFilter.MaxLength = DtbBookingConfirmationSchema.KK_ReferenceNum.MaxLength;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI003", Justification = "Don't have access to the alternatives it wants")]
		ZQuery GetBookingIDQuery(SQLComparisonOperator @operator, ZString value)
		{
			//TODO: Needs to work with multiple values.
			var query = new ZQuery();
			query.AddFilterAndZSQLParameterCollection(Invariant($"ParentBookings.BookingID {@operator.ComparisonText(value)} '{@operator.ValueForLiteralADO(value)}'"), new ZSqlParameterCollection());
			return query;
		}

		ZQuery GetConsigneeReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetQueryForDocAddressType(FilterType.Delivery, comparisonOperator, DocAddressTypes.Codes.LocalCartageImporter, value);
		}

		ZQuery GetConsignorReferenceQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GetQueryForDocAddressType(FilterType.PickUp, comparisonOperator, DocAddressTypes.Codes.LocalCartageExporter, value);
		}

		static ZQuery GetQueryForDocAddressType(FilterType filterType, SQLComparisonOperator comparisonOperator, ZString addressType, ZString value)
		{
			var query = new ZQuery();
			// Check Reference field
			query.AddFilterAndZSQLParameterCollection(Invariant($"{(filterType == FilterType.PickUp ? "PickUps.ConsignorReference" : "Deliveries.ConsigneeReference")} {comparisonOperator.ComparisonText(value)} '{comparisonOperator.ValueForLiteralADO(value)}'"), new ZSqlParameterCollection());

			// Only look for Consignor/Consignee Address
			query.AddFilterAndZSQLParameterCollection(Invariant($"{(filterType == FilterType.PickUp ? "PickUpJobDocAddress" : "DeliveryJobDocAddress")}.{JobDocAddressSchema.E2_AddressType.Name} = '{addressType}'"), new ZSqlParameterCollection());
			return query;
		}

		#endregion

		#region Organisation Filters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var billToPartyFilter = filters.AddGuidFilter(FilterConstants.BillToParty, ModuleIDs.Organisation, GetLocalClientQuery, new OrgHeaderCollection(Factory));
			billToPartyFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|BillToParty", "Bill To Party");
			billToPartyFilter.Category = FilterCategories.Organisations;

			var branchFilter = filters.AddGuidFilter(FilterConstants.Branch, ModuleIDs.GlbBranch, JobHeaderSchema.JH_GB, new GlbBranchCollection(Factory));
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|Branch", "Branch");
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			branchFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);

			var pickUpDeliveryFilter = filters.AddGuidFilter(FilterConstants.PickUpAndDelivery, ModuleIDs.Organisation, GetPickupAndDeliveryQuery, new OrgHeaderCollection(Factory), new OrgHeaderCollection(Factory));
			pickUpDeliveryFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|PickUpAndDelivery", "Pickup/Delivery");
			pickUpDeliveryFilter.SetItemDescriptions(
				Res.GetData("DtbRoutePlannerFilterBusinessObject|PickUpAndDelivery|ItemDescription1", "Pickup"),
				Res.GetData("DtbRoutePlannerFilterBusinessObject|PickUpAndDelivery|ItemDescription2", "Delivery"));
			pickUpDeliveryFilter.Category = FilterCategories.Organisations;

			var depotAddressFilter = filters.AddGuidFilter(FilterConstants.DepotAddress, ModuleIDs.OrgAddresses, GetDepotQuery, new OrgAddressCollection(Factory));
			depotAddressFilter.Category = FilterCategories.Locations;
			depotAddressFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|DepotAddress", "Depot Address");

			var companyNameFilter = filters.AddTextFilter(FilterConstants.CompanyName, GetCompanyNameQuery);
			companyNameFilter.Category = FilterCategories.Organisations;
			companyNameFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|CompanyName", "Company Name");
			companyNameFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsBlank);
			companyNameFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.IsNotBlank);
			companyNameFilter.MaxLength = Math.Min(JobDocAddressSchema.E2_CompanyName.MaxLength, OrgHeaderSchema.OH_FullName.MaxLength);
		}

		ZQuery GetCompanyNameQuery(SQLComparisonOperator comparison, ZString text)
		{
			if (!text.IsEmpty)
			{
				var notIn = SQLComparisonOperator.ReverseOperatorIfItIsNotInOperator(ref comparison);

				var addressType = ViewMode == DtbRoutePlannerViewMode.Deliveries ? DocAddressTypes.Codes.LocalCartageImporter : DocAddressTypes.Codes.LocalCartageExporter;
				var docAddressOverriddenFilter = new ZQuery(JobDocAddressSchema.E2_AddressType, addressType);
				docAddressOverriddenFilter.AddToFilter(JobDocAddressSchema.E2_AddressOverride, ZBool.True);
				docAddressOverriddenFilter.AddToFilter(JobDocAddressSchema.E2_CompanyName, comparison, text);

				var orgHeaderFilter = new ZQuery(OrgHeaderSchema.OH_FullName, comparison, text);

				var docAddressFilter = OrgRelatedPartiesFilterHelper.GetDocAddressFromOrgHeaderFilter(orgHeaderFilter, new ZQuery(), false, false);
				docAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
				docAddressFilter.AddToFilter(JobDocAddressSchema.E2_AddressOverride, ZBool.False);

				var result = new ZQuery();
				result.AddToFilter(FromDocAddressFilter(docAddressOverriddenFilter, notIn));
				result.AddToFilter(FromDocAddressFilter(docAddressFilter, notIn), notIn ? JoinCondition.And : JoinCondition.Or);

				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		ZQuery FromDocAddressFilter(ZQuery docAddressFilter, bool notIn)
		{
			var docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID, notIn);
			docAddressSubQuery.AddToFilter(docAddressFilter);

			var instructionSubquery = new ZDBOnlySubQuery(typeof(DtbConsignmentInstruction), DtbBookingConfirmationSchema.KK_KN_BookingInstruction);
			instructionSubquery.AddSubQuery(docAddressSubQuery, JoinCondition.And);

			var confirmationQuery = new ZDBOnlyQuery(typeof(DtbConsignmentConfirmation));
			confirmationQuery.AddSubQuery(instructionSubquery, JoinCondition.And);

			return confirmationQuery;
		}

		#region GetLocalClientQuery

		ZQuery GetLocalClientQuery(ZGuid localClientGuid)
		{
			var query = new ZQuery();
			if (localClientGuid.IsValid)
			{
				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@LocalClient", localClientGuid, OrgAddressSchema.OA_OH);

				query.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
				query.AddFilterAndZSQLParameterCollection(Invariant($"LocalClients.{OrgAddressSchema.OA_OH.Name} = @LocalClient"), sqlParams);
			}

			return query;
		}

		#endregion

		#region GetPickupAndDeliveryQuery

		ZQuery GetPickupAndDeliveryQuery(ZGuid pickUpOrg, ZGuid deliveryOrg)
		{
			var query = new ZQuery();

			if (pickUpOrg.IsValid)
			{
				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@Consignor", pickUpOrg, OrgAddressSchema.OA_OH);
				query.AddFilterAndZSQLParameterCollection(Invariant($"PickUpOrgAddress.{OrgAddressSchema.OA_OH.Name} = @Consignor"), sqlParams);
			}

			if (deliveryOrg.IsValid)
			{
				var sqlParams = new ZSqlParameterCollection();
				sqlParams.Add("@Consignee", deliveryOrg, OrgAddressSchema.OA_OH);
				query.AddFilterAndZSQLParameterCollection(Invariant($"DeliveryOrgAddress.{OrgAddressSchema.OA_OH.Name} = @Consignee"), sqlParams);
			}

			return query;
		}

		#endregion

		#region GetDepotQuery

		ZQuery GetDepotQuery(ZGuid depotAddressPK)
		{
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), PortHubSelectionSchema.TY_OA_DepotAddress);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.PK, depotAddressPK);

			var portHubQuery = new ZDBOnlyQuery(ObjectFactory.GetType<IPortHubSelection>());
			portHubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			return portHubQuery;
		}

		#endregion

		#endregion

		#region Status And Flags Filters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var specialInstructionsExistFilter = filters.AddTextFilter(FilterConstants.SpecialInstructionsExist, SpecialInstuctionsExistQuery, SpecialInstructionsExistList);
			specialInstructionsExistFilter.MultilingualDescription = (ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|SpecialInstructionsExist", "Special Instructions Exist"));
			specialInstructionsExistFilter.Category = FilterCategories.StatusAndFlags;

			var isHazardousFilter = filters.AddTextFilter(FilterConstants.Hazardous, HazardousGoodsQuery, IsHazardousList);
			isHazardousFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|IsHazardous", "Hazardous");
			isHazardousFilter.Category = FilterCategories.StatusAndFlags;

			var isRefrigeratedFilter = filters.AddTextFilter(FilterConstants.Refrigerated, RefrigeratedGoodsQuery, IsRefrigeratedList);
			isRefrigeratedFilter.MultilingualDescription = ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|IsRefrigerated", "Refrigeration");
			isRefrigeratedFilter.Category = FilterCategories.StatusAndFlags;

			var servicesExistFilter = filters.AddTextFilter(FilterConstants.ServicesExist, ServicesExistQuery, ServicesExistList);
			servicesExistFilter.MultilingualDescription = (ResString.GetMultilingualString("DtbRoutePlannerFilterBusinessObject|ServicesExist", "Services Exist"));
			servicesExistFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery SpecialInstuctionsExistQuery(ZString showConsignmentsWithInstructions)
		{
			var query = new ZQuery();

			if (showConsignmentsWithInstructions.EqualsIgnoringCase(SpecialInstructionsExistStatuses.Codes.Yes))
			{
				query.AddToFilter(DtbBookingInstructionSchema.KN_ServiceInstruction, SQLComparisonOperator.NotEqual, "");
			}
			else if (showConsignmentsWithInstructions.EqualsIgnoringCase(SpecialInstructionsExistStatuses.Codes.No))
			{
				query.AddToFilter(DtbBookingInstructionSchema.KN_ServiceInstruction, "");
			}

			return query;
		}

		ZQuery HazardousGoodsQuery(ZString hazardousGoodsStatus)
		{
			return GetQueryForHazardousAndRefrigeratedConsignments(hazardousGoodsStatus, DtbBookingSchema.KM_IsHazardous,
				IsHazardousStatuses.Codes.Hazardous, IsHazardousStatuses.Codes.Both);
		}

		ZQuery RefrigeratedGoodsQuery(ZString refrigeratedGoodsStatus)
		{
			return GetQueryForHazardousAndRefrigeratedConsignments(refrigeratedGoodsStatus, DtbBookingSchema.KM_RequiresRefrigeration,
				IsRefrigeratedStatuses.Codes.Refrigerated, IsRefrigeratedStatuses.Codes.Both);
		}

		ZQuery GetQueryForHazardousAndRefrigeratedConsignments(ZString selectedStatus, SchemaBoolColumn column, string codeForTrue, string codeForBoth)
		{
			var query = new ZQuery();
			if (!selectedStatus.Trim().EqualsIgnoringCase(codeForBoth))
			{
				query.AddToFilter(column, selectedStatus == codeForTrue);
			}

			return query;
		}

		ZQuery ServicesExistQuery(ZString showConsignmentsWithServices)
		{
			var result = new ZQuery();
			if (showConsignmentsWithServices.EqualsIgnoringCase(ServicesExistStatuses.Codes.Yes))
			{
				result.AddFilterAndZSQLParameterCollection("ConsignmentServices.ES_PK is not null or InstructionServices.ES_PK is not null", new ZSqlParameterCollection());
				if (ViewMode == DtbRoutePlannerViewMode.Direct)
				{
					result.AddFilterAndZSQLParameterCollection("or Related_ES_PK is not null", new ZSqlParameterCollection());
				}
			}
			else if (showConsignmentsWithServices.EqualsIgnoringCase(ServicesExistStatuses.Codes.No))
			{
				result.AddFilterAndZSQLParameterCollection("ConsignmentServices.ES_PK is null and InstructionServices.ES_PK is null", new ZSqlParameterCollection());
				if (ViewMode == DtbRoutePlannerViewMode.Direct)
				{
					result.AddFilterAndZSQLParameterCollection("and Related_ES_PK is not null", new ZSqlParameterCollection());
				}
			}
			return result;
		}

		#endregion

		#region Workflow Filters

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var helper = new TransportWorkflowFilterStripsHelper(typeof(DtbBookingConsignment), WorkflowDescriptors.DtbBookingWorkflowDescriptorCode, Factory);
			helper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(helper);

			return helpers;
		}

		protected override void OnModuleFiltersCreated()
		{
			base.OnModuleFiltersCreated();

			ParentModuleFilters = new HashSet<ModuleFilter>(ModuleFilters);

			// If ChildFilters change this will need to be refreshed, this may happen in future if the child filter controls are used on their own/elsewhere
			ChildFilterBusinessObjects
				.ForEach(cf => cf.ModuleFilters.ToList()
				.ForEach(ModuleFilters.AddFilter));
		}

		#endregion

		#region Lists

		#region DropModeList

		CodeDescriptionPairList DropModeList
		{
			get { return Factory.GetCachedValue("DtbRoutePlannerFilterBusinessObject|DropModeList", () => new CombinedEquipmentNeededList()); }
		}

		#endregion

		#region SpecialInstructionsExistList

		CodeDescriptionPairList SpecialInstructionsExistList
		{
			get { return Factory.GetCachedValue("DtbRoutePlannerFilterBusinessObject|SpecialInstructionsExistList", () => new SpecialInstructionsExistStatuses()); }
		}

		#endregion

		#region ServicesExistList

		CodeDescriptionPairList ServicesExistList
		{
			get { return Factory.GetCachedValue("DtbRoutePlannerFilterBusinessObject|ServicesExistList", () => new ServicesExistStatuses()); }
		}

		#endregion

		#region IsHazardousList

		CodeDescriptionPairList IsHazardousList
		{
			get { return Factory.GetCachedValue("DtbRoutePlannerFilterBusinessObject|IsHazardousList", () => new IsHazardousStatuses()); }
		}

		#endregion

		#region IsRefrigeratedList

		CodeDescriptionPairList IsRefrigeratedList
		{
			get { return Factory.GetCachedValue("DtbRoutePlannerFilterBusinessObject|IsRefrigeratedList", () => new IsRefrigeratedStatuses()); }
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

		#endregion

		#endregion

		#region Filter

		#region Main Query

		public override ZQuery Filter
		{
			get
			{
				// join clause for user-defined filters
				var addressJoin = GetJoinClauseForAddressFilters();
				var consignmentParentBookingJoin = GetJoinClauseForParentBooking();
				var jobHeaderJoin = GetJoinClauseForJobHeader();
				var pickUpsAndDeliveriesJoin = GetJoinClauseForPickUpAndDeliveryInstructions();
				var zoneHubSelectionJoin = GetJoinClauseForZoneHubSelection();
				var servicesExistJoin = GetJoinClauseForServicesExist();

				// where clause for user-defined filters
				var whereClauseForViewMode = GetWhereClauseForViewMode();
				var orCategoriesWithFilters = GetActiveFiltersGroupedByOrCategory();
				var whereClauseForUserDefinedFilters = GetWhereClauseForUserDefinedFilters(orCategoriesWithFilters);

				// 'always-on' filter
				var sql = string.Format(Culture.Invariant, @"
					KK_PK IN
					(
						SELECT
							KK_PK

						FROM
							dbo.DtbBookingConfirmation
							JOIN
							(
								SELECT
									KN_PK, KN_KM_BookingMovement, KN_InstructionType, KN_Status, KN_TZ_DomesticZone, RelatedPickUpStatus, KN_ServiceInstruction, ES_PK as Related_ES_PK
								FROM
									dbo.DtbBookingInstruction
									LEFT JOIN
									(
										SELECT
											KN_KM_BookingMovement as BookingPK, KN_Sequence as Sequence, KN_Status as RelatedPickUpStatus, KN_PK as Related_KN_PK, ES_PK 
										FROM
											dbo.DtbBookingInstruction
											LEFT JOIN dbo.JobService as RelatedInstructionServices ON RelatedInstructionServices.ES_CurrentContextID = KN_PK 								
										WHERE
											KN_InstructionType in ('PIC', 'DLV')
									) as RelatedPickUpInstructions
										ON RelatedPickUpInstructions.BookingPK = KN_KM_BookingMovement
										AND Related_KN_PK != KN_PK
							) as Instructions ON KK_KN_BookingInstruction = KN_PK
							JOIN dbo.DtbBooking ON KN_KM_BookingMovement = KM_PK
							JOIN dbo.DtbBookingConsolidation ON KM_KB_Booking = KB_PK
							{7}" + // Services Exist join
							@"
							{6}" + // Zone Hub Selection join
							@"
							{5}" + // Pickups and Deliveries join
							@"
							{4}" + // JobHeader join
							@"
							{3}" + // Parent Booking join
							@"
							{2}" + // JobDocAddress & OrgAddress join
						@"
						WHERE
							KB_JobType = 'CSN'
							AND (KK_ConfirmationType = 'PIC' OR KK_ConfirmationType = 'DLV')
							AND KM_IsActive = 1
							AND{0}" + // View Mode where clause
							@"
							AND KK_K1_RunSheetInstruction IS NULL -- if not null, already allocated
							{1}
					)", // User-Defined filters where clause
					whereClauseForViewMode, whereClauseForUserDefinedFilters, addressJoin, consignmentParentBookingJoin, jobHeaderJoin,
					pickUpsAndDeliveriesJoin, zoneHubSelectionJoin, servicesExistJoin);

				var query = new ZDBOnlyQuery(typeof(DtbConsignmentConfirmation));
				query.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());

				return query;
			}
		}

		#region ChildFiltersForRunsheets

		public ZQuery ChildFiltersForRunsheets
		{
			get
			{
				var sql = string.Join(" AND ", ChildFilterBusinessObjects
					.Where(f => !f.Filter.IsEmpty)
					.Select(f => Invariant($"{f.FieldOnRunsheet.Name} IN(SELECT {f.ChildBizOPKOrNK.Name} FROM {f.ChildBizOPKOrNK.TableName} WHERE {f.Filter.LiteralTextSqlFormatted})"))); // a part of SQL expression

				var query = new ZDBOnlyQuery(typeof(DtbConsignmentRunSheet));
				return query.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection());
			}
		}

		#endregion

		Dictionary<FilterOrCategory, List<ModuleFilter>> GetActiveFiltersGroupedByOrCategory()
		{
			var result = new Dictionary<FilterOrCategory, List<ModuleFilter>>();

			foreach (var moduleFilter in ActiveModuleFiltersForQueryNonEmpty)
			{
				List<ModuleFilter> filters;
				if (result.TryGetValue(moduleFilter.OrCategory, out filters))
				{
					filters.Add(moduleFilter);
				}
				else
				{
					result.Add(moduleFilter.OrCategory, new List<ModuleFilter> { moduleFilter });
				}
			}

			return result;
		}

		IEnumerable<ModuleFilter> ActiveModuleFiltersForQueryNonEmpty
		{
			get { return ActiveModuleFiltersForQuery.Where(mf => !mf.Query.IsEmpty && ParentModuleFilters.Any(pf => pf.Description == GetDescriptionWithoutParentheses(mf.Description))); }
		}

		public IEnumerable<ModuleFilter> ActiveModuleFiltersForChildFilter
		{
			get { return ActiveModuleFiltersForQuery.Where(mf => !mf.Query.IsEmpty && ChildFilterBusinessObjects.Any(pf => pf.ModuleFilters.Any(ch => ch.Description == GetDescriptionWithoutParentheses(mf.Description)))); }
		}

		ZString GetDescriptionWithoutParentheses(string description)
		{
			var indexOfDuplicateDescription = description.IndexOf("(", StringComparison.CurrentCulture) - 1;
			return indexOfDuplicateDescription > 0 ? description.Substring(0, indexOfDuplicateDescription) : description;
		}

		#endregion

		#region View Mode - Where Clause

		ZString GetWhereClauseForViewMode()
		{
			switch (ViewMode)
			{
				case DtbRoutePlannerViewMode.Pickups:
					return PickupsWhereClause;
				case DtbRoutePlannerViewMode.Deliveries:
					return DeliveriesWhereClause;
				case DtbRoutePlannerViewMode.NextAvailable:
					return NextAvailableWhereClause;
				case DtbRoutePlannerViewMode.Direct:
					return DirectWhereClause;
				default:
					throw new NotSupportedException(ZString.Format("Route Planner View Mode {0} is not supported by the FilterBizO.", ViewMode));
			}
		}

		ZString PickupsWhereClause
		{
			get { return "(KN_Status = 'AVL' AND KN_InstructionType = 'PIC')"; }
		}

		ZString DeliveriesWhereClause
		{
			get
			{
				return @"((Instructions.RelatedPickUpStatus = 'ALC' OR Instructions.RelatedPickUpStatus = 'PIC')
						 AND
						 (KN_Status = 'AVL' AND KN_InstructionType = 'DLV'))";
			}
		}

		ZString NextAvailableWhereClause
		{
			get
			{
				return @"
							(" + PickupsWhereClause + @"
							OR" + DeliveriesWhereClause + @"
							)"; // Part of SQL Expression
			}
		}

		ZString DirectWhereClause
		{
			get
			{
				return @"
						(( 
							KN_Status = 'AVL' AND
							KN_InstructionType = 'PIC'
							-- If no corresponding delivery, will not be consolidated
						)
						OR
						(
							KN_InstructionType = 'DLV' AND
							KN_Status = 'AVL' AND
							Instructions.RelatedPickUpStatus = 'AVL'
						))";
			}
		}

		#endregion

		#region User Defined Filters - Join Clauses

		#region GetJoinClauseForAddressFilters

		ZString GetJoinClauseForAddressFilters()
		{
			return ActiveModuleFiltersForQueryNonEmpty.Any(mf => IsAddressFilter(mf)) ?
				"JOIN dbo.JobDocAddress as PickUpJobDocAddress ON PickUpJobDocAddress.E2_ParentID = PickUpPK" + "\r\n" +
				"LEFT JOIN dbo.OrgAddress as PickUpOrgAddress ON PickUpJobDocAddress.E2_OA_Address = PickUpOrgAddress.OA_PK" +
				Invariant($" AND PickUpJobDocAddress.E2_OA_Address NOT IN ({SQLTextForSystemDefinedAddresses})") + "\r\n" + // Part of SQL expression
				"JOIN dbo.JobDocAddress as DeliveryJobDocAddress ON DeliveryJobDocAddress.E2_ParentID = DeliveryPK" + "\r\n" +
				"LEFT JOIN dbo.OrgAddress as DeliveryOrgAddress ON DeliveryJobDocAddress.E2_OA_Address = DeliveryOrgAddress.OA_PK" +
				Invariant($" AND DeliveryJobDocAddress.E2_OA_Address NOT IN ({SQLTextForSystemDefinedAddresses})") // Part of SQL expression
				: "";
		}

		bool IsAddressFilter(ModuleFilter moduleFilter)
		{
			return
				moduleFilter.Description == FilterConstants.DestinationCity ||
				moduleFilter.Description == FilterConstants.DestinationPostcode ||
				moduleFilter.Description == FilterConstants.DestinationState ||
				moduleFilter.Description == FilterConstants.OriginCity ||
				moduleFilter.Description == FilterConstants.OriginPostcode ||
				moduleFilter.Description == FilterConstants.OriginState ||
				moduleFilter.Description == FilterConstants.ConsigneeReference ||
				moduleFilter.Description == FilterConstants.ConsignorReference ||
				moduleFilter.Description == FilterConstants.PickUpAndDelivery;
		}

		string SQLTextForSystemDefinedAddresses
		{
			get { return string.Join(", ", SystemDefinedAddressPKs.Select(p => SQLTextForGuids(p, JobDocAddressSchema.E2_OA_Address))); }
		}

		string SQLTextForGuids(ZGuid guid, SchemaGuidColumn guidColumn)
		{
			return ZSqlParameter.New("@Guid", guid, guidColumn).ParameterValueTextSql;
		}

		ZGuid[] SystemDefinedAddressPKs
		{
			get
			{
				return new[]
				{
					OrganisationsDataRegistry.Instance.MiscOrganisation.Value.PrimaryOfficeAddress,
					OrgHeader.UnmatchOrg(Factory).MainAddress.PK
				};
			}
		}

		#endregion

		#region GetJoinClauseForParentBooking

		ZString GetJoinClauseForParentBooking()
		{
			return ActiveModuleFiltersForQueryNonEmpty.Any(mf => mf.Description == FilterConstants.BookingID) ?
								@"JOIN
									(
										SELECT KM_PK as BookingPK, KM_JobID as BookingID
										FROM dbo.DtbBooking
									) as ParentBookings ON KB_ParentID = BookingPK" : "";
		}

		#endregion

		#region GetJoinClauseForJobHeader

		ZString GetJoinClauseForJobHeader()
		{
			var isJobHeaderFilterUsed = ActiveModuleFiltersForQueryNonEmpty
				.Any(mf => mf.Description == FilterConstants.BillToParty || mf.Description == FilterConstants.Branch);

			return isJobHeaderFilterUsed
				? @"JOIN dbo.JobHeader ON KM_PK = JH_ParentID
					LEFT JOIN dbo.OrgAddress as LocalClients ON JH_OA_LocalChargesAddr = OA_PK"
				: "";
		}

		#endregion

		#region GetJoinClauseForPickUpAndDeliveryInstructions

		ZString GetJoinClauseForPickUpAndDeliveryInstructions()
		{
			return ActiveModuleFiltersForQueryNonEmpty
				.Any(mf => IsAddressFilter(mf) || IsDateFilter(mf))
				? @"JOIN
							(
								SELECT
									KN_KM_BookingMovement as ConsignmentPK,
									KN_PK as PickUpPK,
									KK_Estimated as PlannedPickUp,
									KK_RequiredFrom as PickUpRequestedFrom,
									KK_RequiredTo as PickUpRequestedTo,
									KK_ReferenceNum as ConsignorReference
								FROM
									dbo.DtbBookingInstruction
									JOIN dbo.DtbBookingConfirmation ON KK_KN_BookingInstruction = KN_PK
								WHERE KN_InstructionType = 'PIC'
							) as PickUps ON PickUps.ConsignmentPK = KM_PK
							JOIN
							(
								SELECT
									KN_KM_BookingMovement as ConsignmentPK,
									KN_PK as DeliveryPK,
									KK_Estimated as PlannedDelivery,
									KK_RequiredFrom as DeliveryRequestedFrom,
									KK_RequiredTo as DeliveryRequestedTo,
									KK_ReferenceNum as ConsigneeReference
								FROM
									dbo.DtbBookingInstruction
									JOIN dbo.DtbBookingConfirmation ON KK_KN_BookingInstruction = KN_PK
								WHERE KN_InstructionType = 'DLV'
							) as Deliveries ON Deliveries.ConsignmentPK = KM_PK" : "";
		}

		bool IsDateFilter(ModuleFilter moduleFilter)
		{
			return moduleFilter.Description == FilterConstants.DeliveryRequestedFrom ||
				moduleFilter.Description == FilterConstants.DeliveryRequestedTo ||
				moduleFilter.Description == FilterConstants.PlannedDelivery ||
				moduleFilter.Description == FilterConstants.PickUpRequestedFrom ||
				moduleFilter.Description == FilterConstants.PickUpRequestedTo ||
				moduleFilter.Description == FilterConstants.PlannedPickUp;
		}

		#endregion

		#region GetJoinClauseForZoneHubSelection

		ZString GetJoinClauseForZoneHubSelection()
		{
			var isDepotFilterUsed = ActiveModuleFiltersForQueryNonEmpty.Any(mf => mf.Description == FilterConstants.DepotAddress);

			return isDepotFilterUsed
				?
						@"JOIN dbo.PortHubSelection ON TY_PK IN
							(
								SELECT
									TX_TY_Hub
								FROM
									dbo.PortHubZonePivot
								WHERE
									TX_TZ_Zone = KN_TZ_DomesticZone
							)"
				: "";
		}

		#endregion

		#region GetJoinClauseForServicesExist

		ZString GetJoinClauseForServicesExist()
		{
			var isServiceExistFilterUsed = ActiveModuleFiltersForQueryNonEmpty.Any(mf => mf.Description == FilterConstants.ServicesExist);

			return isServiceExistFilterUsed
				?
						@"LEFT JOIN dbo.JobService as ConsignmentServices ON ConsignmentServices.ES_ParentID = KM_PK AND ConsignmentServices.ES_CurrentContextID IS NULL 
							LEFT JOIN dbo.JobService as InstructionServices ON InstructionServices.ES_CurrentContextID = Instructions.KN_PK 
						"
						: "";
		}

		#endregion

		#endregion

		#region User Defined Filters - Where Clause

		static string GetWhereClauseForUserDefinedFilters(Dictionary<FilterOrCategory, List<ModuleFilter>> orCategoriesWithFilters)
		{
			var whereClauseBuilder = new ZStringBuilder();

			foreach (var category in orCategoriesWithFilters)
			{
				bool firstFilter = true;

				foreach (var moduleFilter in category.Value.Where(mf => !mf.Query.IsEmpty))
				{
					string prefix;

					if (firstFilter)
					{
						prefix = (category.Key != FilterOrCategory.None) ? (NoResString)"AND (" : "AND "; // Part of SQL expression.
						firstFilter = false;
					}
					else
					{
						prefix = category.Key == FilterOrCategory.None ? " AND " : " OR ";
					}

					whereClauseBuilder.Append(string.Format(Culture.Invariant, "{0}({1})", prefix, moduleFilter.Query.LiteralTextSqlFormatted.TrimEnd()));
				}

				if (category.Key != FilterOrCategory.None)
				{
					whereClauseBuilder.Append(")");
				}
			}

			return whereClauseBuilder.ToString();
		}

		#endregion

		#endregion

		#region ViewMode

		public DtbRoutePlannerViewMode ViewMode
		{
			get;
			set;
		}

		#endregion

		#region ChildFilterBusinessObjects

		internal void AddChildFilterBusinessObject(DtbChildFilterBusinessObject childFilterBusinessObject)
		{
			childFilterBusinessObject.routePlannerFilterBusinessObject = this;
			ChildFilterBusinessObjects.Add(childFilterBusinessObject);
		}

		List<DtbChildFilterBusinessObject> ChildFilterBusinessObjects
		{
			get { return childFilterBusinessObjects ?? (childFilterBusinessObjects = new List<DtbChildFilterBusinessObject>()); }
		}

		List<DtbChildFilterBusinessObject> childFilterBusinessObjects;

		#endregion
	}
}
