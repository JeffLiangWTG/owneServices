using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.PortHubs.GUI
{
	public class PortHubFilterStripBusinessObject : FilterStripBusinessObject
	{
		#region Description Constants

		public static class Descriptions
		{
			#region SuppressResourceStringsCheckRegion

			public const string Direction = "Direction";
			public const string ServiceLevel = "Service Level";
			public const string PackType = "Pack Type";
			public const string Mode = "Mode";
			public const string UndgClass = "UNDG Class";
			public const string OriginDepot = "Origin Depot";
			public const string DestinationDepot = "Destination Depot";

			#endregion
		}

		#endregion

		public PortHubFilterStripBusinessObject()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = ModuleIDs.PortHubSelection.Name;
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var collection = new ModuleFilterCollection();
			AddStatusAndFlagsFilters(collection);
			AddOrganisationFilters(collection);

			return collection;
		}

		#region Status And Flags Filters

		void AddStatusAndFlagsFilters(ModuleFilterCollection filters)
		{
			var directionFilter = filters.AddTextFilter(Descriptions.Direction, PortHubSelectionSchema.TY_Direction, new PortHubSelectionDirectionList());
			directionFilter.Category = FilterCategories.StatusAndFlags;
			directionFilter.MultilingualDescription = ResString.GetMultilingualString("0230e719-191a-493f-bb26-a42b78ce8cfd", "Direction");

			var serviceLevelFilter = filters.AddTextFilter(Descriptions.ServiceLevel, PortHubSelectionSchema.TY_RS_NKServiceLevel, ServiceLevels);
			serviceLevelFilter.Category = FilterCategories.StatusAndFlags;
			serviceLevelFilter.MultilingualDescription = ResString.GetMultilingualString("c2eabc40-e83e-4f5a-99c5-52980959e64a", "Service Level");

			var packTypeFilter = filters.AddTextFilter(Descriptions.PackType, PortHubSelectionSchema.TY_F3_NKPackType, PackTypes);
			packTypeFilter.Category = FilterCategories.StatusAndFlags;
			packTypeFilter.MultilingualDescription = ResString.GetMultilingualString("9548b881-891d-48c2-bd8b-7315a9eba639", "Pack Type");

			var modeFilter = filters.AddTextFilter(Descriptions.Mode, PortHubSelectionSchema.TY_RatingFreightMode, new TransportModeList());
			modeFilter.Category = FilterCategories.StatusAndFlags;
			modeFilter.MultilingualDescription = ResString.GetMultilingualString("433ac554-b9c2-449c-b411-9f5dc8dfd6d3", "Mode");

			var undgClassFilter = filters.AddTextFilter(Descriptions.UndgClass, PortHubSelectionSchema.TY_UndgClass, DGClassList);
			undgClassFilter.Category = FilterCategories.StatusAndFlags;
			undgClassFilter.MultilingualDescription = ResString.GetMultilingualString("a75f7508-d645-4d5c-951d-ba12ecf2a466", "UNDG Class");
		}

		#endregion

		#region Organization Filters

		public void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			var originDepotFilter = filters.AddGuidFilter(Descriptions.OriginDepot, ModuleIDs.Organisation, GetOriginDepotQuery, new OrgHeaderCollection(Factory));
			originDepotFilter.MultilingualDescription = ResString.GetMultilingualString("c3e0139d-8a94-461f-93b4-780f287b2c4a", "Origin Depot");

			var destinationDepotFilter = filters.AddGuidFilter(Descriptions.DestinationDepot, ModuleIDs.Organisation, GetDestinationDepotQuery, new OrgHeaderCollection(Factory));
			destinationDepotFilter.MultilingualDescription = ResString.GetMultilingualString("2a544f60-06af-44d9-85ba-4c3a3a2786f1", "Destination Depot");
		}

		#endregion

		#region Lists

		CodeDescriptionPairList DGClassList
		{
			get
			{
				if (dgClassList == null)
				{
					dgClassList = new CodeDescriptionPairList(UNDGDataItemLookups.GetDGClassList(Factory));
					dgClassList.AddPair(PortHubSelection.All, Res.GetString("adf360df-0323-4892-827e-9d8004666f24", "All"));
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

		ZQuery GetOriginDepotQuery(ZGuid originDepotPK)
		{
			return GetDepotAddressQuery(originDepotPK, PortHubSelectionSchema.TY_OA_DispatchDepotAddress);
		}

		ZQuery GetDestinationDepotQuery(ZGuid destinationDepotPK)
		{
			return GetDepotAddressQuery(destinationDepotPK, PortHubSelectionSchema.TY_OA_DepotAddress);
		}

		ZQuery GetDepotAddressQuery(ZGuid depotPK, SchemaColumn schemaColumn)
		{
			var query = new ZQuery();

			if (depotPK.IsValid)
			{
				var orgHeader = Factory.Load<OrgHeader>(depotPK);
				if (orgHeader != null)
				{
					query.AddToFilter(schemaColumn, orgHeader.Addresses.GetPKs());
				}
				else
				{
					query.IsNoResultQuery = true;
				}
			}

			return query;
		}
	}
}
