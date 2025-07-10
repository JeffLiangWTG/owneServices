using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class CFCRateEntryCollection : FCLRateEntryCollection
	{
		public CFCRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public override ZString RateEntryType => RatingConstants.RateCategory.CFC;
	}
}
