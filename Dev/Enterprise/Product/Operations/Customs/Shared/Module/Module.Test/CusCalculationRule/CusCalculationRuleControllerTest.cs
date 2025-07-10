using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(CusCalculationRulesController))]
	public abstract class CusCalculationRulesControllerTest : ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			var controller = new CusCalculationRulesControllerForTest();
			AssertEquals("For View", Env.Security.CusCalculationRulesView, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.CusCalculationRulesModify, controller.CheckPointForEdit);
			AssertEquals("For New", Env.Security.CusCalculationRulesNew, controller.CheckPointForNew);
			AssertEquals("For Delete", Env.Security.CusCalculationRulesDelete, controller.CheckPointForDelete);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.CusCalculationRules;

		sealed class CusCalculationRulesControllerForTest : CusCalculationRulesController
		{
			public CusCalculationRulesControllerForTest()
			{
			}

			public new SecurityCheckpoint CheckPointForEdit => base.CheckPointForEdit;
			public new SecurityCheckpoint CheckPointForView => base.CheckPointForView;
			public new SecurityCheckpoint CheckPointForNew => base.CheckPointForNew;
			public new SecurityCheckpoint CheckPointForDelete => base.CheckPointForDelete;
		}
	}
}
