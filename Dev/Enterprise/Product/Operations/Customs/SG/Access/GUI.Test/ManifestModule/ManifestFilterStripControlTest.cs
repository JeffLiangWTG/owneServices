using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.Access.Business;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	sealed class ManifestFilterStripControlTest : TestCaseWithFactory
	{
		public void TestGridColums()
		{
			using (var control = new ManifestFilterStripControl(new ManifestModuleCollection(Factory), new ManifestFilterStrip()))
			{
				Assert("AMA_RN_NKCountry", !control.RelatedGrid.Columns.Contains(AsycudaManifestHeader.Schema.AMA_RN_NKCountry));
			}
		}
	}
}
