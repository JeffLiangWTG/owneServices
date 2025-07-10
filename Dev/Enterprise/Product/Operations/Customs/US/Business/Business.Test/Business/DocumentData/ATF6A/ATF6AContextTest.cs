using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.US.Business.Testing
{
	class ATF6AContextTest : TestCaseWithFactory
	{
		public void TestContainerTypes()
		{
			AssertType<RefContainerCollection>(context.ContainerTypes);
		}

		public void TestCountries()
		{
			AssertType<RefCountryCollection>(context.Countries);
		}

		public void TestCurrencies()
		{
			AssertType<RefCurrencyCollection>(context.Currencies);
		}

		public void TestUnlocos()
		{
			AssertType<RefUNLOCOCollection>(context.Unlocos);
		}

		public void TestOthers()
		{
			AssertType<CodeDescriptionPairList>(context.TransportModes);
			AssertEquals(0, context.TransportModes.Count);
			AssertType<CodeDescriptionPairList>(context.TransportTypes);
			AssertEquals(0, context.TransportTypes.Count);
			AssertType<CodeDescriptionPairList>(context.AirVentFlow);
			AssertEquals(0, context.AirVentFlow.Count);
			AssertType<CodeDescriptionPairList>(context.TemperatureUnits);
			AssertEquals(0, context.TemperatureUnits.Count);
			AssertType<CodeDescriptionPairList>(context.Humidity);
			AssertEquals(0, context.Humidity.Count);
			AssertType<CodeDescriptionPairList>(context.WeightUnits);
			AssertEquals(0, context.WeightUnits.Count);
			AssertType<CodeDescriptionPairList>(context.VolumeUnits);
			AssertEquals(0, context.VolumeUnits.Count);
			AssertType<CodeDescriptionPairList>(context.DimensionUnits);
			AssertEquals(0, context.DimensionUnits.Count);
			AssertType<CodeDescriptionPairList>(context.RadioactiveUnits);
			AssertEquals(0, context.RadioactiveUnits.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			context = new ATF6AContext(Factory);
		}
		ATF6AContext context;
	}
}
