using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefUNLOCOController))]
	sealed class RefUNLOCOControllerTest : ZControllerBasherTest
	{
		public void TestModuleID()
		{
			AssertEquals("ModuleID", ModuleIDs.RefUNLOCO, new RefUNLOCOController().ModuleID);
		}

		public void TestSecurityCheckpoints()
		{
			RefUNLOCOControllerForTest controller = new RefUNLOCOControllerForTest();
			AssertEquals("For New", Env.Security.UNLOCONew, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.UNLOCOView, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.UNLOCOModify, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.UNLOCODelete, controller.CheckPointForDelete);
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			RefCountry country = Factory.New<RefCountry>();
			country.RN_Code = "PP";
			RefUNLOCO uNLOCO = Factory.New<RefUNLOCO>();
			uNLOCO.RL_RN_NKCountryCode = country.Code;
			uNLOCO.RL_Code = "PPLLL";
			Factory.Save();
			return uNLOCO;
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.RefUNLOCO;
		}

		#endregion
	}
}
