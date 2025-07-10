using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public interface IReferencesParentWithAdditionalReferences : IReferencesParent
	{
		List<KeyValuePair<ZString, ZString>> References { get; set; }
	}
}
