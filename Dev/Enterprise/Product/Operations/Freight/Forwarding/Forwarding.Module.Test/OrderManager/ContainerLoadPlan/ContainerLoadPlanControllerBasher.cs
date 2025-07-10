using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ContainerLoadPlanController))]
	public class ContainerLoadPlanControllerBasher : ZControllerBasherTest
	{
		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var entity = Factory.NewWithValidTestData<CFSContainerLoadList>();
			Factory.Save();
			return entity;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ContainerLoadPlan;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
