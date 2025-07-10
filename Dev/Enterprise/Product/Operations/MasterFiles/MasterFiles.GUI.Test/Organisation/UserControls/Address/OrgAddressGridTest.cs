using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrgAddressGridTest : TestCaseWithFactory
	{
		public void TestColumnsNotSerializedByDesigner()
		{
			AssertEquals(true, typeof(OrgAddressGrid).IsSubclassOf(typeof(ZGridWithoutColumnStylesSerialisation)));
		}
	}
}
