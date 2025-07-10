namespace Enterprise.Freight.Forwarding.Routing.S8.Business.Test
{
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Freight.Forwarding.Business;

	internal sealed class RoutingRequestConsolGeneratorValidationTest : BusinessObjectValidationTestCase
	{
		public void TestRoutingRequestConsolGenerator_WeightUnit()
		{
			Generator.WeightUnit = "XX";
			AssertHasError(Generator.WeightUnitInfo, "Enter a valid selection.");

			Generator.WeightUnit = "KG";
			AssertNoNotifications(Generator.WeightUnitInfo);
		}

		public void TestRoutingRequestConsolGeneratorValidation_VolumeUnit()
		{
			Generator.VolumeUnit = "XX";
			AssertHasError(Generator.VolumeUnitInfo, "Enter a valid selection.");

			Generator.VolumeUnit = "M3";
			AssertNoNotifications(Generator.VolumeUnitInfo);
		}

		public void TestRoutingRequestConsolGeneratorValidation_ConsolsPerFlight()
		{
			Generator.ConsolsPerFlight = -1;
			AssertHasError(Generator.ConsolsPerFlightInfo, "value cannot be negative.");

			Generator.ConsolsPerFlight = 0;
			AssertHasError(Generator.ConsolsPerFlightInfo, "Please enter a value.");

			Generator.ConsolsPerFlight = 10;
			AssertNoNotifications(Generator.ConsolsPerFlightInfo);
		}

		public void TestRoutingRequestConsolGeneratorValidation_Shipments()
		{
			Generator.Shipments = -1;
			AssertHasError(Generator.ShipmentsInfo, "value cannot be negative.");

			Generator.Shipments = 10;
			AssertNoNotifications(Generator.ShipmentsInfo);
		}

		public void TestRoutingRequestConsolGeneratorValidation_Chargeable()
		{
			Generator.Chargeable = -1;
			AssertHasError(Generator.ChargeableInfo, "value cannot be negative.");

			Generator.Chargeable = 10;
			AssertNoNotifications(Generator.ChargeableInfo);
		}

		public void TestRoutingRequestConsolGeneratorValidation_AirlinePrefix()
		{
			Generator.AirlinePrefix = "VA";
			AssertNoNotifications(Generator.AirlinePrefixInfo);

			Generator.AirlinePrefix = "QF";
			AssertNoNotifications(Generator.AirlinePrefixInfo);

			Generator.AirlinePrefix = "Qf";
			AssertNoNotifications(Generator.AirlinePrefixInfo);

			Generator.AirlinePrefix = "";
			AssertNoNotifications(Generator.AirlinePrefixInfo);
		}

		public void TestRoutingRequestConsolGeneratorValidation_AirlinePrefix_ForDifferentCarriers()
		{
			var message = "There are multiple airline schedules selected.";

			Generator.AirlinePrefix = "MU";
			AssertNoWarning(Generator.AirlinePrefixInfo, message);

			var requestDate = new ZDateTime(2018, 7, 6);
			var testMessageLine1 =
				"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/26 2018/10/06 1.3..6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
			var testMessageLine2 =
				"000 SYD WUH 10:55   11:20   20:15 QF           2018/06/28 2018/10/08 1..4.6. <SYD 1 WUH     11:20   20:15 QF  5003    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
			var header2 = new RoutingResponseHeader(testMessageLine2, Factory);
			var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
			{
				header1,
				header2
			};

			var selection = RoutingMultiDaysSelection.Create(requestDate, routingResponseHeaders, false, false, Factory);
			var generator = new RoutingRequestConsolGenerator(selection);
			generator.AirlinePrefix = "QF";
			AssertHasWarning(generator.AirlinePrefixInfo, message);
		}

		public void TestRoutingRequestConsolGeneratorValidation_Weight()
		{
			Generator.Weight = -1;
			AssertHasError(Generator.WeightInfo, "value cannot be negative.");

			Generator.Weight = 10;
			AssertNoNotifications(Generator.WeightInfo);
		}

		public void TestRoutingRequestConsolGeneratorValidation_Volume()
		{
			Generator.Volume = -1;
			AssertHasError(Generator.VolumeInfo, "value cannot be negative.");

			Generator.Volume = 10;
			AssertNoNotifications(Generator.VolumeInfo);
		}

		#region Implementation

		RoutingRequestConsolGenerator Generator
		{
			get { return generator ?? (generator = new RoutingRequestConsolGenerator(MultiDaysSelectionForTest)); }
		}

		RoutingRequestConsolGenerator generator;

		RoutingMultiDaysSelection MultiDaysSelectionForTest
		{
			get
			{
				var requestDate = new ZDateTime(2018, 7, 6);
				var testMessageLine1 =
					"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/26 2018/10/06 1.3..6. <SYD 1 WUH     11:20   20:15 MU   750    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
				var header1 = new RoutingResponseHeader(testMessageLine1, Factory);
				var testMessageLine2 =
					"000 SYD WUH 10:55   11:20   20:15 MU           2018/06/28 2018/10/08 1..4.6. <SYD 1 WUH     11:20   20:15 QF  5003    332     0  5063                                        1..4.6. 18/06/28 18/10/06 J> ";
				var header2 = new RoutingResponseHeader(testMessageLine2, Factory);
				var routingResponseHeaders = new RoutingResponseHeaderCollection(Factory)
				{
					header1,
					header2
				};

				return RoutingMultiDaysSelection.Create(requestDate, routingResponseHeaders, false, false, Factory);
			}
		}

		#endregion
	}
}
