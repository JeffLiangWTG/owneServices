namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Identifies what Stevedore Terminal Types handle specific Vessel Types.
	/// </summary>
	public static class StevedoreTerminalTypeHelper
	{
		public static string GetFromVesselType(RefVessel vessel)
		{
			string terminalType = StevedoreTerminalType.Codes.ContainerTerminal;

			if (vessel != null)
			{
				switch (vessel.RV_VesselType)
				{
					case Core.Constants.VesselType.BulkCarrier:
						terminalType = StevedoreTerminalType.Codes.BulkTerminal;
						break;

					case Core.Constants.VesselType.LiquidNaturalGasTanker:
					case Core.Constants.VesselType.OilTanker:
					case Core.Constants.VesselType.OtherTanker:
						terminalType = StevedoreTerminalType.Codes.LiquidTerminal;
						break;

					case Core.Constants.VesselType.LiveStockVessel:
						terminalType = StevedoreTerminalType.Codes.LivestockTerminal;
						break;

					case Core.Constants.VesselType.CarCarringVessel:
					case Core.Constants.VesselType.RollOnRollOff:
						terminalType = StevedoreTerminalType.Codes.ROROTerminal;
						break;

					case Core.Constants.VesselType.Barge:
					case Core.Constants.VesselType.CableShip:
					case Core.Constants.VesselType.Dredger:
					case Core.Constants.VesselType.DrillShip:
					case Core.Constants.VesselType.FishingVessel:
					case Core.Constants.VesselType.NavalVessel:
					case Core.Constants.VesselType.OilRig:
					case Core.Constants.VesselType.OtherVessels:
					case Core.Constants.VesselType.PassengerVessel:
					case Core.Constants.VesselType.ResearchVessel:
					case Core.Constants.VesselType.SupplyBoat:
					case Core.Constants.VesselType.Yacht:
					case Core.Constants.VesselType.TugBoat:
						terminalType = StevedoreTerminalType.Codes.OtherTerminal;
						break;

					case Core.Constants.VesselType.CargoVessel:
					case Core.Constants.VesselType.DryCargoVessel:
					case Core.Constants.VesselType.ContainerisedVessel:
					default:
						terminalType = StevedoreTerminalType.Codes.ContainerTerminal;
						break;
				}
			}

			return terminalType;
		}
	}
}
