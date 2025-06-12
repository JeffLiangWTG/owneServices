using System.Reflection;
using CargoWise.eHub.Portal.IntegrationTests.Attributes;
using CargoWise.eHub.Selenium.IntegrationTests.Core;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace CargoWise.eHub.Portal.IntegrationTests.Views
{
	[TestFixture]
	[WithPortalService]
	public class GBCustomsPentantClientRegistrationTests : SeleniumTestBase
	{
		protected override string TestDataLocation { get { return ".Views.ClientRegistrationTests.GBCustomsPentant.Data."; } }
		protected override string TestDataSchemaLocation { get { return ".TestBases.Schemas."; } }

		protected override void SetUpField()
		{
			LinkPaths = new string[]
			{
				// Site base
				"/PlugIns/bootstrap/3.3.7/bootstrap.min.css",
				"/Content/Images/favicon.ico",
				"/Content/Main.css",
				"/Content/eHubAdmin.css",
				"/Content/Site.css",
				"/PlugIns/jQueryUI/1.8.9/jquery-ui.css",
				// Index page
				"/PlugIns/JeeGooContext/jquery.jeegoocontext.css",
				"/PlugIns/jQueryUI/jquery-ui-1.8.11.custom.css",
				"/PlugIns/jqGrid460/ui.jqgrid.css",
				"/PlugIns/jquery-ui-timepicker-addon/1.6.3/jquery-ui-timepicker-addon.min.css"
			};
			PreservedNodesXpaths = new string[]
			{
				".//select[@id='registrationTypesList']",
				".//div[@class='ui-jqgrid ui-widget ui-widget-content ui-corner-all']"
			};
			ReferencePath = "../../../../Portal";
			base.SetUpField();
		}

		[Test]
		public void Test_GBCustomsPentant()
		{
			var endPoint = WithPortalServiceAttribute.Current.GetHttpEndPoint("ClientRegistrations");
			Driver.Url = endPoint.ToString();

			var selectOption = new SelectElement(Driver.FindElement(By.Id("registrationTypesList")));
			WaitUntilFindElement(By.XPath("//option[@value='00000000-0000-0000-1111-000000000000']"));
			CompareHtmlDocuments(ReadEmbeddedHtml("Views.ClientRegistrationTests.GBCustomsPentant.ExpectedPage.ClientRegistration_BeforeSelectPage.html"), SnapShotPage());

			selectOption.SelectByText("GBCustoms-Pentant - Pentent Inbound Mesage Web Service Authorization and URL");
			WaitUntilFindElement(By.Id("registrationsTable"));
			WaitUntilFindElement(By.Id("00000000-3333-0000-0000-000000000000"));

			CompareHtmlDocuments(ReadEmbeddedHtml("Views.ClientRegistrationTests.GBCustomsPentant.ExpectedPage.ClientRegistration_FinalPage.html"), SnapShotPage());
		}
	}
}
