using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Business.Test;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using Urs.Api.Integration.DTOs;
using WiseRates.Api.Model;
using static Enterprise.Rating.Business.UrsConstants;
using RefContainer = Enterprise.MasterFiles.Business.RefContainer;

namespace Enterprise.Rating.CarrierConnect.Test
{
	public class RateSelectorServiceTest : BaseRateSelectorTest
	{
		#region Exchange Rates

		public void TestRateSearch_WithExchangeRate_HasCorrectLocalAmount()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", container: "20GP", removeLines: true);
			costingEntry.TI_ContractNumber = "123";
			var frtLine = costingEntry.AddUnitRateLine("FRT", 20m, "CN", "USD"); // Use USD as rate currency

			var today = ZDateTime.Today;
			var usdCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
			var buyExchangeRate = usdCurrency.ExchangeRates.AddNew();
			buyExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			buyExchangeRate.RE_SellRate = 0.75m; // 1 AUD = 0.75 USD
			buyExchangeRate.RE_StartDate = today.AddMonths(-1);
			buyExchangeRate.RE_ExpiryDate = today.AddMonths(1);

			var sellExchangeRate = usdCurrency.ExchangeRates.AddNew();
			sellExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			sellExchangeRate.RE_SellRate = 0.85;
			sellExchangeRate.RE_StartDate = today.AddMonths(-1);
			sellExchangeRate.RE_ExpiryDate = today.AddMonths(1);
			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", weight: 1000, volume: 1m);

			var response = PerformRateSearch(query);

			AssertResponseEquals([
				new RateResultDto
				{
					ResultId = response.Rates[0].ResultId,
					Source = RateResultDto.RateSource.CargoWise,
					SourcePKs = [costingEntry.PK.ToGuid()],
					Origin = "AUMEL",
					Destination = "USLAX",
					TransportMode = "SEA",
					ContainerMode = "FCL",
					ContainerTypes = ["20GP"],
					UnmappedContainerTypes = [],
					NamedAccounts = [],
					CarrierContractNumber = "123",
					Charges =
					[
						new RateChargeDto
						{
							SourcePK = frtLine.PK.ToGuid(),
							ChargeCode = new ()
							{
								ChargeCode = "FRT",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "International Freight",
							},
							ChargeUnit = "CN",
							ChargeType = string.Empty,
							CalculationDescription = "1 20GP Container(s) @ USD 20.00/Container",
							LocalAmount = 26.67m, // 20 USD / 0.75 USD/AUD
							LocalCurrency = "AUD",
							RateAmount = 20m,
							RateCurrency = "USD",
							ContainerType = "20GP",
							Commodity = new ()
							{
								RefCommodityCode = "GEN",
								Description = "General",
							},
							Route = ["AUMEL", "USLAX"]
						}
					],
					Carrier = null,
					Route = ["AUMEL", "USLAX"],
					StartDate = costingEntry.TI_RateStartDate.ToDateTime(),
					EndDate = costingEntry.TI_RateEndDate.ToDateTime(),
					ServiceProvider = new CarrierDto(TransportProvider1)
				}
			], response);
		}

		public void TestRateSearch_WithMissingExchangeRate_HasZeroLocalAmount()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", container: "20GP", removeLines: true);
			costingEntry.TI_ContractNumber = "123";
			var frtLine = costingEntry.AddUnitRateLine("FRT", 20m, "CN", "USD"); // Use USD as rate currency
			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", weight: 1000, volume: 1m);

			var response = PerformRateSearch(query);

			AssertResponseEquals([
				new RateResultDto
				{
					ResultId = response.Rates[0].ResultId,
					Source = RateResultDto.RateSource.CargoWise,
					SourcePKs = [costingEntry.PK.ToGuid()],
					Origin = "AUMEL",
					Destination = "USLAX",
					TransportMode = "SEA",
					ContainerMode = "FCL",
					ContainerTypes = ["20GP"],
					NamedAccounts = [],
					CarrierContractNumber = "123",
					Charges =
					[
						new RateChargeDto
						{
							SourcePK = frtLine.PK.ToGuid(),
							ChargeCode = new ()
							{
								ChargeCode = "FRT",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "International Freight",
							},
							ChargeUnit = "CN",
							ChargeType = string.Empty,
							CalculationDescription = "1 20GP Container(s) @ USD 20.00/Container",
							LocalAmount = 0m, // Don't have related exchange rate in DB, should return 0
							LocalCurrency = "AUD",
							RateAmount = 20m,
							RateCurrency = "USD",
							ContainerType = "20GP",
							Commodity = new ()
							{
								RefCommodityCode = "GEN",
								Description = "General",
							},
							Route = ["AUMEL", "USLAX"]
						}
					],
					Carrier = null,
					Route = ["AUMEL", "USLAX"],
					StartDate = costingEntry.TI_RateStartDate.ToDateTime(),
					EndDate = costingEntry.TI_RateEndDate.ToDateTime(),
					ServiceProvider = new CarrierDto(TransportProvider1)
				}
			], response);
		}

		#endregion

		#region Rate Search

		public void TestRateSearch_Containerized_WithSingleRate_HasPropertiesSet()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", container: "20GP", removeLines: true);
			costingEntry.TI_ContractNumber = "123";
			costingEntry.TI_OH_TransportProvider = TransportProvider2.PK;
			var frtLine = costingEntry.AddUnitRateLine("FRT", 20m, "CN", "AUD");
			var fscLine = costingEntry.AddUnitRateLine("FSC", 1m, "KG", "AUD");
			var bafLine = costingEntry.AddFlatRateLine("BAF", 500m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", weight: 1000, volume: 1m)
				.AddContainer("20GP", weight: 1000, volume: 1m);

			var response = PerformRateSearch(query);

			AssertResponseEquals([
				new RateResultDto
				{
					ResultId = response.Rates[0].ResultId,
					Source = RateResultDto.RateSource.CargoWise,
					SourcePKs = [costingEntry.PK.ToGuid()],
					Origin = "AUMEL",
					Destination = "USLAX",
					TransportMode = "SEA",
					ContainerMode = "FCL",
					ContainerTypes = ["20GP"],
					CarrierContractNumber = "123",
					NamedAccounts = [],
					Charges =
					[
						new RateChargeDto
						{
							SourcePK = frtLine.PK.ToGuid(),
							ChargeCode = new()
							{
								ChargeCode = "FRT",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "International Freight",
							},
							ChargeUnit = "CN",
							ChargeType = string.Empty,
							CalculationDescription = "2 20GP Container(s) @ AUD 20.00/Container",
							LocalAmount = 40m,
							LocalCurrency = "AUD",
							RateAmount = 40m,
							RateCurrency = "AUD",
							ContainerType = "20GP",
							Commodity = new()
							{
								RefCommodityCode = "GEN",
								Description = "General",
							},
							Route = ["AUMEL", "USLAX"]
						},
						new RateChargeDto
						{
							SourcePK = fscLine.PK.ToGuid(),
							ChargeCode = new()
							{
								ChargeCode = "FSC",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "Fuel Surcharge",
							},
							ChargeUnit = "KG",
							ChargeType = string.Empty,
							CalculationDescription = "2000 Kilogram(s) @ AUD 1.00/KG",
							LocalAmount = 2000m,
							LocalCurrency = "AUD",
							RateAmount = 2000m,
							RateCurrency = "AUD",
							ContainerType = "20GP",
							Commodity = new()
							{
								RefCommodityCode = "GEN",
								Description = "General",
							},
							Route = ["AUMEL", "USLAX"]
						},
						new RateChargeDto
						{
							SourcePK = bafLine.PK.ToGuid(),
							ChargeCode = new()
							{
								ChargeCode = "BAF",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "Bunker Adjustment Factor",
							},
							ChargeUnit = string.Empty,
							ChargeType = string.Empty,
							CalculationDescription = "Base Rate AUD 500.00",
							LocalAmount = 500m,
							LocalCurrency = "AUD",
							RateAmount = 500m,
							RateCurrency = "AUD",
							ContainerType = "20GP",
							Commodity = new()
							{
								RefCommodityCode = "GEN",
								Description = "General",
							},
							Route = ["AUMEL", "USLAX"]
						}
					],
					Carrier = new OrganisationDto(TransportProvider2),
					Route = ["AUMEL", "USLAX"],
					StartDate = costingEntry.TI_RateStartDate.ToDateTime(),
					EndDate = costingEntry.TI_RateEndDate.ToDateTime(),
					ServiceProvider = new CarrierDto(TransportProvider1)
				}
			], response);
		}

		public void TestRateSearch_NonContainerized_WithSingleAirRate_HasPropertiesSet()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUMEL", "USLAX", ZString.Empty, ZString.Empty, removeLines: true);
			costingEntry.TI_ContractNumber = "123";
			var frtLine = costingEntry.AddUnitRateLine("FRT", 2m, "M3", "AUD");
			var fscLine = costingEntry.AddUnitRateLine("FSC", 3m, "KG", "AUD");
			var bafLine = costingEntry.AddFlatRateLine("BAF", 500m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("AIR", "LSE", "AUMEL", "USLAX")
				.AddNonContainerizedCargo("GEN", 10m, 1m);

			var response = PerformRateSearch(query);

			AssertResponseEquals([
				new RateResultDto
				{
					ResultId = response.Rates[0].ResultId,
					Source = RateResultDto.RateSource.CargoWise,
					SourcePKs = [costingEntry.PK.ToGuid()],
					Origin = "AUMEL",
					Destination = "USLAX",
					TransportMode = "AIR",
					ContainerMode = "LSE",
					CarrierContractNumber = "123",
					ContainerTypes = [],
					NamedAccounts = [],
					Charges =
					[
						new RateChargeDto
						{
							SourcePK = frtLine.PK.ToGuid(),
							ChargeCode = new()
							{
								ChargeCode = "FRT",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "International Freight",
							},
							ChargeUnit = "M3",
							ChargeType = string.Empty,
							CalculationDescription = "1 Cubic Meter(s) @ AUD 2.00/M3",
							LocalAmount = 2m,
							LocalCurrency = "AUD",
							RateAmount = 2m,
							RateCurrency = "AUD",
							ContainerType = null,
							Commodity = new()
							{
								RefCommodityCode = "GEN",
								Description = "General",
							},
							Route = ["AUMEL", "USLAX"]
						},
						new RateChargeDto
						{
							SourcePK = fscLine.PK.ToGuid(),
							ChargeCode = new()
							{
								ChargeCode = "FSC",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "Fuel Surcharge",
							},
							ChargeUnit = "KG",
							ChargeType = string.Empty,
							CalculationDescription = "166.667 Kilogram(s) @ AUD 3.00/KG",
							LocalAmount = 500m,
							LocalCurrency = "AUD",
							RateAmount = 500m,
							RateCurrency = "AUD",
							ContainerType = null,
							Commodity = new()
							{
								RefCommodityCode = "GEN",
								Description = "General",
							},
							Route = ["AUMEL", "USLAX"]
						},
						new RateChargeDto
						{
							SourcePK = bafLine.PK.ToGuid(),
							ChargeCode = new()
							{
								ChargeCode = "BAF",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "Bunker Adjustment Factor",
							},
							ChargeUnit = string.Empty,
							ChargeType = string.Empty,
							CalculationDescription = "Base Rate AUD 500.00",
							LocalAmount = 500m,
							LocalCurrency = "AUD",
							RateAmount = 500m,
							RateCurrency = "AUD",
							ContainerType = null,
							Commodity = new()
							{
								RefCommodityCode = "GEN",
								Description = "General",
							},
							Route = ["AUMEL", "USLAX"]
						}
					],
					Carrier = null,
					Route = ["AUMEL", "USLAX"],
					StartDate = costingEntry.TI_RateStartDate.ToDateTime(),
					EndDate = costingEntry.TI_RateEndDate.ToDateTime(),
					ServiceProvider = new CarrierDto(TransportProvider1)
				}
			], response);
		}

		public void TestRateSearch_NonContainerized_WithSingleSeaRate_HasPropertiesSet()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "AUMEL", "USLAX", ZString.Empty, ZString.Empty, removeLines: true);
			costingEntry.TI_ContractNumber = "123";
			var frtLine = costingEntry.AddUnitRateLine("FRT", 20m, "M3", "AUD");
			var fscLine = costingEntry.AddUnitRateLine("FSC", 1m, "KG", "AUD");
			var bafLine = costingEntry.AddFlatRateLine("BAF", 500m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "LCL", "AUMEL", "USLAX")
				.AddNonContainerizedCargo("GEN", 1000m, 1m);

			var response = PerformRateSearch(query);

			AssertResponseEquals([
				new RateResultDto
				{
					ResultId = response.Rates[0].ResultId,
					Source = RateResultDto.RateSource.CargoWise,
					SourcePKs = [costingEntry.PK.ToGuid()],
					Origin = "AUMEL",
					Destination = "USLAX",
					TransportMode = "SEA",
					ContainerMode = "LCL",
					CarrierContractNumber = "123",
					ContainerTypes = [],
					NamedAccounts = [],
					Charges =
					[
						new RateChargeDto
						{
							SourcePK = frtLine.PK.ToGuid(),
							ChargeCode = new()
							{
								ChargeCode = "FRT",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "International Freight",
							},
							ChargeUnit = "M3",
							ChargeType = string.Empty,
							CalculationDescription = "1 Cubic Meter(s) @ AUD 20.00/M3",
							LocalAmount = 20m,
							LocalCurrency = "AUD",
							RateAmount = 20m,
							RateCurrency = "AUD",
							ContainerType = null,
							Commodity = new()
							{
								RefCommodityCode = "GEN",
								Description = "General",
							},
							Route = ["AUMEL", "USLAX"]
						},
						new RateChargeDto
						{
							SourcePK = fscLine.PK.ToGuid(),
							ChargeCode = new()
							{
								ChargeCode = "FSC",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "Fuel Surcharge",
							},
							ChargeUnit = "KG",
							ChargeType = string.Empty,
							CalculationDescription = "1000 Kilogram(s) @ AUD 1.00/KG",
							LocalAmount = 1000m,
							LocalCurrency = "AUD",
							RateAmount = 1000m,
							RateCurrency = "AUD",
							ContainerType = null,
							Commodity = new()
							{
								RefCommodityCode = "GEN",
								Description = "General",
							},
							Route = ["AUMEL", "USLAX"]
						},
						new RateChargeDto
						{
							SourcePK = bafLine.PK.ToGuid(),
							ChargeCode = new()
							{
								ChargeCode = "BAF",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "Bunker Adjustment Factor",
							},
							ChargeUnit = string.Empty,
							ChargeType = string.Empty,
							CalculationDescription = "Base Rate AUD 500.00",
							LocalAmount = 500m,
							LocalCurrency = "AUD",
							RateAmount = 500m,
							RateCurrency = "AUD",
							ContainerType = null,
							Commodity = new()
							{
								RefCommodityCode = "GEN",
								Description = "General",
							},
							Route = ["AUMEL", "USLAX"]
						}
					],
					Carrier = null,
					Route = ["AUMEL", "USLAX"],
					StartDate = costingEntry.TI_RateStartDate.ToDateTime(),
					EndDate = costingEntry.TI_RateEndDate.ToDateTime(),
					ServiceProvider = new CarrierDto(TransportProvider1)
				}
			], response);
		}

		public void Test_RateSearch_ExcludesEntries_WithIncompatibleCommodity()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", container: "20GP", removeLines: true, commodity: "HAZ");
			costingEntry.AddFlatRateLine("FRT", 20m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 10, 1m);

			var response = PerformRateSearch(query);

			AssertEquals("There are no Rate Entries with a matching commodity code", 0, response.Rates.Length);
		}

		#endregion

		#region Rate Curation

		public void TestRateCuration_Containerized_WithMixedContainerType_GroupRatesTogether()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var tp1CostEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			var tp1CostEntry1Frt = tp1CostEntry1.AddUnitRateLine("FRT", 2m, "M3", "AUD");
			var tp1CostEntry1Baf = tp1CostEntry1.AddUnitRateLine("BAF", 14m, "CN", "AUD");

			var tp1CostEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "40GP", removeLines: true);
			var tp1CostEntry2Frt = tp1CostEntry2.AddUnitRateLine("FRT", 10m, "M3", "AUD");
			var tp1CostEntry2Baf = tp1CostEntry2.AddUnitRateLine("BAF", 24m, "CN", "AUD");

			var tp1CostEntryOrg = tp1Costing.AddRateEntry(RatingConstants.RateCategory.ORG, "SEA", "AUMEL", "USLAX", removeLines: true);
			tp1CostEntryOrg.TI_RH_NKCommodityCode = string.Empty;
			var tp1CostEntryOrgLine = tp1CostEntryOrg.AddFlatRateLine("OCART", 500m);

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "ABCD";
			TransportProvider2.OH_RSL_ShippingLine = shippingLine.PK;

			var tp2Costing = Helper.NewCosting(TransportProvider2);

			var tp2CostEntry1 = tp2Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			var tp2CostEntry1Frt = tp2CostEntry1.AddUnitRateLine("FRT", 2m, "M3", "AUD");
			var tp2CostEntry1Baf = tp2CostEntry1.AddUnitRateLine("BAF", 15m, "CN", "AUD");

			var tp2CostEntry2 = tp2Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "40GP", removeLines: true);
			var tp2CostEntry2Frt = tp2CostEntry2.AddUnitRateLine("FRT", 10m, "M3", "AUD");
			var tp2CostEntry2Baf = tp2CostEntry2.AddUnitRateLine("BAF", 18m, "CN", "AUD");

			var tp2CostEntryOrg = tp2Costing.AddRateEntry(RatingConstants.RateCategory.ORG, "SEA", "AUMEL", "USLAX", removeLines: true);
			var tp2CostEntryOcartLine = tp2CostEntryOrg.AddFlatRateLine("OCART", 500m);
			var tp2CostEntryOcaaLine = tp2CostEntryOrg.AddUnitRateLine("OCAA", 10m, "CN");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 3, 100m, 10m)
				.AddContainer("40GP", "GEN", 2, 100m, 10m);

			var response = PerformRateSearch(query);

			AssertResponseContainsRates(new List<RateResultDtoAssertion>
			{
				new ()
				{
					TransportProvider = TransportProvider1,
					Containers = ["20GP", "40GP"],
					RateEntries = [tp1CostEntry1, tp1CostEntry2, tp1CostEntryOrg],
					RateLines =
					[
						tp1CostEntry1Frt, tp1CostEntry1Baf, tp1CostEntry2Frt, tp1CostEntry2Baf, tp1CostEntryOrgLine
					],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
						.AddCharge("BAF", 42m, "20GP", "GEN")
						.AddCharge("FRT", 100m, "40GP", "GEN")
						.AddCharge("BAF", 48m, "40GP", "GEN")
						.AddCharge("OCART", 500m, null, null)
				},
				new ()
				{
					TransportProvider = TransportProvider2,
					Containers = ["20GP", "40GP"],
					RateEntries = [tp2CostEntry1, tp2CostEntry2, tp2CostEntryOrg],
					RateLines =
					[
						tp2CostEntry1Frt, tp2CostEntry1Baf, tp2CostEntry2Frt, tp2CostEntry2Baf,
						tp2CostEntryOcartLine, tp2CostEntryOcaaLine
					],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
						.AddCharge("BAF", 45m, "20GP", "GEN")
						.AddCharge("FRT", 100m, "40GP", "GEN")
						.AddCharge("BAF", 36m, "40GP", "GEN")
						.AddCharge("OCART", 500m, null, "GEN")
						.AddCharge("OCAA", 50m, null, "GEN")
				},
			}, response);
		}

		public void TestRateCuration_GroupRatesByContractNumber()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var costEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			costEntry1.TI_ContractNumber = "Test123";
			var frtLine1 = costEntry1.AddFlatRateLine("FRT", 20m, "AUD");

			var costEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			costEntry2.TI_ContractNumber = "Test456";
			var frtLine2 = costEntry2.AddFlatRateLine("FRT", 40m, "AUD");

			var costEntry3 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			var bafLine = costEntry3.AddFlatRateLine("BAF", 500m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 3, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				// Card 1 - Merged (Test123)
				new ()
				{
					RateEntries = [costEntry1, costEntry3],
					RateLines = [frtLine1, bafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
						.AddCharge("BAF", 500m, "20GP", "GEN")
				},
				// Card 1 - Merged (Test456)
				new ()
				{
					RateEntries = [costEntry2, costEntry3],
					RateLines = [frtLine2, bafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 40m, "20GP", "GEN")
						.AddCharge("BAF", 500m, "20GP", "GEN")
				},
				// Card 3 - Blank (No Contract)
				new ()
				{
					RateEntries = [costEntry3],
					RateLines = [bafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("BAF", 500m, "20GP", "GEN")
				}
			});
		}

		public void TestRateCuration_GroupRatesByContainerType_OnlyShowSingleResultWhenUsingFallback()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var costEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			var frtLine = costEntry1.AddFlatRateLine("FRT", 20m, "AUD");

			var costEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, removeLines: true);
			var bafLine = costEntry2.AddFlatRateLine("BAF", 500m, "AUD");

			var costEntry3 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "40GP", removeLines: true);
			costEntry3.AddFlatRateLine("WAR", 500m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 3, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [costEntry1, costEntry2],
					RateLines = [frtLine, bafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
						.AddCharge("BAF", 500m, null, "GEN")
				},
			});
		}

		public void TestRateCuration_GroupRatesByContainerType_OnlyShowSingleResultWithBlankRate_WithoutUsingFallback()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var costEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA,
				"AUMEL", "USLAX", ZString.Empty, removeLines: true);
			var frtLine = costEntry1.AddFlatRateLine("FRT", 20m, "AUD");
			var bafLine = costEntry1.AddFlatRateLine("BAF", 500m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 3, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				new()
				{
					RateEntries = [costEntry1],
					RateLines = [frtLine, bafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, null, "GEN")
						.AddCharge("BAF", 500m, null, "GEN")
				},
			});
		}

		public void TestRateCuration_GroupRatesByContainerType_AllowMatchingOnContainerClass()
		{
			Helper.Containers["20GP"].RC_FreightRateClass = "20GP";
			Helper.Containers["20FR"].RC_FreightRateClass = "20GP";

			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var costEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			costEntry1.TI_MatchContainerRateClass = true;
			var frtLine = costEntry1.AddFlatRateLine("FRT", 20m, "AUD");

			var costEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, removeLines: true);
			var bafLine = costEntry2.AddFlatRateLine("BAF", 500m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20FR", "GEN", 3, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				new()
				{
					RateEntries = [costEntry1, costEntry2],
					RateLines = [frtLine, bafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
						.AddCharge("BAF", 500m, null, "GEN")
				},
			});
		}

		public void TestRateCuration_GroupRatesByContainerType_ShouldMatchContainerClassCorrectly()
		{
			Helper.Containers["20GP"].RC_FreightRateClass = "20GP";
			Helper.Containers["20FR"].RC_FreightRateClass = "20GP";

			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var costEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			costEntry1.TI_MatchContainerRateClass = true;

			var frtLine = costEntry1.AddFlatRateLine("FRT", 20m, "AUD");
			var bafLine = costEntry1.AddFlatRateLine("BAF", 500m, "AUD");

			var costEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20FR", removeLines: true);
			var frtLine2 = costEntry2.AddFlatRateLine("FRT", 30m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20FR", "GEN", 3, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				new()
				{
					RateEntries = [costEntry1, costEntry2],
					RateLines = [frtLine2, bafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 30m, "20FR", "GEN")
						.AddCharge("BAF", 500m, "20GP", "GEN")
				},
			});
		}

		public void TestRateCuration_ForRateSearch_ShouldBeOneContainerQualityPerResult()
		{
			var (entry, line) = UrsHelper.CreateUrsEntryAndLine(id: "1", "FCL", "FRT", commodityCode: "GEN", containerCode: "20GP");
			var (entry2, line2) = UrsHelper.CreateUrsEntryAndLine(id: "2", "FCL", "BAF", commodityCode: "GEN", containerCode: "40GP");
			var (entry3, line3) = UrsHelper.CreateUrsEntryAndLine(id: "3", "FCL", "DST", commodityCode: "GEN", containerCode: "20FR",
				updateEntry: (e) => e.CustomFields = [new CustomField { Code = Rate.CustomFields.CargoSphere.ContainerQuality, Value = "NOR" }]);
			List<IRateEntry> entries = [entry, entry2, entry3];

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80)
				.AddContainer("40GP", "GEN", 1, 1000, 1000, 80)
				.AddContainer("20FR", "GEN", 1, 1000, 1000, 80);

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			ursServiceProvider.ChildRateEntries = entries;

			var mockProvider = new Mock<IRateSelectorProvider>();
			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(entries));

			PerformSearchAndAssert(query, [
				// DST entry should not be combined
				new ()
				{
					RateEntries = [entry, entry2],
					RateLines = [line, line2],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 5m, "20GP", "GEN")
						.AddCharge("BAF", 5m, "40GP", "GEN")
				},
				new ()
				{
					RateEntries = [entry3],
					RateLines = [line3],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("DST", 5m, "20FR", "GEN")
				},
			], mockProviderFactory.Object);
		}

		public void TestRateCuration_GroupByFieldWithFallback_ContractNumber()
			=> Assert_CurationPropertyGroupsCorrectly("TI_ContractNumber", "Test123");

		public void TestRateCuration_GroupByFieldWithFallback_CarrierServiceLevel()
			=> Assert_CurationPropertyGroupsCorrectly("TI_PL_NKCarrierServiceLevel", "EXP");

		public void TestRateCuration_GroupByFieldWithFallback_ServiceLevel()
			=> Assert_CurationPropertyGroupsCorrectly("TI_RS_NKServiceLevel_NI", "STD");

		public void TestRateCuration_GroupByFieldWithFallback_UniversalServiceLevel()
			=> Assert_CurationPropertyGroupsCorrectly((entry) => entry.UniversalServiceLevel = "EXP");

		public void TestRateCuration_GroupByFieldWithFallback_Consignee()
			=> Assert_CurationPropertyGroupsCorrectly("TI_OH_Consignee", Consignee.PK);

		public void TestRateCuration_GroupByFieldWithFallback_Consignor()
			=> Assert_CurationPropertyGroupsCorrectly("TI_OH_Consignor", Consignor.PK);

		public void TestRateCuration_GroupByFieldWithFallback_ControllingCustomer()
			=> Assert_CurationPropertyGroupsCorrectly("TI_OH_Consignor", Consignor.PK);

		public void TestRateCuration_GroupByFieldWithFallback_OriginPostCode()
			=> Assert_CurationPropertyGroupsCorrectly("TI_CartagePickupAddressPostCode", "3000");

		public void TestRateCuration_GroupByFieldWithFallback_DestinationPostCode()
			=> Assert_CurationPropertyGroupsCorrectly("TI_CartageDeliveryAddressPostCode", "3000");

		public void TestRateCuration_GroupByFieldWithFallback_TransitTime()
			=> Assert_CurationPropertyGroupsCorrectly("TI_TransitTime", "20");

		public void TestRateCuration_GroupByFieldWithFallback_Via()
			=> Assert_CurationPropertyGroupsCorrectly("TI_ViaLRC", "AUSYD");

		public void TestRateCuration_GroupByFieldWithFallback_AircraftType()
			=> Assert_CurationPropertyGroupsCorrectly("TI_AircraftType", "PAX");

		public void TestRateCuration_GroupByFieldWithFallback_PortTransportAddress()
			=> Assert_CurationPropertyGroupsCorrectly("TI_OA_CartagePickupAddressOverride", Consignor.MainAddress.PK);

		public void TestRateCuration_GroupByFieldWithFallback_Vessel()
			=> Assert_CurationPropertyGroupsCorrectly((entry) => entry.CustomFields = [new CustomField { Code = Rate.CustomFields.CargoSphere.Vessel, Value = "123" }]);

		public void TestRateCuration_GroupByFieldWithFallback_ServiceString()
			=> Assert_CurationPropertyGroupsCorrectly((entry) => entry.CustomFields = [new CustomField { Code = Rate.CustomFields.CargoSphere.ServiceString, Value = "123" }]);

		public void TestRateCuration_GroupByFieldWithFallback_TradeLane()
			=> Assert_CurationPropertyGroupsCorrectly((entry) => entry.CustomFields = [new CustomField { Code = Rate.CustomFields.CargoSphere.TradeLane, Value = "123" }]);

		public void TestRateCuration_GroupByFieldWithFallback_UniversalCommodityGroup()
			=> Assert_CurationPropertyGroupsCorrectly((entry) => entry.CommodityGroup = "HAZ");

		public void TestRateCuration_GroupByFieldWithFallback_ProductCode()
			=> Assert_CurationPropertyGroupsCorrectly((entry) => entry.CustomFields = [new CustomField { Code = Rate.CustomFields.Cargoguide.ProductCode, Value = "123" }]);

		public void TestRateCuration_GroupByFieldWithFallback_Deck()
			=> Assert_CurationPropertyGroupsCorrectly((entry) => entry.CustomFields = [new CustomField { Code = Rate.CustomFields.Cargoguide.DeckType, Value = "123" }]);

		public void TestRateCuration_GroupByFieldWithFallback_IsNonOperatedReefer_Y()
			=> Assert_CurationPropertyGroupsCorrectly("TI_IsNonOperatedReefer", "Y");

		public void TestRateCuration_GroupByFieldWithFallback_IsNonOperatedReefer_N()
			=> Assert_CurationPropertyGroupsCorrectly("TI_IsNonOperatedReefer", "N");

		public void TestRateCuration_GroupByFieldWithFallback_Carrier()
			=> Assert_CurationPropertyGroupsCorrectly("TI_OH_TransportProvider", TransportProvider2.PK);

		public void TestRateCuration_GroupByFieldWithFallback_FirstLoad()
			=> Assert_CurationPropertyGroupsCorrectly("TI_FirstLoadLRC", "AUSYD");

		public void TestRateCuration_GroupByFieldWithFallback_LastDischarge()
			=> Assert_CurationPropertyGroupsCorrectly("TI_LastDischargeLRC", "AUSYD");

		public void TestRateCuration_GroupByFieldWithFallback_FirstRouteSetLoad()
			=> Assert_CurationPropertyGroupsCorrectly("TI_FirstRouteSetLoadPortLRC", "AUSYD");

		public void TestRateCuration_GroupByFieldWithFallback_LastRouteSetDischarge()
			=> Assert_CurationPropertyGroupsCorrectly("TI_LastRouteSetDischargePortLRC", "AUSYD");

		void Assert_CurationPropertyGroupsCorrectly<T>(string fieldName, T fieldValue)
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var costEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			costEntry1[fieldName] = fieldValue;
			var frtLine = costEntry1.AddFlatRateLine("FRT", 20m, "AUD");

			var costEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			var bafLine = costEntry2.AddFlatRateLine("BAF", 500m, "AUD");
			costEntry2[fieldName] = null;

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 3, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				// Card 1 - Merged
				new ()
				{
					RateEntries = [costEntry1, costEntry2],
					RateLines = [frtLine, bafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
						.AddCharge("BAF", 500m, "20GP", "GEN")
				},
				// Card 2 - Blank
				new ()
				{
					RateEntries = [costEntry2],
					RateLines = [bafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("BAF", 500m, "20GP", "GEN")
				}
			});
		}

		void Assert_CurationPropertyGroupsCorrectly(Action<WiseEntry> updateEntry)
		{
			var ursEntry1 = UrsHelper.CreateUrsEntryAndLine(id: "1", "FCL", "FRT", commodityCode: "GEN", containerCode: "20GP");
			var ursEntry2 = UrsHelper.CreateUrsEntryAndLine(id: "2", "FCL", "DST", commodityCode: "GEN", containerCode: "40GP", updateEntry: updateEntry);
			var entries = new List<IRateEntry> { ursEntry1.entry, ursEntry2.entry };

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80)
				.AddContainer("40GP", "GEN", 1, 1000, 1000, 80);

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			ursServiceProvider.ChildRateEntries = entries;

			var mockProvider = new Mock<IRateSelectorProvider>();
			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(entries));

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				// Card 1 - Merged
				new ()
				{
					RateEntries = [ursEntry1.entry, ursEntry2.entry],
					RateLines = [ursEntry1.line, ursEntry2.line],
				},
				// Card 2 - Blank
				new ()
				{
					RateEntries = [ursEntry1.entry],
					RateLines = [ursEntry1.line],
				}
			}, mockProviderFactory.Object);
		}

		public void TestRateCuration_GroupByFieldWithFallback_TransitTime_DontDuplicateFrtCharges()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var costEntryBlank = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			var blankFrt = costEntryBlank.AddFlatRateLine("FRT", 50m, "AUD");
			var blankBaf = costEntryBlank.AddFlatRateLine("BAF", 20m, "AUD");
			var blankCaf = costEntryBlank.AddFlatRateLine("CAF", 30m, "AUD");

			var costEntry22Days = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			costEntry22Days.TI_TransitTime = "22";
			var frt22 = costEntry22Days.AddFlatRateLine("FRT", 2m, "AUD");
			var caf22 = costEntry22Days.AddFlatRateLine("CAF", 2.1m, "AUD");

			var costEntry35Days = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			costEntry35Days.TI_TransitTime = "35";
			var frt35 = costEntry35Days.AddFlatRateLine("FRT", 1m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 3, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				// Card 1 - Blank
				new ()
				{
					RateEntries = [costEntryBlank],
					RateLines = [blankFrt, blankBaf, blankCaf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 50m, "20GP", "GEN")
						.AddCharge("BAF", 20m, "20GP", "GEN")
						.AddCharge("CAF", 30m, "20GP", "GEN")
				},
				// Card 2 - 35 days
				new ()
				{
					RateEntries = [costEntryBlank, costEntry35Days],
					RateLines = [frt35, blankBaf, blankCaf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 1m, "20GP", "GEN")
						.AddCharge("BAF", 20m, "20GP", "GEN")
						.AddCharge("CAF", 30m, "20GP", "GEN")
				},
				// Card 3 - 22 days
				new ()
				{
					RateEntries = [costEntryBlank, costEntry22Days],
					RateLines = [frt22, caf22, blankBaf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 2m, "20GP", "GEN")
						.AddCharge("BAF", 20m, "20GP", "GEN")
						.AddCharge("CAF", 2.1m, "20GP", "GEN")
				}
			});
		}

		[DeveloperOnlyTest]
		// TODO: Frequency currently isn't used when comparing charges, so all charges are returned (duplicated).
		// TODO: To be fixed in a future WI.
		public void TestRateCuration_GroupByFieldWithFallback_Frequency_DontDuplicateCharges()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var costEntryBlank = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			var blankFrt = costEntryBlank.AddFlatRateLine("FRT", 50m, "AUD");
			var blankBaf = costEntryBlank.AddFlatRateLine("BAF", 20m, "AUD");
			var blankCaf = costEntryBlank.AddFlatRateLine("CAF", 30m, "AUD");

			var costEntryWeek = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			costEntryWeek.TI_Frequency = 2;
			costEntryWeek.TI_FrequencyUnit = "Week";
			var frt22 = costEntryWeek.AddFlatRateLine("FRT", 2m, "AUD");
			var caf22 = costEntryWeek.AddFlatRateLine("CAF", 2.1m, "AUD");

			var costEntryFortnight = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			costEntryFortnight.TI_Frequency = 2;
			costEntryFortnight.TI_FrequencyUnit = "Fortnight";
			var frt35 = costEntryFortnight.AddFlatRateLine("FRT", 1m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 3, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				// Card 1 - Blank
				new ()
				{
					RateEntries = [costEntryBlank],
					RateLines = [blankFrt, blankBaf, blankCaf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 50m, "20GP", "GEN")
						.AddCharge("BAF", 20m, "20GP", "GEN")
						.AddCharge("CAF", 30m, "20GP", "GEN")
				},
				// Card 2 - 35 days
				new ()
				{
					RateEntries = [costEntryBlank, costEntryFortnight],
					RateLines = [frt35, blankBaf, blankCaf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 1m, "20GP", "GEN")
						.AddCharge("BAF", 20m, "20GP", "GEN")
						.AddCharge("CAF", 30m, "20GP", "GEN")
				},
				// Card 3 - 22 days
				new ()
				{
					RateEntries = [costEntryBlank, costEntryFortnight],
					RateLines = [frt22, caf22, blankBaf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 2m, "20GP", "GEN")
						.AddCharge("BAF", 20m, "20GP", "GEN")
						.AddCharge("CAF", 2.1m, "20GP", "GEN")
				}
			});
		}

		public void TestRateCuration_FilterIrrelevantRates()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var costEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, removeLines: true);
			var frtLine = costEntry1.AddFlatRateLine("FRT", 20m, "AUD");
			var bafLine = costEntry1.AddFlatRateLine("BAF", 500m, "AUD");

			// Incorrect Origin
			tp1Costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "USLAX", "WAR", 100m);
			// Incorrect Destination
			tp1Costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "HKHKG", "WAR", 100m);
			// Incorrect Container Type
			tp1Costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", "WAR", 100m, container: "40GP");
			// Incorrect Commodity
			tp1Costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", "WAR", 100m, commodity: "HAZ");
			// Incorrect Container Mode
			tp1Costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL, "AUMEL", "USLAX", "WAR", 100m);
			// Incorrect Transport Mode
			tp1Costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUMEL", "USLAX", "WAR", 100m);

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 3, 100m, 10m);

			var logger = new MemoryLogger();
			var response = PerformRateSearch(query, logger);

			AssertResponseContainsRates(new List<RateResultDtoAssertion>
			{
				// We only expect relevant rates - all others should be filtered as they're irrelevant for this query.
				new ()
				{
					RateEntries = [costEntry1],
					RateLines = [frtLine, bafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, null, "GEN")
						.AddCharge("BAF", 500m, null, "GEN")
				},
			}, response);
		}

		public void TestRateCuration_GroupByMultipleFields()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var costEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, removeLines: true);
			var frtLine = costEntry1.AddFlatRateLine("FRT", 20m, "AUD");
			var bafLine = costEntry1.AddFlatRateLine("BAF", 500m, "AUD");

			var costEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", "HAZ", removeLines: true);
			var fscLine = costEntry2.AddFlatRateLine("FSC", 100m, "AUD");

			var costEntry3 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", "EXP", removeLines: true);
			var warLine = costEntry3.AddFlatRateLine("WAR", 150m, "AUD");

			var costEntry4 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, removeLines: true);
			costEntry4.TI_ContractNumber = "Test123";
			var cafLine = costEntry4.AddFlatRateLine("CAF", 200m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 3, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [costEntry1],
					RateLines = [frtLine, bafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, null, "GEN")
						.AddCharge("BAF", 500m, null, "GEN")
				},
				new ()
				{
					RateEntries = [costEntry1, costEntry2],
					RateLines = [frtLine, bafLine, fscLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, null, "GEN")
						.AddCharge("BAF", 500m, null, "GEN")
						.AddCharge("FSC", 100m, null, "GEN")
				},
				new ()
				{
					RateEntries = [costEntry1, costEntry3],
					RateLines = [frtLine, bafLine, warLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, null, "GEN")
						.AddCharge("BAF", 500m, null, "GEN")
						.AddCharge("WAR", 150m, null, "GEN")
				},
				new ()
				{
					RateEntries = [costEntry1, costEntry4],
					RateLines = [frtLine, bafLine, cafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, null, "GEN")
						.AddCharge("BAF", 500m, null, "GEN")
						.AddCharge("CAF", 200m, null, "GEN")
				},
				new ()
				{
					RateEntries = [costEntry1, costEntry2, costEntry4],
					RateLines = [frtLine, bafLine, fscLine, cafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, null, "GEN")
						.AddCharge("BAF", 500m, null, "GEN")
						.AddCharge("CAF", 200m, null, "GEN")
						.AddCharge("FSC", 100m, null, "GEN")
				},
				new ()
				{
					RateEntries = [costEntry1, costEntry3, costEntry4],
					RateLines = [frtLine, bafLine, warLine, cafLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, null, "GEN")
						.AddCharge("BAF", 500m, null, "GEN")
						.AddCharge("CAF", 200m, null, "GEN")
						.AddCharge("WAR", 150m, null, "GEN")
				},
			});
		}

		public void TestRateCuration_WithStandardCosts_OnlyIncludeWithOtherServiceProviders()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var tp1CostEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry1Frt = tp1CostEntry1.AddFlatRateLine("FRT", 2m, "AUD");
			var tp1CostEntry1Baf = tp1CostEntry1.AddFlatRateLine("BAF", 14m, "AUD");

			var standardCosting = Helper.NewCosting(null);
			var standardCostingEntry = standardCosting.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			var standardCostingEntryWar = standardCostingEntry.AddFlatRateLine("WAR", 500m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [tp1CostEntry1, standardCostingEntry],
					RateLines = [tp1CostEntry1Frt, tp1CostEntry1Baf, standardCostingEntryWar],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 2m, "20GP", "GEN")
						.AddCharge("BAF", 14m, "20GP", "GEN")
						.AddCharge("WAR", 500m, "20GP", "GEN")
				}
			});

			standardCostingEntry.TI_IsExcludedFromAutoRating = true;
			Factory.Save();

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [tp1CostEntry1],
					RateLines = [tp1CostEntry1Frt, tp1CostEntry1Baf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 2m, "20GP", "GEN")
						.AddCharge("BAF", 14m, "20GP", "GEN")
				}
			});
		}

		public void TestRateCuration_WithLocalStandardCosts_PreferChargesFromLocalCarrierCosting()
		{
			var tp1LocalCosting = Helper.NewCosting(TransportProvider1);

			var tp1CostEntry1 = tp1LocalCosting.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry1Frt = tp1CostEntry1.AddFlatRateLine("FRT", 2m, "AUD");
			var tp1CostEntry1Baf = tp1CostEntry1.AddFlatRateLine("BAF", 14m, "AUD");

			var localStandardCosting = Helper.NewCosting(null);
			var standardCostingEntry = localStandardCosting.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			var standardCostingEntryWar = standardCostingEntry.AddFlatRateLine("WAR", 500m, "AUD");
			var standardCostingFrt = standardCostingEntry.AddFlatRateLine("FRT", 50m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [tp1CostEntry1, standardCostingEntry],
					RateLines = [tp1CostEntry1Frt, tp1CostEntry1Baf, standardCostingEntryWar],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 2m, "20GP", "GEN")
						.AddCharge("BAF", 14m, "20GP", "GEN")
						.AddCharge("WAR", 500m, "20GP", "GEN")
				}
			});
		}

		public void TestRateCuration_WithGlobalCarrierCosting_PreferChargesFromLocalStandardCosts()
		{
			var globalFreightChargeCode = Helper.ChargeCodes.CreateGlobalCharge("FRT");
			var globalBAFChargeCode = Helper.ChargeCodes.CreateGlobalCharge("BAF");
			var tp1GlobalCosting = Helper.NewGlobalCosting(TransportProvider1);

			var tp1CostEntry1 = tp1GlobalCosting.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			tp1CostEntry1.AddRateLine(globalFreightChargeCode).GetCalculator<FlatCalculator>().BaseRate = 2m;
			var tp1CostEntry1Baf = tp1CostEntry1.AddFlatRateLine(globalBAFChargeCode.AC_Code, 14m, "AUD");

			var standardCosting = Helper.NewCosting(null);
			var standardCostingEntry = standardCosting.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			var standardCostingEntryWar = standardCostingEntry.AddFlatRateLine("WAR", 500m, "AUD");
			var standardCostingFrt = standardCostingEntry.AddFlatRateLine("FRT", 50m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [tp1CostEntry1, standardCostingEntry],
					RateLines = [tp1CostEntry1Baf, standardCostingEntryWar, standardCostingFrt],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 50m, "20GP", "GEN")
						.AddCharge("BAF", 14m, "20GP", "GEN")
						.AddCharge("WAR", 500m, "20GP", "GEN")
				}
			});
		}

		public void TestRateCuration_WithRatesOfDifferentLocationTypes_ShouldUseCorrectPriority()
		{
			Helper.NewInternationalZone("ASEA", null, "SGSIN", "MYKUL");

			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var tp1CostEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "SGSIN", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry1Frt = tp1CostEntry1.AddFlatRateLine("FRT", 1m, "AUD");

			var tp1CostEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUEC", "ASEA", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			tp1CostEntry2.AddFlatRateLine("FRT", 88m, "AUD");
			var tp1CostEntry2Baf = tp1CostEntry2.AddFlatRateLine("BAF", 9.6m, "AUD");

			var tp1CostEntry3 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AU", "SG", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			tp1CostEntry3.AddFlatRateLine("FRT", 888m, "AUD");
			tp1CostEntry3.AddFlatRateLine("BAF", 99.3m, "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUSYD", "SGSIN")
				.AddContainer("20GP", "GEN", 1, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>()
			{
				new()
				{
					RateEntries = [tp1CostEntry1, tp1CostEntry2],
					RateLines = [tp1CostEntry1Frt, tp1CostEntry2Baf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 1m, "20GP", "GEN")
						.AddCharge("BAF", 9.6m, "20GP", "GEN")
				}
			});
		}

		public void TestRateCuration_Containerized_RateWithEmptyContainer_UseMostSpecificRates()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var tp1CostEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry1Frt = tp1CostEntry1.AddUnitRateLine("FRT", 2m, "M3", "AUD");
			var tp1CostEntry1Baf = tp1CostEntry1.AddUnitRateLine("BAF", 14m, "CN", "AUD");

			var tp1CostEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "40GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry2Frt = tp1CostEntry2.AddUnitRateLine("FRT", 4m, "M3", "AUD");
			var tp1CostEntry2Baf = tp1CostEntry2.AddUnitRateLine("BAF", 24m, "CN", "AUD");

			var tp1CostEntry3 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry3Frt = tp1CostEntry3.AddUnitRateLine("FRT", 6m, "M3", "AUD");
			var tp1CostEntry3Baf = tp1CostEntry3.AddUnitRateLine("BAF", 20m, "CN", "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 100m, 10m)
				.AddContainer("40GP", "GEN", 1, 100m, 10m)
				.AddContainer("40HC", "GEN", 1, 100m, 10m)
				.AddContainer("45HC", "GEN", 1, 100m, 10m);

			var response = PerformRateSearch(query);

			AssertResponseContainsRates(new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [tp1CostEntry1, tp1CostEntry2, tp1CostEntry3],
					RateLines =
					[
						tp1CostEntry1Frt, tp1CostEntry1Baf, tp1CostEntry2Frt, tp1CostEntry2Baf, tp1CostEntry3Frt,
						tp1CostEntry3Baf
					],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
						.AddCharge("BAF", 14m, "20GP", "GEN")
						.AddCharge("FRT", 40m, "40GP", "GEN")
						.AddCharge("BAF", 24m, "40GP", "GEN")
						.AddCharge("FRT", 120m, null, "GEN") // 40HC + 45HC
						.AddCharge("BAF", 40m, null, "GEN") // 40HC + 45HC
				},
			}, response);
		}

		public void TestRateCuration_Containerized_RateWithEmptyContractNumber_UseMostSpecificRates()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var tp1CostEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry1Frt = tp1CostEntry1.AddUnitRateLine("FRT", 2m, "M3", "AUD");
			var tp1CostEntry1Fsc = tp1CostEntry1.AddFlatRateLine("FSC", 20m, "AUD");

			var tp1CostEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			tp1CostEntry2.TI_ContractNumber = "123";
			var tp1CostEntry2Frt = tp1CostEntry2.AddUnitRateLine("FRT", 4m, "M3", "AUD");
			var tp1CostEntry2Baf = tp1CostEntry2.AddUnitRateLine("BAF", 24m, "CN", "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 100m, 10m);

			var response = PerformRateSearch(query);

			AssertResponseContainsRates(new List<RateResultDtoAssertion>
			{
				// Blank
				new ()
				{
					TransportProvider = TransportProvider1,
					RateEntries = [tp1CostEntry1],
					RateLines = [tp1CostEntry1Frt, tp1CostEntry1Fsc],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
						.AddCharge("FSC", 20m, "20GP", "GEN")
				},
				// 123 + Blank
				new ()
				{
					TransportProvider = TransportProvider1,
					CarrierContractNumber = "123",
					RateEntries = [tp1CostEntry1, tp1CostEntry2],
					RateLines = [tp1CostEntry1Fsc, tp1CostEntry2Frt, tp1CostEntry2Baf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 40m, "20GP", "GEN")
						.AddCharge("BAF", 24m, "20GP", "GEN")
						.AddCharge("FSC", 20m, "20GP", "GEN")
				},
			}, response);
		}

		public void TestRateCuration_Containerized_RateWithEmptyContractNumber_MixedContainerTypes_UseMostSpecificRates()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var tp1CostEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry1Frt = tp1CostEntry1.AddUnitRateLine("FRT", 2m, "M3", "AUD");
			var tp1CostEntry1Fsc = tp1CostEntry1.AddFlatRateLine("FSC", 20m, "AUD");

			var tp1CostEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			tp1CostEntry2.TI_ContractNumber = "123";
			var tp1CostEntry2Frt = tp1CostEntry2.AddUnitRateLine("FRT", 4m, "M3", "AUD");
			var tp1CostEntry2Baf = tp1CostEntry2.AddUnitRateLine("BAF", 24m, "CN", "AUD");

			var tp1CostEntry3 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "40GP", commodity: ZString.Empty, removeLines: true);
			tp1CostEntry3.TI_ContractNumber = "123";
			var tp1CostEntry3Frt = tp1CostEntry3.AddUnitRateLine("FRT", 5m, "M3", "AUD");
			var tp1CostEntry3Baf = tp1CostEntry3.AddUnitRateLine("BAF", 30m, "CN", "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 100m, 10m)
				.AddContainer("40GP", "GEN", 1, 100m, 10m);

			var response = PerformRateSearch(query);

			AssertResponseContainsRates(new List<RateResultDtoAssertion>
			{
				// Contract Number: <blank> - will only be 20GP rate
				new ()
				{
					TransportProvider = TransportProvider1,
					Containers = ["20GP"],
					RateEntries = [tp1CostEntry1],
					RateLines = [tp1CostEntry1Frt, tp1CostEntry1Fsc],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
						.AddCharge("FSC", 20m, "20GP", "GEN")
				},
				// Contract Number: <blank> + 123 - merged rate - 20GP + 40GP
				new ()
				{
					TransportProvider = TransportProvider1,
					CarrierContractNumber = "123",
					Containers = ["20GP", "40GP"],
					RateEntries = [tp1CostEntry1, tp1CostEntry2, tp1CostEntry3],
					RateLines =
					[
						tp1CostEntry1Fsc, tp1CostEntry2Frt, tp1CostEntry2Baf, tp1CostEntry3Frt, tp1CostEntry3Baf
					],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 40m, "20GP", "GEN")
						.AddCharge("BAF", 24m, "20GP", "GEN")
						.AddCharge("FSC", 20m, "20GP", "GEN")
						.AddCharge("FRT", 50m, "40GP", "GEN")
						.AddCharge("BAF", 30m, "40GP", "GEN")
				},
			}, response);
		}

		public void TestRateCuration_Containerized_MixedContainerTypes_MergeCompatibleRates()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var tp1CostEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "SGSIN", "STD", "20GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry1Frt = tp1CostEntry1.AddFlatRateLine("FRT", 1m, "AUD");

			var tp1CostEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "SGSIN", ZString.Empty, "40GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry2Baf = tp1CostEntry2.AddFlatRateLine("BAF", 9.6m, "AUD");
			tp1CostEntry2.TI_OH_Consignor = Consignor.PK;

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUSYD", "SGSIN")
				.AddContainer("20GP", "GEN", 1, 100m, 10m)
				.AddContainer("40GP", "GEN", 1, 200m, 20m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>()
			{
				new()
				{
					RateEntries = [tp1CostEntry1, tp1CostEntry2],
					RateLines = [tp1CostEntry1Frt, tp1CostEntry2Baf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 1m, "20GP", "GEN")
						.AddCharge("BAF", 9.6m, "40GP", "GEN")
				},
				new()
				{
					RateEntries = [tp1CostEntry1],
					RateLines = [tp1CostEntry1Frt],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 1m, "20GP", "GEN")
				},
				new()
				{
					RateEntries = [tp1CostEntry2],
					RateLines = [tp1CostEntry2Baf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("BAF", 9.6m, "40GP", "GEN")
				}
			});
		}

		public void TestRateCuration_CreatesAllRateGroupsCombinations()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var tp1CostEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "SGSIN", "STD", "20GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry1Frt = tp1CostEntry1.AddFlatRateLine("FRT", 1m, "AUD");

			var tp1CostEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "SGSIN", "STD", "20GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry2Baf = tp1CostEntry2.AddFlatRateLine("BAF", 1m, "AUD");

			var tp1CostEntry3 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUSYD", "SGSIN", "STD", "20GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry3Ocart = tp1CostEntry3.AddFlatRateLine("OCART", 1m, "AUD");

			tp1CostEntry1.TI_OH_Consignor = Consignor.PK;
			tp1CostEntry2.TI_CartagePickupAddressPostCode = "123";
			tp1CostEntry3.TI_CartageDeliveryAddressPostCode = "234";

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUSYD", "SGSIN")
				.AddContainer("20GP", "GEN", 1, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>()
			{
				new()
				{
					RateEntries = [tp1CostEntry1, tp1CostEntry2, tp1CostEntry3],
					RateLines = [tp1CostEntry1Frt, tp1CostEntry2Baf, tp1CostEntry3Ocart],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 1m, "20GP", "GEN")
						.AddCharge("BAF", 1m, "20GP", "GEN")
						.AddCharge("OCART", 1m, "20GP", "GEN")
				},
				new()
				{
					RateEntries = [tp1CostEntry1, tp1CostEntry2],
					RateLines = [tp1CostEntry1Frt, tp1CostEntry2Baf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 1m, "20GP", "GEN")
						.AddCharge("BAF", 1m, "20GP", "GEN")
				},
				new()
				{
					RateEntries = [tp1CostEntry2, tp1CostEntry3],
					RateLines = [tp1CostEntry2Baf, tp1CostEntry3Ocart],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("BAF", 1m, "20GP", "GEN")
						.AddCharge("OCART", 1m, "20GP", "GEN")
				},
				new()
				{
					RateEntries = [tp1CostEntry1, tp1CostEntry3],
					RateLines = [tp1CostEntry1Frt, tp1CostEntry3Ocart],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 1m, "20GP", "GEN")
						.AddCharge("OCART", 1m, "20GP", "GEN")
				},
				new()
				{
					RateEntries = [tp1CostEntry3],
					RateLines = [tp1CostEntry3Ocart],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("OCART", 1m, "20GP", "GEN")
				},
				new()
				{
					RateEntries = [tp1CostEntry2],
					RateLines = [tp1CostEntry2Baf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("BAF", 1m, "20GP", "GEN")
				},
				new()
				{
					RateEntries = [tp1CostEntry1],
					RateLines = [tp1CostEntry1Frt],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 1m, "20GP", "GEN")
				},
			});
		}

		public void TestRateCuration_Containerized_RateWithEmptyServiceLevel_UseMostSpecificRates()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var tp1CostEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry1Frt = tp1CostEntry1.AddUnitRateLine("FRT", 2m, "M3", "AUD");
			var tp1CostEntry1Fsc = tp1CostEntry1.AddFlatRateLine("FSC", 20m, "AUD");

			var tp1CostEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", "STD", "20GP", commodity: ZString.Empty, removeLines: true);
			var tp1CostEntry2Frt = tp1CostEntry2.AddUnitRateLine("FRT", 4m, "M3", "AUD");
			var tp1CostEntry2Baf = tp1CostEntry2.AddUnitRateLine("BAF", 24m, "CN", "AUD");

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 100m, 10m);

			var response = PerformRateSearch(query);

			AssertResponseContainsRates(new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [tp1CostEntry1],
					RateLines = [tp1CostEntry1Frt, tp1CostEntry1Fsc],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
						.AddCharge("FSC", 20m, "20GP", "GEN")
				},
				new ()
				{
					CarrierServiceLevel = "STD",
					RateEntries = [tp1CostEntry1, tp1CostEntry2],
					RateLines = [tp1CostEntry1Fsc, tp1CostEntry2Frt, tp1CostEntry2Baf],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 40m, "20GP", "GEN")
						.AddCharge("BAF", 24m, "20GP", "GEN")
						.AddCharge("FSC", 20m, "20GP", "GEN")
				},
			}, response);
		}

		public void TestRateCuration_Containerized_WithMixedCommodities_GroupRatesTogether()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var tp1CostEntry1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: "GEN", removeLines: true);
			var tp1CostEntry1Frt = tp1CostEntry1.AddUnitRateLine("FRT", 2m, "M3", "AUD");
			var tp1CostEntry1Baf = tp1CostEntry1.AddUnitRateLine("BAF", 14m, "CN", "AUD");

			var tp1CostEntry2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: "HAZ", removeLines: true);
			var tp1CostEntry2Frt = tp1CostEntry2.AddUnitRateLine("FRT", 4m, "M3", "AUD");
			var tp1CostEntry2Baf = tp1CostEntry2.AddUnitRateLine("BAF", 24m, "CN", "AUD");

			var tp1CostEntryOrg1 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.ORG, "SEA", "AUMEL", "USLAX", removeLines: true);
			tp1CostEntryOrg1.TI_RH_NKCommodityCode = string.Empty;
			var tp1CostEntryOrgLine1 = tp1CostEntryOrg1.AddFlatRateLine("OCART", 500m);

			var tp1CostEntryOrg2 = tp1Costing.AddRateEntry(RatingConstants.RateCategory.ORG, "SEA", "AUMEL", "USLAX", commodity: "HAZ", removeLines: true);
			var tp1CostEntryOrgLine2 = tp1CostEntryOrg2.AddFlatRateLine("ODANG", 800m);

			Factory.Save();

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 3, 100m, 5m)
				.AddContainer("20GP", "HAZ", 2, 100m, 5m);

			var logger = new MemoryLogger();
			var response = PerformRateSearch(query, logger);

			AssertResponseContainsRates(new List<RateResultDtoAssertion>
			{
				// Transport Provider 1, 20GP GEN + 20GP HAZ
				new ()
				{
					RateEntries = [tp1CostEntry1, tp1CostEntry2, tp1CostEntryOrg1, tp1CostEntryOrg2],
					RateLines =
					[
						tp1CostEntry1Frt, tp1CostEntry1Baf, tp1CostEntry2Frt, tp1CostEntry2Baf,
						tp1CostEntryOrgLine1, tp1CostEntryOrgLine2
					],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 10m, "20GP", "GEN")
						.AddCharge("BAF", 42m, "20GP", "GEN")
						.AddCharge("FRT", 20m, "20GP", "HAZ")
						.AddCharge("BAF", 48m, "20GP", "HAZ")
						.AddCharge("OCART", 500m, null, null)
						.AddCharge("ODANG", 800m, null, "HAZ")
				},
			}, response);
		}

		public void TestRateCuration_Containerized_WithSeaRate_GroupRates()
			=> AssertRelevantRatesReturned("SEA", "FCL", RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "20GP");

		public void TestRateCuration_Containerized_WithAirRate_GroupRates()
			=> AssertRelevantRatesReturned("AIR", "ULD", RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "LD-3");

		public void TestRateCuration_NonContainerized_WithAirRate_GroupRates()
			=> AssertRelevantRatesReturned("SEA", "LCL", RatingConstants.RateCategory.LCL, Core.Constants.RateMode.LCL);

		public void TestRateCuration_NonContainerized_WithSeaRate_GroupRates()
			=> AssertRelevantRatesReturned("AIR", "LSE", RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE);

		void AssertRelevantRatesReturned(string transportMode, string containerMode, string rateCategory, string rateMode, ZString? containerType = null)
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var tp1CostEntry1 = tp1Costing.AddRateEntry(rateCategory, rateMode, "AUMEL", "USLAX", ZString.Empty, containerType ?? ZString.Empty, removeLines: true);
			tp1CostEntry1.TI_ContractNumber = "123";
			var tp1CostEntry1Frt = tp1CostEntry1.AddUnitRateLine("FRT", 2m, "KG", "AUD");
			var tp1CostEntry1Baf = tp1CostEntry1.AddFlatRateLine("BAF", 500m, "AUD");

			var tp1CostEntry2 = tp1Costing.AddRateEntry(rateCategory, rateMode, "AUMEL", "USLAX", ZString.Empty, containerType ?? ZString.Empty, removeLines: true);
			tp1CostEntry2.TI_ContractNumber = "456";
			var tp1CostEntry2Frt = tp1CostEntry2.AddUnitRateLine("FRT", 10m, "KG", "AUD");
			var tp1CostEntry2Baf = tp1CostEntry2.AddFlatRateLine("BAF", 500m, "AUD");

			var tp1CostEntry3 = tp1Costing.AddRateEntry(rateCategory, rateMode, "AUSYD", "USSFO", ZString.Empty, containerType ?? ZString.Empty, removeLines: true);
			tp1CostEntry3.TI_ContractNumber = "789";
			tp1CostEntry3.AddUnitRateLine("FRT", 20m, "KG", "AUD");

			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_StandardCarrierAlphaCode = "ABCD";
			TransportProvider2.OH_RSL_ShippingLine = shippingLine.PK;

			var tp2Costing = Helper.NewCosting(TransportProvider2);
			var tp2CostEntry1 = tp2Costing.AddRateEntry(rateCategory, rateMode, "AUMEL", "USLAX", ZString.Empty, containerType ?? ZString.Empty, removeLines: true);
			var tp2CostEntry1Fsc = tp2CostEntry1.AddUnitRateLine("FSC", 4m, "KG", "AUD");
			var tp2CostEntry1Baf = tp2CostEntry1.AddFlatRateLine("BAF", 600m, "AUD");

			var tp2CostEntry2 = tp2Costing.AddRateEntry(rateCategory, rateMode, "AU", "US", ZString.Empty, containerType ?? ZString.Empty, removeLines: true);
			var tp2CostEntry2Frt = tp2CostEntry2.AddUnitRateLine("FRT", 5m, "KG", "AUD");

			Factory.Save();

			var query = CreateRateQuery(transportMode, containerMode, "AUMEL", "USLAX")
				.AddContainer(containerType ?? "LSE", count: 1, weight: 100m, volume: 100m);

			var response = PerformRateSearch(query);

			AssertResponseContainsRates(new List<RateResultDtoAssertion>
			{
				// Transport Provider 1, Contract 123
				new ()
				{
					RateEntries = [tp1CostEntry1],
					RateLines = [tp1CostEntry1Frt, tp1CostEntry1Baf]
				},
				// Transport Provider 1, Contract 456
				new ()
				{
					RateEntries = [tp1CostEntry2],
					RateLines = [tp1CostEntry2Frt, tp1CostEntry2Baf]
				},
				// Transport Provider 2, Combined Rate
				new ()
				{
					RateEntries = [tp2CostEntry1, tp2CostEntry2],
					RateLines = [tp2CostEntry1Fsc, tp2CostEntry1Baf, tp2CostEntry2Frt]
				}
			}, response);
		}

		public void TestRateCuration_MergesURSRatesWithSameId()
		{
			var ts1UrsFcl = UrsHelper.CreateUrsEntryAndLine(id: "1", "FCL", "FRT", commodityCode: "GEN", contractNumber: "1234567890", containerCode: "20GP", (entry) =>
				entry.CustomFields = [new CustomField { Code = Rate.CustomFields.CargoSphere.RateType, Value = "123" }]
			);
			var ts1UrsOrg = UrsHelper.CreateUrsEntryAndLine(id: "1", "ORG", "FRT", commodityCode: "GEN", contractNumber: string.Empty, containerCode: "20GP", (entry) =>
				entry.CustomFields = [new CustomField { Code = Rate.CustomFields.CargoSphere.RateType, Value = "456" }]
			);
			var ts2UrsFcl = UrsHelper.CreateUrsEntryAndLine(id: "2", "FCL", "FRT", commodityCode: "GEN", contractNumber: string.Empty, containerCode: "20GP");
			var entries = new List<IRateEntry> { ts1UrsFcl.entry, ts1UrsOrg.entry, ts2UrsFcl.entry };

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			ursServiceProvider.ChildRateEntries = entries;

			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(entries));

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				// Trade service Id 1, Contract number: 1234567890
				new ()
				{
					RateEntries = [ts1UrsFcl.entry, ts1UrsOrg.entry],
					RateLines = [ts1UrsFcl.line, ts1UrsOrg.line],
					PerContainerCommodity = [new PerContainerCommodityDto { Container = "20GP", Commodity = "GEN", RateType = "123" }]
				},
				// Trade service Id 2, Contract number: Empty, ts1UrsFrt and ts2UrsFrt shouldn't merge
				new ()
				{
					RateEntries = [ts2UrsFcl.entry],
					RateLines = [ts2UrsFcl.line],
					PerContainerCommodity = [new PerContainerCommodityDto { Container = "20GP", Commodity = "GEN" }]
				},
			}, mockProviderFactory.Object);
		}

		public void TestRateCuration_MergesMultipleURSRatesWithSameId()
		{
			var ts1UrsFcl = UrsHelper.CreateUrsEntryAndLine(id: "1", "FCL", "FRT", commodityCode: "GEN", contractNumber: "1234567890", containerCode: "20GP", (entry) =>
				entry.CustomFields = [new CustomField { Code = Rate.CustomFields.CargoSphere.RateType, Value = "123" }]
			);
			var ts1UrsOrg = UrsHelper.CreateUrsEntryAndLine(id: "1", "ORG", "FRT", commodityCode: "GEN", contractNumber: string.Empty, containerCode: "20GP", (entry) =>
				entry.CustomFields = [new CustomField { Code = Rate.CustomFields.CargoSphere.RateType, Value = "456" }]
			);
			var ts2UrsFcl = UrsHelper.CreateUrsEntryAndLine(id: "2", "FCL", "FRT", commodityCode: "GEN", contractNumber: string.Empty, containerCode: "40GP");
			var ts2UrsOrg = UrsHelper.CreateUrsEntryAndLine(id: "2", "ORG", "FRT", commodityCode: "GEN", contractNumber: string.Empty, containerCode: "40GP");
			var entries = new List<IRateEntry> { ts1UrsFcl.entry, ts1UrsOrg.entry, ts2UrsFcl.entry, ts2UrsOrg.entry };

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			ursServiceProvider.ChildRateEntries = entries;

			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(entries));

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80)
				.AddContainer("40GP", "GEN", 1, 1000, 1000, 80);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				// Trade service Id 1 & 2, Contract number: 1234567890
				new ()
				{
					RateEntries = [ts1UrsFcl.entry, ts1UrsOrg.entry, ts2UrsFcl.entry, ts2UrsOrg.entry],
					RateLines = [ts1UrsFcl.line, ts1UrsOrg.line, ts2UrsFcl.line, ts2UrsOrg.line],
					PerContainerCommodity =
					[
						new PerContainerCommodityDto { Container = "20GP", Commodity = "GEN", RateType = "123" },
						new PerContainerCommodityDto { Container = "40GP", Commodity = "GEN" }
					]
				},
				// Trade service Id 1 & 2, Contract number: Empty
				new ()
				{
					RateEntries = [ts1UrsOrg.entry, ts2UrsFcl.entry, ts2UrsOrg.entry],
					RateLines = [ts1UrsOrg.line, ts2UrsFcl.line, ts2UrsOrg.line],
					PerContainerCommodity =
					[
						new PerContainerCommodityDto { Container = "20GP", Commodity = "GEN", RateType = "456" },
						new PerContainerCommodityDto { Container = "40GP", Commodity = "GEN" }
					]
				},
			}, mockProviderFactory.Object);
		}

		public void TestRateCuration_MergesURSRatesWithDifferentContainers()
		{
			var ts1UrsFrt = UrsHelper.CreateUrsEntryAndLine(id: "1", "FCL", "FRT", commodityCode: "GEN", contractNumber: string.Empty, containerCode: "20GP", (entry) =>
				entry.CustomFields = [new CustomField { Code = Rate.CustomFields.CargoSphere.RateType, Value = "123" }]
			);
			var ts2UrsFrt = UrsHelper.CreateUrsEntryAndLine(id: "2", "FCL", "FRT", commodityCode: "GEN", contractNumber: "1234567890", containerCode: "20GP");
			var ts3UrsDst = UrsHelper.CreateUrsEntryAndLine(id: "3", "FCL", "DST", commodityCode: "GEN", contractNumber: string.Empty, containerCode: "40GP", (entry) =>
				entry.CustomFields = [new CustomField { Code = Rate.CustomFields.CargoSphere.RateType, Value = "456" }]
			);
			var ts4UrsDst = UrsHelper.CreateUrsEntryAndLine(id: "4", "FCL", "DST", commodityCode: "GEN", contractNumber: "0987654321", containerCode: "40GP");
			var entries = new List<IRateEntry> { ts1UrsFrt.entry, ts2UrsFrt.entry, ts3UrsDst.entry, ts4UrsDst.entry };

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80)
				.AddContainer("40GP", "GEN", 1, 1000, 1000, 80);

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			ursServiceProvider.ChildRateEntries = entries;

			var mockProvider = new Mock<IRateSelectorProvider>();
			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(entries));

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				// Contract Number: Empty
				new ()
				{
					RateEntries = [ts1UrsFrt.entry, ts3UrsDst.entry],
					RateLines = [ts1UrsFrt.line, ts3UrsDst.line],
					PerContainerCommodity =
					[
						new PerContainerCommodityDto { Container = "20GP", Commodity = "GEN", RateType = "123" },
						new PerContainerCommodityDto { Container = "40GP", Commodity = "GEN", RateType = "456" }
					]
				},
				// Contract Number: 1234567890
				new ()
				{
					RateEntries = [ts2UrsFrt.entry, ts3UrsDst.entry],
					RateLines = [ts2UrsFrt.line, ts3UrsDst.line],
					PerContainerCommodity =
					[
						new PerContainerCommodityDto { Container = "20GP", Commodity = "GEN" },
						new PerContainerCommodityDto { Container = "40GP", Commodity = "GEN", RateType = "456" }
					]
				},
				// Contract Number: 0987654321
				new ()
				{
					RateEntries = [ts1UrsFrt.entry, ts4UrsDst.entry],
					RateLines = [ts1UrsFrt.line, ts4UrsDst.line],
					PerContainerCommodity =
					[
						new PerContainerCommodityDto { Container = "20GP", Commodity = "GEN", RateType = "123" },
						new PerContainerCommodityDto { Container = "40GP", Commodity = "GEN" }
					]
				},
			}, mockProviderFactory.Object);
		}

		public void TestRateCuration_DoNotMergeUrsRatesWithUnmappedContainer()
		{
			var ts1UrsFrt = UrsHelper.CreateUrsEntryAndLine(id: "1", "FCL", "FRT", commodityCode: "GEN", contractNumber: string.Empty, containerCode: "20GP");
			var ts2UrsDst = UrsHelper.CreateUrsEntryAndLine(id: "2", "FCL", "DST", commodityCode: "GEN", contractNumber: "1234567890", containerCode: "99GP");
			var entries = new List<IRateEntry> { ts1UrsFrt.entry, ts2UrsDst.entry };

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80);

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			ursServiceProvider.ChildRateEntries = entries;

			var mockProvider = new Mock<IRateSelectorProvider>();
			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(entries));

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				// Container: 20GP, should not include unmapped 99GP container
				new ()
				{
					RateEntries = [ts1UrsFrt.entry],
					RateLines = [ts1UrsFrt.line],
					PerContainerCommodity =
					[
						new PerContainerCommodityDto { Container = "20GP", Commodity = "GEN" },
					]
				},
				// Container: 99GP (unmapped)
				new ()
				{
					RateEntries = [ts2UrsDst.entry],
					RateLines = [ts2UrsDst.line],
					PerContainerCommodity =
					[
						new PerContainerCommodityDto { Commodity = "GEN" },
					]
				},
			}, mockProviderFactory.Object);
		}

		public void TestRateCuration_MergesURSRatesWithDifferentCommodities()
		{
			var ts1UrsFrt = UrsHelper.CreateUrsEntryAndLine(id: "1", "FCL", "FRT", commodityCode: "GEN", contractNumber: string.Empty, containerCode: "20GP");
			var ts2UrsFrt = UrsHelper.CreateUrsEntryAndLine(id: "2", "FCL", "FRT", commodityCode: "GEN", contractNumber: "1234567890", containerCode: "20GP");
			var ts3UrsDst = UrsHelper.CreateUrsEntryAndLine(id: "3", "FCL", "DST", commodityCode: "HAZ", contractNumber: string.Empty, containerCode: "20GP");
			var ts4UrsDst = UrsHelper.CreateUrsEntryAndLine(id: "4", "FCL", "DST", commodityCode: "HAZ", contractNumber: "0987654321", containerCode: "20GP");
			var entries = new List<IRateEntry> { ts1UrsFrt.entry, ts2UrsFrt.entry, ts3UrsDst.entry, ts4UrsDst.entry };

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			ursServiceProvider.ChildRateEntries = entries;

			var mockProvider = new Mock<IRateSelectorProvider>();
			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(entries));

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80)
				.AddContainer("20GP", "HAZ", 1, 1000, 1000, 80);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				// Contract Number: Empty
				new ()
				{
					RateEntries = [ts1UrsFrt.entry, ts3UrsDst.entry],
					RateLines = [ts1UrsFrt.line, ts3UrsDst.line],
					PerContainerCommodity =
					[
						new PerContainerCommodityDto { Container = "20GP", Commodity = "GEN" },
						new PerContainerCommodityDto { Container = "20GP", Commodity = "HAZ" }
					]
				},
				// Contract Number: 1234567890, Shouldn't include ts1UrsFrt as its the same Commodity as GEN
				new ()
				{
					RateEntries = [ts2UrsFrt.entry, ts3UrsDst.entry],
					RateLines = [ts2UrsFrt.line, ts3UrsDst.line],
					PerContainerCommodity =
					[
						new PerContainerCommodityDto { Container = "20GP", Commodity = "GEN" },
						new PerContainerCommodityDto { Container = "20GP", Commodity = "HAZ" }
					]
				},
				// Contract Number: 0987654321
				new ()
				{
					RateEntries = [ts1UrsFrt.entry, ts4UrsDst.entry],
					RateLines = [ts1UrsFrt.line, ts4UrsDst.line],
					PerContainerCommodity =
					[
						new PerContainerCommodityDto { Container = "20GP", Commodity = "GEN" },
						new PerContainerCommodityDto { Container = "20GP", Commodity = "HAZ" }
					]
				},
			}, mockProviderFactory.Object);
		}

		#endregion

		#region Urs Rates

		public void TestRateSearch_AirRatesFromUrs_HasCustomFieldsSet()
		{
			var ursLine = UrsHelper.CreateUrsLine("FRT", "M3", "AUD", 5m, "UNT");

			var ursEntry = UrsHelper.CreateUrsEntry(
				mode: "LSE",
				category: "AIR",
				origin: "AUSYD",
				destination: "UAIEV",
				commodityCode: "GEN",
				rateProvider: "URS",
				startDate: new ZDate(2020, 06, 06),
				endDate: new ZDate(2030, 06, 06),
				rateLines: ursLine,
				commodityGroup: "CM1"
			);
			ursEntry.TI_PaymentTerm = "PPD";
			ursEntry.ProductName = "Dangerous Goods";
			ursEntry.CustomFields = new[]
			{
				new CustomField { Code = Rate.CustomFields.Cargoguide.Remarks, Value = "Remarks" },
				new CustomField { Code = Rate.CustomFields.Cargoguide.DeckType, Value = "Deck" },
				new CustomField { Code = Rate.CustomFields.Cargoguide.Ratio, Value = "1:6" },
				new CustomField { Code = Rate.CustomFields.Cargoguide.ProductCode, Value = "Dangerous Goods" },
			};
			ursEntry.CarrierSpecificCommodity = new CarrierSpecificCommodity
			{
				Code = "PRD",
				GroupName = "Some Product Name",
				GroupType = CommodityCategory.NonHazardous,
				IncludedCommodities = ["HEALTH FOOD", "MILK POWDER"]
			};

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			var entries = new List<IRateEntry> { ursEntry };
			ursServiceProvider.ChildRateEntries = entries;

			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(entries));

			var query = CreateRateQuery("AIR", "LSE", "AUMEL", "USLAX")
				.AddNonContainerizedCargo("GEN", 10m, 1m);
			var response = PerformRateSearch(query, providerFactory: mockProviderFactory.Object);

			AssertResponseEquals([
				new RateResultDto
				{
					ResultId = response.Rates[0].ResultId,
					Source = RateResultDto.RateSource.Cargoguide,
					SourcePKs = [ursEntry.PK.ToGuid()],
					Origin = "AUMEL",
					Destination = "USLAX",
					TransportMode = "AIR",
					ContainerMode = "LSE",
					ContainerTypes = [],
					Charges =
					[
						new RateChargeDto
						{
							SourcePK = ursLine.PK.ToGuid(),
							ChargeCode = new()
							{
								ChargeCode = "FRT",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "International Freight",
								CarrierChargeCode = "",
								CarrierChargeCodeDescription = "",
							},
							ChargeUnit = "M3",
							ChargeType = string.Empty,
							CalculationDescription = "1 Cubic Meter(s) @ AUD 5.00/M3",
							LocalAmount = 5m,
							LocalCurrency = "AUD",
							RateAmount = 5m,
							RateCurrency = "AUD",
							ContainerType = null,
							Commodity = new()
							{
								UniversalCommodityGroup = "CM1",
							},
							Route = ["AUMEL", "USLAX"]
						}
					],
					Carrier = null,
					Route = ["AUMEL", "USLAX"],
					StartDate = ursEntry.TI_RateStartDate.ToDateTime(),
					EndDate = ursEntry.TI_RateEndDate.ToDateTime(),
					ProductCode = "Dangerous Goods",
					UniversalCommodityGroup = new()
					{
						UniversalCommodityGroup = "CM1",
					},
					PaymentTerm = "PPD",
					Deck = "Deck",
					ServiceProvider = new CarrierDto(TransportProvider1),
					NamedAccounts = [],
					PerContainerCommodity =
					[
						new PerContainerCommodityDto
						{
							Commodity = "GEN",
							ChargeableFactor = "1:6",
							Remarks = "Remarks",
							CarrierCommodity = new CarrierCommodityDto
							{
								Code = "PRD",
								GroupName = "Some Product Name",
								GroupType = CommodityCategory.NonHazardous,
								Commodities = ["HEALTH FOOD", "MILK POWDER"],
								IncludedCommodities = [],
								ExcludedCommodities = []
							}
						}
					]
				}
			], response);
		}

		public void TestRateSearch_SeaRatesFromUrs_HasCustomFieldsSet()
		{
			var ursLine = UrsHelper.CreateUrsLine("FRT", "CN", "AUD", 5m, "UNT");
			ursLine.CustomFields = new[]
			{
				new CustomField { Code = Rate.CustomFields.CargoSphere.HandlingOffice, Value = "HandlingOffice" }
			};

			var ursEntry = UrsHelper.CreateUrsEntry(
				mode: "SEA",
				category: "FCL",
				origin: "AUSYD",
				destination: "UAIEV",
				commodityCode: "",
				rateProvider: "URS",
				startDate: new ZDate(2020, 06, 06),
				endDate: new ZDate(2030, 06, 06),
				rateLines: ursLine
			);
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			ursEntry.TI_RC = container.PK;
			ursEntry.ChildRateLines = new[] { ursLine };
			ursEntry.CustomFields = new[]
			{
				new CustomField { Code = Rate.CustomFields.CargoSphere.RateType, Value = "RateType" },
				new CustomField { Code = Rate.CustomFields.CargoSphere.RateType2, Value = "RateType2" },
				new CustomField { Code = Rate.CustomFields.CargoSphere.Vessel, Value = "Vessel" },
				new CustomField { Code = Rate.CustomFields.CargoSphere.ArbitraryPermission, Value = "AddOn" },
				new CustomField { Code = Rate.CustomFields.CargoSphere.TradeLane, Value = "Tradelane" },
				new CustomField { Code = Rate.CustomFields.CargoSphere.ServiceString, Value = "Service" },
				new CustomField { Code = Rate.CustomFields.CargoSphere.ContainerQuality, Value = "NOR" },
			};
			ursEntry.CarrierSpecificCommodity = new CarrierSpecificCommodity
			{
				IncludedCommodities = ["HEALTH FOOD", "MILK POWDER"],
				ExcludedCommodities = ["BABY FOOD", "CHEESE"],
				Code = "Some Product Code",
				GroupName = "Some Group Description",
				GroupType = "Test Name"
			};

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			var entries = new List<IRateEntry> { ursEntry };
			ursServiceProvider.ChildRateEntries = entries;

			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(entries));

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80);

			var response = PerformRateSearch(query, providerFactory: mockProviderFactory.Object);

			AssertResponseEquals([
				new RateResultDto
				{
					ResultId = response.Rates[0].ResultId,
					Source = RateResultDto.RateSource.CargoSphere,
					SourcePKs = [ursEntry.PK.ToGuid()],
					Origin = "AUMEL",
					Destination = "USLAX",
					TransportMode = "SEA",
					ContainerMode = "FCL",
					CarrierQuoteNumber = "",
					ContainerTypes = ["20GP"],
					NamedAccounts = [],
					Charges =
					[
						new RateChargeDto
						{
							SourcePK = ursLine.PK.ToGuid(),
							ChargeCode = new()
							{
								ChargeCode = "FRT",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "International Freight",
								CarrierChargeCode = "",
								CarrierChargeCodeDescription = "",
							},
							HandlingOfficeName = "HandlingOffice",
							ChargeUnit = "CN",
							ChargeType = string.Empty,
							CalculationDescription = "1 20GP Container(s) @ AUD 5.00/Container",
							LocalAmount = 5m,
							LocalCurrency = "AUD",
							RateAmount = 5m,
							RateCurrency = "AUD",
							ContainerType = "20GP",
							ContainerQuality = "NOR",
							Commodity = new() { UniversalCommodityGroup = "" },
							Route = ["AUMEL", "USLAX"]
						}
					],
					Carrier = null,
					Vessel = "Vessel",
					Tradelane = "Tradelane",
					ServiceString = "Service",
					Route = ["AUMEL", "USLAX"],
					StartDate = ursEntry.TI_RateStartDate.ToDateTime(),
					EndDate = ursEntry.TI_RateEndDate.ToDateTime(),
					ServiceProvider = new CarrierDto(TransportProvider1),
					PerContainerCommodity =
					[
						new PerContainerCommodityDto
						{
							Container = "20GP",
							Commodity = "",
							ContainerQuality = "NOR",
							AddOn = "AddOn",
							RateType = "RateType",
							RateType2 = "RateType2",
							CarrierCommodity = new CarrierCommodityDto
							{
								IncludedCommodities = ["HEALTH FOOD", "MILK POWDER"],
								ExcludedCommodities = ["BABY FOOD", "CHEESE"],
								Code = "Some Product Code",
								GroupName = "Some Group Description",
								GroupType = "Test Name",
								Commodities = []
							}
						}
					]
				}
			], response);
		}

		public void TestRateSearch_SpotRatesFromUrs_HasBookingInfoAndSchedule()
		{
			var ursLine = UrsHelper.CreateUrsLine("FRT", "CN", "AUD", 5m, "UNT");
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var info = new UrsBookingInfo
			{
				BookingTerms = new BookingTerms
				{
					Items = [
						new BookingTermItem
						{
							Name = "Description",
							Currency = "USD",
							Fee = 100,
						}
					]
				},
				Schedule = new UrsSchedule
				{
					ExternalPriceReference = "Reference",
					ScheduleDetails = [
						new ScheduleDetail
						{
							Origin = "AUSYD",
							DepartureDate = new DateTime(2020, 12, 12),
							Destination = "USLAX",
							ArrivalDate = new DateTime(2020, 12, 15),
							VesselName = "VesselName",
							IMONumber = "VesselNumber",
							VoyageNumber = "VoyageNumber",
							ServiceCode = "ServiceCode",
							ServiceName = "ServiceName",
							FlagCode = "FlagCode",
							DateInfos = [
								new ScheduleDateInfo
								{
									Code = "CY",
									Name = "Commercial cargo cutoff",
									Type = "Documentation",
									Date = new DateTime(2020, 12, 12)
								},
								new ScheduleDateInfo
								{
									Code = "XX",
									Name = "Origin demurrage",
									Type = "STO",
									Date = new DateTime(2020, 12, 15)
								}
							]
						}
					]
				},
			};
			var tradeService = new TradeServiceDto { PriceInfo = new PriceInfoDto() };
			(tradeService.PriceInfo as PriceInfoDto)
				.AddFreeTimeWithSinglePriceEntry(new("ODOC",
					usabilityGroupCode: ChargeUsabilityGroupCode.Pen,
					chargeDescription: "Description",
					shippingPhaseCode: ShippingPhaseCode.PortfLoading,
					breakQuantity: 0,
					quantityUnit: "Day",
					chargeDefinitionCode: "ABC",
					price: 0));
			(tradeService.PriceInfo.FreeTime.Items.First().RateCollections.Items.First() as RateCollectionDto)
				.AddPriceEntry(new () { Price = 50, BreakQuantity = 10 })
				.AddPriceEntry(new () { Price = 12, BreakQuantity = 15 });
			tradeService.CreateRoute().AddWaypoint("AUMEL", RouteWaypointCode.Origin).AddWaypoint("USLAX", RouteWaypointCode.Destination);
			var ursEntry = UrsHelper.CreateUrsEntry(
				mode: "SEA",
				category: "FCL",
				origin: "AUSYD",
				destination: "UAIEV",
				commodityCode: "",
				rateProvider: "URS",
				startDate: new ZDate(2020, 06, 06),
				endDate: new ZDate(2030, 06, 06),
				bookingInfo: info,
				rateLines: ursLine,
				tradeService: tradeService
			);
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			ursEntry.TI_RC = container.PK;
			ursEntry.ChildRateLines = new[] { ursLine };
			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			ursServiceProvider.ChildRateEntries = new[] { ursEntry };

			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(ursEntry));

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80);

			var response = PerformRateSearch(query, providerFactory: mockProviderFactory.Object);

			var expectedBookingInfo = new BookingInfoDto
			{
				SpotTerms = [
					new SpotTermDto
					{
						Currency = "USD",
						Description = "Description",
						Price = 100
					}
				],
				Deadlines = [
					new DeadlineDto
					{
						Code = "CY",
						Description = "Commercial cargo cutoff",
						Type = "Documentation",
						DateTime = new DateTime(2020, 12, 12)
					},
					new DeadlineDto
					{
						Code = "XX",
						Description = "Origin demurrage",
						Type = "STO",
						DateTime = new DateTime(2020, 12, 15)
					},
				],
				Penalties = [
					new PenaltyDto
					{
						Tiers = [
							new PenaltyTierDto
							{
								Process = Core.Constants.FreightShipmentDirection.Code.Export,
								Type = "ABC",
								Description = "Description",
								StartDay = 0,
								EndDay = 9,
								Currency = "USD",
								Price = 0
							},
							new PenaltyTierDto
							{
								Process = Core.Constants.FreightShipmentDirection.Code.Export,
								Type = "ABC",
								Description = "Description",
								StartDay = 10,
								EndDay = 14,
								Currency = "USD",
								Price = 50
							},
							new PenaltyTierDto
							{
								Process = Core.Constants.FreightShipmentDirection.Code.Export,
								Type = "ABC",
								Description = "Description",
								StartDay = 15,
								Currency = "USD",
								Price = 12
							}
						]
					}
				]
			};
			var expectedSchedule = new ScheduleDto
			{
				RateId = "Reference",
				ScheduleLegs = [
					new ScheduleLegDto
					{
						Departure = "AUSYD",
						DepartureDate = new DateTime(2020, 12, 12),
						Arrival = "USLAX",
						ArrivalDate = new DateTime(2020, 12, 15),
						Vessel = "VesselName",
						VesselNumber = "VesselNumber",
						VoyageNumber = "VoyageNumber",
						ServiceCode = "ServiceCode",
						ServiceName = "ServiceName",
						FlagCode = "FlagCode",
					}
				]
			};

			AssertEquals(1, response.Rates.Length);
			AssertEquals(1, response.Rates[0].BookingInfo.Count);
			AssertEquals(expectedBookingInfo.ToString(), response.Rates[0].BookingInfo["20GP"].ToString());
			AssertEquals(expectedSchedule.ToString(), response.Rates[0].Schedule.ToString());
		}

		public void TestRateSearch_SeaRatesFromUrs_UnmappedContainerTypes()
		{
			var ursLine = UrsHelper.CreateUrsLine("FRT", "CN", "AUD", 5m, "UNT");

			var ursEntry = UrsHelper.CreateUrsEntry(
				mode: "SEA",
				category: "FCL",
				origin: "AUSYD",
				destination: "UAIEV",
				commodityCode: "",
				rateProvider: "URS",
				startDate: new ZDate(2020, 06, 06),
				endDate: new ZDate(2030, 06, 06),
				rateLines: ursLine
			);
			ursEntry.UrsContainer = new UrsContainer
			{
				Code = "TST",
				ContainerPKs = [],
			};
			ursEntry.ChildRateLines = [ursLine];

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			var entries = new List<IRateEntry> { ursEntry };
			ursServiceProvider.ChildRateEntries = entries;

			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(entries));

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80);

			var response = PerformRateSearch(query, providerFactory: mockProviderFactory.Object);

			AssertResponseEquals([
				new RateResultDto
				{
					ResultId = response.Rates[0].ResultId,
					Source = RateResultDto.RateSource.CargoSphere,
					SourcePKs = [ursEntry.PK.ToGuid()],
					Origin = "AUMEL",
					Destination = "USLAX",
					TransportMode = "SEA",
					ContainerMode = "FCL",
					CarrierQuoteNumber = "",
					ContainerTypes = [],
					UnmappedContainerTypes = ["TST"],
					NamedAccounts = [],
					Charges =
					[
						new RateChargeDto
						{
							SourcePK = ursLine.PK.ToGuid(),
							ChargeCode = new()
							{
								ChargeCode = "FRT",
								ChargeCodeGroup = "FRT",
								ChargeCodeDescription = "International Freight",
								CarrierChargeCode = "",
								CarrierChargeCodeDescription = "",
							},
							ChargeUnit = "CN",
							ChargeType = string.Empty,
							CalculationDescription = "1 Container(s) @ AUD 5.00/Container",
							LocalAmount = 5m,
							LocalCurrency = "AUD",
							RateAmount = 5m,
							RateCurrency = "AUD",
							Commodity = new () { UniversalCommodityGroup = "" },
							Route = ["AUMEL", "USLAX"],
						}
					],
					Carrier = null,
					Route = ["AUMEL", "USLAX"],
					StartDate = ursEntry.TI_RateStartDate.ToDateTime(),
					EndDate = ursEntry.TI_RateEndDate.ToDateTime(),
					ServiceProvider = new CarrierDto(TransportProvider1),
					PerContainerCommodity = [new PerContainerCommodityDto { Commodity = "" }],
				}
			], response);
		}

		public void TestRateSearch_SeaRatesFromUrs_WithUnmappedServiceProvider()
		{
			var ursEntry = UrsHelper.CreateUrsEntryWithContainer("20GP");

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromSCAC("ABC"));
			ursServiceProvider.ChildRateEntries = new[] { ursEntry };

			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(ursEntry));

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80);

			var results = PerformRateSearch(query, providerFactory: mockProviderFactory.Object).Rates;

			CombineAssertions(() =>
			{
				AssertEquals(1, results.Length);
				var serviceProvider = results[0].ServiceProvider;
				AssertEquals("ABC", serviceProvider.ScacCode);
				Assert(string.IsNullOrEmpty(serviceProvider.IataCode));
				AssertNull(serviceProvider.OrgCode);
				AssertNull(serviceProvider.C1cCode);
				AssertEquals(0, serviceProvider.ReferenceLines.Count);
			});
		}

		public void TestRateSearch_SeaRatesFromUrs_WithNamedAccount()
		{
			var ursEntry = UrsHelper.CreateUrsEntryWithContainer("20GP");
			ursEntry.NamedAccounts = new[] { "company1", "company2" };

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromSCAC("ABC"));
			ursServiceProvider.ChildRateEntries = new[] { ursEntry };

			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(ursEntry));

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80);

			var results = PerformRateSearch(query, providerFactory: mockProviderFactory.Object).Rates;

			CombineAssertions(() =>
			{
				AssertEquals(1, results.Length);
				var namedAccounts = results[0].NamedAccounts;
				AssertEquals(2, namedAccounts.Length);
				AssertContainsExactElementsInAnyOrder(new[] { "company1", "company2" }, namedAccounts);
			});
		}

		public void TestRateSearch_SeaRatesFromUrs_WithUnmappedChargeCode()
		{
			var ursLine = UrsHelper.CreateUrsLine(
				chargeCode: "",
				weightVolume: "CN",
				currency: "AUD",
				amount: 5m,
				calculatorCode: "UNT",
				new BaseChargeDto
				{
					ChargeDefinition = new ChargeDefinitionDto
					{
						UniversalCode = "NonExistent"
					}
				});
			ursLine.TL_AC = ZGuid.Empty;
			var ursEntry = UrsHelper.CreateUrsEntryWithContainer("20GP");
			ursEntry.ChildRateLines = [ursLine];

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromSCAC("ABC"));
			ursServiceProvider.ChildRateEntries = new[] { ursEntry };

			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(ursEntry));

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80);

			var results = PerformRateSearch(query, providerFactory: mockProviderFactory.Object).Rates;

			CombineAssertions(() =>
			{
				AssertEquals("Should have 1 result", 1, results.Length);
				AssertEquals("Should have 1 charge", 1, results[0].Charges.Count);
				var chargeCode = results[0].Charges.First().ChargeCode;
				var expectedChargeCode = new ChargeCodeDto
				{
					UniversalChargeCode = "NonExistent",
				};
				AssertEquals("Charge codes should match",  expectedChargeCode.ToString(), chargeCode.ToString());
			});
		}

		public void TestRateSearch_AirRatesFromUrs_WithUnmappedServiceProvider()
		{
			var ursEntry = UrsHelper.CreateUrsEntryWithContainer("20GP", "AIR");

			var airline = Factory.New<RefAirline>();
			airline.RM_ThreeLetterCode = "XXX";
			Factory.Save();

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromIATA("XXX"));
			ursServiceProvider.ChildRateEntries = new[] { ursEntry };

			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider(ursEntry));

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80);

			var results = PerformRateSearch(query, providerFactory: mockProviderFactory.Object).Rates;

			CombineAssertions(() =>
			{
				AssertEquals(1, results.Length);
				var serviceProvider = results[0].ServiceProvider;
				AssertEquals("XXX", serviceProvider.IataCode);
				Assert(string.IsNullOrEmpty(serviceProvider.ScacCode));
				Assert(string.IsNullOrEmpty(serviceProvider.OrgCode));
				Assert(string.IsNullOrEmpty(serviceProvider.C1cCode));
			});
		}

		public void TestRateSearch_AirULDRatesFromUrs_HasDistinctRateTerms()
		{
			var entryLD2 = UrsHelper.CreateUrsEntryWithContainer("LD-2", "AIR");
			var entryLD3 = UrsHelper.CreateUrsEntryWithContainer("LD-3", "AIR");
			var entryLD32 = UrsHelper.CreateUrsEntryWithContainer("LD-3", "AIR");

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			ursServiceProvider.ChildRateEntries = [entryLD2, entryLD3, entryLD32];

			var query = CreateRateQuery("AIR", "ULD", "AUMEL", "USLAX")
				.AddContainer("LD-2", "GEN", 1, 1000, 1000, 80)
				.AddContainer("LD-3", "GEN", 1, 1000, 1000, 80);

			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider([entryLD2, entryLD3, entryLD32]));
			var results = PerformRateSearch(query, providerFactory: mockProviderFactory.Object).Rates;

			// only 1 RateTermsDto should be created for each containertype
			var expectedRateTerms = new [] { new PayloadDto(entryLD2), new PayloadDto(entryLD3) };
			var actualRateTermStrings = results[0].PayloadInfo.Values.Select(term => term.ToString()).ToArray();

			AssertContainsExactElementsInAnyOrder(
				expectedRateTerms.Select(term => term.ToString()),
				actualRateTermStrings);
		}

		public void TestRateSearch_ComposedRates()
		{
			var preCarriageService = new TradeServiceDto();
			preCarriageService.CreateRoute()
				.AddWaypoint("USCLT", RouteWaypointCode.Origin)
				.AddWaypoint("USCHS", RouteWaypointCode.PortOfLoading);

			var freightService = new TradeServiceDto();
			freightService.CreateRoute()
				.AddWaypoint("USCHS", RouteWaypointCode.PortOfLoading)
				.AddWaypoint("BEANR", RouteWaypointCode.Destination);

			var preCarriageLine = UrsHelper.CreateUrsLine("FRT", "CN", "AUD", 10m, "UNT", null, preCarriageService);
			var freightLine = UrsHelper.CreateUrsLine("FRT", "CN", "AUD", 15m, "UNT", null, freightService);
			var composedLine = UrsHelper.CreateUrsLine("ODOC", "CN", "AUD", 10m, "UNT", null, preCarriageService);

			var freightRateEntry = UrsHelper.CreateUrsEntry(
				mode: "SEA",
				category: "FCL",
				origin: "USCLT",
				destination: "USCHS",
				commodityCode: "GEN",
				rateProvider: "URS",
				startDate: new ZDate(2020, 06, 06),
				endDate: new ZDate(2030, 06, 06),
				rateLines: [preCarriageLine, freightLine]
			);
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			freightRateEntry.TI_RC = container.PK;

			var docRateEntry = UrsHelper.CreateUrsEntry(
				mode: "SEA",
				category: "FCL",
				origin: "USCLT",
				destination: "USCHS",
				commodityCode: "GEN",
				rateProvider: "URS",
				startDate: new ZDate(2020, 06, 06),
				endDate: new ZDate(2030, 06, 06),
				rateLines: [composedLine]
			);
			docRateEntry.TI_RC = container.PK;

			var ursHeader = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			ursHeader.ChildRateEntries = [freightRateEntry, docRateEntry];

			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider([freightRateEntry, docRateEntry]));

			var query = CreateRateQuery("SEA", "FCL", "USCLT", "BEANR")
				.AddContainer("20GP", "GEN", 3, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [freightRateEntry, docRateEntry],
					RateLines = [preCarriageLine, freightLine, composedLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 30m, "20GP", "GEN")
						.AddCharge("FRT", 45m, "20GP", "GEN")
						.AddCharge("ODOC", 30m, "20GP", "GEN")
				},
			}, mockProviderFactory.Object);
		}

		public void TestRateCuration_GroupRatesByRateSource()
		{
			var tp1Costing = Helper.NewCosting(TransportProvider1);

			var cw1RateEntry = tp1Costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", removeLines: true);
			var frtLine1 = cw1RateEntry.AddFlatRateLine("FRT", 20m, "AUD");

			var expectedCW1Rates = new List<IRateEntry> { cw1RateEntry };

			var ursRateLine = UrsHelper.CreateUrsLine("FRT", "CN", "AUD", 10m, "UNT");

			var ursRateEntry = UrsHelper.CreateUrsEntry(
				mode: "SEA",
				category: "FCL",
				origin: "AUSYD",
				destination: "UAIEV",
				commodityCode: "GEN",
				rateProvider: "URS",
				startDate: new ZDate(2020, 06, 06),
				endDate: new ZDate(2030, 06, 06),
				rateLines: ursRateLine
			);
			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			ursRateEntry.TI_RC = container.PK;

			var wiseHeader = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			wiseHeader.ChildRateEntries = new[] { ursRateEntry };

			var mockProvider = new Mock<IRateSelectorProvider>();
			var mockProviderFactory = new Mock<IRateSelectorProviderFactory>();

			var expectedUrsRates = new List<IRateEntry> { ursRateEntry };

			mockProvider
				.SetupSequence(p => p.GetRatesAsync(It.IsAny<RatingCriteria>(), It.IsAny<RateQueryBusinessObject>(), It.IsAny<string>()))
				.ReturnsAsync(expectedCW1Rates)
				.ReturnsAsync(expectedUrsRates);

			mockProviderFactory
				.Setup(f => f.CreateProviders(It.IsAny<LoggerDecorator>(), It.IsAny<BusinessObjectFactory>()))
				.Returns(new List<IRateSelectorProvider> { mockProvider.Object, mockProvider.Object });

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 3, 100m, 10m);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				// Card 1 - CW1 Rate
				new ()
				{
					RateEntries = [cw1RateEntry],
					RateLines = [frtLine1],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 20m, "20GP", "GEN")
				},
				// Card 2 - Urs Rate
				new ()
				{
					RateEntries = [ursRateEntry],
					RateLines = [ursRateLine],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 30m, "20GP", "GEN")
				},
			}, mockProviderFactory.Object);
		}

		public void TestRateCuration_GroupsBySchedule()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));

			var info1 = new UrsBookingInfo
			{
				Schedule = new UrsSchedule
				{
					ScheduleId = "schedule1",
					ScheduleDetails = []
				},
			};
			var info2 = new UrsBookingInfo
			{
				Schedule = new UrsSchedule
				{
					ScheduleId = "schedule2",
					ScheduleDetails = []
				},
			};

			var (ursEntry1, ursLine1) = UrsHelper.CreateUrsEntryAndLine(id: "1", "FCL", "FRT", commodityCode: "GEN", containerCode: "20GP");
			ursEntry1.BookingInfo = info1;
			var (ursEntry2, ursLine2) = UrsHelper.CreateUrsEntryAndLine(id: "1", "FCL", "ODOC", commodityCode: "GEN", containerCode: "20GP");
			ursEntry2.BookingInfo = info1;

			var (ursEntry3, ursLine3) = UrsHelper.CreateUrsEntryAndLine(id: "1", "FCL", "FRT", commodityCode: "GEN", containerCode: "20GP");
			ursEntry3.BookingInfo = info2;

			var container = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			ursEntry1.TI_RC = container.PK;
			ursEntry2.TI_RC = container.PK;
			ursEntry3.TI_RC = container.PK;

			var ursServiceProvider = new UrsRatingHeader(Factory, UrsCarrier.FromOrg(TransportProvider1));
			ursServiceProvider.ChildRateEntries = new[] { ursEntry1, ursEntry2, ursEntry3 };
			var mockProviderFactory = UrsHelper.MockProviderFactory(UrsHelper.MockProvider([ursEntry1, ursEntry2, ursEntry3]));

			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80);

			PerformSearchAndAssert(query, new List<RateResultDtoAssertion>
			{
				new ()
				{
					RateEntries = [ursEntry1, ursEntry2],
					RateLines = [ursLine1, ursLine2],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 5m, "20GP", "GEN")
						.AddCharge("ODOC", 5m, "20GP", "GEN")
				},
				new ()
				{
					RateEntries = [ursEntry3],
					RateLines = [ursLine3],
					Charges = new List<RateChargeDtoAssertion>()
						.AddCharge("FRT", 5m, "20GP", "GEN")
				},
			}, mockProviderFactory.Object);
		}

		#endregion

		#region Chargeable Override

		public void TestRateSearch_LCL_WithSingleContainerAndChargeableOverride()
		{
			//Arrange
			var roundings = new DefaultRoundingsCollection();
			var rounding = roundings.AddNew();
			rounding.Code = RatingConstants.RateCategory.LCL;
			rounding.RoundingType = RatingRoundingTypes.Chargeable;

			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				var costing = Helper.NewCosting(TransportProvider1);
				var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.LCL, "LCL", "AUMEL", "USLAX", ZString.Empty, ZString.Empty, removeLines: true);
				costingEntry.TI_ContractNumber = "123";
				var fscLine = costingEntry.AddUnitRateLine("FSC", 1.1m, "M3", "AUD");
				var ocartLine = costingEntry.AddUnitRateLine("OCART", 1.2m, "KG", "AUD");
				costingEntry.AddUnitRateLine("FRT", 1.3m, "CN", "AUD");
				var bafLine = costingEntry.AddFlatRateLine("BAF", 1.4m, "AUD");

				Factory.Save();

				//Act
				var query = CreateRateQuery("SEA", "LCL", "AUMEL", "USLAX")
					.AddContainer("LCL", "GEN", 1, 1000, 1000, 80);
				var response = PerformRateSearch(query);

				//Assert
				AssertResponseContainsRates(new List<RateResultDtoAssertion>
				{
					new ()
					{
						RateEntries = [costingEntry],
						RateLines = [fscLine, bafLine, ocartLine],
						Charges = new List<RateChargeDtoAssertion>()
							.AddCharge("FSC", 88m, null, "GEN")
							.AddCharge("BAF", 1.4m, null, "GEN")
							.AddCharge("OCART", 0m, null, null)
					},
				}, response);
			}
		}

		public void TestRateSearch_LSE_WithSingleContainerAndChargeableOverride()
		{
			//Arrange
			var roundings = new DefaultRoundingsCollection();
			var rounding = roundings.AddNew();
			rounding.Code = RatingConstants.RateCategory.AIR;
			rounding.RoundingType = RatingRoundingTypes.Chargeable;

			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				var costing = Helper.NewCosting(TransportProvider1);
				var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "AUMEL", "USLAX", ZString.Empty, ZString.Empty, removeLines: true);
				costingEntry.TI_ContractNumber = "123";
				var fscLine = costingEntry.AddUnitRateLine("FSC", 1.1m, "M3", "AUD");
				var ocartLine = costingEntry.AddUnitRateLine("OCART", 1.2m, "KG", "AUD");
				costingEntry.AddUnitRateLine("FRT", 1.3m, "CN", "AUD");
				var bafLine = costingEntry.AddFlatRateLine("BAF", 1.4m, "AUD");

				Factory.Save();

				//Act
				var query = CreateRateQuery("AIR", "LSE", "AUMEL", "USLAX")
					.AddContainer("LSE", "GEN", 1, 1000, 1000, 80);
				var response = PerformRateSearch(query);

				//Assert
				AssertResponseContainsRates(new List<RateResultDtoAssertion>
				{
					new ()
					{
						RateEntries = [costingEntry],
						RateLines = [ocartLine, bafLine, fscLine],
						Charges = new List<RateChargeDtoAssertion>()
							.AddCharge("OCART", 96m, null, "GEN")
							.AddCharge("BAF", 1.4m, null, "GEN")
							.AddCharge("FSC", 0m, null, null)
					},
				}, response);
			}
		}

		public void TestRateSearch_FCL_WithSingleContainerAndChargeableOverride()
		{
			//Arrange
			var roundings = new DefaultRoundingsCollection();
			var rounding = roundings.AddNew();
			rounding.Code = RatingConstants.RateCategory.FCL;
			rounding.RoundingType = RatingRoundingTypes.Chargeable;

			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				var costing = Helper.NewCosting(TransportProvider1);
				var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", container: "20GP", removeLines: true);
				costingEntry.TI_ContractNumber = "123";
				var fscLine = costingEntry.AddUnitRateLine("FSC", 1.1m, "M3", "AUD");
				var ocartLine = costingEntry.AddUnitRateLine("OCART", 1.2m, "KG", "AUD");
				var frtLine = costingEntry.AddUnitRateLine("FRT", 1.3m, "CN", "AUD");
				var bafLine = costingEntry.AddFlatRateLine("BAF", 1.4m, "AUD");

				Factory.Save();

				//Act
				var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
					.AddContainer("20GP", "GEN", 1, 1000, 1000, 80);
				var response = PerformRateSearch(query);

				//Assert
				AssertResponseContainsRates(new List<RateResultDtoAssertion>
				{
					new ()
					{
						RateEntries = [costingEntry],
						RateLines = [ocartLine, bafLine, frtLine, fscLine],
						Charges = new List<RateChargeDtoAssertion>()
							.AddCharge("OCART", 0, null, null)
							.AddCharge("BAF", 1.4m, "20GP", "GEN")
							.AddCharge("FRT", 1.3m, "20GP", "GEN")
							.AddCharge("FSC", 88m, "20GP", "GEN")
					},
				}, response);
			}
		}

		public void TestRateSearch_FCL_WithMultipleContainersAndChargeableOverride()
		{
			//Arrange
			var roundings = new DefaultRoundingsCollection();
			var rounding = roundings.AddNew();
			rounding.Code = RatingConstants.RateCategory.FCL;
			rounding.RoundingType = RatingRoundingTypes.Chargeable;

			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				var costing = Helper.NewCosting(TransportProvider1);
				var costingEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", container: "20GP", removeLines: true);
				var fsc20GP = costingEntry20GP.AddUnitRateLine("FSC", 1.1m, "M3", "AUD");
				var baf20GP = costingEntry20GP.AddUnitRateLine("BAF", 1.4m, "CN", "AUD");

				var costingEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", container: "40GP", removeLines: true);
				var fsc40GP = costingEntry40GP.AddUnitRateLine("FSC", 1.2m, "M3", "AUD");
				var baf40GP = costingEntry40GP.AddUnitRateLine("BAF", 1.5m, "CN", "AUD");

				var costingEntryAll = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", removeLines: true);
				var frtM3 = costingEntryAll.AddUnitRateLine("FRT", 1.3m, "M3", "AUD");
				var frtCN = costingEntryAll.AddUnitRateLine("FRT", 1.6m, "CN", "AUD");
				var ocartAll = costingEntryAll.AddFlatRateLine("OCART", 1.7m, "AUD");

				Factory.Save();

				//Act
				var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
					.AddContainer("20GP", "GEN", 1, 1000, 1000, 5)
					.AddContainer("40GP", "GEN", 1, 1000, 1000, 18)
					.AddContainer("40GP", "GEN", 1, 1000, 1000, 20);
				var response = PerformRateSearch(query);

				//Assert
				AssertResponseContainsRates(new List<RateResultDtoAssertion>
				{
					new ()
					{
						RateEntries = [costingEntry20GP, costingEntry40GP, costingEntryAll],
						RateLines = [fsc20GP, baf20GP, fsc40GP, baf40GP, ocartAll, frtCN, frtM3],
						Charges = new List<RateChargeDtoAssertion>()
							.AddCharge("OCART", 1.7m, null, "GEN")
							.AddCharge("FRT", 55.9m, null, "GEN")
							.AddCharge("FRT", 4.8m, null, "GEN")
							.AddCharge("FSC", 5.5m, "20GP", "GEN")
							.AddCharge("FSC", 45.6m, "40GP", "GEN")
							.AddCharge("BAF", 1.4m, "20GP", "GEN")
							.AddCharge("BAF", 3.0m, "40GP", "GEN")
					},
				}, response);
			}
		}
		public void TestRateSearch_FCL_WithImperialChargeableUnit()
		{
			//Arrange
			var roundings = new DefaultRoundingsCollection();
			var rounding = roundings.AddNew();
			rounding.Code = RatingConstants.RateCategory.FCL;
			rounding.RoundingType = RatingRoundingTypes.Chargeable;

			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				var costing = Helper.NewCosting(TransportProvider1);
				var costingEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", container: "20GP", removeLines: true);
				var fsc20GP = costingEntry20GP.AddUnitRateLine("FSC", 1.1m, "M3", "AUD");
				var baf20GP = costingEntry20GP.AddUnitRateLine("BAF", 1.4m, "CN", "AUD");

				var costingEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", container: "40GP", removeLines: true);
				var fsc40GP = costingEntry40GP.AddUnitRateLine("FSC", 1.2m, "M3", "AUD");
				var baf40GP = costingEntry40GP.AddUnitRateLine("BAF", 1.5m, "CN", "AUD");

				var costingEntryAll = costing.AddRateEntry(RatingConstants.RateCategory.FCL, "SEA", "AUMEL", "USLAX", removeLines: true);
				var frtM3 = costingEntryAll.AddUnitRateLine("FRT", 1.3m, "M3", "AUD");
				var frtCN = costingEntryAll.AddUnitRateLine("FRT", 1.6m, "CN", "AUD");
				var ocartAll = costingEntryAll.AddFlatRateLine("OCART", 1.7m, "AUD");

				Factory.Save();

				//Act
				var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
					.AddContainer("20GP", "GEN", 1, 1000, 1000, 176.573m, "CF")
					.AddContainer("40GP", "GEN", 1, 1000, 1000, 18)
					.AddContainer("40GP", "GEN", 1, 1000, 1000, 20);
				var response = PerformRateSearch(query);

				//Assert
				AssertResponseContainsRates(new List<RateResultDtoAssertion>
				{
					new ()
					{
						RateEntries = [costingEntry20GP, costingEntry40GP, costingEntryAll],
						RateLines = [fsc20GP, baf20GP, fsc40GP, baf40GP, ocartAll, frtCN, frtM3],
						Charges = new List<RateChargeDtoAssertion>()
							.AddCharge("OCART", 1.7m, null, "GEN")
							.AddCharge("FRT", 55.9m, null, "GEN")
							.AddCharge("FRT", 4.8m, null, "GEN")
							.AddCharge("FSC", 5.5m, "20GP", "GEN")
							.AddCharge("FSC", 45.6m, "40GP", "GEN")
							.AddCharge("BAF", 1.4m, "20GP", "GEN")
							.AddCharge("BAF", 3.0m, "40GP", "GEN")
					},
				}, response);
			}
		}

		public void TestRateSearch_ULD_MultipleContainers_MultipleCostings_WithChargeableOverride()
		{
			//Arrange
			var roundings = new DefaultRoundingsCollection();
			var airRounding = roundings.AddNew();
			airRounding.Code = RatingConstants.RateCategory.AIR;
			airRounding.RoundingType = RatingRoundingTypes.Chargeable;

			var orgRounding = roundings.AddNew();
			orgRounding.Code = RatingConstants.RateCategory.ORG;
			orgRounding.RoundingType = RatingRoundingTypes.Chargeable;

			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				var costing = Helper.NewCosting(TransportProvider1);
				var costingEntry20GP = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUMEL", "USLAX", container: "AAA", removeLines: true);
				var fscAAA = costingEntry20GP.AddUnitRateLine("FSC", 1.1m, "KG", "AUD");
				var bafAAA = costingEntry20GP.AddUnitRateLine("BAF", 1.4m, "CN", "AUD");

				var costingEntry40GP = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUMEL", "USLAX", container: "AKE", removeLines: true);
				var fscAKE = costingEntry40GP.AddUnitRateLine("FSC", 1.2m, "KG", "AUD");
				var bafAKE = costingEntry40GP.AddUnitRateLine("BAF", 1.5m, "CN", "AUD");

				var costingEntryAll = costing.AddRateEntry(RatingConstants.RateCategory.ORG, string.Empty, "AUMEL", "USLAX", removeLines: true);
				costingEntryAll.TI_Mode = "ALL";
				var frtM3 = costingEntryAll.AddUnitRateLine("FRT", 1.3m, "KG", "AUD");
				var frtCN = costingEntryAll.AddUnitRateLine("FRT", 1.6m, "CN", "AUD");
				var ocartAll = costingEntryAll.AddFlatRateLine("OCART", 1.7m, "AUD");

				Factory.Save();

				//Act
				var query = CreateRateQuery("AIR", "ULD", "AUMEL", "USLAX")
					.AddContainer("AAA", "GEN", 1, 1000, 1000, 5)
					.AddContainer("AKE", "GEN", 1, 1000, 1000, 18)
					.AddContainer("AKE", "GEN", 1, 1000, 1000, 20);
				var response = PerformRateSearch(query);

				//Assert
				AssertResponseContainsRates(new List<RateResultDtoAssertion>
				{
					new ()
					{
						RateEntries = [costingEntry20GP, costingEntry40GP, costingEntryAll],
						RateLines = [fscAAA, bafAAA, fscAKE, bafAKE, frtM3, frtCN, ocartAll],
						Charges = new List<RateChargeDtoAssertion>()
							.AddCharge("FSC", 5.5m, "AAA", "GEN")
							.AddCharge("FSC", 45.6m, "AKE", "GEN")
							.AddCharge("BAF", 1.4m, "AAA", "GEN")
							.AddCharge("BAF", 3.0m, "AKE", "GEN")
							.AddCharge("OCART", 1.7m, null, "GEN")
							.AddCharge("FRT", 4.8m, null, "GEN")
							.AddCharge("FRT", 55.9m, null, "GEN")
					},
				}, response);
			}
		}

		public void TestRateSearch_ULD_WithSingleContainerAndChargeableOverride()
		{
			//Arrange
			var roundings = new DefaultRoundingsCollection();
			var rounding = roundings.AddNew();
			rounding.Code = RatingConstants.RateCategory.AIR;
			rounding.RoundingType = RatingRoundingTypes.Chargeable;

			using (DataRegistryRating.Instance.DefaultRounding.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, roundings))
			{
				var costing = Helper.NewCosting(TransportProvider1);
				var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.ULD, "AUMEL", "USLAX", ZString.Empty, "AKE", removeLines: true);
				costingEntry.TI_ContractNumber = "123";
				var fscLine = costingEntry.AddUnitRateLine("FSC", 1.1m, "M3", "AUD");
				var ocartLine = costingEntry.AddUnitRateLine("OCART", 1.2m, "KG", "AUD");
				var frtLine = costingEntry.AddUnitRateLine("FRT", 1.3m, "CN", "AUD");
				var bafLine = costingEntry.AddFlatRateLine("BAF", 1.4m, "AUD");

				Factory.Save();

				//Act
				var query = CreateRateQuery("AIR", "ULD", "AUMEL", "USLAX")
					.AddContainer("AKE", "GEN", 1, 1000, 1000, 80);
				var response = PerformRateSearch(query);

				//Assert
				AssertResponseContainsRates(new List<RateResultDtoAssertion>
				{
					new ()
					{
						RateEntries = [costingEntry],
						RateLines = [ocartLine, bafLine, fscLine, frtLine],
						Charges = new List<RateChargeDtoAssertion>()
							.AddCharge("OCART", 96m, "AKE", "GEN")
							.AddCharge("BAF", 1.4m, "AKE", "GEN")
							.AddCharge("FRT", 1.3m, "AKE", "GEN")
							.AddCharge("FSC", 0m, null, null)
					},
				}, response);
			}
		}

		#endregion

		#region Calculators

		public void TestRateSearch_ReturnsCMBCalculators()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var costingEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, Core.Constants.RateMode.SEA, "AUMEL", "USLAX", ZString.Empty, "20GP", commodity: "GEN", removeLines: true);

			var ocartLine = costingEntry.AddFlatRateLine("OCART", 123, "AUD");

			var cmbLine = costingEntry.AddRateLine("ODOC", CombinedCalculator.Code, "M3");
			cmbLine.TL_RX_NKCurrency = "AUD";

			var calculator = cmbLine.Calculator as BaseCombinedCalculator;
			calculator.UseInclusiveBreaks = true;
			calculator.IsAccumulated = true;
			calculator.BreaksPer = "CTN";

			var minItem = calculator.AddRateLineItem(Calculator.Items.Operator.Minus, 50.0m, 20.0m, 10.0m);
			minItem.TM_BreakWeightVolume = Core.Constants.Weight.Pounds;

			var plusItem = calculator.AddRateLineItem(Calculator.Items.Operator.Plus, 50.0m, 200.0m);
			plusItem.TM_CallForPricing = true;
			plusItem.TM_Text = "Reason";

			Factory.Save();

			//Act
			var query = CreateRateQuery("SEA", "FCL", "AUMEL", "USLAX")
				.AddContainer("20GP", "GEN", 1, 1000, 1000, 80);
			var calculators = PerformRateSearch(query).Calculators;

			//Assert
			var expectedCalculator = new CalculatorDto
			{
				Id = cmbLine.PK.ToGuid(),
				CalculatorCode = "CMB",
				Currency = "AUD",
				Tags = new[] { "Cumulative breaks", "Inclusive breaks", "Breaks per container" },
				WeightVolumeUnit = "M3",
				RateLineItems = new[]
				{
					new RateLineItemDto()
					{
						Operator = Calculator.Items.Operator.Minus,
						Rate = 20.0m,
						Break = 50.0m,
						FlatAmount = 10.0m,
						Restrict = false,
						Reason = "",
						BreakUnit = Core.Constants.Weight.Pounds,
					},
					new RateLineItemDto()
					{
						Operator = Calculator.Items.Operator.Plus,
						Rate = 0.0m,
						Break = 50.0m,
						FlatAmount = 0.0m,
						Restrict = true,
						Reason = "Reason",
						BreakUnit = "",
					}
				}
			};

			Assert("Should only contain CMB calculators.", !calculators.ContainsKey(ocartLine.PK.ToGuid()));

			var calculatorDto = calculators[cmbLine.PK.ToGuid()];
			var rateLineDtoStrings = calculatorDto.RateLineItems.Select(item => item.ToString());

			AssertEquals("Calculator should have correct parameters.", expectedCalculator.ToString(), calculatorDto.ToString());
			expectedCalculator.RateLineItems
				.ForEach(item => Assert($"Should contain rate line items, expected {item}", rateLineDtoStrings.Contains(item.ToString())));
		}

		#endregion
	}
}
