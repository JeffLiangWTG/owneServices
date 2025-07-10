using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class ManifestTallyFormTest : BaseFreightTest
	{
		public void TestRNSMFTallyPlugIn()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var tally = Factory.New<TallyContainer>();

			loadList.Containers.Add(tally);

			using (var form = new ManifestTallyForm(tally))
			{
				form.Show();

				AssertNull("RNSMFTallyPlugIn should not be plugged in", form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSMFTallyPlugIn));
				AssertNull("RNS/MF menus should not be plugged in", form.Menu.MenuItems.FindByName("RNS/MF"));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				loadList.JK_RL_NKLoadPort = HomePort;
				loadList.JK_RL_NKDischargePort = "";

				using (var form = new ManifestTallyForm(tally))
				{
					form.Show();

					var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSMFTallyPlugIn);
					AssertNotNull("RNSMFTallyPlugIn should be plugged in", plugIn);
					Assert("RNSMFTallyPlugIn should not be enabled", !plugIn.Enabled);

					var menu = form.Menu.MenuItems.FindByText("RNS/MF");
					AssertNotNull("RNS/MF menus should be plugged in", menu);
					Assert("RNS/MF menus should be invisible", !menu.Visible);
				}

				loadList.JK_RL_NKLoadPort = "";
				loadList.JK_RL_NKDischargePort = HomePort;

				using (var form = new ManifestTallyForm(tally))
				{
					form.Show();

					var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSMFTallyPlugIn);
					Assert("RNSMFTallyPlugIn should be enabled", plugIn.Enabled);

					var menu = form.Menu.MenuItems.FindByText("RNS/MF");
					Assert("RNS/MF menus should be visible", menu.Visible);
				}
			}
		}
	}
}
