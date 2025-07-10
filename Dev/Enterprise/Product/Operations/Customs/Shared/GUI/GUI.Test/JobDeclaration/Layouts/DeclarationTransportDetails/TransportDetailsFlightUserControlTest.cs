using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TransportDetailsFlightUserControlTest : TestCase
	{
		public void TestCaptionRenderingEnabled()
		{
			AssertEquals("The layout needs the caption resource strings", true, control.CaptionRenderingEnabled);
		}

		public void TestVoyageFlightNumberTextBox()
		{
			var voyageFlightNumberTextBox = control.VoyageFlightNumberTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true), voyageFlightNumberTextBox.Location);
				AssertEquals("Before the Folio Number", 0, voyageFlightNumberTextBox.TabIndex);
				AssertEquals("Caption used in the layout", "Flight/Folio", voyageFlightNumberTextBox.CaptionResourceString.Caption);
			});
		}

		public void TestFolioNumberTextBox()
		{
			var folioNumberTextBox = control.FolioNumberTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("Changing position breaks the layout", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 0, true), folioNumberTextBox.Location);
				AssertEquals("After the Filght Number", 1, folioNumberTextBox.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new TransportDetailsFlightUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		TransportDetailsFlightUserControl control;
	}
}
