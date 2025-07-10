using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ShipmentMeasurementsHelperTest : TestCaseWithFactory
	{
		public void TestGetMeasurements_GetTotals_SingleShipment()
		{
			var expectedMeasurements = @"S00001234
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

Totals: 1 PLT                                              500.00 KG       0.528 M3";

			var expectedTotals = @"Packs: 1 PLT            Weight: 500.00 KG          Volume: 0.528 M3      Chargeable: 0.528 M3";

			var shipment = CreateShipmentWithPackline("S00001234");

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(shipment));
			AssertEquals("Incorrect Totals", expectedTotals, ShipmentMeasurementsHelper.GetTotals(true, true, new[] { shipment }));
		}

		public void TestGetMeasurements_GetTotals_MultipleShipment()
		{
			var expectedMeasurements = @"S00001234
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

S00001235
     2 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3
     1 PKG                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

Totals: 4 PKG                                             1500.00 KG       1.584 M3";

			var expectedTotals = @"Packs: 4 PKG           Weight: 1500.00 KG          Volume: 1.584 M3      Chargeable: 1.584 M3";

			var shipment1 = CreateShipmentWithPackline("S00001234");
			var shipment2 = CreateShipment("S00001235");
			CreateValidPackline(shipment2, quantity: 2, volume: 0.528m);
			CreateValidPackline(shipment2, type: "PKG");

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(shipment1, shipment2));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { shipment1, shipment2 }));
		}

		public void TestGetMeasurements_GetTotals_EmptyShipment()
		{
			var expectedMeasurements = @"S00001234
-

Totals: 0 PKG                                                0.00 KG       0.000 M3";

			var expectedTotals = @"Packs: 0 PKG              Weight: 0.00 KG          Volume: 0.000 M3      Chargeable: 0.000 M3";

			var shipment = CreateShipment("S00001234");
			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(shipment));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { shipment }));
		}

		public void TestGetMeasurements_GetTotals_MultipleEmptyShipments()
		{
			var expectedMeasurements = @"S00001234
-

S00001235
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

S00001236
-

Totals: 1 PLT                                              500.00 KG       0.528 M3";

			var expectedTotals = @"Packs: 1 PLT            Weight: 500.00 KG          Volume: 0.528 M3      Chargeable: 0.528 M3";

			var shipments = new ForwardingShipment[]
			{
				CreateShipment("S00001234"),
				CreateShipmentWithPackline("S00001235"),
				CreateShipment("S00001236"),
			};

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(shipments));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, shipments));
		}

		public void TestGetMeasurements_GetTotals_AllEmptyShipments()
		{
			var expectedMeasurements = @"S00001234
-

S00001235
-

S00001236
-

Totals: 0 PKG                                                0.00 KG       0.000 M3";

			var expectedTotals = @"Packs: 0 PKG              Weight: 0.00 KG          Volume: 0.000 M3      Chargeable: 0.000 M3";

			var shipments = new ForwardingShipment[]
			{
				CreateShipment("S00001234"),
				CreateShipment("S00001235"),
				CreateShipment("S00001236"),
			};

			var actual = ShipmentMeasurementsHelper.GetMeasurements(shipments);

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), actual);
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, shipments));
		}

		public void TestGetMeasurements_GetTotals_EmptyPackline()
		{
			var expectedMeasurements = @"S00001234
     0 PLT                       0.00 x 0.00 x 0.00 M       0.00 KG       0.000 M3

Totals: 0 PLT                                                0.00 KG       0.000 M3";

			var expectedTotals = @"Packs: 0 PLT              Weight: 0.00 KG          Volume: 0.000 M3      Chargeable: 0.000 M3";

			var shipment = CreateShipment("S00001234");
			CreateEmptyPackline(shipment);

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(shipment));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { shipment }));
		}

		public void TestGetMeasurements_GetTotals_MultiplePacklines()
		{
			var expectedMeasurements = @"S00001234
     1 PLT                       1.20 x 0.80 x 0.55 M      10.00 KG       0.528 M3
     1 PLT                       1.10 x 1.10 x 1.10 M     500.00 KG       0.528 M3
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG     999.000 M3
     1 PLT                      1.20 x 0.80 x 0.55 FT     500.00 LB       0.528 CF

Totals: 4 PLT                                             1236.80 KG    1000.071 M3";

			var expectedTotals = @"Packs: 4 PLT           Weight: 1236.80 KG       Volume: 1000.071 M3   Chargeable: 1000.071 M3";

			var shipment = CreateShipment("S00001234");
			CreateValidPackline(shipment, weight: 10);
			CreateValidPackline(shipment, l: 1.1m, w: 1.1m, h: 1.1m, volume: 0.528m);
			CreateValidPackline(shipment, volume: 999);
			CreateValidPackline(shipment, weightUQ: "LB", volumeUQ: "CF", dimensionUQ: "FT");

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(shipment));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { shipment }));
		}

		public void TestGetMeasurements_GetTotals_NullValueEntered()
		{
			AssertExceptionThrown("Null values aren't welcome", typeof(ArgumentException), () => { ShipmentMeasurementsHelper.GetMeasurements(null); });
		}

		public void TestGetMeasurements_GetTotals_MasterOnly()
		{
			var expectedMeasurements = @"S00001234
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

Totals: 2 PLT                                             1000.00 KG       1.056 M3";

			var expectedTotals = @"Packs: 2 PLT           Weight: 1000.00 KG          Volume: 1.056 M3      Chargeable: 1.056 M3";

			var masterShipment = CreateShipment("S00001234", Core.Constants.ShipmentTypes.CoLoadMaster);
			CreateShipmentWithPackline("S00001235", masterLeadShipment: masterShipment);
			CreateShipmentWithPackline("S00001236", masterLeadShipment: masterShipment);

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(masterShipment));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { masterShipment }));
		}

		public void TestGetMeasurements_GetTotals_MasterAndPartOfSubShipments()
		{
			var expectedMeasurements = @"S00001234
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

S00001235 (This is a sub-shipment of CLD Master S00001234, Consol# )
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

Totals: 2 PLT                                             1000.00 KG       1.056 M3";

			var expectedTotals = @"Packs: 2 PLT           Weight: 1000.00 KG          Volume: 1.056 M3      Chargeable: 1.056 M3";

			var masterShipment = CreateShipment("S00001234", Core.Constants.ShipmentTypes.CoLoadMaster);
			var subShipment1 = CreateShipmentWithPackline("S00001235", masterLeadShipment: masterShipment);
			CreateShipmentWithPackline("S00001236", masterLeadShipment: masterShipment);

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(masterShipment, subShipment1));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { masterShipment, subShipment1 }));
		}

		public void TestGetMeasurements_GetTotals_MasterAndAllSubShipments()
		{
			var expectedMeasurements = @"S00001234
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

S00001235 (This is a sub-shipment of CLD Master S00001234, Consol# )
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

S00001236 (This is a sub-shipment of CLD Master S00001234, Consol# )
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

Totals: 2 PLT                                             1000.00 KG       1.056 M3";

			var expectedTotals = "Packs: 2 PLT           Weight: 1000.00 KG          Volume: 1.056 M3      Chargeable: 1.056 M3";

			var masterShipment = CreateShipment("S00001234", Core.Constants.ShipmentTypes.CoLoadMaster);
			var subShipment1 = CreateShipmentWithPackline("S00001235", masterLeadShipment: masterShipment);
			var subShipment2 = CreateShipmentWithPackline("S00001236", masterLeadShipment: masterShipment);

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(masterShipment, subShipment1, subShipment2));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { masterShipment, subShipment1, subShipment2 }));
		}

		public void TestGetMeasurements_GetTotals_SubShipmentsWithoutMaster()
		{
			var expectedMeasurements = @"S00001235 (This is a sub-shipment of CLD Master S00001234, Consol# )
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

S00001236 (This is a sub-shipment of CLD Master S00001234, Consol# )
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

Totals: 2 PLT                                             1000.00 KG       1.056 M3";

			var expectedTotals = "Packs: 2 PLT           Weight: 1000.00 KG          Volume: 1.056 M3      Chargeable: 1.056 M3";

			var masterShipment = CreateShipment("S00001234", Core.Constants.ShipmentTypes.CoLoadMaster);
			var subShipment1 = CreateShipmentWithPackline("S00001235", masterLeadShipment: masterShipment);
			var subShipment2 = CreateShipmentWithPackline("S00001236", masterLeadShipment: masterShipment);

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(subShipment1, subShipment2));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { subShipment1, subShipment2 }));
		}

		public void TestGetMeasurements_GetTotals_LeadOnly()
		{
			var expectedMeasurements = @"S00001234 (This is a lead shipment of BCN type)
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

Totals: 1 PLT                                              500.00 KG       0.528 M3";

			var expectedTotals = "Packs: 1 PLT            Weight: 500.00 KG          Volume: 0.528 M3      Chargeable: 0.528 M3";

			var leadShipment = CreateShipmentWithPackline("S00001234", Core.Constants.ShipmentTypes.BuyersConsolLead);
			CreateShipmentWithPackline("S00001235", masterLeadShipment: leadShipment);
			CreateShipmentWithPackline("S00001236", masterLeadShipment: leadShipment);

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(leadShipment));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { leadShipment }));
		}

		public void TestGetMeasurements_GetTotals_LeadAndPartOfSubShipments()
		{
			var expectedMeasurements = @"S00001234 (This is a lead shipment of BCN type)
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

S00001235 (This is a sub-shipment of BCN Lead S00001234, Consol# )
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

Totals: 2 PLT                                             1000.00 KG       1.056 M3";

			var expectedTotals = @"Packs: 2 PLT           Weight: 1000.00 KG          Volume: 1.056 M3      Chargeable: 1.056 M3";

			var leadShipment = CreateShipmentWithPackline("S00001234", Core.Constants.ShipmentTypes.BuyersConsolLead);
			var subShipment1 = CreateShipmentWithPackline("S00001235", masterLeadShipment: leadShipment);
			CreateShipmentWithPackline("S00001236", masterLeadShipment: leadShipment);

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(leadShipment, subShipment1));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { leadShipment, subShipment1 }));
		}

		public void TestGetMeasurements_GetTotals_LeadAndAllSubShipments()
		{
			var expectedMeasurements = @"S00001234 (This is a lead shipment of BCN type)
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

S00001235 (This is a sub-shipment of BCN Lead S00001234, Consol# )
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

S00001236 (This is a sub-shipment of BCN Lead S00001234, Consol# )
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

Totals: 3 PLT                                             1500.00 KG       1.584 M3";

			var expectedTotals = "Packs: 3 PLT           Weight: 1500.00 KG          Volume: 1.584 M3      Chargeable: 1.584 M3";

			var leadShipment = CreateShipmentWithPackline("S00001234", Constants.ShipmentTypes.BuyersConsolLead);
			var subShipment1 = CreateShipmentWithPackline("S00001235", masterLeadShipment: leadShipment);
			var subShipment2 = CreateShipmentWithPackline("S00001236", masterLeadShipment: leadShipment);

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(leadShipment, subShipment1, subShipment2));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { leadShipment, subShipment1, subShipment2 }));
		}

		public void TestGetMeasurements_GetTotals_SubShipmentsWithoutLead()
		{
			var expectedMeasurements = @"S00001235 (This is a sub-shipment of BCN Lead S00001234, Consol# )
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

S00001236 (This is a sub-shipment of BCN Lead S00001234, Consol# )
     1 PLT                       1.20 x 0.80 x 0.55 M     500.00 KG       0.528 M3

Totals: 2 PLT                                             1000.00 KG       1.056 M3";

			var expectedTotals = @"Packs: 2 PLT           Weight: 1000.00 KG          Volume: 1.056 M3      Chargeable: 1.056 M3";

			var leadShipment = CreateShipmentWithPackline("S00001234", Core.Constants.ShipmentTypes.BuyersConsolLead);
			var subShipment1 = CreateShipmentWithPackline("S00001235", masterLeadShipment: leadShipment);
			var subShipment2 = CreateShipmentWithPackline("S00001236", masterLeadShipment: leadShipment);

			AssertEquals("Incorrect Measurements", expectedMeasurements.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetMeasurements(subShipment1, subShipment2));
			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { subShipment1, subShipment2 }));
		}

		public void TestGetTotals_MultipleAirShipments()
		{
			var expectedTotals = @"Packs: 3 PLT            Weight: 350.00 LB          Volume: 0.000 M3    Chargeable: 158.758 KG";

			var shipment1 = CreateShipment("S00001234", transportMode: Constants.TransportModes.Air);
			CreateValidPackline(shipment1, weight: 50, weightUQ: Constants.Weight.Pounds, volume: 0);
			var shipment2 = CreateShipment("S00001235", transportMode: Constants.TransportModes.Air);
			CreateValidPackline(shipment2, weight: 100, weightUQ: Constants.Weight.Pounds, volume: 0);
			CreateValidPackline(shipment2, weight: 200, weightUQ: Constants.Weight.Pounds, volume: 0);

			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { shipment1, shipment2 }));
		}

		public void TestGetTotals_MultipleMixedShipments()
		{
			var expectedTotals = @"Packs: 3 PLT            Weight: 186.08 KG          Volume: 2.832 M3           Chargeable: N/A";

			var shipment1 = CreateShipment("S00001234", transportMode: Constants.TransportModes.Sea);
			CreateValidPackline(shipment1, weight: 50, volume: 100, volumeUQ: Constants.Volume.CubicFeet);
			var shipment2 = CreateShipment("S00001235", transportMode: Constants.TransportModes.Air);
			CreateValidPackline(shipment2, weight: 100, weightUQ: Constants.Weight.Pounds, volume: 0);
			CreateValidPackline(shipment2, weight: 200, weightUQ: Constants.Weight.Pounds, volume: 0);

			AssertEquals("Incorrect Totals", expectedTotals.Replace("\r\n", "\n"), ShipmentMeasurementsHelper.GetTotals(true, true, new[] { shipment1, shipment2 }));
		}

		public void TestGetTotals_NoShipment()
		{
			var expectedTotals = @"Packs: 0 PKG              Weight: 0.00 KG          Volume: 0.000 M3      Chargeable: 0.000 KG";
			AssertEquals("Incorrect Totals", expectedTotals, ShipmentMeasurementsHelper.GetTotals(true, true));
		}

		ForwardingPackLine CreateValidPackline(ForwardingShipment forwardingShipment, int quantity = 1, string type = "PLT", decimal weight = 500, string weightUQ = "KG", decimal l = 1.2m, decimal w = 0.8m, decimal h = 0.55m, string dimensionUQ = "M", decimal volume = 0.528m, string volumeUQ = "M3")
		{
			var packline = CreatePackline(forwardingShipment, quantity, type, weight, weightUQ, l, w, h, dimensionUQ, volume, volumeUQ);
			forwardingShipment.UpdateShipmentFromOuterPackLines();
			return packline;
		}

		ForwardingPackLine CreateEmptyPackline(ForwardingShipment forwardingShipment)
		{
			return CreatePackline(forwardingShipment, 0, "PLT", 0, "KG", 0, 0, 0, "M", 0, "M3");
		}

		ForwardingPackLine CreatePackline(ForwardingShipment forwardingShipment, int quantity, string type, decimal weight, string weightUQ, decimal l, decimal w, decimal h, string dimensionUQ, decimal volume, string volumeUQ)
		{
			var packline = forwardingShipment.OuterPackLines.AddNew();
			packline.JL_PackageCount = quantity;
			packline.JL_F3_NKPackType = type;
			packline.JL_ActualWeight = weight;
			packline.JL_ActualWeightUQ = weightUQ;
			packline.JL_Length = l;
			packline.JL_Width = w;
			packline.JL_Height = h;
			packline.JL_UnitOfDimension = dimensionUQ;
			packline.JL_ActualVolumeUQ = volumeUQ;
			packline.JL_ActualVolume = volume;
			return packline;
		}

		ForwardingShipment CreateShipment(string shipmentNumber, string shipmentType = Core.Constants.ShipmentTypes.StandardHouse, string transportMode = "SEA", ForwardingShipment masterLeadShipment = null)
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = transportMode;
			shipment.JS_UniqueConsignRef = shipmentNumber;
			shipment.JS_ShipmentType = shipmentType;
			if (masterLeadShipment != null)
			{
				shipment.JS_JS_ColoadMasterShipment = masterLeadShipment.PK;
			}
			return shipment;
		}

		ForwardingShipment CreateShipmentWithPackline(string shipmentNumber, string shipmentType = Core.Constants.ShipmentTypes.StandardHouse, string transportMode = "SEA", ForwardingShipment masterLeadShipment = null)
		{
			var shipment = CreateShipment(shipmentNumber, shipmentType, transportMode, masterLeadShipment);
			CreateValidPackline(shipment);
			return shipment;
		}
	}
}
