namespace Enterprise.Rating.GUI.RateSelector
{
	public static class GlowRateSelectorMessageType
	{
		public const string RateSearchRequest = "cw.c3.rate_search_request";
		public const string RateSearchResult = "cw.c3.rate_search_result";
		public const string ApplyRatesRequest = "cw.c3.apply_rates_request";
		public const string ApplyRatesResponse = "cw.c3.apply_rates_response";
		public const string JobUpdateRequired  = "cw.c3.job_update_required";
		public const string AbortSessionRequest = "cw.c3.abort_session_request";
		public const string AbortSessionResponse = "cw.c3.abort_session_response";
		public const string Exception = "cw.c3.exception";
	}
}
