using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsDynamicWorkOrderReferences : IReferencesParent
	{
		public ZString ExternalReference { get; set; }
		public ZByte ExternalReferenceSplit { get; set; }
	}
}
