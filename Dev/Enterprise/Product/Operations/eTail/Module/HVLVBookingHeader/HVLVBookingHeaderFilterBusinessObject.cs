using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Module
{
	public class HVLVBookingHeaderFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			var consignmentFilterBusinessObject = new HVLVConsignmentFilterBusinessObject();
			var consignmentFilters = consignmentFilterBusinessObject.ModuleFilters;

			var descriptionValues = typeof(HVLVConsignmentFilterBusinessObject.Descriptions)
				.GetFields(BindingFlags.Static | BindingFlags.Public)
				.Select(field => field.GetValue(null))
				.ToList();

			foreach (var consignmentFilter in consignmentFilters)
			{
				if (descriptionValues.Contains(consignmentFilter.Description.ToString()))
				{
					if (consignmentFilter.SubGroup == consignmentFilterBusinessObject.ItemFilterProcessor)
					{
						consignmentFilter.SubGroup = ItemFilterProcessor;
						filters.AddFilter(consignmentFilter);
					}
					else
					{
						consignmentFilter.SubGroup = ConsignmentFilterProcessor;
						filters.AddFilter(consignmentFilter);
					}
				}
			}

			AddNumbersAndReferencesFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddOrganisationFilters(filters);
			AddModeAndTypeFilters(filters);
			AddOtherFilters(filters);

			return filters;
		}

		#region Descriptions

		static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string BookingHeaderNo = "Booking Header #";
			public const string LoadListNo = "Load List #";
			public const string ItemShipmentNo = "Item Shipment #";
			public const string ConsolNo = "Consol #";

			public const string ManifestStatus = "Manifest Status";
			public const string LoadListStatus = "Load List Status";

			public const string BillToParty = "Bill To Party";
			public const string OriginDepot = "Origin Depot";
			public const string Dispatch = "Dispatch";
			public const string BookedBy = "Booked By";

			public const string ServiceLevel = "Service Level";

			public const string CustomsIndicators = "Customs Indicators";
			public const string CreatedInPortal = "Created in Portal";
			public const string Confirmed = "Confirmed";

			public const string ReceivedAtOrigin = "Received at Origin";

			#endregion
		}

		#endregion

		#region SubGroups

		public ModuleFilterSubGroup ConsignmentFilterProcessor => consignmentFilterProcessor ?? (consignmentFilterProcessor = new ConsignmentSubGroup());
		ConsignmentSubGroup consignmentFilterProcessor;

		public ModuleFilterSubGroup ItemFilterProcessor => itemFilterProcessor ?? (itemFilterProcessor = new ItemSubGroup(ConsignmentFilterProcessor));
		ItemSubGroup itemFilterProcessor;

		public ModuleFilterSubGroup LoadListFilterProcessor => loadListFilterProcessor ?? (loadListFilterProcessor = new LoadListSubGroup(ItemFilterProcessor));
		LoadListSubGroup loadListFilterProcessor;

		public ModuleFilterSubGroup ShipmentFilterProcessor => shipmentFilterProcessor ?? (shipmentFilterProcessor = new ShipmentSubGroup(ItemFilterProcessor));
		ShipmentSubGroup shipmentFilterProcessor;

		public ModuleFilterSubGroup ConsolFilterProcessor => consolFilterProcessor ?? (consolFilterProcessor = new ConsolSubGroup(ShipmentFilterProcessor));
		ConsolSubGroup consolFilterProcessor;

		public class ConsignmentSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var consignmentSubQuery = new ZDBOnlySubQuery(typeof(HVLVConsignment), HVLVConsignmentSchema.HVC_HVH_BookingHeader);
				consignmentSubQuery.AddToFilter(filter);

				var headerQuery = new ZDBOnlyQuery(typeof(HVLVBookingHeader));
				headerQuery.AddSubQuery(consignmentSubQuery, JoinCondition.And);

				return headerQuery;
			}
		}

		public class ItemSubGroup : ModuleFilterSubGroup
		{
			public ItemSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var itemSubQuery = new ZDBOnlySubQuery(typeof(HVLVItem), HVLVItemSchema.HVI_HVC_Consignment);
				itemSubQuery.AddToFilter(filter);

				var consignmentQuery = new ZDBOnlyQuery(typeof(HVLVConsignment));
				consignmentQuery.AddSubQuery(itemSubQuery, JoinCondition.And);

				return consignmentQuery;
			}
		}

		class LoadListSubGroup : ModuleFilterSubGroup
		{
			public LoadListSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var loadListSubQuery = new ZDBOnlySubQuery(typeof(HVLVOriginLoadList), HVLVItemSchema.HVI_HVL_LoadList);
				loadListSubQuery.AddToFilter(filter);

				var itemQuery = new ZDBOnlyQuery(typeof(HVLVItem));
				itemQuery.AddSubQuery(loadListSubQuery, JoinCondition.And);

				return itemQuery;
			}
		}

		class ShipmentSubGroup : ModuleFilterSubGroup
		{
			public ShipmentSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var shipmentSubQuery = new ZDBOnlySubQuery(typeof(ForwardingShipment), HVLVItemSchema.HVI_JS_LoadedOnShipment);
				shipmentSubQuery.AddToFilter(filter);

				var itemQuery = new ZDBOnlyQuery(typeof(HVLVItem));
				itemQuery.AddSubQuery(shipmentSubQuery, JoinCondition.And);

				return itemQuery;
			}
		}

		class ConsolSubGroup : ModuleFilterSubGroup
		{
			public ConsolSubGroup(ModuleFilterSubGroup parent)
				: base(parent)
			{
			}

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var consolSubQuery = new ZDBOnlySubQuery(typeof(ForwardingConsol), JobConShipLinkSchema.JN_JK);
				consolSubQuery.AddToFilter(filter);

				var pivotSubQuery = new ZDBOnlySubQuery(typeof(JobConShipLink), JobConShipLinkSchema.JN_JS);
				pivotSubQuery.AddSubQuery(consolSubQuery, JoinCondition.And);

				var shipmentQuery = new ZDBOnlyQuery(typeof(ForwardingShipment));
				shipmentQuery.AddSubQuery(pivotSubQuery, JoinCondition.And);

				return shipmentQuery;
			}
		}

		#endregion

		#region Numbers and References

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			var bookingHeaderNoFilter = filters.AddTextFilter(Descriptions.BookingHeaderNo, HVLVBookingHeaderSchema.HVH_BookingReference);
			bookingHeaderNoFilter.MultilingualDescription = ResString.GetMultilingualString("1d36a5de-4aee-493a-9b9e-8320babf9503", "Booking Header #");
			bookingHeaderNoFilter.Category = FilterCategories.NumbersAndReferences;

			var loadListNoFilter = filters.AddTextFilter(Descriptions.LoadListNo, HVLVOriginLoadListSchema.HVL_UniqueReference);
			loadListNoFilter.MultilingualDescription = ResString.GetMultilingualString("1cc1ec6c-1c74-43fd-9942-dd0ff7a12f50", "Load List #");
			loadListNoFilter.SubGroup = LoadListFilterProcessor;
			loadListNoFilter.Category = FilterCategories.NumbersAndReferences;

			var itemShipmentNoFilter = filters.AddTextFilter(Descriptions.ItemShipmentNo, JobShipmentSchema.JS_UniqueConsignRef);
			itemShipmentNoFilter.MultilingualDescription = ResString.GetMultilingualString("536ac12e-42ce-4cbe-a7b8-01d1e1744242", "Item Shipment #");
			itemShipmentNoFilter.SubGroup = ShipmentFilterProcessor;
			itemShipmentNoFilter.Category = FilterCategories.NumbersAndReferences;

			var consolNoFilter = filters.AddTextFilter(Descriptions.ConsolNo, JobConsolSchema.JK_UniqueConsignRef);
			consolNoFilter.MultilingualDescription = ResString.GetMultilingualString("0551445b-e4ab-4e95-ba30-1aa2dc8b22df", "Consol #");
			consolNoFilter.SubGroup = ConsolFilterProcessor;
			consolNoFilter.Category = FilterCategories.NumbersAndReferences;
		}

		#endregion

		#region Status and Flags

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var manifestStatusFilter = filters.AddTextFilter(Descriptions.ManifestStatus, ManifestStatusQuery, ManifestStatusLookup);
			manifestStatusFilter.MultilingualDescription = ResString.GetMultilingualString("06738eb1-0264-4519-a012-d76962d7d789", "Manifest Status");
			manifestStatusFilter.Category = FilterCategories.StatusAndFlags;

			var loadListStatusFilter = filters.AddTextFilter(Descriptions.LoadListStatus, HVLVOriginLoadListSchema.HVL_Status);
			loadListStatusFilter.MultilingualDescription = ResString.GetMultilingualString("c99d2e08-1e61-488b-8805-9fe4e070ca3e", "Load List Status");
			loadListStatusFilter.SubGroup = LoadListFilterProcessor;
			loadListStatusFilter.Category = FilterCategories.StatusAndFlags;

			var confirmedFilter = filters.AddFlagFilter(Descriptions.Confirmed,
				Res.GetString("fa25be45-dc89-4321-9d6b-7e13e5031eca", "Confirmed"),
				HVLVBookingHeaderSchema.HVH_IsBookingConfirmed,
				ModuleFilterSubGroup.Default);
			confirmedFilter.MultilingualDescription = ResString.GetMultilingualString("ebf9a78c-52fc-4cec-83ac-a582e03d9eb0", "Confirmed");
			confirmedFilter.Category = FilterCategories.StatusAndFlags;

			var receivedAtOriginFilter = filters.AddFlagFilter(Descriptions.ReceivedAtOrigin,
				Res.GetString("D6D1DF32-96F1-489C-9D00-96F90451DC5E", "Received at Origin"),
				HVLVBookingHeaderSchema.HVH_IsBookingReceived,
				ModuleFilterSubGroup.Default);
			receivedAtOriginFilter.MultilingualDescription = ResString.GetMultilingualString("6FF22795-CFD9-4486-8C95-D4808DCD76CA", "Received at Origin");
			receivedAtOriginFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery ManifestStatusQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(HVLVBookingHeader));
			query.AddToFilter(HVLVBookingHeaderSchema.HVH_IsBookingConfirmed, value == HVLVBookingStatus.ConfirmedCode);

			return query;
		}

		#endregion

		#region Organisations and Staff

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			Func<string, SchemaGuidColumn, Type, ModuleGuidFilter> addOrgAddressFilter = (name, orgAddressColumn, parentType) =>
			{
				GetGuidQuery queryDelegate = orgHeaderPK =>
				{
					var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), orgAddressColumn);
					orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgHeaderPK);

					var parentQuery = new ZDBOnlyQuery(parentType);
					parentQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

					return parentQuery;
				};

				var filter = filters.AddGuidFilter(name, ModuleIDs.Organisation, queryDelegate, OrganisationsLookup);
				filter.Category = FilterCategories.Organisations;
				return filter;
			};

			var billToPartyFilter = addOrgAddressFilter(Descriptions.BillToParty, HVLVBookingHeaderSchema.HVH_OA_BillToParty, typeof(HVLVBookingHeader));
			billToPartyFilter.MultilingualDescription = ResString.GetMultilingualString("65c355fb-a881-4c70-98f7-d06e99834e0d", "Bill To Party");

			var originDepotFilter = addOrgAddressFilter(Descriptions.OriginDepot, HVLVBookingHeaderSchema.HVH_OA_OriginDepot, typeof(HVLVBookingHeader));
			originDepotFilter.MultilingualDescription = ResString.GetMultilingualString("d07c540a-863c-48ab-984c-3307c39ea3af", "Origin Depot");

			var dispatchFilter = addOrgAddressFilter(Descriptions.Dispatch, HVLVBookingHeaderSchema.HVH_OA_DispatchAddress, typeof(HVLVBookingHeader));
			dispatchFilter.MultilingualDescription = ResString.GetMultilingualString("755e8f8c-14e9-401c-acfe-c4a9b73eb73e", "Dispatch");

			var bookedByFilter = filters.AddGuidFilter(Descriptions.BookedBy, ModuleIDs.OrgContacts, HVLVBookingHeaderSchema.HVH_OC_BookedBy, ContactsLookup);
			bookedByFilter.MultilingualDescription = ResString.GetMultilingualString("b5444932-1f4d-4bc5-9f8d-16f5163a651e", "Booked By");
			bookedByFilter.Category = FilterCategories.Organisations;
		}

		#endregion

		#region Modes and Types

		void AddModeAndTypeFilters(ModuleFilterCollection filters)
		{
			var serviceLevelFilter = filters.AddTextFilter(Descriptions.ServiceLevel, HVLVBookingHeaderSchema.HVH_RS_NKBookingServiceLevel, ServiceLevelLookup);
			serviceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("495df526-9c1f-4914-81b4-2e3076f8aec1", "Service Level");
			serviceLevelFilter.Category = FilterCategories.ModesAndTypes;
		}

		#endregion

		#region Other

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			var customsIndicatorDescriptions = new[]
			{
				Res.GetString("746cf2ab-2320-4d31-897e-f78706808fd0", "Is Perishable"),
				Res.GetString("3ecefc6a-0a9b-4df0-97ce-e20b761b1ee4", "Is Hazardous"),
				Res.GetString("1d124991-eed0-46d8-a46a-d40afb903099", "Is Personal Effects"),
				Res.GetString("4b97138b-d32e-4556-9356-73182021b39c", "Is Timber"),
				Res.GetString("46ae7103-9ea3-4858-a943-862a6dabc83c", "Requires Fumigation")
			};

			var customsIndicatorSchemaColumns = new[]
			{
				HVLVConsignmentSchema.HVC_IsPerishable,
				HVLVConsignmentSchema.HVC_IsHazardous,
				HVLVConsignmentSchema.HVC_IsPersonalEffects,
				HVLVConsignmentSchema.HVC_IsTimber,
				HVLVConsignmentSchema.HVC_RequiresFumigation
			};

			var createdInPortalFilter = filters.AddFlagsFilter(Descriptions.CreatedInPortal,
				new[] { Res.GetString("5fb97043-ed17-445b-a230-627480399de9", "Created in Portal") },
				new[] { (GetFlagsQuery)CreatedInPortalQuery });
			createdInPortalFilter.MultilingualDescription = ResString.GetMultilingualString("13fc8e18-8312-4f38-a721-e577edf88466", "Created in Portal");
			createdInPortalFilter.Category = FilterCategories.Other;
		}

		ZQuery CreatedInPortalQuery(ZBool flagValue)
		{
			var logsSubQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);
			logsSubQuery.AddToFilter(StmALogSchema.SL_Table, HVLVBookingHeaderSchema.Constants.TableName);
			logsSubQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.ELoadListConsolidatedCode);

			var headerQuery = new ZDBOnlyQuery(typeof(HVLVBookingHeader));
			headerQuery.AddSubQuery(logsSubQuery, JoinCondition.And);

			return headerQuery;
		}

		#endregion

		#region Lookups

		OrgHeaderCollection OrganisationsLookup => organisationsLookup ?? (organisationsLookup = new OrgHeaderCollection(Factory));
		OrgHeaderCollection organisationsLookup;

		OrgContactCollection ContactsLookup => contactsLookup ?? (contactsLookup = new OrgContactCollection(Factory));
		OrgContactCollection contactsLookup;

		RefServiceLevelCollection ServiceLevelLookup => serviceLevelLookup ?? (serviceLevelLookup = new RefServiceLevelCollection(Factory));
		RefServiceLevelCollection serviceLevelLookup;

		CodeDescriptionPairList ManifestStatusLookup => manifestStatusLookup ?? (manifestStatusLookup = HVLVBookingStatus.GetAll());
		CodeDescriptionPairList manifestStatusLookup;

		#endregion
	}
}
