using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Registry.Testing
{
	[TestedType(typeof(CargoIMPPhase2RouteMap))]
	public class CargoIMPPhase2RouteMapTest : RegistryBusinessObjectTemplateTestCase<CargoIMPPhase2RouteMap>
	{
		public void TestValidateOrigin()
		{
			CargoIMPPhase2RouteMap map = new CargoIMPPhase2RouteMap();
			map.Origin = "US000";
			Assert(map.HasErrors);
			map.Origin = "UAIEV";
			Assert(!map.HasErrors);
			map.Origin = "UA";
			Assert(!map.HasErrors);
			map.Origin = "";
			Assert(map.HasErrors);
		}

		public void TestValidateDestination()
		{
			CargoIMPPhase2RouteMap map = new CargoIMPPhase2RouteMap();
			map.Destination = "US000";
			Assert(map.HasErrors);
			map.Destination = "UAIEV";
			Assert(!map.HasErrors);
			map.Destination = "UA";
			Assert(!map.HasErrors);
			map.Destination = "";
			Assert(map.HasErrors);
		}

		public void TestValidateAirlineTwoCharacterCode()
		{
			CargoIMPPhase2RouteMap map = new CargoIMPPhase2RouteMap();
			map.AirlineTwoCharacterCode = "~~";
			Assert(map.HasErrors);
			map.AirlineTwoCharacterCode = "FO";
			Assert(!map.HasErrors);
			map.AirlineTwoCharacterCode = "";
			Assert(map.HasErrors);
		}

		public void TestValidateNaturalKey()
		{
			CargoIMPPhase2RouteMapCollection coll = new CargoIMPPhase2RouteMapCollection();
			CargoIMPPhase2RouteMap map1 = coll.AddNew();
			map1.AirlineTwoCharacterCode = "FO";
			map1.Origin = "AUSYD";
			map1.Destination = "NZAKL";
			Assert(!map1.HasErrors);

			CargoIMPPhase2RouteMap map2 = coll.AddNew();
			map2.AirlineTwoCharacterCode = "FO";
			map2.Origin = "AUSYD";
			map2.Destination = "NZLYT";
			Assert(!map2.HasErrors);

			map2.Destination = "NZAKL";
			Assert(map2.HasErrors);

			map2.Origin = "AUMEL";
			Assert(!map2.HasErrors);

			map2.Origin = "AUSYD";
			map2.AirlineTwoCharacterCode = "AA";
			Assert(!map2.HasErrors);
		}

		#region Implementation

		protected override CargoIMPPhase2RouteMap GetBusinessObjectToClone()
		{
			return new CargoIMPPhase2RouteMap();
		}

		protected override CargoIMPPhase2RouteMap GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		#endregion
	}
}
