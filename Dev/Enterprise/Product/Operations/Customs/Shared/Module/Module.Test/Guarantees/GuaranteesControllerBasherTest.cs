using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(GuaranteesController))]
	public class GuaranteesControllerBasherTest : ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			var controller = new GuaranteesControllerForTest();
			AssertEquals("For View", Env.Security.GuaranteesView, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.GuaranteesModify, controller.CheckPointForEdit);
			AssertEquals("For New", Env.Security.GuaranteesNew, controller.CheckPointForNew);
			AssertEquals("For Delete", Env.Security.GuaranteesDelete, controller.CheckPointForDelete);
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.Guarantees;

		sealed class GuaranteesControllerForTest : GuaranteesController
		{
			public GuaranteesControllerForTest()
			{
			}

			public new SecurityCheckpoint CheckPointForEdit => base.CheckPointForEdit;
			public new SecurityCheckpoint CheckPointForView => base.CheckPointForView;
			public new SecurityCheckpoint CheckPointForNew => base.CheckPointForNew;
			public new SecurityCheckpoint CheckPointForDelete => base.CheckPointForDelete;
		}
	}
}
