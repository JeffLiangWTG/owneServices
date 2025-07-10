using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefAirlineCommodityCodeController))]
	sealed class RefAirlineCommodityCodeControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var testObject = Factory.NewWithValidTestData<RefAirlineCommodityCode>();
			testObject.RAC_Code = "11111";
			return testObject;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefAirlineCommodityCode;
		}
	}
}
