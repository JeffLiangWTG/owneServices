using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class QuotationProcessTaskCollection : RatingHeaderProcessTaskCollection<Quote, QuotationProcessTask>
	{
		public QuotationProcessTaskCollection(Quote quote)
			: base(quote)
		{
		}

		protected override ProcessTaskCollection GetNewCollectionCore(Quote parent)
		{
			return new QuotationProcessTaskCollection(parent);
		}
	}
}



