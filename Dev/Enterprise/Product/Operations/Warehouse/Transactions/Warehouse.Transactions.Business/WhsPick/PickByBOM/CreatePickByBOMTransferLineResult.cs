using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Packing.Business;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class CreatePickByBOMTransferLineResult
	{
		public CreatePickByBOMTransferLineResult(HashSet<WhsTransferLine> transferLinesToIgnoreWhenSettingLocation, IEnumerable<PkgPackage> kitPackages)
		{
			TransferLinesToIgnoreWhenSettingLocation = Argument.NotNull(transferLinesToIgnoreWhenSettingLocation, nameof(transferLinesToIgnoreWhenSettingLocation));
			KitPackages = Argument.NotNull(kitPackages, nameof(kitPackages));
		}

		public HashSet<WhsTransferLine> TransferLinesToIgnoreWhenSettingLocation;

		public IEnumerable<PkgPackage> KitPackages;

		public void Deconstruct(out HashSet<WhsTransferLine> transferLinesToIgnoreWhenSettingLocation, out IEnumerable<PkgPackage> kitPackages)
		{
			transferLinesToIgnoreWhenSettingLocation = TransferLinesToIgnoreWhenSettingLocation;
			kitPackages = KitPackages;
		}
	}
}
