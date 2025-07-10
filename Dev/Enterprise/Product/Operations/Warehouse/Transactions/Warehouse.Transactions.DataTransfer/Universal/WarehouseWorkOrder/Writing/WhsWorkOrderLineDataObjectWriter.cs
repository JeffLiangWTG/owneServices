using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal
{
	public class WhsWorkOrderLineDataObjectWriter : WhsPickableDocketLineDataObjectWriter<WhsWorkOrderLine>
	{
		public WhsWorkOrderLineDataObjectWriter(IDataWritingManager manager)
			: base(manager)
		{
		}
	}
}
