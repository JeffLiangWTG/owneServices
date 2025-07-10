using System.Linq;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;
using CodeDescriptionPair = Enterprise.UniversalDataBuss.DataObjects.Universal.CodeDescriptionPair;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class EmptyContainerDataObjectWriter : DataObjectWriter<ForwardingContainer, WeightData>
	{
		readonly ForwardingShipment shipment;

		public EmptyContainerDataObjectWriter(ForwardingShipment shipment, IDataWritingManager writeManager) : base(writeManager)
		{
			this.shipment = shipment;
		}

		protected override WeightData PopulateDataObject(ForwardingContainer container)
		{
			return PopulateEmptyContainerProvider(container);
		}

		WeightData PopulateEmptyContainerProvider(ICO2eEmptyContainerProvider provider)
		{
			if (provider.TotalWeight == 0m && !provider.IncludeTEU)
			{
				return null;
			}

			var weightUnits = BindToLists.GetCachedLists(shipment.Factory).WeightUnits;
			var data = new WeightData();
			data.ContainerJobID = provider.ContainerJobID;
			data.TotalWeight = provider.TotalWeight;
			data.TotalWeightUnit = ListHelper.GetWithDescription<UnitOfWeight>(provider.TotalWeightUnit, weightUnits);
			if (provider.IncludeTEU)
			{
				data.TEU = new TEU
				{
					NumberOfTEU = provider.NumberOfTEU,
					TonnesPerTEU = 0m,
					ContainerEmptyWeightPerTEU = Utilities.Round(provider.ContainerEmptyWeightPerTEU, 6),
					ContainerEmptyWeightPerTEUUnit = ListHelper.GetWithDescription<UnitOfWeight>(provider.ContainerEmptyWeightPerTEUUnit, weightUnits)
				};
			}

			var hasPickup = false;
			var hasReturn = false;

			var emptyContainer = ((ICO2eLegBasedSupporter)shipment).EmptyContainers
				.FirstOrDefault(x => x.Container.ContainerJobID == provider.ContainerJobID);
			if (emptyContainer.HasPickup)
			{
				if (provider.GetCO2eStatus(CO2eTypes.EmptyPickup) != CO2eStatusList.Codes.Current)
				{
					hasPickup = PopulateEmptyContainer(provider.EmptyPickupAddress, data, true);
				}
			}

			if (emptyContainer.HasReturn)
			{
				if (provider.GetCO2eStatus(CO2eTypes.EmptyReturn) != CO2eStatusList.Codes.Current)
				{
					hasReturn = PopulateEmptyContainer(provider.EmptyReturnAddress, data, false);
				}
			}

			return hasPickup || hasReturn ? data : null;
		}

		bool PopulateEmptyContainer(EmptyContainerAddressResolver addressResolver, WeightData data, bool isPickup)
		{
			var (from, to, transportMode) = addressResolver(shipment);
			if (from != null && to != null && from.PK != to.PK)
			{
				var emptyContainerAddress = new EmptyContainerAddress()
				{
					From = from.ToUXmlOrganizationAddress(writeManager.WriterStrategy, shipment.Factory),
					To = to.ToUXmlOrganizationAddress(writeManager.WriterStrategy, shipment.Factory),
					TransportMode = ListHelper.GetWithDescription<CodeDescriptionPair>(transportMode, new CodeDescriptionPairList(OLookUpEditType.TransportType))
				};

				if (isPickup)
				{
					data.EmptyPickup = emptyContainerAddress;
				}
				else
				{
					data.EmptyReturn = emptyContainerAddress;
				}
				return true;
			}
			return false;
		}
	}
}
