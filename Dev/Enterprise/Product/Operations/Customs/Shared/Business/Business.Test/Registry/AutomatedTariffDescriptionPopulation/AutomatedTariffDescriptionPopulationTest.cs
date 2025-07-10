using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DataRegistry.Business.Testing
{
	[TestedType(typeof(AutomatedTariffDescriptionPopulation))]
	sealed class AutomatedTariffDescriptionPopulationTest : RegistryBusinessObjectTemplateTestCase<AutomatedTariffDescriptionPopulation>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override AutomatedTariffDescriptionPopulation GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override AutomatedTariffDescriptionPopulation GetBusinessObjectToSerialise()
		{
			automatedTariffDescriptionPopulation = new AutomatedTariffDescriptionPopulation();

			return automatedTariffDescriptionPopulation;
		}

		AutomatedTariffDescriptionPopulation automatedTariffDescriptionPopulation;
	}
}
