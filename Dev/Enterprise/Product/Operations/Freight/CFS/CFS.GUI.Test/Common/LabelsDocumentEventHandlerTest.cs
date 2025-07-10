using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.CFS.GUI.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	sealed class LabelsDocumentEventHandlerTest : TestCaseWithFactory
	{
		public void TestCanHandleMenuItem()
		{
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			var handler = new LabelsDocumentEventHandler();

			menuItem.SU_MenuName = "blah Label";
			AssertEquals("handler.CanHandleMenuItem(menuItem)", false, handler.CanHandleMenuItem(menuItem));

			menuItem.SU_MenuName = "Import Label";
			AssertEquals("handler.CanHandleMenuItem(menuItem)", true, handler.CanHandleMenuItem(menuItem));

			menuItem.SU_MenuName = "On Forwarding Label";
			AssertEquals("handler.CanHandleMenuItem(menuItem)", true, handler.CanHandleMenuItem(menuItem));

			menuItem.SU_MenuName = "Transhipment Label";
			AssertEquals("handler.CanHandleMenuItem(menuItem)", true, handler.CanHandleMenuItem(menuItem));
		}

		public void TestHandleDocumentPrintRequestedForShipment()
		{
			var shipment = Factory.NewWithValidTestData<CFSShipment>();
			shipment.JS_RL_NKDestination = "AUSYD";

			var consol = shipment.Consols.AddNew();
			consol.JK_RL_NKDischargePort = "USCHI";

			var documentSupporter = new CFSShipmentDocumentSupporter(shipment);
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			var args = new DocumentCancelEventArgs(menuItem);
			var handler = new LabelsDocumentEventHandler();
			handler.DocumentSupporter = documentSupporter;

			args.Cancel = false;
			menuItem.SU_MenuName = LabelsName.ImportLabel;

			handler.HandleDocumentPrintRequested(this, args);

			AssertEquals("args.Cancel", true, args.Cancel);
			AssertEquals("lastTitle", "Import Labels", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("lastMessage", "This is not an Import shipment with at least one pack line.", UnitTestUserNotification.Instance.LastMessage.Text);

			consol.JK_RL_NKDischargePort = "AUSYD";

			handler.DocumentSupporter = documentSupporter;
			args.Cancel = false;
			menuItem.SU_MenuName = "On Forwarding Label";

			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals("args.Cancel", true, args.Cancel);
			AssertEquals("lastTitle", "On Forwarding Labels", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("lastMessage", "This is not an On Forwarding shipment with at least one pack line.", UnitTestUserNotification.Instance.LastMessage.Text);

			consol.JK_RL_NKDischargePort = "AUBNE";

			handler.DocumentSupporter = documentSupporter;
			args.Cancel = false;
			menuItem.SU_MenuName = "Transhipment Label";

			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals("args.Cancel", true, args.Cancel);
			AssertEquals("lastTitle", "Transhipment Labels", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("lastMessage", "This is not a Transhipment with at least one pack line.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestHandleDocumentPrintRequestedForConsol()
		{
			var consol = Factory.NewWithValidTestData<CFSLoadListConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = "USCHI";

			var documentSupporter = new CFSLoadListConsolDocumentSupporter(consol);
			var menuItem = Factory.NewWithValidTestData<StmMenuItem>();
			var args = new DocumentCancelEventArgs(menuItem);
			var handler = new LabelsDocumentEventHandler();
			handler.DocumentSupporter = documentSupporter;

			args.Cancel = false;
			menuItem.SU_MenuName = LabelsName.ImportLabel;

			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals("args.Cancel", true, args.Cancel);
			AssertEquals("lastTitle", "Import Labels", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("lastMessage", "This Load List does not have any Import shipments with at least one pack line.", UnitTestUserNotification.Instance.LastMessage.Text);

			shipment.JS_RL_NKDestination = "AUSYD";

			handler.DocumentSupporter = documentSupporter;
			args.Cancel = false;
			menuItem.SU_MenuName = "On Forwarding Label";

			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals("args.Cancel", true, args.Cancel);
			AssertEquals("lastTitle", "On Forwarding Labels", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("lastMessage", "This Load List does not have any On Forwarding shipments with at least one pack line.", UnitTestUserNotification.Instance.LastMessage.Text);

			consol.JK_RL_NKDischargePort = "AUBNE";

			handler.DocumentSupporter = documentSupporter;
			args.Cancel = false;
			menuItem.SU_MenuName = "Transhipment Label";

			handler.HandleDocumentPrintRequested(this, args);
			AssertEquals("args.Cancel", true, args.Cancel);
			AssertEquals("lastTitle", "Transhipment Labels", UnitTestUserNotification.Instance.LastMessage.Caption);
			AssertEquals("lastMessage", "This Load List does not have any Transhipment shipments with at least one pack line.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
