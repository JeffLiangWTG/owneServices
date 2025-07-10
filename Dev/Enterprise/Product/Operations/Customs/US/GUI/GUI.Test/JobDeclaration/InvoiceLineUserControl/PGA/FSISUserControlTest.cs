using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class FSISUserControlTest : TestCaseWithFactory
	{
		public void TestNoExceptionOnView()
		{
			using (var control = new FSISUserControl())
			{
				AssertNoExceptionThrown(() => control.ViewEditButton.PerformClick());
			}
		}

		public void TestSourceEstNoAndCountryAdded()
		{
			using (var control = new FSISUserControl())
			{
				AssertNotNull(control.LotGrid.GetColumnStyle("US_SourceCountry"));
				AssertNotNull(control.LotGrid.GetColumnStyle("US_SourceEstNo"));
			}
		}

		public void TestDescriptionOfProductAdded()
		{
			using (var control = new FSISUserControl())
			{
				AssertNotNull(control.LotGrid.GetColumnStyle("US_ProductDescription"));
			}
		}

		public void TestLotGridColumnNames()
		{
			using (var control = new FSISUserControl())
			{
				var columnStyle = control.LotGrid.GetColumnStyle("US_SourceCountry");
				AssertEquals("US_SourceCountry caption", "Source Ctry/Rgn.", columnStyle.Caption);
				AssertEquals("US_SourceCountry caption", "Source Ctry/Rgn.", columnStyle.CaptionResourceString.Caption);
			}
		}
	}

	sealed class FSISGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<FSISUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.FSISLines;

		protected override ZGrid GetGrid(FSISUserControl control) => control.CertificateGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((USInvoiceLineFSISLineCollection)collection).AddNew();
	}
}
