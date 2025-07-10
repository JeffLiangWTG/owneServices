using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Test;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Urs.Api.Integration.DTOs;
using Urs.Api.Integration.Interfaces;
using static Enterprise.Rating.Business.RatingUsageCollector;
using static Enterprise.Rating.Business.UrsConstants;
using Model = WiseRates.Api.Model;

namespace Enterprise.Rating.CarrierConnect;

public class UrsHelper(BusinessObjectFactory factory, TestHelper testHelper)
{
	TradeServiceDto GetDefaultTradeService()
	{
		var tradeService = new TradeServiceDto();
		tradeService.CreateRoute().AddWaypoint("AUMEL", RouteWaypointCode.Origin).AddWaypoint("USLAX", RouteWaypointCode.Destination);
		return tradeService;
	}

	public UrsRateLine CreateUrsLine(
		string chargeCode,
		string weightVolume,
		string currency,
		decimal amount,
		string calculatorCode,
		IBaseChargeDto baseChargeDto = null,
		ITradeServiceDto tradeServiceDto = null)
	{
		var ursCharge = new UrsCharge(baseChargeDto ?? new BaseChargeDto(), tradeServiceDto ?? GetDefaultTradeService());
		var rateLine = new UrsRateLine(factory, ursCharge, Model.ChargeType.None)
		{
			TL_AC = testHelper.ChargeCodes[chargeCode].PK,
			TL_WeightVolume = weightVolume,
			TL_RX_NKCurrency = currency,
			RateCalculatorType = CalculatorType.Unit,
		};

		rateLine.ChildRateLineItems = new[]
		{
			new WiseLineItem(rateLine, Calculator.Items.Operator.UNT, string.Empty, amount, ZString.Empty, 0m, 0m, null)
		};

		return rateLine;
	}

	public UrsRateEntry CreateUrsEntry(string mode, string category, string origin, string destination, string commodityCode, string rateProvider, ZDate startDate, ZDate endDate, UrsBookingInfo bookingInfo = null, string commodityGroup = null, TradeServiceDto tradeService = null, params WiseLine[] rateLines)
	{
		var entry = new UrsRateEntry(tradeService ?? GetDefaultTradeService(), factory)
		{
			TI_Mode = mode,
			TI_RateCategory = category,
			TI_OriginLRC = origin,
			TI_DestinationLRC = destination,
			RateProvider = rateProvider,
			TI_RateStartDate = startDate,
			TI_RateEndDate = endDate,
			ChildRateLines = rateLines,
			TI_RH_NKCommodityCode = commodityCode,
			CommodityGroup = commodityGroup,
			BookingInfo = bookingInfo,
			NamedAccounts = []
		};

		rateLines.ForEach(line => line.ParentRateEntry = entry);

		return entry;
	}

	public UrsRateEntry CreateUrsEntryWithContainer(string containerCode, string transportMode = "SEA")
	{
		var ursLine = CreateUrsLine("FRT", "CN", "AUD", 5m, "UNT");
		var ursEntry = CreateUrsEntry(
			mode: transportMode,
			category: "FCL",
			origin: "AUSYD",
			destination: "UAIEV",
			commodityCode: "GEN",
			rateProvider: "URS",
			startDate: new ZDate(2020, 06, 06),
			endDate: new ZDate(2030, 06, 06),
			rateLines: ursLine
		);
		var container = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerCode);
		ursEntry.TI_RC = container.PK;
		ursEntry.ChildRateLines = new[] { ursLine };
		return ursEntry;
	}

	public (UrsRateEntry entry, WiseLine line) CreateUrsEntryAndLine(string id, string category, string chargeCode, string commodityCode = "GEN", string contractNumber = default, string containerCode = default, Action<WiseEntry> updateEntry = null)
	{
		var line = CreateUrsLine(chargeCode, "CN", "AUD", 5m, "UNT");

		var tradeService = GetDefaultTradeService();
		tradeService.Key = id;

		var entry = new UrsRateEntry(tradeService, factory)
		{
			TI_Mode = "SEA",
			TI_RateCategory = category,
			TI_OriginLRC = "AUMEL",
			TI_DestinationLRC = "USLAX",
			TI_RH_NKCommodityCode = commodityCode,
			RateProvider = "URS",
			TI_RateStartDate = new ZDate(2020, 06, 06),
			TI_RateEndDate = new ZDate(2030, 06, 06),
			TI_ContractNumber = contractNumber,
			ChildRateLines = [line],
			NamedAccounts = [],
			UrsContainer = new UrsContainer
			{
				Code = containerCode,
				ContainerPKs = []
			}
		};

		if (containerCode != default)
		{
			var container = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerCode);
			if (container != null)
			{
				entry.TI_RC = container.PK;
				entry.UrsContainer = new UrsContainer
				{
					Code = containerCode,
					ContainerPKs = [container.PK]
				};
			}
		}

		updateEntry?.Invoke(entry);

		return (entry, line);
	}

	public Mock<IRateSelectorProvider> MockProvider(IRateEntry entry, RateProvider provider = RateProvider.URS) =>
		MockProvider([entry], provider);

	public Mock<IRateSelectorProvider> MockProvider(List<IRateEntry> entries, RateProvider provider = RateProvider.URS)
	{
		var mockProvider = new Mock<IRateSelectorProvider>();
		mockProvider
				.Setup(p => p.GetRatesAsync(It.IsAny<RatingCriteria>(), It.IsAny<RateQueryBusinessObject>(), It.IsAny<string>()))
				.Returns(() => Task.FromResult(entries));
		mockProvider.Setup(p => p.Provider).Returns(provider);
		return mockProvider;
	}

	public Mock<IRateSelectorProviderFactory> MockProviderFactory(Mock<IRateSelectorProvider> mockProvider) =>
		MockProviderFactory([mockProvider]);

	public Mock<IRateSelectorProviderFactory> MockProviderFactory(List<Mock<IRateSelectorProvider>> mockProviders)
	{
		var mockProviderFactory = new Mock<IRateSelectorProviderFactory>();
		mockProviderFactory
			.Setup(f => f.CreateProviders(It.IsAny<LoggerDecorator>(), It.IsAny<BusinessObjectFactory>()))
			.Returns(mockProviders.Select(x => x.Object));
		return mockProviderFactory;
	}
}
