namespace Enterprise.Rating.Business.RateSelector
{
	public static class CommodityCodeExtensions
	{
		public static bool IsEmptyCommodityCode(this IRateEntry rate) =>
			rate.TI_RH_NKCommodityCode.IsEmpty;

		public static bool IsGeneralCommodityCode(this IRateEntry rate) =>
			rate.TI_RH_NKCommodityCode == GEN;

		public static bool IsEmptyOrGeneralCommodityCode(this IRateEntry rate) =>
			rate.TI_RH_NKCommodityCode.IsEmpty || rate.TI_RH_NKCommodityCode == GEN;

		public static bool IsSpecificCommodityCode(this IRateEntry rate) =>
			!rate.TI_RH_NKCommodityCode.IsEmpty && rate.TI_RH_NKCommodityCode != GEN;

		public const string GEN = "GEN"; // General Commodity Code
	}
}
