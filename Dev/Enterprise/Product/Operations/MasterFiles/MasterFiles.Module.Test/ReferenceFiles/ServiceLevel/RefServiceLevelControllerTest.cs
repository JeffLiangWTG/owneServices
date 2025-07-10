using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ServiceLevelController))]
	sealed class RefServiceLevelControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var serviceLevel = Factory.New<RefServiceLevel>();
			Factory.Save();
			return serviceLevel;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ServiceLevel;
		}
	}
}
