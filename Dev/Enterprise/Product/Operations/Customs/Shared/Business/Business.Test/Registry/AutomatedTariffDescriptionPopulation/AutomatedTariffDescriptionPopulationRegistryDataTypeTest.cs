using System.IO;
using System.Text;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutomatedTariffDescriptionPopulationRegistryDataType))]
	sealed class AutomatedTariffDescriptionPopulationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<AutomatedTariffDescriptionPopulationRegistryDataType>
	{
		protected override string ExpectedEditorName => "AutomatedTariffDescriptionPopulationRegistryItemEditor";

		protected override AutomatedTariffDescriptionPopulationRegistryDataType GetNewDataType()
		{
			return new AutomatedTariffDescriptionPopulationRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var automatedPopulation1 = new AutomatedTariffDescriptionPopulation();
			automatedPopulation1.EnableCommercialInvoice = true;
			automatedPopulation1.EnableCustomsDeclaration = true;

			var automatedPopulation2 = new AutomatedTariffDescriptionPopulation();
			automatedPopulation2.EnableCommercialInvoice = false;
			automatedPopulation2.EnableCustomsDeclaration = false;

			var serializer = ZXmlSerializer.New(automatedPopulation1.GetType());
			var xml1 = new StringBuilder();
			using (var stream = new StringWriter(xml1))
			{
				serializer.Serialize(stream, automatedPopulation1);
			}
			var xml2 = new StringBuilder();
			using (var stream = new StringWriter(xml2))
			{
				serializer.Serialize(stream, automatedPopulation2);
			}

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(automatedPopulation1, Encoding.Unicode.GetBytes(xml1.ToString())),
				new ValidSampleAndBinaryValueInDB(automatedPopulation2, Encoding.Unicode.GetBytes(xml2.ToString()))
			};
		}
	}
}
