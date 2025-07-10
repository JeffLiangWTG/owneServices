using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;

namespace Enterprise.Rating.CarrierConnect.Test;

public static class JobTestHelpers
{
	#region Dto Helpers

	public static OrgHeader CreateOrgHeader(string code, string name)
	{
		var factory = new BusinessObjectFactory();

		var org = factory.New<OrgHeader>();
		org.OH_Code = code;
		org.OH_FullName = name;

		// Forces creation of MainAddress. This assures that all references to `MainAddress` henceforth refer to the same address.
		_ = org.MainAddress;

		factory.Save();
		return org;
	}

	public static JobContainerDto CreateContainerDto(
		string containerType,
		decimal weight = 0,
		string weightUnit = "KG",
		decimal volume = 0,
		string volumeUnit = "M3",
		int count = 1,
		string commodity = "GEN",
		string chargeableUnit = "",
		decimal chargeableOverride = 0) =>
		new ()
		{
			Commodity = commodity,
			ContainerType = containerType,
			Count = count,
			PackLines = [
				new JobPackLineDto
				{
					Weight = weight,
					Volume = volume,
					VolumeUnit = volumeUnit,
					WeightUnit = weightUnit,
					Commodity = commodity,
					PackageType = "PKG",
					ChargeableOverride = chargeableOverride,
					ChargeableUnit = chargeableUnit,
				}
			]
		};

	public static RateChargeDto CreateChargeDto(
		string chargeCode,
		decimal amount,
		string description,
		string currency) =>
		new ()
		{
			ChargeID = Guid.NewGuid(),
			ChargeCode = new () {
				ChargeCode = chargeCode,
			},
			RateAmount = amount,
			Description = description,
			RateCurrency = currency
		};

	public static CreateJobDto CreateJobDto(OrgHeader orgHeader, ZDate effectiveOn, RateChargeDto appliedCharge = null, JobContainerDto container1 = null, JobContainerDto container2 = null)
	{
		var	container1Dto = container1 ?? CreateContainerDto("20GP", count: 2, weight: 5000, volume: 20, chargeableOverride: 5, chargeableUnit: Core.Constants.Volume.CubicMetres);
		// 35.315 CubicFeet ~= 1 CubicMeter
		var container2Dto = container2 ?? CreateContainerDto("40GP", count: 3, weight: 2000, volume: 30, chargeableOverride: 35.315m, chargeableUnit: Core.Constants.Volume.CubicFeet, commodity: "ATC");

		var charge1 = appliedCharge ?? CreateChargeDto("FRT", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var charge2 = CreateChargeDto("ODOC", 100, "Description", Core.Constants.CurrencyCodes.UnitedStates);
		var dto = new CreateJobDto
		{
			RateQuery = new RateQueryDto
			{
				EffectiveDate = effectiveOn.ToDateTime(),
				JobInfo = new JobInfoDto { Containers = [container1Dto, container2Dto] }
			},
			RateResult = new RateResultDto
			{
				Origin = "AUSYD",
				Destination = "USLAX",
				TransportMode = Core.Constants.TransportModes.Sea,
				ContainerMode = Core.Constants.ContainerModes.FCL,
				ServiceProvider = new(orgHeader),
				CarrierContractNumber = "123",
				CarrierQuoteNumber = "567",
				NamedAccounts = new string[] { "234" },
				PaymentTerm = Core.Constants.PaymentType.Prepaid,
				Charges = [charge1, charge2],
			},
			ChargesToApply = [charge1.ChargeID]
		};

		return dto;
	}

	#endregion
}
