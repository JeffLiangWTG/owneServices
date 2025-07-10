
namespace Enterprise.Customs.SG.V4.Business
{
	public partial class TransportModeCodeList
	{
		public static bool TransportIsSeaOrAir(string transportMode)
		{
			return !string.IsNullOrEmpty(transportMode) && (transportMode == TransportModeCodeList.Codes.TransportMode_1_SEA || transportMode == TransportModeCodeList.Codes.TransportMode_4_Air);
		}
	}
}
