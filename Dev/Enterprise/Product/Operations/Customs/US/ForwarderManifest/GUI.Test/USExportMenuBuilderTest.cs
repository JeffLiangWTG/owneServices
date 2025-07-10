using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.ForwarderManifest.Business;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test
{
	class USExportMenuBuilderTest : TestCaseWithFactory
	{
		public void TestMenuCaption()
		{
			var form = new ZArchitecture.GUI.ZForm();
			var menuBuilder = new USExportMenuBuilder(Factory.New<USExportAsycudaManifestHeader>(), form);
			AssertEquals("US Export Manifest", menuBuilder.MenuCaption);
			form.Dispose();
		}
	}
}
