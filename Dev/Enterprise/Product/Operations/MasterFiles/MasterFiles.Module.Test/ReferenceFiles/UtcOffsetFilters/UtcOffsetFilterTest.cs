using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(UtcOffsetFilter))]
	sealed class UtcOffsetFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsEmpty()
		{
			AssertFilterEmpty(string.Empty, string.Empty, true);
			AssertFilterEmpty(string.Empty, "1", false);
			AssertFilterEmpty("0", string.Empty, false);
			AssertFilterEmpty("0", "1", false);
		}

		void AssertFilterEmpty(ZString utcOffsetFrom, ZString utcOffsetTo, bool isEmpty)
		{
			var filter = (UtcOffsetFilter)GetNewBusinessObject();
			filter.UtcOffsetFrom = utcOffsetFrom;
			filter.UtcOffsetTo = utcOffsetTo;

			AssertEquals(isEmpty, filter.IsEmpty);
		}

		const string filterXml = "<Filter>\r\n  <UtcOffsetFrom>-600</UtcOffsetFrom>\r\n  <UtcOffsetTo>600</UtcOffsetTo>\r\n</Filter>";

		public void TestSerialisation()
		{
			var filter = (UtcOffsetFilter)GetNewBusinessObject();
			filter.UtcOffsetFrom = "-600";
			filter.UtcOffsetTo = "600";

			using (var writer = new StringWriter())
			using (var xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();

				AssertMultilineASCIIEquals(filterXml, writer.ToString());
			}
		}

		public void TestDeserilisation()
		{
			var filter = (UtcOffsetFilter)GetNewBusinessObject();

			using (var reader = new StringReader(filterXml))
			using (var xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement();
			}

			AssertEquals("-600", filter.UtcOffsetFrom);
			AssertEquals("600", filter.UtcOffsetTo);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new UtcOffsetFilter("Desc", (a, b) => new ZQuery(), UtcOffsetList);
		}

		List<ZShort> UtcOffsetList => new List<ZShort> { -660, -600, -540, -480, -420, -360, -300, -240, -180, -120, -60, 0, 60, 120, 180, 240, 300, 360, 420, 480, 540, 600, 660, 720, 780, 840 };
	}
}
