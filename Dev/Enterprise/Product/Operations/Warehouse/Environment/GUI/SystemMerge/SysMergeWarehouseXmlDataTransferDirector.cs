using Enterprise.DataTransfer.SystemMerge.GUI;
using Enterprise.DataTransfer.Xml;
using Enterprise.Warehouse.Environment.DataTransfer;

namespace Enterprise.Warehouse.Environment.GUI
{
	public class SysMergeWarehouseXmlDataTransferDirector : SysMergeXmlDataTransferDirector
	{
		public SysMergeWarehouseXmlDataTransferDirector()
			: base(new SysMergeWarehouseValueObjectDataAdapter())
		{
		}

		public override XmlValueObjectSerializer Serializer
		{
			get { return new SysMergeWarehouseXmlValueObjectSerializer(Adapter.ValueObjectType); }
		}
	}
}
