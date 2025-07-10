namespace Enterprise.Rating.Business
{
	public enum PricingPaginationStrategy
	{
		None = 0x000,

		StandardStyle = 0x001,
		LandscapeSimpleStyle = 0x002,
		LandscapeComplexStyle = 0x003,
		LandscapeCompactStyle = 0x004,

		ForwardingCategoryFilter = 0x010,
		ShippingCategoryFilter = 0x020,
		ShippingDetentionCategoryFilter = 0x030,
		CFSCategoryFilter = 0x040,

		NonAirModeOrContainerizedFilter = 0x100,
		AirModeAndNonContainerizedFilter = 0x200,

		StyleMask = 0x00F,
		CategoryFilterMask = 0x0F0,
		ModeContainerizedMask = 0x300,

		IsAgentPricingPage = 0x1000,
	}
}
