using Enterprise.Customs.TW.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	sealed class XmlHelperTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestSerializerKeepNewLineCharacters()
		{
			var input = "Drill bit Set Drill bit(13mm)*1 \nDrill bit(12.5mm)*1 Drill bit(12mm)*1 \nDrill bit(11.5mm)*1 \nDrill bit(11mm)*1";
			var expectXMLString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<DummySerializeObjectForTest xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">\r\n  <TestString>Drill bit Set Drill bit(13mm)*1 \nDrill bit(12.5mm)*1 Drill bit(12mm)*1 \nDrill bit(11.5mm)*1 \nDrill bit(11mm)*1</TestString>\r\n</DummySerializeObjectForTest>";
			NUnit.Framework.Assert.That(XmlHelper.Serializer(typeof(DummySerializeObjectForTest), new DummySerializeObjectForTest() { TestString = input }), NUnit.Framework.Is.EqualTo(expectXMLString));
		}

		[ExpectNoExceptions]
		public void TestSerializer_SchemaLocation()
		{
			var expectXMLString = "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"yes\"?>\r\n<DummySerializeObjectIncludingSChemaLocationForTest xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\r\n  xsi:schemaLocation=\"urn:wco:datamodel:TW:NX101:R-01-00 NX101.xsd\" />";
			NUnit.Framework.Assert.That(XmlHelper.Serializer(typeof(DummySerializeObjectIncludingSChemaLocationForTest), new DummySerializeObjectIncludingSChemaLocationForTest()), NUnit.Framework.Is.EqualTo(expectXMLString));
		}
	}

	public class DummySerializeObjectForTest
	{
		public DummySerializeObjectForTest()
		{
		}

		public string TestString { get; set; }
	}

	public class DummySerializeObjectIncludingSChemaLocationForTest : DummySerializeObjectForTest
	{
		public DummySerializeObjectIncludingSChemaLocationForTest()
		{
		}

		[System.Xml.Serialization.XmlAttribute("schemaLocation", Namespace = "http://www.w3.org/2001/XMLSchema-instance")]
		public string XsiSchemaLocation = "urn:wco:datamodel:TW:NX101:R-01-00 NX101.xsd";
	}
}
