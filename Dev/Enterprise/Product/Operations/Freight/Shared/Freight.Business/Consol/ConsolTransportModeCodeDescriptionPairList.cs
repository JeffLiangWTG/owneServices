
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class ConsolTransportModeCodeDescriptionPairList : CodeDescriptionPairList
	{
		public ConsolTransportModeCodeDescriptionPairList()
		{
			AddPair(Core.Constants.TransportModes.Air, ResString.GetMultilingualString("abaf6892-6ec7-422e-af60-66f71ac9502b", "Air Freight"));
			AddPair(Core.Constants.TransportModes.Sea, ResString.GetMultilingualString("1255e7f1-2000-41fe-a917-20f2dea05cd3", "Sea Freight"));
			AddPair(Core.Constants.TransportModes.Road, ResString.GetMultilingualString("2dae04d5-1614-4c80-9ee3-1a2e2f2a72c6", "Road Freight"));
			AddPair(Core.Constants.TransportModes.Rail, ResString.GetMultilingualString("39558513-15a9-4009-be27-181506091540", "Rail Freight"));
		}
	}
}
