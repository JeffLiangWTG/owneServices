using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class TRWRateEntryCollection : RateEntryCollection
	{
		public TRWRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.TRW; }
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.ALL; }
		}
	}
}
