namespace Enterprise.Rating.Business
{
	internal sealed class CompactPageEqualityComparer : PageEqualityComparer
	{
		public override bool Equals(RateEntry entry1, RateEntry entry2)
		{
			return entry1.RateType() == entry2.RateType()
				&& entry1.FreightPage == entry2.FreightPage
				&& entry1.CommodityCode == entry2.CommodityCode
				&& entry1.TI_RS_NKServiceLevel_NI == entry2.TI_RS_NKServiceLevel_NI
				&& entry1.TI_OH_TransportProvider == entry2.TI_OH_TransportProvider;
		}

		public override int GetHashCode(RateEntry entry)
		{
			unchecked
			{
				var tmp = (uint)entry.RateType();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.FreightPage.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_RH_NKCommodityCode.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_RS_NKServiceLevel_NI.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_OH_TransportProvider.GetHashCode();
				return (int)tmp;
			}
		}
	}
}

