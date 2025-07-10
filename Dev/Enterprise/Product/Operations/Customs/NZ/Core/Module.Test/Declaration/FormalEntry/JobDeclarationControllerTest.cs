using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry
{
	sealed class JobDeclarationControllerTest : TestCaseWithFactory
	{
		public void TestDeclarationControllerGetsRightPlugin()
		{
			JobDeclarationController controller = (JobDeclarationController)ZControllerFactory.Create(ControllerIDs.Customs.JobDeclaration);
			using (ZArchitecture.PlugIn.ZPlugIn plugIn = controller.GetPlugIn_ForTest(Factory.New(typeof(ForwardingShipment))))
			{
				AssertEquals(typeof(GUI.Declaration.BrokeragePlugIn), plugIn.GetType());
			}
		}
	}
}
