using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class BookingTransportModeCodeDescriptionPairList : CodeDescriptionPairList
	{
		public BookingTransportModeCodeDescriptionPairList()
		{
			AddPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea);
			AddPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air);
			AddPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail);
			AddPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road);
		}
	}

	public class BookingContainerModeCodeDescriptionPairList : CodeDescriptionPairList
	{
		public BookingContainerModeCodeDescriptionPairList(ZString transportMode)
		{
			switch (transportMode)
			{
				case Constants.TransportModes.Air:
					AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
					AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					break;

				case Constants.TransportModes.Sea:
					AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
					break;

				case Constants.TransportModes.Road:
					AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					AddPair(Constants.ContainerModes.FTL, Constants.ContainerModeDescriptions.FTL);
					AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					break;

				case Constants.TransportModes.Rail:
					AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					break;

				case Constants.TransportModes.All:
					AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
					AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
					AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					AddPair(Constants.ContainerModes.FTL, Constants.ContainerModeDescriptions.FTL);
					break;

				case "":
					new CodeDescriptionPairList(OLookUpEditType.FreightContainerMode);
					break;

				default:
					break;
			}
		}
	}

	public class ParentTransportTypeCodeDescriptionPairList : CodeDescriptionPairList
	{
		public ParentTransportTypeCodeDescriptionPairList(ZString transportMode)
		{
			switch (transportMode)
			{
				case Constants.TransportModes.Sea:
					AddPair(Constants.VoyageType.MainVoyage, Res.GetString("395920a0-da93-4f02-a012-bdb183f00cc0", "Main"));
					AddPair(Constants.VoyageType.SlotVoyage, Res.GetString("8f6ae92a-45eb-4ae3-9b8c-72494a0cf84c", "Slot"));
					break;
			}
		}
	}
}
