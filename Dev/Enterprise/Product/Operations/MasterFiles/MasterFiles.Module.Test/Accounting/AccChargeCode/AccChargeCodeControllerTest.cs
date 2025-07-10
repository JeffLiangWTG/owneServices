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
	[TestedType(typeof(AccChargeCodeController))]
	sealed class AccChargeCodeControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AccChargeCode;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			Factory.Save();
			return chargeCode;
		}

		public void TestSecurityCheckpoints()
		{
			AccChargeCode normalChargeCode, normalChargeCodeLinked, globalChargeCode;
			AccChargeCodeTest.SetupGlobalChargeCodeScenario(
				Factory, out normalChargeCode, out normalChargeCodeLinked, out globalChargeCode);

			var controller = new AccChargeCodeControllerForTest();
			AssertEquals("For Delete, Normal Charge Code", Env.Security.ChargeCodesDelete, controller.GetCheckPointForDelete(normalChargeCode));
			AssertEquals("For Edit, Normal Charge Code", Env.Security.ChargeCodesModify, controller.GetCheckPointForEdit(normalChargeCode));
			AssertEquals("For New, Normal Charge Code", Env.Security.ChargeCodesNew, controller.GetCheckPointForNew(normalChargeCode));
			AssertEquals("For View, Normal Charge Code", Env.Security.ChargeCodes, controller.GetCheckPointForView(normalChargeCode));
			AssertEquals("For Copy, Normal Charge Code", Env.Security.ChargeCodesCopy, controller.GetCheckPointForCopy(normalChargeCode));

			AssertEquals("For Delete, Linked Charge Code", Env.Security.ChargeCodesLTGDelete, controller.GetCheckPointForDelete(normalChargeCodeLinked));
			AssertEquals("For Edit, Linked Charge Code", Env.Security.ChargeCodesLTGModify, controller.GetCheckPointForEdit(normalChargeCodeLinked));
			AssertEquals("For New, Linked Charge Code", Env.Security.ChargeCodesLTGNew, controller.GetCheckPointForNew(normalChargeCodeLinked));
			AssertEquals("For View, Linked Charge Code", Env.Security.ChargeCodesLTG, controller.GetCheckPointForView(normalChargeCodeLinked));
			AssertEquals("For Copy, Linked Charge Code", Env.Security.ChargeCodesLTGCopy, controller.GetCheckPointForCopy(normalChargeCodeLinked));
		}

		public void TestShowCopyForm()
		{
			var controller1 = new AccChargeCodeControllerForTest();
			AccChargeCode testAccChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			testAccChargeCode.AC_Code = "Test1";
			testAccChargeCode.AC_Desc = "Test1 Desc";
			Factory.Save();

			bool previousCopySecurity = Env.Security.ChargeCodesCopy.IsAllowed;

			try
			{
				Env.Security.ChargeCodesCopy.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZController controller = ZControllerFactory.Create(ControllerIDs.AccChargeCode);

				using (IZForm form = controller.ShowTemplateCopyForm(testAccChargeCode))
				{
					AssertNotNull("Should have returned a form", form);
					AssertEquals("Should have returned the correct form", typeof(AccChargeCodeForm), form.GetType());
					AccChargeCodeForm accChargeCodeForm = (AccChargeCodeForm)form;
					AccChargeCode copiedAccChargeCode = (AccChargeCode)accChargeCodeForm.BusinessEntity;
					AssertEquals("New Charge Code must be copy of previous Charge Code", testAccChargeCode.AC_Desc, copiedAccChargeCode.AC_Desc);
					Assert(!UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				}

				Env.Security.ChargeCodesCopy.IsAllowed = false;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				controller = ZControllerFactory.Create(ControllerIDs.AccChargeCode);

				using (IZForm form = controller.ShowTemplateCopyForm(testAccChargeCode))
				{
					AssertNull("Should not return a form", form);
					Assert(UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				}
			}
			finally
			{
				Env.Security.ChargeCodesCopy.IsAllowed = previousCopySecurity;
			}
		}
	}
}
