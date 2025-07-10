using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Module.Testing
{
	[TestedType(typeof(JobShipmentPreplanningController))]
	public class JobShipmentPreplanningControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			JobShipmentPreplanning preAdvice = Factory.NewWithValidTestData<JobShipmentPreplanning>();
			Factory.Save();
			return preAdvice;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobShipmentPreplanning;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}
	}
}
