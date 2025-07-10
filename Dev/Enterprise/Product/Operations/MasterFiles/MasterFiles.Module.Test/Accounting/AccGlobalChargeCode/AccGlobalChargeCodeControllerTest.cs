using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccGlobalChargeCodeController))]
	sealed class AccGlobalChargeCodeControllerTest : ZControllerBasherTest
	{
		public override void TestNewForm()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowNewForm() as AccGlobalChargeCodeForm;
				AssertNotNull("Form is AccGlobalChargeCodeForm", form);
				Assert("Charge code for form is created as global", ((AccChargeCode)form.BusinessEntity).IsGlobal);
				AssertEquals("Charge code has no changes", false, ((AccChargeCode)form.BusinessEntity).HasChanges);
				AssertEquals("Charge code not in db", false, ((AccChargeCode)form.BusinessEntity).IsInDatabase);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		public override void TestEditForm()
		{
			AssertControllerNotNull();
			try
			{
				var form = Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()) as AccGlobalChargeCodeForm;
				AssertNotNull("Form is AccGlobalChargeCodeForm", form);
				Assert("Charge code for form is global", ((AccChargeCode)form.BusinessEntity).IsGlobal);
				AssertEquals("Charge code has no changes", false, ((AccChargeCode)form.BusinessEntity).HasChanges);
				AssertEquals("Charge code is in db", true, ((AccChargeCode)form.BusinessEntity).IsInDatabase);
			}
			catch (ModuleFeatureNotSupportedException)
			{
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccGlobalChargeCode;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			return globalChargeCode;
		}

		public void TestSecurityCheckpoints()
		{
			AccGlobalChargeCodeControllerForTest controller = new AccGlobalChargeCodeControllerForTest();
			AssertEquals("For New", Env.Security.GlobalChargeCodesNew, controller.CheckPointForNew);
			AssertEquals("For View", Env.Security.GlobalChargeCodes, controller.CheckPointForView);
			AssertEquals("For Edit", Env.Security.GlobalChargeCodesModify, controller.CheckPointForEdit);
			AssertEquals("For Delete", Env.Security.GlobalChargeCodesDelete, controller.CheckPointForDelete);
		}

		public void TestShowCopyForm()
		{
			AccGlobalChargeCodeControllerForTest controller1 = new AccGlobalChargeCodeControllerForTest();
			AccChargeCode testAccChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testAccChargeCode.AC_Code = "Test1";
			testAccChargeCode.AC_Desc = "Test1 Desc";
			Factory.Save();

			bool previousCopySecurity = Env.Security.GlobalChargeCodesCopy.IsAllowed;

			try
			{
				Env.Security.GlobalChargeCodesCopy.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZController controller = ZControllerFactory.Create(ControllerIDs.AccGlobalChargeCode);

				using (IZForm form = controller.ShowTemplateCopyForm(testAccChargeCode))
				{
					AssertNotNull("Should have returned a form", form);
					AssertEquals("Should have returned the correct form", typeof(AccGlobalChargeCodeForm), form.GetType());
					AccGlobalChargeCodeForm accGlobalChargeCodeForm = (AccGlobalChargeCodeForm)form;
					AccChargeCode copiedAccChargeCode = (AccChargeCode)accGlobalChargeCodeForm.BusinessEntity;
					AssertEquals("New Charge Code must be copy of previous Charge Code", testAccChargeCode.AC_Desc, copiedAccChargeCode.AC_Desc);
					Assert(!UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				}

				Env.Security.GlobalChargeCodesCopy.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				controller = ZControllerFactory.Create(ControllerIDs.AccGlobalChargeCode);

				using (IZForm form = controller.ShowTemplateCopyForm(testAccChargeCode))
				{
					AssertNull("Should not return a form", form);
					Assert(UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				}
			}
			finally
			{
				Env.Security.GlobalChargeCodesCopy.IsAllowed = previousCopySecurity;
			}
		}
	}
}
