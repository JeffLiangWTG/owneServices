using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class GatePassShipmentSaveAndPrintUITest : TestCaseWithFactory
	{
		public void TestGuiFactoryServices()
		{
			GatePassShipment gatePassShipment = Factory.New<GatePassShipment>();

			ICommonShipmentDocumentSupporterQueryProvider shipmentDocSupporterQueryProvider = Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
			AssertNull("shipment doc supporter", shipmentDocSupporterQueryProvider);

			IServicesSelectionProvider servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
			AssertNull("services selection provider", servicesSelectionProvider);

			using (new ShipmentGatePassForm(gatePassShipment))
			{
				shipmentDocSupporterQueryProvider = Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
				AssertNotNull("shipment doc supporter", shipmentDocSupporterQueryProvider);
				Assert(shipmentDocSupporterQueryProvider is ShipmentDocumentSupporterGuiQueryProvider);

				servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull("services selection provider", servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		public void TestAsk()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();

			using (ShipmentGatePassForm form = new ShipmentGatePassForm(shipment))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				bool result = ((ISaveAndPrintUI)form).Ask("Foo?");
				AssertEquals(true, result);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				result = ((ISaveAndPrintUI)form).Ask("Foo?");
				AssertEquals(false, result);

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Foo?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowError()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();

			using (ShipmentGatePassForm form = new ShipmentGatePassForm(shipment))
			{
				((ISaveAndPrintUI)form).ShowError("Bleh");
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Bleh", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowWarning()
		{
			GatePassShipment shipment = Factory.New<GatePassShipment>();

			using (ShipmentGatePassForm form = new ShipmentGatePassForm(shipment))
			{
				((ISaveAndPrintUI)form).ShowWarning("Nobody expects the spanish inquisition");
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("Nobody expects the spanish inquisition", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
