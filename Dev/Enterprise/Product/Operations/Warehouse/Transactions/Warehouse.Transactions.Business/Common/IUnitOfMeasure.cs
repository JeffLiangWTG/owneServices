using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface IUnitOfMeasure
	{
		ZString Name { get; }
		ZString TotalUQ { get; }

		int DecimalPlacesForRounding { get; }

		ZDecimal GetQuantityFromProduct(OrgSupplierPart part);
		ZString GetUQFromProduct(OrgSupplierPart part);
	}
}
