using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class TWURateEntryCollection : RateEntryCollection
	{
		public TWURateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public override ZString RateEntryType => RatingConstants.RateCategory.TWU;

		protected override ZString DefaultRateEntryMode => Core.Constants.RateMode.ALL;
	}
}
