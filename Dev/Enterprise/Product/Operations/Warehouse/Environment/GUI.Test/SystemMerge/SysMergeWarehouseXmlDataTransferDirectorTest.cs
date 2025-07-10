using Enterprise.DataTransfer.Integration;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Environment.DataTransfer;

namespace Enterprise.Warehouse.Environment.GUI.Testing
{
	public class SysMergeWarehouseXmlDataTransferDirectorTest : WhsTestCaseWithFactoryEnv
	{
		#region TestAdapter

		public void TestAdapter()
		{
			var director = new SysMergeWarehouseXmlDataTransferDirectorForTest();
			AssertEquals(typeof(SysMergeWarehouseValueObjectDataAdapter), director.Adapter_Exposed.GetType());
		}

		#endregion

		#region TestSerializer

		public void TestSerializer()
		{
			var director = new SysMergeWarehouseXmlDataTransferDirectorForTest();
			AssertEquals(typeof(SysMergeWarehouseXmlValueObjectSerializer), director.Serializer.GetType());
		}

		#endregion

		#region TestIsPermitted

		public void TestIsPermitted()
		{
			var director = new SysMergeWarehouseXmlDataTransferDirectorForTest();
			AssertEquals(true, director.IsPermitted);
		}

		#endregion

		#region SysMergeWarehouseXmlDataTransferDirectorForTest class

		class SysMergeWarehouseXmlDataTransferDirectorForTest : SysMergeWarehouseXmlDataTransferDirector
		{
			public IValueObjectDataAdapter Adapter_Exposed
			{
				get { return Adapter; }
			}
		}

		#endregion
	}
}
