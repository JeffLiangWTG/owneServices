using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsPutawayGroup))]
	public class WhsPutawayGroupTest : WhsEnvBusinessObjectTestCase
	{
		#region TestIWhsPutawayGroup

		public void TestIWhsPutawayGroup_PK()
		{
			var whsPutawayGroup = Factory.New<WhsPutawayGroup>();
			AssertEquals(whsPutawayGroup.PK, ((IWhsPutawayGroup)whsPutawayGroup).PK);
		}

		public void TestIWhsPutawayGroup_Code()
		{
			var whsPutawayGroup = Factory.New<WhsPutawayGroup>();

			whsPutawayGroup.WPG_Code = "TST";
			AssertEquals("TST", ((IWhsPutawayGroup)whsPutawayGroup).WPG_Code);
		}

		public void TestIWhsPutawayGroup_Description()
		{
			var whsPutawayGroup = Factory.New<WhsPutawayGroup>();

			whsPutawayGroup.WPG_Description = "Test Group";
			AssertEquals("Test Group", ((IWhsPutawayGroup)whsPutawayGroup).WPG_Description);
		}

		#endregion
	}
}
