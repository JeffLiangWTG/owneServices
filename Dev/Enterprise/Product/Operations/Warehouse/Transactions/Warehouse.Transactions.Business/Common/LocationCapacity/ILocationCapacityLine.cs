using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ILocationCapacityLine
	{
		ZDecimal GetQuantity(WhsLocation location);
		ZString PalletID { get; }
		ZGuid ProductPK { get; }
	}
}
