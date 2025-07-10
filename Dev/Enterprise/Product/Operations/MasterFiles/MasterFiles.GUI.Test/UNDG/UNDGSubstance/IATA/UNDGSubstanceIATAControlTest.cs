using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class UNDGSubstanceIATAControlTest : TestCaseWithFactory
	{
		public void TestUNNOLabelShowsCorrectPrefix()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_Mode = Core.Constants.TransportModes.Air;

			substance.DG_UNNO = "8000";
			AssertUNNOTextBoxCaptionStartsWith("When substance UNNO is 8000, prefix should be ID.", substance, "ID");

			substance.DG_UNNO = "8001";
			AssertUNNOTextBoxCaptionStartsWith("When substance UNNO is 8001, prefix should be ID.", substance, "ID");

			substance.DG_UNNO = "6969";
			AssertUNNOTextBoxCaptionStartsWith("When substance UNNO is not 8000 or 8001, prefix should be UN.", substance, "UN");
		}

		[RequiresSTA]
		public void TestControlsVisibility()
		{
			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			substance.DG_CargoPackIns = LithiumBatteryConstants.RefPackingInstructions.PI966;
			substance.DG_Code = "1003";
			substance.DG_UNNO = "1003";
			AssertVisibility(substance, true);

			substance.DG_CargoPackIns = "111";
			AssertVisibility(substance, false);
		}

		void AssertUNNOTextBoxCaptionStartsWith(string message, UNDGSubstance substance, string prefix)
		{
			using (var control = new UNDGSubstanceIATAControl(substance))
			{
				var unnoTextBox = control.Controls.Find("DG_UNNumberTextBox", true)?.FirstOrDefault() as ZTextBox;
				AssertNotNull("Precondition", unnoTextBox);

				AssertStartsWith(message, prefix, unnoTextBox.CaptionResourceString.Caption);
			}
		}

		void AssertVisibility(UNDGSubstance substance, bool visibility)
		{
			using (var control = new UNDGSubstanceIATAControl(substance))
			{
				var passengerAndCargoGroupBox = control.FindSingle<ZGroupBox>(c => c.Name == "PassengerAndCargoGroupBox");
				var paxMaxAmtTextBox = passengerAndCargoGroupBox.FindSingle<ZTextBox>(c => c.Name == "DG_LQ2OrPaxMaxAmtUQTextBox2");
				AssertNotNull("Precondition", paxMaxAmtTextBox);
				AssertEquals(paxMaxAmtTextBox.Visible, visibility);

				var paxMaxAmtUQTextBox = passengerAndCargoGroupBox.FindSingle<ZCalcEdit>(c => c.Name == "DG_LQ2OrPaxMaxAmtCalcEdit2");
				AssertNotNull("Precondition", paxMaxAmtUQTextBox);
				AssertEquals(paxMaxAmtUQTextBox.Visible, visibility);

				var paxPackInsTextBox = passengerAndCargoGroupBox.FindSingle<ZTextBox>(c => c.Name == "DG_PaxPackInsTextBox2");
				AssertNotNull("Precondition", paxPackInsTextBox);
				AssertEquals(paxPackInsTextBox.Visible, visibility);

				var paxPackInsSecTextBox = passengerAndCargoGroupBox.FindSingle<ZTextBox>(c => c.Name == "DG_PaxPackInsSecTextBox2");
				AssertNotNull("Precondition", paxPackInsSecTextBox);
				AssertEquals(paxPackInsSecTextBox.Visible, visibility);

				var cargoOnlyGroupBox = control.FindSingle<ZGroupBox>(c => c.Name == "CargoOnlyGroupBox");
				var caoMaxAmtTextBox = cargoOnlyGroupBox.FindSingle<ZTextBox>(c => c.Name == "DG_CargoMaxAmtUQTextBox2");
				AssertNotNull("Precondition", caoMaxAmtTextBox);
				AssertEquals(caoMaxAmtTextBox.Visible, visibility);

				var caoMaxAmtUQTextBox = cargoOnlyGroupBox.FindSingle<ZCalcEdit>(c => c.Name == "DG_CargoMaxAmtCalcEdit2");
				AssertNotNull("Precondition", caoMaxAmtUQTextBox);
				AssertEquals(caoMaxAmtUQTextBox.Visible, visibility);

				var caoPackInsTextBox = cargoOnlyGroupBox.FindSingle<ZTextBox>(c => c.Name == "DG_CargoPackInsTextBox2");
				AssertNotNull("Precondition", caoPackInsTextBox);
				AssertEquals(caoPackInsTextBox.Visible, visibility);

				var caoPackInsSecTextBox = cargoOnlyGroupBox.FindSingle<ZTextBox>(c => c.Name == "DG_CargoPackInsSecTextBox2");
				AssertNotNull("Precondition", caoPackInsSecTextBox);
				AssertEquals(caoPackInsSecTextBox.Visible, visibility);
			}
		}
	}
}
