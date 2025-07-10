using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class CommonContextTest : TestCaseWithFactory
	{
		public void TestTransportModes()
		{
			var context = new CommonContext(Factory);

			AssertContainsExactElementsInAnyOrder("TransportModes",
				new[]
				{
					"AIR|Air",
					"SEA|Sea",
					"ROA|Road",
					"RAI|Rail",
					"IWT|Inland Waterways"
				},
				context.TransportModes.OfType<ICodeDescription>().Select(cd => $"{cd.Code}|{cd.Description}"));
		}

		public void TestTransportTypes()
		{
			var context = new CommonContext(Factory);

			AssertContainsExactElementsInAnyOrder("TransportTypes",
				new[]
				{
					"FL1|Flight 1",
					"FL2|Flight 2",
					"FL3|Flight 3",
					"OTH|Other",
					"PRE|Pre-Carriage",
					"MAI|Main",
					"ONF|On-Forwarding",
					"LTL|Local Transport"
				},
				context.TransportTypes.OfType<ICodeDescription>().Select(cd => $"{cd.Code}|{cd.Description}"));
		}

		public void TestUnlocos()
		{
			var context = new CommonContext(Factory);
			AssertNotNull(context.Unlocos);
		}

		public void TestCountries()
		{
			var context = new CommonContext(Factory);
			AssertNotNull(context.Countries);
		}

		public void TestContainerTypes()
		{
			var context = new CommonContext(Factory);

			var containerTypes = context.ContainerTypes;

			AssertNotNull("lookup is not null", containerTypes);
			AssertEquals("lookup is cached", containerTypes, context.ContainerTypes);
		}

		public void TestTemperatureUnits()
		{
			var context = new CommonContext(Factory);

			var temperatureUnits = context.TemperatureUnits;

			AssertNotNull("lookup is not null", temperatureUnits);
			AssertEquals("lookup is cached", temperatureUnits, context.TemperatureUnits);
		}

		public void TestHumidity()
		{
			var context = new CommonContext(Factory);

			var humidity = context.Humidity;

			AssertNotNull("lookup is not null", humidity);
			AssertEquals("lookup is cached", humidity, context.Humidity);
		}

		public void TestAirVentFlow()
		{
			var context = new CommonContext(Factory);

			var airVentFlow = context.AirVentFlow;

			AssertNotNull("lookup is not null", airVentFlow);
			AssertEquals("lookup is cached", airVentFlow, context.AirVentFlow);
		}

		public void TestWeightUnits()
		{
			var context = new CommonContext(Factory);

			var weightUnits = context.WeightUnits;

			AssertNotNull("lookup is not null", weightUnits);
			AssertEquals("lookup is cached", weightUnits, context.WeightUnits);
		}

		public void TestVolumeUnits()
		{
			var context = new CommonContext(Factory);

			var volumeUnits = context.VolumeUnits;

			AssertNotNull("lookup is not null", volumeUnits);
			AssertEquals("lookup is cached", volumeUnits, context.VolumeUnits);
		}
	}
}
