using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Constants = Enterprise.Core.Constants;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Universal.CO2e
{
	public sealed class RelatedConsolCO2eRequestDataObjectWriter : CO2eLegBasedRequestDataObjectWriter<ForwardingConsol, Transport>
	{
		readonly ForwardingShipment shipment;
		readonly IPrePostCarriageLocation lastPostCarriageLocation;
		readonly ZString preCarriageHblDeliveryMode;
		readonly ZString postCarriageHblDeliveryMode;
		readonly ZBool excludePreCarriageLegsIfHasTransportBooking;
		readonly ZBool excludePostCarriageLegsIfHasTransportBooking;

		public RelatedConsolCO2eRequestDataObjectWriter(ForwardingShipment shipment,
			IPrePostCarriageLocation lastPostCarriageLocation,
			ZString preCarriageHblDeliveryMode,
			ZString postCarriageHblDeliveryMode,
			ZBool excludePreCarriageLegsIfHasTransportBooking,
			ZBool excludePostCarriageLegsIfHasTransportBooking,
			IDataWritingManager writeManager)
			: base(writeManager)
		{
			this.shipment = shipment;
			this.lastPostCarriageLocation = lastPostCarriageLocation;
			this.preCarriageHblDeliveryMode = preCarriageHblDeliveryMode;
			this.postCarriageHblDeliveryMode = postCarriageHblDeliveryMode;
			this.excludePreCarriageLegsIfHasTransportBooking = excludePreCarriageLegsIfHasTransportBooking;
			this.excludePostCarriageLegsIfHasTransportBooking = excludePostCarriageLegsIfHasTransportBooking;
		}

		protected override void PopulateDataObject(ForwardingConsol consol, UniversalShipment shipmentData)
		{
			base.PopulateDataObject(consol, shipmentData);
			PopulateEmptyContainers(shipmentData, consol);
		}

		protected override void PopulateWeightAndTEU(UniversalShipment shipmentData, ForwardingConsol consol)
		{
			var cO2eSupporter = consol as ICO2eLegBasedSupporter;

			var weight = GetPackLinesWeightPackedToConsolFallbackToShipmentWeight(consol, cO2eSupporter.UnitOfWeight);
			shipmentData.TotalWeight = weight;
			shipmentData.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(cO2eSupporter.UnitOfWeight, BindToLists.GetCachedLists(consol.Factory).WeightUnits);

			if (cO2eSupporter.IncludeTEU && ShipmentHasPackLinesPackedIntoConsol(consol))
			{
				shipmentData.TEU = GetConsolTEUForShipment(consol, weight, cO2eSupporter.UnitOfWeight);
			}
		}

		bool ShipmentHasPackLinesPackedIntoConsol(ForwardingConsol consol)
		{
			return shipment.OuterPackLines.Cast<ForwardingPackLine>().Any(p => p.GetContainer(consol) != null);
		}

		ZDecimal GetPackLinesWeightPackedToConsolFallbackToShipmentWeight(ForwardingConsol consol, ZString uow)
		{
			if (ShipmentHasPackLinesPackedIntoConsol(consol))
			{
				return shipment.OuterPackLines.Cast<ForwardingPackLine>()
					.Where(p => p.GetContainer(consol) != null)
					.Sum(p => Constants.Weight.ConvertSafe(p.JL_ActualWeight, p.JL_ActualWeightUQ, uow, false));
			}

			return Constants.Weight.ConvertSafe(shipment.JS_ActualWeight, shipment.JS_UnitOfWeight, uow, false);
		}

		TEU GetConsolTEUForShipment(ForwardingConsol consol, ZDecimal weight, ZString uow)
		{
			var weightInTonne = Constants.Weight.ConvertSafe(weight, uow, Constants.Weight.Tonnes);
			var teuCount = shipment.GetNumberOfTEUForConsol(consol);

			return new TEU
			{
				NumberOfTEU = teuCount,
				TonnesPerTEU = teuCount == 0 ? 0 : weightInTonne / teuCount,
				ContainerEmptyWeightPerTEU = GetContainerEmptyWeightPerTEU(consol),
				ContainerEmptyWeightPerTEUUnit = ListHelper.GetWithDescription<UnitOfWeight>(Constants.Weight.Kilograms, BindToLists.GetCachedLists(consol.Factory).WeightUnits)
			};
		}

		ZDecimal GetContainerEmptyWeightPerTEU(ForwardingConsol consol)
		{
			var containersForShipment = consol.Containers
				.Where(c => shipment.OuterPackLines.Cast<ForwardingPackLine>()
					.Any(p => (p.GetContainer(consol)?.PK ?? ZGuid.Empty) == c.PK))
				.ToArray();
			var emptyWeightSum = containersForShipment.Sum(c => Constants.Weight.Convert(c.JC_Calc_TareWeight, c.ContainerWeightUnit, Constants.Weight.Kilograms));
			var containerTEUSum = containersForShipment.Sum(c => c.JC_Calc_TEUCount);

			return containerTEUSum == 0 ? 0 : emptyWeightSum / containerTEUSum;
		}

		void PopulateEmptyContainers(UniversalShipment shipmentData, ForwardingConsol consol)
		{
			var consolEmptyContainers = consol.Containers
				.Where(c => ((ICO2eLegBasedSupporter)shipment).EmptyContainers
					.Any(s => s.Container?.ContainerJobID.Equals(c.JC_ContainerJobID) ?? false));
			shipmentData.SetEmptyContainerCollection(() =>
				ProcessCollection(consolEmptyContainers, new EmptyContainerDataObjectWriter(shipment, writeManager), CollectionContent.Complete));
		}

		#region Pre/Post Carriage

		protected override bool IncludePrePostCarriageLegs(ForwardingConsol sourceBO) => (shipment as ICO2ePrePostCarriage).RequiresPrePostCarriageLegs;

		protected override IEnumerable<PrePostCarriageLegWrapper> GetPreCarriageLegs(ForwardingConsol sourceBO)
		{
			if (excludePreCarriageLegsIfHasTransportBooking && sourceBO.HasTransportBooking(DtbBookingDirection.PIC))
			{
				return Enumerable.Empty<PrePostCarriageLegWrapper>();
			}

			return sourceBO.GetPreCarriageLegs(preCarriageHblDeliveryMode);
		}

		protected override IEnumerable<PrePostCarriageLegWrapper> GetPostCarriageLegs(ForwardingConsol sourceBO)
		{
			if (excludePostCarriageLegsIfHasTransportBooking && sourceBO.HasTransportBooking(DtbBookingDirection.DLV))
			{
				return Enumerable.Empty<PrePostCarriageLegWrapper>();
			}

			var result = sourceBO.GetPostCarriageLegs(postCarriageHblDeliveryMode).ToList();
			result.AddLastLeg((sourceBO as ICO2ePrePostCarriage).GetPostCarriageLocations(postCarriageHblDeliveryMode).LastOrDefault(location => !location.IsEmpty), lastPostCarriageLocation);
			return result;
		}

		#endregion

		protected override DataObjectWriter<Transport, TransportLeg> GetLegDataObjectWriter()
		{
			return new CO2eTransportLegDataObjectWriter(writeManager);
		}

		protected override DataContextType GetTopLevelDataContextType()
		{
			return DataContextType.ForwardingConsol;
		}
	}
}
