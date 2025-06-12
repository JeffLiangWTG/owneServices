using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace eHub.DatImplementation.MavenDeployment
{
	public class PomFileParser
	{
		public static bool CheckPomFileForProfileName(XDocument pomFile, string profileName)
		{
			if (profileName == null)
			{
				return false;
			}

			XmlNamespaceManager xmlPomNamespaceManager = new XmlNamespaceManager(new NameTable());
			xmlPomNamespaceManager.AddNamespace("x", "http://maven.apache.org/POM/4.0.0");
			var listOfProfilesInPom = pomFile.XPathSelectElements("/x:project/x:profiles/x:profile/x:id", xmlPomNamespaceManager).Select(xElement => xElement.Value).ToList();
			return listOfProfilesInPom.Contains(profileName);
		}

		public static string GetPasswordFromPomXDocument(XDocument pomXDocument, string profileName)
		{
			XmlNamespaceManager pomXmlNamespaceManager = new XmlNamespaceManager(new NameTable());
			pomXmlNamespaceManager.AddNamespace("x", "http://maven.apache.org/POM/4.0.0");

			XElement passwordElement;
			if (profileName == null)
			{
				passwordElement = pomXDocument.XPathSelectElements("/x:project/x:properties/x:tomcat_password", pomXmlNamespaceManager).FirstOrDefault();
			}
			else
			{
				passwordElement = pomXDocument.XPathSelectElements($"/x:project/x:profiles/x:profile[x:id='{profileName}']/x:properties/x:tomcat_password", pomXmlNamespaceManager).FirstOrDefault();
				if (passwordElement == null)
				{
					passwordElement = pomXDocument.XPathSelectElements("/x:project/x:properties/x:tomcat_password", pomXmlNamespaceManager).FirstOrDefault();
				}
			}

			return passwordElement?.Value ?? null;
		}
	}
}
