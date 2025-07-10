using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(UNDGSubstanceCFRForm))]
	sealed class UNDGSubstanceCFRFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new UNDGSubstanceCFRForm(Factory.New<UNDGSubstanceCFR>());
		}

		#endregion

		public void TestAllowNew()
		{
			using (var form = GetFormToBash())
			{
				Assert("Not allowed New display mode", !((IPostingButtonsProvider)form).AllowNew);
			}
		}

		public void TestUNNOLabelShowsCorrectPrefix()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstanceCFR>();

			AssertUNNOTextBoxCaptionStartsWith("UNNO label prefix should default to UN when CFR_Prefix is empty.", substance, "UN");

			substance.CFR_Prefix = "NA";
			AssertUNNOTextBoxCaptionStartsWith("UNNO label prefix should match CFR_Prefix when CFR_Prefix is not empty.", substance, "NA");
		}

		[RequiresSTA]
		public void TestMiscellaneousTabSplitIntoThreeSections()
		{
			var substance = Factory.NewWithValidTestData<UNDGSubstanceCFR>();
			using (var form = new UNDGSubstanceCFRForm(substance))
			{
				var miscellaneousTabPage = form.Controls.Find("MiscellaneousTabPage", true)?.FirstOrDefault() as ZTabPage;
				AssertNotNull("MiscellaneousTabPage should not be null", miscellaneousTabPage);

				var passengerAircraftRailCarZGroupBox = form.Controls.Find("CFR_PassengerAircraftRailCarGroupBox", true)?.FirstOrDefault() as ZGroupBox;
				AssertNotNull("CFR_PassengerAircraftRailCarGroupBox should not be null", passengerAircraftRailCarZGroupBox);

				var cargoAircraftOnlyZGroupBox = form.Controls.Find("CFR_CargoAircraftOnlyGroupBox", true)?.FirstOrDefault() as ZGroupBox;
				AssertNotNull("CFR_CargoAircraftOnlyGroupBox should not be null", cargoAircraftOnlyZGroupBox);

				var symbolsZGroupBox = form.Controls.Find("CFR_SymbolsGroupBox", true)?.FirstOrDefault() as ZGroupBox;
				AssertNotNull("CFR_SymbolsGroupBox should not be null", symbolsZGroupBox);
			}
		}

		void AssertUNNOTextBoxCaptionStartsWith(string message, UNDGSubstanceCFR substance, string prefix)
		{
			using (var form = new UNDGSubstanceCFRForm(substance))
			{
				var unnoTextBox = form.Controls.Find("CFR_UNNOTextBox", true)?.FirstOrDefault() as ZTextBox;
				AssertNotNull("Precondition", unnoTextBox);

				AssertStartsWith(message, prefix, unnoTextBox.CaptionResourceString.Caption);
			}
		}
	}
}
