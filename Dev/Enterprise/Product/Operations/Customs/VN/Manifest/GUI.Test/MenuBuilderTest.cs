using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI.Testing;
using Enterprise.Customs.VN.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.VN.Manifest.GUI.Test
{
	// TODO Implement more test cases here when we implement the logic
	// Currently, it's just a placeholder
	sealed class MenuBuilderTest : TestCaseWithFactory
	{
		public void TestBuildMenu()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			Factory.Save();

			using var menu = new AsycudaMenuForTest(header);
			using var form = new ZForm(header);
			var menuBuilder = new MenuBuilder(header, form);

			// Currently, it's just an empty menu
			// Update this later
			AssertEquals(0, menuBuilder.BuildMenu().Length);
		}
	}
}
