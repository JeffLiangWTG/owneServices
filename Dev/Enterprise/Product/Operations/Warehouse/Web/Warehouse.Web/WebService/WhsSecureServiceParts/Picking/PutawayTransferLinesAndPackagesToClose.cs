using CargoWise.Common;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Web.WebService
{
	public class PutawayTransferLinesAndPackagesToClose
	{
		public PutawayTransferLinesAndPackagesToClose(WhsTransferLine[] transferLinesToPutaway, PkgPackage[] packagesToClose)
		{
			TransferLinesToPutaway = Argument.NotNull(transferLinesToPutaway, nameof(transferLinesToPutaway));
			PackagesToClose = Argument.NotNull(packagesToClose, nameof(packagesToClose));
		}

		public void Deconstruct(out WhsTransferLine[] transferLinesToPutaway, out PkgPackage[] packagesToClose)
		{
			transferLinesToPutaway = TransferLinesToPutaway;
			packagesToClose = PackagesToClose;
		}

		public WhsTransferLine[] TransferLinesToPutaway { get; }
		public PkgPackage[] PackagesToClose { get; }
	}
}
