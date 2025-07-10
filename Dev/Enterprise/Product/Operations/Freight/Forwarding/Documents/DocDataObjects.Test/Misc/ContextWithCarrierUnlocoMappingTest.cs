using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class ContextWithCarrierUnlocoMappingTest : TestCaseWithFactory
	{
		public void TestLookups()
		{
			var context = new ContextWithCarrierUnlocoMapping(Factory, ZGuid.Empty);

			AssertNotNull(nameof(context.Unlocos), context.Unlocos);
			Assert(nameof(context.Unlocos), context.Unlocos is ICodeMapper);
			Assert(((ICodeMapper)context.Unlocos).ShowForeignCode);

			AssertNotNull(nameof(context.TransportModes), context.TransportModes);
			AssertNotNull(nameof(context.TransportTypes), context.TransportTypes);
			AssertNotNull(nameof(context.ContainerTypes), context.ContainerTypes);
			AssertNotNull(nameof(context.Countries), context.Countries);
			AssertNotNull(nameof(context.AirVentFlow), context.AirVentFlow);
			AssertNotNull(nameof(context.TemperatureUnits), context.TemperatureUnits);
			AssertNotNull(nameof(context.Humidity), context.Humidity);
			AssertNotNull(nameof(context.WeightUnits), context.WeightUnits);
			AssertNotNull(nameof(context.VolumeUnits), context.VolumeUnits);
			AssertNotNull(nameof(context.DimensionUnits), context.DimensionUnits);
			AssertNotNull(nameof(context.RadioactiveUnits), context.RadioactiveUnits);

			context = new ContextWithCarrierUnlocoMapping(Factory, ZGuid.Empty, true);
			Assert(((ICodeMapper)context.Unlocos).ShowForeignCode);
		}
	}
}
