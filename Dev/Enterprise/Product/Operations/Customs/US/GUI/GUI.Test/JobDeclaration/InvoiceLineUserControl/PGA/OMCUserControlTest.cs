using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ACEPGATestForm<OMCUserControl>))]
	sealed class OMCUserControlTest : ZPGAFormBasherAbstractTest<OMCUserControl>
	{
		public void TestOMCLineGridColumnNames()
		{
			using (var control = new OMCUserControl())
			{
				AssertEquals("US_SourceCountry caption", "Source Ctry/Rgn.", control.OMCLineGrid.GetColumnStyle("US_SourceCountry").CaptionResourceString.Caption);
			}
		}

		protected override CargoWise.EntityFramework.BusinessObject GetPGABusinessObject(JobComInvoiceLine invoiceLine)
		{
			invoiceLine.US_OMCInd = OGAIndicatorList.Codes.Declared;
			var header = invoiceLine.OMCHeaders.AddNew();
			return header;
		}

		protected override string BindMember => "FilteredInvoiceLines.OMCHeaders";
	}

	sealed class OMCGridPGADataCorrectionSupporterTest : ZGridPGADataCorrectionSupporterTest<OMCUserControl>
	{
		protected override IPGADataCorrectionCollection GetPGACollection(JobComInvoiceLine invoiceLine) => invoiceLine.OMCHeaders;

		protected override ZGrid GetGrid(OMCUserControl control) => control.OMCLineGrid;

		protected override IPGADataCorrection AddNewItemToCollection(IPGADataCorrectionCollection collection) => ((OMCHeaderCollection)collection).AddNew();
	}
}
