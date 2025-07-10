using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Confirmations.Module
{
	public class ConsolidatedTransportBookingFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddNumbersAndReferencesFilters(filters);
			AddDatesFilters(filters);
			AddOrganisationsAndStaffFilters(filters);
			SecurityProvider.AddCRMSecurityFilterStrips(Factory, filters);

			return filters;
		}

		#region Numbers

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddFountainFilter("Booking #", JobConsolidatedTransportBookingSchema.D1_UniqueConsignRef, "CT");
			filter.MultilingualDescription = ResString.GetMultilingualString("a44793b9-92c1-4b52-afad-073c00a1a8d0", "Booking #");
			filter.Category = FilterCategories.NumbersAndReferences;

			filters.AddNumberFilter("Booking Ref", JobConsolidatedTransportBookingSchema.D1_BookingReference).MultilingualDescription = ResString.GetMultilingualString("95b2c24c-4fd8-4d5f-b77a-5f9ac61471fb", "Booking Ref");

			filter = filters.AddNumberFilter("Container #", GetContainerNumberQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_ContainerNum);
			filter.MultilingualDescription = ResString.GetMultilingualString("f19c26f3-ec82-4b09-9311-047b8098032e", "Container #");
			filter.IsCommon = true;

			filter = filters.AddFountainFilter("Shipment #", GetShipmentNumberQuery, "S")
				.WithMaxLengthOf<ModuleFountainFilter>(JobShipmentSchema.JS_UniqueConsignRef);
			filter.MultilingualDescription = ResString.GetMultilingualString("d6b33fe1-4f2b-40da-9daf-0a91f4f1f51e", "Shipment #");
			filter.IsCommon = true;
		}

		#region GetContainerNumberQuery

		ZQuery GetContainerNumberQuery(SQLComparisonOperator comparisonOperator, ZString containerNo)
		{
			return GetContainerQuery(JobContainerSchema.JC_ContainerNum, comparisonOperator, containerNo);
		}

		#endregion

		#region GetShipmentNumberQuery

		ZQuery GetShipmentNumberQuery(SQLComparisonOperator comparisonOperator, ZString shipmentNo)
		{
			return GetShipmentQuery(JobShipmentSchema.JS_UniqueConsignRef, comparisonOperator, shipmentNo);
		}

		#endregion

		#endregion

		#region Dates

		void AddDatesFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddDateFilter("Booked", JobConsolidatedTransportBookingSchema.D1_BookingDate);
			filter.MultilingualDescription = ResString.GetMultilingualString("2df0a9a8-6558-4c02-8521-cde89d21ce8a", "Booked");
			filter.Category = FilterCategories.Dates;
		}

		#endregion

		#region Organisations And Staff

		void AddOrganisationsAndStaffFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddGuidFilter("Transport Company", ModuleIDs.Organisation, GetTransportOrganisationQuery, BindingLists.TransportProviders);
			filter.MultilingualDescription = ResString.GetMultilingualString("870f1c6c-e66d-41c0-8b84-81ae724cb9f1", "Transport Company");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddGuidFilter("Customer", ModuleIDs.Organisation, GetCustomerOrganisationQuery, BindingLists.Organisations);
			filter.MultilingualDescription = ResString.GetMultilingualString("367f29cc-52b8-4a37-b2fd-18e9aacb731c", "Customer");
			filter.Category = FilterCategories.Organisations;
		}

		#region GetTransportOrganisationQuery

		ZQuery GetTransportOrganisationQuery(ZGuid orgPK)
		{
			ZQuery result = new ZQuery();

			if (orgPK.IsValid)
			{
				result.AddToFilter(GetOrgAddressQuery(JobConsolidatedTransportBookingSchema.D1_OA_TransportCo, orgPK));
			}

			return result;
		}

		#endregion

		#region GetCustomerOrganisationQuery

		ZQuery GetCustomerOrganisationQuery(ZGuid orgPK)
		{
			ZQuery result = new ZQuery();

			if (orgPK.IsValid)
			{
				result.AddToFilter(GetOrgAddressQuery(JobConsolidatedTransportBookingSchema.D1_OA_Customer, orgPK));
			}

			return result;
		}

		#endregion

		#endregion

		#region Implementation

		#region GetContainerQuery

		protected ZDBOnlyQuery GetContainerQuery(SchemaColumn containerSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			var result = new ZDBOnlyQuery(typeof(CommonConsolidatedTransportBooking));

			var containerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobPickupDeliveryConfirmSchema.EU_JC);
			containerSubQuery.AddToFilter_PossiblyCommaSeparated(containerSchemaColumn, comparisonOperator, value);

			var confirmSubQuery = new ZDBOnlySubQuery(typeof(CommonPickupDeliveryConfirm), JobPickupDeliveryConfirmSchema.EU_D1);
			confirmSubQuery.AddSubQuery(containerSubQuery, JoinCondition.And);

			result.AddSubQuery(confirmSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetShipmentQuery

		protected ZDBOnlyQuery GetShipmentQuery(SchemaColumn shipmentSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonConsolidatedTransportBooking));

			ZDBOnlySubQuery confirmSubQuery = new ZDBOnlySubQuery(typeof(CommonPickupDeliveryConfirm), JobPickupDeliveryConfirmSchema.EU_D1);
			ZDBOnlySubQuery divotSubQuery = new ZDBOnlySubQuery(typeof(CommonConfirmDivot), JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm);
			ZDBOnlySubQuery packlineSubQuery = new ZDBOnlySubQuery(typeof(PackLine), JobTransportLegPackLineDivotSchema.J8_JL);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobPackLinesSchema.JL_JS);
			shipmentSubQuery.AddToFilter_PossiblyCommaSeparated(shipmentSchemaColumn, comparisonOperator, value);
			packlineSubQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
			divotSubQuery.AddSubQuery(packlineSubQuery, JoinCondition.And);
			confirmSubQuery.AddSubQuery(divotSubQuery, JoinCondition.And);
			result.AddSubQuery(confirmSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetOrgAddressQuery

		ZDBOnlyQuery GetOrgAddressQuery(SchemaColumn oaSchemaColumn, ZGuid orgPK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonConsolidatedTransportBooking));

			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), oaSchemaColumn);
			ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);

			orgHeaderSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);

			result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region Lookups

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#endregion

		readonly ConsolidatedTransportBookingCRMSecurityProvider SecurityProvider = new ConsolidatedTransportBookingCRMSecurityProvider();

		#endregion
	}
}
