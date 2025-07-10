using Enterprise.DataTransfer.SystemMerge.GUI;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Transactions.DataTransfer;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class SysMergeWarehouseInventoryXmlDataTransferDirector : SysMergeXmlDataTransferDirector
	{
		public SysMergeWarehouseInventoryXmlDataTransferDirector()
			: base(new SysMergeWarehouseInventoryValueObjectDataAdapter())
		{
		}

		public override XmlValueObjectSerializer Serializer => new SysMergeWarehouseInventoryXmlValueObjectSerializer(Adapter.ValueObjectType);
	}
}
