using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.SG.Access.Business;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	sealed class ManifestBillFilterStripControlTest : TestCaseWithFactory
	{
		public void TestGridColums()
		{
			using (var control = new ManifestBillFilterStripControlForTest(Factory))
			{
				Assert(!control.grid.Columns.Contains(AsycudaBill.Schema.CountryCode));
			}
		}
	}
}
