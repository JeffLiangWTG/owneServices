using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Yard.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Yard.Module
{
	public abstract class CYDFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string Warehouse = "Warehouse";
			public const string TransportationReference = "TransportationReference";
			public const string JobNumber = "JobNumber";
			public const string UnitNumber = "UnitNumber";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();
			AddWarehouseFilter(result);
			return result;
		}

		#region Warehouse Filter

		protected abstract SchemaGuidColumn WarehouseFKSchemaColumn { get; }

		void AddWarehouseFilter(ModuleFilterCollection filters)
		{
			var warehouseCollection = new WhsWarehouseCollection(Factory, WarehouseCollectionType.CYDWarehouse);
			var filter = filters.AddGuidFilter(Schema.Warehouse, ModuleIDs.WhsConfigWarehouse, WarehouseFKSchemaColumn, warehouseCollection);
			filter.MultilingualDescription = ResString.GetMultilingualString("ContainerYard|CYDFilterBusinessObject|Warehouse", "Warehouse");
			filter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
			filter.ComparisonOperator = ModuleGuidFilter.ComparisonConstants.Exact;
			filter.DefaultProperty = ContainerYardHelper.GetContainerYardInCurrentBranch(Factory)?.PK ?? ZGuid.Empty;
			filter.Category = FilterCategories.Other;
		}

		#endregion
	}
}
