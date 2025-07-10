using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class FreightRatesControlTest : TestCaseWithFactory
	{
		public void TestMoveButtons()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var gateway1 = shipment.Gateways.AddNew();
			var gateway2 = shipment.Gateways.AddNew();
			var gateway3 = shipment.Gateways.AddNew();

			var org1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "A"));
			var org2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B"));
			var org3 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "C"));

			gateway1.JSG_OA_ForwarderAddress = org1.MainAddress.PK;
			gateway2.JSG_OA_ForwarderAddress = org2.MainAddress.PK;
			gateway3.JSG_OA_ForwarderAddress = org3.MainAddress.PK;

			using (ZForm form = new ZForm(shipment))
			{
				var freightRatesControl = new FreightRatesControl();
				form.Controls.Add(freightRatesControl);
				freightRatesControl.SetDataBinding(shipment, "");

				form.Show();

				var gatewaysGroupBox = freightRatesControl.Controls["GatewaysGroupBox"];
				var gatewaysGrid = gatewaysGroupBox.Controls["GatewaysGrid"] as ZGrid;
				var gatewaysMovementPanel = gatewaysGroupBox.Controls["GatewaysMovementPanel"];
				var moveUpButton = gatewaysMovementPanel.Controls["MoveUpButton"] as ZButton;
				var moveDownButton = gatewaysMovementPanel.Controls["MoveDownButton"] as ZButton;

				UnitTestUserNotification.Instance.ClearMessages();
				moveUpButton.PerformClick();
				AssertEquals("Please select a Gateway to move from the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				moveDownButton.PerformClick();
				AssertEquals("Please select a Gateway to move from the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				gatewaysGrid.Select(0);
				gatewaysGrid.Select(1);

				UnitTestUserNotification.Instance.ClearMessages();
				moveUpButton.PerformClick();
				AssertEquals("Please select only one Gateway to move in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				moveDownButton.PerformClick();
				AssertEquals("Please select only one Gateway to move in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				gatewaysGrid.UnSelectAll();
				gatewaysGrid.Select(1);

				UnitTestUserNotification.Instance.ClearMessages();
				moveUpButton.PerformClick();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Top row is selected", gatewaysGrid.IsSelected(0));
				AssertEquals("Org2 is on top", org2.OH_Code, ((ShipmentGateway)gatewaysGrid.SelectedElements.Single()).Forwarder.OH_Code);

				gatewaysGrid.UnSelectAll();
				gatewaysGrid.Select(1);

				moveDownButton.PerformClick();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Bottom row is selected", gatewaysGrid.IsSelected(2));
				AssertEquals("Org1 is at the bottom", org1.OH_Code, ((ShipmentGateway)gatewaysGrid.SelectedElements.Single()).Forwarder.OH_Code);
			}
		}

		public void TestMoveButtonHasFormBasherTestPopupExcludeAttribute()
		{
			using (var control = new FreightRatesControl())
			{
				var upButton = control.Controls["GatewaysGroupBox"].Controls["GatewaysMovementPanel"].Controls["MoveUpButton"];
				var downButton = control.Controls["GatewaysGroupBox"].Controls["GatewaysMovementPanel"].Controls["MoveDownButton"];
				AssertEquals("Form Basher Test can't cope with the up and down arrows used on the ZButton control", 1, upButton.GetType().GetCustomAttributes(typeof(FormBasherTestPopupExcludeAttribute), false).Length);
				AssertEquals("Form Basher Test can't cope with the up and down arrows used on the ZButton control", 1, downButton.GetType().GetCustomAttributes(typeof(FormBasherTestPopupExcludeAttribute), false).Length);
			}
		}

		[ExpectNoExceptions]
		public void TestShipmentGateway_WhenButtonClick_ShouldNotThrowIndexOutOfBoundsException()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (ZForm form = new ZForm(shipment))
			{
				var freightRatesControl = new FreightRatesControl();
				form.Controls.Add(freightRatesControl);
				freightRatesControl.SetDataBinding(shipment, "");

				form.Show();

				var gatewaysGrid = freightRatesControl.Controls.Find("GatewaysGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull(gatewaysGrid);
				gatewaysGrid.Select(0);

				var moveDownButton = freightRatesControl.Controls.Find("MoveDownButton", true).FirstOrDefault() as ZButton;
				AssertNotNull(moveDownButton);
				var moveUpButton = freightRatesControl.Controls.Find("MoveUpButton", true).FirstOrDefault() as ZButton;
				AssertNotNull(moveUpButton);

				moveUpButton.PerformClick();
				moveDownButton.PerformClick();
			}
		}
	}
}
