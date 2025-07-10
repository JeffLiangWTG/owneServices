using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business
{
	[TestedType(typeof(PartialEventsInfo))]
	sealed class PartialEventsInfoTestCase : NonPersistentBusinessObjectTestCase
	{
		#region TestEventsDetailslAndTotal

		public void TestEventsDetailslAndTotal()
		{
			var consol = Factory.New<ForwardingConsol>();

			AddLog(consol, Events.FreightLoadedCode, "AAA", new ZDateTime(2015, 1, 1), "AUSYD", 1, 5);
			AddLog(consol, Events.FreightLoadedCode, "BBB", new ZDateTime(2015, 1, 1), "AUSYD", 3, 5);

			AddLog(consol, Events.FreightLoadedCode, "AAA", new ZDateTime(2015, 1, 1), "AUMEL", 1, 5);
			AddLog(consol, Events.FreightLoadedCode, "BBB", new ZDateTime(2015, 1, 1), "AUMEL", 3, 5);

			AddLog(consol, Events.AuthorisedCode, "CCC", new ZDateTime(2015, 1, 2), "AUSYD", 1, 1);

			AddLog(consol, Events.BookedCode, "DDD", new ZDateTime(2015, 1, 2), "AUSYD", 1, 1);

			var info = new PartialEventsInfo(consol, new[]
			{
				(ZString)Events.FreightLoadedCode,
				(ZString)Events.AuthorisedCode
			}, "AUSYD");

			AssertMultilineASCIIEquals("Details",
@"AUSYD AAA 2015-01-01 1 of 5 pieces
AUSYD BBB 2015-01-01 3 of 5 pieces
AUSYD CCC 2015-01-02 1 of 1 pieces",
			info.Details);

			AssertMultilineASCIIEquals("Total", "Running Total: 5 of 5", info.Total);
			AssertEquals(true, true);
		}

		#endregion

		#region TestMessageErrorWhenInconsistentTotals

		public void TestMessageErrorWhenInconsistentTotals()
		{
			var consol = Factory.New<ForwardingConsol>();

			AddLog(consol, AutoEvents.FreightLoadedCode, "AAA", new ZDateTime(2015, 1, 1), "AUSYD", 1, 2);
			AddLog(consol, AutoEvents.FreightLoadedCode, "BBB", new ZDateTime(2015, 1, 1), "AUSYD", 2, 3);

			AddLog(consol, AutoEvents.FreightLoadedCode, "AAA", new ZDateTime(2015, 1, 1), "AUMEL", 1, 2);
			AddLog(consol, AutoEvents.FreightLoadedCode, "BBB", new ZDateTime(2015, 1, 1), "AUMEL", 2, 3);

			var info = new PartialEventsInfo(consol, new[] { (ZString)Events.FreightLoadedCode }, "AUSYD");

			AssertMultilineASCIIEquals("Details",
@"AUSYD AAA 2015-01-01 1 of 2 pieces
AUSYD BBB 2015-01-01 2 of 3 pieces",
			info.Details);

			AssertMultilineASCIIEquals("Total", "Running Total: 3 of 3", info.Total);

			AssertHasWarning(info.TotalInfo, "Event totals are inconsistent.");
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new PartialEventsInfo(null, null, "");
		}

		void AddLog(IStmALogParent logParent, string eventCode, ZString flightNumber, ZDateTime flightDate, ZString location, int partial, int total)
		{
			logParent.Logs.AddNew(Events.All[eventCode],
				ZString.Empty,
				flightDate.ToOffset(),
				new[]
				{
					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location,
						location),

					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Partial,
						partial.ToString(CultureInfo.InvariantCulture)),

					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Total,
						total.ToString(CultureInfo.InvariantCulture)),

					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.VoyageFlightNumber,
						flightNumber),

					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.FlightDate,
						flightDate.ToISO8601ShortDateString()),

					new KeyValuePair<string, string>(
						CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility,
						"CTO")
				});
		}

		#endregion
	}
}
