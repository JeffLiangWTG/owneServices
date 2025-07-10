using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.PortHubs.Business
{
	public class TransportModeList : CodeDescriptionPairList
	{
		public TransportModeList()
		{
			AddPair(Core.Constants.TransportModes.All, ResString.GetMultilingualString("5bacff5d-2641-4b47-a5cf-0703becb57ea", "All Freight"));
			AddPair(Core.Constants.TransportModes.Air, ResString.GetMultilingualString("c685509e-7d0c-4b5b-9fd6-331c61aa7f1e", "Air Freight"));
			AddPair(Core.Constants.TransportModes.Sea, ResString.GetMultilingualString("13b55e3d-3f6f-45ea-b4f3-11e699d02fbc", "Sea Freight"));
			AddPair(Core.Constants.TransportModes.Road, ResString.GetMultilingualString("dc04e275-95d3-405b-9919-3940e6e804af", "Road Freight"));
			AddPair(Core.Constants.TransportModes.Rail, ResString.GetMultilingualString("6866c44b-cb6f-43e5-8a4e-ff9488b413cb", "Rail Freight"));
		}
	}
}
