using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<DEAUserControl>))]
	sealed class DEAUserControlTest : ZPGAFormBasherAbstractTest<DEAUserControl>
	{
		public void TestDEAHeaderGridColumnNames()
		{
			using (var control = new DEAUserControl())
			{
				var columnStyle = control.DEAHeaderGrid.GetColumnStyle("US_CountryOfShipment");
				AssertEquals("US_CountryOfShipment caption", "Ctry/Rgn. of Shipment", columnStyle.Caption);
			}
		}

		protected override CargoWise.EntityFramework.BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_DEAInd = OGAIndicatorList.Codes.Declared;
			var dea = invoiceLine.DEAHeaders.AddNew();
			dea.US_PermitNumber = "1234567";
			dea.Constituents.AddNew();
			return dea;
		}

		protected override string BindMember => "FilteredInvoiceLines.DEAHeaders";
	}

	sealed class DEAGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<DEAUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.DEAHeaders;

		protected override ZGrid GetGrid(DEAUserControl control) => control.DEAHeaderGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((DEAHeaderCollection)collection).AddNew();
	}
}
