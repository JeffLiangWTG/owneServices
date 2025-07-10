using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Warehouse.Integration.BondedWarehouse
{
	public interface IWhsBondedWarehouseTransaction : IWhsWarehouseTransaction // per declaration
	{
		bool IsWarehousedByExternalAgent { get; }
		IEnumerable<AdditionalReference> AdditionalReferences { get; }
		new IWhsBondedWarehouseTransactionLineCollection Lines { get; }

		// obsolete
		IOrgAddress Warehouse { get; }
	}
}
