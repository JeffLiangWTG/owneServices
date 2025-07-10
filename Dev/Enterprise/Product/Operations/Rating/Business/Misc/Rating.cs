using System;

namespace Enterprise.Rating.Business
{
	class Rating : Enterprise.Integration.Rating.IRating
	{
		public Type RateEntryType
		{
			get { return typeof(RateEntry); }
		}

		public Type CompanyTariffRatingHeaderType
		{
			get { return typeof(CompanyTariff); }
		}

		public Type QuotationProcessTaskType
		{
			get { return typeof(QuotationProcessTask); }
		}

		public Type QuoteType
		{
			get { return typeof(Quote); }
		}

		public Type RateLineType
		{
			get { return typeof(RateLine); }
		}

		public Type RateLineItemType
		{
			get { return typeof(RateLineItem); }
		}

		public Type RatingHeaderType
		{
			get { return typeof(RatingHeader); }
		}
	}
}
