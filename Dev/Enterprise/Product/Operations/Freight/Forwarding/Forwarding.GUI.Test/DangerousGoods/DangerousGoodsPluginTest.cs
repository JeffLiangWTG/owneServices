using System;
using System.Web;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class DangerousGoodsPluginTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void Test_AddPluginToShipmentForm_ShouldCauseNoErrorsAndShouldAddAnActionMenuItem()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.DangerousGoodsPlugin);
				form.Show();

				AssertEquals("Should be no errors thrown.", 0, ErrorReporter.TotalErrorCount);
				AssertNotNull("Form actions menu should contain 'Open Dangerous Goods Portal' item.", form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Dangerous Goods Portal"));
			}
		}

		public void Test_ClickingOpenDangerousGoodsPortalMenuItem_ShouldOpenDangerousGoodsPortal_WhenTransportModeIsSea()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			AssertDangerousGoodsPortalLaunch(shipment, isLaunchExpected: true);

			shipment.JS_TransportMode = Core.Constants.TransportModes.SeaAir;
			AssertDangerousGoodsPortalLaunch(shipment, isLaunchExpected: true);

			shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			AssertDangerousGoodsPortalLaunch(shipment, isLaunchExpected: true);
		}

		public void TestShipment_ClickingOpenDangerousGoodsPortalMenuItem_ShouldShowMessage_WhenTransportIsNotSea()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertDangerousGoodsPortalLaunch(shipment, isLaunchExpected: false);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			AssertDangerousGoodsPortalLaunch(shipment, isLaunchExpected: false);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			AssertDangerousGoodsPortalLaunch(shipment, isLaunchExpected: false);

			shipment.JS_TransportMode = Core.Constants.TransportModes.Courier;
			AssertDangerousGoodsPortalLaunch(shipment, isLaunchExpected: false);
		}

		public void Test_ClickingOpenDangerousGoodsPortalMenuItem_ShouldOpenDangerousGoodsPortal_WhenThereIeSeaLegTransport()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var transport1 = shipment.TransportsIncludingRelated.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;

			var transport2 = shipment.TransportsIncludingRelated.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Sea;

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			AssertDangerousGoodsPortalLaunch(shipment, isLaunchExpected: true);
		}

		public void TestShipment_ClickingOpenDangerousGoodsPortalMenuItem_ShouldShowMessage_WhenNoSeaTransportLeg()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var transport1 = shipment.TransportsIncludingRelated.AddNew();
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;

			var transport2 = shipment.TransportsIncludingRelated.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Road;

			var transport3 = shipment.TransportsIncludingRelated.AddNew();
			transport3.JW_TransportMode = Core.Constants.TransportModes.Rail;

			var transport4 = shipment.TransportsIncludingRelated.AddNew();
			transport4.JW_TransportMode = Core.Constants.TransportModes.Storage;

			var transport5 = shipment.TransportsIncludingRelated.AddNew();
			transport5.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			AssertDangerousGoodsPortalLaunch(shipment, isLaunchExpected: false);
		}

		void AssertDangerousGoodsPortalLaunch(ForwardingShipment shipment, bool isLaunchExpected)
		{
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.DangerousGoodsPlugin);
				form.Show();
				WebUrlLauncher.ClearLastUrlLaunched();
				form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Dangerous Goods Portal").PerformClick();

				if (isLaunchExpected)
				{
					AssertDangerousGoodsPortalUrl(shipment);
				}
				else
				{
					AssertPortalNotLaunchedWithErrorMessage(shipment.TransportMode);
				}
			}
		}

		void AssertDangerousGoodsPortalUrl(ForwardingShipment shipment)
		{
			var launchedUrl = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedUrl, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
			var accessToken = queryKeyValuePairs["sso_otp"];

			AssertNotNull("A Glow Access Token should be attached", accessToken);
			AssertEquals("The PK of the shipment should be included in the query string", shipment.PK.ToString(), queryKeyValuePairs["entityPK"]);
			AssertEquals("https", uri.Scheme);
			AssertEquals("address", uri.Host);
			AssertEquals("/Goto/ManageDangerousGoodsForShipments", uri.AbsolutePath);
		}

		void AssertPortalNotLaunchedWithErrorMessage(string transportMode)
		{
			var launchedUrl = WebUrlLauncher.LastUrlLaunched;
			AssertEquals($"Url should empty for {transportMode}", string.Empty, launchedUrl);
			AssertEquals($"Expected message not displayed for {transportMode}", "This shipment does not have a Sea leg movement.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
