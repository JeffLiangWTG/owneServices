using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class ACELaceyActUserControlTest : TestCase
	{
		public void TestConstituentElementsGridColumnNames()
		{
			using (var control = new ACELaceyActUserControl())
			{
				var columnStyle = control.ConstituentElementsGrid.GetColumnStyle("US_UnknownBreakdownCountryCode");
				AssertEquals("US_UnknownBreakdownCountryCode caption", "Country/Region", columnStyle.CaptionResourceString.Caption);
			}
		}

		public void TestCountriesGridColumnNames()
		{
			using (var control = new ACELaceyActUserControl())
			{
				var columnStyle = control.CountriesGrid.GetColumnStyle("US_CountryCode");
				AssertEquals("US_CountryCode caption", "Country/Region", columnStyle.Caption);
			}
		}
	}

	sealed class ACELaceyActGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<ACELaceyActUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.LaceyActLines;

		protected override ZGrid GetGrid(ACELaceyActUserControl control) => control.PGACommonGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((PGACollection)collection).AddNew();
	}
}
