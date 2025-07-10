using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerTranshipmentIndicator))]
	internal class ContainerTranshipmentIndicatorTest : RegistryBusinessObjectTemplateTestCase<ContainerTranshipmentIndicator>
	{
		public void TestSerialisation()
		{
			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(ContainerTranshipmentIndicator));
			ContainerTranshipmentIndicator indicator1 = ContainerTranshipmentIndicatorCollection.NewAndPopulate()[0];
			string xml;
			using (StringWriter stream = new StringWriter())
			{
				serialiser.Serialize(stream, indicator1);
				xml = stream.ToString();
			}

			const string expectedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n" + "<ContainerTranshipmentIndicator>\n" + "  <Key>Empty</Key>\n" + "  <Direct>E</Direct>\n" + "  <Tranship>M</Tranship>\n" + "  <Domestic>P</Domestic>\n" + "</ContainerTranshipmentIndicator>\n" + "";
			AssertMultilineASCIIEquals("", expectedXml, xml);
			ContainerTranshipmentIndicator indicator2;
			using (StringReader stream = new StringReader(xml))
			{
				indicator2 = (ContainerTranshipmentIndicator)serialiser.Deserialize(stream);
			}

			AssertEquals("Direct", indicator1.Direct, indicator2.Direct);
			AssertEquals("Tranship", indicator1.Tranship, indicator2.Tranship);
			AssertEquals("Domestic", indicator1.Domestic, indicator2.Domestic);
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ContainerTranshipmentIndicator();
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected override ContainerTranshipmentIndicator GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override ContainerTranshipmentIndicator GetBusinessObjectToSerialise()
		{
			return ContainerTranshipmentIndicatorCollection.NewAndPopulate().Empty;
		}
		#endregion
	}
}
