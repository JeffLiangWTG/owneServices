using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.LVS.Business;
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
	sealed class USLowValueEntriesMenuItemCommandEndToEndTest : RelatedJobCommandMenuItemEndToEndBaseTest
	{
		public void TestMenuItemAction_WhenConsignmentWaybillHasNoSCACCodeAndIsOver12Characters_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.Shipments.Add(shipment);

				var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
				var cusCode = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
				cusCode.OK_CodeType = "CCC";
				cusCode.OK_CustomsRegNo = "ABCD";
				cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

				consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);
				var consignment_WithWaybillLength13 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment_WithWaybillLength13.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment_WithWaybillLength13.HVC_WaybillNumber = "1234567890123";
				consignment_WithWaybillLength13.HVC_HCH_Header = consignmentHeader.PK;
				consignment_WithWaybillLength13.Items.AddNew();

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == "Low Value Entries");
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create Low Value Entries").PerformClick();

					var waybillValidationErrorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on Low Value Entries.\r\nWould you like to proceed?", waybillValidationErrorMessage);
				}
			}
		}

		public void TestMenuItemAction_WhenConsignmentWaybillHasSCACCodeAndIsOver16Characters_ShowsMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "USLAX";

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.Shipments.Add(shipment);

				var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
				var cusCode = sendingAgentOrg.ConfigOrg.CustomsCodes.AddNew();
				cusCode.OK_CodeType = "CCC";
				cusCode.OK_CustomsRegNo = "ABCD";
				cusCode.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;

				consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

				var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);

				var consignment_WithWaybillLength17 = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment_WithWaybillLength17.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment_WithWaybillLength17.HVC_WaybillNumber = "ABCD5678901234567";
				consignment_WithWaybillLength17.HVC_HCH_Header = consignmentHeader.PK;
				consignment_WithWaybillLength17.Items.AddNew();

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var hVLVMenu = plugin.TopLevelMenu;
					hVLVMenu.PerformSelect();

					var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					customsMenu.OnPopup(EventArgs.Empty);

					var jobCommandMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().FirstOrDefault(menu => menu.Caption == "Low Value Entries");
					jobCommandMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == "Create Low Value Entries").PerformClick();

					var waybillValidationErrorMessage = UnitTestUserNotification.Instance.LastMessage.Text;
					AssertEquals("There are waybill(s) that exceed 12 in length, proceeding may cause message errors on Low Value Entries.\r\nWould you like to proceed?", waybillValidationErrorMessage);
				}
			}
		}

		protected override ForwardingShipment PrepareShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_RL_NKDestination = "USLAX";

			return shipment;
		}

		protected override ConsigneeInfo GetConsigneeInfoFromJob(IBusiness job)
		{
			var housebill = ((CusUSLVClearance)job).CusUSLVConsignments.Single() as CusUSLVConsignment;

			return new ConsigneeInfo()
			{
				Name = housebill.ULB_ConsigneeName,
				State = housebill.ULB_ConsigneeState,
				City = housebill.ULB_ConsigneeCity,
				Address1 = housebill.ULB_ConsigneeAddress1,
				Address2 = housebill.ULB_ConsigneeAddress2,
				PostCode = housebill.ULB_ConsigneePostCode
			};
		}

		protected override string TestingCountry => CountryCodes.UnitedStates;
		protected override string RelatedJobName => "Low Value Entries";
		protected override string ExpectedAppLockKey => "USLowValueCommand";
		protected override BooleanRegistryItem RemoveNonEuropeanWesternCharactersRegistry => HVLVDataRegistry.Instance.RemoveNonWesternEuropeanCharactersUSLowValueEntries;
	}
}
