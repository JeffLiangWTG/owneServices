using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Transit.Business
{
	public interface ITransitJobForConsolCosting
	{
		ZString HouseBillNumber { get; }

		RefUNLOCO Destination { get; }

		IEnumerable<WhsItemPackageState> PackageStatesForConsolCosting { get; }
	}
}
