using System;

using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	internal sealed class LandscapeComplexPageEqualityComparer : PageEqualityComparer
	{
		enum Category
		{
			ForwardingFreight,
			ForwardingSupplementary,

			ShippingFreight,
			ShippingSupplementary,

			ShippingDetention,

			NonFreight,
		}

		public override bool Equals(RateEntry entry1, RateEntry entry2)
		{
			var category = Categorise(entry1);

			if (Categorise(entry2) != category)
			{
				return false;
			}
			else
			{
				switch (category)
				{
					case Category.ForwardingFreight:
					case Category.ShippingFreight:
						return Equals_Quote_Freight(entry1, entry2);

					case Category.ForwardingSupplementary:
					case Category.ShippingSupplementary:
						return Equals_Quote_Supplementary(entry1, entry2);

					case Category.ShippingDetention:
						return Equals_ShippingDetention(entry1, entry2);

					case Category.NonFreight:
						return Equals_NonFreight(entry1, entry2);

					default:
						throw new InvalidOperationException("Invalid Category: " + category);
				}
			}
		}
		public override int GetHashCode(RateEntry entry)
		{
			var category = Categorise(entry);

			switch (category)
			{
				case Category.ForwardingFreight:
				case Category.ShippingFreight:
					return category.GetHashCode() ^ GetHashCode_Quote_Freight(entry);

				case Category.ForwardingSupplementary:
				case Category.ShippingSupplementary:
					return category.GetHashCode() ^ GetHashCode_Quote_Supplementary(entry);

				case Category.ShippingDetention:
					return category.GetHashCode() ^ GetHashCode_ShippingDetention(entry);

				case Category.NonFreight:
					return category.GetHashCode() ^ GetHashCode_NonFreight(entry);

				default:
					throw new InvalidOperationException("Invalid Category: " + category);
			}
		}

		static bool Equals_Quote_Freight(RateEntry entry1, RateEntry entry2)
		{
			return entry1.RateType() == entry2.RateType()
				&& entry1.FreightPage == entry2.FreightPage
				&& ((IImportExport)entry1).JobDirection == ((IImportExport)entry2).JobDirection
				&& entry1.LocalPortCode == entry2.LocalPortCode
				&& entry1.TI_RH_NKCommodityCode == entry2.TI_RH_NKCommodityCode
				&& entry1.TI_RS_NKServiceLevel_NI == entry2.TI_RS_NKServiceLevel_NI;
		}
		static int GetHashCode_Quote_Freight(RateEntry entry)
		{
			unchecked
			{
				var tmp = (uint)entry.RateType();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.FreightPage.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)((IImportExport)entry).JobDirection.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.LocalPortCode.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_RH_NKCommodityCode.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_RS_NKServiceLevel_NI.GetHashCode();
				return (int)tmp;
			}
		}

		static bool Equals_Quote_Supplementary(RateEntry entry1, RateEntry entry2)
		{
			return entry1.RateType() == entry2.RateType()
				&& entry1.FreightPage == entry2.FreightPage
				&& entry1.TI_RH_NKCommodityCode == entry2.TI_RH_NKCommodityCode
				&& entry1.TI_RS_NKServiceLevel_NI == entry2.TI_RS_NKServiceLevel_NI;
		}
		static int GetHashCode_Quote_Supplementary(RateEntry entry)
		{
			unchecked
			{
				var tmp = (uint)entry.RateType();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.FreightPage.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_RH_NKCommodityCode.GetHashCode();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_RS_NKServiceLevel_NI.GetHashCode();
				return (int)tmp;
			}
		}

		static bool Equals_ShippingDetention(RateEntry entry1, RateEntry entry2)
		{
			return entry1.RateType() == entry2.RateType()
				&& entry1.TI_OH_TransportProvider == entry2.TI_OH_TransportProvider;
		}
		static int GetHashCode_ShippingDetention(RateEntry entry)
		{
			unchecked
			{
				var tmp = (uint)entry.RateType();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.TI_OH_TransportProvider.GetHashCode();
				return (int)tmp;
			}
		}

		static bool Equals_NonFreight(RateEntry entry1, RateEntry entry2)
		{
			return entry1.RateType() == entry2.RateType();
		}
		static int GetHashCode_NonFreight(RateEntry entry)
		{
			unchecked
			{
				return (int)entry.RateType();
			}
		}

		static Category Categorise(RateEntry entry)
		{
			if (entry.IsForwarding())
			{
				if (entry.IsFreightEntry())
				{
					return Category.ForwardingFreight;
				}
				else
				{
					return Category.ForwardingSupplementary;
				}
			}
			else if (entry.IsShipping())
			{
				if (entry.IsFreightEntry())
				{
					return Category.ShippingFreight;
				}
				else
				{
					return Category.ShippingSupplementary;
				}
			}
			else if (entry.IsShippingExportDetention() || entry.IsShippingImportDetention())
			{
				return Category.ShippingDetention;
			}
			else
			{
				return Category.NonFreight;
			}
		}
	}
}

