using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class CYDRateEntryCollection : RateEntryCollection
	{
		public CYDRateEntryCollection(BusinessObject master, BusinessObjectFactory factory)
			: base(master, factory)
		{
		}

		public override ZString RateEntryType
		{
			get { return RatingConstants.RateCategory.CYD; }
		}

		protected override ZString DefaultRateEntryMode
		{
			get { return Core.Constants.RateMode.ALL; }
		}
	}
}
