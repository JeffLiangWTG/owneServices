using System.IO;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Business.Testing
{
	[TestedType(typeof(ContainerTranshipmentIndicatorCollection))]
	internal class ContainerTranshipmentIndicatorCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ContainerTranshipmentIndicatorCollection>
	{
		public void TestSerialisation()
		{
			ZXmlSerializer serialiser = ZXmlSerializer.New(typeof(ContainerTranshipmentIndicatorCollection));
			ContainerTranshipmentIndicatorCollection collection1 = ContainerTranshipmentIndicatorCollection.NewAndPopulate();
			string xml;
			using (StringWriter stream = new StringWriter())
			{
				serialiser.Serialize(stream, collection1);
				xml = stream.ToString();
			}

			const string expectedXml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>\n" + "<ContainerTranshipmentIndicatorCollection>\n" + "  <ContainerTranshipmentIndicator>\n" + "    <Key>Empty</Key>\n" + "    <Direct>E</Direct>\n" + "    <Tranship>M</Tranship>\n" + "    <Domestic>P</Domestic>\n" + "  </ContainerTranshipmentIndicator>\n" + "  <ContainerTranshipmentIndicator>\n" + "    <Key>BreakBulk</Key>\n" + "    <Direct>B</Direct>\n" + "    <Tranship>U</Tranship>\n" + "    <Domestic>A</Domestic>\n" + "  </ContainerTranshipmentIndicator>\n" + "  <ContainerTranshipmentIndicator>\n" + "    <Key>Laden</Key>\n" + "    <Direct>0</Direct>\n" + "    <Tranship>T</Tranship>\n" + "    <Domestic>0</Domestic>\n" + "  </ContainerTranshipmentIndicator>\n" + "</ContainerTranshipmentIndicatorCollection>\n" + "";
			AssertMultilineASCIIEquals("", expectedXml, xml);
			ContainerTranshipmentIndicatorCollection collection2;
			using (StringReader stream = new StringReader(xml))
			{
				collection2 = (ContainerTranshipmentIndicatorCollection)serialiser.Deserialize(stream);
			}

			for (int i = 0; i < collection1.Count; i++)
			{
				AssertEquals("Direct", collection1[i].Direct, collection2[i].Direct);
				AssertEquals("Tranship", collection1[i].Tranship, collection2[i].Tranship);
				AssertEquals("Domestic", collection1[i].Domestic, collection2[i].Domestic);
			}
		}

		public void TestDefaultProperties()
		{
			ContainerTranshipmentIndicatorCollection collection = ContainerTranshipmentIndicatorCollection.NewAndPopulate();
			AssertNoNotifications("precondition:", collection.Empty);
			AssertEquals("Empty Direct Default Value", "E", collection.Empty.Direct);
			AssertEquals("Empty Tranship Default Value", "M", collection.Empty.Tranship);
			AssertEquals("Empty Domestic Default Value", "P", collection.Empty.Domestic);
			AssertNoNotifications("precondition:", collection.BreakBulk);
			AssertEquals("BreakBulk Direct Default Value", "B", collection.BreakBulk.Direct);
			AssertEquals("BreakBulk Tranship Default Value", "U", collection.BreakBulk.Tranship);
			AssertEquals("BreakBulk Domestic Default Value", "A", collection.BreakBulk.Domestic);
			AssertNoNotifications("precondition:", collection.Laden);
			AssertEquals("Laden Direct Default Value", "0", collection.Laden.Direct);
			AssertEquals("Laden Tranship Default Value", "T", collection.Laden.Tranship);
			AssertEquals("Laden Domestic Default Value", "0", collection.Laden.Domestic);
		}

		#region Implementation
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

		protected override ContainerTranshipmentIndicatorCollection GetCollectionToTest()
		{
			return new ContainerTranshipmentIndicatorCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ContainerTranshipmentIndicator();
		}
		#endregion
	}
}
