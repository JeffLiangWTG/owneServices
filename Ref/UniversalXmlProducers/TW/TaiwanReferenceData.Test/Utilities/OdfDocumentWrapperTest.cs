using System.IO;
using System.Xml;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.TaiwanReferenceData.Test
{
	public class OdfDocumentWrapperTest
	{
		[Test]
		public void TestGetTextLineThroughStyleNames()
		{
			var filename = Path.Combine(FolderHelper.GetBinFolder(), "doc/RefCusCodeList/OdtContent.xml");
			var document = new XmlDocument();
			document.Load(filename);
			var nsManager = OdfHelper.GetXmlNamespaceManager(document);
			var wrapper = new OdfDocumentWrapper(document, nsManager);
			var styleNames = wrapper.TextLineThroughStyleNames;
			Assert.AreEqual("P14,P15,P16", string.Join(",", styleNames));
		}
	}
}
