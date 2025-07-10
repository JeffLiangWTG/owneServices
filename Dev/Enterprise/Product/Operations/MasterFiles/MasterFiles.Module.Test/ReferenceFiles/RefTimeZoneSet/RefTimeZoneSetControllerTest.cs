using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefTimeZoneSetController))]
	sealed class RefTimeZoneSetControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObject testObject = Factory.New(typeof(RefTimeZoneSet));
			Factory.Save();
			return testObject;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefTimeZoneSet;
		}
	}
}
