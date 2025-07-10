#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.CarrierConnect;

public abstract class AutoratedJobCreator(CreateJobDto dto)
{
	public abstract BusinessObject CreateJob();

	protected BusinessObjectFactory Factory => factory;
	readonly BusinessObjectFactory factory = new ();

	protected CreateJobDto Dto => dto;
	protected JobInfoDto JobInfo => dto.RateQuery.JobInfo;

	protected List<RateChargeDto> ChargesToApply =>
		dto.RateResult.Charges.Where(charge => IsChargeToApply(charge) && ShouldApplyZeroCharges(charge)).ToList();

	bool IsChargeToApply(RateChargeDto charge) => dto.ChargesToApply.Contains(charge.ChargeID);

	bool ShouldApplyZeroCharges(RateChargeDto charge) => (dto.ApplyZeroCharges || charge.RateAmount > 0) && !charge.IsInclusiveCalculator;

	protected bool NewJobShouldHaveContainers { get; } = new[] { ContainerModes.FCL, ContainerModes.ULD }.Contains(dto.RateResult.ContainerMode);

	protected string CarrierOrgCode => !string.IsNullOrEmpty(Dto.RateResult.Carrier?.OrgCode) ? Dto.RateResult.Carrier!.OrgCode : Dto.RateResult.ServiceProvider?.OrgCode!;

	protected string ChargeableUnit => chargeableUnit ??= ChargeableAmountCalculator.GetChargeableUnit(
		Dto.RateResult.TransportMode,
		Env.Registry.FreightWeightUnit,
		Env.Registry.FreightVolumeUnit);
	string? chargeableUnit;

	protected decimal ContainerTotalWeight => CalculateContainerTotal(line => line.Weight ?? 0, line => line.WeightUnit);
	protected decimal ContainerTotalVolume => CalculateContainerTotal(line => line.Volume ?? 0, line => line.VolumeUnit);
	protected decimal ContainerTotalChargeable => CalculateContainerTotal(line => line.ChargeableOverride ?? 0, line => line.ChargeableUnit);

	decimal CalculateContainerTotal(Func<JobPackLineDto, decimal> valueGetter, Func<JobPackLineDto, string> unitGetter)
	{
		var packLines = JobInfo.Containers.Select(container => new { Line = container.PackLines[0], Count = container.Count ?? 0 });
		return Weight.ContainsCode(unitGetter(packLines.First().Line))
			? packLines.Sum(p => Weight.Convert(
					valueGetter(p.Line),
					unitGetter(p.Line) ?? Env.Registry.FreightWeightUnit,
					Env.Registry.FreightWeightUnit) * p.Count)
			: packLines.Sum(p => Volume.Convert(
					valueGetter(p.Line),
					unitGetter(p.Line) ?? Env.Registry.FreightVolumeUnit,
					Env.Registry.FreightVolumeUnit) * p.Count);
	}

	protected void AddCharges(ChargeCollection charges)
	{
		foreach (var chargeDto in ChargesToApply)
		{
			var cost = charges.AddNew();

			cost.JR_AC = GetChargeCode(chargeDto.ChargeCode.ChargeCode ?? "")?.PK ?? ZGuid.Empty;
			cost.JR_RX_NKCostCurrency = chargeDto.RateCurrency;
			cost.JR_OSCostAmt = chargeDto.RateAmount;
			cost.CostCalculationDescription = ZBlob.FromUTF8(chargeDto.Description);
		}
	}

	#region Queries

	protected OrgHeader? GetOrgHeader(string code) =>
		factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, code));

	protected AccChargeCode? GetChargeCode(string code)
	{
		var query = new ZQuery(AccChargeCodeSchema.AC_Code, code)
			.AddToFilter(new ZQuery(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK.ToGuid()));
		return factory.LoadTop1<AccChargeCode>(query);
	}

	protected RefContainer? GetRefContainer(string code) =>
		factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, code));

	#endregion
}
