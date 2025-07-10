using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class CLCRateEntryCollection : LCLRateEntryCollection
	{
		public CLCRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public override ZString RateEntryType => RatingConstants.RateCategory.CLC;
	}
}
