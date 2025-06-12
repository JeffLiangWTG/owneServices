using System.Data.SqlClient;
using CargoWise.eHub.Portal.IntegrationTests.Attributes;
using CargoWise.eHub.Selenium.IntegrationTests.Core;
using CargoWise.eServices.TestHelpers.Database.Common;
using NUnit.Framework;
using OpenQA.Selenium;

namespace CargoWise.eHub.Portal.IntegrationTests.Views
{
    [TestFixture]
	[WithPortalService]
	public class ClientTests : SeleniumTestBase
	{
		protected override string TestDataLocation { get { return ".Views.ClientTests.Data."; } }
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
                "/PlugIns/jQueryUI/1.8.9/jquery-ui.css"
            };
            ReferencePath = "../../../../Portal";
            base.SetUpField();
        }

        [Test]
		public void Test_eHubClient()
		{
			var endPoint = WithPortalServiceAttribute.Current.GetHttpEndPoint("Client/CreateThirdPartyPartner");
			Driver.Url = endPoint.ToString();

            Driver.FindElement(By.Id("Id")).SendKeys("WTL666JAY");
			Driver.FindElement(By.Id("Password")).SendKeys("Test");
			Driver.FindElement(By.Id("OrgCode")).SendKeys("WISTESSYD");
			Driver.FindElement(By.Id("Email")).SendKeys("Test@test.com");
			Driver.FindElement(By.XPath("//input[@type='submit' and @value='Create']")).Click();

            using (SqlConnection connection = new SqlConnection(SqlServerHelper.GetAdminConnectionString("eHubTransactions")))
            using (SqlCommand command = new SqlCommand("select * from eHubClient where CC_ID = 'WTL666JAY'", connection))
            {
                connection.Open();
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Assert.AreEqual(true, (bool)reader["CC_PermitInboxSender"]);
                        Assert.AreEqual(true, (bool)reader["CC_PermitInboxRecipient"]);
                    }
                }
            }

            CompareHtmlDocuments(ReadEmbeddedHtml("Views.ClientTests.ExpectedPage.AfterCreateThirdPartyPartnerPage.html"), SnapShotPage());
        }
	}
}
