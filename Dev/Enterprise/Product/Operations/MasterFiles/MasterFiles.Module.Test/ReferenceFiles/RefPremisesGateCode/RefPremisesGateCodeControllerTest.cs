using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefPremisesGateCodeController))]
	sealed class RefPremisesGateCodeControllerTest : ZControllerBasherTest
	{
		public void TestModuleId()
		{
			AssertEquals(ModuleIDs.RefPremisesGateCode, ((RefPremisesGateCodeController)base.Controller).ModuleID);
		}

		public void TestTypeOfTopLevelBusinessObject()
		{
			AssertEquals(typeof(RefPremisesGateCode), ((RefPremisesGateCodeController)base.Controller).TypeOfTopLevelBusinessObject);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObject testObject = Factory.New(typeof(RefPremisesGateCode));
			Factory.Save();
			return testObject;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefPremisesGateCode;
		}

		public void TestCheckPoints()
		{
			RefPremisesGateCodeController testObject = new RefPremisesGateCodeController();
			AssertEquals(Env.Security.PremisesGateCode, testObject.CheckPointForViewForTesting);
			AssertEquals(Env.Security.PremisesGateCodeModify, testObject.CheckPointForDeleteForTesting);
			AssertEquals(Env.Security.PremisesGateCodeModify, testObject.CheckPointForEditForTesting);
			AssertEquals(Env.Security.PremisesGateCodeModify, testObject.CheckPointForNewForTesting);
		}
	}
}
