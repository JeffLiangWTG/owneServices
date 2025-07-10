using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public static class FreightDefaultPortHelper
	{
		public static ZString GetDefaultShipmentOriginPort(GlbBranch branch, ZString transportMode, ZString containerMode, bool isExport = true)
		{
			return GetDefaultPort(branch, GlbBranchDefaultToList.Codes.ShipmentOrigin, transportMode, containerMode, isExport);
		}

		public static ZString GetDefaultShipmentDestinationPort(GlbBranch branch, ZString transportMode, ZString containerMode, bool isExport = false)
		{
			return GetDefaultPort(branch, GlbBranchDefaultToList.Codes.ShipmentDestination, transportMode, containerMode, isExport);
		}

		public static ZString GetDefaultConsolFirstLoadPort(GlbBranch branch, ZString transportMode, ZString containerMode, bool isExport = true)
		{
			return GetDefaultPort(branch, GlbBranchDefaultToList.Codes.ConsolFirstLoad, transportMode, containerMode, isExport);
		}

		public static ZString GetDefaultConsolLastDischargePort(GlbBranch branch, ZString transportMode, ZString containerMode, bool isExport = false)
		{
			return GetDefaultPort(branch, GlbBranchDefaultToList.Codes.ConsolLastDischarge, transportMode, containerMode, isExport);
		}

		static ZString GetDefaultPort(GlbBranch branch, ZString defaultTo, ZString transportMode, ZString containerMode, bool isExport)
		{
			return branch.DefaultPorts.GetDefaultPort(defaultTo,
				GetDefaultTransportMode(transportMode),
				GetDefaultContainerMode(containerMode, transportMode, isExport))?.GBP_RL_NKPort ?? branch.GB_RL_NKHomePort;
		}

		static ZString GetDefaultTransportMode(ZString transportMode)
		{
			if (transportMode == Core.Constants.TransportModes.AirSea)
			{
				return Core.Constants.TransportModes.Air;
			}

			if (transportMode == Core.Constants.TransportModes.SeaAir)
			{
				return Core.Constants.TransportModes.Sea;
			}

			return transportMode;
		}

		static ZString GetDefaultContainerMode(ZString containerMode, ZString transportMode, bool isExport)
		{
			if (containerMode == Core.Constants.ContainerModes.BuyersConsol)
			{
				switch (transportMode)
				{
					case Core.Constants.TransportModes.Sea:
					case Core.Constants.TransportModes.SeaAir:
					case Core.Constants.TransportModes.Rail:
						return isExport ? Core.Constants.ContainerModes.LCL : Core.Constants.ContainerModes.FCL;
					case Core.Constants.TransportModes.Air:
					case Core.Constants.TransportModes.AirSea:
						return isExport ? Core.Constants.ContainerModes.Loose : Core.Constants.ContainerModes.ULD;
					case Core.Constants.TransportModes.Road:
						return isExport ? Core.Constants.ContainerModes.LTL : Core.Constants.ContainerModes.FTL;
				}
			}

			return containerMode;
		}
	}
}
