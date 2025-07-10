using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class TRNRateEntryCollection : RateEntryCollection
	{
		public TRNRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.TRN; }
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.ALL; }
		}
	}
}
