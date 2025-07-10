using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.RefDbRepo.MXReferenceData.Services;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.MXReferenceData.Tests
{
	[TestFixture]
	public class XMLReverterTestcs
	{
		[Test]
		public void TestRevertAndUnzipXML()
		{
			using (var zipInverted = Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.Utils.TestFiles.Input.CTARC_RECARG.zip"))
			using (StreamReader expectedXmlStream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("CargoWise.RefDbRepo.MXReferenceData.Tests.Utils.TestFiles.Output.CTARC_RECARG.xml"), Encoding.UTF8))
			using (var memoryStream = new MemoryStream())
			{
				zipInverted.CopyTo(memoryStream);
				using (var memoryStreamReversed = new MemoryStream(memoryStream.ToArray().Reverse().ToArray()))
				{
					var xml = XMLReverter.RevertAndUnzipXML(memoryStreamReversed);
					var expectedXml = expectedXmlStream.ReadToEnd();
					Assert.That(xml, Is.EqualTo(expectedXml));
				}
			}
		}
	}
}
