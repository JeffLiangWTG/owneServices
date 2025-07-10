using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefTransitTimeController))]
	sealed class RefTransitTimeControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var transitTime = Factory.NewWithValidTestData<RefTransitTime>();
			Factory.Save();

			return transitTime;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefTransitTime;
		}
	}
}
