using System.Collections.Generic;

namespace CargoWise.eHub.Products.OceanCarrierMessaging.PipelineComponents
{
	public class CargoWiseOneToCargoWiseOneMessageTypes
    {
        public static readonly Dictionary<string, string> MessageTypes = new Dictionary<string, string>
        {
            {"http://www.cargowise.com/Schemas/Universal/2012/11/BookingConfirmation/1", "CARGOWISE_BC"},
            {"http://www.cargowise.com/Schemas/Universal/2012/11/Acknowledgement/1", "CARGOWISE_AC"},
            {"http://www.cargowise.com/Schemas/Universal/2012/11/BLData/1", "CARGOWISE_BL"},
            {"http://www.cargowise.com/Schemas/Universal/2012/11/TrackAndTrace/1", "CARGOWISE_CT"}
        };
    }
}
