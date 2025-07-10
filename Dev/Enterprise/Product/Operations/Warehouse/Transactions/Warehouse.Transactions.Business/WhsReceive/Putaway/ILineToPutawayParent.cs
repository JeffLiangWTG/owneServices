using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ILineToPutawayParent
	{
		BusinessObjectFactory Factory { get; }

		ZGuid PK { get; }
		ZGuid WarehousePK { get; }
		ZGuid ClientPK { get; }
		WhsWarehouse Warehouse { get; }
		OrgHeader Client { get; }
	}
}
