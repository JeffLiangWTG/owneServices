using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsWorkOrderReferences : IReferencesParentWithAdditionalReferences
	{
		public ZString ExternalReference { get; set; }
		public ZByte ExternalReferenceSplit { get; set; }
		public List<KeyValuePair<ZString, ZString>> References { get; set; }
	}
}
