using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.CountryCompliance.Mexico;

namespace Enterprise.MasterFiles.Business.CountryCompliance.Testing.Mexico
{
	sealed class MexicoEInvoicingExtensionTest : TestCaseWithFactory
	{
		public void TestGetUsosCFDI_CountValid()
		{
			var usosCFDI = MexicoEInvoicingExtension.GetUsosCFDI();
			AssertEquals(24, usosCFDI.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			MexicoEInvoicingExtension = new MexicoEInvoicingExtension();
		}

		IMexicoEInvoicingExtension MexicoEInvoicingExtension;
	}
}
