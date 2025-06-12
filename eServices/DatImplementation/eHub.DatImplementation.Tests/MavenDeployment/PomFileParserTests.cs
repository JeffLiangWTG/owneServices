using System.Reflection;
using System.Xml.Linq;
using NUnit.Framework;

namespace eHub.DatImplementation.MavenDeployment.Tests
{
	[TestFixture]
	public class PomFileParserTests
	{
		[TestCase("test", true)]
		[TestCase("ThisIsNotAProfile", false)]
		[TestCase(null, false)]
		public void TestCheckPomFileForProfileName(string profileName, bool expectedOutput)
		{
			var pomFilePath = Assembly.GetExecutingAssembly().GetName().Name + ".MavenDeployment.Deployment.TestProject.deploy.pom.xml";
			var pomFile = Assembly.GetExecutingAssembly().GetManifestResourceStream(pomFilePath);
			var pomXDocument = XDocument.Load(pomFile);

			Assert.AreEqual(expectedOutput, PomFileParser.CheckPomFileForProfileName(pomXDocument, profileName));
		}

		[Test]
		public void TestGetPasswordFromPomXDocument()
		{
			var pomFilePath = Assembly.GetExecutingAssembly().GetName().Name + ".MavenDeployment.Deployment.TestProject.deploy.pom.xml";
			var pomFile = Assembly.GetExecutingAssembly().GetManifestResourceStream(pomFilePath);
			var pomXDocument = XDocument.Load(pomFile);

			Assert.That(PomFileParser.GetPasswordFromPomXDocument(pomXDocument, "test"), Is.EqualTo("WijUQcbS3MtCuZ7Y25Qb664DBxy5l4J68/+C/8diYWXOfbMoVauX7syB6ODksNA64AbkjxLzhm4DzyIpL2NygeT3okIBjquhJQ31M7ri8u36HE44LUzecNdWH3zgXOHdjaaMdGLy0cUMSVZy3GFwfbVoxWK70HFzkeim04r2gJ8="));
			Assert.That(PomFileParser.GetPasswordFromPomXDocument(pomXDocument, "production1"), Is.EqualTo("ehubrocks"));
			Assert.That(PomFileParser.GetPasswordFromPomXDocument(pomXDocument, "production2"), Is.EqualTo("nRge4G48c2V0ACZshjdbc0v/d/v9lincl6r77Z6R6PoNwKb05jacTHjeJz2J+8hamvzufxqIOC74FYBsIiA2uhGZsfXnIz83NWpuSly+/QHQwDbjJucMERcMNgRwyLPgjqinf6r4sGcuiSJaBrgcURbBKu41dr5jqg2RjzHJ3b4="));
			Assert.That(PomFileParser.GetPasswordFromPomXDocument(pomXDocument, null), Is.EqualTo("ehubrocks"));
		}
	}
}
