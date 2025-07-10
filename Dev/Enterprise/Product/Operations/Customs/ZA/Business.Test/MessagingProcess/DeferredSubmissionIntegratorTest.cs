using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ZA.Business.MessagingProcess;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.ZA.Business.Testing.MessagingProcess
{
	internal sealed class DeferredSubmissionIntegratorTest : TestCaseWithFactory
	{
		public void TestShouldShowDialog()
		{
			var wrapper = new JobDeclarationMessageSendingObjectParent(DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory));
			var integrator = new DeferredSubmissionIntegrator(wrapper);

			CombineAssertions(() =>
			{
				AssertEquals("Everything set - can send", true, integrator.ShouldShowDialog());

				wrapper.ParentDeclaration.JE_DateOfArrival = ZDateTime.Empty;
				AssertEquals("No JE_DateOfArrival", false, integrator.ShouldShowDialog());

				wrapper.ParentDeclaration.JE_DateOfArrival = ZDateTime.Now;
				wrapper.ParentDeclaration.ActiveEntryHeaders[0].CH_PaymentMethod = "";
				AssertEquals("No Dutiable entry line", false, integrator.ShouldShowDialog());
			});
		}

		public void TestUpdateDeferment()
		{
			var wrapper = new JobDeclarationMessageSendingObjectParent(DeferredSubmissionTestHelper.CreateDeferableJobDeclaration(Factory));
			var integrator = new DeferredSubmissionIntegrator(wrapper);

			AssertEquals("Pre-Req - Should show", true, integrator.ShouldShowDialog());
			AssertEquals("Pre-Req - Declaration Arrival date", ZDateTime.Today.AddDays(2), wrapper.ParentDeclaration.JE_DateOfArrival);
			AssertEquals("Pre-Req - Submission date", ZDateTime.Today, integrator.Submission.SubmissionDate);

			integrator.Submission.IsOverwritten = true;
			integrator.Submission.SubmissionDate = ZDateTime.Today.AddDays(1);

			integrator.UpdateDeferment();

			AssertEquals("Submission date changed", ZDateTime.Today.AddDays(1), wrapper.SendingObjectsCollection[0].SubmissionDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var messageDeferralSettings = new AutomaticDeferredSelection() { AllowAutomaticDeferredSelection = true, DaysBeforeETA = 2 };
			registrySetup = ZACustomsRegistry.Instance.AutomaticDeferredSelection.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, messageDeferralSettings);
		}

		protected override void TearDown()
		{
			registrySetup.Dispose();
			base.TearDown();
		}

		IDisposable registrySetup;
	}
}
