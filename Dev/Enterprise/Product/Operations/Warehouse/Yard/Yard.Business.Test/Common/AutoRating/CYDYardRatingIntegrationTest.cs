using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.RatingTests.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test.GUI
{
	public class CYDYardRatingIntegrationTest : BaseRatingIntegrationTest
	{
		public void TestAutoRating_ReceiveAdvice_HasYardInRateEntry()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var unloadLocation = row.Locations[0];

			var yardSpecificEntryParams = new CYDRateEntryParameters() { YardPk = yard.PK, LiftInCharge = 25, LiftInChargePlus = 5, LiftOutCharge = 30, LiftOutChargePlus = 3 };
			var localClient = SetupClientForRating("AAA", yardSpecificEntryParams, out RateEntry rateEntry);
			var otherRateEntryParams = new CYDRateEntryParameters() { LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			var otherClient = SetupClientForRating("BBB", otherRateEntryParams, out RateEntry otherRateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, unloadLocation);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			var unloadedYardUnit2 = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000039");
			Helper.UnloadYardUnit(unloadedYardUnit2, unloadTime: ZDateTimeOffset.Today, unloadLocation);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_ReceiveAdvice_DifferentYardInRateEntry()
		{
			var yard = Helper.CreateCYDWarehouse();
			var otherYard = Helper.CreateCYDWarehouse("WH2");
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var unloadLocation = row.Locations[0];

			var rateEntryForAnotherYardParams = new CYDRateEntryParameters() { YardPk = otherYard.PK };
			var localClient = SetupClientForRating("AAA", rateEntryForAnotherYardParams, out RateEntry rateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, unloadLocation);

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected, receiveAdvice, localClient, autorateCosts: false);
			AssertAutoratingAuditLogNotContains(receiveAdvice, "reason: expected '' Warehouse while rate is for '' Warehouse");
		}

		public void TestAutoRating_ReceiveAdvice_NoYardInRateEntry()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var unloadLocation = row.Locations[0];

			var rateEntryParams = new CYDRateEntryParameters() { LiftInCharge = 25, LiftInChargePlus = 5, LiftOutCharge = 30, LiftOutChargePlus = 3 };
			var localClient = SetupClientForRating("AAA", rateEntryParams, out RateEntry rateEntry);
			var otherRateEntryParams = new CYDRateEntryParameters() { LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			var otherClient = SetupClientForRating("BBB", otherRateEntryParams, out RateEntry otherRateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, unloadLocation);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_ReceiveAdvice_YardUnitType()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var unloadLocation = row.Locations[0];

			var rateEntryParamsForChassis = new CYDRateEntryParameters() { UnitType = "CNT", LiftInCharge = 1, LiftInChargePlus = 2, LiftOutCharge = 3, LiftOutChargePlus = 4 };
			var localClient = SetupClientForRating("AAA", rateEntryParamsForChassis, out RateEntry rateEntry);

			var rateEntryParamsForContainer = new CYDRateEntryParameters() { UnitType = "CHS", LiftInCharge = 5, LiftInChargePlus = 6, LiftOutCharge = 7, LiftOutChargePlus = 8 };
			Helper.CreateRateEntry((ClientRate)rateEntry.Parent, rateEntryParamsForContainer);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedContainerYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.UnloadYardUnit(unloadedContainerYardUnit, unloadTime: ZDateTimeOffset.Today, unloadLocation);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");

			Factory.Save();
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 1,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 2,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_ReceiveAdvice_YardUnitLoad()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var rateEntryParamsForEmpty = new CYDRateEntryParameters() { UnitLoad = "EMP", LiftInCharge = 1, LiftInChargePlus = 2, LiftOutCharge = 3, LiftOutChargePlus = 4 };
			var localClient = SetupClientForRating("AAA", rateEntryParamsForEmpty, out RateEntry rateEntry);

			var rateEntryParamsForLaden = new CYDRateEntryParameters() { UnitLoad = "LAD", LiftInCharge = 5, LiftInChargePlus = 6, LiftOutCharge = 7, LiftOutChargePlus = 8 };
			Helper.CreateRateEntry((ClientRate)rateEntry.Parent, rateEntryParamsForLaden);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedFullContainer = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017", isContainerEmpty: false);
			Helper.UnloadYardUnit(unloadedFullContainer, unloadTime: ZDateTimeOffset.Today, location);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 6,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_ReceiveAdvice_ContainerType()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var unloadLocation = row.Locations[0];

			var rateEntryParamsFor20GP = new CYDRateEntryParameters() { ContainerType = "20GP", LiftInCharge = 1, LiftInChargePlus = 2, LiftOutCharge = 3, LiftOutChargePlus = 4 };
			var localClient = SetupClientForRating("AAA", rateEntryParamsFor20GP, out RateEntry rateEntry);

			var rateEntryParamsFor20FR = new CYDRateEntryParameters() { ContainerType = "20FR", LiftInCharge = 5, LiftInChargePlus = 6, LiftOutCharge = 7, LiftOutChargePlus = 8 };
			Helper.CreateRateEntry((ClientRate)rateEntry.Parent, rateEntryParamsFor20FR);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloaded20GPUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017", containerTypeCode: "20GP");
			Helper.UnloadYardUnit(unloaded20GPUnit, unloadTime: ZDateTimeOffset.Today, unloadLocation);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 1,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 2,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_ReceiveAdvice_ContainerClassMatch()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var unloadLocation = row.Locations[0];

			var rateEntryParams = new CYDRateEntryParameters() { ContainerType = "20FR" };
			var localClient = SetupClientForRating("AAA", rateEntryParams, out RateEntry rateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017", containerTypeCode: "20GP");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, unloadLocation);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");

			Helper.SetContainerHandlingRateClass("CLS1", "20GP");
			Helper.SetContainerHandlingRateClass("CLS2", "20FR");

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected, receiveAdvice, localClient);

			var rateEntryParamsForContainerClassMatch = new CYDRateEntryParameters() { ContainerType = "20FR", ContainerClassMatch = true };
			Helper.UpdateRateEntry(rateEntry, rateEntryParamsForContainerClassMatch);
			Factory.Save();
			AutorateAndAssert(expected, receiveAdvice, localClient);

			Helper.SetContainerHandlingRateClass("CLS1", "20FR");
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_ReceiveAdvice_TransportMode()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var rateEntryParams = new CYDRateEntryParameters() { Mode = "ROA" };
			var localClient = SetupClientForRating("AAA", rateEntryParams, out RateEntry rateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, location);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_ReceiveAdvice_UseYardInDate()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var unloadLocation = row.Locations[0];

			var unloadedRateEntryParams = new CYDRateEntryParameters() { StartDate = ZDateTime.Now.AddDays(-5).Date, EndDate = ZDateTime.Now.AddDays(-3).Date };
			var localClient = SetupClientForRating("AAA", unloadedRateEntryParams, out RateEntry rateEntry, out ClientRate clientRate);
			var outsideRateEntryParams = new CYDRateEntryParameters() { StartDate = ZDateTime.Now.AddDays(-2).Date, LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			Helper.CreateRateEntry(clientRate, outsideRateEntryParams);
			var yardInRateEntryParams = new CYDRateEntryParameters() { StartDate = ZDateTime.Now.AddDays(-9).Date, EndDate = ZDateTime.Now.AddDays(-6).Date, LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			Helper.CreateRateEntry(clientRate, yardInRateEntryParams);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			var receiveTransportationUnit = Helper.CreateTransportationUnit("ADCEDS");
			Helper.AddDelivery(receiveTransportationUnit, unloadedYardUnit);
			Helper.GateInTransportationUnit(receiveTransportationUnit, new ZDateTimeOffset(ZDateTime.Now.AddDays(-8).Date), unloadLocation);
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: new ZDateTimeOffset(ZDateTime.Now.AddDays(-4).AddHours(9).Date), unloadLocation);
			Helper.GateOutTransportationUnit(receiveTransportationUnit, new ZDateTimeOffset(ZDateTime.Now.AddDays(-4).AddHours(10).Date));

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			var interactor = new TestInteractor();
			AutorateAndAssert(expected, receiveAdvice, localClient, testInteractor: interactor);
		}

		public void TestAutoRating_ReleaseAdvice_HasYardInRateEntry()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var unloadLocation = row.Locations[0];

			var rateEntryParams = new CYDRateEntryParameters() { YardPk = yard.PK };
			var localClient = SetupClientForRating("AAA", rateEntryParams, out RateEntry rateEntry);
			var otherRateEntryParams = new CYDRateEntryParameters() { LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			var otherClient = SetupClientForRating("BBB", rateEntryParams, out RateEntry otherRateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var releaseAdvice = Helper.CreateReleaseAdvice(localClient, yard);
			var loadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.AddReleaseAdviceLine(releaseAdvice, loadedYardUnit);
			Helper.UnloadYardUnit(loadedYardUnit, unloadTime: ZDateTimeOffset.Today.AddDays(-1), unloadLocation);
			Helper.LoadYardUnit(loadedYardUnit, loadTime: ZDateTimeOffset.Today);
			var waitingForLoadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			Helper.AddReleaseAdviceLine(releaseAdvice, waitingForLoadingYardUnit);
			Helper.UnloadYardUnit(waitingForLoadingYardUnit, unloadTime: ZDateTimeOffset.Today, unloadLocation);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYO Container liftout charge",
					RevenueCalculationDescription = "LIFTOUT"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYO Container liftout surcharge",
					RevenueCalculationDescription = "LIFTOUT+"
				},
			};
			AutorateAndAssert(expected, releaseAdvice, localClient);
		}

		public void TestAutoRating_ReleaseAdvice_NoYardInRateEntry()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var unloadLocation = row.Locations[0];

			var rateEntryParams = new CYDRateEntryParameters();
			var localClient = SetupClientForRating("AAA", rateEntryParams, out RateEntry rateEntry);
			var otherRateEntryParams = new CYDRateEntryParameters() { LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			var otherClient = SetupClientForRating("BBB", rateEntryParams, out RateEntry otherRateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var releaseAdvice = Helper.CreateReleaseAdvice(localClient, yard);
			var loadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.AddReleaseAdviceLine(releaseAdvice, loadedYardUnit);
			Helper.UnloadYardUnit(loadedYardUnit, unloadTime: ZDateTimeOffset.Today.AddDays(-1), unloadLocation);
			Helper.LoadYardUnit(loadedYardUnit, loadTime: ZDateTimeOffset.Today);
			var waitingForLoadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			Helper.AddReleaseAdviceLine(releaseAdvice, waitingForLoadingYardUnit);
			Helper.UnloadYardUnit(waitingForLoadingYardUnit, unloadTime: ZDateTimeOffset.Today, unloadLocation);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYO Container liftout charge",
					RevenueCalculationDescription = "LIFTOUT"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYO Container liftout surcharge",
					RevenueCalculationDescription = "LIFTOUT+"
				},
			};
			AutorateAndAssert(expected, releaseAdvice, localClient);
		}

		public void TestAutoRating_ReleaseAdvice_YardUnitType()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var rateEntryParams = new CYDRateEntryParameters() { UnitLoad = "EMP" };
			var localClient = SetupClientForRating("AAA", rateEntryParams, out RateEntry rateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var releaseAdvice = Helper.CreateReleaseAdvice(localClient, yard);
			var loadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017", isContainerEmpty: false);
			Helper.AddReleaseAdviceLine(releaseAdvice, loadedYardUnit);
			Helper.UnloadYardUnit(loadedYardUnit, unloadTime: ZDateTimeOffset.Today.AddDays(-1), location);
			Helper.LoadYardUnit(loadedYardUnit, loadTime: ZDateTimeOffset.Today);
			var waitingForLoadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			Helper.AddReleaseAdviceLine(releaseAdvice, waitingForLoadingYardUnit);
			Helper.UnloadYardUnit(waitingForLoadingYardUnit, unloadTime: ZDateTimeOffset.Today, location);

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();
			AutorateAndAssert("No rate matching for laden container.", expected, receiveAdvice, localClient);

			rateEntryParams = new CYDRateEntryParameters() { UnitLoad = "LAD" };
			Helper.UpdateRateEntry(rateEntry, rateEntryParams);
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYO Container liftout charge",
					RevenueCalculationDescription = "LIFTOUT"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYO Container liftout surcharge",
					RevenueCalculationDescription = "LIFTOUT+"
				},
			};
			AutorateAndAssert(expected, releaseAdvice, localClient);
		}

		public void TestAutoRating_ReleaseAdvice_YardUnitLoad()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var unloadLocation = row.Locations[0];

			var rateEntryParamsForChassis = new CYDRateEntryParameters() { UnitType = "CHS" };
			var localClient = SetupClientForRating("AAA", rateEntryParamsForChassis, out RateEntry rateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var releaseAdvice = Helper.CreateReleaseAdvice(localClient, yard);
			var loadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.AddReleaseAdviceLine(releaseAdvice, loadedYardUnit);
			Helper.UnloadYardUnit(loadedYardUnit, unloadTime: ZDateTimeOffset.Today.AddDays(-1), unloadLocation);
			Helper.LoadYardUnit(loadedYardUnit, loadTime: ZDateTimeOffset.Today);
			var waitingForLoadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			Helper.AddReleaseAdviceLine(releaseAdvice, waitingForLoadingYardUnit);
			Helper.UnloadYardUnit(waitingForLoadingYardUnit, unloadTime: ZDateTimeOffset.Today, unloadLocation);

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();
			AutorateAndAssert("No rate matching for a container.", expected, receiveAdvice, localClient);

			var rateEntryParamsForContainer = new CYDRateEntryParameters() { UnitType = "CNT" };
			Helper.UpdateRateEntry(rateEntry, rateEntryParamsForContainer);
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYO Container liftout charge",
					RevenueCalculationDescription = "LIFTOUT"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYO Container liftout surcharge",
					RevenueCalculationDescription = "LIFTOUT+"
				},
			};
			AutorateAndAssert(expected, releaseAdvice, localClient);
		}

		public void TestAutoRating_ReleaseAdvice_ContainerType()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var rateEntryParamsFor20FR = new CYDRateEntryParameters() { ContainerType = "20FR" };
			var localClient = SetupClientForRating("AAA", rateEntryParamsFor20FR, out RateEntry rateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var releaseAdvice = Helper.CreateReleaseAdvice(localClient, yard);
			var loadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.AddReleaseAdviceLine(releaseAdvice, loadedYardUnit);
			Helper.UnloadYardUnit(loadedYardUnit, unloadTime: ZDateTimeOffset.Today.AddDays(-1), location);
			Helper.LoadYardUnit(loadedYardUnit, loadTime: ZDateTimeOffset.Today);
			var waitingForLoadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			Helper.AddReleaseAdviceLine(releaseAdvice, waitingForLoadingYardUnit);
			Helper.UnloadYardUnit(waitingForLoadingYardUnit, unloadTime: ZDateTimeOffset.Today, location);

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected, receiveAdvice, localClient);

			var rateEntryParamsFor20GP = new CYDRateEntryParameters() { ContainerType = "20GP" };
			Helper.UpdateRateEntry(rateEntry, rateEntryParamsFor20GP);
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYO Container liftout charge",
					RevenueCalculationDescription = "LIFTOUT"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYO Container liftout surcharge",
					RevenueCalculationDescription = "LIFTOUT+"
				},
			};
			AutorateAndAssert(expected, releaseAdvice, localClient);
		}

		public void TestAutoRating_ReleaseAdvice_ContainerClassMatch()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var rateEntryParams = new CYDRateEntryParameters() { ContainerType = "20FR" };
			var localClient = SetupClientForRating("AAA", rateEntryParams, out RateEntry rateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var releaseAdvice = Helper.CreateReleaseAdvice(localClient, yard);
			var loadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.AddReleaseAdviceLine(releaseAdvice, loadedYardUnit);
			Helper.UnloadYardUnit(loadedYardUnit, unloadTime: ZDateTimeOffset.Today.AddDays(-1), location);
			Helper.LoadYardUnit(loadedYardUnit, loadTime: ZDateTimeOffset.Today);
			var waitingForLoadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			Helper.AddReleaseAdviceLine(releaseAdvice, waitingForLoadingYardUnit);
			Helper.UnloadYardUnit(waitingForLoadingYardUnit, unloadTime: ZDateTimeOffset.Today, location);

			Helper.SetContainerHandlingRateClass("CLS1", "20GP");
			Helper.SetContainerHandlingRateClass("CLS2", "20FR");

			Factory.Save();

			var expected = Array.Empty<AssertionCharge>();
			AutorateAndAssert(expected, receiveAdvice, localClient);

			rateEntryParams = new CYDRateEntryParameters() { ContainerType = "20FR", ContainerClassMatch = true };
			Helper.UpdateRateEntry(rateEntry, rateEntryParams);
			Factory.Save();
			AutorateAndAssert(expected, receiveAdvice, localClient);

			Helper.SetContainerHandlingRateClass("CLS1", "20FR");
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYO Container liftout charge",
					RevenueCalculationDescription = "LIFTOUT"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYO Container liftout surcharge",
					RevenueCalculationDescription = "LIFTOUT+"
				},
			};
			AutorateAndAssert(expected, releaseAdvice, localClient);
		}

		public void TestAutoRating_ReleaseAdvice_TransportMode()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var rateEntryParams = new CYDRateEntryParameters() { Mode = "ROA" };
			var localClient = SetupClientForRating("AAA", rateEntryParams, out RateEntry rateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var releaseAdvice = Helper.CreateReleaseAdvice(localClient, yard);
			var loadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.AddReleaseAdviceLine(releaseAdvice, loadedYardUnit);
			Helper.UnloadYardUnit(loadedYardUnit, unloadTime: ZDateTimeOffset.Today.AddDays(-1), location);
			Helper.LoadYardUnit(loadedYardUnit, loadTime: ZDateTimeOffset.Today);
			var waitingForLoadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			Helper.AddReleaseAdviceLine(releaseAdvice, waitingForLoadingYardUnit);
			Helper.UnloadYardUnit(waitingForLoadingYardUnit, unloadTime: ZDateTimeOffset.Today, location);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYO Container liftout charge",
					RevenueCalculationDescription = "LIFTOUT"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYO Container liftout surcharge",
					RevenueCalculationDescription = "LIFTOUT+"
				},
			};
			AutorateAndAssert(expected, releaseAdvice, localClient);
		}

		public void TestAutoRating_ReleaseAdvice_UseYardOutDate()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var rateEntryParams = new CYDRateEntryParameters() { StartDate = ZDateTime.Now.AddDays(-5).Date, EndDate = ZDateTime.Now.AddDays(-3).Date };
			var localClient = SetupClientForRating("AAA", rateEntryParams, out RateEntry rateEntry, out ClientRate clientRate);
			var otherRateEntryParams = new CYDRateEntryParameters() { StartDate = ZDateTime.Now.AddDays(-2).Date, LiftInCharge = 1, LiftInChargePlus = 2, LiftOutCharge = 3, LiftOutChargePlus = 4 };
			Helper.CreateRateEntry(clientRate, otherRateEntryParams);
			var pastRateEntryParams = new CYDRateEntryParameters() { StartDate = ZDateTime.Now.AddDays(-9).Date, EndDate = ZDateTime.Now.AddDays(-6).Date, LiftInCharge = 5, LiftInChargePlus = 6, LiftOutCharge = 7, LiftOutChargePlus = 8 };
			Helper.CreateRateEntry(clientRate, pastRateEntryParams);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var releaseAdvice = Helper.CreateReleaseAdvice(localClient, yard);
			var loadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.AddReleaseAdviceLine(releaseAdvice, loadedYardUnit);

			var receiveTransportationUnit = Helper.CreateTransportationUnit("ABCDE");
			Helper.GateInTransportationUnit(receiveTransportationUnit, new ZDateTimeOffset(ZDateTime.Now.AddDays(-5).AddHours(8).Date), location);
			Helper.UnloadYardUnit(loadedYardUnit, unloadTime: new ZDateTimeOffset(ZDateTime.Now.AddDays(-5).AddHours(9).Date), location);
			Helper.AddDelivery(receiveTransportationUnit, loadedYardUnit);
			Helper.GateOutTransportationUnit(receiveTransportationUnit, new ZDateTimeOffset(ZDateTime.Now.AddDays(-5).AddHours(10).Date));

			var dispatchTransportationUnit = Helper.CreateTransportationUnit("ABCD2");
			Helper.GateInTransportationUnit(dispatchTransportationUnit, new ZDateTimeOffset(ZDateTime.Now.AddDays(-4).AddHours(8).Date), location);
			Helper.LoadYardUnit(loadedYardUnit, loadTime: new ZDateTimeOffset(ZDateTime.Now.AddDays(-4).AddHours(9).Date));
			Helper.AddPickup(dispatchTransportationUnit, loadedYardUnit);
			Helper.GateOutTransportationUnit(dispatchTransportationUnit, new ZDateTimeOffset(ZDateTime.Now.AddDays(-4).AddHours(10).Date));

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYO Container liftout charge",
					RevenueCalculationDescription = "LIFTOUT"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTOUT+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYO Container liftout surcharge",
					RevenueCalculationDescription = "LIFTOUT+"
				},
			};
			var interactor = new TestInteractor();
			AutorateAndAssert(expected, releaseAdvice, localClient, testInteractor: interactor);
		}

		public void TestAutoRating_MultipleMatches_SpecificYardAndAllYards()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var specificYardRateEntryParams = new CYDRateEntryParameters() { YardPk = yard.PK };
			var localClient = SetupClientForRating("AAA", specificYardRateEntryParams, out RateEntry rateEntry, out ClientRate clientRate);
			var allYardsRateEntryParams = new CYDRateEntryParameters() { LiftInCharge = 1, LiftInChargePlus = 2, LiftOutCharge = 3, LiftOutChargePlus = 4 };
			Helper.CreateRateEntry(clientRate, allYardsRateEntryParams);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, location);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			var unloadedYardUnit2 = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000039");
			Helper.UnloadYardUnit(unloadedYardUnit2, unloadTime: ZDateTimeOffset.Today, location);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_MultipleMatches_SpecificYardUnitTypeAndEmpty()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var specificRateEntryParams = new CYDRateEntryParameters() { UnitType = "CNT" };
			var localClient = SetupClientForRating("AAA", specificRateEntryParams, out RateEntry rateEntry, out ClientRate clientRate);
			var genericRateEntryParams = new CYDRateEntryParameters() { UnitType = "", LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			Helper.CreateRateEntry(clientRate, genericRateEntryParams);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, location);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			var unloadedYardUnit2 = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000039");
			Helper.UnloadYardUnit(unloadedYardUnit2, unloadTime: ZDateTimeOffset.Today, location);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_MultipleMatches_SpecificYardUnitLoadAndEmpty()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var specificRateEntryParams = new CYDRateEntryParameters() { UnitLoad = "LAD" };
			var localClient = SetupClientForRating("AAA", specificRateEntryParams, out RateEntry rateEntry, out ClientRate clientRate);
			var genericRateEntryParams = new CYDRateEntryParameters() { UnitLoad = "", LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			Helper.CreateRateEntry(clientRate, genericRateEntryParams);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, location);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			var unloadedYardUnit2 = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000039");
			Helper.UnloadYardUnit(unloadedYardUnit2, unloadTime: ZDateTimeOffset.Today, location);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_MultipleMatches_SpecificTransportModeAndEmpty()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var specificRateEntryParams = new CYDRateEntryParameters() { Mode = "ROA" };
			var localClient = SetupClientForRating("AAA", specificRateEntryParams, out RateEntry rateEntry, out ClientRate clientRate);
			var genericRateEntryParams = new CYDRateEntryParameters() { Mode = "ALL", LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			Helper.CreateRateEntry(clientRate, genericRateEntryParams);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, location);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");
			var unloadedYardUnit2 = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000039");
			Helper.UnloadYardUnit(unloadedYardUnit2, unloadTime: ZDateTimeOffset.Today, location);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_MultipleMatches_ContainerTypeAndContainerClassMatch()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var containerClassMatchRateEntryParams = new CYDRateEntryParameters() { ContainerType = "20FR", ContainerClassMatch = true };
			var localClient = SetupClientForRating("AAA", containerClassMatchRateEntryParams, out RateEntry rateEntry, out ClientRate clientRate);
			var containerTypeRateEntryParams = new CYDRateEntryParameters() { ContainerType = "20GP", LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			Helper.CreateRateEntry(clientRate, containerTypeRateEntryParams);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017", containerTypeCode: "20GP");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, location);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");

			Helper.SetContainerHandlingRateClass("CLS1", "20GP");
			Helper.SetContainerHandlingRateClass("CLS1", "20FR");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_MultipleMatches_ContainerTypeAndContainerClassMatch_BothHaveMatchClass()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var containerClassMatchRateEntryParams = new CYDRateEntryParameters() { ContainerType = "20FR", ContainerClassMatch = true };
			var localClient = SetupClientForRating("AAA", containerClassMatchRateEntryParams, out RateEntry rateEntry, out ClientRate clientRate);
			var containerTypeRateEntryParams = new CYDRateEntryParameters() { YardPk = yard.PK, ContainerType = "20GP", ContainerClassMatch = true, LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			Helper.CreateRateEntry(clientRate, containerTypeRateEntryParams);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017", containerTypeCode: "20GP");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, location);
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");

			Helper.SetContainerHandlingRateClass("CLS1", "20GP");
			Helper.SetContainerHandlingRateClass("CLS1", "20FR");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_ChangeContainerTypeOnDelivery()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var containerClassMatchRateEntryParams = new CYDRateEntryParameters() { ContainerType = "20GP" };
			var localClient = SetupClientForRating("AAA", containerClassMatchRateEntryParams, out RateEntry rateEntry, out ClientRate clientRate);
			var containerTypeRateEntryParams = new CYDRateEntryParameters() { ContainerType = "20FR", LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			Helper.CreateRateEntry(clientRate, containerTypeRateEntryParams);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var receiveAdviceLineForUnloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017", containerTypeCode: "20GP");
			Helper.UnloadYardUnit(receiveAdviceLineForUnloadedYardUnit, unloadTime: ZDateTimeOffset.Today, location, containerTypeCode: "20FR"); // Container type changes at the delivery
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_ChangeContainerLoadOnDelivery()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var location = row.Locations[0];

			var containerClassMatchRateEntryParams = new CYDRateEntryParameters() { UnitLoad = "EMP" };
			var localClient = SetupClientForRating("AAA", containerClassMatchRateEntryParams, out RateEntry rateEntry, out ClientRate clientRate);
			var containerTypeRateEntryParams = new CYDRateEntryParameters() { UnitLoad = "LAD", LiftInCharge = 30, LiftInChargePlus = 3, LiftOutCharge = 25, LiftOutChargePlus = 5 };
			Helper.CreateRateEntry(clientRate, containerTypeRateEntryParams);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var receiveAdviceLineForUnloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017", isContainerEmpty: true);
			Helper.UnloadYardUnit(receiveAdviceLineForUnloadedYardUnit, unloadTime: ZDateTimeOffset.Today, location, "20GP", isContainerEmpty: false); // At the delivery container is found empty
			var waitingForUnloadingYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000028");

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 30,
					JR_Desc = "CYD - CYI Container liftin charge",
					RevenueCalculationDescription = "LIFTIN"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 3,
					JR_Desc = "CYD - CYI Container liftin surcharge",
					RevenueCalculationDescription = "LIFTIN+"
				},
			};
			AutorateAndAssert(expected, receiveAdvice, localClient);
		}

		public void TestAutoRating_ChargeDescription_UseOrganisationConfig()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var unloadLocation = row.Locations[0];

			var yardSpecificEntryParams = new CYDRateEntryParameters() { YardPk = yard.PK, LiftInCharge = 25, LiftInChargePlus = 5, LiftOutCharge = 30, LiftOutChargePlus = 3 };
			var localClient = SetupClientForRating("AAA", yardSpecificEntryParams, out RateEntry rateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			var unloadedYardUnit2 = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000039");
			var transportationUnit = Helper.CreateTransportationUnit("ABCDE", "TPU0001");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, unloadLocation);
			Helper.UnloadYardUnit(unloadedYardUnit2, unloadTime: ZDateTimeOffset.Today, unloadLocation);
			Helper.AddDelivery(transportationUnit, unloadedYardUnit);
			Helper.AddDelivery(transportationUnit, unloadedYardUnit2);

			Factory.Save();

			AutorateAndAssert([
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					RevenueCalculationDescription = "LIFTIN",
					JR_Desc = "CYD - CYI Container liftin charge"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					RevenueCalculationDescription = "LIFTIN",
					JR_Desc = "CYD - CYI Container liftin charge"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					RevenueCalculationDescription = "LIFTIN+",
					JR_Desc = "CYD - CYI Container liftin surcharge"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					RevenueCalculationDescription = "LIFTIN+",
					JR_Desc = "CYD - CYI Container liftin surcharge"
				},
			], receiveAdvice, localClient);

			var orgInvoiceRollupOrGroup = Factory.NewWithValidTestData<OrgInvoiceRollupOrGroup>();
			orgInvoiceRollupOrGroup.PG_JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			orgInvoiceRollupOrGroup.PG_ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			orgInvoiceRollupOrGroup.PG_TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			orgInvoiceRollupOrGroup.PG_GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			orgInvoiceRollupOrGroup.PG_GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;
			orgInvoiceRollupOrGroup.PG_InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;
			orgInvoiceRollupOrGroup.PG_InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;
			orgInvoiceRollupOrGroup.PG_OB = localClient.CompanyData.PK;

			Factory.Save();

			AutorateAndAssert([
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					RevenueCalculationDescription = "LIFTIN",
					JR_Desc = "CYD - CYI Container liftin charge - 1 Unit(s) @ AUD 25.00/Unit: CNTN0000017 - TPU0001"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					RevenueCalculationDescription = "LIFTIN",
					JR_Desc = "CYD - CYI Container liftin charge - 1 Unit(s) @ AUD 25.00/Unit: CNTN0000039 - TPU0001"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					RevenueCalculationDescription = "LIFTIN+",
					JR_Desc = "CYD - CYI Container liftin surcharge - 1 Unit(s) @ AUD 5.00/Unit: CNTN0000017 - TPU0001"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					RevenueCalculationDescription = "LIFTIN+",
					JR_Desc = "CYD - CYI Container liftin surcharge - 1 Unit(s) @ AUD 5.00/Unit: CNTN0000039 - TPU0001"
				},
			], receiveAdvice, localClient);
		}

		public void TestAutoRating_ChargeDescription_UseRegistryConfig()
		{
			var yard = Helper.CreateCYDWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(yard, "Dock", 2, 2);
			var unloadLocation = row.Locations[0];

			var yardSpecificEntryParams = new CYDRateEntryParameters() { YardPk = yard.PK, LiftInCharge = 25, LiftInChargePlus = 5, LiftOutCharge = 30, LiftOutChargePlus = 3 };
			var localClient = SetupClientForRating("AAA", yardSpecificEntryParams, out RateEntry rateEntry);

			var receiveAdvice = Helper.CreateReceiveAdvice(localClient, yard);
			var unloadedYardUnit = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000017");
			var unloadedYardUnit2 = Helper.AddReceiveAdviceLine(receiveAdvice, "CNTN0000039");
			var transportationUnit = Helper.CreateTransportationUnit("ABCDE", "TPU0001");
			Helper.UnloadYardUnit(unloadedYardUnit, unloadTime: ZDateTimeOffset.Today, unloadLocation);
			Helper.UnloadYardUnit(unloadedYardUnit2, unloadTime: ZDateTimeOffset.Today, unloadLocation);
			Helper.AddDelivery(transportationUnit, unloadedYardUnit);
			Helper.AddDelivery(transportationUnit, unloadedYardUnit2);

			Factory.Save();

			AutorateAndAssert([
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					RevenueCalculationDescription = "LIFTIN",
					JR_Desc = "CYD - CYI Container liftin charge"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN",
					JR_OSSellAmt = 25,
					RevenueCalculationDescription = "LIFTIN",
					JR_Desc = "CYD - CYI Container liftin charge"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					RevenueCalculationDescription = "LIFTIN+",
					JR_Desc = "CYD - CYI Container liftin surcharge"
				},
				new AssertionCharge
				{
					ChargeCode = "LIFTIN+",
					JR_OSSellAmt = 5,
					RevenueCalculationDescription = "LIFTIN+",
					JR_Desc = "CYD - CYI Container liftin surcharge"
				},
			], receiveAdvice, localClient);

			var invoiceRollupOrGroupCollection = new InvoiceRollupOrGroupCollection();
			var item = invoiceRollupOrGroupCollection.AddNew();
			item.JobType = OrgInvoiceRollupOrGroupLookups.JobType_List.All.Code;
			item.ServiceDirection = OrgConstants.ServiceDirection.Code.All;
			item.TransportMode = OrgConstants.ModesForGroupOrSubTotal.Codes.All;
			item.GroupOrSubTotal = OrgConstants.GroupOrSubTotalCharges.Code.RollUp;
			item.GroupOrSubtotalStyle = OrgConstants.InvoiceLineGroupings.Code.None;
			item.InvoiceLineDisplayOption = InvoiceDescriptionOptionsList.Codes.All;
			item.InvoicePostingStyle = InvoicePostingOptionsList.Codes.FinalInvoiceOnly;

			using (OrganisationRegistry.Instance.InvoiceRollupOrGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, invoiceRollupOrGroupCollection))
			{
				AutorateAndAssert([
					new AssertionCharge
					{
						ChargeCode = "LIFTIN",
						JR_OSSellAmt = 25,
						RevenueCalculationDescription = "LIFTIN",
						JR_Desc = "CYD - CYI Container liftin charge - 1 Unit(s) @ AUD 25.00/Unit: CNTN0000017 - TPU0001"
					},
					new AssertionCharge
					{
						ChargeCode = "LIFTIN",
						JR_OSSellAmt = 25,
						RevenueCalculationDescription = "LIFTIN",
						JR_Desc = "CYD - CYI Container liftin charge - 1 Unit(s) @ AUD 25.00/Unit: CNTN0000039 - TPU0001"
					},
					new AssertionCharge
					{
						ChargeCode = "LIFTIN+",
						JR_OSSellAmt = 5,
						RevenueCalculationDescription = "LIFTIN+",
						JR_Desc = "CYD - CYI Container liftin surcharge - 1 Unit(s) @ AUD 5.00/Unit: CNTN0000017 - TPU0001"
					},
					new AssertionCharge
					{
						ChargeCode = "LIFTIN+",
						JR_OSSellAmt = 5,
						RevenueCalculationDescription = "LIFTIN+",
						JR_Desc = "CYD - CYI Container liftin surcharge - 1 Unit(s) @ AUD 5.00/Unit: CNTN0000039 - TPU0001"
					},
				], receiveAdvice, localClient);
			}
		}

		public void TestRunInInteractiveEnvironment()
		{
			var testMethod = typeof(CYDYardRatingIntegrationTest).GetMethod(nameof(TestAutoRating_ReceiveAdvice_YardUnitLoad));
			Assert(GUITestDetection.IsGuiTest(testMethod));
		}

		#region Implementation

		new CYDYardTestHelper Helper
		{
			get { return helper ?? (helper = new CYDYardTestHelper(Factory)); }
		}

		CYDYardTestHelper helper;

		OrgHeader SetupClientForRating(string clientCode, CYDRateEntryParameters rateEntryParams, out RateEntry rateEntry)
		{
			var client = Helper.CreateClient(clientCode);
			client.OH_IsDebtor = true;
			var clientRate = Helper.CreateClientRate(client);
			rateEntry = Helper.CreateRateEntry(clientRate, rateEntryParams);

			return client;
		}

		OrgHeader SetupClientForRating(string clientCode, CYDRateEntryParameters rateEntryParams, out RateEntry rateEntry, out ClientRate clientRate)
		{
			var client = Helper.CreateClient(clientCode);
			client.OH_IsDebtor = true;
			clientRate = Helper.CreateClientRate(client);
			rateEntry = Helper.CreateRateEntry(clientRate, rateEntryParams);

			return client;
		}

		#endregion
	}
}
