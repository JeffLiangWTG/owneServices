using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	internal class TemperatureControlBlockTest : TestCaseWithFactory
	{
		public void TestTemperatureControlButtonEnabledWithNoPackLine()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var temperatureControl = new TemperatureControlBlock())
			{
				temperatureControl.SetDataBinding(shipment, "OuterPackLines");

				AssertEquals("TemperatureControlMinButton should be disabled if no packlines exist", false, ((ZButton)temperatureControl.Controls.Find("TemperatureControlMinButton", true)[0]).Enabled);
				AssertEquals("TemperatureControlMaxButton should be disabled if no packlines exist", false, ((ZButton)temperatureControl.Controls.Find("TemperatureControlMaxButton", true)[0]).Enabled);
			}
		}

		public void TestTemperatureControlButtonsIsNotEnabledWithPackLine()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.OuterPackLines.AddNew();
			using (var temperatureControl = new TemperatureControlBlock())
			{
				temperatureControl.SetDataBinding(shipment, "OuterPackLines");

				AssertEquals("TemperatureControlMinButton should be enabled if packlines exist", true, ((ZButton)temperatureControl.Controls.Find("TemperatureControlMinButton", true)[0]).Enabled);
				AssertEquals("TemperatureControlMaxButton should be enabled if packlines exist", true, ((ZButton)temperatureControl.Controls.Find("TemperatureControlMaxButton", true)[0]).Enabled);
			}
		}

		public void TestTemperatureControlButtonsChangeEnabledWithPackLineChange()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var temperatureControl = new TemperatureControlBlock())
			{
				temperatureControl.SetDataBinding(shipment, "OuterPackLines");

				AssertEquals("TemperatureControlMinButton should be disabled if no packlines exist", false, ((ZButton)temperatureControl.Controls.Find("TemperatureControlMinButton", true)[0]).Enabled);
				AssertEquals("TemperatureControlMaxButton should be disabled if no packlines exist", false, ((ZButton)temperatureControl.Controls.Find("TemperatureControlMaxButton", true)[0]).Enabled);

				shipment.OuterPackLines.AddNew();

				AssertEquals("TemperatureControlMinButton should be enabled if packlines are added", true, ((ZButton)temperatureControl.Controls.Find("TemperatureControlMinButton", true)[0]).Enabled);
				AssertEquals("TemperatureControlMaxButton should be enabled if packlines are added", true, ((ZButton)temperatureControl.Controls.Find("TemperatureControlMaxButton", true)[0]).Enabled);

				shipment.OuterPackLines.RemoveAll();

				AssertEquals("TemperatureControlMinButton should be disabled if all packlines are removed", false, ((ZButton)temperatureControl.Controls.Find("TemperatureControlMinButton", true)[0]).Enabled);
				AssertEquals("TemperatureControlMaxButton should be disabled if all packlines are removed", false, ((ZButton)temperatureControl.Controls.Find("TemperatureControlMaxButton", true)[0]).Enabled);
			}
		}
	}
}
