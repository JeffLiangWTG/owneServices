using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	[TestedType(typeof(ShipmentLabelRangeForm))]
	public class ShipmentLabelRangeFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestPrintLabels()
		{
			var printer = Factory.New<StmPrintQueue>();
			printer.SQ_DisplayName = "DisplayName";
			printer.SQ_QueueName = "QueueName";
			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "SGSIN";
			shipment.JS_HouseBill = "08112345678";
			shipment.JS_OuterPacks = 7;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var actions = new ShipmentAWBActions(shipment, AWBActions.ActionsModeType.LabelsOnly);

			AssertEquals(consol.PK, actions.LabelConsol);

			using (var form = new ShipmentLabelRangeForm(actions))
			{
				form.Show();
				Factory.SetValue<IForwardingShipmentDocumentSupporterQueryProvider, ForwardingShipmentDocumentSupporterGuiQueryProvider>();
				form.AcceptButton.PerformClick();

				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				actions.LabelPrinter = printer.PK;

				form.AcceptButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			AssertEquals(1, Factory.Load<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_SQ, printer.PK)).Length);
		}

		protected override Form GetFormToBashCore()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			var actions = new ShipmentAWBActions(shipment, AWBActions.ActionsModeType.LabelsOnly);

			return new ShipmentLabelRangeForm(actions);
		}
	}
}
