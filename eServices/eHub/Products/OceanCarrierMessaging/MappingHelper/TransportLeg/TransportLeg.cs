namespace CargoWise.eHub.Products.OceanCarrierMessaging.Transforms.Helper
{
    public class TransportLeg
    {
        public string TransportMode { get; set; }
        public string LegType { get; set; }
        public string OldLegOrder { get; set; }
        public string LegOrder { get; set; }
        public string PortOfLoadingCode { get; set; }
        public string PortOfLoadingName { get; set; }
        public string PortOfDischargeCode { get; set; }
        public string PortOfDischargeName { get; set; }
        public string VesselName { get; set; }
        public string VoyageFlightNo { get; set; }
    }

    public static class LegTypeConstant
    {
        public const string PreCarriage = "PreCarriage";
        public const string OnForwarding = "OnForwarding";
        public const string Main = "Main";
    }

    public static class TransportModeConstant
    {
        public const string Sea = "Sea";
        public const string Rail = "Rail";
        public const string Road = "Road";
        public const string InlandWaterway = "InlandWaterway";
        public const string RailRoad = "RailRoad";
        public const string RailWater = "RailWater";
        public const string RoadWater = "RoadWater";
    }
}