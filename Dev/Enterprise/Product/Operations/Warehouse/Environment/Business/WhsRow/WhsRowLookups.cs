using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsRowLookups : AutoWhsRowLookups
	{
		public WhsRowLookups(AutoWhsRow parent)
			: base(parent) { }

		#region ParentRow

		protected WhsRow ParentRow => (WhsRow)Parent;

		#endregion

		#region Warehouses

		public WhsWarehouseCollection Warehouses => Factory.GetCachedValue("WhsRowLookups|Warehouses", GetWarehouses);

		WhsWarehouseCollection GetWarehouses()
		{
			var result = new WhsWarehouseCollectionWithSecurityCheck(Factory);
			((IWhsWarehouseCollection)result).WarehouseCollectionType = WarehouseCollectionType.All;
			return result;
		}

		#endregion

		#region Areas

		public WhsAreaCollection PickingAreas => WhsAreaCollection.GetPickingAreas(Factory, ParentRow.WR_WW_Whs);

		public WhsAreaCollection PutawayAreas => WhsAreaCollection.GetPutawayAreas(Factory, ParentRow.WR_WW_Whs);

		#endregion

		#region LocationTypes

		public WhsLocationTypeCollection LocationTypes => Factory.GetCachedValue("WhsLocationTypeCollection", () => new WhsLocationTypeCollection(Factory));

		#endregion

		#region WeightUnitTypes

		public CodeDescriptionPairList WeightUnitTypes => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		#endregion

		#region CubicUnitTypes

		public CodeDescriptionPairList CubicUnitTypes => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume);

		#endregion

		#region DimensionUnitTypes

		public CodeDescriptionPairList DimensionUnitTypes => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Length);

		#endregion

		#region ApprovedKnownStatuses

		public virtual CodeDescriptionPairList ApprovedKnownStatuses => new CodeLists.ApprovedKnownStatus();

		#endregion

		#region SortPathMethods

		public CodeDescriptionPairList SortPathMethods => new CodeLists.SortPathMethods();

		#endregion
	}
}
