#nullable enable
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.CarrierConnect;

public class AutoratedConsolCreator(CreateJobDto dto) : AutoratedJobCreator(dto)
{
	public override BusinessObject CreateJob()
	{
		var consol = Factory.New<ForwardingConsol>();
		consol.JK_TransportMode = Dto.RateResult.TransportMode;
		consol.JK_ConsolMode = Dto.RateResult.ContainerMode;
		consol.JK_RL_NKLoadPort = Dto.RateResult.Origin;
		consol.JK_RL_NKDischargePort = Dto.RateResult.Destination;
		consol.AutoratingDate = new ZDate(Dto.RateQuery.EffectiveDate);
		consol.JK_CarrierContractNumber = Dto.RateResult.CarrierContractNumber;
		consol.JK_AWBServiceLevel = Dto.RateResult.CarrierServiceLevel ?? "STD";
		consol.JK_PrepaidCollect = Dto.RateResult.PaymentTerm ?? Env.Registry.ConsolPaymentTerm;

		if (!NewJobShouldHaveContainers)
		{
			consol.JK_RH_NKConsolCommodity = Dto.RateQuery.JobInfo.Containers.First().Commodity;
		}

		consol.JK_OA_ShippingLineAddress = GetOrgHeader(CarrierOrgCode)!.MainAddress.PK;

		if (!string.IsNullOrEmpty(Dto.RateResult.CarrierQuoteNumber))
		{
			consol.Numbers.AddNewIfNotExist(
				CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CQN,
				Dto.RateResult.CarrierQuoteNumber);
		}

		if (Dto.RateResult.NamedAccounts != null && Dto.RateResult.NamedAccounts.Length > 0)
		{
			foreach (var namedAccount in Dto.RateResult.NamedAccounts)
			{
				consol.Numbers.AddNewIfNotExist(
					CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.ContractNamedAccount, namedAccount);
			}
		}

		if (NewJobShouldHaveContainers)
		{
			AddContainers(consol);
		}

		AddShipments(consol);
		AddCharges(consol);

		return consol;
	}

	void AddShipments(ForwardingConsol consol)
	{
		var shipment = consol.Shipments.AddNew();

		shipment.JS_UnitOfWeight = Env.Registry.FreightWeightUnit;
		shipment.JS_ActualWeight = ContainerTotalWeight;
		shipment.JS_UnitOfVolume = Env.Registry.FreightVolumeUnit;
		shipment.JS_ActualVolume = ContainerTotalVolume;

		if (ContainerTotalChargeable > 0)
		{
			shipment.JS_ActualChargeable = ContainerTotalChargeable;
		}
	}

	void AddContainers(ForwardingConsol consol)
	{
		foreach (var containerDto in JobInfo.Containers)
		{
			var container = consol.Containers.AddNew();
			container.JC_RC = GetRefContainer(containerDto.ContainerType)!.PK;
			container.JC_ContainerCount = (ZShort)(containerDto.Count ?? 1);
			container.JC_RH_NKContainerCommodityCode = containerDto.Commodity;

			// When creating a new job from a rate search result, the container quality for each should be the same.
			container.JC_ContainerQuality = Dto.RateResult.PerContainerCommodity.FirstOrDefault()?.ContainerQuality;
		}
	}

	void AddCharges(ForwardingConsol consol)
	{
		var costs = consol.GetApportionments().CostsCollection;

		foreach (var chargeDto in ChargesToApply)
		{
			var cost = costs.TryAddNew();

			cost.E6_AC_ChargeCode = GetChargeCode(chargeDto.ChargeCode.ChargeCode ?? "")?.PK ?? ZGuid.Empty;
			cost.E6_RX_NKCurrency = chargeDto.RateCurrency;
			cost.E6_OSCostAmount = chargeDto.RateAmount;
			cost.CostCalculationDescription = ZBlob.FromUTF8(chargeDto.Description);
			cost.E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge;
		}
	}
}
