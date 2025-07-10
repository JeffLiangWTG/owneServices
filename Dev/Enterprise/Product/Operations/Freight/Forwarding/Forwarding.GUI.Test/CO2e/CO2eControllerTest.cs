using System;
using System.Reflection;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	[TestedType(typeof(CO2eController))]
	public class CO2eControllerTest : ZControllerBasherTest
	{
		public void TestGetPlugin()
		{
			var controller = new CO2eController();
			var dummyBizo = Factory.New<DummyBusinessObject>();
			var getPlugin = typeof(CO2eController).GetMethod("GetPlugIn", BindingFlags.Instance | BindingFlags.NonPublic);
			var plugin = getPlugin.Invoke(controller, new object[] { dummyBizo });
			AssertEquals("Plugin type", typeof(CO2ePlugin), plugin.GetType());
			((ZPlugIn)plugin).Dispose();
			ErrorReporter.Clear();
		}

		public override Type ControllerToBashType => typeof(CO2eController);

		protected override ControllerID GetControllerID() => ControllerIDs.CO2ePlugin;
	}
}
