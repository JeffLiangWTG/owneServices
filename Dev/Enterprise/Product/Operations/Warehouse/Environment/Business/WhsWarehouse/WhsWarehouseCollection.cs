using CargoWise.EntityFramework;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	[ModuleID(ModuleId.WhsConfigWarehouse)]
	public class WhsWarehouseCollection : BusinessObjectCollection<WhsWarehouse>, IWhsWarehouseCollection
	{
		public WhsWarehouseCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WhsWarehouseCollection(BusinessObjectFactory factory, WarehouseCollectionType collectionType)
			: base(factory)
		{
			this.collectionType = collectionType;
		}

		public WhsWarehouseCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}

		#region Filter

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			var warehouseCollectionType = ((IWhsWarehouseCollection)this).WarehouseCollectionType;
			if (warehouseCollectionType == WarehouseCollectionType.ProductWarehouse)
			{
				AddFilterForWarehouseType(result, SQLComparisonOperator.Equal, new[] { WarehouseTypes.Codes.Product, WarehouseTypes.Codes.FreeTradeZone });
			}
			else if (warehouseCollectionType == WarehouseCollectionType.TransitWarehouse)
			{
				AddFilterForWarehouseType(result, SQLComparisonOperator.Equal, new[] { WarehouseTypes.Codes.Transit });
			}
			else if (warehouseCollectionType == WarehouseCollectionType.FTZWarehouse)
			{
				AddFilterForWarehouseType(result, SQLComparisonOperator.Equal, new[] { WarehouseTypes.Codes.FreeTradeZone });
			}
			else if (warehouseCollectionType == WarehouseCollectionType.CYDWarehouse)
			{
				AddFilterForWarehouseType(result, SQLComparisonOperator.Equal, new[] { WarehouseTypes.Codes.ContainerYard });
			}

			return result;
		}

		void AddFilterForWarehouseType(ZQuery result, SQLComparisonOperator sqlComparisonOperator, string[] typeNames)
		{
			result.AddToFilter(WhsWarehouseSchema.WW_WarehouseType, sqlComparisonOperator, typeNames);
		}

		#endregion

		// interfaces
		#region IWhsWarehouseCollection Members

		WarehouseCollectionType IWhsWarehouseCollection.WarehouseCollectionType
		{
			get => collectionType;
			set => collectionType = value;
		}
		WarehouseCollectionType collectionType = WarehouseCollectionType.ProductWarehouse;

		#endregion
	}
}
