using CargoWise.EntityFramework;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	[TestedType(typeof(ThreeLetterRefAirlineController))]
	sealed class ThreeLetterRefAirlineControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var testObject = Factory.New(typeof(ThreeLetterRefAirline));
			Factory.Save();
			return testObject;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.ThreeLetterRefAirline;
	}
}
