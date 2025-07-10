using System;

namespace Enterprise.Rating.Business
{
	internal sealed class LandscapeSimplePageEqualityComparer : PageEqualityComparer
	{
		enum Category
		{
			NonFreight,
			FreightRate,
			ShippingDetention,
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
					case Category.NonFreight:
						return Equals_NonFreight(entry1, entry2);

					case Category.FreightRate:
						return Equals_FreightRate(entry1, entry2);

					case Category.ShippingDetention:
						return Equals_ShippingDetention(entry1, entry2);

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
				case Category.FreightRate:
					return category.GetHashCode() ^ GetHashCode_FreightRate(entry);

				case Category.ShippingDetention:
					return category.GetHashCode() ^ GetHashCode_ShippingDetention(entry);

				case Category.NonFreight:
					return category.GetHashCode() ^ GetHashCode_NonFreight(entry);

				default:
					throw new InvalidOperationException("Invalid Category: " + category);
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

		static bool Equals_FreightRate(RateEntry entry1, RateEntry entry2)
		{
			return entry1.RateType() == entry2.RateType()
				&& entry1.FreightPage == entry2.FreightPage;
		}
		static int GetHashCode_FreightRate(RateEntry entry)
		{
			unchecked
			{
				var tmp = (uint)entry.RateType();
				tmp = ((tmp << 27) | (tmp >> 5)) ^ (uint)entry.FreightPage.GetHashCode();
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

		static Category Categorise(RateEntry entry)
		{
			if (entry.IsForwarding() || entry.IsShipping())
			{
				return Category.FreightRate;
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

