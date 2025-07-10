using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class StevedoreTerminalTypeHelperTest : TestCaseWithFactory
	{
		public void TestVesselTerminalTypeMapping()
		{
			AssertEquals(StevedoreTerminalType.Codes.ContainerTerminal, StevedoreTerminalTypeHelper.GetFromVesselType(null));

			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.BulkCarrier, StevedoreTerminalType.Codes.BulkTerminal);

			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.LiquidNaturalGasTanker, StevedoreTerminalType.Codes.LiquidTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.OilTanker, StevedoreTerminalType.Codes.LiquidTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.OtherTanker, StevedoreTerminalType.Codes.LiquidTerminal);

			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.LiveStockVessel, StevedoreTerminalType.Codes.LivestockTerminal);

			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.CarCarringVessel, StevedoreTerminalType.Codes.ROROTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.RollOnRollOff, StevedoreTerminalType.Codes.ROROTerminal);

			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.Barge, StevedoreTerminalType.Codes.OtherTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.CableShip, StevedoreTerminalType.Codes.OtherTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.Dredger, StevedoreTerminalType.Codes.OtherTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.DrillShip, StevedoreTerminalType.Codes.OtherTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.FishingVessel, StevedoreTerminalType.Codes.OtherTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.NavalVessel, StevedoreTerminalType.Codes.OtherTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.OilRig, StevedoreTerminalType.Codes.OtherTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.OtherVessels, StevedoreTerminalType.Codes.OtherTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.PassengerVessel, StevedoreTerminalType.Codes.OtherTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.ResearchVessel, StevedoreTerminalType.Codes.OtherTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.SupplyBoat, StevedoreTerminalType.Codes.OtherTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.Yacht, StevedoreTerminalType.Codes.OtherTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.TugBoat, StevedoreTerminalType.Codes.OtherTerminal);

			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.CargoVessel, StevedoreTerminalType.Codes.ContainerTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.DryCargoVessel, StevedoreTerminalType.Codes.ContainerTerminal);
			AssertVesselTerminalTypeMapping(Core.Constants.VesselType.ContainerisedVessel, StevedoreTerminalType.Codes.ContainerTerminal);
		}

		void AssertVesselTerminalTypeMapping(string vesselType, string expectedTerminalType)
		{
			RefVessel vessel = Factory.New<RefVessel>();
			vessel.RV_VesselType = vesselType;
			AssertEquals(expectedTerminalType, StevedoreTerminalTypeHelper.GetFromVesselType(vessel));
		}
	}
}
