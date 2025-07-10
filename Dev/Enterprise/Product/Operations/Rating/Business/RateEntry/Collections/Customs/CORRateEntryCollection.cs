using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class CORRateEntryCollection : ORGRateEntryCollection
	{
		public CORRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public override ZString RateEntryType => RatingConstants.RateCategory.COR;
	}
}

