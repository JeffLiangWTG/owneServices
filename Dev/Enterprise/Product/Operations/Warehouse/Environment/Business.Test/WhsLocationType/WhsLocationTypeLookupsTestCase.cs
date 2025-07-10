using System.Linq;
using Enterprise.Warehouse.Environment.CodeLists;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	class WhsLocationTypeLookupsTestCase : WhsBusinessObjectLookupsTestCase
	{
		#region TestLocationClasses

		public void TestLocationClasses()
		{
			var locationTypeLookups = Factory.New<WhsLocationType>().Lookups;
			var expectedLocationClasses = new LocationClasses();
			AssertContainsExactElementsInAnyOrder(expectedLocationClasses, locationTypeLookups.LocationClasses);
		}

		#endregion

		#region TestTemperatureUnits

		public void TestTemperatureUnits()
		{
			var locationTypeLookups = Factory.New<WhsLocationType>().Lookups;
			AssertContainsExactElementsInAnyOrder(new[] { "C", "F" }, locationTypeLookups.TemperatureUnits.GetAllCodes());
			AssertContainsExactElementsInAnyOrder(new[] { "Centigrade", "Fahrenheit" }, locationTypeLookups.TemperatureUnits.ToArray().Select(t => t.Description));
		}

		#endregion

		#region DefaultCycleCountGranularities

		public void TestCycleCountGranularities()
		{
			var locationTypeLookups = Factory.New<WhsLocationType>().Lookups;
			AssertContainsExactElementsInAnyOrder(new CycleCountGranularities(), locationTypeLookups.CycleCountGranularities);
		}

		#endregion
	}
}
