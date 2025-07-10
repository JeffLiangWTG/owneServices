using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(UNDGSubstanceForm))]
	sealed class UNDGSubstanceFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new UNDGSubstanceForm(Factory.New<UNDGSubstance>());
		}

		#endregion

		public void TestAllowNew()
		{
			using (var form = GetFormToBash())
			{
				Assert("Not allowed New display mode", !((IPostingButtonsProvider)form).AllowNew);
			}
		}

		[RequiresSTA]
		public void TestCountryReferencesModuleButtonGrid()
		{
			var dgSubstance = Factory.NewWithValidTestData<UNDGSubstance>();
			dgSubstance.DG_ExceptedQuantityCode = UNDGSubstanceLookups.ExceptedQuantity.Code.E2;

			using (var form = new UNDGSubstanceForm(dgSubstance))
			{
				form.Show();

				var countryReferencesGrid = GUITestHelper.FindControl<CountryReferencesModuleButtonGrid>(form.Controls, "CountryReferencesModuleButtonGrid");
				AssertEquals("New MUST BE disabled", false, countryReferencesGrid.ShowNewButton);
				AssertEquals("Edit MUST BE disabled", false, countryReferencesGrid.ShowEditButton);
				AssertEquals("Attach should be enabled", true, countryReferencesGrid.ShowAttachButton);
				AssertEquals("Detach should be enabled", true, countryReferencesGrid.ShowDetachButton);

				form.Close();
			}
		}

		[GuiTest]
		[RequiresSTA]
		public override void TestMinimumSizeNotTooBig()
		{
			using (var testForm = GetFormToBash())
			{
				const int MinScreenWidthSupported = 1366;
				const int MinScreenHeightSupported = 810;
				const int TypicalTaskbarHeight = 43;
				var maxSizeWidth = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(MinScreenWidthSupported);
				var maxSizeHeight = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(MinScreenHeightSupported - TypicalTaskbarHeight);
				Assert("Form min size too wide (" + testForm.MinimumSize.Width + ") for the screen. Should be less than or equal to " + maxSizeWidth, testForm.MinimumSize.Width <= maxSizeWidth);
				Assert("Form min size too high (" + testForm.MinimumSize.Height + ") for the screen. Should be less than or equal to " + maxSizeHeight, testForm.MinimumSize.Height <= maxSizeHeight);
			}
		}

		[RequiresSTA]
		public void TestBorderPanel()
		{
			using var control = new UNDGSubstanceControl();
			var horizontalDivider = control.Controls.Find("DG_BorderPanel", searchAllChildren: true)?.FirstOrDefault() as ZPanel;
			AssertNotNull(horizontalDivider);
			AssertEquals("DG_BorderPanel", horizontalDivider.Name);
			AssertEquals(new Size(820, 1), horizontalDivider.Size);
			AssertEquals(new Point(60, 40), horizontalDivider.Location);
			AssertEquals(Color.DarkGray, horizontalDivider.BackColor);
		}
	}
}
