using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.Services.ServiceHost
{
	public class ScreeningResponse
	{
		public bool HasRisk;
		public DpsResponseCode ResponseCode;
		public string ExtraMessage;
	}
}
