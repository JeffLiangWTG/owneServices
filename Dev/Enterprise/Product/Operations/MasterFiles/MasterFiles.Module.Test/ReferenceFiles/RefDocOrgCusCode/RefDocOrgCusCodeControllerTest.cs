using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefDocOrgCusCodeController))]
	sealed class RefDocOrgCusCodeControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var code = Factory.New<RefDocOrgCusCode>();
			code.DOC_CodeType = "TST";
			code.DOC_DocumentType = "HAW";
			code.DOC_ShortLabel = "TST";
			code.DOC_LongLabel = "TEST";
			code.DOC_Description = "For Testing";

			Factory.Save();

			return code;
		}

		protected override ControllerID GetControllerID() => ControllerIDs.RefDocOrgCusCode;

		public void TestSecurityCheckpoints()
		{
			var controller = new TestRefDocOrgCusCodeController();

			AssertEquals("For New", Env.Security.None, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.None, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.None, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.None, controller.CheckPointForDelete);
		}

		class TestRefDocOrgCusCodeController : RefDocOrgCusCodeController
		{
			public new SecurityCheckpoint CheckPointForNew => base.CheckPointForNew;

			public new SecurityCheckpoint CheckPointForView => base.CheckPointForView;

			public new SecurityCheckpoint CheckPointForEdit => base.CheckPointForEdit;

			public new SecurityCheckpoint CheckPointForDelete => base.CheckPointForDelete;
		}
	}
}
