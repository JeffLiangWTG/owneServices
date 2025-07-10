using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.Rating.Business;
using Moq;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.CarrierShipmentHeader;

internal class OcsRatingControllerTest : TestCaseWithFactory
{
	OcsRatingController controller;
	Mock<ICarrierShipmentRateService> mockService;
	IDisposable disposableServiceSubstitute;

	protected override void SetUp()
	{
		base.SetUp();
		mockService = new Mock<ICarrierShipmentRateService>();
		disposableServiceSubstitute = ObjectFactory.Substitute(mockService.Object);
		controller = new OcsRatingController();
		var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
		controller.ControllerContext = controllerContext;
	}

	protected override void TearDown()
	{
		base.TearDown();

		disposableServiceSubstitute?.Dispose();
		controller?.Dispose();
	}

	public void TestAuthorization_ControllerUsesGlowTicketAuthentication()
	{
		var glowAuthenticationAttribute = controller.GetType()
			.GetCustomAttributes(typeof(GlowTicketAuthenticationAttribute), inherit: true).FirstOrDefault() as GlowTicketAuthenticationAttribute;

		AssertNotNull(glowAuthenticationAttribute);
	}

	public void TestRateCalculation_MissingQueryObject_ReturnsBadRequestWithMessage()
	{
		var response = controller.CalculateRates(null);

		response.AssertResultContains(HttpStatusCode.BadRequest, "Please provide parameters for the rate calculation.");
	}

	public void TestRateCalculation_ValidParameters_ReturnsValidRateResultFromService()
	{
		CarrierShipmentRateChargeDto[] charges = [new CarrierShipmentRateChargeDto()];
		CarrierShipmentRateResultDto[] rates = [new CarrierShipmentRateResultDto(charges, new CarrierShipmentRateCriteriaDto(), CostSell.Cost, "EUR")];
		string[] logs = [];
		string[] errors = [];

		var queryParameters = GetValidCalculateRatesQueryParameters();
		mockService.Setup(x => x.GetCarrierShipmentRates(queryParameters)).Returns(new CarrierShipmentRateResult(rates, logs, errors));

		var response = controller.CalculateRates(queryParameters);

		mockService.Verify(x => x.GetCarrierShipmentRates(queryParameters), Times.Once);

		response.AssertResultContains(HttpStatusCode.OK);

		AssertEquals(response.GetType(), typeof(JsonResult<OcsRatingResponse>));
		var jsonResponse = (JsonResult<OcsRatingResponse>)response;
		var content = jsonResponse.Content;

		AssertNotNull(content);
		AssertEquals(1, content.Rates.Count());
	}

	public void TestRateCalculation_GivenConvertReturnedErrors_ThenReturnsBadRequestWithMessage()
	{
		var queryParameters = GetValidCalculateRatesQueryParameters();
		mockService.Setup(x => x.GetCarrierShipmentRates(queryParameters)).Returns(new CarrierShipmentRateResult([], [], ["Test error"]));

		var response = controller.CalculateRates(queryParameters);

		response.AssertResultContains(HttpStatusCode.BadRequest, "Test error");
	}

	CalculateRatesQueryParameters GetValidCalculateRatesQueryParameters()
	{
		return new CalculateRatesQueryParameters
		{
			ShipmentHeaderId = Guid.NewGuid(),
			RouteLegIds = [Guid.NewGuid(), Guid.NewGuid()],
			GetRatesForCosts = true,
		};
	}
}
