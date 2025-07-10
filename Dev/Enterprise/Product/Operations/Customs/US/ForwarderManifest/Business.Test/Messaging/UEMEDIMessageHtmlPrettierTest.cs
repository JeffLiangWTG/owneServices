using System.IO;
using System.Reflection;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.ForwarderManifest.Business.Test
{
	[TestedType(typeof(UEMEDIMessage))]
	public class UEMEDIMessageHtmlPrettierTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMessageInterpretationHtmlPrettier()
		{
			var testFilePath = "Enterprise.Customs.US.ForwarderManifest.Business.Test.Messaging.HtmlPrettier.TestFiles.TestMessage.xml";
			var expectedFilePath = "Enterprise.Customs.US.ForwarderManifest.Business.Test.Messaging.HtmlPrettier.TestFiles.ExpectedHtml.html";
			var assembly = Assembly.GetExecutingAssembly();
			var testFileString = GetFileString(testFilePath);
			var expectedHtml = GetFileString(expectedFilePath).Replace("\r\n", string.Empty).Replace("\t", string.Empty);

			var actualHtml = new UEMEDIMessageHtmlPrettier().PrettyHtml(testFileString);
			AssertEquals(expectedHtml, actualHtml);
			string GetFileString(string fileName)
			{
				using (var stream = assembly.GetManifestResourceStream(fileName))
				{
					using (var reader = new StreamReader(stream, true))
					{
						return reader.ReadToEnd();
					}
				}
			}
		}
	}
}
