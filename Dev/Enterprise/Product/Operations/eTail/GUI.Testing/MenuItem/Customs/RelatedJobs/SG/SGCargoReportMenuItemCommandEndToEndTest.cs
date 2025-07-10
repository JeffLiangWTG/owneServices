using System;
using System.Linq;
using CargoWise.Application;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	sealed class SGCargoReportMenuItemCommandEndToEndTest : RelatedJobCommandMenuItemEndToEndBaseTest
	{
		public void TestMenuItemAction_ShouldShowSGAccessForm()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Singapore))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Air;
				var shipment = consol.Shipments.AddNew();

				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_HCH_Header = consignmentHeader.PK;
				consignment.Items.AddNew();

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == "SG ACCESS Export Manifest");
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create SG ACCESS Export Manifest").PerformClick();

					var sgAccessForm = ZFormModaliser.LastFormShownForTest as ManifestForm;
					AssertNotNull(sgAccessForm);

					var sgAccess = sgAccessForm.BusinessEntity;
					Assert("would not save AMS Header when create", !sgAccess.IsInDatabase);
				}
			}
		}

		protected override ForwardingShipment PrepareShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "TestWaybill";
			consol.JK_TransportMode = TransportModes.Road;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Road;
			shipment.JS_RL_NKDestination = "SGWDL";
			shipment.JS_RL_NKOrigin = "AUSYD";

			return shipment;
		}

		protected override string TestingCountry => CountryCodes.Singapore;
		protected override string RelatedJobName => "SG ACCESS Import Manifest";
		protected override string ExpectedAppLockKey => "SGCargoReportCommand";
		protected override BooleanRegistryItem RemoveNonEuropeanWesternCharactersRegistry => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersSGSGAccessImportManifest;

		IDisposable registrySetting;

		protected override void SetUp()
		{
			var sgRegistry = ObjectFactory.Get<Enterprise.Integration.Customs.SG.ISGCustomsRegistry>();
			registrySetting = sgRegistry.ACCESSEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			registrySetting?.Dispose();
		}
	}
}
