using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost.Tests.WebAPI.Controllers.Rating;

public class RatingUtilitiesControllerTest : TestCaseWithFactory
{
	public void TestCalculateChargeable_SEA_ValidParameters_ReturnsChargeableResponse()
	{
		var parameters = new ChargeableParametersDto
		{
			TransportMode = "SEA",
			WeightUnit = "KG",
			VolumeUnit = "M3",
			Weight = 100,
			Volume = 10,
			IsDomestic = true
		};
		var response = controller.CalculateChargeable(parameters);
		response.AssertResultContains(HttpStatusCode.OK);

		AssertEquals(response.GetType(), typeof(JsonResult<UnitAmountDto>));
		var jsonResponse = (JsonResult<UnitAmountDto>)response;
		var content = jsonResponse.Content;
		AssertEquals(10, Decimal.ToInt32(content.Amount));
		AssertEquals("M3", content.Unit);
	}

	public void TestCalculateChargeable_AIR_ValidParameters_ReturnsChargeableResponse()
	{
		var parameters = new ChargeableParametersDto
		{
			TransportMode = "AIR",
			WeightUnit = "KG",
			VolumeUnit = "M3",
			Weight = 10,
			Volume = 1,
			IsDomestic = true
		};

		var response = controller.CalculateChargeable(parameters);
		var jsonResponse = (JsonResult<UnitAmountDto>)response;
		var content = jsonResponse.Content;
		AssertEquals("No registry", 166.667m, content.Amount);

		ChargeableWeightRoundingCollection registryEntry = FreightDataRegistry.Instance.FreightChargeableWeightRoundings.Value;
		registryEntry[0].RoundingMode = nameof(ChargeableWeightRoundingType.Down);
		registryEntry[0].RoundingScale = ChargeableWeightRoundingScales.Scale05;
		FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEntry);
		response = controller.CalculateChargeable(parameters);
		jsonResponse = (JsonResult<UnitAmountDto>)response;
		content = jsonResponse.Content;
		AssertEquals("Based on registry, should rounding down 0.5", 166.5m, content.Amount);

		registryEntry[0].RoundingMode = nameof(ChargeableWeightRoundingType.Up);
		FreightDataRegistry.Instance.FreightChargeableWeightRoundings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryEntry);
		response = controller.CalculateChargeable(parameters);
		jsonResponse = (JsonResult<UnitAmountDto>)response;
		content = jsonResponse.Content;
		AssertEquals("Based on registry, should rounding up 0.5", 167.0m, content.Amount);
	}

	public void TestCalculateChargeableValidation_NoParameters()
	{
		var response = controller.CalculateChargeable(null);
		response.AssertResultContains(HttpStatusCode.BadRequest);
	}

	public void TestCalculateActual_ReturnsActualValue_WhenCalculationIsNull() =>
		AssertActualValue(
			new()
			{
				TransportMode = "AIR",
				WeightUnit = "KG",
				VolumeUnit = "M3",
				Weight = 50,
				Volume = 40,
				IsDomestic = true,
				ChargeableAmount = 20,
				ChargeableUnit = "KG",
			},
			new()
			{
				Amount = 40,
				Unit = "M3",
			});

	public void TestCalculateActual_ReturnsActualValue_WhenCalculationIsSuccessful() =>
		AssertActualValue(
			new()
			{
				TransportMode = "AIR",
				WeightUnit = "KG",
				VolumeUnit = "M3",
				Weight = 10,
				Volume = 40,
				IsDomestic = true,
				ChargeableAmount = 20,
				ChargeableUnit = "KG",
			},
			new()
			{
				Amount = 0.12m,
				Unit = "M3",
			});

	public void AssertActualValue(CalculateActualValueDto dto, UnitAmountDto expected)
	{
		var response = controller.CalculateActual(dto);
		response.AssertResultContains(HttpStatusCode.OK);

		AssertEquals(response.GetType(), typeof(JsonResult<UnitAmountDto>));
		var jsonResponse = (JsonResult<UnitAmountDto>)response;
		var content = jsonResponse.Content;
		AssertEquals(expected.Amount, content.Amount);
		AssertEquals(expected.Unit, content.Unit);
	}

	public void TestCalculateActualValidation_NoParameters()
	{
		var response = controller.CalculateActual(null);
		response.AssertResultContains(HttpStatusCode.BadRequest);
	}

	public void TestCalculateActualValidation_WhenIncorrectChargeableUnit_AndTransporeModeAIR()
	{
		var response = controller.CalculateActual(new()
		{
			TransportMode = "AIR",
			WeightUnit = "KG",
			VolumeUnit = "M3",
			Weight = 10,
			Volume = 40,
			IsDomestic = true,
			ChargeableAmount = 20,
			ChargeableUnit = "M3",
		});
		response.AssertResultContains(HttpStatusCode.BadRequest, "Transport mode AIR expects weight chargeable unit, got M3");
	}

	public void TestCalculateActualValidation_WhenIncorrectChargeableUnit_AndTransporeModeSEA()
	{
		var response = controller.CalculateActual(new()
		{
			TransportMode = "SEA",
			WeightUnit = "KG",
			VolumeUnit = "M3",
			Weight = 10,
			Volume = 40,
			IsDomestic = true,
			ChargeableAmount = 20,
			ChargeableUnit = "KG",
		});
		response.AssertResultContains(HttpStatusCode.BadRequest, "Transport mode SEA expects volume chargeable unit, got KG");
	}

	public void TestCalculateChargeableValidation_WhenWeightUnitAndVolumeUnitAllEmpty()
	{
		var response = controller.CalculateChargeable(new()
		{
			TransportMode = "AIR",
			WeightUnit = "",
			VolumeUnit = ""
		});
		response.AssertResultContains(HttpStatusCode.BadRequest, "Weight or volume unit is required");
	}

	public void TestCalculateChargeableValidation_WhenIncorrectWeightUnit()
	{
		var response = controller.CalculateChargeable(new()
		{
			TransportMode = "AIR",
			WeightUnit = "InvalidUnit",
			VolumeUnit = "M3"
		});
		response.AssertResultContains(HttpStatusCode.BadRequest, "Invalid weight unit: InvalidUnit");
	}

	public void TestCalculateChargeableValidation_WhenIncorrectVolumeUnit()
	{
		var response = controller.CalculateChargeable(new()
		{
			TransportMode = "AIR",
			WeightUnit = "MC",
			VolumeUnit = "InvalidUnit"
		});
		response.AssertResultContains(HttpStatusCode.BadRequest, "Invalid volume unit: InvalidUnit");
	}

	public void TestCalculateActualValidation_WhenWeightUnitAndVolumeUnitAllEmpty()
	{
		var response = controller.CalculateActual(new()
		{
			TransportMode = "AIR",
			WeightUnit = "",
			VolumeUnit = ""
		});
		response.AssertResultContains(HttpStatusCode.BadRequest, "Weight or volume unit is required");
	}

	public void TestCalculateActualValidation_WhenIncorrectWeightUnit_AndTransportModeExpectsWeight()
	{
		var response = controller.CalculateActual(new()
		{
			TransportMode = "AIR",
			WeightUnit = "InvalidUnit",
			ChargeableUnit = "MC",
		});
		response.AssertResultContains(HttpStatusCode.BadRequest, "Transport mode AIR expects valid weight unit, got InvalidUnit");
	}

	public void TestCalculateActualValidation_WhenIncorrectVolumeUnit_AndTransportModeExpectsVolume()
	{
		var response = controller.CalculateActual(new()
		{
			TransportMode = "SEA",
			VolumeUnit = "InvalidUnit",
			ChargeableUnit = "M3",
		});
		response.AssertResultContains(HttpStatusCode.BadRequest, "Transport mode SEA expects valid volume unit, got InvalidUnit");
	}

	public void TestCalculateChargeable_ShouldUseGlowUserContext()
	{
		var parameters = new ChargeableParametersDto
		{
			TransportMode = "AIR",
			WeightUnit = "KG",
			VolumeUnit = "M3",
			Weight = 1,
			Volume = 10,
			IsDomestic = false
		};
		var response = controller.CalculateChargeable(parameters);
		response.AssertResultEquals(HttpStatusCode.OK);

		var jsonResponse = (JsonResult<UnitAmountDto>)response;
		var content = jsonResponse.Content;
		AssertEquals(5m, content.Amount); // 10m3 / (2 m3/kg) = 5kg
	}

	public void TestCalculateActual_ShouldUseGlowUserContext_WhenCalculatingWeight() =>
		AssertActualValue(
			new()
			{
				TransportMode = "AIR",
				WeightUnit = "KG",
				VolumeUnit = "M3",
				Weight = 15,
				Volume = 1,
				IsDomestic = false,
				ChargeableAmount = 20,
				ChargeableUnit = "KG",
			},
			new()
			{
				Amount = 40m, // 20 kg * 2 m3/kg = 40 m3
				Unit = "M3",
			});

	public void TestCalculateActual_ShouldUseGlowUserContext_WhenCalculatingVolume() =>
		AssertActualValue(
			new()
			{
				TransportMode = "SEA",
				WeightUnit = "KG",
				VolumeUnit = "M3",
				Weight = 15,
				Volume = 1,
				IsDomestic = false,
				ChargeableAmount = 20,
				ChargeableUnit = "M3",
			},
			new()
			{
				Amount = 60m, // 20 m3 * 3 kg/m3 = 60 kg
				Unit = "KG",
			});

	protected override void SetUp()
	{
		base.SetUp();

		controller = new RatingUtilitiesController();
		var controllerContext = new HttpControllerContext(new HttpConfiguration(), new HttpRouteData(new HttpRoute()), new HttpRequestMessage());
		controllerContext.Controller = controller;
		controller.ControllerContext = controllerContext;

		var newUser = Factory.NewWithValidTestData<GlbStaff>();
		var branch = Factory.NewWithValidTestData<GlbBranch>();
		var department = Factory.NewWithValidTestData<GlbDepartment>();

		newUser.GS_LoginName = "User";
		newUser.GS_GB_HomeBranch = branch.PK;
		newUser.GS_GE_HomeDepartment = department.PK;

		Factory.Save();

		var companyPK = branch.Company.PK.ToGuid();
		var companyConversionFactor = new ConversionFactor(2m, "M3", "KG"); // 2 m3/kg
		var companyChargeableFactor = new ChargeableFactor(companyConversionFactor, companyConversionFactor);
		FreightDataRegistry.Instance.InternationalChargeableFactorAir.SetValue(companyPK, Guid.Empty, Guid.Empty, companyChargeableFactor);

		var companyConversionFactorSea = new ConversionFactor(3m, "KG", "M3"); // 3 kg/m3
		var companyChargeableFactorSea = new ChargeableFactor(companyConversionFactorSea, companyConversionFactorSea);
		FreightDataRegistry.Instance.InternationalChargeableFactorSea.SetValue(companyPK, Guid.Empty, Guid.Empty, companyChargeableFactorSea);

		GlowTicketTestHelper.SetUpStaffPrincipal(controller, newUser, branch.PK.ToGuid(), department.PK.ToGuid());
	}

	protected override void TearDown()
	{
		base.TearDown();

		controller?.Dispose();
	}

	RatingUtilitiesController controller;
}
