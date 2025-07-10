using System.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Integration
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IWhsWarehouseCollection : IBusinessObjectCollection
	{
		[EditorBrowsable(EditorBrowsableState.Never)]
		WarehouseCollectionType WarehouseCollectionType { get; set; }

		void Load();
	}

	public enum WarehouseCollectionType
	{
		All = 0,
		ProductWarehouse = 1,
		TransitWarehouse = 2,
		FTZWarehouse = 3,
		CYDWarehouse = 4
	}
}
