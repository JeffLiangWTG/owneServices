using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NZ.GUI.Testing
{
	public class ICRPluginToConsolTest : ZArchitecture.PlugIn.Testing.ZPlugInGenericTest
	{
		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest()
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = "SEA";
			return new ICRPluginToConsol(consol);
		}

		public void TestEnabledForImportVessel()
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = "SEA";
			using (var plugIn = new ICRPluginToConsol(consol))
			{
				AssertEquals("PlugIn is enabled", true, plugIn.Enabled);
			}
		}

		public void TestDisabledForExportVessel()
		{
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = "SEA";
			using (ICRPluginToConsol plugIn = new ICRPluginToConsol(consol))
			{
				AssertEquals("PlugIn is disabled", false, plugIn.Enabled);
			}
		}

		public void TestManifestStatus()
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = "SEA";
			using (ICRPluginToConsol plugIn = new ICRPluginToConsol(consol))
			{
				plugIn.OnGUIShown();
				AssertNotNull("BusinessObject is referenced", plugIn.BusinessEntity);
			}
		}

		public void TestConsolValidationMessageErrors()
		{
			var refHelper = new UniversalReferenceTestDataHelper(Factory);
			refHelper.CreateNewOrGetExistingCusCodeType("NZFAV", "NZFlightsAndVessels");
			refHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.NewZealand, "NewZealand");
			refHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.NewZealand, "NZFAV", "AA220", "AA220", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			TestHelper.SetupMessagingEnvironment();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTORG";
			var transport = consol.Transports[0];
			transport.JW_Vessel = "AA100";
			transport.JW_VoyageFlight = "AA100";
			transport.CarrierPK = orgHeader.PK;
			Factory.Save();
			using (var plugIn = new ICRPluginToConsol(consol))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var sendMenuItem = plugIn.TopLevelMenu.MenuItems[0];
				sendMenuItem.PerformClick();
				AssertEquals(@"It is likely that your message(s) will be rejected by Customs, as they have the following message errors:

Vessel / Journey Name: Vessel name is not in the list of valid Vessel names supported by NZ Customs.

Do you want to send the message(s) despite these errors?", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}

			transport.JW_Vessel = "AA220";
			transport.JW_VoyageFlight = "AA220";
			Factory.Save();
			using (var plugIn = new ICRPluginToConsol(consol))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var sendMenuItem = plugIn.TopLevelMenu.MenuItems[0];
				sendMenuItem.PerformClick();
				AssertEquals(@"ICR message queued for sending", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		#region Implementation
		ForwardingConsol consol;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
		}
		#endregion
	}
}
