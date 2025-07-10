using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.GUI.FormExtensions;
using Enterprise.Registry.Business.eServices;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.DataTransfer.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class ModuleToModuleFormExtensionsTest : WhsGuiTestCaseWithFactory
	{
		public void TestCreateShipmentMenuItem()
		{
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			using (var form = new DummyIWarehouseToForwardingForm())
			{
				var relatedJobsMenuItem = form.CreateRelatedJobsMainMenuItem<DummyIWarehouseToForwardingForm, WhsOrder>();

				AssertEquals("Related Jobs", relatedJobsMenuItem.Text);
				AssertEquals(1, relatedJobsMenuItem.MenuItems.Count);

				var shipmentsMenuItem = relatedJobsMenuItem.MenuItems[0];
				AssertEquals("Shipments", shipmentsMenuItem.Text);

				relatedJobsMenuItem.OnPopup(EventArgs.Empty);
				AssertEquals(false, shipmentsMenuItem.Enabled);

				var data = new TestDataSimpleEnvironment(Factory);
				form.Order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
				form.Order.WD_ExternalReference = "123";
				form.Order.WD_GoodsDescription = "Goods";
				form.Order.Warehouse.WarehouseAddress.OA_RL_NKRelatedPortCode = "AUMEL";
				form.Order.ConsigneeAddress.OA_RL_NKRelatedPortCode = "AUSYD";
				relatedJobsMenuItem.OnPopup(EventArgs.Empty);
				AssertEquals(true, shipmentsMenuItem.Enabled);
				AssertEquals(1, shipmentsMenuItem.MenuItems.Count);

				var createShipmentMenuItem = shipmentsMenuItem.MenuItems[0];
				AssertEquals("123: Goods Create Shipment", createShipmentMenuItem.Text);

				form.HasChanges = true;
				createShipmentMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Create Shipment", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Please save", UnitTestUserNotification.Instance.LastMessage.Text);

				form.HasChanges = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				createShipmentMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Create Shipment", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(@"Failed to create Shipment:
Cannot create a Shipment as no Freight Forwarder is specified.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Order.WD_OH_Forwarder = Factory.New<OrgHeader>().PK;
				form.Order.Forwarder.OH_Code = "SAM";
				createShipmentMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Create Shipment", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertMultilineASCIIEquals("UnitTestUserNotification.Instance.LastMessage.Text",
@"Failed to create Shipment:
No EDI Communications settings were found on the Recipient Organization [SAM]. Please add an entry on the [Details > Config > EDI Communications] tab of this Organization before sending Universal Data.
						".Trim(), UnitTestUserNotification.Instance.LastMessage.Text);

				var communicationMode = form.Order.Forwarder.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "Universal";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = "WOU"; //Warehouse Outwards

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				createShipmentMenuItem.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Create Shipment", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Shipment queued for sending to Organization [SAM]", UnitTestUserNotification.Instance.LastMessage.Text);

				var poke = form.Order.RelatedJobs;
				AssertEquals("Precondition", 0, form.Order.RelatedJobs.Count);
			}
		}

		#region Implementation

		internal class DummyIWarehouseToForwardingForm : ZForm, IModuleToModuleForm<WhsOrder>
		{
			public DummyIWarehouseToForwardingForm()
			{
			}

			public DummyIWarehouseToForwardingForm(WhsOrder order)
				: base(order)
			{
			}

			public bool HasChanges { get; set; }
			public WhsOrder Order { get; set; }
			WhsOrder IModuleToModuleForm<WhsOrder>.BusinessEntity { get { return Order; } }
			public ZString ErrorMessage { get { return "Please save"; } }

			ZString IModuleToModuleForm<WhsOrder>.RelatedEntityDescription
			{
				get { return string.Format("{0}: {1}", Order.WD_ExternalReference, Order.GoodsDescriptionWithFallback); }
			}

			public ZString RelatedEntityName { get { return "Shipment"; } }

			ResourceString IModuleToModuleForm<WhsOrder>.MainMenuItemCaption
			{
				get { return ResString.GetMultilingualString("0d69b25b-526b-48a4-8d3f-c172ea1f922c", "Shipments"); }
			}

			ResourceString IModuleToModuleForm<WhsOrder>.TopLevelMenuItemCaption
			{
				get { return ResString.GetMultilingualString("656beba3-97bb-4492-ba91-97f278fab96f", "Related Jobs"); }
			}

			ResourceString IModuleToModuleForm<WhsOrder>.WasExportedNotificationMessage
			{
				get { return ResString.GetMultilingualString("c439064b-bbe0-4bef-ad90-cac414f320d6", "Shipment queued for sending to Organization [{0}]", Order.Forwarder.OH_Code); }
			}

			IModuleToModuleSender IModuleToModuleForm<WhsOrder>.GetModuleToModuleSender()
			{
				return new WarehouseOrderModuleToModuleSender();
			}

			ControllerID IModuleToModuleForm<WhsOrder>.ControllerID
			{
				get { return ControllerIDs.JobShipment; }
			}
		}

		#endregion

	}
}
