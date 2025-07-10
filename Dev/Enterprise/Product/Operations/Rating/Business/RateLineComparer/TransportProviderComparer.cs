using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	class TransportProviderComparer : ColumnComparer
	{
		public TransportProviderComparer()
			: base(RateEntrySchema.TI_OH_TransportProvider)
		{
		}

		public override int Compare(FastLine line1, FastLine line2)
		{
			var entry1 = line1.ParentRateEntry;
			var entry2 = line2.ParentRateEntry;

			var parentRatingHeader1 = entry1.ParentRatingHeader.TH_OH;
			var parentRatingHeader2 = entry2.ParentRatingHeader.TH_OH;

			if (entry1.IsCosting() && entry2.IsCosting() && parentRatingHeader1 != parentRatingHeader2)
			{
				if (parentRatingHeader1 == entry2.TI_OH_TransportProvider && !parentRatingHeader1.IsEmpty)
				{
					return 0;
				}

				if (parentRatingHeader2 == entry1.TI_OH_TransportProvider && !parentRatingHeader2.IsEmpty)
				{
					return 0;
				}
			}

			return base.Compare(line1, line2);
		}
	}
}
