using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using WiseRates.Constants;

namespace Enterprise.Rating.Business.Testing
{
	[TestedType(typeof(FallbackSubjectToChargesCollection))]
	public class FallbackSubjectToChargesCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<FallbackSubjectToChargesCollection>
	{
		public void TestUniqueConfigurations()
		{
			var fallbackCollection = new FallbackSubjectToChargesCollection();

			var newFallbackChargesRow1 = new FallbackSubjectToCharges();
			fallbackCollection.Add(newFallbackChargesRow1);
			newFallbackChargesRow1.RatesProviderCode = WRConstants.RateProviders.CargoSphere;
			newFallbackChargesRow1.TransportMode = Core.Constants.TransportModes.Sea;
			newFallbackChargesRow1.ContainerMode = Core.Constants.ContainerModes.FCL;

			var newFallbackChargesRow2 = new FallbackSubjectToCharges();
			fallbackCollection.Add(newFallbackChargesRow2);
			newFallbackChargesRow2.RatesProviderCode = WRConstants.RateProviders.CargoSphere;
			newFallbackChargesRow2.TransportMode = Core.Constants.TransportModes.Sea;
			newFallbackChargesRow2.ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertHasError(newFallbackChargesRow1.RatesProviderCodeInfo, "Fallback for Subject To charges with identical values already exists");
			AssertHasError(newFallbackChargesRow2.RatesProviderCodeInfo, "Fallback for Subject To charges with identical values already exists");

			newFallbackChargesRow2.IsFallbackEnabled = true;
			AssertHasError(newFallbackChargesRow1.RatesProviderCodeInfo, "Fallback for Subject To charges with identical values already exists");
			AssertHasError(newFallbackChargesRow2.RatesProviderCodeInfo, "Fallback for Subject To charges with identical values already exists");

			newFallbackChargesRow2.ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertNoErrors(newFallbackChargesRow2.RatesProviderCodeInfo);
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

		protected override FallbackSubjectToChargesCollection GetCollectionToTest()
		{
			return new FallbackSubjectToChargesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FallbackSubjectToCharges();
		}

		#endregion
	}
}
