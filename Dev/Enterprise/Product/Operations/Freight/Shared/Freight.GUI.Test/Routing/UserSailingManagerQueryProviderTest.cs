using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class UserSailingManagerQueryProviderTest : BaseFreightTest
	{
		#region TestExactMatch

		public void TestExactMatch()
		{
			QueryFreshMatchBehaviourArgs queryArgs = new QueryFreshMatchBehaviourArgs()
			{
				DateName = "ETD",
				FoundDate = ZDateTime.Now,
				RequestedDate = ZDateTime.Now
			};

			AssertEquals(SailingManagerUpdateMode.ScheduleUnchanged, Provider.QueryFreshMatchBehaviour(queryArgs));
			AssertNull("should not have shown a form", ZFormModaliser.LastFormShownDialogForTest);
		}

		#endregion

		#region TestInExactMatch

		public void TestInExactMatch()
		{
			ScheduleUpdateDialog.ResultForTesting.Value = SailingManagerUpdateMode.NewSchedule;

			QueryFreshMatchBehaviourArgs queryArgs = new QueryFreshMatchBehaviourArgs()
			{
				AddCheckpoint = Env.Security.FlightScheduleCreateFromJob,
				EditCheckpoint = Env.Security.FlightScheduleEdit,
				DateName = "ETD",
				FoundDate = ZDateTime.Now,
				RequestedDate = ZDateTime.Now.AddHours(1)
			};

			AssertEquals(SailingManagerUpdateMode.NewSchedule, Provider.QueryFreshMatchBehaviour(queryArgs));
			AssertNotNull("Should have shown a dialog", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should have shown the correct dialog", typeof(ScheduleUpdateDialog), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		#endregion

		#region ScheduleDateUpdates_UserSelectsToKeepScheduleDate

		public void TestJW_ETDShouldKeepETDWhenEnteringETDFirst()
		{
			var initialFlightTime = ZDateTime.Today;
			var voyage = CreateVoyage("CX100", HomePort, initialFlightTime, OverseasPort, initialFlightTime.AddDays(1));
			voyage.GenerateSailings();
			Factory.Save();

			var transport = CreateTransport(ZString.Empty, HomePort, OverseasPort);
			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);
			UserSailingManagerQueryProvider.Register(Factory);
			ScheduleUpdateDialog.ResultForTesting.Value = SailingManagerUpdateMode.ScheduleUnchanged;

			transport.JW_ETD = initialFlightTime.AddHours(3);
			transport.JW_VoyageFlight = "CX100";
			Factory.Save();

			AssertEquals(transport.Sailing.Origin.JA_E_DEP, transport.JW_ETD);
			AssertEquals(transport.Sailing.Destination.JB_E_ARV, transport.JW_ETA);
			AssertEquals(initialFlightTime.AddDays(1), transport.JW_ETA);
			AssertEquals(initialFlightTime, transport.JW_ETD);
		}

		public void TestJW_ETAShouldKeepETAWhenEnteringETAFirst()
		{
			var initialFlightTime = ZDateTime.Today;
			var voyage = CreateVoyage("CX100", OverseasPort, initialFlightTime, HomePort, initialFlightTime.AddDays(1));
			voyage.GenerateSailings();
			Factory.Save();

			var transport = CreateTransport(ZString.Empty, OverseasPort, HomePort);
			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);
			UserSailingManagerQueryProvider.Register(Factory);
			ScheduleUpdateDialog.ResultForTesting.Value = SailingManagerUpdateMode.ScheduleUnchanged;

			transport.JW_ETA = initialFlightTime.AddHours(3);
			transport.JW_VoyageFlight = "CX100";
			Factory.Save();

			AssertEquals(transport.Sailing.Destination.JB_E_ARV, transport.JW_ETA);
			AssertEquals(transport.Sailing.Origin.JA_E_DEP, transport.JW_ETD);
			AssertEquals(initialFlightTime.AddDays(1), transport.JW_ETA);
			AssertEquals(initialFlightTime, transport.JW_ETD);
		}

		public void TestJW_ETAShouldKeepETAWhenSaving()
		{
			var initialFlightTime = ZDateTime.Today;
			var voyage = CreateVoyage("CX100", HomePort, initialFlightTime, OverseasPort, initialFlightTime.AddDays(1));
			voyage.GenerateSailings();
			Factory.Save();

			var transport = CreateTransport("CX100", HomePort, OverseasPort2);
			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);
			UserSailingManagerQueryProvider.Register(Factory);
			ScheduleUpdateDialog.ResultForTesting.Value = SailingManagerUpdateMode.ScheduleUnchanged;

			transport.JW_ETD = initialFlightTime.AddHours(1);
			transport.JW_ETA = initialFlightTime.AddHours(3);

			Factory.Save();
			AssertEquals(transport.Sailing.Destination.JB_E_ARV, transport.JW_ETA);
			AssertEquals(initialFlightTime.AddHours(3), transport.Sailing.Destination.JB_E_ARV);

			transport.JW_ETA = ZDateTime.Today.AddHours(5);
			Factory.Save();

			AssertEquals(ZDateTime.Today.AddHours(5), transport.JW_ETA);
			AssertEquals("Sailing ETA should not change because user selected KeepScheduleDate", initialFlightTime.AddHours(3), transport.Sailing.Destination.JB_E_ARV);
		}

		public void TestJW_ETDShouldKeepETDWhenSaving()
		{
			var initialFlightTime = ZDateTime.Today;
			var voyage = CreateVoyage("CX100", "USCHI", initialFlightTime, HomePort, initialFlightTime.AddDays(1));
			voyage.GenerateSailings();
			Factory.Save();

			var transport = CreateTransport("CX100", "USLAX", HomePort);
			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);
			UserSailingManagerQueryProvider.Register(Factory);
			ScheduleUpdateDialog.ResultForTesting.Value = SailingManagerUpdateMode.ScheduleUnchanged;
	
			transport.JW_ETA = initialFlightTime.AddHours(1);
			transport.JW_ETD = initialFlightTime.AddHours(3);

			Factory.Save();
			AssertEquals(transport.Sailing.Origin.JA_E_DEP, transport.JW_ETD);
			AssertEquals(initialFlightTime.AddHours(3), transport.Sailing.Origin.JA_E_DEP);

			transport.JW_ETD = ZDateTime.Today.AddHours(5);
			Factory.Save();

			AssertEquals(ZDateTime.Today.AddHours(5), transport.JW_ETD);
			AssertEquals("Sailing ETD should not change because user selected KeepScheduleDate", initialFlightTime.AddHours(3), transport.Sailing.Origin.JA_E_DEP);
		}

		public void TestJW_ETA_IsDirty_ShouldUpdateETAOnFoundVoyage()
		{
			var initialFlightTime = ZDateTime.Today;
			var voyage = CreateVoyage("CX100", HomePort, initialFlightTime, OverseasPort, initialFlightTime.AddDays(1));
			voyage.GenerateSailings();
			Factory.Save();

			var transport = CreateTransport("CX100", HomePort, OverseasPort2);
			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);
			UserSailingManagerQueryProvider.Register(Factory);
			ScheduleUpdateDialog.ResultForTesting.Value = SailingManagerUpdateMode.ScheduleUnchanged;

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			transport.JW_ETD = ZDateTime.Today;
			transport.JW_ETA = ZDateTime.Today.AddDays(1);
			Factory.Save();

			AssertNotNull("Precondition", transport.Sailing);
			AssertEquals("Voyage ETA should be set from Transport ETA", ZDateTime.Today.AddDays(1), transport.Sailing.Destination.JB_E_ARV);
		}

		#endregion

		#region ScheduleDateUpdates_UserSelectsToUpdateScheduleDate

		public void TestJW_ETDShouldUpdateETDWhenEnteringETDFirst()
		{
			var initialFlightTime = ZDateTime.Today;
			var voyage = CreateVoyage("CX100", HomePort, initialFlightTime, OverseasPort, initialFlightTime.AddDays(1));
			voyage.GenerateSailings();
			Factory.Save();

			var transport = CreateTransport(ZString.Empty, HomePort, OverseasPort);
			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);
			UserSailingManagerQueryProvider.Register(Factory);
			ScheduleUpdateDialog.ResultForTesting.Value = SailingManagerUpdateMode.UpdateSchedule;

			transport.JW_ETD = initialFlightTime.AddHours(3);
			transport.JW_VoyageFlight = "CX100";
			Factory.Save();

			AssertEquals(transport.Sailing.Destination.JB_E_ARV, transport.JW_ETA);
			AssertEquals(transport.Sailing.Origin.JA_E_DEP, transport.JW_ETD);
			AssertEquals(initialFlightTime.AddHours(3), transport.Sailing.Origin.JA_E_DEP);
			AssertEquals(initialFlightTime.AddDays(1), transport.JW_ETA);
		}

		public void TestJW_ETAShouldUpdateETAWhenEnteringETAFirst()
		{
			var initialFlightTime = ZDateTime.Today;
			var voyage = CreateVoyage("CX100", OverseasPort, initialFlightTime, HomePort, initialFlightTime.AddDays(1));
			voyage.GenerateSailings();
			Factory.Save();

			var transport = CreateTransport(ZString.Empty, OverseasPort, HomePort);
			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);
			UserSailingManagerQueryProvider.Register(Factory);
			ScheduleUpdateDialog.ResultForTesting.Value = SailingManagerUpdateMode.UpdateSchedule;

			transport.JW_ETA = initialFlightTime.AddHours(3);
			transport.JW_VoyageFlight = "CX100";
			Factory.Save();

			AssertEquals(transport.Sailing.Destination.JB_E_ARV, transport.JW_ETA);
			AssertEquals(transport.Sailing.Origin.JA_E_DEP, transport.JW_ETD);
			AssertEquals(initialFlightTime.AddHours(3), transport.Sailing.Destination.JB_E_ARV);
			AssertEquals(initialFlightTime, transport.JW_ETD);
		}

		public void TestJW_ETD_IsDirty_ShouldUpdateETDOnFoundVoyage()
		{
			var initialFlightTime = ZDateTime.Today;
			var voyage = CreateVoyage("CX100", HomePort, initialFlightTime, OverseasPort, initialFlightTime.AddDays(1));
			voyage.GenerateSailings();
			Factory.Save();

			var transport = CreateTransport("CX100", HomePort, OverseasPort);
			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);
			UserSailingManagerQueryProvider.Register(Factory);
			ScheduleUpdateDialog.ResultForTesting.Value = SailingManagerUpdateMode.UpdateSchedule;

			var addOneMinute = initialFlightTime.AddMinutes(1);
			transport.JW_ETD = addOneMinute;
			Factory.Save();

			AssertNotNull("Precondition", transport.Sailing);
			AssertEquals("Voyage ETD should be set from Transport ETD", addOneMinute, transport.Sailing.Origin.JA_E_DEP);
		}

		public void TestJW_ETAShouldUpdateETAWhenSaving()
		{
			var initialFlightTime = ZDateTime.Today;
			var voyage = CreateVoyage("CX100", HomePort, initialFlightTime, OverseasPort, initialFlightTime.AddDays(1));
			voyage.GenerateSailings();
			Factory.Save();

			var transport = CreateTransport("CX100", HomePort, OverseasPort2);
			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);
			UserSailingManagerQueryProvider.Register(Factory);
			ScheduleUpdateDialog.ResultForTesting.Value = SailingManagerUpdateMode.UpdateSchedule;

			transport.JW_ETD = initialFlightTime.AddHours(1);
			transport.JW_ETA = initialFlightTime.AddHours(3);

			Factory.Save();
			AssertEquals(transport.Sailing.Destination.JB_E_ARV, transport.JW_ETA);
			AssertEquals(initialFlightTime.AddHours(3), transport.Sailing.Destination.JB_E_ARV);

			transport.JW_ETA = ZDateTime.Today.AddHours(5);
			Factory.Save();

			AssertEquals(ZDateTime.Today.AddHours(5), transport.JW_ETA);
			AssertEquals("Sailing ETA should change because user selected UpdateSchedule", initialFlightTime.AddHours(5), transport.Sailing.Destination.JB_E_ARV);
		}

		public void TestJW_ETDShouldUpdateETDWhenSaving()
		{
			var initialFlightTime = ZDateTime.Today;
			var voyage = CreateVoyage("CX100", "USCHI", initialFlightTime, HomePort, initialFlightTime.AddDays(1));
			voyage.GenerateSailings();
			Factory.Save();

			var transport = CreateTransport("CX100", "USLAX", HomePort);
			var mockQueryProvider = new Mock<ISailingManagerQueryProvider>();
			SailingManagerQueryProviderFactory.Set(Factory, mockQueryProvider.Object);
			UserSailingManagerQueryProvider.Register(Factory);
			ScheduleUpdateDialog.ResultForTesting.Value = SailingManagerUpdateMode.UpdateSchedule;

			transport.JW_ETA = initialFlightTime.AddHours(1);
			transport.JW_ETD = initialFlightTime.AddHours(3);

			Factory.Save();
			AssertEquals(transport.Sailing.Origin.JA_E_DEP, transport.JW_ETD);
			AssertEquals(initialFlightTime.AddHours(3), transport.Sailing.Origin.JA_E_DEP);

			transport.JW_ETD = ZDateTime.Today.AddHours(5);
			Factory.Save();

			AssertEquals(ZDateTime.Today.AddHours(5), transport.JW_ETD);
			AssertEquals("Sailing ETD should change because user selected UpdateSchedule", initialFlightTime.AddHours(5), transport.Sailing.Origin.JA_E_DEP);
		}

		#endregion

		#region Implementation

		JobVoyage CreateVoyage(ZString flightNumber, ZString origin, ZDateTime etd, ZString destination, ZDateTime eta)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = flightNumber;

			var voyageOrigin = voyage.Origins.AddNew();
			voyageOrigin.JA_RL_NKPortOfLoading = origin;
			voyageOrigin.JA_E_DEP = etd;

			var voyageDestination = voyage.Destinations.AddNew();
			voyageDestination.JB_RL_NKPortOfDischarge = destination;
			voyageDestination.JB_E_ARV = eta;

			return voyage;
		}

		Transport CreateTransport(ZString voyageFlight, ZString origin, ZString destination)
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = ZGuid.NewZGuid().ToString().Substring(0, 16);
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Air;

			// Transport has nested ScheduleManager
			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_VoyageFlight = voyageFlight;
			transport.JW_RL_NKLoadPort = origin;
			transport.JW_RL_NKDiscPort = destination;
			transport.JW_IsLinked = true;

			return transport;
		}

		#endregion

		#region Provider

		UserSailingManagerQueryProvider Provider
		{
			get { return new UserSailingManagerQueryProvider(); }
		}

		#endregion
	}
}
