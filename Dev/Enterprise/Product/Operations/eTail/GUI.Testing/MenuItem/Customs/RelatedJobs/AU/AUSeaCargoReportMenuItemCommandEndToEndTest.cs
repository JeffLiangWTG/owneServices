using System.Linq;
using CargoWise.EntityFramework;
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
	sealed class AUSeaCargoReportMenuItemCommandEndToEndTest : RelatedJobCommandMenuItemEndToEndBaseTest
	{
		public void TestMenuItemAction_WhenConvertShipmentToSeaCargoReport_ItemContainerIsMandatory()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_MasterBillNum = "MAWB1234";

			AssertItemContainerIsMandatory(false, "Create HVLV SeaCargo Report", true);
			AssertItemContainerIsMandatory(true, "Create HVLV SeaCargo Report", false);
			AssertItemContainerIsMandatory(false, "Sync HVLV SeaCargo Report", true);
			AssertItemContainerIsMandatory(true, "Sync HVLV SeaCargo Report", false);

			void AssertItemContainerIsMandatory(bool itemHasContainer, string menuItemCaption, bool hasItemContainerMandatoryErrorMessage)
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";

				var consignmentHeader = shipment.GetOrCreateHVLVConsignmentHeader();
				var consignment = consignmentHeader.Consignments.AddNew();
				var item = consignment.Items.AddNew();
				if (itemHasContainer)
				{
					item.HVI_ContainerNumber = "TestContainerNumber";
				}

				Factory.Save();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var topLevelMenu = plugin.TopLevelMenu;
					topLevelMenu.PerformSelect();

					var customsMenu = topLevelMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == "HVLV SeaCargo Report");
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == menuItemCaption).PerformClick();

					if (hasItemContainerMandatoryErrorMessage)
					{
						AssertEquals("Please enter container number for Item(s).", UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertNotContains("Please enter container number for Item(s).", UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		protected override ForwardingShipment PrepareShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "TestWaybill";
			consol.JK_TransportMode = TransportModes.Sea;
			var container = consol.Containers.AddNew();
			container.ContainerNumberForBinding = "TestContainer";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUSYD";

			return shipment;
		}

		protected override ConsigneeInfo GetConsigneeInfoFromJob(IBusiness job)
		{
			var housebill = ((CusSCAOceanBill)job).HouseBills.Single() as CusSCAHouse;

			return new ConsigneeInfo()
			{
				Name = housebill.CA_ConsigneeName,
				State = housebill.CA_ConsigneeState,
				City = housebill.CA_ConsigneeSuburb,
				Address1 = housebill.CA_ConsigneeAddress1,
				Address2 = housebill.CA_ConsigneeAddress2,
				PostCode = housebill.CA_ConsigneePostcode
			};
		}

		protected override string TestingCountry => CountryCodes.Australia;
		protected override string RelatedJobName => "HVLV SeaCargo Report";
		protected override string ExpectedAppLockKey => "AUSeaCargoReportCommand";
		protected override BooleanRegistryItem RemoveNonEuropeanWesternCharactersRegistry => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersAUSeaCargoReport;
	}
}
