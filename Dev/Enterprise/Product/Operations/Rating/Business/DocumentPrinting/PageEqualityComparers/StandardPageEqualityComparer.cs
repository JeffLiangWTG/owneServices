namespace Enterprise.Rating.Business
{
	internal sealed class StandardPageEqualityComparer : PageEqualityComparer
	{
		public override bool Equals(RateEntry entry1, RateEntry entry2)
		{
			if (entry1.IsFreightEntry())
			{
				if (entry2.IsFreightEntry())
				{
					return entry1.TI_RateCategory == entry2.TI_RateCategory
						&& Equals_Standard(entry1, entry2);
				}
				else
				{
					return false;
				}
			}
			else
			{
				if (entry2.IsFreightEntry())
				{
					return false;
				}
				else
				{
					return Equals_Standard(entry1, entry2);
				}
			}
		}
		public override int GetHashCode(RateEntry entry)
		{
			var result = GetHashCode_Standard(entry);

			if (entry.IsFreightEntry())
			{
				result ^= entry.TI_RateCategory.GetHashCode();
			}

			return result;
		}

		static bool Equals_Standard(RateEntry entry1, RateEntry entry2)
		{
			return entry1.TI_Mode == entry2.TI_Mode
				&& entry1.TI_OriginLRC == entry2.TI_OriginLRC
				&& entry1.TI_DestinationLRC == entry2.TI_DestinationLRC
				&& entry1.TI_ViaLRC == entry2.TI_ViaLRC
				&& entry1.TI_OH_Supplier == entry2.TI_OH_Supplier
				&& entry1.TI_RS_NKServiceLevel_NI == entry2.TI_RS_NKServiceLevel_NI
				&& entry1.TI_RH_NKCommodityCode == entry2.TI_RH_NKCommodityCode
				&& entry1.TI_OH_TransportProvider == entry2.TI_OH_TransportProvider
				&& entry1.TI_Frequency == entry2.TI_Frequency
				&& entry1.TI_FrequencyUnit == entry2.TI_FrequencyUnit
				&& entry1.TI_TransitTime == entry2.TI_TransitTime
				&& entry1.TI_RateStartDate == entry2.TI_RateStartDate
				&& entry1.TI_RateEndDate == entry2.TI_RateEndDate
				&& entry1.TI_WW_Warehouse == entry2.TI_WW_Warehouse
				&& entry1.TI_OH_Consignee == entry2.TI_OH_Consignee
				&& entry1.TI_OH_Consignor == entry2.TI_OH_Consignor
				&& entry1.TI_QuotePageIncoTerm == entry2.TI_QuotePageIncoTerm;
		}
		static int GetHashCode_Standard(RateEntry entry)
		{
			unchecked
			{
				var tmp = (uint)entry.TI_Mode.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_OriginLRC.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_DestinationLRC.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_ViaLRC.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_OH_Supplier.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_RS_NKServiceLevel_NI.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_RH_NKCommodityCode.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_OH_TransportProvider.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_Frequency.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_FrequencyUnit.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_TransitTime.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_RateStartDate.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_RateEndDate.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_WW_Warehouse.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_OH_Consignee.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_OH_Consignor.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_QuotePageIncoTerm.GetHashCode();
				return (int)tmp;
			}
		}
	}
}

