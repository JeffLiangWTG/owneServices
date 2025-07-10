using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(InBondNumberController))]
	sealed class InBondNumberControllerTest : NumberControllerTest
	{
		public void TestGetForm()
		{
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			var controller = ZControllerFactory.Create(GetControllerID());
			DeclarationTestHelper.SetupBranchSpecificInBondNumberRanges();
			using (var form = controller.ShowNewForm())
			{
				AssertEquals(typeof(InBondNumberSettingForBranchSpecificForm), form.GetType());
			}
		}

		public void TestCheckPoints()
		{
			var declaration = Factory.New<JobDeclaration>();
			var controller = new InBondNumberController();
			AssertEquals(Env.Security.InBondNumber, controller.GetCheckPointForNew(declaration));
		}

		protected override ControllerID GetControllerID() => ControllerIDs.Customs.US.InBondNumber;
	}
}
