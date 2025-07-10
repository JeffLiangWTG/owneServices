using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class SailingFilterBuilderTest_ToShipmentFilter : SailingFilterBuilderTest<CommonShipment>
	{
		public void TestDontSplitAcrossTransports_ToShipmentFilter()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			CommonConsol consol = shipment.Consols.AddNew();
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
			CommonShipment[] shipments;

			Builder.LoadPort = HomePort;
			Builder.DischargePort = OverseasPort;
			shipments = Factory.Load<CommonShipment>(GetFilter(Builder));
			AssertCollectionContains(shipment, shipments);

			Builder.DischargePort = OverseasPort2;
			shipments = Factory.Load<CommonShipment>(GetFilter(Builder));
			AssertCollectionNotContains(shipment, shipments);

			Builder.LoadPort = OverseasPort;
			shipments = Factory.Load<CommonShipment>(GetFilter(Builder));
			AssertCollectionContains(shipment, shipments);
		}

		public void TestVesselFilterWithIsBlankOperatorForShipment()
		{
			var voyage1 = CreateVoyage(TestVessel1, "111", Core.Constants.TransportModes.Sea);
			var sailing1 = GetOrCreateSailing(voyage1, HomePort, OverseasPort);

			Factory.Save();

			var shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_JX = sailing1.PK;
			var transport1 = shipment1.Transports.AddNew();
			transport1.JW_JX = sailing1.PK;

			var shipment2 = Factory.New<CommonShipment>();
			var transport2 = shipment2.Transports.AddNew();
			transport2.JW_VoyageFlight = "FF56";
			transport2.JW_IsLinked = false;

			var shipment3 = Factory.New<CommonShipment>();
			var transport3 = shipment3.Transports.AddNew();
			transport3.JW_Vessel = TestVessel1.RV_FK;
			transport3.JW_VoyageFlight = ZString.Empty;
			transport3.JW_IsLinked = false;

			var consol4 = Factory.New<CommonConsol>();
			var shipment4 = consol4.Shipments.AddNew();

			var shipment5 = Factory.New<CommonShipment>();

			Factory.Save();

			Builder.Reset();
			CommonShipment[] shipments;

			Builder.Vessel = TestVessel1.RV_Name;
			Builder.VoyageFlight = "";
			Builder.VoyageFlightComparisonOperator = SQLComparisonOperator.Contains;

			shipments = Load(GetFilter(Builder));
			AssertContainsExactElementsInAnyOrder(new[] { shipment1, shipment3 }, shipments);

			Builder.Vessel = "";
			Builder.VoyageFlight = "";
			Builder.VoyageFlightComparisonOperator = SpecialComparisonOperator.IsBlank;

			shipments = Load(GetFilter(Builder));
			AssertContainsExactElementsInAnyOrder("Expect blank shipments -  in this test 4 & 5", new[] { shipment4, shipment5 }, shipments);

			Builder.VoyageFlightComparisonOperator = SpecialComparisonOperator.IsNotBlank;

			shipments = Load(GetFilter(Builder));
			AssertContainsExactElementsInAnyOrder("Expect not blank shipments -  in this test 1, 2 & 3", new[] { shipment1, shipment2, shipment3 }, shipments);
		}

		#region Implementation

		protected override CommonShipment[] CreateBusinessObjects(JobSailing sailing)
		{
			CommonShipment shipment1 = Factory.New<CommonShipment>();
			shipment1.JS_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			shipment1.JS_JX = sailing.PK;

			CommonConsol consol2 = Factory.New<CommonConsol>();
			consol2.JK_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			consol2.Transports[0].JW_JX = sailing.PK;
			consol2.Transports[0].JW_IsLinked = true;

			CommonShipment shipment2 = Factory.New<CommonShipment>();
			shipment2.JS_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			shipment2.Consols.Add(consol2);

			CommonConsol consol3 = Factory.New<CommonConsol>();
			consol3.JK_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			consol3.Transports[0].JW_JX = sailing.PK;
			consol3.Transports[0].JW_IsLinked = false;

			CommonShipment shipment3 = Factory.New<CommonShipment>();
			shipment3.JS_TransportMode = sailing.Voyage.JV_AirSeaRoad;
			shipment3.Consols.Add(consol3);

			CommonShipment shipment4 = Factory.New<CommonShipment>();
			shipment4.JS_TransportMode = sailing.Voyage.JV_AirSeaRoad;

			Transport transport4 = shipment4.Transports.AddNew();
			transport4.JW_IsLinked = true;
			transport4.JW_JX = sailing.PK;

			CommonShipment shipment5 = Factory.New<CommonShipment>();
			shipment5.JS_TransportMode = sailing.Voyage.JV_AirSeaRoad;

			Transport transport5 = shipment5.Transports.AddNew();
			transport5.JW_IsLinked = true;
			transport5.JW_JX = sailing.PK;
			transport5.JW_IsLinked = false;

			return new CommonShipment[] { shipment1, shipment2, shipment3, shipment4, shipment5 };
		}

		protected override ZQuery GetFilter(SailingFilterBuilder builder)
		{
			return builder.ToShipmentFilter(SailingFilterBuilder.RelationshipFlags.AllSchedules);
		}

		protected override bool IsLinked(CommonShipment bo)
		{
			return !bo.JS_JX.IsEmpty || (bo.TransportsIncludingRelated.Count > 0 && bo.TransportsIncludingRelated[0].JW_IsLinked);
		}

		#endregion
	}
}
