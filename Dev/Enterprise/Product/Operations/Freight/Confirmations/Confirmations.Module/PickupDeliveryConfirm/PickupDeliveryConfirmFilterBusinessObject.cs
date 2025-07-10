using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Confirmations.Module
{
	public class PickupDeliveryConfirmFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			AddNumbersAndReferencesFilters(filters);
			AddDatesFilters(filters);
			AddOrganisationsAndStaffFilters(filters);
			AddModeFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddLocationFilters(filters);

			return filters;
		}

		#region Numbers

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			var transportBookingFilter = filters.AddFountainFilter("Transport Booking #", GetTransportBookingNumberQuery, "CT")
				.WithMaxLengthOf<ModuleFountainFilter>(JobConsolidatedTransportBookingSchema.D1_UniqueConsignRef);
			transportBookingFilter.MultilingualDescription = ResString.GetMultilingualString("9830b7f6-0f44-4d49-9b06-f50f13052ac7", "Transport Booking #");
			transportBookingFilter.IsCommon = true;

			var containerNumberFilter = filters.AddNumberFilter("Container #", GetContainerNumberQuery)
				.WithMaxLengthOf<ModuleNumberFilter>(JobContainerSchema.JC_ContainerNum);
			containerNumberFilter.MultilingualDescription = ResString.GetMultilingualString("c9b14222-7db3-4d92-b7e7-7f886c271d0d", "Container #");
			containerNumberFilter.IsCommon = true;

			var shipmentNumberFilter = filters.AddFountainFilter("Shipment #", GetShipmentNumberQuery, "S")
				.WithMaxLengthOf<ModuleFountainFilter>(JobShipmentSchema.JS_UniqueConsignRef);
			shipmentNumberFilter.MultilingualDescription = ResString.GetMultilingualString("15170c6e-2dad-41bb-a1e4-22b1b7a95c56", "Shipment #");
			shipmentNumberFilter.IsCommon = true;
		}

		#region GetTransportBookingNumberQuery

		ZQuery GetTransportBookingNumberQuery(SQLComparisonOperator comparisonOperator, ZString transportBookingNo)
		{
			return GetTransportBookingQuery(JobConsolidatedTransportBookingSchema.D1_UniqueConsignRef, comparisonOperator, transportBookingNo);
		}

		#endregion

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
			var filter = filters.AddDateFilter("Actual", JobPickupDeliveryConfirmSchema.EU_PickupDeliveryTime);
			filter.MultilingualDescription = ResString.GetMultilingualString("a1439683-9a17-4a71-87da-e6cde1aa6e3c", "Actual");
			filter.Category = FilterCategories.Dates;

			filter = filters.AddDateFilter("Planned", JobPickupDeliveryConfirmSchema.EU_PlannedPickupDeliveryTime);
			filter.MultilingualDescription = ResString.GetMultilingualString("7c6d4831-e564-4b9e-8ab2-533645c7f111", "Planned");
			filter.Category = FilterCategories.Dates;

			filter = filters.AddDateFilter("Requested By", JobPickupDeliveryConfirmSchema.EU_RequestedPickupDeliveryTime);
			filter.MultilingualDescription = ResString.GetMultilingualString("d839d36e-9341-4364-a460-070bda3cf407", "Requested By");
			filter.Category = FilterCategories.Dates;
		}

		#endregion

		#region Organisations And Staff

		void AddOrganisationsAndStaffFilters(ModuleFilterCollection filters)
		{
			ModuleFilter filter = filters.AddGuidFilter("Transport Company", ModuleIDs.Organisation, GetTransportOrganisationQuery, BindingLists.TransportProviders);
			filter.MultilingualDescription = ResString.GetMultilingualString("14341b62-72c5-4077-a646-c4c0d04ed95c", "Transport Company");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddTextFilter("Drivers Name", JobPickupDeliveryConfirmSchema.EU_DriversName);
			filter.MultilingualDescription = ResString.GetMultilingualString("f93a0835-7da1-4680-976b-644c181cee21", "Drivers Name");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddTextFilter("Transport Company Name", JobPickupDeliveryConfirmSchema.EU_TransportCoName);
			filter.MultilingualDescription = ResString.GetMultilingualString("8089e0d4-0105-44c1-a565-f9014d84191d", "Transport Company Name");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddTextFilter("Drivers Licence", JobPickupDeliveryConfirmSchema.EU_DriversLicence);
			filter.MultilingualDescription = ResString.GetMultilingualString("528072af-00b9-4432-a695-848be1b4840c", "Drivers License");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddTextFilter("Vehicle Registration", JobPickupDeliveryConfirmSchema.EU_VehicleRegistration);
			filter.MultilingualDescription = ResString.GetMultilingualString("774f8542-561f-4882-8efa-a9aa29a5c0e5", "Vehicle Registration");
			filter.Category = FilterCategories.Organisations;

			filter = filters.AddGuidFilter("Pickup/Delivery", ModuleIDs.Organisation, GetPickupDeliveryOrganisationQuery, BindingLists.Organisations);
			filter.MultilingualDescription = ResString.GetMultilingualString("FF2DE8C8-57F6-4D1E-AC72-8E46CEDA1777", "Pickup/Delivery");
			filter.Category = FilterCategories.Organisations;
		}

		#region GetTransportOrganisationQuery

		ZQuery GetTransportOrganisationQuery(ZGuid orgPK)
		{
			ZQuery result = new ZQuery();

			if (orgPK.IsValid)
			{
				result.AddToFilter(GetOrgAddressQuery(JobPickupDeliveryConfirmSchema.EU_OA_TransportProvider, orgPK));
			}

			return result;
		}

		#endregion

		#region GetPickupDeliveryOrganisationQuery

		ZQuery GetPickupDeliveryOrganisationQuery(ZGuid orgPK)
		{
			ZQuery result = new ZQuery();

			if (orgPK.IsValid)
			{
				result.AddToFilter(GetDocAddressOAQuery(DocAddressType.None, OrgAddressSchema.OA_OH, SQLComparisonOperator.Equal, orgPK));
			}

			return result;
		}

		#endregion

		#endregion

		#region Modes

		void AddModeFilters(ModuleFilterCollection filters)
		{
			ModuleTextFilter confirmationTypeFilter = filters.AddTextFilter("Confirmation Type", GetConfirmationTypeQuery, PickupDeliveryConfirmTypes);
			confirmationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("71741d30-9b0c-4d97-bb02-9805126d7cfc", "Confirmation Type");
			confirmationTypeFilter.Category = FilterCategories.ModesAndTypes;
			confirmationTypeFilter.Visibility = FilterVisibility.AlwaysApplied;
			confirmationTypeFilter.DefaultProperty = CommonPickupDeliveryConfirm.ConfirmFilterConstants.All;
		}

		#region GetConfirmationTypeQuery

		ZQuery GetConfirmationTypeQuery(ZString confirmType)
		{
			ZQuery result = new ZQuery();

			if (confirmType.Trim().EqualsIgnoringCase(CommonPickupDeliveryConfirm.ConfirmFilterConstants.All))
			{
				ZQuery allQuery = new ZQuery(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType,
					Array.ConvertAll(Array.FindAll(PickupDeliveryConfirmTypes.ToArray(), pair => pair.Code != CommonPickupDeliveryConfirm.ConfirmFilterConstants.All), pair => pair.Code));

				result.AddToFilter(allQuery);
			}
			else if (!confirmType.Trim().IsEmpty)
			{
				result.AddToFilter(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryType, SQLComparisonOperator.Equal, confirmType);
			}

			return result;
		}

		#endregion

		#region PickupDeliveryConfirmTypes

		public CodeDescriptionPairList PickupDeliveryConfirmTypes
		{
			get
			{
				if (pickupDeliveryConfirmTypes == null)
				{
					pickupDeliveryConfirmTypes = new CodeDescriptionPairList();
					pickupDeliveryConfirmTypes.AddPair(CommonPickupDeliveryConfirm.ConfirmFilterConstants.All, CommonPickupDeliveryConfirm.ConfirmFilterConstants.All);
					pickupDeliveryConfirmTypes.AddRange(BindingLists.ForwardingPickupDeliveryConfirmTypes);
				}
				return pickupDeliveryConfirmTypes;
			}
		}
		CodeDescriptionPairList pickupDeliveryConfirmTypes;

		#endregion

		#endregion

		#region StatusAndFlags Filters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter("Booking Allocated", GetBookingStatusQuery, BookingStatusList);
			filter.MultilingualDescription = ResString.GetMultilingualString("4c023410-f783-45f2-a203-9b1fda2afc6d", "Booking Allocated");
			filter.Category = FilterCategories.StatusAndFlags;

			filter = filters.AddTextFilter("Completed", GetCompletedQuery, CompleteStatusList);
			filter.MultilingualDescription = ResString.GetMultilingualString("3e49bf5a-a5d6-4895-88eb-2502eff610d2", "Completed");
			filter.Category = FilterCategories.StatusAndFlags;
		}

		#region GetBookingStatusQuery

		ZQuery GetBookingStatusQuery(ZString bookingStatus)
		{
			ZQuery result = new ZQuery();

			if (bookingStatus.Trim().EqualsIgnoringCase(CommonPickupDeliveryConfirm.ConfirmFilterConstants.Allocated))
			{
				result.AddToFilter(JobPickupDeliveryConfirmSchema.EU_D1, SQLComparisonOperator.NotEqual, null);
			}
			else if (bookingStatus.Trim().EqualsIgnoringCase(CommonPickupDeliveryConfirm.ConfirmFilterConstants.Unallocated))
			{
				result.AddToFilter(JobPickupDeliveryConfirmSchema.EU_D1, SQLComparisonOperator.Equal, null);
			}

			return result;
		}

		#endregion

		#region GetCompletedQuery

		ZQuery GetCompletedQuery(ZString completeStatus)
		{
			ZQuery result = new ZQuery();

			if (completeStatus.Trim().EqualsIgnoringCase(CommonPickupDeliveryConfirm.ConfirmFilterConstants.Complete))
			{
				result.AddToFilter(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryTime, SQLComparisonOperator.NotEqual, null);
			}
			else if (completeStatus.Trim().EqualsIgnoringCase(CommonPickupDeliveryConfirm.ConfirmFilterConstants.Incomplete))
			{
				result.AddToFilter(JobPickupDeliveryConfirmSchema.EU_PickupDeliveryTime, SQLComparisonOperator.Equal, null);
			}

			return result;
		}

		#endregion

		#endregion

		#region AddLocationFilters

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var relatedPortFilter = filters.AddNkFilter("Related Port", GetRelatedPortQuery, ModuleIDs.RefUNLOCO, new RefUNLOCOCollection(Factory))
				.WithMaxLengthOf<ModuleNkFilter>(OrgAddressSchema.OA_RL_NKRelatedPortCode);
			relatedPortFilter.Category = FilterCategories.Locations;
			relatedPortFilter.MultilingualDescription = ResString.GetMultilingualString("CF90C71E-BA93-41F6-8142-EDD2B3A795F4", "Related Port");
		}

		ZQuery GetRelatedPortQuery(ZString relatedPort)
		{
			ZDBOnlySubQuery orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			orgAddressQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.StartsWith, relatedPort);

			ZDBOnlySubQuery orgAddress_WithHeaderQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);
			orgAddress_WithHeaderQuery.AddToFilter(OrgAddressSchema.OA_RL_NKRelatedPortCode, SQLComparisonOperator.Equal, ZString.Empty);
			ZDBOnlySubQuery orgSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);
			orgSubQuery.AddToFilter(OrgHeaderSchema.OH_RL_NKClosestPort, SQLComparisonOperator.StartsWith, relatedPort);
			orgAddress_WithHeaderQuery.AddSubQuery(orgSubQuery, JoinCondition.And);

			ZDBOnlyQuery docAddressWithOrgAddressQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			docAddressWithOrgAddressQuery.AddSubQuery(orgAddressQuery, JoinCondition.Or);
			docAddressWithOrgAddressQuery.AddSubQuery(orgAddress_WithHeaderQuery, JoinCondition.Or);
			//Don't care about DocAddressType
			return GetDocAddressQuery(docAddressWithOrgAddressQuery);
		}

		#endregion

		#region Lookups

		BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#region BookingStatusList

		public CodeDescriptionPairList BookingStatusList
		{
			get
			{
				if (bookingStatusList == null)
				{
					bookingStatusList = new CodeDescriptionPairList();
					bookingStatusList.AddPair(CommonPickupDeliveryConfirm.ConfirmFilterConstants.All, Res.GetString("e27f8991-d715-4f3b-91d4-2e5c1d7dddcb", "Show both Allocated and Unallocated Confirmations"));
					bookingStatusList.AddPair(CommonPickupDeliveryConfirm.ConfirmFilterConstants.Allocated, Res.GetString("f223fe84-174f-43f0-b694-35cba1f7bfc4", "Show only Confirmations allocated to a Consolidate Transport Booking"));
					bookingStatusList.AddPair(CommonPickupDeliveryConfirm.ConfirmFilterConstants.Unallocated, Res.GetString("66b697f7-fc5a-43df-b266-0907057a2b21", "Show only Confirmations that are not allocated to a Consolidate Transport Booking"));
				}
				return bookingStatusList;
			}
		}
		CodeDescriptionPairList bookingStatusList;

		#endregion

		#region CompleteStatusList

		public CodeDescriptionPairList CompleteStatusList
		{
			get
			{
				if (completeStatusList == null)
				{
					completeStatusList = new CodeDescriptionPairList();
					completeStatusList.AddPair(CommonPickupDeliveryConfirm.ConfirmFilterConstants.All, Res.GetString("8ca45981-63d9-41e5-b0c1-3c14933213a7", "Show both Completed and Non-Completed Confirmations"));
					completeStatusList.AddPair(CommonPickupDeliveryConfirm.ConfirmFilterConstants.Complete, Res.GetString("98a00390-9a0e-4de8-b37d-f5f3e44bd5a3", "Show only Confirmations that are Completed"));
					completeStatusList.AddPair(CommonPickupDeliveryConfirm.ConfirmFilterConstants.Incomplete, Res.GetString("a8726100-92d7-429f-9201-78b0efa84c9d", "Show only Confirmations that are not Completed"));
				}
				return completeStatusList;
			}
		}
		CodeDescriptionPairList completeStatusList;

		#endregion

		#endregion

		#region Implementation

		#region GetTransportBookingQuery

		protected ZDBOnlyQuery GetTransportBookingQuery(SchemaColumn bookingSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonPickupDeliveryConfirm));

			ZDBOnlySubQuery transportBookingSubQuery = new ZDBOnlySubQuery(typeof(CommonConsolidatedTransportBooking), JobPickupDeliveryConfirmSchema.EU_D1);
			transportBookingSubQuery.AddToFilter_PossiblyCommaSeparated(bookingSchemaColumn, comparisonOperator, value);
			result.AddSubQuery(transportBookingSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetContainerQuery

		protected ZDBOnlyQuery GetContainerQuery(SchemaColumn containerSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			var result = new ZDBOnlyQuery(typeof(CommonPickupDeliveryConfirm));

			var containerSubQuery = new ZDBOnlySubQuery(typeof(CommonContainer), JobPickupDeliveryConfirmSchema.EU_JC);
			containerSubQuery.AddToFilter_PossiblyCommaSeparated(containerSchemaColumn, comparisonOperator, value);

			result.AddSubQuery(containerSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetShipmentQuery

		protected ZDBOnlyQuery GetShipmentQuery(SchemaColumn shipmentSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonPickupDeliveryConfirm));

			ZDBOnlySubQuery divotSubQuery = new ZDBOnlySubQuery(typeof(CommonConfirmDivot), JobTransportLegPackLineDivotSchema.J8_EU_PickupDeliverConfirm);
			ZDBOnlySubQuery packlineSubQuery = new ZDBOnlySubQuery(typeof(PackLine), JobTransportLegPackLineDivotSchema.J8_JL);
			ZDBOnlySubQuery shipmentSubQuery = new ZDBOnlySubQuery(typeof(CommonShipment), JobPackLinesSchema.JL_JS);
			shipmentSubQuery.AddToFilter_PossiblyCommaSeparated(shipmentSchemaColumn, comparisonOperator, value);
			packlineSubQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);
			divotSubQuery.AddSubQuery(packlineSubQuery, JoinCondition.And);
			result.AddSubQuery(divotSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetOrgAddressQuery

		ZDBOnlyQuery GetOrgAddressQuery(SchemaColumn oaSchemaColumn, ZGuid orgPK)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonPickupDeliveryConfirm));

			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), oaSchemaColumn);
			ZDBOnlySubQuery orgHeaderSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgAddressSchema.OA_OH);

			orgHeaderSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			orgAddressSubQuery.AddSubQuery(orgHeaderSubQuery, JoinCondition.And);

			result.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetDocAddressQuery (helper)

		ZDBOnlyQuery GetDocAddressOAQuery(DocAddressType addressType, SchemaColumn oaSchemaColumn, SQLComparisonOperator comparisonOperator, IZType value)
		{
			ZDBOnlyQuery docAddressQuery = new ZDBOnlyQuery(typeof(JobDocAddress));
			ZDBOnlySubQuery orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			orgAddressSubQuery.AddToFilter(oaSchemaColumn, comparisonOperator, value);

			if (addressType != DocAddressType.None)
			{
				docAddressQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, addressType));
			}

			docAddressQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			return GetDocAddressQuery(docAddressQuery);
		}

		ZDBOnlyQuery GetDocAddressQuery(ZQuery docAddressQuery)
		{
			ZDBOnlyQuery result = new ZDBOnlyQuery(typeof(CommonPickupDeliveryConfirm));
			ZDBOnlySubQuery docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			docAddressSubQuery.AddToFilter(docAddressQuery);
			result.AddSubQuery(docAddressSubQuery, JoinCondition.And);
			return result;
		}

		#endregion

		#endregion
	}
}
