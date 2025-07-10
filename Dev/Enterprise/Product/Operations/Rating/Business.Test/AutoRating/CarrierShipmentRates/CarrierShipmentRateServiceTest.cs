using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Rating.Business.Testing;
using Moq;

namespace Enterprise.Rating.Business.Test.AutoRating.CarrierShipmentRates;

public class CarrierShipmentRateServiceTest : RatingTestCase
{
	#region Query DTO Converter

	public void Test_GivenShipmentIdAndRouteSegmentIds_WhenGettingRates_ThenCallsConverter()
	{
		var shipmentId = Guid.NewGuid();
		Guid[] segmentIds = [Guid.NewGuid(), Guid.NewGuid()];
		var parameters = new CalculateRatesQueryParameters
		{
			ShipmentHeaderId = shipmentId,
			RouteLegIds = segmentIds,
			GetRatesForCosts = true,
			GetRatesForSales = true
		};

		var convertedDto = new CarrierShipmentRateQueryDto();

		var converterMock = new Mock<IRateQueryDtoConverter>();
		converterMock.Setup(x => x.Convert(parameters, It.IsAny<IFactory>())).Returns((convertedDto, []));

		using var disposableSubstitute1 = ObjectFactory.Substitute(converterMock.Object);
		using var disposableSubstitute2 = ObjectFactory.Substitute(Mock.Of<ICarrierShipmentAutoRater>);

		var service = new CarrierShipmentRateService();
		var result = service.GetCarrierShipmentRates(parameters);

		converterMock.Verify(x => x.Convert(parameters, It.IsAny<IFactory>()), Times.Once);
		Assert("This test uses NUnit assertions", true);
	}

	public void Test_GivenConverterHasErrors_WhenGettingRates_ThenReturnsErrorsFromConverter()
	{
		var parameters = GetValidQueryParameters();

		var converterMock = new Mock<IRateQueryDtoConverter>();
		converterMock.Setup(x => x.Convert(It.IsAny<CalculateRatesQueryParameters>(), It.IsAny<IFactory>())).Returns((null, ["Err1", "Err2"]));

		var carrierShipmentAutoRaterMock = new Mock<ICarrierShipmentAutoRater>();

		using var disposableSubstitute1 = ObjectFactory.Substitute(converterMock.Object);
		using var disposableSubstitute2 = ObjectFactory.Substitute(carrierShipmentAutoRaterMock.Object);

		var service = new CarrierShipmentRateService();
		var result = service.GetCarrierShipmentRates(parameters);

		carrierShipmentAutoRaterMock.Verify(
			x => x.AutoRateCarrierShipment(It.IsAny<CarrierShipmentRateQueryDto>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<IRateChooserServices>(), It.IsAny<ILogger>()),
			Times.Never);
		AssertEquals(0, result.Rates.Count);
		AssertEquals(0, result.Logs.Length);
		AssertContainsExactElementsInExactOrder(["Err1", "Err2"], result.Errors);
	}

	#endregion

	#region Auto Rater

	public void Test_GivenValidQueryParametersAndConvertedDto_WhenGettingRates_ThenReturnsResultFromAutoRater()
	{
		CarrierShipmentRateResultDto[] rateResults = [new([], null, CostSell.Cost, "EUR")];
		var carrierShipmentAutoRaterMock = new Mock<ICarrierShipmentAutoRater>();
		carrierShipmentAutoRaterMock.Setup(x => x.AutoRateCarrierShipment(It.IsAny<CarrierShipmentRateQueryDto>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<IRateChooserServices>(), It.IsAny<ILogger>()))
			.Returns(new CarrierShipmentRateResult(rateResults, ["log"], []));

		var parameters = GetValidQueryParameters();
		var rateQueryDto = GetValidRateQueryDto();
		var converterMock = new Mock<IRateQueryDtoConverter>();
		converterMock.Setup(x => x.Convert(It.IsAny<CalculateRatesQueryParameters>(), It.IsAny<IFactory>())).Returns((rateQueryDto, []));

		using var disposableSubstitute1 = ObjectFactory.Substitute(converterMock.Object);
		using var disposableSubstitute2 = ObjectFactory.Substitute(carrierShipmentAutoRaterMock.Object);

		var results = new CarrierShipmentRateService().GetCarrierShipmentRates(parameters);

		carrierShipmentAutoRaterMock.Verify(
			x => x.AutoRateCarrierShipment(rateQueryDto, It.IsAny<BusinessObjectFactory>(), It.IsAny<IRateChooserServices>(), It.IsAny<ILogger>()),
			Times.Once);
		AssertEquals(rateResults, results.Rates);
		AssertContainsExactElementsInExactOrder(["log"], results.Logs);
	}

	public void Test_GivenAutoRaterHasErrors_WhenGettingRates_ThenReturnsErrorsFromAutoRater()
	{
		var carrierShipmentAutoRaterMock = new Mock<ICarrierShipmentAutoRater>();
		carrierShipmentAutoRaterMock.Setup(x => x.AutoRateCarrierShipment(It.IsAny<CarrierShipmentRateQueryDto>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<IRateChooserServices>(), It.IsAny<ILogger>()))
			.Returns(new CarrierShipmentRateResult([], [], ["Err"]));

		var parameters = GetValidQueryParameters();
		var rateQueryDto = GetValidRateQueryDto();
		var converterMock = new Mock<IRateQueryDtoConverter>();
		converterMock.Setup(x => x.Convert(It.IsAny<CalculateRatesQueryParameters>(), It.IsAny<IFactory>())).Returns((rateQueryDto, []));

		using var disposableSubstitute1 = ObjectFactory.Substitute(converterMock.Object);
		using var disposableSubstitute2 = ObjectFactory.Substitute(carrierShipmentAutoRaterMock.Object);

		var results = new CarrierShipmentRateService().GetCarrierShipmentRates(parameters);

		AssertEquals(0, results.Rates.Count);
		Assert(results.Errors.Contains("Err"));
	}

	#endregion

	#region Errors

	public void TestRateSearchValidation_NoRatingsFound_AddNoteToLogButNoError()
	{
		var carrierShipmentAutoRaterMock = new Mock<ICarrierShipmentAutoRater>();
		carrierShipmentAutoRaterMock.Setup(x => x.AutoRateCarrierShipment(It.IsAny<CarrierShipmentRateQueryDto>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<IRateChooserServices>(), It.IsAny<ILogger>()))
			.Returns(new CarrierShipmentRateResult([], [], []));

		var parameters = GetValidQueryParameters();
		var rateQueryDto = GetValidRateQueryDto();
		var converterMock = new Mock<IRateQueryDtoConverter>();
		converterMock.Setup(x => x.Convert(It.IsAny<CalculateRatesQueryParameters>(), It.IsAny<IFactory>())).Returns((rateQueryDto, []));

		using var disposableSubstitute1 = ObjectFactory.Substitute(converterMock.Object);
		using var disposableSubstitute2 = ObjectFactory.Substitute(carrierShipmentAutoRaterMock.Object);

		var results = new CarrierShipmentRateService().GetCarrierShipmentRates(parameters);

		AssertNotNull(results.Logs);
		AssertContainsExactElementsInExactOrder(results.Logs, ["No rates were found for the given query."]);
	}

	public void TestRateQueryDtoValidation_NoCargoInQueryDto() => TestRateQueryDtoValidation_CargoDto([]);
	public void TestRateQueryDtoValidation_CargoIsNullInQueryDto() => TestRateQueryDtoValidation_CargoDto(null);
	void TestRateQueryDtoValidation_CargoDto(CarrierShipmentRateCargoDto[] cargoDtos)
	{
		var parameters = GetValidQueryParameters();
		var rateQueryDto = GetValidRateQueryDto();
		rateQueryDto.Cargo = cargoDtos;
		var converterMock = new Mock<IRateQueryDtoConverter>();
		converterMock.Setup(x => x.Convert(It.IsAny<CalculateRatesQueryParameters>(), It.IsAny<IFactory>())).Returns((rateQueryDto, []));

		using var disposableSubstitute1 = ObjectFactory.Substitute(converterMock.Object);
		using var disposableSubstitute2 = ObjectFactory.Substitute(Mock.Of<ICarrierShipmentAutoRater>());

		var results = new CarrierShipmentRateService().GetCarrierShipmentRates(parameters);

		AssertRatingError(results.Errors, "At least 1 Cargo piece must be provided.");
	}

	public void TestRateQueryDtoValidation_NoRouteLegsInQueryDto() => TestRateQueryDtoValidation_RouteLegsDto([]);
	public void TestRateQueryDtoValidation_RouteLegsIsNullInQueryDto() => TestRateQueryDtoValidation_RouteLegsDto(null);
	public void TestRateQueryDtoValidation_RouteLegsDto(CarrierShipmentRateRouteLegDto[] routeLegDtos)
	{
		var parameters = GetValidQueryParameters();
		var rateQueryDto = GetValidRateQueryDto();
		rateQueryDto.RouteLegs = routeLegDtos;
		var converterMock = new Mock<IRateQueryDtoConverter>();
		converterMock.Setup(x => x.Convert(It.IsAny<CalculateRatesQueryParameters>(), It.IsAny<IFactory>())).Returns((rateQueryDto, []));

		using var disposableSubstitute1 = ObjectFactory.Substitute(converterMock.Object);
		using var disposableSubstitute2 = ObjectFactory.Substitute(Mock.Of<ICarrierShipmentAutoRater>());

		var results = new CarrierShipmentRateService().GetCarrierShipmentRates(parameters);

		AssertRatingError(results.Errors, "At least 1 valid Route Leg must be provided.");
	}

	public void TestQueryParameterValidation_NoShipmentHeaderIdInParameters()
	{
		var parameters = GetValidQueryParameters();
		parameters.ShipmentHeaderId = Guid.Empty;

		using var disposableSubstitute1 = ObjectFactory.Substitute(Mock.Of<IRateQueryDtoConverter>());
		using var disposableSubstitute2 = ObjectFactory.Substitute(Mock.Of<ICarrierShipmentAutoRater>());

		var results = new CarrierShipmentRateService().GetCarrierShipmentRates(parameters);

		AssertRatingError(results.Errors, "The Shipment ID must be provided.");
	}

	public void TestQueryParameterValidation_NoRouteLegIdsInParameters() => TestQueryParameterValidation_RouteLegIds(null);
	public void TestQueryParameterValidation_EmptyRouteLegIdsInParameters() => TestQueryParameterValidation_RouteLegIds([]);
	public void TestQueryParameterValidation_AllEmptyGuidRouteLegIdsInParameters() => TestQueryParameterValidation_RouteLegIds([Guid.Empty, Guid.Empty]);
	void TestQueryParameterValidation_RouteLegIds(Guid[] routeLegIds)
	{
		var parameters = GetValidQueryParameters();
		parameters.RouteLegIds = routeLegIds;

		using var disposableSubstitute1 = ObjectFactory.Substitute(Mock.Of<IRateQueryDtoConverter>());
		using var disposableSubstitute2 = ObjectFactory.Substitute(Mock.Of<ICarrierShipmentAutoRater>());

		var results = new CarrierShipmentRateService().GetCarrierShipmentRates(parameters);

		AssertRatingError(results.Errors, "At least 1 Route Leg ID must be provided.");
	}

	public void TestQueryParameterValidation_NoRateTypeSelected() => TestQueryParameterValidation_RateTypes(false, false, ["You have to select at least one type of rates."]);
	public void TestQueryParameterValidation_BothRateTypesSelected() => TestQueryParameterValidation_RateTypes(true, true, []);
	public void TestQueryParameterValidation_CostsRateTypesSelected() => TestQueryParameterValidation_RateTypes(true, false, []);
	public void TestQueryParameterValidation_SalesRateTypesSelected() => TestQueryParameterValidation_RateTypes(false, true, []);

	void TestQueryParameterValidation_RateTypes(bool getRatesForCosts, bool getRatesForSales, string[] expectedErrors)
	{
		var carrierShipmentAutoRaterMock = new Mock<ICarrierShipmentAutoRater>();
		carrierShipmentAutoRaterMock.Setup(x => x.AutoRateCarrierShipment(It.IsAny<CarrierShipmentRateQueryDto>(), It.IsAny<BusinessObjectFactory>(), It.IsAny<IRateChooserServices>(), It.IsAny<ILogger>()))
			.Returns(new CarrierShipmentRateResult([new([], null, CostSell.Cost, "EUR")], [], []));

		var parameters = GetValidQueryParameters();
		parameters.GetRatesForCosts = getRatesForCosts;
		parameters.GetRatesForSales = getRatesForSales;
		var rateQueryDto = GetValidRateQueryDto();
		var converterMock = new Mock<IRateQueryDtoConverter>();
		converterMock.Setup(x => x.Convert(It.IsAny<CalculateRatesQueryParameters>(), It.IsAny<IFactory>())).Returns((rateQueryDto, []));

		using var disposableSubstitute1 = ObjectFactory.Substitute(converterMock.Object);
		using var disposableSubstitute2 = ObjectFactory.Substitute(carrierShipmentAutoRaterMock.Object);

		var results = new CarrierShipmentRateService().GetCarrierShipmentRates(parameters);

		AssertRatingError(results.Errors, expectedErrors);
	}

	#endregion

	void AssertRatingError(string[] errorList, params string[] expectedErrorMessages)
	{
		AssertNotNull("errorList", errorList);
		AssertContainsExactElementsInExactOrder("Error messages", expectedErrorMessages, errorList);
	}

	CalculateRatesQueryParameters GetValidQueryParameters()
	{
		return new CalculateRatesQueryParameters
		{
			ShipmentHeaderId = Guid.NewGuid(),
			RouteLegIds = [Guid.NewGuid()],
			GetRatesForCosts = true,
			GetRatesForSales = true
		};
	}

	CarrierShipmentRateQueryDto GetValidRateQueryDto()
	{
		return new CarrierShipmentRateQueryDto
		{
			Shipment = new CarrierShipmentRateShipmentDto(),
			Cargo = [new CarrierShipmentRateCargoDto()],
			RouteLegs = [new CarrierShipmentRateRouteLegDto()]
		};
	}
}
