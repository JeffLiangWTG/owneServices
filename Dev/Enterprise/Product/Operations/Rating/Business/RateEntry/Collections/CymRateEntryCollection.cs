using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class CYMRateEntryCollection : RateEntryCollection
	{
		public CYMRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.CYM; }
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.ALL; }
		}
	}
}
