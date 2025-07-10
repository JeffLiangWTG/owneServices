using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.NZ.Module.Declaration.FormalEntry
{
	sealed class JobDeclarationShipmentControllerTest : TestCaseWithFactory
	{
		public void TestShipmentControllerGetsRightPlugin()
		{
			JobDeclarationShipmentController controller = (JobDeclarationShipmentController)ZControllerFactory.Create(ControllerIDs.Customs.JobDeclarationPluggedIntoShipment);
			using (ZArchitecture.PlugIn.ZPlugIn plugIn = controller.GetPlugIn_ForTest(Factory.New(typeof(ForwardingShipment))))
			{
				AssertEquals(typeof(GUI.Declaration.BrokeragePlugIn), plugIn.GetType());
			}
		}
	}
}
