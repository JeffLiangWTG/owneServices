using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Moq;

namespace Enterprise.Rating.Business.Test.AutoRating.CarrierShipmentRates;

public class CarrierShipmentAutoRaterTest : RatingTestCase
{
	#region Rate Entry Matching

	#region Location

	public void TestRateSearch_OriginCityMatchesRateEntryCity()
	{
		var rateEntry = CreateFCLCostingEntry("DEHAM", null, "20GP");
		var rateLine = rateEntry.AddFlatRateLine("FRT", 1000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containerCargo = CreateContainerCargo(1, "20GP");
		var query = CreateRateQuery(shipment, routeLegs, containerCargo, true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AUSYD", [[new("SourcePK", rateLine.PK.ToGuid()), new("RateAmount", 1000m)]]);
	}

	public void TestRateSearch_OriginCityMatchesRateEntryCountry()
	{
		var rateEntry = CreateFCLCostingEntry("DE", null, "20GP");
		var rateLine = rateEntry.AddFlatRateLine("FRT", 1000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containerCargo = CreateContainerCargo(1, "20GP");
		var query = CreateRateQuery(shipment, routeLegs, containerCargo, true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AUSYD", [[new("SourcePK", rateLine.PK.ToGuid()), new("RateAmount", 1000m)]]);
	}

	public void TestRateSearch_OriginCountryMatchesRateEntryCountry()
	{
		var rateEntry = CreateFCLCostingEntry("DE", null, "20GP");
		var rateLine = rateEntry.AddFlatRateLine("FRT", 1000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DE", "AUSYD")]);
		var containerCargo = CreateContainerCargo(1, "20GP");
		var query = CreateRateQuery(shipment, routeLegs, containerCargo, true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DE", "AUSYD", [[new("SourcePK", rateLine.PK.ToGuid()), new("RateAmount", 1000m)]]);
	}

	public void TestRateSearch_DestinationCityMatchesRateEntryCity()
	{
		var rateEntry = CreateFCLCostingEntry(null, "AUSYD", "20GP");
		var rateLine = rateEntry.AddFlatRateLine("FRT", 1000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containerCargo = CreateContainerCargo(1, "20GP");
		var query = CreateRateQuery(shipment, routeLegs, containerCargo, true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AUSYD", [[new("SourcePK", rateLine.PK.ToGuid()), new("RateAmount", 1000m)]]);
	}

	public void TestRateSearch_DestinationCityMatchesRateEntryCountry()
	{
		var rateEntry = CreateFCLCostingEntry(null, "AU", "20GP");
		var rateLine = rateEntry.AddFlatRateLine("FRT", 1000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containerCargo = CreateContainerCargo(1, "20GP");
		var query = CreateRateQuery(shipment, routeLegs, containerCargo, true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AUSYD", [[new("SourcePK", rateLine.PK.ToGuid()), new("RateAmount", 1000m)]]);
	}

	public void TestRateSearch_DestinationCountryMatchesRateEntryCountry()
	{
		var rateEntry = CreateFCLCostingEntry(null, "AU", "20GP");
		var rateLine = rateEntry.AddFlatRateLine("FRT", 1000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AU")]);
		var containerCargo = CreateContainerCargo(1, "20GP");
		var query = CreateRateQuery(shipment, routeLegs, containerCargo, true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AU", [[new("SourcePK", rateLine.PK.ToGuid()), new("RateAmount", 1000m)]]);
	}

	public void TestRateSearch_RateEntryWithoutOriginAndDestinationIsMatched()
	{
		var rateEntry = CreateFCLCostingEntry(null, null, "20GP");
		var rateLine = rateEntry.AddFlatRateLine("FRT", 1000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containerCargo = CreateContainerCargo(1, "20GP");
		var query = CreateRateQuery(shipment, routeLegs, containerCargo, true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AUSYD", [[new("SourcePK", rateLine.PK.ToGuid()), new("RateAmount", 1000m)]]);
	}

	public void TestRateSearch_RateEntryWithDifferentOriginAndDestinationIsNotMatched()
	{
		var rateEntry = CreateFCLCostingEntry("NLRTM", "HUBUD", "20GP");
		var rateLine = rateEntry.AddFlatRateLine("FRT", 1000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containerCargo = CreateContainerCargo(1, "20GP");
		var query = CreateRateQuery(shipment, routeLegs, containerCargo, true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasNoCharge(results.Rates, CostSell.Cost, "DEHAM", "AUSYD");
	}

	#endregion

	#region Commodity

	public void TestRateSearch_CommodityMatchesRateEntryCommodity()
	{
		var standardCosting = Helper.NewCosting(null);
		var rateEntryGlue = CreateFCLCostingEntry(null, null, "20GP", "GLUE", costing: standardCosting);
		var rateLineGlue = rateEntryGlue.AddFlatRateLine("FRT", 1000);
		var rateEntryBeer = CreateFCLCostingEntry(null, null, "20GP", "BEER", costing: standardCosting);
		var rateLineBeer = rateEntryBeer.AddFlatRateLine("FRT", 2000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containerCargo = CreateContainerCargo(1, "20GP", "BEER");
		var query = CreateRateQuery(shipment, routeLegs, containerCargo, true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AUSYD", [[new("SourcePK", rateLineBeer.PK.ToGuid()), new("RateAmount", 2000m), new("CommodityCode", "BEER")]]);
	}

	#endregion

	#region Ready Date

	public void TestRateSearch_ReadyDateMatchesRateEntryWithValidDates()
	{
		var today = ZDate.Today;

		var standardCosting = Helper.NewCosting(null);
		var rateEntryValidStartAndExpiryDate = CreateFCLCostingEntry(null, null, "20GP", startDate: today.AddDays(-5), expiryDate: today.AddDays(12), costing: standardCosting);
		var rateLineValidStartAndExpiryDate = rateEntryValidStartAndExpiryDate.AddFlatRateLine("FRT", 1000);

		var rateEntryStartDateAfterReady = CreateFCLCostingEntry(null, null, "20GP", startDate: today.AddDays(13), expiryDate: today.AddDays(23), costing: standardCosting);
		var rateLineStartDateAfterReady = rateEntryStartDateAfterReady.AddFlatRateLine("FRT", 2000);

		var rateEntryExpiryDateBeforeReady = CreateFCLCostingEntry(null, null, "20GP", startDate: today.AddDays(-20), expiryDate: today.AddDays(-6), costing: standardCosting);
		var rateLineExpiryDateBeforeReady = rateEntryExpiryDateBeforeReady.AddFlatRateLine("FRT", 3000);

		var shipment = CreateShipment(today.ToDateTime());
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containerCargo = CreateContainerCargo(1, "20GP");
		var query = CreateRateQuery(shipment, routeLegs, containerCargo, true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AUSYD", [[new("SourcePK", rateLineValidStartAndExpiryDate.PK.ToGuid()), new("RateAmount", 1000m)]]);
	}

	#endregion

	#region Costings, Client Rates, Company Tariffs

	public void TestRateSearch_CostsAndSales_NonChargeableCargoIsSkippedForSales()
	{
		var standardCosting = Helper.NewCosting(null);
		var costingRateEntryBeer_20GP = CreateFCLCostingEntry("DEHAM", "AUSYD", "20GP", commodity: "BEER", costing: standardCosting);
		var costingRateLineBeer_20GP = costingRateEntryBeer_20GP.AddFlatRateLine("FRT", 1000);
		var costingRateEntryBeer_40GP = CreateFCLCostingEntry("DEHAM", "AUSYD", "40GP", commodity: "BEER", costing: standardCosting);
		var costingRateLineBeer_40GP = costingRateEntryBeer_40GP.AddFlatRateLine("FSC", 2000);
		var costingRateEntryGlue_20GP = CreateFCLCostingEntry("DEHAM", "AUSYD", "20GP", commodity: "GLUE", costing: standardCosting);
		var costingRateLineGlue_20GP = costingRateEntryGlue_20GP.AddFlatRateLine("BAF", 3000);

		var clientRate = Helper.NewClientRate(Consignor);
		var clientRateEntryBeer_20GP = CreateFCLClientRateEntry(clientRate, "DEHAM", "AUSYD", "20GP", commodity: "BEER");
		var clientRateLineBeer_20GP = clientRateEntryBeer_20GP.AddFlatRateLine("FRT", 1000);
		var clientRateEntryBeer_40GP = CreateFCLClientRateEntry(clientRate, "DEHAM", "AUSYD", "40GP", commodity: "BEER");
		var clientRateLineBeer_40GP = clientRateEntryBeer_40GP.AddFlatRateLine("FSC", 2000);
		var clientRateEntryGlue_20GP = CreateFCLClientRateEntry(clientRate, "DEHAM", "AUSYD", "20GP", commodity: "GLUE");
		var clientRateLineGlue_20GP = clientRateEntryGlue_20GP.AddFlatRateLine("BAF", 3000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containers = new List<CarrierShipmentRateCargoDto>();
		containers.AddRange(CreateContainerCargo(1, "20GP", "BEER"));
		containers.AddRange(CreateContainerCargo(1, "40GP", "BEER"));
		containers.AddRange(CreateContainerCargo(1, "20GP", "GLUE", isChargeable: false));

		var query = CreateRateQuery(shipment, routeLegs, [.. containers], true, true);
		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AUSYD",
			[
				[new("SourcePK", costingRateLineBeer_20GP.PK.ToGuid()), new("RateAmount", 1000m)],
				[new("SourcePK", costingRateLineBeer_40GP.PK.ToGuid()), new("RateAmount", 2000m)],
				[new("SourcePK", costingRateLineGlue_20GP.PK.ToGuid()), new("RateAmount", 3000m)],
			]);

		AssertHasCharges(results.Rates, CostSell.Revenue, "DEHAM", "AUSYD",
			[
				[new("SourcePK", clientRateLineBeer_20GP.PK.ToGuid()), new("RateAmount", 1000m)],
				[new("SourcePK", clientRateLineBeer_40GP.PK.ToGuid()), new("RateAmount", 2000m)],
			]);
	}

	public void TestRateSearch_NoClientSpecificRate_CompanyTariffIsMatched()
	{
		var companyTariff = Helper.NewCompanyTariff();
		companyTariff.TH_GlobalRateLevel = 1;
		var companyTariffRateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.SCO, Constants.RateMode.SEA, container: "20GP", removeLines: true);
		var companyTariffRateLine = companyTariffRateEntry.AddFlatRateLine("FRT", 1000);
		companyTariff.Factory.Save();
		var companyData = Consignor.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany);
		companyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containers = new List<CarrierShipmentRateCargoDto>();
		containers.AddRange(CreateContainerCargo(1, "20GP"));

		var query = CreateRateQuery(shipment, routeLegs, [.. containers], false, true);
		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Revenue, "DEHAM", "AUSYD", [[new("SourcePK", companyTariffRateLine.PK.ToGuid()), new("RateAmount", 1000m)]]);
	}

	public void TestRateSearch_ClientSpecificRateAvailable_CompanyTariffIsNotMatched()
	{
		var companyTariff = Helper.NewCompanyTariff();
		companyTariff.TH_GlobalRateLevel = 1;
		var companyTariffRateEntry = companyTariff.AddRateEntry(RatingConstants.RateCategory.SCO, Constants.RateMode.SEA, container: "20GP", removeLines: true);
		var companyTariffRateLine = companyTariffRateEntry.AddFlatRateLine("FRT", 1000);
		companyTariff.Factory.Save();
		var companyData = Consignor.GetCompanyDataForGlbCompany(GlbCompany.CurrentCompany);
		companyData.RateTariffLevels.SetLevel(OrgRateTariffLevel.DefaultTariffType, 1);

		var clientRate = Helper.NewClientRate(Consignor);
		var clientRateEntry = CreateFCLClientRateEntry(clientRate, null, null, "20GP");
		var clientRateLine = clientRateEntry.AddFlatRateLine("FRT", 500);
		Factory.Save();

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containers = new List<CarrierShipmentRateCargoDto>();
		containers.AddRange(CreateContainerCargo(1, "20GP"));

		var query = CreateRateQuery(shipment, routeLegs, [.. containers], false, true);
		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Revenue, "DEHAM", "AUSYD", [[new("SourcePK", clientRateLine.PK.ToGuid()), new("RateAmount", 500m)]]);
	}

	public void TestRateSearch_ClientSpecificRateAvailable_ConsigneeMatchesClient()
	{
		var consigneeOrg = Helper.NewOrgHeader("TSTCNE1");
		var clientRate = Helper.NewClientRate(consigneeOrg);
		var clientRateEntry = CreateFCLClientRateEntry(clientRate, null, null, "20GP");
		var clientRateLine = clientRateEntry.AddFlatRateLine("FRT", 500);
		Factory.Save();

		var shipment = CreateShipment(consignee: consigneeOrg.OH_Code);
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containers = new List<CarrierShipmentRateCargoDto>();
		containers.AddRange(CreateContainerCargo(1, "20GP"));

		var query = CreateRateQuery(shipment, routeLegs, [.. containers], false, true);
		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Revenue, "DEHAM", "AUSYD", [[new("SourcePK", clientRateLine.PK.ToGuid()), new("RateAmount", 500m)]]);
	}

	public void TestRateSearch_ClientSpecificRateAvailable_BookingPartyMatchesClient()
	{
		var bookingParty = Helper.NewOrgHeader("TSTBKG1");
		var clientRate = Helper.NewClientRate(bookingParty);
		var clientRateEntry = CreateFCLClientRateEntry(clientRate, null, null, "20GP");
		var clientRateLine = clientRateEntry.AddFlatRateLine("FRT", 500);
		Factory.Save();

		var shipment = CreateShipment(bookingParty: bookingParty.OH_Code);
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containers = new List<CarrierShipmentRateCargoDto>();
		containers.AddRange(CreateContainerCargo(1, "20GP"));

		var query = CreateRateQuery(shipment, routeLegs, [.. containers], false, true);
		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Revenue, "DEHAM", "AUSYD", [[new("SourcePK", clientRateLine.PK.ToGuid()), new("RateAmount", 500m)]]);
	}

	public void TestRateSearch_ClientSpecificRateAvailable_ConsignorMatchesClient()
	{
		var clientRate = Helper.NewClientRate(Consignor);
		var clientRateEntry = CreateFCLClientRateEntry(clientRate, null, null, "20GP");
		var clientRateLine = clientRateEntry.AddFlatRateLine("FRT", 500);
		Factory.Save();

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containers = new List<CarrierShipmentRateCargoDto>();
		containers.AddRange(CreateContainerCargo(1, "20GP"));

		var query = CreateRateQuery(shipment, routeLegs, [.. containers], false, true);
		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Revenue, "DEHAM", "AUSYD", [[new("SourcePK", clientRateLine.PK.ToGuid()), new("RateAmount", 500m)]]);
	}

	#endregion

	#region FCL & LCL

	public void TestRateSearch_ContainerMatchesRateEntryContainerType()
	{
		var standardCosting = Helper.NewCosting(null);
		var rateEntry20GP = CreateFCLCostingEntry(null, null, "20GP", costing: standardCosting);
		var rateLine20GP = rateEntry20GP.AddFlatRateLine("FRT", 1000);

		var rateEntry40GP = CreateFCLCostingEntry(null, null, "40GP", costing: standardCosting);
		var rateLine40GP = rateEntry40GP.AddFlatRateLine("FRT", 2000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containerCargo = CreateContainerCargo(1, "40GP");
		var breakBulkCargo = CreateBreakBulkCargo(1);
		var query = CreateRateQuery(shipment, routeLegs, [.. containerCargo, .. breakBulkCargo], true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AUSYD", [[new("SourcePK", rateLine40GP.PK.ToGuid()), new("RateAmount", 2000m), new("ContainerType", "40GP")]]);
	}

	public void TestRateSearch_NonContainerizedMatchesRateEntryWithoutContainerType()
	{
		var rateEntryLCL = CreateLCLCostingEntry(null, null);
		var rateLineLCL = rateEntryLCL.AddFlatRateLine("FRT", 1000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var containerCargo = CreateContainerCargo(1, "40GP");
		var breakBulkCargo = CreateBreakBulkCargo(1);
		var query = CreateRateQuery(shipment, routeLegs, [.. containerCargo, .. breakBulkCargo], true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AUSYD", [[new("SourcePK", rateLineLCL.PK.ToGuid()), new("RateAmount", 1000m), new("ContainerType", null)]]);
	}

	#endregion

	#endregion

	#region Rate Line Calculation

	public void TestRateSearch_CalculationWithDifferentRateLines()
	{
		var standardCosting = Helper.NewCosting(null);
		var rateEntry20GP = CreateFCLCostingEntry(null, null, "20GP", costing: standardCosting);

		var rateAmount20GP_Flat = 1000m;
		var rateLine20GP_Flat = rateEntry20GP.AddFlatRateLine("FRT", rateAmount20GP_Flat);

		var rateAmount20GP_CN = 10m;
		var rateLine20GP_CN = rateEntry20GP.AddUnitRateLine("FSC", rateAmount20GP_CN, "CN");

		var rateAmount20GP_BAG = 30m;
		var rateLine20GP_BAG = rateEntry20GP.AddUnitRateLine("BAF", rateAmount20GP_BAG, "BAG");

		var rateAmount20GP_KG = 50m;
		var rateLine20GP_KG = rateEntry20GP.AddUnitRateLine("CAF", rateAmount20GP_KG, "KG");

		var rateAmount20GP_M3 = 70m;
		var rateLine20GP_M3 = rateEntry20GP.AddUnitRateLine("WAR", rateAmount20GP_M3, "M3");
		Factory.Save();

		var rateEntryLCL = CreateLCLCostingEntry(null, null, costing: standardCosting);

		var rateAmountLCL_Flat = 2000m;
		var rateLineLCLFlat = rateEntryLCL.AddFlatRateLine("FRT", rateAmountLCL_Flat);

		var rateAmountLCL_CN = 20m;
		var rateLineLCL_CN = rateEntryLCL.AddUnitRateLine("FSC", rateAmountLCL_CN, "CN");

		var rateAmountLCL_BAG = 60m;
		var rateLineLCL_BAG = rateEntryLCL.AddUnitRateLine("BAF", rateAmountLCL_BAG, "BAG");

		var rateAmountLCL_KG = 100m;
		var rateLineLCL_KG = rateEntryLCL.AddUnitRateLine("CAF", rateAmountLCL_KG, "KG");

		var rateAmountLCL_M3 = 140m;
		var rateLineLCL_M3 = rateEntryLCL.AddUnitRateLine("WAR", rateAmountLCL_M3, "M3");
		Factory.Save();

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);

		var containerCount = 2;
		var pieceCountPerContainer = 3;
		var containerWeight = 5m;
		var containerVolume = 10m;
		var containerCargo = CreateContainerCargo(containerCount, "20GP", weight: containerWeight, volume: containerVolume, pieceCount: pieceCountPerContainer);

		var breakBulkCount = 10;
		var pieceCountPerBreakBulk = 4;
		var breakBulkWeight = 8m;
		var breakBulkVolume = 15m;
		var breakBulkCargo = CreateBreakBulkCargo(breakBulkCount, packageType: "BAG", weight: breakBulkWeight, volume: breakBulkVolume, pieceCount: pieceCountPerBreakBulk);

		var query = CreateRateQuery(shipment, routeLegs, [.. containerCargo, .. breakBulkCargo], true, false);

		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AUSYD",
			[
				[new("SourcePK", rateLine20GP_Flat.PK.ToGuid()), new("RateAmount", rateAmount20GP_Flat), new("ChargeCode", "FRT")],
				[new("SourcePK", rateLineLCLFlat.PK.ToGuid()), new("RateAmount", rateAmountLCL_Flat), new("ChargeCode", "FRT")],
				[new("SourcePK", rateLine20GP_CN.PK.ToGuid()), new("RateAmount", containerCount * pieceCountPerContainer * rateAmount20GP_CN), new("ChargeCode", "FSC")],
				[new("SourcePK", rateLineLCL_BAG.PK.ToGuid()), new("RateAmount", breakBulkCount * pieceCountPerBreakBulk * rateAmountLCL_BAG), new("ChargeCode", "BAF")],
				[new("SourcePK", rateLine20GP_KG.PK.ToGuid()), new("RateAmount", containerCount * pieceCountPerContainer * containerWeight * rateAmount20GP_KG), new("ChargeCode", "CAF")],
				[new("SourcePK", rateLineLCL_KG.PK.ToGuid()), new("RateAmount", breakBulkCount * pieceCountPerBreakBulk * breakBulkWeight * rateAmountLCL_KG), new("ChargeCode", "CAF")],
				[new("SourcePK", rateLine20GP_M3.PK.ToGuid()), new("RateAmount", containerCount * pieceCountPerContainer * containerVolume * rateAmount20GP_M3), new("ChargeCode", "WAR")],
				[new("SourcePK", rateLineLCL_M3.PK.ToGuid()), new("RateAmount", breakBulkCount * pieceCountPerBreakBulk * breakBulkVolume * rateAmountLCL_M3), new("ChargeCode", "WAR")],
			]);
	}

	#endregion

	#region Rating Mode

	public void TestRateSearch_End2End_MultipleLegs()
	{
		var standardCosting = Helper.NewCosting(null);
		var rateEntry = CreateFCLCostingEntry("DEHAM", "AUSYD", "20GP", costing: standardCosting);
		var rateLine = rateEntry.AddFlatRateLine("FRT", 1000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "SGSIN"), ("SGSIN", "AUSYD")]);
		var cargo = CreateContainerCargo(1, "20GP");
		var query = CreateRateQuery(shipment, routeLegs, cargo, true, false);
		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "AUSYD", [[new("SourcePK", rateLine.PK.ToGuid()), new("RateAmount", 1000m)]]);
	}

	public void TestRateSearch_End2End_SingleLegWithIndirectRates_NoResults()
	{
		var standardCosting = Helper.NewCosting(null);
		var rateEntrySG = CreateFCLCostingEntry("DEHAM", "SGSIN", "20GP", costing: standardCosting);
		var rateLineSG = rateEntrySG.AddFlatRateLine("FRT", 1000);
		var rateEntryAU = CreateFCLCostingEntry("SGSIN", "AUSYD", "20GP", costing: standardCosting);
		var rateLineAU = rateEntryAU.AddFlatRateLine("FRT", 2000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "AUSYD")]);
		var cargo = CreateContainerCargo(1, "20GP");
		var query = CreateRateQuery(shipment, routeLegs, cargo, true, false);
		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.E2E);

		AssertHasNoCharge(results.Rates, CostSell.Cost, "DEHAM", "AUSYD");
	}

	public void TestRateSearch_LegWise_MultipleLegsWithAvailableRates()
	{
		var standardCosting = Helper.NewCosting(null);
		var costingRateEntryDE_US = CreateFCLCostingEntry("DEHAM", "USLAX", "20GP", costing: standardCosting);
		var costingRateLineDE_US = costingRateEntryDE_US.AddFlatRateLine("FRT", 1000);
		var costingRateEntryUS_SG = CreateFCLCostingEntry("USLAX", "SGSIN", "20GP", costing: standardCosting);
		var costingRateLineUS_SG = costingRateEntryUS_SG.AddFlatRateLine("FRT", 2000);
		var costingRateEntrySG_AU = CreateFCLCostingEntry("SGSIN", "AUSYD", "20GP", costing: standardCosting);
		var costingRateLineSG_AU = costingRateEntrySG_AU.AddFlatRateLine("FRT", 3000);

		var shipment = CreateShipment();
		var routeLegs = CreateRouteLegs([("DEHAM", "USLAX"), ("USLAX", "SGSIN"), ("SGSIN", "AUSYD")]);

		var cargo = CreateContainerCargo(1, "20GP");
		var query = CreateRateQuery(shipment, routeLegs, cargo, true, false);
		var results = PerformRateSearch(query, TariffMatchingModeCodeList.Codes.LEG);

		AssertHasCharges(results.Rates, CostSell.Cost, "DEHAM", "USLAX", [[new("SourcePK", costingRateLineDE_US.PK.ToGuid()), new("RateAmount", 1000m)]]);

		AssertHasCharges(results.Rates, CostSell.Cost, "USLAX", "SGSIN", [[new("SourcePK", costingRateLineUS_SG.PK.ToGuid()), new("RateAmount", 2000m)]]);

		AssertHasCharges(results.Rates, CostSell.Cost, "SGSIN", "AUSYD", [[new("SourcePK", costingRateLineSG_AU.PK.ToGuid()), new("RateAmount", 3000m)]]);
	}

	#endregion	

	#region Helper functions

	CarrierShipmentRateResult PerformRateSearch(CarrierShipmentRateQueryDto rateQueryDto, string mode, ILogger logger = null)
	{
		using (RatingDataRegistry.Instance.EnableAutoRatingLogNoteForDebug.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
		using (OceanCarrierDataRegistry.Instance.TariffMatchingModeForCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mode))
		using (OceanCarrierDataRegistry.Instance.TariffMatchingModeForRevenue.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, mode))
		using (FreightDataRegistry.Instance.InternationalChargeableFactorSea.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new ChargeableFactor(ConversionFactor.Empty, ConversionFactor.Empty)))
		using (ObjectFactory.Substitute(Mock.Of<IRateQueryDtoConverter>()))
		{
			string defaultCurrencyCode = GlbCompany.CurrentCompany?.GC_RX_NKLocalCurrency ?? string.Empty;
			var chooserServices = new RateChooserServices(Factory, ZDateTime.Today, defaultCurrencyCode);
			return new CarrierShipmentAutoRater().AutoRateCarrierShipment(rateQueryDto, Factory, chooserServices, logger);
		}
	}

	CarrierShipmentRateCargoDto[] CreateContainerCargo(int numberOfContainers, string containerType,
		string commodity = "GEN", decimal? weight = null, decimal? volume = null, int pieceCount = 1,
		bool isChargeable = true)
	{
		var cargoArray = new CarrierShipmentRateCargoDto[numberOfContainers];
		for (var i = 0; i < numberOfContainers; i++)
		{
			cargoArray[i] = new CarrierShipmentRateCargoDto
			{
				ContainerType = containerType,
				Commodity = commodity,
				GrossWeight = weight,
				Volume = volume,
				PieceCount = pieceCount,
				ContainerNumber = null,
				IsChargeable = isChargeable,
			};
		}
		return cargoArray;
	}

	CarrierShipmentRateCargoDto[] CreateBreakBulkCargo(int numberOfBreakBulks, string commodity = "GEN",
		string packageType = "PKG", decimal? weight = null, decimal? volume = null, int pieceCount = 1,
		bool isChargeable = true)
	{
		var cargoArray = new CarrierShipmentRateCargoDto[numberOfBreakBulks];
		for (var i = 0; i < numberOfBreakBulks; i++)
		{
			cargoArray[i] = new CarrierShipmentRateCargoDto
			{
				PackageType = packageType,
				Commodity = commodity,
				GrossWeight = weight,
				Volume = volume,
				PieceCount = pieceCount,
				IsChargeable = isChargeable,
			};
		}
		return cargoArray;
	}

	CarrierShipmentRateRouteLegDto[] CreateRouteLegs(List<(string origin, string destination)> legs)
	{
		var legArray = new CarrierShipmentRateRouteLegDto[legs.Count];
		for (var i = 0; i < legs.Count; i++)
		{
			legArray[i] = new CarrierShipmentRateRouteLegDto
			{
				FromAddress = legs[i].origin,
				ToAddress = legs[i].destination,
				TransportMode = "SEA"
			};
		}
		return legArray;
	}

	CarrierShipmentRateShipmentDto CreateShipment(DateTime readyDate = default, string consignor = null, string consignee = null, string bookingParty = null)
	{
		var dto = new CarrierShipmentRateShipmentDto
		{
			Carrier = TransportProvider1.OH_Code,
			ReadyDate = readyDate
		};

		var rateParties = new List<CarrierShipmentRateRatePartyDto>
		{
			new() { Code = consignor ?? Consignor.OH_Code, Role = "CRD" }
		};

		if (consignee != null)
		{
			rateParties.Add(new() { Code = consignee, Role = "CED" });
		}

		if (bookingParty != null)
		{
			rateParties.Add(new() { Code = bookingParty, Role = "BKD" });
		}

		dto.RateParties = rateParties;

		return dto;
	}

	CarrierShipmentRateQueryDto CreateRateQuery(CarrierShipmentRateShipmentDto shipment, CarrierShipmentRateRouteLegDto[] routeLegs, CarrierShipmentRateCargoDto[] cargo, bool getRatesForCosts, bool getRatesForSales)
	{
		return new CarrierShipmentRateQueryDto
		{
			Shipment = shipment,
			RouteLegs = routeLegs,
			Cargo = cargo,
			GetRatesForCosts = getRatesForCosts,
			GetRatesForSales = getRatesForSales
		};
	}

	RateEntry CreateFCLClientRateEntry(ClientRate clientRate, string origin, string destination, string container, string commodity = null, ZDate startDate = default, ZDate expiryDate = default)
	{
		var rateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.SCO, Constants.RateMode.SEA, origin, destination,
			container: container, commodity: commodity, startDate: startDate, endDate: expiryDate, removeLines: true);

		Factory.Save();

		return rateEntry;
	}

	RateEntry CreateFCLCostingEntry(string origin, string destination, string container, string commodity = null, ZDate startDate = default, ZDate expiryDate = default, Costing costing = null)
	{
		var standardCosting = costing ?? Helper.NewCosting(null);
		var rateEntry = standardCosting.AddRateEntry(RatingConstants.RateCategory.SCO, Constants.RateMode.SEA, origin, destination,
			container: container, commodity: commodity, startDate: startDate, endDate: expiryDate, removeLines: true);

		Factory.Save();

		return rateEntry;
	}

	RateEntry CreateLCLCostingEntry(string origin, string destination, string commodity = null,
		ZDate startDate = default, ZDate expiryDate = default, Costing costing = null)
	{
		var standardCosting = costing ?? Helper.NewCosting(null);
		var rateEntry = standardCosting.AddRateEntry(RatingConstants.RateCategory.SNC, Constants.RateMode.LCL, origin,
			destination, commodity: commodity, startDate: startDate, endDate: expiryDate, removeLines: true);

		Factory.Save();

		return rateEntry;
	}

	void AssertHasCharges(ICollection<CarrierShipmentRateResultDto> results, CostSell costOrSell, string origin, string destination, List<List<(string propertyName, object value)>> expected)
	{
		AssertNotNull("results", results);
		var resultRecord = GetSpecificRateResultDto(results, costOrSell, origin, destination);
		AssertNotNull("resultRecord", resultRecord);
		AssertEquals(expected.Count, resultRecord.Charges.Count);

		foreach (var expectedItem in expected)
		{
			var errorMessage = $"No matching charge found for: {string.Join("; ", expectedItem.Select(x => $"{x.propertyName}={x.value?.ToString() ?? "null"}"))}";
			Assert(errorMessage, resultRecord.Charges.Any(x =>
			{
				var equalCharge = true;
				foreach (var (propertyName, value) in expectedItem)
				{
					equalCharge = value?.Equals(x.GetPropertyValue(propertyName)) ?? value == x.GetPropertyValue(propertyName);
				}

				return equalCharge;
			}));
		}
	}

	void AssertHasNoCharge(ICollection<CarrierShipmentRateResultDto> results, CostSell costOrSell, string origin, string destination)
	{
		AssertNotNull("results", results);
		var resultRecord = GetSpecificRateResultDto(results, costOrSell, origin, destination);
		AssertNull("resultRecord", resultRecord);
	}

	CarrierShipmentRateResultDto GetSpecificRateResultDto(ICollection<CarrierShipmentRateResultDto> results, CostSell costOrSell, string origin, string destination)
	{
		return results.FirstOrDefault(v =>
			v.CostOrSell == costOrSell && v.Criteria.Origin == origin && v.Criteria.Destination == destination);
	}

	#endregion
}
