using System;
using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Customs.SG.V4.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	[TestedType(typeof(CMDShipmentController))]
	sealed class CMDShipmentControllerTest : ZControllerBasherTest
	{
		public void TestPlugIn()
		{
			var controller = new CMDShipmentController();
			var shipment = Factory.New<ForwardingShipment>();
			var getPlugInMethod = controller.GetType().GetMethod("GetPlugIn", BindingFlags.NonPublic | BindingFlags.Instance);
			using (var plugIn = (ZPlugIn)getPlugInMethod.Invoke(controller, new object[] { shipment }))
			{
				AssertEquals(typeof(CMDShipmentPlugIn), plugIn.GetType());
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			throw new ModuleGuiNotSupportedException("");
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.SG.CMDShipment;

		protected override Type GetBusinessObjectType() => typeof(ForwardingShipment);

		protected override string CountryCode => Core.Constants.CountryCodes.Singapore;
	}
}
