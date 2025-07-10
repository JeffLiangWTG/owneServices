using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Freight.Business.SailingFilterBuilder;

namespace Enterprise.Freight.Business.Testing
{
	sealed class SailingFilterBuilderTest_ToTransportFilter : SailingFilterBuilderTest<Transport>
	{
		public void TestVesselFilterWithIsBlankOperatorForTransport()
		{
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Name = "";

			var voyage1 = CreateVoyage(TestVessel1, "111", Core.Constants.TransportModes.Sea);
			var voyage2 = CreateVoyage(vessel, "", Core.Constants.TransportModes.Sea);

			var sailing1 = GetOrCreateSailing(voyage1, HomePort, OverseasPort);
			var sailing2 = GetOrCreateSailing(voyage2, HomePort, OverseasPort);

			Factory.Save();

			var transport1 = Factory.New<Transport>();
			transport1.ParentType = typeof(CommonConsol);
			transport1.JW_TransportMode = sailing1.Voyage.JV_AirSeaRoad;
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing1.PK;

			var transport2 = Factory.New<Transport>();
			transport2.ParentType = typeof(CommonConsol);
			transport2.JW_VoyageFlight = "FF56";
			transport2.JW_IsLinked = false;

			var transport3 = Factory.New<Transport>();
			transport3.ParentType = typeof(CommonConsol);
			transport3.JW_IsLinked = false;

			var transport4 = Factory.New<Transport>();
			transport4.ParentType = typeof(CommonConsol);
			transport4.JW_Vessel = TestVessel1.RV_FK;
			transport4.JW_VoyageFlight = ZString.Empty;
			transport4.JW_IsLinked = false;

			Factory.Save();

			Builder.Reset();
			Transport[] transports;

			Builder.Vessel = "";
			Builder.VoyageFlight = "";
			Builder.VoyageFlightComparisonOperator = SpecialComparisonOperator.IsBlank;

			transports = Load(GetFilter(Builder));
			AssertContainsExactElementsInAnyOrder(new[] { transport3 }, transports);

			Builder.Vessel = TestVessel1.RV_Name;
			Builder.VoyageFlight = "";
			Builder.VoyageFlightComparisonOperator = SQLComparisonOperator.Contains;

			transports = Load(GetFilter(Builder));
			AssertContainsExactElementsInAnyOrder(new[] { transport1, transport4 }, transports);

			Builder.Vessel = "";
			Builder.VoyageFlight = "";
			Builder.VoyageFlightComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			transports = Load(GetFilter(Builder));
			AssertContainsExactElementsInAnyOrder(new[] { transport1, transport2, transport4 }, transports);
		}

		public void TestCommonDatatimeFilters()
		{
			var todayMorning = ZDateTime.Today.AddHours(9);
			var transport = Factory.NewWithValidTestData<Transport>();
			transport.ParentType = typeof(CommonConsol);
			transport.JW_TerminalCutOff = todayMorning;
			transport.JW_DepotCutOff = todayMorning;
			transport.JW_DocumentaryCutOff = todayMorning;
			transport.JW_VGMCutOff = todayMorning;
			transport.JW_TerminalReceivalCommences = todayMorning;
			transport.JW_DepotReceivalCommences = todayMorning;
			transport.JW_TerminalAvailabilityDate = todayMorning;
			transport.JW_DepotAvailabilityDate = todayMorning;
			transport.JW_TerminalStorageDate = todayMorning;
			transport.JW_DepotStorageDate = todayMorning;
			Factory.Save();

			AssertDatatimeFilter(transport, Dates.CTOCutOff, todayMorning);
			AssertDatatimeFilter(transport, Dates.CFSCutOff, todayMorning);
			AssertDatatimeFilter(transport, Dates.DocsDue, todayMorning);
			AssertDatatimeFilter(transport, Dates.VGMCutOff, todayMorning);
			AssertDatatimeFilter(transport, Dates.CTOReceival, todayMorning);
			AssertDatatimeFilter(transport, Dates.CFSReceival, todayMorning);
			AssertDatatimeFilter(transport, Dates.CTOAvailable, todayMorning);
			AssertDatatimeFilter(transport, Dates.CFSAvailable, todayMorning);
			AssertDatatimeFilter(transport, Dates.CTOStorage, todayMorning);
			AssertDatatimeFilter(transport, Dates.CFSStorage, todayMorning);
		}

		void AssertDatatimeFilter(Transport transport, Dates dateType, ZDateTime benchmark)
		{
			Builder.Reset();
			var found = Load(GetFilter(Builder));
			AssertCollectionContains("Precondition - Empty filter should match all records", transport, found);

			Builder.Reset();
			Builder.SetDateRange(dateType, DateComparisonOperator.HasDateInRange, benchmark.AddHours(-2), benchmark.AddHours(3));
			found = Load(GetFilter(Builder));
			AssertCollectionContains("Expected to find Morning Records", transport, found);

			Builder.Reset();
			Builder.SetDateRange(dateType, DateComparisonOperator.HasDateInRange, benchmark.AddHours(3), benchmark.AddHours(12));
			found = Load(GetFilter(Builder));
			AssertCollectionNotContains("Expected to NOT find any Morning Records", transport, found);
		}

		#region Implementation

		protected override Transport[] CreateBusinessObjects(JobSailing sailing)
		{
			Transport transport1 = Factory.New<Transport>();
			transport1.ParentType = typeof(CommonConsol);
			transport1.JW_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			transport1.JW_IsLinked = true;
			transport1.JW_JX = sailing.PK;

			Transport transport2 = Factory.New<Transport>();
			transport2.ParentType = typeof(CommonConsol);
			transport2.JW_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			transport2.JW_IsLinked = true;
			transport2.JW_JX = sailing.PK;
			transport2.JW_IsLinked = false;

			return new Transport[] { transport1, transport2 };
		}

		protected override ZQuery GetFilter(SailingFilterBuilder builder)
		{
			return builder.ToTransportFilter();
		}

		protected override bool IsLinked(Transport bo)
		{
			return bo.JW_IsLinked;
		}

		#endregion
	}
}
