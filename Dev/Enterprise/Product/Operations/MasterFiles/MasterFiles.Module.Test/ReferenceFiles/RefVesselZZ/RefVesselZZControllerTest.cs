using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefVesselZZController))]
	sealed class RefVesselZZControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var testObject = Factory.NewWithValidTestData<RefVesselZZ>();
			testObject.ZZO_ZZZ_NKDataGrouping = "ZA";
			return testObject;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefVesselZZ;
		}
	}
}
