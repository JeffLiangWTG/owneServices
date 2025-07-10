using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.MarketingManager.WebVoting.Testing
{
	class GlobalTest : ZGlobalTest
	{
		public void TestDefaultPage()
		{
			AssertEquals("~/Default.aspx", AppInstance.DefaultPage);
		}

		public void TestLoginPage()
		{
			AssertEquals("~/Login.aspx", AppInstance.LoginPage);
		}

		Global AppInstance
		{
			get
			{
				if (fAppInstance == null)
				{
					fAppInstance = new Global();
				}
				return fAppInstance;
			}
		}

		Global fAppInstance;

		protected override int NumberOfLocations
		{
			get { return 0; }
		}

		protected override string WebConfigPath => GetSupplementaryContentPath("Enterprise", "Product", "Operations", "MarketingManager", "WebVoting", "Web.config");

		public override void AssertLocations(System.Xml.XmlNodeList locations)
		{
			AssertEquals("Error.aspx", locations[0].Attributes["path"].Value);
			AssertEquals("*", locations[0].SelectSingleNode("system.web/authorization/allow").Attributes["users"].Value);
		}
	}
}
