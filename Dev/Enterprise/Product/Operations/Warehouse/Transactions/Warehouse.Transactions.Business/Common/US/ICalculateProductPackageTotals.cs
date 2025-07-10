using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ICalculateProductPackageTotals : ILineAttributes
	{
		ZString PackageGroupID { get; }
		ZDecimal PerPackageQty { get; }
		ZGuid ProductPK { get; }
		ZGuid LocationPK { get; }
		ZGuid ClientPK { get; }
		ZDecimal Units { get; }
	}
}
