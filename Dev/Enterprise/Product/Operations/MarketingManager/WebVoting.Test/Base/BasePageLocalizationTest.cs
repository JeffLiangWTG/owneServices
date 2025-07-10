using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.WebVoting.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.MarketingManager.WebVoting
{
	class BasePageLocalizationTest : ZPageTestCase
	{
		protected override ZPage GetNewZPage()
		{
			return new BasePage();
		}

		public void TestInitializeCultureShouldNotThrowExceptionWhenQueryStringContainsInvalidData()
		{
			Page.Request.QueryString[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey] = "invalidString";
			AssertNoExceptionThrown(() => { ((BasePage)Page).InitializeCultureInternal(); });
		}

		public void TestLanguage()
		{
			var languageString = "Language";
			try
			{
				var allowedLanguages = WebDataRegistry.Instance.AllowedLanguages.Value;
				allowedLanguages.RemoveAll();
				var allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = "EN";
				allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = "DE-DE";
				allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = "FR-FR";
				WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);

				AssertType(typeof(LanguageSelectionControl), ((BasePage)Page).FooterInternal.Controls.Cast<Control>().FirstOrDefault(control => control.ID == "LanguageSelection"));

				AssertEquals(Res.DefaultLanguage, Res.CurrentLanguage);
				Page.Session["Language"] = "DE-DE";
				((BasePage)Page).InitializeCultureInternal();
				AssertEquals("DE-DE", Res.CurrentLanguage);

				Page.Request.Cookies.Add(new HttpCookie(languageString, "FR-FR"));
				((BasePage)Page).InitializeCultureInternal();
				AssertEquals("FR-FR", Res.CurrentLanguage);

				AddQueryString(VoteExamSurveyUrlHelper.LanguageStringKey, "DE-DE");
				((BasePage)Page).InitializeCultureInternal();
				AssertEquals("DE-DE", Res.CurrentLanguage);
			}
			finally
			{
				ObjectFactory.Get<IResourceStrings>().CurrentLanguage = Res.DefaultLanguage;
			}
		}

		[UseSnapshotProtection]
		public void TestInitializeCulture_NoUndisposedDbConnectionError()
		{
			WebTestHelper.AssertNoUndisposedDbConnectionError(this, () =>
			{
				AddQueryString(VoteExamSurveyUrlHelper.LanguageStringKey, "FR-FR");
				((BasePage)Page).InitializeCultureInternal();
			});
		}

		void AddQueryString(string key, string value)
		{
			AddQueryString(Page, key, value);
		}

		void AddQueryString(ZPage page, string key, string value)
		{
			string secureQueryStringData = page.Request[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey];
			SecureQueryString queryString = new SecureQueryString(secureQueryStringData);
			queryString.Remove(SecureQueryString.TimeStampKey);
			queryString.Add(key, value);
			page.Request.QueryString[VoteExamSurveyUrlHelper.VoteExamSurveyQueryStringKey] = queryString.ToString();
		}
	}
}
