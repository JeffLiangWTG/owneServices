using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Module.Testing;
using Enterprise.Customs.US.DIS.Business.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Module.Testing
{
	[TestedType(typeof(DISController))]
	sealed class DISControllerTest : DISControllerBaseTest
	{
		public override void TestSecurity()
		{
			var controller = new DISController();
			AssertEquals(Env.Security.None, controller.GetCheckPointForDelete(null));
			AssertEquals(Env.Security.CustomsDISEdit, controller.GetCheckPointForEdit(null));
			AssertEquals(Env.Security.None, controller.GetCheckPointForNew(null));
			AssertEquals(Env.Security.CustomsDISView, controller.GetCheckPointForView(null));
		}

		public override Type ControllerToBashType => typeof(DISController);

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase() => new TestHelper(Factory).GetJobDeclaration();
	}
}
