using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	sealed class AUAirCargoReportMenuItemCommandEndToEndTest : RelatedJobCommandMenuItemEndToEndBaseTest
	{
		public void TestMenuItemAction_WhenPreScreeningStatusCodeIsUnKnownOrPassed_ThenClickCreateAirCargoReport()
		{
			var australianAirCargoShipment = HVLVMenuItemTestHelper.CreateShipmentValidForNewETailData(Factory);
			australianAirCargoShipment.JS_TransportMode = TransportModes.Air;
			australianAirCargoShipment.JS_RL_NKOrigin = "NZAKL";
			australianAirCargoShipment.JS_RL_NKDestination = "AUSYD";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "MAWB1234";
			consol.Shipments.Add(australianAirCargoShipment);

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = australianAirCargoShipment.PK;
			consignment.HVC_PreScreeningStatus = HVLVConsignmentPreScreeningStatusCodes.Codes.Unknown;
			consignment.Items.AddNew();

			Factory.Save();

			var preScreeningConfiguration = new HVLVDetailsPreScreeningConfiguration() { IsEnabled = true };

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			using (HVLVDataRegistry.Instance.HVLVDetailsPreScreeningConfiguration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, preScreeningConfiguration))
			using (HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUAirCargoReport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm(australianAirCargoShipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var customsMenu = topLevelMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
				var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == "HVLV AirCargo Report");
				commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create HVLV AirCargo Report").PerformClick();
				var messageWithUnknownCode = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains("HVLV Pre-Screening has been enabled but not run on this shipment.", messageWithUnknownCode);

				UnitTestUserNotification.Instance.ClearMessages();

				var preScreenMenu = topLevelMenu.MenuItems.OfType<HVLVPreScreenMenuGroup>().Single();
				var preScreenMenuItem = preScreenMenu.MenuItems.OfType<PreScreenMenuItem>().Single();
				preScreenMenuItem.OnPopup(EventArgs.Empty);

				AssertNotNull(preScreenMenuItem);
				preScreenMenuItem.PerformClick();
				Factory.Save();

				commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create HVLV AirCargo Report").PerformClick();
				var messageWithPassedCode = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertNullOrEmpty("HVLV Pre-Screening has been enabled to run on this shipment", messageWithPassedCode);
			}
		}

		protected override ForwardingShipment PrepareShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "TestWaybill";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			return shipment;
		}

		protected override ConsigneeInfo GetConsigneeInfoFromJob(IBusiness job)
		{
			var housebill = ((CusMAWB)job).CurrentHouseBills.Single() as CusHAWB;

			return new ConsigneeInfo()
			{
				Name = housebill.CS_ConsigneeName,
				State = housebill.CS_ConsigneeState,
				City = housebill.CS_ConsigneeCity,
				Address1 = housebill.CS_ConsigneeStreet,
				Address2 = housebill.CS_ConsigneeStreet2,
				PostCode = housebill.CS_ConsigneePostcode
			};
		}

		protected override string TestingCountry => CountryCodes.Australia;
		protected override string RelatedJobName => "HVLV AirCargo Report";
		protected override string ExpectedAppLockKey => "AUAirCargoReportCommand";
		protected override BooleanRegistryItem RemoveNonEuropeanWesternCharactersRegistry => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUAirCargoReport;
	}
}
