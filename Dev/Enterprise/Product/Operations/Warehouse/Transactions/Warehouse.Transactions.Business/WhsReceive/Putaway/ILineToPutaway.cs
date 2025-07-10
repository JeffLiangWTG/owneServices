using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	public interface ILineToPutaway : ILineToPutawayInfo
	{
		WhsLocation Location { get; }
		OrgSupplierPart Product { get; }

		ILineToPutaway Split(ZDecimal quantityForNewLine);
	}
}
