using System;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(CourtesyNoticesOfLiquidationController))]
	sealed class CourtesyNoticesOfLiquidationControllerTest : ZControllerBasherTest
	{
		public void TestCheckPoints()
		{
			var liquidation = Factory.New<CusLiquidation>();
			var controller = new CourtesyNoticesOfLiquidationController();
			AssertEquals(Env.Security.CourtesyNoticesOfLiquidationMessagesView, controller.GetCheckPointForView(liquidation));
			AssertEquals(Env.Security.None, controller.GetCheckPointForEdit(liquidation));
			AssertEquals(Env.Security.None, controller.GetCheckPointForDelete(liquidation));
			AssertEquals(Env.Security.None, controller.GetCheckPointForNew(liquidation));
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.CourtesyNoticesOfLiquidation;

		public override Type ControllerToBashType => typeof(CourtesyNoticesOfLiquidationController);
	}
}
