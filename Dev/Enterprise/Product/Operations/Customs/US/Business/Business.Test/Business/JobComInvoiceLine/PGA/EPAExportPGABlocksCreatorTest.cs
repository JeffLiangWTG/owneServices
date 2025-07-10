using CargoWise.Types;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	class EPAExportPGABlocksCreatorTest : ExportPGABlocksCreatorTest
	{
		protected override void SetupData()
		{
			base.SetupData();

			invoiceLine.US_PSTIndicator = OGAIndicatorList.Codes.Declared;
			invoiceLine.US_EPAConsentNumber = "288127374";
			invoiceLine.US_HazWasteTrackingNo = "123456789ABC";
			invoiceLine.US_EPANetQty = 123m;
			invoiceLine.US_EPANetQtyUQ = "KG";
		}

		protected override ZString ExpectedResult
		{
			get { return "PGAEP1Y288127374   123456789ABCKG 0000000123                                    "; }
		}
	}
}
