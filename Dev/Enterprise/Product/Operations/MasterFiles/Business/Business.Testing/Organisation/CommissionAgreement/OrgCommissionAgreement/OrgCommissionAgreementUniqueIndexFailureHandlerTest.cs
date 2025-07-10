using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCommissionAgreementUniqueIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestNotifyUserAndAttemptToResolve()
		{
			OrganisationsDataRegistry.Instance.AutoApproveFutureCommissionAgreements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var agreementInFactory1 = factory1.NewWithValidTestData<OrgCommissionAgreement>();
			factory1.Save();

			var agreementInFactory2 = factory2.Load<OrgCommissionAgreement>(agreementInFactory1.PK);
			agreementInFactory2.CreateDraft();
			factory2.Save();

			agreementInFactory1.CreateDraft();

			try
			{
				factory1.Save();
				Fail("Save should not have succeeded.");
			}
			catch (ZSaveException ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				AssertEquals("User should be notified of the situation.", false, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals("User should be notified of the situation", string.Format("Another user has changed {0}", agreementInFactory1.HumanReadableName), UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("User should be notified of the situation", string.Format("Another user has already made changes to {0}. You must re-open this form and re-apply your changes to continue.", agreementInFactory1.HumanReadableName), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
