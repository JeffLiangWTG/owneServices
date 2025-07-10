using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsSalesChannel))]
	public class WhsSalesChannelTest : WhsEnvBusinessObjectTestCase
	{
		#region TestIWhsSalesChannel

		public void TestIWhsSalesChannel_PK()
		{
			var whsSalesChannel = Factory.New<WhsSalesChannel>();
			AssertEquals(whsSalesChannel.PK, ((IWhsSalesChannel)whsSalesChannel).PK);
		}

		public void TestIWhsSalesChannel_Code()
		{
			var whsSalesChannel = Factory.New<WhsSalesChannel>();

			whsSalesChannel.WSH_Code = "TST";
			AssertEquals("TST", ((IWhsSalesChannel)whsSalesChannel).WSH_Code);
		}

		public void TestIWhsSalesChannel_Description()
		{
			var whsSalesChannel = Factory.New<WhsSalesChannel>();

			whsSalesChannel.WSH_Description = "Test Channel";
			AssertEquals("Test Channel", ((IWhsSalesChannel)whsSalesChannel).WSH_Description);
		}

		#endregion
	}
}
