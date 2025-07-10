using System.Collections;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsWarehouseTransactionLineCollection : IEnumerable
	{
		IWhsWarehouseTransactionLineCollection Clone();
		void Add(IWhsWarehouseTransactionLine line);
		IWhsWarehouseTransactionLine this[int i] { get; set; }
		int Count { get; }
	}
}
