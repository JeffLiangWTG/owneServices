using System.Xml.Linq;
using System.Xml.XPath;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test
{
	public class WebConfigTest : TestCase
	{
		string webConfigPath
		{
			get { return GetSupplementaryContentPath("Enterprise", "Services", "Scim", "Enterprise.Services.Scim.Api", "Web.config"); }
		}

		public void TestCustomErrorsModeRemoteOnlyWithRedirect()
		{
			var webConfigXml = XDocument.Load(webConfigPath);
			var customErrors = webConfigXml.XPathSelectElement("//system.web/customErrors");

			AssertEquals("RemoteOnly", (string)customErrors.Attribute("mode"));
			AssertEquals("~/Error.aspx", (string)customErrors.Attribute("defaultRedirect"));
		}
	}
}
