using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Confirmations.GUI;
using Enterprise.Freight.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.Module.Testing
{
	public class ConfirmationsPluginTest : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestContructor()
		{
			IConfirmationsHost host = Factory.New<CommonShipment>();
			using (TestConfirmationsPluginForm form = new TestConfirmationsPluginForm(host))
			{
			}
		}

		[ExpectException(typeof(InvalidOperationException))]
		public void TestSetStrategy()
		{
			IConfirmationsHost host = Factory.New<CommonShipment>();
			using (TestConfirmationsPluginForm form = new TestConfirmationsPluginForm(host))
			{
				Control control = form.GetConfirmationsPlugin().UserControl;
			}
		}

		public void TestConfirmationTypeOriginPickup()
		{
			IConfirmationsHost host = Factory.New<CommonShipment>();
			using (TestConfirmationsPluginForm form = new TestConfirmationsPluginForm(host))
			{
				ConfirmationsPlugin plugin = (ConfirmationsPlugin)form.GetConfirmationsPlugin();
				plugin.SetStrategy(ConfirmationType.OriginPickup, "Pickup");
				AssertEquals(typeof(ShipmentPickupConfirmControl), plugin.UserControl.GetType());
			}
		}

		public void TestConfirmationTypeDestinationDelivery()
		{
			IConfirmationsHost host = Factory.New<CommonShipment>();
			using (TestConfirmationsPluginForm form = new TestConfirmationsPluginForm(host))
			{
				ConfirmationsPlugin plugin = (ConfirmationsPlugin)form.GetConfirmationsPlugin();
				plugin.SetStrategy(ConfirmationType.DestinationDelivery, "Delivery");
				AssertEquals(typeof(ShipmentDeliveryConfirmControl), plugin.UserControl.GetType());
			}
		}

		class TestConfirmationsPluginForm : ZForm
		{
			public TestConfirmationsPluginForm(IConfirmationsHost host)
				: base(host)
			{
				tabControl = new ZTabControl();
				Controls.Add(tabControl);
				tabControl.PlugIns.Add(ControllerIDs.ConfirmationsPlugin);
			}
			readonly ZTabControl tabControl;

			public ZPlugIn GetConfirmationsPlugin()
			{
				return tabControl.PlugIns.GetPlugIn(ControllerIDs.ConfirmationsPlugin);
			}
		}
	}
}
