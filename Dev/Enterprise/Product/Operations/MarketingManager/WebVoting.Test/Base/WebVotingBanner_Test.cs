using System;
using System.Web.UI.WebControls;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.WebVoting
{
	[HttpContextEnabledTest]
	class WebVotingBanner_Test : ZPageTestCase
	{
		public void TestPageLoad()
		{
			Banner.LogoImage = new HyperLink();
			Banner.Page = new ZPageForTest();
			Banner.Page_Load(Banner.Page, EventArgs.Empty);
			AssertEquals(Banner.PageInternal.AppInstance.LogoImage, Banner.LogoImage.ImageUrl);
			AssertEquals("http://www.test.cargowise.com/", Banner.LogoImage.NavigateUrl);
			AssertEquals("TestCompany", Banner.LogoImage.ToolTip);
		}

		[TestDate(2017, 11, 14, 13, 25, 10)]
		public void TestShowScheduledSystemUpgradeWarning()
		{
			var systemUpgradeServiceTask = Factory.New<StmScheduleTask>();
			systemUpgradeServiceTask.S5_IsActive = true;

			var pageLoadedDateTimeUtc = ZDateTime.UtcNow;
			var pageLoadedDateTime = pageLoadedDateTimeUtc.ToLocalBranchTime();
			var isDefaultPage = true;

			var warning = new VoteExamSurveySystemUpgradeScheduleTaskWarning(systemUpgradeServiceTask);

			systemUpgradeServiceTask.S5_NextScheduledPrintRunTimeUtc = pageLoadedDateTimeUtc.AddMinutes(-60);
			Banner.DisplayScheduledSystemUpgradeWarning(warning, isDefaultPage);
			Assert("1. WarningMessageLabel should not be visible", !Banner.WarningMessageLabel.Visible);

			systemUpgradeServiceTask.S5_NextScheduledPrintRunTimeUtc = pageLoadedDateTimeUtc.AddMinutes(61);
			Banner.DisplayScheduledSystemUpgradeWarning(warning, isDefaultPage);
			Assert("2. WarningMessageLabel should not be visible", !Banner.WarningMessageLabel.Visible);

			systemUpgradeServiceTask.S5_NextScheduledPrintRunTimeUtc = pageLoadedDateTimeUtc.AddHours(26);
			Banner.DisplayScheduledSystemUpgradeWarning(warning, isDefaultPage);
			Assert("3. WarningMessageLabel should not be visible", !Banner.WarningMessageLabel.Visible);

			systemUpgradeServiceTask.S5_NextScheduledPrintRunTimeUtc = pageLoadedDateTimeUtc.AddMinutes(60);
			Banner.DisplayScheduledSystemUpgradeWarning(warning, isDefaultPage);
			Assert("4. WarningMessageLabel should be visible", Banner.WarningMessageLabel.Visible);
		}

		public void TestTimeExpiredConfirmationTextEscapeQuote()
		{
			AssertEquals("Time Expired. Would you like to submit your answers?", Banner.TimeExpiredConfirmationText);

			using (var mockFR = Res.GetLanguageInstance(Core.SharedConstants.Languages.French).UseMockData())
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.French))
			{
				mockFR.Put("06f1d1da-3ff6-417e-a5bd-482f14265fb5", new ResourceStringData("06f1d1da-3ff6-417e-a5bd-482f14265fb5", "Heure d'arrivée à expiration. Souhaitez-vous envoyer vos réponses ?"));
				AssertEquals("Heure d\\'arrivée à expiration. Souhaitez-vous envoyer vos réponses ?", Banner.TimeExpiredConfirmationText);
			}
		}

		WebVotingBanner Banner
		{
			get
			{
				if (fBanner == null)
				{
					fBanner = new WebVotingBanner();
				}
				return fBanner;
			}
		}

		WebVotingBanner fBanner;

		#region class ZPageForTest

		class ZPageForTest : ZPage
		{
		}

		protected override ZPage GetNewZPage()
		{
			return new ZPage();
		}

		#endregion
	}
}
