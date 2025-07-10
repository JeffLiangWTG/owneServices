using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(PackContainerRegistrationController))]
	sealed class PackContainerRegistrationControllerBasherTest : ZControllerBasherTest
	{
		public void TestCFSContextService()
		{
			AssertEquals("CFS controllers must set CFSContextService on factory", FreightDomainContext.CFS, Controller.Factory.GetFreightDomainContext());
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			CFSContainer container = Factory.New<CFSContainer>();
			Factory.Save();
			return container;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.PackContainerRegistration;
		}

		#endregion
	}
}
