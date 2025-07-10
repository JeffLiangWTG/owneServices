using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using CargoWise.Data;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost;

[GlowTicketAuthentication]
[RoutePrefix("api/rating/utilities")]
public class RatingUtilitiesController : ApiController
{
	[HttpPost]
	[Route("calculate-chargeable")]
	[ResponseType(typeof(UnitAmountDto))]
	public IHttpActionResult CalculateChargeable([FromBody] ChargeableParametersDto parameters)
	{
		if (parameters is null)
		{
			return BadRequest();
		}
		if (string.IsNullOrEmpty(parameters.WeightUnit) && string.IsNullOrEmpty(parameters.VolumeUnit))
		{
			return BadRequest((NoResString)"Weight or volume unit is required");
		}

		if (!Core.Constants.Volume.ContainsCode(parameters.VolumeUnit))
		{
			return BadRequest($"Invalid volume unit: {parameters.VolumeUnit}");
		}

		if (!Core.Constants.Weight.ContainsCode(parameters.WeightUnit))
		{
			return BadRequest($"Invalid weight unit: {parameters.WeightUnit}");
		}

		using (Db.DisposableActionForDbConnection())
		using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
		{
			var targetUnit = ChargeableAmountCalculator.GetChargeableUnit(parameters.TransportMode, parameters.WeightUnit, parameters.VolumeUnit);
			var chargeableAmount = ChargeableAmountCalculator.CalculateChargeable(new ChargeableParameters
			{
				Weight = new ZWeight(parameters.Weight ?? 0, parameters.WeightUnit),
				Volume = new ZVolume(parameters.Volume ?? 0, parameters.VolumeUnit),
				TargetUnit = targetUnit,
				ConversionFactors = ChargeableAmountCalculator.GetDefaultConversionFactors(parameters.IsDomestic, parameters.TransportMode, targetUnit)
			}).Chargeable.Amount;

			if (parameters.TransportMode == "AIR")
			{
				chargeableAmount = ChargeableWeightRoundingHelper.GetRoundedValueAir(null, chargeableAmount);
			}

			return Json(new UnitAmountDto
			{
				Amount = chargeableAmount,
				Unit = targetUnit,
			});
		}
	}

	[HttpPost]
	[Route("calculate-actual")]
	[ResponseType(typeof(UnitAmountDto))]
	public IHttpActionResult CalculateActual([FromBody] CalculateActualValueDto dto)
	{
		if (dto is null)
		{
			return BadRequest();
		}
		if (string.IsNullOrEmpty(dto.WeightUnit) && string.IsNullOrEmpty(dto.VolumeUnit))
		{
			return BadRequest((NoResString)"Weight or volume unit is required");
		}

		var actualWeight = new ZWeight(dto.Weight ?? 0, dto.WeightUnit);
		var actualVolume = new ZVolume(dto.Volume ?? 0, dto.VolumeUnit);

		if (FreightDataRegistry.Instance.VolumeChargableTransportModes.Contains(dto.TransportMode))
		{
			if (!Core.Constants.Volume.ContainsCode(dto.ChargeableUnit))
			{
				return BadRequest($"Transport mode {dto.TransportMode} expects volume chargeable unit, got {dto.ChargeableUnit}");
			}

			if (!actualVolume.IsValid)
			{
				return BadRequest($"Transport mode {dto.TransportMode} expects valid volume unit, got {dto.VolumeUnit}");
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = ChargeableAmountCalculator.GetActualFromChargeable(
				 dto.TransportMode,
				 dto.IsDomestic,
				 new ZVolume(dto.ChargeableAmount ?? 0, dto.ChargeableUnit),
				 actualWeight,
				 actualVolume) ?? actualWeight;

				return Json(new UnitAmountDto
				{
					Amount = result.Amount,
					Unit = result.Unit
				});
			}
		}

		if (FreightDataRegistry.Instance.WeightChargableTransportModes.Contains(dto.TransportMode))
		{
			if (!Core.Constants.Weight.ContainsCode(dto.ChargeableUnit))
			{
				return BadRequest($"Transport mode {dto.TransportMode} expects weight chargeable unit, got {dto.ChargeableUnit}");
			}

			if (!actualWeight.IsValid)
			{
				return BadRequest($"Transport mode {dto.TransportMode} expects valid weight unit, got {dto.WeightUnit}");
			}

			using (Db.DisposableActionForDbConnection())
			using (GlowUserContextSwitcher.SetUserContextBasedOnHttp(ControllerContext))
			{
				var result = ChargeableAmountCalculator.GetActualFromChargeable(
				 dto.TransportMode,
				 dto.IsDomestic,
				 new ZWeight(dto.ChargeableAmount ?? 0, dto.ChargeableUnit),
				 actualWeight,
				 actualVolume) ?? actualVolume;

				return Json(new UnitAmountDto
				{
					Amount = result.Amount,
					Unit = result.Unit
				});
			}
		}

		return BadRequest($"Cannot calculate actual amount for transport mode {dto.TransportMode}");
	}
}
