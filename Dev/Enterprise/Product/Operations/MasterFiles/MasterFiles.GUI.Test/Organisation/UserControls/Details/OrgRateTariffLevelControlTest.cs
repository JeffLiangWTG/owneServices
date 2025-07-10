using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class OrgRateTariffLevelControlTest : TestCaseWithFactory
	{
		public void TestColourDeciding()
		{
			OrgHeader org = (new BusinessObjectFactory()).NewWithValidTestData<OrgHeader>();
			OrgRateTariffLevel level = org.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);
			level.P7_TariffType = "FRT";

			using (OrgRateTariffLevelControl panel = new OrgRateTariffLevelControl())
			{
				var args = new ColourDecidingEventArgs(level);
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Empty, args.Colour);

				args = new ColourDecidingEventArgs(level);
				level.P7_StartDate = ZDate.Today.AddDays(-2);
				level.P7_ExpiryDate = ZDate.Today.AddDays(-1);
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.PaleGoldenrod, args.Colour);

				args = new ColourDecidingEventArgs(level);
				level.P7_StartDate = ZDate.Today.AddDays(1);
				level.P7_ExpiryDate = ZDate.Today.AddDays(2);
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Empty, args.Colour);

				args = new ColourDecidingEventArgs(level);
				level.P7_StartDate = ZDate.Empty;
				level.P7_ExpiryDate = ZDate.Today.AddDays(2);
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Empty, args.Colour);

				args = new ColourDecidingEventArgs(level);
				level.P7_StartDate = ZDate.Today;
				level.P7_ExpiryDate = ZDate.Empty;
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Empty, args.Colour);
			}
		}

		public void TestColourDecidingOnDeletedEntry()
		{
			OrgHeader org = (new BusinessObjectFactory()).NewWithValidTestData<OrgHeader>();
			OrgRateTariffLevel level = org.CompanyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 0);
			level.Delete();

			using (OrgRateTariffLevelControl panel = new OrgRateTariffLevelControl())
			{
				var args = new ColourDecidingEventArgs(level);
				panel.EntriesGrid_ColourDeciding_Exposed(args);
				AssertEquals(Color.Empty, args.Colour);
			}
		}

		public void TestControlReadOnly()
		{
			OrgHeader testHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (ZForm testForm = new ZForm(testHeader))
			{
				using (OrgRateTariffLevelControl control = new OrgRateTariffLevelControl())
				{
					testForm.Controls.Add(control);
					control.ReadOnly = true;
					Assert("Grid is readonly", control.TariffLevelGrid.ReadOnly);

					control.ReadOnly = false;
					Assert("Grid is NOT readonly", !control.TariffLevelGrid.ReadOnly);
				}
			}
		}
	}
}
