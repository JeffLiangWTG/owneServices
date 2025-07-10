
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsOrderXmlExporter : WhsDocketXmlExporter<WhsOrder>
	{
		public WhsOrderXmlExporter(WhsOrderValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}
	}
}
