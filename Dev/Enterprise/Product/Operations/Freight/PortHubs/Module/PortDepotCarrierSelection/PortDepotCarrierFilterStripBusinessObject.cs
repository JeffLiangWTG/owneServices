using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.PortHubs.Module
{
	public class PortDepotCarrierFilterStripBusinessObject : FilterStripBusinessObject
	{
		#region Description Constants

		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string Direction = "Direction";
			public const string ServiceLevel = "Service Level";
			public const string PackType = "Pack Type";
			public const string UndgClass = "UNDG Class";
			public const string OriginDepot = "Origin Depot";
			public const string DestinationDepot = "Destination Depot";
			public const string ProcessType = "Process Type";
			public const string MasterHouse = "Is Master House";
			public const string TransportMode = "Transport Mode";
			public const string Shipper = "Shipper";
			public const string Carrier = "Carrier";
			public const string WeightMin = "Weight (Min)";
			public const string WeightMax = "Weight (Max)";
			public const string WeightUnit = "Weight Unit";
			public const string VolumeMin = "Volume (Min)";
			public const string VolumeMax = "Volume (Max)";
			public const string VolumeUnit = "Volume Unit";

			#endregion
		}

		#endregion

		public PortDepotCarrierFilterStripBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = ModuleIDs.PortHubSelection.Name;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var collection = new ModuleFilterCollection();
			AddStatusAndFlagsFilters(collection);
			AddOrganisationFilters(collection);
			AddOtherFilters(collection);
			return collection;
		}

		#region Status And Flags Filters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var directionFilter = filters.AddTextFilter(Descriptions.Direction, PortHubSelectionSchema.TY_Direction, new PortHubSelectionDirectionList());
			directionFilter.Category = FilterCategories.StatusAndFlags;
			directionFilter.MultilingualDescription = ResString.GetMultilingualString("d260fa6e-412f-4708-ba2f-e3df8d9e24eb", "Direction");

			var seriveLevelFilter = filters.AddTextFilter(Descriptions.ServiceLevel, PortHubSelectionSchema.TY_RS_NKServiceLevel, ServiceLevels);
			seriveLevelFilter.Category = FilterCategories.StatusAndFlags;
			seriveLevelFilter.MultilingualDescription = ResString.GetMultilingualString("616c34bd-4733-4f2b-ac92-4ce584e48789", "Service Level");

			var packTypeFilter = filters.AddTextFilter(Descriptions.PackType, PortHubSelectionSchema.TY_F3_NKPackType, PackTypes);
			packTypeFilter.Category = FilterCategories.StatusAndFlags;
			packTypeFilter.MultilingualDescription = ResString.GetMultilingualString("1d013dde-a66d-4650-9db1-7a703f2cb2eb", "Pack Type");

			var transportModeFilter = filters.AddTextFilter(Descriptions.TransportMode, PortHubSelectionSchema.TY_RatingFreightMode, new TransportModeList());
			transportModeFilter.Category = FilterCategories.StatusAndFlags;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("7931ca0b-731d-42d5-a5c6-485a93785ea7", "Transport Mode");

			var undgClassFilter = filters.AddTextFilter(Descriptions.UndgClass, PortHubSelectionSchema.TY_UndgClass, DGClassList);
			undgClassFilter.Category = FilterCategories.StatusAndFlags;
			undgClassFilter.MultilingualDescription = ResString.GetMultilingualString("4dfdada2-d954-4a68-8a30-9b3703bf97ca", "UNDG Class");

			var processTypeFilter = filters.AddTextFilter(Descriptions.ProcessType, PortHubSelectionSchema.TY_ProcessType, new PortHubSelectionProcessTypeList());
			processTypeFilter.Category = FilterCategories.StatusAndFlags;
			processTypeFilter.MultilingualDescription = ResString.GetMultilingualString("e384ac62-b821-49e3-98c6-ca941f8b0cd4", "Process Type");

			var masterHouseFilter = filters.AddFlagFilter(Descriptions.MasterHouse, Res.GetString("3a46cc5d-e46f-4901-b1c2-3e49fe2f9f37", "Is Master House"), PortHubSelectionSchema.TY_IsMasterHouse, ModuleFilterSubGroup.Default);
			masterHouseFilter.Category = FilterCategories.StatusAndFlags;
			masterHouseFilter.MultilingualDescription = ResString.GetMultilingualString("d370ec4a-4aec-4d8c-835e-f5eba0bd5435", "Is Master House");
		}

		#endregion

		#region Organization Filters

		void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			Func<string, Type, SchemaGuidColumn, SchemaColumn, BusinessObjectCollection, ModuleGuidFilter> addOrgFilter = (name, subQueryType, subQueryColumn, parentColumn, lookupList) =>
			{
				GetGuidQuery queryDelegate = orgHeaderPK =>
				{
					var subQuery = new ZDBOnlySubQuery(subQueryType, subQueryColumn);
					subQuery.AddToFilter(parentColumn, orgHeaderPK);

					var parentQuery = new ZDBOnlyQuery(typeof(PortHubSelection));
					parentQuery.AddSubQuery(subQuery, JoinCondition.And);
					return parentQuery;
				};

				var filter = filters.AddGuidFilter(name, ModuleIDs.Organisation, queryDelegate, lookupList);
				filter.Category = FilterCategories.Organisations;
				return filter;
			};

			var originDepotFilter = addOrgFilter(Descriptions.OriginDepot, typeof(OrgAddress), PortHubSelectionSchema.TY_OA_DispatchDepotAddress, OrgAddressSchema.OA_OH, OrganisationsLookup);
			originDepotFilter.MultilingualDescription = ResString.GetMultilingualString("f33a50e0-ecea-41a9-a62e-2f58ac3c9138", "Origin Depot");

			var destinationDepotFilter = addOrgFilter(Descriptions.DestinationDepot, typeof(OrgAddress), PortHubSelectionSchema.TY_OA_DepotAddress, OrgAddressSchema.OA_OH, OrganisationsLookup);
			destinationDepotFilter.MultilingualDescription = ResString.GetMultilingualString("88114442-c96a-4ca5-b663-de99f930daf1", "Destination Depot");

			var shipperFilter = addOrgFilter(Descriptions.Shipper, typeof(OrgAddress), PortHubSelectionSchema.TY_OA_ShipperAddress, OrgAddressSchema.OA_OH, ConsignorsLookUp);
			shipperFilter.MultilingualDescription = ResString.GetMultilingualString("22d49318-be79-42e4-a144-f099911f328a", "Shipper");

			var carrierFilter = addOrgFilter(Descriptions.Carrier, typeof(OrgHeader), PortHubSelectionSchema.TY_OH_Carrier, OrgHeaderSchema.PK, CarriersLookup);
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("32e485a9-e5a9-4ed4-b865-97e7f438d07d", "Carrier");
		}

		#endregion

		#region Other Filters

		void AddOtherFilters(ModuleFilterCollection filters)
		{
			var weightUnitFilter = filters.AddTextFilter(Descriptions.WeightUnit, PortHubSelectionSchema.TY_WeightUQ, WeightUnits);
			weightUnitFilter.Category = FilterCategories.Other;
			weightUnitFilter.MultilingualDescription = ResString.GetMultilingualString("6edf3f16-ef76-464c-8217-101149d79873", "Weight Unit");

			var weightMaxFilter = filters.AddNumberRangeFilter(Descriptions.WeightMax, PortHubSelectionSchema.TY_MaxWeight);
			weightMaxFilter.Category = FilterCategories.Other;
			weightMaxFilter.MultilingualDescription = ResString.GetMultilingualString("aaf454a2-48ba-47da-96a0-81fd020e01c7", "Weight (Max)");

			var weightMinFilter = filters.AddNumberRangeFilter(Descriptions.WeightMin, PortHubSelectionSchema.TY_MinWeight);
			weightMinFilter.Category = FilterCategories.Other;
			weightMinFilter.MultilingualDescription = ResString.GetMultilingualString("ea61bcc1-7d75-4599-901e-a3addac3b5dd", "Weight (Min)");

			var volumeUnitFilter = filters.AddTextFilter(Descriptions.VolumeUnit, PortHubSelectionSchema.TY_VolumeUQ, VolumeUnits);
			volumeUnitFilter.Category = FilterCategories.Other;
			volumeUnitFilter.MultilingualDescription = ResString.GetMultilingualString("7a6150e4-7f21-457a-b4c5-613adf198569", "Volume Unit");

			var volumeMaxFilter = filters.AddNumberRangeFilter(Descriptions.VolumeMax, PortHubSelectionSchema.TY_MaxVolume);
			volumeMaxFilter.Category = FilterCategories.Other;
			volumeMaxFilter.MultilingualDescription = ResString.GetMultilingualString("6a882483-a1cd-45c3-88d0-e19e607dfe74", "Volume (Max)");

			var volumeMinFilter = filters.AddNumberRangeFilter(Descriptions.VolumeMin, PortHubSelectionSchema.TY_MinVolume);
			volumeMinFilter.Category = FilterCategories.Other;
			volumeMinFilter.MultilingualDescription = ResString.GetMultilingualString("3a46f9e6-7245-494a-8ecf-0d913f2b9b83", "Volume (Min)");
		}

		CodeDescriptionPairList WeightUnits => Factory.GetCachedValue("PortDepotCarrierFilterStripBusinessObject|WeightUnits", () => new CodeDescriptionPairList(OLookUpEditType.Weight));

		CodeDescriptionPairList VolumeUnits => Factory.GetCachedValue("PortDepotCarrierFilterStripBusinessObject|VolumeUnits", () => new CodeDescriptionPairList(OLookUpEditType.Volume));

		#endregion

		#region Lists

		CodeDescriptionPairList DGClassList
		{
			get
			{
				if (dgClassList == null)
				{
					dgClassList = new CodeDescriptionPairList(UNDGDataItemLookups.GetDGClassList(Factory));
					dgClassList.AddPair(PortHubSelection.All, Res.GetString("9aad513a-fc7b-4fc3-aeb3-a269619f31c3", "All"));
				}

				return dgClassList;
			}
		}

		CodeDescriptionPairList dgClassList;

		RefServiceLevelCollection ServiceLevels
		{
			get { return servicelevels ?? (servicelevels = new RefServiceLevelCollection(Factory)); }
		}

		RefServiceLevelCollection servicelevels;

		public RefPackTypeCollection PackTypes
		{
			get { return packTypes ?? (packTypes = new RefPackTypeCollection(Factory)); }
		}

		RefPackTypeCollection packTypes;

		#endregion

		#region Lookups

		OrgHeaderCollection ConsignorsLookUp => consignorsLookup ?? (consignorsLookup = new ConsignorCollection(Factory));
		OrgHeaderCollection consignorsLookup;

		OrgHeaderCollection CarriersLookup => carriersLookup ?? (carriersLookup = new CarrierCollection(Factory));
		OrgHeaderCollection carriersLookup;

		OrgHeaderCollection OrganisationsLookup => organisationsLookup ?? (organisationsLookup = new OrgHeaderCollection(Factory));
		OrgHeaderCollection organisationsLookup;

		#endregion
	}
}
