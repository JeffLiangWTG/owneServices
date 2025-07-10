using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class SummaryRateEntryCollection : RateEntryCollection
	{
		public SummaryRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public override ZString RateEntryType
		{
			get { return ZString.Empty; }
		}

		public override ZString CategoryForFiltering
		{
			get { return RatingConstants.RateCategory.SummaryRatesCategory; }
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}
	}
}

