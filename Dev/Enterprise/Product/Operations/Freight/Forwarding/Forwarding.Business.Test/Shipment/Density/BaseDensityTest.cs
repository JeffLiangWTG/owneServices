using System;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using DensityValues = Enterprise.Freight.Forwarding.Business.Density.DensityValues;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	public abstract class BaseDensityTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDensity_Lookup()
		{
			var input_outputTestPairs = new[]
			{
				new Tuple<ZDecimal, DensityValues>(0.00m, new DensityValues(0.0000m, "1:1", "Dense ++++")),
				new Tuple<ZDecimal, DensityValues>(0.12m, new DensityValues(0.0000m, "1:1", "Dense ++++")),
				new Tuple<ZDecimal, DensityValues>(0.26m, new DensityValues(0.2505m, "1:2", "Dense +++")),
				new Tuple<ZDecimal, DensityValues>(0.45m, new DensityValues(0.4200m, "1:3", "Dense ++")),
				new Tuple<ZDecimal, DensityValues>(0.58m, new DensityValues(0.5800m, "1:4", "Dense +")),
				new Tuple<ZDecimal, DensityValues>(0.73m, new DensityValues(0.5800m, "1:4", "Dense +")),
				new Tuple<ZDecimal, DensityValues>(0.84m, new DensityValues(0.7500m, "1:5", "Dense")),
				new Tuple<ZDecimal, DensityValues>(1.04m, new DensityValues(0.9200m, "1:6", "1 to 1 cargo")),
				new Tuple<ZDecimal, DensityValues>(1.24m, new DensityValues(1.0855m, "1:7", "Volume")),
				new Tuple<ZDecimal, DensityValues>(1.37m, new DensityValues(1.2525m, "1:8", "Volume +")),
				new Tuple<ZDecimal, DensityValues>(1.50m, new DensityValues(1.4195m, "1:9", "Volume ++")),
				new Tuple<ZDecimal, DensityValues>(1.68m, new DensityValues(1.5865m, "1:10", "Volume +++")),
				new Tuple<ZDecimal, DensityValues>(1.76m, new DensityValues(1.7535m, "1:11", "Volume ++++")),
				new Tuple<ZDecimal, DensityValues>(1.92m, new DensityValues(1.9190m, "1:12", "Volume +++++")),
				new Tuple<ZDecimal, DensityValues>(2.15m, new DensityValues(1.9190m, "1:12", "Volume +++++")),
			};

			var density = new ShipmentDensity(Factory.New<ForwardingShipment>());
			var sortedDensityValuesLookup = density.DensityValuesLookup.OrderBy(x => -x.DensityFactor);
			Array.ForEach(input_outputTestPairs, x =>
			{
				AssertDensities("Should get a matched set of density values", x.Item2, sortedDensityValuesLookup.FirstOrDefault(d => x.Item1 >= d.DensityFactor));
			});
		}

		void AssertDensities(string message, DensityValues expected, DensityValues result)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Density Factor",
					expected.DensityFactor.ToString("F4", CultureInfo.CurrentCulture),
					result.DensityFactor.ToString("F4", CultureInfo.CurrentCulture)
				);
				AssertEquals("Density Remark", expected.DensityRemark, result.DensityRemark);
				AssertEquals("Volume Ratio", expected.VolumeRatio, result.VolumeRatio);
			});
		}
	}
}
