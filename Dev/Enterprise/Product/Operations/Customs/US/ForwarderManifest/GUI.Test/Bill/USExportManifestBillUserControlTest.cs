using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.ForwarderManifest.GUI.Test
{
	class USExportManifestBillUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var userControl = new USExportManifestUserControl())
			{
				AssertNotNull(userControl.Controls.Find("PriorTransportationModeDropEdit", true));
				AssertNotNull(userControl.Controls.Find("BoardedQuantityCalcEdit", true));
				AssertNotNull(userControl.Controls.Find("BoardedWeightCalcDropEdit", true));
				AssertNotNull(userControl.Controls.Find("FinalDestinationPortUserControl", true));
				AssertNotNull(userControl.Controls.Find("ArrivalPortUserControl", true));
				AssertNotNull(userControl.Controls.Find("DeparturePortUserControl", true));
				AssertNotNull(userControl.Controls.Find("LadingPortUserControl", true));
				AssertNotNull(userControl.Controls.Find("UnladingPortUserControl", true));
				AssertNotNull(userControl.Controls.Find("OriginPortUserControl", true));
				AssertNotNull(userControl.Controls.Find("SpecialCargoCodesDropEdit", true));
				AssertNotNull(userControl.Controls.Find("PlaceOfReceiptTextBox", true));
				AssertNotNull(userControl.Controls.Find("AESITNNumbersUserControl", true));
				AssertNotNull(userControl.Controls.Find("InBondNumbersUserControl", true));
				AssertNotNull(userControl.Controls.Find("AESExemptionCodeTextBox", true));
			}
		}
	}
}
