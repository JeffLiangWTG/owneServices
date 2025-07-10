using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.Module.Testing
{
	public abstract class BaseAirCargoControllerBasherTest : ZControllerBasherTest
	{
		public void TestErrorWhenOpeningFromAnotherCountry()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes._TemplateCountryName_;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_IsCTOMAWB = false;
			mawb.CM_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			mawb.CM_GB = branch.PK;
			var hawb = mawb.ChildBills.AddNew();
			Factory.Save();
			var expectedMessage = "You are trying to view an Air Cargo that belongs to a different country. Please log into a company which country is";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNull(Controller.ShowEditForm(mawb));
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNull(Controller.ShowViewForm(mawb));
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNull(Controller.ShowDeleteForm(mawb));
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			company.GC_RN_NKCountryCode = CountryCode;
			Factory.Save();
			using (var editForm = Controller.ShowEditForm(mawb))
			using (var viewForm = Controller.ShowViewForm(mawb))
			using (var deleteForm = Controller.ShowDeleteForm(mawb))
			{
				AssertNotNull(editForm);
				AssertNotContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNotNull(viewForm);
				AssertNotContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNotNull(deleteForm);
				AssertNotContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			expectedMessage = "You are trying to view an Air Cargo House Bill that belongs to a different country. Please log into a company which country is";
			company.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes._TemplateCountryName_;
			Factory.Save();
			AssertNull(Controller.ShowEditForm(hawb));
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNull(Controller.ShowViewForm(hawb));
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertNull(Controller.ShowDeleteForm(hawb));
			AssertContains(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		protected override string CountryCode => GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
	}
}
