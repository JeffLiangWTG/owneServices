using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class TBCRateEntryCollection : RateEntryCollection
	{
		public TBCRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.TBC; }
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.ALL; }
		}
	}
}
