using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Xsd.Testing
{
	public abstract class XsdSchemaValidationTest : TestCaseWithFactory
	{
		public void TestDBColumnHasCorrectXsdSchema()
		{
			var bizObj = GetFullyPopulatedBizObj();
			var xsdData = new BusinessObjectXsdGenerator().GenerateXsd(bizObj, XsdVersion, XsdNamespace);
			AssertMultilineASCIIEquals("Xsd Data", GetExpectedXsdData(), xsdData);
			AssertStartsWith("Should contain LazyLoading Schema", @"<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""placeholder"">
    <xs:complexType>
      <xs:simpleContent>
        <xs:extension base=""xs:string"">
          <xs:attribute name=""LazyLoading"" type=""xs:string"" fixed=""Yes"">
          </xs:attribute>
        </xs:extension>
      </xs:simpleContent>
    </xs:complexType>
  </xs:element>
</xs:schema>", xsdData);
			xsdData = xsdData.Substring(xsdData.IndexOf(@"<xs:schema targetNamespace=""" + XsdNamespace));
			var xmlData = bizObj.Serialize(XsdNamespace);
			AssertMultilineASCIIEquals("XML Data", GetExpectedXmlData(), xmlData);

			var schemas = new XmlSchemaSet();
			schemas.Add(XsdNamespace, XmlReader.Create(new StringReader(xsdData)));

			var doc = XDocument.Load(new StringReader(xmlData));
			var error = new ZStringBuilder();
			doc.Validate(schemas, (o, e) =>
			{
				error.AppendLine(e.Message);
			});
			AssertMultilineASCIIEquals("Should not have any error", "", error.ToStringWithNewLineBetweenAppends());

			try
			{
				Factory.Save();
				ExtraAssertion(bizObj);
			}
			catch (ZSaveException ex)
			{
				Fail("The data could not be saved; please ensure that the database has the latest Xsd applied.\r\nIf the Xsd has been changed then it need to be given to Brett to include in the Schema collection.\r\nError: " + ex.Message);
			}

			var newFactory = new BusinessObjectFactory();
			var bizObjInDiffFactory = GetBizObjInDiffFactory(bizObj, newFactory);
			var xmlDataInDiffFactory = bizObjInDiffFactory.Serialize(XsdNamespace);
			AssertMultilineASCIIEquals("XML Data", xmlData, xmlDataInDiffFactory);
		}

		public void TestEmptyBizObjIsSaved()
		{
			var bizObj = GetEmptyBizObj();
			var xsdData = new BusinessObjectXsdGenerator().GenerateXsd(bizObj, XsdVersion, XsdNamespace);
			AssertMultilineASCIIEquals("Xsd Data", GetExpectedXsdData(), xsdData);
			AssertStartsWith("Should contain LazyLoading Schema", @"<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:element name=""placeholder"">
    <xs:complexType>
      <xs:simpleContent>
        <xs:extension base=""xs:string"">
          <xs:attribute name=""LazyLoading"" type=""xs:string"" fixed=""Yes"">
          </xs:attribute>
        </xs:extension>
      </xs:simpleContent>
    </xs:complexType>
  </xs:element>
</xs:schema>", xsdData);
			xsdData = xsdData.Substring(xsdData.IndexOf(@"<xs:schema targetNamespace=""" + XsdNamespace));
			var xmlData = bizObj.Serialize(XsdNamespace);
			AssertMultilineASCIIEquals("XML Data", GetExpectedEmptyXmlData(), xmlData);

			var schemas = new XmlSchemaSet();
			schemas.Add(XsdNamespace, XmlReader.Create(new StringReader(xsdData)));

			var doc = XDocument.Load(new StringReader(xmlData));
			var error = new ZStringBuilder();
			doc.Validate(schemas, (o, e) =>
			{
				error.AppendLine(e.Message);
			});
			AssertMultilineASCIIEquals("Should not have any error", "", error.ToStringWithNewLineBetweenAppends());

			try
			{
				Factory.Save();
			}
			catch (ZSaveException ex)
			{
				Fail("The data could not be saved; please ensure that the database has the latest Xsd applied.\r\nIf the Xsd has been changed then it need to be given to Brett to include in the Schema collection.\r\nError: " + ex.Message);
			}
		}

		protected abstract XmlSerializableNonPersistentBusinessObject GetBizObjInDiffFactory(XmlSerializableNonPersistentBusinessObject bizObj, BusinessObjectFactory newFactory);
		protected abstract XmlSerializableNonPersistentBusinessObject GetEmptyBizObj();
		protected abstract string GetExpectedEmptyXmlData();

		protected virtual void ExtraAssertion(XmlSerializableNonPersistentBusinessObject bizObj)
		{
		}

		protected abstract string GetExpectedXmlData();
		protected abstract string GetExpectedXsdData();
		protected abstract XmlSerializableNonPersistentBusinessObject GetFullyPopulatedBizObj();
		protected abstract string XsdNamespace { get; }
		protected abstract int XsdVersion { get; }
	}
}
