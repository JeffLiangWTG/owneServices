using Enterprise.DeniedPartyScreening.Common;

namespace Enterprise.MasterFiles.Business
{
	public class DpsResponseWithScreeningParty
	{
		public DpsResponseWithScreeningParty(ScreeningParty screeningParty, DpsResponse response, DpsRequestHeaderWithAddressMatching requestHeaderWithAddressMatching)
		{
			ScreeningParty = screeningParty;
			Response = response;
			RequestHeaderWithAddressMatching = requestHeaderWithAddressMatching;
		}

		public ScreeningParty ScreeningParty { get; }
		public DpsResponse Response { get; }
		public DpsRequestHeaderWithAddressMatching RequestHeaderWithAddressMatching { get; }
	}
}
