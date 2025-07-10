
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsAdjustmentXmlExporter : WhsDocketXmlExporter<WhsAdjustment>
	{
		public WhsAdjustmentXmlExporter(WhsAdjustmentValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}
	}
}
