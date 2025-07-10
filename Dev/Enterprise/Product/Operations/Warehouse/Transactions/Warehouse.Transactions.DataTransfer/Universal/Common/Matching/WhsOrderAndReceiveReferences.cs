using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsOrderAndReceiveReferences : IReferencesParentWithAdditionalReferences
	{
		public ZString ExternalReference { get; set; }
		public ZByte ExternalReferenceSplit { get; set; }
		public ZGuid ClientOrganization { get; set; }
		public ZString ClientReference { get; set; }
		public ZString TransportReference { get; set; }
		public List<KeyValuePair<ZString, ZString>> References { get; set; }
	}
}
