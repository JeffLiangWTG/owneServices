
using CargoWise.EntityFramework;
using Enterprise.Rating.Business;

namespace Enterprise.Tracking.Business.Quotations
{
	public class TrackingQuoteCollection : QuoteCollection
	{
		public TrackingQuoteCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new TrackingQuote this[int index]
		{
			get { return (TrackingQuote)Elements[index]; }
		}

		public new TrackingQuote AddNew()
		{
			return (TrackingQuote)base.AddNew();
		}

		protected override ZQuery GetCurrentCompanyFilter()
		{
			return new ZQuery();
		}
	}
}
