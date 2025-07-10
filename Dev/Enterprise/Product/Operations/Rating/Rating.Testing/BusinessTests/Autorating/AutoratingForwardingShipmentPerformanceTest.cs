using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Testing;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.Business.Testing
{
	sealed class AutoratingForwardingShipmentPerformanceTest : BaseRatingIntegrationTest
	{
		[DeveloperOnlyTest]
		public void TestUserDefinedRateConditions_NoJSEngine() => AutorateWithUserDefinedRateConditions(useJSEngine: false, useDocEngineMacros: true);

		[DeveloperOnlyTest]
		public void TestUserDefinedRateConditions_UseJSEngine() => AutorateWithUserDefinedRateConditions(useJSEngine: true, useDocEngineMacros: true);

		[DeveloperOnlyTest]
		public void TestUserDefinedRateConditions_UseFormBuilderMacros() => AutorateWithUserDefinedRateConditions(useJSEngine: false, useDocEngineMacros: false);

		void AutorateWithUserDefinedRateConditions(bool useJSEngine, bool useDocEngineMacros)
		{
			Helper.ChargeCodes["FRT"].AC_IsGroupageCharge = false;

			var rate1 = CreateCostRate(container: "20GP", commodity: "ALUM").AddPerUnitCharge("FRT", 100, "KG");
			rate1.TL_Condition = RateLineConditions.UserDefined;
			rate1.TL_ConditionalExpression = useDocEngineMacros
				? "\"<Destination.Location.Country.Code>\"!=\"AU\"&&\"<Origin.Location.Country.Code>\"!=\"AU\"&&\"<FreightShipment.CoLoadMasterShipment.JS_HouseBill>\"==\"\""
				: "!JS_RL_NKDestination.StartsWith(\"AU\") && !JS_RL_NKOrigin.StartsWith(\"AU\") && JS_JS_ColoadMasterShipment.IsEmpty";

			var rate2 = CreateCostRate(container: "20GP", commodity: "SALT").AddPerUnitCharge("FRT", 200, "KG");
			rate2.TL_Condition = RateLineConditions.UserDefined;
			rate2.TL_ConditionalExpression = useDocEngineMacros
				? "\"<FreightShipment.CoLoadMasterShipment.JS_HouseBill>\" == \"\""
				: "JS_JS_ColoadMasterShipment.IsEmpty";

			var rate3 = CreateCostRate(container: "40GP", commodity: "SALT").AddPerUnitCharge("FRT", 400, "KG");
			rate3.TL_Condition = RateLineConditions.UserDefined;
			rate3.TL_ConditionalExpression = useDocEngineMacros
				? "\"<FreightShipment.CoLoadMasterShipment.JS_HouseBill>\"==\"\"&&\"<CurrentDepartmentCode>\"!=\"FEA\""
				: "JS_JS_ColoadMasterShipment.IsEmpty && @env.Department.Code != \"FEA\"";

			var rate4 = CreateCostRate(container: "40GP", commodity: "GEN").AddPerUnitCharge("FRT", 150, "KG");
			rate4.TL_Condition = RateLineConditions.UserDefined;
			rate4.TL_ConditionalExpression = useDocEngineMacros
				? "\"<BaseShipment.GetCustomField(PRIORITY)>\"==\"SRVB\" && \"<FreightShipment.CoLoadMasterShipment.JS_HouseBill>\"==\"\""
				: "GetCustomField(\"PRIORITY\")==\"SRVB\" && JS_JS_ColoadMasterShipment.IsEmpty";

			var consol = CreateConsol();

			var container20GPSALTPackingLines = new List<PackLine>();
			var container40GPALUMPackingLines = new List<PackLine>();
			var container40GPSALTPackingLines = new List<PackLine>();

			const int numberOfShipmentsToAutorate = 17;

			for (int i = 0; i < numberOfShipmentsToAutorate; i++)
			{
				var shipment = CreateShipment();
				consol.Shipments.Add(shipment);

				container20GPSALTPackingLines.AddRange(new[]
				{
					shipment.AddPackLine(commodity: "SALT", weight: 30),
					shipment.AddPackLine(commodity: "SALT", weight: 40),
					shipment.AddPackLine(commodity: "ALUM", weight: 200)
				});

				container40GPALUMPackingLines.AddRange(new[]
				{
					shipment.AddPackLine(commodity: "SALT", weight: 50),
					shipment.AddPackLine(commodity: "ALUM", weight: 150),
					shipment.AddPackLine(commodity: "GEN", weight: 250),
				});

				container40GPSALTPackingLines.AddRange(new[]
				{
					shipment.AddPackLine(commodity: "SALT", weight: 100),
					shipment.AddPackLine(commodity: "ALUM", weight: 200),
				});
			}

			consol.AddContainer("20GP", "SALT", packLines: container20GPSALTPackingLines);
			consol.AddContainer("40GP", "ALUM", packLines: container40GPALUMPackingLines);
			consol.AddContainer("40GP", "SALT", packLines: container40GPSALTPackingLines);

			Factory.Save();

			var shipmentsToAutorate = consol
				.Shipments
				.Cast<ForwardingShipment>()
				.ToArray();

			AssertEquals("prerequisite: have required number of shipments to autorate",
				numberOfShipmentsToAutorate,
				shipmentsToAutorate.Length);

			var times = Autorate(shipmentsToAutorate, useJSEngine);
			var sanitisedTimes = RemoveOutliers(times);

			var elapsedTimesInMs = string.Join(", ", times.Select(t => t.TotalMilliseconds));
			var elapsedSanitisedTimesInMs = string.Join(", ", sanitisedTimes.Select(t => t.TotalMilliseconds));
			var sanitisedAverageInMs = sanitisedTimes.Average(t => t.TotalMilliseconds);

			var useJSEngineDesc = useDocEngineMacros
				? useJSEngine.ToString()
				: "n/a";

			const double baselineInMs = 500d;
			const double allowedVariation = 2d;

			var autorateDurationMessage = $@"Autorated {numberOfShipmentsToAutorate} shipments with the following settings:
Used JSEngine: {useJSEngineDesc} (note: this is only applicable when using DocEngine macros as JS engine is not used to evaluate Form Builder macros)
Used DocEngine macros: {useDocEngineMacros}
Autorate times (in ms): {elapsedTimesInMs}
Sanitized Autorate times (in ms): {elapsedSanitisedTimesInMs}
Average: {sanitisedAverageInMs}ms
The average time  exceeded significantly the baseline of {baselineInMs}ms created on SYDCO-WMEP-2.
Please make sure that your changes in code (if any) did not contribute to making autorating slower.
If this is a false positive then please disregard and carry on :) 

Note: This is DEVELOPER ONLY test and won't fail on DAT";

			Assert(autorateDurationMessage, sanitisedAverageInMs < baselineInMs * allowedVariation);
		}

		TimeSpan[] Autorate(IReadOnlyCollection<ForwardingShipment> shipments, bool useJSEngine)
		{
			UserExpressionEvaluator.ClearStaticCaches();
			UnitTestUserNotification.Instance.ClearMessages();

			var expected = new AssertionCharge[]
			{
				new AssertionCharge
				{
					// 30 KG + 40 KG of SALT packed in 20 GP Container
					ChargeCode = "FRT",
					JR_OSCostAmt = 14000.00m,
					CostCalculationDescription = "FRT: 70 Kilogram(s) @ USD 200.00/KG"
				},
				new AssertionCharge
				{
					// 50 KG + 666 KG of SALT packed in 2 x 40 GP Containers
					ChargeCode = "FRT",
					JR_OSCostAmt = 60000.00m,
					CostCalculationDescription = "FRT: 150 Kilogram(s) @ USD 400.00/KG"
				}
			};

			var res = new List<TimeSpan>(shipments.Count);

			foreach (var shipment in shipments)
			{
				using (RawDataRegistry.Instance.UseJSEngineForAutoRatingConditionsEvaluation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, useJSEngine))
				using (var job = new JobHeader.Loader(shipment).TryLoadOrCreate() as Job)
				using (var plugin = new InvoicingPluginToFreight(shipment))
				{
					job.Charges.RemoveAndDeleteAll();
					job.ExchangeRates.RemoveAndDeleteAll();
					job.JH_OA_LocalChargesAddr = Consignee.MainAddress.PK;

					var options = new AutoRateOptions
					(
						autoRateCost: true,
						autoRateRevenue: false,
						excludeConsolLevelChargesOnCosting: false
					);

					var stopwatch = Stopwatch.StartNew();

					var result = plugin.ExecuteAutorating(options);

					stopwatch.Stop();

					AssertCharges("summary", expected, job);
					res.Add(stopwatch.Elapsed);
				}
			}

			return res.ToArray();
		}

		TimeSpan[] RemoveOutliers(TimeSpan[] times)
		{
			if (times.Length == 0)
			{
				return Array.Empty<TimeSpan>();
			}

			var meanValue = times.Average(t => t.TotalMilliseconds);
			var orderedTimes = times.OrderByDescending(t => Math.Abs(t.TotalMilliseconds - meanValue)).ToArray();
			var objectFarthestFromMean = orderedTimes[0];

			// remove time if is 1.2 times further from the mean
			var maxRange = 1.2d * meanValue;

			if (Math.Abs(objectFarthestFromMean.TotalMilliseconds - meanValue) > maxRange)
			{
				var timesWithoutOutlier = times.Skip(1).ToArray();

				return RemoveOutliers(timesWithoutOutlier);
			}

			return times;
		}

		RatingHeader Costing => costing ?? (costing = Helper.NewCosting(TransportProvider1));
		RatingHeader costing;

		RateEntry CreateCostRate(
			string category = "FCL",
			string mode = "SEA",
			string origin = "UAIEV",
			string destination = "AUSYD",
			string container = "20GP",
			string commodity = "GEN")
		{
			var entry = Costing.AddRateEntry(category, mode, origin, destination, "STD", container);
			entry.TI_RH_NKCommodityCode = commodity;
			entry.RateLines.RemoveAndDeleteAll();

			return entry;
		}
	}
}
