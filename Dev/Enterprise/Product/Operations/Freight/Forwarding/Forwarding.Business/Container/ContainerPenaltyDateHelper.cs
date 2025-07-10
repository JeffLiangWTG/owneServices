using CargoWise.Types;
using Enterprise.Freight.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ContainerPenaltyDateHelper
	{
		public static ContainerPenaltyDate GetAvailableDateForDetention(ForwardingContainer container, string freeDayType)
		{
			return freeDayType switch
			{
				ContainerDetentionFreeDayType.CTOAvailable when container.JC_FCLAvailable.IsValid
					=> new ContainerPenaltyDate(container.JC_FCLAvailable, Res.GetString("47fb1b25-0df3-4734-8d61-d156a403bf40", "CTO Available")),
				ContainerDetentionFreeDayType.CTOAvailable
					=> new ContainerPenaltyDate(container.SailingCTOAvailableDate, Res.GetString("47fb1b25-0df3-4734-8d61-d156a403bf40", "CTO Available")),
				ContainerDetentionFreeDayType.CTOGateOut
					=> new ContainerPenaltyDate(container.JC_FCLWharfGateOut, Res.GetString("58375544-a02f-40fe-bdb5-f6c78786d6ff", "CTO Gate Out")),
				ContainerDetentionFreeDayType.DayAfterFCLUnload when container.JC_FCLUnloadFromVessel.IsEmpty
					=> new ContainerPenaltyDate(ZDateTime.Empty, Res.GetString("f4df559b-aa00-4ed9-ac4e-b01772f18607", "Day after FCL Unload")),
				ContainerDetentionFreeDayType.DayAfterFCLUnload
					=> new ContainerPenaltyDate(container.JC_FCLUnloadFromVessel.AddDays(1), Res.GetString("f4df559b-aa00-4ed9-ac4e-b01772f18607", "Day after FCL Unload")),
				ContainerDetentionFreeDayType.FCLUnload
					=> new ContainerPenaltyDate(container.JC_FCLUnloadFromVessel, Res.GetString("8a057992-7b1a-4640-86d6-577035fa64f8", "FCL Unload")),
				ContainerDetentionFreeDayType.VesselArrival
					=> new ContainerPenaltyDate(container.Consol?.JK_ATAForLastTransport ?? ZDateTime.Empty, Res.GetString("22410970-2473-40f8-81df-fe8b367510b8", "Vessel Arrival")),
				ContainerDetentionFreeDayType.WharfGateIn
					=> new ContainerPenaltyDate(container.JC_FCLWharfGateIn, Res.GetString("6783b04f-3d99-4044-a6d9-d773607dd8af", "CTO Gate In")),
				ContainerDetentionFreeDayType.FCLLoad
					=> new ContainerPenaltyDate(container.JC_FCLOnBoardVessel, Res.GetString("8a945285-6c9c-4f74-9c81-53c23e509d2a", "FCL Load")),
				ContainerDetentionFreeDayType.DayBeforeFCLLoad when container.JC_FCLOnBoardVessel.IsEmpty
					=> new ContainerPenaltyDate(ZDateTime.Empty, Res.GetString("5ab4be0f-b1ac-4afa-9c4c-305b95c3dd87", "Day before FCL Load")),
				ContainerDetentionFreeDayType.DayBeforeFCLLoad
					=> new ContainerPenaltyDate(container.JC_FCLOnBoardVessel.AddDays(-1), Res.GetString("5ab4be0f-b1ac-4afa-9c4c-305b95c3dd87", "Day before FCL Load")),
				ContainerDetentionFreeDayType.VesselDeparture when container.Consol?.JK_DepartureForFirstTransport != null
					=> new ContainerPenaltyDate(container.Consol.JK_DepartureForFirstTransport, Res.GetString("c5680edd-0bd5-42af-8265-ed456d3d4be7", "Vessel Departure")),
				ContainerDetentionFreeDayType.VesselDeparture
					=> new ContainerPenaltyDate(ZDateTime.Empty, Res.GetString("c5680edd-0bd5-42af-8265-ed456d3d4be7", "Vessel Departure")),
				ContainerDetentionFreeDayType.DayBeforeVesselDeparture when (container.Consol?.JK_DepartureForFirstTransport ?? ZDateTime.Empty).IsEmpty
					=> new ContainerPenaltyDate(ZDateTime.Empty, Res.GetString("6fa4e518-053b-4906-afa4-b006a7a4f4e0", "Day before Vessel Departure")),
				ContainerDetentionFreeDayType.DayBeforeVesselDeparture
					=> new ContainerPenaltyDate(container.Consol.JK_DepartureForFirstTransport.AddDays(-1), Res.GetString("6fa4e518-053b-4906-afa4-b006a7a4f4e0", "Day before Vessel Departure")),
				_ => new ContainerPenaltyDate(ZDateTime.Empty, ZString.Empty)
			};
		}
	}
}
