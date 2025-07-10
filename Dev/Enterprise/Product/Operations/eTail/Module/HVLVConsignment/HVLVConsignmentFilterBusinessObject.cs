using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Module
{
	public class HVLVConsignmentFilterBusinessObject : FilterStripBusinessObject
	{
		public HVLVConsignmentFilterBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = AutoHVLVConsignment.Schema.TableName;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddNumbersAndReferencesFilters(filters);
			AddStatusAndFlagsFilters(filters);
			AddLocationFilters(filters);
			AddOrganisationFilters(filters);
			AddModeAndTypeFilters(filters);
			AddOtherFilters(filters);
			AddRelationalFilters(filters);

			return filters;
		}

		#region Descriptions

		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string ConsignmentId = "Consignment ID";
			public const string ItemId = "Item ID";
			public const string ConsignmentShipperReference = "Consignment Shipper Reference";
			public const string ItemShipperReference = "Item Shipper Reference";
			public const string ConsignmentWaybillNo = "Consignment Waybill #";
			public const string ItemCurrentBarcode = "Item Current Barcode";
			public const string ConsignmentAdditionalReference = "Consignment Additional Reference #";

			public const string ManifestStatus = "Manifest Status";
			public const string ConsignmentStatus = "Consignment Status";
			public const string LoadListStatus = "Load List Status";
			public const string ConsignmentReferencesValidated = "Consignment References Validated";
			public const string ItemReferencesValidated = "Item References Validated";
			public const string ConsignmentPreScreeningStatus = "Consignment Pre-Screening Status";
			public const string ExportCustomsClearanceStatus = "Export Customs Clearance Status";
			public const string ImportCustomsClearanceStatus = "Import Customs Clearance Status";
			public const string ReleaseStatus = "Release Status";
			public const string ImportReleaseStatus = "Import Release Status";
			public const string ExportReleaseStatus = "Export Release Status";
			public const string ItemCarrierBookingStatus = "Item Carrier Booking Status";

			public const string ConsigneeCity = "Consignee City";
			public const string ConsigneeState = "Consignee State";
			public const string ConsigneePostcode = "Consignee Postcode";
			public const string ConsigneeCountry = "Consignee Country";
			public const string ShipperCity = "Shipper City";
			public const string ShipperState = "Shipper State";
			public const string ShipperPostcode = "Shipper Postcode";
			public const string ShipperCountry = "Shipper Country";
			public const string ReturnCity = "Return City";
			public const string ReturnState = "Return State";
			public const string ReturnPostcode = "Return Postcode";
			public const string ReturnCountry = "Return Country";

			public const string BillToParty = "Bill To Party";
			public const string OriginDepot = "Origin Depot";
			public const string Dispatch = "Dispatch";
			public const string DestinationDepot = "Destination Depot";
			public const string Consignee = "Consignee";
			public const string Shipper = "Shipper";
			public const string ReturnName = "Return Name";
			public const string BookedBy = "Booked By";
			public const string LastMileCarrier = "Last Mile Carrier";

			public const string ServiceLevel = "Service Level";
			public const string LastMileCarrierServiceLevel = "Last Mile Carrier Service Level";

			public const string GoodsDescription = "Goods Description";
			public const string CustomsIndicators = "Customs Indicators";
			public const string UNDGClass = "UNDG Class";
			public const string SignatureRequired = "Signature Required";
			public const string SelfBooked = "Self-booked";
			public const string AuthorityToLeave = "Authority to Leave";
			public const string CreatedInPortal = "Created in Portal";
			public const string Confirmed = "Confirmed";

			public const string RelatedOriginLoadLists = "Related HVLV Load Lists";
			public const string RelatedShipments = "Related Shipments";
			public const string RelatedBrokerageJobs = "Related Brokerage Jobs";
			public const string Shipments = "Shipments";

			public const string ReceivedAtOrigin = "Received at Origin";

			#endregion
		}

		#endregion

		#region SubGroups
		public ModuleFilterSubGroup ConsignmentFilterProcessor => null;

		public ModuleFilterSubGroup ItemFilterProcessor => itemFilterProcessor ?? (itemFilterProcessor = new ItemSubGroup(ModuleFilterSubGroup.Default));
		ItemSubGroup itemFilterProcessor;

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

		#endregion

		#region Numbers and References

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			var consignmentIdFilter = filters.AddTextFilter(Descriptions.ConsignmentId, HVLVConsignmentSchema.HVC_ConsignmentId);
			consignmentIdFilter.MultilingualDescription = ResString.GetMultilingualString("92c65898-fb8f-4b85-8ebb-e8ee6f794fe4", "Consignment ID");
			consignmentIdFilter.SubGroup = ConsignmentFilterProcessor;
			consignmentIdFilter.Category = FilterCategories.NumbersAndReferences;

			var itemIdFilter = filters.AddTextFilter(Descriptions.ItemId, HVLVItemSchema.HVI_ItemId);
			itemIdFilter.MultilingualDescription = ResString.GetMultilingualString("c18b14e0-94af-42d4-9afe-6edc7767329b", "Item ID");
			itemIdFilter.SubGroup = ItemFilterProcessor;
			itemIdFilter.Category = FilterCategories.NumbersAndReferences;

			var shipperReferenceFilter = filters.AddTextFilter(Descriptions.ConsignmentShipperReference, HVLVConsignmentSchema.HVC_ShipperReference);
			shipperReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("3af60d1e-d139-4842-ad8f-4861a51903af", "Consignment Shipper Reference");
			shipperReferenceFilter.SubGroup = ConsignmentFilterProcessor;
			shipperReferenceFilter.Category = FilterCategories.NumbersAndReferences;

			var itemShipperReferenceFilter = filters.AddTextFilter(Descriptions.ItemShipperReference, HVLVItemSchema.HVI_ShipperReference);
			itemShipperReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("1bfe520d-a1b4-4748-9fab-1ae04e560f5e", "Item Shipper Reference");
			itemShipperReferenceFilter.SubGroup = ItemFilterProcessor;
			itemShipperReferenceFilter.Category = FilterCategories.NumbersAndReferences;

			var consignmentWaybillNumberFilter = filters.AddTextFilter(Descriptions.ConsignmentWaybillNo, HVLVConsignmentSchema.HVC_WaybillNumber);
			consignmentWaybillNumberFilter.MultilingualDescription = ResString.GetMultilingualString("f9e1261d-d899-4ef7-a9f5-056ca3d9791a", "Consignment Waybill #");
			consignmentWaybillNumberFilter.SubGroup = ConsignmentFilterProcessor;
			consignmentWaybillNumberFilter.Category = FilterCategories.NumbersAndReferences;

			var itemCurrentBarcodeFilter = filters.AddTextFilter(Descriptions.ItemCurrentBarcode, HVLVItemSchema.HVI_CurrentBarcode);
			itemCurrentBarcodeFilter.MultilingualDescription = ResString.GetMultilingualString("8d83fa01-b66c-4c91-8f2b-d61298e545db", "Item Current Barcode");
			itemCurrentBarcodeFilter.SubGroup = ItemFilterProcessor;
			itemCurrentBarcodeFilter.Category = FilterCategories.NumbersAndReferences;

			var referenceNumberFilter = new ReferenceNumberFilter(Descriptions.ConsignmentAdditionalReference,
				new ReferenceNumberFilterHelper<HVLVConsignment>().GetReferenceNumberFilter,
				new RefCountryCollection(Factory)).
				WithMaxLengthOf<ReferenceNumberFilter>(CusEntryNumSchema.CE_EntryNum);

			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("A70E83DD-5A3E-4BF7-9716-A61F5B6C0194", "Consignment Additional Reference #");
			referenceNumberFilter.SubGroup = ConsignmentFilterProcessor;
			referenceNumberFilter.Category = FilterCategories.NumbersAndReferences;

			filters.AddCustomFilter(referenceNumberFilter);
		}

		#endregion

		#region Status and Flags

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var consignmentStatusFilter = filters.AddTextFilter(Descriptions.ConsignmentStatus, HVLVConsignmentSchema.HVC_Status, ConsignmentStatusLookup);
			consignmentStatusFilter.MultilingualDescription = ResString.GetMultilingualString("435d1ac8-a1f2-41fa-8176-81c35f96ff36", "Consignment Status");
			consignmentStatusFilter.SubGroup = ConsignmentFilterProcessor;
			consignmentStatusFilter.Category = FilterCategories.StatusAndFlags;

			var preScreeningStatusFilter = filters.AddTextFilter(Descriptions.ConsignmentPreScreeningStatus, HVLVConsignmentSchema.HVC_PreScreeningStatus, ConsignmentPreScreeningStatusLookup);
			preScreeningStatusFilter.MultilingualDescription = ResString.GetMultilingualString("d332f8d1-623f-49a2-9cd2-4bec90789d31", "Consignment Pre-Screening Status");
			preScreeningStatusFilter.SubGroup = ConsignmentFilterProcessor;
			preScreeningStatusFilter.Category = FilterCategories.StatusAndFlags;

			var exportCustomsClearanceStatusFilter = filters.AddTextFilter(Descriptions.ExportCustomsClearanceStatus, HVLVConsignmentSchema.HVC_ExportCustomsClearanceStatus, ExportCustomsStatusLookup);
			exportCustomsClearanceStatusFilter.MultilingualDescription = ResString.GetMultilingualString("E89A3B94-24FB-4B1B-9C14-7C960F51404C", "Export Customs Clearance Status");
			exportCustomsClearanceStatusFilter.SubGroup = ConsignmentFilterProcessor;
			exportCustomsClearanceStatusFilter.Category = FilterCategories.StatusAndFlags;

			var importCustomsClearanceStatusFilter = filters.AddTextFilter(Descriptions.ImportCustomsClearanceStatus, HVLVConsignmentSchema.HVC_ImportCustomsClearanceStatus, ImportCustomsStatusLookup);
			importCustomsClearanceStatusFilter.MultilingualDescription = ResString.GetMultilingualString("F30AD0D4-1708-461C-881E-D3D905E7551B", "Import Customs Clearance Status");
			importCustomsClearanceStatusFilter.SubGroup = ConsignmentFilterProcessor;
			importCustomsClearanceStatusFilter.Category = FilterCategories.StatusAndFlags;

			var releaseStatusFilter = filters.AddTextFilter(Descriptions.ReleaseStatus, HVLVConsignmentSchema.HVC_ReleaseStatus, ReleaseStatusLookup);
			releaseStatusFilter.MultilingualDescription = ResString.GetMultilingualString("B367CB06-2B2B-4568-B433-E565A0EE727D", "Release Status");
			releaseStatusFilter.SubGroup = ConsignmentFilterProcessor;
			releaseStatusFilter.Category = FilterCategories.StatusAndFlags;

			var importReleaseStatusFilter = filters.AddTextFilter(Descriptions.ImportReleaseStatus, HVLVConsignmentSchema.HVC_ImportReleaseStatus, ReleaseStatusLookup);
			importReleaseStatusFilter.MultilingualDescription = ResString.GetMultilingualString("4edc91fe-a520-4f87-85f2-c292720c7e7d", "Import Release Status");
			importReleaseStatusFilter.SubGroup = ConsignmentFilterProcessor;
			importReleaseStatusFilter.Category = FilterCategories.StatusAndFlags;

			var exportReleaseStatusFilter = filters.AddTextFilter(Descriptions.ExportReleaseStatus, HVLVConsignmentSchema.HVC_ExportReleaseStatus, ReleaseStatusLookup);
			exportReleaseStatusFilter.MultilingualDescription = ResString.GetMultilingualString("e874edad-e445-4cd4-ab3f-67f8aced4c24", "Export Release Status");
			exportReleaseStatusFilter.SubGroup = ConsignmentFilterProcessor;
			exportReleaseStatusFilter.Category = FilterCategories.StatusAndFlags;

			var itemCarrierBookingStatusFilter = filters.AddTextFilter(Descriptions.ItemCarrierBookingStatus, HVLVItemSchema.HVI_CarrierBookingStatus, ItemCarrierBookingStatusLookup);
			itemCarrierBookingStatusFilter.MultilingualDescription = ResString.GetMultilingualString("c4d85dc9-3487-4e2b-9ea4-4fe7f618ee19", "Item Carrier Booking Status");
			itemCarrierBookingStatusFilter.SubGroup = ItemFilterProcessor;
			itemCarrierBookingStatusFilter.Category = FilterCategories.StatusAndFlags;

			var consignmentActiveFilter = filters.AddTextFilter("Active Status", ConsignmentActiveQuery, CancelledStatusList);
			consignmentActiveFilter.MultilingualDescription = ResString.GetMultilingualString("445e8fad-ccb6-4a62-9a6a-08088bfa8943", "Active Status");
			consignmentActiveFilter.Category = FilterCategories.StatusAndFlags;
			consignmentActiveFilter.Property = CancelledStatusList[0].Code;
			consignmentActiveFilter.Visibility = FilterVisibility.AlwaysApplied;

			var consignmentACASStatusFilter = filters.AddTextFilter("ACAS Status", ConsignmentACASQuery, HVLVConsignmentLookups.GetHVC_ACASStatus_List(Factory));
			consignmentACASStatusFilter.MultilingualDescription = ResString.GetMultilingualString("097d8305-6511-4cac-bf86-c0d081824b70", "ACAS Status");
			consignmentACASStatusFilter.Category = FilterCategories.StatusAndFlags;
		}

		ZQuery ConsignmentActiveQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.IgnoreActiveFilter = true;

			if (value == CancelledStatusList[0].Code)
			{
				query.AddToFilter(HVLVConsignmentSchema.HVC_IsActive, true);
			}
			else if (value == CancelledStatusList[1].Code)
			{
				query.AddToFilter(HVLVConsignmentSchema.HVC_IsActive, false);
			}

			return query;
		}

		ZQuery ConsignmentACASQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZQuery();
			query.AddToFilter(JoinCondition.And, HVLVConsignmentSchema.HVC_ACASStatus, value);

			return query;
		}

		#endregion

		#region Locations

		void AddLocationFilters(ModuleFilterCollection filters)
		{
			var consigneeCityFilter = filters.AddTextFilter(Descriptions.ConsigneeCity, HVLVConsignmentSchema.HVC_ConsigneeCity);
			consigneeCityFilter.MultilingualDescription = ResString.GetMultilingualString("c43cd9a8-a491-4b7c-a0ed-8780fea441c1", "Consignee City");
			consigneeCityFilter.SubGroup = ConsignmentFilterProcessor;
			consigneeCityFilter.Category = FilterCategories.Locations;

			var consigneeStateFilter = filters.AddTextFilter(Descriptions.ConsigneeState, HVLVConsignmentSchema.HVC_ConsigneeState);
			consigneeStateFilter.MultilingualDescription = ResString.GetMultilingualString("8263bb51-bb99-4bdd-bcf1-0a7dd71f8bbb", "Consignee State");
			consigneeStateFilter.SubGroup = ConsignmentFilterProcessor;
			consigneeStateFilter.Category = FilterCategories.Locations;

			var consigneePostcodeFilter = filters.AddTextFilter(Descriptions.ConsigneePostcode, HVLVConsignmentSchema.HVC_ConsigneePostcode);
			consigneePostcodeFilter.MultilingualDescription = ResString.GetMultilingualString("f1f05cd6-7d55-4733-97fc-d2d76362752b", "Consignee Postcode");
			consigneePostcodeFilter.SubGroup = ConsignmentFilterProcessor;
			consigneePostcodeFilter.Category = FilterCategories.Locations;

			var consigneeCountryFilter = filters.AddNkFilter(Descriptions.ConsigneeCountry, HVLVConsignmentSchema.HVC_RN_NKConsigneeCountryCode, ModuleIDs.RefCountry, CountriesLookup);
			consigneeCountryFilter.MultilingualDescription = ResString.GetMultilingualString("23ba10e8-5a1e-4bdd-84e3-1cdb27fc6d15", "Consignee Country/Region");
			consigneeCountryFilter.SubGroup = ConsignmentFilterProcessor;
			consigneeCountryFilter.Category = FilterCategories.Locations;

			var shipperCityFilter = filters.AddTextFilter(Descriptions.ShipperCity, HVLVConsignmentSchema.HVC_ShipperCity);
			shipperCityFilter.MultilingualDescription = ResString.GetMultilingualString("a1af42a5-c24f-4725-9fda-4b9f3d742340", "Shipper City");
			shipperCityFilter.SubGroup = ConsignmentFilterProcessor;
			shipperCityFilter.Category = FilterCategories.Locations;

			var shipperStateFilter = filters.AddTextFilter(Descriptions.ShipperState, HVLVConsignmentSchema.HVC_ShipperState);
			shipperStateFilter.MultilingualDescription = ResString.GetMultilingualString("d79c951d-16c6-4af2-ba99-2394476bae26", "Shipper State");
			shipperStateFilter.SubGroup = ConsignmentFilterProcessor;
			shipperStateFilter.Category = FilterCategories.Locations;

			var shipperPostcodeFilter = filters.AddTextFilter(Descriptions.ShipperPostcode, HVLVConsignmentSchema.HVC_ShipperPostcode);
			shipperPostcodeFilter.MultilingualDescription = ResString.GetMultilingualString("40f8507e-7641-4571-9bf1-ed818721e38a", "Shipper Postcode");
			shipperPostcodeFilter.SubGroup = ConsignmentFilterProcessor;
			shipperPostcodeFilter.Category = FilterCategories.Locations;

			var shipperCountryFilter = filters.AddNkFilter(Descriptions.ShipperCountry, HVLVConsignmentSchema.HVC_RN_NKShipperCountryCode, ModuleIDs.RefCountry, CountriesLookup);
			shipperCountryFilter.MultilingualDescription = ResString.GetMultilingualString("ca40752f-0dae-46ad-9e57-64f9df7b214f", "Shipper Country/Region");
			shipperCountryFilter.SubGroup = ConsignmentFilterProcessor;
			shipperCountryFilter.Category = FilterCategories.Locations;

			var returnCityFilter = filters.AddTextFilter(Descriptions.ReturnCity, HVLVConsignmentSchema.HVC_ReturnCity);
			returnCityFilter.MultilingualDescription = ResString.GetMultilingualString("55b0711d-7171-4af5-855e-b7a5398d3008", "Return City");
			returnCityFilter.SubGroup = ConsignmentFilterProcessor;
			returnCityFilter.Category = FilterCategories.Locations;

			var returnStateFilter = filters.AddTextFilter(Descriptions.ReturnState, HVLVConsignmentSchema.HVC_ReturnState);
			returnStateFilter.MultilingualDescription = ResString.GetMultilingualString("55f22354-2a07-43c4-ad34-010c59e34bb3", "Return State");
			returnStateFilter.SubGroup = ConsignmentFilterProcessor;
			returnStateFilter.Category = FilterCategories.Locations;

			var returnPostcodeFilter = filters.AddTextFilter(Descriptions.ReturnPostcode, HVLVConsignmentSchema.HVC_ReturnPostcode);
			returnPostcodeFilter.MultilingualDescription = ResString.GetMultilingualString("6d405470-f386-4ba0-a4a1-864537f4a5c1", "Return Postcode");
			returnPostcodeFilter.SubGroup = ConsignmentFilterProcessor;
			returnPostcodeFilter.Category = FilterCategories.Locations;

			var returnCountryFilter = filters.AddNkFilter(Descriptions.ReturnCountry, HVLVConsignmentSchema.HVC_RN_NKReturnCountryCode, ModuleIDs.RefCountry, CountriesLookup);
			returnCountryFilter.MultilingualDescription = ResString.GetMultilingualString("d38ec812-28ef-4c07-bace-ee4d89caab8b", "Return Country");
			returnCountryFilter.SubGroup = ConsignmentFilterProcessor;
			returnCountryFilter.Category = FilterCategories.Locations;
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

			var destinationDepotFilter = addOrgAddressFilter(Descriptions.DestinationDepot, HVLVConsignmentSchema.HVC_OA_DestinationDepot, typeof(HVLVConsignment));
			destinationDepotFilter.MultilingualDescription = ResString.GetMultilingualString("5c9eb9f5-3971-4004-bae5-63e85f3a451b", "Destination Depot");
			destinationDepotFilter.SubGroup = ConsignmentFilterProcessor;

			var consigneeFilter = filters.AddTextFilter(Descriptions.Consignee, HVLVConsignmentSchema.HVC_ConsigneeName);
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("f1c4787e-fead-488c-96da-7e14cd829758", "Consignee");
			consigneeFilter.SubGroup = ConsignmentFilterProcessor;
			consigneeFilter.Category = FilterCategories.Organisations;

			var shipperFilter = filters.AddTextFilter(Descriptions.Shipper, HVLVConsignmentSchema.HVC_ShipperName);
			shipperFilter.MultilingualDescription = ResString.GetMultilingualString("23182582-1e67-4917-8a21-1c4841fc2464", "Shipper");
			shipperFilter.SubGroup = ConsignmentFilterProcessor;
			shipperFilter.Category = FilterCategories.Organisations;

			var returnFilter = filters.AddTextFilter(Descriptions.ReturnName, HVLVConsignmentSchema.HVC_ReturnName);
			returnFilter.MultilingualDescription = ResString.GetMultilingualString("03c80991-52cd-475d-ab2e-088b9aab9116", "Return Name");
			returnFilter.SubGroup = ConsignmentFilterProcessor;
			returnFilter.Category = FilterCategories.Organisations;

			var lastMileCarrierFilter = filters.AddGuidFilter(Descriptions.LastMileCarrier, ModuleIDs.Organisation, HVLVConsignmentSchema.HVC_OH_LastMileCarrier, OrganisationsLookup);
			lastMileCarrierFilter.MultilingualDescription = ResString.GetMultilingualString("7d76fdbd-f9a1-4fcd-894c-4bef76c3fe07", "Last Mile Carrier");
			lastMileCarrierFilter.SubGroup = ConsignmentFilterProcessor;
			lastMileCarrierFilter.Category = FilterCategories.Organisations;
		}

		#endregion

		#region Modes and Types

		void AddModeAndTypeFilters(ModuleFilterCollection filters)
		{
			var lastMileCarrierServiceLevelFilter = filters.AddTextFilter(Descriptions.LastMileCarrierServiceLevel, HVLVConsignmentSchema.HVC_PL_NKLastMileCarrierServiceLevel, CarrierServiceLevelLookup);
			lastMileCarrierServiceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("f3cc5459-cd12-4d03-85a6-81c68eec40f8", "Last Mile Carrier Service Level");
			lastMileCarrierServiceLevelFilter.SubGroup = ConsignmentFilterProcessor;
			lastMileCarrierServiceLevelFilter.Category = FilterCategories.ModesAndTypes;
		}

		#endregion

		#region Other

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			var goodsDescriptionFilter = filters.AddTextFilter(Descriptions.GoodsDescription, HVLVConsignmentSchema.HVC_GoodsDescription);
			goodsDescriptionFilter.MultilingualDescription = ResString.GetMultilingualString("b33ce476-e649-487b-8e66-f789cc91d5a2", "Goods Description");
			goodsDescriptionFilter.SubGroup = ConsignmentFilterProcessor;
			goodsDescriptionFilter.Category = FilterCategories.Other;

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

			var customsIndicatorsFilter = filters.AddFlagsFilter(Descriptions.CustomsIndicators, customsIndicatorDescriptions, customsIndicatorSchemaColumns);
			customsIndicatorsFilter.MultilingualDescription = ResString.GetMultilingualString("4637843a-05ca-41db-973d-4c81c73e4aff", "Customs Indicators");
			customsIndicatorsFilter.SubGroup = ConsignmentFilterProcessor;
			customsIndicatorsFilter.Category = FilterCategories.Other;

			var undgClassFilter = filters.AddTextFilter(Descriptions.UNDGClass, HVLVConsignmentSchema.HVC_UndgClass, DGClassLookup);
			undgClassFilter.MultilingualDescription = ResString.GetMultilingualString("bd475f4d-3b23-4f94-b723-b28b2f0a47d7", "UNDG Class");
			undgClassFilter.SubGroup = ConsignmentFilterProcessor;
			undgClassFilter.Category = FilterCategories.Other;

			var signatureRequiredFilter = filters.AddFlagsFilter(Descriptions.SignatureRequired,
				new[] { Res.GetString("dd55e963-08c5-476f-a4f4-3d65d3efeaac", "Signature Required") },
				new[] { HVLVConsignmentSchema.HVC_IsSignatureRequired });
			signatureRequiredFilter.MultilingualDescription = ResString.GetMultilingualString("42c044c4-6df9-44ac-8b37-423f4eec31a2", "Signature Required");
			signatureRequiredFilter.SubGroup = ConsignmentFilterProcessor;
			signatureRequiredFilter.Category = FilterCategories.Other;

			var selfBookedFilter = filters.AddFlagsFilter(Descriptions.SelfBooked,
				new[] { Res.GetString("c56e0d97-2db6-44c1-8ab2-c9f14ae320df", "Self-booked") },
				 new[] { HVLVConsignmentSchema.HVC_IsSelfBooked });
			selfBookedFilter.MultilingualDescription = ResString.GetMultilingualString("183ebafd-bc5a-4a50-9570-7d581cf2c0f7", "Self-booked");
			selfBookedFilter.SubGroup = ConsignmentFilterProcessor;
			selfBookedFilter.Category = FilterCategories.Other;

			var authorityToLeaveFilter = filters.AddFlagsFilter(Descriptions.AuthorityToLeave,
				new[] { Res.GetString("a472fb0c-1e6c-41f1-9695-443a221899e9", "Authority to Leave") },
				new[] { HVLVConsignmentSchema.HVC_AuthorityToLeave });
			authorityToLeaveFilter.MultilingualDescription = ResString.GetMultilingualString("4e692054-26b4-4b6d-b0ac-192a0f525dd0", "Authority to Leave");
			authorityToLeaveFilter.SubGroup = ConsignmentFilterProcessor;
			authorityToLeaveFilter.Category = FilterCategories.Other;

			var consignmentReferencesValidatedFilter = filters.AddFlagsFilter(Descriptions.ConsignmentReferencesValidated,
				new[] { Res.GetString("658cbd23-11b7-4ade-a3cc-19f21e46af7e", "Consignment References Validated") },
				new[] { HVLVConsignmentSchema.HVC_IsValidatedForUniqueness });
			consignmentReferencesValidatedFilter.MultilingualDescription = ResString.GetMultilingualString("6a85d375-0d35-4570-802c-6b2b71fa0037", "Consignment References Validated");
			consignmentReferencesValidatedFilter.SubGroup = ConsignmentFilterProcessor;
			consignmentReferencesValidatedFilter.Category = FilterCategories.Other;

			var itemReferencesValidatedFilter = filters.AddFlagsFilter(Descriptions.ItemReferencesValidated,
				new[] { Res.GetString("89bfb49f-a8b0-48d5-ac5c-813c40fad61d", "Item References Validated") },
				new[] { HVLVItemSchema.HVI_IsValidatedForUniqueness });
			itemReferencesValidatedFilter.MultilingualDescription = ResString.GetMultilingualString("b446e8f1-5458-4872-80cb-9fc6df4e40d0", "Item References Validated");
			itemReferencesValidatedFilter.SubGroup = ItemFilterProcessor;
			itemReferencesValidatedFilter.Category = FilterCategories.Other;
		}

		void AddRelationalFilters(ModuleFilterCollection filters)
		{
			var relatedShipmentsFilter = filters.AddGuidFilter(Descriptions.RelatedShipments, ModuleIDs.JobShipment, HVLVItemSchema.HVI_JS_LoadedOnShipment, ShipmentsLookup);
			relatedShipmentsFilter.MultilingualDescription = ResString.GetMultilingualString("e79f870b-b6af-4156-ae87-27f3434184ed", "Related Shipments");
			relatedShipmentsFilter.SubGroup = ItemFilterProcessor;
			relatedShipmentsFilter.Category = FilterCategories.Other;

			var relatedBrokerageJobFilter = filters.AddGuidFilter(Descriptions.RelatedBrokerageJobs, ModuleIDs.Customs.JobDeclaration, JobDeclarationsQuery, JobDeclarationsLookup, JobDeclarationSchema.PK);
			relatedBrokerageJobFilter.IsBlankFilterQueryDelegate = GetConsignmentBlankDeclarationQuery;
			relatedBrokerageJobFilter.MultilingualDescription = ResString.GetMultilingualString("2b5607fa-d192-4f13-ac50-b6b7f050ab37", "Related Brokerage Jobs");
			relatedBrokerageJobFilter.SubGroup = ConsignmentFilterProcessor;
			relatedBrokerageJobFilter.Category = FilterCategories.Other;

			var relatedOriginLoadListsFilter = filters.AddGuidFilter(Descriptions.RelatedOriginLoadLists, ModuleIDs.HVLVOriginLoadList, HVLVItemSchema.HVI_HVL_LoadList, LoadListsLookup);
			relatedOriginLoadListsFilter.MultilingualDescription = ResString.GetMultilingualString("8ad110b5-0ef6-4682-9f51-f68a2d440f98", "Related HVLV Load Lists");
			relatedOriginLoadListsFilter.SubGroup = ItemFilterProcessor;
			relatedOriginLoadListsFilter.Category = FilterCategories.Other;

			var shipmentsFilter = filters.AddGuidFilter(Descriptions.Shipments, ModuleIDs.JobShipment, ShipmentsQuery, ShipmentsLookup);
			shipmentsFilter.MultilingualDescription = ResString.GetMultilingualString("ca9faf21-14f3-40ca-95f2-b6299172652d", "Shipments");
			shipmentsFilter.Category = FilterCategories.Other;
		}

		ZQuery ShipmentsQuery(ZGuid value)
		{
			var consignmentQuery = new ZDBOnlyQuery(typeof(HVLVConsignment));

			var consignmentHeaderQuery = new ZDBOnlySubQuery(typeof(HVLVConsignmentHeader), HVLVConsignmentHeaderSchema.PK);
			consignmentHeaderQuery.AddToFilter(HVLVConsignmentHeaderSchema.HCH_JS_Shipment, value);

			consignmentQuery.AddSubQuery(HVLVConsignmentSchema.HVC_HCH_Header, HVLVConsignmentHeaderSchema.PK, consignmentHeaderQuery, JoinCondition.And);
			return consignmentQuery;
		}

		ZQuery JobDeclarationsQuery(ZDBOnlySubQuery filterQuery, SQLComparisonOperator comparisonOperator, object value)
		{
			var consignmentQuery = new ZDBOnlyQuery(typeof(HVLVConsignment));
			if (filterQuery == null)
			{
				if (comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					var importNotEqualQuery = GetGuidNotEqualQuery(HVLVConsignmentSchema.HVC_JE_ImportDeclaration, value, typeof(HVLVConsignment));
					var exportNotEqualQuery = GetGuidNotEqualQuery(HVLVConsignmentSchema.HVC_JE_ExportDeclaration, value, typeof(HVLVConsignment));
					consignmentQuery.AddToFilter(importNotEqualQuery, JoinCondition.And);
					consignmentQuery.AddToFilter(exportNotEqualQuery, JoinCondition.And);
				}
				else if (comparisonOperator == SQLComparisonOperator.IsNotBlank)
				{
					var importNotBlankQuery = GetGuidBlankOrNotBlankQuery(HVLVConsignmentSchema.HVC_JE_ImportDeclaration, typeof(HVLVConsignment), false);
					var exportNotBlankQuery = GetGuidBlankOrNotBlankQuery(HVLVConsignmentSchema.HVC_JE_ExportDeclaration, typeof(HVLVConsignment), false);
					consignmentQuery.AddToFilter(importNotBlankQuery, JoinCondition.Or);
					consignmentQuery.AddToFilter(exportNotBlankQuery, JoinCondition.Or);
				}
				else if(comparisonOperator == SQLComparisonOperator.Equal)
				{
					consignmentQuery.AddToFilter(JoinCondition.Or, HVLVConsignmentSchema.HVC_JE_ImportDeclaration, comparisonOperator, value);
					consignmentQuery.AddToFilter(JoinCondition.Or, HVLVConsignmentSchema.HVC_JE_ExportDeclaration, comparisonOperator, value);
				}
			}
			else
			{
				consignmentQuery.AddSubQuery(HVLVConsignmentSchema.HVC_JE_ImportDeclaration, filterQuery, JoinCondition.Or);
				consignmentQuery.AddSubQuery(HVLVConsignmentSchema.HVC_JE_ExportDeclaration, filterQuery, JoinCondition.Or);
			}

			return consignmentQuery;
		}

		ZQuery GetGuidNotEqualQuery(SchemaGuidColumn guidColumn, object value, Type queryType)
		{
			var guidNotEqualQuery = new ZDBOnlyQuery(queryType);
			guidNotEqualQuery.AddToFilter(guidColumn, SQLComparisonOperator.Equal, null);
			guidNotEqualQuery.AddToFilter(JoinCondition.Or, guidColumn, SQLComparisonOperator.NotEqual, value);
			return guidNotEqualQuery;
		}

		ZQuery GetConsignmentBlankDeclarationQuery()
		{
			var consignmentQuery = new ZDBOnlyQuery(typeof(HVLVConsignment));
			var importBlankQuery = GetGuidBlankOrNotBlankQuery(HVLVConsignmentSchema.HVC_JE_ImportDeclaration, typeof(HVLVConsignment), true);
			var exportBlankQuery = GetGuidBlankOrNotBlankQuery(HVLVConsignmentSchema.HVC_JE_ExportDeclaration, typeof(HVLVConsignment), true);
			consignmentQuery.AddToFilter(importBlankQuery, JoinCondition.And);
			consignmentQuery.AddToFilter(exportBlankQuery, JoinCondition.And);

			return consignmentQuery;
		}

		ZQuery GetGuidBlankOrNotBlankQuery(SchemaGuidColumn guidColumn, Type queryType, bool blank)
		{
			var guidBlankQuery = new ZDBOnlyQuery(queryType);
			SQLComparisonOperator comparisonOperator;
			JoinCondition joinCondition;
			if (blank)
			{
				comparisonOperator = SQLComparisonOperator.Equal;
				joinCondition = JoinCondition.Or;
			}
			else
			{
				comparisonOperator = SQLComparisonOperator.NotEqual;
				joinCondition = JoinCondition.And;
			}

			guidBlankQuery.AddToFilter(guidColumn, comparisonOperator, null);
			guidBlankQuery.AddToFilter(joinCondition, guidColumn, comparisonOperator, ZGuid.Empty);
			return guidBlankQuery;
		}

		#endregion

		#region Lookups

		RefCountryCollection CountriesLookup => countriesLookup ?? (countriesLookup = new RefCountryCollection(Factory));
		RefCountryCollection countriesLookup;

		OrgHeaderCollection OrganisationsLookup => organisationsLookup ?? (organisationsLookup = new OrgHeaderCollection(Factory));
		OrgHeaderCollection organisationsLookup;

		OrgCarrierServiceLevelCollection CarrierServiceLevelLookup => carrierServiceLevelLookup ?? (carrierServiceLevelLookup = new OrgCarrierServiceLevelCollection(Factory));
		OrgCarrierServiceLevelCollection carrierServiceLevelLookup;

		CodeDescriptionPairList DGClassLookup => dgClassLookup ?? (dgClassLookup = UNDGDataItemLookups.GetDGClassList(Factory));
		CodeDescriptionPairList dgClassLookup;

		ForwardingShipmentCollection ShipmentsLookup => shipmentsLookup ?? (shipmentsLookup = new ForwardingShipmentCollection(Factory));
		ForwardingShipmentCollection shipmentsLookup;

		BaseJobDeclarationCollection JobDeclarationsLookup => jobDeclarationsLookup ??= new BaseJobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
		BaseJobDeclarationCollection jobDeclarationsLookup;

		HVLVOriginLoadListCollection LoadListsLookup => loadListsLookup ?? (loadListsLookup = new HVLVOriginLoadListCollection(Factory));
		HVLVOriginLoadListCollection loadListsLookup;

		HVLVConsignmentPreScreeningStatusCodes ConsignmentPreScreeningStatusLookup => consignmentPreScreeningStatusLookup ?? (consignmentPreScreeningStatusLookup = new HVLVConsignmentPreScreeningStatusCodes());
		HVLVConsignmentPreScreeningStatusCodes consignmentPreScreeningStatusLookup;

		CodeDescriptionPairList ImportCustomsStatusLookup => importCustomsStatusLookup ?? (importCustomsStatusLookup = HVLVConsignmentLookups.GetHVC_ImportCustomsStatus_List(Factory));
		CodeDescriptionPairList importCustomsStatusLookup;

		CodeDescriptionPairList ExportCustomsStatusLookup => exportCustomsStatusLookup ?? (exportCustomsStatusLookup = HVLVConsignmentLookups.GetHVC_ExportCustomsStatus_List(Factory));
		CodeDescriptionPairList exportCustomsStatusLookup;

		CodeDescriptionPairList ConsignmentStatusLookup => consignmentStatusLookup ?? (consignmentStatusLookup = HVLVConsignmentLookups.GetAllHVLVConsignmentStatuses());
		CodeDescriptionPairList consignmentStatusLookup;

		CodeDescriptionPairList ReleaseStatusLookup => releaseStatusLookup ?? (releaseStatusLookup = HVLVReleaseStatus.GetAll());
		CodeDescriptionPairList releaseStatusLookup;

		CodeDescriptionPairList ItemCarrierBookingStatusLookup => itemCarrierBookingStatusLookup ?? (itemCarrierBookingStatusLookup = HVLVItemLookups.GetAllHVLVItemCarrierBookingStatuses());
		CodeDescriptionPairList itemCarrierBookingStatusLookup;

		#endregion
	}
}
