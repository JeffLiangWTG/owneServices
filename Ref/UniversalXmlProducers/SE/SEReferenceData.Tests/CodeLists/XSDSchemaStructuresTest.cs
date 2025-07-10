using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.RefDbRepo.SEReferenceData.Tests;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.SEReferenceData.XSDSchema.Tests
{
	[TestFixture]
	class XSDSchemaStructuresTest
	{
		[Test]
		public void NoItemsComplexTypeInBaseDataExport()
		{
			var schema = XDocument.Load(Path.Combine(TestHelper.BaseSourcePath, ServicesProjectPath, "BaseDataExport.xsd"));
			Assert.That(!ContainsComplexTypeItemsInDefintion(schema),
				@"BaseDataExport.xsd contains invalid <xsd:complexType name=""items"">. Delete the ComplexType items from BaseDataExport and add to the CertificateData.xsd with elements of the certificate type. ");
		}

		[Test]
		public void RedefineIncludeInIncrementalObjectTraderExport()
		{
			var schema = XDocument.Load(Path.Combine(TestHelper.BaseSourcePath, ServicesProjectPath, @"ExportSchema\ObjectTraderExport.xsd"));
			Assert.That(!schema.Descendants().Any(x => x.Name.LocalName.Equals("redefine", StringComparison.OrdinalIgnoreCase)),
				@"IncrementalObjectTraderExport.xsd contains invalid xsd:redefine element. Add the ComplexType items to IncrementalObjectTraderExport and delete from BaseDataExport.xsd for IncrementalTraderExportSchema. XSD.exe does not support xsd:redefine.");
		}

		[Test]
		public void CertificateDataRequiresItemsDefinition()
		{
			var schema = XDocument.Load(Path.Combine(TestHelper.BaseSourcePath, ServicesProjectPath, @"CodeLists\Certificates\Model\XSDs\CertificateData.xsd"));
			Assert.That(ContainsComplexTypeItemsInDefintion(schema),
				@"CertificateData.xsd must contain a defintion of complexType items. Add the ComplexType items as the one from SE customs does not define each object type correctly for the Total files");
		}

		bool ContainsComplexTypeItemsInDefintion(XDocument schema)
			=> schema.Descendants().Where(x => x.Name.LocalName.Equals("complexType", StringComparison.OrdinalIgnoreCase)).Attributes("name").Any(x => x.Value.Equals("items", StringComparison.OrdinalIgnoreCase));

		const string ServicesProjectPath = @"UniversalXmlProducers\SE\SEReferenceData.Services\";
	}
}
