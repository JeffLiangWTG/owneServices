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
	[TestedType(typeof(CMDConsolController))]
	sealed class CMDConsolControllerTest : ZControllerBasherTest
	{
		public void TestPlugIn()
		{
			var controller = new CMDConsolController();
			var consol = Factory.New<ForwardingConsol>();
			var getPlugInMethod = controller.GetType().GetMethod("GetPlugIn", BindingFlags.NonPublic | BindingFlags.Instance);
			using (var plugIn = (ZPlugIn)getPlugInMethod.Invoke(controller, new object[] { consol }))
			{
				AssertEquals(typeof(CMDConsolPlugIn), plugIn.GetType());
			}
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			throw new ModuleGuiNotSupportedException("");
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.SG.CMDConsol;

		protected override Type GetBusinessObjectType() => typeof(ForwardingConsol);

		protected override string CountryCode => Core.Constants.CountryCodes.Singapore;
	}
}
