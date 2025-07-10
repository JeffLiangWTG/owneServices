using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class GatePassShipmentFormTest : BaseFreightTest
	{
		public void TestWorkflowTabHiddenIfForwardRegistered()
		{
			var workflowTabPageField = typeof(ShipmentGatePassForm).GetField("WorkflowTabPage", BindingFlags.NonPublic | BindingFlags.Instance);

			var shipment = Factory.New<GatePassShipment>();
			shipment.JS_IsForwardRegistered = true;

			using (var form = new ShipmentGatePassForm(shipment))
			{
				var tabPage = (ZWorkflowTabPage)workflowTabPageField.GetValue(form);
				Assert("Should not be visible", !tabPage.TabVisible);
				Assert("Should not be initialized", !((IWorkflowTabPage)tabPage).Initialized);

				shipment.JS_IsForwardRegistered = false;
				form.FireValidateAllForTest();

				UserIdleWorker.Flush();
				Assert("Should be visible", tabPage.TabVisible);
			}

			shipment.JS_IsForwardRegistered = false;

			using (var form = new ShipmentGatePassForm(shipment))
			{
				var tabPage = (ZWorkflowTabPage)workflowTabPageField.GetValue(form);
				Assert("Should be visible", tabPage.TabVisible);
				Assert("Should be initialized", ((IWorkflowTabPage)tabPage).Initialized);

				shipment.JS_IsForwardRegistered = true;
				Assert("Should not be visible", !tabPage.TabVisible);
			}
		}

		public void TestDeliveringNonCustomsControlledGoods()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GatePassLoadListConsol loadList = Factory.New<GatePassLoadListConsol>();
			GatePassContainer container = loadList.Containers.AddNew();

			Transport transport = loadList.Transports[0];
			transport.JW_JX = ImportSailing1.PK;

			GatePassShipment createdShipment = loadList.Shipments.AddNew();
			createdShipment.OuterPackLines.AddNew();
			createdShipment.OuterPackLines[0].JL_PackageCount = 10;
			createdShipment.OuterPackLines[0].JL_Outturn = 10;
			Factory.Save();

			using (ShipmentGatePassForm testForm = new ShipmentGatePassForm(createdShipment))
			{
				testForm.Show();
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertEquals("Should have added the delivery line", 1, createdShipment.DestinationCFSDepartures.Count);
				AssertNull("No Message Should have been shown", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeliveringShipmentWithIncompleteServices()
		{
			GatePassShipment shipment = GetShipmentWithServices(ZDateTime.Empty);

			using (ShipmentGatePassForm testForm = new ShipmentGatePassForm(shipment))
			{
				testForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertEquals("Dialogue box should be displayed", "There are still incomplete services on this shipment.\r\n Do you still want to deliver the shipment?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Delivery should not continue if 'NO' was selected", 0, shipment.DestinationCFSDepartures.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertEquals("Dialogue box should be displayed", "There are still incomplete services on this shipment.\r\n Do you still want to deliver the shipment?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Delivery should continue if 'YES' was selected", 1, shipment.DestinationCFSDepartures.Count);
			}
		}

		public void TestDeliveringShipmentWithCompletedServices()
		{
			GatePassShipment shipment = GetShipmentWithServices(ZDateTime.Now);

			using (ShipmentGatePassForm testForm = new ShipmentGatePassForm(shipment))
			{
				testForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertNull("No dialogue box should be displayed", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		GatePassShipment GetShipmentWithServices(ZDateTime completedDate)
		{
			GatePassLoadListConsol loadList = Factory.New<GatePassLoadListConsol>();
			GatePassContainer container = loadList.Containers.AddNew();

			Transport transport = loadList.Transports[0];
			transport.JW_JX = ImportSailing1.PK;

			GatePassShipment shipment = loadList.Shipments.AddNew();
			JobService fumigation = shipment.DocsAndCartage.Services.AddNew();
			fumigation.ES_ServiceCode = Constants.FreightServiceType.Codes.Fumigation;
			fumigation.ES_Completed = completedDate;

			GatePassPackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.JL_PackageCount = 29;
			packLine.JL_Outturn = 29;

			Factory.Save();

			return shipment;
		}

		public void TestJobDocsAndCartage_IsContingencyReleaseChecked()
		{
			GatePassLoadListConsol loadList = Factory.New<GatePassLoadListConsol>();
			GatePassContainer container = loadList.Containers.AddNew();

			Transport transport = loadList.Transports[0];
			transport.JW_JX = ImportSailing1.PK;

			GatePassShipment shipment = loadList.Shipments.AddNew();

			using (ShipmentGatePassForm testForm = new ShipmentGatePassForm(shipment))
			{
				testForm.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				shipment.DocsAndCartage.JP_IsContingencyRelease = true;
				AssertEquals("In manually changing", UnitTestUserNotification.Instance.LastMessage.Text.Substring(0, 20));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				shipment.DocsAndCartage.JP_IsContingencyRelease = false;
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestDeliveringNonClearGoods()
		{
			FreightDataRegistry.Instance.DefaultShipmentDestinationFromConsolDischarge.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			FreightDataRegistry.Instance.DefaultShipmentOriginFromConsolLoad.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GatePassLoadListConsol loadList = Factory.New<GatePassLoadListConsol>();
			GatePassContainer container = loadList.Containers.AddNew();
			EDIMessage containerMessage = EDIMessageTestFactory.New(Factory);
			containerMessage.EM_ApplicationCode = "SCA";
			containerMessage.EM_LinkTable = container.TableName;
			containerMessage.EM_LinkUniqueID = container.PK;
			containerMessage.EM_MessageType = "IMP";
			containerMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			container.Logs.AddNew(Events.SeaCargoDepotEvent, "IMPENDING ARRIVAL XXXX");

			Transport transport = loadList.Transports[0];
			transport.JW_JX = ImportSailing1.PK;

			loadList.JK_RL_NKLoadPort = transport.JW_RL_NKLoadPort;
			loadList.JK_RL_NKDischargePort = transport.JW_RL_NKDiscPort;

			GatePassShipment createdShipment = loadList.Shipments.AddNew();
			createdShipment.OuterPackLines.AddNew();
			createdShipment.OuterPackLines[0].JL_PackageCount = 10;
			createdShipment.OuterPackLines[0].JL_Outturn = 10;
			createdShipment.OuterPackLines[0].SetContainer(container.PK);
			Factory.Save();

			using (ShipmentGatePassForm testForm = new ShipmentGatePassForm(createdShipment))
			{
				testForm.Show();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertEquals("Delivery Should not be added", 0, createdShipment.DestinationCFSDepartures.Count);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertEquals("Delivery Should not be added", 0, createdShipment.DestinationCFSDepartures.Count);
				createdShipment.DocsAndCartage.JP_IsContingencyRelease = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertEquals("Delivery Should not be added", 0, createdShipment.DestinationCFSDepartures.Count);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertEquals("Should have added the delivery line", 1, createdShipment.DestinationCFSDepartures.Count);
			}
		}

		public void TestDeliveringClearedGoods()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GatePassLoadListConsol loadList = Factory.New<GatePassLoadListConsol>();
			GatePassContainer container = loadList.Containers.AddNew();

			Transport transport = loadList.Transports[0];
			transport.JW_JX = ImportSailing1.PK;

			GatePassShipment createdShipment = loadList.Shipments.AddNew();
			EDIMessage cargoStatusAdviceMessage = EDIMessageTestFactory.New(Factory);
			cargoStatusAdviceMessage.EM_ApplicationCode = "SCA";
			cargoStatusAdviceMessage.EM_LinkTable = container.TableName;
			cargoStatusAdviceMessage.EM_LinkUniqueID = createdShipment.PK;
			cargoStatusAdviceMessage.EM_MessageType = "CSA";
			cargoStatusAdviceMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			createdShipment.OuterPackLines.AddNew();
			createdShipment.Logs.AddNew(Events.SeaCargoDepotEvent, "CLEAR 39392");
			createdShipment.OuterPackLines[0].JL_PackageCount = 10;
			createdShipment.OuterPackLines[0].JL_Outturn = 10;
			Factory.Save();

			using (ShipmentGatePassForm testForm = new ShipmentGatePassForm(createdShipment))
			{
				testForm.Show();
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertEquals("Should have added the delivery line", 1, createdShipment.DestinationCFSDepartures.Count);
			}
		}

		public void TestActionsMenuItemsNotAvailableInViewMode()
		{
			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(new ShipmentGatePassForm(Factory.New<GatePassShipment>()));
		}

		public void TestShouldNotDeliverForCLDShipment()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var loadList = Factory.New<GatePassLoadListConsol>();
			var container = loadList.Containers.AddNew();

			var shipment = loadList.Shipments.AddNew();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			shipment.OuterPackLines.AddNew();
			shipment.OuterPackLines[0].JL_PackageCount = 10;
			shipment.OuterPackLines[0].JL_Outturn = 10;

			Factory.Save();

			using (var testForm = new ShipmentGatePassForm(shipment))
			{
				testForm.Show();
				testForm.GatePassDetailsUserControl.DeliverButton.PerformClick();
				AssertEquals("Should Not add the delivery line for ColoadMaster shipment", 0, shipment.DestinationCFSDepartures.Count);
				AssertEquals("Co-Load Master Shipments have no packlines – Gate Pass must be done from the linked sub Shipments", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
