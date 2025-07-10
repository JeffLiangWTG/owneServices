using Enterprise.DataTransfer.Integration;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class SysMergeWarehouseInventoryXmlDataTransferDirectorTest : WhsTestCaseWithFactory
	{
		public void TestAdapter()
		{
			var director = new SysMergeWarehouseXmlDataTransferDirectorForTest();
			AssertEquals(typeof(SysMergeWarehouseInventoryValueObjectDataAdapter), director.Adapter_Exposed.GetType());
		}

		public void TestSerializer()
		{
			var director = new SysMergeWarehouseXmlDataTransferDirectorForTest();
			AssertEquals(typeof(SysMergeWarehouseInventoryXmlValueObjectSerializer), director.Serializer.GetType());
		}

		public void TestIsPermitted()
		{
			var director = new SysMergeWarehouseXmlDataTransferDirectorForTest();
			AssertEquals(true, director.IsPermitted);
		}

		#region SysMergeWarehouseXmlDataTransferDirectorForTest class

		class SysMergeWarehouseXmlDataTransferDirectorForTest : SysMergeWarehouseInventoryXmlDataTransferDirector
		{
			public IValueObjectDataAdapter Adapter_Exposed => Adapter;
		}

		#endregion
	}
}
