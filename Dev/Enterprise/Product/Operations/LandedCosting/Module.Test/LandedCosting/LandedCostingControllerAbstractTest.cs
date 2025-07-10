using Enterprise.Environment;
using Enterprise.LandedCosting.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Module.Testing
{
	abstract class LandedCostingControllerAbstractTest : ZControllerBasherTest
	{
		[ExpectException(typeof(ModuleGuiNotSupportedException))]
		public void TestGetForm()
		{
			var controller = GetControllerToTest();
			((ZControllerInternals)controller).GetForm(Factory.New<LandedCostHeader>());
		}

		public void TestCheckPoint()
		{
			var controller = GetControllerToTest();
			CombineAssertions(() =>
			{
				AssertEquals("View", Env.Security.CustomsDeclarationLandedCost, controller.CheckPointForViewExposedForTest);
				AssertEquals("Edit", Env.Security.CustomsDeclarationLandedCost, controller.CheckPointForEditExposedForTest);
				AssertEquals("New", Env.Security.CustomsDeclarationLandedCost, controller.CheckPointForNewExposedForTest);
				AssertEquals("Delete", Env.Security.CustomsDeclarationLandedCost, controller.CheckPointForDeleteExposedForTest);
			});
		}

		protected abstract LandedCostingControllerBase GetControllerToTest();
	}
}
