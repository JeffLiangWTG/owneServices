using System.Web;
using CargoWise.Application;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.ImporterSecurityFiling;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Moq;

namespace Enterprise.Tracking.Web.Testing
{
	[HttpContextEnabledTest]
	sealed class ImporterSecurityFilingDetailsTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.ImporterSecurityFilingDetails;
		}

		public void TestDeleteISF()
		{
			ImporterSecurityFilingDetailsForTest testPage = TestPage as ImporterSecurityFilingDetailsForTest;
			TrackingSiteUser user = testPage.SiteUser;
			AssertNotNull(user);

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "X";
			OrgContact contact = org.Contacts.AddNew();
			contact.OC_Email = "user@user.com";
			contact.SetHashedPassword("password");
			contact.OC_WebAccessEnabled = true;

			OrgContact contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "user2";
			contact2.OC_Email = "user2@user.com";
			contact2.SetHashedPassword("password");
			contact2.OC_WebAccessEnabled = true;

			OrgSecurity orgRight = org.SecurityRights.AddNew();
			orgRight.OX_Granted = true;
			orgRight.OX_SecurityItemName = WebSecurityRightsList.WebISFDelete.Code;
			OrgSecurityContacts user2Right = contact2.SecurityRightsForBindingOnly.AddNew();
			user2Right.OZ_OX = orgRight.PK;
			user2Right.OZ_Granted = false;

			Factory.Save();

			user.Login(org.OH_Code, "user@user.com", "password");

			TrackingCusISFHeader source = testPage.DataSource as TrackingCusISFHeader;
			source.BF_CustomsStatus = MessageStatusList.Codes.AwaitingISFAdd;
			testPage.ForTest_OnLoad();

			Assert(testPage.ForTest_DeleteISF.Visible);
			Assert(!testPage.ForTest_DeleteISF.Enabled);

			source.BF_CustomsStatus = MessageStatusList.Codes.ClearWithWarningISFAdd;
			source.BF_CustomsReference = "bla";
			testPage.ForTest_OnLoad();

			Assert(testPage.ForTest_DeleteISF.Visible);
			Assert(testPage.ForTest_DeleteISF.Enabled);

			user.Logout();
			user.Login(org.OH_Code, "user2@user.com", "password");

			testPage.ForTest_OnLoad();
			Assert(!testPage.ForTest_DeleteISF.Visible);
		}

		[HttpContextEnabledTest]
		public void TestDeleteISF_Success()
		{
			var page = (ImporterSecurityFilingDetailsForTest)TestPage;
			page.SetupPageForTesting();
			var header = (TrackingCusISFHeader)page.DataSource;

			var webMessageSenderMock = new Mock<Integration.Customs.US.ISF.IUSISFWebMessageSender>(MockBehavior.Strict);
			webMessageSenderMock.Setup(x => x.SendDeleteMessage(header.PK)).Returns<string>(null);
			ObjectFactory.Substitute("ISF.IUSISFWebMessageSender", webMessageSenderMock.Object);

			page.ForTest_DeleteISF_Click();

			webMessageSenderMock.VerifyAll();
			AssertNull(page.Session[ZPage.zPageCustomAlertMessageIndexer]);
			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals(string.Format("{0}?Ref={1}", page.AppInstance.ISFDetailsPage, header.PK), HttpContext.Current.Response.RedirectLocation);
		}

		[HttpContextEnabledTest]
		public void TestDeleteISF_Error()
		{
			var page = (ImporterSecurityFilingDetailsForTest)TestPage;
			page.SetupPageForTesting();
			var header = (TrackingCusISFHeader)page.DataSource;

			var webMessageSenderMock = new Mock<Integration.Customs.US.ISF.IUSISFWebMessageSender>(MockBehavior.Strict);
			webMessageSenderMock.Setup(x => x.SendDeleteMessage(header.PK)).Returns("Error message");
			ObjectFactory.Substitute("ISF.IUSISFWebMessageSender", webMessageSenderMock.Object);

			page.ForTest_DeleteISF_Click();

			webMessageSenderMock.VerifyAll();
			AssertEquals("Error message", page.Session[ZPage.zPageCustomAlertMessageIndexer]);
			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals(string.Format("{0}?Ref={1}", page.AppInstance.ISFDetailsPage, header.PK), HttpContext.Current.Response.RedirectLocation);
		}

		public void TestShowContainersGrid()
		{
			ImporterSecurityFilingDetailsForTest testPage = TestPage as ImporterSecurityFilingDetailsForTest;
			AssertNotNull("TestPage should be proper type", testPage);
			testPage.SetupPageForTesting();

			TrackingCusISFHeader source = testPage.DataSource as TrackingCusISFHeader;
			AssertNotNull("DataSource should be TrackingCusISFHeader", source);

			source.MainShipToParty.E2_GovRegNum = "123";

			source.BF_EntryType = SubmissionTypeList.Codes.ISF10;
			testPage.SetupPageForTest();

			Assert(testPage.ForTest_ShipToGovRegNoArea.Visible);
			Assert(!testPage.ForTest_SellingPartyGovRegNoArea.Visible);

			Assert(testPage.ForTest_ISF10Details.Visible);
			Assert(!testPage.ForTest_RoutingDetails.Visible);
			Assert(!testPage.ForTest_ISF5Details.Visible);
			Assert(testPage.ForTest_SellingPartyAddressHolder.Visible);
			Assert(testPage.ForTest_BuyingPartyAddressHolder.Visible);
			Assert(testPage.ForTest_StuffingLocationHolder.Visible);
			Assert(testPage.ForTest_ConsolidatorAddressHolder.Visible);
			Assert(!testPage.ForTest_BookingPartyAddressHolder.Visible);

			source.BF_EntryType = SubmissionTypeList.Codes.ISF5;
			testPage.SetupPageForTest();

			Assert(!testPage.ForTest_ISF10Details.Visible);
			Assert(!testPage.ForTest_RoutingDetails.Visible);
			Assert(testPage.ForTest_ISF5Details.Visible);
			Assert(!testPage.ForTest_SellingPartyAddressHolder.Visible);
			Assert(!testPage.ForTest_BuyingPartyAddressHolder.Visible);
			Assert(!testPage.ForTest_StuffingLocationHolder.Visible);
			Assert(!testPage.ForTest_ConsolidatorAddressHolder.Visible);
			Assert(testPage.ForTest_BookingPartyAddressHolder.Visible);
		}

		#region Overrides

		protected override System.Web.UI.Control GetNewControl()
		{
			return new ImporterSecurityFilingDetailsForTest();
		}

		protected override BooleanRegistryItem UseWebModule
		{
			get { return WebDataRegistry.Instance.UseWebISFModule; }
		}

		protected override WebSecurityRight SiteUserSecurityRight
		{
			get { return WebSecurityRightsList.WebISFView; }
		}

		protected override void SetUp()
		{
			base.SetUp();
			((ImporterSecurityFilingDetailsForTest)TestPage).SetupPageForTesting();
			foreach (OrgSecurityContacts securityContact in TestPage.SiteUser.LoggedInUser.SecurityRightsForBindingOnly)
			{
				if (securityContact.Security.SecurityKey == WebSecurityRightsList.WebISFView.Code)
				{
					securityContact.OZ_Granted = true;
					break;
				}
			}
		}
		#endregion
	}
}
