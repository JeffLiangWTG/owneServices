using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using CargoWise.RefDbRepo.UniversalXmlParser.Resources;
using NUnit.Framework;
using Unity;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Test.XmlParser
{
	[TestFixture]
	public class UniversalXmlSchemaHandlerFixtures : BaseUnitTestFixture
	{
		#region Member Variables

		const string UniversalReferenceDataXml = @"CargoWise.UniversalXmlParserTests.TestFiles.UniversalReferenceData.xml";

		readonly string[] ExpectedEntityTypes = {
			"RefCusTariff",
			"RefCusApplicability",
			"RefCusCondition",
			"RefCusConditionValue",
			"RefCusExcludedTradeGroup",
			"RefCusRate",
			"RefCusTariffAttribute",
			"RefCusTariffNationalCode",
			"RefCusTariffRelationship",
			"RefCusTariffUOM",
			"RefCusVATApplicability"
		};

		#endregion

		[Test]
		public void TestParseSchemaXmlToEntityTypes()
		{
			var handler = Container.Resolve<IUniversalXmlSchemaHandler>();
			Assert.IsNotNull(handler);

			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(UniversalReferenceDataXml);
			if (stream == null)
			{
				return;
			}

			using (var xmlReader = XmlReader.Create(new StreamReader(stream, Encoding.UTF8)))
			{
				xmlReader.MoveToContent();
				while (xmlReader.Read())
				{
					if ((xmlReader.NodeType != XmlNodeType.Element) || (xmlReader.Name != Constants.UniversalXml.SchemaElement))
					{
						continue;
					}

					XDocument document;
					var entityTypes = handler.ParseSchemaXmlToEntityTypes(xmlReader.ReadOuterXml(), out document);
					Assert.IsNotNull(document);
					Assert.IsNotNull(entityTypes);
					Assert.IsTrue(entityTypes.Any());

					foreach (var entityType in entityTypes)
					{
						Assert.IsTrue(ExpectedEntityTypes.Contains(entityType.Name));
					}

					break;
				}
			}
		}
	}
}
