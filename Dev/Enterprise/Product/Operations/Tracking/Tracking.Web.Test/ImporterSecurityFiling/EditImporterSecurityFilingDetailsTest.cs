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
	sealed class EditImporterSecurityFilingDetailsTest : BasePageWithAuthorisationTest
	{
		protected override string GetExpectedPageName()
		{
			return WebTracker.Pages.EditImporterSecurityFiling;
		}

		public void TestTextTransformFields()
		{
			var testPage = TestPage as EditImporterSecurityFilingDetailsForTest;
			AssertNotNull(testPage);
			AssertNotNull(testPage.ReferenceTestGrid);

			testPage.SetupPageForTest();
			AssertEquals(2, testPage.ReferenceTestGrid.Columns.Count);
			var testColumn = testPage.ReferenceTestGrid.Columns[1] as ZTemplateColumn;
			AssertNotNull(testColumn);
			AssertEquals("Number", testColumn.HeaderText);
			AssertEquals(TextTransformOptions.UpperCase, testColumn.TextTransform);
		}

		public void TestSendButtonAvailability()
		{
			EditImporterSecurityFilingDetailsForTest testPage = TestPage as EditImporterSecurityFilingDetailsForTest;
			AssertNotNull("TestPage should be proper type", testPage);
			testPage.SetupPageForTesting();

			TrackingCusISFHeader source = testPage.DataSource as TrackingCusISFHeader;
			AssertNotNull("DataSource should be TrackingCusISFHeader", source);

			source.BF_CustomsStatus = MessageStatusList.Codes.AwaitingISFAdd;
			testPage.SetSendButtonAvailabilityForTest();

			Assert(!testPage.ForTest_SendISF.Enabled);
			AssertEquals(testPage.ForTest_SendISF.ToolTip, "Unable to send while awaiting response");

			source.BF_CustomsStatus = MessageStatusList.Codes.ClearWithWarningISFAdd;
			testPage.SetSendButtonAvailabilityForTest();
			Assert(testPage.ForTest_SendISF.Enabled);
			AssertEquals(testPage.ForTest_SendISF.ToolTip, "Save and Send");
		}

		[HttpContextEnabledTest]
		public void TestSendISFMessage_Success()
		{
			var page = (EditImporterSecurityFilingDetailsForTest)TestPage;
			page.SetupPageForTesting();
			var header = (TrackingCusISFHeader)page.DataSource;

			var webMessageSenderMock = new Mock<Integration.Customs.US.ISF.IUSISFWebMessageSender>(MockBehavior.Strict);
			webMessageSenderMock.Setup(x => x.SendUpsertMessage(header.PK)).Returns<string>(null);
			ObjectFactory.Substitute("ISF.IUSISFWebMessageSender", webMessageSenderMock.Object);

			page.ForTest_SendISFMessage();

			webMessageSenderMock.VerifyAll();
			AssertNull(page.Session[ZPage.zPageCustomAlertMessageIndexer]);
			Assert(HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals(string.Format("{0}?Ref={1}", page.AppInstance.ISFDetailsPage, header.PK), HttpContext.Current.Response.RedirectLocation);
		}

		[HttpContextEnabledTest]
		public void TestSendISFMessage_Error()
		{
			var page = (EditImporterSecurityFilingDetailsForTest)TestPage;
			page.SetupPageForTesting();
			var header = (TrackingCusISFHeader)page.DataSource;

			var webMessageSenderMock = new Mock<Integration.Customs.US.ISF.IUSISFWebMessageSender>(MockBehavior.Strict);
			webMessageSenderMock.Setup(x => x.SendUpsertMessage(header.PK)).Returns("Error message");
			ObjectFactory.Substitute("ISF.IUSISFWebMessageSender", webMessageSenderMock.Object);

			page.ForTest_SendISFMessage();

			webMessageSenderMock.VerifyAll();
			AssertEquals("Error message", page.Session[ZPage.zPageCustomAlertMessageIndexer]);
			Assert(!HttpContext.Current.Response.IsRequestBeingRedirected);
		}

		[HttpContextEnabledTest]
		public void TestSendISFMessage_CantSend()
		{
			var page = (EditImporterSecurityFilingDetailsForTest)TestPage;
			page.SetupPageForTesting();
			page.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var header = (TrackingCusISFHeader)page.DataSource;

			page.ForTest_SendISFMessage();

			AssertEquals("You are not authorized to send ISF.", page.Session[ZPage.zPageCustomAlertMessageIndexer]);
			Assert(!HttpContext.Current.Response.IsRequestBeingRedirected);
		}

		#region Overrides

		protected override System.Web.UI.Control GetNewControl()
		{
			return new EditImporterSecurityFilingDetailsForTest();
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
			((EditImporterSecurityFilingDetailsForTest)TestPage).SetupPageForTesting();
			var findCount = 0;
			foreach (OrgSecurityContacts securityContact in TestPage.SiteUser.LoggedInUser.SecurityRightsForBindingOnly)
			{
				if (securityContact.Security.SecurityKey == WebSecurityRightsList.WebISFView.Code || securityContact.Security.SecurityKey == WebSecurityRightsList.WebISFAddEdit.Code || securityContact.Security.SecurityKey == WebSecurityRightsList.WebISFSend.Code)
				{
					findCount++;
					securityContact.OZ_Granted = true;
				}

				if (findCount == 3)
				{
					break;
				}
			}
		}

		#endregion
	}
}
