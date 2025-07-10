using Enterprise.DataTransfer.Business;
using Enterprise.Warehouse.Environment.DataTransfer;

namespace Enterprise.Warehouse.Environment.GUI
{
	public class SysMergeWarehouseXmlDataTransferExporter : XmlDataTransferExporter
	{
		public SysMergeWarehouseXmlDataTransferExporter()
			: base(new SysMergeWarehouseValueObjectDataAdapter(), false)
		{
		}
	}
}
