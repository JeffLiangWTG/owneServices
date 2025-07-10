using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI.Testing
{
	abstract class RelatedJobCommandMenuItemEndToEndBaseTest : BaseNeedAddApplicationLockHVLVMenuItemTest
	{
		public void TestRemoveNonEuropeanWesternCharactersRegistry()
		{
			if (RemoveNonEuropeanWesternCharactersRegistry != null && HasConsigneeInfo)
			{
				var shipment = PrepareShipment();

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_WaybillNumber = "1234";
				consignment.HVC_ConsigneeName = "上海ABC";
				consignment.HVC_ConsigneeState = "南AB";
				consignment.HVC_ConsigneeCity = "はいABC";
				consignment.HVC_ConsigneeAddress1 = "はいABC";
				consignment.HVC_ConsigneeAddress2 = "户的ABC";
				consignment.HVC_ConsigneePostcode = "아니오ABC";

				var item = consignment.Items.AddNew();
				if (shipment.Consols[0].Containers.Count > 0)
				{
					item.HVI_ContainerNumber = shipment.Consols[0].Containers[0].ContainerNumberForBinding;
				}

				Factory.Save();

				using (RemoveNonEuropeanWesternCharactersRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var job = ConvertToRelatedJobInNewFactoryWithoutSaving(shipment.PK);
					var consigneeInfo = GetConsigneeInfoFromJob(job);

					CombineAssertions("Expected the european non-western characters to be not removed when registry is false", () =>
					{
						AssertEquals("上海ABC", consigneeInfo.Name);
						AssertEquals("南AB", consigneeInfo.State);
						AssertEquals("はいABC", consigneeInfo.City);
						AssertEquals("はいABC", consigneeInfo.Address1);
						AssertEquals("户的ABC", consigneeInfo.Address2);
						AssertEquals("아니오ABC", consigneeInfo.PostCode);
					});
				}

				using (RemoveNonEuropeanWesternCharactersRegistry.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var job = ConvertToRelatedJobInNewFactoryWithoutSaving(shipment.PK);
					var consigneeInfo = GetConsigneeInfoFromJob(job);

					CombineAssertions("Expected the european non-western characters to be removed when registry is true", () =>
					{
						AssertEquals("ABC", consigneeInfo.Name);
						AssertEquals("AB", consigneeInfo.State);
						AssertEquals("ABC", consigneeInfo.City);
						AssertEquals("ABC", consigneeInfo.Address1);
						AssertEquals("ABC", consigneeInfo.Address2);
						AssertEquals("ABC", consigneeInfo.PostCode);
					});
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestReloadFormWorksForOpenedRelatedCustomsJobForm()
		{
			var shipment = PrepareShipment();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item = consignment.Items.AddNew();
			if (shipment.Consols[0].Containers.Count > 0)
			{
				item.HVI_ContainerNumber = shipment.Consols[0].Containers[0].ContainerNumberForBinding;
			}
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(TestingCountry))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();

				form.FireSaveButton();
				plugin.OnSaveCompletedOrAborted(true);

				UnitTestUserNotification.Instance.AddOKAnswer();

				var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == RelatedJobName);
				commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == $"Create {RelatedJobName}").PerformClick();

				var lastForm = ZFormModaliser.LastFormShownForTest as ZForm;
				lastForm.ReloadForm();

				var formCreatedByReloading = new ZFormUtilitiesTest().GetFormCreatedByReloading(lastForm);
				Application.DoEvents();
				AssertNotNull(formCreatedByReloading);
				formCreatedByReloading.Close();
			}
		}

		public void TestMenuItemAction_WhenShipmentTransportTypeDoesntMatchConsolTransportType_ThenShowWarningMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(TestingCountry))
			{
				var shipment = PrepareShipment();
				var consol = shipment.LocalConsol;
				consol.JK_TransportMode = shipment.JS_TransportMode == TransportModes.Sea ? TransportModes.Air : TransportModes.Sea;

				var header = shipment.GetOrCreateHVLVConsignmentHeader();

				var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
				consignment.HVC_IsActive = true;
				header.Consignments.Add(consignment);

				Factory.Save();

				using (var form = new ZForm(shipment))
				{
					form.PlugIns.Add(ControllerIDs.ETailShipment);
					form.Show();

					var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
					var menuHVLV = plugin.TopLevelMenu;
					menuHVLV.PerformSelect();

					var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
					var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == RelatedJobName);
					commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == $"Create {RelatedJobName}").PerformClick();

					AssertEquals("The transport mode does not match the transport mode for this consol." + System.Environment.NewLine + "Would you like to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		IBusiness ConvertToRelatedJobInNewFactoryWithoutSaving(ZGuid shipmentPK)
		{
			var factory = new BusinessObjectFactory();

			var shipment = factory.Load<ForwardingShipment>(shipmentPK);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(TestingCountry))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var hVLVMenu = plugin.TopLevelMenu;
				hVLVMenu.PerformSelect();

				var customsMenu = hVLVMenu.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();

				form.FireSaveButton();
				plugin.OnSaveCompletedOrAborted(true);

				UnitTestUserNotification.Instance.AddOKAnswer();

				var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single(x => x.Caption == RelatedJobName);
				commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == $"Create {RelatedJobName}").PerformClick();

				var job = ((ZForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
				AssertNotNull("Pre-req: customs job is generated", job);

				return job;
			}
		}

		protected virtual ConsigneeInfo GetConsigneeInfoFromJob(IBusiness job)
		{
			var housebill = ((AsycudaManifestHeader)job).Bills.AsEnumerable().Single();

			return new ConsigneeInfo
			{
				Name = housebill.ABL_ConsigneeName,
				State = housebill.ABL_ConsigneeState,
				City = housebill.ABL_ConsigneeCity,
				Address1 = housebill.ABL_ConsigneeStreet1,
				Address2 = housebill.ABL_ConsigneeStreet2,
				PostCode = housebill.ABL_ConsigneePostcode
			};
		}

		protected struct ConsigneeInfo
		{
			public string Name { get; set; }
			public string State { get; set; }
			public string City { get; set; }
			public string Address1 { get; set; }
			public string Address2 { get; set; }
			public string PostCode { get; set; }
		}

		protected abstract BooleanRegistryItem RemoveNonEuropeanWesternCharactersRegistry { get; }

		protected virtual bool HasConsigneeInfo => true;
	}
}
