using System.IO;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(AllocationMethodDefaultRule))]
	sealed class AllocationMethodDefaultRuleTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSerialisation()
		{
			const string ExpectedXML =
				"<Rule>" +
					"<Country>NZ</Country>" +
					"<AllocationMethod>ORI</AllocationMethod>" +
				"</Rule>" +
				"";

			using (StringWriter writer = new StringWriter())
			using (XmlTextWriter xmlWriter = new XmlTextWriter(writer))
			{
				AllocationMethodDefaultHeader header = new AllocationMethodDefaultHeader(Factory);
				AllocationMethodDefaultRule rule = header.Rules.AddNew();
				rule.CountryCode = "NZ";
				rule.AllocationMethod = AllocationMethodList.Codes.Origin;

				xmlWriter.WriteStartElement("Rule");
				((IXmlSerializable)rule).WriteXml(xmlWriter);
				xmlWriter.WriteEndElement();
				xmlWriter.Flush();

				AssertMultilineASCIIEquals("", ExpectedXML.Replace("><", ">\r\n<"), writer.ToString().Replace("><", ">\r\n<"));
			}

			using (StringReader reader = new StringReader(ExpectedXML))
			using (XmlTextReader xmlReader = new XmlTextReader(reader))
			{
				AllocationMethodDefaultHeader header = new AllocationMethodDefaultHeader(Factory);
				AllocationMethodDefaultRule rule = header.Rules.AddNew();

				((IXmlSerializable)rule).ReadXml(xmlReader);

				AssertEquals("NZ", rule.CountryCode);
				AssertEquals(AllocationMethodList.Codes.Origin, rule.AllocationMethod);
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AllocationMethodDefaultRule(Factory);
		}

		#endregion
	}
}
