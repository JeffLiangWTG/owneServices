using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.Transit.Business;

namespace Enterprise.Warehouse.Transit.DataTransfer.Universal
{
	public interface IWhsTransitPackageStateBusinessObjectFinder
	{
		IEnumerable<WhsItemPackageState> Find();

		WhsItemPackageStateDTO Find(PackingLine packingLine);
	}
}
