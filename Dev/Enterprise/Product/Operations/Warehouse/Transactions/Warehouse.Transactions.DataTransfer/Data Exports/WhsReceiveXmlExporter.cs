using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsReceiveXmlExporter : WhsDocketXmlExporter<WhsReceive>
	{
		public WhsReceiveXmlExporter(WhsReceiveValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}
	}
}
