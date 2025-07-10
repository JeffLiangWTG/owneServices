using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	public class AWBBarcodeLabelDocumentEventsHandlerTest : TestCaseWithFactory
	{
		public void TestCanHandleMenuItem()
		{
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			var handler = new AWBBarcodeLabelDocumentEventsHandler();

			menuItem.SU_MenuName = "Not an AWB Label";
			AssertEquals("handler.CanHandleMenuItem(menuItem)", false, handler.CanHandleMenuItem(menuItem));

			menuItem.SU_MenuName = "AWB Barcode Label";
			AssertEquals("handler.CanHandleMenuItem(menuItem)", true, handler.CanHandleMenuItem(menuItem));
		}

		public void TestHandleDocumentPrintRequested()
		{
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			ForwardingShipmentDocumentSupporter documentSupporter = new ForwardingShipmentDocumentSupporter(shipment);
			StmMenuItem menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			DocumentCancelEventArgs args = new DocumentCancelEventArgs(menuItem);
			AWBBarcodeLabelDocumentEventsHandler handler = new AWBBarcodeLabelDocumentEventsHandler();
			handler.DocumentSupporter = documentSupporter;

			args.Cancel = false;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OuterPacks = 0;

			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals("args.Cancel", true, args.Cancel);
			AssertEquals("lastTitle", "AWB Barcode Label Printing", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("lastMessage", "No labels will be printed.\r\nLabels will only be printed if this Shipment has Outer Packs.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			bool originalIsInterative = Globals.IsUserInteractive;
			using (new DisposableAction(() => Globals.IsUserInteractive = originalIsInterative))
			{
				Globals.IsUserInteractive = false;
				handler.HandleDocumentPrintRequested(this, args);
			}
			AssertEquals("args.Cancel", true, args.Cancel);
			AssertEquals("lastTitle", null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("lastMessage", null, UnitTestUserNotification.Instance.LastMessage.Caption);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			args.Cancel = false;
			shipment.JS_OuterPacks = 10;
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals("args.Cancel", true, args.Cancel);
			AssertEquals("lastTitle", "AWB Barcode Label Printing", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("lastMessage", "No labels will be printed.\r\nLabels will only be printed if this Shipment has departure Consolidation attached with 'Air' transport mode.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			args.Cancel = false;
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.Shipments.Add(shipment);
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals("args.Cancel", true, args.Cancel);
			AssertEquals("lastTitle", "AWB Barcode Label Printing", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("lastMessage", "No labels will be printed.\r\nLabels will only be printed if this Shipment has departure Consolidation attached with 'Air' transport mode.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			args.Cancel = false;
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals("args.Cancel", true, args.Cancel);
			AssertEquals("lastTitle", null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("lastMessage", null, UnitTestUserNotification.Instance.LastMessage.Caption);
		}

		public void TestShipmentAWBActionsConsolSet()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_OuterPacks = 5;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			AssertEquals("Prerequisite", consol, shipment.CurrentBranchDepartureAirConsol);

			var documentSupporter = new ForwardingShipmentDocumentSupporter(shipment);
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			var args = new DocumentCancelEventArgs(menuItem);
			var handler = new AWBBarcodeLabelDocumentEventsHandler();
			handler.DocumentSupporter = documentSupporter;
			handler.HandleDocumentPrintRequested(this, args);

			AssertNull(ZFormModaliser.LastFormShownForTest);

			handler.HandleDocumentPrintRequested(this, args);

			AssertEquals(consol.PK, (ZFormModaliser.LastIBusinessShownOnDialogForTest as ShipmentAWBActions).LabelConsol);
		}
	}
}
