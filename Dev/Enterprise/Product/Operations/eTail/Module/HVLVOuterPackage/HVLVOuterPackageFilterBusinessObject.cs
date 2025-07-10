using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.Module
{
	public class HVLVOuterPackageFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			AddNumbersAndReferencesFilters(filters);
			AddOrganisationFilters(filters);
			AddModeAndTypeFilters(filters);
			AddStatusFilters(filters);
			AddOtherFilters(filters);
			return filters;
		}

		public static class Descriptions
		{
			public static readonly string BarcodeNumber = nameof(BarcodeNumber);
			public static readonly string ContainerNumber = nameof(ContainerNumber);
			public static readonly string ReferenceNumber = nameof(ReferenceNumber);
			public static readonly string LastMileCarrier = nameof(LastMileCarrier);
			public static readonly string LoadedOnConsol = nameof(LoadedOnConsol);
			public static readonly string Status = nameof(Status);
			public static readonly string DestinationDepot = nameof(DestinationDepot);
			public static readonly string LastMileCarrierServiceLevel = nameof(LastMileCarrierServiceLevel);
			public static readonly string CommodityCode = nameof(CommodityCode);
		}

		#region Numbers and References

		void AddNumbersAndReferencesFilters(ModuleFilterCollection filters)
		{
			var barcodeNumberFilter = filters.AddTextFilter(Descriptions.BarcodeNumber, HVLVOuterPackageSchema.HVO_PackageBarcode);
			barcodeNumberFilter.MultilingualDescription = ResString.GetMultilingualString("f1aea4a8-eeba-4383-98a1-4a9b81afd3c5", "Barcode #");
			barcodeNumberFilter.MaxLength = HVLVOuterPackageSchema.HVO_PackageBarcode.MaxLength;
			barcodeNumberFilter.Category = FilterCategories.NumbersAndReferences;

			var containerNumberFilter = filters.AddTextFilter(Descriptions.ContainerNumber, HVLVOuterPackageSchema.HVO_ContainerNumber);
			containerNumberFilter.MultilingualDescription = ResString.GetMultilingualString("793d0f3f-06a3-4c50-be23-36f284ddb841", "Container #");
			containerNumberFilter.MaxLength = HVLVOuterPackageSchema.HVO_ContainerNumber.MaxLength;
			containerNumberFilter.Category = FilterCategories.NumbersAndReferences;

			var referenceNumberFilter = filters.AddTextFilter(Descriptions.ReferenceNumber, HVLVOuterPackageSchema.HVO_PackageReference);
			referenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("245f2750-cbe8-4a26-8b65-4e7ee28407b2", "Reference #");
			referenceNumberFilter.MaxLength = HVLVOuterPackageSchema.HVO_PackageReference.MaxLength;
			referenceNumberFilter.Category = FilterCategories.NumbersAndReferences;

			var commodityCodeFilter = filters.AddNkFilter(Descriptions.CommodityCode, HVLVOuterPackageSchema.HVO_RH_NKCommodityCode, ModuleIDs.RefCommodityCode, CommodityCodesLookup);
			commodityCodeFilter.MultilingualDescription = ResString.GetMultilingualString("3eaec9fc-2a58-44bc-be95-b54ed03574b6", "Commodity Code");
			commodityCodeFilter.Category = FilterCategories.NumbersAndReferences;
		}

		#endregion

		#region Organisations

		public void AddOrganisationFilters(ModuleFilterCollection filters)
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

			var lastMileCarrierFilter = filters.AddGuidFilter(Descriptions.LastMileCarrier, ModuleIDs.Organisation, HVLVOuterPackageSchema.HVO_OH_LastMileCarrier, OrganisationsLookup);
			lastMileCarrierFilter.MultilingualDescription = ResString.GetMultilingualString("61d2efd8-3064-49a1-9e10-f5c272617ef6", "Last Mile Carrier");
			lastMileCarrierFilter.Category = FilterCategories.Organisations;

			var destinationDepotFilter = addOrgAddressFilter(Descriptions.DestinationDepot, HVLVOuterPackageSchema.HVO_OA_DestinationDepot, typeof(HVLVOuterPackage));
			destinationDepotFilter.MultilingualDescription = ResString.GetMultilingualString("ec109ccf-6038-4dc4-9563-5f293b9a8bfc", "Destination Depot");
			destinationDepotFilter.Category = FilterCategories.Organisations;
		}

		#endregion

		#region Modes and Types

		public void AddModeAndTypeFilters(ModuleFilterCollection filters)
		{
			var lastMileCarrierServiceLevelFilter = filters.AddTextFilter(Descriptions.LastMileCarrierServiceLevel, HVLVOuterPackageSchema.HVO_PL_NKLastMileCarrierServiceLevel, CarrierServiceLevelLookup);
			lastMileCarrierServiceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("15942a1f-ee92-41ab-b97e-78fea9ad6f1c", "Last Mile Carrier Service Level");
			lastMileCarrierServiceLevelFilter.Category = FilterCategories.ModesAndTypes;
		}

		#endregion

		#region Status

		public void AddStatusFilters(ModuleFilterCollection filters)
		{
			var statusFilter = filters.AddTextFilter(Descriptions.Status, HVLVOuterPackageSchema.HVO_Status, HVLVOuterPackageLookups.GetAllHVLVOuterPackageStatus());
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("288747cc-ed63-434d-9967-9fe080f41edc", "Status");
			statusFilter.Category = FilterCategories.StatusAndFlags;
		}

		#endregion

		#region Other

		public void AddOtherFilters(ModuleFilterCollection filters)
		{
			var loadedOnConsolFilter = filters.AddGuidFilter(Descriptions.LoadedOnConsol, ModuleIDs.JobConsol, HVLVOuterPackageSchema.HVO_JK_LoadedOnConsol, ConsolsLookup);
			loadedOnConsolFilter.MultilingualDescription = ResString.GetMultilingualString("df195fea-d88e-49e0-9aee-7519fedc6b8f", "Loaded-On Consol");
			loadedOnConsolFilter.Category = FilterCategories.Other;
		}

		#endregion

		#region Lookups

		OrgHeaderCollection OrganisationsLookup => organisationsLookup ?? (organisationsLookup = new OrgHeaderCollection(Factory));
		OrgHeaderCollection organisationsLookup;

		ForwardingConsolCollection ConsolsLookup => consolsLookup ?? (consolsLookup = new ForwardingConsolCollection(Factory));
		ForwardingConsolCollection consolsLookup;

		OrgCarrierServiceLevelCollection CarrierServiceLevelLookup => carrierServiceLevelLookup ?? (carrierServiceLevelLookup = new OrgCarrierServiceLevelCollection(Factory));
		OrgCarrierServiceLevelCollection carrierServiceLevelLookup;

		RefCommodityCodeCollection CommodityCodesLookup => commodityCodesLookup ?? (commodityCodesLookup = new RefCommodityCodeCollection(Factory));
		RefCommodityCodeCollection commodityCodesLookup;

		#endregion
	}
}
