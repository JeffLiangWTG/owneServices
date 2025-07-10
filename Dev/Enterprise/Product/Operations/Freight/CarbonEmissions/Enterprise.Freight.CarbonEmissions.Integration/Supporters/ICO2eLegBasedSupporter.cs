using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CarbonEmissions.Integration
{
	[ResponseImporter("Enterprise.Freight.DataTransfer.Universal.CO2eLegBasedResponseImporter, Enterprise.Freight.DataTransfer")]
	public interface ICO2eLegBasedSupporter : ICO2eCalculationSupporter, ICO2eTEUProvider, ICO2ePrePostCarriage
	{
		ZDecimal Weight { get; }
		ZString UnitOfWeight { get; }
		ZString TransportMode { get; }
		RefUNLOCO LoadPort { get; }
		RefUNLOCO DischargePort { get; }
		IPrePostCarriageLocation LoadPortForCO2eCalc { get; }
		IPrePostCarriageLocation DischargePortForCO2eCalc { get; }
		IPrePostCarriageLocation AdditionalLoadPortForCO2eCalc { get; }
		IPrePostCarriageLocation AdditionalDischargePortForCO2eCalc { get; }
		IPrePostCarriageLocation ViaPortForCO2eCalc { get; }
		ZDateTime ETA { get; }
		ZDateTime ETD { get; }
		ZString ContainerMode { get; }
		bool RequiresTemperatureControl { get; }

		List<ICO2eLegProvider> Legs { get; }
		bool SupportVirtualLegs { get; }
		bool ShouldPopulateCO2eForLegs { get; }

		ZDecimal GetNumberOfTEUForLeg(ICO2eLegProvider leg);

		IList<CO2eEmptyContainer> EmptyContainers { get; }

		ZDecimal GetTotalEmptyContainerEmissions();
		bool RequireTEUForTransportMode(string transportMode);
	}

	public struct CO2eEmptyContainer
	{
		public CO2eEmptyContainer(ICO2eEmptyContainerProvider container, bool hasPickup, bool hasReturn)
		{
			Container = container;
			HasPickup = hasPickup;
			HasReturn = hasReturn;
		}

		public ICO2eEmptyContainerProvider Container;
		public bool HasPickup;
		public bool HasReturn;
	}
}
