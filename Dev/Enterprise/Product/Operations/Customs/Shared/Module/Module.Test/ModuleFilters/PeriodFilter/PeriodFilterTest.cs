using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(PeriodFilter))]
	sealed class PeriodFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsEmpty()
		{
			Assert("Is Empty", Filter.IsEmpty);
			Filter.PeriodYear = 2015;
			Filter.PeriodMonth = 0;
			Assert("Is Empty", Filter.IsEmpty);
			Filter.PeriodYear = 2015;
			Filter.PeriodMonth = 0;
			Assert("Is Empty", Filter.IsEmpty);
			Filter.PeriodYear = 2015;
			Filter.PeriodMonth = 3;
			Assert("Is not Empty", !Filter.IsEmpty);
		}

		public void TestSerialisation()
		{
			Filter.PeriodYear = 2015;
			Filter.PeriodMonth = 3;
			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)Filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();
				AssertMultilineASCIIEquals("serialisation", SampleXml, writer.ToString());
			}
		}

		public void TestDeserilisation()
		{
			using (StringReader reader = new StringReader(SampleXml))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter"); // because we follow a broken pattern for reading xml.
				((IXmlSerializable)Filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement(); // because we follow a broken pattern for reading xml.
			}

			CombineAssertions(delegate
			{
				AssertEquals("Year", 2015, Filter.PeriodYear);
				AssertEquals("Month", 3, Filter.PeriodMonth);
			});
		}

		const string SampleXml =
			"<Filter>\r\n" +
			"  <Comparer>exact</Comparer>\r\n" +
			"  <Property />\r\n" +
			"  <Year>2015</Year>\r\n" +
			"  <Month>3</Month>\r\n" +
			"</Filter>\r\n" +
			"";

		PeriodFilter Filter
		{
			get
			{
				return filter ?? (filter = new PeriodFilter("Period", delegate
				{
					return new ZQuery();
				}));
			}
		}

		PeriodFilter filter;
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PeriodFilter("Period", delegate
			{ return new ZQuery(); });
		}
	}
}
