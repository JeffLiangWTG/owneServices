using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(WebReportHolder))]
	sealed class WebReportHolderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCollectionsContainsWebReportsOnly()
		{
			ReportCommand report = CreateReportCommand("RepWhsReport", "RepWhsReport", true, true, Core.Constants.StmMenuItemTypes.WebReports);
			ReportCommand document = CreateReportCommand("DocCustomsDocument", "DocCustomsDocument", true, true, Core.Constants.StmMenuItemTypes.WebReports);
			Factory.Save();

			AssertEquals("Precondition: SiteUser is Logged In", true, TestSiteUser.IsLoggedIn);
			OrgSecurityContacts reportSecurityRight = FindReportSecurityRight(TestSiteUser.LoggedInUser, report);
			reportSecurityRight.OZ_Granted = true;

			OrgSecurityContacts documentSecurityRight = FindReportSecurityRight(TestSiteUser.LoggedInUser, document);
			var testHolder = (WebReportHolder)GetNewBusinessObject();
			AssertEquals("Collection Contains RepWhsReport", true, testHolder.WebReports.Contains(report));
			AssertEquals("Collection Does not Contains DocCustomsDocument", false, testHolder.WebReports.Contains(document));
		}

		public void TestSettingReportPKChangesSelectedReport()
		{
			// Need to use real reports in this test so that we actualy get a Report BizObj generated
			ReportCommand commandOne = FindReportCommand("Client - Order Lines");
			ReportCommand commandTwo = FindReportCommand("Client - Order Status Summary");

			using (WebReportHolder reportholder = (WebReportHolder)GetNewBusinessObject())
			{
				AssertNull("Selected Report should be null by default", reportholder.SelectedReport);

				reportholder.ReportPK = commandOne.PK;
				AssertNotNull("Selected Report", reportholder.SelectedReport);
				AssertEquals("Report Name", "Client - Order Line Report", reportholder.SelectedReport.Name);

				ZGuid originalReportPK = reportholder.SelectedReport.PK;

				reportholder.ReportPK = commandTwo.PK;
				AssertNotNull("Selected Report", reportholder.SelectedReport);
				AssertEquals("Report Name", "Client - Order Status Summary Report", reportholder.SelectedReport.Name);

				AssertNotEquals("Should be a different BizObj", originalReportPK, reportholder.SelectedReport.PK);
			}
		}

		public void TestCollectionContainsPublishedWebReports()
		{
			ReportCommand enterpriseNotPublishedClientCreatedReport = CreateReportCommand("RepCustomsReport", "enterpriseNotPublishedClientCreatedReport", false, false, Core.Constants.StmMenuItemTypes.Documents);
			ReportCommand enterpriseNotPublishedSystemReport = CreateReportCommand("RepCustomsReport", "enterpriseNotPublishedSystemReport", false, true, Core.Constants.StmMenuItemTypes.Documents);
			ReportCommand enterprisePublishedClientCreatedReport = CreateReportCommand("RepCustomsReport", "enterprisePublishedClientCreatedReport", true, false, Core.Constants.StmMenuItemTypes.Documents);
			ReportCommand enterprisePublishedNotSystemReport = CreateReportCommand("RepCustomsReport", "enterprisePublishedNotSystemReport", true, true, Core.Constants.StmMenuItemTypes.Documents);

			ReportCommand webNotPublishedClientCreatedReport = CreateReportCommand("RepCustomsReport", "webNotPublishedClientCreatedReport", false, false, Core.Constants.StmMenuItemTypes.WebReports);
			ReportCommand webNotPublishedSystemReport = CreateReportCommand("RepCustomsReport", "webNotPublishedSystemReport", false, true, Core.Constants.StmMenuItemTypes.WebReports);
			ReportCommand webPublishedClientCreatedReport = CreateReportCommand("RepCustomsReport", "webPublishedClientCreatedReport", true, false, Core.Constants.StmMenuItemTypes.WebReports);
			ReportCommand webPublishedSystemReport = CreateReportCommand("RepWhsReport", "webPublishedSystemReport", true, true, Core.Constants.StmMenuItemTypes.WebReports);

			AssertEquals("Precondition: SiteUser is Logged In", true, TestSiteUser.IsLoggedIn);
			WebReportHolder testHolder = (WebReportHolder)GetNewBusinessObject();

			CheckThatEnterpriseReportIsNotAvailableOnTheWeb(TestSiteUser, enterpriseNotPublishedClientCreatedReport);
			CheckThatEnterpriseReportIsNotAvailableOnTheWeb(TestSiteUser, enterpriseNotPublishedSystemReport);
			CheckThatEnterpriseReportIsNotAvailableOnTheWeb(TestSiteUser, enterprisePublishedClientCreatedReport);
			CheckThatEnterpriseReportIsNotAvailableOnTheWeb(TestSiteUser, enterprisePublishedNotSystemReport);

			CheckThatWebReportIsAvailableOnTheWeb(TestSiteUser, webNotPublishedClientCreatedReport);
			CheckThatWebReportIsAvailableOnTheWeb(TestSiteUser, webNotPublishedSystemReport);
			CheckThatWebReportIsAvailableOnTheWeb(TestSiteUser, webPublishedClientCreatedReport);
			CheckThatWebReportIsAvailableOnTheWeb(TestSiteUser, webPublishedSystemReport);

			ReportCommand[] webReports = new ReportCommand[]
										 {
												 webNotPublishedClientCreatedReport,
												 webNotPublishedSystemReport,
												 webPublishedClientCreatedReport,
												 webPublishedSystemReport
										 };

			foreach (ReportCommand webReport in webReports)
			{
				OrgSecurityContacts webReportSecurityRight = FindReportSecurityRight(TestSiteUser.LoggedInUser, webReport);
				AssertEquals(string.Format("Default Permissions for [{0}]", webReport.SU_MenuName), false, webReportSecurityRight.OZ_Granted);

				webReportSecurityRight.OZ_Granted = true;
				WebSecurityRight securityRight = ReportsWebSecurityRights.GetSecurityRightForReport(webReport);

				AssertEquals(String.Format("Permissions for [{0}].", webReport.SU_MenuName), true, TestSiteUser.LoggedInUser.SecurityRightsForBindingOnly.IsRightGranted(securityRight));
			}

			AssertEquals("Precondition: Collection has more than one item", true, testHolder.WebReports.Count > 0);

			AssertEquals("Collection Contains enterpriseNotPublishedClientCreatedReport", false, testHolder.WebReports.Contains(enterpriseNotPublishedClientCreatedReport.PK));
			AssertEquals("Collection Contains enterpriseNotPublishedSystemReport", false, testHolder.WebReports.Contains(enterpriseNotPublishedSystemReport.PK));
			AssertEquals("Collection Contains enterprisePublishedClientCreatedReport", false, testHolder.WebReports.Contains(enterprisePublishedClientCreatedReport.PK));
			AssertEquals("Collection Contains enterprisePublishedNotSystemReport", false, testHolder.WebReports.Contains(enterprisePublishedNotSystemReport.PK));

			AssertEquals("Collection Contains webNotPublishedClientCreatedReport", true, testHolder.WebReports.Contains(webNotPublishedClientCreatedReport.PK));
			AssertEquals("Collection Contains webNotPublishedSystemReport", true, testHolder.WebReports.Contains(webNotPublishedSystemReport.PK));
			AssertEquals("Collection Contains webPublishedClientCreatedReport", true, testHolder.WebReports.Contains(webPublishedClientCreatedReport.PK));
			AssertEquals("Collection Contains webPublishedSystemReport", true, testHolder.WebReports.Contains(webPublishedSystemReport.PK));

			List<ZString> modes = new List<ZString>();
			modes.Add("RepWhsReport");
			testHolder.Modes = modes;
			AssertEquals("Precondition: Collection has more than one item", true, testHolder.WebReports.Count > 0);

			AssertEquals("Collection Does not Contains enterpriseNotPublishedClientCreatedReport", false, testHolder.WebReports.Contains(enterpriseNotPublishedClientCreatedReport.PK));
			AssertEquals("Collection Does not Contains enterpriseNotPublishedSystemReport", false, testHolder.WebReports.Contains(enterpriseNotPublishedSystemReport.PK));
			AssertEquals("Collection Does not Contains enterprisePublishedClientCreatedReport", false, testHolder.WebReports.Contains(enterprisePublishedClientCreatedReport.PK));
			AssertEquals("Collection Does not Contains enterprisePublishedNotSystemReport", false, testHolder.WebReports.Contains(enterprisePublishedNotSystemReport.PK));

			AssertEquals("Collection Does not Contains webNotPublishedClientCreatedReport", false, testHolder.WebReports.Contains(webNotPublishedClientCreatedReport.PK));
			AssertEquals("Collection Does not Contains webNotPublishedSystemReport", false, testHolder.WebReports.Contains(webNotPublishedSystemReport.PK));
			AssertEquals("Collection Does not Contains webPublishedClientCreatedReport", false, testHolder.WebReports.Contains(webPublishedClientCreatedReport.PK));
			AssertEquals("Collection Contains webPublishedSystemReport", true, testHolder.WebReports.Contains(webPublishedSystemReport.PK));

			modes = new List<ZString>();
			modes.Add("RepCustomsReport");
			testHolder.Modes = modes;
			AssertEquals("Precondition: Collection has more than one item", true, testHolder.WebReports.Count > 0);

			AssertEquals("Collection Does not Contains enterpriseNotPublishedClientCreatedReport", false, testHolder.WebReports.Contains(enterpriseNotPublishedClientCreatedReport.PK));
			AssertEquals("Collection Does not Contains enterpriseNotPublishedSystemReport", false, testHolder.WebReports.Contains(enterpriseNotPublishedSystemReport.PK));
			AssertEquals("Collection Does not Contains enterprisePublishedClientCreatedReport", false, testHolder.WebReports.Contains(enterprisePublishedClientCreatedReport.PK));
			AssertEquals("Collection Does not Contains enterprisePublishedNotSystemReport", false, testHolder.WebReports.Contains(enterprisePublishedNotSystemReport.PK));

			AssertEquals("Collection Contains webNotPublishedClientCreatedReport", true, testHolder.WebReports.Contains(webNotPublishedClientCreatedReport.PK));
			AssertEquals("Collection Contains webNotPublishedSystemReport", true, testHolder.WebReports.Contains(webNotPublishedSystemReport.PK));
			AssertEquals("Collection Contains webPublishedClientCreatedReport", true, testHolder.WebReports.Contains(webPublishedClientCreatedReport.PK));
			AssertEquals("Collection Does not Contains webPublishedSystemReport", false, testHolder.WebReports.Contains(webPublishedSystemReport.PK));

			modes = new List<ZString>();
			modes.Add("RepCustomsReport");
			modes.Add("RepWhsReport");
			testHolder.Modes = modes;
			AssertEquals("Precondition: Collection has more than one item", true, testHolder.WebReports.Count > 0);

			AssertEquals("Collection Does not Contains enterpriseNotPublishedClientCreatedReport", false, testHolder.WebReports.Contains(enterpriseNotPublishedClientCreatedReport.PK));
			AssertEquals("Collection Does not Contains enterpriseNotPublishedSystemReport", false, testHolder.WebReports.Contains(enterpriseNotPublishedSystemReport.PK));
			AssertEquals("Collection Does not Contains enterprisePublishedClientCreatedReport", false, testHolder.WebReports.Contains(enterprisePublishedClientCreatedReport.PK));
			AssertEquals("Collection Does not Contains enterprisePublishedNotSystemReport", false, testHolder.WebReports.Contains(enterprisePublishedNotSystemReport.PK));

			AssertEquals("Collection Contains webNotPublishedClientCreatedReport", true, testHolder.WebReports.Contains(webNotPublishedClientCreatedReport.PK));
			AssertEquals("Collection Contains webNotPublishedSystemReport", true, testHolder.WebReports.Contains(webNotPublishedSystemReport.PK));
			AssertEquals("Collection Contains webPublishedClientCreatedReport", true, testHolder.WebReports.Contains(webPublishedClientCreatedReport.PK));
			AssertEquals("Collection Contains webPublishedSystemReport", true, testHolder.WebReports.Contains(webPublishedSystemReport.PK));
		}

		void CheckThatEnterpriseReportIsNotAvailableOnTheWeb(OrgContactWebUser user, ReportCommand enterpriseReport)
		{
			AssertEquals("The test data is not set up correctly. This test method is for enterprise (non-web) reports only",
				Core.Constants.StmMenuItemTypes.Documents, enterpriseReport.SU_MenuType);

			OrgSecurityContacts reportSecurityRight = FindReportSecurityRight(user.LoggedInUser, enterpriseReport);
			Assert("There shouldn't be a web security right for [" + enterpriseReport.SU_MenuName + "]", reportSecurityRight == null);
		}

		void CheckThatWebReportIsAvailableOnTheWeb(OrgContactWebUser user, ReportCommand webReport)
		{
			AssertEquals("The test data is not set up correctly. This test method is for web reports only",
				Core.Constants.StmMenuItemTypes.WebReports, webReport.SU_MenuType);

			OrgSecurityContacts reportSecurityRight = FindReportSecurityRight(user.LoggedInUser, webReport);
			Assert("There should be a web security right for [" + webReport.SU_MenuName + "]", reportSecurityRight != null);
		}

		#region Implementation

		OrgSecurityContacts FindReportSecurityRight(OrgContact user, ReportCommand report)
		{
			OrgSecurityContacts result = null;
			var rightCodeAndDescription = ReportsWebSecurityRights.GetSecurityRightCodeAndDescriptionForReport(report);
			string rightCode = rightCodeAndDescription != null ? rightCodeAndDescription.Code : string.Empty;

			foreach (OrgSecurityContacts right in user.SecurityRightsForBindingOnly)
			{
				if (right.Security.SecurityKey == rightCode)
				{
					result = right;
					break;
				}
			}

			return result;
		}

		ReportCommand CreateReportCommand(string module, string menuName, bool published, bool systemDefined, string menuType)
		{
			ReportCommand command = Factory.New<ReportCommand>();
			command.SU_MenuName = menuName;
			command.SU_BusinessContext = module;
			command.SU_IsSystemDefined = systemDefined;
			command.SU_IsPublished = published;
			command.SU_IsVisibleOnWeb = menuType == Core.Constants.StmMenuItemTypes.WebReports;
			command.SU_MenuType = menuType;
			return command;
		}

		ReportCommand FindReportCommand(string menuName)
		{
			ZQuery findReportQuery = new ZQuery(StmMenuItemSchema.SU_MenuName, menuName);
			ReportCommand result = Factory.LoadTop1<ReportCommand>(findReportQuery);

			if (result == null)
			{
				Fail("Unable to find Report Command to test: [" + menuName + "]");
			}

			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WebReportHolder(Factory, TestSiteUser);
		}

		OrgContactWebUser TestSiteUser
		{
			get
			{
				if (fTestSiteUser == null)
				{
					ZWebTestHelper helper = new ZWebTestHelper(Factory);
					fTestSiteUser = helper.TestSiteUser as OrgContactWebUser;
					helper.TestContact.OC_Email = "test@cargowise.com";
					helper.TestContact.SetHashedPassword("test");
					helper.TestContact.OC_WebAccessEnabled = true;
					helper.TestContact.Factory.Save();

					fTestSiteUser.Login(helper.TestOrg.OH_Code, helper.TestContact.OC_Email, helper.TestContact.PasswordForTesting);
					if (WebEnv.AppInstance is DummyHttpApplication)
					{
						((DummyHttpApplication)WebEnv.AppInstance).SetSiteUser(fTestSiteUser);
					}
				}
				return fTestSiteUser;
			}
		}
		OrgContactWebUser fTestSiteUser;

		#endregion
	}
}
