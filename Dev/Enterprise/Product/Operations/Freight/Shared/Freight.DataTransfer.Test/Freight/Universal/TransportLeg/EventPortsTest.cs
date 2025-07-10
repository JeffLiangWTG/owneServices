using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Event = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.DataTransfer.Universal
{
	sealed class EventPortsTest : TestCaseWithFactory
	{
		public void TestLegOriginsFallBackToLocationEventParameter()
		{
			var uxmlEvent = new Event();
			uxmlEvent.EventType = Events.ArrivalCode;
			uxmlEvent.EventParameters = new EventParameters { Location = "NLRTM" };

			var eventPorts = new EventLegPorts(uxmlEvent, new BusinessObjectFactory());
			AssertEquals("No match as not a Departure event", 0, eventPorts.LegOrigins.Count());

			uxmlEvent.EventType = Events.DepartureCode;

			eventPorts = new EventLegPorts(uxmlEvent, new BusinessObjectFactory());
			AssertContainsExactElementsInAnyOrder("Matches on Departure Event Location parameter", new[] { "NLRTM" }, eventPorts.LegOrigins.Select(x => x.RL_Code));
		}

		public void TestLegOriginMatchOnLegOriginUNLOCO()
		{
			var uxmlEvent = new Event();
			uxmlEvent.EventType = Events.DepartureCode;
			uxmlEvent.EventParameters = new EventParameters { Location = "NLRTM" };

			uxmlEvent.ContextCollection = new List<Context>();
			uxmlEvent.ContextCollection.Add(new Context() { Type = nameof(Event.ContextTypes.LegOriginUNLOCO), Value = "USJFK" });

			var eventPorts = new EventLegPorts(uxmlEvent, new BusinessObjectFactory());
			AssertContainsExactElementsInAnyOrder("Matches on LegOriginUNLOCO", new[] { "USJFK" }, eventPorts.LegOrigins.Select(x => x.RL_Code));
		}

		public void TestLegDestinationsFallBackToLocationEventParameter()
		{
			var uxmlEvent = new Event();
			uxmlEvent.EventType = Events.DepartureCode;
			uxmlEvent.EventParameters = new EventParameters { Location = "NLRTM" };

			var eventPorts = new EventLegPorts(uxmlEvent, new BusinessObjectFactory());
			AssertEquals("No match as not a Arrival event", 0, eventPorts.LegDestinations.Count());

			uxmlEvent.EventType = Events.ArrivalCode;

			eventPorts = new EventLegPorts(uxmlEvent, new BusinessObjectFactory());
			AssertContainsExactElementsInAnyOrder("Matches on Arrival Event Location parameter", new[] { "NLRTM" }, eventPorts.LegDestinations.Select(x => x.RL_Code));
		}

		public void TestLegDestinationMatchOnLegDestinationUNLOCO()
		{
			var uxmlEvent = new Event();
			uxmlEvent.EventType = Events.ArrivalCode;
			uxmlEvent.EventParameters = new EventParameters { Location = "NLRTM" };

			uxmlEvent.ContextCollection = new List<Context>();
			uxmlEvent.ContextCollection.Add(new Context() { Type = nameof(Event.ContextTypes.LegDestinationUNLOCO), Value = "USJFK" });

			var eventPorts = new EventLegPorts(uxmlEvent, new BusinessObjectFactory());
			AssertContainsExactElementsInAnyOrder("Matches on LegDestinationUNLOCO", new[] { "USJFK" }, eventPorts.LegDestinations.Select(x => x.RL_Code));
		}
	}
}
