#nullable enable
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.Registry.Business;

namespace Enterprise.Rating.CarrierConnect;

public class AutoratedShipmentCreator(CreateJobDto dto) : AutoratedJobCreator(dto)
{
	public override BusinessObject CreateJob()
	{
		var shipment = Factory.New<ForwardingShipment>();

		shipment.JS_TransportMode = Dto.RateResult.TransportMode;
		shipment.JS_PackingMode = Dto.RateResult.ContainerMode;
		shipment.JS_RL_NKOrigin = Dto.RateResult.Origin;
		shipment.JS_RL_NKDestination = Dto.RateResult.Destination;
		shipment.JS_UnitOfWeight = Env.Registry.FreightWeightUnit;
		shipment.JS_ActualWeight = ContainerTotalWeight;
		shipment.JS_UnitOfVolume = Env.Registry.FreightVolumeUnit;
		shipment.JS_ActualVolume = ContainerTotalVolume;

		// Set the shipment's packline commodity to the first container's commodity if there is only one; If multiple, use the default GEN
		var commodities = Dto.RateQuery.JobInfo.Containers.Select(c => c.Commodity).Distinct().ToList();
		if (commodities.Count == 1)
		{
			shipment.OuterPackLines[0].JL_RH_NKCommodityCode = commodities[0];
		}

		shipment.JS_OA_BookedShippingLineAddress = GetOrgHeader(CarrierOrgCode)!.MainAddress.PK;

		shipment.Numbers.AddNewIfNotExist(
			CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON, Dto.RateResult.CarrierContractNumber);

		if (ContainerTotalChargeable > 0)
		{
			shipment.JS_ActualChargeable = ContainerTotalChargeable;
		}

		AddChargesToShipment(shipment);

		return shipment;
	}

	void AddChargesToShipment(ForwardingShipment shipment)
	{
		using (new JobHeader.Loader(shipment.Factory, shipment).TryCreateWithMutex())
		{
			ChildEditableService.SetState(shipment.Factory, ChildEditableServiceStates.Shipment);
			var job = (Job)shipment.Job;
			var costCollection = job.Charges;

			AddCharges(costCollection);
		}
	}
}

