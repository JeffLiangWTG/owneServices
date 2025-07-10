using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI.Testing
{
	sealed class PortMessagingControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestControl_Consol()
		{
			var consol = Factory.New<ForwardingConsol>();
			using (var control = new PortMessagingControl())
			{
				control.SetDataBinding(new ConsolPortMessagingManager(consol), null);
			}
		}

		[ExpectNoExceptions]
		public void TestControl_Shipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var control = new PortMessagingControl())
			{
				control.SetDataBinding(new ShipmentPortMessagingManager(shipment), null);
			}
		}

		[ExpectNoExceptions]
		public void TestControl_TabVisibility()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (var control = new PortMessagingControl())
			{
				control.SetDataBinding(new ShipmentPortMessagingManager(shipment), null);
				control.ProcessContent();
				var tab = control.FindSingle<ZTabPage>("portOrderTabPage");
				
				using (PortMessagingRegistry.Instance.EnablePortOrderFormBuilderForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					control.ProcessContent();

					AssertEquals(false, tab.TabVisible);
				}

				using (PortMessagingRegistry.Instance.EnablePortOrderFormBuilderForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					control.ProcessContent();

					AssertEquals(true, tab.TabVisible);
				}
			}

			var consol = Factory.New<ForwardingConsol>();
			using (var control = new PortMessagingControl())
			{
				control.SetDataBinding(new ConsolPortMessagingManager(consol), null);
				control.ProcessContent();
				var tab = control.FindSingle<ZTabPage>("portOrderTabPage");

				using (PortMessagingRegistry.Instance.EnablePortOrderFormBuilderForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					control.ProcessContent();

					AssertEquals(false, tab.TabVisible);
				}

				using (PortMessagingRegistry.Instance.EnablePortOrderFormBuilderForm.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					control.ProcessContent();

					AssertEquals(true, tab.TabVisible);
				}
			}
		}
	}
}
