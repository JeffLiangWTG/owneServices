using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.GUI.Test
{
	class AutoratingDateFilteringUserControlTest : TestCaseWithFactory
	{
		public void TestChargeGroupSetupGridAndChargeGroupGrid_AutoratingDateFiltering_Default()
			=> TestChargeGroupSetupGridAndChargeGroupGrid_AutoratingDateFiltering(RatingDateFilterTypes.Codes.Default, expectedGrids: false);

		public void TestChargeGroupSetupGridAndChargeGroupGrid_AutoratingDateFiltering_Standard()
			=> TestChargeGroupSetupGridAndChargeGroupGrid_AutoratingDateFiltering(RatingDateFilterTypes.Codes.Standard, expectedGrids: false);

		public void TestChargeGroupSetupGridAndChargeGroupGrid_AutoratingDateFiltering_Arrival()
			=> TestChargeGroupSetupGridAndChargeGroupGrid_AutoratingDateFiltering(RatingDateFilterTypes.Codes.Arrival, expectedGrids: false);

		public void TestChargeGroupSetupGridAndChargeGroupGrid_AutoratingDateFiltering_Departure()
			=> TestChargeGroupSetupGridAndChargeGroupGrid_AutoratingDateFiltering(RatingDateFilterTypes.Codes.Departure, expectedGrids: false);

		public void TestChargeGroupSetupGridAndChargeGroupGrid_AutoratingDateFiltering_Custom()
			=> TestChargeGroupSetupGridAndChargeGroupGrid_AutoratingDateFiltering(RatingDateFilterTypes.Codes.Custom, expectedGrids: true);

		void TestChargeGroupSetupGridAndChargeGroupGrid_AutoratingDateFiltering(string autoratingDateFiltering, bool expectedGrids)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.MiscServ.OM_AutoratingDateFiltering = autoratingDateFiltering;

			using (var form = new ZForm(orgHeader))
			{
				using (var control = new AutoratingDateFilteringUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					Application.DoEvents();

					var chargeGroupSetupGrid = ControlTestHelper.FindControls<ZGrid>(form).Single(x => x.Name == "ChargeGroupSetupGrid");
					var chargeGroupGrid = ControlTestHelper.FindControls<ZGrid>(form).Single(x => x.Name == "ChargeGroupGrid");
					AssertEquals("ChargeGroupSetupGrid Visible", expectedGrids, chargeGroupSetupGrid.Visible);
					AssertEquals("ChargeGroupGrid Visible", expectedGrids, chargeGroupGrid.Visible);
					AssertEquals("ChargeGroupGrid ReadOnly", true, chargeGroupGrid.ReadOnly);
				}
			}
		}
	}
}
