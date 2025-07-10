using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GenCustomAddOnRuleController))]
	sealed class GenCustomAddOnRuleControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.GenCustomAddOnRule;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var parent = Factory.New<GenCustomAddOnRule>();
			Factory.Save();
			return parent;
		}

		public void TestCheckPoints()
		{
			var bizo = GetBusinessObjectThatIsInTheDatabase();
			var controller = new GenCustomAddOnRuleController();

			AssertEquals(Env.Security.AddOnRulesNew, controller.GetCheckPointForNew(bizo));
			AssertEquals(Env.Security.AddOnRulesEdit, controller.GetCheckPointForEdit(bizo));
			AssertEquals(Env.Security.AddOnRulesView, controller.GetCheckPointForView(bizo));
			AssertEquals(Env.Security.AddOnRulesDelete, controller.GetCheckPointForDelete(bizo));
		}
	}
}
