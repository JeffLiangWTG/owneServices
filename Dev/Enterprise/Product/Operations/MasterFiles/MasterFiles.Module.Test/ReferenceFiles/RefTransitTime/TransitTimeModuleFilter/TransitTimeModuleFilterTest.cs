using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(TransitTimeModuleFilter))]
	sealed class TransitTimeModuleFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsEmpty()
		{
			var filter = (TransitTimeModuleFilter)GetNewBusinessObject();
			AssertFilterEmpty(filter, 3, 0, false);
			AssertFilterEmpty(filter, 0, 5, false);
			AssertFilterEmpty(filter, 0, 0, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseSystemTimeSpanTypeForDuration", Justification = "Baseline")]
		void AssertFilterEmpty(TransitTimeModuleFilter filter, int days, int hours, bool isEmpty)
		{
			filter.TransitDays = days;
			filter.TransitHours = hours;
			AssertEquals(isEmpty, filter.IsEmpty);
		}

		public void TestClear()
		{
			var filter = (TransitTimeModuleFilter)GetNewBusinessObject();
			filter.ComparisonOperator = "Less than";
			filter.TransitDays = 5;
			filter.TransitHours = 10;

			filter.Clear();

			AssertEquals("Equals", filter.ComparisonOperator);
			AssertEquals(0, filter.TransitDays);
			AssertEquals(0, filter.TransitHours);
		}

		public void TestSerialization()
		{
			var filter = (TransitTimeModuleFilter)GetNewBusinessObject();
			filter.ComparisonOperator = "Greater than";
			filter.TransitDays = 7;
			filter.TransitHours = 12;

			using (var writer = new StringWriter())
			using (var xmlWriter = new XmlTextWriter(writer))
			{
				xmlWriter.Formatting = Formatting.Indented;
				xmlWriter.WriteStartElement("Filter");
				((IXmlSerializable)filter).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();

				AssertMultilineASCIIEquals("", filterXml, writer.ToString());
			}
		}

		public void TestDeserialization()
		{
			var filter = (TransitTimeModuleFilter)GetNewBusinessObject();

			using (var reader = new StringReader(filterXml))
			using (var xmlReader = new XmlTextReader(reader))
			{
				xmlReader.WhitespaceHandling = WhitespaceHandling.None;
				xmlReader.MoveToContent();
				xmlReader.ReadStartElement("Filter");
				((IXmlSerializable)filter).ReadXml(xmlReader);
				xmlReader.ReadEndElement();
			}

			AssertEquals("Greater than", filter.ComparisonOperator);
			AssertEquals(7, filter.TransitDays);
			AssertEquals(12, filter.TransitHours);
		}

		const string filterXml = @"<Filter>
  <ComparisonOperator>Greater than</ComparisonOperator>
  <TransitDays>7</TransitDays>
  <TransitHours>12</TransitHours>
</Filter>";

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TransitTimeModuleFilter("desc", RefTransitTimeSchema.RTT_TransitHours);
		}
	}
}
