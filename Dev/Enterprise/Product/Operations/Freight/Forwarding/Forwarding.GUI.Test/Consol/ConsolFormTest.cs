using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ComplianceRisk.GUI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DeniedPartyScreening.GUI.Test;
using Enterprise.Environment;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.Freight.Forwarding.GUI.AWB;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.Testing;
using Enterprise.Freight.Integration;
using Enterprise.Freight.Integration.ApiClient;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.SystemToSystemTrust;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using CO2eBusinessTestHelper = Enterprise.Freight.CarbonEmissions.Business.Testing.CO2eTestHelper;
using CO2eTestHelper = Enterprise.Freight.DataTransfer.Universal.Testing.CO2eTestHelper;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	internal sealed class ConsolFormTest : BaseFreightTest
	{
		public void TestSendTransitWarehouseInstructionWithComfirmation()
		{
			using (FreightDataRegistry.Instance.EnableMexicanPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "SH00001";
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "MCUU";
				shipment.JS_RL_NKDestination = "BRSAO";
				var packLine1 = shipment.OuterPackLines.AddNew();
				packLine1.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Surplus;
				packLine1.JL_PackageCount = 1;
				var packLine2 = shipment.OuterPackLines.AddNew();
				packLine2.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Discrepencies;
				packLine2.JL_PackageCount = 2;
				var packLine3 = shipment.OuterPackLines.AddNew();
				packLine3.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed;
				packLine3.JL_PackageCount = 3;
				var packLine4 = shipment.OuterPackLines.AddNew();
				packLine4.JL_OriginTransitWarehouseStatus = FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown;
				packLine4.JL_PackageCount = 4;
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
				using (var form = new ConsolForm(consol))
				{
					form.Show();

					form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(mi => mi.Text == "Actio&ns").OnPopup(EventArgs.Empty);

					var sendDepartureTWReceiptInstructionMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Actio&ns")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Transit Warehouse")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Departure")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Send Receipt Instruction");

					AssertNotNull("Electronic Messaging menu item exists", sendDepartureTWReceiptInstructionMenuItem);

					sendDepartureTWReceiptInstructionMenuItem.PerformClick();
					AssertEquals(4, shipment.OuterPackLines.Count);
					Assert(UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text?.StartsWith("Pack lines of below shipment/s have TW Matching Status that are not confirmed. Are you sure you want to proceed?") ?? false));
				}
			}

			UnitTestUserNotification.Instance.ClearMessages();
			UnitTestUserNotification.Instance.ClearUserResponses();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			using (FreightDataRegistry.Instance.EnableMexicanPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "SH00002";
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_RL_NKOrigin = "MCUU";
				shipment.JS_RL_NKDestination = "BRSAO";
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
				using (var form = new ConsolForm(consol))
				{
					form.Show();

					form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(mi => mi.Text == "Actio&ns").OnPopup(EventArgs.Empty);

					var sendDepartureTWReceiptInstructionMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Actio&ns")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Transit Warehouse")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Departure")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Send Receipt Instruction");

					AssertNotNull("Electronic Messaging menu item exists", sendDepartureTWReceiptInstructionMenuItem);

					sendDepartureTWReceiptInstructionMenuItem.PerformClick();
					Assert(!UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text?.Contains("TW Matching Status is not confirmed.  Are you sure you want to proceed?") ?? false));
				}
			}
		}

		public void TestSendTransitWarehouseInstructionWhenDCNIsSplit()
		{
			using (FreightDataRegistry.Instance.EnableMexicanPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.New<ForwardingConsol>();
				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "SH00001";

				var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
				shipment.JS_OA_ExportReceivingDepot = depot.PK;
				var communicationMode = depot.Header.EDICommunicationsModes.AddNew();
				var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
				warehouse.WW_WarehouseType = "TRW";
				warehouse.WW_IsActive = true;
				warehouse.WW_IsActive = true;
				warehouse.WW_OA_WarehouseAddress = depot.PK;

				var dispatchConsignment1 = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
				dispatchConsignment1.WDC_ConsignmentID = "DCN1";
				dispatchConsignment1.WDC_ParentID = shipment.PK;
				dispatchConsignment1.WDC_ParentTableCode = shipment.TablePrefix;
				dispatchConsignment1.WDC_WW_Warehouse = warehouse.PK;

				var dispatchConsignment2 = Factory.NewWithValidTestData<WhsItemDispatchConsignment>();
				dispatchConsignment2.WDC_ConsignmentID = "DCN2";
				dispatchConsignment2.WDC_ParentID = shipment.PK;
				dispatchConsignment2.WDC_ParentTableCode = shipment.TablePrefix;
				dispatchConsignment2.WDC_WW_Warehouse = warehouse.PK;
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
				using (var form = new ConsolForm(consol))
				{
					form.Show();

					form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(mi => mi.Text == "Actio&ns").OnPopup(EventArgs.Empty);

					var sendPickupTWDispatchInstructionMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Actio&ns")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Transit Warehouse")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Departure")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Send Dispatch Instruction");

					AssertNotNull("Pickup TW's Send Dispatch Instruction menu item exists", sendPickupTWDispatchInstructionMenuItem);

					sendPickupTWDispatchInstructionMenuItem.PerformClick();
					Assert(UnitTestUserNotification.Instance.PreviousMessages.Any(x => x.Text?.StartsWith("Failed to send Dispatch Instruction - Transit Warehouse Dispatch Consignments linked to SH00001 have already been split.") ?? false));
				}
			}
		}

		public void TestSendTransitWarehouseInstructionByLogSubscriber()
		{
			using (FreightDataRegistry.Instance.EnableSendingForwardingConsolToTWHAsynchronously.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.EnableMexicanPortIntegrationFeatures.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.New<ForwardingConsol>();
				var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());
				AssertNotNull("Precondition: Depot should exist in DB.", depot);
				for (var i = 1; i <= 11; i++)
				{
					var shipment = consol.Shipments.AddNew();
					shipment.JS_UniqueConsignRef = $"SH0000{i}";
					shipment.JS_OA_ExportReceivingDepot = depot.PK;
				}

				var communicationMode = depot.Header.EDICommunicationsModes.AddNew();
				var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
				warehouse.WW_WarehouseType = "TRW";
				warehouse.WW_IsActive = true;
				warehouse.WW_OA_WarehouseAddress = depot.PK;
				Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
				using (var form = new ConsolForm(consol))
				{
					form.Show();

					form.Menu.MenuItems.Cast<MenuItem>().FirstOrDefault(mi => mi.Text == "Actio&ns").OnPopup(EventArgs.Empty);

					var sendPickupTWDispatchInstructionMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Actio&ns")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Transit Warehouse")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Departure")
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Send Dispatch Instruction");

					AssertNotNull("Pickup TW's Send Dispatch Instruction menu item exists", sendPickupTWDispatchInstructionMenuItem);

					sendPickupTWDispatchInstructionMenuItem.PerformClick();
					AssertEquals(1, consol.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.MessageSendingRequestCode).Count());
				}
			}
		}

		public void TestShowViewConsignmentInCargoTracker()
		{
			AssertShowViewConsignmentInCargoTracker(Constants.TransportModes.Air);
			AssertShowViewConsignmentInCargoTracker(Constants.TransportModes.Sea);

			void AssertShowViewConsignmentInCargoTracker(string transportMode)
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = transportMode;
				using (FreightDataRegistry.Instance.EnableCargoTracker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					using (var form = new ConsolForm(consol))
					{
						form.Show();
						var actionsMenu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
						AssertNotNull("Actions menu contains 'View in Cargo Tracker' menu item", actionsMenu.MenuItems.FindByText("View in Cargo Tracker"));
					}
				}
			}
		}

		public void TestHideViewConsignmentInCargoTracker()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.InlandWaterwayTransport;
			using (FreightDataRegistry.Instance.EnableCargoTracker.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					var actionsMenu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
					AssertNull("Actions menu not contains 'View in Cargo Tracker' menu item", actionsMenu.MenuItems.FindByText("View in Cargo Tracker"));
				}
			}
		}

		public void TestDefaultingSalesRepFromControllingCustomerIsNotDoneInConsolForm()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consignor = testObjectCreator.CreateOrgHeader("ORGCNR", false, false, "AUSYD");
			consignor.OH_IsConsignor = true;
			var consignee = testObjectCreator.CreateOrgHeader("ORGCNE", false, false, "NZAKL");
			consignee.OH_IsConsignee = true;
			var gatewayConsol = testObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			testObjectCreator.CreateJob(gatewayConsol);
			var shipment = testObjectCreator.CreateShipment("S0001", gatewayConsol);
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			Factory.Save();

			var bizObjCheckFactory = new BusinessObjectFactory();
			AssertNull("Precondition : Shipment job does not exist", new JobHeader.Loader(bizObjCheckFactory, shipment).Load());

			using (FreightDataRegistry.Instance.ControllingCustomerUseSalesRep.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var gatewayConsolFormFactory = new BusinessObjectFactory();
				var gatewayConsolForForm = gatewayConsolFormFactory.Load<ForwardingConsol>(gatewayConsol.PK);
				using (var gatewayConsolForm = new ConsolForm(gatewayConsolForForm))
				{
					gatewayConsolForm.Show();
					var shipmentGrid = gatewayConsolForm.ConsolControl.ShipmentModuleButtonGrid;
					shipmentGrid.InnerGrid.UnSelectAll();
					shipmentGrid.InnerGrid.Select(0);

					var createdJobInvoicingRecordMenuItem = shipmentGrid.InnerGrid.ContextMenu.MenuItems.FindByText("Create Job Invoicing Record");
					AssertNotNull(createdJobInvoicingRecordMenuItem);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					createdJobInvoicingRecordMenuItem.PerformClick();
					AssertEquals(@"Job Invoicing Record has been created for the following shipment(s):
- Shipment S0001
", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotNull("Shipment job is created", new JobHeader.Loader(gatewayConsolFormFactory, shipment).Load());

					var isConsolFormSaved = gatewayConsolForm.FireSaveButton();
					AssertEquals("Consol form is saved.", ContinueWithSave.Yes, isConsolFormSaved);
					AssertNotNull("Shipment job is saved to DB", new JobHeader.Loader(bizObjCheckFactory, shipment).Load());

					shipmentGrid.InnerGrid.UnSelectAll();
					shipmentGrid.InnerGrid.Select(0);

					var toolStrip = (ZToolStrip)shipmentGrid.Controls.Find("toolStrip", true)[0];
					AssertNotNull("Tool Strip should exist.", toolStrip);
					var editButton = (ZToolStripButton)toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true)[0];
					AssertNotNull("Edit Button should exist.", editButton);
					editButton.PerformClick();

					using (var lastShownForm = shipmentGrid.LastShownZForm)
					{
						AssertNotNull(lastShownForm);
						if (lastShownForm is ShipmentForm shipmentForm)
						{
							AssertNotEquals(((IBusiness)gatewayConsolForm.DataSource).Factory._Instance, ((IBusiness)shipmentForm.DataSource).Factory._Instance);

							using (var jobInvoicingPlugIn = shipmentForm.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing))
							{
								var deactivateInvoicingJobMenuItem = jobInvoicingPlugIn.TopLevelMenu.MenuItems.FindByText("Mark Job Header as Inactive");
								AssertNotNull("Mark Job Header as Inactive menu item should exist.", deactivateInvoicingJobMenuItem);

								UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
								UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
								deactivateInvoicingJobMenuItem.PerformClick();

								var isShipmentFormSaved = shipmentForm.FireSaveButton();
								AssertEquals("Shipment form is saved.", ContinueWithSave.Yes, isShipmentFormSaved);
							}
						}
						else
						{
							Fail("Last shown form should be ShipmentForm.");
						}
					}

					AssertNull("Shipment job is cancelled", new JobHeader.Loader(bizObjCheckFactory, shipment).Load());
					AssertNoExceptionThrown(() => gatewayConsolForm.FireSaveButton());
				}
			}
		}

		public void TestResynchronizeScreeningStatusOnFormLoad()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_IsConsignor = true;

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_IsConsignee = true;

				var shipment = Factory.New<ForwardingShipment>();
				shipment.ConsignorPK = consignor.PK;
				shipment.ConsigneePK = consignee.PK;

				consol.Shipments.Add(shipment);
				Factory.Save();

				consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();

				CombineAssertions("Precondition: ", () =>
				{
					AssertEquals(false, consol.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DeniedPartyStatusUpdated.Code).Any());
					AssertEquals(ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
				});

				using (var form = new ConsolForm(consol))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.Show();
					CombineAssertions(() =>
					{
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);
						AssertEquals(1, consol.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DeniedPartyStatusUpdated.Code).Count());
						AssertContains("|NEW=CLR|OLD=MAT|TYP=SYNC", consol.Logs.MostRecentLogByEventTime(ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated).SL_Reference);
					});
				}

				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(true)))
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					AssertEquals(ScreeningStatusesList.Codes.Matched, consol.JK_ScreeningStatus);
					AssertEquals("The number of logs is still 1.", 1, consol.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DeniedPartyStatusUpdated.Code).Count());
				}
			}
		}

		public void TestResynchronizeScreeningStatusOnFormLoad_NoDeveloperNotificationExceptionThrown()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
				Factory.Save();

				sendingForwarder.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				Factory.Save();

				AssertNoExceptionThrown(() =>
				{
					using (TestingState.SuspendIsRunningTests())
					using (var form = new ConsolForm(consol))
					{
						form.Show();
						AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
						AssertEquals("Consol screening status CLR.", ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);
					}
				});
			}
		}

		public void TestNoNeedToResynchronizeScreeningStatusOnFormLoad()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

				var consignor = Factory.NewWithValidTestData<OrgHeader>();
				consignor.OH_IsConsignor = true;

				var consignee = Factory.NewWithValidTestData<OrgHeader>();
				consignee.OH_IsConsignee = true;

				var shipment = Factory.New<ForwardingShipment>();
				shipment.ConsignorPK = consignor.PK;
				shipment.ConsigneePK = consignee.PK;

				consol.Shipments.Add(shipment);
				Factory.Save();

				consignor.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				Factory.Save();

				CombineAssertions("Precondition: ", () =>
				{
					AssertEquals(false, consol.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.DeniedPartyStatusUpdated.Code).Any());
					AssertEquals(ScreeningStatusesList.Codes.Clear, consignor.OH_ScreeningStatus);
					AssertEquals(ScreeningStatusesList.Codes.Clear, consignee.OH_ScreeningStatus);
					AssertEquals(ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);
				});

				using (var form = new ConsolForm(consol))
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.Show();
					CombineAssertions(() =>
					{
						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(false, consol.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated.Code).Any());
						AssertEquals(ScreeningStatusesList.Codes.Clear, consignor.OH_ScreeningStatus);
						AssertEquals(ScreeningStatusesList.Codes.Clear, consignee.OH_ScreeningStatus);
						AssertEquals(ScreeningStatusesList.Codes.Clear, consol.JK_ScreeningStatus);
					});
				}
			}
		}

		public void TestMarkAsJobClearMenuItemExist()
		{
			AssertMarkAsJobClearMenuItemExist(false);
			AssertMarkAsJobClearMenuItemExist(true);

			void AssertMarkAsJobClearMenuItemExist(bool registryValue)
			{
				var consol = Factory.New<ForwardingConsol>();
				using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					new DpsMarkJobScreeningStatusClearTest().AssertMenuItemAccessibilityCheckpoint(form, registryValue);
				}
			}
		}

		public void TestMarkAsJobClearWhenSecurityRightsIsDenied_ShouldErrorMessage()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();
			var consol = Factory.New<ForwardingConsol>();

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				consol.JK_ScreeningStatus = "UNK";
				Factory.Save();

				tmpSecurityCore.OrgDeniedPartyScreeningAllowJobLevelClear.IsAllowed = false;
				new DpsMarkJobScreeningStatusClearTest().AssertSecurityRightsAccessibilityCheckpoint(form, tmpSecurityCore);
			}
		}

		public void TestMarkJobClearWhenScreeningStatusIsJCLorCLR_ShouldShowWarningMessage()
		{
			var consol = Factory.New<ForwardingConsol>();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ConsolForm(consol))
			{
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				AssertEquals("Precondition consol screening status", "JCL", consol.JK_ScreeningStatus);
				Factory.Save();

				var markJobClearTestHelper = new DpsMarkJobScreeningStatusClearTest();
				markJobClearTestHelper.AssertStatusAlreadyClearOrJobClear(form);

				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertEquals("Precondition consol screening status", "CLR", consol.JK_ScreeningStatus);
				Factory.Save();

				markJobClearTestHelper.AssertStatusAlreadyClearOrJobClear(form);
			}
		}

		public void TestMarkJobClearWhenJobIsNotSaved_ShouldShowWarningMessage()
		{
			var consol = Factory.New<ForwardingConsol>();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ConsolForm(consol))
			{
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
				AssertEquals("Precondition consol screening status", "NOT", consol.JK_ScreeningStatus);
				new DpsMarkJobScreeningStatusClearTest().AssertSaveBeforeMarkingClear(form);
			}
		}

		public void TestJobShipmentWhenMarkingJobClear_ShouldUpdateJobScreeningStatusToJCL()
		{
			var consol = Factory.New<ForwardingConsol>();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ConsolForm(consol))
			{
				consol.JK_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertEquals("Precondition consol screening status", "MAT", consol.JK_ScreeningStatus);
				Factory.Save();

				new DpsMarkJobScreeningStatusClearTest().AssertScreeenigStatusToJCL(form, consol);
			}
		}

		public void TestSettingBranchOnDefaultDepartmentChargeDoesNotSetHasChangesOnConsol()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var consignee = objectCreator.CreateOrgHeader("SIGNEE", false, false);
			consignee.OH_IsConsignee = true;
			var consignor = objectCreator.CreateOrgHeader("SIGNOR", false, false);
			consignor.OH_IsConsignor = true;

			var branchDefaultingRule = new JobBranchDefaultOrderRule
			{
				DefaultToBlank = 1,
				DefaultToBranchRelatedToPortOrWarehouseBranch = 0,
				DefaultToBranchOfOrganisation = 0,
				DefaultToLoginUserDefault = 0
			};

			var brnDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "BRN"));
			AssertNotNull(brnDepartment);

			using (AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, branchDefaultingRule))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, brnDepartment.PK.ToGuid()))
			{
				var consol = objectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
				var shipment1 = objectCreator.CreateShipment("S001001", consol);
				shipment1.JS_ActualWeight = 10m;
				shipment1.JS_ActualVolume = 10m;
				shipment1.JS_PackingMode = Constants.ContainerModes.Loose;
				shipment1.ConsignorPK = consignor.PK;
				shipment1.ConsigneePK = consignee.PK;

				var departmentChargeCode = objectCreator.FEADepartment.DeptCharges.AddNew();
				departmentChargeCode.GD_AC = objectCreator.FRT.PK;
				Factory.Save();

				AssertNull("Precondition : Shipment Job is not created yet.", shipment1.Job);

				using (var consolForm = new ConsolForm(consol))
				{
					var shipmentGrid = consolForm.ConsolControl.ShipmentModuleButtonGrid;
					AssertNotNull("Shipment grid should exist.", shipmentGrid);
					shipmentGrid.InnerGrid.CopyCaptionsToPropertyHumanReadableNameForTest = true;
					var jobHeaderColumns = shipmentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(col => col.ColumnName.Contains("ShipmentJobHeader")).ToList();
					jobHeaderColumns.ForEach(x => x.IsVisible = true);
					consolForm.Show();

					var consolCostingPlugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Apportionment);
					consolCostingPlugin.SelectTabPage();
					Application.DoEvents();

					AssertNotNull("Precondition : ApportionmentListing should create the shipment job.", shipment1.Job);
					AssertEquals("Precondition : Department should be defaulted to FEA.", objectCreator.FEADepartment, shipment1.Job.Department);
					AssertEquals("Precondition : Branch should be defaulted to current branch.", Env.CurrentBranch.PK, shipment1.Job.Branch.PK);
					AssertEquals("Precondition : Default Department charge should be added.", 1, ((Job)shipment1.Job).Charges.Count);

					consol.Transports[0].JW_VoyageFlight = "AB123";
					Assert("Precondition : Consol must have changes.", consol.HasChanges);

					shipmentGrid.InnerGrid.Select(0);
					var toolStrip = (ZToolStrip)shipmentGrid.Controls.Find("toolStrip", true)[0];
					AssertNotNull("Tool Strip should exist.", toolStrip);
					var editButton = (ZToolStripButton)toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true)[0];
					AssertNotNull("Edit Button should exist.", editButton);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					AssertNoExceptionThrown(editButton.PerformClick);
					AssertEquals("The form must be saved before a Shipment can be edited or a new Shipment can be created. Do you wish to save the form?", UnitTestUserNotification.Instance.LastMessage.Text);
					using (var shipmentForm = shipmentGrid.LastShownZForm)
					{
						AssertNotNull(shipmentForm);
						AssertType(typeof(ShipmentForm), shipmentForm);
					}
				}
			}
		}

		public void TestJobIsNotCreatedWhenAttachingExistingShipmentToConsol()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consignor = testObjectCreator.CreateOrgHeader("ORGCNR", false, false, "AUSYD");
			consignor.OH_IsConsignor = true;
			var consignee = testObjectCreator.CreateOrgHeader("ORGCNE", false, false, "NZAKL");
			consignee.OH_IsConsignee = true;
			var gatewayConsol = testObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = testObjectCreator.CreateShipment("S0001", false);
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			Factory.Save();

			var bizObjCheckFactory = new BusinessObjectFactory();
			AssertNull("Precondition : Shipment job does not exist", new JobHeader.Loader(bizObjCheckFactory, shipment).Load());
			AssertNull("Precondition : Gateway job does not exist", new JobHeader.Loader(bizObjCheckFactory, gatewayConsol).Load());

			var consolFormFactory = new BusinessObjectFactory();
			var gatewayConsolForForm = consolFormFactory.Load<ForwardingConsol>(gatewayConsol.PK);
			var shipmentForForm = consolFormFactory.Load<ForwardingShipment>(shipment.PK);
			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, gatewayDepartment.PK.ToGuid()))
			using (var consolForm = new ConsolForm(gatewayConsolForForm))
			{
				consolForm.Show();
				var shipmentGrid = consolForm.ConsolControl.ShipmentModuleButtonGrid;
				AssertEquals("No shipments attached to consol.", 0, shipmentGrid.InnerGrid.List.Count);

				shipmentGrid.AttachButtonForTest.PerformClick();
				using (var popup = shipmentGrid.LastShownAttachPopupForTesting)
				{
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { shipmentForForm });
				}

				AssertEquals("Shipment is attached to consol.", 1, shipmentGrid.InnerGrid.List.Count);
				AssertNull("Shipment job does not exist", new JobHeader.Loader(consolFormFactory, shipment).Load());

				var isFormSaved = consolForm.FireSaveButton();
				AssertEquals("Consol form is saved.", ContinueWithSave.Yes, isFormSaved);
				AssertNull("Shipment job does not exist", new JobHeader.Loader(bizObjCheckFactory, shipment).Load());
			}
		}

		public void TestShipmentJobHeaderIsDeletedOnSave_DoesNotTriggerException_OpeningUnsavedShipmentFromGrid()
		{
			AssertDeletingShipmentJobHeaderOnSaveDoesNotTriggerExceptionWhenOpeningShipmentFromGrid(false);
		}

		public void TestShipmentJobHeaderIsDeletedOnSave_DoesNotTriggerException_OpeningSavedShipmentFromGrid()
		{
			AssertDeletingShipmentJobHeaderOnSaveDoesNotTriggerExceptionWhenOpeningShipmentFromGrid(true);
		}

		void AssertDeletingShipmentJobHeaderOnSaveDoesNotTriggerExceptionWhenOpeningShipmentFromGrid(bool shouldSaveFactoryBeforeOpeningShipment)
		{
			// Arrange
			var testObjectCreator = new TestObjectCreator(Factory);
			var creditor = testObjectCreator.CreateOrgHeader("CREDITOR", true, true, "AUSYD");
			creditor.OH_IsConsignor = true;

			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			ForwardingShipment GetShipment(bool hasChargeable)
			{
				var shipment = testObjectCreator.CreateShipment(hasChargeable ? "S1" : "S2", consol);
				shipment.ConsignorPK = creditor.PK;
				shipment.JS_PackingMode = ContainerModes.Loose;
				shipment.JS_ActualChargeable = hasChargeable ? 100m : 0m;

				return shipment;
			}

			using (AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var shipment1 = GetShipment(true);
				var shipment2 = GetShipment(false);
				AssertNull("Pre-condition: Shipment Job is not created due to registry", shipment1.ShipmentJobHeader);
				AssertNull("Pre-condition: Shipment Job is not created due to registry", shipment2.ShipmentJobHeader);
				Factory.Save();

				using (var consolForm = new ConsolForm(consol))
				{
					var shipmentGrid = consolForm.ConsolControl.ShipmentModuleButtonGrid;
					shipmentGrid.InnerGrid.CopyCaptionsToPropertyHumanReadableNameForTest = true;
					shipmentGrid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(col => col.ColumnName.Contains("ShipmentJobHeader")).IsVisible = true;

					consolForm.Show();

					#region Add consol costs to the Accounting tab

					var consolCostingPlugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Apportionment);
					consolCostingPlugin.SelectTabPage();
					AssertNotNull("Selecting the Apportionment tab should create jobs for all shipments", shipment1.ShipmentJobHeader);
					AssertNotNull("Selecting the Apportionment tab should create jobs for all shipments", shipment2.ShipmentJobHeader);
					var originalShipmentJobHeader = shipment2.ShipmentJobHeader;

					shipment1.ShipmentJobHeader.LocalChargesPK = creditor.PK;
					shipment2.ShipmentJobHeader.LocalChargesPK = creditor.PK;

					var consolCost = testObjectCreator.CreateConsolCost(consol, testObjectCreator.FRT, creditor);
					consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
					consolCost.E6_OSCostAmount = 100m;
					consolCost.E6_RX_NKCurrency = CurrencyCodes.UnitedStates;

					var apportionedCharges = consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().ToArray();
					AssertEquals("Consolcost should be apportioned to both shipments.", 2, apportionedCharges.Length);

					var shipment1Charges = apportionedCharges.Where(c => c.JR_JH == shipment1.ShipmentJobHeader.PK).ToArray();
					var shipment2Charges = apportionedCharges.Where(c => c.JR_JH == shipment2.ShipmentJobHeader.PK).ToArray();
					AssertEquals(1, shipment1Charges.Length);
					AssertEquals(1, shipment2Charges.Length);
					AssertEquals(true, shipment1Charges[0].JR_IsUsedForApportionment);
					shipment2Charges[0].JR_IsUsedForApportionment = false;

					#endregion

					if (shouldSaveFactoryBeforeOpeningShipment)
					{
						Factory.Save();
					}

					shipmentGrid.InnerGrid.Select(1); //select the row of shipmentNotToBeApportioned
					var toolStrip = (ZToolStrip)shipmentGrid.Controls.Find("toolStrip", true)[0];
					var editButton = (ZToolStripButton)toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true)[0];

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					//Act
					AssertNoExceptionThrown(editButton.PerformClick);

					//Assert
					if (!shouldSaveFactoryBeforeOpeningShipment)
					{
						var expectedMessage = "The form must be saved before a Shipment can be edited or a new Shipment can be created. Do you wish to save the form?";
						AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}

					using (var shipmentForm = shipmentGrid.LastShownZForm)
					{
						AssertNotNull(shipmentForm);
						AssertType(typeof(ShipmentForm), shipmentForm);
					}

					AssertEquals(false, shipment2.HasChanges);
					AssertEquals("Expected to be deleted by ReleaseMutexesOnUnusedJobs because it was created automatically and has no apportioned consol costs", true, originalShipmentJobHeader.IsDeleted);
					AssertNotEquals("Since we deleted the original ShipmentJobHeader, a new job should be created", originalShipmentJobHeader.PK, shipment2.ShipmentJobHeader.PK);
				}
			}
		}

		public void TestJobIsSavedWhenItIsCreatedThroughCreateJobInvoicingRecordMenuItem()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consignor = testObjectCreator.CreateOrgHeader("ORGCNR", false, false, "AUSYD");
			consignor.OH_IsConsignor = true;
			var consignee = testObjectCreator.CreateOrgHeader("ORGCNE", false, false, "NZAKL");
			consignee.OH_IsConsignee = true;
			var gatewayConsol = testObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			testObjectCreator.CreateJob(gatewayConsol);
			var shipment = testObjectCreator.CreateShipment("S0001", gatewayConsol);
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			Factory.Save();

			var bizObjCheckFactory = new BusinessObjectFactory();
			AssertNull("Precondition : Shipment job does not exist", new JobHeader.Loader(bizObjCheckFactory, shipment).Load());
			AssertNotNull("Precondition : Gateway job exist", new JobHeader.Loader(bizObjCheckFactory, gatewayConsol).Load());

			var consolFormFactory = new BusinessObjectFactory();
			var gatewayConsolForForm = consolFormFactory.Load<ForwardingConsol>(gatewayConsol.PK);
			using (var consolForm = new ConsolForm(gatewayConsolForForm))
			{
				consolForm.Show();
				var shipmentGrid = consolForm.ConsolControl.ShipmentModuleButtonGrid.InnerGrid;
				shipmentGrid.UnSelectAll();
				shipmentGrid.Select(0);

				var createdJobInvoicingRecordMenuItem = shipmentGrid.ContextMenu.MenuItems.FindByText("Create Job Invoicing Record");
				AssertNotNull(createdJobInvoicingRecordMenuItem);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				createdJobInvoicingRecordMenuItem.PerformClick();
				AssertEquals(@"Job Invoicing Record has been created for the following shipment(s):
- Shipment S0001
", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull("Shipment job is created", new JobHeader.Loader(consolFormFactory, shipment).Load());

				var isFormSaved = consolForm.FireSaveButton();
				AssertEquals("Consol form is saved.", ContinueWithSave.Yes, isFormSaved);
				AssertNotNull("Shipment job is saved to DB", new JobHeader.Loader(bizObjCheckFactory, shipment).Load());
			}
		}

		public void TestDisposeAviationSecuritySupport()
		{
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Sea);
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Air);
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Road);
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Rail);
			AssertDisposeAviationSecuritySupportWithTransportMode(Constants.TransportModes.Courier);
		}

		void AssertDisposeAviationSecuritySupportWithTransportMode(string transportMode)
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var consol = shipment.Consols.AddNew();

			using (var frm = new ConsolForm(consol))
			{
				frm.Show();
			}

			Assert("Should have no leaked one or more disposable objects that have not been collected by the GC.", true);
		}

		#region Gateway - Show Billing and Apportionment Tabs

		public void TestGatewayConsolNotSavedAndShowErrorMessageWhenMutexException_AgentWithHandlingType()
		{
			TestGatewayConsolNotSavedAndShowErrorMessageWhenMutexException(Constants.AgentType.Agent);
		}

		public void TestGatewayConsolNotSavedAndShowErrorMessageWhenMutexException_CoLoadWithHandlingType()
		{
			TestGatewayConsolNotSavedAndShowErrorMessageWhenMutexException(Constants.AgentType.CoLoad);
		}

		void TestGatewayConsolNotSavedAndShowErrorMessageWhenMutexException(string gatewayType)
		{
			var gatewayDepartmentQuery = new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, "G");
			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(gatewayDepartmentQuery);

			using (new TemporaryUserContext { DepartmentPK = gatewayDepartment.PK.ToGuid() }.Set())
			using (FreightDataRegistry.Instance.SuppressValidationOfConsolsSendingOrReceivingAgents.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_UniqueConsignRef = "C10011991";
				consol.JK_AgentType = gatewayType;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
				gatewayAgentPort.O5_PortOrCountry = "AUSYD";
				gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
				gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

				Assert("Pre-condition", consol.IsGateway());

				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				orgProxy.OH_IsConsignee = true;

				var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment1.JS_TransportMode = Constants.TransportModes.Air;
				shipment1.JS_ActualWeight = 20m;
				shipment1.JS_ActualVolume = 1m;
				shipment1.ConsigneePK = orgProxy.PK;
				consol.Shipments.Add(shipment1);

				Factory.Save();

				consol.JK_MasterBillNum = "08155555625";
				consol.JK_AgentType = gatewayType;
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_ConsolMode = "FCL";
				consol.Transports[0].JW_ETA = DateTime.Today;
				consol.Transports[0].JW_ETD = DateTime.Today;
				consol.Transports[0].JW_VoyageFlight = "JK584";
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = "GTA";

				var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
				port.O5_PortOrCountry = consol.JK_RL_NKLoadPort;
				port.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
				port.O5_SeaAgentStatus = "GTA";
				consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

				var testObjectCreator = new TestObjectCreator(Factory);

				using (var gatewayJob = testObjectCreator.CreateJob(consol))
				{
					gatewayJob.JH_OA_LocalChargesAddr = consol.JK_OA_SendingForwarderAddress;
					gatewayJob.JH_GE = testObjectCreator.GEADepartment.PK;
					gatewayJob.JH_GS_NKRepSales = GlbStaff.CurrentUser.GS_Code;

					var charge = gatewayJob.Charges.AddNew();
					charge.JR_AC = testObjectCreator.CC1.PK;
					charge.JR_GE = testObjectCreator.GEADepartment.PK;
					charge.JR_OH_SellAccount = GlbCompany.CurrentCompany.OrgProxy.PK;
					charge.JR_LocalSellAmt = 250m;
					charge.JR_JH_InternalJob = gatewayJob.PK;
					charge.JR_SellRatingOverrideComment = "reason 01";

					var newFactory = new BusinessObjectFactory();
					var shipmentFactory = newFactory.Load<ForwardingShipment>(shipment1.PK);

					UnitTestUserNotification.Instance.ClearMessages();

					using (new JobHeader.Loader(shipmentFactory).TryCreateWithMutex(GlbBranch.CurrentBranch))
					using (var consolForm = new ConsolForm(consol))
					{
						consolForm.Show();
						AssertNoExceptionThrown(() =>
						{
							var result = consolForm.FireSaveButton();
							AssertEquals(ContinueWithSave.No, result);
						});

						var expectedMessage = string.Format("You have created the job {0} on another form, but haven't saved it yet.\r\nPlease close or save other forms that use job {0} to continue.", shipment1.JobNumber);
						AssertEquals("An Error message should be shown to the user if he try to save a job already used by another user", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		public void TestShipmentCustomField_TypeChanged()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			template.P0_IsActive = true;

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "CustomString";
			genCustomColumnString.XC_Type = "STR";
			template.GenCustomColumnDefinitions.Add(genCustomColumnString);

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.FillWithValidTestData();

			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();

			var customFields = ((ICustomFieldProvider)shipment).GetCustomBusinessObject();
			var propertyName_Str = ((IDynamicBusinessObject)customFields).PropertyNames[0];
			customFields[propertyName_Str] = "NH";

			Factory.Save();

			genCustomColumnString.XC_Type = "BOO";
			Factory.Save();

			var consolCopy = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			using (var consolForm = new ConsolForm(consolCopy))
			{
				var shipmentGrid = (ConsolShipmentModuleButtonGrid)consolForm.Controls.Find("ShipmentModuleButtonGrid", true)[0];
				foreach (var col in shipmentGrid.ColumnStyles)
				{
					if (col is ZGridColumnInfo && (((ZGridColumnInfo)col).Caption == "CustomString" || ((ZGridColumnInfo)col).Caption == "CustomBool"))
					{
						((ZGridColumnInfo)col).IsVisible = true;
					}
				}

				consolForm.MaximumSize = new System.Drawing.Size(1920, 768);
				consolForm.Size = consolForm.MaximumSize;
				consolForm.Show();
				Application.DoEvents();

				customFields = ((ICustomFieldProvider)consolCopy.Shipments[0]).GetCustomBusinessObject();
				var propertyName_StrToBoo = ((IDynamicBusinessObject)customFields).PropertyNames[0];
				AssertEquals("String property should be converted to Bool correctly", false, customFields[propertyName_StrToBoo]);
			}
		}

		[ExpectNoExceptions]
		public void TestShipmentCustomFields_WithSameNameButDiffrentTypes()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			template1.P0_IsActive = true;
			template1.P0_SubType1 = "AIR";

			var genCustomColumnString = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnString.XC_Name = "CustomField";
			genCustomColumnString.XC_Type = "STR";
			template1.GenCustomColumnDefinitions.Add(genCustomColumnString);

			Factory.Save();

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = JobInvoicingConsumerTypes.Shipment.Code;
			template2.P0_IsActive = true;
			template2.P0_SubType1 = "SEA";

			var genCustomColumnBool = Factory.New<GenCustomColumnDefinition>();
			genCustomColumnBool.XC_Name = "CustomField";
			genCustomColumnBool.XC_Type = "Boo";
			template2.GenCustomColumnDefinitions.Add(genCustomColumnBool);

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.FillWithValidTestData();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.FillWithValidTestData();
			shipment1.JS_TransportMode = "AIR";

			Factory.Save();

			var customField1 = ((ICustomFieldProvider)shipment1).GetCustomBusinessObject();
			var propertyName_Str = ((IDynamicBusinessObject)customField1).PropertyNames[0];
			customField1[propertyName_Str] = "NH";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.FillWithValidTestData();
			shipment2.JS_TransportMode = "SEA";

			Factory.Save();

			var customField2 = ((ICustomFieldProvider)shipment2).GetCustomBusinessObject();
			var propertyName_Bool = ((IDynamicBusinessObject)customField2).PropertyNames[0];
			customField2[propertyName_Bool] = true;

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var consolCopy = factory2.Load<ForwardingConsol>(consol.PK);
			using (var consolForm = new ConsolForm(consolCopy))
			{
				var shipmentGrid = (ConsolShipmentModuleButtonGrid)consolForm.Controls.Find("ShipmentModuleButtonGrid", true)[0];
				int customColumnCount = 0;
				foreach (var col in shipmentGrid.ColumnStyles)
				{
					if (col is ZGridColumnInfo && (((ZGridColumnInfo)col).Caption?.StartsWith("CustomField") ?? false))
					{
						((ZGridColumnInfo)col).IsVisible = true;
						customColumnCount++;
					}
				}

				consolForm.MaximumSize = new System.Drawing.Size(1920, 768);
				consolForm.Size = consolForm.MaximumSize;
				consolForm.Show();
				Application.DoEvents();

				AssertEquals("ShipmentGrid should have 2 custom columns", 2, customColumnCount);

				customField1 = ((ICustomFieldProvider)consolCopy.Shipments[0]).GetCustomBusinessObject();
				var propertyName = ((IDynamicBusinessObject)customField1).PropertyNames[0];
				if (propertyName == "__CUSTOMFIELD__prop__ZString")
				{
					customField1[propertyName] = "Changed";
				}
				else if (propertyName == "__CUSTOMFIELD__prop__ZBool")
				{
					customField1[propertyName] = false;
				}
				shipmentGrid.Refresh();

				customField2 = ((ICustomFieldProvider)consolCopy.Shipments[1]).GetCustomBusinessObject();
				propertyName = ((IDynamicBusinessObject)customField2).PropertyNames[0];

				if (propertyName == "__CUSTOMFIELD__prop__ZString")
				{
					customField2[propertyName] = "Changed";
				}
				else if (propertyName == "__CUSTOMFIELD__prop__ZBool")
				{
					customField2[propertyName] = false;
				}
				shipmentGrid.Refresh();

				factory2.Save();
			}

			consolCopy = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			customField1 = ((ICustomFieldProvider)consolCopy.Shipments[0]).GetCustomBusinessObject();
			var propName = ((IDynamicBusinessObject)customField1).PropertyNames[0];
			if (propName == "__CUSTOMFIELD__prop__ZString")
			{
				AssertEquals("The custom field should be changed correctly", "Changed", customField1[propName]);
			}
			else if (propName == "__CUSTOMFIELD__prop__ZBool")
			{
				AssertEquals("The custom field should be changed correctly", false, customField1[propName]);
			}

			customField2 = ((ICustomFieldProvider)consolCopy.Shipments[1]).GetCustomBusinessObject();
			propName = ((IDynamicBusinessObject)customField2).PropertyNames[0];

			if (propName == "__CUSTOMFIELD__prop__ZString")
			{
				AssertEquals("The custom field should be changed correctly", "Changed", customField2[propName]);
			}
			else if (propName == "__CUSTOMFIELD__prop__ZBool")
			{
				AssertEquals("The custom field should be changed correctly", false, customField2[propName]);
			}
		}

		public void TestBillingAndApportionmentTabs_InteractWithGatewayJobAndCosts_AgentWithHandlingType()
		{
			TestBillingAndApportionmentTabs_InteractWithGatewayJobAndCosts(Constants.AgentType.Agent);
		}

		public void TestBillingAndApportionmentTabs_InteractWithGatewayJobAndCosts_CoLoadWithHandlingType()
		{
			TestBillingAndApportionmentTabs_InteractWithGatewayJobAndCosts(Constants.AgentType.CoLoad);
		}

		void TestBillingAndApportionmentTabs_InteractWithGatewayJobAndCosts(string gatewayType)
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			orgProxy.OH_IsConsignee = true;
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_UniqueConsignRef = "C00001111";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = orgProxy.PK;
			consol.Shipments.Add(shipment);

			consol.JK_OA_SendingForwarderAddress = orgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertNotNull("Precondition", consol.SendingForwarder);

			var orgAppointedAgentPorts1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.JK_AgentType = gatewayType;

			Assert(consol.IsGateway());

			using (var job = new JobHeader.Loader(consol).TryCreateWithMutex(GlbBranch.CurrentBranch))
			{
				job.JH_GE = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GEA").PK;
				var chargesProperty = (PropertyInfo)job.GetType().GetMember("Charges")[0];
				var collection = (BusinessObjectCollection)chargesProperty.GetValue(job);
				var charge = (JobCharge)collection.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_JH = job.PK;
				charge.JR_OH_SellAccount = orgProxy.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;

				Factory.Save();

				Assert(consol.HasConsolCosts(GlbCompany.CurrentCompany));
			}

			var loadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);

			using (var consolForm = new ConsolForm(loadedConsol))
			{
				Assert(loadedConsol.IsGateway());
				AssertStandardApportionmentPlugInTabShown(consolForm, isShownExpected: true);
				AssertGatewaySellApportionmentPlugInTabShown(consolForm, isShownExpected: true);
				AssertGatewayJobInvoicingPlugInTabShown(consolForm, isShownExpected: true);

				loadedConsol.JK_AgentType = Constants.AgentType.Courier;
				Assert("Is no longer gateway due to the agent type being changed, despite having a job and charges", !loadedConsol.IsGateway());
				Assert("should not have a legacy billing job", !loadedConsol.IsLegacyGateway);
				AssertNotEquals(((IOrgHeader)null, (IOrgHeader)null), ((IGateway)loadedConsol).GatewayBillingSupporter.GatewayAgent());
				AssertStandardApportionmentPlugInTabShown(consolForm, isShownExpected: true);
				AssertGatewaySellApportionmentPlugInTabShown(consolForm, isShownExpected: false);
				AssertGatewayJobInvoicingPlugInTabShown(consolForm, isShownExpected: false);
			}
		}

		public void TestBillingAndApportionmentTabs_ReceivingAgentRefreshesGateway_AgentWithHandlingType()
		{
			TestBillingAndApportionmentTabs_ReceivingAgentRefreshesGateway(Constants.AgentType.Agent);
		}

		public void TestBillingAndApportionmentTabs_ReceivingAgentRefreshesGateway_CoLoadWithHandlingType()
		{
			TestBillingAndApportionmentTabs_ReceivingAgentRefreshesGateway(Constants.AgentType.CoLoad);
		}

		void TestBillingAndApportionmentTabs_ReceivingAgentRefreshesGateway(string gatewayType)
		{
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GES"));
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, department.PK.ToGuid()))
			{
				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				orgProxy.OH_IsConsignee = true;

				var agentPorts = orgProxy.AppointedGatewayAgentPorts.AddNew();
				agentPorts.O5_OA_AgentOfficeAddress = orgProxy.MainAddress.PK;
				agentPorts.O5_PortOrCountry = "AU";
				agentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
				agentPorts.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_AgentType = gatewayType;
				consol.JK_RL_NKLoadPort = "GNSBY";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_ReceivingForwarderAddress = orgProxy.MainAddress.PK;
				consol.JK_ReceivingForwarderHandlingType = ZString.Empty;
				Assert(!consol.IsGateway());
				using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
				{
					using (var consolForm = new ConsolForm(consol))
					{
						AssertStandardApportionmentPlugInTabShown(consolForm, isShownExpected: true);
						AssertGatewaySellApportionmentPlugInTabShown(consolForm, isShownExpected: false);
						AssertGatewayJobInvoicingPlugInTabShown(consolForm, isShownExpected: false);
					}

					consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
					Assert(consol.IsGateway());

					using (var consolForm = new ConsolForm(consol))
					{
						AssertStandardApportionmentPlugInTabShown(consolForm, isShownExpected: true);
						AssertGatewaySellApportionmentPlugInTabShown(consolForm, isShownExpected: true);
						AssertGatewayJobInvoicingPlugInTabShown(consolForm, isShownExpected: true);
					}
				}
			}
		}

		public void TestBillingAndApportionmentTabs_SendingAgentRefreshesGateway_AgentWithHandlingType()
		{
			TestBillingAndApportionmentTabs_SendingAgentRefreshesGateway(Constants.AgentType.Agent);
		}

		public void TestBillingAndApportionmentTabs_SendingAgentRefreshesGateway_CoLoadWithHandlingType()
		{
			TestBillingAndApportionmentTabs_SendingAgentRefreshesGateway(Constants.AgentType.CoLoad);
		}

		void TestBillingAndApportionmentTabs_SendingAgentRefreshesGateway(string gatewayType)
		{
			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GES"));
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, department.PK.ToGuid()))
			{
				var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				orgProxy.OH_IsConsignee = true;

				var agentPorts = orgProxy.AppointedGatewayAgentPorts.AddNew();
				agentPorts.O5_OA_AgentOfficeAddress = orgProxy.MainAddress.PK;
				agentPorts.O5_PortOrCountry = "GN";
				agentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
				agentPorts.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_AgentType = gatewayType;
				consol.JK_RL_NKLoadPort = "GNSBY";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_SendingForwarderAddress = orgProxy.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = ZString.Empty;
				Assert(!consol.IsGateway());
				using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
				{
					using (var consolForm = new ConsolForm(consol))
					{
						AssertStandardApportionmentPlugInTabShown(consolForm, isShownExpected: true);
						AssertGatewaySellApportionmentPlugInTabShown(consolForm, isShownExpected: false);
						AssertGatewayJobInvoicingPlugInTabShown(consolForm, isShownExpected: false);
					}

					consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
					Assert(consol.IsGateway());
					using (var consolForm = new ConsolForm(consol))
					{
						AssertStandardApportionmentPlugInTabShown(consolForm, isShownExpected: true);
						AssertGatewaySellApportionmentPlugInTabShown(consolForm, isShownExpected: true);
						AssertGatewayJobInvoicingPlugInTabShown(consolForm, isShownExpected: true);
					}
				}
			}
		}

		public void TestBillingAndApportionmentTabs_InteractWithConsolCosts_AgentWithHandlingType()
		{
			TestBillingAndApportionmentTabs_InteractWithConsolCosts(Constants.AgentType.Agent);
		}

		public void TestBillingAndApportionmentTabs_InteractWithConsolCosts_CoLoadWithHandlingType()
		{
			TestBillingAndApportionmentTabs_InteractWithConsolCosts(Constants.AgentType.CoLoad);
		}

		void TestBillingAndApportionmentTabs_InteractWithConsolCosts(string gatewayType)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_UniqueConsignRef = "C00001111";

			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			var orgAppointedAgentPorts1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var consolCost = CreateConsolCost(consol);
			Factory.Save();
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				using (var consolForm = new ConsolForm(consol))
				{
					Assert("Consol should be considered gateway ignoring existing consol costs because registry is ON", consol.IsGateway());
					AssertStandardApportionmentPlugInTabShown(consolForm, isShownExpected: true);
					AssertGatewaySellApportionmentPlugInTabShown(consolForm, isShownExpected: true);
					AssertGatewayJobInvoicingPlugInTabShown(consolForm, isShownExpected: true);

					consol.JK_AgentType = Constants.AgentType.Direct;
					consolCost.Delete();
					consol.JK_AgentType = gatewayType;
					consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

					Assert(consol.IsGateway());
					AssertStandardApportionmentPlugInTabShown(consolForm, isShownExpected: true);
					AssertGatewaySellApportionmentPlugInTabShown(consolForm, isShownExpected: true);
					AssertGatewayJobInvoicingPlugInTabShown(consolForm, isShownExpected: true);
				}
			}
		}

		BusinessObject CreateConsolCost(ForwardingConsol consol)
		{
			var consolCostType = ObjectFactory.GetType<IJobConsolCost>();
			BusinessObject consolCost = Factory.NewWithValidTestData(consolCostType);
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = consol.TablePrefix;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}

			return consolCost;
		}

		public void TestBillingAndApportionmentTabs_GatewayShownForLegacyGateway()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_UniqueConsignRef = "C11111111";

			var job = Factory.NewJobForTesting<JobHeader>();
			job.Parent = consol;
			job.JH_JobNum += Constants.GatewaySuffixForJobHeaderDeprecated;

			Factory.Save();

			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK); // Reload Consol in a new Factory to avoid caching IsLegacyGateway value
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (var consolForm = new ConsolForm(consol))
			{
				Assert(consol.IsLegacyGateway);
				Assert(!consol.IsGateway());

				AssertStandardApportionmentPlugInTabShown(consolForm, isShownExpected: true);
				AssertGatewaySellApportionmentPlugInTabShown(consolForm, isShownExpected: true);
				AssertGatewayJobInvoicingPlugInTabShown(consolForm, isShownExpected: true);
			}
		}

		public void TestBillingAndApportionmentTabs_GatewayShouldBeShownWhenJobExists_GatewayAgent()
		{
			TestBillingAndApportionmentTabs_GatewayShouldBeShownWhenJobExists(Constants.AgentType.Agent);
		}

		public void TestBillingAndApportionmentTabs_GatewayShouldBeShownWhenJobExists_GatewayCoLoad()
		{
			TestBillingAndApportionmentTabs_GatewayShouldBeShownWhenJobExists(Constants.AgentType.CoLoad);
		}

		void TestBillingAndApportionmentTabs_GatewayShouldBeShownWhenJobExists(string gatewayType)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = gatewayType;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C11111111";
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var orgProxy = GlbBranch.CurrentBranch.OrgProxy;
			consol.JK_OA_SendingForwarderAddress = orgProxy.MainAddress.PK;

			var orgAppointedAgentPorts3 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts3.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts3.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts3.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts3.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Assert(consol.IsGateway());

			using (new JobHeader.Loader(consol).TryCreateWithMutex(GlbBranch.CurrentBranch))
			{
				Factory.Save();
			}

			var newBusinessObjectFactory = new BusinessObjectFactory();
			consol = newBusinessObjectFactory.Load<ForwardingConsol>(consol.PK);

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				using (var consolForm = new ConsolForm(consol))
				{
					AssertStandardApportionmentPlugInTabShown(consolForm);
					AssertGatewaySellApportionmentPlugInTabShown(consolForm);
					AssertGatewayJobInvoicingPlugInTabShown(consolForm, isShownExpected: true);

					consol.JK_AgentType = Constants.AgentType.Courier;

					Assert(!consol.IsGateway());
					AssertStandardApportionmentPlugInTabShown(consolForm);
					AssertGatewaySellApportionmentPlugInTabShown(consolForm, isShownExpected: false);
					AssertGatewayJobInvoicingPlugInTabShown(consolForm, isShownExpected: false);
				}
			}
		}

		public void TestOrderOfAccountingPluginTabs_WhenGatewayConsol()
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			orgProxy.OH_IsConsignee = true;
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_UniqueConsignRef = "C00001111";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = orgProxy.PK;
			consol.Shipments.Add(shipment);

			consol.JK_OA_SendingForwarderAddress = orgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertNotNull("Precondition", consol.SendingForwarder);

			var orgAppointedAgentPorts1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.JK_AgentType = Constants.AgentType.Agent;

			Assert(consol.IsGateway());

			using (var job = new JobHeader.Loader(consol).TryCreateWithMutex(GlbBranch.CurrentBranch))
			{
				job.JH_GE = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GEA").PK;
				var chargesProperty = (PropertyInfo)job.GetType().GetMember("Charges")[0];
				var collection = (BusinessObjectCollection)chargesProperty.GetValue(job);
				var charge = (JobCharge)collection.AddNew();
				charge.JR_AC = Env.Registry.FreightChargeCode;
				charge.JR_JH = job.PK;
				charge.JR_OH_SellAccount = orgProxy.PK;
				charge.JR_OSSellAmt = 250m;
				charge.JR_JH_InternalJob = job.PK;
				charge.JR_GB_InternalBranch = charge.JR_GB;
				charge.JR_GE_InternalDept = charge.JR_GE;

				Factory.Save();

				Assert(consol.HasConsolCosts(GlbCompany.CurrentCompany));
			}

			var loadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);

			using (var consolForm = new ConsolForm(loadedConsol))
			{
				Assert(loadedConsol.IsGateway());
				consolForm.Show();

				var accountingTabControl = consolForm.Controls.Find("AccountingTabControl", true).FirstOrDefault() as ZTabControl;
				AssertNotNull("Precondition: found Accounting tab control", accountingTabControl);

				var tabsInOrder = accountingTabControl.TabPages.Cast<TabPage>().Select(tp => tp.Text);
				var expectedTabsInOrder = new[]
				{
					"Gateway Billing",				// ControllerIDs.JobInvoicing
					"Gateway Sell Apportionment",	// ControllerIDs.SellApportionmentForGateway
					"Consol Costing",				// ControllerIDs.Apportionment
					"AR and AP Invoices",			// ControllerIDs.JobInvoicingConsol
					"Consol Profit/Loss",			// ControllerIDs.JobProfitLossConsol
				};
				AssertContainsExactElementsInExactOrder(expectedTabsInOrder, tabsInOrder);
			}
		}

		static void AssertGatewaySellApportionmentPlugInTabShown(ConsolForm consolForm, string message = "", bool isShownExpected = true)
		{
			var sellApportionmentPlugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.SellApportionmentForGateway);

			if (isShownExpected)
			{
				Assert(message, sellApportionmentPlugIn.Enabled);
				AssertNull("Expect Gateway sell apportionment plugin should have no menu", sellApportionmentPlugIn.TopLevelMenu);
			}
			else
			{
				Assert(message, !sellApportionmentPlugIn.Enabled);
			}
		}

		static void AssertStandardApportionmentPlugInTabShown(ConsolForm consolForm, string message = "", bool isShownExpected = true)
		{
			var apportionmentPlugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Apportionment);
			if (isShownExpected)
			{
				Assert(message, apportionmentPlugIn.Enabled);
				Assert(apportionmentPlugIn.TopLevelMenu.Visible);
				AssertEquals("&Job Invoicing", apportionmentPlugIn.TopLevelMenu.Text);
				AssertEquals("Consol Costing", apportionmentPlugIn.TabPage.Text);
			}
			else
			{
				Assert(message, !apportionmentPlugIn.Enabled);
			}
		}

		static void AssertGatewayJobInvoicingPlugInTabShown(ConsolForm consolForm, string message = "", bool isShownExpected = true)
		{
			var invoicingPlugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
			if (isShownExpected)
			{
				Assert(message, invoicingPlugIn.Enabled);
				AssertEquals("Gateway Billing", invoicingPlugIn.TabPage.Text);
			}
			else
			{
				Assert(message, !invoicingPlugIn.Enabled);
			}
		}

		#endregion

		#region Gateway - Show Apportion Menu Items

		public void TestShowApportionPluginMenuItems()
		{
			var creator = new TestObjectCreator(Factory);

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GES"));
			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, department.PK.ToGuid()))
			{
				var currentBranchOrgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
				currentBranchOrgProxy.OH_IsConsignee = true;
				var gatewayOrgProxy = creator.CreateOrgHeader("GTWORG", true, true);
				creator.CreateNewCompany("CGW", orgProxy: gatewayOrgProxy);

				foreach (var org in new[] { currentBranchOrgProxy, gatewayOrgProxy })
				{
					var agentPorts = org.AppointedGatewayAgentPorts.AddNew();
					agentPorts.O5_OA_AgentOfficeAddress = org.MainAddress.PK;
					agentPorts.O5_PortOrCountry = "GN";
					agentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
					agentPorts.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
				}

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = "GNSBY";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_OA_SendingForwarderAddress = gatewayOrgProxy.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = ZString.Empty;

				using (new JobHeader.Loader(consol).TryCreateWithMutex(GlbBranch.CurrentBranch))
				{
					Factory.Save();
				}

				var newBusinessObjectFactory = new BusinessObjectFactory();
				consol = newBusinessObjectFactory.Load<ForwardingConsol>(consol.PK);

				using (var consolForm = new ConsolForm(consol))
				using (var apportionmentPlugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Apportionment))
				{
					Assert(!consol.IsGateway());
					Assert(!consol.IsGatewayConsol);
					foreach (MenuItem menuItem in apportionmentPlugIn.TopLevelMenu.MenuItems)
					{
						AssertEquals($"This menu should be shown when Consol is not Gateway: {menuItem.Text}",
							menuItem.Visible,
							!menuItem.Text.Equals(Constants.MenuNameConstants.PostGatewayAgentCharges));
					}
				}

				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				using (var consolForm = new ConsolForm(consol))
				using (var apportionmentPlugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Apportionment))
				{
					Assert(!consol.IsGateway());
					Assert(consol.IsGatewayConsol);
					foreach (MenuItem menuItem in apportionmentPlugIn.TopLevelMenu.MenuItems)
					{
						Assert($"This menu should be shown when Consol is Gateway but Gateway Billing is disabled: {menuItem.Text}",
							menuItem.Visible);
					}
				}

				consol.JK_OA_SendingForwarderAddress = currentBranchOrgProxy.MainAddress.PK;
				using (var consolForm = new ConsolForm(consol))
				using (var apportionmentPlugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Apportionment))
				{
					Assert(consol.IsGateway());
					Assert(consol.IsGatewayConsol);
					foreach (MenuItem menuItem in apportionmentPlugIn.TopLevelMenu.MenuItems)
					{
						AssertEquals($"This menu should be shown when Gateway Billing is enabled: {menuItem.Text}",
							true, menuItem.Visible);
					}
				}
			}
		}

		#endregion

		public void TestBusyIndicatorProviderIsRegistered()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (var form = new ConsolForm(consol))
			{
				AssertEquals(true, Factory.GetValue<IBusyIndicatorProvider>() is BusyIndicatorProvider);
			}
		}

		public void TestChangingShipmentConsigneeConsignorDoesNotThrowException()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			using (var consolForm = new ConsolForm(consol))
			{
				AssertNoExceptionThrown(() => shipment.ConsignorPK = Factory.New<OrgHeader>().PK);
				AssertNoExceptionThrown(() => shipment.ConsigneePK = Factory.New<OrgHeader>().PK);
			}
		}

		public void TestGuiFactoryServices()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			IForwardingConsolDocumentSupporterQueryProvider consolDocSupporterQueryProvider = Factory.GetValue<IForwardingConsolDocumentSupporterQueryProvider>();
			AssertNull("consol doc supporter", consolDocSupporterQueryProvider);

			IForwardingShipmentDocumentSupporterQueryProvider shipmentDocSupporterQueryProvider = Factory.GetValue<IForwardingShipmentDocumentSupporterQueryProvider>();
			AssertNull("shipment doc supporter", shipmentDocSupporterQueryProvider);

			IServicesSelectionProvider servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
			AssertNull("services selection provider", servicesSelectionProvider);

			using (new ConsolForm(consol))
			{
				consolDocSupporterQueryProvider = Factory.GetValue<IForwardingConsolDocumentSupporterQueryProvider>();
				AssertNotNull("consol doc supporter", consolDocSupporterQueryProvider);
				Assert(consolDocSupporterQueryProvider is ForwardingConsolDocumentSupporterGuiQueryProvider);

				shipmentDocSupporterQueryProvider = Factory.GetValue<IForwardingShipmentDocumentSupporterQueryProvider>();
				AssertNotNull("shipment doc supporter", shipmentDocSupporterQueryProvider);
				Assert(shipmentDocSupporterQueryProvider is ForwardingShipmentDocumentSupporterGuiQueryProvider);

				servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull("services selection provider", servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		JobSailing CreateJobSailing(ZString flightNumber, ZString loadingPort, ZString dischargePort, ZDateTime etdDate, ZDateTime etaDate, bool saveData = true)
		{
			var carrierOrganisation = Factory.NewWithValidTestData<OrgHeader>();
			carrierOrganisation.OH_Code = "CMAC";

			var cusCode = carrierOrganisation.CustomsCodes.AddNew(
				OrgCusCode.CodeTypes.CarrierCode,
				"CMAC",
				Core.Constants.CountryCodes.UnitedStates);

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = flightNumber;
			voyage.JV_OH_Line = carrierOrganisation.PK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = loadingPort;
			origin.JA_E_DEP = etdDate;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = dischargePort;
			destination.JB_E_ARV = etaDate;
			voyage.GenerateSailings();

			if (saveData)
			{
				Factory.Save();
			}

			return voyage.Sailings[0];
		}

		public void TestConfirmOriginAndDestinationUpdate()
		{
			MockOnlineSailingSchedulesDataVendor.RegisterThisSubTypeOverride();

			try
			{
				MockOnlineSailingSchedulesDataVendor.Instance.SetRoutesProvider(new EmptyRoutesProviderForTest());

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = "SEA";
				consol.JK_ConsolMode = "FCL";
				consol.JK_RL_NKLoadPort = "NLAMS";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.MasterBillAirlinePrefix = "333";

				var sailing = CreateJobSailing("52W", "NLAMS", "USLAX", new ZDateTime(2017, 8, 4), new ZDateTime(2017, 9, 5), false);

				var transport = consol.Transports[0];
				transport.JW_JX = sailing.PK;
				transport.JW_IsLinked = true;
				consol.Shipments.Add(shipment);
				Factory.Save();

				AssertEquals("Prerequisite", null, Factory.GetValue<IConfirmationProvider>());

				using (var form = new ConsolForm(consol))
				{
					AssertEquals(true, Factory.GetValue<IConfirmationProvider>() is ConfirmationProvider);

					form.Show();

					MockOnlineSailingSchedulesDataVendor.Instance.SetRoutesProvider(new RoutesProviderForTest());

					var expectedMessage = @"Question Global Sailing Schedules module could not find NLAMS 04-Aug-17 00:00:00 to USLAX 05-Sep-17 00:00:00 route.
Related port routing is found for NLRTM 10-Aug-17 00:00:00 to USLGB 15-Aug-17 00:00:00.
Would you like to update your routing details with the related ports?";

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					transport.JW_Vessel = "VESSEL NAME";
					AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals(transport.JW_RL_NKLoadPort, "NLAMS");
					AssertEquals(transport.JW_RL_NKDiscPort, "USLAX");
					AssertEquals(transport.JW_ETD, new ZDateTime(2017, 8, 4));
					AssertEquals(transport.JW_ETA, new ZDateTime(2017, 9, 5));
					AssertEquals(transport.JW_ETD, transport.Sailing.Origin.JA_E_DEP);
					AssertEquals(transport.JW_ETA, transport.Sailing.Destination.JB_E_ARV);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					transport.JW_Vessel = "NYK METEOR";
					AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals(transport.JW_RL_NKLoadPort, "NLRTM");
					AssertEquals(transport.JW_RL_NKDiscPort, "USLGB");
					AssertEquals(transport.JW_ETD, new ZDateTime(2017, 8, 10));
					AssertEquals(transport.JW_ETA, new ZDateTime(2017, 8, 15));
					AssertEquals(transport.JW_ETD, transport.Sailing.Origin.JA_E_DEP);
					AssertEquals(transport.JW_ETA, transport.Sailing.Destination.JB_E_ARV);

					expectedMessage = @"Question Global Sailing Schedules module could not find NLRTM 11-Nov-11 00:00:00 to USLGB 15-Aug-17 00:00:00 route.
Related port routing is found for NLRTM 10-Aug-17 00:00:00 to USLGB 15-Aug-17 00:00:00.
Would you like to update your routing details with the related ports?";
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					transport.JW_ETD = new ZDateTime(2011, 11, 11);
					AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.ToString());
					AssertEquals(transport.JW_RL_NKLoadPort, "NLRTM");
					AssertEquals(transport.JW_RL_NKDiscPort, "USLGB");
					AssertEquals(transport.JW_ETD, new ZDateTime(2017, 8, 10));
					AssertEquals(transport.JW_ETA, new ZDateTime(2017, 8, 15));
					AssertEquals(transport.JW_ETD, transport.Sailing.Origin.JA_E_DEP);
					AssertEquals(transport.JW_ETA, transport.Sailing.Destination.JB_E_ARV);
				}
			}
			finally
			{
				MockOnlineSailingSchedulesDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestDoNotShowConfirmationFormDuringTransaction()
		{
			MockOnlineSailingSchedulesDataVendor.RegisterThisSubTypeOverride();

			try
			{
				MockOnlineSailingSchedulesDataVendor.Instance.SetRoutesProvider(new EmptyRoutesProviderForTest());

				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = "SEA";
				consol.JK_ConsolMode = "FCL";
				consol.JK_RL_NKLoadPort = "NLAMS";
				consol.JK_RL_NKDischargePort = "USLAX";
				consol.MasterBillAirlinePrefix = "333";

				var sailing = CreateJobSailing("52W", "NLAMS", "USLAX", new ZDateTime(2017, 8, 4), new ZDateTime(2017, 9, 5), false);

				var transport = consol.Transports[0];
				transport.JW_JX = sailing.PK;
				transport.JW_IsLinked = true;
				consol.Shipments.Add(shipment);
				Factory.Save();

				AssertEquals("Prerequisite", null, Factory.GetValue<IConfirmationProvider>());

				using (var form = new ConsolForm(consol))
				{
					AssertEquals(true, Factory.GetValue<IConfirmationProvider>() is ConfirmationProvider);

					form.Show();

					MockOnlineSailingSchedulesDataVendor.Instance.SetRoutesProvider(new RoutesProviderForTest());

					Db.Connection.BeginTransaction();

					try
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						transport.JW_Vessel = "VESSEL NAME";
						AssertContains("", UnitTestUserNotification.Instance.LastMessage.ToString());
						AssertEquals(transport.JW_RL_NKLoadPort, "NLAMS");
						AssertEquals(transport.JW_RL_NKDiscPort, "USLAX");
						AssertEquals(transport.JW_ETD, new ZDateTime(2017, 8, 4));
						AssertEquals(transport.JW_ETA, new ZDateTime(2017, 9, 5));
						AssertEquals(transport.JW_ETD, transport.Sailing.Origin.JA_E_DEP);
						AssertEquals(transport.JW_ETA, transport.Sailing.Destination.JB_E_ARV);
					}
					finally
					{
						Db.Connection.RollbackTransaction();
					}
				}
			}
			finally
			{
				MockOnlineSailingSchedulesDataVendor.UnregisterThisSubTypeOverride();
			}
		}

		public void TestIsAnyShipmentOpenForEdit()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "LSE";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "QF123";
			consol.MasterBillAirlinePrefix = "333";
			transport.JW_ETD = ZDateTime.Today;
			consol.Shipments.Add(shipment);

			Factory.Save();

			using (var consolForm = new ConsolFormTestClass(consol))
			{
				consolForm.Show();
				Assert(!consolForm.IsAnyShipmentOpenForEdit);
				var grid = consolForm.ConsolControl.ShipmentModuleButtonGrid;
				AssertNotNull(grid);
				grid.InnerGrid.Select(0);
				var toolStrip = grid.Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
				var editButton = toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).OfType<ZToolStripButton>().First();
				AssertNotNull(editButton);
				try
				{
					editButton.PerformClick();
					Assert(consolForm.IsAnyShipmentOpenForEdit);
				}
				finally
				{
					grid.LastShownZForm.Dispose();
				}
			}
		}

		public void TestSpecialHandlingItems_WhenShipmentReattachedMultipleTimes()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNRAIR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNEAIR";
			consignee.OH_IsConsignee = true;

			shipment.JS_TransportMode = "AIR";
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.OuterPackLines.RemoveAll();
			var packline = shipment.OuterPackLines.AddNew();

			var undgForPackLine1 = packline.UNDGs.AddNew();
			var undgSubstance1 = Factory.New<UNDGSubstance>();
			undgSubstance1.DG_UNNO = "0001";
			undgSubstance1.DG_Code = "0001";
			undgSubstance1.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IATA;
			undgSubstance1.DG_CargoMaxAmt = 60.0;
			undgSubstance1.DG_CargoMaxAmtUQ = "KG";
			undgSubstance1.DG_LQ2OrPaxMaxAmt = 5.0;
			undgSubstance1.DG_LQ2OrPaxMaxAmtUQ = "KG";

			undgForPackLine1.DI_F3_NKPackType = "BOX";
			undgForPackLine1.DI_DGWeight = 20;
			undgForPackLine1.DI_UnitOfWeight = "KG";
			undgForPackLine1.DI_DG = undgSubstance1.PK;
			undgForPackLine1.DI_PackageCount = 1;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			Factory.Save();
			var errors = shipment.GetErrors();

			var shipmentController = ZControllerFactory.Create(ControllerIDs.JobShipment);
			var consolController = ZControllerFactory.Create(ControllerIDs.JobConsol);

			using (var shipmentForm = new ShipmentFormForTest(shipment))
			using (var consolForm = (ConsolForm)consolController.ShowEditForm(consol))
			{
				shipmentForm.Show();
				consolForm.Show();

				AssertEquals(ContinueWithSave.Yes, consolForm.FireSaveButton());

				var shipmentsGrid = consolForm.ConsolControl.ShipmentModuleButtonGrid;
				AssertEquals("Precondition: ConsolForm's shipments grid must be empty before attach", 0, shipmentsGrid.InnerGrid.ListManager.Count);

				var consolGrid = shipmentForm.ConsolUserControl.ConsolModuleButtonGrid;

				var shipmentToolStrip = consolGrid.Controls.Find("toolstrip", true).OfType<ZToolStrip>().First();
				var consolAttachButton = shipmentToolStrip.Items.Find(ZModuleButtonGrid.Buttons.Attach, true).OfType<ZToolStripButton>().First();
				var consolDettachButton = shipmentToolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();

				AssertEquals("Precondition: ShipmentForm's consol grid must be empty before attach", 0, consolGrid.InnerGrid.ListManager.Count);
				consolAttachButton.PerformClick();

				var consolForAttaching = new BusinessObject[] { consol };
				MethodInfo attacherSelectFunc = consolGrid.Attacher.LastShownAttachPopupForTesting.GetType().GetMethod("HandleSelection", BindingFlags.NonPublic | BindingFlags.Instance);
				attacherSelectFunc.Invoke(consolGrid.Attacher.LastShownAttachPopupForTesting, new object[] { consolForAttaching });

				AssertEquals(ContinueWithSave.Yes, shipmentForm.FireSaveButton());
				AssertEquals("ShipmentForm's consol grid must contain one consol after attach and save.", 1, consolGrid.InnerGrid.ListManager.Count);

				var specialHandlingControl = consolForm.ConsolControl.Controls.Find("SpecialHandlingUserControl", true).OfType<SpecialHandlingUserControl>().First();
				var specialHandlingGrid = specialHandlingControl.Controls.Find("Grid", true).OfType<ZGrid>().First();
				AssertEquals("specialHandlingGrid should be empty before opening the Docs tab page", null, specialHandlingGrid.ListManager);

				var docsTabPage = consolForm.FindAll<ZTabPage>().SingleOrDefault(n => n.CaptionResourceString.Caption == "Docs");
				docsTabPage.Show(); //Only after showing the docsTabPage binds the data source
	
				AssertEquals("specialHandlingGrid should have one special handling item as attach was successfull", 1, specialHandlingGrid.ListManager.Count);
				AssertEquals("ConsolForm's shipments grid must have one shipment", 1, shipmentsGrid.InnerGrid.ListManager.Count);
				AssertEquals(ContinueWithSave.Yes, consolForm.FireSaveButton());

				consolForm.ConsolControl.ShipmentModuleButtonGrid.InnerGrid.Select(consol.Shipments.IndexOf(x => x.PK == shipment.PK));
				var shipmentsToolStrip = shipmentsGrid.Controls.Find("toolstrip", true).OfType<ZToolStrip>().First();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var detachButton = shipmentsToolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true).OfType<ZToolStripButton>().First();
				detachButton.PerformClick();

				AssertEquals("ConsolForm's shipments grid must be empty after detach.", 0, shipmentsGrid.InnerGrid.ListManager.Count);

				specialHandlingGrid.Select(0);
				KeySender.PostKeyDown(specialHandlingGrid, Keys.Delete);
				Application.DoEvents();

				AssertEquals("Special handling items should be empty as we have deleted the entries from grid", 0, specialHandlingGrid.ListManager.Count);
				AssertEquals(ContinueWithSave.Yes, consolForm.FireSaveButton());
				AssertEquals("Special handling items should be empty after consol form save", 0, specialHandlingGrid.ListManager.Count);

				consolAttachButton.PerformClick();
				attacherSelectFunc.Invoke(consolGrid.Attacher.LastShownAttachPopupForTesting, new object[] { consolForAttaching });

				AssertEquals(ContinueWithSave.Yes, shipmentForm.FireSaveButton());
				AssertEquals("ConsolForm's shipments grid must contain one shipment after attach and save", 1, shipmentsGrid.InnerGrid.ListManager.Count);
				AssertEquals("Reattaching the consol should add only one special handling item", 1, specialHandlingGrid.ListManager.Count);
			}
		}

		public void TestMAWBUnallocation_Continue()
		{
			var consol = CreateMAWBWithConsol();
			var mawb = consol.MAWBAllocation.AllocatedMawb;
			using (new ConsolForm(consol))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				consol.JK_IsNeutralMaster = false;
				AssertEquals("This neutral MAWB has already been printed in final.\r\nYou should only deallocate this MAWB from the Consolidation if it was issued in error, if you have reason to use a new MAWB for this Consolidation, or the Consolidation was canceled.\r\n\r\nAre you sure you wish to deallocate this MAWB number?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, consol.MAWBAllocation.Parent.MAWBAllocation.GetShouldDeallocate());
				AssertEquals("081", consol.JK_MasterBillNum);
				AssertEquals(false, consol.IsNeutralMAWBPrinted);
			}
		}

		public void TestMAWBUnallocation_Cancel()
		{
			var consol = CreateMAWBWithConsol();
			var mawb = consol.MAWBAllocation.AllocatedMawb;
			using (new ConsolForm(consol))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				consol.JK_IsNeutralMaster = false;
				AssertEquals("This neutral MAWB has already been printed in final.\r\nYou should only deallocate this MAWB from the Consolidation if it was issued in error, if you have reason to use a new MAWB for this Consolidation, or the Consolidation was canceled.\r\n\r\nAre you sure you wish to deallocate this MAWB number?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(mawb, consol.MAWBAllocation.AllocatedMawb);
				AssertEquals("08155555625", consol.JK_MasterBillNum);
				AssertEquals(true, consol.IsNeutralMAWBPrinted);
			}
		}

		public void TestConsolMawbAllocationDifferentMawbIsAllocatedToDifferentConsols()
		{
			JobMawb mawb1 = AddMawb("081", "00000011", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb("081", "00000022", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			ForwardingConsol consol1 = factory1.New<ForwardingConsol>();
			ForwardingConsol consol2 = factory2.New<ForwardingConsol>();

			using (new ConsolForm(consol1))
			{
				consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol1.JK_RL_NKLoadPort = "AUBNE";
				consol1.JK_IsNeutralMaster = true;
				consol1.MasterBillAirlinePrefix = "081";

				factory1.Save();

				AssertEquals("00000011", consol1.MAWBAllocation.AllocatedMawb.JM_MAWB);

				using (new ConsolForm(consol2))
				{
					consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
					consol2.JK_RL_NKLoadPort = "AUBNE";
					consol2.JK_IsNeutralMaster = true;
					consol2.MasterBillAirlinePrefix = "081";
					factory2.Save();

					AssertEquals("00000022", consol2.MAWBAllocation.AllocatedMawb.JM_MAWB);
				}

				factory1.Save();
			}

			BusinessObjectFactory factory3 = new BusinessObjectFactory();

			consol1 = factory3.Load<ForwardingConsol>(consol1.PK);
			mawb1 = factory3.Load<JobMawb>(mawb1.PK);
			consol2 = factory3.Load<ForwardingConsol>(consol2.PK);
			mawb2 = factory3.Load<JobMawb>(mawb2.PK);

			AssertEquals(mawb1.JM_ParentID, consol1.PK);
			AssertEquals(mawb1.JM_ParentTableCode, consol1.Prefix);
			AssertEquals(mawb1, consol1.MAWBAllocation.AllocatedMawb);
			AssertEquals("08100000011", consol1.JK_MasterBillNum);

			AssertEquals(mawb2.JM_ParentID, consol2.PK);
			AssertEquals(mawb2.JM_ParentTableCode, consol2.Prefix);
			AssertEquals(mawb2, consol2.MAWBAllocation.AllocatedMawb);
			AssertEquals("08100000022", consol2.JK_MasterBillNum);
		}

		public void TestConsolMawbAllocationDeallocatedMawbDoesNotOverrideSelf()
		{
			AssertConsolMawbAllocationDeallocatedMawbDoesNotOverrideSelf(true);
		}

		public void TestConsolMawbAllocationDeallocatedMawbDoesNotOverrideSelfNoDataRefresh()
		{
			AssertConsolMawbAllocationDeallocatedMawbDoesNotOverrideSelf(false);
		}

		void AssertConsolMawbAllocationDeallocatedMawbDoesNotOverrideSelf(bool factoryDataRefreshEnabled)
		{
			JobMawb mawb1 = AddMawb("081", "001", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb("081", "002", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = factoryDataRefreshEnabled;

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = factoryDataRefreshEnabled;

			ForwardingConsol consol1 = factory1.New<ForwardingConsol>();
			ForwardingConsol consol2 = factory2.New<ForwardingConsol>();

			using (new ConsolForm(consol1))
			{
				JobMawb mawb1Factory1 = consol1.Factory.Load<JobMawb>(mawb1.PK);
				JobMawb mawb2Factory1 = consol1.Factory.Load<JobMawb>(mawb2.PK);

				consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol1.JK_RL_NKLoadPort = "AUBNE";
				consol1.JK_IsNeutralMaster = true;
				consol1.MasterBillAirlinePrefix = "081";

				consol1.Factory.Save();

				AssertEquals("mawb 001 allocated to consol 1", mawb1Factory1, consol1.MAWBAllocation.AllocatedMawb);

				consol1.JK_IsNeutralMaster = false;
				consol1.Factory.Save();

				AssertEquals("mawb 001 is not marked as printed", false, mawb1Factory1.JM_IsPrinted);

				using (new ConsolForm(consol2))
				{
					JobMawb mawb1Factory2 = consol2.Factory.Load<JobMawb>(mawb1.PK);

					consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
					consol2.JK_RL_NKLoadPort = "AUBNE";
					consol2.JK_IsNeutralMaster = true;
					consol2.MasterBillAirlinePrefix = "081";
					consol2.Factory.Save();
					AssertEquals("mawb 001 allocated to consol 2", mawb1Factory2, consol2.MAWBAllocation.AllocatedMawb);

					consol1.JK_IsNeutralMaster = true;
					consol1.Factory.Save();
					AssertEquals("mawb 002 allocated to consol 1", mawb2Factory1, consol1.MAWBAllocation.AllocatedMawb);

					mawb1Factory2.JM_IsPrinted = true;
					factory2.Save();
				}

				if (factoryDataRefreshEnabled)
				{
					AssertEquals("mawb 001 is marked as printed (updated by refresh)", true, mawb1Factory1.JM_IsPrinted);
					factory1.Save();
				}
				else
				{
					AssertEquals("mawb 001 is not marked as printed (was not updated by refresh)", false, mawb1Factory1.JM_IsPrinted);
					factory1.Save();
				}
			}

			BusinessObjectFactory factory3 = new BusinessObjectFactory();

			consol1 = factory3.Load<ForwardingConsol>(consol1.PK);
			mawb1 = factory3.Load<JobMawb>(mawb1.PK);
			consol2 = factory3.Load<ForwardingConsol>(consol2.PK);
			mawb2 = factory3.Load<JobMawb>(mawb2.PK);

			AssertEquals(mawb2.JM_ParentID, consol1.PK);
			AssertEquals(mawb2.JM_ParentTableCode, consol1.Prefix);
			AssertEquals(mawb2, consol1.MAWBAllocation.AllocatedMawb);
			AssertEquals("081002", consol1.JK_MasterBillNum);

			AssertEquals(mawb1.JM_ParentID, consol2.PK);
			AssertEquals(mawb1.JM_ParentTableCode, consol2.Prefix);
			AssertEquals(mawb1, consol2.MAWBAllocation.AllocatedMawb);
			AssertEquals("081001", consol2.JK_MasterBillNum);

			AssertEquals(true, mawb1.JM_IsPrinted);
		}

		public void TestConsolMawbAllocationDeallocatedMawbDoesNotOverrideSelfSaveConcurrencyCheckKicksIn()
		{
			JobMawb mawb1 = AddMawb("081", "001", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb("081", "002", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			ForwardingConsol consol1 = factory1.New<ForwardingConsol>();
			ForwardingConsol consol2 = factory2.New<ForwardingConsol>();

			using (new ConsolForm(consol1))
			{
				JobMawb mawb1Factory1 = consol1.Factory.Load<JobMawb>(mawb1.PK);
				JobMawb mawb2Factory1 = consol1.Factory.Load<JobMawb>(mawb2.PK);

				consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol1.JK_RL_NKLoadPort = "AUBNE";
				consol1.MasterBillAirlinePrefix = "081";
				consol1.JK_IsNeutralMaster = true;

				consol1.Factory.Save();
				mawb1Factory1.Reload();
				AssertEquals("mawb 001 allocated to consol 1", mawb1Factory1, consol1.MAWBAllocation.AllocatedMawb);

				consol1.JK_IsNeutralMaster = false;
				consol1.Factory.Save();

				AssertEquals("mawb 001 deallocated from consol 1", "", consol1.MasterBillMAWB);

				using (new ConsolForm(consol2))
				{
					JobMawb mawb1Factory2 = consol2.Factory.Load<JobMawb>(mawb1.PK);

					consol2.JK_TransportMode = Core.Constants.TransportModes.Air;
					consol2.JK_RL_NKLoadPort = "AUBNE";
					consol2.MasterBillAirlinePrefix = "081";
					consol2.JK_IsNeutralMaster = true;
					consol2.Factory.Save();

					AssertEquals("mawb 001 now allocated to consol 2", mawb1Factory2, consol2.MAWBAllocation.AllocatedMawb);

					consol1.JK_IsNeutralMaster = true;
					consol1.Factory.Save();

					AssertEquals("mawb 002 allocated to consol 1", mawb2Factory1, consol1.MAWBAllocation.AllocatedMawb);
					mawb1Factory2.JM_IsPrinted = true;

					factory2.Save();
				}

				AssertEquals("mawb 001 in factory 1 is not dirty and will not be saved", false, mawb1Factory1.HasChanges);
				AssertEquals("mawb 001 is not marked as printed ", false, mawb1Factory1.JM_IsPrinted);

				AssertEquals("prerequsite to force save concurrency to kick in", ZString.Empty, mawb1Factory1.JM_RL_NKPortOfLoading);

				mawb1Factory1.JM_RL_NKPortOfLoading = "AUBNE";

				Exception saveConcurrencyException = null;

				try
				{
					factory1.Save();
				}
				catch (Exception exc)
				{
					saveConcurrencyException = exc;
				}

				AssertNotNull(saveConcurrencyException);
				AssertEquals(typeof(ZSaveConcurrencyException), saveConcurrencyException.GetType());
				AssertContains("Business object around row = Enterprise.Freight.Business.JobMawb", saveConcurrencyException.Message);
			}
		}

		public void TestPreallocationWeightAjustmentException()
		{
			var preAllocationChecks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			preAllocationChecks.Weight.Action = PreAllocationCheck.Actions.Restriction;
			preAllocationChecks.Weight.Percentage = 10m;
			preAllocationChecks.Volume.Percentage = 10m;

			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, preAllocationChecks);

			var consol = Factory.New<ForwardingConsol>();

			using (var testForm = new ConsolFormTestClass(consol))
			{
				consol.JK_TotalShipmentActWeightCheck = 3000m;
				consol.WeightVerificationUnit = Constants.Weight.Tonnes;
				consol.VolumeVerificationUnit = Constants.Volume.MegaLitre;
				consol.JK_TotalShipmentActVolumeCheck = 250m;
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var shipment = consol.Shipments.AddNew();
				shipment.JS_ActualWeight = 35000m;
				shipment.JS_UnitOfWeight = Constants.Weight.Tonnes;
				shipment.JS_ActualVolume = 300m;
				shipment.JS_UnitOfVolume = Constants.Volume.MegaLitre;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				testForm.Show();
				var continueWithSave = testForm.ValidateAndSave();

				var dialog = ZFormModaliser.LastFormShownDialogForTest as AllocationAdjustmentDialog;

				AssertNotNull("Dialog was shown", dialog);
				AssertEquals("Saving should not be allowed", ContinueWithSave.No, continueWithSave);
			}
		}

		public void TestReallocatePrintedMawb()
		{
			JobMawb mawb1 = AddMawb("081", "00000011", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb("081", "00000022", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			using (new ConsolForm(consol))
			{
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_AWBServiceLevel = "STD";
				consol.JK_RL_NKLoadPort = HomePort;
				consol.JK_RL_NKDischargePort = OverseasPort;
				consol.MasterBillAirlinePrefix = "081";

				consol.Factory.Save();

				AssertEquals("08100000011", consol.JK_MasterBillNum);
				mawb1.JM_IsPrinted = true;
				consol.Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				consol.JK_IsNeutralMaster = false;
				AssertEquals("This neutral MAWB has already been printed in final.\r\nYou should only deallocate this MAWB from the Consolidation if it was issued in error, if you have reason to use a new MAWB for this Consolidation, or the Consolidation was canceled.\r\n\r\nAre you sure you wish to deallocate this MAWB number?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, consol.JK_IsNeutralMaster);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				consol.JK_IsNeutralMaster = false;
				AssertEquals(false, consol.JK_IsNeutralMaster);
				consol.Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				consol.JK_IsNeutralMaster = true;
				consol.Factory.Save();
				AssertEquals("08100000022", consol.JK_MasterBillNum);
				AssertEquals("Do you wish to reallocate printed neutral MAWB 081-00000011?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestReportError_WhenReallocatePrintedMawbOnSave()
		{
			ErrorReporter.Instance.Clear();
			var mawb = AddMawb("081", "00000011", GlbBranch.CurrentBranch, "STD");
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Factory.Saving += factory =>
			{
				consol.MAWBAllocation.ReallocatePrintedMawb(mawb);
			};

			using (var form = new ConsolForm(consol))
			{
				form.Show();

				var result = form.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertEquals("Report error when attempting to reallocate printed MAWB on form save", "OnConsolReallocatingPrintedMawbDuringSave", ErrorReporter.LastKeyReported);
			}

			ErrorReporter.Instance.Clear();
		}

		public void TestPromptingForDeallocatedPrintedMAWBUsage()
		{
			JobMawb mawb1 = AddMawb("081", "00000011", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb("081", "00000022", GlbBranch.CurrentBranch, "STD");
			Factory.Save();

			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			using (new ConsolForm(consol))
			{
				consol.JK_AgentType = Constants.AgentType.Agent;
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.JK_AWBServiceLevel = "STD";
				consol.JK_RL_NKLoadPort = HomePort;
				consol.JK_RL_NKDischargePort = OverseasPort;
				consol.MasterBillAirlinePrefix = "081";
				consol.Factory.Save();

				AssertEquals("08100000011", consol.JK_MasterBillNum);
				mawb1.JM_IsPrinted = true;
				consol.Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				consol.JK_IsNeutralMaster = false;
				AssertEquals("This neutral MAWB has already been printed in final.\r\nYou should only deallocate this MAWB from the Consolidation if it was issued in error, if you have reason to use a new MAWB for this Consolidation, or the Consolidation was canceled.\r\n\r\nAre you sure you wish to deallocate this MAWB number?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, consol.JK_IsNeutralMaster);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				consol.JK_IsNeutralMaster = false;
				AssertEquals(false, consol.JK_IsNeutralMaster);
				consol.Factory.Save();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				consol.JK_IsNeutralMaster = true;

				UnitTestUserNotification.Instance.ClearMessages();
				consol.Factory.Save();
				AssertNull("It is forbidden to show a message during a transaction.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSendingarnumerMenuItem()
		{
			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);

			using (var form = new ConsolForm(Factory.New<ForwardingConsol>()))
			{
				var thrown = false;
				try
				{
					FindMenuItem(form, "Actions", "Generate Shipment Sendingarnumers");
				}
				catch (ArgumentException)
				{
					thrown = true;
				}
				AssertEquals("Not Iceland", true, thrown);
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Iceland);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKDischargePort = "ISREY";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_CRN = "F-123-0808-8-AU-SYD";
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			using (var form = new ConsolForm(consol))
			{
				var sendingarnumerMenuItem = FindMenuItem(form, "Actions", "Generate Shipment Sendingarnumers");
				AssertNotNull("Iceland", sendingarnumerMenuItem);

				AssertEquals("", consol.Shipments[0].CustomsEntryNumber);
				AssertEquals("", consol.Shipments[1].CustomsEntryNumber);
				sendingarnumerMenuItem.PerformClick();
				AssertNotEquals("", consol.Shipments[0].CustomsEntryNumber);
				AssertNotEquals("", consol.Shipments[1].CustomsEntryNumber);
			}
		}

		public void TestHasCreateHAWBActionMenu()
		{
			using (var form = new ConsolForm(Factory.New<ForwardingConsol>()))
			{
				var item = FindMenuItem(form, "Actions", "Create New MAWB");
				AssertNotNull("should have found the 'Create New MAWB' option");
			}
		}

		public void TestHasDeniedPartyActionMenuWhenEnableComplianceRiskIsFalse()
		{
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ConsolForm(Factory.New<ForwardingConsol>()))
			{
				var actionsMenu = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var containsDeniedParty = false;
				foreach (MenuItem item in actionsMenu.MenuItems)
				{
					if (item.Text.Contains("Screen"))
					{
						containsDeniedParty = true;
						break;
					}
				}
				AssertEquals("Actions menu contains 'Denied Party' menu item", true, containsDeniedParty);
			}
		}

		public void TestHasAttachELoadListActionMenu()
		{
			var menuText = "Attach HVLV Origin Load List";
			using (var form = new ConsolForm(Factory.New<ForwardingConsol>()))
			{
				var actionsMenuItems = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.ToList<MenuItem>();
				AssertEquals(
					$"Actions menu contains '{menuText}' menu item",
					true,
					actionsMenuItems.Any(menuItem => menuItem.Text.Contains(menuText)));
			}
		}

		#region Recalculate Related Parties

		public void TestHasRecalculateCreditorMenuItem()
		{
			using (var form = new ConsolForm(Factory.New<ForwardingConsol>()))
			{
				AssertNotNull(FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company"));
			}
		}

		public void TestRecalculateCreditorPartiesRefreshesAddresses()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "Aardvark";
			carrier.OH_IsCreditor = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = "FCL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				form.SelectAddressesTabPage_ForTest();
				Application.DoEvents();

				var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses) as DocAddressesPlugIn;
				Assert(plugIn.TabPage.TabVisible);
				var control = (DocAddressUserControl)plugIn.UserControl;
				var splitContainer = (SplitContainer)control.Controls["SplitContainer"];
				var addressGrid = (MasterFiles.GUI.Internal.DocAddressGrid)splitContainer.Panel1.Controls["AddressGrid"];
				AssertEquals(0, addressGrid.VisibleRowCount);

				var menuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				menuItem.PerformClick();

				Application.DoEvents();
				AssertEquals(1, addressGrid.VisibleRowCount);
			}
		}

		public void TestHasRecalculateRelatedPartiesMenuItem()
		{
			using (var form = new ConsolForm(Factory.New<ForwardingConsol>()))
			{
				AssertNotNull(FindMenuItem(form, "Actions", "Recalculate Related Parties for logged in Company"));
			}
		}

		public void TestRecalculateRelatedParties()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "CNSHA";
				consol.JK_AgentType = Core.Constants.AgentType.CoLoad;

				ChildEditableService.SetState(consol.Factory, ChildEditableServiceStates.Shipment);

				using (var form = new ConsolForm(consol))
				{
					form.Show();
					var calculateDeliveryDueDateMenuItem = FindMenuItem(form, "Actions", "Recalculate Related Parties for logged in Company");
					calculateDeliveryDueDateMenuItem.PerformClick();
					AssertEquals("Should show company does not match message",
						"You cannot Recalculate Related Parties for this company because the company does not match Pickup or Delivery direction of this job.",
						UnitTestUserNotification.Instance.LastMessage.Text);

					consol.JK_RL_NKLoadPort = "NASWP";

					var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
					consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
					AssertNotNull(consol.SendingForwarder);

					var oldValue = Factory.NewWithValidTestData<OrgHeader>();
					consol.JK_OA_CreditorAddress_ZAddress.OrgPK = oldValue.PK;

					var coLoadWithRelatedParty = Factory.NewWithValidTestData<OrgHeader>();
					sendingForwarder.AddRelatedParty(coLoadWithRelatedParty.PK,
						RelatedPartyTypeList.Codes.ForwarderCoLoadWith,
						RelatedPartyDirectionList.Codes.Pickup,
						Constants.TransportModes.All,
						ZString.Empty, GlbCompany.CurrentCompany);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					calculateDeliveryDueDateMenuItem.PerformClick();
					AssertEquals("Should show already has value message",
						"Co-Load With has already been entered. Do you wish to update the Co-Load With based on your Company Related Party Configuration?",
						UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Response is defaultable.", UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
					AssertEquals("Co-Load With should have its old value", oldValue.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					calculateDeliveryDueDateMenuItem.PerformClick();
					AssertEquals("Should show already has value message",
						"Co-Load With has already been entered. Do you wish to update the Co-Load With based on your Company Related Party Configuration?",
						UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Response is defaultable.", UnitTestUserNotification.Instance.LastMessage.WasDefaultable);
					AssertEquals("Co-Load With should have its old value", coLoadWithRelatedParty.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);

					UnitTestUserNotification.Instance.ClearMessages();
					consol.JK_OA_CreditorAddress_ZAddress.OrgPK = ZGuid.Empty;

					calculateDeliveryDueDateMenuItem.PerformClick();
					AssertEquals("Co-Load With should be defaulted by Related Party", coLoadWithRelatedParty.PK, consol.JK_OA_CreditorAddress_ZAddress.OrgPK);
					AssertEquals("Should not show already has value message", null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion

		#region Transit Warehouse

		[TestDate(2020, 11, 3, 1, 2, 3)]
		public void TestSendTransitWarehousePickupReceiptInstruction()
		{
			TestTransitWarehouseInstruction(true, TransitWarehouseInstructionHelper.ServiceRequest.Receipt);
		}

		[TestDate(2020, 11, 3, 1, 2, 3)]
		public void TestSendTransitWarehousePickupDispatchInstruction()
		{
			TestTransitWarehouseInstruction(true, TransitWarehouseInstructionHelper.ServiceRequest.Dispatch);
		}

		[TestDate(2020, 11, 3, 1, 2, 3)]
		public void TestSendTransitWarehousePickupReceiptAndDispatchInstruction()
		{
			TestTransitWarehouseInstruction(true, TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch);
		}

		[TestDate(2020, 11, 3, 1, 2, 3)]
		public void TestSendTransitWarehouseDeliveryReceiptInstruction()
		{
			TestTransitWarehouseInstruction(false, TransitWarehouseInstructionHelper.ServiceRequest.Receipt);
		}

		[TestDate(2020, 11, 3, 1, 2, 3)]
		public void TestSendTransitWarehouseDeliveryDispatchInstruction()
		{
			TestTransitWarehouseInstruction(false, TransitWarehouseInstructionHelper.ServiceRequest.Dispatch);
		}

		[TestDate(2020, 11, 3, 1, 2, 3)]
		public void TestSendTransitWarehouseDeliveryReceiptAndDispatchInstruction()
		{
			TestTransitWarehouseInstruction(false, TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch);
		}

		ZPropertyInfo[] GetTransitWarehouseInstructionRequestedDateInfos(ForwardingConsol consol, bool testPickup, TransitWarehouseInstructionHelper.ServiceRequest receiptDispatch)
		{
			if (testPickup)
			{
				if (receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.Receipt)
				{
					return new ZPropertyInfo[] { consol.JK_PackDepotReceiptRequestedInfo };
				}
				else if (receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.Dispatch)
				{
					return new ZPropertyInfo[] { consol.JK_PackDepotDispatchRequestedInfo };
				}
				else
				{
					return new ZPropertyInfo[] { consol.JK_PackDepotReceiptRequestedInfo, consol.JK_PackDepotDispatchRequestedInfo };
				}
			}
			else
			{
				if (receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.Receipt)
				{
					return new ZPropertyInfo[] { consol.JK_UnpackDepotReceiptRequestedInfo };
				}
				else if (receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.Dispatch)
				{
					return new ZPropertyInfo[] { consol.JK_UnpackDepotDispatchRequestedInfo };
				}
				else
				{
					return new ZPropertyInfo[] { consol.JK_UnpackDepotReceiptRequestedInfo, consol.JK_UnpackDepotDispatchRequestedInfo };
				}
			}
		}

		void TestTransitWarehouseInstruction(bool testPickup, TransitWarehouseInstructionHelper.ServiceRequest receiptDispatch)
		{
			var arrivalOrDeparture = testPickup ? "Departure" : "Arrival";
			var receiptOrDispatch = "Receipt and Dispatch";
			if (receiptDispatch != TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch)
			{
				receiptOrDispatch = receiptDispatch == TransitWarehouseInstructionHelper.ServiceRequest.Receipt ? "Receipt" : "Dispatch";
			}

			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = "AIR";

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock.Setup(m => m.GetNotificationForInstruction(It.IsAny<string>(), It.IsAny<string>()))
				.Returns(new WarningNotification("Instruction has been queued to send. Check DEX logs for details."));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var transitWarehouseMenu = FindMenuItem(consolForm, "Actions").MenuItems.FindByText("Transit Warehouse").MenuItems.FindByText(arrivalOrDeparture);
				var menu = transitWarehouseMenu.MenuItems.FindByText(FormattableString.Invariant($"Send {receiptOrDispatch} Instruction"));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertEquals("Please save your changes before sending the Transit Warehouse Instruction.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(FormattableString.Invariant($"The {arrivalOrDeparture} CFS must be entered before the {arrivalOrDeparture} TW {receiptOrDispatch} Instruction can be sent."), UnitTestUserNotification.Instance.LastMessage.Text);

				if (testPickup)
				{
					consol.JK_OA_PackDepotAddress = depot.PK;
				}
				else
				{
					consol.JK_OA_UnpackDepotAddress = depot.PK;
				}

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertStartsWith("Should have EDI comms failure message", "No EDI Communications settings were found", UnitTestUserNotification.Instance.LastMessage.Text);

				var logs = consol.Logs.GetAllLogs();
				Assert("No DEX event", !logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == "DEX"));
				Assert("No SVR event", !logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == "SVR"));

				var dateInfos = GetTransitWarehouseInstructionRequestedDateInfos(consol, testPickup, receiptDispatch);

				foreach (var dateInfo in dateInfos)
				{
					AssertEquals("Date has not been updated", ZDateTime.Empty, (ZDateTime)dateInfo.Value);
				}

				var communicationMode = depot.Header.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationMode.EK_Destination = "Blah";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = "CON";

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);

				if (receiptDispatch != TransitWarehouseInstructionHelper.ServiceRequest.ReceiveAndDispatch)
				{
					AssertEquals(@"The Transit Warehouse Instruction has been sent.
Processing Consol C00001111
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.", UnitTestUserNotification.Instance.LastMessage.Text);

					var newFactory = Factory.CreateNewFactory();
					consol = newFactory.Load<ForwardingConsol>(consol.PK);
					logs = consol.Logs.GetAllLogs();
					var svrEvent = logs.Cast<StmALog>().Single(l => l.SL_SE_NKEvent == "SVR");
					AssertNotNull("SVR event created", svrEvent);

					var expectedReference = FormattableString.Invariant($"|FAC=CFS|LOC=AUBNE|TYP={receiptOrDispatch}");
					AssertEquals("SVR event reference", expectedReference, svrEvent.SL_Reference);

					Assert("DEX event created", logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == "DEX"));
				}
				else
				{
					AssertEquals(@"The Transit Warehouse Instruction has been sent.
Processing Consol C00001111
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.
Processing Consol C00001111
Universal Shipment queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.", UnitTestUserNotification.Instance.LastMessage.Text);

					var newFactory = Factory.CreateNewFactory();
					consol = newFactory.Load<ForwardingConsol>(consol.PK);
					logs = consol.Logs.GetAllLogs();
					var svrEvent = logs.Cast<StmALog>().First(l => l.SL_SE_NKEvent == "SVR");
					AssertNotNull("SVR event created", svrEvent);

					var expectedReference = FormattableString.Invariant($"|FAC=CFS|LOC=AUBNE|TYP=Receipt");
					AssertEquals("SVR event reference", expectedReference, svrEvent.SL_Reference);

					svrEvent = logs.Cast<StmALog>().Last(l => l.SL_SE_NKEvent == "SVR");
					AssertNotNull("SVR event created", svrEvent);

					expectedReference = FormattableString.Invariant($"|FAC=CFS|LOC=AUBNE|TYP=Dispatch");
					AssertEquals("SVR event reference", expectedReference, svrEvent.SL_Reference);

					Assert("DEX event created", logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == "DEX"));
				}

				dateInfos = GetTransitWarehouseInstructionRequestedDateInfos(consol, testPickup, receiptDispatch);
				foreach (var dateInfo in dateInfos)
				{
					AssertNotEquals("Date has been updated", ZDateTime.Empty, (ZDateTime)dateInfo.Value);
				}

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
			transitUniversalServiceMock.VerifyAll();
		}

		#endregion

		#region Transit Warehouse Stop Load Event

		public void TestSendTransitWarehousePickupDispatchStopLoadInstruction()
		{
			TestSendTransitWarehouseDispatchStopLoadInstruction(true, false);
		}

		public void TestSendTransitWarehousePickupDispatchCancelStopLoadInstruction()
		{
			TestSendTransitWarehouseDispatchStopLoadInstruction(true, true);
		}

		public void TestSendTransitWarehouseDeliveryDispatchStopLoadInstruction()
		{
			TestSendTransitWarehouseDispatchStopLoadInstruction(false, false);
		}

		public void TestSendTransitWarehouseDeliveryDispatchCancelStopLoadInstruction()
		{
			TestSendTransitWarehouseDispatchStopLoadInstruction(false, true);
		}

		void TestSendTransitWarehouseDispatchStopLoadInstruction(bool testPickup, bool isCancel)
		{
			var arrivalOrDeparture = testPickup ? "Departure" : "Arrival";
			var actionName = isCancel ? "Cancel Stop Load" : "Stop Load";
			var eventcode = isCancel ? Events.ServiceRequestedCode : Events.ServiceSuspendedCode;

			var depot = Factory.LoadTop1<OrgAddress>(new ZQuery());

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = "AIR";

			var transitUniversalServiceMock = new Mock<ITransitUniversalService>();
			transitUniversalServiceMock.Setup(m => m.GetNotificationForStopLoadInstructionEvent(It.IsAny<string>(), It.IsAny<bool>()))
				.Returns(new WarningNotification("Instruction has been queued to send. Check DEX logs for details."));

			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
			using (ObjectFactory.Substitute(nameof(ITransitUniversalService), _ => transitUniversalServiceMock.Object))
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var transitWarehouseMenu = FindMenuItem(consolForm, "Actions").MenuItems.FindByText("Transit Warehouse").MenuItems.FindByText(arrivalOrDeparture);
				var menu = transitWarehouseMenu.MenuItems.FindByText(FormattableString.Invariant($"{actionName} Request"));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertEquals("Please save your changes before sending the Transit Warehouse Instruction.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(FormattableString.Invariant($"The {arrivalOrDeparture} CFS must be entered before the {arrivalOrDeparture} TW Dispatch Instruction can be sent."), UnitTestUserNotification.Instance.LastMessage.Text);

				if (testPickup)
				{
					consol.JK_OA_PackDepotAddress = depot.PK;
				}
				else
				{
					consol.JK_OA_UnpackDepotAddress = depot.PK;
				}

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(FormattableString.Invariant($"The {arrivalOrDeparture} Dispatch Instructions must be sent before a Load can be stopped."), UnitTestUserNotification.Instance.LastMessage.Text);

				if (testPickup)
				{
					consol.JK_PackDepotDispatchRequested = ZDateTime.Now;
				}
				else
				{
					consol.JK_UnpackDepotDispatchRequested = ZDateTime.Now;
				}

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertStartsWith("Should have EDI comms failure message", "No EDI Communications settings were found", UnitTestUserNotification.Instance.LastMessage.Text);

				var logs = consol.Logs.GetAllLogs();
				Assert("No DEX event", !logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.DataExportCode));
				Assert("No SVS or SVR event", !logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == eventcode));

				var communicationMode = depot.Header.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
				communicationMode.EK_Destination = "Blah";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
				communicationMode.EK_Module = "CON";

				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				menu.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(@"The Transit Warehouse Instruction has been sent.
Processing Consol C00001111
Universal Event queued for sending to Organization [MIDINT].

Warning: Instruction has been queued to send. Check DEX logs for details.", UnitTestUserNotification.Instance.LastMessage.Text);

				logs = consol.Logs.GetAllLogs();
				var log = logs.Cast<StmALog>().Single(l => l.SL_SE_NKEvent == eventcode);
				AssertNotNull("SVS or SVR event created", log);

				var expectedReference = FormattableString.Invariant($"Queued|FAC=CFS|LOC=AUBNE|TYP=Load");
				AssertEquals("SVS or SVR event reference", expectedReference, log.SL_Reference);

				Assert("DEX event created", logs.Cast<StmALog>().Any(l => l.SL_SE_NKEvent == Events.DataExportCode));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
			transitUniversalServiceMock.VerifyAll();
		}

		#endregion

		public void TestSettingExportCreditorAddressFromCarrier_WhenConsolIsExportAndCreditorIsEmpty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = "FCL";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor

			Factory.Save();

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			var parent = new JobDocAddressParentForTesting(Factory);
			using (var form = new ConsolForm(consol))
			{
				form.Show();

				var allAddressTypes = Enum.GetValues(typeof(DocAddressType)).Cast<DocAddressType>().Where(type => type != DocAddressType.None);
				var allAddressTypesCount = allAddressTypes.Count();

				using (var plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.SelectTabPage();
					plugIn.OnUserControlShown();
					var control = (DocAddressUserControl)plugIn.UserControl;
					var splitContainer = (SplitContainer)control.Controls["SplitContainer"];
					var addressGrid = (MasterFiles.GUI.Internal.DocAddressGrid)splitContainer.Panel1.Controls["AddressGrid"];
					addressGrid.OnPopup_CallForTesting();

					var carrierExportCreditorMenu = addressGrid.ContextMenu.MenuItems.FindByText("Add Carrier Export Creditor");
					AssertNotNull(carrierExportCreditorMenu);
					carrierExportCreditorMenu.PerformClick(); // should set export from carrier
					AssertEquals("Export creditor address should fallback to carrier if consol is Export", carrier.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);

					var carrierImportCreditorMenu = addressGrid.ContextMenu.MenuItems.FindByText("Add Carrier Import Creditor");
					AssertNotNull(carrierExportCreditorMenu);
					carrierImportCreditorMenu.PerformClick(); // should Not set import from carrier
					AssertEquals("Import creditor address should Not fallback to carrier if consol is Export", ZGuid.Empty, consol.CarrierImportCreditorAddress.E2_OA_Address);
				}
			}
		}

		public void TestSettingImportCreditorAddressFromCarrier_WhenConsolIsImportAndCreditorIsEmpty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = "SEA";
			consol.JK_ConsolMode = "FCL";

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor

			Factory.Save();

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			var parent = new JobDocAddressParentForTesting(Factory);
			using (var form = new ConsolForm(consol))
			{
				form.Show();

				var allAddressTypes = Enum.GetValues(typeof(DocAddressType)).Cast<DocAddressType>().Where(type => type != DocAddressType.None);
				var allAddressTypesCount = allAddressTypes.Count();

				using (var plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.SelectTabPage();
					plugIn.OnUserControlShown();
					var control = (DocAddressUserControl)plugIn.UserControl;
					var splitContainer = (SplitContainer)control.Controls["SplitContainer"];
					var addressGrid = (MasterFiles.GUI.Internal.DocAddressGrid)splitContainer.Panel1.Controls["AddressGrid"];
					addressGrid.OnPopup_CallForTesting();

					var carrierImportCreditorMenu = addressGrid.ContextMenu.MenuItems.FindByText("Add Carrier Import Creditor");
					AssertNotNull(carrierImportCreditorMenu);
					carrierImportCreditorMenu.PerformClick(); // should set Import from carrier
					AssertEquals("Import creditor address should fallback to carrier if consol is Import", carrier.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);

					var carrierExportCreditorMenu = addressGrid.ContextMenu.MenuItems.FindByText("Add Carrier Export Creditor");
					AssertNotNull(carrierExportCreditorMenu);
					carrierExportCreditorMenu.PerformClick(); // should not set export from carrier
					AssertEquals("Export creditor address should Not fallback to carrier if consol is Import", ZGuid.Empty, consol.CarrierExportCreditorAddress.E2_OA_Address);
				}
			}
		}

		public void TestSettingCreditorFromExportCreditorAddress_WhenConsolIsExportAgent()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.AgentConsol;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			Factory.Save();

			//Test existing consol loaded from database and make sure even handler is hooked up correctly
			var factory = new BusinessObjectFactory();
			consol = factory.Load<ForwardingConsol>(consol.PK);

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				var mainPanel = (ZPanel)form.Controls["MainPanel"];
				var mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				mainTabControl.SelectedTab = mainTabControl.GetTabPage("AddressesTabPage");

				var exportCreditorAddress = consol.DocAddresses.FindOrCreateWithRequirement(new JobDocAddressRequirement(DocAddressType.CarrierExportCreditor));
				exportCreditorAddress.E2_OA_Address = carrier.MainAddress.PK;

				AssertEquals("Creditor address should sync with Carrier Export Creditor Address", consol.JK_OA_CreditorAddress, exportCreditorAddress.E2_OA_Address);
			}
		}

		public void TestSettingCreditorFromImportCreditorAddress_WhenConsolIsImportAgent()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = OverseasPort;
			consol.JK_RL_NKDischargePort = HomePort;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.AgentConsol;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			Factory.Save();

			//Test existing consol loaded from database and make sure even handler is hooked up correctly
			var factory = new BusinessObjectFactory();
			consol = factory.Load<ForwardingConsol>(consol.PK);

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				var mainPanel = (ZPanel)form.Controls["MainPanel"];
				var mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				mainTabControl.SelectedTab = mainTabControl.GetTabPage("AddressesTabPage");

				var importCreditorAddress = consol.DocAddresses.FindOrCreateWithRequirement(new JobDocAddressRequirement(DocAddressType.CarrierImportCreditor));
				importCreditorAddress.E2_OA_Address = carrier.MainAddress.PK;

				AssertEquals("Creditor address should sync with Carrier Import Creditor Address", consol.JK_OA_CreditorAddress, importCreditorAddress.E2_OA_Address);
			}
		}

		public void TestSettingImportExportCreditorAddressFromCarrier_WhenUserClicksOnRecalculateRelatedParties_CreditorIsTheSameAsNewCreditor()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var creditorForExport = Factory.NewWithValidTestData<OrgHeader>();
			creditorForExport.OH_IsCreditor = true;

			var creditorForImport = Factory.NewWithValidTestData<OrgHeader>();
			creditorForImport.OH_IsCreditor = true;

			var relatedParties = carrier.AllRelatedParties;
			var partyRecord = relatedParties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;
			partyRecord.PR_OH_RelatedParty = creditorForExport.PK;
			partyRecord.PR_FreightTransportMode = consol.JK_TransportMode;
			partyRecord.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord.PR_Location = origin;

			var partyRecord2 = relatedParties.AddNew();
			partyRecord2.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord2.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;
			partyRecord2.PR_OH_RelatedParty = creditorForImport.PK;
			partyRecord2.PR_FreightTransportMode = consol.JK_TransportMode;
			partyRecord2.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord2.PR_Location = destination;

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Iceland))
			{
				AssertEquals("Preconditions: crosstrade consol expected.", true, consol.IsCrossTrade());

				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consol.JK_OA_CreditorAddress = creditorForImport.MainAddress.PK;

				Factory.Save();

				AssertEquals("Import creditor address should be equal to import creditor", creditorForImport.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);
				AssertEquals("Export creditor address should be empty because consol is cross trade", ZGuid.Empty, consol.CarrierExportCreditorAddress.E2_OA_Address);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

				using (var form = new ConsolForm(consol))
				{
					AssertEquals("Consol creditor is equal to import creditor", creditorForImport.MainAddress.PK, consol.JK_OA_CreditorAddress);
					AssertEquals("Import creditor address should not change", creditorForImport.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);
					AssertEquals("Export creditor address still should be empty when we logged in as export", ZGuid.Empty, consol.CarrierExportCreditorAddress.E2_OA_Address);

					var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
					recalculateMenuItem.PerformClick();

					AssertEquals("Consol creditor still be equal to export creditor after recalculate related parties", creditorForExport.MainAddress.PK, consol.JK_OA_CreditorAddress);
					AssertEquals("Import creditor address still should still be available after recalculate related parties", creditorForImport.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);
					AssertEquals("Export creditor address should be equal to consol creditor after recalculate related parties ", consol.JK_OA_CreditorAddress, consol.CarrierExportCreditorAddress.E2_OA_Address);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Iceland))
			{
				AssertEquals("Preconditions: crosstrade consol expected.", true, consol.IsCrossTrade());

				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consol.JK_OA_CreditorAddress = creditorForImport.MainAddress.PK;

				Factory.Save();

				using (var form = new ConsolForm(consol))
				{
					AssertEquals("Import creditor address should be still creditor import", creditorForImport.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);
					AssertEquals("Export creditor address should be still creditor export, because we already set it and shouldn't change when we login to cross trade company", creditorForExport.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);

					var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
					recalculateMenuItem.PerformClick();

					AssertEquals("Consol creditor is equal to creditor we set for import", creditorForImport.MainAddress.PK, consol.JK_OA_CreditorAddress);
					AssertEquals("Import creditor address still should be creditor import after recalculate related parties", creditorForImport.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);
					AssertEquals("Export creditor address should be still creditor export, because we already set it and shouldn't change when we login to cross trade company", creditorForExport.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

				using (var form = new ConsolForm(consol))
				{
					AssertEquals("Consol creditor is equal to creditor we set for import", creditorForImport.MainAddress.PK, consol.JK_OA_CreditorAddress);
					AssertEquals("Import creditor address still should be creditor import when we logged in as import", creditorForImport.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);
					AssertEquals("Export creditor address should be still equal to creditor for export, because we already set it and shouldn't change when we login as import", creditorForExport.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);

					var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
					recalculateMenuItem.PerformClick();

					AssertEquals("Consol creditor still be equal to import creditor after recalculate related parties", creditorForImport.MainAddress.PK, consol.JK_OA_CreditorAddress);
					AssertEquals("Export creditor address should be still creditor fo export, because we already set it and shouldn't change when we login to import company", creditorForExport.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);
					AssertEquals("Import creditor address should be equal to consol creditor after recalculate related parties ", consol.JK_OA_CreditorAddress, consol.CarrierImportCreditorAddress.E2_OA_Address);
				}
			}
		}

		ForwardingConsol GetCrossTradeConsol(bool isCoLoad)
		{
			var origin = OverseasPort;
			var destination = OverseasPort2;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = isCoLoad ? AgentType.CoLoad : AgentType.Agent;
			Factory.Save();
			AssertEquals("Preconditions: Cross Trade Consol expected.", true, consol.IsCrossTrade());
			AssertEquals("Preconditions: CoLoad mode.", isCoLoad, consol.IsCoLoad);
			return consol;
		}

		void AddRelatedPartyRecord(ForwardingConsol consol, OrgRelatedPartyCompanySpecificCollection relatedParties, OrgHeader creditor, ZString freightDirection)
		{
			var partyRecord = relatedParties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord.PR_FreightDirection = freightDirection;
			partyRecord.PR_OH_RelatedParty = creditor.PK;
			partyRecord.PR_FreightTransportMode = consol.JK_TransportMode;
			partyRecord.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord.PR_Location = consol.JK_RL_NKDischargePort;
		}

		void TestCrossTradeCoLoadConsol_WhenUserClicksOnRecalculateRelatedParties_UpdateImportExportCreditorAddress(bool hasMatchingRelatedParty, bool isPayable)
		{
			var consol = GetCrossTradeConsol(true);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			Factory.Save();

			AssertEquals("Carrier Import Creditor Address is empty", consol.CarrierImportCreditorAddress.E2_OA_Address, ZGuid.Empty);

			consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				AssertEquals("Import creditor address should be equal to 'CoLoad With'", consol.JK_OA_CreditorAddress, consol.CarrierImportCreditorAddress.E2_OA_Address);

				var creditorForImport = Factory.NewWithValidTestData<OrgHeader>();
				creditorForImport.OH_IsCreditor = isPayable;

				var relatedParties = carrier.AllRelatedParties;
				if (hasMatchingRelatedParty)
				{
					AddRelatedPartyRecord(consol, relatedParties, creditorForImport, RelatedPartyDirectionList.Codes.Delivery);
				}
				else
				{
					AddRelatedPartyRecord(consol, relatedParties, creditorForImport, RelatedPartyDirectionList.Codes.Pickup);
				}

				Factory.Save();

				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();

				if (hasMatchingRelatedParty && isPayable)
				{
					AssertEquals("Import creditor address should be creditor import after recalculate related parties", creditorForImport.MainAddress.PK, consol.CarrierImportCreditorAddress.E2_OA_Address);
				}
				else
				{
					AssertEquals("Import creditor address should be same as 'Coload with' when matching related party not found", consol.JK_OA_CreditorAddress, consol.CarrierImportCreditorAddress.E2_OA_Address);
				}
				AssertEquals("Export creditor address should be empty", consol.CarrierExportCreditorAddress.E2_OA_Address, ZGuid.Empty);
			}
		}

		public void TestCrossTradeCoLoadConsol_WhenUserClicksOnRecalculateRelatedParties_UpdateImportExportCreditorAddress_WhenMatchingRelatedPartyFound()
		{
			TestCrossTradeCoLoadConsol_WhenUserClicksOnRecalculateRelatedParties_UpdateImportExportCreditorAddress(true, true);
		}

		public void TestCrossTradeCoLoadConsol_WhenUserClicksOnRecalculateRelatedParties_UpdateImportExportCreditorAddress_WhenMatchingRelatedPartyNotFound()
		{
			TestCrossTradeCoLoadConsol_WhenUserClicksOnRecalculateRelatedParties_UpdateImportExportCreditorAddress(false, true);
		}

		public void TestCrossTradeCoLoadConsol_WhenUserClicksOnRecalculateRelatedParties_UpdateImportExportCreditorAddress_WhenMatchingRelatedPartyIsNotPayable()
		{
			TestCrossTradeCoLoadConsol_WhenUserClicksOnRecalculateRelatedParties_UpdateImportExportCreditorAddress(true, true);
		}

		public void TestCrossTradeNonCoLoadConsol_DLY_Fallback_PAD_OR_PIC_WhenUserClicksOnRecalculateRelatedParties()
		{
			var consol = GetCrossTradeConsol(false);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var creditorForDlv = Factory.NewWithValidTestData<OrgHeader>();
			creditorForDlv.OH_IsCreditor = true;

			var creditorForPad = Factory.NewWithValidTestData<OrgHeader>();
			creditorForPad.OH_IsCreditor = true;

			var creditorForPickup = Factory.NewWithValidTestData<OrgHeader>();
			creditorForPickup.OH_IsCreditor = true;

			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");

				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

				Factory.Save();

				AssertEquals("Consol creditor should be equal to carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

				var relatedParties = carrier.AllRelatedParties;

				AddRelatedPartyRecord(consol, relatedParties, creditorForPad, RelatedPartyDirectionList.Codes.PickupAndDelivery);
				Factory.Save();
				recalculateMenuItem.PerformClick();
				AssertEquals("Consol creditor should be equal to PickupAndDelivery carrier", consol.JK_OA_CreditorAddress, creditorForPad.MainAddress.PK);

				AddRelatedPartyRecord(consol, relatedParties, creditorForDlv, RelatedPartyDirectionList.Codes.Delivery);
				Factory.Save();
				recalculateMenuItem.PerformClick();
				AssertEquals("Consol creditor should be equal to Delivery carrier as Delivery has higher precedance over PickupAndDelivery", consol.JK_OA_CreditorAddress, creditorForDlv.MainAddress.PK);

				relatedParties.DeleteAll();

				Factory.Save();
				recalculateMenuItem.PerformClick();
				AssertEquals("Consol creditor should be equal to carrier after deleting all related party records", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

				AddRelatedPartyRecord(consol, relatedParties, creditorForDlv, RelatedPartyDirectionList.Codes.Delivery);
				AddRelatedPartyRecord(consol, relatedParties, creditorForPad, RelatedPartyDirectionList.Codes.PickupAndDelivery);
				AddRelatedPartyRecord(consol, relatedParties, creditorForPickup, RelatedPartyDirectionList.Codes.Pickup);
				Factory.Save();
				recalculateMenuItem.PerformClick();
				AssertEquals("Consol creditor should be equal to new Delivery carrier", consol.JK_OA_CreditorAddress, creditorForDlv.MainAddress.PK);

				relatedParties.DeleteAll();

				AddRelatedPartyRecord(consol, relatedParties, creditorForPickup, RelatedPartyDirectionList.Codes.Pickup);
				Factory.Save();
				recalculateMenuItem.PerformClick();
				AssertEquals("Consol creditor should be equal to consol carrier and pickup related party in cross-trade non-coload console should ignore", consol.JK_OA_CreditorAddress, consol.JK_OA_ShippingLineAddress);
			}
		}

		public void TestCrossTradeCoLoadConsol_DLY_Fallback_PAD_OR_PIC_WhenUserClicksOnRecalculateRelatedParties()
		{
			var consol = GetCrossTradeConsol(true);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var creditorForDlv = Factory.NewWithValidTestData<OrgHeader>();
			creditorForDlv.OH_IsCreditor = true;

			var creditorForPad = Factory.NewWithValidTestData<OrgHeader>();
			creditorForPad.OH_IsCreditor = true;

			var creditorForPickup = Factory.NewWithValidTestData<OrgHeader>();
			creditorForPickup.OH_IsCreditor = true;

			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");

				consol.JK_OA_CreditorAddress = carrier.MainAddress.PK;

				Factory.Save();

				AssertEquals("Import creditor address should be equal to 'CoLoad With'", consol.CarrierImportCreditorAddress.E2_OA_Address, carrier.MainAddress.PK);

				var relatedParties = carrier.AllRelatedParties;

				AddRelatedPartyRecord(consol, relatedParties, creditorForPad, RelatedPartyDirectionList.Codes.PickupAndDelivery);
				Factory.Save();
				recalculateMenuItem.PerformClick();
				AssertEquals("Import creditor address should be equal to PickupAndDelivery carrier", consol.CarrierImportCreditorAddress.E2_OA_Address, creditorForPad.MainAddress.PK);

				AddRelatedPartyRecord(consol, relatedParties, creditorForDlv, RelatedPartyDirectionList.Codes.Delivery);
				Factory.Save();
				recalculateMenuItem.PerformClick();
				AssertEquals("Import creditor address should be equal to Delivery carrier as Delivery has higher precedance over PickupAndDelivery", consol.CarrierImportCreditorAddress.E2_OA_Address, creditorForDlv.MainAddress.PK);

				relatedParties.DeleteAll();

				Factory.Save();
				recalculateMenuItem.PerformClick();
				AssertEquals("Import creditor address should be equal to 'CoLoad With' after deleting all related party records", consol.CarrierImportCreditorAddress.E2_OA_Address, carrier.MainAddress.PK);

				AddRelatedPartyRecord(consol, relatedParties, creditorForDlv, RelatedPartyDirectionList.Codes.Delivery);
				AddRelatedPartyRecord(consol, relatedParties, creditorForPad, RelatedPartyDirectionList.Codes.PickupAndDelivery);
				AddRelatedPartyRecord(consol, relatedParties, creditorForPickup, RelatedPartyDirectionList.Codes.Pickup);
				Factory.Save();
				recalculateMenuItem.PerformClick();
				AssertEquals("Import creditor address should be equal to new Delivery carrier", consol.CarrierImportCreditorAddress.E2_OA_Address, creditorForDlv.MainAddress.PK);

				relatedParties.DeleteAll();

				AddRelatedPartyRecord(consol, relatedParties, creditorForPickup, RelatedPartyDirectionList.Codes.Pickup);
				Factory.Save();
				recalculateMenuItem.PerformClick();
				AssertEquals("Import creditor address should be equal to consol 'CoLoad With' as pickup related party in cross-trade coload console should ignore", consol.CarrierImportCreditorAddress.E2_OA_Address, carrier.MainAddress.PK);
			}
		}

		public void TestSettingExportCreditorAddressFromCarrier_WhenUserClicksOnRecalculateRelatedParties_CreditorIsSetFromCarrierRelatedPartyForNonCLDDomesticConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = AlternateHomePort;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.Agent;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			Factory.Save();

			AssertEquals("Preconditions: domestic consol expected.", true, consol.IsDomestic());
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			AssertEquals("Consol creditor should be equal to carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

			var creditorForExport = Factory.NewWithValidTestData<OrgHeader>();
			creditorForExport.OH_IsCreditor = true;

			using (var form = new ConsolForm(consol))
			{
				consol.JK_OA_CreditorAddress = ZGuid.Empty;
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();
				AssertEquals("Consol creditor should be equal to carrier after recalculate related parties", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);
				AssertEquals("Export creditor address should be equal to carrier after recalculate related parties ", carrier.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);
			}

			var relatedParties = carrier.AllRelatedParties;
			var partyRecord = relatedParties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.PickupAndDelivery;
			partyRecord.PR_OH_RelatedParty = creditorForExport.PK;
			partyRecord.PR_FreightTransportMode = consol.JK_TransportMode;
			partyRecord.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord.PR_Location = HomePort;

			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();
				AssertEquals("Consol creditor should be equal to carrier related party after recalculate related parties", creditorForExport.MainAddress.PK, consol.JK_OA_CreditorAddress);
				AssertEquals("Export creditor address should be equal to consol creditor after recalculate related parties ", consol.JK_OA_CreditorAddress, consol.CarrierExportCreditorAddress.E2_OA_Address);
			}
		}

		public void TestSettingExportCreditorAddressFromCarrier_WhenCarrierDoesNotHaveMatchingRelatedParty_CreditorIsSetToCarrierForNonCLDDomesticConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = AlternateHomePort;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.Agent;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			consol.JK_OA_CreditorAddress = ZGuid.Empty; // Clear Creditor

			Factory.Save();

			AssertEquals("Preconditions: domestic consol expected.", true, consol.IsDomestic());

			using (var form = new ConsolForm(consol))
			{
				form.Show();

				using (var plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.SelectTabPage();
					plugIn.OnUserControlShown();
					var control = (DocAddressUserControl)plugIn.UserControl;
					var splitContainer = (SplitContainer)control.Controls["SplitContainer"];
					var addressGrid = (MasterFiles.GUI.Internal.DocAddressGrid)splitContainer.Panel1.Controls["AddressGrid"];
					addressGrid.OnPopup_CallForTesting();

					var carrierExportCreditorMenu = addressGrid.ContextMenu.MenuItems.FindByText("Add Carrier Export Creditor");
					AssertNotNull(carrierExportCreditorMenu);
					carrierExportCreditorMenu.PerformClick(); // should set export from carrier
					AssertEquals("Export creditor address should fallback to carrier if consol is Domestic", carrier.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);
				}
			}
		}

		public void TestExportCreditorAddress_WhenUserClicksOnRecalculateRelatedParties_CreditorIsSetFromColoadWithRelatedPartyForCLDDomesticConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = AlternateHomePort;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			AssertEquals("Preconditions: Domestic Consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: CLD mode.", true, consol.IsCoLoad);

			var coloadOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			coloadOrg.OH_IsCreditor = true;
			relatedParty.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = coloadOrg.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;

			AssertEquals("Co-Load With address is valid", consol.JK_OA_CreditorAddress, coloadOrg.MainAddress.PK);
			AssertEquals("Carrier Export Creditor Address  is empty", consol.CarrierExportCreditorAddress.E2_OA_Address, ZGuid.Empty);

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();
				AssertEquals("Carrier Export Creditor Address should be set to Co-load With", consol.JK_OA_CreditorAddress, consol.CarrierExportCreditorAddress.E2_OA_Address);
			}

			coloadOrg.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery
				, consol.JK_TransportMode
				, consol.JK_ConsolMode);
			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();
				AssertEquals("Carrier Export Creditor Address should be set to Co-load With related party", relatedParty.MainAddress.PK, consol.CarrierExportCreditor.MainAddress.PK);
			}
		}

		public void TestExportPenalties_WhenUserClicksOnRecalculateRelatedParties_PenaltyCreditorIsSetFromCarrierRelatedPartyForNonColoadExportConsol()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();

			var consol = CreateConsolWithCarrierAndCreditor(carrier, creditor, isImport: false, coload: false);
			var container = consol.Containers.AddNew();
			var exportPenalty = AddExportPenaltyWithoutCreditor(container);
			var importPenalty = AddImportPenaltyWithCreditor(container, creditor);

			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();

				AssertEquals("Should update export penalty when consol is export", carrier.PK, exportPenalty.Creditor.PK);
				AssertEquals("Should not update import penalty when consol is export", creditor.PK, importPenalty.Creditor.PK);
			}

			var relatedParty = CreateRelatedPartyForCreditor(consol, carrier);

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();

				AssertEquals("Should update export penalty from SPC when consol is export", relatedParty.PK, exportPenalty.Creditor.PK);
				AssertEquals("Should not update import penalty when consol is export", creditor.PK, importPenalty.Creditor.PK);
			}
		}

		public void TestExportPenalties_WhenUserClicksOnRecalculateRelatedParties_PenaltyCreditorIsSetFromCreditorRelatedPartyForColoadExportConsol()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var consol = CreateConsolWithCarrierAndCreditor(carrier, creditor, isImport: false, coload: true);

			var container = consol.Containers.AddNew();
			var exportPenalty = AddExportPenaltyWithoutCreditor(container);
			var importPenalty = AddImportPenaltyWithCreditor(container, carrier);

			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();

				AssertEquals("Should update export penalty when consol is export", creditor.PK, exportPenalty.Creditor.PK);
				AssertEquals("Should not update import penalty when consol is export", carrier.PK, importPenalty.Creditor.PK);
			}

			var relatedParty = CreateRelatedPartyForCreditor(consol, creditor);

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();

				AssertEquals("Should update export penalty from SPC when consol is export", relatedParty.PK, exportPenalty.Creditor.PK);
				AssertEquals("Should not update import penalty when consol is export", carrier.PK, importPenalty.Creditor.PK);
			}
		}

		public void TestImportPenalties_WhenUserClicksOnRecalculateRelatedParties_PenaltyCreditorIsSetFromCarrierRelatedPartyForNonColoadImportConsol()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();

			var consol = CreateConsolWithCarrierAndCreditor(carrier, creditor, isImport: true, coload: false);
			var container = consol.Containers.AddNew();
			var importPenalty = AddImportPenaltyWithoutCreditor(container);
			var exportPenalty = AddExportPenaltyWithCreditor(container, creditor);

			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();

				AssertEquals("Should update import penalty when consol is import", carrier.PK, importPenalty.Creditor.PK);
				AssertEquals("Should not update export penalty when consol is import", creditor.PK, exportPenalty.Creditor.PK);
			}

			var relatedParty = CreateRelatedPartyForCreditor(consol, carrier);

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();

				AssertEquals("Should update import penalty from SPC when consol is import", relatedParty.PK, importPenalty.Creditor.PK);
				AssertEquals("Should not update export penalty when consol is import", creditor.PK, exportPenalty.Creditor.PK);
			}
		}

		public void TestImportPenalties_WhenUserClicksOnRecalculateRelatedParties_PenaltyCreditorIsSetFromCreditorRelatedPartyForColoadImportConsol()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var consol = CreateConsolWithCarrierAndCreditor(carrier, creditor, isImport: true, coload: true);
			var container = consol.Containers.AddNew();
			var importPenalty = AddImportPenaltyWithoutCreditor(container);
			var exportPenalty = AddExportPenaltyWithCreditor(container, carrier);

			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();

				AssertEquals("Should update import penalty when consol is import", creditor.PK, importPenalty.Creditor.PK);
				AssertEquals("Should not update export penalty when consol is import", carrier.PK, exportPenalty.Creditor.PK);
			}

			var relatedParty = CreateRelatedPartyForCreditor(consol, creditor);

			using (var form = new ConsolForm(consol))
			{
				var recalculateMenuItem = FindMenuItem(form, "Actions", "Recalculate Creditor for logged in Company");
				recalculateMenuItem.PerformClick();

				AssertEquals("Should update import penalty from SPC when consol is import", relatedParty.PK, importPenalty.Creditor.PK);
				AssertEquals("Should not update export penalty when consol is import", carrier.PK, exportPenalty.Creditor.PK);
			}
		}

		ForwardingConsol CreateConsolWithCarrierAndCreditor(OrgHeader carrier, OrgHeader creditor, bool isImport, bool coload)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = isImport ? OverseasPort : HomePort;
			consol.JK_RL_NKDischargePort = isImport ? HomePort : OverseasPort;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = coload ? Constants.AgentType.CoLoad : Constants.AgentType.Agent;

			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.CreditorPK = creditor.PK;

			var expectedDirection = isImport ? "Import" : "Export";
			AssertEquals($"Preconditions: {expectedDirection} Consol expected.", isImport, consol.IsImport());

			var expectedMode = coload ? "CLD" : "Non CLD";
			AssertEquals($"Preconditions: {expectedMode} mode.", coload, consol.IsCoLoad);

			return consol;
		}

		OrgHeader CreateRelatedPartyForCreditor(ForwardingConsol consol, OrgHeader creditor)
		{
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_IsCreditor = true;

			creditor.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery
				, consol.JK_TransportMode
				, consol.JK_ConsolMode);
			Factory.Save();
			return relatedParty;
		}

		ContainerPenalty AddExportPenaltyWithoutCreditor(ForwardingContainer container)
		{
			var exportPenalty = container.ExportPenalties.AddNew();
			exportPenalty.CPY_RL_NKLocation = HomePort;
			exportPenalty.CPY_PenaltyType = "STO";
			exportPenalty.CPY_CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			exportPenalty.CPY_OH_Creditor = ZGuid.Empty; // clear penalty creditor
			return exportPenalty;
		}

		ContainerPenalty AddExportPenaltyWithCreditor(ForwardingContainer container, OrgHeader creditor)
		{
			var exportPenalty = container.ExportPenalties.AddNew();
			exportPenalty.CPY_RL_NKLocation = HomePort;
			exportPenalty.CPY_PenaltyType = "STO";
			exportPenalty.CPY_CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			exportPenalty.CPY_OH_Creditor = creditor.PK;
			return exportPenalty;
		}

		ContainerPenalty AddImportPenaltyWithoutCreditor(ForwardingContainer container)
		{
			var importPenalty = container.ImportPenalties.AddNew();
			importPenalty.CPY_RL_NKLocation = HomePort;
			importPenalty.CPY_PenaltyType = "STO";
			importPenalty.CPY_CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			importPenalty.CPY_OH_Creditor = ZGuid.Empty; // clear penalty creditor
			return importPenalty;
		}

		ContainerPenalty AddImportPenaltyWithCreditor(ForwardingContainer container, OrgHeader creditor)
		{
			var importPenalty = container.ImportPenalties.AddNew();
			importPenalty.CPY_RL_NKLocation = HomePort;
			importPenalty.CPY_PenaltyType = "STO";
			importPenalty.CPY_CreditorType = Constants.ContainerPenaltyCreditorType.Codes.Carrier;
			importPenalty.CPY_OH_Creditor = creditor.PK;
			return importPenalty;
		}

		public void TestAddExportCreditorAddress_ForCLDDomesticConsol_ShouldBeSetFromColoadWithRelatedParty()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = AlternateHomePort;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ConsolMode = Constants.ContainerModes.FCL;
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			AssertEquals("Preconditions: Domestic Consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: CLD mode.", true, consol.IsCoLoad);

			var coloadOrg = Factory.NewWithValidTestData<OrgHeader>();
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			coloadOrg.OH_IsCreditor = true;
			relatedParty.OH_IsCreditor = true;
			Factory.Save();

			consol.JK_OA_CreditorAddress = coloadOrg.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				using (var plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.SelectTabPage();
					plugIn.OnUserControlShown();
					var control = (DocAddressUserControl)plugIn.UserControl;
					var splitContainer = (SplitContainer)control.Controls["SplitContainer"];
					var addressGrid = (MasterFiles.GUI.Internal.DocAddressGrid)splitContainer.Panel1.Controls["AddressGrid"];
					addressGrid.OnPopup_CallForTesting();

					var carrierExportCreditorMenu = addressGrid.ContextMenu.MenuItems.FindByText("Add Carrier Export Creditor");
					AssertNotNull(carrierExportCreditorMenu);
					carrierExportCreditorMenu.PerformClick();
					AssertEquals("Export creditor address should fallback to Co-load With", coloadOrg.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);
				}
			}

			coloadOrg.SetRelatedParty(relatedParty
				, RelatedPartyTypeList.Codes.ServiceProviderCreditor
				, RelatedPartyDirectionList.Codes.PickupAndDelivery
				, consol.JK_TransportMode
				, consol.JK_ConsolMode);
			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;
			Factory.Save();

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				using (var plugIn = (DocAddressesPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses))
				{
					AssertNotNull(plugIn);
					plugIn.SelectTabPage();
					plugIn.OnUserControlShown();
					var control = (DocAddressUserControl)plugIn.UserControl;
					var splitContainer = (SplitContainer)control.Controls["SplitContainer"];
					var addressGrid = (MasterFiles.GUI.Internal.DocAddressGrid)splitContainer.Panel1.Controls["AddressGrid"];
					addressGrid.OnPopup_CallForTesting();

					var carrierExportCreditorMenu = addressGrid.ContextMenu.MenuItems.FindByText("Add Carrier Export Creditor");
					AssertNotNull(carrierExportCreditorMenu);
					carrierExportCreditorMenu.PerformClick();
					AssertEquals("Export creditor address should be set to Co-load With related party", relatedParty.MainAddress.PK, consol.CarrierExportCreditorAddress.E2_OA_Address);
				}
			}
		}

		public void TestConsolForm_CarrierBookingAgent_IfExists()
		{
			var orgAgentAUMEL = Factory.New<OrgHeader>();
			orgAgentAUMEL.OH_Code = "AGN01";
			orgAgentAUMEL.OH_FullName = "Carrier Booking Agent - AUMEL";
			orgAgentAUMEL.MainAddress.Address1 = "Address 1-1";

			var orgAgentAUSYD = Factory.New<OrgHeader>();
			orgAgentAUSYD.OH_Code = "AGN02";
			orgAgentAUSYD.OH_FullName = "Carrier Booking Agent - AUSYD";
			orgAgentAUSYD.MainAddress.Address1 = "Address 1-2";

			var orgAgentAUMEL1 = Factory.New<OrgHeader>();
			orgAgentAUMEL1.OH_Code = "AGN03";
			orgAgentAUMEL1.OH_FullName = "Carrier Booking Agent - AUMEL 2";
			orgAgentAUMEL1.MainAddress.Address1 = "Address 1-1";

			var orgAgentAUSYD2 = Factory.New<OrgHeader>();
			orgAgentAUSYD2.OH_Code = "AGN04";
			orgAgentAUSYD2.OH_FullName = "Carrier Booking Agent - AUSYD 2";
			orgAgentAUSYD2.MainAddress.Address1 = "Address 1-2";

			var agentPort1 = ShippingCompany1.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort1.O5_PortOrCountry = "AUMEL";
			agentPort1.OrganisationPK = orgAgentAUMEL.PK;

			var agentPort2 = ShippingCompany1.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort2.O5_PortOrCountry = "AUSYD";
			agentPort2.OrganisationPK = orgAgentAUSYD.PK;

			var agentPort3 = ShippingCompany2.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort3.O5_PortOrCountry = "AUMEL";
			agentPort3.OrganisationPK = orgAgentAUMEL1.PK;

			var agentPort4 = ShippingCompany2.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort4.O5_PortOrCountry = "AUSYD";
			agentPort4.OrganisationPK = orgAgentAUSYD2.PK;

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			Factory.Save();

			using (var consolForm = new ConsolFormTestClass(consol))
			{
				consolForm.Show();

				AssertEquals(0, consol.DocAddresses.Count);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;

				AssertEquals(2, consol.DocAddresses.Count);
				AssertEquals(orgAgentAUMEL.MainAddress.PK, consol.DocAddresses.ToList<JobDocAddress>().FirstOrDefault(x => x.DocAddressType == DocAddressType.CarrierBookingAgent).E2_OA_Address);
				AssertEquals(ShippingCompany1.MainAddress.PK, consol.DocAddresses.ToList<JobDocAddress>().FirstOrDefault(x => x.DocAddressType == DocAddressType.CarrierExportCreditor).E2_OA_Address);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				consol.JK_RL_NKLoadPort = "AUSYD";

				AssertEquals(2, consol.DocAddresses.Count);
				AssertEquals(ShippingCompany1.MainAddress.PK, consol.DocAddresses.ToList<JobDocAddress>().FirstOrDefault(x => x.DocAddressType == DocAddressType.CarrierExportCreditor).E2_OA_Address);
				AssertEquals(orgAgentAUSYD.MainAddress.PK, consol.DocAddresses.ToList<JobDocAddress>().FirstOrDefault(x => x.DocAddressType == DocAddressType.CarrierBookingAgent).E2_OA_Address);
				AssertEquals("The 1st Load Port has been changed.  Do you wish to re-default the Carrier Booking Agent from the Carrier based on the new 1st Load Port?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				consol.JK_OA_ShippingLineAddress = ShippingCompany2.MainAddress.PK;
				AssertEquals("The Carrier has been changed.  Do you wish to re-default the Carrier Booking Agent from the Carrier based on the new 1st Load Port?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolForm_CarrierBookingAgent_IfNotExists()
		{
			var orgAgentAUMEL = Factory.New<OrgHeader>();
			orgAgentAUMEL.OH_Code = "AGN01";
			orgAgentAUMEL.OH_FullName = "Carrier Booking Agent - AUMEL";
			orgAgentAUMEL.MainAddress.Address1 = "Address 1-1";

			var orgAgentAUSYD = Factory.New<OrgHeader>();
			orgAgentAUSYD.OH_Code = "AGN02";
			orgAgentAUSYD.OH_FullName = "Carrier Booking Agent - AUSYD";
			orgAgentAUSYD.MainAddress.Address1 = "Address 1-2";

			var agentPort1 = ShippingCompany1.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort1.O5_PortOrCountry = "AUMEL";
			agentPort1.OrganisationPK = orgAgentAUMEL.PK;

			var agentPort2 = ShippingCompany1.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort2.O5_PortOrCountry = "AUSYD";
			agentPort2.OrganisationPK = orgAgentAUSYD.PK;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUXXX";
			Factory.Save();

			using (var consolForm = new ConsolFormTestClass(consol))
			{
				consolForm.Show();

				AssertEquals(0, consol.DocAddresses.Count);

				consol.JK_OA_ShippingLineAddress = ShippingCompany1.MainAddress.PK;

				AssertEquals(1, consol.DocAddresses.Count);
				AssertEquals(ShippingCompany1.MainAddress.PK, consol.DocAddresses.ToList<JobDocAddress>().FirstOrDefault(x => x.DocAddressType == DocAddressType.CarrierExportCreditor).E2_OA_Address);

				Factory.Save();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				consol.JK_RL_NKLoadPort = "AUSYD";
				AssertEquals(2, consol.DocAddresses.Count);
				AssertEquals(ShippingCompany1.MainAddress.PK, consol.DocAddresses.ToList<JobDocAddress>().FirstOrDefault(x => x.DocAddressType == DocAddressType.CarrierExportCreditor).E2_OA_Address);
				AssertEquals(orgAgentAUSYD.MainAddress.PK, consol.DocAddresses.ToList<JobDocAddress>().FirstOrDefault(x => x.DocAddressType == DocAddressType.CarrierBookingAgent).E2_OA_Address);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolForm_CarrierBookingAgent_DoNotDefault_DuringTransaction()
		{
			var orgAgentAUMEL = Factory.New<OrgHeader>();
			orgAgentAUMEL.OH_Code = "AGN01";
			orgAgentAUMEL.OH_FullName = "Carrier Booking Agent - AUMEL";
			orgAgentAUMEL.MainAddress.Address1 = "Address 1-1";

			var orgAgentAUSYD = Factory.New<OrgHeader>();
			orgAgentAUSYD.OH_Code = "AGN02";
			orgAgentAUSYD.OH_FullName = "Carrier Booking Agent - AUSYD";
			orgAgentAUSYD.MainAddress.Address1 = "Address 1-2";

			var shippingOrg = Factory.NewWithValidTestData<OrgHeader>();

			var agentPort1 = shippingOrg.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort1.O5_PortOrCountry = "AUMEL";
			agentPort1.OrganisationPK = orgAgentAUMEL.PK;

			var agentPort2 = shippingOrg.CarrierAppointedAgentPorts_Agency.AddNew();
			agentPort2.O5_PortOrCountry = "AUSYD";
			agentPort2.OrganisationPK = orgAgentAUSYD.PK;

			var consol = CreateMAWBWithConsol();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_OA_ShippingLineAddress = shippingOrg.MainAddress.PK;

			Factory.Save();

			AssertEquals(orgAgentAUSYD, consol.CarrierBookingAgent);
			AssertEquals(orgAgentAUSYD.MainAddress.PK, consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address);

			using (ConsolFormTestClass form = new ConsolFormTestClass(consol))
			{
				form.Show();

				try
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					Db.Connection.BeginTransaction();

					consol.JK_RL_NKLoadPort = "AUMEL";

					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(orgAgentAUSYD, consol.CarrierBookingAgent);
					AssertEquals(orgAgentAUSYD.MainAddress.PK, consol.CarrierBookingAgentDocumentaryAddress.E2_OA_Address);
				}
				finally
				{
					Db.Connection.RollbackTransaction();
				}
			}
		}

		[TestDate(2006, 9, 28, 12, 0, 0)]
		public void TestExportToVerboseXml_ConsolReferenceInFileName()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			Factory.Save();
			SystemDataRegistry.Instance.ConsolExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, EnvProxy.Instance.TempPath);

			var expectedFileName = consol.JK_UniqueConsignRef + "_" + ZDateTime.Today.ToString("yyyyMMddhhmmss");
			ZFormModaliser.FileNameToSelectInShowCommonDialog = Path.Combine(EnvProxy.Instance.TempPath, expectedFileName + ".xml");
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			try
			{
				var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				using (var consolForm = new ConsolFormWithXmlDirector(consol))
				{
					consolForm.Show();
					Application.DoEvents();

					var exportToXmlMenu = consolForm.ActionsMenuItem.MenuItems.FindByText(ExportXmlMenuItemHelper.VerboseMenuItemText);
					MenuItem storeAsFileMenu = null;
					foreach (MenuItem menu in exportToXmlMenu.MenuItems)
					{
						if (menu.Text == "Store as File")
						{
							storeAsFileMenu = menu;
							break;
						}
					}
					if (storeAsFileMenu != null)
					{
						storeAsFileMenu.PerformClick();
					}
				}
				Assert("File Should have been created", File.Exists(Path.Combine(EnvProxy.Instance.TempPath, expectedFileName + ".xml")));
			}
			finally
			{
				DeleteIfExists(Path.Combine(EnvProxy.Instance.TempPath, expectedFileName + ".xml"));
				var empty = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, empty);
			}
		}

		[TestDate(2006, 9, 28, 12, 0, 0)]
		public void TestExportToLightWeightXml_ConsolReferenceInFileName()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			Factory.Save();
			SystemDataRegistry.Instance.ConsolExportDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, EnvProxy.Instance.TempPath);
			var expectedFileName = consol.JK_UniqueConsignRef + "_" + ZDateTime.Today.ToString("yyyyMMddhhmmss");
			ZFormModaliser.FileNameToSelectInShowCommonDialog = Path.Combine(EnvProxy.Instance.TempPath, expectedFileName + ".xml");
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			try
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				using (var consolForm = new ConsolFormWithXmlDirector(consol))
				{
					consolForm.Show();
					Application.DoEvents();

					var exportToXmlMenu = consolForm.ActionsMenuItem.MenuItems.FindByText(ExportXmlMenuItemHelper.LightWeightMenuItemText);
					MenuItem storeAsFileMenu = null;
					foreach (MenuItem menu in exportToXmlMenu.MenuItems)
					{
						if (menu.Text == "Store as File")
						{
							storeAsFileMenu = menu;
							break;
						}
					}
					if (storeAsFileMenu != null)
					{
						storeAsFileMenu.PerformClick();
					}
				}
				Assert("File Should have been created", File.Exists(Path.Combine(EnvProxy.Instance.TempPath, expectedFileName + ".xml")));
			}
			finally
			{
				DeleteIfExists(Path.Combine(EnvProxy.Instance.TempPath, expectedFileName + ".xml"));

				tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			}
		}
		public void TestAWBPopulatedOnTabChanging()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.AWBHeader.Populate();
			AssertEquals("", consol.AWBHeader.EH_AWBOriginCode);
			AssertEquals(0, consol.AWBHeader.ShipperAddressPickList.Count);

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_OA_SendingForwarderAddress = Factory.New<OrgHeader>().MainAddress.PK;
				consol.SendingForwarder.MainAddress.OA_Address1 = "TEST";
				consolForm.SelectAWBTabPage_ForTest();
				AssertEquals("SYD", consol.AWBHeader.EH_AWBOriginCode);
				AssertEquals(2, consol.AWBHeader.ShipperAddressPickList.Count);
			}
		}

		public void TestAWBPopulatedOnTabChanging_MacrosExceptionOccur()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			using (FreightDataRegistry.Instance.MAWBNatureAndQtyOfGoodsExtraText.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "<BillNumber.Find(\"{First}\" == \"1\").First()>"))
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				consolForm.SelectAWBTabPage_ForTest();
				AssertEquals($@"MAWB cannot be generated.
Please correct macro format in Registry -> {FreightDataRegistry.Instance.MAWBNatureAndQtyOfGoodsExtraText.HumanReadableRegistryPath()}.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestRowRemovedFromTableExceptionOnDetachShipments() // Issue 17416
		{
			var savedShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var newConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			newConsol.JK_TransportMode = "AIR";
			newConsol.JK_ConsolMode = "LSE";
			newConsol.JK_RL_NKLoadPort = "AUSYD";
			newConsol.JK_RL_NKDischargePort = "USLAX";

			var transport = newConsol.Transports[0];
			transport.JW_VoyageFlight = "QF123";
			newConsol.MasterBillAirlinePrefix = "333";
			transport.JW_ETD = ZDateTime.Today;
			Factory.Save();

			using (var consolForm = new ConsolFormTestClass(newConsol))
			{
				consolForm.Show();

				newConsol.Shipments.Add(savedShipment);
				CommonShipment newShipmentWithErrors = newConsol.Shipments.AddNew();
				newShipmentWithErrors.ConsigneePK = ZGuid.Invalid;

				newConsol.Shipments.Remove(savedShipment);
				Factory.Save();

				newConsol.Shipments.Remove(newShipmentWithErrors);
				Factory.Save();
			}
		}

		public void TestAWBUserControl_TransportModeVisibility()
		{
			using (var consolForm = GetNewZConsolForm())
			{
				consolForm.Show();

				Consol.JK_TransportMode = "";

				CombineAssertions("Pre-conditions", delegate
				{
					AssertEquals("Default no AWB tab page", false, consolForm.MainTabControl.TabPages.Contains(consolForm.AWBTabPage));
					AssertEquals("View menu should not show AWB", false, consolForm.AWBTabPage.TabRelevant);
				});

				var transportModes = typeof(TransportModes).GetFields();

				foreach (var transportMode in transportModes)
				{
					Consol.JK_TransportMode = transportMode.GetValue(null) as string;

					if (Consol.JK_TransportMode.Equals(Constants.TransportModes.Air))
					{
						CombineAssertions("The only transport mode that should show AWB tab is AIR", delegate
						{
							Assert("AIR consol should have AWB tab.", consolForm.MainTabControl.TabPages.Contains(consolForm.AWBTabPage));
							Assert("View menu should show AWB", consolForm.AWBTabPage.TabRelevant);
						});
					}
					else
					{
						CombineAssertions($"{Consol.JK_TransportMode} should not have AWB tab/view menu", delegate
						{
							Assert("This consol should not have AWB tab.", !consolForm.MainTabControl.TabPages.Contains(consolForm.AWBTabPage));
							Assert("View menu should not show AWB", !consolForm.AWBTabPage.TabRelevant);
						});
					}
				}
			}
		}

		public void TestAWBUserControl_IsHiddenForMultiAWBMaster()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.AWBMaster;

			using (var form = new ConsolFormTestClass(consol))
			{
				form.Show();
				AssertEquals("Invisible for multi AWB master", false, form.MainTabControl.TabPages.Contains(form.AWBTabPage));

				consol.JK_AgentType = Constants.AgentType.AWBCoload;
				AssertEquals("Visible for AWB coload", true, form.MainTabControl.TabPages.Contains(form.AWBTabPage));

				consol.JK_AgentType = Constants.AgentType.AWBMaster;
				AssertEquals("Invisible for multi AWB master", false, form.MainTabControl.TabPages.Contains(form.AWBTabPage));

				consol.JK_AgentType = Constants.AgentType.Agent;
				AssertEquals("Visible for Agent", true, form.MainTabControl.TabPages.Contains(form.AWBTabPage));
			}
		}

		public void TestAWBUserControl_IsHiddenForCourier()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentType = Constants.AgentType.Courier;

			using (var form = new ConsolFormTestClass(consol))
			{
				form.Show();
				AssertEquals("Invisible for Courier", false, form.MainTabControl.TabPages.Contains(form.AWBTabPage));

				consol.JK_AgentType = Constants.AgentType.Agent;
				AssertEquals("Visible for Agent", true, form.MainTabControl.TabPages.Contains(form.AWBTabPage));
			}
		}

		public void TestAWBUserControlIntegrationForIsDomesticFreightCheckBox()
		{
			using (var consolForm = GetNewZConsolForm())
			{
				consolForm.Show();

				AssertEquals(false, Consol.IsDomesticFreight);
				AssertEquals("Default no AWB tab page", false, consolForm.MainTabControl.TabPages.Contains(consolForm.AWBTabPage));

				Consol.IsDomesticFreight = true;
				AssertEquals("Consol should have not an AWB tab.", false, consolForm.MainTabControl.TabPages.Contains(consolForm.AWBTabPage));

				Consol.JK_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Consol should have an AWB tab.", true, consolForm.MainTabControl.TabPages.Contains(consolForm.AWBTabPage));

				Consol.IsDomesticFreight = false;
				AssertEquals("Consol should have an AWB tab.", true, consolForm.MainTabControl.TabPages.Contains(consolForm.AWBTabPage));
			}
		}

		[ExpectNoExceptions]
		public void TestDeliveryAgentsFormShow()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			using (var consolForm = new ConsolForm(consol))
			{
				consol.JK_UniqueConsignRef = "C1";
				var deliveryAgent = Factory.New<DeliveryAgentOrgHeader>();
				deliveryAgent.OH_Code = "CCC";
				deliveryAgent.OH_FullName = "Delivery Agent";
				deliveryAgent.MainAddress.OA_Address1 = "Address 1";

				var shipment1 = (ForwardingShipment)consol.Shipments.AddNew(typeof(ForwardingShipment));
				shipment1.JS_OH_DeliveryAgent = deliveryAgent.PK;
				shipment1.JS_UniqueConsignRef = "S1";
				shipment1.JS_HouseBillOfLadingType = "FIA";
				shipment1.JS_TransportMode = Constants.TransportModes.Sea;
				shipment1.JS_PackingMode = Constants.ContainerModes.LCL;
				shipment1.JS_NoCopyBills = 1;
				shipment1.JS_NoOriginalBills = 1;

				var shipment2 = (ForwardingShipment)consol.Shipments.AddNew(typeof(ForwardingShipment));
				shipment2.JS_OH_DeliveryAgent = deliveryAgent.PK;
				shipment2.JS_UniqueConsignRef = "S2";
				shipment2.JS_HouseBillOfLadingType = "FIA";
				shipment2.JS_TransportMode = Constants.TransportModes.Sea;
				shipment2.JS_PackingMode = Constants.ContainerModes.LCL;
				shipment2.JS_NoCopyBills = 1;
				shipment2.JS_NoOriginalBills = 1;

				var accHeader1 = Factory.New<AccTransactionHeader>();
				accHeader1.AH_ConsolidatedInvoiceRef = consol.JK_UniqueConsignRef;
				accHeader1.AH_Ledger = LedgerTypes.AccountsReceivable;
				accHeader1.AH_TransactionType = TransactionTypes.Invoice;
				accHeader1.AH_OH = deliveryAgent.PK;
				accHeader1.AH_InvoiceDate = ZDateTime.Now;
				accHeader1.AH_GB = GlbBranch.CurrentBranch.PK;
				accHeader1.AH_GE = GlbDepartment.CurrentDepartment.PK;

				var accHeader2 = Factory.New<AccTransactionHeader>();
				accHeader2.AH_ConsolidatedInvoiceRef = shipment2.JS_UniqueConsignRef;
				accHeader2.AH_Ledger = LedgerTypes.AccountsReceivable;
				accHeader2.AH_TransactionType = TransactionTypes.Invoice;
				accHeader2.AH_OH = deliveryAgent.PK;
				accHeader2.AH_InvoiceDate = ZDateTime.Now;
				accHeader2.AH_GB = GlbBranch.CurrentBranch.PK;
				accHeader2.AH_GE = GlbDepartment.CurrentDepartment.PK;

				consolForm.Show();

				var queryProvider = new Mock<IForwardingConsolDocumentSupporterQueryProvider>(MockBehavior.Strict);
				Factory.SetValue(() => queryProvider.Object);

				queryProvider
					.Setup(m => m.GetDeliveryAgentsToPrint(It.IsAny<DeliveryAgentToSelectFromForPrintingCollection>()))
					.Returns((DeliveryAgentOrgHeader[])null);
				var deliveryAgentPackMenu = Factory.LoadTop1<StmMenuItem>(new DocumentZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.StartsWith, "Delivery Agent Pack (Air)"));
				consol.DocumentSupporter.GetDataStateBeforeRun(deliveryAgentPackMenu);
				queryProvider
					.Verify(m => m.GetDeliveryAgentsToPrint(It.IsAny<DeliveryAgentToSelectFromForPrintingCollection>()), Times.Once);
			}
		}

		public void TestExportToOverseasAgentMenuItem()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Transports[0].JW_JX = ExportSailing.PK;
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			Factory.Save();

			Env.OutgoingMailManager.EmailsCreated.Clear();
			AssertEquals("EmailsNotSentBecauseInTestMode", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			using (var form = new ConsolForm(consol))
			{
				form.Show();

				ClickExportToOverseasAgentMenuItem(form);
				AssertEquals("EmailsNotSentBecauseInTestMode", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();
				consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
				Factory.Save();

				ClickExportToOverseasAgentMenuItem(form);
				AssertEquals("EmailsNotSentBecauseInTestMode", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				var mode = receivingForwarder.EDICommunicationsModes.AddNew();
				mode.EK_FileFormat = "XML";
				mode.EK_Module = JobInvoicingConsumerTypes.Consol.Code;
				mode.EK_Destination = "BLAH";
				Factory.Save();

				ClickExportToOverseasAgentMenuItem(form);
				AssertEquals("EmailsNotSentBecauseInTestMode", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				mode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				Factory.Save();

				ClickExportToOverseasAgentMenuItem(form);
				AssertEquals("EmailsNotSentBecauseInTestMode", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				mode.EK_Destination = "art@edi.com";
				Factory.Save();

				ClickExportToOverseasAgentMenuItem(form);
				AssertEquals("EmailsNotSentBecauseInTestMode", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Subject", "ediEnterprise Consol XML File", email.Subject);
				AssertEquals("Number of Recipients", 1, email.Recipients.Count);
				AssertEquals("Recipient", receivingForwarder.EDICommunicationsModes.GetXmlCommunicationMode(JobInvoicingConsumerTypes.Consol.Code).EK_Destination, email.Recipients[0]);
				AssertEquals("Number of Attachments", 1, email.Attachments.Count);
				var displayName = "Consol_" + consol.JK_UniqueConsignRef + "___MasterBill_" + consol.JK_MasterBillNum + ".xml";
				AssertEquals("Attachment File Name", displayName, email.Attachments[0].DisplayName);
				AssertEquals("From Address", Env.Registry.MailboxEmailAddress, email.FromAddress);
				AssertEquals("From Name", Env.Registry.MailboxDisplayName, email.FromDisplayName);

				tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			}
		}

		public void TestPlugInsForGatewayConsol()
		{
			var orgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			orgProxy.OH_IsConsignee = true;
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_UniqueConsignRef = "C00001111";

			consol.JK_OA_SendingForwarderAddress = orgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertNotNull("Precondition", consol.SendingForwarder);

			var orgAppointedAgentPorts1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.JK_AgentType = Constants.AgentType.Agent;

			Assert(consol.IsGateway());

			using (AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly())
			{
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					AssertContainsPlugIn(form, ControllerIDs.JobInvoicingConsol, Env.Security.GatewayConsolJobInvoicing);
					AssertContainsPlugIn(form, ControllerIDs.Apportionment, Env.Security.MaintainConsolJobInvoicing);
					AssertContainsPlugIn(form, ControllerIDs.SellApportionmentForGateway, Env.Security.GatewayConsolJobInvoicing);
				}
			}
			using (AutoJRJRegistryStatusHelper.SetAutoJRJDisabled_ForTestOnly())
			{
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					AssertContainsPlugIn(form, ControllerIDs.JobInvoicingConsol, Env.Security.GatewayConsolJobInvoicing);
					AssertContainsPlugIn(form, ControllerIDs.Apportionment, Env.Security.MaintainConsolJobInvoicing);
					AssertDoesNotContainPlugin(form, ControllerIDs.SellApportionmentForGateway);

					var consolCost = CreateConsolCost(consol);
					consolCost[JobConsolCostSchema.E6_GatewaySellChargeID] = ZGuid.NewZGuid();
					AssertEquals(true, consol.HasGatewaySellApportionments(GlbCompany.CurrentCompany));
					form.Refresh();
					AssertContainsPlugIn(form, ControllerIDs.Apportionment, Env.Security.MaintainConsolJobInvoicing);
					AssertContainsPlugIn(form, ControllerIDs.SellApportionmentForGateway, Env.Security.GatewayConsolJobInvoicing);
					consolCost.Delete();
				}
			}
		}

		public void TestPlugIns()
		{
			var originalCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var orginalIsDeveloper = GlbStaff.CurrentUser.GS_IsDeveloper;

			try
			{
				var consol = Factory.New<ForwardingConsol>();

				using (var form = new ConsolForm(consol))
				{
					form.Show();

					var electronicMessagingTabControl = (ZTabControl)form.Controls.Find("ElectronicMessagingTabControl", true).First();

					AssertContainsPlugIn(form, ControllerIDs.Customs.JP.AFRPluggedIntoConsol);
					AssertContainsPlugIn(form, ControllerIDs.DtbBooking);
				}

				using (var form1 = new ConsolForm(consol))
				{
					form1.Show();
					AssertContainsPlugIn(form1, (ControllerIDs.Customs.GB.CcsukAirInventory), Constants.CountryCodes.UnitedKingdom);
					AssertDoesNotContainPlugin(form1, (ControllerIDs.Customs.GB.ChiefExportConsolIntegrationController));
					AssertDoesNotContainPlugin(form1, (ControllerIDs.Customs.FR.CINExportConsolIntegrationController));
				}
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedKingdom);

				using (var form = new ConsolForm(consol))
				{
					form.Show();
					AssertContainsPlugIn(form, (ControllerIDs.Customs.ASYCUDA.ASYCUDAManifest));
				}

				using (var form = new ConsolForm(consol))
				{
					form.Show();
					AssertContainsPlugIn(form, ControllerIDs.Customs.US.AMS);

					var jobInvoicingMenuItem = form.Menu.MenuItems.FindByText("Job Invoicing");
					var amsMenuItem = form.Menu.MenuItems.FindByText("AMS");
					var amsIndex = form.Menu.MenuItems.IndexOf(amsMenuItem);
					var jobInvoicingIndex = form.Menu.MenuItems.IndexOf(jobInvoicingMenuItem);
					AssertEquals(string.Format("AMS menu ({0}) should be before Job Invoicing ({1})", amsIndex, jobInvoicingIndex), true, amsIndex < jobInvoicingIndex);
				}

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedKingdom);
				consol.JK_RL_NKLoadPort = "GBXXX";
				consol.JK_RL_NKDischargePort = "ERXXX";
				using (var form = new ConsolFormTestClass(consol))
				{
					form.Show();
					var plugin = form.ElectronicMessagingTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.GB.ChiefExportConsolIntegrationController);
					AssertNotNull("ChiefExportConsolIntegrationController should be plugged in", plugin);
				}
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.France);
				consol = Factory.New<ForwardingConsol>();
				consol.JK_RL_NKLoadPort = "FRXXX";
				consol.JK_RL_NKDischargePort = "ERXXX";
				using (var form = new ConsolFormTestClass(consol))
				{
					form.Show();
					var plugin = form.ElectronicMessagingTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.FR.CINExportConsolIntegrationController);
					AssertNotNull("CINExportConsolIntegrationController should be plugged in", plugin);
				}

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.France);
				consol.JK_TransportMode = "AIR";
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					AssertDoesNotContainPlugin(form, ControllerIDs.Customs.FR.CINTemporaryStorageConsolController);
				}

				consol.JK_RL_NKDischargePort = "FRXXX";
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					AssertContainsPlugIn(form, ControllerIDs.Customs.FR.CINTemporaryStorageConsolController);
				}

				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Israel);
				consol.JK_RL_NKDischargePort = "ILXXX";
				using (var form = new ConsolFormTestClass(consol))
				{
					form.Show();
					var plugin = form.ElectronicMessagingTabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.IL.CustomsMessaging);
					AssertNotNull("IL.CustomsMessaging should be plugged in", plugin);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(originalCountryCode);
				GlbStaff.CurrentUser.GS_IsDeveloper = orginalIsDeveloper;
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestElectronicMenuItems()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_IsSeaWholesaler = false;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			using (var form = new ConsolForm(consol))
			{
				form.Show();

				var electronicMessagingMenuItem = (ZMenuItem)form.Menu
					.MenuItems
					.Cast<MenuItem>()
					.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

				AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

				electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

				AssertMultilineASCIIEquals("Electronic Menu Items for sea consol",
@"Electronic Messaging
   Carrier
      Booking Request
      Shipping Instruction
      Verified Gross Container Weight",
					electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestElectronicMenuItems_Acas()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };

			using (eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.Transports[0].JW_RL_NKDiscPort = "USLAX";

				using (var form = new ConsolForm(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items for US bound air consol",
@"Electronic Messaging
   Advanced Air Cargo Report
      ACAS House Checklist
   Carrier
      Air Cargo Booking Request",
						electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestElectronicMenuItems_CCT()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };

			using (eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.Transports[0].JW_RL_NKDiscPort = "BRSAO";

				using (var form = new ConsolForm(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items for BR bound air consol",
						@"Electronic Messaging
   Advanced Air Cargo Report
      CCT House Manifest
   Carrier
      Air Cargo Booking Request",
						electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestElectronicMenuItems_Netherlands()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Netherlands))
			{
				var carrier = Factory.New<OrgHeader>();
				carrier.OH_IsShippingLine = true;
				carrier.OH_IsSeaWholesaler = false;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = TransportModes.Sea;
				consol.JK_AgentType = AgentType.Agent;
				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

				using (var form = new ConsolForm(consol))
				{
					form.Show();

					var electronicMessagingMenuItem = (ZMenuItem)form.Menu
						.MenuItems
						.Cast<MenuItem>()
						.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

					AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items for sea consol",
@"Electronic Messaging
   Carrier
      Booking Request
      Shipping Instruction
      Verified Gross Container Weight",
						electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());

					consol.JK_TransportMode = Core.Constants.TransportModes.Air;

					electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

					AssertMultilineASCIIEquals("Electronic Menu Items for air consol",
@"Electronic Messaging
   Carrier
      Air Cargo Booking Request",
						electronicMessagingMenuItem.GetVisibleMenuItemsCaptions());
				}
			}
		}

		public void TestCarrierElectronicMenuItems_Version()
		{
			var shippingInstructionStmMenu = GetStmMenuItem(ConsolSystemFormMenuItems.DocumentMenuShippingInstructionPK);
			shippingInstructionStmMenu.SU_MenuName = "Latest Shipping Instruction";

			var bookingRequestStmMenu = GetStmMenuItem(ConsolSystemFormMenuItems.DocumentMenuBookingRequestPK);
			bookingRequestStmMenu.SU_MenuName = "Latest Booking Request";
			var airBookingRequestStmMenu = GetStmMenuItem(ConsolSystemFormMenuItems.DocumentMenuAirBookingRequestPK);
			airBookingRequestStmMenu.SU_MenuName = "Air Booking Request";

			Factory.Save();

			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			var consolAir = Factory.New<ForwardingConsol>();
			consolAir.JK_TransportMode = Core.Constants.TransportModes.Air;

			using (var form = new ConsolForm(consolAir))
			{
				form.Show();

				var airBookingRequestMenuItem = FindCarrierSubMenuItem(form.Menu, "Air Booking Request");
				AssertEquals("Using air booking request", "Air Booking Request", airBookingRequestMenuItem.Caption);
			}

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				var shippingInstructionMenuItem = FindCarrierSubMenuItem(form.Menu, "Shipping Instruction");
				AssertEquals("Using latest shipping instruction version", "Latest Shipping Instruction", shippingInstructionMenuItem.Caption);

				var bookingRequestMenuItem = FindCarrierSubMenuItem(form.Menu, "Booking Request");
				AssertEquals("Using latest booking request version", "Latest Booking Request", bookingRequestMenuItem.Caption);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		StmMenuItem GetStmMenuItem(ZGuid menuPK)
		{
			var query = new ZQuery(StmMenuItemSchema.PK, menuPK);
			query.AddToFilter(StmMenuItemSchema.SU_MenuType, Constants.StmMenuItemTypes.Forms);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, CargoWise.Definitions.BusinessContext.Consol);

			return Factory.LoadTop1<StmMenuItem>(query);
		}

		ZMenuItem FindCarrierSubMenuItem(MainMenu mainMenu, string menuText)
		{
			var electronicMessagingMenuItem = (ZMenuItem)mainMenu
				.MenuItems
				.Cast<MenuItem>()
				.FirstOrDefault(mi => mi.Text == "Electronic Messaging");

			AssertNotNull("Electronic Messaging menu item exists", electronicMessagingMenuItem);

			electronicMessagingMenuItem.OnPopup(EventArgs.Empty);

			var carrierMenuItem = (ZMenuItem)electronicMessagingMenuItem
				.MenuItems
				.Cast<MenuItem>()
				.FirstOrDefault(mi => mi.Text == "Carrier");

			AssertNotNull("Carrier menu item exists", carrierMenuItem);

			carrierMenuItem.OnPopup(EventArgs.Empty);

			return (ZMenuItem)carrierMenuItem
				.MenuItems
				.Cast<MenuItem>()
				.FirstOrDefault(mi => mi.Text.Contains(menuText));
		}

		public void TestGatewayConsolForm_CompanyOrgProxy_GatewayAgent()
		{
			TestGatewayConsolForm(GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK, Constants.AgentType.Agent);
		}

		public void TestGatewayConsolForm_CompanyOrgProxy_GatewayCoLoad()
		{
			TestGatewayConsolForm(GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK, Constants.AgentType.CoLoad);
		}

		public void TestGatewayConsolForm_BranchOrgProxy_GatewayAgent()
		{
			TestGatewayConsolForm(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, Constants.AgentType.Agent);
		}

		public void TestGatewayConsolForm_BranchOrgProxy_GatewayCoLoad()
		{
			TestGatewayConsolForm(GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, Constants.AgentType.CoLoad);
		}

		void TestGatewayConsolForm(ZGuid orgProxyAddressGuid, string gatewayType)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "AUSYD";

			using (var form = new ConsolForm(consol))
			{
				form.Show();

				AssertContainsPlugIn(form, ControllerIDs.JobInvoicingConsol, Env.Security.MaintainConsolJobInvoicing);
				AssertContainsPlugIn(form, ControllerIDs.JobProfitLossConsol, Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsol, SecurityCore.ProfitLoss));
				AssertContainsPlugIn(form, ControllerIDs.Apportionment, Env.Security.MaintainConsolJobInvoicing);

				consol.JK_OA_SendingForwarderAddress = orgProxyAddressGuid;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				var gatewayagentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				gatewayagentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
				gatewayagentPort.O5_PortOrCountry = "AUSYD";
				gatewayagentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
				gatewayagentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

				consol.JK_AgentType = gatewayType;

				AssertContainsPlugIn(form, ControllerIDs.JobInvoicingConsol, Env.Security.GatewayConsolJobInvoicing);
				AssertContainsPlugIn(form, ControllerIDs.JobProfitLossConsol, Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsol, SecurityCore.ProfitLoss));
				AssertContainsPlugIn(form, ControllerIDs.Apportionment, Env.Security.MaintainConsolJobInvoicing);
			}
		}

		public void TestAddNewShipmentFromAttachOption_ShouldSynchronizeGatewaysForNewShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			Factory.Save();

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();

				var shipmentGrid = consolForm.ConsolControl.ShipmentModuleButtonGrid;
				shipmentGrid.AttachButtonForTest.PerformClick();

				using (var shipmentModule = shipmentGrid.LastShownAttachPopupForTesting.Module_ForTest)
				{
					var button = shipmentModule.ToolBarButtons.FirstOrDefault(t => t.Text == "New") as ZToolBarButton;
					button.PerformClick();

					using (var lastShownForm = ZFormModaliser.LastFormShownForTest)
					{
						AssertNotNull(lastShownForm);
						Assert("Last shown form should be ShipmentForm.", lastShownForm is ShipmentForm);

						if (lastShownForm is ShipmentForm shipmentForm)
						{
							Assert("Shipment form should have forwarding shipment as business entity.", shipmentForm.BusinessEntity is ForwardingShipment);

							if (shipmentForm.BusinessEntity is ForwardingShipment forwardingShipment)
							{
								AssertEquals(2, forwardingShipment.Gateways.Count);
								AssertEquals(sendingForwarder.MainAddress.PK, forwardingShipment.Gateways[0].JSG_OA_ForwarderAddress);
								AssertEquals(receivingForwarder.MainAddress.PK, forwardingShipment.Gateways[1].JSG_OA_ForwarderAddress);
							}
						}
					}
				}
			}
		}

		public void TestAddNewShipmentFromNewOption_ShouldSynchronizeGatewaysForNewShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;

			var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;

			Factory.Save();

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();

				var shipmentGrid = consolForm.ConsolControl.ShipmentModuleButtonGrid;
				shipmentGrid.InnerGrid.Select(0);

				var toolStrip = (ZToolStrip)shipmentGrid.Controls.Find("toolStrip", true)[0];
				AssertNotNull("Tool Strip should exist.", toolStrip);

				var newButton = (ZToolStripButton)toolStrip.Items.Find(ZModuleButtonGrid.Buttons.New, true)[0];
				AssertNotNull("Edit Button should exist.", newButton);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				AssertNoExceptionThrown(newButton.PerformClick);

				using (var lastShownForm = shipmentGrid.LastShownZForm)
				{
					AssertNotNull(lastShownForm);
					Assert("Last shown form should be ShipmentForm.", lastShownForm is ShipmentForm);

					if (lastShownForm is ShipmentForm shipmentForm)
					{
						Assert("Shipment form should have forwarding shipment as business entity.", shipmentForm.BusinessEntity is ForwardingShipment);

						if (shipmentForm.BusinessEntity is ForwardingShipment forwardingShipment)
						{
							AssertEquals(2, forwardingShipment.Gateways.Count);
							AssertEquals(sendingForwarder.MainAddress.PK, forwardingShipment.Gateways[0].JSG_OA_ForwarderAddress);
							AssertEquals(receivingForwarder.MainAddress.PK, forwardingShipment.Gateways[1].JSG_OA_ForwarderAddress);
						}
					}
				}
			}
		}

		public void TestAMSMenuItemsIsBetweenActionAndJobInvoicing()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			using (var form = new ConsolForm(consol))
			{
				form.Show();
				int actionPosition = -1;
				int amsPosition = -1;
				int jobInvoicingPosition = -1;
				int count = 0;
				foreach (MenuItem item in form.Menu.MenuItems)
				{
					count++;
					switch (item.Text)
					{
						case "Actio&ns":
							if (actionPosition == -1)
							{
								actionPosition = count;
							}

							break;
						case "A&MS":
							if (amsPosition == -1)
							{
								amsPosition = count;
							}

							break;
						case "&Job Invoicing":
							if (jobInvoicingPosition == -1)
							{
								jobInvoicingPosition = count;
							}

							break;
					}
				}
				AssertEquals(string.Format("AMS Menu ({0}) should be before Action Menu ({1})", amsPosition, actionPosition), true, actionPosition < amsPosition);
				AssertEquals(string.Format("AMS Menu ({0}) should be after JobInvoicing Menu ({1})", amsPosition, jobInvoicingPosition), true, amsPosition < jobInvoicingPosition);
			}
		}

		public void TestAMSPlugInsAreBeforeAccountingTab()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "NZAKL";

			// CAMIR Consol changing from non-AMS to AMS
			int amsTabPageIndexFromNonAMS = -1;
			consol.JK_RL_NKDischargePort = ZString.Empty;
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				AssertContainsPlugIn(form, ControllerIDs.Customs.US.AMS);
				AssertContainsPlugIn(form, ControllerIDs.JobInvoicingConsol, Env.Security.MaintainConsolJobInvoicing);
				var mainPanel = (ZPanel)form.Controls["MainPanel"];
				var mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				AssertNull("AMSTabPage should not be in the TabPages", mainTabControl.GetTabPage("AMSDirectTabPage"));

				consol.JK_RL_NKDischargePort = "USLAX";
				AssertContainsPlugIn(form, ControllerIDs.JobInvoicingConsol, Env.Security.MaintainConsolJobInvoicing);
				AssertContainsPlugIn(form, ControllerIDs.Customs.US.AMS);
				var accountingTabPage = mainTabControl.GetTabPage("AccountingTabPage");
				var amsTabPage = mainTabControl.GetTabPage("AMSDirectTabPage");
				var accountingTabPageIndex = mainTabControl.TabPages.IndexOf(accountingTabPage);
				var amsTabPageIndex = mainTabControl.TabPages.IndexOf(amsTabPage);
				amsTabPageIndexFromNonAMS = amsTabPageIndex;
				AssertEquals(string.Format("AMS Tab Page ({0}) should be before Accounting TabPage ({1})", amsTabPageIndex, accountingTabPageIndex), true, accountingTabPageIndex > amsTabPageIndex);

				consol.JK_RL_NKDischargePort = "CATOR";
				AssertContainsPlugIn(form, ControllerIDs.Customs.US.AMS);
				AssertNull("AMSTabPage should not be in the TabPages", mainTabControl.GetTabPage("AMSDirectTabPage"));

				consol.JK_RL_NKDischargePort = "USLAX";
				AssertContainsPlugIn(form, ControllerIDs.JobInvoicingConsol, Env.Security.MaintainConsolJobInvoicing);
				AssertContainsPlugIn(form, ControllerIDs.Customs.US.AMS);
				accountingTabPageIndex = mainTabControl.TabPages.IndexOf(accountingTabPage);
				var amsTabPageIndex2 = mainTabControl.TabPages.IndexOf(amsTabPage);
				AssertEquals("AMS Tap Page Index should be the same", amsTabPageIndex, amsTabPageIndex2);
				AssertEquals(string.Format("AMS Tab Page ({0}) should be before Accounting TabPage ({1})", amsTabPageIndex, accountingTabPageIndex), true, accountingTabPageIndex > amsTabPageIndex);
			}

			// CAMIR Consol changing from AMS to non-AMS
			consol.JK_RL_NKDischargePort = "USLAX";
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				AssertContainsPlugIn(form, ControllerIDs.JobInvoicingConsol, Env.Security.MaintainConsolJobInvoicing);
				AssertContainsPlugIn(form, ControllerIDs.Customs.US.AMS);
				var mainPanel = (ZPanel)form.Controls["MainPanel"];
				var mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				var accountingTabPage = mainTabControl.GetTabPage("AccountingTabPage");
				var amsTabPage = mainTabControl.GetTabPage("AMSDirectTabPage");
				var accountingTabPageIndex = mainTabControl.TabPages.IndexOf(accountingTabPage);
				var amsTabPageIndex = mainTabControl.TabPages.IndexOf(amsTabPage);
				AssertEquals(string.Format("AMS Tab Page ({0}) should be before Accounting TabPage ({1})", amsTabPageIndex, accountingTabPageIndex), true, accountingTabPageIndex > amsTabPageIndex);
				AssertEquals(string.Format("AMS Tab Page position ({0}) should be same for loading an AMS Consol to changing to AMS Consol ({1})", amsTabPageIndex, amsTabPageIndexFromNonAMS), amsTabPageIndexFromNonAMS, amsTabPageIndex);

				consol.JK_RL_NKDischargePort = "CATOR";
				AssertContainsPlugIn(form, ControllerIDs.Customs.US.AMS);
				AssertNull("AMSTabPage should not be in the TabPages", mainTabControl.GetTabPage("AMSDirectTabPage"));

				consol.JK_RL_NKDischargePort = "USLAX";
				AssertContainsPlugIn(form, ControllerIDs.JobInvoicingConsol, Env.Security.MaintainConsolJobInvoicing);
				AssertContainsPlugIn(form, ControllerIDs.Customs.US.AMS);
				accountingTabPageIndex = mainTabControl.TabPages.IndexOf(accountingTabPage);
				var amsTabPageIndex2 = mainTabControl.TabPages.IndexOf(amsTabPage);
				AssertEquals("AMS Tap Page Index should be the same", amsTabPageIndex, amsTabPageIndex2);
				AssertEquals(string.Format("AMS Tab Page ({0}) should be before Accounting TabPage ({1})", amsTabPageIndex, accountingTabPageIndex), true, accountingTabPageIndex > amsTabPageIndex);
			}
		}

		public void TestACIPlugInsAreBeforeAccountingTab()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "CC", Constants.CountryCodes.Canada);
				Factory.Save();

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				var transport = consol.Transports.MostInterestingTransport;
				transport.JW_RL_NKDiscPort = "CATOR";

				AssertPluginTabIsBeforeAccountingTab("", consol, ControllerIDs.Customs.CA.CAConsolACI, "ACITabPage", null);
			}
		}

		public void TestCcsukPlugInIsBeforeAccountingTab()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "GBLHR";
				consol.JK_MasterBillNum = "11122222222";

				AssertPluginTabIsBeforeAccountingTab("", consol, ControllerIDs.Customs.GB.CcsukAirInventory, "CCS-UKTabPage", null);
			}
		}

		public void TestNZCustomsOutwardReportPlugInIsBeforeAccountingTab()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "NZAKL";
				consol.JK_RL_NKDischargePort = "AUSYD";
				consol.JK_MasterBillNum = "11122222222";

				AssertPluginTabIsBeforeAccountingTab("", consol, ControllerIDs.Customs.NZ.OutwardReport, "OutwardReportTabPage", null);
			}
		}

		public void TestAUCustomsAirCargoPlugInIsBeforeAccountingTab()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;

				AssertPluginTabIsBeforeAccountingTab("New Consol:", consol, ControllerIDs.Customs.AU.AirCargo, "AirCargoTabPage", () =>
				{
					consol.JK_RL_NKLoadPort = "NZAKL";
					consol.JK_RL_NKDischargePort = "AUSYD";
				});

				AssertPluginTabIsBeforeAccountingTab("Existing Consol:", consol, ControllerIDs.Customs.AU.AirCargo, "AirCargoTabPage", null);
			}
		}

		public void TestAUCustomsSeaCargoPlugInIsBeforeAccountingTab()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

				AssertPluginTabIsBeforeAccountingTab("New Consol:", consol, ControllerIDs.Customs.AU.SeaCargo, "SeaCargoTabPage", () =>
				{
					consol.JK_RL_NKLoadPort = "NZAKL";
					consol.JK_RL_NKDischargePort = "AUSYD";
				});

				AssertPluginTabIsBeforeAccountingTab("Existing Consol:", consol, ControllerIDs.Customs.AU.SeaCargo, "SeaCargoTabPage", null);
			}
		}

		void AssertPluginTabIsBeforeAccountingTab(ZString message, ForwardingConsol consol, ControllerID pluginId, string tabPageName, Action consolConfigFunc)
		{
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				var mainPanel = (ZPanel)form.Controls["MainPanel"];
				var mainTabControl = (ZTemplateTabControl)mainPanel.Controls["MainTabControl"];
				AssertContainsPlugIn(form, ControllerIDs.JobInvoicingConsol, Env.Security.MaintainConsolJobInvoicing);
				AssertContainsPlugIn(form, pluginId);
				var pluginTabPage = mainTabControl.GetTabPage(tabPageName);

				if (consolConfigFunc != null)
				{
					AssertNull($"{message} {tabPageName} is not displayed without required config", pluginTabPage);
					consolConfigFunc();
					pluginTabPage = mainTabControl.GetTabPage(tabPageName);
				}
				AssertNotNull($"{message} {tabPageName} is displayed when Consol is appropriately configured", pluginTabPage);

				var pluginTabPageIndex = mainTabControl.TabPages.IndexOf(pluginTabPage);
				var accountingTabPage = mainTabControl.GetTabPage("AccountingTabPage");
				var accountingTabPageIndex = mainTabControl.TabPages.IndexOf(accountingTabPage);
				Assert($"{message} {tabPageName} ({pluginTabPageIndex}) should be before accountingTabPage ({accountingTabPageIndex})", accountingTabPageIndex > pluginTabPageIndex);
			}
		}

		public void TestCAConsolACIPlugin()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				CountrySpecificForwardingShipmentSupportTest.DeleteAnyCarrierCode(Factory);
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				var transport = consol.Transports.MostInterestingTransport;
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					AssertDoesNotContainPlugin(form, ControllerIDs.Customs.CA.CAConsolACI);
				}

				try
				{
					GlbCompany.CurrentCompany.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "CC", Constants.CountryCodes.Canada);
					Factory.Save();
					Factory.ClearCachedValue<ZString>("CA.Business.CandianCarrierCode");
					using (var form = new ConsolForm(consol))
					{
						form.Show();
						var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsolACI);
						AssertEquals("PlugIn should exists, but should be disabled, because not all conditions followed", false, plugIn.Enabled);
					}

					transport.JW_RL_NKDiscPort = "CATOR";
					using (var form = new ConsolForm(consol))
					{
						form.Show();
						var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.CAConsolACI);
						AssertEquals("PlugIn should exists and should be enabled", true, plugIn.Enabled);
					}
				}
				finally
				{
					CountrySpecificForwardingShipmentSupportTest.DeleteAnyCarrierCode(Factory);
				}
			}
		}

		public void TestAirConsolInBondPlugin()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					AssertDoesNotContainPlugin(form, ControllerIDs.Customs.US.InBond);
				}

				using (ObjectFactory.Get<Enterprise.Integration.Customs.US.IUSCustomsDataRegistry>().TransportModeForInBondCreationFromConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "AIR"))
				{
					using (var form = new ConsolForm(consol))
					{
						form.Show();
						AssertDoesNotContainPlugin(form, ControllerIDs.Customs.US.InBond);
					}

					consol.JK_TransportMode = Constants.TransportModes.Air;
					using (var form = new ConsolForm(consol))
					{
						form.Show();
						var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Customs.US.InBond);
						AssertEquals("PlugIn should exists and should be enabled", true, plugIn.Enabled);
					}
				}
			}
		}

		public void TestBolPrintingNoPermission()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = false;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "TestRef";
			using (new ConsolForm(consol))
			{
				IForwardingConsolDocumentSupporterQueryProvider queryProvider = GetQueryProvider(consol);
				AssertEquals(false, queryProvider.ConfirmBOLPrinting(shipment));
				AssertEquals("should show correct message", string.Format(WhatAreYouDoingYouPermissionlessFool, shipment.JS_UniqueConsignRef), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBolPrintingWithPermission_AnswersNo()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = true;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "TestRef";
			using (new ConsolForm(consol))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				IForwardingConsolDocumentSupporterQueryProvider queryProvider = GetQueryProvider(consol);
				AssertEquals(false, queryProvider.ConfirmBOLPrinting(shipment));
				AssertEquals("should show correct message", string.Format(YouThinkYouHavePermissionForAnythingMrBigShot, shipment.JS_UniqueConsignRef), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBolPrintingWithPermission_AnswersYes()
		{
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = true;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "TestRef";
			using (new ConsolForm(consol))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				IForwardingConsolDocumentSupporterQueryProvider queryProvider = GetQueryProvider(consol);
				AssertEquals(true, queryProvider.ConfirmBOLPrinting(shipment));
				AssertEquals("should show correct message", string.Format(YouThinkYouHavePermissionForAnythingMrBigShot, shipment.JS_UniqueConsignRef), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions] // test for DisposableLeakListener
		public void TestPrintFinalMasterDoesNotLeak()
		{
			var consol = CreateMAWBWithConsol();
			using (var form = new ConsolForm(consol))
			{
				IForwardingConsolDocumentSupporterQueryProvider queryProvider = GetQueryProvider(consol);
				queryProvider.PrintFinalMaster(consol, string.Empty);
			}
		}

		public void TestActionsMenuItemsNotAvailableInViewMode()
		{
			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(new ConsolForm(Factory.New<ForwardingConsol>()));
		}

		public void TestConsolSavedAndNoControllingPartiesAuthorizeRequestIfUserHasPermission()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, consol.IsInDatabase);
		}

		public void TestShipmentShouldNotHaveEmptyBranchValidationError()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_IsConsignee = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_ActualWeight = 20m;
			shipment.JS_ActualVolume = 1m;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment);

			var rule = new JobBranchDefaultOrderRule();
			rule.DefaultToBlank = 1;
			rule.DefaultToBranchRelatedToPortOrWarehouseBranch = 0;
			rule.DefaultToBranchOfOrganisation = 0;
			rule.DefaultToLoginUserDefault = 0;

			using (AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rule))
			using (var consolForm = new ConsolForm(consol))
			{
				var shipmentGrid = (ConsolShipmentModuleButtonGrid)consolForm.Controls.Find("ShipmentModuleButtonGrid", true)[0];
				var jobHeaderColumns = shipmentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(col => col.ColumnName.Contains("ShipmentJobHeader")).ToList();
				jobHeaderColumns.ForEach(x => x.IsVisible = false);
				consolForm.Show();
				var plugin = consolForm.PlugIns.GetPlugIn(ControllerIDs.Apportionment);
				plugin.SelectTabPage();
				consolForm.FireValidateAllForTest();
				var emptyBranchErrors = shipment.NotificationsIncludingChildren.Where(x => x.Type.Severity == CargoWise.EntityFramework.NotificationType.Error.Severity && x.Message.Contains("Please enter a Branch", StringComparison.CurrentCultureIgnoreCase)).ToList();
				AssertEquals("Shipment should not have empty branch error, as GlbBranch.CurrentBranch will be set as the branch in LoadChildShipmentsAndAcquireMutexesWhereRequired()", 0, emptyBranchErrors.Count);
			}
		}

		public void TestScreeningLogsTabPage()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (var form = new ConsolFormTestClass(consol))
			{
				var tabControl = (ZTabControl)form.LogsTabPageForTest.Controls[0].Controls[0];
				AssertEquals(2, tabControl.TabPages.Count);
				AssertEquals("Denied Party Screening Logs", tabControl.TabPages[1].Text);
				AssertEquals(typeof(RelatedDeniedPartyScreeningStatusControl), tabControl.TabPages[1].Controls[0].GetType());
			}
		}

		public void TestAddComplianceLogsTabPage_WhenComplianceWiseIsEnabled()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new ConsolFormTestClass(consol))
			{
				var tabControl = (ZTabControl)form.LogsTabPageForTest.Controls[0].Controls[0];
				AssertEquals(2, tabControl.TabPages.Count);
				AssertEquals("Compliance Logs", tabControl.TabPages[1].Text);
				AssertEquals(typeof(ComplianceLogUserControl), tabControl.TabPages[1].Controls[0].GetType());

				var tabControl2 = (ZTabControl)tabControl.TabPages[1].Controls[0].Controls[0];
				AssertEquals("Compliance Risk Status Changes", tabControl2.TabPages[0].Text);
				AssertEquals("Denied Party Screening Logs", tabControl2.TabPages[1].Text); // Removed Commodities is hidden because this is consolidation
			}
		}

		public void TestAddComplianceLogsTabPage_WhenComplianceWiseIsEnabled_AndDpsLogTabIsAdded()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new ConsolFormTestClass(consol))
			{
				form.AddScreeningLogsTabPage();
				var tabControl = (ZTabControl)form.LogsTabPageForTest.Controls[0].Controls[0];
				AssertEquals(3, tabControl.TabPages.Count);
				AssertEquals("Compliance Logs", tabControl.TabPages[1].Text);
				AssertEquals(typeof(ComplianceLogUserControl), tabControl.TabPages[1].Controls[0].GetType());

				var tabControl2 = (ZTabControl)tabControl.TabPages[1].Controls[0].Controls[0];
				AssertEquals(2, tabControl2.TabPages.Count);
				AssertEquals("Compliance Risk Status Changes", tabControl2.TabPages[0].Text);
				AssertEquals("Denied Party Screening Logs", tabControl2.TabPages[1].Text);
			}
		}

		public void TestAddComplianceLogsTabPage_WhenComplianceWiseIsDisabled()
		{
			ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (var form = new ConsolFormTestClass(consol))
			{
				var tabControl = (ZTabControl)form.LogsTabPageForTest.Controls[0].Controls[0];
				AssertEquals(2, tabControl.TabPages.Count);
				AssertEquals("Change Logs", tabControl.TabPages[0].Text);
				AssertEquals("Denied Party Screening Logs", tabControl.TabPages[1].Text);
				AssertEquals(typeof(RelatedDeniedPartyScreeningStatusControl), tabControl.TabPages[1].Controls[0].GetType());
			}
		}

		public void TestShipmentJobHeaderIsCreatedOnSave()
		{
			var savedShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Factory.Save();

			AssertNull("Precondition: savedShipment doesn't have job", savedShipment.JobHeader);

			var consol = Factory.New<ForwardingConsol>();

			using (var testForm = new ConsolFormTestClass(consol))
			{
				var unsavedShipment1 = consol.Shipments.AddNew();
				var unsavedShipment2 = consol.Shipments.AddNew();
				consol.Shipments.Add(savedShipment);

				testForm.Show();

				unsavedShipment1.JS_RL_NKOrigin = "AUBNE";
				unsavedShipment1.JS_RL_NKDestination = "MYKUL";

				unsavedShipment2.JS_RL_NKOrigin = "AUBNE";
				unsavedShipment2.JS_RL_NKDestination = "MYKUL";

				AssertNull("Unsaved Shipment 1: no job yet", unsavedShipment1.JobHeader);
				AssertNull("Unsaved Shipment 2: no job yet", unsavedShipment2.JobHeader);
				AssertNull("Saved Shipment: no job yet", savedShipment.JobHeader);

				testForm.ValidateAndSave();

				AssertNotNull("Unsaved Shipment 1: job created", unsavedShipment1.JobHeader);
				AssertNotNull("Unsaved Shipment 2: job created", unsavedShipment2.JobHeader);
				AssertNull("Saved Shipment: no job is created for previously saved shipment", savedShipment.JobHeader);
			}
		}

		public void TestAttachHVLVOriginLoadListMenuItem_WithDuplicateWaybill_ErrorMessageBox()
		{
			TestCase_TestAttachHVLVOriginLoadListMenuItem_WithDuplicateWaybill_ErrorMessageBox(registryValue: true);
		}

		public void TestAttachHVLVOriginLoadListMenuItem_WithDuplicateWaybill_ErrorMessageBox_Legacy()
		{
			TestCase_TestAttachHVLVOriginLoadListMenuItem_WithDuplicateWaybill_ErrorMessageBox(registryValue: false);
		}

		void TestCase_TestAttachHVLVOriginLoadListMenuItem_WithDuplicateWaybill_ErrorMessageBox(bool registryValue)
		{
			using (HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var masterBillNumber1 = "MBN20181227";

				var loadList = Factory.New<IHVLVOriginLoadList>();
				((BusinessObject)loadList).FillWithValidTestData();
				loadList.HVL_Status = "LDG";
				loadList.HVL_MasterBillNumber = masterBillNumber1;

				var bookingHeader1 = Factory.New<IHVLVBookingHeader>();
				var bookingHeader2 = Factory.New<IHVLVBookingHeader>();
				bookingHeader1.HVH_BookingReference = "TestHeader1";
				bookingHeader2.HVH_BookingReference = "TestHeader2";
				var billToParty = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
				bookingHeader1.HVH_OA_BillToParty = billToParty;
				bookingHeader2.HVH_OA_BillToParty = billToParty;
				bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";
				bookingHeader2.HVH_RS_NKBookingServiceLevel = "EXP";

				CreateConsignmentWithItem(bookingHeader1, "Waybill1", loadList.PK);
				CreateConsignmentWithItem(bookingHeader2, "Waybill1", loadList.PK);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var consol = newFactory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_MasterBillNum = masterBillNumber1;
				newFactory.Save();

				using (var form = new ConsolFormTestClass(consol))
				{
					var menuText = "Attach HVLV Origin Load List";
					var menuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText(menuText);

					AssertNotNull(menuItem);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
					{
						var popupModule = obj as EmbeddedModulePopup;
						var decisionProvider = popupModule.Module_ForTest.ModuleDecisionProvider;
						var loadListInNewFactory = new BusinessObjectFactory().ImportFromAnotherFactory((BusinessObject)loadList);
						decisionProvider.HandleDefaultAction(new[] { loadListInNewFactory });
					});
					menuItem.PerformClick();

					var errorMessageText = UnitTestUserNotification.Instance.LastMessage.Text;
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

					var savedConsol = Factory.Load<ForwardingConsol>(consol.PK);
					var expectedErrorMessage = $"Error: Error processing the HVLV Origin Load List '{loadList.HVL_UniqueReference}': Consignment Waybill number 'Waybill1' is repeated on booking headers: TestHeader1, TestHeader2";
					AssertContains("Error message contents", expectedErrorMessage, errorMessageText);
					AssertEquals("No shipments should be added from loadlist", 0, savedConsol.ShipmentCount);
				}
			}
		}

		public void TestAttachHVLVOriginLoadListMenuItem_ShipmentCreatedOnConsol()
		{
			TestCase_TestAttachHVLVOriginLoadListMenuItem_ShipmentCreatedOnConsol(registryValue: true);
		}

		public void TestAttachHVLVOriginLoadListMenuItem_ShipmentCreatedOnConsol_Legacy()
		{
			TestCase_TestAttachHVLVOriginLoadListMenuItem_ShipmentCreatedOnConsol(registryValue: false);
		}

		void TestCase_TestAttachHVLVOriginLoadListMenuItem_ShipmentCreatedOnConsol(bool registryValue)
		{
			using (HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var masterBillNumber1 = "MBN20181227";

				var loadList = Factory.New<IHVLVOriginLoadList>();
				((BusinessObject)loadList).FillWithValidTestData();
				loadList.HVL_Status = "LDG";
				loadList.HVL_MasterBillNumber = masterBillNumber1;

				var bookingHeader1 = Factory.New<IHVLVBookingHeader>();
				var bookingHeader2 = Factory.New<IHVLVBookingHeader>();
				bookingHeader1.HVH_BookingReference = "TestHeader1";
				bookingHeader2.HVH_BookingReference = "TestHeader2";
				var billToParty = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
				bookingHeader1.HVH_OA_BillToParty = billToParty;
				bookingHeader2.HVH_OA_BillToParty = billToParty;
				bookingHeader1.HVH_RS_NKBookingServiceLevel = "EXP";
				bookingHeader2.HVH_RS_NKBookingServiceLevel = "EXP";

				CreateConsignmentWithItem(bookingHeader1, "Waybill1", loadList.PK);
				CreateConsignmentWithItem(bookingHeader2, "Waybill2", loadList.PK);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var consol = newFactory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_MasterBillNum = masterBillNumber1;
				newFactory.Save();

				AssertEquals("Precondition: No Shipments on loadlist", 0, consol.ShipmentCount);

				using (var form = new ConsolFormTestClass(consol))
				{
					var menuText = "Attach HVLV Origin Load List";
					var menuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText(menuText);

					AssertNotNull(menuItem);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
					{
						var popup = obj as EmbeddedModulePopup;
						var decisionProvider = popup.Module_ForTest.ModuleDecisionProvider;
						decisionProvider.HandleDefaultAction(new[] { (BusinessObject)loadList });
					});

					menuItem.PerformClick();
					var savedConsol = Factory.Load<ForwardingConsol>(consol.PK);
					AssertEquals("Shipment should be added from loadlist", 1, savedConsol.ShipmentCount);
				}
			}
		}

		public void TestAttachHVLVOriginLoadListMenuItem_NoMatchedDestinationUNLOCO_ShipmentCreatedOnConsolWithWarning()
		{
			TestCase_TestAttachHVLVOriginLoadListMenuItem_NoMatchedDestinationUNLOCO_ShipmentCreatedOnConsolWithWarning(registryValue: true);
		}

		public void TestAttachHVLVOriginLoadListMenuItem_NoMatchedDestinationUNLOCO_ShipmentCreatedOnConsolWithWarning_Legacy()
		{
			TestCase_TestAttachHVLVOriginLoadListMenuItem_NoMatchedDestinationUNLOCO_ShipmentCreatedOnConsolWithWarning(registryValue: false);
		}

		void TestCase_TestAttachHVLVOriginLoadListMenuItem_NoMatchedDestinationUNLOCO_ShipmentCreatedOnConsolWithWarning(bool registryValue)
		{
			using (HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var masterBillNum = "MasterBillNum";
				var loadList = Factory.New<IHVLVOriginLoadList>();
				((BusinessObject)loadList).FillWithValidTestData();
				loadList.HVL_Status = "LDG";
				loadList.HVL_IsMasterHouse = true;
				var loadListDestinationDepot = Factory.NewWithValidTestData<OrgAddress>();
				loadList.HVL_OA_DestinationDepot = loadListDestinationDepot.PK;
				loadList.HVL_MasterBillNumber = masterBillNum;

				var bookingHeader = Factory.New<IHVLVBookingHeader>();
				bookingHeader.HVH_BookingReference = "TestHeader1";
				var billToParty = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
				bookingHeader.HVH_OA_BillToParty = billToParty;

				var consignment = CreateConsignmentWithItem(bookingHeader, "Waybill1", loadList.PK);
				consignment.HVC_RN_NKConsigneeCountryCode = "US";

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var consol = newFactory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_MasterBillNum = masterBillNum;
				newFactory.Save();

				AssertEquals("Precondition: No Shipments on consol", 0, consol.ShipmentCount);

				using (var form = new ConsolFormTestClass(consol))
				{
					var menuText = "Attach HVLV Origin Load List";
					var menuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText(menuText);

					AssertNotNull(menuItem);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
					{
						var popup = obj as EmbeddedModulePopup;
						var decisionProvider = popup.Module_ForTest.ModuleDecisionProvider;
						decisionProvider.HandleDefaultAction(new[] { (BusinessObject)loadList });
					});

					menuItem.PerformClick();

					var warningMessageText = UnitTestUserNotification.Instance.LastMessage.Text;
					var warningCaption = UnitTestUserNotification.Instance.LastMessage.Caption;

					CombineAssertions(() =>
					{
						AssertEquals("master shipment and hvl shipment both created", 2, consol.Shipments.Count);
						AssertEquals("Warning message caption", "Action completed with warning", warningCaption);
						AssertContains("Warning message contents",
							"Cannot determine suitable Destination Port for HVL Shipment(s) on Consol 'MasterBillNum'. There are zero or multiple UNLOCOs for country(s) 'US'.",
							warningMessageText);
					});
				}
			}
		}

		public void TestAttachHVLVOriginLoadListMenuItem_NoMatchingMasterBill_PromptsWithWarning()
		{
			TestCase_TestAttachHVLVOriginLoadListMenuItem_NoMatchingMasterBill_PromptsWithWarning(registryValue: true);
		}

		public void TestAttachHVLVOriginLoadListMenuItem_NoMatchingMasterBill_PromptsWithWarning_Legacy()
		{
			TestCase_TestAttachHVLVOriginLoadListMenuItem_NoMatchingMasterBill_PromptsWithWarning(registryValue: false);
		}

		void TestCase_TestAttachHVLVOriginLoadListMenuItem_NoMatchingMasterBill_PromptsWithWarning(bool registryValue)
		{
			using (HVLVDataRegistry.Instance.ProcessLoadListUsingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			{
				var loadList = Factory.New<IHVLVOriginLoadList>();
				((BusinessObject)loadList).FillWithValidTestData();
				loadList.HVL_Status = "LDG";
				loadList.HVL_IsMasterHouse = true;
				var loadListDestinationDepot = Factory.NewWithValidTestData<OrgAddress>();
				loadList.HVL_OA_DestinationDepot = loadListDestinationDepot.PK;

				var bookingHeader = Factory.New<IHVLVBookingHeader>();
				bookingHeader.HVH_BookingReference = "TestHeader1";
				var billToParty = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
				bookingHeader.HVH_OA_BillToParty = billToParty;

				var consignment = CreateConsignmentWithItem(bookingHeader, "Waybill1", loadList.PK);
				consignment.HVC_RN_NKConsigneeCountryCode = "US";

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var consol = newFactory.NewWithValidTestData<ForwardingConsol>();
				consol.JK_MasterBillNum = "MasterBillNum";
				newFactory.Save();

				AssertEquals("Precondition: No Shipments on consol", 0, consol.ShipmentCount);

				using (var form = new ConsolFormTestClass(consol))
				{
					var menuText = "Attach HVLV Origin Load List";
					var menuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText(menuText);

					AssertNotNull(menuItem);

					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
					{
						var popup = obj as EmbeddedModulePopup;
						var decisionProvider = popup.Module_ForTest.ModuleDecisionProvider;
						decisionProvider.HandleDefaultAction(new[] { (BusinessObject)loadList });
					});

					CombineAssertions(() =>
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
						menuItem.PerformClick();
						var result = form.ShowPreSaveDialogs();
						AssertEquals("Prompt message caption", "Attach Load List Confirmation", UnitTestUserNotification.Instance.LastMessage.Caption);
						AssertContains("Prompts Attach Load List Confirmation",
							"Selected HVLV Origin Load List(s) do not have matching Master Bill with this Consol, do you wish to proceed?",
							UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("Shipments are not created with dialog NO", 0, consol.Shipments.Count);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						menuItem.PerformClick();
						result = form.ShowPreSaveDialogs();
						AssertEquals("Shipments are created with dialog YES", 2, consol.Shipments.Count);
					});
				}
			}
		}

		IHVLVConsignment CreateConsignmentWithItem(IHVLVBookingHeader header, string waybillNumber, ZGuid loadListPK)
		{
			var consignment = (IHVLVConsignment)header.Consignments.AddNew();
			consignment.HVC_WaybillNumber = waybillNumber;
			var item = (IHVLVItem)consignment.Items.AddNew();
			item.HVI_HVL_LoadList = loadListPK;
			return consignment;
		}

		#region MAWB With Messages

		public void TestShowConsignmentSecurityDeclaration()
		{
			var consol = CreateMAWBWithConsol();

			using (var form = new ConsolFormTestClass(consol))
			{
				form.Show();
				form.MainTabControl.SelectedTab = form.AWBTabPage;
				var mawbControl = form.Controls.Find("MAWBWithMessages", true).First() as MAWBWithMessagesUserControl;
				var securityDeclarationTabPage = mawbControl.Controls.Find("SecurityDeclarationTabPage", true).First() as ZTabPage;
				AssertEquals("Tab is visible for AU", true, securityDeclarationTabPage.TabVisible);
			}
		}

		public void TestShowConsignmentSecurityDeclaration_US()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				var consol = CreateMAWBWithConsol();
				consol.JK_RL_NKLoadPort = "USLAX";

				using (var form = new ConsolFormTestClass(consol))
				{
					form.Show();
					form.MainTabControl.SelectedTab = form.AWBTabPage;
					var mawbControl = form.Controls.Find("MAWBWithMessages", true).First() as MAWBWithMessagesUserControl;
					AssertEquals("Tab is not loaded for US", 0, mawbControl.Controls.Find("SecurityDeclarationTabPage", true).Length);
				}
			}
		}

		#endregion

		#region CO2 Emission

		public void TestCO2ePlugin()
		{
			var consol = Factory.New<ForwardingConsol>();
			ChildEditableService.SetState(consol.Factory, ChildEditableServiceStates.Consol);

			void AssertMenuItem(string pluginMessage, string menuItemMessage, bool shouldBeAvailable)
			{
				using (var form = new ConsolForm(consol))
				{
					form.Show();
					var co2ePlugin = form.PlugIns.GetPlugIn(ControllerIDs.CO2ePlugin);
					var menuItem = FindMenuItem(form, "Actions").MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)");
					AssertEquals(pluginMessage, shouldBeAvailable, co2ePlugin != null);
					AssertEquals(menuItemMessage, shouldBeAvailable, menuItem != null);
				}
			}

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(false))
			{
				AssertMenuItem("CO2e Plugin is not added when registry is disabled", "Calculate Greenhouse Gas Emissions (CO2e) action menu item should not be visible when registry is disabled", false);
			}

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			{
				AssertMenuItem("CO2e Plugin is added when registry is enabled", "Calculate Greenhouse Gas Emissions (CO2e) action menu item should be visible when registry is enabled", true);
			}
		}

		public void TestNoCO2ePluginInConsolTemplate()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.IsTemplateRecord = true;
			ChildEditableService.SetState(consol.Factory, ChildEditableServiceStates.Consol);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				var co2ePlugin = form.PlugIns.GetPlugIn(ControllerIDs.CO2ePlugin);
				AssertNull(co2ePlugin);
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_EHub()
		{
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory);

			ChildEditableService.SetState(consol.Factory, ChildEditableServiceStates.Consol);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();
				FindMenuItem(form, "Actions").MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request sent", lastMessage.Caption);
					AssertEquals("Check message text", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
				});
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_Api()
		{
			// Arrange
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory);

			ChildEditableService.SetState(consol.Factory, ChildEditableServiceStates.Consol);

			var client = new Mock<IApiClient>();
			client.Setup(x => x.PostAsync<EmissionResult>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(() => CO2eTestHelper.GenerateEmissionResponse(CO2eTestHelper.CO2eUniversalShipment_Response));

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Api))
			using (var form = new ConsolFormForTest(consol))
			using (ObjectFactory.Substitute("HttpClient", client.Object))
			{
				form.Show();
				Factory.Save();

				// Act
				FindMenuItem(form, "Actions").MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				// Assert
				AssertEquals("Shipment CO2eStatus should be set", CO2eStatusList.Codes.Current, consol.GetCO2eStatus());
				AssertEquals("Transport CO2eStatus should be set", CO2eStatusList.Codes.Current, consol.Transports[0].GetCO2eStatus());
				AssertEquals("Transport CO2eStatus should be set", CO2eStatusList.Codes.Current, consol.Transports[1].GetCO2eStatus());
				AssertEquals("TotalCO2eForBinding should be set", "10,000", consol.TotalCO2eForBinding);
				AssertEquals("TotalCO2eForSorting should be set", 10000m, consol.TotalCO2eForSorting);

				form.SwitchTabPage("RoutingTabPage");
				var grid = form.GetControl<ZGrid>("TransportsGrid");
				var transport1 = grid.List[0] as Transport;
				var transport2 = grid.List[1] as Transport;
				AssertEquals("CO2ePerTonneInKgForBinding should be set", 8000m, transport1.TotalCO2eForSorting);
				AssertEquals("CO2ePerTonneInKgForBinding should be set", 2000m, transport2.TotalCO2eForSorting);
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ActionMenuItemClick_WithShipmentWeightZeroAndPreAllocationWeight()
		{
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory);
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ActualWeight = 0m;
			shipment.JS_ActualVolume = 1m;
			consol.Shipments.Add(shipment);

			ChildEditableService.SetState(consol.Factory, ChildEditableServiceStates.Consol);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();
				FindMenuItem(form, "Actions").MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request sent", lastMessage.Caption);
					AssertEquals("Check message text", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
				});
			}
		}

		public void TestCalculateCO2EmissionMenuItem_SentSuccessfullyWhenNoWeightAndRequireTEU() => SentSuccessfullyWhenNoWeightUsingTEU(TransportModes.Sea);

		public void TestCalculateCO2EmissionMenuItem_SentSuccessfullyWhenNoWeightAndIncludeTEU() => SentSuccessfullyWhenNoWeightUsingTEU(TransportModes.Road);

		void SentSuccessfullyWhenNoWeightUsingTEU(string transportMode)
		{
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolRequiringTEU(Factory);
			consol.JK_TransportMode = transportMode;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_TotalShipmentActWeightCheck = 0m;
			consol.Shipments.RemoveAll();

			ChildEditableService.SetState(consol.Factory, ChildEditableServiceStates.Consol);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (FreightDataRegistry.Instance.CO2eUserRequestProcessingMethod.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, CO2eUserRequestProcessingMethodCodeList.Codes.Ehub))
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				Factory.Save();
				FindMenuItem(form, "Actions").MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request sent", lastMessage.Caption);
					AssertEquals("Check message text", "The greenhouse gas emissions calculation has been requested.", lastMessage.Text);
				});
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ShowErrorMessage_WhenMandatoryDataMissing()
		{
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory);

			ChildEditableService.SetState(consol.Factory, ChildEditableServiceStates.Consol);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				var menuItem = FindMenuItem(form, "Actions").MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)");
				void TestMissingField(Action change, string text)
				{
					change.Invoke();
					Factory.Save();
					menuItem.PerformClick();
					var lastMessage = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals("Check message caption", "Request failed", lastMessage.Caption);
						AssertContains("Check message text", "The greenhouse gas emissions calculation cannot be requested because following mandatory input is missing or invalid:", lastMessage.Text);
						AssertContains("Check mandatory field", text, lastMessage.Text);
					});
				}

				TestMissingField(() => consol.JK_TransportMode = ZString.Empty, "Consol > Details > Transport Mode");
				TestMissingField(() => consol.JK_RL_NKLoadPort = ZString.Empty, "Consol > Details > 1st Load");
				TestMissingField(() => consol.JK_RL_NKDischargePort = ZString.Empty, "Consol > Details > Last Discharge");
				TestMissingField(() => consol.JK_TotalShipmentActWeightCheck = 0m, "Consol > Total Shipment Weight");
				TestMissingField(() => {
					var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					var expectedAddress = Factory.NewWithValidTestData<OrgAddress>();
					expectedAddress.OA_OH = orgHeader.PK;
					expectedAddress.OA_Code = "blerbity";
					expectedAddress.City = "AUSydney";
					expectedAddress.Postcode = string.Empty;
					expectedAddress.OA_RN_NKCountryCode = string.Empty;

					consol.JK_OA_PackDepotAddress = expectedAddress.PK;
				}, "Consol > Departure > CFS (Ctry/Rgn. field is required with City field)");
				TestMissingField(() => {
					var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					var expectedAddress = Factory.NewWithValidTestData<OrgAddress>();
					expectedAddress.OA_OH = orgHeader.PK;
					expectedAddress.OA_Code = "blerbity";
					expectedAddress.City = string.Empty;
					expectedAddress.Postcode = "2121";
					expectedAddress.OA_RN_NKCountryCode = string.Empty;

					consol.JK_OA_PackDepotAddress = expectedAddress.PK;
				}, "Consol > Departure > CFS (Ctry/Rgn. field is required with City field)");
				TestMissingField(() =>
				{
					var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					var expectedAddress = Factory.NewWithValidTestData<OrgAddress>();
					expectedAddress.OA_OH = orgHeader.PK;
					expectedAddress.OA_Code = "blerbity";
					expectedAddress.City = "Melbourne";
					expectedAddress.Postcode = string.Empty;
					expectedAddress.OA_RN_NKCountryCode = string.Empty;

					consol.JK_OA_UnpackDepotAddress = expectedAddress.PK;
				}, "Consol > Arrival > CFS (Ctry/Rgn. field is required with City field)");
				TestMissingField(() =>
				{
					var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
					var expectedAddress = Factory.NewWithValidTestData<OrgAddress>();
					expectedAddress.OA_OH = orgHeader.PK;
					expectedAddress.OA_Code = "blerbity";
					expectedAddress.City = string.Empty;
					expectedAddress.Postcode = "2121";
					expectedAddress.OA_RN_NKCountryCode = string.Empty;

					consol.JK_OA_UnpackDepotAddress = expectedAddress.PK;
				}, "Consol > Arrival > CFS (Ctry/Rgn. field is required with City field)");
			}
		}

		public void TestCalculateCO2EmissionMenuItem_ShowErrorMessage_WhenShipmentNotSaved()
		{
			var consol = (ForwardingConsol)CO2eTestHelper.CreateForwardingConsolWithLegs(Factory);

			ChildEditableService.SetState(consol.Factory, ChildEditableServiceStates.Consol);

			using (CO2eBusinessTestHelper.MockCO2eFeatureControl(true))
			using (var form = new ConsolForm(consol))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.ClearUserResponses();
				FindMenuItem(form, "Actions").MenuItems.FindByText("Calculate Greenhouse Gas Emissions (CO2e)").PerformClick();

				var lastMessage = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals("Check message caption", "Request failed", lastMessage.Caption);
					AssertEquals("Check message text", "Please save before calculating greenhouse gas emissions.", lastMessage.Text);
				});
			}
		}

		#endregion

		#region SecuredVerificationChecker Tests

		public void TestSecuredFreightVerificationCheckerIsRegisteredForSCSSupportedCountries()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (var testForm = new ConsolFormTestClass(consol))
			{
				AssertNotNull(Factory.GetValue<ISecuredFreightVerificationChecker>());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			using (var testForm = new ConsolFormTestClass(consol))
			{
				AssertNotNull(Factory.GetValue<ISecuredFreightVerificationChecker>());
			}
		}

		public void TestSecuredFreightVerificationCheckedIsNotRegisteredWhenCompanyNotSCSSupported()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Bangladesh))
			using (var testForm = new ConsolFormTestClass(consol))
			{
				AssertNull(Factory.GetValue<ISecuredFreightVerificationChecker>());
			}
		}

		public void TestSecuredFreightVerificationCheckerDisplaysMAWBOverriddenMessageWhenMAWBOverridden()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (var testForm = new ConsolFormTestClass(consol))
			{
				testForm.Show();

				Factory.GetValue<ISecuredFreightVerificationChecker>().FreightIsVerifiedToBeSecure(true);

				var msg = UnitTestUserNotification.Instance.LastMessage;

				AssertNotNull("Should prompt user if FreightIsVerifiedToBeSecure is called", msg);
				AssertEquals("SPX Verification", msg.Caption);
				AssertEquals(@"MAWB has been overridden. Please ensure security status on MAWB matches security status on Consol.
Setting the secured status to 'SPX' requires verification. 
Have you verified that the freight is secure and the accompanying CSC or CSD has been checked and found to be correct?", msg.Text);
			}
		}

		public void TestSecuredFreightVerificationCheckerDisplaysRegularMessageWhenMAWBNotOverridden()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			using (var testForm = new ConsolFormTestClass(consol))
			{
				testForm.Show();

				Factory.GetValue<ISecuredFreightVerificationChecker>().FreightIsVerifiedToBeSecure(false);

				var msg = UnitTestUserNotification.Instance.LastMessage;

				AssertNotNull("Should prompt user if FreightIsVerifiedToBeSecure is called", msg);
				AssertEquals("SPX Verification", msg.Caption);
				AssertEquals(@"Setting the secured status to 'SPX' requires verification.
Have you verified that the freight is secure and the accompanying CSC or CSD has been checked and found to be correct?", msg.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestSecuredFreightVerificationCheckerDialogNotShownOnSave()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var consol = new TestObjectCreator(Factory).CreateConsol(origin: "AUSYD");

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			using (Globals.SetIsUnitTestingProductionFunctionality())
			using (var testForm = new ConsolFormTestClass(consol))
			{
				Factory.Saving += delegate(BusinessObjectFactory factory)
				{
					consol.SecurityStatusCode = "SPX";
				};

				testForm.Show();
				var saveResult = testForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, saveResult);
				AssertNotEquals("SPX cannot be set without verifcation check", "SPX", consol.SecurityStatusCode);
				AssertNull("SPX Verification dialog should not show", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		#endregion

		#region JobProfitLossControl Summary And Details Message Visibility Tests

		public void TestJobProfitLossControlSummaryAndDetailsChargeHidingMessageVisibility_UserCanLoginToTestBranchAndAllowedToViewCostsOutsideLoginBranches()
		{
			AssertJobProfitLossControlSummaryAndDetailsChargeHidingMessageVisibility(true, true, 2, 4, false);
		}

		public void TestJobProfitLossControlSummaryAndDetailsChargeHidingMessageVisibility_UserCanLoginToTestBranchButNotAllowedToViewCostsOutsideLoginBranches()
		{
			AssertJobProfitLossControlSummaryAndDetailsChargeHidingMessageVisibility(true, false, 2, 4, false);
		}

		public void TestJobProfitLossControlSummaryAndDetailsChargeHidingMessageVisibility_UserCanNotLoginToTestBranchButAllowedToViewCostsOutsideLoginBranches()
		{
			AssertJobProfitLossControlSummaryAndDetailsChargeHidingMessageVisibility(false, true, 2, 4, false);
		}

		public void TestJobProfitLossControlSummaryAndDetailsChargeHidingMessageVisibility_UserCanNotLoginToTestBranchAndNotAllowedToViewCostsOutsideLoginBranches()
		{
			AssertJobProfitLossControlSummaryAndDetailsChargeHidingMessageVisibility(false, false, 1, 2, true);
		}

		void AssertJobProfitLossControlSummaryAndDetailsChargeHidingMessageVisibility(bool isAllowedToLoginToTestBranch, bool isAllowedToViewCostOutsideBranch, int expectedNumberOfProfitLossSummary, int expectedNumberOfProfitLossDetails, bool expectedMessageVisibility)
		{
			var objectCreator = new TestObjectCreator(Factory);

			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;

			var testBranch = objectCreator.CreateBranch("TBR", "Test Branch", GlbCompany.CurrentCompany);
			Factory.Save();

			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				#region security settings

				var securityFactory = new BusinessObjectFactory();

				var securityForCurrentBranch = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				var securityForTestBranch = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK);

				GlbSecurity loginSecurityForCurrentBranch = securityFactory.New<GlbSecurity>();
				loginSecurityForCurrentBranch.GU_GB = Env.CurrentBranch.PK;
				loginSecurityForCurrentBranch.GU_GE = Env.CurrentDepartment.PK;
				loginSecurityForCurrentBranch.GU_GS = Env.CurrentUser.PK;
				loginSecurityForCurrentBranch.GU_SecurityRight = securityForCurrentBranch.Login.Code;

				GlbSecurity loginSecurityForTestBranch = securityFactory.New<GlbSecurity>();
				loginSecurityForTestBranch.GU_GB = testBranch.PK;
				loginSecurityForTestBranch.GU_GE = Env.CurrentDepartment.PK;
				loginSecurityForTestBranch.GU_GS = Env.CurrentUser.PK;
				loginSecurityForTestBranch.GU_SecurityRight = securityForTestBranch.Login.Code;

				GlbSecurity invoicingSecurityForViewConsolCost = securityFactory.New<GlbSecurity>();
				invoicingSecurityForViewConsolCost.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurityForViewConsolCost.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurityForViewConsolCost.GU_GS = Env.CurrentUser.PK;
				invoicingSecurityForViewConsolCost.GU_SecurityRight = securityForCurrentBranch.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowViewCosts).Code;

				GlbSecurity invoicingSecurityForProfitLoss = securityFactory.New<GlbSecurity>();
				invoicingSecurityForProfitLoss.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurityForProfitLoss.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurityForProfitLoss.GU_GS = Env.CurrentUser.PK;
				invoicingSecurityForProfitLoss.GU_SecurityRight = securityForCurrentBranch.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsol, SecurityCore.ProfitLoss).Code;

				loginSecurityForCurrentBranch.GU_SecurityItemIsAllowed = true;
				loginSecurityForTestBranch.GU_SecurityItemIsAllowed = isAllowedToLoginToTestBranch;
				invoicingSecurityForViewConsolCost.GU_SecurityItemIsAllowed = true;
				invoicingSecurityForProfitLoss.GU_SecurityItemIsAllowed = true;

				securityForTestBranch.Login.IsAllowed = isAllowedToLoginToTestBranch;
				securityForCurrentBranch.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowViewCosts).IsAllowed = isAllowedToViewCostOutsideBranch;

				securityFactory.Save();

				Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());

				#endregion

				var consol = objectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
				var shipment1 = objectCreator.CreateShipment("S001001", consol);
				var job1 = objectCreator.CreateJob(shipment1, createWithMutex: false);
				job1.JH_GB = testBranch.PK;
				var shipment2 = objectCreator.CreateShipment("S001002", consol);
				var job2 = objectCreator.CreateJob(shipment2, createWithMutex: false);

				var consolCost = objectCreator.CreateConsolCost(consol, objectCreator.CC1, 100m, objectCreator.Creditor1, AllocationMethod.Shipment);

				Factory.Save();

				using (var form = new ConsolForm(consol))
				{
					form.Show();
					var profitLossplugin = form.PlugIns.GetPlugIn(ControllerIDs.JobProfitLossConsol);
					profitLossplugin.SelectTabPage();
					Application.DoEvents();

					var profitLossSummaryGrid = (ZGrid)profitLossplugin.UserControl.Controls.Find("ProfitLossSummaryGrid", true)[0];
					var summaryChargeHidingMessageLabel = (ZLabel)profitLossplugin.UserControl.Controls.Find("SummaryChargeHidingMessageLabel", true)[0];
					AssertEquals(expectedNumberOfProfitLossSummary, profitLossSummaryGrid.ListManager.Count);
					AssertEquals(expectedMessageVisibility, summaryChargeHidingMessageLabel.Visible);

					var tabControl = (ZTemplateTabControl)profitLossplugin.UserControl.Controls.Find("TabControl", true)[0];
					var detailsTabPage = (ZTabPage)profitLossplugin.UserControl.Controls.Find("DetailsTabPage", true)[0];
					tabControl.SelectedTab = detailsTabPage;
					Application.DoEvents();

					var profitLossDetailsGrid = (ZGrid)profitLossplugin.UserControl.Controls.Find("ProfitLossGrid", true)[0];
					var detailsChargeHidingMessageLabel = (ZLabel)profitLossplugin.UserControl.Controls.Find("ChargeHidingMessageLabel", true)[0];
					AssertEquals(expectedNumberOfProfitLossDetails, profitLossDetailsGrid.ListManager.Count);
					AssertEquals(expectedMessageVisibility, detailsChargeHidingMessageLabel.Visible);
				}
			}
		}

		#endregion

		#region Controlling Agent Authorization

		public void TestConsolSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsNotEmpty()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			controllingAgent.OH_Code = "CAG";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = controllingAgent.PK;
			orgAddress.OA_Code = "CAG";

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			shipment1.ControllingAgentDocumentaryAddress.E2_OA_Address = orgAddress.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			shipment2.ControllingAgentDocumentaryAddress.E2_OA_Address = orgAddress.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, consol.IsInDatabase);
		}

		public void TestConsolSavedAndDoesNotShowAuthorizeRequestWhenControllingAgentIsEmptyAndUserHasPermissions()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, consol.IsInDatabase);
		}

		public void TestConsolNotSavedWhenControllingAgentIsEmptyAndAuthorizeRequestCancelled()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentAir.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(false, shipment1.IsInDatabase);
		}

		public void TestConsolSavedWhenControllingAgentIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentAir.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();

				int loginFormShownCount = 0;
				var newUserLogin = "User1";
				var newUserPassword = "pass";

				SecurityTestObject.CreateTestUser(true, Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.Code, "US1", newUserLogin, newUserPassword);
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest(newUserLogin, newUserPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(true, consol.IsInDatabase);
		}

		public void TestConsolNotSavedWhenControllingAgentIsEmptyAndUserEnteredToAthorizationFormDoesNotExists()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = false;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentAir.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest("aaa", "pass");
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
				AssertEquals("User does not exist, password is invalid or password is expired.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(false, consol.IsInDatabase);
		}

		public void TestConsolNotSavedAndControllingAgentShowAuthorizeRequestIfUserHasNoPermission()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUBNE";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "MYKUL";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentAir.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(false, consol.IsInDatabase);
		}

		public void TestConsolSavedAndDoesNotShowAuthorizeRequestPriorToControllingAgentRegistryEffectiveDate()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentAir.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			using (FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateAir.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(3).ToDateTime()))
			{
				consolForm.Show();
				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, consol.IsInDatabase);
		}

		public void TestConsolShowAuthorizeRequestAfterControllingAgentRegistryEffectiveDate()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentAir.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			using (FreightDataRegistry.Instance.MandatoryControllingAgentEffectiveDateAir.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(-3).ToDateTime()))
			{
				consolForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(false, consol.IsInDatabase);
		}

		#endregion

		#region Controlling Customer Authorization

		public void TestConsolSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsNotEmpty()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var controllingCustomer = Factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CAG";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = controllingCustomer.PK;
			orgAddress.OA_Code = "CAG";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_PackingMode = Constants.ContainerModes.Loose;
			shipment.JS_ActualWeight = 20m;
			shipment.JS_ActualVolume = 1m;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			shipment.ControllingCustomerAddress.E2_OA_Address = orgAddress.PK;
			new Job.Loader(shipment).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, consol.IsInDatabase);
		}

		public void TestConsolSavedAndDoesNotShowAuthorizeRequestWhenControllingCustomerIsEmptyAndUserHasPermissions()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer.IsAllowed = false;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerAir.IsAllowed = true;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, consol.IsInDatabase);
		}

		public void TestConsolNotSavedWhenControllingCusotmerIsEmptyAndAuthorizeRequestCancelled()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer.IsAllowed = false;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerAir.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(false, consol.IsInDatabase);
		}

		public void TestConsolSavedWhenControllingCustomerIsEmptyAndCorrectSupervisorCredentialsEnteredToAuthorizationForm()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer.IsAllowed = false;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerAir.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();

				int loginFormShownCount = 0;
				var newUserLogin = "User1";
				var newUserPassword = "pass";

				SecurityTestObject.CreateTestUser(true, Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerAir.Code, "US1", newUserLogin, newUserPassword);
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest(newUserLogin, newUserPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(true, consol.IsInDatabase);
		}

		public void TestConsolNotSavedWhenControllingCustomerIsEmptyAndUserEnteredToAthorizationFormDoesNotExists()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgent.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomer.IsAllowed = false;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerAir.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						loginForm.DoLoginForTest("aaa", "pass");
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					}
				});

				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
				AssertEquals("User does not exist, password is invalid or password is expired.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AssertEquals(false, consol.IsInDatabase);
		}

		public void TestConsolSavedAndNoAuthorizeRequestPriorToControllingCustomerRegistryEffectiveDate()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentAir.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerAir.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			using (FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateAir.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(3).ToDateTime()))
			{
				consolForm.Show();
				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.Yes, result);
			}

			AssertEquals(true, consol.IsInDatabase);
		}

		public void TestConsolShowAuthorizeRequestAfterControllingCustomerRegistryEffectiveDate()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_Code = "CNR";
			consignor.OH_RL_NKClosestPort = "AUSYD";
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNE";
			consignee.OH_RL_NKClosestPort = "DEFRA";
			consignee.OH_IsConsignee = true;

			var shipment1 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 20m;
			shipment1.JS_ActualVolume = 1m;
			shipment1.ConsignorPK = consignor.PK;
			shipment1.ConsigneePK = consignee.PK;
			new Job.Loader(shipment1).TryCreate();

			var shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 12m;
			shipment2.JS_ActualVolume = 2m;
			shipment2.ConsignorPK = consignor.PK;
			shipment2.ConsigneePK = consignee.PK;
			new Job.Loader(shipment2).TryCreate();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = Constants.ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_TransportMode = Constants.TransportModes.Air;
			consol.Transports[0].JW_TransportType = "FL1";
			consol.Transports[0].JW_ETA = DateTime.Today;
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";
			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);

			Env.Security.MaintainShipmentAllowSaveWithoutControllingAgentAir.IsAllowed = true;
			Env.Security.MaintainShipmentAllowSaveWithoutControllingCustomerAir.IsAllowed = false;

			using (var consolForm = new ConsolForm(consol))
			using (FreightDataRegistry.Instance.MandatoryControllingCustomerEffectiveDateAir.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ZDateTime.UtcToday.AddDays(-3).ToDateTime()))
			{
				consolForm.Show();

				int loginFormShownCount = 0;
				EnvProxy.Instance.Registry.ShowSaveProgressBox = false;

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;

					if (loginForm != null)
					{
						loginFormShownCount++;
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
					}
				});

				var result = consolForm.FireSaveButton();
				AssertEquals(ContinueWithSave.No, result);
				AssertEquals(1, loginFormShownCount);
			}

			AssertEquals(false, consol.IsInDatabase);
		}

		#endregion

		#region Saving

		public void TestSaving_PreAllocationsNotExceeded()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertEquals("Precondition: consol's pre-allocation not exceeded", false, consol.IsPreAllocationExceededAndRestricted);

			using (ConsolFormTestClass form = new ConsolFormTestClass(consol))
			{
				form.Show();
				ContinueWithSave continueWithSave = form.ShowPreSaveDialogs();

				AssertNull("Dialog wasn't shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Saving allowed", ContinueWithSave.Yes, continueWithSave);
			}
		}

		public void TestSaveShouldNotHappen_WithNoReopenJobAccessAndConsignorChanged()
		{
			Env.Security.ReopenJob.IsAllowed = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

			var testObjectCreator = new TestObjectCreator(Factory);
			var consignor = testObjectCreator.CreateOrgHeader("ORGCNR", false, false, "AUSYD");
			consignor.OH_IsConsignor = true;
			var newConsignor = testObjectCreator.CreateOrgHeader("ORGNEW", false, false, "AUSYD");
			newConsignor.OH_IsConsignor = true;

			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C0001");
			var shipment = testObjectCreator.CreateShipment("S0001", false);
			var closedJob = testObjectCreator.CreateJob(shipment);
			closedJob.JH_Status = JobHeaderStatus.Closed.Code;
			shipment.ConsignorPK = consignor.PK;
			consol.Shipments.Add(shipment);
			Factory.Save();

			shipment.ConsignorPK = newConsignor.PK;

			using (var form = new ConsolFormTestClass(consol))
			{
				// Saving not allowed with No reopen job access and consignor/consignee changed
				form.Show();
				var consolCostingPlugin = form.PlugIns.GetPlugIn(ControllerIDs.Apportionment);
				consolCostingPlugin.SelectTabPage();
				_ = testObjectCreator.CreateConsolCost(consol, testObjectCreator.CC1, 20, testObjectCreator.Creditor1, AllocationMethod.Shipment);
				var continueWithSave = form.ShowPreSaveDialogs();
				AssertEquals("Saving not allowed", ContinueWithSave.No, continueWithSave);
			}
		}

		public void TestSaving_PreAllocationsExceeded_Adjusted()
		{
			var consol = GetConsolWithPreAllocationExceeded();
			using (var form = new ConsolFormTestClass(consol))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				form.Show();
				var continueWithSave = form.ShowAllocationAdjustmentDialog();

				var dialog = ZFormModaliser.LastFormShownDialogForTest as AllocationAdjustmentDialog;
				AssertNotNull("Dialog was shown", dialog);
				AssertEquals("Saving allowed", ContinueWithSave.Yes, continueWithSave);

				var adjustmentsSecurity = (AllocationAdjustmentsSecurity)dialog.LastDataSourceForTest;
				AssertContainsExactElementsInAnyOrder(new ForwardingConsol[] { consol }, adjustmentsSecurity.Adjustments.Cast<AllocationAdjustment>().Select(x => x.Consol));
			}
		}

		public void TestSaving_PreAllocationsExceeded_NotAdjusted()
		{
			var consol = GetConsolWithPreAllocationExceeded();
			using (var form = new ConsolFormTestClass(consol))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

				form.Show();
				var continueWithSave = form.ShowAllocationAdjustmentDialog();

				var dialog = ZFormModaliser.LastFormShownDialogForTest as AllocationAdjustmentDialog;
				AssertNotNull("Dialog was shown", dialog);
				AssertEquals("Saving restricted", ContinueWithSave.No, continueWithSave);
			}
		}

		ForwardingConsol GetConsolWithPreAllocationExceeded()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 100m;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 100m;

			var allocationChecks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			allocationChecks.Weight.Action = PreAllocationCheck.Actions.Restriction;
			allocationChecks.Weight.Percentage = 50m;
			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allocationChecks);

			AssertEquals("Precondition: consol's pre-allocation exceeded", true, consol.IsPreAllocationExceededAndRestricted);

			return consol;
		}

		public void TestSaving_NonMatchingAgents()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertEquals("Precondition: Receiving Forwarder matches shipment related agents", false, consol.Validation.ReceivingForwarderHasBeenChanged());
			using (ConsolFormTestClass form = new ConsolFormTestClass(consol))
			{
				form.Show();
				ContinueWithSave continueWithSave = form.ShowPreSaveDialogs();

				AssertNull("Dialog wasn't shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Saving allowed", ContinueWithSave.Yes, continueWithSave);
			}
		}

		public void TestSaving_NonMatchingAgents_Confirmed()
		{
			var consol = GetConsolWithReceivingForwarderChanged();
			using (ConsolFormTestClass form = new ConsolFormTestClass(consol))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				form.Show();
				ContinueWithSave continueWithSave = form.ShowPreSaveDialogs();

				var dialog = ZFormModaliser.LastFormShownDialogForTest as NonMatchingAgentsDialog;
				AssertNotNull("Dialog was shown", dialog);
				AssertEquals("Saving allowed", ContinueWithSave.Yes, continueWithSave);
			}
		}

		public void TestSaving_NonMatchingAgents_NotConfirmed()
		{
			ForwardingConsol consol = GetConsolWithShipmentConsigneeChanged();
			using (ConsolFormTestClass form = new ConsolFormTestClass(consol))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.No;

				form.Show();
				ContinueWithSave continueWithSave = form.ShowPreSaveDialogs();

				var dialog = ZFormModaliser.LastFormShownDialogForTest as NonMatchingAgentsDialog;
				AssertNotNull("Dialog was not shown", dialog);
				AssertEquals("Saving restricted", ContinueWithSave.No, continueWithSave);
			}
		}

		public void TestSaving_ConsolWithAssemblyMasterShipment_ChangedToDirect()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			shipment.JS_ShipmentType = "ASM";
			consol.JK_AgentType = "DRT";
			using (ConsolFormTestClass form = new ConsolFormTestClass(consol))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				form.ShowPreSaveDialogs();

				var notificationMessage = UnitTestUserNotification.Instance.LastMessage;
				AssertNotNull("Dialog was shown", notificationMessage);
				AssertEquals("Assembly Master as Direct Master may breach customs law", notificationMessage.Caption);
			}
		}

		public void TestSaving_AssemblyMasterAsDirectMaster_ShouldShowAMessageboxWithCheckboxIfRegistrySettingIsSetToTrue()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "ASM";
			consol.JK_AgentType = "DRT";
			using (FreightDataRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ConsolFormTestClass form = new ConsolFormTestClass(consol))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.ShowPreSaveDialogs();
				var shownDialog = ZFormModaliser.LastFormShownDialogForTest as AcknowledgementMessageBox;
				AssertNotNull("Acknowledgement message box is shown", shownDialog);
			}
		}

		public void TestSaving_AssemblyMasterAsDirectMaster_ShouldNotShowAMessageboxWithCheckboxIfRegistrySettingIsSetToFalse()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "ASM";
			consol.JK_AgentType = "DRT";
			using (FreightDataRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ConsolFormTestClass form = new ConsolFormTestClass(consol))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.ShowPreSaveDialogs();
				var shownDialog = ZFormModaliser.LastFormShownDialogForTest as AcknowledgementMessageBox;
				AssertNull("Acknowledgement message box is not shown", shownDialog);
			}
		}

		public void TestSaving_AssemblyMasterAsDirectMaster_ShouldLogEvent()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "ASM";
			consol.JK_AgentType = "DRT";
			using (FreightDataRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ConsolFormTestClass form = new ConsolFormTestClass(consol))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var continueWithSaveResult = form.ShowPreSaveDialogs();
				AssertEquals(continueWithSaveResult, ContinueWithSave.Yes);
				var ackEvent = consol.Logs.MostRecentLogByEventTime(Events.Acknowledged);
				AssertNotNull("Event was logged", ackEvent);
				var eventParameters = StmALog.GetParametersFromReference(ackEvent.SL_Reference);
				AssertEquals(eventParameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type], "that using Assembly Master as Direct Master might breach customs law by typing the disclaimer");
			}
		}

		public void TestSaving_AssemblyMasterAsDirectMaster_ShouldLogEventIndicatingThatTheUserTickedTheCheckbox()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "ASM";
			consol.JK_AgentType = "DRT";
			using (FreightDataRegistry.Instance.AssemblyMasterOnDirectConsolComplianceDisclaimer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ConsolFormTestClass form = new ConsolFormTestClass(consol))
			{
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var continueWithSaveResult = form.ShowPreSaveDialogs();
				AssertEquals(continueWithSaveResult, ContinueWithSave.Yes);
				var ackEvent = consol.Logs.MostRecentLogByEventTime(Events.Acknowledged);
				AssertNotNull("Event was logged", ackEvent);
				var eventParameters = StmALog.GetParametersFromReference(ackEvent.SL_Reference);
				AssertEquals(eventParameters[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type], "that using Assembly Master as Direct Master might breach customs law by checking the \"I acknowledge\" tick box");
			}
		}

		public void TestSaving_ControllingPartiesDefaulted()
		{
			FreightDataRegistry.Instance.DefaultShipmentControllingCustomer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentControllingAgent.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			var controllingCustomer1 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingCustomer2 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent1 = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent2 = Factory.NewWithValidTestData<OrgHeader>();

			var relatedParty1 = controllingCustomer1.AllRelatedParties.AddNew();
			relatedParty1.PR_OH_RelatedParty = controllingAgent1.PK;
			relatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			relatedParty1.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;

			var relatedParty2 = controllingCustomer2.AllRelatedParties.AddNew();
			relatedParty2.PR_OH_RelatedParty = controllingAgent2.PK;
			relatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.ControllingAgent;
			relatedParty2.PR_FreightDirection = RelatedPartyDirectionList.Codes.Sales;

			var supplierLink = consignee.SupplierLinks.AddNew();
			supplierLink.OL_OH_Supplier = consignor.PK;

			var supplierLinkTransportMode = supplierLink.OrgSupBuyLinkTrnModes[0];
			supplierLinkTransportMode.PF_TransportMode = shipment.JS_TransportMode;
			supplierLinkTransportMode.PF_ContainerMode = shipment.JS_PackingMode;
			supplierLinkTransportMode.PF_OH_ControllingCustomer = controllingCustomer1.PK;

			using (var form = new ConsolFormTestClass(consol))
			{
				form.Show();
				form.FireSaveButton();

				AssertEquals(controllingCustomer1.PK, shipment.ControllingCustomer.PK);
				AssertEquals(controllingAgent1.PK, shipment.ControllingAgentDocumentaryAddress.OrganisationPK);

				shipment.ControllingCustomerNameOrPK = controllingCustomer2.PK.ToString();

				form.FireSaveButton();

				AssertEquals(controllingCustomer2.PK, shipment.ControllingCustomer.PK);
				AssertEquals(controllingAgent2.PK, shipment.ControllingAgentDocumentaryAddress.OrganisationPK);
			}
		}

		public void TestActionMenu_HasAirlineConnect_WhenEnableAirlineConnectMenuRegistryIsTrue()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			using (FreightDataRegistry.Instance.EnableAirlineConnectMenu.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var airlineConnectMenu = FindMenuItem(consolForm, "Actions").MenuItems.FindByText("AirlineConnect");

				AssertNull(airlineConnectMenu);
			}

			using (FreightDataRegistry.Instance.EnableAirlineConnectMenu.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var airlineConnectMenu = FindMenuItem(consolForm, "Actions").MenuItems.FindByText("AirlineConnect");

				AssertNotNull(airlineConnectMenu);
			}
		}

		public void TestAirlineConnectMenuItemClick_ValidatesAirlinePreRequisites()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.Transports[0].JW_Status = Constants.TransportStatus.Confirmed;

			using (FreightDataRegistry.Instance.EnableAirlineConnectMenu.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var airlineConnectMenu = FindMenuItem(consolForm, "Actions").MenuItems.FindByText("AirlineConnect");

				airlineConnectMenu.PerformClick();
				AssertEquals("Please save before opening AirlineConnect form", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				airlineConnectMenu.PerformClick();
				AssertEquals("AirlineConnect is only available for consols with a transport mode of AIR", UnitTestUserNotification.Instance.LastMessage.Text);

				consol.JK_TransportMode = Constants.TransportModes.Air;
				Factory.Save();

				airlineConnectMenu.PerformClick();
				AssertEquals(ConsolPacklineHelper.NoPreAllocationShipmentAndContainerErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				consol.Transports[0].JW_Status = Constants.TransportStatus.Queued;
				Factory.Save();
				airlineConnectMenu.PerformClick();
				AssertEquals(ConsolPacklineHelper.NoPreAllocationShipmentAndContainerErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				consol.Transports[0].JW_Status = Constants.TransportStatus.Planned;
				Factory.Save();
				airlineConnectMenu.PerformClick();
				AssertEquals(ConsolPacklineHelper.NoPreAllocationShipmentAndContainerErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				consol.Shipments.AddNew();
				consol.Shipments[0].OuterPackLines.AddNew();
				consol.Shipments[0].OuterPackLines[0].JL_Length = 1;
				consol.Shipments[0].OuterPackLines[0].JL_Width = 1;
				consol.Shipments[0].OuterPackLines[0].JL_Height = 1;
				consol.Shipments[0].OuterPackLines[0].JL_ActualWeight = 1;
				consol.Shipments[0].OuterPackLines[0].JL_ActualVolume = 1;
				consol.Shipments[0].OuterPackLines[0].JL_PackageCount = 1;

				consol.JK_TotalShipmentCountCheck = 1;
				consol.JK_TotalShipmentActWeightCheck = 1;
				consol.JK_TotalShipmentActVolumeCheck = 1;
				consol.JK_MaximumAllowablePackageHeight = 1;
				consol.JK_MaximumAllowablePackageLength = 1;
				consol.JK_MaximumAllowablePackageWidth = 1;
				consol.JK_MaximumAllowablePackageUnit = Core.Constants.Length.Centimetres;

				Factory.Save();

				airlineConnectMenu.PerformClick();
				AssertEquals(string.Format("{0} cannot be opened in a browser as GLOW has not been configured for this client.\r\nRegistry: {1}/{2}", "AirlineConnect", GlowRegistry.Instance.GlowPortalsUri.Category, GlowRegistry.Instance.GlowPortalsUri.Caption)
					, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAirlineConnectMenuItemClick_OpenInBrowser()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.Shipments.AddNew();
			consol.Shipments[0].OuterPackLines.AddNew();
			consol.Shipments[0].OuterPackLines[0].JL_Length = 1;
			consol.Shipments[0].OuterPackLines[0].JL_Width = 1;
			consol.Shipments[0].OuterPackLines[0].JL_Height = 1;
			consol.Shipments[0].OuterPackLines[0].JL_ActualWeight = 1;
			consol.Shipments[0].OuterPackLines[0].JL_ActualVolume = 1;
			consol.Shipments[0].OuterPackLines[0].JL_PackageCount = 1;

			var container = consol.Containers.AddNew();
			container.JC_ContainerCount = 1;
			container.JC_GrossWeight = 100;
			container.JC_TotalWidth = 1;
			container.JC_TotalLength = 1m;
			container.JC_TotalHeight = 1m;
			consol.JK_TotalShipmentCountCheck = 1;
			consol.JK_TotalShipmentActWeightCheck = 1;
			consol.JK_TotalShipmentActVolumeCheck = 1;
			consol.JK_MaximumAllowablePackageHeight = 1;
			consol.JK_MaximumAllowablePackageLength = 1;
			consol.JK_MaximumAllowablePackageWidth = 1;
			consol.JK_MaximumAllowablePackageUnit = Core.Constants.Length.Centimetres;

			Factory.Save();

			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");
			using (var consolForm = new ConsolForm(consol))
			{
				consolForm.Show();
				var airlineConnectMenu = FindMenuItem(consolForm, "Actions").MenuItems.FindByText("AirlineConnect");
				airlineConnectMenu.PerformClick();

				var launchedUrl = WebUrlLauncher.LastUrlLaunched;
				var uri = new Uri(launchedUrl, UriKind.Absolute);
				var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);
				var accessToken = queryKeyValuePairs["sso_otp"];
				var consolkey = queryKeyValuePairs["pk"];

				AssertNotNull("A Glow Access Token should be attached", accessToken);
				AssertEquals("https", uri.Scheme);
				AssertEquals(consol.PK.ToString(), consolkey);
				AssertEquals("address", uri.Host);
				AssertEquals("/RTS", uri.AbsolutePath);
			}
		}

		public void TestOverrideCheckBoxForAWBAndSecurityDeclarationAreIndependent()
		{
			var consol = CreateMAWBWithConsol();
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.MaintainConsolAWBOverride.IsAllowed = true;

			using (var form = new ConsolFormTestClass(consol))
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				form.Show();
				form.MainTabControl.SelectedTab = form.AWBTabPage;
				var mawbControl = form.Controls.Find("MAWBWithMessages", true).First() as MAWBWithMessagesUserControl;
				var securityDeclarationTabPage = mawbControl.Controls.Find("SecurityDeclarationTabPage", true).First() as ZTabPage;
				var mawbTabPage = mawbControl.Controls.Find("MAWBTabPage", true).First() as ZTabPage;

				var sdControl = securityDeclarationTabPage.Controls.Find("SecurityDeclarationControl", true).First();
				var awbControl = form.Controls.Find("MAWBUserControl", true).First() as AWBUserControl;

				var awbOverrideCheckBox = (ZCheckBox)awbControl.Controls.Find("OverrideValuesCheckBox", true)[0];
				var sdOverrideCheckBox = (ZCheckBox)sdControl.Controls.Find("overrideValuesCheckBox", true)[0];

				AssertEquals("IsAWBValuesOverriddenProperty", awbOverrideCheckBox.GetBindingMember());
				AssertEquals("IsCSDValuesOverriddenProperty", sdOverrideCheckBox.GetBindingMember());

				consol.IsAWBValuesOverriddenProperty = true;
				consol.IsCSDValuesOverriddenProperty = false;
				AssertEquals(true, consol.IsAWBValuesOverriddenProperty);
				AssertEquals(false, consol.IsCSDValuesOverriddenProperty);
				AssertEquals("EH_SecurityStatus should always be readonly", true, consol.AWBHeader.EH_SecurityStatusInfo.ReadOnly);
				AssertEquals("AWB should not be readonly", false, consol.AWBHeader.ReadOnly);
				AssertEquals("AWB should not be readonly", false, consol.AWBHeader.EH_FinalizationDateInfo.ReadOnly);
				AssertEquals("SD should be readonly", true, consol.AWBHeader.ExportAWBSecurityStatusLines.ReadOnly);
				AssertEquals("SD should be readonly", true, consol.AWBHeader.EH_SecurityStatusIssueDateInfo.ReadOnly);
				AssertEquals("SD should be readonly", true, consol.AWBHeader.EH_AgentApprovalExpiryDateInfo.ReadOnly);
				AssertEquals("SD should be readonly", true, consol.AWBHeader.EH_RN_NKAgentApprovalCountryCodeInfo.ReadOnly);
				AssertEquals("SD should be readonly", true, consol.AWBHeader.EH_GS_NKSecurityStatusIssuedByCodeInfo.ReadOnly);
				AssertEquals("SD should be readonly", true, consol.AWBHeader.EH_AdditionalSecurityInformationInfo.ReadOnly);
				AssertEquals("SD should be readonly", true, consol.AWBHeader.EH_AdditionalSecurityInformationStatementInfo.ReadOnly);

				consol.IsAWBValuesOverriddenProperty = false;
				consol.IsCSDValuesOverriddenProperty = true;
				AssertEquals(false, consol.JK_OverrideWaybillDefaults);
				AssertEquals(true, consol.JK_OverrideSecurityDeclarationDefaults);
				AssertEquals("EH_SecurityStatus should always be readonly", true, consol.AWBHeader.EH_SecurityStatusInfo.ReadOnly);
				AssertEquals("AWB should not be readonly", true, consol.AWBHeader.ReadOnly);
				AssertEquals("AWB should not be readonly", true, consol.AWBHeader.EH_SecurityStatusInfo.ReadOnly);
				AssertEquals("SD should not be readonly", false, consol.AWBHeader.ExportAWBSecurityStatusLines.ReadOnly);
				AssertEquals("SD should not be readonly", false, consol.AWBHeader.EH_SecurityStatusIssueDateInfo.ReadOnly);
				AssertEquals("SD should not be readonly", false, consol.AWBHeader.EH_AgentApprovalExpiryDateInfo.ReadOnly);
				AssertEquals("SD should not be readonly", false, consol.AWBHeader.EH_RN_NKAgentApprovalCountryCodeInfo.ReadOnly);
				AssertEquals("SD should not be readonly", false, consol.AWBHeader.EH_GS_NKSecurityStatusIssuedByCodeInfo.ReadOnly);
				AssertEquals("SD should not be readonly", false, consol.AWBHeader.EH_AdditionalSecurityInformationInfo.ReadOnly);
				AssertEquals("SD should not be readonly", false, consol.AWBHeader.EH_AdditionalSecurityInformationStatementInfo.ReadOnly);
			}
		}

		ForwardingConsol GetConsolWithReceivingForwarderChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_ReceivingForwarderAddress = org1.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = consignee.PK;
			consignee.SetRelatedParty(org1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery);

			consol.Factory.Save();
			consol.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;

			return consol;
		}

		ForwardingConsol GetConsolWithShipmentConsigneeChanged()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var consignee1 = Factory.NewWithValidTestData<OrgHeader>();
			consignee1.SetRelatedParty(org1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_ReceivingForwarderAddress = org.MainAddress.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = consignee.PK;
			consignee.SetRelatedParty(org1, RelatedPartyTypeList.Codes.ReceivingAgent, RelatedPartyDirectionList.Codes.Delivery);

			consol.Factory.Save();
			shipment.ConsigneePK = consignee1.PK;

			return consol;
		}

		void PreparedDataForTestSavingConsolidationWithPrimaryFieldChanged(string transportMode, string shipment1Id, string shipment2Id,out ForwardingConsol consol)
		{
			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "TEST001";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment1.JS_TransportMode = transportMode;
			shipment1.JS_UniqueConsignRef = shipment1Id;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_UniqueConsignRef = shipment2Id;
			shipment2.JS_TransportMode = transportMode;

			shipment1.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));

			shipment2.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));

			var genPivot1 = Factory.New<IGenPivot>();
			genPivot1.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
			genPivot1.XX_Relation1ID = shipment1.HVLVConsignmentHeader.PK;
			genPivot1.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;

			var genPivot2 = Factory.New<IGenPivot>();
			genPivot2.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
			genPivot2.XX_Relation1ID = shipment2.HVLVConsignmentHeader.PK;
			genPivot2.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;

			BusinessObject customJob1 = null;
			BusinessObject customJob2 = null;

			if (transportMode == TransportModes.Air)
			{
				customJob1 = Factory.NewWithValidTestData<CusMAWB>();
				customJob2 = Factory.NewWithValidTestData<CusMAWB>();
				genPivot1.XX_Relation2TableCode = CusMAWBSchema.Constants.Prefix;
				genPivot2.XX_Relation2TableCode = CusMAWBSchema.Constants.Prefix;
			}
			else if (transportMode == TransportModes.Sea)
			{
				customJob1 = Factory.NewWithValidTestData<BaseCusSCAOceanBill>();
				customJob2 = Factory.NewWithValidTestData<BaseCusSCAOceanBill>();
				genPivot1.XX_Relation2TableCode = CusSCAOceanBillSchema.Constants.Prefix;
				genPivot2.XX_Relation2TableCode = CusSCAOceanBillSchema.Constants.Prefix;
			}

			genPivot1.XX_Relation2ID = customJob1.PK;
			genPivot2.XX_Relation2ID = customJob2.PK;
			Factory.Save();
		}

		void TestSavingConsolidationWithPrimaryFieldChangedForDifferentCountry_ClickCancel_ConsolidationNotSaved(string transportMode, string shipment1Id, string shipment2Id)
		{
			PreparedDataForTestSavingConsolidationWithPrimaryFieldChanged(transportMode, shipment1Id, shipment2Id, out var consol);

			using (var form = new ConsolFormTestClass(consol))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				consol.JK_MasterBillNum = "HB0012";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				var continueWithSaveResult = form.ShowPreSaveDialogs();
				AssertEquals(continueWithSaveResult, ContinueWithSave.No);

				var message = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals($"Customs job(s) have already been created for the following shipment(s), saving the consolidation may negatively affect existing Customs job(s) due to primary field(s) update.\r\n{shipment1Id},{shipment2Id}", message);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestSavingConsolidationWithPrimaryFieldChanged_ClickCancel_ConsolidationNotSaved()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				TestSavingConsolidationWithPrimaryFieldChangedForDifferentCountry_ClickCancel_ConsolidationNotSaved(TransportModes.Air, "S00001000", "S00001001");
				TestSavingConsolidationWithPrimaryFieldChangedForDifferentCountry_ClickCancel_ConsolidationNotSaved(TransportModes.Sea, "S00001002", "S00001003");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				TestSavingConsolidationWithPrimaryFieldChangedForDifferentCountry_ClickCancel_ConsolidationNotSaved(TransportModes.Air, "S00001004", "S00001005");
				TestSavingConsolidationWithPrimaryFieldChangedForDifferentCountry_ClickCancel_ConsolidationNotSaved(TransportModes.Sea, "S00001006", "S00001007");
			}
		}

		void TestSavingConsolidationWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanBeCancelled_DoNotDeactivateOldJobAndCancelTRF(string transportMode, string shipment1Id, string shipment2Id)
		{
			PreparedDataForTestSavingConsolidationWithPrimaryFieldChanged(transportMode, shipment1Id, shipment2Id, out var consol);

			using (var form = new ConsolFormTestClass(consol))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				consol.JK_MasterBillNum = "HB0012";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var continueWithSaveResult = form.ShowPreSaveDialogs();

				AssertEquals(continueWithSaveResult, ContinueWithSave.Yes);
				var shipments = consol.Shipments.Cast<ForwardingShipment>()
									  .Where(shipment => shipment.JS_ShipmentType == ShipmentTypes.HighVolumeLowValue && shipment.HVLVConsignmentHeader.CancellableCustomsJobs.Any()).ToList();
				AssertEquals(2, shipments.Count);
				AssertEquals("There exists only one log with TRF mark",
					shipments[0].Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == AutoEvents.TransferredCode), 1);
				Assert("This log is not cancelled in shipment1",
					!(shipments[0].Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == AutoEvents.TransferredCode).IsCancelled));
				AssertEquals("There exists only one log with TRF mark in shipment2",
					shipments[1].Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == AutoEvents.TransferredCode), 1);
				Assert("This log is not cancelled in shipment2",
					!(shipments[1].Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == AutoEvents.TransferredCode).IsCancelled));

				var customsJobsOnShipment1 = shipments[0].HVLVConsignmentHeader.CancellableCustomsJobs;
				AssertEquals(1, customsJobsOnShipment1.Count);
				Assert(!(customsJobsOnShipment1[0].IsCancelled));

				var customsJobsOnShipment2 = shipments[1].HVLVConsignmentHeader.CancellableCustomsJobs;
				AssertEquals(1, customsJobsOnShipment2.Count);
				Assert(!(customsJobsOnShipment2[0].IsCancelled));
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestSavingConsolidationWithPrimaryFieldChanged_RelatedJobsCanBeCancelled_DoNotDeactivateOldJobAndCancelTRF()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				TestSavingConsolidationWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanBeCancelled_DoNotDeactivateOldJobAndCancelTRF(TransportModes.Air, "S00001000", "S00001001");
				TestSavingConsolidationWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanBeCancelled_DoNotDeactivateOldJobAndCancelTRF(TransportModes.Sea, "S00001002", "S00001003");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				TestSavingConsolidationWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanBeCancelled_DoNotDeactivateOldJobAndCancelTRF(TransportModes.Air, "S00001004", "S00001005");
				TestSavingConsolidationWithPrimaryFieldChangedForDifferentCountry_RelatedJobsCanBeCancelled_DoNotDeactivateOldJobAndCancelTRF(TransportModes.Sea, "S00001006", "S00001007");
			}
		}

		void TestSavingConsolidationWithPrimaryFieldOnChangedForDifferentCountry_ClickOk_DetailedErrorMessageDisplay(string shipment1Id, string shipment2Id)
		{
			PreparedDataForTestSavingConsolidationWithPrimaryFieldChanged(TransportModes.Air, shipment1Id, shipment2Id, out var consol1);

			var consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.Shipments.AddNew();
			consol2.JK_IsCancelled = true;

			var jobs = Factory.Load<CusMAWB>(new ZQuery());
			jobs.ForEach(job => job.CM_JK = consol2.PK);
			jobs.ForEach(job => job.IsCancelled = false);

			using (var form = new ConsolFormTestClass(consol1))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				consol1.JK_MasterBillNum = "HB0012";
				var continueWithSaveResult = form.ShowPreSaveDialogs();
				AssertEquals(continueWithSaveResult, ContinueWithSave.No);

				var shipments = consol1.Shipments.Cast<ForwardingShipment>()
										.Where(shipment => shipment.JS_ShipmentType == ShipmentTypes.HighVolumeLowValue && shipment.HVLVConsignmentHeader.CancellableCustomsJobs.Any(job => !job.IsCancelled)).ToList();

				AssertEquals(2, shipments.Count);

				AssertEquals("There exists only one log with TRF mark",
					shipments[0].Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == AutoEvents.TransferredCode), 1);
				Assert("This log has not been cancelled in shipment1",
					!shipments[0].Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == AutoEvents.TransferredCode).IsCancelled);

				AssertEquals("There exists only one log with TRF mark  in shipment2",
					shipments[1].Logs.GetAllLogs().Cast<StmALog>().Count(l => l.SL_SE_NKEvent == AutoEvents.TransferredCode), 1);
				Assert("This log has not been cancelled in shipment2",
					!shipments[1].Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => l.SL_SE_NKEvent == AutoEvents.TransferredCode).IsCancelled);

				var customsJobsOnShipment1 = shipments[0].HVLVConsignmentHeader.CancellableCustomsJobs;
				AssertEquals(1, customsJobsOnShipment1.Count);
				Assert(!customsJobsOnShipment1[0].IsCancelled);

				var customsJobsOnShipment2 = shipments[1].HVLVConsignmentHeader.CancellableCustomsJobs;
				AssertEquals(1, customsJobsOnShipment2.Count);
				Assert(!customsJobsOnShipment2[0].IsCancelled);

				var message = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("Shipment cannot be cancelled.",
					$"Consolidation can't be saved for primary field(s) change because Customs job(s) have already been created for the following shipment(s) and reason:This record cannot be deactivated as its parent host record cannot be deactivated due to the following reason.\r\nConsol {consol2.JK_UniqueConsignRef}: You cannot deactivate Consol {consol2.JK_UniqueConsignRef} since it has shipments attached. : {shipment1Id},{shipment2Id}\r\nPlease detach these shipment(s) from the consolidation and create new shipment(s) and/or consolidation as required.", message);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestSavingConsolidationWithPrimaryFieldOnChanged_ClickOk_DetailedErrorMessageDisplay()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Australia))
			{
				TestSavingConsolidationWithPrimaryFieldOnChangedForDifferentCountry_ClickOk_DetailedErrorMessageDisplay("S00001010", "S00001011");
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.NewZealand))
			{
				TestSavingConsolidationWithPrimaryFieldOnChangedForDifferentCountry_ClickOk_DetailedErrorMessageDisplay("S00001012", "S00001013");
			}
		}

		public void TestSavingConsolidationWithPrimaryFieldOnChanged_NoneCustomJobCreate_NoneMessageDisplay()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "TEST001";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment1.JS_TransportMode = TransportModes.Air;
			shipment1.JS_UniqueConsignRef = "S00001000";
			shipment1.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment2.JS_TransportMode = TransportModes.Air;
			shipment2.JS_UniqueConsignRef = "S00001001";
			shipment2.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));

			Factory.Save();

			using (var form = new ConsolFormTestClass(consol))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				consol.JK_MasterBillNum = "HB0012";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var continueWithSaveResult = form.ShowPreSaveDialogs();

				AssertEquals(continueWithSaveResult, ContinueWithSave.Yes);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestSavingConsolidationWithPrimaryFieldOnChanged_NotFoundHVLVConsignmentHeader_NoneMessageDisplay()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "TEST001";

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment1.JS_TransportMode = TransportModes.Air;
			shipment1.JS_UniqueConsignRef = "S00001000";

			Factory.Save();

			using (var form = new ConsolFormTestClass(consol))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);

				consol.JK_MasterBillNum = "HB0012";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var continueWithSaveResult = form.ShowPreSaveDialogs();

				AssertEquals(continueWithSaveResult, ContinueWithSave.Yes);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestJS_HouseBillIsPrimaryFieldOnShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_TransportMode = TransportModes.Air;
			shipment.JS_UniqueConsignRef = "S00001000";

			shipment.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));

			var genPivot = Factory.New<IGenPivot>();
			genPivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
			genPivot.XX_Relation1ID = shipment.HVLVConsignmentHeader.PK;
			genPivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;

			BusinessObject customJob = null;

			customJob = Factory.NewWithValidTestData<CusMAWB>();
			genPivot.XX_Relation2TableCode = CusMAWBSchema.Constants.Prefix;
			genPivot.XX_Relation2ID = customJob.PK;

			Factory.Save();

			using (var form = new ConsolFormTestClass(consol))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				shipment.JS_HouseBill = "Test Bill";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				var continueWithSaveResult = form.ShowPreSaveDialogs();
				AssertEquals(continueWithSaveResult, ContinueWithSave.No);

				var message = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals($"Customs job(s) have already been created for the following shipment(s), saving the consolidation may negatively affect existing Customs job(s) due to primary field(s) update.\r\nS00001000", message);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestJS_TransportModeIsPrimaryFieldOnShipment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_UniqueConsignRef = "S00001000";

			shipment.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, EventReferenceParameterReasons.CargoReportCreated));

			var genPivot = Factory.New<IGenPivot>();
			genPivot.XX_RelationType = GenPivotTypes.HighVolumeLowValue;
			genPivot.XX_Relation1ID = shipment.HVLVConsignmentHeader.PK;
			genPivot.XX_Relation1TableCode = HVLVConsignmentHeaderSchema.Constants.Prefix;

			BusinessObject customJob = null;

			customJob = Factory.NewWithValidTestData<CusMAWB>();
			genPivot.XX_Relation2TableCode = CusMAWBSchema.Constants.Prefix;
			genPivot.XX_Relation2ID = customJob.PK;

			Factory.Save();

			using (var form = new ConsolFormTestClass(consol))
			{
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				shipment.JS_TransportMode = TransportModes.Air;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				var continueWithSaveResult = form.ShowPreSaveDialogs();
				AssertEquals(continueWithSaveResult, ContinueWithSave.No);

				var message = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals($"Customs job(s) have already been created for the following shipment(s), saving the consolidation may negatively affect existing Customs job(s) due to primary field(s) update.\r\nS00001000", message);
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		public void TestSavingConsolidationWithHVLShipments_ShipmentTypeIsReadOnly()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_MasterBillNum = "TEST001";

			using (var form = new ConsolFormTestClass(consol))
			{
				form.Show();

				var shipment = consol.Shipments.AddNew();
				shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Air;
				shipment.JS_UniqueConsignRef = "S00001000";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var continueWithSaveResult = form.ShowPreSaveDialogs();
				AssertEquals(continueWithSaveResult, ContinueWithSave.No);

				var message = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertContains("There are no HVLV Consignments on the HVL Shipment(s). Do you want to continue saving? Shipment type cannot be changed after consolidation is saved.", message);
			}
		}

		#endregion

		#region Sub classes

		class ConsolFormTestClass : ConsolForm
		{
			public ConsolFormTestClass(ForwardingConsol bO)
				: base(bO)
			{
			}

			public new ZTemplateTabControl MainTabControl
			{
				get { return base.MainTabControl; }
			}

			public new ZTabPage AWBTabPage
			{
				get { return base.AWBTabPage; }
			}

			public new ZTabControl ElectronicMessagingTabControl
			{
				get { return base.ElectronicMessagingTabControl; }
			}

			public new ContinueWithSave ShowPreSaveDialogs()
			{
				return base.ShowPreSaveDialogs();
			}

			public new ContinueWithSave ValidateAndSave()
			{
				return base.ValidateAndSave();
			}

			public new ContinueWithSave ShowAllocationAdjustmentDialog()
			{
				return base.ShowAllocationAdjustmentDialog();
			}

			public ZTabPage LogsTabPageForTest => LogsTabPage;
		}

		class ConsolFormWithXmlDirector : ConsolForm
		{
			public ConsolFormWithXmlDirector(ForwardingConsol bO)
				: base(bO)
			{
			}

			protected override XmlDataTransferExporter GetNewXmlDataTransferExporter(IValueObjectDataAdapter adapter, bool checkForLicence)
			{
				return new XmlDataTransferExporter(adapter, checkForLicence);
			}

			public new MenuItem ActionsMenuItem
			{
				get { return base.ActionsMenuItem; }
			}
		}

		#endregion

		#region View/Transact eBL

		public void TestViewOrTransacteBL()
		{
			AssertShowViewOrTransacteBL(Constants.TransportModes.Air, true, "EBOLRef", false);
			AssertShowViewOrTransacteBL(Constants.TransportModes.Sea, true, string.Empty, false);
			AssertShowViewOrTransacteBL(Constants.TransportModes.Sea, false, "EBOLRef", false);
			AssertShowViewOrTransacteBL(Constants.TransportModes.Sea, true, "EBOLRef", true);

			void AssertShowViewOrTransacteBL(string transportMode, bool enableBoleroEBLIntegration, string electronicBillOfLadingReference, bool visible)
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = transportMode;
				consol.JK_ElectronicBillOfLadingReference = electronicBillOfLadingReference;
				using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = enableBoleroEBLIntegration, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
				{
					using (var form = new ConsolForm(consol))
					{
						form.Show();

						var electronicMessagingMenuItem = (ZMenuItem)form.Menu.MenuItems.FindByText("Electronic Messaging");
						electronicMessagingMenuItem.OnPopup(EventArgs.Empty);
						var carrierMenuItem = (ZMenuItem)electronicMessagingMenuItem.MenuItems.FindByText("Carrier");
						carrierMenuItem.OnPopup(EventArgs.Empty);

						AssertEquals(visible, carrierMenuItem.MenuItems.FindByText("View/Transact eBL").Visible);
					}
				}
			}
		}

		public void TestOpenBolero_Security()
		{
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.MaintainConsolAllowViewTransactEBL.IsAllowed = false;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ElectronicBillOfLadingReference = "EBOLRef";

			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				ConsolForm.OpenBolero(consol);

				AssertEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainConsolAllowViewTransactEBL), UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			securityInstance.MaintainConsolAllowViewTransactEBL.IsAllowed = true;

			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				ConsolForm.OpenBolero(consol);

				AssertNotEquals(Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainConsolAllowViewTransactEBL), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestOpenBolero_SystemDoesNotHaveAValidCertificate()
		{
			var systemToSystemTrustHandler = new Mock<ISystemToSystemTrustHandler>();
			systemToSystemTrustHandler.Setup(x => x.SendMessage(It.IsAny<string>(), It.IsAny<string>()));

			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.MaintainConsolAllowViewTransactEBL.IsAllowed = true;

			var oryProxy = GlbCompany.CurrentCompany.OrgProxy;
			var rid = oryProxy.CustomsCodes.AddNew();
			rid.OK_CodeType = OrgCusCode.CodeTypes.BoleroTitleRegisterID;
			rid.OK_CustomsRegNo = "Test123";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ElectronicBillOfLadingReference = "EBOLRef";

			using (ObjectFactory.Substitute(systemToSystemTrustHandler.Object))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new SystemToSystemTrustInfo()))
			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ConsolForm.OpenBolero(consol);

				AssertEquals("The system does not have a valid certificate. Please update the certificate via service task 'TCM'. If this issue persists, please contact your administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (ObjectFactory.Substitute(systemToSystemTrustHandler.Object))
			using (SystemDataRegistry.Instance.SystemToSystemCertificate.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, AuthenticationCertificateInfoTest.GetDummyAuthenticationInfo()))
			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ConsolForm.OpenBolero(consol);

				AssertNotEquals("The system does not have a valid certificate. Please update the certificate via service task 'TCM'. If this issue persists, please contact your administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Template Records

		public void TestTemplateRecord_ShouldNotShow_ElectronicMessaging()
		{
			var expectedMenuItem = "Electronic Messaging";
			var expectedTabPage = "ElectronicMessagingTabPage";

			var consol = Factory.New<ForwardingConsol>();
			var consolTemplate = GetNewTemplateConsol();

			using (var form = new ConsolFormForTest(consol))
			{
				form.Show();

				CombineAssertions("Regular consol should display the menu item and tab page", () =>
				{
					AssertNotNull("Menu item exists", FindMenuItemOrNull(form, expectedMenuItem));
					Assert("Tab page exists", form.MainTabControl_ForTest.AllTabPages.Any(tab => tab.Name == expectedTabPage));
				});
			}

			using (var form = new ConsolFormForTest(consolTemplate))
			{
				form.Show();

				CombineAssertions("Consol template should not display the menu item or tab page", () =>
				{
					AssertNull("No menu item", FindMenuItemOrNull(form, expectedMenuItem));
					Assert("No tab page", !form.MainTabControl_ForTest.AllTabPages.Any(tab => tab.Name == expectedTabPage));
				});
			}
		}

		public void TestTemplateRecord_ShouldNotShow_Workflow()
		{
			var expectedTabPage = "WorkflowTabPage";

			var consol = Factory.New<ForwardingConsol>();
			var consolTemplate = GetNewTemplateConsol();

			using (var form = new ConsolFormForTest(consol))
			{
				form.Show();
				Assert("Regular consol should display workflow", form.MainTabControl_ForTest.AllTabPages.Any(tab => tab.Name == expectedTabPage));
			}

			using (var form = new ConsolFormForTest(consolTemplate))
			{
				form.Show();
				Assert("Consol template should not display workflow", !form.MainTabControl_ForTest.AllTabPages.Any(tab => tab.Name == expectedTabPage));
			}
		}

		public void TestTemplateRecord_ShouldNotShow_ShipmentsGrid()
		{
			var consol = Factory.New<ForwardingConsol>();
			var consolTemplate = GetNewTemplateConsol();

			using (var form = new ConsolFormForTest(consol))
			{
				form.Show();
				Assert("Regular consol should display shipments", !form.ConsolControl.ShipmentModuleButtonGrid.IsDisposed);
			}

			using (var form = new ConsolFormForTest(consolTemplate))
			{
				form.Show();
				Assert("Consol template should not display shipments", form.ConsolControl.ShipmentModuleButtonGrid.IsDisposed);
			}
		}

		public void TestTemplateRecord_ShouldNotInitializePlugins()
		{
			var consolTemplate = GetNewTemplateConsol();

			using (var form = new ConsolFormForTest(consolTemplate))
			{
				form.Show();
				Assert(
					"Only plugin that should be initialised is eDocs (from parent)",
					form.PlugIns.Instances.Length == 1
				);
			}
		}

		public void TestTemplateRecord_ShouldShowYellowBanner()
		{
			var expectedLabel = "templateRecordLabel";
			var consol = Factory.New<ForwardingConsol>();
			var consolTemplate = GetNewTemplateConsol();

			using (var form = new ConsolFormForTest(consol))
			{
				form.Show();
				Assert("Regular consol should not display `template record`", !(form.Find(control => control.Name == expectedLabel).Any()));
			}

			using (var form = new ConsolFormForTest(consolTemplate))
			{
				form.Show();
				Assert("Consol template should display `template record`", form.Find(control => control.Name == expectedLabel).Any());
			}
		}

		public void TestTemplateRecordValidationStandard()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.StandardValidation);
			AssertTemplateRecordValidation(true, false);
		}

		public void TestTemplateRecordValidationNone()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.NoValidation);
			AssertTemplateRecordValidation(false, true);
		}

		public void TestTemplateRecordValidationIgnoreAndSave()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.IgnoreAndSave);
			AssertTemplateRecordValidation(true, true, DialogResult.Ignore);
		}

		public void TestTemplateRecordValidationIgnoreAndSaveAbort()
		{
			DataRegistry.Instance.RawRegistry.TemplateRecordValidation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RawDataRegistry.TemplateRecordValidationCodes.IgnoreAndSave);
			AssertTemplateRecordValidation(true, false, DialogResult.Abort);
		}

		void AssertTemplateRecordValidation(bool expectValidation, bool expectSave, DialogResult errorDialogResult = DialogResult.OK)
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();
			factory.TemplateRecordProvider = consol;

			var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
			var templateRecordProvider = (ITemplateRecordProvider)consol;
			templateRecordProvider.IsTemplateRecord = true;
			templateRecordProvider.TemplateRecord = templateRecord;

			consol.JK_TransportMode = "XYZ";
			AssertEquals(expectValidation, consol.JK_TransportModeInfo.HasErrors());

			ChildEditableService.SetState(consol.Factory, ChildEditableServiceStates.Consol);
			using (var consolForm = new ConsolFormForTest(consol))
			{
				consolForm.ShowErrorsDialogCloseDialogResultForTest = errorDialogResult;

				AssertEquals(true, consol.HasChanges);
				AssertEquals(false, templateRecord.IsInDatabase);
				AssertEquals(ODisplayMode.Edit, consolForm.DisplayMode);

				consolForm.FireSaveButton();

				AssertEquals(!expectSave, consol.HasChanges);
				AssertEquals(expectSave, templateRecord.IsInDatabase);
				AssertEquals(expectSave ? ODisplayMode.Browse : ODisplayMode.Edit, consolForm.DisplayMode);
			}
		}

		#endregion

		public void TestNullRefIsNotThrownWhenInvoicePluginLoaded()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "CAG";
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Code = "CAG";

			var appointedPort = orgHeader.AppointedGatewayAgentPorts.AddNew();
			appointedPort.O5_PortOrCountry = "AUSYD";
			appointedPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			appointedPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedPort.O5_OA_AgentOfficeAddress = orgAddress.PK;

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgHeader.PK;

			var consolTemplate = GetNewTemplateConsol();
			consolTemplate.JK_TransportMode = Constants.TransportModes.Air;
			consolTemplate.JK_AgentType = Constants.AgentType.Agent;
			consolTemplate.JK_RL_NKLoadPort = "AUSYD";
			consolTemplate.JK_RL_NKDischargePort = "BA4CA";
			consolTemplate.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consolTemplate.JK_OA_SendingForwarderAddress = orgAddress.PK;
			Factory.Save();

			using (var form = new ConsolForm(consolTemplate))
			{
				AssertNoExceptionThrown("Template records should not thrown when invoice plugin loaded.", form.Show);
			}
		}

		public void TestSetupControlsBasedOnTransportMode_VGM()
		{
			var consol = Factory.New<ForwardingConsol>();

			using (var form = new ConsolForm(consol))
			{
				form.Show();

				consol.JK_TransportMode = Constants.TransportModes.Air;
				AssertEquals(false, form.ConsolContainerControl.VGMVisible);

				consol.JK_TransportMode = Constants.TransportModes.Sea;
				AssertEquals(true, form.ConsolContainerControl.VGMVisible);

				consol.JK_TransportMode = Constants.TransportModes.Road;
				AssertEquals(true, form.ConsolContainerControl.VGMVisible);

				consol.JK_TransportMode = Constants.TransportModes.Rail;
				AssertEquals(true, form.ConsolContainerControl.VGMVisible);
			}
		}

		public void TestGrossWeightOverridden_False()
			=> TestGrossWeightOverridden
			(
				grossWeightOverride: false,
				expectedNotification: null
			);

		public void TestGrossWeightOverridden_True()
			=> TestGrossWeightOverridden
			(
				grossWeightOverride: true,
				expectedNotification: "Attaching Shipment S0001. Gross Weight of Container CON1 is overridden. Would you like to retain the Overridden Gross Weight?"
			);

		void TestGrossWeightOverridden(bool grossWeightOverride, string expectedNotification)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var consignor = testObjectCreator.CreateOrgHeader("ORGCNR", false, false, "AUSYD");
			consignor.OH_IsConsignor = true;
			var consignee = testObjectCreator.CreateOrgHeader("ORGCNE", false, false, "NZAKL");
			consignee.OH_IsConsignee = true;
			var gatewayConsol = testObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var shipment = testObjectCreator.CreateShipment("S0001", false);
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;
			shipment.JS_ReleaseType = Constants.ShipmentReleaseTypes.BankLetterOfCredit;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			var outerpacklines = shipment.OuterPackLines.AddNew();
			outerpacklines.JL_PackageCount = 5;

			var container = gatewayConsol.Containers.AddNew();
			container.JC_ContainerNum = "con1";
			container.JC_IsGrossWeightOverridden = grossWeightOverride;
			Factory.Save();

			var bizObjCheckFactory = new BusinessObjectFactory();
			AssertNull("Precondition : Shipment job does not exist", new JobHeader.Loader(bizObjCheckFactory, shipment).Load());
			AssertNull("Precondition : Gateway job does not exist", new JobHeader.Loader(bizObjCheckFactory, gatewayConsol).Load());

			var consolFormFactory = new BusinessObjectFactory();
			var gatewayConsolForForm = consolFormFactory.Load<ForwardingConsol>(gatewayConsol.PK);
			var shipmentForForm = consolFormFactory.Load<ForwardingShipment>(shipment.PK);
			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA"));

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, Env.CurrentBranchPK, gatewayDepartment.PK.ToGuid()))
			using (var consolForm = new ConsolForm(gatewayConsolForForm))
			{
				consolForm.Show();
				var shipmentGrid = consolForm.ConsolControl.ShipmentModuleButtonGrid;
				AssertEquals("No shipments attached to consol.", 0, shipmentGrid.InnerGrid.List.Count);

				shipmentGrid.AttachButtonForTest.PerformClick();
				using (var popup = shipmentGrid.LastShownAttachPopupForTesting)
				{
					popup.EmbeddedModulePopupOKButtonStrategy.HandleFindBoxOKButton(new[] { shipmentForForm });
					AssertEquals("Notification", expectedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				AssertEquals("Shipment is attached to consol.", 1, shipmentGrid.InnerGrid.List.Count);
			}
		}

		public void TestExitSummaryControllerPlugin_Visible()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using var form = new ConsolFormForTest(consol);
			var exitSummaryControllerPlugin = form.PlugIns.GetPlugIn(ControllerIDs.Customs.EU.ExitSummaryController);
			AssertNotNull(exitSummaryControllerPlugin);
		}

		#region ComplianceRiskPlugin

		public void TestIncidentDefaultModuleOnComplianceRiskTab()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (var form = new ConsolFormForTest(consol))
			{
				form.Show();
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.Forwarding, (form as ICustomerServiceMenuSectionCodeOverridable).SectionCode);

				form.MainTabControl_ForTest.SelectTab("ComplianceRiskTabPage");
				AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.ComplianceWise, (form as ICustomerServiceMenuSectionCodeOverridable).SectionCode);
			}
		}

		public void TestComplianceRiskPlugin_Visible()
		{
			AssertComplianceRiskPluginVisibility(false);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			{
				AssertComplianceRiskPluginVisibility(true);
			}

			void AssertComplianceRiskPluginVisibility(bool registryValue)
			{
				ChildEditableService.SetState(Factory, ChildEditableServiceStates.Consol);
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();

				using (var form = new ConsolFormForTest(consol))
				{
					var complianceRiskPlugin = form.PlugIns.GetPlugIn(ControllerIDs.ComplianceRiskPlugin);
					if (registryValue)
					{
						AssertNotNull(complianceRiskPlugin);
					}
					else
					{
						AssertNull(complianceRiskPlugin);
					}
				}
			}
		}

		#endregion

		#region GeneratePayload

		public void TestGeneratePayload()
		{
			using (SetTemporaryCurrentUser(Factory))
			{
				var payload1 = ConsolForm.GeneratePayload("eblIssuer", "rid", "eblDocumentId", out ZString errorMsg1);
				AssertEquals("The user code is not found in the current user.", errorMsg1);
			}

			using (SetTemporaryCurrentCompany(Factory))
			{
				var payload2 = ConsolForm.GeneratePayload("eblIssuer", "rid", "eblDocumentId", out ZString errorMsg2);
				AssertEquals("The company code is not found in the current company.", errorMsg2);
			}

			var payload = ConsolForm.GeneratePayload("eblIssuer", "rid", "eblDocumentId", out ZString errorMsg);
			AssertNullOrEmpty(errorMsg);

			var companyCode = GlbCompany.CurrentCompany?.GC_Code ?? ZString.Empty;
			var userCode = GlbStaff.CurrentUser?.GS_Code ?? ZString.Empty;
			var language = GlbStaff.CurrentUser?.Language;
			language = language?.ToString() ?? "EN";
			var timeZone = GlbBranch.CurrentBranch?.HomePort?.TimeZoneSet?.StandardZone?.R2_CivilianTimeZoneCode;
			timeZone = timeZone?.ToString() ?? "UTC";
			AssertEquals($"{{\"companyCode\":\"{companyCode}\",\"eblDocumentId\":\"eblDocumentId\",\"timeZone\":\"{timeZone}\",\"language\":\"{language}\",\"rid\":\"rid\",\"userCode\":\"{userCode}\",\"action\":\"\",\"eblIssuer\":\"eblIssuer\"}}", payload.SerializeToJson());

			ErrorReporter.Clear();
		}

		IDisposable SetTemporaryCurrentUser(BusinessObjectFactory factory)
		{
			var staff = factory.New<GlbStaff>();
			staff.GS_Code = ZString.Empty;
			return Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK));
		}

		IDisposable SetTemporaryCurrentCompany(BusinessObjectFactory factory)
		{
			var branch = factory.New<GlbBranch>();
			return Env.SetTemporaryUserContext(new UserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK));
		}

		#endregion

		public void TestDeleteContainer_NoDeveloperException_CantOpenForm()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = objectCreator.CreateShipment("S001001", consol);
			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
			Factory.Save();

			var container = AddContainerFromConsolForm(consol, shipment, controller);

			var loadedContainer = Factory.Load<ForwardingContainer>(container.PK);
			AssertNotNull(loadedContainer);

			using (var form = (ConsolForm)controller.ShowEditForm(consol))
			{
				var shipmentGrid = form.ConsolControl.ShipmentModuleButtonGrid;
				AssertNotNull("Shipment grid should exist.", shipmentGrid);
				shipmentGrid.InnerGrid.CopyCaptionsToPropertyHumanReadableNameForTest = true;
				var jobHeaderColumns = shipmentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(col => col.ColumnName.Contains("ShipmentJobHeader")).ToList();
				jobHeaderColumns.ForEach(x => x.IsVisible = true);
				form.Show();

				form.SelectContainersTabPage_ForTest();
				Application.DoEvents();

				var loadedConsol = (ForwardingConsol)form.BusinessEntity;
				var containerGrid = form.FindSingle<ZModuleButtonGrid>("JobContainerBoundGrid");

				var vgmTabPage = (ZTabPage)form.ConsolContainerControl.containersUserControl1.DetailTabControl.Controls.Find("VGMTabPage", true)[0];
				AssertNotNull(vgmTabPage);
				form.ConsolContainerControl.containersUserControl1.DetailTabControl.SelectedTab = vgmTabPage;

				container = (ForwardingContainer)containerGrid.InnerGrid.ListManager.Current;
				container.JC_ContainerNum = "CNTR002";
				Application.DoEvents();

				container.Delete();

				shipmentGrid.InnerGrid.Select(0);
				var toolStrip = (ZToolStrip)shipmentGrid.Controls.Find("toolStrip", true)[0];
				AssertNotNull("Tool Strip should exist.", toolStrip);
				var editButton = (ZToolStripButton)toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true)[0];
				AssertNotNull("Edit Button should exist.", editButton);
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoExceptionThrown(editButton.PerformClick);
				AssertEquals("The form must be saved before a Shipment can be edited or a new Shipment can be created. Do you wish to save the form?", UnitTestUserNotification.Instance.LastMessage.Text);

				using (var shipmentForm = shipmentGrid.LastShownZForm)
				{
					AssertNotNull(shipmentForm);
					AssertType(typeof(ShipmentForm), shipmentForm);
				}
			}
		}

		[UseSnapshotProtection]
		public void TestDeleteContainer_NoError_HasChangesShoulNotBeSetAfterTransactionIsCommitted()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = objectCreator.CreateShipment("S001001", consol);
			var controller = ZControllerFactory.Create(ControllerIDs.JobConsol);
			Factory.Save();

			var container = AddContainerFromConsolForm(consol, shipment, controller);

			var loadedContainer = Factory.Load<ForwardingContainer>(container.PK);
			AssertNotNull(loadedContainer);

			var errorReporter = new Mock<IErrorReporter>();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporter.Object))
			using (new DisposableAction(() => Globals.IsTest_ForTest.ResetValue()))
			using (var form = (ConsolForm)controller.ShowEditForm(consol))
			{
				ExceptionReporter.Instance.TestingDoReportException.Value = false;
				var shipmentGrid = form.ConsolControl.ShipmentModuleButtonGrid;
				AssertNotNull("Shipment grid should exist.", shipmentGrid);
				shipmentGrid.InnerGrid.CopyCaptionsToPropertyHumanReadableNameForTest = true;
				var jobHeaderColumns = shipmentGrid.ColumnStyles.Cast<ZGridColumnInfo>().Where(col => col.ColumnName.Contains("ShipmentJobHeader")).ToList();
				jobHeaderColumns.ForEach(x => x.IsVisible = true);
				form.Show();

				form.SelectContainersTabPage_ForTest();
				Application.DoEvents();

				var loadedConsol = (ForwardingConsol)form.BusinessEntity;
				var containerGrid = form.FindSingle<ZModuleButtonGrid>("JobContainerBoundGrid");

				var vgmTabPage = (ZTabPage)form.ConsolContainerControl.containersUserControl1.DetailTabControl.Controls.Find("VGMTabPage", true)[0];
				AssertNotNull(vgmTabPage);
				form.ConsolContainerControl.containersUserControl1.DetailTabControl.SelectedTab = vgmTabPage;

				container = (ForwardingContainer)containerGrid.InnerGrid.ListManager.Current;
				container.JC_ContainerNum = "CNTR002";
				Application.DoEvents();

				container.Delete();

				shipmentGrid.InnerGrid.Select(0);
				var toolStrip = (ZToolStrip)shipmentGrid.Controls.Find("toolStrip", true)[0];
				AssertNotNull("Tool Strip should exist.", toolStrip);
				var editButton = (ZToolStripButton)toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true)[0];
				AssertNotNull("Edit Button should exist.", editButton);
				Application.DoEvents();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var originalIsUserInteractive = Globals.IsUserInteractive;

				try
				{
					Globals.IsTest_ForTest.Value = false;
					Globals.IsUserInteractive = false;
					AssertNoExceptionThrown(editButton.PerformClick);
					AssertEquals(
						"The form must be saved before a Shipment can be edited or a new Shipment can be created. Do you wish to save the form?",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}

				finally
				{
					var key = "HasChangesShouldNotBeSetWhileTransactionIsCommiting_2";
					var errorMessage = "HasChanges should not be set after factory transaction is committed. Inform IL team";
					Globals.IsUserInteractive = originalIsUserInteractive;
					errorReporter.Verify(x => x.Report(key, errorMessage, It.IsAny<Exception>()), Times.Never);

					using (var shipmentForm = shipmentGrid.LastShownZForm)
					{
						AssertNotNull(shipmentForm);
						AssertType(typeof(ShipmentForm), shipmentForm);
					}
				}
			}
		}

		ForwardingContainer AddContainerFromConsolForm(ForwardingConsol consol, ForwardingShipment shipment, ZController controller)
		{
			ForwardingContainer container;
			var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();
			var seaAppointedAgentPortAU = sendingForwarder.AppointedAgentPorts.AddNew();
			seaAppointedAgentPortAU.O5_OA_AgentOfficeAddress = sendingForwarder.MainAddress.PK;
			seaAppointedAgentPortAU.O5_AgentDirection = AgentDirectionList.Codes.Both;
			seaAppointedAgentPortAU.O5_SeaAgentStatus = AgentStatusList.Codes.Published;
			seaAppointedAgentPortAU.O5_PortOrCountry = "AU";

			var refC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");

			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_MasterBillNum = "TEST001";
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_OA_SendingForwarderAddress_ZAddress.OrgPK = sendingForwarder.PK;
			consol.JK_CoLoadBookingReference = "Booking123456";
			consol.JK_CoLoadMasterBill = "MaterBill123456";
			consol.JK_OA_CreditorAddress = new ZGuid("2bf0030e-27eb-42b9-8936-5a76074e82da");
			Factory.Save();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = orgHeader.MainAddress;
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.TaxIDNumber;
			orgCusCode.OK_CustomsRegNo = "1234/AA/0111";
			orgCusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			orgCusCode.OK_OA_PremisesAddress = address.PK;
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VGMRegistrationNumber;
			Factory.Save();

			var objectCreator = new TestObjectCreator(Factory);
			var consignee = objectCreator.CreateOrgHeader("SIGNEE", false, false);
			consignee.OH_IsConsignee = true;
			var consignor = objectCreator.CreateOrgHeader("SIGNOR", false, false);
			consignor.OH_IsConsignor = true;

			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_ReleaseType = ShipmentReleaseTypes.OriginalReq;
			shipment.ConsignorPK = consignor.PK;
			shipment.ConsigneePK = consignee.PK;
			Factory.Save();

			using (var form = (ConsolForm)controller.ShowEditForm(consol))
			{
				form.SelectContainersTabPage_ForTest();
				Application.DoEvents();

				var loadedConsol = (ForwardingConsol)form.BusinessEntity;

				var containerGrid = form.FindSingle<ZModuleButtonGrid>("JobContainerBoundGrid");
				AssertEquals("There should be a container being edited", 1, containerGrid.InnerGrid.List.Count);

				container = (ForwardingContainer)containerGrid.InnerGrid.ListManager.Current;
				container.JC_ContainerNum = "CNTR001";
				container.JC_RC = refC.PK;
				container.JC_ContainerMode = Constants.ContainerModes.Groupage;
				container.JC_GrossWeightVerificationType = Constants.ContainerGrossWeightVerificationTypes.Codes.WeightAtTerminal;
				container.JC_GrossWeightVerificationDateTime = new ZDateTime(2022, 11, 18, 11, 50, 30);

				container.GrossWeightVerifiedByAddress.OrganisationPK = orgHeader.PK;
				container.GrossWeightVerifiedByAddress.E2_OA_Address = address.PK;

				containerGrid.InnerGrid.ListManager.EndCurrentEdit();
				Application.DoEvents();

				Assert(!container.IsInDatabase);

				var shipmentGrid = form.ConsolControl.ShipmentModuleButtonGrid;
				shipmentGrid.InnerGrid.Select(0);
				Application.DoEvents();

				UnitTestUserNotification.Instance.AddOKAnswer();
				var didSave = form.FireSaveButton();
				Application.DoEvents();

				AssertNoErrors("Errors on the container will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", container);
				AssertNoErrors("Errors on the consol will stop the form from saving and links from being created, so fix up any errors listed here please (warnings are okay):", loadedConsol);
				AssertEquals("The save should have been successful.", ContinueWithSave.Yes, didSave);
				Assert("The container should be saved", container.IsInDatabase);
			}

			return container;
		}

		public void TestConsol_WithTemplateRecordCopied_ButConsolIsNotTemplateRecord_WithShipmentAttached_ShouldNotBeDeactivated_ByUserAction()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "DEHAM";
			consol.IsTemplateRecord = true;

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;
			templateRecord.STR_ReferenceId = "TR0000001";

			var templateRecordProvider1 = consol as ITemplateRecordProvider;
			templateRecordProvider1.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				templateRecordProvider1.SaveToTemplateRecord();
			}

			Factory.Save();

			var copiedConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			copiedConsol.JK_STR = consol.JK_STR;
			copiedConsol.JK_UniqueConsignRef = "CONSOL123";
			Factory.Save();

			Assert(!copiedConsol.IsTemplateRecord);
			AssertNotNull(copiedConsol.TemplateRecord);

			var shipment = copiedConsol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "DEHAM";

			Factory.Save();

			using (var form = new ConsolForm(copiedConsol))
			{
				form.Show();

				var actionMenuItemsProvider = (IFileMenuItemsProvider)form;
				var actionsMenuItem = actionMenuItemsProvider.ActionsMenuItem;
				var makeInactive = actionsMenuItem.MenuItems.FindByText("Make Inactive");

				UnitTestUserNotification.Instance.ClearMessages();
				makeInactive.PerformClick();
				Application.DoEvents();

				var msg = UnitTestUserNotification.Instance.LastMessage;

				CombineAssertions(() =>
				{
					AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.WasNone);
					AssertNotNull("Should prompt user if shipment attached to consol", msg);
					AssertEquals("Cannot cancel/deactivate", msg.Caption);
					AssertEquals($"You cannot deactivate Consol {copiedConsol.JK_UniqueConsignRef} since it has shipments attached.", msg.Text);
				});
			}
		}

		#region Implementation

		SecurityCore GetTemporarySecurityCore()
		{
			return new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}

		IForwardingConsolDocumentSupporterQueryProvider GetQueryProvider(ForwardingConsol consol)
		{
			return typeof(ForwardingConsolDocumentSupporter).GetProperty("QueryProvider", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).GetValue(consol.DocumentSupporter, null) as IForwardingConsolDocumentSupporterQueryProvider;
		}

		const string WhatAreYouDoingYouPermissionlessFool =
			"The aggregate values of merchandise within shipment {0} that are covered by any single HTS number exceed $2500 and Customs Entry Number is not entered.\r\n" +
			"\r\n" +
			"You do not have the appropriate security rights to continue running this document.\r\n" +
			"\r\n" +
			"If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: \r\n" +
			"\r\n" +
			"Operations -> Forwarding -> Shipments -> US Specifics -> Allow Printing of AWB/HBL if No Export Declaration Is Filed";

		const string YouThinkYouHavePermissionForAnythingMrBigShot =
			"The aggregate values of merchandise within shipment {0} that are covered by any single HTS number exceed $2500 and Customs Entry Number is not entered but you have the necessary security access to continue running this document.\r\n" +
			"\r\n" +
			"Are you sure you want to run this document?";

		void AssertContainsPlugIn(ConsolForm form, ControllerID controllerID)
		{
			AssertContainsPlugIn(form, controllerID, null, "");
		}

		void AssertContainsPlugIn(ConsolForm form, ControllerID controllerID, ZString countryCode)
		{
			AssertContainsPlugIn(form, controllerID, null, countryCode);
		}

		void AssertContainsPlugIn(ConsolForm form, ControllerID controllerID, SecurityCheckpoint expectedSecurityCheckpoint)
		{
			AssertContainsPlugIn(form, controllerID, expectedSecurityCheckpoint, "");
		}

		void AssertContainsPlugIn(ConsolForm form, ControllerID controllerID, SecurityCheckpoint expectedSecurityCheckpoint, ZString countryCode)
		{
			if (countryCode.IsEmpty)
			{
				GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.Australia);
			}
			else
			{
				GlbCompany.CurrentCompany.SetCountry(countryCode);
			}
			var plugIn = form.PlugIns.GetPlugIn(controllerID);
			AssertNotNull("Should be plugged in", plugIn);
			if (expectedSecurityCheckpoint != null)
			{
				AssertEquals("Incorrect security checkpoint", expectedSecurityCheckpoint, plugIn.SecurityCheckpoint);
			}
		}

		void AssertContainsPlugIn(ZTabControl tab, ControllerID controllerID)
		{
			var plugIn = tab.PlugIns.GetPlugIn(controllerID);
			AssertNotNull("Should be plugged in", plugIn);
		}

		void AssertDoesNotContainPlugin(ConsolForm form, ControllerID controllerID)
		{
			var plugIn = form.PlugIns.GetPlugIn(controllerID);
			Assert("Should not be plugged in", plugIn == null || !plugIn.Enabled);
		}

		void ClickImportBookingsMenuItem(ConsolForm form)
		{
			var field = typeof(ConsolForm).GetField("ImportBookingsMenuItem",
				BindingFlags.NonPublic | BindingFlags.Instance);

			var importBookingsMenuItem = (MenuItem)field.GetValue(form);
			importBookingsMenuItem.PerformClick();
		}

		void ClickExportToOverseasAgentMenuItem(ConsolForm form)
		{
			foreach (MenuItem item1 in form.Menu.MenuItems)
			{
				if (item1.Text == "Actio&ns")
				{
					foreach (MenuItem item2 in item1.MenuItems)
					{
						if (item2.Text == ExportXmlMenuItemHelper.VerboseMenuItemText)
						{
							foreach (MenuItem item3 in item2.MenuItems)
							{
								if (item3.Text == "Export to Overseas Agent")
								{
									item3.PerformClick();
									break;
								}
							}
							break;
						}
					}
					break;
				}
			}
		}

		ForwardingConsol CreateMAWBWithConsol()
		{
			var mawb = AddMawb("081", "55555625", GlbBranch.CurrentBranch, "STD");

			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AWBServiceLevel = "STD";
			consol.JK_RL_NKLoadPort = HomePort;
			consol.JK_RL_NKDischargePort = OverseasPort;
			consol.MasterBillAirlinePrefix = "081";

			consol.JK_IsNeutralMaster = true;
			consol.Factory.Save();
			AssertEquals("08155555625", consol.JK_MasterBillNum);
			mawb.Reload();
			mawb.JM_IsPrinted = true;
			Factory.Save();
			AssertEquals(true, consol.IsNeutralMAWBPrinted);
			AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

			return consol;
		}

		JobMawb AddMawb(string prefix, string mawbNo, GlbBranch branch, string serviceLevel)
		{
			JobMawb mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_GB = branch.PK;
			mawb.JM_ServiceLevel = serviceLevel;

			return mawb;
		}

		JobSailing fSailing;
		JobSailing Sailing
		{
			get
			{
				if (fSailing == null)
				{
					var voyage1 = Factory.New<JobVoyage>();
					voyage1.JV_RV_NKVessel = RefVessel.LookupVesselByName("APL IVORY", Factory).First().RV_FK;
					voyage1.JV_VoyageFlight = "2098372";
					var o1 = voyage1.Origins.AddNew();
					o1.JA_RL_NKPortOfLoading = "HKHKG";
					o1.JA_E_DEP = ZDateTime.Today;
					var d1 = voyage1.Destinations.AddNew();
					d1.JB_RL_NKPortOfDischarge = "SGSIN";
					d1.JB_E_ARV = ZDateTime.Today.AddDays(3);

					fSailing = voyage1.Sailings[0];
				}
				return fSailing;
			}
		}

		ForwardingConsol Consol;
		ConsolFormTestClass GetNewZConsolForm()
		{
			Consol = Factory.New<ForwardingConsol>();
			return new ConsolFormTestClass(Consol);
		}

		static MenuItem FindMenuItem(Form form, params string[] path)
		{
			return FindMenuItemCore(form.Menu.MenuItems, path, 0);
		}

		static MenuItem FindMenuItemOrNull(Form form, params string[] path)
		{
			return FindMenuItemCore(form.Menu.MenuItems, path, 0, true);
		}

		static MenuItem FindMenuItemCore(Menu.MenuItemCollection items, string[] path, int index, bool returnNullIfNotFound = false)
		{
			MenuItem next = null;

			foreach (MenuItem item in items)
			{
				var text = item.Text.Replace("&", "");
				if (text == path[index])
				{
					next = item;
					break;
				}
			}

			if (next == null)
			{
				if (returnNullIfNotFound)
				{
					return null;
				}

				throw new ArgumentException(string.Format("cant find '{0}'", path[index]));
			}

			return (index + 1 == path.Length) ? next : FindMenuItemCore(next.MenuItems, path, index + 1, returnNullIfNotFound);
		}

		ForwardingConsol GetNewTemplateConsol()
		{
			var factory = new TemplateRecordBusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();

			var templateRecord = factory.TemplateRecordFactory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = ModuleIDs.JobConsol.Name;

			factory.TemplateRecordProvider = consol;
			factory.TemplateRecordProvider.IsTemplateRecord = true;
			factory.TemplateRecordProvider.TemplateRecord = templateRecord;

			consol.JK_MasterBillNum = "ABC";
			consol.Factory.Save();

			return consol;
		}

		class ConsolFormForTest : ConsolForm, ISupportSwitchTabPage
		{
			public ConsolFormForTest(ForwardingConsol businessEntity) : base(businessEntity)
			{
			}

			public ZTabControl MainTabControl_ForTest => base.MainTabControl;

			public new ZTabPage AWBTabPage
			{
				get { return base.AWBTabPage; }
			}

			public void SwitchTabPage(string tabPageName)
			{
				var tabPage = MainTabControl.GetTabPage(tabPageName);
				if (tabPage != null)
				{
					MainTabControl.SelectedTab = tabPage;
				}
			}
		}

		class ShipmentFormForTest : ZForm
		{
			public ShipmentFormForTest(ForwardingShipment businessEntity)
				: base(businessEntity)
			{
			}

			public ConsolFormUserControlForTest ConsolUserControl;

			protected override void InitializeComponent()
			{
				base.InitializeComponent();
				this.BindingSource.DataSourceType = typeof(ForwardingShipment);
				ConsolUserControl = new ConsolFormUserControlForTest();
				ConsolUserControl.Dock = DockStyle.Fill;
				Controls.Add(ConsolUserControl);
				BindingSource.SetBindingMember(this.ConsolUserControl, ".");
			}
		}

		class ConsolFormUserControlForTest : ZUserControl
		{
			public readonly ConsolModuleButtonGridForTest ConsolModuleButtonGrid;

			public ConsolFormUserControlForTest()
			{
				this.BindingSource.DataSourceType = typeof(ForwardingShipment);
				ConsolModuleButtonGrid = new ConsolModuleButtonGridForTest();
				ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo
				{
					ColumnName = "JK_UniqueConsignRef"
				};
				ConsolModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
				ConsolModuleButtonGrid.AlwaysRequiresSaveBeforeEdit = true;
				BindingSource.SetBindingMember(this.ConsolModuleButtonGrid, "Consols");
				ConsolModuleButtonGrid.BindToFindBoxList = "Lookups+Consols_List";
				ConsolModuleButtonGrid.Dock = DockStyle.Fill;
				ConsolModuleButtonGrid.InnerGrid.ReadOnly = true;

				Controls.Add(this.ConsolModuleButtonGrid);
			}
		}

		class ConsolModuleButtonGridForTest : ConsolModuleButtonGrid
		{
			public ZToolStripButton EditButton
			{
				get
				{
					var toolStrip = Controls.Find("toolStrip", true).OfType<ZToolStrip>().First();
					return toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Edit, true).OfType<ZToolStripButton>().First();
				}
			}

			public ConsolModuleButtonGridAttacher Attacher { get; set; }

			protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			{
				return Attacher ?? (Attacher = new ConsolModuleButtonGridAttacher(this.ParentShipment, destinationCollection, findBoxList, moduleID));
			}
		}

		bool rawEnableComplianceRisk;
		EnableComplianceWiseRegistryBusinessObject rawFreightComplianceWiseRegistry;

		protected override void SetUp()
		{
			base.SetUp();
			rawEnableComplianceRisk = RawDataRegistry.Instance.EnableComplianceRisk.Value;
			rawFreightComplianceWiseRegistry = FreightDataRegistry.Instance.FreightEnableComplianceWise.DefaultValue;

			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.SetValue(false));
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			RawDataRegistry.Instance.EnableComplianceRisk.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawEnableComplianceRisk);
			FreightDataRegistry.Instance.FreightEnableComplianceWise.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rawFreightComplianceWiseRegistry);
		}

		#endregion
	}
}
