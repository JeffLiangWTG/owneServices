using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Test
{
	public class RateOneOffCarrierLookupsTest : BusinessObjectLookupsTestCase
	{
		#region Transit Time List

		#region Transit Time List Air

		public void TestTransitTimeList_Air_LSE() => TestTransitTimeList(TransportModes.Air, ContainerModes.Loose, expectedTransitTimesList: new[] { "SMD-Same Day", "OVN-Overnight", "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		public void TestTransitTimeList_Air_ULD() => TestTransitTimeList(TransportModes.Air, ContainerModes.ULD, expectedTransitTimesList: new[] { "SMD-Same Day", "OVN-Overnight", "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		#endregion

		#region Transit Time List Sea

		public void TestTransitTimeList_Sea_SEA() => TestTransitTimeList(TransportModes.Sea, RateMode.SEA, expectedTransitTimesList: new[] { "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		public void TestTransitTimeList_Sea_LCL() => TestTransitTimeList(TransportModes.Sea, ContainerModes.LCL, expectedTransitTimesList: new[] { "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		public void TestTransitTimeList_Sea_FCL() => TestTransitTimeList(TransportModes.Sea, ContainerModes.FCL, expectedTransitTimesList: new[] { "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		#endregion

		#region Transit Time List Rail

		public void TestTransitTimeList_Rail_RAI() => TestTransitTimeList(TransportModes.Rail, RateMode.RAI, expectedTransitTimesList: new[] { "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		public void TestTransitTimeList_Rail_LRA() => TestTransitTimeList(TransportModes.Rail, ContainerModes.LCL, expectedTransitTimesList: new[] { "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		public void TestTransitTimeList_Rail_FRA() => TestTransitTimeList(TransportModes.Rail, ContainerModes.FCL, expectedTransitTimesList: new[] { "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		public void TestTransitTimeList_Rail_FWL() => TestTransitTimeList(TransportModes.Rail, RateMode.FWL, expectedTransitTimesList: new[] { "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		#endregion

		#region Transit Time List Road

		public void TestTransitTimeList_Road_ROA() => TestTransitTimeList(TransportModes.Road, RateMode.ROA, expectedTransitTimesList: new[] { "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		public void TestTransitTimeList_Road_LRO() => TestTransitTimeList(TransportModes.Road, ContainerModes.LCL, expectedTransitTimesList: new[] { "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		public void TestTransitTimeList_Road_FRO() => TestTransitTimeList(TransportModes.Road, ContainerModes.FCL, expectedTransitTimesList: new[] { "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		public void TestTransitTimeList_Road_FTL() => TestTransitTimeList(TransportModes.Road, ContainerModes.FTL, expectedTransitTimesList: new[] { "1-1 day" }.Concat(GetExpectedValues(2, 120, "days")));

		#endregion

		void TestTransitTimeList(string transportMode, string containerMode, IEnumerable<string> expectedTransitTimesList)
		{
			var quote = Factory.New<Quote>();
			var oneOffShipment = quote.OneOffQuote.AddNew();
			oneOffShipment.TT_TransportMode = transportMode;
			oneOffShipment.TT_ContainerMode = containerMode;
			var possibleCarrier = oneOffShipment.PossibleCarriers.AddNew();

			AssertContainsExactElementsInAnyOrder(
				expectedTransitTimesList,
				possibleCarrier.Lookups.TransitTimesList.Cast<CodeDescriptionPair>().Select(x => $"{x.Code}-{x.Description}")
			);
		}

		IEnumerable<string> GetExpectedValues(int fromIndex, int toIndex, string text)
		{
			for (var i = fromIndex; i <= toIndex; i++)
			{
				yield return $"{i}-{i} {text}";
			}
		}

		#endregion

		public void TestFrequencyUnits()
		{
			var quote = Factory.New<Quote>();
			var oneOffShipment = quote.OneOffQuote.AddNew();
			oneOffShipment.TT_TransportMode = TransportModes.Air;
			oneOffShipment.TT_ContainerMode = ContainerModes.Loose;
			var possibleCarrier = oneOffShipment.PossibleCarriers.AddNew();

			AssertContainsExactElementsInAnyOrder(
				new[] { "Daily-X per day", "Days-Every X days", "-", "Fortnight-X per fortnight", "Monthly-X per month", "Week-X per week" },
				possibleCarrier.Lookups.FrequencyUnits.Cast<CodeDescriptionPair>().Select(x => $"{x.Code}-{x.Description}")
			);
		}
	}
}
