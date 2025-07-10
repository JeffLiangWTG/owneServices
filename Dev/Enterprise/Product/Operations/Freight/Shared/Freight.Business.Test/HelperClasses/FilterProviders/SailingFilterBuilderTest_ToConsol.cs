using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class SailingFilterBuilderTest_ToConsol : SailingFilterBuilderTest<CommonConsol>
	{
		public void TestDontSplitAccrossTransports()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			Transport transport1 = consol.Transports[0];
			Transport transport2 = consol.Transports.AddNew();

			transport1.JW_IsLinked = false;
			transport1.JW_RL_NKLoadPort = HomePort;
			transport1.JW_RL_NKDiscPort = OverseasPort;

			transport2.JW_IsLinked = false;
			transport2.JW_RL_NKLoadPort = OverseasPort;
			transport2.JW_RL_NKDiscPort = OverseasPort2;

			Factory.Save();

			Builder.Reset();
			CommonConsol[] consols;

			Builder.LoadPort = HomePort;
			Builder.DischargePort = OverseasPort;
			consols = Factory.Load<CommonConsol>(GetFilter(Builder));
			AssertCollectionContains(consol, consols);

			Builder.DischargePort = OverseasPort2;
			consols = Factory.Load<CommonConsol>(GetFilter(Builder));
			AssertCollectionNotContains(consol, consols);

			Builder.LoadPort = OverseasPort;
			consols = Factory.Load<CommonConsol>(GetFilter(Builder));
			AssertCollectionContains(consol, consols);
		}

		public void TestToShipmentFilter_WithDirectAndViaDirectTransports()
		{
			var voyage1 = CreateVoyage(TestVessel1, "3905SSSSS", Core.Constants.TransportModes.Sea);
			var sailing1 = GetOrCreateSailing(voyage1, HomePort, "USJAX");

			Factory.Save();

			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_JX = sailing1.PK;
			var transport1 = shipment1.Transports.AddNew();
			transport1.JW_JX = sailing1.PK;
			transport1.JW_IsLinked = true;

			var shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_JX = sailing1.PK;
			var transport2 = shipment2.Transports.AddNew();
			transport2.JW_JX = sailing1.PK;
			transport2.JW_IsLinked = true;

			Factory.Save();

			Builder.Reset();

			CommonShipment[] shipments;

			Builder.VoyageFlightComparisonOperator = SQLComparisonOperator.StartsWith;
			Builder.VoyageFlight = "3905X";
			Builder.DischargePort = "USJAX";
			var filter = Builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.Direct | SailingFilterBuilder.RelationshipFlags.ViaDirectTransports);

			shipments = Factory.Load<CommonShipment>(filter);
			AssertEquals(0, shipments.Length);

			Builder.VoyageFlight = "3905S";
			filter = Builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.Direct | SailingFilterBuilder.RelationshipFlags.ViaDirectTransports);
			shipments = Factory.Load<CommonShipment>(filter);
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment2 }, shipments);
		}

		public void TestVesselFilterWithIsBlankOperatorForConsol()
		{
			var voyage1 = CreateVoyage(TestVessel1, "111", Core.Constants.TransportModes.Sea);
			voyage1.JV_IsActive = true;
			var voyage2 = CreateVoyage(TestVessel2, "", Core.Constants.TransportModes.Sea);
			voyage2.JV_IsActive = true;

			var sailing1 = GetOrCreateSailing(voyage1, HomePort, OverseasPort);
			var sailing2 = GetOrCreateSailing(voyage2, HomePort, OverseasPort);

			Factory.Save();

			var consol1 = Factory.New<CommonConsol>();
			consol1.Transports[0].JW_JX = sailing1.PK;
			consol1.Transports[0].JW_IsLinked = true;

			var consol2 = Factory.New<CommonConsol>();
			consol2.Transports[0].JW_VoyageFlight = "FF56";
			consol2.Transports[0].JW_IsLinked = false;

			var consol3 = Factory.NewWithValidTestData<CommonConsol>();
			consol3.Transports[0].JW_IsLinked = false;

			var consol4 = Factory.New<CommonConsol>();
			consol4.Transports[0].JW_Vessel = TestVessel1.RV_FK;
			consol4.Transports[0].JW_VoyageFlight = ZString.Empty;
			consol4.Transports[0].JW_IsLinked = false;

			var consol5 = Factory.New<CommonConsol>();
			consol5.Transports[0].JW_JX = sailing2.PK;
			consol5.Transports[0].JW_IsLinked = true;

			Factory.Save();

			Builder.Reset();
			CommonConsol[] consols;

			Builder.Vessel = "";
			Builder.VoyageFlight = "";
			Builder.VoyageFlightComparisonOperator = SpecialComparisonOperator.IsBlank;
			consols = Load(GetFilter(Builder));

			AssertContainsExactElementsInAnyOrder(new[] { consol3 }, consols);

			Builder.VoyageFlightComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			consols = Load(GetFilter(Builder));

			AssertContainsExactElementsInAnyOrder(new[] { consol1, consol2, consol4, consol5 }, consols);
		}

		#region Implementation

		protected override CommonConsol[] CreateBusinessObjects(JobSailing sailing)
		{
			CommonConsol consol1 = Factory.New<CommonConsol>();
			consol1.JK_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			consol1.Transports[0].JW_JX = sailing.PK;
			consol1.Transports[0].JW_IsLinked = true;

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			consol2.Transports[0].JW_JX = sailing.PK;
			consol2.Transports[0].JW_IsLinked = false;

			return new CommonConsol[] { consol1, consol2 };
		}

		protected override ZQuery GetFilter(SailingFilterBuilder builder)
		{
			return builder.ToConsolFilter();
		}

		protected override bool IsLinked(CommonConsol bo)
		{
			return bo.Transports[0].JW_IsLinked;
		}

		#endregion
	}
}
