using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Common.Business.Testing
{
	sealed class DefaultSailingManagerQueryProviderTest : TestCaseWithFactory
	{
		#region TestQueryFreshMatchBehaviour

		public void TestQueryFreshMatchBehaviour()
		{
			QueryFreshMatchBehaviourArgs args = new QueryFreshMatchBehaviourArgs()
			{
				DateName = "",
				FoundDate = ZDateTime.Now,
				RequestedDate = ZDateTime.Today
			};

			AssertEquals(SailingManagerUpdateMode.ScheduleUnchanged, Provider.QueryFreshMatchBehaviour(args));
		}

		public void TestQueryFreshMatchBehaviour_NoExceptions()
		{
			var args = new QueryFreshMatchBehaviourArgs
			{
				DateName = "Oops"
			};

			AssertEquals(SailingManagerUpdateMode.ScheduleUnchanged, Provider.QueryFreshMatchBehaviour(args));

			AssertEquals(SailingManagerUpdateMode.ScheduleUnchanged, Provider.QueryFreshMatchBehaviour(null));
		}

		public void TestQueryFreshMatchBehaviour_FlightScheduleThresholdFromRegistry()
		{
			Assert("Pre-condition: should be true by default", SystemDataRegistry.Instance.UpdateSchedulesDuringAutomaticImport.DefaultValue);

			var today = ZDateTime.Today;
			var args = new QueryFreshMatchBehaviourArgs
			{
				IsImportingData = true,
				TransportMode = Constants.TransportModes.Air,
				FoundDate = today,
				RequestedDate = today.AddHours(6)
			};

			AssertEquals("Expected schedule to remain unchanged", SailingManagerUpdateMode.ScheduleUnchanged, Provider.QueryFreshMatchBehaviour(args));

			SystemDataRegistry.Instance.FlightScheduleUpdateThresholdForDataImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);

			AssertEquals("Should return New Schedule as the difference between the times is now less than the registry threshold",
				SailingManagerUpdateMode.NewSchedule, Provider.QueryFreshMatchBehaviour(args));

			args.RequestedDate = today.AddHours(4);

			AssertEquals(SailingManagerUpdateMode.NewSchedule, Provider.QueryFreshMatchBehaviour(args));

			args.RequestedDate = today.AddHours(4).AddSeconds(1);

			AssertEquals(SailingManagerUpdateMode.NewSchedule, Provider.QueryFreshMatchBehaviour(args));

			args.RequestedDate = today.AddHours(-4);

			AssertEquals("Expected to be updated regardless of which date is bigger",
				SailingManagerUpdateMode.NewSchedule, Provider.QueryFreshMatchBehaviour(args));
		}

		public void TestQueryFreshMatchBehaviour_RegistryCanCreateNewAcrossDays()
		{
			SystemDataRegistry.Instance.FlightScheduleUpdateThresholdForDataImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 16);

			var args = new QueryFreshMatchBehaviourArgs
			{
				IsImportingData = true,
				TransportMode = Constants.TransportModes.Air,
				FoundDate = new ZDateTime(2014, 01, 02),
				RequestedDate = new ZDateTime(2014, 01, 01, 07, 59, 59)
			};

			AssertEquals("Difference between dates should trigger new schedule",
				SailingManagerUpdateMode.NewSchedule, Provider.QueryFreshMatchBehaviour(args));

			args.FoundDate = new ZDateTime(2014, 01, 01, 07, 59, 59);
			args.RequestedDate = new ZDateTime(2014, 01, 02);

			AssertEquals("Shouldn't matter which date is first, as long as the difference > 15 hours apart",
				SailingManagerUpdateMode.NewSchedule, Provider.QueryFreshMatchBehaviour(args));

			args.FoundDate = new ZDateTime(2014, 01, 01, 08, 00, 01);

			AssertEquals("The difference between dates > 16 hours apart so should not change",
				SailingManagerUpdateMode.ScheduleUnchanged, Provider.QueryFreshMatchBehaviour(args));
		}

		public void TestQueryFreshMatchBehaviour_RegistryOnlyAffectsImportingFlightSchedules()
		{
			SystemDataRegistry.Instance.FlightScheduleUpdateThresholdForDataImport.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 4);

			var today = ZDateTime.Today;
			var airArgs = new QueryFreshMatchBehaviourArgs
			{
				IsImportingData = true,
				TransportMode = Constants.TransportModes.Air,
				FoundDate = today,
				RequestedDate = today.AddHours(6)
			};

			var nonAirArgs = new QueryFreshMatchBehaviourArgs
			{
				IsImportingData = true,
				TransportMode = Constants.TransportModes.Sea,
				FoundDate = today,
				RequestedDate = today.AddHours(6)
			};

			var nonDataImport = new QueryFreshMatchBehaviourArgs
			{
				IsImportingData = false,
				TransportMode = Constants.TransportModes.Air,
				FoundDate = today,
				RequestedDate = today.AddHours(6)
			};

			AssertEquals(SailingManagerUpdateMode.NewSchedule, Provider.QueryFreshMatchBehaviour(airArgs));
			AssertEquals(SailingManagerUpdateMode.ScheduleUnchanged, Provider.QueryFreshMatchBehaviour(nonAirArgs));
			AssertEquals(SailingManagerUpdateMode.ScheduleUnchanged, Provider.QueryFreshMatchBehaviour(nonDataImport));
		}

		#endregion

		#region Implementation

		ISailingManagerQueryProvider Provider
		{
			get { return new DefaultSailingManagerQueryProvider(); }
		}

		#endregion
	}
}
