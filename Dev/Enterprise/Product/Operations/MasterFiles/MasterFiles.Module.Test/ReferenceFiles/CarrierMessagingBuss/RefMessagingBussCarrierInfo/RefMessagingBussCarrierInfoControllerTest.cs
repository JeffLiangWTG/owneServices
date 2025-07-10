using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefMessagingBussCarrierInfoController))]
	class RefMessagingBussCarrierInfoControllerTest : ZControllerBasherTest
	{
		public void TestImplementation()
		{
			AssertEquals(ModuleIDs.RefMessagingBussCarrierInfo, Controller.ModuleID);
			AssertEquals(typeof(RefMessagingBussCarrierInfo), Controller.TypeOfTopLevelBusinessObject);
			AssertEquals(expected: true, Controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}

		public void TestSecurityCheckpoints()
		{
			var refMessagingBussCarrierInfo = Factory.New<RefMessagingBussCarrierInfo>();

			CombineAssertions(() =>
			{
				AssertEquals(Controller.GetCheckPointForNew(refMessagingBussCarrierInfo), Env.Security.None);
				AssertEquals(Controller.GetCheckPointForEdit(refMessagingBussCarrierInfo), Env.Security.None);
				AssertEquals(Controller.GetCheckPointForView(refMessagingBussCarrierInfo), Env.Security.RefMessagingBussCarrierInfoView);
				AssertEquals(Controller.GetCheckPointForDelete(refMessagingBussCarrierInfo), Env.Security.None);
			});
		}

		protected override ControllerID GetControllerID()
			=> ControllerIDs.RefMessagingBussCarrierInfo;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = base.Factory.NewWithValidTestData(GetBusinessObjectType());
			base.Factory.Save();
			return result;
		}
	}
}
