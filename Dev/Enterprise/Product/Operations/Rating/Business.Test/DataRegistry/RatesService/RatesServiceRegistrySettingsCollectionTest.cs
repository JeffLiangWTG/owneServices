using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(RatesServiceRegistrySettingsCollection))]
	public class RatesServiceRegistrySettingsCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<RatesServiceRegistrySettingsCollection>
	{
		public void TestUniqueConfigurations()
		{
			var ratesServiceRateSelectorCollection = new RatesServiceRegistrySettingsCollection();

			var newRatesServiceRateSelectorRow1 = new RatesServiceRegistrySettings();
			ratesServiceRateSelectorCollection.Add(newRatesServiceRateSelectorRow1);
			newRatesServiceRateSelectorRow1.TransportMode = Core.Constants.TransportModes.Sea;
			newRatesServiceRateSelectorRow1.ContainerMode = Core.Constants.ContainerModes.FCL;

			var newRatesServiceRateSelectorRow2 = new RatesServiceRegistrySettings();
			ratesServiceRateSelectorCollection.Add(newRatesServiceRateSelectorRow2);
			newRatesServiceRateSelectorRow2.TransportMode = Core.Constants.TransportModes.Sea;
			newRatesServiceRateSelectorRow2.ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertHasError(newRatesServiceRateSelectorRow1.TransportModeInfo, "Configuration with identical criteria already exists.");
			AssertHasError(newRatesServiceRateSelectorRow2.TransportModeInfo, "Configuration with identical criteria already exists.");

			newRatesServiceRateSelectorRow2.IsSubscriptionEnabled = true;
			AssertHasError(newRatesServiceRateSelectorRow1.TransportModeInfo, "Configuration with identical criteria already exists.");
			AssertHasError(newRatesServiceRateSelectorRow2.TransportModeInfo, "Configuration with identical criteria already exists.");

			newRatesServiceRateSelectorRow2.ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertNoErrors(newRatesServiceRateSelectorRow2.TransportModeInfo);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override RatesServiceRegistrySettingsCollection GetCollectionToTest()
		{
			return new RatesServiceRegistrySettingsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new RatesServiceRegistrySettings();
		}

		#endregion
	}
}
