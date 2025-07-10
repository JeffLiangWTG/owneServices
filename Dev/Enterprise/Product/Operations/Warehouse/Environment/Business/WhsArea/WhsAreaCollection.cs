using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	[ModuleID(ModuleId.WhsConfigArea)]
	public class WhsAreaCollection : ActiveBusinessObjectCollection<WhsArea>, IWhsAreaCollection
	{
		#region FilterConstants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Filter constant")]
		public static class FilterConstants
		{
			public const string AreaName = "Area Name";
			public const string AreaType = "Area Type";
			public const string Warehouse = "Warehouse";
			public const string IsPickingArea = "Is Pick Area";
			public const string IsPutawayArea = "Is Putaway Area";
		}

		#endregion

		public WhsAreaCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsAreaCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		public static WhsAreaCollection GetPickingAreas(BusinessObjectFactory factory, ZGuid warehousePK)
		{
			return factory.GetCachedValue("WhsAreaCollection|GetPickingAreas|" + (warehousePK.IsEmpty ? "" : warehousePK.ToString()), () =>
			{
				var master = warehousePK.IsEmpty ? null : factory.Load<WhsWarehouse>(warehousePK);
				var areas = new WhsAreaCollection(factory, master, new ZQuery(WhsAreaSchema.WA_IsPickingArea, true), WhsAreaSchema.WA_WW_Whs);
				areas.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.IsPickingArea, "Property0", ZBool.True, false));
				return areas;
			});
		}

		public static WhsAreaCollection GetPutawayAreas(BusinessObjectFactory factory, ZGuid warehousePK)
		{
			return factory.GetCachedValue("WhsAreaCollection|GetPutawayAreas|" + (warehousePK.IsEmpty ? "" : warehousePK.ToString()), () =>
			{
				var master = warehousePK.IsEmpty ? null : factory.Load<WhsWarehouse>(warehousePK);
				var areas = new WhsAreaCollection(factory, master, new ZQuery(WhsAreaSchema.WA_IsPutawayArea, true), WhsAreaSchema.WA_WW_Whs);
				areas.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.IsPutawayArea, "Property0", ZBool.True, false));
				return areas;
			});
		}

		protected WhsAreaCollection(BusinessObjectFactory factory, BusinessObject master, ZQuery filter, SchemaColumn relationshipColumn)
			: base(factory, master, filter, relationshipColumn)
		{
			AddFilterBusinessObjectDefaults(FilterBusinessObjectDefaults, master);
		}

		public WhsAreaCollection(BusinessObjectFactory factory, WhsWarehouse master)
			: base(factory, master, null, WhsAreaSchema.WA_WW_Whs)
		{
			AddFilterBusinessObjectDefaults(FilterBusinessObjectDefaults, Relationship.Master);
		}

		public WhsAreaCollection(BusinessObjectFactory factory, AdhocCollectionRelationship adHoc)
			: base(factory, adHoc)
		{
		}

		static void AddFilterBusinessObjectDefaults(FilterBusinessObjectDefaults filterDefaults, BusinessObject master)
		{
			if (master != null)
			{
				filterDefaults.Add(new FilterBusinessObjectDefault(FilterConstants.Warehouse, "Property", master.PK, false));
			}
		}
	}
}
