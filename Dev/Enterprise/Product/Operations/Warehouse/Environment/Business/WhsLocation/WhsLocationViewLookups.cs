using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsLocationViewLookups : AutoWhsLocationViewLookups
	{
		public WhsLocationViewLookups(AutoWhsLocationView parent)
			: base(parent)
		{
		}

		#region ParentLocn

		protected WhsLocation ParentLocn => (WhsLocation)Parent;

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get
			{
				return Factory.GetCachedValue("LocationCollection",
					() => new LocationCollection(Factory,
						new ZoneTypeList { ZoneTypeCodeDescriptionPair.TransitWarehouse, ZoneTypeCodeDescriptionPair.All }));
			}
		}

		#endregion

		#region PickingAreas

		public WhsAreaCollection PickingAreas => WhsAreaCollection.GetPickingAreas(Factory, ParentLocn.WLV_WW_Whs);

		#endregion

		#region PutawayAreas

		public WhsAreaCollection PutawayAreas => WhsAreaCollection.GetPutawayAreas(Factory, ParentLocn.WLV_WW_Whs);

		#endregion

		#region AreaList

		public CodeDescriptionPairList PickingAreaList => Factory.GetCachedValue($"PickingAreaList|{ParentLocn.WLV_WW_Whs}", () => GetAreaList(PickingAreas));
		public CodeDescriptionPairList PutawayAreaList => Factory.GetCachedValue($"PutawayAreaList|{ParentLocn.WLV_WW_Whs}", () => GetAreaList(PutawayAreas));

		CodeDescriptionPairList GetAreaList(WhsAreaCollection areas)
		{
			var list = new CodeDescriptionPairList();
			foreach (var area in areas)
			{
				list.AddPair(area.PK, area.WA_NameMultilingual.ToString(), area.WA_NameMultilingual);
			}
			return list;
		}

		#endregion

		#region WeightUnits

		public CodeDescriptionPairList WeightUnits => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		#endregion

		#region CubicUnits

		public CodeDescriptionPairList CubicUnits => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);

		#endregion

		#region DimensionUnits

		public CodeDescriptionPairList DimensionUnits => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length);

		#endregion

		#region LocationTypes

		public WhsLocationTypeCollection LocationTypes => Factory.GetCachedValue("WhsLocationTypeCollection", () => new WhsLocationTypeCollection(Factory));

		#endregion

		#region TransitServiceLevels

		public override RefServiceLevelCollection TransitServiceLevels => Factory.GetCachedValue("WhsLocationViewLookups|TransitServiceLevels", () => new ActiveServiceLevelCollection(Factory));

		#endregion

		#region ApprovedKnownStatuses

		public CodeDescriptionPairList ApprovedKnownStatuses => ApprovedKnownStatusesCore;

		protected virtual CodeDescriptionPairList ApprovedKnownStatusesCore => Factory.GetCachedValue("WhsLocationViewLookups|ApprovedKnownStatuses", () => new ApprovedKnownStatus());

		#endregion
	}
}
