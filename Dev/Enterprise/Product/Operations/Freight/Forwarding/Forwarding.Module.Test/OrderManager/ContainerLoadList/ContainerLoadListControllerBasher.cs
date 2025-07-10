using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(ContainerLoadListController))]
	public class ContainerLoadListControllerBasher : ZControllerBasherTest
	{
		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var entity = Factory.NewWithValidTestData<CYContainerLoadList>();
			Factory.Save();
			return entity;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ContainerLoadList;
		}

		public override void TestGetOpenFormUrlslDoesNotHitDatabase()
		{
			Assert(true);
		}

		#endregion
	}
}
