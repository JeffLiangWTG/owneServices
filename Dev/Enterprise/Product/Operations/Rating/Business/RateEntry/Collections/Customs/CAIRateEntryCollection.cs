using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class CAIRateEntryCollection : AIRRateEntryCollection
	{
		public CAIRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public override ZString RateEntryType => RatingConstants.RateCategory.CAI;
	}
}
