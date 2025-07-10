using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Business
{
	public class JobAddressAdditionalInfoTransportModeCodeDescriptionPairList : CodeDescriptionPairList
	{
		public JobAddressAdditionalInfoTransportModeCodeDescriptionPairList()
		{
			AddPair(Core.Constants.TransportModes.Road, ResString.GetMultilingualString("EABD1899-C0B4-4663-96A6-1904105DBFBE", "Road Freight"));
			AddPair(Core.Constants.TransportModes.Rail, ResString.GetMultilingualString("BD7FE09E-5813-4C28-8407-67CF16E96F9B", "Rail Freight"));
			AddPair(Core.Constants.TransportModes.InlandWaterwayTransport, ResString.GetMultilingualString("8828BBDE-393E-4EE3-85D4-3A4CBFC9C7CD", "Inland Waterways"));
		}
	}
}
